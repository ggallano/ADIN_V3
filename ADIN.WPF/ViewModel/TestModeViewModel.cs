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
        private TestModeListingModel _selectedTestMode;

        public TestModeViewModel(SelectedDeviceStore selectedDeviceStore)
        {
            _selectedDeviceStore = selectedDeviceStore;

            TestModeCommand = new TestModeCommand(this, _selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.TestmodeChanged += _selectedDeviceStore_TestmodeChanged;
            _selectedDeviceStore.TestModeStateChanged += _selectedDeviceStore_TestModeStateChanged;
        }

        public TestModeListingModel SelectedTestMode
        {
            get { return _selectedDevice?.TestMode.TestMode ?? _selectedDevice?.TestMode.TestMode; }
            set
            {
                if (value != null && _selectedTestMode != value)
                {
                    _selectedTestMode = value;
                    _testMode.TestMode = value;
                }
                OnPropertyChanged(nameof(SelectedTestMode));
            }
        }

        public ICommand TestModeCommand { get; set; }
        public List<TestModeListingModel> TestModes => _selectedDevice?.TestMode.TestModes;
        private ADINDeviceModel _selectedDevice => _selectedDeviceStore.SelectedDevice;
        private TestModeModel _testMode => _selectedDevice.TestMode;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.TestmodeChanged -= _selectedDeviceStore_TestmodeChanged;
            _selectedDeviceStore.TestModeStateChanged -= _selectedDeviceStore_TestModeStateChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedTestMode));
            OnPropertyChanged(nameof(TestModes));
        }

        private void _selectedDeviceStore_TestmodeChanged(TestModeType obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedTestMode = _testMode.TestModes.Where(x => x.TestmodeType == obj).ToList()[0];
                //TestModeCommand.Execute(SelectedTestMode);
            }));
        }

        private void _selectedDeviceStore_TestModeStateChanged(TestModeListingModel obj)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SelectedTestMode = obj;
                TestModeCommand.Execute(SelectedTestMode);
            }));
        }
    }
}