using ADIN.Device.Models;
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
        private string _linkStatus = "-";
        private bool SelectedSoftPowerDownText;
        private SelectedDeviceStore _selectedDeviceStore;

        public ExtraCommandsViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            SoftwarePowerDownCommand = new SoftwarePowerDownCommand(this, selectedDeviceStore);
            //AutoNegCommand = new AutoNegCommand(this, selectedDeviceStore);
            //SubSysResetCommand = new ResetCommand(this, selectedDeviceStore);
            //PhyResetCommand = new ResetCommand(this, selectedDeviceStore);
            //RegisterActionCommand = new RegisterActionCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged += _selectedDeviceStore_LinkStateStatusChanged;
        }

        public ICommand AutoNegCommand { get; set; }

        public string LinkStatus
        {
            get { return _linkStatus; }
            set
            {
                _linkStatus = value;
                OnPropertyChanged(nameof(LinkStatus));
            }
        }
        public string SoftwarePowerDownButtonText
        {
            get
            {
                if(_deviceStatus?.IsSoftwarePowerDown == true)
                {
                    return "Software Power Up";
                }
                else
                {
                    return "Software Power Down";
                }
            }
            set
            {
                if(value == "Software Power Up")
                {
                    _deviceStatus.IsSoftwarePowerDown = true;
                }
                else
                {
                    _deviceStatus.IsSoftwarePowerDown = false;
                }
                OnPropertyChanged(nameof(SoftwarePowerDownButtonText));
            }
        }
        public ICommand PhyResetCommand { get; set; }
        public ICommand SoftwarePowerDownCommand { get; set; }
        public ICommand SubSysResetCommand { get; set; }
        public ICommand RegisterActionCommand { get; set; }

        private IDeviceStatus _deviceStatus => _selectedDeviceStore.SelectedDevice?.DeviceStatus;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged -= _selectedDeviceStore_LinkStateStatusChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_LinkStateStatusChanged(string linkStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LinkStatus = linkStatus;
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(SoftwarePowerDownButtonText));
        }
    }
}