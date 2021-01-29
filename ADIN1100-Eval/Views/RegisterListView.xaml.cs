// <copyright file="RegisterListView.xaml.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1300_Eval.Views
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using TargetInterface;
    using Telerik.Windows.Controls;
    using Utilities.JSONParser.JSONClasses;
    using ViewModel;

    /// <summary>
    /// Interaction logic for RegisterListView.xaml
    /// </summary>
    public partial class RegisterListView : UserControl
    {
        public RegisterListView()
        {
            this.InitializeComponent();
        }

        private void RadGridView_BitFieldSelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
        }

        private void RadGridView_RegisterSelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is RegisterDetails)
                {
                    RegisterDetails details = (RegisterDetails)e.AddedItems[0];
                    System.Uri imguri = new Uri(string.Format("../Images/Yoda/{0}", details.Image), UriKind.Relative);
                    BitmapImage ni = new BitmapImage(imguri);
                    this.RegImage.Source = ni;
                }
            }
        }

        /// <summary>
        /// The BeginningEdit event occurs when the cell is about to enter into EditMode.
        /// Only the "Value" column (full register or field) is possible to edit and this
        /// is not allow if there is only read-only fields present.
        /// </summary>
        /// <param name="sender">The sender argument contains the RadGridView. This argument is of type object, but can be cast to the RadGridView type.</param>
        /// <param name="e">A GridViewBeginningEditRoutedEventArgs object.</param>
        private void RadGridView_BeginningEdit(object sender, Telerik.Windows.Controls.GridViewBeginningEditRoutedEventArgs e)
        {
            if ((string)e.Cell.Column.Header == "Value")
            {
                RadGridView gridView = (RadGridView)sender;
                if (e.Row.Item is RegisterBase)
                {
                    RegisterBase rdetails = (RegisterBase)e.Row.Item;
                    if (rdetails.Access == "R")
                    {
                        e.Cancel = true;
                    }
                }

                if (e.Cancel)
                {
                    ToolTipService.SetToolTip(e.Cell, "Editing the value is not possible due to the underlying register value being read-only.");
                }
            }
        }

        /// <summary>
        /// The CellEditEnded occurs when cell validation is passed successfully and new data is committed to the RadGridView.ItemsSource.
        /// </summary>
        /// <param name="sender">The sender argument contains the RadGridView. This argument is of type object, but can be cast to the RadGridView type.</param>
        /// <param name="e">A GridViewCellEditEndedEventArgs  object.</param>
        private void RadGridView_CellEditEnded(object sender, Telerik.Windows.Controls.GridViewCellEditEndedEventArgs e)
        {
            if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
            {
                if (e.Cell.DataContext is RegisterBase)
                {
                    DeviceViewModel deviceViewModel = (DeviceViewModel)this.DataContext;
                    RegisterBase rdetails = (RegisterBase)e.Cell.DataContext;
                    uint newValue = 0;
                    if (this.ParseValue((string)e.NewData, out newValue))
                    {
                        deviceViewModel.WriteRegister(rdetails.MMap, rdetails.Name, newValue);
                    }
                }
            }
        }

        /// <summary>
        /// Parse a hex string value
        /// </summary>
        /// <param name="hexstring">Input hex string</param>
        /// <param name="value">Ouutput value</param>
        /// <returns>True if parse ok</returns>
        private bool ParseValue(string hexstring, out uint value)
        {
            if (hexstring.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                hexstring = hexstring.Substring(2);
            }

            return uint.TryParse(hexstring, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value);
        }

        private void RadGridView_CellValidating(object sender, Telerik.Windows.Controls.GridViewCellValidatingEventArgs e)
        {
            if ((string)e.Cell.Column.Header == "Value")
            {
                DeviceViewModel deviceViewModel = (DeviceViewModel)this.DataContext;
                uint newValue = 0;
                if (e.NewValue is string)
                {
                    if (!this.ParseValue((string)e.NewValue, out newValue))
                    {
                        e.ErrorMessage = string.Format("Not able to interpret '{0}' as a hex value", (string)e.NewValue);
                        e.IsValid = false;
                    }
                    else
                    {
                        e.IsValid = true;
                    }
                }
                else
                {
                    if (e.NewValue is uint)
                    {
                        newValue = (uint)e.NewValue;
                    }
                }

                if (e.IsValid)
                {
                    if (e.Row.Item is FieldDetails)
                    {
                        e.Handled = true;
                        FieldDetails fieldDetails = (FieldDetails)e.Row.Item;
                        uint maxValue = (1U << (int)fieldDetails.Width) - 1;
                        if (newValue > maxValue)
                        {
                            e.ErrorMessage = string.Format("Please enter values in the range of 0x0 - 0x{0:X}", maxValue);
                            e.IsValid = false;
                        }
                    }
                    else if (e.Row.Item is RegisterDetails)
                    {
                        e.Handled = true;
                        if (newValue > 0xFFFF)
                        {
                            e.ErrorMessage = string.Format("Please enter values in the range of 0x0000 - 0xFFFF");
                            e.IsValid = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The PreparingCellForEdit event fires after the BeginningEdit event.
        /// </summary>
        /// <param name="sender">The sender argument contains the RadGridView. This argument is of type object, but can be cast to the RadGridView type.</param>
        /// <param name="e">A GridViewPreparingCellForEditEventArgs   object.</param>
        private void RadGridView_PreparedCellForEdit(object sender, GridViewPreparingCellForEditEventArgs e)
        {
            if ((string)e.Column.Header == "Value")
            {
                if (e.Row.Item is RegisterBase)
                {
                    RegisterBase rdetails = (RegisterBase)e.Row.Item;
                    var tb = e.EditingElement as TextBox;
                    tb.Text = rdetails.ToString();
                    if (tb.Text.StartsWith("0x"))
                    {
                        tb.SelectionStart = 2;
                        tb.SelectionLength = tb.Text.Length - 2;
                    }
                }
            }
        }

        private void gvRegisters_DataLoading(object sender, Telerik.Windows.Controls.GridView.GridViewDataLoadingEventArgs e)
        {

            if (e.ItemsSource is System.Collections.ObjectModel.ObservableCollection<RegisterDetails>)
            {
                System.Collections.ObjectModel.ObservableCollection<RegisterDetails> reglist = (System.Collections.ObjectModel.ObservableCollection<RegisterDetails>)e.ItemsSource;
                if (reglist.Count > 0)
                {
                    RegisterDetails details = reglist[0];
                    System.Uri imguri = new Uri(string.Format("../Images/Yoda/{0}", details.Image), UriKind.Relative);
                    BitmapImage ni = new BitmapImage(imguri);
                    this.RegImage.Source = ni;
                }
            }
        }
    }
}
