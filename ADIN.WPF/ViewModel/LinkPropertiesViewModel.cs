using ADIN.Device.Models;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ADIN.WPF.ViewModel
{
    public class LinkPropertiesViewModel : ViewModelBase
    {
        private IFTDIServices _ftdiService;
        private AutoNegMasterSlaveAdvertisementModel _selectedAutoNegMasterSlaveAdvertisement;
        private AutoNegTxLevelAdvertisementModel _selectedAutoNegTxLevelAdvertisement;
        private SelectedDeviceStore _selectedDeviceStore;

        public LinkPropertiesViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.NegotiationMasterSlaveChanged += _selectedDeviceStore_NegotiationMasterSlaveChanged;
            _selectedDeviceStore.PeakVoltageChanged += _selectedDeviceStore_PeakVoltageChanged;
        }

        public List<AutoNegMasterSlaveAdvertisementModel> AutoNegMasterSlaveAdvertisements => _selectedDevice?.LinkProperty.AutoNegMasterSlaveAdvertisements;
        public List<AutoNegTxLevelAdvertisementModel> AutoNegTxLevelAdvertisements => _selectedDevice?.LinkProperty.AutoNegTxLevelAdvertisements;

        public AutoNegMasterSlaveAdvertisementModel SelectedAutoNegMasterSlaveAdvertisement
        {
            get { return _selectedDevice?.LinkProperty.AutoNegMasterSlaveAdvertisement ?? _selectedDevice?.LinkProperty.AutoNegMasterSlaveAdvertisement; }
            set
            {
                if (value != null && _selectedAutoNegMasterSlaveAdvertisement != value)
                {
                    _selectedAutoNegMasterSlaveAdvertisement = value;
                    _linkProperty.AutoNegMasterSlaveAdvertisement = value;
                    _selectedDevice.FirmwareAPI.SetNegotiateMasterSlaveSetting((AutoNegMasterSlaveAdvertisementItem)Enum.Parse(typeof(AutoNegMasterSlaveAdvertisementItem), value.Name));
                }
                OnPropertyChanged(nameof(SelectedAutoNegMasterSlaveAdvertisement));
            }
        }

        public AutoNegTxLevelAdvertisementModel SelectedAutoNegTxLevelAdvertisement
        {
            get { return _selectedDevice?.LinkProperty.AutoNegTxLevelAdvertisement ?? _selectedDevice?.LinkProperty.AutoNegTxLevelAdvertisement; }
            set
            {
                if (value != null && _selectedAutoNegTxLevelAdvertisement != value)
                {
                    _selectedAutoNegTxLevelAdvertisement = value;
                    _linkProperty.AutoNegTxLevelAdvertisement = value;
                    _selectedDevice.FirmwareAPI.SetPeakToPeakVoltageSetting((PeakVoltageAdvertisementItem)Enum.Parse(typeof(PeakVoltageAdvertisementItem), value.Name));
                }
                OnPropertyChanged(nameof(SelectedAutoNegTxLevelAdvertisement));
            }
        }

        private LinkPropertyModel _linkProperty => _selectedDevice.LinkProperty;
        private ADINDeviceModel _selectedDevice => _selectedDeviceStore.SelectedDevice;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.NegotiationMasterSlaveChanged -= _selectedDeviceStore_NegotiationMasterSlaveChanged;
            _selectedDeviceStore.PeakVoltageChanged -= _selectedDeviceStore_PeakVoltageChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_NegotiationMasterSlaveChanged(AutoNegMasterSlaveAdvertisementItem obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedAutoNegMasterSlaveAdvertisement = _linkProperty.AutoNegMasterSlaveAdvertisements.Where(x => x.Name == obj.ToString()).ToList()[0];
            }));
        }

        private void _selectedDeviceStore_PeakVoltageChanged(PeakVoltageAdvertisementItem obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedAutoNegTxLevelAdvertisement = _linkProperty.AutoNegTxLevelAdvertisements.Where(x => x.Name == obj.ToString()).ToList()[0];
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedAutoNegMasterSlaveAdvertisement));
            OnPropertyChanged(nameof(SelectedAutoNegTxLevelAdvertisement));
            OnPropertyChanged(nameof(AutoNegMasterSlaveAdvertisements));
            OnPropertyChanged(nameof(AutoNegTxLevelAdvertisements));
        }
    }
}