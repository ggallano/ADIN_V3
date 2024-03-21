using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.Feedback;
using Helper.FileToLoad;
using Helper.SaveToFile;
using System;
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

        private ADINDeviceModel _selectedDevice => _selectedDeviceStore.SelectedDevice;

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            var typeAction = (RegisterActionType)Enum.Parse(typeof(RegisterActionType), parameter.ToString());

            switch (typeAction)
            {
                case RegisterActionType.Export:
                    ExportRegister();
                    break;

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

        private void ExportRegister()
        {
            var registers = _selectedDevice.Registers;
            for (int i = registers.Count() - 1; i > 0; i--)
            {
                var bitfields = _selectedDevice.Registers[i].BitFields;
                for (int y = 0; y < bitfields.Count(); y++)
                {
                    if (bitfields[y].Documentation != null)
                        _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}]   -->{bitfields[y].Name} = {bitfields[y].Value} [{bitfields[y].Documentation.Replace("\n", string.Empty).Replace("\r", string.Empty)}]", FeedbackType.Verbose);
                    else
                        _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}]   -->{bitfields[y].Name} = {bitfields[y].Value}", FeedbackType.Verbose);
                }

                if (registers[i].Documentation != null)
                    _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}] {registers[i].Name} = {registers[i].Value} [{registers[i].Documentation.Replace("\n", string.Empty).Replace("\r", string.Empty)}]", FeedbackType.Verbose);
                else
                    _selectedDeviceStore.OnViewModelErrorOccured($"[{_selectedDevice.SerialNumber}] {registers[i].Name} = {registers[i].Value}", FeedbackType.Verbose);
            }
        }

        private void LoadXmlFile()
        {
            XmlFileLoader loader = new XmlFileLoader();

            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
            var ofdResult = ofd.ShowDialog();

            if (ofdResult == false)
                return;

            try
            {
                _selectedDeviceStore.OnViewModelErrorOccured($"Load Register....", FeedbackType.Verbose);
                _selectedDevice.Registers = loader.XmlFileLoadContent(ofd.FileName);
                foreach (var register in _selectedDevice.Registers)
                {
                    var response = _selectedDevice.FirmwareAPI.MdioWriteCl45(register.Address, Convert.ToUInt32(register.Value, 16));
                    if (!response.Contains("OK"))
                    {
                        _selectedDeviceStore.OnViewModelErrorOccured($"[Load Register] Error in writing the register[{register.Name}]");
                        continue;
                    }
                    _selectedDeviceStore.OnViewModelErrorOccured($"Loading the register: {register.Name}, value: {register.Value}", FeedbackType.Verbose);
                }
                
                _selectedDeviceStore.OnViewModelErrorOccured($"Registers load from {ofd.FileName}.", FeedbackType.Verbose);
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

            _selectedDeviceStore.OnViewModelErrorOccured($"Refreshing register contents before saving to {sfd.FileName}");
            var registers = _selectedDevice.FirmwareAPI.GetStatusRegisters();
            _selectedDeviceStore.OnViewModelErrorOccured($"Refreshing register contents done.");

            try
            {
                writer.WriteContent(sfd.FileName, registers);
                _selectedDeviceStore.OnViewModelErrorOccured($"Registers saved to {sfd.FileName}");
            }
            catch (InvalidOperationException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured("[Save Registers] Invalid operation occurred while attempting to save the XML");
            }
        }
    }
}