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

namespace ADIN.WPF.Components
{
    /// <summary>
    /// Interaction logic for TDRDiagram.xaml
    /// </summary>
    public partial class TDRDiagram : UserControl
    {
        public TDRDiagram()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FaultDetectCommandProperty =
            DependencyProperty.Register("FaultDetectCommand", typeof(ICommand), typeof(TDRDiagram));

        public ICommand FaultDetectCommand
        {
            get
            {
                return (ICommand)GetValue(FaultDetectCommandProperty);
            }
            set
            {
                SetValue(FaultDetectCommandProperty, value);
            }
        }

        public static readonly DependencyProperty IsFaultVisibilityProperty =
            DependencyProperty.Register("IsFaultVisibility", typeof(bool), typeof(TDRDiagram));

        public bool IsFaultVisibility
        {
            get
            {
                return (bool)GetValue(IsFaultVisibilityProperty);
            }
            set
            {
                SetValue(IsFaultVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty DistToFaultProperty =
            DependencyProperty.Register("DistToFault", typeof(string), typeof(TDRDiagram));

        public string DistToFault
        {
            get
            {
                return (string)GetValue(FaultBackgroundBrushProperty);
            }
            set
            {
                SetValue(FaultBackgroundBrushProperty, value);
            }
        }

        public static readonly DependencyProperty FaultBackgroundBrushProperty =
            DependencyProperty.Register("FaultBackgroundBrush", typeof(Brush), typeof(TDRDiagram));

        public Brush FaultBackgroundBrush
        {
            get
            {
                return (Brush)GetValue(FaultBackgroundBrushProperty);
            }
            set
            {
                SetValue(FaultBackgroundBrushProperty, value);
            }
        }

        public static readonly DependencyProperty FaultStateProperty =
            DependencyProperty.Register("FaultState", typeof(string), typeof(TDRDiagram));

        public string FaultState
        {
            get
            {
                return (string)GetValue(FaultStateProperty);
            }
            set
            {
                SetValue(FaultStateProperty, value);
            }
        }


    }
}
