using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Collections.Generic;

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
                            _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.CableOffset));
                        }
                        else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                        {
                            ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                            _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.CableOffset));
                        }
                        else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                        {
                            ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                            if (_selectedDeviceStore.SelectedDevice.PortNumber == 1)
                                _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.CableOffset));
                            else
                                _viewModel.OffsetValue = Decimal.Parse(fwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.TimeDomainReflectometry.CableOffset));
                        }
                        
                        break;

                    case CalibrateType.Cable:
                        if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                        {
                            ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                            results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.NVP);
                        }
                        else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                        {
                            ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                            results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.NVP);
                        }
                        else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                        {
                            ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                            if (_selectedDeviceStore.SelectedDevice.PortNumber == 1)
                                results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.NVP);
                            else
                                results = fwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.TimeDomainReflectometry.NVP);
                        }
                        //fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        //var results = fwADIN1100API.SetNvp(_selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry.NVP);
                        _viewModel.NvpValue = Decimal.Parse(results[0]);
                        if (_selectedDeviceStore.SelectedDevice.PortNumber == 1)
                            _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.TimeDomainReflectometry.Mode = (CalibrationMode)Enum.Parse(typeof(CalibrationMode), results[1]);
                        else
                            _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.TimeDomainReflectometry.Mode = (CalibrationMode)Enum.Parse(typeof(CalibrationMode), results[1]);
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