using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ADIN.WPF.Commands.CableDiag
{
    public class TDRFaultDetectorCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private TimeDomainReflectometryViewModel _viewModel;

        public TDRFaultDetectorCommand(TimeDomainReflectometryViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            FaultType fault;

            _viewModel.IsOngoingCalibration = true;
            Task.Run(() =>
            {
                if ((_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100)
                 || (_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100_S1))
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _viewModel.BusyContent = "Software Reset";
                    }));
                    _selectedDeviceStore.SelectedDevice.FwAPI.ResetPhy(ResetType.Phy);
                    Thread.Sleep(1500);
                }

                try
                {
                    //ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _viewModel.BusyContent = "Running TDR";
                    }));
                    Thread.Sleep(1000);

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fault = fwADIN1100API.PerformFaultDetection();
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                    {
                        ADIN1110FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                        fault = fwADIN1100API.PerformFaultDetection();
                    }
                    else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                    {
                        ADIN2111FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                        fault = fwADIN1100API.PerformFaultDetection();
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (fault)
                        {
                            case FaultType.None:
                                _viewModel.FaultState = "No Fault";
                                _viewModel.FaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(40, 158, 8));
                                _viewModel.DistToFault = "0.00";
                                _viewModel.IsFaultVisibility = false;
                                break;

                            case FaultType.Open:

                                _viewModel.FaultState = "Open";
                                _viewModel.FaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                                {
                                    ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                    _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                }
                                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                                {
                                    ADIN1110FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                    _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                }
                                else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                                {
                                    ADIN2111FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                    _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                }
                                _viewModel.IsFaultVisibility = true;

                                break;

                            case FaultType.Short:
                                _viewModel.FaultState = "Short";
                                _viewModel.FaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                                {
                                    ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                    _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                }
                                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                                {
                                    ADIN1110FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                    _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                }
                                else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                                {
                                    ADIN2111FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                    _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                }
                                _viewModel.IsFaultVisibility = true;
                                break;

                            default:
                                _viewModel.IsFaultVisibility = false;
                                break;
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _viewModel.IsOngoingCalibration = false;
                    }));
                }
            });
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}