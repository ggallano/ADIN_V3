using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ADIN.Avalonia.Commands
{
    public class LogWindowSaveCommand : CommandBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private LogActivityViewModel _viewModel;

        public LogWindowSaveCommand(LogActivityViewModel logActivityViewModel, SelectedDeviceStore selectedDeviceStore)
        {
            _viewModel = logActivityViewModel;
            _selectedDeviceStore = selectedDeviceStore;
        }

        public override void Execute(object parameter)
        {
            DateTime timeNow = DateTime.Now;
            string filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ActivityLog_" + timeNow.ToLongDateString() + "_" + timeNow.Hour + "_ " + timeNow.Minute + "_" + timeNow.Second);

            //SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "LOG | *.log", FileName = filePath, InitialDirectory = System.IO.Directory.GetCurrentDirectory() };
            //if (saveFileDialog.ShowDialog() != true)
            //{
            //    _selectedDeviceStore.OnViewModelErrorOccured("Activity log NOT saved", FeedbackType.Verbose);
            //    return;
            //}

            //try
            //{
            //    filePath = saveFileDialog.FileName;
            //    using (System.IO.StreamWriter file =
            //    new System.IO.StreamWriter(filePath, false))
            //    {
            //        foreach (string line in _viewModel.LogMessages)
            //        {
            //            file.WriteLine(line);
            //        }
            //    }

            //    _selectedDeviceStore.OnViewModelErrorOccured($"Activity log saved to {filePath}", FeedbackType.Verbose);
            //}
            //catch (System.IO.IOException e)
            //{
            //    _selectedDeviceStore.OnViewModelErrorOccured($"Activity log NOT saved to {filePath} due to {e.Message}", FeedbackType.Error);
            //}
        }
    }
}
