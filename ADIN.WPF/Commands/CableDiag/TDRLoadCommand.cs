using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.ReadFile;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

            try
            {
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
                                var res = _selectedDeviceStore.SelectedDevice.FwAPI.SetOffset(Decimal.Parse(values[0], CultureInfo.InvariantCulture));

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.OffsetValue = Decimal.Parse(res, CultureInfo.InvariantCulture);
                                    _viewModel.OffsetFileName = Path.GetFileName(openFileDialog.FileName);
                                    _viewModel.OffsetBackgroundBrush = new SolidColorBrush(Color.FromRgb(40, 158, 8));
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
                                var res = _selectedDeviceStore.SelectedDevice.FwAPI.SetCoeff(nvp, coeff0, coeffi);
                                _selectedDeviceStore.SelectedDevice.FwAPI.SetMode(CalibrationMode.Optimized);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _viewModel.NvpValue = Decimal.Parse(res[0], CultureInfo.InvariantCulture);
                                    _viewModel.CableFileName = Path.GetFileName(openFileDialog.FileName);
                                    _viewModel.CableBackgroundBrush = new SolidColorBrush(Color.FromRgb(40, 158, 8));
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
                        _viewModel.OffsetBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
                        break;

                    case CalibrateType.Cable:
                        _viewModel.CableBackgroundBrush = new SolidColorBrush(Color.FromRgb(168, 3, 3));
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