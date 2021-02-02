// <copyright file="DeviceView.xaml.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.Views
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using ViewModel;

    /// <summary>
    /// Interaction logic for DeviceView.xaml
    /// </summary>
    public partial class DeviceView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceView"/> class.
        /// </summary>
        public DeviceView()
        {
            this.InitializeComponent();
        }

        private void DongleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DongleList.SelectedItem != null)
            {
                Trace.WriteLine(this.DongleList.SelectedItem.ToString());
            }
            else
            {
                Trace.WriteLine("No Device is selected.");
            }
        }

        private void LinkPropertiesSummary_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }
    }
}