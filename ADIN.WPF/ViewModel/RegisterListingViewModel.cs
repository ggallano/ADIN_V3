using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class RegisterListingViewModel : ViewModelBase
    {
        private readonly RegisterService _registerService = new RegisterService();
        private IFTDIServices _ftdiService;
        private string _imagePath;
        private BackgroundWorker _readRegisterWorker;
        private SelectedDeviceStore _selectedDeviceStore;
        private RegisterModel _selectedRegister;
        public RegisterListingViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;


            //SetRegsiterWorker();
            //SaveRegisterDataCommand = new RegisterSaveDataCommand(this);
            //SaveBitFielddataCommand = new RegisterSaveDataCommand(this);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            //_selectedDeviceStore.RegistersValueChanged += _selectedDeviceStore_RegistersValueChanged;
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
            get 
            { 
                return _selectedDevice?.Registers; 
            }
            //set
            //{
            //    //OnPropertyChanged(nameof(Registers));
            //}
        }

        public ICommand SaveBitFielddataCommand { get; set; }

        public ICommand SaveRegisterDataCommand { get; set; }

        //public List<RegisterModel> Registers => _selectedDevice?.Registers;
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

        private ADINDevice _selectedDevice => _selectedDeviceStore.SelectedDevice;

        public void WriteRegister(string name, uint value)
        {
            return;
        }

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            //_selectedDeviceStore.RegistersValueChanged -= _selectedDeviceStore_RegistersValueChanged;
            base.Dispose();
        }

        private void _readRegisterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_readRegisterWorker.CancellationPending)
            {
                try
                {
                    //lock (_thisLock)
                    {
                        if (_selectedDevice != null && _ftdiService.IsComOpen)
                            _selectedDevice.FwAPI.ReadRegsiters();

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnPropertyChanged(nameof(Registers));
                        });
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                }
                e.Result = "Done";
            }
        }

        private void _readRegisterWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("Progress Changed");
        }

        private void _readRegisterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("_readRegisterWorker Completed");
        }

        private void _selectedDeviceStore_RegistersValueChanged()
        {
            OnPropertyChanged(nameof(Registers));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(this.Registers));
        }

        private void SetRegsiterWorker()
        {
            _readRegisterWorker = new BackgroundWorker();
            _readRegisterWorker.WorkerReportsProgress = true;
            _readRegisterWorker.WorkerSupportsCancellation = true;

            _readRegisterWorker.DoWork += _readRegisterWorker_DoWork;
            _readRegisterWorker.RunWorkerCompleted += _readRegisterWorker_RunWorkerCompleted;
            _readRegisterWorker.ProgressChanged += _readRegisterWorker_ProgressChanged;

            _readRegisterWorker.RunWorkerAsync();
        }
    }
}