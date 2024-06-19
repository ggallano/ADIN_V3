using ADI.Register.Services;
using ADIN.Device.Models;
using FTDIChip.Driver.Services;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace ADIN.Device.Services
{
    public static class ADINConfirmBoard
    {
        private static List<string> AcceptedBoardNames = new List<string>()
        {
            "EVAL-ADIN1100EBZ",
            "EVAL-ADIN1100FMCZ",
            "DEMO-ADIN1100-DIZ",
            "DEMO-ADIN1100D2Z",

            "EVAL-ADIN1110EBZ",
            //"EVAL-ADIN2111EBZ",

            "ADIN1300 MDIO DONGLE",
            "EVAL-ADIN1300",
            "ADIN1200 MDIO DONGLE",
            "EVAL-ADIN1200",
        };

        public static bool ConfirmADINBoard(string boardName)
        {
            if (AcceptedBoardNames.Contains(boardName))
                return true;

            return false;
        }

        public static ADINDevice GetADINBoard(string BoardName, IFTDIServices ftdtService, IRegisterService _registerService, object mainLock)
        {
            if (BoardName == "EVAL-ADIN1100EBZ" 
             || BoardName == "EVAL-ADIN1100FMCZ" 
             || BoardName == "DEMO-ADIN1100-DIZ" 
             || BoardName == "DEMO-ADIN1100D2Z")
            {
                return new ADINDevice(new ADIN1100Model(ftdtService, _registerService, mainLock));
            }

            if (BoardName == "EVAL-ADIN1110EBZ")
            {
                return new ADINDevice(new ADIN1110Model(ftdtService, _registerService, mainLock));
            }

            if (BoardName == "EVAL-ADIN1300" 
             || BoardName == "ADIN1300 MDIO DONGLE")
            {
                return new ADINDevice(new ADIN1300Model(ftdtService, _registerService, mainLock));
            }

            if (BoardName == "EVAL-ADIN1200" 
             || BoardName == "ADIN1200 MDIO DONGLE")
            {
                return new ADINDevice(new ADIN1200Model(ftdtService, _registerService, mainLock));
            }

            return null;
        }
    }
}