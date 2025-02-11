using System.Linq;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Types;

namespace CppSharp.Passes
{
    public class IgnoreSystemDeclarationsPass : TranslationUnitPass
    {
        public IgnoreSystemDeclarationsPass()
            => VisitOptions.ResetFlags(
                VisitFlags.NamespaceEnums | VisitFlags.ClassTemplateSpecializations | 
                VisitFlags.NamespaceTemplates | VisitFlags.NamespaceTypedefs |
                VisitFlags.NamespaceFunctions | VisitFlags.NamespaceVariables);

        private static bool IsPrivateStdDecl(Declaration decl)
        {
            return decl.OriginalName.Length > 2 &&
                   decl.OriginalName[0] == '_' &&
                   decl.OriginalName[1] != '_';
        }

        public override bool VisitTranslationUnit(TranslationUnit unit)
        {
            if (!unit.IsValid)
                return false;

            if (ClearVisitedDeclarations)
                Visited.Clear();

            VisitDeclarationContext(unit);

            return true;
        }

        public override bool VisitDeclaration(Declaration decl)
        {
            if (!base.VisitDeclaration(decl))
                return false;

            // Only check system types
            if (!decl.TranslationUnit.IsSystemHeader)
                return false;

            // Names of template parameters are irrelevant
            /*if (decl is TypeTemplateParameter or NonTypeTemplateParameter)
                return true;

            // Ignore std implementation detail classes
            if (IsPrivateStdDecl(decl))
            {
                decl.ExplicitlyIgnore();
                return true;
            }*/

            // Ignore std decls in `detail` namespace
            if (decl.QualifiedOriginalName.Contains("::detail"))
            {
                decl.ExplicitlyIgnore();
                return false;
            }

            return true;
        }

        public override bool VisitClassDecl(Class @class)
        {
            if (!base.VisitClassDecl(@class) || @class.Ignore)
                return false;

            // Ignore std implementation detail classes
            if (IsPrivateStdDecl(@class))
            {
                @class.ExplicitlyIgnore();
                return false;
            }

            if (Options.IsCLIGenerator)
                return false;

            if (Context.TypeMaps.FindTypeMap(@class, out TypeMap mappedType) && !mappedType.IsIgnored)
                return true;

            @class.ExplicitlyIgnore();

            if (!@class.IsDependent || @class.Specializations.Count == 0)
                return false;

            foreach (var specialization in @class.Specializations.Where(s => s.IsGenerated))
            {
                if (Context.TypeMaps.FindTypeMap(specialization, out mappedType) && !mappedType.IsIgnored)
                    continue;

                specialization.ExplicitlyIgnore();
            }

            // we only need a few members for marshalling so strip the rest
            switch (@class.Name)
            {
                case "basic_string":
                case "allocator":
                case "char_traits":
                    @class.GenerationKind = GenerationKind.Generate;
                    foreach (var specialization in from s in @class.Specializations
                             where !s.Arguments.Any(a =>
                                 s.UnsupportedTemplateArgument(a, Context.TypeMaps))
                             let arg = s.Arguments[0].Type.Type.Desugar()
                             where arg.IsPrimitiveType(PrimitiveType.Char)
                             select s)
                    {
                        specialization.GenerationKind = GenerationKind.Generate;
                        InternalizeSpecializationsInFields(specialization);
                    }
                    break;

                case "optional":
                case "vector":
                    @class.GenerationKind = GenerationKind.Generate;
                    foreach (var specialization in from s in @class.Specializations
                                                   where !s.Arguments.Any(a =>
                                                       s.UnsupportedTemplateArgument(a, Context.TypeMaps))
                                                   select s)
                    {
                        specialization.GenerationKind = GenerationKind.Generate;
                        InternalizeSpecializationsInFields(specialization);
                    }
                    return true;
            }

            Diagnostics.Warning("Ignoring unsupported std type: {0}", @class.QualifiedName);
            return false;
        }

        public override bool VisitEnumDecl(Enumeration @enum)
        {
            if (!base.VisitEnumDecl(@enum))
                return false;

            if (IsPrivateStdDecl(@enum))
            {
                @enum.ExplicitlyIgnore();
                return false;
            }

            return true;
        }

        public override bool VisitFunctionDecl(Function function)
        {
            if (!base.VisitFunctionDecl(function))
                return false;

            if (IsPrivateStdDecl(function))
            {
                function.ExplicitlyIgnore();
                return false;
            }

            return true;
        }
        
        public override bool VisitTypedefNameDecl(TypedefNameDecl typedef)
        {
            if (!base.VisitTypedefNameDecl(typedef))
                return false;

            if (IsPrivateStdDecl(typedef))
            {
                typedef.ExplicitlyIgnore();
                return false;
            }

            return true;
        }

        public override bool VisitVariableDecl(Variable variable)
        {
            if (!base.VisitVariableDecl(variable))
                return false;

            if (IsPrivateStdDecl(variable))
            {
                variable.ExplicitlyIgnore();
                return false;
            }

            return true;
        }
        public override bool VisitParameterDecl(Parameter parameter)
        {
            if (!base.VisitParameterDecl(parameter))
                return false;

            if (IsPrivateStdDecl(parameter))
            {
                parameter.ExplicitlyIgnore();
                return false;
            }

            return true;
        }

        private void InternalizeSpecializationsInFields(ClassTemplateSpecialization specialization)
        {
            foreach (Field field in specialization.Fields)
            {
                ASTUtils.CheckTypeForSpecialization(field.Type, specialization,
                    specialization =>
                    {
                        if (!specialization.IsExplicitlyGenerated &&
                            specialization.GenerationKind != GenerationKind.Internal)
                        {
                            specialization.GenerationKind = GenerationKind.Internal;
                            InternalizeSpecializationsInFields(specialization);
                        }
                    }, Context.TypeMaps, true);
            }
        }
    }
}
