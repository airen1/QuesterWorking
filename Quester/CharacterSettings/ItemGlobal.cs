using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class ItemGlobal : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private EItemQuality _quality;
        public EItemQuality Quality
        {
            get => _quality;
            set
            {
                if (_quality == value)
                {
                    return;
                }

                _quality = value;
                NotifyPropertyChanged();
            }
        }

        private uint _itemLevel;
        public uint ItemLevel
        {
            get => _itemLevel;
            set
            {
                if (_itemLevel == value)
                {
                    return;
                }

                _itemLevel = value;
                NotifyPropertyChanged();
            }
        }
    }
}