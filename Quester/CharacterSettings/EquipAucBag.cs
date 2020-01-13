using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class EquipAucBag : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private byte _cell;
        public byte Cell
        {
            get => _cell;
            set
            {
                if (_cell == value)
                {
                    return;
                }

                _cell = value;
                NotifyPropertyChanged();
            }
        }

        private EItemClass _class;
        public EItemClass Class
        {
            get => _class;
            set
            {
                if (_class == value)
                {
                    return;
                }

                _class = value;
                NotifyPropertyChanged();
            }
        }

        private uint _bagSize;
        public uint MaxBagSize
        {
            get => _bagSize;
            set
            {

                _bagSize = value;
                NotifyPropertyChanged();
            }
        }
        private ulong _maxPrice;
        public ulong MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (_maxPrice == value)
                {
                    return;
                }

                _maxPrice = value;
                NotifyPropertyChanged();
            }
        }

       
    }
}