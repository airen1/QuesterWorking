using WowAI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Out.Utility;
using WoWBot.Core;

namespace WowAI
{
    public enum MobsPriority
    {
        Normal, Hight, Ignore
    }

    [Serializable]
    public class CharacterSettings
    {
        [NonSerialized]
        public static string path = "test.json";
       // public bool HideQuesterUi { get; set; } = false;
        public bool AutoEquip { get; set; } = true;
        public bool WaitSixMin = true;
        public bool FindBestPoint = true;
        public bool FightIfMobs = false;
        public bool UnmountMoveFail = true;
        public double QuesterTop = -1;
        public double QuesterLeft = -1;
        
       
        public bool UseRegen = true;
        public int MpRegen = 85;
        public int HpRegen = 85;

        public bool CheckRepairAndSell = true;
        public bool UseFilterMobs { get; set; } = false;
        public bool AoeFarm { get; set; } = false;
        public int AoeMobsCount { get; set; } = 0;
        public string FormForFight = "Не использовать";
        public string FormForMove = "Не использовать";
        public bool WorldQuest = true;
        public float Price { get; set; } = (float)0.0;
        public int PriceKK { get; set; } = 100000;
        public string Valuta { get; set; } = "USD";
        public bool RunQuestHerbalism = false;
        public int InvFreeSlotCount { get; set; } = 5;
        public bool LaunchScript = false;
        public int BattlePetNumber { get; set; } = 0;
        public float GatherLocX { get; set; } = 0;
        public float GatherLocY { get; set; } = 0;
        public float GatherLocZ { get; set; } = 0;
        public int GatherLocMapId { get; set; } = 0;
        public int GatherLocAreaId { get; set; } = 0;
        public int GatherRadius { get; set; } = 0;
        public bool DebuffDeath { get; set; } = true;
        public bool SummonBattlePet = false;
        public float FarmLocX { get; set; } = 0;
        public float FarmLocY { get; set; } = 0;
        public float FarmLocZ { get; set; } = 0;
        public int FarmLocMapId { get; set; } = 0;
        public uint FarmLocAreaId { get; set; } = 0;
        public int FarmRadius { get; set; } = 0;
        public bool SummonMount = true;
        public float MountLocX { get; set; } = 0;
        public float MountLocY { get; set; } = 0;
        public float MountLocZ { get; set; } = 0;
        public int MountLocMapId { get; set; } = 0;
        public int MountLocAreaId { get; set; } = 0;
        public bool LogScriptAction { get; set; } = true;
        public bool LogSkill { get; set; } = true;
        public bool ForceMoveScriptEnable { get; set; } = true;
        public int ForceMoveScriptDist { get; set; } = 15;
        public bool FightIfHPLess { get; set; } = true;
        public int FightIfHPLessCount { get; set; } = 50;
        public bool UseStoneForSellAndRepair = false;
        public bool UseWhistleForSellAndRepair = false;
        public int RepairCount = 5;
        public bool CheckRepair = true;
        public bool UseMountMyLoc = false;
        public float AukLocX { get; set; }
        public float AukLocY { get; set; }
        public float AukLocZ { get; set; }
        public int AukMapId { get; set; }
        public int AukAreaId { get; set; }
        public bool AukRun { get; set; }

        public bool Skinning { get; set; } = true;
        public bool NoAttackOnMount { get; set; } = true;

        public bool LogAll { get; set; } = true;
        public bool GatherResouceScript { get; set; } = true;
        public int GatherRadiusScript { get; set; } = 30;
        public bool CheckBoxAttackForSitMount { get; set; }
        public bool Attack { get; set; } = false;
        public int AttackRadius { get; set; } = 20;

        public bool UseDash { get; set; } = false;

        public int Zrange { get; set; } = 10;

        public bool UseMultiZone = false;

        public bool CheckAukInTimeRange = false;
        public TimeSpan StartAukTime = new TimeSpan();
        public TimeSpan EndAukTime = new TimeSpan();
        public bool ScriptScheduleEnable = false;
        public bool ScriptReverse = false;


        public bool SendMail;
        public TimeSpan SendMailStartTime { get; set; }
        public TimeSpan SendMailStopTime { get; set; }
        public string SendMailName { get; set; }

        public float SendMailLocX { get; set; } = 0;
        public float SendMailLocY { get; set; } = 0;
        public float SendMailLocZ { get; set; } = 0;
        public int SendMailLocMapId { get; set; } = 0;
        public int SendMailLocAreaId { get; set; } = 0;


        public bool AlternateAuk = false;

        public enum EventsAction
        {
            NotSellected = -1,
            Log,
            ExitGame,
            PlaySound,
            Pause,
            ShowGameClient,
            ShowQuester
        }

        public enum EventsType
        {
            NotSellected = -1,
            [Description("Нет активности")]
            Inactivity,
            [Description("Смерть")]
            Death,
            DeathPlayer,
            ChatMessage,
            GMAssHer,
            AttackPlayer,
            PartyInvite,
            ClanInvite,
            GMServer,
        }
        //  public EMode Mode2 { get; set; } = EMode.Questing;
        /// <summary>
        /// 0 - Квестинг; 1 - Фарм; 2 - Сбор;
        /// </summary>
        public EMode Mode { get; set; } = EMode.Questing;
        public int QuestMode = 0;
        public bool PickUpLoot = true;
        public int IgnoreMob = 20000;
        public int FreeInvCountForAuk = 10;
        public bool CheckAuk = false;
        public uint FreeInvCountForAukId = 0;
        public int AukTime = 0;
        public int StopQuestingLevel = 12;
        public bool StopQuesting = false;
        public int EquipItemStat = 0;
        public string Script = "Не выбрано";
        public string Quest = "Не выбрано";

        public List<PropSettings> PropssSettings = new List<PropSettings>();
        public List<MobsSettings> MobsSettings = new List<MobsSettings>();
        public List<ItemSettings> ItemSettings = new List<ItemSettings>();
        public List<SkillSettings> SkillSettings = new List<SkillSettings>();
        public List<PetSettings> PetSettings = new List<PetSettings>();
        public List<NpcForAction> NpcForActionSettings = new List<NpcForAction>();
        public List<MyBuff> MyBuffSettings = new List<MyBuff>();
        public List<ItemGlobal> MyItemGlobals = new List<ItemGlobal>();
        public List<EventSettings> EventSettings = new List<EventSettings>();
        public List<IgnoreQuest> IgnoreQuests = new List<IgnoreQuest>();

        public List<MultiZone> MultiZones = new List<MultiZone>();
        public List<GameObjectIgnore> GameObjectIgnores = new List<GameObjectIgnore>();
        public List<AukSettings> AukSettingses = new List<AukSettings>();
        public List<ScriptSchedule> ScriptSchedules = new List<ScriptSchedule>();
        public List<EquipAuc> EquipAucs = new List<EquipAuc>();
    }




    [Serializable]
    public class EquipAuc : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private EEquipmentSlot _slot;
        public EEquipmentSlot Slot
        {
            get { return _slot; }

            set
            {
                if (_slot == value) return;
                _slot = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private ulong _maxPrice;
        public ulong MaxPrice
        {
            get { return _maxPrice; }
            set
            {
                if (_maxPrice == value) return;
                _maxPrice = value;
                NotifyPropertyChanged();
            }
        }

        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
                NotifyPropertyChanged();
            }
        }

        private int _stat1;
        public int Stat1
        {
            get { return _stat1; }
            set
            {
                if (_stat1 == value) return;
                _stat1 = value;
                NotifyPropertyChanged();
            }
        }

        private int _stat2;
        public int Stat2
        {
            get { return _stat2; }
            set
            {
                if (_stat2 == value) return;
                _stat2 = value;
                NotifyPropertyChanged();
            }
        }

        //  public EEquipmentSlot Slot { get; set; }
        //public string Name { get; set; }
        // public ulong MaxPrice { get; set; }
        // public int Level { get; set; }
        //  public int Stat1 { get; set; }
        // public int Stat2 { get; set; }
    }
    public class ScriptSchedule : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private TimeSpan _scriptStartTime;
        public TimeSpan ScriptStartTime
        {
            get { return _scriptStartTime; }
            set
            {
                if (_scriptStartTime == value) return;
                _scriptStartTime = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _scriptStopTime;
        public TimeSpan ScriptStopTime
        {
            get { return _scriptStopTime; }
            set
            {
                if (_scriptStopTime == value) return;
                _scriptStopTime = value;
                NotifyPropertyChanged();
            }
        }

        private string _scriptName;
        public string ScriptName
        {
            get { return _scriptName; }
            set
            {
                if (_scriptName == value) return;
                _scriptName = value;
                NotifyPropertyChanged();
            }
        }
        private bool _reverse;
        public bool Reverse
        {
            get { return _reverse; }
            set
            {
                if (_reverse == value) return;
                _reverse = value;
                NotifyPropertyChanged();
            }
        }

        // public TimeSpan ScriptStartTime { get; set; }
        //  public TimeSpan ScriptStopTime { get; set; }
        // public string ScriptName { get; set; }
        //  public bool Reverse { get; set; }
    }
    public class AukSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _id;
        public int Id
        {
            get { return _id; }
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
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
                NotifyPropertyChanged();
            }
        }

        private ulong _maxPrice;
        public ulong MaxPrice
        {
            get { return _maxPrice; }
            set
            {
                if (_maxPrice == value) return;
                _maxPrice = value;
                NotifyPropertyChanged();
            }
        }
        private ulong _disscount;
        public ulong Disscount
        {
            get { return _disscount; }
            set
            {
                if (_disscount == value) return;
                _disscount = value;
                NotifyPropertyChanged();
            }
        }

        private uint _maxCount;
        public uint MaxCount
        {
            get { return _maxCount; }
            set
            {
                if (_maxCount == value) return;
                _maxCount = value;
                NotifyPropertyChanged();
            }
        }
        // public int Id { get; set; }
        //  public string Name { get; set; }
        // public int Level { get; set; }
        // public ulong MaxPrice { get; set; }
        //  public ulong Disscount { get; set; }
        // public uint MaxCount { get; set; }
    }
    public class GameObjectIgnore : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _id;
        public uint Id
        {
            get { return _id; }
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
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private Vector3F _loc;
        public Vector3F Loc
        {
            get { return _loc; }
            set
            {
                if (_loc == value) return;
                _loc = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ignore;
        public bool Ignore
        {
            get { return _ignore; }
            set
            {
                if (_ignore == value) return;
                _ignore = value;
                NotifyPropertyChanged();
            }
        }
        //   public uint Id { get; set; }
        //  public string Name { get; set; }
        //  public Vector3F Loc { get; set; }
        // public bool Ignore { get; set; }
    }
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
            get { return _use; }
            set
            {
                if (_use == value) return;
                _use = value;
                NotifyPropertyChanged();
            }
        }
        private uint _id;
        public uint Id
        {
            get { return _id; }
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
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private Vector3F _loc;
        public Vector3F Loc
        {
            get { return _loc; }
            set
            {
                if (_loc == value) return;
                _loc = value;
                NotifyPropertyChanged();
            }
        }

        private uint _mapId;
        public uint MapId
        {
            get { return _mapId; }
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
            get { return _areaId; }
            set
            {
                if (_areaId == value) return;
                _areaId = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isVendor;
        public bool IsVendor
        {
            get { return _isVendor; }
            set
            {
                if (_isVendor == value) return;
                _isVendor = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isArmorer;
        public bool IsArmorer
        {
            get { return _isArmorer; }
            set
            {
                if (_isArmorer == value) return;
                _isArmorer = value;
                NotifyPropertyChanged();
            }
        }
        //  public bool Use { get; set; }
        // public uint Id { get; set; }
        //  public string Name { get; set; }
        //public Vector3F Loc { get; set; }
        // public uint MapId { get; set; }
        // public uint AreaId { get; set; }
        // public bool IsVendor { get; set; }
        // public bool IsArmorer { get; set; }

    }
    public class MyBuff : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _itemId;
        public uint ItemId
        {
            get { return _itemId; }
            set
            {
                if (_itemId == value) return;
                _itemId = value;
                NotifyPropertyChanged();
            }
        }

        private uint _skillId;
        public uint SkillId
        {
            get { return _skillId; }
            set
            {
                if (_skillId == value) return;
                _skillId = value;
                NotifyPropertyChanged();
            }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                if (_itemName == value) return;
                _itemName = value;
                NotifyPropertyChanged();
            }
        }
        //public uint ItemId { get; set; }
        // public uint SkillId { get; set; }
        //  public string ItemName { get; set; }
        // public string SkillName { get; set; }
    }
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
            get { return _id; }
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
            get { return _loc; }
            set
            {
                if (_loc == value) return;
                _loc = value;
                NotifyPropertyChanged();
            }
        }

        private int _radius;
        public int Radius
        {
            get { return _radius; }
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
            get { return _changeByTime; }
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
            get { return _time; }
            set
            {
                if (_time == value) return;
                _time = value;
                NotifyPropertyChanged();
            }
        }

        private bool _changeByDeathPlayer;
        public bool ChangeByDeathPlayer
        {
            get { return _changeByDeathPlayer; }
            set
            {
                if (_changeByDeathPlayer == value) return;
                _changeByDeathPlayer = value;
                NotifyPropertyChanged();
            }
        }

        private int _countDeathByPlayer;
        public int CountDeathByPlayer
        {
            get { return _countDeathByPlayer; }
            set
            {
                if (_countDeathByPlayer == value) return;
                _countDeathByPlayer = value;
                NotifyPropertyChanged();
            }
        }
        // public int Id { get; set; }
        //public Vector3F Loc { get; set; }
        //public int Radius { get; set; }
        //public bool ChangeByTime { get; set; }
        // public int Time { get; set; }
        // public bool ChangeByDeathPlayer { get; set; }
        // public int CountDeathByPlayer { get; set; }
    }

    public class IgnoreQuest : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _id;
        public uint Id
        {
            get { return _id; }
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
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }
        // public uint Id { get; set; }
        //  public string Name { get; set; }
    }
    public class PetSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _id;
        public int Id
        {
            get { return _id; }
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
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;
                _type = value;
                NotifyPropertyChanged();
            }
        }
        // public int Id { get; set; }
        //public string Name { get; set; }
        //public string Type { get; set; }
    }
    public class EventSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private CharacterSettings.EventsAction _actionEvent;
        public CharacterSettings.EventsAction ActionEvent
        {
            get { return _actionEvent; }
            set
            {
                if (_actionEvent == value) return;
                _actionEvent = value;
                NotifyPropertyChanged();
            }
        }

        private CharacterSettings.EventsType _typeEvents;
        public CharacterSettings.EventsType TypeEvents
        {
            get { return _typeEvents; }
            set
            {
                if (_typeEvents == value) return;
                _typeEvents = value;
                NotifyPropertyChanged();
            }
        }

        private string _soundFile;
        public string SoundFile
        {
            get { return _soundFile; }
            set
            {
                if (_soundFile == value) return;
                _soundFile = value;
                NotifyPropertyChanged();
            }
        }

        private int _pause;
        public int Pause
        {
            get { return _pause; }
            set
            {
                if (_pause == value) return;
                _pause = value;
                NotifyPropertyChanged();
            }
        }
        // public CharacterSettings.EventsAction ActionEvent { get; set; }
        // public CharacterSettings.EventsType TypeEvents { get; set; }
        //  public string SoundFile { get; set; }
        //public int Pause { get; set; }
    }
    public class PropSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _id;
        public uint Id
        {
            get { return _id; }
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
            get { return _name; }
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
            get { return _priority; }
            set
            {
                if (_priority == value) return;
                _priority = value;
                NotifyPropertyChanged();
            }
        }
        // public uint Id { get; set; }
        // public string Name { get; set; }
        // public int Priority { get; set; }
        //  public EPropCategory Category { get; set; }
        //  public EPropInteractByType InteractByType { get; set; }
        //   public  EPropInteractType InteractType { get; set; }    
    }
    public class MobsSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _id;
        public uint Id
        {
            get { return _id; }
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
            get { return _name; }
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
            get { return _priority; }
            set
            {
                if (_priority == value) return;
                _priority = value;
                NotifyPropertyChanged();
            }
        }

        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
                NotifyPropertyChanged();
            }
        }
        // public uint Id { get; set; }
        // public string Name { get; set; }
        // public int Priority { get; set; }
        //  public ENPCGradeType Grade { get; set; }
        //  public EAggressiveType Agr { get; set; }
        // public int Level { get; set; }
    }
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
            get { return _class; }
            set
            {
                if (_class == value) return;
                _class = value;
                NotifyPropertyChanged();
            }
        }

        private EItemQuality _quality;
        public EItemQuality Quality
        {
            get { return _quality; }
            set
            {
                if (_quality == value) return;
                _quality = value;
                NotifyPropertyChanged();
            }
        }

        private uint _itemLevel;
        public uint ItemLevel
        {
            get { return _itemLevel; }
            set
            {
                if (_itemLevel == value) return;
                _itemLevel = value;
                NotifyPropertyChanged();
            }
        }
    }


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
            get { return _id; }
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
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private int _use;
        public int Use
        {
            get { return _use; }
            set
            {
                if (_use == value) return;
                _use = value;
                NotifyPropertyChanged();
            }
        }

        private int _meLevel;
        public int MeLevel
        {
            get { return _meLevel; }
            set
            {
                if (_meLevel == value) return;
                _meLevel = value;
                NotifyPropertyChanged();
            }
        }

        private EItemClass _class;
        public EItemClass Class
        {
            get { return _class; }
            set
            {
                if (_class == value) return;
                _class = value;
                NotifyPropertyChanged();
            }
        }
        private EItemQuality _quality;
        public EItemQuality Quality
        {
            get { return _quality; }
            set
            {
                if (_quality == value) return;
                _quality = value;
                NotifyPropertyChanged();
            }
        }


        // public uint Id { get; set; }
        // public string Name { get; set; }
        // public EItemCategoryType Type { get; set; }
        // public int Use { get; set; }

        //public int MeLevel { get; set; } = 1;
        //public EItemClass Class { get; set; }
        // public EItemQuality Quality { get; set; }

    }
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
            get { return _checked; }

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
            get { return _id; }
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
            get { return _name; }
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
            get { return _priority; }
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
            get { return _meMaxHp; }
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
            get { return _meMinHp; }
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
            get { return _meMaxMp; }
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
            get { return _meMinMp; }
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
            get { return _targetMinHp; }
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
            get { return _targetMaxHp; }
            set
            {
                if (_targetMaxHp == value) return;
                _targetMaxHp = value;
                NotifyPropertyChanged();
            }
        }

        private int _maxDist;
        public int MaxDist
        {
            get { return _maxDist; }
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
            get { return _minDist; }
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
            get { return _baseDist; }
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
            get { return _moveDist; }
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
            get { return _aoeRadius; }
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
            get { return _aoeMin; }
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
            get { return _aoeMax; }
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
            get { return _aoeMe; }
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
            get { return _selfTarget; }
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
            get { return _notTargetEffect; }
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
            get { return _notMeEffect; }
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
            get { return _isTargetEffect; }
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
            get { return _isMeEffect; }
            set
            {
                if (_isMeEffect == value) return;
                _isMeEffect = value;
                NotifyPropertyChanged();
            }
        }

        private int _minLevel;
        public int MinLevel
        {
            get { return _minLevel; }
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
            get { return _maxLevel; } 
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
            get { return _combatElementCountMore; }
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
            get { return _combatElementCountLess; }
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
            get { return _useInFight; }
            set
            {
                if (_useInFight == value) return;
                _useInFight = value;
                NotifyPropertyChanged();
            }
        }

        private bool _useInPvp;
        public bool UseInPVP
        {
            get { return _useInPvp; }
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
            get { return _targetId; }
            set
            {
                if (_targetId == value) return;
                _targetId = value;
                NotifyPropertyChanged();
            }
        }
        //  public bool Checked { get; set; }
        // public uint Id { get; set; }
        //public string Name { get; set; }
        // public int Priority { get; set; }
        //public int MeMaxHp { get; set; } = 100;
        // public int MeMinHp { get; set; }
        //public int MeMaxMp { get; set; } = 100;
        // public int MeMinMp { get; set; }
       // public int TargetMinHp { get; set; }
       // public int TargetMaxHp { get; set; } = 100;
        //public int MaxDist { get; set; } = 20;
        //public int MinDist { get; set; }
      //  public bool BaseDist { get; set; } = true;
       // public bool MoveDist { get; set; } = true;
       // public int AoeRadius { get; set; } = 5;
        //public int AoeMin { get; set; }
       // public int AoeMax { get; set; } = 100;
       // public bool AoeMe { get; set; } = true;
       // public bool SelfTarget { get; set; }
        //public int NotTargetEffect { get; set; }
        //public int NotMeEffect { get; set; }
        //public int IsTargetEffect { get; set; }
        //public int IsMeEffect { get; set; }
        //public int MinLevel { get; set; } = 1;
       // public int MaxLevel { get; set; } = 100;
        //public int CombatElementCountMore { get; set; }
        //public int CombatElementCountLess { get; set; }
        //public bool UseInFight { get; set; } = true;
       // public bool UseInPVP { get; set; } = false;
       // public int TargetId { get; set; } = 0;
    }
}