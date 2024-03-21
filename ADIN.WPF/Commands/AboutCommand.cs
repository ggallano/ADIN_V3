using ADIN.WPF.ViewModel;

namespace ADIN.WPF.Commands
{
    public class AboutCommand : CommandBase
    {
        private MenuItemViewModel _viewModel;

        public AboutCommand(MenuItemViewModel menuItemViewModel)
        {
            _viewModel = menuItemViewModel;
        }

        public override void Execute(object parameter)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}