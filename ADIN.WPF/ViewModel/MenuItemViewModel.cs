using ADIN.WPF.Commands;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class MenuItemViewModel : ViewModelBase
    {
        public MenuItemViewModel()
        {
            AboutCommand = new AboutCommand(this);
        }

        public ICommand AboutCommand { get; set; }
    }
}