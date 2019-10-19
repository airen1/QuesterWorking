using Out.Utility;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class LearnSkill : INotifyPropertyChanged
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
                if (_id == value) return;
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
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }



        private uint _level;
        public uint Level
        {
            get => _level;
            set
            {
                if (_level == value) return;
                _level = value;
                NotifyPropertyChanged();
            }
        }

        private uint _price;
        public uint Price
        {
            get => _price;
            set
            {
                if (_price == value) return;
                _price = value;
                NotifyPropertyChanged();
            }
        }

        private uint _npcId;
        public uint NpcId
        {
            get => _npcId;
            set
            {
                if (_npcId == value) return;
                _npcId = value;
                NotifyPropertyChanged();
            }
        }

        private Vector3F _loc;
        public Vector3F Loc
        {
            get => _loc;
            set
            {
                if (_loc == value) return;
                _loc = value;
                NotifyPropertyChanged();
            }
        }

        private int _mapId;
        public int MapId
        {
            get => _mapId;
            set
            {
                if (_mapId == value) return;
                _mapId = value;
                NotifyPropertyChanged();
            }
        }

        private uint _areaId;
        public uint AreaId
        {
            get => _areaId;
            set
            {
                if (_areaId == value) return;
                _areaId = value;
                NotifyPropertyChanged();
            }
        }
    }
}