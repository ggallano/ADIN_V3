// <copyright file="LogWindowSaveCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Helper.Feedback;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;

namespace ADIN.Avalonia.Commands
{
    public class LogWindowSaveCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private LogActivityViewModel _viewModel;

        public LogWindowSaveCommand(LogActivityViewModel logActivityViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = logActivityViewModel;
            _selectedDeviceStore = selectedDeviceStore;
        }

        public override async void Execute(object parameter)
        {
            DateTime timeNow = DateTime.Now;
            string fileName = "ActivityLog_" + timeNow.ToLongDateString() + "_" + timeNow.Hour + "_ " + timeNow.Minute + "_" + timeNow.Second;

            var currentPath = Environment.CurrentDirectory;
            var lifetime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = lifetime?.MainWindow;
            var currentFolder = await window.StorageProvider.TryGetFolderFromPathAsync(currentPath);

            var saveFileOptions = new FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedFileName = fileName,
                SuggestedStartLocation = currentFolder,
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.log" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
                }
            };

            var file = await window.StorageProvider.SaveFilePickerAsync(saveFileOptions);
            if (file == null)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("Activity log NOT saved", FeedbackType.Error);
                return;
            }
            
            try
            {
                await using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream);

                foreach (string logMessage in _viewModel.LogMessages)
                {
                    await writer.WriteLineAsync(logMessage);
                }

                _selectedDeviceStore.OnViewModelErrorOccured($"Activity log saved at {file.Path.LocalPath}", FeedbackType.Verbose);
            }
            catch (IOException e)
            {
                _selectedDeviceStore.OnViewModelErrorOccured($"Activity log NOT saved at {file.Path.LocalPath} due to {e.Message}", FeedbackType.Error);
            }
        }
    }
}
