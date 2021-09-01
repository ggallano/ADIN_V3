// <copyright file="BoardNameParser.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2021 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace Utilities
{
    using System.Text.RegularExpressions;

    public static class BoardNameParser
    {
        private const string PATTERN = @"ADIN\d{4}";

        public static Match GetBoardNameType(string boardInfo)
        {
            Regex rg = new Regex(PATTERN);
            Match matchedBoardName = rg.Match(boardInfo);

            return matchedBoardName;
        }
    }
}
