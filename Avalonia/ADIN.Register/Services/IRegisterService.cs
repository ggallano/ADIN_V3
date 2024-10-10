using ADIN.Register.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Register.Services
{
    public interface IRegisterService
    {
        /// <summary>
        /// gets the register set in the json file
        /// </summary>
        /// <param name="registerFileName">json file name</param>
        /// <returns>retruns the list of registers</returns>
        ObservableCollection<RegisterModel> GetRegisterSet(string registerFileName);

        /// <summary>
        /// gets the hide register set
        /// </summary>
        /// <returns>returns the list of hide registers</returns>
        ObservableCollection<RegisterModel> GetAdditionalRegisterSetRev1(ObservableCollection<RegisterModel> registers);

        ObservableCollection<RegisterModel> GetAdditionalRegisterSetRev0(ObservableCollection<RegisterModel> registers);

        ObservableCollection<RegisterModel> GetAdditionalRegisterSet_ADIN1200_ADIN1300(ObservableCollection<RegisterModel> registers);

        /// <summary>
        /// gets the dictionary register
        /// </summary>
        /// <param name="registerFileName">json file name</param>
        /// <returns>returns the dictionary of registers</returns>
        Dictionary<string, RegisterModel> GetDictRegister(string registerFileName);
    }
}
