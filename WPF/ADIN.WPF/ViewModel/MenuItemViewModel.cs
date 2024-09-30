// <copyright file="MenuItemViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Commands;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class MenuItemViewModel : ViewModelBase
    {
        public MenuItemViewModel()
        {
            AboutCommand = new AboutCommand(this);
        }

        public ICommand AboutCommand { get; set; }
    }
}