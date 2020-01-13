using Out.Utility;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class NpcForAction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
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

        private Vector3F _loc;
        public Vector3F Loc
        {
            get => _loc;
            set
            {
                if (_loc == value)
                {
                    return;
                }

                _loc = value;
                NotifyPropertyChanged();
            }
        }

        private uint _mapId;
        public uint MapId
        {
            get => _mapId;
            set
            {
                if (_mapId == value)
                {
                    return;
                }

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
                if (_areaId == value)
                {
                    return;
                }

                _areaId = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isVendor;
        public bool IsVendor
        {
            get => _isVendor;
            set
            {
                if (_isVendor == value)
                {
                    return;
                }

                _isVendor = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isArmorer;
        public bool IsArmorer
        {
            get => _isArmorer;
            set
            {
                if (_isArmorer == value)
                {
                    return;
                }

                _isArmorer = value;
                NotifyPropertyChanged();
            }
        }
    }
}