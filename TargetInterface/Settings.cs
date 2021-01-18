// <copyright file="Settings.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Target Settings
    /// </summary>
    public abstract class Settings : Utilities.PropertyChangeNotifierBase
    {
        /// <summary>
        /// Property change list
        /// </summary>
        protected List<string> propertiesChangedList = new List<string>();

        /// <summary>
        /// Gets a value property change list
        /// </summary>
        public List<string> PropertiesChangedList
        {
            get
            {
                return this.propertiesChangedList;
            }
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public virtual void ClearPropertiesChangedList()
        {
            this.propertiesChangedList.Clear();
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public abstract void FlagAllPropertiesChanged();

        /// <summary>
        /// A property has changed but we keep a list of these for later processing
        /// </summary>
        /// <param name="propertyName">Proprty that has changed</param>
        protected void NotifyPropertyChange(string propertyName)
        {
            if (!this.propertiesChangedList.Contains(propertyName))
            {
                this.propertiesChangedList.Add(propertyName);
            }

            this.RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Handle a property change event
        /// </summary>
        /// <param name="propertyName">Name of the property that is potentially modified</param>
        /// <param name="fieldreference">Reference to </param>
        /// <param name="newvalue">Value this is being assigned</param>
        protected void HandledChangedProperty(string propertyName, ref bool fieldreference, bool newvalue)
        {
            if (newvalue != fieldreference)
            {
                fieldreference = newvalue;
                this.NotifyPropertyChange(propertyName);
            }
        }

        /// <summary>
        /// Checks to see if a list is identical
        /// </summary>
        /// <returns>Value of field</returns>
        /// <param name="a">First list</param>
        /// <param name="b">Second list</param>
        /// <typeparam name="T">Type</typeparam>
        protected bool IdenticalList<T>(IList<T> a, IList<T> b)
        {
            if (a == null || b == null)
            {
                return a == null && b == null;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            return a.SequenceEqual(b);
        }
    }
}
