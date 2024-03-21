using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly RegisterService _registerService = new RegisterService();
        private IFTDIServices _ftdiService;
        private string _imagePath;
        private SelectedDeviceStore _selectedDeviceStore;
        private RegisterModel _selectedRegister;

        public RegisterViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            SaveRegisterDataCommand = new RegisterSaveDataCommand(this);
            //SaveBitFielddataCommand = new RegisterSaveDataCommand(this);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.RegistersValueChanged += _selectedDeviceStore_RegistersValueChanged;
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = "../Images/" + value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        //public ObservableCollection<RegisterModel> Registers => new ObservableCollection<RegisterModel>(_selectedDevice?.Registers);
        //public ObservableCollection<RegisterModel> Registers => _selectedDevice?.Registers;
        public ObservableCollection<RegisterModel> Registers
        {
            get { return _selectedDevice?.Registers; }
        }

        //public List<RegisterModel> Registers => _selectedDevice?.Registers;


        public ICommand SaveRegisterDataCommand { get; set; }

        public ICommand SaveBitFielddataCommand { get; set; }

        public RegisterModel SelectedRegister
        {
            get
            {
                return _selectedRegister;
            }
            set
            {
                _selectedRegister = value;
                ImagePath = _selectedRegister.Image;
                OnPropertyChanged(nameof(SelectedRegister));
            }
        }

        private ADINDeviceModel _selectedDevice => _selectedDeviceStore.SelectedDevice;

        public void WriteRegister(string name, uint value)
        {
            return;
        }

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.RegistersValueChanged -= _selectedDeviceStore_RegistersValueChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_RegistersValueChanged()
        {
            OnPropertyChanged(nameof(Registers));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(Registers));
        }
    }
}