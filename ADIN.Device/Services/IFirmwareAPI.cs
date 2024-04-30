using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADIN.Device.Services
{
    public interface IFirmwareAPI : IADIN1300API, IADIN1200API
    {
        void ReadRegsiters();
    }
}