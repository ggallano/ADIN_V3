// <copyright file="LocalLoopbackParametersConverter.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1300_Eval.Themes.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using static TargetInterface.FirmwareAPI;

    /// <summary>
    /// Convertor for paramters for GePhyLoopbackConfig
    /// </summary>
    public class LocalLoopbackParametersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            LocalLoopbackParameters parameters = new LocalLoopbackParameters();

            if (values.Length == 3)
            {
                if (values[0] is string)
                {
                    LoopBackMode gePhyLb_sel;
                    if (Enum.TryParse((string)values[0], out gePhyLb_sel))
                    {
                        parameters.gePhyLb_selt = gePhyLb_sel;
                    }
                }

                if (values[1] is bool)
                {
                    parameters.isolateRx_st = (bool)values[1];
                }

                if (values[2] is bool)
                {
                    parameters.lbTxSup_st = (bool)values[2];
                }
            }

            return parameters;
        }

        /// <summary>
        /// Convert back
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
