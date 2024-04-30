using ADI.Register.Models;
using ADIN.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace ADIN.WPF.View
{
    /// <summary>
    /// Interaction logic for RegisterListingView.xaml
    /// </summary>
    public partial class RegisterListingView : UserControl
    {
        public RegisterListingView()
        {
            InitializeComponent();
        }

        private void RadGridView_CellEditEnded(object sender, Telerik.Windows.Controls.GridViewCellEditEndedEventArgs e)
        {
            if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
            {
                if (e.Cell.DataContext is RegisterModel)
                {
                    RegisterListingViewModel registerViewModel = (RegisterListingViewModel)this.DataContext;
                    RegisterModel rdetails = (RegisterModel)e.Cell.DataContext;
                    uint newValue = 0;
                    if (ParseValue((string)e.NewData, out newValue))
                    {
                        registerViewModel.WriteRegister(rdetails.Name, newValue);
                    }
                }
            }
        }

        private bool ParseValue(string hexstring, out uint value)
        {
            if (hexstring.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                hexstring = hexstring.Substring(2);
            }

            return uint.TryParse(hexstring, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value);
        }

        private void RadGridView_RegisterSelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is RegisterModel)
                {
                    RegisterModel details = (RegisterModel)e.AddedItems[0];
                    System.Uri imguri = new Uri(string.Format("../Images/{0}", details.Image), UriKind.Relative);
                    BitmapImage ni = new BitmapImage(imguri);
                    //this.RegImage.Source = ni;
                }
            }
        }

        private void RadGridView_BeginningEdit(object sender, Telerik.Windows.Controls.GridViewBeginningEditRoutedEventArgs e)
        {
            if ((string)e.Cell.Column.Header == "Value")
            {
                RadGridView gridView = (RadGridView)sender;
                if (e.Row.Item is RegisterModel)
                {
                    RegisterModel rdetails = (RegisterModel)e.Row.Item;
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

        private void RadGridView_CellValidating(object sender, Telerik.Windows.Controls.GridViewCellValidatingEventArgs e)
        {
            if ((string)e.Cell.Column.Header == "Value")
            {
                RegisterListingViewModel deviceViewModel = (RegisterListingViewModel)this.DataContext;
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
                    if (e.Row.Item is BitFieldModel)
                    {
                        e.Handled = true;
                        BitFieldModel fieldDetails = (BitFieldModel)e.Row.Item;
                        uint maxValue = (1U << (int)fieldDetails.Width) - 1;
                        if (newValue > maxValue)
                        {
                            e.ErrorMessage = string.Format("Please enter values in the range of 0x0 - 0x{0:X}", maxValue);
                            e.IsValid = false;
                        }
                    }
                    else if (e.Row.Item is RegisterModel)
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

        private void RadGridView_PreparedCellForEdit(object sender, Telerik.Windows.Controls.GridViewPreparingCellForEditEventArgs e)
        {
            if ((string)e.Column.Header == "Value")
            {
                if (e.Row.Item is RegisterModel)
                {
                    RegisterModel rdetails = (RegisterModel)e.Row.Item;
                    var tb = e.EditingElement as TextBox;
                    tb.Text = rdetails.Value.ToString();
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
            if (e.ItemsSource is List<RegisterModel>)
            {
                List<RegisterModel> reglist = (List<RegisterModel>)e.ItemsSource;
                if (reglist.Count > 0)
                {
                    RegisterModel details = reglist[0];
                    System.Uri imguri = new Uri(string.Format("../Images/{0}", details.Image), UriKind.Relative);
                    BitmapImage ni = new BitmapImage(imguri);
                    //this.RegImage.Source = ni;
                }
            }
        }
    }
}
