using System;
using System.Collections.Generic;
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

namespace ADIN1100_Eval.Views
{
    /// <summary>
    /// Interaction logic for Framechecker.xaml
    /// </summary>
    public partial class Framechecker : UserControl
    {
        public Framechecker()
        {
            InitializeComponent();
            chkEnableMacAddress_Click(this, null);
        }

        /// <summary>
        /// enables or disables of MAC Addresses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkEnableMacAddress_Click(object sender, RoutedEventArgs e)
        {
            if (this.chkEnableMacAddress.IsChecked == true)
            {
                this.txtDestMacAddress.IsEnabled = true;
                this.txtSrcMacAddress.IsEnabled = true;
            }
            else
            {
                this.txtDestMacAddress.IsEnabled = false;
                this.txtSrcMacAddress.IsEnabled = false;
            }
        }
    }
}
