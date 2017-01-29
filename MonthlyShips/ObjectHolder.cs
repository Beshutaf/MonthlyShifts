using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonthlyShips
{
    public class ObjectHolder : INotifyPropertyChanged
    {
        private string _text;
        private bool _isChecked;
        private bool _isDimmed;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            }
        }
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
        public bool IsDimmed
        {
            get
            {
                return _isDimmed;
            }
            set
            {
                _isDimmed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDimmed"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
