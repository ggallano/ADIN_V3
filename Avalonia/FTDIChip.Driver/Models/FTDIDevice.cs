using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTDIChip.Driver.Models
{
    public class FTDIDevice
    {
        public uint ID { get; set; }

        public string SerialNumber { get; set; }

        public string Description { get; set; }
    }
}
