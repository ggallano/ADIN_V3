// <copyright file="AboutCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class AboutCommand : CommandBase
    {
        private MenuItemViewModel _viewModel;

        public AboutCommand(MenuItemViewModel menuItemViewModel)
        {
            _viewModel = menuItemViewModel;
        }

        public override void Execute(object parameter)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}