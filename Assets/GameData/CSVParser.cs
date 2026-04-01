using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Data
{
    public class CSVParser
    {
        public static string[] ParseCSV(string csvText)
        {
            string[] result = csvText.Trim().Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

            return result;
        }
        public static long[] ParseLongArray(string field)
        {
            if (string.IsNullOrEmpty(field) || field == "-1") return new long[0];
    
            string[] elements = field.Split('|');
            long[] result = new long[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                long.TryParse(elements[i], out result[i]);
            }
            return result;
        }
    }
}