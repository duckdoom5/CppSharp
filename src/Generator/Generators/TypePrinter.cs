using CppSharp.AST;
using CppSharp.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators
{
    public interface ITypePrinterResult
    {
        public string ToString();
    }

    public abstract class TypePrinterResultBase : ITypePrinterResult
    {
        public static implicit operator string(TypePrinterResultBase result) =>
            result.ToString();

        public abstract override string ToString();
        
    }

    public class TypePrinterResult : TypePrinterResultBase
    {
        public string Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public StringBuilder TypePrefix { get; set; } = new();
        public StringBuilder TypeSuffix { get; set; } = new();
        public StringBuilder NamePrefix { get; set; } = new();
        public StringBuilder NameSuffix { get; set; } = new();
        public TypeMap TypeMap { get; set; }
        public GeneratorKind Kind { get; set; }

        public TypePrinterResult(string type = "", string nameSuffix = "")
        {
            Type = type;
            NameSuffix.Append(nameSuffix);
        }

        public void RemoveNamespace()
        {
            var index = Type.LastIndexOf('.');
            if (index != -1)
                Type = Type[(index + 1)..];
        }

        public static implicit operator TypePrinterResult(string type) =>
            new() { Type = type };

        public static implicit operator string(TypePrinterResult result) =>
           result.ToString();

        public override string ToString()
        {
            if (Kind == GeneratorKind.TypeScript)
                return $"{Name}{NameSuffix}: {Type}";

            var hasPlaceholder = Type.Contains("{0}");
            if (hasPlaceholder)
                return string.Format(Type, $"{NamePrefix}{Name}{NameSuffix}");

            var namePrefix = Name.Length > 0 && (NamePrefix.Length > 0 || Type.Length > 0) ?
                $"{NamePrefix} " : NamePrefix.ToString();

            TypePrefix.AppendIfNeeded(' ');

            return $"{TypePrefix}{Type}{TypeSuffix}{namePrefix}{Name}{NameSuffix}";
        }
    }
    
    public class TypeTypePrinterResult : TypePrinterResult
    {
        public string TypeQualifiers { get; set; }
        //public string Type { get; set; }
        public string TypeModifiers { get; set; }


        public static implicit operator string(TypeTypePrinterResult result) =>
            result.ToString();

        public override string ToString()
        {
            var namePrefix = Name.Length > 0 && (NamePrefix.Length > 0 || Type.Length > 0) ?
                $"{NamePrefix} " : NamePrefix.ToString();

            return new StringBuilder()
                .AppendJoinIfNeeded(' ', TypeQualifiers, $"{Type}{TypeModifiers}{TypeSuffix}{namePrefix}{Name}{NameSuffix}")
                .ToString();
        }
    }

    public class NamedTypePrinterResult : TypeTypePrinterResult
    {
        public string VariableName { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendJoinIfNeeded(' ', 
                    TypeQualifiers,
                    $"{Type}{TypeModifiers}",
                    VariableName)
                .ToString();
        }
    }

    public class CSVListPrinterResult : TypeTypePrinterResult
    {
        public string[] Values { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendJoinIfNeeded(',', Values)
                .ToString();
        }
    }

    public class TypePrinter : ITypePrinter<ITypePrinterResult>,
        IDeclVisitor<TypePrinterResult>
    {
        private readonly Stack<TypePrinterContextKind> contexts;
        private readonly Stack<MarshalKind> marshalKinds;
        private readonly Stack<TypePrintScopeKind> scopeKinds;

        public BindingContext Context { get; set; }
        public TypePrinterContextKind ContextKind => contexts.Peek();

        public MarshalKind MarshalKind => marshalKinds.Peek();

        public TypePrintScopeKind ScopeKind => scopeKinds.Peek();
        public bool IsGlobalQualifiedScope => ScopeKind == TypePrintScopeKind.GlobalQualified;

        public TypePrinter(BindingContext context, TypePrinterContextKind contextKind = TypePrinterContextKind.Managed)
        {
            Context = context;
            contexts = new Stack<TypePrinterContextKind>();
            marshalKinds = new Stack<MarshalKind>();
            scopeKinds = new Stack<TypePrintScopeKind>();
            PushContext(contextKind);
            PushMarshalKind(MarshalKind.Unknown);
            PushScope(TypePrintScopeKind.GlobalQualified);
        }

        public void PushContext(TypePrinterContextKind kind) => contexts.Push(kind);
        public TypePrinterContextKind PopContext() => contexts.Pop();

        public void PushMarshalKind(MarshalKind kind) => marshalKinds.Push(kind);
        public MarshalKind PopMarshalKind() => marshalKinds.Pop();

        public void PushScope(TypePrintScopeKind kind) => scopeKinds.Push(kind);
        public TypePrintScopeKind PopScope() => scopeKinds.Pop();

        public Parameter Parameter;

        #region Dummy implementations

        public virtual string ToString(CppSharp.AST.Type type)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitArrayType(ArrayType array,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitAttributedType(AttributedType attributed,
            TypeQualifiers quals)
        {
            return attributed.Modified.Visit(this);
        }

        public virtual ITypePrinterResult VisitBuiltinType(BuiltinType builtin,
            TypeQualifiers quals)
        {
            return VisitPrimitiveType(builtin.Type, quals);
        }

        public virtual ITypePrinterResult VisitCILType(CILType type, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitClassDecl(Class @class)
        {
            return VisitDeclaration(@class);
        }

        public virtual TypePrinterResult VisitClassTemplateDecl(ClassTemplate template)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitClassTemplateSpecializationDecl(
            ClassTemplateSpecialization specialization)
        {
            return VisitClassDecl(specialization);
        }

        public virtual ITypePrinterResult VisitDecayedType(DecayedType decayed,
            TypeQualifiers quals)
        {
            return decayed.Decayed.Visit(this);
        }

        public virtual TypePrinterResult VisitDeclaration(Declaration decl)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitDeclaration(Declaration decl, TypeQualifiers quals)
        {
            return VisitDeclaration(decl);
        }

        public virtual ITypePrinterResult VisitDelegate(FunctionType function)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitDependentNameType(
            DependentNameType dependent, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitDependentTemplateSpecializationType(
            DependentTemplateSpecializationType template, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitEnumDecl(Enumeration @enum)
        {
            return VisitDeclaration(@enum);
        }

        public virtual TypePrinterResult VisitEnumItemDecl(Enumeration.Item item)
        {
            return VisitDeclaration(@item);
        }

        public virtual TypePrinterResult VisitEvent(Event @event)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitFieldDecl(Field field)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitFriend(Friend friend)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitFunctionDecl(Function function)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitFunctionTemplateDecl(FunctionTemplate template)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitFunctionTemplateSpecializationDecl(FunctionTemplateSpecialization specialization)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitFunctionType(FunctionType function, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitInjectedClassNameType(
            InjectedClassNameType injected, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitMacroDefinition(MacroDefinition macro)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitMemberPointerType(
            MemberPointerType member, TypeQualifiers quals)
        {
            return member.QualifiedPointee.Visit(this);
        }

        public virtual TypePrinterResult VisitMethodDecl(Method method)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitNamespace(Namespace @namespace)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitNonTypeTemplateParameterDecl(
            NonTypeTemplateParameter nonTypeTemplateParameter)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitPackExpansionType(
            PackExpansionType packExpansionType, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitParameter(Parameter param,
            bool hasName = true)
        {
            Parameter = param;
            var type = param.QualifiedType.Visit(this);
            Parameter = null;

            return new NamedTypePrinterResult
            {
                Type = type.ToString(),
                VariableName = hasName ? param.Name : string.Empty
            };
        }

        public virtual TypePrinterResult VisitParameterDecl(Parameter parameter)
        {
            return (TypePrinterResult)VisitParameter(parameter, hasName: false);
        }

        public virtual ITypePrinterResult VisitParameters(IEnumerable<Parameter> @params,
            bool hasNames = true)
        {
            var args = new List<string>();

            foreach (var param in @params)
            {
                Parameter = param;
                args.Add(((TypePrinterResult)VisitParameter(param, hasNames)).Type);
            }

            Parameter = null;
            return new CSVListPrinterResult
            {
                Values = args.ToArray()
            };
        }

        public virtual ITypePrinterResult VisitPointerType(PointerType pointer,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitPrimitiveType(PrimitiveType type,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitProperty(Property property)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitTagType(TagType tag, TypeQualifiers quals)
        {
            if (tag.Declaration == null)
                return new TypePrinterResult();

            return tag.Declaration.Visit(this);
        }

        public virtual TypePrinterResult VisitTemplateParameterDecl(
            TypeTemplateParameter templateParameter)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitTemplateParameterSubstitutionType(
            TemplateParameterSubstitutionType param, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitTemplateParameterType(
            TemplateParameterType param, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitTemplateSpecializationType(
            TemplateSpecializationType template, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitTemplateTemplateParameterDecl(
            TemplateTemplateParameter templateTemplateParameter)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitTranslationUnit(TranslationUnit unit)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitTypeAliasDecl(TypeAlias typeAlias)
        {
            return VisitDeclaration(typeAlias);
        }

        public virtual TypePrinterResult VisitTypeAliasTemplateDecl(
            TypeAliasTemplate typeAliasTemplate)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitTypedefDecl(TypedefDecl typedef)
        {
            return VisitDeclaration(typedef);
        }

        public virtual TypePrinterResult VisitTypedefNameDecl(TypedefNameDecl typedef)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitTypedefType(TypedefType typedef,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitUnaryTransformType(
            UnaryTransformType unaryTransformType, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitUnresolvedUsingType(UnresolvedUsingType unresolvedUsingType, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitUnsupportedType(UnsupportedType type,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitVariableDecl(Variable variable)
        {
            return VisitDeclaration(variable);
        }

        public virtual TypePrinterResult VisitVarTemplateDecl(VarTemplate template)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitVarTemplateSpecializationDecl(
            VarTemplateSpecialization template)
        {
            throw new NotImplementedException();
        }

        public virtual TypePrinterResult VisitUnresolvedUsingDecl(UnresolvedUsingTypename unresolvedUsingTypename)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitVectorType(VectorType vectorType,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public virtual ITypePrinterResult VisitQualifiedType(QualifiedType type)
        {
            return type.Type.Visit(this, type.Qualifiers);
        }

        #endregion
    }
}