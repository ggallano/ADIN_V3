using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Stores;
using ADIN.Device.Models;
using Avalonia.Threading;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ADIN.Avalonia.ViewModels
{
    public class ExtraCommandsViewModel : ViewModelBase
    {
        private bool _enableButton = true;
        private IFTDIServices _ftdiService;
        private bool _isLoadingRegisters = false;
        private bool _isSavingRegisters = false;
        private string _linkStatus = "Disable Linking";
        private string _powerDownStatus = "Software Power Down";
        private SelectedDeviceStore _selectedDeviceStore;
        private NavigationStore _navigationStore;

        public ExtraCommandsViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _navigationStore = navigationStore;
            _ftdiService = ftdiService;

            SoftwarePowerDownCommand = new SoftwarePowerDownCommand(this, selectedDeviceStore);
            AutoNegCommand = new AutoNegCommand(this, selectedDeviceStore);
            DisableLinkCommand = new DisableLinkCommand(this, selectedDeviceStore);
            SubSysResetCommand = new ResetCommand(this, selectedDeviceStore);
            SubSysPinResetCommand = new ResetCommand(this, selectedDeviceStore);
            PhyResetCommand = new ResetCommand(this, selectedDeviceStore);
            //RegisterActionCommand = new RegisterActionCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged += _selectedDeviceStore_PowerDownStateStatusChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStateStatusChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged; 
        }

        public IPhyMode _phyMode => _selectedDeviceStore.SelectedDevice?.PhyMode;

        public ObservableCollection<string> PhyModes => _phyMode?.PhyModes;

        public string ActivePhyMode
        {
            get => _phyMode?.ActivePhyMode;
            set
            {
                _phyMode.ActivePhyMode = value;

                _selectedDeviceStore.OnPhyModeChanged();
                OnPropertyChanged(nameof(ActivePhyMode));
            }
        }



        public ICommand AutoNegCommand { get; set; }

        public ICommand DisableLinkCommand { get; set; }

        public bool EnableButton
        {
            get
            {
                return _enableButton;
            }

            set
            {
                _enableButton = value;
                OnPropertyChanged(nameof(EnableButton));
            }
        }

        public bool ShowSaveLoad => _navigationStore?.CurrentViewModel is RegisterListingViewModel;

        public bool IsLoadingRegisters
        {
            get
            {
                return _isLoadingRegisters;
            }
            set
            {
                _isLoadingRegisters = value;
                if (_isLoadingRegisters)
                    _selectedDeviceStore.OnBusyStateChanged("Loading registers...");
                else
                    _selectedDeviceStore.OnBusyStateChanged("Done");
                OnPropertyChanged(nameof(IsLoadingRegisters));
            }
        }

        public bool IsSavingRegisters
        {
            get
            {
                return _isSavingRegisters;
            }
            set
            {
                _isSavingRegisters = value;
                if (_isSavingRegisters)
                    _selectedDeviceStore.OnBusyStateChanged("Saving registers...");
                else
                    _selectedDeviceStore.OnBusyStateChanged("Done");
                OnPropertyChanged(nameof(IsLoadingRegisters));
            }
        }

#if !DISABLE_TSN && !DISABLE_T1L

        public bool IsT1LBoard
        {
            get
            {
                return ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)) == true;
            }
        }

        public bool IsGigabitBoard => !IsT1LBoard;

#elif !DISABLE_TSN
        public bool IsGigabitBoard { get; } = true;
        public bool IsT1LBoard { get; } = false;
#elif !DISABLE_T1L
        public bool IsGigabitBoard { get; } = false;

        public bool IsT1LBoard { get; } = true;
#endif

        public bool IsPortNumVisible
        {
            get { return _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111; }
        }

        public bool IsResetButtonVisible
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110
                    || _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)
                    return false;
                else
                    return true;
            }
        }

        public string LinkStatus
        {
            get
            {
                return _linkStatus;
            }

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
            get
            {
                return _powerDownStatus;
            }

            set
            {
                _powerDownStatus = value;
                OnPropertyChanged(nameof(PowerDownStatus));
            }
        }

        public ICommand RegisterActionCommand { get; set; }

        public ICommand SoftwarePowerDownCommand { get; set; }

        public ICommand SubSysPinResetCommand { get; set; }

        public ICommand SubSysResetCommand { get; set; }

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.SoftwarePowerDownChanged -= _selectedDeviceStore_PowerDownStateStatusChanged;
            _selectedDeviceStore.LinkStatusChanged -= _selectedDeviceStore_LinkStateStatusChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged -= _selectedDeviceStore_OnGoingCalibrationStatusChanged;
            _navigationStore.CurrentViewModelChanged -= _navigationStore_CurrentViewModelChanged;

            base.Dispose();
        }

        private void _selectedDeviceStore_LinkStateStatusChanged(EthPhyState linkStatus)
        {
            Dispatcher.UIThread.Post(() =>
            {
                LinkStatus = linkStatus.ToString();
            });
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            Dispatcher.UIThread.Post(() =>
            {
                EnableButton = !onGoingCalibrationStatus;
            });
        }

        private void _selectedDeviceStore_PowerDownStateStatusChanged(string powerDownStatus)
        {
            Dispatcher.UIThread.Post(() =>
            {
                PowerDownStatus = powerDownStatus;
            });
        }

        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ShowSaveLoad));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(IsGigabitBoard));
            OnPropertyChanged(nameof(IsT1LBoard));

            OnPropertyChanged(nameof(PowerDownStatus));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(IsPortNumVisible));
            OnPropertyChanged(nameof(IsResetButtonVisible));
            OnPropertyChanged(nameof(EnableButton));
            OnPropertyChanged(nameof(IsLoadingRegisters));
            OnPropertyChanged(nameof(ShowSaveLoad));

            OnPropertyChanged(nameof(PhyModes));
            OnPropertyChanged(nameof(ActivePhyMode));
        }
    }
}