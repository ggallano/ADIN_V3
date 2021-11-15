// <copyright file="CalibrateCableDialog.xaml.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2021 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.Views
{
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

    /// <summary>
    /// Interaction logic for CalibrateCableDialog.xaml
    /// </summary>
    public partial class CalibrateCableDialog : Window
    {
        public CalibrateCableDialog()
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
