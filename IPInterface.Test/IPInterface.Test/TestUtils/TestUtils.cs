using System;
using System.Collections.Generic;
using System.Text;

namespace IPInterface.Test
{
    internal static class TestUtils
    {
        public static string RepeatStrNTimes(string str, int n)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < n; i++)
                sb.Append(str);
            return sb.ToString();
        }
    }
}
