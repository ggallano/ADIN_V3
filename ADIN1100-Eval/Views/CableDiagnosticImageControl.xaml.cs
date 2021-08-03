//-----------------------------------------------------------------------
// <copyright file="CableDiagnosticImageControl.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

using System.Windows.Controls;

namespace ADIN1100_Eval.Views
{
    /// <summary>
    /// Interaction logic for CableDiagnosticImageControl.xaml
    /// </summary>
    public partial class CableDiagnosticImageControl : UserControl
    {
        public CableDiagnosticImageControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        //public static readonly DependencyProperty TextValueProperty = DependencyProperty.Register("FaultResult", typeof(object), typeof(CableDiagnosticImageControl), new PropertyMetadata(null));
    }
}
