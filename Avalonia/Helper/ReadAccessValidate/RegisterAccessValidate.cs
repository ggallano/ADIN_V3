// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Globalization;

namespace Helper.ReadAccessValidate
{
    public static class RegisterAccessValidate
    {
        public static bool ValidateInput(string inputString, out uint value)
        {
            bool result = false;
            value = 0;

            if (UInt32.TryParse(inputString, NumberStyles.HexNumber, null, out value))
                result = true;

            return result;
        }
    }
}
