using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IMDIOAPI
    {
        string MdioReadCl22(uint regAddress);
        string MdioReadCl45(uint regAddress);
        string MdioWriteCl22(uint regAddress, uint data);
        string MdioWriteCl45(uint regAddress, uint data);

        string RegisterRead(uint regAddress);
        string RegisterWrite(uint regAddress, uint data);

    }
}
