using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IFrameGenCheckerAPI
    {
        bool isFrameGenCheckerOngoing { get; set; }
        void SetFrameCheckerSetting(FrameGenCheckerModel frameContent);
        void ResetFrameGenCheckerStatistics();
    }
}
