using System;
using System.Collections.Generic;

namespace ADI.Register.Models
{
    public enum GroupType
    {
        Global
    }

    public enum RegisterAccess
    {
        RW,
        R,
        W
    }

    public class RegisterModel
    {
        private string _value;

        public string Access { get; set; }
        public uint Address { get; set; }
        public List<BitFieldModel> BitFields { get; set; }
        public string Documentation { get; set; }
        public GroupType Group { get; set; }
        public string Image { get; set; }
        public bool IncludeInDump { get; set; }
        public string MMap { get; set; }
        public string Name { get; set; }
        public uint ResetValue { get; set; }

        public string Value
        {
            get
            {
                _value = GetBitFieldsValue();
                return _value;
            }

            set
            {
                _value = value;
                SetBitFieldsValue(Convert.ToUInt32(_value, 16));
            }
        }

        public string Visibility { get; set; }

        private string GetBitFieldsValue()
        {
            uint value = 0;

            foreach (var bitfield in BitFields)
            {
                value |= bitfield.Value << (int)bitfield.Start;
            }
            return value.ToString("X");
        }

        private void SetBitFieldsValue(uint value)
        {
            uint maskWidth = 0;
            uint maskValue = 0;

            foreach (var bitfield in BitFields)
            {
                maskWidth = (1U << (int)bitfield.Width) - 1;
                maskValue = maskWidth << (int)bitfield.Start;
                bitfield.Value = (value & maskValue) >> (int)bitfield.Start;
            }
        }
    }
}