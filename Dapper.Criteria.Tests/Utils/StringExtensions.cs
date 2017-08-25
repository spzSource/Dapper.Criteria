using System;
using System.Text.RegularExpressions;

namespace Dapper.Criteria.Tests.Utils
{
    public static class StringExtensions
    {
        public static string Simplify(this string source)
        {
            return new Regex("\\s+").Replace(
                source
                    .Replace("\\r\\n", " ")
                    .Replace("\\r", " ")
                    .Replace("\\n", " ")
                    .Replace(Environment.NewLine, " "),
                 " ")
                .Trim()
                .Replace("  ", " ");
        }
    }
}