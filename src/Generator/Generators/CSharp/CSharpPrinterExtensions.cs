using System;
using CppSharp.AST;

namespace CppSharp.Generators.CSharp
{
    public static class CSharpPrinterExtensions
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
    };
}
