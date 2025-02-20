using System;
using System.Collections.Generic;
using System.Linq;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Generators;
using CppSharp.Generators.C;
using CppSharp.Generators.CSharp;

using static CppSharp.CodeGeneratorHelpers;

namespace CppSharp
{
    class DeclDeclarationsCodeGenerator : StmtDeclarationsCodeGenerator
    {
        public DeclDeclarationsCodeGenerator(BindingContext context,
            IEnumerable<Declaration> declarations)
            : base(context, declarations)
        {
        }

        public override void GenerateIncludes()
        {
            WriteInclude("Sources.h", CInclude.IncludeKind.Quoted);
            WriteInclude("Type.h", CInclude.IncludeKind.Quoted);
            WriteInclude("string", CInclude.IncludeKind.Angled);
            WriteInclude("vector", CInclude.IncludeKind.Angled);
        }

        public override void GenerateForwardDecls()
        {
            WriteLine("class Type;");
            WriteLine("class QualifiedType;");
            WriteLine("class TemplateArgument;");
        }

        public override bool VisitClassDecl(Class @class)
        {
            if (!@class.IsGenerated)
                return false;

            GenerateClassSpecifier(@class);
            NewLine();

            WriteOpenBraceAndIndent();

            PushBlock();
            VisitDeclContext(@class);
            PopBlock(NewLineKind.Always);

            WriteLine($"public:");
            Indent();

            WriteLine($"{@class.Name}();");

            foreach (var method in @class.Methods)
            {
                if (SkipMethod(method))
                    continue;

                GenerateMethod(method);
            }

            foreach (var property in @class.Properties)
            {
                if (SkipProperty(property))
                    continue;

                GenerateProperty(property);
            }

            Unindent();
            UnindentAndWriteCloseBrace();

            return true;
        }

        protected void GenerateMethod(Method method)
        {
            // Handle method parameter types
            foreach (var param in method.Parameters)
            {
                ValidateMethodParameter(param);
            }

            var iteratorType = GetIteratorType(method);
            var iteratorTypeName = GetIteratorTypeName(iteratorType, CodeGeneratorHelpers.CppTypePrinter);
            
            // Validate return type
            if (method.ReturnType.Type.IsPointer())
            {
                var returnTypeName = ValidateMethodReturnType(method.ReturnType.Type);
                WriteLine($"{returnTypeName} Get{method.Name}(uint i);");
            }
            else
                WriteLine($"{iteratorTypeName} Get{method.Name}(uint i);");

            WriteLine($"uint Get{method.Name}Count();");
        }

        private string ValidateMethodReturnType(AST.Type type)
        {
            if (type.IsPointerTo(out TagType tagType))
            {
                var pointeeType = tagType.Declaration?.Visit(CodeGeneratorHelpers.CppTypePrinter).Type ?? "void";
                if (pointeeType.Contains("Decl") || pointeeType.Contains("Stmt"))
                    return $"AST::{pointeeType}*";
                return $"{pointeeType}*";
            }
            
            return type.Visit(CodeGeneratorHelpers.CppTypePrinter).ToString();
        }

        private void ValidateMethodParameter(Parameter param)
        {
            if (param.Type.IsPointer())
            {
                var pointee = param.Type.GetFinalPointee();
                if (pointee is ArrayType || pointee is FunctionType)
                {
                    param.ExplicitlyIgnore();
                    return;
                }
            }

            // Ignore parameters with template types that we don't handle
            if (param.Type is TemplateSpecializationType template)
            {
                if (!template.Template.TemplatedDecl.Name.Contains("Decl") &&
                    !template.Template.TemplatedDecl.Name.Contains("Stmt"))
                {
                    param.ExplicitlyIgnore();
                }
            }
        }

        protected void GenerateProperty(Property property)
        {
            var typeName = GetDeclTypeName(property);
            var fieldName = GetDeclName(property);

            // Handle declaration-specific types
            if (property.Type.IsPointerTo(out TagType tagType) && tagType.Declaration != null)
            {
                var pointeeType = tagType.Declaration.Visit(CodeGeneratorHelpers.CppTypePrinter).Type;
                if (pointeeType.Contains("Decl") || pointeeType.Contains("Stmt"))
                    WriteLine($"AST::{pointeeType}* {fieldName};");
                else
                    WriteLine($"{pointeeType}* {fieldName};");

                // Generate getter if needed
                if (property.GetMethod != null)
                {
                    WriteLine($"AST::{pointeeType}* Get{FirstLetterToUpperCase(fieldName)}() const;");
                }

                // Generate setter if needed
                if (property.SetMethod != null)
                {
                    WriteLine($"void Set{FirstLetterToUpperCase(fieldName)}(AST::{pointeeType}* value);");
                }
            }
            else
            {
                WriteLine($"{typeName} {fieldName};");
                
                // Generate getter if needed
                if (property.GetMethod != null)
                {
                    WriteLine($"{typeName} Get{FirstLetterToUpperCase(fieldName)}() const;");
                }

                // Generate setter if needed
                if (property.SetMethod != null)
                {
                    WriteLine($"void Set{FirstLetterToUpperCase(fieldName)}({typeName} value);");
                }
            }
        }
    }

    class DeclDefinitionsCodeGenerator : StmtDefinitionsCodeGenerator
    {
        public DeclDefinitionsCodeGenerator(BindingContext context,
            IEnumerable<Declaration> declarations)
            : base(context, declarations)
        {
        }

        public override void GenerateIncludes()
        {
            GenerateCommonIncludes();
            WriteInclude("Decl.h", CInclude.IncludeKind.Quoted);
            WriteInclude("Type.h", CInclude.IncludeKind.Quoted);
        }

        public override bool VisitClassDecl(Class @class)
        {
            if (!@class.IsGenerated)
                return false;

            WriteLine($"{@class.Name}::{@class.Name}()");
            GenerateMemberInits(@class);
            WriteOpenBraceAndIndent();
            UnindentAndWriteCloseBrace();
            NewLine();

            foreach (var method in @class.Methods)
            {
                if (SkipMethod(method))
                    continue;

                var iteratorType = GetIteratorType(method);
                string iteratorTypeName = GetIteratorTypeName(iteratorType,
                    CodeGeneratorHelpers.CppTypePrinter);

                WriteLine($"DEF_VECTOR({@class.Name}, {iteratorTypeName}, {method.Name})");
                NewLine();
            }

            return true;
        }
    }

    internal class DeclASTConverterCodeGenerator : ASTConverterCodeGenerator
    {
        public DeclASTConverterCodeGenerator(BindingContext context, IEnumerable<Declaration> declarations)
            : base(context, declarations)
        {
        }

        public override string BaseTypeName => "Decl";
        public override Enumeration ClassKindEnum { get; init; }

        public override bool IsAbstractClassKind(Class kind)
        {
            return CodeGeneratorHelpers.IsAbstractDecl(kind);
        }

        public override void GenerateSwitchCases(IEnumerable<string> classes)
        {
            foreach (var className in classes)
            {
                WriteLine($"case Parser.AST.{BaseTypeName}Kind.{className}:");
                WriteLineIndent($"return Visit{className}({ParamName} as Parser.AST.{className}Decl);");
            }

            WriteLine("default:");
            WriteLineIndent($"throw new System.NotImplementedException(" +
                $"{ParamName}.DeclKind.ToString());");
        }
    }
}