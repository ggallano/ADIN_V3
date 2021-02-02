//-----------------------------------------------------------------------
// <copyright file="BindingCommand.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace ADIN1100_Eval.ViewModel.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The class used to bind commands and functions
    /// </summary>
    public class BindingCommand<T> : ICommand
    {
        /// <summary>
        /// The Function to run in command
        /// </summary>
        private Action<T> executeCode = null;

        /// <summary>
        /// The Condition to check whether the function can run or not
        /// </summary>
        private Func<T, bool> canExecuteCheckerCode = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingCommand"/> class
        /// </summary>
        /// <param name="toRun">The function to run</param>
        /// <param name="canRun">The condition to be checked</param>
        public BindingCommand(Action<T> toRun, Func<T, bool> canRun = null)
        {
            this.executeCode = toRun;
            this.canExecuteCheckerCode = canRun ?? (_ => true);
        }

        /// <summary>
        /// The event Capturing if the canExecute is changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// THe function to be executed if canExecute returns true
        /// </summary>
        /// <param name="parameter">Name of the function to be executed</param>
        public void Execute(object parameter = null)
        {
            this.executeCode((T)parameter);
        }

        /// <summary>
        /// The function returning whether the execute function can be called
        /// </summary>
        /// <param name="parameter">Name of the function to be executed</param>
        /// <returns>Boolean variable</returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecuteCheckerCode((T)parameter);
        }
    }

    /// <summary>
    /// The class used to bind commands and functions
    /// </summary>
    public class BindingCommand : BindingCommand<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingCommand"/> class
        /// </summary>
        /// <param name="execute">The function to run</param>
        public BindingCommand(Action<object> execute) : base(execute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingCommand"/> class
        /// </summary>
        /// <param name="execute">The function to run</param>
        /// <param name="canExecute">The condition to be checked</param>
        public BindingCommand(Action<object> execute, Func<object, bool> canExecute) : base(execute, canExecute)
        {
        }
    }
}
