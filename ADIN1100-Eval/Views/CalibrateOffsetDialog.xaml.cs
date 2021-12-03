//-----------------------------------------------------------------------
// <copyright file="CalibrateOffsetDialog.xaml.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2021 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace ADIN1100_Eval.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for FaultDetectorDialog.xaml
    /// </summary>
    public partial class CalibrateOffsetDialog : Window
    {
        public CalibrateOffsetDialog()
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
