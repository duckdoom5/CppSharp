using System;
using System.Collections.Generic;
using CppSharp.AST;

namespace CppSharp.Generators.C
{
    public static class CppPrinterExtensions
    {
        public static string ToCodeString(this ExceptionSpecType self) =>
            self switch
            {
                ExceptionSpecType.BasicNoexcept => "noexcept",
                ExceptionSpecType.NoexceptTrue => "noexcept(true)",
                ExceptionSpecType.NoexceptFalse => "noexcept(false)",
                // TODO: research and handle the remaining cases
                ExceptionSpecType.Dynamic or
                ExceptionSpecType.DynamicNone or
                ExceptionSpecType.DependentNoexcept or
                ExceptionSpecType.MSAny or
                ExceptionSpecType.Unevaluated or
                ExceptionSpecType.Uninstantiated or
                ExceptionSpecType.Unparsed or
                ExceptionSpecType.None => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
            };
        
        public static string ToCodeString(this TypeQualifiers self)
        {
            return string.Join(" ", EnumerateQualifiers());

            IEnumerable<string> EnumerateQualifiers()
            {
                if (self.IsConst)
                    yield return "const";
                if (self.IsVolatile)
                    yield return "volatile";
            }
        }

        public static string ToCodeString(this PrimitiveType self, CppTypePrintFlavorKind printFlavorKind) =>
            self switch
            {
                PrimitiveType.Bool => "bool",
                PrimitiveType.Void => "void",
                PrimitiveType.Char16 => "char16_t",
                PrimitiveType.Char32 => "char32_t",
                PrimitiveType.WideChar => "wchar_t",
                PrimitiveType.Char => "char",
                PrimitiveType.SChar => "signed char",
                PrimitiveType.UChar => "unsigned char",
                PrimitiveType.Short => "short",
                PrimitiveType.UShort => "unsigned short",
                PrimitiveType.Int => "int",
                PrimitiveType.UInt => "unsigned int",
                PrimitiveType.Long => "long",
                PrimitiveType.ULong => "unsigned long",
                PrimitiveType.LongLong => "long long",
                PrimitiveType.ULongLong => "unsigned long long",
                PrimitiveType.Int128 => "__int128_t",
                PrimitiveType.UInt128 => "__uint128_t",
                PrimitiveType.Half => "__fp16",
                PrimitiveType.Float => "float",
                PrimitiveType.Double => "double",
                PrimitiveType.LongDouble => "long double",
                PrimitiveType.Float128 => "__float128",
                PrimitiveType.IntPtr => "void*",
                PrimitiveType.UIntPtr => "uintptr_t",

                PrimitiveType.Null when printFlavorKind == CppTypePrintFlavorKind.Cpp => "std::nullptr_t",
                PrimitiveType.Null => "NULL",

                PrimitiveType.String when printFlavorKind == CppTypePrintFlavorKind.C => "const char*",
                PrimitiveType.String when printFlavorKind == CppTypePrintFlavorKind.Cpp => "std::string",
                PrimitiveType.String when printFlavorKind == CppTypePrintFlavorKind.ObjC => "NSString",

                PrimitiveType.Decimal when printFlavorKind == CppTypePrintFlavorKind.ObjC => "NSDecimalNumber",
                PrimitiveType.Decimal => "_Decimal32",

                _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
            };

        public static string ToCodeString(this PointerType.TypeModifier modifier)
        {
            return modifier switch
            {
                PointerType.TypeModifier.Value => "[]",
                PointerType.TypeModifier.Pointer => "*",
                PointerType.TypeModifier.LVReference => "&",
                PointerType.TypeModifier.RVReference => "&&",
                _ => string.Empty
            };
        }
    };
}