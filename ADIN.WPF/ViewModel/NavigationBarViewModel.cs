using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class NavigationBarViewModel : ViewModelBase
    {
        public ICommand NavigateLinkPropertiesCommand { get; }
        public ICommand NavigateRegisterAccessCommand { get; }

        public NavigationBarViewModel()
        {
            //NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>()
        }
    }
}
