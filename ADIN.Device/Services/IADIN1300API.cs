using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IADIN1300API
    {
        void Speed1000FdAdvertisement(bool spd1000FdAdv_st = true);
    }
}
