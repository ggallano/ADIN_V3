using ADI.Register.Models;
using System.Windows;
using System.Windows.Controls;

namespace ADIN.WPF.Themes
{
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
            if (item is RegisterModel)
            {
                RegisterModel regDetails = (RegisterModel)item;
                if (regDetails.Access == "R")
                {
                    return this.NoEditStyle;
                }
                else
                {
                    return this.EditStyle;
                }
            }

            if (item is BitFieldModel)
            {
                BitFieldModel fieldDetails = (BitFieldModel)item;
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
