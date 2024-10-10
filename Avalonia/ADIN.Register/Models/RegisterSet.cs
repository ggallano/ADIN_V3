using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Register.Models
{
    public class RegisterSet
    {
        public ObservableCollection<RegisterModel> Registers { get; set; }
    }
}
