using System.Windows;

namespace ADIN.WPF.Components
{
    /// <summary>
    /// Interaction logic for CableManualDialog.xaml
    /// </summary>
    public partial class CableManualDialog : Window
    {
        public CableManualDialog()
        {
            InitializeComponent();
        }

        public string ContentMessage
        {
            get
            {
                return this.TxtContentMessage.Text;
            }

            set
            {
                this.TxtContentMessage.Text = value;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
