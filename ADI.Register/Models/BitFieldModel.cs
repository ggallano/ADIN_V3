// <copyright file="BitFieldModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.ComponentModel;

namespace ADI.Register.Models
{
    public class BitFieldModel : INotifyPropertyChanged
    {
        private uint _value;

        public string Access { get; set; }
        public string Documentation { get; set; }
        public bool IncludeInDump { get; set; }
        public bool IsPublicShown => Visibility == "Public";
        public string MMap { get; set; }
        public string Name { get; set; }
        public string Position => $"[{Start}:{Width}]";
        public uint ResetValue { get; set; }
        public uint Start { get; set; }
        public uint Value 
        { 
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnBitValueChanged(nameof(Value));
            } 
        }
        public string Visibility { get; set; }
        public uint Width { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void Dispose()
        {
        }

        protected virtual void OnBitValueChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}