// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Register.Models;
using System.Collections.ObjectModel;

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
