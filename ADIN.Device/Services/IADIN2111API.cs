using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN2111API : IADIN1100API
    {
        uint GetPortNum ();
        void SetPortNum (uint portNum);
    }
}
