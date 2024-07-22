// <copyright file="TDRManualCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ADIN.WPF.Commands.CableDiag
{
    public class TDRManualCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private TimeDomainReflectometryViewModel _viewModel;

        public TDRManualCommand(TimeDomainReflectometryViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            List<string> results;
            try
            {
                //ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;

                switch ((CalibrateType)Enum.Parse(typeof(CalibrateType), parameter.ToString()))
                {
                    case CalibrateType.Offset:

                        if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                        {
                            ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                            _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.CableOffset), CultureInfo.InvariantCulture);
                        }
                        else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                        {
                            ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                            _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.CableOffset), CultureInfo.InvariantCulture);
                        }
                        else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                        {
                            ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                            //if (_selectedDeviceStore.SelectedDevice.PortNumber == 1)
                                _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.CableOffset), CultureInfo.InvariantCulture);
                            //else
                            //    _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.TimeDomainReflectometry.CableOffset), CultureInfo.InvariantCulture);
                        }
                        
                        break;

                    case CalibrateType.Cable:
                        if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                        {
                            ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                            results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.NVP);
                        }
                        else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                        {
                            ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                            results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.NVP);
                        }
                        else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                        {
                            ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                            //if (_selectedDeviceStore.SelectedDevice.PortNumber == 1)
                                results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.NVP);
                            //else
                            //    results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.TimeDomainReflectometry.NVP);
                        }

                        _viewModel.NvpValue = Decimal.Parse(results[0], CultureInfo.InvariantCulture);
                        //if (_selectedDeviceStore.SelectedDevice.PortNumber == 1)
                            _selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.Mode = (CalibrationMode)Enum.Parse(typeof(CalibrationMode), results[1]);
                        //else
                        //    _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.TimeDomainReflectometry.Mode = (CalibrationMode)Enum.Parse(typeof(CalibrationMode), results[1]);
                        break;

                    default:
                        break;
                }
            }
            catch (ApplicationException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
            catch (FormatException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}