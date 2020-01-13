using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class AdvancedEquipStat : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private EItemModType _eStatType;
        public EItemModType StatType
        {
            get => _eStatType;
            set
            {
                if (_eStatType == value)
                {
                    return;
                }

                _eStatType = value;
                NotifyPropertyChanged();
            }
        }

        private double _coef;
        public double Coef
        {
            get => _coef;
            set
            {
                _coef = value;
                NotifyPropertyChanged();
            }
        }
    }
}