// <copyright file="TestModeItem.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>



namespace ADIN1100_Eval
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class for handling test function
    /// </summary>
    public class TestModeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestModeItem"/> class.
        /// </summary>
        /// <param name="testName">Test Name</param>
        /// <param name="TestDescription">Test Description</param>
        /// <param name="requiresFrameLength">Whether a frame length is needed</param>
        public TestModeItem(string testName, string testName1, string TestDescription, bool requiresFrameLength)
        {
            this.TestName = testName;
            this.TestName1 = testName1;
            this.TestDescription = TestDescription;
            this.RequiresFrameLength = requiresFrameLength;
        }

        /// <summary>
        /// Gets or sets test Name
        /// </summary>
        public string TestName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets test Name
        /// </summary>
        public string TestName1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets test description
        /// </summary>
        public string TestDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a frame length is needed.
        /// </summary>
        public bool RequiresFrameLength
        {
            get;
            set;
        }
    }
}
