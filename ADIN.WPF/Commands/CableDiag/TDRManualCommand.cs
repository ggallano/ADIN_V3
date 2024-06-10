using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;

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
            try
            {
                switch ((CalibrateType)Enum.Parse(typeof(CalibrateType), parameter.ToString()))
                {
                    case CalibrateType.Offset:
                        _viewModel.OffsetValue = Decimal.Parse(_selectedDeviceStore.SelectedDevice.FwAPI.SetOffset(_selectedDeviceStore.SelectedDevice.FaultDetector.CableDiagnostics.CableOffset));
                        break;

                    case CalibrateType.Cable:
                        var result = _selectedDeviceStore.SelectedDevice.FwAPI.SetNvp(_selectedDeviceStore.SelectedDevice.FaultDetector.CableDiagnostics.NVP);
                        _viewModel.NvpValue = Decimal.Parse(result[0]);
                        _selectedDeviceStore.SelectedDevice.FaultDetector.CableDiagnostics.Mode = (CalibrationMode)Enum.Parse(typeof(CalibrationMode), result[1]);
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