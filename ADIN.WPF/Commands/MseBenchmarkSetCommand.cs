using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using SciChart.Charting.Model.ChartSeries;
using System.Windows.Media;

namespace ADIN.WPF.Commands
{
    public class MseBenchmarkSetCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ActiveLinkMonitoringViewModel _viewModel;

        public MseBenchmarkSetCommand(ActiveLinkMonitoringViewModel activeLinkMonitoringViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = activeLinkMonitoringViewModel;
            _selectedDeviceStore = selectedDeviceStore;

            _viewModel.PropertyChanged += _activeLinkMonitoringViewModel_PropertyChanged;
        }

        private string _mseValue => _viewModel.MseValue;

        public override bool CanExecute(object parameter)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return false;
            return base.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            if (_mseValue.Contains("N/A") || _mseValue.Contains("∞"))
            {
                _selectedDeviceStore.OnViewModelErrorOccured("Could not set the MSE Benchmark");
                return;
            }
            if (_viewModel.Annotations.Count != 0)
                _viewModel.Annotations.Remove(_viewModel.Annotations[0]);

            var temp = double.Parse(_mseValue.Replace("dB", "").Trim());
            _viewModel.MseBenchmarkValue = _mseValue;
            _viewModel.IsMseBenchmarkVisible = true;
            _viewModel.Annotations.Add(new HorizontalLineAnnotationViewModel() { Y1 = temp, StrokeThickness = 2, Stroke = Colors.Green });
        }

        private void _activeLinkMonitoringViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnCanExecuteChanged();
        }
    }
}