using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Register.Models
{
    public class BitFieldModel : INotifyPropertyChanged
    {
        private uint _value;

        public string Access { get; set; }
        public string Documentation { get; set; }
        public bool IncludeInDump { get; set; }
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
