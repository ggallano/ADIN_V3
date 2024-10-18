// <copyright file="SignalToNoiseRatio.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace Helper.SignalToNoiseRatio
{
    public static class SignalToNoiseRatio
    {
        public static double GigabitCompute(double mseValue)
        {
            return 10 * Math.Log10(mseValue / 1024);
        }

        public static double T1LCompute(double mseValue)
        {
            // Formula:
            // where mse is the value from the register, and sym_pwr_exp is a constant 0.64423.
            // mse_db = 10 * log10((mse / 218) / sym_pwr_exp)
            double sym_pwr_exp = 0.64423;

            return 10 * Math.Log10((mseValue / Math.Pow(2, 18)) / sym_pwr_exp);
        }
    }
}
