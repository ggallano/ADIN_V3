using ADIN.Device.Models;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ADIN.WPF.ViewModel
{
    public class LoopbackViewModel : ViewModelBase
    {
        private IFTDIServices _ftdiService;
        private bool _isRxSuppression;
        private bool _isTxSuppression;
        private SelectedDeviceStore _selectedDeviceStore;
        private LoopbackListingModel _selectedLoopback;

        public LoopbackViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, LogActivityViewModel logActivityViewModel)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.LoopbackChanged += _selectedDeviceStore_LoopbackChanged;
            _selectedDeviceStore.LoopbackStateChanged += _selectedDeviceStore_LoopbackStateChanged;
        }

        public string ImagePath => _selectedDevice?.Loopback.Loopback.ImagePath;

        public bool IsRxSuppression
        {
            get { return _selectedDevice?.Loopback.Loopback.RxSuppression ?? false; }
            set
            {
                if (_selectedDevice != null)
                {
                    _isRxSuppression = value;
                    _selectedDevice.Loopback.Loopback.RxSuppression = value;
                    _selectedDevice.FirmwareAPI.SetRxSuppressionSetting(value);
                }
                OnPropertyChanged(nameof(IsRxSuppression));
            }
        }

        public bool IsTxSuppression
        {
            get { return _selectedDevice?.Loopback.Loopback.TxSuppression ?? false; }
            set
            {
                if (_selectedDevice != null)
                {
                    _isTxSuppression = value;
                    _selectedDevice.Loopback.Loopback.TxSuppression = value;
                    _selectedDevice.FirmwareAPI.SetTxSuppressionSetting(value);
                }
                OnPropertyChanged(nameof(IsTxSuppression));
            }
        }

        public List<LoopbackListingModel> Loopbacks => _selectedDevice?.Loopback.Loopbacks;

        public LoopbackListingModel SelectedLoopback
        {
            get { return _selectedDevice?.Loopback.Loopback ?? _selectedDevice?.Loopback.Loopbacks[0]; }
            set
            {
                if (value != null)
                {
                    _selectedLoopback = value;
                    _selectedDevice.Loopback.Loopback = value;
                    _selectedDevice.Loopback.Loopback.RxSuppression = _isRxSuppression;
                    _selectedDevice.Loopback.Loopback.TxSuppression = _isTxSuppression;
                    _selectedDevice.FirmwareAPI.SetLoopbackSetting(_selectedDevice?.Loopback.Loopback);
                }
                OnPropertyChanged(nameof(SelectedLoopback));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private ADINDeviceModel _selectedDevice => _selectedDeviceStore.SelectedDevice;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.LoopbackChanged -= _selectedDeviceStore_LoopbackChanged;
            _selectedDeviceStore.LoopbackStateChanged -= _selectedDeviceStore_LoopbackStateChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_LoopbackChanged(LoopBackMode obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedLoopback = _selectedDevice.Loopback.Loopbacks.Where(x => x.EnumLoopbackType == obj).ToList()[0];
            }));
        }

        private void _selectedDeviceStore_LoopbackStateChanged(LoopbackListingModel obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedLoopback = obj;
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedLoopback));
            OnPropertyChanged(nameof(Loopbacks));
            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));
            OnPropertyChanged(nameof(ImagePath));
        }
    }
}