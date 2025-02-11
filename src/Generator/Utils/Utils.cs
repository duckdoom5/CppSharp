﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CppSharp
{
    public static class IntHelpers
    {
        public static bool IsPowerOfTwo(this ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
    }

    public static class StringHelpers
    {
        public static string CommonPrefix(this string[] ss)
        {
            if (ss.Length == 0)
            {
                return "";
            }

            if (ss.Length == 1)
            {
                return ss[0];
            }

            int prefixLength = 0;

            foreach (char c in ss[0])
            {
                if (ss.Any(s => s.Length <= prefixLength || s[prefixLength] != c))
                {
                    return ss[0].Substring(0, prefixLength);
                }
                prefixLength++;
            }

            return ss[0]; // all strings identical
        }

        public static string[] SplitCamelCase(string input)
        {
            var str = Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled);
            return str.Trim().Split();
        }

        public static string Capitalize(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string ReplaceLineBreaks(this string lines, string replacement)
        {
            return lines.Replace("\r\n", replacement)
                        .Replace("\r", replacement)
                        .Replace("\n", replacement);
        }

        /// <summary>
        /// Get the string slice between the two indexes.
        /// Inclusive for start index, exclusive for end index.
        /// </summary>
        public static string Slice(this string source, int start, int end)
        {
            if (end < 0)
                end = source.Length + end;

            return source.Substring(start, end - start);
        }

        public static void TrimUnderscores(this StringBuilder stringBuilder)
        {
            while (stringBuilder.Length > 0 && stringBuilder[0] == '_')
                stringBuilder.Remove(0, 1);
            while (stringBuilder.Length > 0 && stringBuilder[^1] == '_')
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }

        /// <summary>
        /// Appends a character to the string builder if it is not already the last character.
        /// Note: Empty strings are not appended to.
        /// </summary>
        public static void AppendIfNeeded(this StringBuilder self, char toAppend)
        {
            if (self.Length == 0)
                return;

            if (self[^1] != toAppend)
                self.Append(toAppend);
        }

        public static StringBuilder AppendJoinIfNeeded(this StringBuilder self, char separator, params string[] values)
        {
            return AppendJoinIfNeededCore(self, separator, values);
        }
        
        public static StringBuilder AppendJoinIfNeeded(this StringBuilder self, char separator, params object[] values)
        {
            return AppendJoinIfNeededCore(self, separator, values);
        }

        private static StringBuilder AppendJoinIfNeededCore(this StringBuilder self, char separator, ReadOnlySpan<object> values)
        {
            if (values.IsEmpty)
                return self;
            
            for (int i = 0; i < values.Length; i++)
            {
                self.AppendIfNeeded(separator);
                self.Append(values[i]!);
            }
            return self;
        }
    }

    public static class LinqHelpers
    {
        public static IEnumerable<T> WithoutLast<T>(this IEnumerable<T> xs)
        {
            T lastX = default(T);

            var first = true;
            foreach (var x in xs)
            {
                if (first)
                    first = false;
                else
                    yield return lastX;
                lastX = x;
            }
        }
    }

    public static class AssemblyHelpers
    {
        public static IEnumerable<Type> FindDerivedTypes(this Assembly assembly,
                                                         Type baseType)
        {
            return assembly.GetTypes()
                .Where(type => !type.FullName.Contains("CppSharp.MSVCToolchain"))
                .Where(baseType.IsAssignableFrom);
        }
    }

    public static class PathHelpers
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            var path1 = fromPath.Trim('\\', '/');
            var path2 = toPath.Trim('\\', '/');

            var uri1 = new Uri("c:\\" + path1 + "\\");
            var uri2 = new Uri("c:\\" + path2 + "\\");

            return uri1.MakeRelativeUri(uri2).ToString();
        }
    }
}
