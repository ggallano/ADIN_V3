using System.Windows;

namespace ADIN.WPF.Components
{
    /// <summary>
    /// Interaction logic for LinkLengthBenchmarkDialog.xaml
    /// </summary>
    public partial class LinkLengthBenchmarkDialog : Window
    {
        public LinkLengthBenchmarkDialog()
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