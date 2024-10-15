using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class LoopbackModel
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public LoopBackMode EnumLoopbackType { get; set; }
        public List<string> DisabledModes { get; set; }
    }
}
