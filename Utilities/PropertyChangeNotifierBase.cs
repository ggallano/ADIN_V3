//-----------------------------------------------------------------------
// <copyright file="PropertyChangeNotifierBase.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities
{
    using System.ComponentModel;

    /// <summary>
    /// The class holding the Property changed event handler
    /// </summary>
    [System.Runtime.Serialization.DataContract]
    public class PropertyChangeNotifierBase : INotifyPropertyChanged
    {
        /// <summary>
        /// The Event Capturing the property change event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The Function called when any properties are changed
        /// </summary>
        /// <param name="property">The Name of property which is being changed</param>
        protected void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
