using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.Feedback;
using Helper.FileToLoad;
using Helper.SaveToFile;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ADIN.WPF.Commands
{
    public class RegisterActionCommand : CommandBase
    {
        private ExtraCommandsViewModel _viewModel;
        private SelectedDeviceStore _selectedDeviceStore;

        public RegisterActionCommand(ExtraCommandsViewModel extraCommandsViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = extraCommandsViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private ADINDevice _selectedDevice => _selectedDeviceStore.SelectedDevice;

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null ||
                _viewModel.IsOngoingCalibrationStatus)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var typeAction = (RegisterActionType)Enum.Parse(typeof(RegisterActionType), parameter.ToString());

            switch (typeAction)
            {
                //case RegisterActionType.Export:
                //    ExportRegister();
                //    break;

                case RegisterActionType.Load:
                    LoadXmlFile();
                    break;

                case RegisterActionType.Save:
                    SaveXmlFile();
                    break;

                default:
                    break;
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }

        //private void ExportRegister()
        //{
        //    var registers = _selectedDevice.Registers;
        //    for (int i = registers.Count() - 1; i > 0; i--)
        //    {
        //        var bitfields = _selectedDevice.Registers[i].BitFields;
        //        for (int y = 0; y < bitfields.Count(); y++)
        //        {
        //            if (bitfields[y].Documentation != null)
        //                _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}]   -->{bitfields[y].Name} = {bitfields[y].Value} [{bitfields[y].Documentation.Replace("\n", string.Empty).Replace("\r", string.Empty)}]", FeedbackType.Verbose);
        //            else
        //                _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}]   -->{bitfields[y].Name} = {bitfields[y].Value}", FeedbackType.Verbose);
        //        }

        //        if (registers[i].Documentation != null)
        //            _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}] {registers[i].Name} = {registers[i].Value} [{registers[i].Documentation.Replace("\n", string.Empty).Replace("\r", string.Empty)}]", FeedbackType.Verbose);
        //        else
        //            _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}] {registers[i].Name} = {registers[i].Value}", FeedbackType.Verbose);
        //    }
        //}

        private void LoadXmlFile()
        {
            XmlFileLoader loader = new XmlFileLoader();
            ObservableCollection<RegisterModel> register_temp = new ObservableCollection<RegisterModel>();

            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
            var ofdResult = ofd.ShowDialog();

            if (ofdResult == false)
                return;

            try
            {
                _selectedDeviceStore.OnViewModelFeedbackLog($"Load Register....", FeedbackType.Verbose);
                register_temp = loader.XmlFileLoadContent(ofd.FileName);
                foreach (var register in register_temp)
                {
                    string response = string.Empty;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        response = fwADIN1100API.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        response = fwADIN1200API.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        response = fwADIN1300API.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
                    }
                    //var response = _selectedDevice.FwAPI.RegisterWrite(register.Address, Convert.ToUInt32(register.Value, 16));
                    if (!response.Contains("OK"))
                    {
                        _selectedDeviceStore.OnViewModelErrorOccured($"[Load Register] Error in writing the register[{register.Name}]");
                        continue;
                    }
                    _selectedDeviceStore.OnViewModelFeedbackLog($"Loading the register: {register.Name}, value: {register.Value}", FeedbackType.Verbose);
                }
                
                _selectedDeviceStore.OnViewModelFeedbackLog($"Registers load from {ofd.FileName}.", FeedbackType.Verbose);
            }
            catch (InvalidOperationException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("[Load Registers] Invalid operation occurred while attempting to load the XML");
                throw;
            }
        }

        private void SaveXmlFile()
        {
            AbstractFileWriter writer = new XmlFileWriter();

            var sfd = new Microsoft.Win32.SaveFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
            var sfdResult = sfd.ShowDialog();

            if (sfdResult == false)
                return;

            _selectedDeviceStore.OnViewModelFeedbackLog($"Refreshing register contents before saving to {sfd.FileName}");
            var registers = _selectedDevice.FwAPI.ReadRegsiters();
            _selectedDeviceStore.OnViewModelFeedbackLog($"Refreshing register contents done.");

            try
            {
                writer.WriteContent(sfd.FileName, registers);
                _selectedDeviceStore.OnViewModelFeedbackLog($"Registers saved to {sfd.FileName}");
            }
            catch (InvalidOperationException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("[Save Registers] Invalid operation occurred while attempting to save the XML");
            }
        }
    }
}