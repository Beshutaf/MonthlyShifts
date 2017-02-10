using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonthlyShifts
{
    public class ObjectHolder : INotifyPropertyChanged
    {
        private string _text;
        private bool _isChecked;
        private int _timesSelected;

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
        public int TimesSelected
        {
            get
            {
                return _timesSelected;
            }
            set
            {
                _timesSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimesSelected"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
