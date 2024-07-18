using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.MSE
{
    public class MseSnr
    {
        public MseSnr() { }

        public MseSnr(string blankString)
        {
            MseA = blankString;
            MseB = blankString;
            MseC = blankString;
            MseD = blankString;
        }

        public string MseA { get; set; }
        public string MseB { get; set; }
        public string MseC { get; set; }
        public string MseD { get; set; }
    }
}
