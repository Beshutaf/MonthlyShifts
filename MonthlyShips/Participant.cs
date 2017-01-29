using MonthlyShips;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MonthlyShifts
{
    public class Participant : INotifyPropertyChanged
    {
        private string _name;
        private string _preferences;
        public ObservableCollection<ObjectHolder> OptionsY { get; set; }
        public ObservableCollection<ObjectHolder> OptionsM { get; set; }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public string Preferences
        {
            get
            {
                return _preferences;
            }
            set
            {
                _preferences = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Preferences"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}