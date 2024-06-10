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
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _viewModel.BusyContent = "Software Reset";
                }));
                _selectedDeviceStore.SelectedDevice.FwAPI.ResetPhy(ResetType.Phy);
                Thread.Sleep(1000);

                try
                {
                    ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;

                    //var loopbackState = _selectedDeviceStore.SelectedDevice.FwAPI.GetLoopbackState();
                    //var testmodeState = _selectedDeviceStore.SelectedDevice.FwAPI.GetTestModeState();

                    //if (loopbackState != LoopBackMode.OFF)
                    //{
                    //    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        _viewModel.BusyContent = "Loopback Reset";
                    //    }));
                    //    _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_selectedDeviceStore.SelectedDevice.Loopback.Loopbacks[0]);
                    //    _selectedDeviceStore.OnLoopbackStateChanged(_selectedDeviceStore.SelectedDevice.Loopback.Loopbacks[0]);
                    //}
                    //Thread.Sleep(250);

                    //if (testmodeState != TestModeType.Normal)
                    //{
                    //    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        _viewModel.BusyContent = "Testmode Reset";
                    //    }));
                    //    _selectedDeviceStore.SelectedDevice.FwAPI.SetTestModeSetting(_selectedDeviceStore.SelectedDevice.TestMode.TestModes[0]);
                    //    _selectedDeviceStore.OnTestModeStateChanged(_selectedDeviceStore.SelectedDevice.TestMode.TestModes[0]);
                    //}
                    //Thread.Sleep(250);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _viewModel.BusyContent = "Running TDR";
                    }));
                    Thread.Sleep(250);

                    fault = fwADIN1100API.PerformFaultDetection();
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
                                _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
                                _viewModel.IsFaultVisibility = true;

                                break;

                            case FaultType.Short:
                                _viewModel.FaultState = "Short";
                                _viewModel.FaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                                _viewModel.DistToFault = fwADIN1100API.GetFaultDistance().ToString();
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