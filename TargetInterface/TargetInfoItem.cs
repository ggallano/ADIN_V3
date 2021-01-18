// <copyright file="TargetInfoItem.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class that describes a piece of textual information from target
    /// </summary>
    public class TargetInfoItem : Utilities.PropertyChangeNotifierBase
    {
        private string itemName;

        private string itemContent;

        private bool isAvailable = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetInfoItem"/> class.
        /// </summary>
        /// <param name="name">Item name</param>
        public TargetInfoItem(string name)
        {
            this.itemName = name;
        }

        /// <summary>
        /// Gets or sets a value indicating whether item is available
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return this.isAvailable;
            }

            set
            {
                if (value != this.isAvailable)
                {
                    this.isAvailable = value;
                    this.RaisePropertyChanged("IsAvailable");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating the item name
        /// </summary>
        public string ItemName
        {
            get
            {
                return this.itemName;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the item content
        /// </summary>
        public string ItemContent
        {
            get
            {
                return this.itemContent;
            }

            set
            {
                if (value != this.itemContent)
                {
                    this.itemContent = value;
                    this.RaisePropertyChanged("ItemContent");
                }
            }
        }

        /// <summary>
        /// See if objects are equal 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Return true if objets are identical</returns>
        public override bool Equals(object obj)
        {
            bool isEqual = true;
            var compareItem = obj as TargetInfoItem;

            if (compareItem == null)
            {
                // If it is null then it is not equal to this instance.
                return false;
            }

            if (this.ItemName != compareItem.ItemName)
            {
                isEqual = false;
            }

            if (this.ItemContent != compareItem.ItemContent)
            {
                isEqual = false;
            }

            if (this.IsAvailable != compareItem.IsAvailable)
            {
                isEqual = false;
            }

            return isEqual;
        }
    }
}
