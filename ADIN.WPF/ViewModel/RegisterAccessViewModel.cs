using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class RegisterAccessViewModel : ViewModelBase
    {
        public ICommand NavigateLinkPorpCommand { get; set; }

        public RegisterAccessViewModel(NavigationStore navigationStore)
        {
            NavigateLinkPorpCommand = new NavigateCommand<LinkPropertiesViewModel>(navigationStore, ()=> new LinkPropertiesViewModel(navigationStore));
        }
    }
}