using ADIN.Device.Models;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using Helper.SaveToFile;
using Microsoft.Win32;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.WPF.Commands
{
    public class TDRSaveCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private FaultDetectorViewModel _viewModel;

        public TDRSaveCommand(FaultDetectorViewModel viewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = viewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            StringBuilder content = new StringBuilder();
            AbstractFileWriter writer = new CsvFileWriter();

            try
            {
                switch ((CalibrateType)Enum.Parse(typeof(CalibrateType), parameter.ToString()))
                {
                    case CalibrateType.Offset:
                        saveFileDialog.Filter = "Calibrate Offset file (*.cof)|*.cof";
                        var result = _selectedDeviceStore.SelectedDevice.FirmwareAPI.GetOffset();

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            Task.Run(() =>
                            {
                                content.Append(result);
                                writer.WriteContent(saveFileDialog.FileName, content);
                            });
                        }
                        break;

                    case CalibrateType.Cable:
                        saveFileDialog.Filter = "Calibrate Cable file (*.ccf)|*.ccf";
                        var results = _selectedDeviceStore.SelectedDevice.FirmwareAPI.GetCoeff();

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            Task.Run(() =>
                            {
                                content.Append($"{results[0]},");
                                content.Append($"{results[1]},");
                                content.Append($"{results[2]}");
                                writer.WriteContent(saveFileDialog.FileName, content);
                            });
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (ApplicationException ex)
            {
                // insert here alternate tasks
            }
            catch (ArgumentNullException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
            catch (FormatException ex)
            {
                _selectedDeviceStore.OnViewModelErrorOccured(ex.Message);
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}