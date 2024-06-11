using ADIN.WPF.Components;
using System.Windows;
using System.Windows.Input;

internal static class TDRCalibrationBoxHelpers
{

    public static readonly DependencyProperty CalibrateCommandProperty =
        DependencyProperty.Register("CalibrateCommand", typeof(ICommand), typeof(TDRCalibrationBox));
}