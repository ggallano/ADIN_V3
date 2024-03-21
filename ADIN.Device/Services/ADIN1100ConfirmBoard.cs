using System.Collections.Generic;

namespace ADIN.Device.Services
{
    public static class ADIN1100ConfirmBoard
    {
        private static List<string> AcceptedBoardNames = new List<string>()
        {
            //"EVAL-ADIN1100EBZ",
            //"EVAL-ADIN1100FMCZ",
            //"DEMO-ADIN1100-DIZ"
            "EVAL-ADIN2111EBZ"
        };

        public static bool ConfirmADINBoard(string boardName)
        {
            if (AcceptedBoardNames.Contains(boardName))
                return true;

            return false;
        }
    }
}