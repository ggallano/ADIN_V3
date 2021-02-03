//-----------------------------------------------------------------------
// <copyright file="AboutWindow.xaml.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace ADIN1100_Eval
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for AboutWindow
    /// </summary>
    public partial class AboutWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutWindow"/> class.
        /// </summary>
        public AboutWindow()
        {
            this.InitializeComponent();
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
