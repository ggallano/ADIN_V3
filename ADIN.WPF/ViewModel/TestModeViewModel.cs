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
        private SelectedDeviceStore _selectedDeviceStore;

        public TestModeViewModel(SelectedDeviceStore selectedDeviceStore)
        {
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

        public uint TestModeFrameLengthValue
        {
            get { return _testMode?.TestModeFrameLength ?? 0; }
            set
            {
                _testMode.TestModeFrameLength = value;
                OnPropertyChanged(nameof(TestModeFrameLengthValue));
            }
        }

        public List<TestModeListingModel> TestModes => _testMode?.TestModes;
        private ITestMode _testMode => _selectedDeviceStore.SelectedDevice?.TestMode;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedTestMode));
            OnPropertyChanged(nameof(TestModes));
            OnPropertyChanged(nameof(TestModeFrameLengthValue));
        }
    }
}