using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADI.Register.Models
{
    public class RegisterSet
    {
        public ObservableCollection<RegisterModel> Registers { get; set; }
    }
}