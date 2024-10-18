// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.ViewModels;

namespace ADIN.Avalonia.Commands
{
    public class ClearLogCommand : CommandBase
    {
        private LogActivityViewModel _viewModel;

        public ClearLogCommand(LogActivityViewModel logActivityViewModel)
        {
            _viewModel = logActivityViewModel;
        }

        public override void Execute(object parameter)
        {
            _viewModel.LogMessages.Clear();
        }
    }
}
