// <copyright file="ICableDiagnostic.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.Generic;

namespace ADIN.Device.Services
{
    public interface ICableDiagnostic
    {
        /// <summary>
        /// gets the coeffiecent values
        /// </summary>
        /// <returns></returns>
        List<string> GetCoeff();

        /// <summary>
        /// gets the fault distance
        /// </summary>
        /// <returns>returns the fault distance</returns>
        decimal GetFaultDistance();

        /// <summary>
        /// gets the MSE Value
        /// </summary>
        /// <returns></returns>
        string GetMseValue();

        /// <summary>
        /// gets the nvp value
        /// </summary>
        /// <returns></returns>
        string GetNvp();

        /// <summary>
        /// gets the offset value
        /// </summary>
        /// <returns></returns>
        string GetOffset();

        /// <summary>
        /// sets the coeffiecent calues
        /// </summary>
        /// <param name="nvp">nvp value</param>
        /// <param name="coeff0">coeff0 value</param>
        /// <param name="coeffi">coeffi value</param>
        /// <returns>retruns the list of written values</returns>
        List<string> SetCoeff(decimal nvp, decimal coeff0, decimal coeffi);

        /// <summary>
        /// tdr set nvp
        /// </summary>
        /// <param name="nvpValue">nvp value</param>
        /// <returns></returns>
        List<string> SetNvp(decimal nvpValue);

        /// <summary>
        /// tdr set offset
        /// </summary>
        /// <param name="offset">offset value</param>
        /// <returns>returns the offset value</returns>
        string SetOffset(decimal offset);

        /// <summary>
        /// tdr initialization
        /// </summary>
        /// <returns></returns>
        void TDRInit();
    }
}
