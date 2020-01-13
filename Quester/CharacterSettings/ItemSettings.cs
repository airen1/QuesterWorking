using Out.Utility;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class ItemSettings : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _id;
        public uint Id
        {
            get => _id;
            set
            {
                if (_id == value)
                {
                    return;
                }

                _id = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                NotifyPropertyChanged();
            }
        }



        private EItemUse _use;
        public EItemUse Use
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

        private int _meLevel;
        public int MeLevel
        {
            get => _meLevel;
            set
            {
                if (_meLevel == value)
                {
                    return;
                }

                _meLevel = value;
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

        public Vector3F Loc { get; set; }
        public int MapId { get; set; }
        public uint AreaId { get; set; }
        public uint NpcId { get; set; }
        public uint MaxCount { get; set; }
        public uint MinCount { get; set; }
        public uint BuyPricePerOne { get; set; }
    }
}