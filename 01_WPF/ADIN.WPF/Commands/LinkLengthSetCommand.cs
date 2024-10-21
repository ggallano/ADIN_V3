using ADIN.Device.Models;
using ADIN.WPF.Components;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ADIN.WPF.Commands
{
    public class LinkLengthSetCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ActiveLinkMonitoringViewModel _viewModel;

        public LinkLengthSetCommand(ActiveLinkMonitoringViewModel activeLinkMonitoringViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = activeLinkMonitoringViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _activeLinkMonitoringViewModel_PropertyChanged;
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

            LinkLengthBenchmarkDialog dialog = new LinkLengthBenchmarkDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ContentMessage = "Please connect cable link and leave its end open";

            if (dialog.ShowDialog() == false)
                return;

            _viewModel.IsOngoingCalibration = true;
            Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _viewModel.BusyContent = "Software Reset";
                }));
                _selectedDeviceStore.SelectedDevice.FirmwareAPI.ResetPhy(ResetType.Phy);
                Thread.Sleep(1000);

                try
                {
                    var loopbackState = _selectedDeviceStore.SelectedDevice.FirmwareAPI.GetLoopbackState();
                    var testmodeState = _selectedDeviceStore.SelectedDevice.FirmwareAPI.GetTestModeState();

                    if (loopbackState != LoopBackMode.OFF)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _viewModel.BusyContent = "Loopback Reset";
                        }));
                        _selectedDeviceStore.SelectedDevice.FirmwareAPI.SetLoopbackSetting(_selectedDeviceStore.SelectedDevice.Loopback.Loopbacks[0]);
                        _selectedDeviceStore.OnLoopbackStateChanged(_selectedDeviceStore.SelectedDevice.Loopback.Loopbacks[0]);
                    }
                    Thread.Sleep(250);

                    if (testmodeState != TestModeType.Normal)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _viewModel.BusyContent = "Testmode Reset";
                        }));
                        _selectedDeviceStore.SelectedDevice.FirmwareAPI.SetTestModeSetting(_selectedDeviceStore.SelectedDevice.TestMode.TestModes[0]);
                        _selectedDeviceStore.OnTestModeStateChanged(_selectedDeviceStore.SelectedDevice.TestMode.TestModes[0]);
                    }
                    Thread.Sleep(250);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _viewModel.BusyContent = "Running TDR";
                    }));
                    Thread.Sleep(250);

                    fault = _selectedDeviceStore.SelectedDevice.FirmwareAPI.PerformFaultDetection();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (fault)
                        {
                            case FaultType.None:
                                _selectedDeviceStore.OnViewModelErrorOccured("Link length not detected");
                                _viewModel.IsLinkLengthVisible = false;
                                break;

                            case FaultType.Open:
                            case FaultType.Short:
                                _viewModel.LinkLengthBenchmarkValue = _selectedDeviceStore.SelectedDevice.FirmwareAPI.GetFaultDistance().ToString();
                                _viewModel.IsLinkLengthVisible = true;
                                break;

                            default:
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

        private void _activeLinkMonitoringViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}