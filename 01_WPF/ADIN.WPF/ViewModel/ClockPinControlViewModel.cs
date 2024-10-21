// <copyright file="ClockPinControlViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using System.Collections.Generic;
using System.Windows.Input;

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

            ClkPnCtrlCmd_None = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_Rcvr125Mhz = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_Free125Mhz = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_HrtRcvr = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_HrtFree = new ClockPinControlCommand(this, selectedDeviceStore);
            ClkPnCtrlCmd_25Mhz = new ClockPinControlCommand(this, selectedDeviceStore);

            Clk25RefPnCtrlCmd_None = new ClockRefPinControlCommand(this, selectedDeviceStore);
            Clk25RefPnCtrlCmd_25Mhz = new ClockRefPinControlCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public bool Clk25RefPinPresent
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice == null)
                    return false;

                return _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1300;
            }
        }

        public ICommand Clk25RefPnCtrlCmd_25Mhz { get; set; }

        public ICommand Clk25RefPnCtrlCmd_None { get; set; }

        public ICommand ClkPnCtrlCmd_25Mhz { get; set; }

        public ICommand ClkPnCtrlCmd_Free125Mhz { get; set; }

        public ICommand ClkPnCtrlCmd_HrtFree { get; set; }

        public ICommand ClkPnCtrlCmd_HrtRcvr { get; set; }

        public ICommand ClkPnCtrlCmd_None { get; set; }

        public ICommand ClkPnCtrlCmd_Rcvr125Mhz { get; set; }

        public bool IsClk25RefPnCtrlCmd_25Mhz { get; set; }

        public bool IsClk25RefPnCtrlCmd_None { get; set; } = true;

        public bool IsClkPnCtrlCmd_25Mhz { get; set; }

        public bool IsClkPnCtrlCmd_Free125Mhz { get; set; }

        public bool IsClkPnCtrlCmd_HrtFree { get; set; }

        public bool IsClkPnCtrlCmd_HrtRcvr { get; set; }

        public bool IsClkPnCtrlCmd_None { get; set; } = true;

        public bool IsClkPnCtrlCmd_Rcvr125Mhz { get; set; }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        public string SelectedClk25RefPnCtrl
        {
            set
            {
                _clockPinControl.Clk25RefPnCtrl = value;
                SetClkRefPinCntrl(value);
            }
        }

        public string SelectedGpClk
        {
            set
            {
                _clockPinControl.GpClkPinControl = value;
                SetClkPinCntrl(value);
            }
        }

        private IClockPinControl _clockPinControl => _selectedDeviceStore.SelectedDevice?.ClockPinControl;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            if (_selectedDeviceStore.SelectedDevice.DeviceType != BoardType.ADIN1300
             && _selectedDeviceStore.SelectedDevice.DeviceType != BoardType.ADIN1200)
                return;

            SetClkPinCntrl(_clockPinControl.GpClkPinControl);
            SetClkRefPinCntrl(_clockPinControl.Clk25RefPnCtrl);
            OnPropertyChanged(nameof(Clk25RefPinPresent));
        }

        private void SetClkPinCntrl(string clkPinCntrl)
        {
            switch (clkPinCntrl)
            {
                case "125 MHz PHY Recovered":
                    IsClkPnCtrlCmd_25Mhz = false;
                    IsClkPnCtrlCmd_Free125Mhz = false;
                    IsClkPnCtrlCmd_HrtFree = false;
                    IsClkPnCtrlCmd_HrtRcvr = false;
                    IsClkPnCtrlCmd_None = false;
                    IsClkPnCtrlCmd_Rcvr125Mhz = true;
                    break;
                case "125 MHz PHY Free Running":
                    IsClkPnCtrlCmd_25Mhz = false;
                    IsClkPnCtrlCmd_Free125Mhz = true;
                    IsClkPnCtrlCmd_HrtFree = false;
                    IsClkPnCtrlCmd_HrtRcvr = false;
                    IsClkPnCtrlCmd_None = false;
                    IsClkPnCtrlCmd_Rcvr125Mhz = false;
                    break;
                case "Recovered HeartBeat":
                    IsClkPnCtrlCmd_25Mhz = false;
                    IsClkPnCtrlCmd_Free125Mhz = false;
                    IsClkPnCtrlCmd_HrtFree = false;
                    IsClkPnCtrlCmd_HrtRcvr = true;
                    IsClkPnCtrlCmd_None = false;
                    IsClkPnCtrlCmd_Rcvr125Mhz = false;
                    break;
                case "Free Running HeartBeat":
                    IsClkPnCtrlCmd_25Mhz = false;
                    IsClkPnCtrlCmd_Free125Mhz = false;
                    IsClkPnCtrlCmd_HrtFree = true;
                    IsClkPnCtrlCmd_HrtRcvr = false;
                    IsClkPnCtrlCmd_None = false;
                    IsClkPnCtrlCmd_Rcvr125Mhz = false;
                    break;
                case "25 MHz Reference":
                    IsClkPnCtrlCmd_25Mhz = true;
                    IsClkPnCtrlCmd_Free125Mhz = false;
                    IsClkPnCtrlCmd_HrtFree = false;
                    IsClkPnCtrlCmd_HrtRcvr = false;
                    IsClkPnCtrlCmd_None = false;
                    IsClkPnCtrlCmd_Rcvr125Mhz = false;
                    break;
                default:
                    IsClkPnCtrlCmd_25Mhz = false;
                    IsClkPnCtrlCmd_Free125Mhz = false;
                    IsClkPnCtrlCmd_HrtFree = false;
                    IsClkPnCtrlCmd_HrtRcvr = false;
                    IsClkPnCtrlCmd_None = true;
                    IsClkPnCtrlCmd_Rcvr125Mhz = false;
                    break;
            }

            OnPropertyChanged(nameof(IsClkPnCtrlCmd_None));
            OnPropertyChanged(nameof(IsClkPnCtrlCmd_Rcvr125Mhz));
            OnPropertyChanged(nameof(IsClkPnCtrlCmd_Free125Mhz));
            OnPropertyChanged(nameof(IsClkPnCtrlCmd_HrtRcvr));
            OnPropertyChanged(nameof(IsClkPnCtrlCmd_HrtFree));
            OnPropertyChanged(nameof(IsClkPnCtrlCmd_25Mhz));
        }

        private void SetClkRefPinCntrl(string clkRefPinCntrl)
        {
            switch (clkRefPinCntrl)
            {
                case "25 MHz Reference":
                    IsClk25RefPnCtrlCmd_None = false;
                    IsClk25RefPnCtrlCmd_25Mhz = true;
                    break;
                default:
                    IsClk25RefPnCtrlCmd_None = true;
                    IsClk25RefPnCtrlCmd_25Mhz = false;
                    break;
            }

            OnPropertyChanged(nameof(IsClk25RefPnCtrlCmd_25Mhz));
            OnPropertyChanged(nameof(IsClk25RefPnCtrlCmd_None));
        }
    }
}