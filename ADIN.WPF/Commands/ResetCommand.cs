using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using System;

namespace ADIN.WPF.Commands
{
    public class ResetCommand : CommandBase
    {
        private ExtraCommandsViewModel _extraCommandsViewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public ResetCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _extraCommandsViewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _extraCommandsViewModel.PropertyChanged += _extraCommandsViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                _extraCommandsViewModel.IsOngoingCalibrationStatus)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var resetType = (ResetType)Enum.Parse(typeof(ResetType), parameter.ToString());
            _selectedDeviceStore.SelectedDevice.FwAPI.ResetPhy(resetType);
        }

        private void _extraCommandsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}