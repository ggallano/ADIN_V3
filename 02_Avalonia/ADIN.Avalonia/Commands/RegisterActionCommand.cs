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
using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
                    //LoadXmlFile();
                    break;

                case RegisterActionType.Save:
                    //SaveXmlFile();
                    break;

                default:
                    break;
            }
        }

        //        private void _selectedDeviceStore_SelectedDeviceChanged()
        //        {
        //            OnCanExecuteChanged();
        //        }
        //        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //        {
        //            OnCanExecuteChanged();
        //        }

        //        private async Task<string> OpenXmlFileAsync(Window window)
        //        {
        //            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        //            {
        //                Title = "Open XML File",
        //                FileTypeFilter = new List<FilePickerFileType>
        //                {
        //                    new FilePickerFileType("XML Files")
        //                    {
        //                        Patterns = new[] { "*.xml" }
        //                    }
        //                },
        //                AllowMultiple = false
        //            });

        //            if (files.Count > 0)
        //            {
        //                return files[0].Path.LocalPath;
        //            }

        //            return null;
        //        }

        //        private void LoadXmlFile()
        //        {
        //            XmlFileLoader loader = new XmlFileLoader();
        //            ObservableCollection<RegisterModel> register_temp = new ObservableCollection<RegisterModel>();

        //            //var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
        //            //var ofdResult = ofd.ShowDialog();

        //            //if (ofdResult == false)
        //            //    return;

        //            //FindControl<Button>("OpenFileButton").Click += async (sender, e) => await ShowOpenFileDialog(this);

        //            _viewModel.IsLoadingRegisters = true;

        //            Task.Run(() =>
        //            {
        //                try
        //                {
        //                    //_selectedDeviceStore.OnViewModelFeedbackLog($"Load Register....", FeedbackType.Verbose);
        //                    register_temp = loader.XmlFileLoadContent(ofd.FileName);
        //                    foreach (var register in register_temp)
        //                    {
        //                        string response = string.Empty;
        //#if !DISABLE_T1L
        //                        if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
        //                        {
        //                            ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
        //                            response = fwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
        //                        }
        //#endif
        //#if !DISABLE_TSN
        //                        if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
        //                        {
        //                            ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
        //                            response = fwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
        //                        }
        //                        else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
        //                        {
        //                            ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
        //                            response = fwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
        //                        }
        //                        else { } //Do nothing
        //#endif
        //                        //var response = _selectedDevice.FwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
        //                        if (!response.Contains("OK"))
        //                        {
        //                            _selectedDeviceStore.OnViewModelErrorOccured($"[Load Register] Error in writing the register[{register.Name}]");
        //                            continue;
        //                        }
        //                        _selectedDeviceStore.OnViewModelFeedbackLog($"Loading the register: {register.Name}, value: {register.Value}", FeedbackType.Verbose);
        //                    }

        //                    _selectedDeviceStore.OnViewModelFeedbackLog($"Registers load from {ofd.FileName}.", FeedbackType.Verbose);
        //                }
        //                catch (InvalidOperationException ex)
        //                {
        //                    _selectedDeviceStore.OnViewModelErrorOccured("[Load Registers] Invalid operation occurred while attempting to load the XML");
        //                    throw;
        //                }

        //                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    _viewModel.IsLoadingRegisters = false;
        //                }));
        //            });
        //        }

        //        private void SaveXmlFile()
        //        {
        //            AbstractFileWriter writer = new XmlFileWriter();

        //            var sfd = new Microsoft.Win32.SaveFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
        //            var sfdResult = sfd.ShowDialog();

        //            if (sfdResult == false)
        //                return;

        //            _viewModel.IsSavingRegisters = true;

        //            Task.Run(() =>
        //            {
        //                _selectedDeviceStore.OnViewModelFeedbackLog($"Refreshing register contents before saving to {sfd.FileName}");
        //                var registers = _selectedDevice.FwAPI.ReadRegsiters();
        //                _selectedDeviceStore.OnViewModelFeedbackLog($"Refreshing register contents done.");

        //                try
        //                {
        //                    writer.WriteContent(sfd.FileName, registers);
        //                    _selectedDeviceStore.OnViewModelFeedbackLog($"Registers saved to {sfd.FileName}");
        //                }
        //                catch (InvalidOperationException ex)
        //                {
        //                    _selectedDeviceStore.OnViewModelErrorOccured("[Save Registers] Invalid operation occurred while attempting to save the XML");
        //                }

        //                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    _viewModel.IsSavingRegisters = false;
        //                }));
        //            });
        //        }
    }
}