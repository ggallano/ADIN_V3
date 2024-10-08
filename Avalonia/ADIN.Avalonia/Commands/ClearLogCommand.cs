using ADIN.Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Avalonia.Commands
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
