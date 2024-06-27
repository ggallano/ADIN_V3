using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Models;
using ADIN.WPF.Stores;
using System.Collections.Generic;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class TestModeViewModel : ViewModelBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private TestModeListingModel _TM_NoDevice { get; set; }
        private List<TestModeListingModel> _testModeList_NoDeviceSelected { get; set; }

        public TestModeViewModel(SelectedDeviceStore selectedDeviceStore)
        {
            _selectedDeviceStore = selectedDeviceStore;

            TestModeCmd = new TestModeCommand(this, _selectedDeviceStore);
            _TM_NoDevice = new TestModeListingModel()
            {
                Name1 = "No test mode available",
                Description = ""
            };
            _testModeList_NoDeviceSelected = new List<TestModeListingModel>()
            { 
                _TM_NoDevice 
            };

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
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

        public List<TestModeListingModel> TestModes
        {
            get 
            {
                if (_testMode != null)
                    return _testMode.TestModes;
                return _testModeList_NoDeviceSelected;
            }
        }

        private ITestMode _testMode => _selectedDeviceStore.SelectedDevice?.TestMode;

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(SelectedTestMode));
            OnPropertyChanged(nameof(TestModes));
            OnPropertyChanged(nameof(TestModeFrameLengthValue));
            OnPropertyChanged(nameof(IsDeviceSelected));
        }
    }
}