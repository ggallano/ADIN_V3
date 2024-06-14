using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Microsoft.Win32;
using System;
using Telerik.Windows.Controls.DataVisualization.Map.BingRest;

namespace ADIN.WPF.Commands
{
    public class ResetFrameDeviceCheckerCommnad : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameGenCheckerViewModel _viewModel;

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="selectedDeviceStore"></param>
        /// <param name="ftdiService"></param>
        public ResetFrameDeviceCheckerCommnad(FrameGenCheckerViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
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
            if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
            {
                ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                fwADIN1100API.ResetFrameGenCheckerStatistics();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
            {
                ADIN1110FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                fwADIN1100API.ResetFrameGenCheckerStatistics();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
            {
                ADIN2111FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                fwADIN1100API.ResetFrameGenCheckerStatistics();
            }
            else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
            {
                ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                fwADIN1200API.ResetFrameGenCheckerStatistics();
            }
            else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
            {
                ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                fwADIN1300API.ResetFrameGenCheckerStatistics();
            }
            //_selectedDeviceStore.SelectedDevice.FwAPI.ResetFrameGenCheckerStatistics();
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}