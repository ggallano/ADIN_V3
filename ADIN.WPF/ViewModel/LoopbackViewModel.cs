using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class LoopbackViewModel : ViewModelBase
    {
        private IFTDIServices _ftdiService;
        private bool _isRxSuppression;
        private bool _isTxSuppression;
        private SelectedDeviceStore _selectedDeviceStore;

        public LoopbackViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;

            LoopbackCmd = new LoopbackCommand(this, _selectedDeviceStore);
        }

        public ICommand LoopbackCmd { get; set; }

        public string ImagePath => _loopback?.SelectedLoopback.ImagePath;

        public bool IsRxSuppression
        {
            get { return _loopback?.SelectedLoopback.RxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.SelectedLoopback.RxSuppression = value;
                }
                OnPropertyChanged(nameof(IsRxSuppression));
            }
        }

        public bool IsTxSuppression
        {
            get { return _loopback?.SelectedLoopback.TxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.SelectedLoopback.TxSuppression = value;
                }
                OnPropertyChanged(nameof(IsTxSuppression));
            }
        }

        public bool IsLoopback_None
        {
            get { return _loopback.SelectedLoopback.EnumLoopbackType == LoopBackMode.OFF; }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback.EnumLoopbackType = LoopBackMode.OFF;
                    _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
            }
        }
        public bool IsLoopback_Digital
        {
            get { return _loopback.SelectedLoopback.EnumLoopbackType == LoopBackMode.Digital; }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback.EnumLoopbackType = LoopBackMode.Digital;
                    _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
            }
        }

        public List<LoopbackListingModel> Loopbacks => _loopback?.Loopbacks;

        public LoopbackListingModel SelectedLoopback
        {
            set
            {
                //if (value != null)
                //{
                //    _isTxSuppression = IsTxSuppression;
                //    _isRxSuppression = IsRxSuppression;
                //    _loopback.SelectedLoopback = value;
                //    _loopback.SelectedLoopback.TxSuppression = _isTxSuppression;
                //    _loopback.SelectedLoopback.RxSuppression = _isRxSuppression;
                //    _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                //}
                //OnPropertyChanged(nameof(ImagePath));
                //OnPropertyChanged(nameof(IsLoopback_None));
                //OnPropertyChanged(nameof(IsLoopback_Digital));
            }
        }

        private ILoopback _loopback => _selectedDeviceStore.SelectedDevice?.Loopback;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedLoopback));
            OnPropertyChanged(nameof(Loopbacks));
            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(IsLoopback_None));
            OnPropertyChanged(nameof(IsLoopback_Digital));
        }
    }
}