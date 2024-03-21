using System;
using System.Globalization;

namespace Helper.ReadAccessValidate
{
    public static class RegisterAccessValidate
    {
        public static bool ValidateInput(string inputString, out uint value)
        {
            bool result = false;
            value = 0;

            if (UInt32.TryParse(inputString,NumberStyles.HexNumber,null, out value))
                result = true;

            return result;
        }
    }
}
