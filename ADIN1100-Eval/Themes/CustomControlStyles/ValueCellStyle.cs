// <copyright file="ValueCellStyle.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.Themes.CustomControlStyles
{
    using System.Windows;
    using System.Windows.Controls;
    using Utilities.JSONParser.JSONClasses;

    /// <summary>
    /// Style the value cell
    /// </summary>
    public class ValueCellStyle : StyleSelector
    {
        /// <summary>
        /// Gets or sets cell can be editted
        /// </summary>
        public Style EditStyle { get; set; }

        /// <summary>
        /// Gets or sets cell CANNOT be editted
        /// </summary>
        public Style NoEditStyle { get; set; }

        /// <summary>
        /// Select the style of the Value cell
        /// </summary>
        /// <param name="item">IDK 1</param>
        /// <param name="container">IDK 2</param>
        /// <returns>Style</returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is RegisterDetails)
            {
                RegisterDetails regDetails = (RegisterDetails)item;
                if (regDetails.Access == "R")
                {
                    return this.NoEditStyle;
                }
                else
                {
                    return this.EditStyle;
                }
            }

            if (item is FieldDetails)
            {
                FieldDetails fieldDetails = (FieldDetails)item;
                if (fieldDetails.Access == "R")
                {
                    return this.NoEditStyle;
                }
                else
                {
                    return this.EditStyle;
                }
            }

            return this.NoEditStyle;
        }
    }
}
