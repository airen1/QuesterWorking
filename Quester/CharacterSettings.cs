using WowAI.UI;
using Newtonsoft.Json.Linq;
using Out.Internal.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Markup;
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

        //Настройки
        public bool HideQuesterUi { get; set; } = false;
        public bool AutoEquip { get; set; } = true;

        public bool WaitSixMin = true;
        public bool FindBestPoint = true;

        public bool FightIfMobs = false;
        public bool UnmountMoveFail = true;
        public double QuesterTop = -1;
        public double QuesterLeft = -1;

        public bool CheckRepairAndSell = true;
        public bool UseFilterMobs { get; set; } = false;
        public bool AoeFarm { get; set; } = false;
        public int AoeMobsCount { get; set; } = 0;
        public string FormForFight = "Не использовать";
        public string FormForMove = "Не использовать";

        public float Price { get; set; } = (float)0.0;
        public int PriceKK { get; set; } = 100000;
        public string Valuta { get; set; } = "USD";

        public bool RunQuestHerbalism = false;
        public int InvFreeSlotCount { get; set; } = 5;


        public int BattlePetNumber { get; set; } = 0;
        //Сбор
        public float GatherLocX { get; set; } = 0;
        public float GatherLocY { get; set; } = 0;
        public float GatherLocZ { get; set; } = 0;
        public int GatherLocMapId { get; set; } = 0;
        public int GatherLocAreaId { get; set; } = 0;
        public int GatherRadius { get; set; } = 0;
        public bool DebuffDeath { get; set; } = true;

        public bool SummonBattlePet = false;

        //Фарм
        public float FarmLocX { get; set; } = 0;
        public float FarmLocY { get; set; } = 0;
        public float FarmLocZ { get; set; } = 0;
        public int FarmLocMapId { get; set; } = 0;
        public uint FarmLocAreaId { get; set; } = 0;
        public int FarmRadius { get; set; } = 0;

        //Маунт
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


        public bool GatherResouceScript { get; set; } = true;
        public int GatherRadiusScript { get; set; } = 30;

        public bool Attack { get; set; } = false;
        public int AttackRadius { get; set; } = 20;

        public bool UseDash { get; set; } = false;

        public int Zrange { get; set; } = 10;

        public bool UseMultiZone = false;


        //  public EMode Mode2 { get; set; } = EMode.Questing;
        /// <summary>
        /// 0 - Квестинг; 1 - Фарм; 2 - Сбор;
        /// </summary>
        public EMode Mode { get; set; } = EMode.Questing;
        /// <summary>
        /// 0 - По списку; 1 - Рандомно; 2 - По дистанции;
        /// </summary>
        public int QuestMode = 0;

        public bool PickUpLoot = true;
        public int IgnoreMob = 20000;

        public int FreeInvCountForAuk = 10;
        public bool CheckAuk = false;
        public uint FreeInvCountForAukId = 0;


        public int StopQuestingLevel = 12;
        public bool StopQuesting = false;



        public string Script = "Не выбрано";
        public string Quest = "Не выбрано";

        public List<PropSettings> PropssSettings { get; set; } = new List<PropSettings>();
        public List<MobsSettings> MobsSettings { get; set; } = new List<MobsSettings>();
        public List<ItemSettings> ItemSettings { get; set; } = new List<ItemSettings>();
        public List<SkillSettings> SkillSettings { get; set; } = new List<SkillSettings>();
        public List<PetSettings> PetSettings { get; set; } = new List<PetSettings>();
        public List<NpcForAction> NpcForActionSettings { get; set; } = new List<NpcForAction>();
      
        public List<MyBuff> MyBuffSettings { get; set; } = new List<MyBuff>();

        public List<ItemGlobal> MyItemGlobals { get; set; } = new List<ItemGlobal>();

        public List<EventSettings> EventSettings = new List<EventSettings>();
        
        public List<IgnoreQuest> IgnoreQuests = new List<IgnoreQuest>();
        public List<IgnoreMB> IgnoreMB = new List<IgnoreMB>();
        public List<MultiZone> MultiZones = new List<MultiZone>();
        public List<GameObjectIgnore> GameObjectIgnores = new List<GameObjectIgnore>();

        public List<AukSettings> AukSettingses = new List<AukSettings>();

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
    }




    [Serializable]

    public class AukSettings
    {
        public int Id { get; set; }
        public string Name { get; set; }
      /*  public int Count { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrixe { get; set; } = 9999999;
        public bool FixPrice { get; set; }
        public int FixPriceCount { get; set; }
        public int SellTime { get; set; } = 3;*/
    }

    public class GameObjectIgnore
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3F Loc { get; set; }
        public bool Ignore { get; set; }
    }

    public class NpcForAction
    {
        public bool Use { get; set; }
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3F Loc { get; set; }
        public uint MapId { get; set; }
        public uint AreaId { get; set; }
        public bool IsVendor { get; set; }
        public bool IsArmorer { get; set; }

        public bool IsAuctioner { get; set; }
        public bool IsBanker { get; set; }
        public bool IsBattleMaster { get; set; }
        public bool IsCharmed { get; set; }
        public bool IsCritter { get; set; }
        public bool IsGossip { get; set; }
        public bool IsInnkeeper { get; set; }
        public bool IsServiceProvider { get; set; }
        public bool IsSpiritGuide { get; set; }
        public bool IsSpiritHealer { get; set; }
        public bool IsSpiritService { get; set; }
        public bool IsTabardDesigner { get; set; }
        public bool IsTapped { get; set; }
        public bool IsTaxi { get; set; }
        public bool IsTotem { get; set; }
        public bool IsTrainer { get; set; }
        public bool IsTurning { get; set; }
    }

    public class MyBuff
    {
        public uint ItemId { get; set; }
        public uint SkillId { get; set; }
        public string ItemName { get; set; }
       // public string SkillName { get; set; }
    }

    public class MultiZone
    {
        public int Id { get; set; }
        public Vector3F Loc { get; set; }
        public int Radius { get; set; }
        public bool ChangeByTime { get; set; }
        public int Time { get; set; }
        public bool ChangeByDeathPlayer { get; set; }
        public int CountDeathByPlayer { get; set; }
    }

    public class IgnoreMB
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class IgnoreQuest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PetSettings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public class EventSettings
    {
        public CharacterSettings.EventsAction ActionEvent { get; set; }
        public CharacterSettings.EventsType TypeEvents { get; set; }
        public string SoundFile { get; set; }
        public int Pause { get; set; }
    }

    public class PropSettings
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        //  public EPropCategory Category { get; set; }
        //  public EPropInteractByType InteractByType { get; set; }
        //   public  EPropInteractType InteractType { get; set; }    
    }

    public class MobsSettings
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        //  public ENPCGradeType Grade { get; set; }
        //  public EAggressiveType Agr { get; set; }
        public int Level { get; set; }
    }

    [Serializable]

    public class ItemGlobal
    {
        public EItemClass Class { get; set; }
        public EItemQuality Quality { get; set; }
    }

    public class ItemSettings
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        // public EItemCategoryType Type { get; set; }
        public int Use { get; set; }

        public bool EnableSale { get; set; }
        public uint SellPrice { get; set; }
        public int MeLevel { get; set; } = 1;
        public EItemClass Class { get; set; }
        public EItemQuality Quality { get; set; }
        
    }
    [Serializable]
    public class SkillSettings
    {
        public bool Checked { get; set; }
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int MeMaxHp { get; set; } = 100;
        public int MeMinHp { get; set; }
        public int MeMaxMp { get; set; } = 100;
        public int MeMinMp { get; set; }
        public int TargetMinHp { get; set; }
        public int TargetMaxHp { get; set; } = 100;
        public int MaxDist { get; set; } = 20;
        public int MinDist { get; set; }
        public bool BaseDist { get; set; } = true;
        public bool MoveDist { get; set; } = true;
        public int AoeRadius { get; set; } = 5;
        public int AoeMin { get; set; }
        public int AoeMax { get; set; } = 100;
        public bool AoeMe { get; set; } = true;
        public bool SelfTarget { get; set; }
        public int NotTargetEffect { get; set; }
        public int NotMeEffect { get; set; }
        public int IsTargetEffect { get; set; }
        public int IsMeEffect { get; set; }
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 100;
        public int CombatElementCountMore { get; set; }
        public int CombatElementCountLess { get; set; }
        public bool UseInFight { get; set; } = true;
        public bool UseInPVP { get; set; } = false;
        public int TargetId { get; set; } = 0;
    }
}


/*[13.08.2016 10:45:21] OutSide: [Serializable]
    internal class AppConfig
    {
        [NonSerialized]
        public static string path = "settings.json";
        private bool startClientMinimized = false;
        
        public bool StartClientMinimized
        {
            get
            {
                return startClientMinimized;
            }
            set
            {
                startClientMinimized = value;
                ConfigLoader.SaveConfig(path, this);
            }
        }
    }
//[13.08.2016 10:45:34] OutSide: вот тебе простейший пример конфига (потом можно добавлять другие нужные переменные
//[13.08.2016 10:45:43] OutSide: internal static class ConfigLoader
    {
        public static object LoadConfig(string path, Type targetType, object def)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, JObject.FromObject(def).ToString());
            var obj = JObject.Parse(File.ReadAllText(path)).ToObject(targetType);
            (obj as Config).InitAfterLoad();
            return obj;
        }

        public static void SaveConfig(string path, object def)
        {
            File.WriteAllText(path, JObject.FromObject(def).ToString());
        }
    }

//класс который его сохраняет\загружает
[13.08.2016 10:46:01] OutSide: 
сама загрузка
var aconfig = new AppConfig();
aconfig = (AppConfig)ConfigLoader.LoadConfig(AppConfig.path, typeof(AppConfig), aconfig);
[13.08.2016 10:46:30] OutSide: читай сколько хочешь, хочешь чтото изменить - новое значение присвоил, и он его и сохранит сразу на диске
[13.08.2016 10:47:16] OutSide: в референсах - Newtonsoft.Json
*/
