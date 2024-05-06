using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Models;
using ADIN.WPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class TestModeViewModel : ViewModelBase
    {
        private NavigationStore _navigationStore;
        private SelectedDeviceStore _selectedDeviceStore;

        public TestModeViewModel(NavigationStore navigationStore, SelectedDeviceStore selectedDeviceStore)
        {
            _navigationStore = navigationStore;
            _selectedDeviceStore = selectedDeviceStore;
            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;

            TestModeCmd = new TestModeCommand(this, _selectedDeviceStore);
        }

        public ICommand TestModeCmd { get; set; }

        public TestModeListingModel SelectedTestMode
        {
            get { return _testMode?.TestMode; }
            set
            {
                _testMode.TestMode = value;
                OnPropertyChanged(nameof(SelectedTestMode));
            }
        }
        public List<TestModeListingModel> TestModes => _testMode?.TestModes;
        private ITestMode _testMode => _selectedDeviceStore.SelectedDevice?.TestMode;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedTestMode));
            OnPropertyChanged(nameof(TestModes));
        }
    }
}