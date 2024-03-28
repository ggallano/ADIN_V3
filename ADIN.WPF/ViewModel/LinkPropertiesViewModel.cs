using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class LinkPropertiesViewModel : ViewModelBase
    {
        private ILinkProperties _linkProperties => _selectedDeviceStore.SelectedDevice.LinkProperties;
        private NavigationStore _navigationStore;
        private SelectedDeviceStore _selectedDeviceStore;

        public List<string> SpeedModes => _linkProperties.SpeedMode;


        public LinkPropertiesViewModel(NavigationStore navigationStore, SelectedDeviceStore selectedDeviceStore)
        {
            _navigationStore = navigationStore;
            _selectedDeviceStore = selectedDeviceStore;
        }
    }
}