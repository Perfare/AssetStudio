using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity_Studio
{
    public static class StringExtensions
    {
        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool Like(this string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }
    }
}
