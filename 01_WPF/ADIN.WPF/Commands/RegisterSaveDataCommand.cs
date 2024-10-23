using ADIN.WPF.ViewModel;
using System;

namespace ADIN.WPF.Commands
{
    public class RegisterSaveDataCommand : CommandBase
    {
        private RegisterViewModel registerViewModel;

        public RegisterSaveDataCommand(RegisterViewModel registerViewModel)
        {
            this.registerViewModel = registerViewModel;
        }

        public override void Execute(object parameter)
        {
            //throw new NotImplementedException();
        }
    }
}