using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class AdvancedEquipArmor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _use;
        public bool Use
        {
            get => _use;
            set
            {
                if (_use == value)
                {
                    return;
                }

                _use = value;
                NotifyPropertyChanged();
            }
        }

        private EItemSubclassArmor _armorType;
        public EItemSubclassArmor ArmorType
        {
            get => _armorType;
            set
            {
                if (_armorType == value)
                {
                    return;
                }

                _armorType = value;
                NotifyPropertyChanged();
            }
        }

        private bool _buyAuction;
        public bool BuyAuc
        {
            get => _buyAuction;
            set
            {
                if (_buyAuction == value)
                {
                    return;
                }

                _buyAuction = value;
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