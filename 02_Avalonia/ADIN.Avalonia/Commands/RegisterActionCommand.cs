// <copyright file="RegisterActionCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.Helper.Feedback;
using ADIN.Helper.FileToLoad;
using ADIN.Helper.SaveToFile;
using ADIN.Register.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ADIN.Avalonia.Commands
{
    public class RegisterActionCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ExtraCommandsViewModel _viewModel;
        public RegisterActionCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            //_viewModel.PropertyChanged += _viewModel_PropertyChanged;
            //_selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        //        private ADINDevice _selectedDevice => _selectedDeviceStore.SelectedDevice;

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                !_viewModel.EnableButton)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var typeAction = (RegisterActionType)Enum.Parse(typeof(RegisterActionType), parameter.ToString());

            switch (typeAction)
            {
                case RegisterActionType.Load:
                    LoadXmlFile();
                    break;

                case RegisterActionType.Save:
                    SaveXmlFile();
                    break;

                default:
                    break;
            }
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }
        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        private async void LoadXmlFile()
        {
            XmlFileLoader loader = new XmlFileLoader();
            var currentPath = Environment.CurrentDirectory;
            var lifetime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = lifetime?.MainWindow;
            var currentFolder = await window.StorageProvider.TryGetFolderFromPathAsync(currentPath);


            var openFileOptions = new FilePickerOpenOptions
            {
                Title = "Open File",
                SuggestedStartLocation = currentFolder,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.xml" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
                }
            };

            var file = await window.StorageProvider.OpenFilePickerAsync(openFileOptions);
            if (file == null)
                return;

            ObservableCollection<RegisterModel> register_temp = new ObservableCollection<RegisterModel>();

            _viewModel.IsLoadingRegisters = true;
            _selectedDeviceStore.OnLoadingStatusChanged(_viewModel, true, "Loading registers...");
            try
            {
                register_temp = loader.XmlFileLoadContent(file[0].Name);

                await Task.Run(() =>
                {
                    foreach (var register in register_temp)
                    {
                        string response = string.Empty;

                        IMDIOAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IMDIOAPI;
                        response = fwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));

                        //var response = _selectedDevice.FwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
                        if (!response.Contains("OK"))
                        {
                            _selectedDeviceStore.OnViewModelErrorOccured($"[Load Register] Error in writing the register[{register.Name}]");
                            continue;
                        }
                        _selectedDeviceStore.OnViewModelFeedbackLog($"Loading the register: {register.Name}, value: {register.Value}", FeedbackType.Verbose);
                    }

                    _selectedDeviceStore.OnViewModelFeedbackLog($"Registers load from {file[0].Name}.", FeedbackType.Verbose);
                });
            }
            catch (InvalidOperationException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("[Load Registers] Invalid operation occurred while attempting to load the XML");
                throw;
            }

            _viewModel.IsLoadingRegisters = false;
            _selectedDeviceStore.OnLoadingStatusChanged(_viewModel, false, "Registers loaded");
        }

        private async void SaveXmlFile()
        {
            AbstractFileWriter writer = new XmlFileWriter();
            var currentPath = Environment.CurrentDirectory;
            var lifetime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = lifetime?.MainWindow;
            var currentFolder = await window.StorageProvider.TryGetFolderFromPathAsync(currentPath);

            var saveFileOptions = new FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedStartLocation = currentFolder,
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.xml" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
                }
            };

            var file = await window.StorageProvider.SaveFilePickerAsync(saveFileOptions);
            if (file == null)
                return;

            _viewModel.IsSavingRegisters = true;
            _selectedDeviceStore.OnViewModelFeedbackLog($"Refreshing register contents before saving to {file.Path.LocalPath}");
            _selectedDeviceStore.OnLoadingStatusChanged(_viewModel, true, "Saving registers...");

            ObservableCollection<RegisterModel> registers = new ObservableCollection<RegisterModel>();
            await Task.Run(() =>
            {
                registers = _selectedDeviceStore.SelectedDevice.FwAPI.ReadRegsiters();
            });
            _selectedDeviceStore.OnViewModelFeedbackLog($"Refreshing register contents done.");

            try
            {
                writer.WriteContent(file.Path.LocalPath, registers);
                _selectedDeviceStore.OnViewModelFeedbackLog($"Registers saved to {file.Path.LocalPath}");
            }
            catch (InvalidOperationException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("[Save Registers] Invalid operation occurred while attempting to save the XML");
            }
            
            _viewModel.IsSavingRegisters = false;
            _selectedDeviceStore.OnLoadingStatusChanged(_viewModel, false, "Registers saved");
        }
    }
}