using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class SkillSettings
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _checked;
        public bool Checked
        {
            get => _checked;

            set
            {
                if (_checked == value) return;
                _checked = value;
                NotifyPropertyChanged();
            }
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

        private int _priority;
        public int Priority
        {
            get => _priority;
            set
            {
                if (_priority == value) return;
                _priority = value;
                NotifyPropertyChanged();
            }
        }

        private int _meMaxHp;
        public int MeMaxHp
        {
            get => _meMaxHp;
            set
            {
                if (_meMaxHp == value) return;
                _meMaxHp = value;
                NotifyPropertyChanged();
            }
        }
        private int _meMinHp;
        public int MeMinHp
        {
            get => _meMinHp;
            set
            {
                if (_meMinHp == value) return;
                _meMinHp = value;
                NotifyPropertyChanged();
            }
        }

        private int _meMaxMp;
        public int MeMaxMp
        {
            get => _meMaxMp;
            set
            {
                if (_meMaxMp == value) return;
                _meMaxMp = value;
                NotifyPropertyChanged();
            }
        }

        private int _meMinMp;
        public int MeMinMp
        {
            get => _meMinMp;
            set
            {
                if (_meMinMp == value) return;
                _meMinMp = value;
                NotifyPropertyChanged();
            }
        }

        private int _targetMinHp;
        public int TargetMinHp
        {
            get => _targetMinHp;
            set
            {
                if (_targetMinHp == value) return;
                _targetMinHp = value;
                NotifyPropertyChanged();
            }
        }

        private int _targetMaxHp;
        public int TargetMaxHp
        {
            get => _targetMaxHp;
            set
            {
                if (_targetMaxHp == value) return;
                _targetMaxHp = value;
                NotifyPropertyChanged();
            }
        }

        private int _petMinHp;
        public int PetMinHp
        {
            get => _petMinHp;
            set
            {
                if (_petMinHp == value) return;
                _petMinHp = value;
                NotifyPropertyChanged();
            }
        }

        private int _petMaxHp;
        public int PetMaxHp
        {
            get => _petMaxHp;
            set
            {
                if (_petMaxHp == value) return;
                _petMaxHp = value;
                NotifyPropertyChanged();
            }
        }

        private int _maxDist;
        public int MaxDist
        {
            get => _maxDist;
            set
            {
                if (_maxDist == value) return;
                _maxDist = value;
                NotifyPropertyChanged();
            }
        }

        private int _minDist;
        public int MinDist
        {
            get => _minDist;
            set
            {
                if (_minDist == value) return;
                _minDist = value;
                NotifyPropertyChanged();
            }
        }

        private bool _baseDist;
        public bool BaseDist
        {
            get => _baseDist;
            set
            {
                if (_baseDist == value) return;
                _baseDist = value;
                NotifyPropertyChanged();
            }
        }

        private bool _moveDist;
        public bool MoveDist
        {
            get => _moveDist;
            set
            {
                if (_moveDist == value) return;
                _moveDist = value;
                NotifyPropertyChanged();
            }
        }

        private int _aoeRadius;
        public int AoeRadius
        {
            get => _aoeRadius;
            set
            {
                if (_aoeRadius == value) return;
                _aoeRadius = value;
                NotifyPropertyChanged();
            }
        }

        private int _aoeMin;
        public int AoeMin
        {
            get => _aoeMin;
            set
            {
                if (_aoeMin == value) return;
                _aoeMin = value;
                NotifyPropertyChanged();
            }
        }
        private int _aoeMax;
        public int AoeMax
        {
            get => _aoeMax;
            set
            {
                if (_aoeMax == value) return;
                _aoeMax = value;
                NotifyPropertyChanged();
            }
        }


        private bool _aoeMe;
        public bool AoeMe
        {
            get => _aoeMe;
            set
            {
                if (_aoeMe == value) return;
                _aoeMe = value;
                NotifyPropertyChanged();
            }
        }

        private bool _selfTarget;
        public bool SelfTarget
        {
            get => _selfTarget;
            set
            {
                if (_selfTarget == value) return;
                _selfTarget = value;
                NotifyPropertyChanged();
            }
        }

        private int _notTargetEffect;
        public int NotTargetEffect
        {
            get => _notTargetEffect;
            set
            {
                if (_notTargetEffect == value) return;
                _notTargetEffect = value;
                NotifyPropertyChanged();
            }
        }

        private int _notMeEffect;
        public int NotMeEffect
        {
            get => _notMeEffect;
            set
            {
                if (_notMeEffect == value) return;
                _notMeEffect = value;
                NotifyPropertyChanged();
            }
        }

        private int _isTargetEffect;
        public int IsTargetEffect
        {
            get => _isTargetEffect;
            set
            {
                if (_isTargetEffect == value) return;
                _isTargetEffect = value;
                NotifyPropertyChanged();
            }
        }

        private int _isMeEffect;
        public int IsMeEffect
        {
            get => _isMeEffect;
            set
            {
                if (_isMeEffect == value) return;
                _isMeEffect = value;
                NotifyPropertyChanged();
            }
        }

        private int _minLevel;
        public int MinMeLevel
        {
            get => _minLevel;
            set
            {
                if (_minLevel == value) return;
                _minLevel = value;
                NotifyPropertyChanged();
            }
        }

        private int _maxLevel = 999;
        public int MaxMeLevel
        {
            get => _maxLevel;
            set
            {
                if (_maxLevel == value) return;
                _maxLevel = value;
                NotifyPropertyChanged();
            }
        }

        private int _combatElementCountMore;
        public int CombatElementCountMore
        {
            get => _combatElementCountMore;
            set
            {
                if (_combatElementCountMore == value) return;
                _combatElementCountMore = value;
                NotifyPropertyChanged();
            }
        }

        private int _combatElementCountLess;
        public int CombatElementCountLess
        {
            get => _combatElementCountLess;
            set
            {
                if (_combatElementCountLess == value) return;
                _combatElementCountLess = value;
                NotifyPropertyChanged();
            }
        }

        private bool _useInFight;
        public bool UseInFight
        {
            get => _useInFight;
            set
            {
                if (_useInFight == value) return;
                _useInFight = value;
                NotifyPropertyChanged();
            }
        }

        private bool _useInPvp;
        // ReSharper disable once InconsistentNaming
        public bool UseInPVP
        {
            get => _useInPvp;
            set
            {
                if (_useInPvp == value) return;
                _useInPvp = value;
                NotifyPropertyChanged();
            }
        }

        private int _targetId;
        public int TargetId
        {
            get => _targetId;
            set
            {
                if (_targetId == value) return;
                _targetId = value;
                NotifyPropertyChanged();
            }
        }

        private int _nottargetId;
        public int NotTargetId
        {
            get => _nottargetId;
            set
            {
                if (_nottargetId == value) return;
                _nottargetId = value;
                NotifyPropertyChanged();
            }
        }
    }
}