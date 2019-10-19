using Out.Utility;
using System;
using System.Collections.Generic;
using WoWBot.Core;


namespace WowAI
{
    [Serializable]
    public class CharacterSettings
    {
        public bool StopQuesting { get; set; } = true;

        public bool CheckRepairInCity { get; set; } = true;
        public bool LearnAllSpell { get; set; } = true;
        public bool CraftConjured { get; set; } = true;
        public int CraftConjuredHp { get; set; } = 80;
        public int CraftConjuredMp { get; set; } = 80;
        public bool AdvancedFight { get; set; } = false;
        public bool AutoEquip { get; set; } = true;
        public bool WaitSixMin { get; set; } = true;
        public bool FindBestPoint { get; set; } = true;
        public bool FightIfMobs { get; set; } = false;
        public bool UnmountMoveFail { get; set; } = true;
        public double QuesterTop { get; set; } = -1;
        public double QuesterLeft { get; set; } = -1;
        public bool UseRegen { get; set; } = false;
        public int MpRegen { get; set; } = 85;
        public int HpRegen { get; set; } = 85;
        public int PetRegen { get; set; } = 85;
        public bool UsePoligon { get; set; } = false;
        public PolygonZone PolygoneZone { get; set; } = null;
        public bool CheckRepairAndSell { get; set; } = true;
        public bool UseFilterMobs { get; set; } = false;
        public bool AoeFarm { get; set; } = false;
        public int AoeMobsCount { get; set; } = 0;
        public string FormForFight { get; set; } = "Не использовать";
        public string FormForMove { get; set; } = "Не использовать";
        public bool WorldQuest { get; set; } = true;
        public bool RunQuestHerbalism { get; set; } = false;
        public int InvFreeSlotCount { get; set; } = 5;
        public bool LaunchScript { get; set; } = false;
        public int BattlePetNumber { get; set; } = 0;
        public float GatherLocX { get; set; } = 0;
        public float GatherLocY { get; set; } = 0;
        public float GatherLocZ { get; set; } = 0;
        public int GatherLocMapId { get; set; } = 0;
        public int GatherLocAreaId { get; set; } = 0;
        public int GatherRadius { get; set; } = 0;
        public bool DebuffDeath { get; set; } = true;
        public bool SummonBattlePet { get; set; } = false;
        public float FarmLocX { get; set; } = 0;
        public float FarmLocY { get; set; } = 0;
        public float FarmLocZ { get; set; } = 0;
        public int FarmLocMapId { get; set; } = 0;
        public uint FarmLocAreaId { get; set; } = 0;
        public int FarmRadius { get; set; } = 0;
        public bool SummonMount { get; set; } = true;
        public float MountLocX { get; set; } = 0;
        public float MountLocY { get; set; } = 0;
        public float MountLocZ { get; set; } = 0;
        public int MountLocMapId { get; set; } = 0;
        public int MountLocAreaId { get; set; } = 0;
        public bool LogScriptAction { get; set; } = true;
        public bool LogSkill { get; set; }
        public bool ForceMoveScriptEnable { get; set; } = true;
        public int ForceMoveScriptDist { get; set; } = 15;
        // ReSharper disable once InconsistentNaming
        public bool FightIfHPLess { get; set; } = true;
        // ReSharper disable once InconsistentNaming
        public int FightIfHPLessCount { get; set; } = 50;
        public bool UseStoneForSellAndRepair { get; set; } = false;
        public bool UseWhistleForSellAndRepair { get; set; } = false;
        public int RepairCount { get; set; } = 5;
        public bool CheckRepair { get; set; } = true;
        public bool UseMountMyLoc { get; set; } = false;
        public float AukLocX { get; set; }
        public float AukLocY { get; set; }
        public float AukLocZ { get; set; }
        public int AukMapId { get; set; }
        public int AukAreaId { get; set; }
        public bool AukRun { get; set; }
        public bool UseStoneForLearnSpell { get; set; }
        public bool Skinning { get; set; } = true;
        public bool NoAttackOnMount { get; set; } = true;
        public bool KillRunaways { get; set; } = true;
        public bool LogAll { get; set; } = true;
        public bool GatherResouceScript { get; set; } = true;
        public int GatherRadiusScript { get; set; } = 30;
        public bool CheckBoxAttackForSitMount { get; set; }
        public bool Attack { get; set; } = false;
        public int AttackRadius { get; set; } = 20;
        public bool RunScriptFromBegin { get; set; } = false;
        public bool KillMobFirst { get; set; } = false;
        public bool UseFly { get; set; } = true;
        public bool UseDash { get; set; } = false;
        public bool ResPetInCombat { get; set; } = true;
        public int ResPetMeMp { get; set; } = 90;
        public int Zrange { get; set; } = 10;
        public bool ReturnToCenter { get; set; } = false;
        public bool UseMultiZone { get; set; } = false;
        public bool RandomJump { get; set; } = false;
        public bool PikPocket { get; set; } = false;
        public int PikPocketMapId { get; set; }
        public bool RandomDistForAttack { get; set; }
        public int RandomDistForAttackCount { get; set; } = 10;
        public bool CheckAukInTimeRange { get; set; } = false;
        public TimeSpan StartAukTime { get; set; } = new TimeSpan();
        public TimeSpan EndAukTime { get; set; } = new TimeSpan();
        public bool ScriptScheduleEnable { get; set; } = false;
        public bool ScriptReverse { get; set; } = false;
        public bool FightForSell { get; set; } = false;
        public bool CheckSellAndRepairScript { get; set; } = false;
        public bool SendMail { get; set; }
        public bool TwoWeapon { get; set; } = false;
        public TimeSpan SendMailStartTime { get; set; }
        public TimeSpan SendMailStopTime { get; set; }
        public string SendMailName { get; set; }
        public float SendMailLocX { get; set; } = 0;
        public float SendMailLocY { get; set; } = 0;
        public float SendMailLocZ { get; set; } = 0;
        public int SendMailLocMapId { get; set; } = 0;
        public int SendMailLocAreaId { get; set; } = 0;
        public bool AdvancedEquip { get; set; } = false;
        public bool AdvancedEquipUseShield { get; set; }
        public bool AlternateAuk { get; set; } = false;
        public bool UseStoneIfStuck { get; set; } = true;
        public bool AttackMobForDrop { get; set; } = true;
        public bool ChangeTargetInCombat { get; set; } = false;
        public bool Pvp { get; set; }
        public Vector3F StoneLoc { get; set; }

        public int StoneMapId { get; set; }
        public uint StoneAreaId { get; set; }
        public bool StoneRegister { get; set; }
        public bool RunRun { get; set; }
        public int RunRunPercent { get; set; } = 80;
        public bool UseArrow { get; set; } = false;
        public uint UseArrowId { get; set; } = 0;
        public Mode Mode { get; set; } = Mode.Questing;
        public bool PickUpLoot { get; set; } = true;
        public int IgnoreMob { get; set; } = 20000;
        public int FreeInvCountForAuk { get; set; } = 10;
        public bool CheckAuk { get; set; } = false;
        public uint FreeInvCountForAukId { get; set; } = 0;
        public int AukTime { get; set; } = 0;
        public int AukTimeClassic { get; set; } = 0;
        public int StopQuestingLevel { get; set; } = 12;

        public int EquipItemStat { get; set; } = 0;
        public string Script { get; set; } = "Не выбрано";
        public string Quest { get; set; } = "Не выбрано";
        public EItemQuality MaxItemQuality { get; set; } = EItemQuality.Legendary;

        public List<PropSettings> PropssSettings { get; set; } = new List<PropSettings>();
        public List<MobsSettings> MobsSettings { get; set; } = new List<MobsSettings>();
        public List<ItemSettings> ItemSettings { get; set; } = new List<ItemSettings>();
        public List<PetSettings> PetSettings { get; set; } = new List<PetSettings>();
        public List<NpcForAction> NpcForActionSettings { get; set; } = new List<NpcForAction>();
        public List<ItemGlobal> MyItemGlobals { get; set; } = new List<ItemGlobal>();
        public List<EventSettings> EventSettings { get; set; } = new List<EventSettings>();
        public List<IgnoreQuest> IgnoreQuests { get; set; } = new List<IgnoreQuest>();
        public List<RegenItems> RegenItemses { get; set; } = new List<RegenItems>();
        public List<MultiZone> MultiZones { get; set; } = new List<MultiZone>();
        public List<GameObjectIgnore> GameObjectIgnores { get; set; } = new List<GameObjectIgnore>();
        public List<AukSettings> AukSettingses { get; set; } = new List<AukSettings>();
        public List<ScriptSchedule> ScriptSchedules { get; set; } = new List<ScriptSchedule>();
        public List<EquipAuc> EquipAucs { get; set; } = new List<EquipAuc>();
        public List<AdvancedEquipWeapon> AdvancedEquipsWeapon { get; set; } = new List<AdvancedEquipWeapon>();
        public List<AdvancedEquipArmor> AdvancedEquipArmors { get; set; } = new List<AdvancedEquipArmor>();
        public List<AdvancedEquipStat> AdvancedEquipStats { get; set; } = new List<AdvancedEquipStat>();
        public List<LearnTalent> LearnTalents { get; set; } = new List<LearnTalent>();
        public List<LearnSkill> LearnSkill { get; set; } = new List<LearnSkill>();
        public List<SkillSettings> SkillSettings { get; set; } = new List<SkillSettings>();
    }
}