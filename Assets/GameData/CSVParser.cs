using System;

namespace GameData
{
    public static class CsvParser
    {
        public static string[] ParseCSV(string csvText)
        {
            string[] result = csvText.Trim().Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

            return result;
        }
        public static long[] ParseLongArray(string field)
        {
            if (string.IsNullOrEmpty(field) || field == "-1") return Array.Empty<long>();
    
            var elements = field.Split('|');
            var result = new long[elements.Length];
            for (var i = 0; i < elements.Length; i++)
            {
                long.TryParse(elements[i], out result[i]);
            }
            return result;
        }

        public static string[] ParseArray(string field)
        {
            return string.IsNullOrEmpty(field) ? Array.Empty<string>() : field.Split('|');
        }

        public static int[] ParseIntArray(string field)
        {
            if (string.IsNullOrEmpty(field)) return Array.Empty<int>();
            
            var elements = field.Split('|');
            var result = new int[elements.Length];
            for (var i = 0; i < elements.Length; i++)
            {
                int.TryParse(elements[i], out result[i]);
            }
            return result;
        }
    }
}