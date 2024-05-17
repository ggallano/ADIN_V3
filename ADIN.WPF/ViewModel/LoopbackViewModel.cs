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

        public LoopbackViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.LoopbackChanged += _selectedDeviceStore_LoopbackChanged;
            _selectedDeviceStore.LoopbackStateChanged += _selectedDeviceStore_LoopbackStateChanged;
        }

        public string ImagePath => _loopback?.Loopback.ImagePath;

        public bool IsRxSuppression
        {
            get { return _loopback?.Loopback.RxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _isRxSuppression = value;
                    _loopback.Loopback.RxSuppression = value;
                    _selectedDeviceStore.SelectedDevice.FwAPI.SetRxSuppressionSetting(value);
                }
                OnPropertyChanged(nameof(IsRxSuppression));
            }
        }

        public bool IsTxSuppression
        {
            get { return _loopback?.Loopback.TxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _isTxSuppression = value;
                    _loopback.Loopback.TxSuppression = value;
                    _selectedDeviceStore.SelectedDevice.FwAPI.SetTxSuppressionSetting(value);
                }
                OnPropertyChanged(nameof(IsTxSuppression));
            }
        }

        public List<LoopbackListingModel> Loopbacks => _loopback?.Loopbacks;

        public LoopbackListingModel SelectedLoopback
        {
            get { return _loopback?.Loopback; }
            set
            {
                if (value != null)
                {
                    _selectedLoopback = value;
                    _loopback.Loopback = value;
                    _loopback.Loopback.RxSuppression = _isRxSuppression;
                    _loopback.Loopback.TxSuppression = _isTxSuppression;
                    _selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.Loopback);
                }
                OnPropertyChanged(nameof(SelectedLoopback));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private ILoopback _loopback => _selectedDeviceStore.SelectedDevice?.Loopback;

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
                SelectedLoopback = _loopback.Loopbacks.Where(x => x.EnumLoopbackType == obj).ToList()[0];
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