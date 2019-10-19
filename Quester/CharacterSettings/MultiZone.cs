using Out.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWBot.Core;

namespace WowAI
{
    public class MultiZone : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
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

        private int _radius;
        public int Radius
        {
            get => _radius;
            set
            {
                if (_radius == value) return;
                _radius = value;
                NotifyPropertyChanged();
            }
        }

        private bool _changeByTime;
        public bool ChangeByTime
        {
            get => _changeByTime;
            set
            {
                if (_changeByTime == value) return;
                _changeByTime = value;
                NotifyPropertyChanged();
            }
        }

        private int _time;
        public int Time
        {
            get => _time;
            set
            {
                if (_time == value) return;
                _time = value;
                NotifyPropertyChanged();
            }
        }

        private bool _changeByLevel;
        public bool ChangeByLevel
        {
            get => _changeByLevel;
            set
            {
                if (_changeByLevel == value) return;
                _changeByLevel = value;
                NotifyPropertyChanged();
            }
        }

        private int _minLevel;
        public int MinLevel
        {
            get => _minLevel;
            set
            {
                if (_minLevel == value) return;
                _minLevel = value;
                NotifyPropertyChanged();
            }
        }

        private int _maxLevel;
        public int MaxLevel
        {
            get => _maxLevel;
            set
            {
                if (_maxLevel == value) return;
                _maxLevel = value;
                NotifyPropertyChanged();
            }
        }

        private bool _useFilter;
        public bool UseFilter
        {
            get => _useFilter;
            set
            {
                if (_useFilter == value) return;
                _useFilter = value;
                NotifyPropertyChanged();
            }
        }

        private List<uint> _listMobs;
        public List<uint> ListMobs
        {
            get => _listMobs;
            set
            {
                if (_listMobs == value) return;
                _listMobs = value;
                NotifyPropertyChanged();
            }
        }


        private bool _changeByPlayer;
        public bool ChangeByPlayer
        {
            get => _changeByPlayer;
            set
            {
                if (_changeByPlayer == value) return;
                _changeByPlayer = value;
                NotifyPropertyChanged();
            }
        }
        private int _timePlayer;
        public int TimePlayer
        {
            get => _timePlayer;
            set
            {
                if (_timePlayer == value) return;
                _timePlayer = value;
                NotifyPropertyChanged();
            }
        }

        private bool _usePoligon;
        public bool UsePoligon
        {
            get => _usePoligon;
            set
            {
                if (_usePoligon == value) return;
                _usePoligon = value;
                NotifyPropertyChanged();
            }
        }

        private PolygonZone _polygoneZone;
        public PolygonZone PolygoneZone
        {
            get => _polygoneZone;
            set
            {
                if (_polygoneZone == value) return;
                _polygoneZone = value;
                NotifyPropertyChanged();
            }
        }

    }
}