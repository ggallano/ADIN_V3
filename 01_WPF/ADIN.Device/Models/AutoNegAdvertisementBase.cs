using System.Collections.Generic;

namespace ADI.Device.Models
{
    public abstract class AutoNegAdvertisementBase
    {
        public List<string> Items { get; set; } = new List<string>();
        public abstract void Execute();
    }
}
