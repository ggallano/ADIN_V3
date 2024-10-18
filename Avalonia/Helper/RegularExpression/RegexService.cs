// <copyright file="RegexService.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Globalization;
using System.Text.RegularExpressions;

namespace Helper.RegularExpression
{
    public static class RegexService
    {
        private static Regex _regExpression;
        private static MatchCollection _matches;
        private static Match _match;

        public static List<decimal> ExtractNumberData(string text, string expr = @"(\d+(\.|\,)?\d*)|(?<=\=)(\d+\.?\d*)")
        {
            List<decimal> result = new List<decimal>();

            _regExpression = new Regex(expr);
            _matches = _regExpression.Matches(text);

            foreach (var match in _matches)
            {
                result.Add(decimal.Parse(match.ToString(), CultureInfo.InvariantCulture));
            }

            return result;
        }

        public static string ExtractFaultType(string text, string expr = "open|short")
        {
            string fault = string.Empty;

            _regExpression = new Regex(expr);
            _match = _regExpression.Match(text);

            return _match.ToString();
        }

        public static string ExtractNVP(string text, string expr = @"NVP\=(\d+.\d+)")
        {
            string result = string.Empty;

            _regExpression = new Regex(expr);
            _match = _regExpression.Match(text);

            return _match.ToString();
        }
    }
}
