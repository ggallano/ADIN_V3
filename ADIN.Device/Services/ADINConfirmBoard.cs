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
            "EVAL-ADIN2111EBZ",
            "EVAL-ADIN1110EBZ",
        };

        public static bool ConfirmADINBoard(string boardName)
        {
            if (AcceptedBoardNames.Contains(boardName))
                return true;

            return false;
        }

        public static ADINDevice GetADINBoard(string BoardName, IFTDIServices ftdtService)
        {
            if (BoardName == "EVAL-ADIN1100EBZ" || BoardName == "EVAL-ADIN1100FMCZ" || BoardName == "DEMO-ADIN1100-DIZ")
            {
                return new ADINDevice(new ADIN1100Model(ftdtService));
            }

            if (BoardName == "EVAL-ADIN2111EBZ")
            {
                return new ADINDevice(new ADIN2111Model(ftdtService));
            }

            //if (BoardName == "EVAL-ADIN1110EBZ")
            //{

            //}

            return null;
        }
    }
}