// <copyright file="CalibrationCommand.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Components;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
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
                        _viewModel.IsVisibleOffsetCalibration = false;

                        Task.Run(() =>
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _viewModel.BusyContent = "Executing offset calibration.";
                            }));
                            Thread.Sleep(2000);

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
                                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                                {
                                    ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                    result = Decimal.Parse(fwAPI.PerformOffsetCalibration());
                                }
                                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                                {
                                    ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                    result = Decimal.Parse(fwAPI.PerformOffsetCalibration());
                                }
                                else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                                {
                                    ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                    result = Decimal.Parse(fwAPI.PerformOffsetCalibration());
                                }

                                //result = Decimal.Parse(_selectedDeviceStore.SelectedDevice.FwAPI.PerformOffsetCalibration());

                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _viewModel.OffsetValue = result;
                                    _viewModel.OffsetCalibrationMessage = "Calibration Success";
                                    _viewModel.IsVisibleOffsetCalibration = true;
                                }));
                            }
                            catch (ApplicationException ex)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _viewModel.OffsetCalibrationMessage = "Calibration Failed";
                                    _viewModel.IsVisibleOffsetCalibration = true;
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
                        _viewModel.IsVisibleCableCalibration = false;

                        Task.Run(() =>
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                _viewModel.BusyContent = "Executing cable calibration.";
                            }));
                            Thread.Sleep(2000);

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

                                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                                {
                                    ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                    result = Decimal.Parse(fwAPI.PerformCableCalibration(cableLengthInput));
                                }
                                else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                                {
                                    ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                    result = Decimal.Parse(fwAPI.PerformCableCalibration(cableLengthInput));
                                }
                                else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                                {
                                    ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                    result = Decimal.Parse(fwAPI.PerformCableCalibration(cableLengthInput));
                                }
                                //result = Decimal.Parse(_selectedDeviceStore.SelectedDevice.FwAPI.PerformCableCalibration(cableLengthInput));

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.NvpValue = result;
                                    _viewModel.CableCalibrationMessage = "Calibration Success";
                                    _viewModel.IsVisibleCableCalibration = true;
                                });
                            }
                            catch (ApplicationException ex)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    _viewModel.CableCalibrationMessage = "Calibration Failed";
                                    _viewModel.IsVisibleCableCalibration = true;
                                }));
                            }
                            catch (Exception ex)
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
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

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnCanExecuteChanged();
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