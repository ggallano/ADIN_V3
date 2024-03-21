using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class ClearLogCommand : CommandBase
    {
        private LogActivityViewModel _viewModel;

        public ClearLogCommand(LogActivityViewModel logActivityViewModel)
        {
            _viewModel = logActivityViewModel;
        }

        public override void Execute(object parameter)
        {
            _viewModel.LogMessages.Clear();
        }
    }
}