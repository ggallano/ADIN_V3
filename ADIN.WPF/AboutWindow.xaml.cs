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
using System.Windows.Shapes;

namespace ADIN.WPF
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The event called when OK is pressed, that closes this window.
        /// </summary>
        /// <param name="sender">The OK button pressed</param>
        /// <param name="e">The routed event arguments</param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
