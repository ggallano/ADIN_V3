using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.ReadAccessValidate
{
    public static class RegisterAccessValidate
    {
        public static bool ValidateInput(string inputString, out uint value)
        {
            bool result = false;
            value = 0;

            if (UInt32.TryParse(inputString, NumberStyles.HexNumber, null, out value))
                result = true;

            return result;
        }
    }
}
