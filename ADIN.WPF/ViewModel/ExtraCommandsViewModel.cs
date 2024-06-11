using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class ExtraCommandsViewModel : ViewModelBase
    {
        private IFTDIServices _ftdiService;
        private bool _isPoweredUp = false;
        private string _linkStatus = "Disable Linking";
        private string _powerDownStatus = "Software Power Down";
        private SelectedDeviceStore _selectedDeviceStore;

        public ExtraCommandsViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            SoftwarePowerDownCommand = new SoftwarePowerDownCommand(this, selectedDeviceStore);
            AutoNegCommand = new AutoNegCommand(this, selectedDeviceStore);
            DisableLinkCommand = new DisableLinkCommand(this, selectedDeviceStore);
            SubSysResetCommand = new ResetCommand(this, selectedDeviceStore);
            SubSysPinResetCommand = new ResetCommand(this, selectedDeviceStore);
            PhyResetCommand = new ResetCommand(this, selectedDeviceStore);
            RegisterActionCommand = new RegisterActionCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged += _selectedDeviceStore_PowerDownStateStatusChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStateStatusChanged;
        }

        public ICommand AutoNegCommand { get; set; }

        public ICommand DisableLinkCommand { get; set; }

        public bool IsADIN1100Board
        {
            get { return _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100 || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1; }
        }

        public bool IsPoweredUp
        {
            get { return _isPoweredUp; }
            set
            {
                _isPoweredUp = value;
                OnPropertyChanged(nameof(IsPoweredUp));
            }
        }

        public string LinkStatus
        {
            get { return _linkStatus; }
            set
            {
                if (value == EthPhyState.Standby.ToString())
                {
                    _linkStatus = "Enable Linking";
                }
                else
                {
                    _linkStatus = "Disable Linking";
                }
                OnPropertyChanged(nameof(LinkStatus));
            }
        }

        public ICommand PhyResetCommand { get; set; }

        public string PowerDownStatus
        {
            get { return _powerDownStatus; }
            set
            {
                _powerDownStatus = value;
                OnPropertyChanged(nameof(PowerDownStatus));
                if (_powerDownStatus == "Software Power Up")
                {
                    IsPoweredUp = false;
                }
                else
                {
                    IsPoweredUp = true;
                }
            }
        }
        public ICommand RegisterActionCommand { get; set; }
        public ICommand SoftwarePowerDownCommand { get; set; }
        public ICommand SubSysPinResetCommand { get; set; }
        public ICommand SubSysResetCommand { get; set; }
        //private IDeviceStatus _deviceStatus => _selectedDeviceStore.SelectedDevice?.DeviceStatus;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged -= _selectedDeviceStore_PowerDownStateStatusChanged;
            _selectedDeviceStore.LinkStatusChanged -= _selectedDeviceStore_LinkStateStatusChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_LinkStateStatusChanged(string linkStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LinkStatus = linkStatus;
            }));
        }

        private void _selectedDeviceStore_PowerDownStateStatusChanged(string powerDownStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                PowerDownStatus = powerDownStatus;
            }));
        }
        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(PowerDownStatus));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(IsPoweredUp));
            OnPropertyChanged(nameof(IsADIN1100Board));
        }
    }
}