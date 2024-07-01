// <copyright file="TDRLoadCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.ReadFile;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ADIN.WPF.Commands.CableDiag
{
    public class TDRLoadCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private TimeDomainReflectometryViewModel _viewModel;

        public TDRLoadCommand(TimeDomainReflectometryViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string[] values = null;
            string result = string.Empty;
            List<string> results;

            try
            {
                //ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;

                switch ((CalibrateType)Enum.Parse(typeof(CalibrateType), parameter.ToString()))
                {
                    case CalibrateType.Offset:
                        openFileDialog.Filter = "Calibrate Offset file (*.cof)|*.cof";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            _viewModel.IsOngoingCalibration = true;

                            Task.Run(() =>
                            {
                                values = ReadContent.Read(openFileDialog.FileName);
                                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                                {
                                    ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                    result = fwAPI.SetOffset(Decimal.Parse(values[0], CultureInfo.InvariantCulture));
                                }
                                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                                {
                                    ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                    result = fwAPI.SetOffset(Decimal.Parse(values[0], CultureInfo.InvariantCulture));
                                }
                                else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                                {
                                    ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                    result = fwAPI.SetOffset(Decimal.Parse(values[0], CultureInfo.InvariantCulture));
                                }
                                //var result = fwADIN1100API.SetOffset(Decimal.Parse(values[0], CultureInfo.InvariantCulture));

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.OffsetValue = Decimal.Parse(result, CultureInfo.InvariantCulture);
                                    _viewModel.OffsetFileName = Path.GetFileName(openFileDialog.FileName);
                                });
                            });
                        }

                        break;

                    case CalibrateType.Cable:
                        openFileDialog.Filter = "Calibrate Cable file (*.ccf)|*.ccf";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            _viewModel.IsOngoingCalibration = true;

                            Task.Run(() =>
                            {
                                values = ReadContent.Read(openFileDialog.FileName);
                                var nvp = Decimal.Parse(values[0], CultureInfo.InvariantCulture);
                                var coeff0 = Decimal.Parse(values[0], CultureInfo.InvariantCulture);
                                var coeffi = Decimal.Parse(values[0], CultureInfo.InvariantCulture);
                                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                                {
                                    ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                    results = fwAPI.SetCoeff(nvp, coeff0, coeffi);
                                    fwAPI.SetMode(CalibrationMode.Optimized);
                                }
                                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                                {
                                    ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                    results = fwAPI.SetCoeff(nvp, coeff0, coeffi);
                                    fwAPI.SetMode(CalibrationMode.Optimized);
                                }
                                else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                                {
                                    ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                    results = fwAPI.SetCoeff(nvp, coeff0, coeffi);
                                    fwAPI.SetMode(CalibrationMode.Optimized);
                                }
                                //var results = fwADIN1100API.SetCoeff(nvp, coeff0, coeffi);
                                //fwADIN1100API.SetMode(CalibrationMode.Optimized);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.NvpValue = Decimal.Parse(results[0], CultureInfo.InvariantCulture);
                                    _viewModel.CableFileName = Path.GetFileName(openFileDialog.FileName);
                                });
                            });
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (ApplicationException ex)
            {
                switch ((CalibrateType)Enum.Parse(typeof(CalibrateType), parameter.ToString()))
                {
                    case CalibrateType.Offset:
                        break;

                    case CalibrateType.Cable:
                        break;

                    default:
                        break;
                }
            }
            catch (ArgumentNullException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
            catch (FormatException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
            finally
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _viewModel.IsOngoingCalibration = false;
                }));
            }
        }

        private void _viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}