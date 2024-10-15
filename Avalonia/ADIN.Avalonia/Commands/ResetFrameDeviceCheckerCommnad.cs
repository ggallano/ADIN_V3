using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Device.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Avalonia.Commands
{
    public class ResetFrameDeviceCheckerCommnad : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private LoopbackFrameGenViewModel _loopbackFrameGenViewModel;

        public ResetFrameDeviceCheckerCommnad(LoopbackFrameGenViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _loopbackFrameGenViewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _loopbackFrameGenViewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            IFrameGenCheckerAPI fwAPI = _selectedDeviceStore.SelectedDevice?.FwAPI as IFrameGenCheckerAPI;
            fwAPI.ResetFrameGenCheckerStatistics();
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}