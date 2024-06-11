using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ADIN.WPF.Components
{
    /// <summary>
    /// Interaction logic for TDRCalibrationBox.xaml
    /// </summary>
    public partial class TDRCalibrationBox : UserControl
    {
        public TDRCalibrationBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(TDRCalibrationBox));

        public string Header
        {
            get
            {
                return (string)GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(TDRCalibrationBox));

        public string FileName
        {
            get
            {
                return (string)GetValue(FileNameProperty);
            }
            set
            {
                SetValue(FileNameProperty, value);
            }
        }

        public static readonly DependencyProperty CalibrateCommandProperty =
            DependencyProperty.Register("CalibrateCommand", typeof(ICommand), typeof(TDRCalibrationBox));

        public ICommand CalibrateCommand
        {
            get
            {
                return (ICommand)GetValue(CalibrateCommandProperty);
            }
            set
            {
                SetValue(CalibrateCommandProperty, value);
            }
        }

        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(TDRCalibrationBox));

        public ICommand SaveCommand
        {
            get
            {
                return (ICommand)GetValue(SaveCommandProperty);
            }
            set
            {
                SetValue(SaveCommandProperty, value);
            }
        }

        public static readonly DependencyProperty LoadCommandProperty =
            DependencyProperty.Register("LoadCommand", typeof(ICommand), typeof(TDRCalibrationBox));

        public ICommand LoadCommand
        {
            get
            {
                return (ICommand)GetValue(LoadCommandProperty);
            }
            set
            {
                SetValue(LoadCommandProperty, value);
            }
        }

        public static readonly DependencyProperty ManualCommandProperty =
            DependencyProperty.Register("ManualCommand", typeof(ICommand), typeof(TDRCalibrationBox));

        public ICommand ManualCommand
        {
            get
            {
                return (ICommand)GetValue(ManualCommandProperty);
            }
            set
            {
                SetValue(ManualCommandProperty, value);
            }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(TDRCalibrationBox));

        public string ValueText
        {
            get
            {
                return (string)GetValue(ValueTextProperty);
            }
            set
            {
                SetValue(ValueTextProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("ValueBox", typeof(double), typeof(TDRCalibrationBox));

        public double ValueBox
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChangeValue", typeof(double), typeof(TDRCalibrationBox));

        public double SmallChangeValue
        {
            get
            {
                return (double)GetValue(SmallChangeProperty);
            }
            set
            {
                SetValue(SmallChangeProperty, value);
            }
        }
    }
}
