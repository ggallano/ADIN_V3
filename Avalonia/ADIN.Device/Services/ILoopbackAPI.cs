using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface ILoopbackAPI
    {
        void SetLoopbackSetting(LoopbackModel loopback);
        void SetTxSuppression(bool isTxSuppressed);
        void SetRxSuppression(bool isRxSuppressed);
    }
}
