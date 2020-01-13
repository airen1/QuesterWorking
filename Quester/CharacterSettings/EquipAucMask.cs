using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class EquipAucMask : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private EInventoryTypeMask _mask;
        public EInventoryTypeMask Mask
        {
            get => _mask;
            set
            {
                if (_mask == value)
                {
                    return;
                }

                _mask = value;
                NotifyPropertyChanged();
            }
        }

        private double _coefRange;
        public double CoefRange
        {
            get => _coefRange;
            set
            {

                _coefRange = value;
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