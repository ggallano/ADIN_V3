// <copyright file="ReadContent.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Helper.ReadFile
{
    public static class ReadContent
    {
        public static string[] Read(string fileName)
        {
            string[] values = null;

            using (StreamReader sr = new StreamReader(fileName))
            {
                string content = sr.ReadToEnd();
                values = content.Split(',');
            }

            return values;
        }
    }
}
