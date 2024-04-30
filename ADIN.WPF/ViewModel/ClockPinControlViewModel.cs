using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace ADIN.WPF.ViewModel
{
    public class ClockPinControlViewModel : ViewModelBase
    {
        private NavigationStore _navigationStore;
        private SelectedDeviceStore _selectedDeviceStore;

        public ClockPinControlViewModel(NavigationStore navigationStore, SelectedDeviceStore selectedDeviceStore)
        {
            _navigationStore = navigationStore;
            _selectedDeviceStore = selectedDeviceStore;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;

            ClkPnCtrlCmd_None = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_Rcvr125Mhz = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_Free125Mhz = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_HrtRcvr = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_HrtFree = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_25Mhz = new ClockPinControlCommand(this, selectedDeviceStore);
        }

        public ICommand ClkPnCtrlCmd_None { get; set; }
        public ICommand ClkPnCtrlCmd_Rcvr125Mhz { get; set; }
        public ICommand ClkPnCtrlCmd_Free125Mhz { get; set; }
        public ICommand ClkPnCtrlCmd_HrtRcvr { get; set; }
        public ICommand ClkPnCtrlCmd_HrtFree { get; set; }
        public ICommand ClkPnCtrlCmd_25Mhz { get; set; }

        public string SelectedGpClk
        {
            get { return _clockPinControl?.GpClkPinControl; }
            set
            {
                _clockPinControl.GpClkPinControl = value;
                OnPropertyChanged(nameof(SelectedGpClk));
            }
        }

        public List<string> GpClkPinControls => _clockPinControl?.GpClkPinControls;
        private IClockPinControl _clockPinControl => _selectedDeviceStore.SelectedDevice?.ClockPinControl;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedGpClk));
            OnPropertyChanged(nameof(GpClkPinControls));
        }
    }
}