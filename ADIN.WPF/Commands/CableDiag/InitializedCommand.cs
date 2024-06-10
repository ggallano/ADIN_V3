using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;
using System.Windows.Media;

namespace ADIN.WPF.Commands.CableDiag
{
    public class InitializedCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private TimeDomainReflectometryViewModel _viewModel;

        public InitializedCommand(TimeDomainReflectometryViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
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
            ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;

            fwADIN1100API.TDRInit();
            _viewModel.OffsetValue = Decimal.Parse(fwADIN1100API.GetOffset());
            _viewModel.NvpValue = Decimal.Parse(fwADIN1100API.GetNvp());
            _viewModel.OffsetBackgroundBrush = new SolidColorBrush(Colors.Transparent);
            _viewModel.CableBackgroundBrush = new SolidColorBrush(Colors.Transparent);
            _viewModel.CableFileName = "-";
            _viewModel.OffsetFileName = "-";

            _viewModel.FaultState = "";
            _viewModel.DistToFault = "0.00";
            _viewModel.FaultBackgroundBrush = new SolidColorBrush(Colors.LightGray);
            _viewModel.IsFaultVisibility = false;
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}