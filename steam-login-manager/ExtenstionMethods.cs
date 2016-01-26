using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steam_login_manager
{
    internal static class ExtenstionMethods
    {
        public static string Repeat(this string str, int count)
        {
            Debug.Assert(count > 0);

            var sbulder = new StringBuilder(str.Length * count);

            for (int it = 0; it < count; ++it)
                sbulder.Append(str);

            return sbulder.ToString();
        }

        public static bool IsEmpty(this string str)
        {
            return str.Length == 0;
        }

        public static string Escape(this string str)
        {
            return str.Replace("\"", "\\\"");
        }
    }
}
