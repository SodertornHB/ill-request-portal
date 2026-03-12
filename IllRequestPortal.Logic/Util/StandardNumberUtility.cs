using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public class StandardNumberUtil
    {
        public static class StandardNumberUtility
        {
            public static bool IsValidIsbnOrIssn(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return false;

                var normalized = Normalize(input);

                return IsValidIsbn10(normalized) || IsValidIsbn13(normalized) || IsValidIssn(normalized);
            }

            public static string GetStandardNumberType(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                var normalized = Normalize(input);

                if (IsValidIsbn10(normalized) || IsValidIsbn13(normalized))
                    return "isbn";

                if (IsValidIssn(normalized))
                    return "issn";

                return null;
            }

            public static string Normalize(string input)
            {
                return input
                    .Replace(" ", "")
                    .ToUpperInvariant();
            }

            public static bool IsValidIsbn10(string value)
            {
                if (value.Length != 10)
                    return false;

                int sum = 0;

                for (int i = 0; i < 9; i++)
                {
                    if (!char.IsDigit(value[i]))
                        return false;

                    sum += (value[i] - '0') * (10 - i);
                }

                int check = value[9] == 'X' ? 10 : (char.IsDigit(value[9]) ? value[9] - '0' : -1);

                if (check < 0)
                    return false;

                sum += check;

                return sum % 11 == 0;
            }

            public static bool IsValidIsbn13(string value)
            {
                if (value.Length != 13 || !value.All(char.IsDigit))
                    return false;

                int sum = 0;

                for (int i = 0; i < 12; i++)
                {
                    int digit = value[i] - '0';
                    sum += i % 2 == 0 ? digit : digit * 3;
                }

                int expected = (10 - (sum % 10)) % 10;
                int actual = value[12] - '0';

                return expected == actual;
            }

            public static bool IsValidIssn(string value)
            {
                if (value.Length != 8)
                    return false;

                int sum = 0;

                for (int i = 0; i < 7; i++)
                {
                    if (!char.IsDigit(value[i]))
                        return false;

                    sum += (value[i] - '0') * (8 - i);
                }

                int check = value[7] == 'X' ? 10 : (char.IsDigit(value[7]) ? value[7] - '0' : -1);

                if (check < 0)
                    return false;

                sum += check;

                return sum % 11 == 0;
            }
        }   
    }
}
