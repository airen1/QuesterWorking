using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class EquipAuc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private EEquipmentSlot _slot;
        public EEquipmentSlot Slot
        {
            get => _slot;
            set
            {
                if (_slot == value) return;
                _slot = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private ulong _maxPrice;
        public ulong MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (_maxPrice == value) return;
                _maxPrice = value;
                NotifyPropertyChanged();
            }
        }

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                if (_level == value) return;
                _level = value;
                NotifyPropertyChanged();
            }
        }

        private int _stat1;
        public int Stat1
        {
            get => _stat1;
            set
            {
                if (_stat1 == value) return;
                _stat1 = value;
                NotifyPropertyChanged();
            }
        }

        private int _stat2;
        public int Stat2
        {
            get => _stat2;
            set
            {
                if (_stat2 == value) return;
                _stat2 = value;
                NotifyPropertyChanged();
            }
        }
    }
}