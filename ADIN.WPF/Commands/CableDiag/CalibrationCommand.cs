using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Components;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ADIN.WPF.Commands.CableDiag
{
    public class CalibrationCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private object _thisLock;
        private TimeDomainReflectometryViewModel _viewModel;

        public CalibrationCommand(TimeDomainReflectometryViewModel viewModel, SelectedDeviceStore selectedDeviceStore, object thisLock)
        {
            _viewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;
            _thisLock = thisLock;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private TDRModel _cablediagnostic => _selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.TimeDomainReflectometry;

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            decimal result;
            switch ((CalibrateType)Enum.Parse(typeof(CalibrateType), parameter.ToString()))
            {
                case CalibrateType.Offset:
                    OffsetManualDialog offsetDialog = new OffsetManualDialog();
                    offsetDialog.Owner = Application.Current.MainWindow;
                    offsetDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    offsetDialog.ContentMessage = "Please disconnect cable from MDI connector and \nclick OK to perform offset calibration.";

                    if (offsetDialog.ShowDialog() == true)
                    {
                        _viewModel.IsOngoingCalibration = true;

                        Task.Run(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _viewModel.BusyContent = "Executing offset calibration.";
                            });
                            Thread.Sleep(500);

                            //PerformResetPhy();
                            //Thread.Sleep(1000);

                            //CheckLoopbackState();
                            //Thread.Sleep(250);

                            //CheckTestModeState();
                            //Thread.Sleep(250);

                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _viewModel.BusyContent = "Calibrating";
                            }));

                            try
                            {
                                ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                result = Decimal.Parse(fwADIN1100API.PerformOffsetCalibration());
                                //result = Decimal.Parse(_selectedDeviceStore.SelectedDevice.FwAPI.PerformOffsetCalibration());

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.OffsetValue = result;
                                    _viewModel.OffsetBackgroundBrush = new SolidColorBrush(Color.FromRgb(40, 158, 8));
                                });
                            }
                            catch (ApplicationException ex)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _viewModel.OffsetBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                                }));
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

                    break;

                case CalibrateType.Cable:
                    CableManualDialog cableDialog = new CableManualDialog();
                    cableDialog.txtCableLength.Value = 100.0;
                    cableDialog.Owner = Application.Current.MainWindow;
                    cableDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    cableDialog.ContentMessage = "Please connect cable at MDI connector and enter the cable \nlength to perform cable calibration.";

                    decimal cableLengthInput = 0.0M;

                    if (cableDialog.ShowDialog() == true)
                    {
                        _viewModel.IsOngoingCalibration = true;
                        Task.Run(() =>
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _viewModel.BusyContent = "Executing cable calibration.";
                            }));
                            Thread.Sleep(500);

                            //PerformResetPhy();
                            //Thread.Sleep(1000);

                            //CheckLoopbackState();
                            //Thread.Sleep(250);

                            //CheckTestModeState();
                            //Thread.Sleep(250);

                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _viewModel.BusyContent = "Calibrating";
                            }));

                            try
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    cableLengthInput = Convert.ToDecimal(cableDialog.txtCableLength.Value);
                                });

                                ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                result = Decimal.Parse(fwADIN1100API.PerformCableCalibration(cableLengthInput));
                                //result = Decimal.Parse(_selectedDeviceStore.SelectedDevice.FwAPI.PerformCableCalibration(cableLengthInput));

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.NvpValue = result;
                                    _viewModel.CableBackgroundBrush = new SolidColorBrush(Color.FromRgb(40, 158, 8));
                                });
                            }
                            catch (ApplicationException ex)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _viewModel.CableBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                                }));
                            }
                            catch (Exception ex)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _viewModel.CableBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                                    _selectedDeviceStore.OnViewModelErrorOccured($"{ex.Message}");
                                }));
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
                    break;

                default:
                    break;
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        //private void CheckLoopbackState()
        //{
        //    var loopbackState = _selectedDeviceStore.SelectedDevice.FwAPI.GetLoopbackState();
        //    if (loopbackState != LoopBackMode.OFF)
        //    {
        //        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            _viewModel.BusyContent = "Loopback Reset";
        //        }));
        //        _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_selectedDeviceStore.SelectedDevice.Loopback.Loopbacks[0]);
        //        _selectedDeviceStore.OnLoopbackStateChanged(_selectedDeviceStore.SelectedDevice.Loopback.Loopbacks[0]);
        //    }
        //}

        //private void CheckTestModeState()
        //{
        //    var testmodeState = _selectedDeviceStore.SelectedDevice.FwAPI.GetTestModeState();
        //    if (testmodeState != TestModeType.Normal)
        //    {
        //        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            _viewModel.BusyContent = "Testmode Reset";
        //        }));
        //        _selectedDeviceStore.SelectedDevice.FwAPI.SetTestModeSetting(_selectedDeviceStore.SelectedDevice.TestMode.TestModes[0]);
        //        _selectedDeviceStore.OnTestModeStateChanged(_selectedDeviceStore.SelectedDevice.TestMode.TestModes[0]);
        //    }
        //}

        //private void PerformResetPhy()
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        _viewModel.BusyContent = "Software Reset";
        //    });
        //    _selectedDeviceStore.SelectedDevice.FwAPI.ResetPhy(ResetType.Phy);
        //}
    }
}