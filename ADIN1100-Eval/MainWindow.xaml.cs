// <copyright file="MainWindow.xaml.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1300_Eval
{
    using System.Windows;
    using ADIN1300_Eval.ViewModel;
    using Telerik.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The event is called when about menu is clicked
        /// </summary>
        /// <param name="sender">The Main Window</param>
        /// <param name="e">The Router event arguments</param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Script1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //((MainWindowViewModel)this.DataContext).DeviceViewModel.SelectedScript1 = (string)((RadListBox)sender).SelectedValue;
        }

        private void Script2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).DeviceViewModel.SelectedScript2 = (string)((RadListBox)sender).SelectedValue;
        }

        private void Script3_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).DeviceViewModel.SelectedScript3 = (string)((RadListBox)sender).SelectedValue;
        }

        private void Script4_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).DeviceViewModel.SelectedScript4 = (string)((RadListBox)sender).SelectedValue;
        }
    }
}
