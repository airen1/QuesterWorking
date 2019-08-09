using WowAI.UI;
using Out.Internal.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Navigation;
using WoWBot.Core;
using Out.Utility;
using WowAI.ComboRoutes;
using System.Xml.Serialization;
using System.IO;

namespace WowAI.Modules
{

    enum ExecuteType
    {
        MonsterHunt,
        ItemGatherFromMonster,
        ItemGatherFromGameObject,
        ItemUse,
        Iteraction,
        Talk,
        Sphere,
        Unknown
    }


    internal class AutoQuests : Module
    {
        public List<uint> ListQuest;
        private readonly List<uint> _ignoreQuest = new List<uint>();
        private bool logQuest = true;
        public uint BestQuestId;
        public MyQuestHelp MyQuestHelp = new MyQuestHelp();

        public override void Start(Host host)
        {
            ListQuest = new List<uint>();
            base.Start(host);
        }

        public override void Stop()
        {
            base.Stop();
        }

        public bool NeedDebugMove;
        public Vector3F NeedGebugMoveLoc = new Vector3F();
        public bool NeedActionNpcSell;
        public bool NeedActionNpcRepair;


        public bool StopQuestModule;
        public List<MultiZone> IgnoreMultiZones = new List<MultiZone>();
        public bool NeedChangeMultizone = true;
        public MultiZone bestMultizone = new MultiZone();
        public double bestDist = 9999999;

        public bool SavePointMove = false;
        public Vector3F PreViliousPoint;

        public List<uint> IsMapidDungeon = new List<uint>
        {
            600, 43, 725, 1175, 557, 2207
        };

        public int StartLevel;


        public bool IsNeedAuk()
        {
            if (!Host.CharacterSettings.CheckAuk)
                return false;
            var count = Host.MeGetItemsCount(Host.CharacterSettings.FreeInvCountForAukId);


            if (count < Host.CharacterSettings.FreeInvCountForAuk)
                return false;
            if (Host.CharacterSettings.CheckAukInTimeRange)
            {

                /* if (Host.CharacterSettings.StartAukTime.Hour > Host.CharacterSettings.EndAukTime.Hour)
                 {
                     Host.log("Добавляю день");
                     Host.CharacterSettings.EndAukTime = Host.CharacterSettings.EndAukTime.AddDays(1);
                 }*/
                Host.log(Host.CharacterSettings.StartAukTime + "   ");
                Host.log(Host.CharacterSettings.EndAukTime + "   ");
                Host.log(DateTime.Now.TimeOfDay.IsBetween(Host.CharacterSettings.StartAukTime, Host.CharacterSettings.EndAukTime) + " ");
                //   Host.log(Host.IsInRange(DateTime.Now, Host.CharacterSettings.StartAukTime, Host.CharacterSettings.EndAukTime).ToString());
                if (!DateTime.Now.TimeOfDay.IsBetween(Host.CharacterSettings.StartAukTime, Host.CharacterSettings.EndAukTime))
                    return false;
                /* if (!Host.IsInRange(DateTime.Now, Host.CharacterSettings.StartAukTime, Host.CharacterSettings.EndAukTime))
                     return false;*/
            }

            return true;
        }


        private bool CheckRewardNextQuest(uint id)
        {

            foreach (var gameDbQuestTemplate in Host.GameDB.QuestTemplates)
            {
                if (gameDbQuestTemplate.Value.RewardNextQuest == id)
                    return false;
            }

            return true;
        }

        public bool CheckQuestSide(uint side)
        {
            if (side == 0)
                return true;
            if (side == 10)
                return true;
            if (side == 67 && Host.Me.Team == ETeam.Horde)
                return true;
            if (side == 469 && Host.Me.Team == ETeam.Alliance)
                return true;
            return false;
        }

        public bool CheckQuestRace(List<uint> race)
        {
            if (race.Count == 0)
                return true;
            foreach (var u in race)
            {
                if ((uint)Host.Me.Race == u)
                    return true;
            }

            return false;
        }

        public bool CheckQuestClass(List<uint> clas)
        {
            if (clas.Count == 0)
                return true;
            foreach (var u in clas)
            {
                if ((uint)Host.Me.Class == u)
                    return true;
            }

            return false;
        }

        public bool CheckQuestPrevious(uint id)
        {
            if (id == 0)
                return true;
            if (Host.CheckQuestCompleted(id))
                return true;
            return false;
        }

        public Vector3F MyGetCoordQuestNpc(uint id)
        {
            foreach (var entity in Host.GetEntities())
            {
                if (entity.Id == id && (entity.QuestGiverStatus & EQuestGiverStatus.Available) == EQuestGiverStatus.Available)
                    return entity.Location;
            }

            double bestDist = 99999;
            Vector3F bestLoc = new Vector3F();
            foreach (var myNpcLoc in Host.MyNpcLocss.NpcLocs)
            {
                if (myNpcLoc.Id == id)
                {

                    var badloc = false;
                    foreach (var f in BadLoc)
                    {
                        if (f.Distance(myNpcLoc.Loc) < 20)
                            badloc = true;
                    }
                    if (!badloc)
                    {
                        bestDist = Host.Me.Distance(myNpcLoc.Loc);
                        bestLoc = myNpcLoc.Loc;
                    }

                    foreach (var vector3F in myNpcLoc.ListLoc)
                    {
                        badloc = false;
                        foreach (var f in BadLoc)
                        {
                            if (f.Distance(vector3F) < 20)
                                badloc = true;
                        }
                        if (badloc)
                            continue;

                        if (Host.Me.Distance(vector3F) < bestDist)
                        {
                            bestDist = Host.Me.Distance(vector3F);
                            bestLoc = vector3F;
                        }
                    }
                    return bestLoc;
                }

            }
            return Vector3F.Zero;
        }

        public List<Vector3F> BadLoc = new List<Vector3F>();


        public override void Run(CancellationToken ct)
        {
            try
            {
                StartLevel = Host.Me.Level;
                StopQuestModule = false;
                Thread.Sleep(3000);
                while (!Host.cancelRequested && !ct.IsCancellationRequested)
                {
                    base.Run(ct);
                    Thread.Sleep(100);

                    if (NeedDebugMove && Host.GetBotLogin() == "Daredevi1")
                    {
                        Host.log("Бегу " + NeedGebugMoveLoc);
                        Host.MoveTo(NeedGebugMoveLoc);
                        NeedDebugMove = false;
                    }


                    if (SavePointMove)
                    {
                        Host.MainForm.Dispatcher.Invoke(() =>
                        {
                            if (Host.Me.Distance(PreViliousPoint) > Convert.ToInt32(Host.MainForm.textBoxScriptSavePointMoveDist.Text))
                            {
                                // host.log(host.MainForm.CollectionDungeonCoord.Count + "   " + PreViliousPoint);

                                PreViliousPoint = Host.Me.Location;

                                Host.MainForm.CollectionDungeonCoord.Add(new DungeonCoordSettings
                                {
                                    Id = Host.MainForm.CollectionDungeonCoord.Count,
                                    Action = "Бежать на точку",
                                    Loc = new Vector3F(Host.Me.Location),
                                    MobId = 0,
                                    PropId = 0,
                                    Attack = Host.MainForm.CheckBoxAttackMobs.IsChecked != null && Host.MainForm.CheckBoxAttackMobs.IsChecked.Value,

                                    ItemId = 0,
                                    AreaId = Host.Area.Id,
                                    MapId = Convert.ToInt32(Host.MapID)
                                });

                            }
                        });
                    }
                    if (!Host.MainForm.On)
                        continue;
                    if (Host.Me == null)
                        continue;

                    if (Host.CharacterSettings.DebuffDeath)
                    {
                        var debuf = Host.MyGetAura(15007);
                        if (debuf != null)
                        {
                            Thread.Sleep(5000);
                            Host.log("Ожидаю " + debuf.SpellName + " " + debuf.Remaining + "  " + debuf.Duraction);
                            continue;
                        }

                    }

                    if (Host.Check()
                        && !Host.CommonModule.InFight()
                        && Host.FarmModule.readyToActions
                        && !Host.CommonModule.IsMoveSuspended()
                    )
                    {
                        if (NeedActionNpcRepair || NeedActionNpcSell)
                        {

                            if (!Host.CharacterSettings.CheckRepairAndSell)
                                if (Host.CharacterSettings.MountLocMapId == Host.MapID && Host.CharacterSettings.MountLocAreaId == Host.Area.Id)
                                {

                                }
                                else
                                {
                                    NeedActionNpcSell = false;
                                    NeedActionNpcRepair = false;
                                }


                            Host.FarmModule.farmState = FarmState.Disabled;

                            if (Host.CharacterSettings.UseStoneForSellAndRepair)
                                Host.MyUseStone();

                            if (Host.CharacterSettings.UseWhistleForSellAndRepair)
                                Host.MyUseStone2();

                            if (NeedActionNpcSell)
                            {
                                if (!Host.MySell())
                                {
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                    continue;
                                }

                                NeedActionNpcSell = false;
                            }

                            if (NeedActionNpcRepair)
                            {
                                if (!Host.MyRepair())
                                {
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                    continue;
                                }

                                NeedActionNpcRepair = false;
                            }

                            Host.FarmModule.farmState = FarmState.AttackOnlyAgro;

                        }


                        //Продажа
                        if (Host.GetAgroCreatures().Count == 0 && Host.FarmModule.readyToActions && Host.CharacterSettings.CheckRepairAndSell)
                        {
                            if (!IsMapidDungeon.Contains(Host.MapID))
                            {
                                if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount || Host.MyIsNeedRepair())
                                {
                                    if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount)
                                        Host.log("Необходима продажа " + Host.ItemManager.GetFreeInventorySlotsCount() + "/" + Host.CharacterSettings.InvFreeSlotCount, Host.LogLvl.Important);

                                    if (Host.MyIsNeedRepair())
                                        Host.log("Необходим ремонт", Host.LogLvl.Important);
                                    NeedActionNpcSell = true;
                                    NeedActionNpcRepair = true;
                                    continue;

                                }
                            }
                        }

                        if (IsNeedAuk())
                        {
                            //камень телепорта
                            Host.MyUseStone(true);
                            //Бег к ауку
                            if (Host.Me.Team == ETeam.Horde)
                                if (Host.Area.Id != 1637)
                                {
                                    Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                    Thread.Sleep(5000);
                                    continue;
                                }

                            if (Host.Me.Team == ETeam.Alliance)
                                if (Host.Area.Id != 1519)
                                {
                                    Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                    Thread.Sleep(5000);
                                    continue;
                                }

                            if (Host.GetBotLogin() == "deathstar")
                                Host.Mail();
                            Host.Auk();
                            Host.Mail();
                            Host.Auk();
                            Host.Mail();
                            continue;

                        }





                        //   if(host.CharacterSettings.Mode > )
                        //host.log("Выбран режим " + host.CharacterSettings.Mode);








                        switch (Host.CharacterSettings.Mode)
                        {
                            case EMode.Questing:// "Выполнение квестов": //Квест
                                {
                                    if (Host.CharacterSettings.Quest.Contains("War") && Host.Me.Level < 120)
                                    {
                                        Host.log("Для выполения квестов необходим 120 уровень " + Host.Me.Level);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                    if (Host.CharacterSettings.StopQuesting)
                                    {
                                        if (Host.Me.Level >= Host.CharacterSettings.StopQuestingLevel)
                                        {
                                            Host.MainForm.On = false;
                                            continue;
                                        }
                                    }

                                    if (Host.Check() && !Host.CommonModule.InFight() && Host.FarmModule.readyToActions && !Host.CommonModule.IsMoveSuspended())
                                    {
                                        if (Host.Me.Level == 10)
                                        {
                                            if (Host.Area.Id == 141 || Host.Area.Id == 1657)
                                            {
                                                if (Host.Area.Id == 1657)//добежать до дерева
                                                {
                                                    if (!Host.CommonModule.MoveTo(9944.84, 2608.60, 1316.28))
                                                        continue;
                                                    if (!Host.CommonModule.MoveTo(9948.55, 2642.74, 1316.87))
                                                        continue;
                                                    Thread.Sleep(2000);
                                                    continue;
                                                }

                                                if (Host.Area.Id == 141)
                                                {

                                                    if (Host.Me.Distance(8381.02, 987.79, 29.61) < 50)
                                                        Host.MyUseTaxi(148, new Vector3F((float)7459.90, (float)-326.56, (float)8.09));
                                                    else
                                                    {
                                                        if (!Host.CommonModule.MoveTo(new Vector3F((float)9944.84, (float)2608.60, (float)1316.28)))
                                                            continue;
                                                        continue;
                                                    }
                                                }
                                            }

                                        }

                                        if (Host.Zone.Id == 9830 && Host.MapID == 1950 && Host.Area.Id == 9830 || Host.Zone.Id == 9826 && Host.MapID == 1949 && Host.Area.Id == 9826)
                                        {
                                            Host.log("Необходимо выбраться с корабля");
                                            Host.MyUseStone();
                                            Host.log("Необходим перезапуск после пролога ");
                                            Thread.Sleep(15000);
                                            Host.TerminateGameClient();
                                            Thread.Sleep(1000);
                                            continue;
                                        }

                                        if (Host.MapID == 1)
                                        {
                                            if (Host.CharacterSettings.Quest.Contains("OrdaBFA") && Host.Me.Level != 110)
                                            {
                                                var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1432.93, -4518.37, 18.40), Host.Me.Location);
                                                Host.log(path.Count + "  Путь");
                                                foreach (var vector3F in path)
                                                {
                                                    Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                                    Host.CommonModule.ForceMoveTo2(vector3F);
                                                }

                                                foreach (var gameObject in Host.GetEntities<GameObject>())
                                                {
                                                    if (gameObject.Id != 323855)
                                                        continue;
                                                    Host.CommonModule.MoveTo(gameObject);
                                                    Host.CommonModule.MyUnmount();
                                                    Host.CanselForm();
                                                    Thread.Sleep(5000);
                                                    gameObject.Use();
                                                    Thread.Sleep(2000);
                                                    while (Host.GameState != EGameState.Ingame)
                                                    {
                                                        Thread.Sleep(1000);
                                                    }
                                                    Host.Wait(10000);
                                                }
                                                continue;
                                            }
                                        }

                                        var sw = new Stopwatch();
                                        if (Host.AdvancedLog)
                                            sw.Start();


                                        if (Host.GetQuest(48854) != null)//Сильное предложение
                                        {
                                            RunQuest(48854);
                                            continue;
                                        }
                                        if (Host.GetQuest(49082) != null)//Сильное предложение
                                        {
                                            RunQuest(49082);
                                            continue;
                                        }


                                        var listWorldQuest = new List<uint>
                                       {
                                           50178,
                                           47647,
                                           50805,
                                           49406,
                                           48852,
                                           48934,
                                           47996,
                                           51689,
                                           49918,
                                       };
                                        if (Host.CharacterSettings.WorldQuest)
                                        {
                                            var nextLoop = false;
                                            foreach (var u in listWorldQuest)
                                            {
                                                if (Host.GetQuest(u) != null)
                                                {
                                                    RunQuest(u);
                                                    nextLoop = true;
                                                }
                                            }
                                            if (nextLoop)
                                                continue;
                                        }




                                        ListQuest.Clear();
                                        uint nextQuestId = 0;
                                        foreach (var questCoordSetting in Host.QuestSettings.QuestCoordSettings)
                                        {
                                            if (!questCoordSetting.Run)
                                                continue;

                                            if (Host.CheckQuestCompleted(questCoordSetting.QuestId))
                                            {
                                                // Host.MainForm.CollectionQuestSettings[0].State = "Выполнен";
                                                continue;
                                            }

                                            if (Host.QuestStates.QuestState.Contains(questCoordSetting.QuestId))
                                                continue;

                                            if (questCoordSetting.QuestId == 48684 && !Host.CheckQuestCompleted(48555))
                                                continue;

                                            var questTemplate = Host.GameDB.QuestTemplates[questCoordSetting.QuestId];
                                            var minlevel = questTemplate.MinLevel;
                                            var quest = Host.GetQuest(questCoordSetting.QuestId);
                                            if (quest != null)
                                            {
                                                nextQuestId = questTemplate.RewardNextQuest;
                                                var objectiveindex = -1;

                                                for (var index = 0; index < (quest?.Template.QuestObjectives).Length; index++)
                                                {

                                                    var templateQuestObjective = (quest?.Template.QuestObjectives)[index];
                                                    Host.log("Type: " + templateQuestObjective.Type
                                                                      + " Amount:" + templateQuestObjective.Amount
                                                                      + " ObjectID:" + templateQuestObjective.ObjectID
                                                                      + "StorageIndex" + templateQuestObjective.StorageIndex);
                                                    if (templateQuestObjective.StorageIndex == -1)
                                                        continue;
                                                    if (quest?.Counts[templateQuestObjective.StorageIndex] >= templateQuestObjective.Amount)
                                                        continue;


                                                    objectiveindex = index;
                                                    break;
                                                }

                                                switch (quest.Id)
                                                {


                                                    case 47959:
                                                        {
                                                            if (!Host.CheckQuestCompleted(47959) || !Host.CheckQuestCompleted(50755))
                                                                ListQuest.Add(questCoordSetting.QuestId);
                                                        }
                                                        break;
                                                    case 47324:
                                                        {
                                                            ListQuest.Add(questCoordSetting.QuestId);
                                                        }
                                                        break;
                                                    case 48684:
                                                        {
                                                            ListQuest.Add(questCoordSetting.QuestId);
                                                        }
                                                        break;

                                                    default:
                                                        {
                                                            if (objectiveindex == -1)
                                                                ListQuest.Add(questCoordSetting.QuestId);
                                                            else
                                                                ListQuest.Insert(0, questCoordSetting.QuestId);
                                                        }
                                                        break;
                                                }

                                                continue;
                                            }

                                            /*    if (questTemplate.MinLevel == 1)
                                                {
                                                    minlevel = questTemplate.MinLevel;
                                                }
                                                else
                                                {
                                                    if (Host.Me.Race == ERace.Troll)
                                                    {
                                                        minlevel = questTemplate.MinLevel + 1;
                                                        if (Host.Me.Level == 5)
                                                            minlevel = questTemplate.MinLevel;
                                                        if (Host.Me.Level > 18)
                                                            minlevel = questTemplate.MinLevel;
                                                    }


                                                    if (Host.Me.Race == ERace.NightElf && Host.Me.Level > 3 && Host.Me.Level < 110)
                                                        minlevel = questTemplate.MinLevel + 1;

                                                    if (questCoordSetting.QuestId == 25232)
                                                        minlevel = 7;
                                                    if (questCoordSetting.QuestId == 25196)
                                                        minlevel = 8;

                                                    if (questCoordSetting.QuestId == 13518)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13522)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13520)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13521)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13527)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13528)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13529)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13523)
                                                        minlevel = 13;
                                                    if (questCoordSetting.QuestId == 13547)
                                                        minlevel = 13;
                                                    if (questCoordSetting.QuestId == 13558)
                                                        minlevel = 13;

                                                    if (questCoordSetting.QuestId == 13554)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13562)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13561)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13564)
                                                        minlevel = 10;
                                                    if (questCoordSetting.QuestId == 13563)
                                                        minlevel = 10;

                                                    if (questCoordSetting.QuestId == 13615)
                                                        minlevel = 18;
                                                    if (questCoordSetting.QuestId == 13613)
                                                        minlevel = 18;
                                                    if (questCoordSetting.QuestId == 13612)
                                                        minlevel = 18;
                                                    if (questCoordSetting.QuestId == 13618)
                                                        minlevel = 18;
                                                    if (questCoordSetting.QuestId == 6544)
                                                        minlevel = 18;

                                                    if (questCoordSetting.QuestId == 25945)
                                                        minlevel = 20;
                                                    if (questCoordSetting.QuestId == 25)
                                                        minlevel = 20;
                                                    if (questCoordSetting.QuestId == 13967)
                                                        minlevel = 20;

                                                }

                                                if (questCoordSetting.QuestId == 2459)
                                                    minlevel = 5;
                                                if (questCoordSetting.QuestId == 2438)
                                                    minlevel = 5;
                                                */

                                            if (Host.Me.Level < minlevel)
                                                continue;


                                            if (nextQuestId == questCoordSetting.QuestId)
                                            {
                                                nextQuestId = questTemplate.RewardNextQuest;
                                                continue;
                                            }

                                            nextQuestId = questTemplate.RewardNextQuest;
                                            ListQuest.Add(questCoordSetting.QuestId);
                                        }


                                        /*   foreach (var quest in Host.GetQuests())
                                           {
                                               if (quest.Id == 48534 && Host.GetQuest(47324) != null)//48534 Последний смешок Рыкозуба
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 51573 && Host.GetQuest(47324) != null)//51573 Я прикрою тебя!
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 50794 && Host.GetQuest(47324) != null)//50794 В поисках убежища
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48531 && Host.GetQuest(47324) != null)//48531 Загадочное мясо
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48533 && Host.GetQuest(47324) != null)// 48533 Жареный цыпленок по-волдунски
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48657 && Host.GetQuest(47324) != null)//48657 Вкуснятина!
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48532 && Host.GetQuest(47324) != null)//48532 Дикие альпаки
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48656 && Host.GetQuest(47324) != null)//48656 Злобные завролиски
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48846 && Host.GetQuest(47324) != null)//48846 Жидкое поощрение
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 50536 && Host.GetQuest(47324) != null)//50536 Волшебный дешифратор
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48530 && Host.GetQuest(47324) != null)//50536 Волшебный дешифратор
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 48529 && Host.GetQuest(47324) != null)//50536 Волшебный дешифратор
                                                   ListQuest.Insert(0, quest.Id);

                                               if (quest.Id == 47959 && Host.GetQuest(50739) != null)//50536 Волшебный дешифратор
                                                   ListQuest.Insert(0, 50739);
                                               if (ListQuest.Contains(47324) && quest.Id != 47324 && Host.QuestSettings.QuestCoordSettings.Any(i => i.QuestId == quest.Id))
                                               {
                                                   if (quest.Id != 51772)
                                                       ListQuest.Insert(0, quest.Id);
                                               }


                                               if (quest.Id == 13557)//13557 State:None LogTitle:Неожиданная удача 
                                                   ListQuest.Insert(0, quest.Id);
                                               if (quest.Id == 26447)//26447 State:None LogTitle:Демонические планы 
                                                   ListQuest.Add(quest.Id);
                                               if (quest.Id == 2)//2 State: None LogTitle:Коготь гиппогрифа
                                                   ListQuest.Add(quest.Id);
                                               if (quest.Id == 1918) //1918 State:None LogTitle:Оскверненная стихия 
                                                   ListQuest.Add(quest.Id);
                                           }*/




                                        if (Host.AdvancedLog)
                                        {
                                            Host.log("Квесты добавлены за                               " + sw.ElapsedMilliseconds + " мс. Всего квестов " + ListQuest.Count, Host.LogLvl.Important);
                                            for (var index = 0; index < ListQuest.Count; index++)
                                            {
                                                var i = ListQuest[index];
                                                Host.log(Host.GameDB.QuestTemplates[i].LogTitle + "[" + i + "]");
                                                if (index > 2)
                                                    break;
                                            }
                                        }

                                        for (var i = ListQuest.Count - 1; i > -1; i--)
                                        {
                                            if (_ignoreQuest.Contains(ListQuest[i]))
                                            {
                                                ListQuest.Remove(ListQuest[i]);
                                                continue;
                                            }
                                            if (Host.CharacterSettings.IgnoreQuests.Any(item => ListQuest[i] == item.Id))
                                                ListQuest.Remove(ListQuest[i]);
                                        }


                                        if (Host.AdvancedLog)
                                        {
                                            Host.log("Квесты обновлены за                               " + sw.ElapsedMilliseconds + " мс");
                                            Host.log("Квестов можно выполнить: " + ListQuest.Count);
                                        }



                                        /*  if (ListQuest.Contains(50755) && Host.GetQuest(50755) != null)
                                              RunQuest(50755);
                                          if (ListQuest.Contains(51829) && Host.GetQuest(51829) != null)
                                              RunQuest(51829);*/



                                        foreach (var entity in Host.GetEntities<Unit>())
                                        {
                                            if ((entity).TaxiStatus == ETaxiNodeStatus.Unlearned)
                                            {
                                                if (!Host.CommonModule.MoveTo(entity, 2, 2))
                                                    continue;

                                                Thread.Sleep(500);
                                                if (!Host.OpenTaxi(entity))
                                                {
                                                    Host.log("Не смог открыть диалог с " + entity.Name + " TaxiStatus:" + entity.TaxiStatus + " IsTaxi:" + entity.IsTaxi + "  " + Host.GetLastError(), Host.LogLvl.Error);
                                                }
                                            }
                                        }


                                        //Сдать все квесты
                                        //Взять все доступные квесты
                                        var isNeedApply = false;
                                        foreach (var i in ListQuest)
                                        {
                                            var quest = Host.GetQuest(i);
                                            if (quest != null)
                                            {
                                                //Проверить завершен ли квест
                                                var objectiveindex = -1;
                                                /*    if (quest.Id == 13998 && quest.State != EQuestState.Complete)
                                                    {
                                                        objectiveindex = 30;
                                                    }

                                                    if (quest.Id == 13562 && quest.State != EQuestState.Complete)
                                                    {
                                                        objectiveindex = 30;
                                                    }*/
                                                for (var index = 0; index < (quest?.Template.QuestObjectives).Length; index++)
                                                {
                                                    var templateQuestObjective = (quest?.Template.QuestObjectives)[index];
                                                    if (templateQuestObjective.Type == EQuestRequirementType.AreaTrigger && quest.State == EQuestState.None)
                                                        objectiveindex = 0;
                                                    if (templateQuestObjective.StorageIndex == -1)
                                                        continue;

                                                    Host.log("Type: " + templateQuestObjective.Type + " Amount:" + templateQuestObjective.Amount + " ObjectID:" + templateQuestObjective.ObjectID + "  StorageIndex:" + templateQuestObjective.StorageIndex);
                                                    if (quest?.Counts[templateQuestObjective.StorageIndex] >= templateQuestObjective.Amount)
                                                        continue;

                                                    objectiveindex = index;
                                                    break;
                                                }

                                                QuestPOI questPoi = null;
                                                foreach (var questPois in quest.GetQuestPOI())
                                                {
                                                    if (questPois.ObjectiveIndex != -1)
                                                        continue;
                                                    questPoi = questPois;
                                                    break;
                                                }
                                                if (questPoi == null)
                                                {
                                                    foreach (var questPois in quest.GetQuestPOI())
                                                    {
                                                        if (questPois.ObjectiveIndex != -1)
                                                            continue;
                                                        questPoi = questPois;
                                                        break;
                                                    }
                                                }
                                                /*   if (objectiveindex == -1 && quest.Id == 28726)
                                                   {
                                                       Host.log("Сдаю квест поблизости " + objectiveindex, Host.LogLvl.Ok);
                                                       BestQuestId = quest.Id;
                                                       MyComliteQuest(quest);// завершил квест
                                                       Thread.Sleep(1000);
                                                       isNeedApply = true;
                                                       break;
                                                   }
                                                   if (objectiveindex == -1 && quest.Id == 28727)
                                                   {
                                                       Host.log("Сдаю квест поблизости " + objectiveindex, Host.LogLvl.Ok);
                                                       BestQuestId = quest.Id;
                                                       MyComliteQuest(quest);// завершил квест
                                                       Thread.Sleep(1000);
                                                       isNeedApply = true;
                                                       break;
                                                   }*/
                                                if (quest.Id == 50561)
                                                    continue;
                                                if (objectiveindex == -1 && questPoi != null && Host.DistanceNoZ(Host.Me.Location.X, Host.Me.Location.Y, questPoi.Points[0].X, questPoi.Points[0].Y) < 90)
                                                {
                                                    Host.log("Сдаю квест поблизости " + objectiveindex + "  " + quest.Template.LogTitle + "[" + quest.Id + "]", Host.LogLvl.Ok);
                                                    BestQuestId = quest.Id;
                                                    MyComliteQuest(quest);// завершил квест
                                                    Thread.Sleep(1000);
                                                    isNeedApply = true;
                                                    break;
                                                }

                                                continue;
                                            }


                                            QuestCoordSettings questSet = null;
                                            foreach (var questLoc in Host.QuestSettings.QuestCoordSettings)
                                            {
                                                if (questLoc.QuestId == i)
                                                {
                                                    questSet = questLoc;
                                                }
                                            }
                                            if (questSet != null)
                                            {
                                                /* if (Host.Me.Distance(questSet.Loc) < 80)
                                                 {*/
                                                var npc = Host.GetNpcById(questSet.NpcId);
                                                if (npc != null && npc.Type == EBotTypes.Unit)
                                                {
                                                    if (((npc as Unit).QuestGiverStatus & EQuestGiverStatus.Available) == EQuestGiverStatus.Available)
                                                    {
                                                        /*  if (questSet.QuestId == 46928)
                                                          {
                                                              Host.log("Беру квест поблизости  7 " + questSet.QuestId, Host.LogLvl.Ok);
                                                              BestQuestId = 46927;
                                                              MyApplyQuest(npc, 46927); // взял квест
                                                              Thread.Sleep(1000);
                                                              isNeedApply = true;
                                                              break;
                                                          }

                                                          if (questSet.QuestId == 48456 && !Host.CheckQuestCompleted(46929))
                                                          {
                                                              Host.log("Беру квест поблизости 6 " + questSet.QuestId, Host.LogLvl.Ok);
                                                              BestQuestId = 46929;
                                                              MyApplyQuest(npc, 46929); // взял квест
                                                              Thread.Sleep(1000);
                                                              isNeedApply = true;
                                                              break;
                                                          }

                                                          if (questSet.QuestId == 48456 && !Host.CheckQuestCompleted(50881))
                                                          {
                                                              Host.log("Беру квест поблизости 5 " + questSet.QuestId, Host.LogLvl.Ok);
                                                              BestQuestId = 50881;
                                                              MyApplyQuest(npc, 50881); // взял квест
                                                              Thread.Sleep(1000);
                                                              isNeedApply = true;
                                                              break;
                                                          }

                                                          if (questSet.QuestId == 51057 && !Host.CheckQuestCompleted(51056) && Host.GetQuest(51056) == null)
                                                          {
                                                              Host.log("Беру квест поблизости 4 " + questSet.QuestId, Host.LogLvl.Ok);
                                                              BestQuestId = 51056;
                                                              MyApplyQuest(npc, 51056); // взял квест
                                                              Thread.Sleep(1000);
                                                              isNeedApply = true;
                                                              break;
                                                          }

                                                          if (questSet.QuestId == 51057 && !Host.CheckQuestCompleted(51055) && Host.GetQuest(51055) == null)
                                                          {
                                                              Host.log("Беру квест поблизости 3 " + questSet.QuestId, Host.LogLvl.Ok);
                                                              BestQuestId = 51055;
                                                              MyApplyQuest(npc, 51055); // взял квест
                                                              Thread.Sleep(1000);
                                                              isNeedApply = true;
                                                              break;
                                                          }*/

                                                        if (questSet.QuestId == 51233 || questSet.QuestId == 51985 || questSet.QuestId == 51233 || questSet.QuestId == 51057 || questSet.QuestId == 47499)
                                                            continue;

                                                        Host.log("Беру квест поблизости у NPC " + npc.Name + "[" + npc.Id + "]  " + Host.GameDB.QuestTemplates[questSet.QuestId].LogTitle + "[" + questSet.QuestId + "]", Host.LogLvl.Ok);
                                                        BestQuestId = questSet.QuestId;
                                                        MyApplyQuest(npc, questSet.QuestId); // взял квест
                                                        Thread.Sleep(1000);
                                                        isNeedApply = true;
                                                        break;
                                                    }
                                                }
                                                if (npc != null && npc.Type == EBotTypes.GameObject)
                                                {
                                                    if ((npc as GameObject).QuestGiverStatus == EQuestGiverStatus.Available)
                                                    {
                                                        Host.log("Беру квест поблизости у GO " + npc.Name + "[" + npc.Id + "]  " + Host.GameDB.QuestTemplates[questSet.QuestId].LogTitle + "[" + questSet.QuestId + "]", Host.LogLvl.Ok);
                                                        BestQuestId = questSet.QuestId;
                                                        MyApplyQuest(npc, questSet.QuestId); // взял квест
                                                        Thread.Sleep(1000);
                                                        isNeedApply = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }


                                        if (isNeedApply)
                                            continue;

                                        //Выполнение квестов
                                        foreach (var quest in ListQuest)
                                        {
                                            if ((!Host.FarmModule.readyToActions) & (Host.MainForm.On))
                                                break;

                                            if ((Host.CommonModule.IsMoveSuspended()) & (Host.MainForm.On))
                                                break;
                                            BestQuestId = quest;
                                            /*  if (quest == 48456 && Host.GetQuest(48454) != null)
                                              {
                                                  RunQuest(48454);
                                              }

                                              if (quest == 48456 && Host.GetQuest(48404) != null)
                                              {
                                                  RunQuest(48404);
                                                  break;
                                              }*/


                                            if (!RunQuest(quest))
                                                break;
                                        }


                                        if (ListQuest.Count == 0)
                                        {
                                            Host.log("Нет квестов");

                                            if (Host.CharacterSettings.LaunchScript)
                                            {
                                                Host.CharacterSettings.Mode = EMode.Script;
                                                continue;
                                            }

                                            if (Host.Me.Level > 100)
                                            {
                                                Host.CanselForm();
                                                Host.CommonModule.MyUnmount();
                                                Host.log("Отключаюсь");
                                                Host.MainForm.On = false;
                                                if (Host.GetBotLogin() == "easymoney")
                                                    Host.StopPluginNow();
                                                continue;
                                            }

                                            if (Host.Me.Level > 12 && Host.Me.Race == ERace.Troll)
                                            {
                                                Host.log("Бегу к данжу");
                                                if (!Host.CommonModule.MoveTo(new Vector3F((float)-742.13, (float)-2217.77, (float)16.17)))
                                                    continue;
                                                Host.GetCurrentAccount().IsAutoLaunch = false;
                                                Host.TerminateGameClient();
                                            }

                                            var meLevel = Host.Me.Level;
                                            var coord = Host.Me.Location;
                                            var mobs = false;
                                            if (Host.Me.Race == ERace.Troll)
                                            {
                                                if (Host.Me.Level == 4)
                                                {
                                                    coord = Host.Me.Location;
                                                    mobs = true;
                                                }


                                                if (Host.Me.Level == 9 || Host.Me.Level == 8)
                                                {
                                                    coord = new Vector3F((float)984.65, (float)-3922.08, (float)18.48);
                                                    mobs = true;
                                                }

                                            }
                                            if (Host.Me.Race == ERace.NightElf)
                                            {
                                                if (Host.Me.Level == 3 || Host.Me.Level == 4)
                                                {
                                                    coord = new Vector3F((float)9714.98, (float)536.52, (float)1307.60);
                                                    mobs = true;
                                                }

                                                if (Host.Me.Level == 7 || Host.Me.Level == 8 || Host.Me.Level == 9)
                                                {
                                                    coord = new Vector3F((float)9571.36, (float)1097.43, (float)1264.45);
                                                    mobs = true;
                                                }


                                            }



                                            if (mobs == true)
                                            {
                                                if (!Host.CommonModule.MoveTo(coord, 20, 20))
                                                    continue;
                                                var zone = new RoundZone(coord.X, coord.Y, 200);

                                                var farmmoblist = new List<uint>();
                                                var badRadius = 0;



                                                foreach (var i in Host.GetEntities<Unit>())
                                                {

                                                    if (i.Level == 1)
                                                        continue;
                                                    if (!Host.CanAttack(i, Host.CanSpellAttack))
                                                        continue;
                                                    if (!farmmoblist.Contains(i.Id))
                                                        farmmoblist.Add(i.Id);
                                                }

                                                Host.FarmModule.SetFarmMobs(zone, farmmoblist);

                                                while (Host.MainForm.On
                                                       && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                                                       && meLevel == Host.Me.Level
                                                       && Host.FarmModule.readyToActions
                                                       && Host.FarmModule.farmState == FarmState.FarmMobs
                                                )
                                                {
                                                    if (Host.MyIsNeedRepair())
                                                        break;
                                                    if (Host.FarmModule.BestMob == null && Host.Me.HpPercents > 80)
                                                        badRadius++;
                                                    else
                                                        badRadius = 0;
                                                    if (badRadius > 100)
                                                        Host.CommonModule.MoveTo(coord, 2, 2);
                                                    Thread.Sleep(100);

                                                }

                                                Host.FarmModule.StopFarm();
                                                Thread.Sleep(1000);
                                                continue;

                                            }
                                            Thread.Sleep(1000);
                                        }


                                    }
                                }
                                break;

                            case EMode.FarmMob:// "Убийство мобов": //ФАрм
                                {
                                    if (Host.MainForm.On && Host.Check())
                                    {

                                        if (!Host.CharacterSettings.UseMultiZone)
                                        {
                                            if (Host.CharacterSettings.FarmLocX == 0)
                                            {
                                                Host.log("Не выбраны координаты зоны ", Host.LogLvl.Error);
                                                Thread.Sleep(5000);
                                                continue;
                                            }

                                            if (Host.CharacterSettings.FarmRadius == 0)
                                            {
                                                Host.log("Радиус = 0 ", Host.LogLvl.Error);
                                                Thread.Sleep(5000);
                                                continue;
                                            }

                                            if (Host.CharacterSettings.FarmLocAreaId != Host.Area.Id || Host.CharacterSettings.FarmLocMapId != Host.MapID)
                                            {
                                                if (!Host.MyUseTaxi(Host.CharacterSettings.FarmLocAreaId, new Vector3F(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ)))
                                                    continue;
                                                Host.log("Зона фарма находится в другой зоне 1", Host.LogLvl.Error);
                                                Host.MainForm.On = false;
                                                continue;
                                            }


                                            // host.log("Фарм мобов");
                                            var zone = new RoundZone(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY,
                                            Host.CharacterSettings.FarmRadius);
                                            var farmmoblist = new List<uint>();
                                            if (!zone.ObjInZone(Host.Me))
                                            {
                                                Host.log("Бегу в зону");
                                                if (Host.MainForm.On && !Host.CommonModule.MoveTo(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ, Host.CharacterSettings.FarmRadius / 2, Host.CharacterSettings.FarmRadius / 2))
                                                    break;
                                            }



                                            //   host.log("Выбираю мобов для атаки");
                                            if (!Host.CharacterSettings.UseFilterMobs)
                                            {
                                                foreach (var i in Host.GetEntities<Unit>())
                                                {

                                                    if (!Host.CanAttack(i, Host.CanSpellAttack))
                                                        continue;
                                                    if (!farmmoblist.Contains(i.Id))
                                                        farmmoblist.Add(i.Id);
                                                }
                                            }

                                            else
                                            {
                                                foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                                {
                                                    if (characterSettingsMobsSetting.Priority == 1)
                                                        continue;
                                                    farmmoblist.Add(Convert.ToUInt32(characterSettingsMobsSetting.Id));
                                                }
                                            }
                                            if (farmmoblist.Count > 0)
                                                Host.log("Выбранно: " + farmmoblist.Count + " мобов для фарма");
                                            else
                                            {
                                                Host.log("Не нашел мобов или фильтр мобов пустой", Host.LogLvl.Error);
                                                Host.MainForm.On = false;
                                            }

                                            if (Host.CharacterSettings.AoeFarm)
                                            {
                                                AoeFarm(farmmoblist, zone);
                                            }
                                            else
                                            {
                                                var badRadius = 0;




                                                Host.FarmModule.SetFarmMobs(zone, farmmoblist);

                                                while (Host.MainForm.On
                                                       && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                                                       && Host.CharacterSettings.Mode == EMode.FarmMob// "Убийство мобов"
                                                       && Host.FarmModule.readyToActions
                                                       && Host.FarmModule.farmState == FarmState.FarmMobs
                                                       )
                                                {
                                                    if (Host.MyIsNeedRepair())
                                                        break;

                                                    if (Host.FarmModule.BestMob == null && Host.Me.HpPercents > 80)
                                                        badRadius++;
                                                    else
                                                        badRadius = 0;

                                                    if (badRadius > 100)
                                                        Host.CommonModule.MoveTo(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ, 2, 2);


                                                    Thread.Sleep(100);

                                                }

                                                Host.FarmModule.StopFarm();
                                                Thread.Sleep(1000);
                                            }

                                        }
                                        else
                                        {

                                            if (NeedChangeMultizone)
                                                foreach (var characterSettingsMultiZone in Host.CharacterSettings.MultiZones)
                                                {
                                                    if (IgnoreMultiZones.Contains(characterSettingsMultiZone))
                                                        continue;
                                                    if (Host.Me.Distance(characterSettingsMultiZone.Loc) < bestDist)
                                                    {
                                                        bestMultizone = characterSettingsMultiZone;
                                                        bestDist = Host.Me.Distance(characterSettingsMultiZone.Loc);
                                                        NeedChangeMultizone = false;
                                                    }
                                                }



                                            Host.log("Фарм мобов " + bestMultizone.Id);
                                            if (!Host.MainForm.On)
                                                break;
                                            if (bestMultizone.Loc.X == 0)
                                            {
                                                Host.log("Не выбраны координаты зоны ", Host.LogLvl.Error);
                                                Host.MainForm.On = false;
                                            }

                                            var zone = new RoundZone(bestMultizone.Loc.X, bestMultizone.Loc.Y, bestMultizone.Radius);
                                            var farmmoblist = new List<uint>();
                                            //    if (!zone.ObjInZone(host.Me))
                                            //   {
                                            //  host.log("Бегу в зону");
                                            while (Host.Me.Distance(bestMultizone.Loc) > bestMultizone.Radius && Host.MainForm.On && Host.IsAlive(Host.Me))
                                            {
                                                while (Host.CommonModule.IsMoveSuspended())
                                                {
                                                    Thread.Sleep(100);
                                                }
                                                //  host.FarmModule.farmState = FarmState.Disabled;
                                                if (Host.MainForm.On)
                                                    Host.CommonModule.MoveTo(bestMultizone.Loc, bestMultizone.Radius / 2, bestMultizone.Radius / 2);
                                                Thread.Sleep(100);
                                            }



                                            //   host.log("Выбираю мобов для атаки");
                                            if (!Host.CharacterSettings.UseFilterMobs)
                                            {
                                                Host.log("нет базы");
                                            }
                                            /*  foreach (var cachedDbNpcInfo in host.GameDB.npc_origin)
                                                  farmmoblist.Add(cachedDbNpcInfo.Value.mInfoIndex)*/
                                            else
                                            {
                                                foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                                {
                                                    // farmmoblist.Add(characterSettingsMobsSetting.Id);
                                                }
                                            }
                                            if (farmmoblist.Count > 0)
                                                Host.log("Выбранно: " + farmmoblist.Count + " мобов для фарма");
                                            else
                                            {
                                                Host.log("Не нашел мобов или фильтр мобов пустой", Host.LogLvl.Error);
                                                Host.MainForm.On = false;
                                            }
                                            var farmTimeSecond = bestMultizone.Time;
                                            var farmTime = 0;
                                            var countDeathOnSpot = Host.ComboRoute.DeadCountInPVP;

                                            Host.FarmModule.SetFarmMobs(zone, farmmoblist);
                                            while (Host.MainForm.On
                                                   && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                                                   && Host.CharacterSettings.Mode == EMode.FarmMob//"Убийство мобов"
                                                   && Host.FarmModule.readyToActions
                                                   && Host.FarmModule.farmState == FarmState.FarmMobs)
                                            {
                                                Thread.Sleep(1000);


                                                farmTime++;
                                                if (bestMultizone.ChangeByTime && !Host.CommonModule.InFight() && farmTime > farmTimeSecond)
                                                {
                                                    Host.log("Меняю зону по времени " + farmTime + " / " + farmTimeSecond);
                                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                                    IgnoreMultiZones.Add(bestMultizone);
                                                    NeedChangeMultizone = true;
                                                }
                                                if (bestMultizone.ChangeByDeathPlayer && (Host.ComboRoute.DeadCountInPVP - countDeathOnSpot) > bestMultizone.CountDeathByPlayer)
                                                {
                                                    Host.log("Меняю зону по смерти от игроков " + (Host.ComboRoute.DeadCountInPVP - countDeathOnSpot) + " / " + bestMultizone.CountDeathByPlayer);
                                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                                    IgnoreMultiZones.Add(bestMultizone);
                                                    NeedChangeMultizone = true;
                                                }
                                            }

                                            Host.FarmModule.StopFarm();
                                            Thread.Sleep(1000);

                                        }

                                    }
                                }

                                break;

                            case EMode.FarmResource://"Сбор ресурсов": //Сбор
                                {
                                    if (Host.MainForm.On && Host.Check())
                                    {
                                        Host.log("Сбор");
                                        if (Host.CharacterSettings.GatherLocX == 0)
                                        {
                                            Host.log("Не выбраны координаты зоны ", Host.LogLvl.Error);
                                            Host.MainForm.On = false;
                                            continue;

                                        }

                                        if (Host.CharacterSettings.GatherLocAreaId != Host.Area.Id || Host.CharacterSettings.GatherLocMapId != Host.MapID)
                                        {
                                            Host.log("Зона фарма находится в другой зоне 2", Host.LogLvl.Error);
                                            Host.MainForm.On = false;
                                            continue;
                                        }


                                        var zone = new RoundZone(Host.CharacterSettings.GatherLocX, Host.CharacterSettings.GatherLocY, Host.CharacterSettings.GatherRadius);
                                        Host.log(Host.CharacterSettings.GatherLocX + "    " + Host.CharacterSettings.GatherLocY + "   " + Host.CharacterSettings.GatherRadius);
                                        var farmmoblist = new List<uint>();
                                        //   if (!zone.ObjInZone(host.Me))
                                        if (!Host.CommonModule.MoveTo(Host.CharacterSettings.GatherLocX, Host.CharacterSettings.GatherLocY, Host.CharacterSettings.GatherLocZ, 25, 25))
                                            break;

                                        foreach (var dbPropInfo in Host.CharacterSettings.PropssSettings)
                                            farmmoblist.Add(dbPropInfo.Id);


                                        if (farmmoblist.Count == 0)
                                        {
                                            Host.log("Не нашел пропы или фильтр пропов пустой", Host.LogLvl.Error);
                                            Host.MainForm.On = false;
                                        }
                                        Host.FarmModule.SetFarmProps(zone, farmmoblist);
                                        //int badRadius = 0;
                                        while (Host.MainForm.On
                                               && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                                               && Host.CharacterSettings.Mode == EMode.FarmResource// "Сбор ресурсов"
                                               && Host.FarmModule.readyToActions
                                               && Host.FarmModule.farmState == FarmState.FarmProps)
                                        {
                                            if (Host.MyIsNeedRepair())
                                                break;
                                            Thread.Sleep(100);
                                            /* if (host.FarmModule.bestProp == null && host.Me.HpPercents > 80)
                                                 badRadius++;
                                             else
                                                 badRadius = 0;*/
                                            /*   if (host.FarmModule.bestMob != null)
                                                   badRadius = 0;*/


                                        }

                                        Host.FarmModule.StopFarm();
                                        Thread.Sleep(1000);

                                    }
                                }
                                break;

                            case EMode.Script:// "Данж.(п)":
                                {
                                    while (Host.MainForm.On && Host.Check() && Host.CharacterSettings.Mode == EMode.Script)
                                    {
                                        // Host.log("Скрипт");
                                        Thread.Sleep(100);

                                        while (Host.GameState != EGameState.Ingame)
                                            Thread.Sleep(1000);
                                        if (Host.CharacterSettings.DebuffDeath)
                                        {
                                            var debuf = Host.MyGetAura(15007);
                                            if (debuf != null)
                                            {
                                                Thread.Sleep(5000);
                                                Host.log("Ожидаю " + debuf.SpellName + " " + debuf.Remaining);
                                                continue;
                                            }

                                        }
                                        if (IsNeedWaitAfterLoading)
                                        {
                                            Thread.Sleep(1000);
                                            IsNeedWaitAfterLoading = false;
                                        }
                                        //  if (Host. != EWorldMapType.Dungeon)
                                        // {
                                        if (!IsMapidDungeon.Contains(Host.MapID))
                                            if (Host.Me.GetThreats().Count == 0)
                                                if (NeedActionNpcSell || NeedActionNpcRepair || IsNeedAuk() || Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount || Host.MyIsNeedRepair())
                                                    break;




                                        //  if (IsMapidDungeon.Contains(Host.MapID) && Host.CharacterSettings.CheckRepairAndSell && Host.CharacterSettings.MountLocMapId != 0 && Host.CharacterSettings.MountLocMapId == Host.MapID)
                                        /* if ()
                                         {
                                             // Host.CommonModule.LoadCurrentZoneMesh(Host.Me.Location.X, Host.Me.Location.Y);
                                             break;
                                         }*/


                                        Mode_86();
                                        // }

                                    }
                                }
                                break;


                            case EMode.OnlyAttack:
                                {
                                    while (Host.GameState == EGameState.Ingame && Host.CharacterSettings.Mode == EMode.OnlyAttack)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                                break;


                                /* case EMode.PartyFarm:
                                     {
                                         while (Host.MainForm.On && Host.Check() && Host.CharacterSettings.Mode == EMode.PartyFarm)
                                         {
                                             Thread.Sleep(100);
                                             while (Host.GameState != EGameState.Ingame)
                                                 Thread.Sleep(1000);
                                             PartyFarm();
                                         }
                                     }
                                     break;*/
                        }
                    }
                }
                StopQuestModule = true;
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception err)
            {
                Host.log("Run" + err);
            }
            finally
            {
                Host.log("QuestModule Stop");
            }
        }




        public bool IsNeedWaitAfterLoading;

        public void AoeFarm(List<uint> farmmoblist, RoundZone zone)
        {
            var aoeMobsCount = Host.CharacterSettings.AoeMobsCount;
            var isFind = true;
            var aoeTimer = 0;

            while (Host.GetAgroCreatures().Count < aoeMobsCount && isFind && Host.IsAlive(Host.Me) && Host.MainForm.On && zone.ObjInZone(Host.Me))
            {
                Host.log(" Ищу мобов для АОЕ");
                Thread.Sleep(100);
                Host.FarmModule.farmState = FarmState.Disabled;
                var listMob = Host.GetEntities<Entity>();


                if (Host.GetAgroCreatures().Count == 0 && Host.Me.HpPercents > 80)
                    aoeTimer++;
                else
                    aoeTimer = 0;

                if (aoeTimer > 50)
                {
                    Host.CommonModule.MoveTo(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ, 5, 5);

                }

                foreach (var creature in listMob.OrderBy(i => Host.Me.Distance(i)))
                {
                    //  if(host.Me.Distance(i) > AoeDistMob)
                    if (!farmmoblist.Contains(creature.Guid.GetEntry()))
                        continue;
                    if (!Host.IsAlive(Host.Me))
                        break;
                    if (!Host.IsExists(creature))
                        continue;
                    /*  if (creature.Type != EBotTypes.Npc)
                          continue;*/
                    if (Host.GetVar(creature, "InFight") != null)
                    {
                        var infight = (bool)(Host.GetVar(creature, "InFight"));
                        if (infight)
                        {
                            //  host.log(interception.Name + " В бою!!", Host.LogLvl.Error);
                            continue;
                        }
                    }
                    if (!zone.ObjInZone(creature))
                        continue;

                    /*   if (!host.IsAlive(creature))
                           continue;*/

                    /*   if (host.GetAgroCreatures().Contains(creature))
                           continue;*/

                    /*   if (host.Me.Distance(creature) > AoeRadius)
                           continue;*/
                    Host.log("Нашел моба " + creature.Name + "  [" + creature.Guid + "] Дистанция: " + Host.Me.Distance(creature));
                    if (!Host.CommonModule.ForceMoveTo(creature, 1, 1))
                    {
                        Thread.Sleep(100);
                        isFind = false;
                        break;
                    }
                    break;

                }
            }


            Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
            Host.CommonModule.SuspendMove();
            while (Host.CommonModule.IsMoveSuspended() && Host.IsAlive(Host.Me))
            {
                Thread.Sleep(100);
            }

            while (Host.Me.HpPercents < 80 && !Host.CommonModule.InFight())
            {
                Thread.Sleep(100);
            }
        }




        public bool WaitTeleport;

        public void MyEnterDangeon(DungeonCoordSettings dungeon)
        {
            try
            {

            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        public void MyExitDangeon(DungeonCoordSettings dungeon)
        {
            try
            {

            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        public bool NeedFindBestPoint;


        public List<Core> PartyList = new List<Core>();


        public bool IsAllOnline()
        {
            try
            {
                foreach (var core in PartyList)
                {
                    if (core.GameState != EGameState.Ingame)
                    {
                        Host.log("Нет в игре " + core.Me?.Name + " " + core.GameState);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }


        public Stopwatch scriptStopwatch = new Stopwatch();

        public void MyGroupInvite(string inviterName, WowGuid inviterGuid)
        {
            //   Host.onGroupInvite += MyGroupInvite;
            Host.log("Пришел запрос  " + Host.Me.Name + " " + inviterName);
            /*  if (inviterName == Host.Me.Name)
              {
                  if (Host.Group.AcceptInvite(true))
                  {
                      Host.log("Принял запрос от  " + inviterName);
                  }
                  else
                  {
                      Host.log("Не смог принять запрос от " + inviterName + " " + Host.GetLastError(), Host.LogLvl.Error);
                  }
              }
              else
              {
                  Host.log("Неизвестный запрос на пати " + inviterName, Host.LogLvl.Error);
              }*/
        }

        public bool MoveGroup()
        {
            try
            {
                Host.log("Вывожу группу из данжа ");
                Thread.Sleep(1000);
                var loc = new Vector3F();
                var areaid = new List<uint>();
                if (Host.Area.Id == 718) //В данже
                {

                    //  loc = new Vector3F((float)-168.27, (float)133.95, (float)-72.90);
                    areaid.Add(718);
                }

                if (Host.Area.Id == 7979)//Изумрудный
                {
                    //  loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(493);
                    areaid.Add(17);
                    areaid.Add(7979);
                    areaid.Add(66);
                    areaid.Add(5042);
                }

                if (Host.Area.Id == 493)//Поляна
                {
                    // loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(493);
                    areaid.Add(17);
                    areaid.Add(7979);
                    areaid.Add(66);
                    areaid.Add(5042);
                }

                if (Host.Area.Id == 17)//перед 12 данжем
                {
                    // loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(493);
                    areaid.Add(17);
                    areaid.Add(7979);
                    areaid.Add(66);
                    areaid.Add(5042);
                }

                if (Host.Area.Id == 66)//перед 62 данжем
                {
                    // loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(493);
                    areaid.Add(17);
                    areaid.Add(66);
                    areaid.Add(7979);
                    areaid.Add(5042);
                }

                if (Host.Area.Id == 5042)//перед 85 данжем
                {
                    // loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(493);
                    areaid.Add(17);
                    areaid.Add(66);
                    areaid.Add(7979);
                    areaid.Add(5042);
                }

                if (Host.Area.Id == 4196)//в 62 данже
                {
                    // loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(4196);
                }



                if (Host.Area.Id == 5088)//в 85 данже
                {
                    // loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                    areaid.Add(5088);
                }

                for (var index = 0; index < PartyList.Count; index++)
                {
                    var core = PartyList[index];
                    if (core.Me.Level > 89)
                    {
                        core.GetCurrentAccount().IsAutoLaunch = false;
                        core.TerminateGameClient();
                        PartyList.Remove(core);
                        break;
                    }
                }

                foreach (var core in PartyList)
                {
                    if (!Host.MainForm.On)
                        break;
                    if (core.GameState != EGameState.Ingame)
                        continue;
                    if (core.Me == null)
                        continue;

                    if (core.Me.IsMoving)
                        continue;
                    if (core.SpellManager.IsCasting)
                        continue;
                    if (core.SpellManager.IsChanneling)
                        continue;



                    //AddMech(core);

                    if (areaid.Contains(core.Area.Id))
                    {
                        Host.log("Необходимо выйти из зоны " + core.Me.Name + " Моя зона " + core.Area.Id + " GameState:" + core.GameState + " лидер в зоне: " + Host.Area.Id);
                        foreach (var u in areaid)
                        {
                            Host.log("Я не должен находиться в зоне " + u + " " + core.Me.Name);
                        }
                        /*  if (core.Me.Level > 13 && core.Me.Class == EClass.Druid)
                          {
                              if (core.SpellManager.CastSpell(18960) != ESpellCastError.SUCCESS)
                                  Host.log("Не удалось использовать для выхода " + skill.Name + "  " + core.GetLastError(), Host.LogLvl.Error);
                              else
                              {
                                  if (Host.CharacterSettings.LogScriptAction)
                                      Host.log("Использовал скилл для выхода " + skill.Name, Host.LogLvl.Ok);

                              }
                              Thread.Sleep(1000);
                              while (core.Me.IsMoving)
                                  Thread.Sleep(100);
                              while (core.SpellManager.IsCasting)
                                  Thread.Sleep(100);
                              while (core.SpellManager.IsChanneling)
                                  Thread.Sleep(100);
                              Thread.Sleep(1000);
                              continue;
                          */
                        if (core.Me.Distance(1029.80, 615.07, 153.27) < 100)
                        {
                            loc = new Vector3F((float)1029.80, (float)615.07, (float)153.27);
                        }


                        if (core.Me.Distance(819.63, 985.99, 320.19) < 100)
                        {
                            loc = new Vector3F((float)819.63, (float)985.99, (float)320.19);
                        }


                        if (core.Me.Distance(-168.27, 133.95, -72.90) < 100)
                        {
                            loc = new Vector3F((float)-168.27, (float)133.95, (float)-72.90);
                        }

                        if (core.Me.Distance(-746.35, -2215.15, 15.42) < 50)
                        {
                            loc = new Vector3F((float)-746.35, (float)-2215.15, (float)15.42);
                        }
                        //Второй данж
                        if (core.Me.Distance(4774.46, -2020.02, 229.34) < 50)
                        {
                            loc = new Vector3F((float)4774.46, (float)-2020.02, (float)229.34);//возле данжа
                        }

                        if (core.Me.Distance(-519.22, -480.26, 10.97) < 50)
                        {
                            loc = new Vector3F((float)-519.22, (float)-480.26, (float)10.97);//в данже
                        }

                        if (loc.X == 0)
                        {
                            Host.log("не нашел координаты ", Host.LogLvl.Error);
                            continue;
                        }
                        var doneDist = Host.Me.RunSpeed / 5.0;

                        /* if (core.Me.IsAFK || core.Me.IsDND)
                         {
                             if (core.Me.StandState != EStandState.Stand)
                                 if (!core.ChangeStandState(EStandState.Stand))
                                 {
                                     Host.log("Не удалось встать " + core.Me.StandState + " " + core.GetLastError(), Host.LogLvl.Error);
                                 }
                         }
                         */
                        Host.log("Выхожу " + core.Me.Name + " GameState:" + core.GameState + " " + core.Me.Distance(loc));
                        if (!core.ComeTo(loc, doneDist, doneDist))
                            Host.log("Не смог добежать " + core.Me.Name + "  " + core.Me.Distance(loc) + "  " + core.GetLastError(), Host.LogLvl.Error);
                        Host.log("Вышел " + core.Me.Name + " GameState:" + core.GameState);


                        /*    core.MoveForward(true);
                            Thread.Sleep(1000);
                            core.MoveForward(false);*/
                    }
                    else
                    {
                        Host.log("Уже вышел? " + core.Me.Name + " " + core.Area.Id);
                    }
                }


                //Проверить все ли вышли
                var isAllExit = true;
                foreach (var core in PartyList)
                {
                    if (core.GameState != EGameState.Ingame || areaid.Contains(core.Area.Id) || core.Me == null)
                    {
                        isAllExit = false;
                    }
                }

                if (isAllExit)
                {
                    Host.log("Все вышли ");
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }

        }


        /*  public void AddMech(Core core)
          {
              core.AddNonUnloadableMesh(600, 31, 31);
              core.AddNonUnloadableMesh(600, 31, 32);
              core.AddNonUnloadableMesh(600, 31, 33);
              core.AddNonUnloadableMesh(600, 32, 31);
              core.AddNonUnloadableMesh(600, 32, 32);
              core.AddNonUnloadableMesh(600, 32, 33);
              core.AddNonUnloadableMesh(600, 33, 31);
              core.AddNonUnloadableMesh(600, 33, 32);
              core.AddNonUnloadableMesh(600, 33, 33);
              core.AddNonUnloadableMesh(600, 34, 31);
              core.AddNonUnloadableMesh(600, 34, 32);
              core.AddNonUnloadableMesh(600, 34, 33);
              core.AddNonUnloadableMesh(600, 32, 34);
              core.AddNonUnloadableMesh(600, 33, 34);
              core.AddNonUnloadableMesh(600, 34, 34);

              core.AddNonUnloadableMesh(571, 34, 22);
              core.AddNonUnloadableMesh(571, 34, 23);
              core.AddNonUnloadableMesh(571, 34, 24);
              core.AddNonUnloadableMesh(571, 35, 22);
              core.AddNonUnloadableMesh(571, 35, 23);
              core.AddNonUnloadableMesh(571, 35, 24);
              core.AddNonUnloadableMesh(571, 36, 22);
              core.AddNonUnloadableMesh(571, 36, 23);
              core.AddNonUnloadableMesh(571, 36, 24);
              core.AddNonUnloadableMesh(571, 34, 21);
              core.AddNonUnloadableMesh(571, 35, 21);
              core.AddNonUnloadableMesh(571, 36, 21);


              core.AddNonUnloadableMesh(1, 36, 16);
              core.AddNonUnloadableMesh(1, 36, 17);
              core.AddNonUnloadableMesh(1, 36, 18);
              core.AddNonUnloadableMesh(1, 37, 16);
              core.AddNonUnloadableMesh(1, 37, 17);
              core.AddNonUnloadableMesh(1, 37, 18);
              core.AddNonUnloadableMesh(1, 38, 16);
              core.AddNonUnloadableMesh(1, 38, 17);
              core.AddNonUnloadableMesh(1, 38, 18);


              core.AddNonUnloadableMesh(1, 35, 32);
              core.AddNonUnloadableMesh(1, 35, 33);
              core.AddNonUnloadableMesh(1, 35, 34);
              core.AddNonUnloadableMesh(1, 36, 32);
              core.AddNonUnloadableMesh(1, 36, 33);
              core.AddNonUnloadableMesh(1, 36, 34);
              core.AddNonUnloadableMesh(1, 37, 32);
              core.AddNonUnloadableMesh(1, 37, 33);
              core.AddNonUnloadableMesh(1, 37, 34);
              //12 данж
              core.AddNonUnloadableMesh(43, 30, 31);
              core.AddNonUnloadableMesh(43, 30, 32);
              core.AddNonUnloadableMesh(43, 30, 33);
              core.AddNonUnloadableMesh(43, 31, 31);
              core.AddNonUnloadableMesh(43, 31, 32);
              core.AddNonUnloadableMesh(43, 31, 33);
              core.AddNonUnloadableMesh(43, 32, 31);
              core.AddNonUnloadableMesh(43, 32, 32);
              core.AddNonUnloadableMesh(43, 32, 33);


              //перед 85 данж 
              core.AddNonUnloadableMesh(646, 29, 29);
              core.AddNonUnloadableMesh(646, 29, 30);
              core.AddNonUnloadableMesh(646, 29, 31);
              core.AddNonUnloadableMesh(646, 30, 29);
              core.AddNonUnloadableMesh(646, 30, 30);
              core.AddNonUnloadableMesh(646, 30, 31);
              core.AddNonUnloadableMesh(646, 31, 29);
              core.AddNonUnloadableMesh(646, 31, 30);
              core.AddNonUnloadableMesh(646, 31, 31);

              //в 85 данж 
              core.AddNonUnloadableMesh(725, 29, 29);
              core.AddNonUnloadableMesh(725, 29, 30);
              core.AddNonUnloadableMesh(725, 29, 31);
              core.AddNonUnloadableMesh(725, 30, 29);
              core.AddNonUnloadableMesh(725, 30, 30);
              core.AddNonUnloadableMesh(725, 30, 31);
              core.AddNonUnloadableMesh(725, 31, 29);
              core.AddNonUnloadableMesh(725, 31, 30);
              core.AddNonUnloadableMesh(725, 31, 31);
          }

          */
        public bool HerbQuest;
        public bool CheckHerbalism()
        {
            try
            {
                if (Host.Me.Team == ETeam.Alliance)
                {
                    if (Host.MyGetItem(159877) != null)
                    {
                        if (Host.GetQuest(51312) != null) //51447 State:None LogTitle:Душистый опылитель 
                        {
                            if (Host.Area.Id == 8717)
                            {
                                if (!Host.CommonModule.MoveTo(1263.38, -543.17, 33.40))
                                    return false;
                                RunQuest(51312);
                            }
                            else
                            {
                                Host.MyUseTaxi(8717, new Vector3F(1263.38, -543.17, 33.40));
                            }
                            return false;
                        }
                    }

                    if (Host.MyGetItem(159956) != null)
                    {
                        if (Host.GetQuest(48758) != null) //Отвратительно слизкий цветок
                        {
                            if (Host.Area.Id == 8717)
                            {
                                if (!Host.CommonModule.MoveTo(1263.38, -543.17, 33.40))
                                    return false;
                                RunQuest(48758);
                            }
                            else
                            {
                                Host.MyUseTaxi(8717, new Vector3F(1263.38, -543.17, 33.40));
                            }
                            return false;
                        }
                    }

                    uint id = 0;




                    if (Host.CheckQuestCompleted(51312) && Host.SpellManager.GetSpell(252418) != null)
                        id = 51313;


                    if (id == 51313 && Host.GetQuest(id) != null)
                    {
                        RunQuest(id);
                        return false;
                    }

                    if (Host.CheckQuestCompleted(48758) && Host.SpellManager.GetSpell(252419) != null)
                        id = 48755;




                    if (id == 48755 && Host.GetQuest(id) != null)
                    {
                        var step = 0;
                        foreach (var iCount in Host.GetQuest(48755).Counts)
                        {
                            step = step + iCount;
                        }

                        if (step < 12)
                            return true;
                        if (step == 12)
                        {
                            Host.PrepareCompleteQuest(48755);
                            if (!Host.CompleteQuest(48755, 0))
                            {
                                Host.log("Не смог завершить квест " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(5000);
                            }
                            return false;
                        }
                    }

                    if (Host.SpellManager.GetSpell(252408) != null)//мох 2
                        id = 48756;
                    if (Host.SpellManager.GetSpell(252409) != null)//мох 3
                        id = 48757;

                    if (Host.SpellManager.GetSpell(252405) != null)//горох 2
                        id = 48753;
                    if (Host.SpellManager.GetSpell(252406) != null)//горох 3
                        id = 48754;

                    if (id == 48756 && Host.GetQuest(id) != null)
                    {
                        var step = 0;
                        foreach (var iCount in Host.GetQuest(48756).Counts)
                        {
                            step = step + iCount;
                        }

                        if (step < 10)
                            return true;
                    }
                    if (id == 48753 && Host.GetQuest(id) != null)
                    {
                        RunQuest(id);
                        return false;
                    }
                    if (id == 48757 && Host.GetQuest(id) != null)
                    {
                        RunQuest(id);
                        return false;
                    }
                    if (id == 48754 && Host.GetQuest(id) != null)
                    {
                        RunQuest(id);
                        return false;
                    }
                    //  Host.log("Травничество: " + id);
                    if (id != 0) //Речной горох
                    {
                        HerbQuest = true;
                        if (Host.Area.Id == 8717)
                        {
                            if (Host.GetQuest(id) == null)
                            {
                                if (!Host.CommonModule.MoveTo(1263.38, -543.17, 33.40))
                                    return false;

                                var npc = Host.GetNpcById(136096);
                                if (npc != null)
                                {
                                    if (MyApplyQuest(npc, id))
                                        return false;
                                }
                            }
                            else
                            {

                                /* if (id == 51464)
                                 {
                                     var step = 0;
                                     foreach (var iCount in Host.GetQuest(51464).Counts)
                                     {
                                         step = step + iCount;
                                     }

                                     if (step < 10)
                                         return true;
                                 }*/
                                RunQuest(id);
                            }

                        }
                        else
                        {
                            Host.MyUseTaxi(8717, new Vector3F(1263.38, -543.17, 33.40));
                        }

                        return false;
                    }

                }


                if (Host.Me.Team == ETeam.Horde)
                {
                    if (Host.MyGetItem(160250) != null)
                    {
                        if (Host.GetQuest(51447) != null) //51447 State:None LogTitle:Душистый опылитель 
                        {
                            if (Host.Area.Id == 8499)
                            {
                                if (!Host.CommonModule.MoveTo(-932.67, 1006.92, 321.04))
                                    return false;
                                RunQuest(51447);
                            }
                            else
                            {
                                Host.MyUseTaxi(8499, new Vector3F(-932.67, 1006.92, 321.04));
                            }
                            return false;
                        }
                    }

                    if (Host.MyGetItem(160301) != null)
                    {
                        if (Host.GetQuest(51451) != null) //Отвратительно слизкий цветок
                        {
                            if (Host.Area.Id == 8499)
                            {
                                if (!Host.CommonModule.MoveTo(-932.67, 1006.92, 321.04))
                                    return false;
                                RunQuest(51451);
                            }
                            else
                            {
                                Host.MyUseTaxi(8499, new Vector3F(-932.67, 1006.92, 321.04));
                            }
                            return false;
                        }
                    }



                    uint id = 0;
                    if (Host.SpellManager.GetSpell(252408) != null)//мох 2
                        id = 51464;
                    if (Host.SpellManager.GetSpell(252409) != null)//мох 3
                        id = 51478;

                    if (Host.SpellManager.GetSpell(252405) != null)//горох 2
                        id = 51230;
                    if (Host.SpellManager.GetSpell(252406) != null)//горох 3
                        id = 51243;

                    if (Host.CheckQuestCompleted(51447) && Host.SpellManager.GetSpell(252418) != null)
                        id = 51448;

                    if (Host.CheckQuestCompleted(51451) && Host.SpellManager.GetSpell(252419) != null)
                        id = 51452;




                    if (id == 51452 && Host.GetQuest(id) != null)
                    {
                        var step = 0;
                        foreach (var iCount in Host.GetQuest(51452).Counts)
                        {
                            step = step + iCount;
                        }

                        if (step < 12)
                            return true;
                        if (step == 12)
                        {
                            Host.PrepareCompleteQuest(51452);
                            if (!Host.CompleteQuest(51452, 0))
                            {
                                Host.log("Не смог завершить квест " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(5000);
                            }
                            return false;
                        }
                    }

                    if (id == 51448 && Host.GetQuest(id) != null)
                    {
                        RunQuest(id);
                        return false;
                    }



                    if (id == 51464 && Host.GetQuest(id) != null)
                    {
                        var step = 0;
                        foreach (var iCount in Host.GetQuest(51464).Counts)
                        {
                            step = step + iCount;
                        }

                        if (step < 10)
                            return true;
                    }


                    if (id != 0) //Речной горох
                    {
                        HerbQuest = true;
                        if (Host.Area.Id == 8499)
                        {

                            if (Host.GetQuest(id) == null)
                            {
                                if (!Host.CommonModule.MoveTo(-932.67, 1006.92, 321.04))
                                    return false;

                                var npc = Host.GetNpcById(122704);
                                if (npc != null)
                                {
                                    if (MyApplyQuest(npc, id))
                                        return false;
                                }
                            }
                            else
                            {

                                if (id == 51464)
                                {
                                    var step = 0;
                                    foreach (var iCount in Host.GetQuest(51464).Counts)
                                    {
                                        step = step + iCount;
                                    }

                                    if (step < 10)
                                        return true;
                                }
                                RunQuest(id);
                            }

                        }
                        else
                        {
                            Host.MyUseTaxi(8499, new Vector3F(-932.67, 1006.92, 321.04));
                        }

                        return false;
                    }
                }



                return true;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return true;
            }
        }

        public bool StartWait;

        public void CheckGO(GameObject go, int anim)
        {
            if (go.OwnerGuid == Host.Me.Guid && go.Name == "Fishing Bobber")
            {
                Thread.Sleep(Host.RandGenerator.Next(100, 300));
                go.Use();
                StartWait = false;
            }
        }

        public string shedulePlugin = "";
        public bool EnableFarmProp = true;
        public bool EnableSkinning = true;
        public bool Continue;

        public class CraftRecept
        {
            public uint Id;
            public List<CraftIngridient> CraftIngridients;
        }


        public class CraftIngridient
        {
            public uint Id;
            public uint Count;
        }

        private bool Send;

        private void Mode_86()
        {
            try
            {
                if (Host.GetCurrentAccount().Name.Contains("Leader"))
                {
                    var partIndexLeader = Host.GetCurrentAccount().Name.IndexOf("Leader");
                    var partNumberLeader = Host.GetCurrentAccount().Name.Substring(partIndexLeader + 7, 2);
                    Host.log("Лидер группы " + partNumberLeader);

                    foreach (var account in Host.GetAccounts())
                    {
                        if (!account.Name.Contains("Party"))
                            continue;
                        if (account.RealmId != Host.GetCurrentAccount().RealmId)
                            continue;



                        var partIndex = account.Name.IndexOf("Party");
                        var partNumber = account.Name.Substring(partIndex + 6, 2);

                        Host.log(account.Name + " " + account.IsAutoLaunch + "  " + partNumber);

                        if (partNumber == partNumberLeader && !PartyList.Contains(account.GetCore() as Core) && account.IsAutoLaunch)
                        {
                            if (PartyList.Count > 4)
                            {
                                Host.log("Лишний аккаунт " + PartyList.Count);
                                continue;
                            }
                            var core = account.GetCore() as Core;
                            PartyList.Add(account.GetCore() as Core);


                        }
                    }
                    Host.log("Найдено " + PartyList.Count + " аккаунтов. В группе " + Host.Group.GetMembers().Count);

                    if (!IsAllOnline())
                        return;

                    foreach (var core in PartyList)
                    {
                        var isNeedInvite = true;
                        foreach (var groupMember in Host.Group.GetMembers())
                        {
                            if (core.Me.Name == groupMember.Name)
                            {
                                isNeedInvite = false;
                            }
                        }

                        if (isNeedInvite)
                        {


                            var guid = default(WowGuid);
                            uint virtualRealmAddress = 0;
                            if (Host.Me.Guid.GetRealmId() != core.Me.Guid.GetRealmId())
                            {
                                guid = core.Me.Guid;
                                virtualRealmAddress = core.CurrentServer.Id;
                            }

                            Host.log("Приглашаю в группу " + core.Me.Name + " " + core.Me.Guid.GetRealmId() + "  " + core.Me.Guid.GetServerId() + "  guid:" + guid + "  VirtualRealmAddress:" + virtualRealmAddress, Host.LogLvl.Important);
                            var result = Host.Group.Invite(core.Me.Name, virtualRealmAddress, guid);
                            if (result != EPartyResult.Ok)
                                Host.log("Не смог пригласить в группу " + result + " " + Host.GetLastError(), Host.LogLvl.Error);
                            else
                            {
                                Host.log("Пригласил в группу ", Host.LogLvl.Ok);
                            }
                            Thread.Sleep(10000);
                        }
                    }


                    Thread.Sleep(1000);
                    //continue;
                }

                if (Host.CharacterSettings.RunQuestHerbalism)
                    if (!CheckHerbalism())
                        return;

                scriptStopwatch = new Stopwatch();
                scriptStopwatch.Start();


                var mobsStart = Host.KillMobsCount;
                double tempGold = Host.Me.Money;
                double goldStart = tempGold / 10000;
                long startinvgold = 0;

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        //   log(item.Id + "  "+ item.Name);
                        //  startinvgold = startinvgold + item.GetSellPrice() * item.Count;
                    }
                }


                double bestPoint = 999999;
                var bestpoint = new DungeonCoordSettings();
                var lastPoint = new DungeonCoordSettings();
                DungeonCoordSettings checkPoint = null;
                var reverse = false;
                if (Host.CharacterSettings.ScriptScheduleEnable)
                {
                    foreach (var characterSettingsScriptSchedule in Host.CharacterSettings.ScriptSchedules)
                    {
                        if (!DateTime.Now.TimeOfDay.IsBetween(characterSettingsScriptSchedule.ScriptStartTime, characterSettingsScriptSchedule.ScriptStopTime))
                            continue;

                        if (shedulePlugin == characterSettingsScriptSchedule.ScriptName)
                            break;


                        var ScriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\" + characterSettingsScriptSchedule.ScriptName;
                        // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;
                        Host.log("Применяю скрипт: " + ScriptName + " Текущее время: " + DateTime.Now + " Время запуска " + characterSettingsScriptSchedule.ScriptStartTime + " Время окончания" + characterSettingsScriptSchedule.ScriptStopTime, Host.LogLvl.Ok);

                        var reader = new XmlSerializer(typeof(DungeonSetting));
                        using (var fs = File.Open(ScriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                            Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                        shedulePlugin = characterSettingsScriptSchedule.ScriptName;
                        if (characterSettingsScriptSchedule.Reverse)
                            reverse = true;
                        break;
                    }
                    //  Host.log("Выбрал скрипт " + shedulePlugin);
                }

                var ScriptActionList = Host.DungeonSettings.DungeonCoordSettings;

                if (Host.CharacterSettings.ScriptReverse || reverse)
                    ScriptActionList.Reverse();

                foreach (var dungeonSettingsScriptCoordSetting in ScriptActionList)
                {
                    if (dungeonSettingsScriptCoordSetting.Action != "Бежать на точку")
                        continue;
                    if (checkPoint == null)
                    {
                        checkPoint = dungeonSettingsScriptCoordSetting;
                    }
                    lastPoint = dungeonSettingsScriptCoordSetting;
                    if (dungeonSettingsScriptCoordSetting.AreaId != Host.Area.Id)
                        continue;
                    if (dungeonSettingsScriptCoordSetting.MapId != Host.MapID)
                        continue;

                    if (Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc.X, dungeonSettingsScriptCoordSetting.Loc.Y, dungeonSettingsScriptCoordSetting.Loc.Z)
                       /* Host.FarmModule.GetDistToMobFromMech(dungeonSettingsScriptCoordSetting.Loc)*/ < bestPoint)
                    {
                        bestPoint = Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc.X, dungeonSettingsScriptCoordSetting.Loc.Y, dungeonSettingsScriptCoordSetting.Loc.Z);
                        bestpoint = dungeonSettingsScriptCoordSetting;
                    }
                }



                if (bestpoint == lastPoint)// если лучшая точка последняя, то начать сначала
                {
                    bestpoint = ScriptActionList[0];
                    bestPoint = 999999;
                }
                if (checkPoint != null && checkPoint == bestpoint)
                {
                    bestpoint = ScriptActionList[0];
                    bestPoint = 999999;
                }

                if (Host.CharacterSettings.LogScriptAction)
                    Host.log("Начал выполение скрипта " + Host.Me.Name + "  " + scriptStopwatch.ElapsedMilliseconds, Host.LogLvl.Ok);

                for (var index = 0; index < ScriptActionList.Count; index++)
                {

                    while (Host.GameState != EGameState.Ingame)
                        Thread.Sleep(100);
                    while (Host.CommonModule.IsMoveSuspended())
                        Thread.Sleep(100);
                    if (!Host.MainForm.On)
                        break;
                    if (!Host.IsAlive(Host.Me))
                        break;
                    if (Host.Me.IsDeadGhost)
                        break;

                    if (Host.CharacterSettings.SendMail && !Send && Host.MapID == Host.CharacterSettings.SendMailLocMapId && DateTime.Now.TimeOfDay.IsBetween(Host.CharacterSettings.SendMailStartTime, Host.CharacterSettings.SendMailStopTime))
                    {
                        while (!Send)
                        {
                            Thread.Sleep(1000);
                            if (!Host.MainForm.On)
                                return;
                            if (!Host.CommonModule.MoveTo(Host.CharacterSettings.SendMailLocX, Host.CharacterSettings.SendMailLocY, Host.CharacterSettings.SendMailLocZ, 10, 10))
                                continue;

                            GameObject mailBox = null;
                            foreach (var gameObject in Host.GetEntities<GameObject>().OrderBy(i => Host.Me.Distance(i)))
                            {
                                if (gameObject.GameObjectType != EGameObjectType.Mailbox)
                                    continue;
                                mailBox = gameObject;
                                break;
                            }
                            var result = ((double)Host.Me.Money / 100) * 99;
                            if (mailBox != null)
                            {
                                Host.ForceComeTo(mailBox, 2);
                                Host.MyCheckIsMovingIsCasting();
                                Thread.Sleep(1000);
                                if (!Host.OpenMailbox(mailBox))
                                    Host.log("Не удалось открыть ящик " + Host.GetLastError(), Host.LogLvl.Error);
                                else
                                    Host.log("Открыл ящик", Host.LogLvl.Ok);
                                Thread.Sleep(2000);
                                if (!Host.SendMail(Host.CharacterSettings.SendMailName, "123", "", Convert.ToInt64(result), new List<Item>()))
                                    Host.log("Не смог отправить письмо " + Convert.ToInt64(result) + "   " + Host.GetLastError(), Host.LogLvl.Error);
                                else
                                {
                                    Host.log("Отправил письмо " + Convert.ToInt64(result), Host.LogLvl.Ok);
                                }

                                Send = true;
                            }
                        }
                    }

                    if (NeedFindBestPoint)
                    {
                        Host.log("Ищу новую ближайшую точку");
                        foreach (var dungeonSettingsScriptCoordSetting in ScriptActionList)
                        {
                            if (dungeonSettingsScriptCoordSetting.Action != "Бежать на точку")
                                continue;
                            if (dungeonSettingsScriptCoordSetting.AreaId != Host.Area.Id)
                                continue;
                            if (dungeonSettingsScriptCoordSetting.MapId != Host.MapID)
                                continue;

                            if (Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc.X, dungeonSettingsScriptCoordSetting.Loc.Y, dungeonSettingsScriptCoordSetting.Loc.Z)
                               /* Host.FarmModule.GetDistToMobFromMech(dungeonSettingsScriptCoordSetting.Loc)*/ < bestPoint)
                            {
                                // host.log("Нашел " + dungeonSettingsScriptCoordSetting.Id);
                                bestPoint = Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc.X, dungeonSettingsScriptCoordSetting.Loc.Y, dungeonSettingsScriptCoordSetting.Loc.Z);
                                bestpoint = dungeonSettingsScriptCoordSetting;
                            }
                        }
                        NeedFindBestPoint = false;
                    }


                    var dungeon = ScriptActionList[index];
                    if (bestPoint != 999999 && bestpoint != dungeon)
                    {
                        continue;
                    }
                    else
                    {
                        bestPoint = 999999;
                    }

                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if ((entity).TaxiStatus == ETaxiNodeStatus.Unlearned)
                        {
                            if (!Host.CommonModule.MoveTo(entity, 2, 2))
                                continue;

                            Thread.Sleep(500);
                            if (!Host.OpenTaxi(entity))
                            {
                                Host.log("Не смог открыть диалог с " + entity.Name + " TaxiStatus:" + entity.TaxiStatus + " IsTaxi:" + entity.IsTaxi + "  " + Host.GetLastError(), Host.LogLvl.Error);
                            }
                        }
                    }

                    if (Host.CharacterSettings.LogScriptAction)
                        Host.log(dungeon.Id + ")Выполняю действие: " + dungeon.Action + " координаты:" + dungeon.Loc + "  Attack:" + dungeon.Attack + " дист:" + Host.Me.Distance(dungeon.Loc), Host.LogLvl.Important);

                    //  host.CommonModule.LoadCurrentZoneMesh(host.Me.Location.X, host.Me.Location.Y);



                    /*  if (dungeon.ItemId > 0)
                          if (host.Me.GetItemsCount(dungeon.ItemId) == 0)
                          {
                              host.log("Нет предмета " + dungeon.ItemId, Host.LogLvl.Error);
                              MyExitDangeon(dungeon);
                              break;
                          }*/
                    switch (dungeon.Action)
                    {
                        case "Использовать портал":
                            {
                                foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i)))
                                {
                                    if (!entity.IsSpellClick)
                                        continue;
                                    if (Host.Me.Distance(entity) > 3)
                                        Host.CommonModule.MoveTo(entity, 2);
                                    MyUseSpellClick(entity.Id);
                                    Thread.Sleep(1000);
                                    while (Host.GameState != EGameState.Ingame)
                                    {
                                        if (!Host.MainForm.On)
                                            return;
                                        Thread.Sleep(500);

                                    }
                                    break;
                                }
                            }
                            break;

                        case "Купить нитки":
                            {

                                if (Host.MeGetItemsCount(159959) < dungeon.Pause)
                                {
                                    while (Host.Me.Distance(-1075, 780, 435) > 20)
                                    {
                                        if (!Host.MainForm.On)
                                            return;
                                        Host.CommonModule.MoveTo(-1075, 780, 435, 3);
                                        Thread.Sleep(1000);
                                    }

                                    var npc = Host.GetNpcById(141928);
                                    if (npc != null)
                                    {
                                        Host.CommonModule.MoveTo(npc, 3);
                                        Thread.Sleep(2000);
                                        if (!Host.OpenShop(npc as Unit))
                                            Host.log("Не смог открыть диалог " + npc.Name + " " + Host.GetLastError(), Host.LogLvl.Error);
                                        Thread.Sleep(2000);
                                        foreach (var item in Host.GetVendorItems())
                                        {
                                            if (item.ItemId != 159959)
                                                continue;
                                            if (!Host.GameDB.ItemTemplates.ContainsKey(item.ItemId))
                                            {
                                                Host.log("Не нашел в базе" + item.ItemId);
                                                continue;
                                            }
                                            var itemTemplate = Host.GameDB.ItemTemplates[item.ItemId];
                                            Host.log("Предметы на продажу: " + item.ItemId + " " + item.Price + " " + itemTemplate.GetName() + " " + itemTemplate.GetMaxStackSize() + item.Quantity);
                                            var needcount = dungeon.Pause - Host.MeGetItemsCount(159959);
                                            var needSlot = needcount / itemTemplate.GetMaxStackSize() + 1;
                                            var needGold = itemTemplate.GetMaxStackSize() * item.Price;
                                            Host.log("Нужно " + needcount + " шт. " + needSlot + " слот " + (needGold / 10000f).ToString("F2") + " г.");


                                            while (needcount > 0)
                                            {
                                                if (Host.ItemManager.GetFreeInventorySlotsCount() < 11)
                                                {
                                                    Host.log("Нет свободных слотов " + Host.ItemManager.GetFreeInventorySlotsCount(), Host.LogLvl.Error);
                                                    break;
                                                }

                                                if (Host.Me.Money < Convert.ToUInt64(needGold))
                                                {
                                                    Host.log("Не хватает денег " + Host.Me.Money + "/" + needGold);
                                                    break;
                                                }

                                                Thread.Sleep(100);
                                                if (!Host.MainForm.On)
                                                    return;
                                                int buyCount = Math.Min(Convert.ToInt32(needcount), Convert.ToInt32(itemTemplate.GetMaxStackSize()));
                                                var res = item.Buy(buyCount);
                                                if (res != EBuyResult.Success)
                                                {
                                                    Host.log("Не смог купить " + itemTemplate.GetName() + "  " + buyCount + " " + res + " " + Host.GetLastError(), Host.LogLvl.Error);
                                                    Thread.Sleep(10000);
                                                    break;
                                                }
                                                Thread.Sleep(2000);
                                                needcount = dungeon.Pause - Host.MeGetItemsCount(159959);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "Крафт":
                            {

                                var Craft = new CraftRecept();

                                switch (dungeon.SkillId)
                                {
                                    case 257099:
                                        {
                                            Craft = new CraftRecept
                                            {
                                                Id = 257099,
                                                CraftIngridients = new List<CraftIngridient>
                                                {
                                                     new CraftIngridient{Id = 152576, Count = 17},
                                                     new CraftIngridient{Id = 159959, Count = 10}
                                                }
                                            };
                                        }
                                        break;

                                    default:
                                        {
                                            Host.log("Неизветный рецепт крафта ", Host.LogLvl.Error);
                                            continue;
                                        }
                                }

                                while (Host.MeGetItemsCount(Craft.CraftIngridients[0].Id) > Craft.CraftIngridients[0].Count)
                                {
                                    Thread.Sleep(100);
                                    if (Host.ItemManager.GetFreeInventorySlotsCount() < 3)
                                    {
                                        Host.log("Необходима продажа ", Host.LogLvl.Important);
                                        Host.MySell(true);
                                    }
                                    Host.CanselForm();
                                    if (Host.Me.MountId != 0)
                                    {
                                        Host.CommonModule.MyUnmount();
                                        Thread.Sleep(2000);
                                    }

                                    var craftSpell = Host.SpellManager.GetSpell(Craft.Id);
                                    var count = Host.MeGetItemsCount(Craft.CraftIngridients[0].Id) / Craft.CraftIngridients[0].Count;
                                    Host.log("Можно скрафтить " + craftSpell.Name + "[" + craftSpell.Id + "] " + count + " шт.");
                                    if (!Host.MainForm.On)
                                        return;
                                    if (Host.Me.Team == ETeam.Horde)
                                    {
                                        while (Host.Me.Distance(-891.93, 975.31, 321.12) > 20)
                                        {
                                            if (!Host.MainForm.On)
                                                return;
                                            Host.CommonModule.MoveTo(-891.93, 975.31, 321.12);
                                            Thread.Sleep(1000);
                                        }
                                        if (Host.MeGetItemsCount(Craft.CraftIngridients[1].Id) < Craft.CraftIngridients[1].Count)
                                        {
                                            var npc = Host.GetNpcById(141609);
                                            if (npc != null)
                                            {
                                                Host.CommonModule.MoveTo(npc, 3);
                                                if (!Host.OpenShop(npc as Unit))
                                                    Host.log("Не смог открыть диалог " + npc.Name);
                                                foreach (var item in Host.GetVendorItems())
                                                {
                                                    if (item.ItemId != Craft.CraftIngridients[1].Id)
                                                        continue;
                                                    if (!Host.GameDB.ItemTemplates.ContainsKey(item.ItemId))
                                                    {
                                                        Host.log("Не нашел в базе" + item.ItemId);
                                                        continue;
                                                    }
                                                    var itemTemplate = Host.GameDB.ItemTemplates[item.ItemId];
                                                    Host.log("Предметы на продажу: " + item.ItemId + " " + item.Price + " " + itemTemplate.GetName() + " " + itemTemplate.GetMaxStackSize() + item.Quantity);
                                                    var needcount = count * Craft.CraftIngridients[1].Count - Host.MeGetItemsCount(Craft.CraftIngridients[1].Id);
                                                    var needSlot = needcount / itemTemplate.GetMaxStackSize() + 1;
                                                    var needGold = itemTemplate.GetMaxStackSize() * item.Price;
                                                    Host.log("Нужно " + needcount + " шт. " + needSlot + " слот " + (needGold / 10000f).ToString("F2") + " г.");


                                                    while (needcount > 0)
                                                    {
                                                        if (Host.ItemManager.GetFreeInventorySlotsCount() < 11)
                                                        {
                                                            Host.log("Нет свободных слотов " + Host.ItemManager.GetFreeInventorySlotsCount(), Host.LogLvl.Error);
                                                            break;
                                                        }

                                                        if (Host.Me.Money < Convert.ToUInt64(needGold))
                                                        {
                                                            Host.log("Не хватает денег " + Host.Me.Money + "/" + needGold);
                                                            break;
                                                        }

                                                        Thread.Sleep(100);
                                                        if (!Host.MainForm.On)
                                                            return;
                                                        int buyCount = Math.Min(Convert.ToInt32(needcount), Convert.ToInt32(itemTemplate.GetMaxStackSize()));
                                                        var res = item.Buy(buyCount);
                                                        if (res != EBuyResult.Success)
                                                        {
                                                            Host.log("Не смог купить " + itemTemplate.GetName() + "  " + buyCount + " " + res + " " + Host.GetLastError(), Host.LogLvl.Error);
                                                            Thread.Sleep(10000);
                                                            break;
                                                        }
                                                        Thread.Sleep(2000);
                                                        needcount = count * Craft.CraftIngridients[1].Count - Host.MeGetItemsCount(Craft.CraftIngridients[1].Id);
                                                    }



                                                }
                                            }

                                        }
                                        else
                                        {
                                            var res = Host.SpellManager.CastSpell(craftSpell.Id);
                                            if (res != ESpellCastError.SUCCESS)
                                            {
                                                Host.log("Не смог скрафтить " + craftSpell.Name + "  " + " " + res + " " + Host.GetLastError(), Host.LogLvl.Error);
                                                Thread.Sleep(10000);

                                            }
                                            while (Host.SpellManager.IsCasting)
                                                Thread.Sleep(100);
                                            // крафт
                                        }

                                    }
                                    Thread.Sleep(500);
                                }
                                Host.MySell(true);
                            }
                            break;

                        case "Ремонт":
                            {
                                if (Host.GetBotLogin() == "wowklausvovot")
                                {

                                }
                                else
                                {
                                    Host.MyUseStone(true);
                                    if (Host.Me.Team == ETeam.Horde)
                                        if (Host.Area.Id != 1637)
                                        {
                                            Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }

                                    if (Host.Me.Team == ETeam.Alliance)
                                        if (Host.Area.Id != 1519)
                                        {
                                            Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }
                                }

                                Host.MyRepair();

                            }
                            break;
                        case "Почта":
                            {
                                Host.MyUseStone(true);
                                if (Host.Me.Team == ETeam.Horde)
                                    if (Host.Area.Id != 1637)
                                    {
                                        Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                if (Host.Me.Team == ETeam.Alliance)
                                    if (Host.Area.Id != 1519)
                                    {
                                        Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }
                                Host.Mail();
                            }
                            break;
                        case "Аукцион":
                            {
                                Host.MyUseStone(true);
                                if (Host.Me.Team == ETeam.Horde)
                                    if (Host.Area.Id != 1637)
                                    {
                                        Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                if (Host.Me.Team == ETeam.Alliance)
                                    if (Host.Area.Id != 1519)
                                    {
                                        Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }


                                Host.Auk();
                            }
                            break;

                        case "Включить сбор шкуры":
                            {
                                EnableSkinning = true;
                            }
                            break;
                        case "Выключить сбор шкуры":
                            {
                                EnableSkinning = false;
                            }
                            break;

                        case "Включить сбор":
                            {
                                EnableFarmProp = true;
                            }
                            break;
                        case "Выключить сбор":
                            {
                                EnableFarmProp = false;
                            }
                            break;
                        case "Рыбалка":
                            {
                                if (Host.Me.Distance(dungeon.Loc) > 15)
                                {
                                    Host.log("Нахожусь далеко от точки для рыбалки, начинаю скрипт заново");
                                    return;
                                }

                                var isTimer = false;
                                var timer = new Stopwatch();
                                if (dungeon.Pause != 0)
                                {
                                    isTimer = true;
                                    timer.Start();
                                }
                                var state = Host.FarmModule.farmState;
                                Host.FarmModule.farmState = FarmState.Disabled;
                                Host.onGameObjectCustomAnim += CheckGO;
                                WaitTeleport = true;
                                while (true)
                                {
                                    Thread.Sleep(100);
                                    if (Host.Me.GetThreats().Count > 0)
                                    {
                                        Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                        continue;
                                    }
                                    if (!Host.MainForm.On)
                                        break;
                                    Host.CanselForm();
                                    StartWait = true;
                                    if (isTimer)
                                    {
                                        if (timer.Elapsed.TotalSeconds > dungeon.Pause)
                                            break;
                                    }
                                    Host.MyCheckIsMovingIsCasting();
                                    var result = Host.SpellManager.CastSpell(131474);
                                    if (result != ESpellCastError.SUCCESS)
                                    {
                                        Host.log("Не удалось закинуть удочку" + result, Host.LogLvl.Error);
                                        break;
                                    }

                                    else
                                    {
                                        for (int i = 0; i < 220; i++)
                                        {
                                            if (!Host.MainForm.On)
                                                break;
                                            if (!StartWait)
                                                break;
                                            Thread.Sleep(100);
                                        }
                                    }
                                    Thread.Sleep(Host.RandGenerator.Next(500, 1000));
                                    if (Host.CanPickupLoot())
                                        Host.PickupLoot();
                                    Thread.Sleep(Host.RandGenerator.Next(100, 500));

                                    if (Host.CharacterSettings.ScriptScheduleEnable)
                                    {
                                        foreach (var characterSettingsScriptSchedule in Host.CharacterSettings.ScriptSchedules)
                                        {
                                            if (!DateTime.Now.TimeOfDay.IsBetween(characterSettingsScriptSchedule.ScriptStartTime, characterSettingsScriptSchedule.ScriptStopTime))
                                                continue;

                                            if (shedulePlugin == characterSettingsScriptSchedule.ScriptName)
                                                break;


                                            return;
                                        }
                                    }

                                    if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount || Host.MyIsNeedRepair())
                                    {
                                        if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount)
                                            Host.log("Необходима продажа " + Host.ItemManager.GetFreeInventorySlotsCount() + "/" + Host.CharacterSettings.InvFreeSlotCount, Host.LogLvl.Important);

                                        if (Host.MyIsNeedRepair())
                                            Host.log("Необходим ремонт", Host.LogLvl.Important);
                                        NeedActionNpcSell = true;
                                        NeedActionNpcRepair = true;
                                        return;
                                    }

                                }
                                Host.onGameObjectCustomAnim -= CheckGO;
                                Host.FarmModule.farmState = state;
                                WaitTeleport = false;
                            }
                            break;
                        case "Запустить другой плагин":
                            {
                                Host.CommonModule.MyUnmount();

                                Host.LaunchPlugin(dungeon.PluginPath);
                            }
                            break;

                        case "Выключить плагин":
                            {
                                Host.cancelRequested = true;
                                Host.StopPluginNow();
                            }
                            break;
                        case "Проверить инвентарь":
                            {
                                if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount || Host.MyIsNeedRepair())
                                {
                                    if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount)
                                        Host.log("Необходима продажа " + Host.ItemManager.GetFreeInventorySlotsCount() + "/" + Host.CharacterSettings.InvFreeSlotCount, Host.LogLvl.Important);

                                    if (Host.MyIsNeedRepair())
                                        Host.log("Необходим ремонт", Host.LogLvl.Important);
                                    NeedActionNpcSell = true;
                                    NeedActionNpcRepair = true;
                                    return;
                                }
                            }
                            break;

                        case "Выбрать диалог":
                            {
                                if (Host.MapID == 43 && Host.Area.Id == 718)
                                {
                                    var npc = Host.GetNpcById(3678);
                                    if (npc != null)
                                    {
                                        Host.ComeTo(npc.Location);
                                        Thread.Sleep(500);

                                        if (!Host.OpenDialog(npc))
                                        {
                                            Host.log("Не смог начать диалог для выбора диалога с " + npc.Name + "[" + npc.Id + "] " + Host.GetLastError(), Host.LogLvl.Error);
                                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                                            {
                                                MyMoveFromNpc(npc as Unit);
                                            }
                                        }

                                        Thread.Sleep(500);
                                        var isFindDialog = false;
                                        foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                        {
                                            if (gossipOptionsData.Text.Contains("Нужен ру перевод") || gossipOptionsData.Text.Contains("event begin"))
                                            {
                                                isFindDialog = true;
                                                if (!Host.SelectNpcDialog(gossipOptionsData))
                                                    Host.log("Не смог выбрать диалог " + Host.GetLastError(), Host.LogLvl.Error);

                                            }
                                            Host.log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  ");
                                        }
                                        if (!isFindDialog)
                                        {
                                            Host.log("Необходим диалог ");
                                            Thread.Sleep(5000);
                                        }

                                    }
                                    else
                                    {
                                        Host.log("Не найден НПС ", Host.LogLvl.Error);
                                    }
                                }
                                else
                                {
                                    Host.log("Выбрать диалог, находимся не в той зоне " + Host.MapID + " " + Host.Area.Id, Host.LogLvl.Error);
                                }
                            }
                            break;

                        case "Проверить группу":
                            {
                                if (Host.Group.GetMembers().Count > 0)
                                {


                                    var allMove = false;
                                    if (scriptStopwatch.Elapsed.Minutes > 2 && scriptStopwatch.Elapsed.Minutes < 6)
                                    {
                                        while (scriptStopwatch.Elapsed.Minutes < 5 && Host.MainForm.On)
                                        {
                                            Thread.Sleep(10000);
                                            Host.log("Ожидаю до 5 минут " + scriptStopwatch.Elapsed.Minutes + " " + scriptStopwatch.Elapsed.Seconds);
                                        }
                                    }

                                    while (!allMove)
                                    {
                                        if (!Host.MainForm.On)
                                            break;
                                        if (MoveGroup())
                                            break;
                                    }
                                }
                            }
                            break;

                        case "Использовать скилл":
                            {
                                if (!dungeon.Attack)
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                else
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                Spell skill = Host.SpellManager.GetSpell(dungeon.SkillId);

                                if (skill != null)
                                {
                                    var isMoon = false;
                                    if (skill.Id == 5215)
                                    {
                                        if (Host.MyGetAura(skill.Id) != null)
                                            continue;
                                    }



                                    if (skill.Id == 18960 || skill.Id == 193753)
                                    {
                                        Host.CanselForm();
                                        Host.CommonModule.MyUnmount();

                                        //Вывести из данжа группу
                                        if (Host.Group.GetMembers().Count > 0)
                                        {


                                            var allMove = false;
                                            if (scriptStopwatch.Elapsed.Minutes > 2 && scriptStopwatch.Elapsed.Minutes < 6)
                                            {
                                                while (scriptStopwatch.Elapsed.Minutes < 5 && Host.MainForm.On)
                                                {
                                                    Thread.Sleep(10000);
                                                    Host.log("Ожидаю до 5 минут " + scriptStopwatch.Elapsed.Minutes + " " + scriptStopwatch.Elapsed.Seconds);
                                                }
                                            }

                                            while (!allMove)
                                            {
                                                if (!Host.MainForm.On)
                                                    break;
                                                if (MoveGroup())
                                                    break;
                                            }
                                        }
                                        Thread.Sleep(1000);

                                    }

                                    if (!Host.MainForm.On)
                                        return;

                                    if (skill.Id == 18960 && Host.MapID == 1 && Host.Area.Id == 493)
                                    {
                                        isMoon = true;
                                    }
                                    var isNeedWaitKD = false;
                                    if (skill.Id == 193753)
                                        isNeedWaitKD = true;
                                    if (skill.Id == 193753 && Host.MapID == 1540 && Host.Area.Id == 7979)
                                    {
                                        isMoon = true;
                                    }


                                    Host.MyCheckIsMovingIsCasting();
                                    while (Host.SpellManager.IsChanneling)
                                        Thread.Sleep(50);
                                    /* while (!Host.CheckCanUseGameActions() && Host.Me.IsAlive)
                                         Thread.Sleep(50);*/
                                    //   Host.IsRegen = true;
                                    if (isNeedWaitKD)
                                    {
                                        if (Host.SpellManager.GetSpellCooldown(dungeon.SkillId) != 0)
                                        {
                                            while (Host.SpellManager.GetSpellCooldown(dungeon.SkillId) != 0)
                                            {
                                                Thread.Sleep(1000);
                                                Host.log("Ожидаю кд " + Host.SpellManager.GetSpellCooldown(dungeon.SkillId));
                                            }
                                        }
                                    }

                                    if (isMoon || isNeedWaitKD)
                                    {
                                        Thread.Sleep(500);
                                    }
                                    var result2 = Host.SpellManager.CastSpell(dungeon.SkillId);
                                    if (result2 != ESpellCastError.SUCCESS)
                                        Host.log("Не удалось использовать скилл из скрипта " + skill.Name + "  " + result2 + "   " + Host.GetLastError(), Host.LogLvl.Error);
                                    else
                                    {
                                        if (Host.CharacterSettings.LogScriptAction)
                                            Host.log("Использовал скилл из скрипта " + skill.Name, Host.LogLvl.Ok);
                                        Thread.Sleep(200);
                                    }
                                    Host.MyCheckIsMovingIsCasting();

                                    while (Host.SpellManager.IsChanneling)
                                    {
                                        Thread.Sleep(50);
                                        //  Host.log("Использовал скилл из скрипта IsChanneling " + skill.Name, Host.LogLvl.Ok);

                                    }


                                    if (skill.Id == 18960)
                                        Thread.Sleep(5000);

                                    if (skill.Id == 193753)
                                        Thread.Sleep(5000);

                                    while (Host.GameState != EGameState.Ingame)
                                        Thread.Sleep(200);

                                    if (isMoon)
                                    {
                                        if (Host.Area.Id == 493 && Host.MapID == 1)
                                        {
                                            Host.log("После телепорта из лунной поляны оказались там же, использую камень");
                                            foreach (var item in Host.ItemManager.GetItems())
                                            {
                                                if (item.Id == 6948)
                                                {
                                                    if (Host.SpellManager.GetItemCooldown(item) != 0)
                                                    {
                                                        Host.log("Камень в КД " + Host.SpellManager.GetItemCooldown(item));
                                                        break;
                                                    }
                                                    var result = Host.SpellManager.UseItem(item);
                                                    if (result != EInventoryResult.OK)
                                                    {
                                                        Host.log("Не удалось использовать камень " + item.Name + " " + result + " " + Host.GetLastError(), Host.LogLvl.Error);
                                                    }
                                                    else
                                                    {
                                                        Host.log("Использовал камень ", Host.LogLvl.Ok);
                                                    }
                                                    Host.MyCheckIsMovingIsCasting();
                                                    while (Host.SpellManager.IsChanneling)
                                                        Thread.Sleep(50);
                                                    Thread.Sleep(5000);
                                                    while (Host.GameState != EGameState.Ingame)
                                                    {
                                                        Thread.Sleep(200);
                                                    }
                                                    Thread.Sleep(1000);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    //    Host.IsRegen = false;
                                }
                                else
                                {
                                    Host.log("Скилл не найден на панели", Host.LogLvl.Error);
                                }
                            }
                            break;

                        case "Сбросить все данжи":
                            {
                                if (!Host.ResetInstances())
                                {
                                    Host.log("Не удалось сбросить данжи " + Host.GetLastError(), Host.LogLvl.Error);
                                    Thread.Sleep(10000);
                                }
                            }
                            break;

                        case "Пауза":
                            {
                                if (!dungeon.Attack)
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                else
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                var delay = dungeon.Pause;

                                Thread.Sleep(delay);
                            }
                            break;
                        case "Пауза 2":
                            {
                                Host.MainForm.Dispatcher.Invoke(() =>
                                {
                                    Host.MainForm.ButtonContinue.IsEnabled = true;

                                });
                                Continue = false;
                                WaitTeleport = true;


                                while (Host.MainForm.On)
                                {
                                    Thread.Sleep(1000);
                                    if (Continue)
                                        break;
                                }
                                WaitTeleport = false;
                            }
                            break;

                        case "Продажа через Телепорт":
                            {
                                /*  if (host.Me.Pet != null)
                                      host.Me.Pet.Dismiss();
                                  WaitTeleport = true;
                                  var teleport = host.Me.GetItem(50169);
                                  while (host.GetItemCooldownTime(teleport.Id) != 0)
                                      Thread.Sleep(5000);

                                  WaitTeleport = false;
                                  if (host.GetItemCooldownTime(teleport.Id) == 0)
                                  {
                                      Thread.Sleep(1000);
                                      while (host.Me.IsMoving)
                                          Thread.Sleep(50);

                                      if (host.UseItem(teleport.Id, EItemUseType.UseByItemUniqId))
                                      {
                                          host.log("Использовал телепорт для продажи ");
                                          Thread.Sleep(7000);
                                          while (host.Me.IsCastingSkill)
                                              Thread.Sleep(1000);
                                          while (host.GameState != EGameState.Ingame)
                                              Thread.Sleep(100);
                                          Thread.Sleep(100);
                                          host.CommonModule.LoadCurrentZoneMesh(host.Me.Location.X, host.Me.Location.Y);

                                      }
                                      else
                                      {
                                          host.log("Не удалось использовать телепорт 1" + host.GetLastError(), Host.LogLvl.Error);
                                          Thread.Sleep(1000);

                                          while (host.GameState != EGameState.Ingame)
                                              Thread.Sleep(100);
                                          Thread.Sleep(1000);

                                      }

                                  }



                                  if (host.WorldMapType == EWorldMapType.Field)
                                  {
                                      if (host.CharacterSettings.Mode == "Данж.(п)")
                                          if (host.WorldMapType == EWorldMapType.Field)
                                              foreach (var fieldChannel in host.GetFieldChannels())
                                                  if (fieldChannel.ChannelId == 2 && fieldChannel != host.CurrentFieldChannel)
                                                  {
                                                      if (!fieldChannel.Transfer())
                                                          host.log("Не смог поменять канал " + host.GetLastError(), Host.LogLvl.Error);
                                                      break;
                                                  }

                                      host.CommonModule.LoadCurrentZoneMesh(host.Me.Location.X, host.Me.Location.Y);
                                      host.log("Необходима продажа", Host.LogLvl.Important);
                                      host.FarmModule.farmState = FarmState.Disabled;


                                      host.FarmModule.farmState = FarmState.AttackOnlyAgro;*/
                            }



                            break;
                        case "Вход в данж":
                            {
                                MyEnterDangeon(dungeon);
                            }
                            break;
                        case "Выход из данжа":
                            {
                                MyExitDangeon(dungeon);
                            }
                            break;

                        case "Бежать на точку":
                            {
                                if (dungeon.MapId != Host.MapID)
                                {
                                    Host.log("Точка на другом континенете " + dungeon.MapId + "  " + Host.MapID, Host.LogLvl.Error);
                                    Thread.Sleep(1000);
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                    if (dungeon.MapId == 1643 && Host.MapID == 0)
                                    {
                                        var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(-9077.94, 891.76, 68.04), Host.Me.Location);
                                        Host.log(path.Count + "  Путь");
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                        }

                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 323845)
                                                continue;
                                            Host.CommonModule.MoveTo(gameObject);
                                            Host.CommonModule.MyUnmount();
                                            Host.CanselForm();
                                            Thread.Sleep(5000);
                                            gameObject.Use();
                                            Thread.Sleep(2000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            Host.Wait(10000);
                                        }

                                        if (string.Compare(Host.GetBotLogin(), "zawww", true) == 0)
                                        {
                                            if (index != 0)
                                                index = index - 1;
                                            continue;
                                        }
                                        return;
                                    }

                                    if (dungeon.MapId == 1643 && Host.MapID == 1642)
                                    {
                                        if (Host.Me.Distance(-2174.64, 766.01, 20.92) > 20)
                                            Host.CommonModule.MoveTo(-2138.24, 797.57, 5.93);

                                        Host.CommonModule.MyUnmount();
                                        Host.CommonModule.MoveTo(new Vector3F(-2174.64, 766.01, 20.92), 1, 0.5, false);

                                        var npc = Host.GetNpcById(135690);
                                        if (npc != null)
                                        {
                                            /*  Host.OpenDialog(npc);
                                              Thread.Sleep(1000);*/
                                            switch (Host.GetBotLogin())
                                            {
                                                case "deathstar":
                                                    {
                                                        Host.MyDialog(npc, 8);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        Host.MyDialog(npc, 6);
                                                    }
                                                    break;
                                            }

                                            Thread.Sleep(5000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            Thread.Sleep(10000);
                                            /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                             {
                                                 Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                                             }*/
                                        }
                                        if (string.Compare(Host.GetBotLogin(), "zawww", true) == 0)
                                        {
                                            if (index != 0)
                                                index = index - 1;
                                            continue;
                                        }
                                        return;
                                    }

                                    if ((dungeon.MapId == 1642 || dungeon.MapId == 1643 || dungeon.MapId == 1718) && Host.MapID == 1)
                                    {
                                        var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1432.93, -4518.37, 18.40), Host.Me.Location);
                                        Host.log(path.Count + "  Путь");
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F);
                                        }

                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 323855)
                                                continue;
                                            Host.CommonModule.MoveTo(gameObject);
                                            Host.CommonModule.MyUnmount();
                                            Host.CanselForm();
                                            Thread.Sleep(5000);
                                            gameObject.Use();
                                            Thread.Sleep(2000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            Host.Wait(10000);
                                        }
                                        if (string.Compare(Host.GetBotLogin(), "zawww", true) == 0)
                                        {
                                            Host.log("Повторяю действие");
                                            if (index != 0)
                                                index = index - 1;
                                            continue;
                                        }
                                        return;
                                    }

                                    if (dungeon.MapId == 1718 && Host.MapID == 1642)
                                    {
                                        if (!Host.CommonModule.MoveTo(-1132.67, 772.36, 433.32))
                                            return;
                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 327526)
                                                continue;
                                            Host.CommonModule.MoveTo(gameObject);
                                            Host.CommonModule.MyUnmount();
                                            Host.CanselForm();
                                            Thread.Sleep(5000);
                                            gameObject.Use();
                                            Thread.Sleep(2000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            Host.Wait(10000);
                                        }
                                    }


                                    if (index != 0)
                                        index = index - 1;
                                }

                                if (dungeon.AreaId != Host.Area.Id || Host.Me.Distance(dungeon.Loc) > 600)
                                {
                                    if (Host.Me.Distance(dungeon.Loc) < 200)
                                    {

                                    }
                                    else
                                    {
                                        switch (Host.Area.Id)
                                        {
                                            case 8721:
                                                {

                                                }
                                                break;
                                            default:
                                                {
                                                    Host.log("Точка в другой зоне ", Host.LogLvl.Error);
                                                    if (Host.Me.Distance(-217.74, -1554.51, 2.73) < 100)
                                                    {
                                                        Host.MyUseTaxi(dungeon.AreaId, new Vector3F(1397.26, 2043.43, 264.55));
                                                        return;
                                                    }
                                                    Host.MyUseTaxi(dungeon.AreaId, dungeon.Loc);
                                                    Host.MyUseTaxi(dungeon.AreaId, dungeon.Loc);
                                                    if (Host.GetBotLogin() != "deathstar")
                                                        if (index != 0)
                                                            index = index - 1;
                                                }
                                                break;
                                        }

                                    }


                                }



                                /* if (!Host.IsInsideNavMesh(dungeon.Loc) && Host.Me.Distance(dungeon.Loc) < 50)
                                 {
                                     Host.log(index + ")Точка вне мешей. пропускаю " + dungeon.Loc);
                                     continue;
                                 }*/

                                if (!dungeon.Attack)
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                else
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;

                                if (Host.Me.Distance(dungeon.Loc) > 150)
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                //  Host.log("Бегу на точку " + dungeon.Loc + "  " + "  " + dungeon.Attack + " дист:" + Host.Me.Distance(dungeon.Loc));
                                while (Host.Me.Distance2D(dungeon.Loc) > 3 && Host.IsAlive(Host.Me) && Host.MainForm.On && !Host.Me.IsDeadGhost)
                                {
                                    Host.log("Бегу на точку  2     " + dungeon.Loc + "  " + "  " + dungeon.Attack + " дист:" + Host.Me.Distance(dungeon.Loc) + Host.IsAlive(Host.Me) + " " + Host.Me.IsDeadGhost);
                                    Thread.Sleep(10);
                                    if (!Host.MainForm.On)
                                        break;


                                    if (Host.GameState != EGameState.Ingame)
                                    {
                                        Thread.Sleep(100);
                                        continue;
                                    }
                                    while (Host.FarmModule.BestMob != null || Host.FarmModule.BestProp != null)
                                        Thread.Sleep(100);
                                    while (Host.SpellManager.IsCasting)
                                        Thread.Sleep(100);

                                    if (dungeon.Attack)
                                    {
                                        while (Host.CommonModule.IsMoveSuspended())
                                            Thread.Sleep(100);
                                    }
                                    else
                                    {

                                        if (Host.FarmModule.farmState != FarmState.Disabled)
                                        {
                                            while (Host.CommonModule.IsMoveSuspended())
                                                Thread.Sleep(100);
                                            if (Host.GetAgroCreatures().Count == 0)
                                                Host.FarmModule.farmState = FarmState.Disabled;
                                        }


                                    }



                                    if (NeedFindBestPoint)
                                    {
                                        break;
                                    }
                                    if (Host.CommonModule.IsMoveSuspended())
                                        continue;
                                    if (Host.CharacterSettings.ForceMoveScriptEnable)
                                    {
                                        if (Host.Me.Distance(dungeon.Loc) < Host.CharacterSettings.ForceMoveScriptDist)
                                        {
                                            Host.CommonModule.MySitMount(dungeon.Loc);
                                            Host.log("Вошел в ForceComeTo");


                                            if (!Host.CommonModule.ForceMoveTo2(dungeon.Loc, 1))
                                            {
                                                Host.CommonModule.MyUseGps(dungeon.Loc);
                                                /* if (Host.GetLastError() != ELastError.Movement_MoveCanceled)
                                                     Host.log("Не смог добежать до точки " + Host.GetLastError() + "  " + Host.Me.Distance(dungeon.Loc), Host.LogLvl.Error);*/
                                            }
                                            else
                                            {
                                                Host.CommonModule._moveFailCount = 0;
                                                break;
                                            }
                                            Host.log("Вышел из ForceComeTo");
                                            if (Host.Me.Distance2D(dungeon.Loc) > 4 && Host.MainForm.On && !Host.CommonModule.IsMoveSuspended())
                                            {
                                                Host.CommonModule.MoveTo(dungeon.Loc);
                                            }
                                            if (Host.CommonModule._moveFailCount > 1)
                                                break;
                                            continue;
                                        }


                                    }
                                    //   host.log("тест 2");

                                    if (Host.MainForm.On && !Host.CommonModule.IsMoveSuspended())
                                    {
                                        if (Host.CommonModule.MoveTo(dungeon.Loc, 1, Host.Me.RunSpeed / 5.0))
                                            break;
                                    }


                                    if (Host.CommonModule._moveFailCount > 1)
                                    {
                                        if (Host.Me.Distance(dungeon.Loc) < 20)
                                            break;
                                    }
                                    if (Host.CommonModule._moveFailCount > 20)
                                        break;
                                }
                            }
                            break;

                        case "Отбиться от мобов":
                            {
                                /*  if (host.WorldMapType == EWorldMapType.Dungeon && host.Me.Pet == null)
                                      host.CommonModule.SummonPet();*/
                                //   host.log("Отбиваюсь от мобов");
                                Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                Thread.Sleep(100);
                                Host.CommonModule.SuspendMove();
                                //  var timerfix = 0;
                                while (Host.GetAgroCreatures().Count > 0 && Host.IsAlive(Host.Me) && Host.FarmModule.farmState == FarmState.AttackOnlyAgro)
                                {
                                    Thread.Sleep(100);
                                    /*  timerfix++;
                                      if (timerfix > 1500)
                                          host.FarmModule.farmState = FarmState.Disabled;*/
                                }
                                Host.FarmModule.BestMob = null;
                                if (Host.CharacterSettings.LogScriptAction)
                                    Host.log("Отбился от мобов");
                                //   timerfix = 0;
                                while (Host.CommonModule.IsMoveSuspended() && Host.IsAlive(Host.Me))
                                {
                                    Thread.Sleep(100);
                                    /*  timerfix++;
                                      if (timerfix > 1500)
                                          break;*/
                                }
                                Host.CommonModule.ResumeMove();
                                if (Host.CharacterSettings.LogScriptAction)
                                    Host.log("Собрал лут");




                            }
                            break;
                        case "Остановить скрипт":
                            {
                                Host.MainForm.On = false;
                                return;
                            }
                        case "Фарм пропов":
                            {
                                /*  if (host.WorldMapType == EWorldMapType.Dungeon && host.Me.Pet == null)
                                      host.CommonModule.SummonPet();
                                  //  host.log("Фарм пропов " + dungeon.PropId);
                                  host.CommonModule.MoveTo(dungeon.Loc);
                                  var zone = new RoundZone(dungeon.Loc.X, dungeon.Loc.Y, 40);
                                  var farmproplist = new List<int> { dungeon.PropId };
                                  host.FarmModule.SetFarmProps(zone, farmproplist);
                                  while (host.MainForm.On
                                         && host.IsAlive(host.Me)
                                         && host.CharacterSettings.Mode == "Данж.(п)"
                                         && host.FarmModule.readyToActions
                                         && host.FarmModule.farmState == FarmState.FarmProps)
                                  {
                                      Thread.Sleep(100); //213032
                                      if (!host.CommonModule.InFight())
                                          if (!host.IsPropExitis(dungeon.PropId))
                                              host.FarmModule.StopFarm();
                                  }

                                  host.FarmModule.StopFarm();
                                  host.CommonModule.SuspendMove();
                                  Thread.Sleep(100);
                                  while (host.CommonModule.IsMoveSuspended() && host.IsAlive(host.Me))
                                      Thread.Sleep(100);
                                  //   host.log("Пропов с Id " + dungeon.PropId + " нет");*/
                            }
                            break;

                        case "Фарм мобов":
                            {
                                /*  if (host.WorldMapType == EWorldMapType.Dungeon && host.Me.Pet == null)
                                      host.CommonModule.SummonPet();
                                  host.log("Фарм мобов до смерти " + dungeon.MobId);
                                  host.CommonModule.MoveTo(dungeon.Loc);
                                  var zone = new RoundZone(dungeon.Loc.X, dungeon.Loc.Y, 40);
                                  var farmmoblist = new List<int> { dungeon.MobId };
                                  host.FarmModule.SetFarmMobs(zone, farmmoblist);
                                  while (host.MainForm.On
                                         && host.IsAlive(host.Me)
                                         && host.CharacterSettings.Mode == "Данж.(п)"
                                         && host.FarmModule.readyToActions
                                         && host.FarmModule.farmState == FarmState.FarmMobs)
                                  {
                                      Thread.Sleep(100);
                                      if (!host.CommonModule.InFight())
                                          if (!host.IsBossAlive(dungeon.MobId))
                                              host.FarmModule.StopFarm();
                                  }

                                  host.FarmModule.StopFarm();
                                  host.CommonModule.SuspendMove();
                                  Thread.Sleep(100);
                                  while (host.CommonModule.IsMoveSuspended() && host.IsAlive(host.Me))
                                      Thread.Sleep(100);
                                  host.log("Убил " + dungeon.MobId);*/
                            }
                            break;
                        case "Эквип на ауке":
                            {

                                // host.log("Тест  EquipBestArmorAndWeapon");
                                if (Host.Me.Team == ETeam.Horde)
                                    if (Host.Area.Id != 1637)
                                    {
                                        Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                if (Host.Me.Team == ETeam.Alliance)
                                    if (Host.Area.Id != 1519)
                                    {
                                        Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }
                                var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1635, -4445, 17), Host.Me.Location);
                                if (Host.Me.Team == ETeam.Horde)
                                    if (Host.Me.Distance(1654.84, -4350.49, 26.35) < 50 || Host.Me.Distance(1573.36, -4437.08, 16.05) < 50)
                                    {
                                        if (Host.CharacterSettings.AlternateAuk)
                                        {
                                            path = Host.CommonModule.GpsBase.GetPath(new Vector3F(2065.83, -4668.45, 32.52), Host.Me.Location);
                                        }
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                        }
                                    }

                                if (Host.Me.Team == ETeam.Alliance)
                                {
                                    path = Host.CommonModule.GpsBase.GetPath(new Vector3F(-8816.10, 660.36, 98.01), Host.Me.Location);
                                    foreach (var vector3F in path)
                                    {
                                        Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                        Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                    }
                                }

                                //Проверка НПС
                                Unit npc = null;
                                foreach (var entity in Host.GetEntities<Unit>())
                                {
                                    if (!entity.IsAuctioner)
                                        continue;
                                    if (entity.Id == 44868)
                                        continue;
                                    if (entity.Id == 44865)
                                        continue;
                                    if (entity.Id == 44866)
                                        npc = entity;
                                    if (entity.Id == 8719)
                                        npc = entity;
                                    if (entity.Id == 46640)
                                        npc = entity;
                                }

                                if (npc == null)
                                {
                                    Host.log("Нет НПС для аука", Host.LogLvl.Error);
                                    Thread.Sleep(5000);
                                    return;
                                }
                                Host.log("Выбран " + npc.Name + " " + npc.Id);
                                Host.CommonModule.MoveTo(npc, 3);
                                Host.MyCheckIsMovingIsCasting();
                                if (!Host.OpenAuction(npc))
                                    Host.log("Не смог открыть диалог для аука " + Host.GetLastError(), Host.LogLvl.Error);
                                else
                                {
                                    Host.log("Открыл диалог для аука", Host.LogLvl.Ok);
                                }
                                Thread.Sleep(3000);
                                foreach (var characterSettingsEquipAuc in Host.CharacterSettings.EquipAucs)
                                {
                                    if (IsFindItem(characterSettingsEquipAuc.Name, characterSettingsEquipAuc.Stat1, characterSettingsEquipAuc.Stat2))
                                    {
                                        Host.log("Нашел в инвентаре " + characterSettingsEquipAuc.Name + " " + characterSettingsEquipAuc.Slot);
                                        equipCells.Add(characterSettingsEquipAuc.Slot);
                                    }

                                }

                                foreach (var characterSettingsEquipAuc in Host.CharacterSettings.EquipAucs)
                                {
                                    if (equipCells.Contains(characterSettingsEquipAuc.Slot))
                                        continue;
                                    var req = new AuctionSearchRequest();
                                    req.MaxReturnItems = 50;
                                    req.SearchText = characterSettingsEquipAuc.Name;
                                    req.ExactMatch = true;
                                    req.SortType = EAuctionSortType.PriceAsc;
                                    Host.log("Ищу на ауке " + characterSettingsEquipAuc.Name + " " + characterSettingsEquipAuc.Slot);
                                    var aucItems = Host.GetAuctionBuyList(req);
                                    if (aucItems == null || aucItems.Count == 0)
                                        continue;
                                    foreach (var aucItem in aucItems)
                                    {
                                        if (aucItem.BuyoutPrice == 0)
                                            continue;
                                        if (aucItem.BuyoutPrice > characterSettingsEquipAuc.MaxPrice)
                                        {
                                            Host.log("Цена1: " + aucItem.ItemId + " " + aucItem.BuyoutPrice);
                                            Host.log("Цена2: " + aucItem.ItemId + " " + characterSettingsEquipAuc.MaxPrice);
                                            continue;
                                        }

                                        if (characterSettingsEquipAuc.Level != 0)
                                            if (characterSettingsEquipAuc.Level != aucItem.ItemLevel)
                                            {
                                                Host.log("Не подходит уровень");
                                                continue;
                                            }


                                        if (characterSettingsEquipAuc.Stat1 != 0)
                                        {
                                            if (aucItem.ItemStatType == null || aucItem.ItemStatType.Count == 0)
                                            {
                                                Host.log("Нет Информации о стате " + aucItem.ItemStatType?.Count);
                                                continue;
                                            }

                                            if (!aucItem.ItemStatType.Contains((EItemModType)characterSettingsEquipAuc.Stat1))
                                            {
                                                Host.log("Нет стата " + characterSettingsEquipAuc.Stat1);
                                                continue;
                                            }
                                        }
                                        if (characterSettingsEquipAuc.Stat2 != 0)
                                        {
                                            if (aucItem.ItemStatType == null || aucItem.ItemStatType.Count == 0)
                                            {
                                                Host.log("Нет Информации о стате " + aucItem.ItemStatType?.Count);
                                                continue;
                                            }
                                            if (!aucItem.ItemStatType.Contains((EItemModType)characterSettingsEquipAuc.Stat2))
                                            {
                                                Host.log("Нет стата " + characterSettingsEquipAuc.Stat2);
                                                continue;
                                            }
                                        }

                                        Host.log("Покупаю " + characterSettingsEquipAuc.Name + "[" + aucItem.ItemId + "]   " + "  " + aucItem.BuyoutPrice + " " + aucItem.ItemLevel);
                                        equipCells.Add(characterSettingsEquipAuc.Slot);
                                        Thread.Sleep(2000);
                                        var result = aucItem.MakeBuyout();
                                        if (result == EAuctionHouseError.Ok)
                                        {
                                            Host.log("Выкупил ");
                                            equipCells.Add(characterSettingsEquipAuc.Slot);
                                            Thread.Sleep(5000);
                                        }
                                        else
                                        {
                                            Host.log("Не смог выкупить " + result + " " + Host.GetLastError(), Host.LogLvl.Error);
                                            Thread.Sleep(10000);
                                        }
                                        break;
                                    }
                                }
                                Host.log("Все покупки завершены ");
                                Host.MainForm.On = false;
                                return;
                            }
                        case "Токен"://В плагин квестер добавить в раздел скрипта строчку с проверкой:
                            {
                                if (Host.MyGetItem(122284) != null)
                                {
                                    if (!Host.ActivateWowTokenToBalance())
                                        Host.log(
                                            "Не смог активировать токен на баланс " + Host.GetLastError(), Host.LogLvl.Error);
                                    else
                                        Host.log("Активировал токен на баланс ", Host.LogLvl.Ok);

                                    Thread.Sleep(10000);
                                    return;
                                }

                                if (notGoldToken)
                                {
                                    Host.log("Не хватает голды на токен или баланс > 90");
                                    Thread.Sleep(10000);
                                    continue;
                                }
                                var balance = Host.GetBattleNetBalance();
                                Host.log("Баланс " + balance + " $");
                                if (balance < 930000)//если баланс близард <90$ <Core.GetBattleNetBalance()> (в центах)
                                {
                                    if (Host.Me.Team == ETeam.Horde)
                                        if (Host.Area.Id != 1637)
                                        {
                                            Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }

                                    if (Host.Me.Team == ETeam.Alliance)
                                        if (Host.Area.Id != 1519)
                                        {
                                            Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }
                                    var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1635, -4445, 17), Host.Me.Location);
                                    if (Host.Me.Team == ETeam.Horde)
                                        if (Host.Me.Distance(1654.84, -4350.49, 26.35) < 50 || Host.Me.Distance(1573.36, -4437.08, 16.05) < 50)
                                        {
                                            if (Host.CharacterSettings.AlternateAuk)
                                            {
                                                path = Host.CommonModule.GpsBase.GetPath(new Vector3F(2065.83, -4668.45, 32.52), Host.Me.Location);
                                            }
                                            foreach (var vector3F in path)
                                            {
                                                Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                                Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                            }
                                        }

                                    if (Host.Me.Team == ETeam.Alliance)
                                    {
                                        path = Host.CommonModule.GpsBase.GetPath(new Vector3F(-8816.10, 660.36, 98.01), Host.Me.Location);
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                        }
                                    }

                                    //Проверка НПС
                                    Unit npc = null;
                                    foreach (var entity in Host.GetEntities<Unit>())
                                    {
                                        if (!entity.IsAuctioner)
                                            continue;
                                        if (entity.Id == 44868)
                                            continue;
                                        if (entity.Id == 44865)
                                            continue;
                                        if (entity.Id == 44866)
                                            npc = entity;
                                        if (entity.Id == 8719)
                                            npc = entity;
                                        if (entity.Id == 46640)
                                            npc = entity;
                                    }

                                    if (npc == null)
                                    {
                                        Host.log("Нет НПС для аука", Host.LogLvl.Error);
                                        Thread.Sleep(5000);
                                        return;
                                    }
                                    Host.log("Выбран " + npc.Name + " " + npc.Id);
                                    Host.CommonModule.MoveTo(npc, 3);
                                    Host.MyCheckIsMovingIsCasting();
                                    if (!Host.OpenAuction(npc))
                                        Host.log("Не смог открыть диалог для аука " + Host.GetLastError(), Host.LogLvl.Error);
                                    else
                                    {
                                        Host.log("Открыл диалог для аука", Host.LogLvl.Ok);
                                    }
                                    Thread.Sleep(3000);
                                    var priceToken = Host.GetAuctionWowTokenPrice();
                                    Host.log("Money " + Host.Me.Money + " GetAuctionWowTokenPrice: " + priceToken);
                                    if (Host.Me.Money > priceToken + 20000000)//если у меня голды больше N суммы (N определить подойдя на аук и проверив цену токена (если такое возможно), или если не получится, то в ручную указывать)
                                    {
                                        if (!Host.BuyAuctionWowToken())//купить вов токен <ulong Core.GetAuctionWowTokenPrice()>      <bool Core.BuyAuctionWowToken()>
                                        {
                                            Host.log("Не смог купить токен " + Host.GetLastError());
                                            Thread.Sleep(10000);
                                            return;
                                        }
                                        else
                                        {
                                            Thread.Sleep(60000);//подождать минуту (токен бывает идет долго и не сразу появляется на почте)
                                            Host.Mail();


                                            if (Host.MyGetItem(122284) != null)
                                            {
                                                TimeSpan time = Host.GetCurrentAccount().Premium.ToUniversalTime() - DateTime.UtcNow;
                                                Host.log("Осталось " + time.Days + " дней " + time.Hours + " часов");
                                                if (time.Days < 2 && !BuySubs)
                                                {

                                                    if (!Host.ActivateWowTokenToSubscription())
                                                        Host.log(
                                                            "Не смог активировать токен на подписку " + Host.GetLastError(), Host.LogLvl.Error);
                                                    else
                                                        Host.log("Активировал токен на подписку ", Host.LogLvl.Ok);
                                                    BuySubs = true;
                                                }
                                                else
                                                {

                                                    if (!Host.ActivateWowTokenToBalance())
                                                        Host.log(
                                                            "Не смог активировать токен на баланс " + Host.GetLastError(), Host.LogLvl.Error);
                                                    else
                                                        Host.log("Активировал токен на баланс ", Host.LogLvl.Ok);
                                                }



                                                Thread.Sleep(10000);
                                            }
                                            // пойти к почте забрать токен
                                            // удалить оставшееся письмо после токена<Mail.Delete>
                                            // активировать токен на баланс близард < Core.ActivateWowTokenToBalance() >
                                            // если получится потом можно еще сделать
                                            // если время подписки меньше N времени
                                            // сделать все тоже самое, но только в конце активировать токен в подписку<Core.ActivateWowTokenToSubscription()> 
                                        }

                                    }
                                    else
                                    {
                                        Host.log("Не хватает голды на токен");
                                        notGoldToken = true;
                                    }


                                }
                                else
                                {
                                    File.AppendAllText(Host.AssemblyDirectory + "\\Token.txt",
                                        DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + @" " + Host.GetCurrentAccount().Login + "  " + Host.GetCurrentAccount().Name + Environment.NewLine);

                                    notGoldToken = true;
                                }



                            }
                            break;



                        case "Положить нитки на склад":
                            {
                                foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i)))
                                {
                                    if (!entity.IsBanker)
                                        continue;
                                    Host.CommonModule.MoveTo(entity, 4);
                                    Host.MyCheckIsMovingIsCasting();
                                    Thread.Sleep(2000);
                                    if (!Host.OpenBank(entity))
                                        Host.log("Не смог открыть банк " + Host.GetLastError(), Host.LogLvl.Error);
                                    Thread.Sleep(2000);
                                    foreach (var item in Host.ItemManager.GetItems())
                                    {
                                        if (item.Id != 159959)
                                            continue;
                                        if (item.MoveToBank())
                                        {
                                            Host.log("Положил в банк", Host.LogLvl.Ok);
                                        }
                                        else
                                        {
                                            Host.log("Не смог положить в банк " + Host.GetLastError(), Host.LogLvl.Error);
                                            Thread.Sleep(5000);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            break;

                    }
                }
                double tempGold2 = Host.Me.Money;
                var gold = tempGold2 / 10000;
                // var gold = Host.Me.Money / 10000;
                var formattedTimeSpan = $"{scriptStopwatch.Elapsed.Hours:D2} hr, {scriptStopwatch.Elapsed.Minutes:D2} min, {scriptStopwatch.Elapsed.Seconds:D2} sec";
                long invgold = 0;

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        //   log(item.Id + "  "+ item.Name);
                        //    invgold = invgold + item.GetSellPrice() * item.Count;
                    }
                }

                invgold = invgold - startinvgold;
                double doubleGold = Convert.ToDouble(invgold) / 10000;
                // Host.log("Тест " + scriptStopwatch.Elapsed.Minutes + " " + scriptStopwatch.Elapsed.Seconds);
                /* if (Host.GetBotLogin() == "Daredevi1")
                 {*/
                if (Host.CharacterSettings.WaitSixMin)
                    if (scriptStopwatch.Elapsed.Minutes > 2 && scriptStopwatch.Elapsed.Minutes < 6)
                    {
                        while (scriptStopwatch.Elapsed.Minutes < 6 && Host.MainForm.On)
                        {
                            Thread.Sleep(10000);
                            Host.log("Ожидаю до 6 минут " + scriptStopwatch.Elapsed.Minutes + " " + scriptStopwatch.Elapsed.Seconds + " " + scriptStopwatch.Elapsed.TotalSeconds);
                        }


                    }
                //  }

                Host.log(
                    "Скрипт выполнен за " + formattedTimeSpan + ". Мобов убито: " + (Host.KillMobsCount - mobsStart) +
                    " Золота получено: " + Math.Round(gold - goldStart, 2) + "г. + " + Math.Round(doubleGold, 2) +
                    " г. Всего: " + Math.Round((gold - goldStart) + doubleGold, 2) + " г. " + Host.Me.Name,
                    Host.LogLvl.Ok);

                Host.Log(
                    "Скрипт выполнен за " + formattedTimeSpan + ". Мобов убито: " + (Host.KillMobsCount - mobsStart) +
                    " Золота получено: " + Math.Round(gold - goldStart, 2) + "г. + " + Math.Round(doubleGold, 2) +
                    " г. Всего: " + Math.Round((gold - goldStart) + doubleGold, 2) + " г. " + Host.Me.Name
                    , "Статистика Данжа");


                //  host.Log("После прохода стало " + invgold + "(" + (invgold - startinvgold ) + ")", "Статистика Данжа");
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                Host.log(err.ToString());
                Host.Log(err.ToString(), "Ошибки GetCore");
            }
        }

        private bool BuySubs;

        private List<EEquipmentSlot> equipCells = new List<EEquipmentSlot>();

        private bool IsFindItem(string name, int stat1, int stat2)
        {
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                    item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Equipment)
                    if (name.Contains(item.Name))
                    {
                        var find1 = true;
                        var find2 = true;
                        if (stat1 != 0)
                        {
                            if (!item.ItemStatType.Contains((EItemModType)stat1))
                                find1 = false;
                        }
                        if (stat2 != 0)
                        {
                            if (!item.ItemStatType.Contains((EItemModType)stat2))
                                find2 = false;
                        }

                        if (!find2 || !find1)
                            return false;
                        Host.log("Нашел в инвентаре " + item.Name + " " + item.InventoryType);
                        return true;
                    }

            }

            return false;
        }

        private bool notGoldToken;

        private Host.MyNpcLoc MyGetLocNpcById(uint id)
        {
            foreach (var myNpcLoc in Host.MyNpcLocss.NpcLocs)
            {
                if (myNpcLoc.Id == id)
                {
                    return myNpcLoc;
                }
            }
            return null;
        }

        private void MyMoveFromNpc(Unit npc, int dist = 7)
        {
            var safePoint = new List<Vector3F>();


            var xc = Host.Me.Location.X;
            var yc = Host.Me.Location.Y;

            var radius = dist;
            const double a = Math.PI / 16;
            double u = 0;
            for (var i = 0; i < 32; i++)
            {
                var x1 = xc + radius * Math.Cos(u);
                var y1 = yc + radius * Math.Sin(u);
                // log(" " + i + " x:" + x + " y:" + y);
                u = u + a;
                var z1 = Host.GetNavMeshHeight(new Vector3F(x1, y1, 0));
                if (Host.IsInsideNavMesh(new Vector3F((float)x1, (float)y1, (float)z1)))
                    safePoint.Add(new Vector3F((float)x1, (float)y1, (float)z1));
            }
            Host.log("Точек " + safePoint.Count);
            if (safePoint.Count > 0)
            {
                Vector3F bestPoint = new Vector3F();
                double bestDist = 0;
                foreach (var vector3F in safePoint)
                {
                    if (npc.Distance(vector3F) > bestDist)
                    {
                        bestPoint = vector3F;
                        bestDist = npc.Distance(vector3F);
                    }
                }

                Host.ComeTo(bestPoint);
            }
        }


        private int fixBadDialog;

        private bool MyApplyQuest(Entity npc, uint id)
        {
            try
            {
                if (id == 47103)
                {
                    if (Host.Me.Location.Z < 440)
                    {
                        Host.CommonModule.MoveTo(Host.Me.Location.X + 2, Host.Me.Location.Y + 2, Host.Me.Location.Z);
                    }
                }

                switch (npc.Id)
                {
                    case 122688:
                        {
                            if (!Host.CommonModule.MoveTo(2666.69, 1364.10, 11.17))
                                return false;
                        }
                        break;

                    case 271706:
                        {
                            if (!Host.CommonModule.MoveTo(npc, 4, 4))
                                return false;
                        }
                        break;
                    case 150206:
                        break;

                    default:
                        {
                            if (!Host.CommonModule.MoveTo(npc.Location, 2, 2))
                                return false;
                        }
                        break;
                }



                if (npc.Guid == Host.CurrentInteractionGuid)
                {
                    Host.log("Диалог уже открыт " + Host.GetNpcDialogs().Count + "  " + fixBadDialog + "/5", Host.LogLvl.Ok);
                    if (Host.GetNpcDialogs().Count == 0)
                    {
                        fixBadDialog++;
                        if (fixBadDialog >= 10)
                        {
                            Host.log("Перезапускаю окно, нет диалогов");
                            Host.TerminateGameClient();
                            return false;
                        }
                    }
                }
                else
                {
                    fixBadDialog = 0;
                    Thread.Sleep(500);
                    if (!Host.OpenDialog(npc))
                    {
                        Host.log("Не смог начать диалог для начала квеста с " + npc.Name + " " + Host.GetLastError() + " CurrentInteractionGuid:" + Host.CurrentInteractionGuid + " IsAlive:" + Host.Me.IsAlive, Host.LogLvl.Error);
                        Host.log(npc.Guid + "   npc.Guid");
                        Host.log(Host.CurrentInteractionGuid + " CurrentInteractionGuid");
                        Host.CommonModule.MyUnmount();
                        foreach (var entity in Host.GetEntities())
                        {
                            if (entity.Guid == Host.CurrentInteractionGuid)
                            {
                                Host.log("Имя: " + entity.Name);
                                Host.CommonModule.MoveTo(entity);
                            }
                        }

                        if (Host.GetLastError() == ELastError.ActionNotAllowed)
                        {
                            MyMoveFromNpc(npc as Unit);
                            return false;
                        }
                    }
                    else
                    {
                        Host.log("Открыл диалог ");
                    }

                }


                Thread.Sleep(500);
                /*  if (id == 0)
                  {*/
                foreach (var gossipOptionsData in Host.GetNpcDialogs())
                {
                    Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + " " + gossipOptionsData.Confirm + " " + gossipOptionsData.OptionCost + " " + gossipOptionsData.OptionFlags + " " + gossipOptionsData.OptionNPC);
                }

                var isQuestFound = false;
                foreach (var gossipQuestTextData in Host.GetNpcQuestDialogs())
                {
                    if (gossipQuestTextData.QuestID == id)
                    {
                        isQuestFound = true;
                        // Host.log("Нашел квест " + id);
                    }

                    Host.log("QuestID: " + gossipQuestTextData.QuestID
                        + "  QuestTitle:" + gossipQuestTextData.QuestTitle
                        + " QuestType:" + gossipQuestTextData.QuestType
                        + " QuestLevel:" + gossipQuestTextData.QuestLevel
                        + " Repeatable:" + gossipQuestTextData.Repeatable
                        + " QuestMaxScalingLevel:" + gossipQuestTextData.QuestMaxScalingLevel);
                    foreach (var questFlag in gossipQuestTextData.QuestFlags)
                    {
                        Host.log("questFlag: " + questFlag);
                    }
                }



                /*   if (id == 46927 && !isQuestFound)
                   {
                       id = 46928;
                       isQuestFound = true;
                   }*/

                if (id == 51916 && !isQuestFound)
                {

                    id = 52451;
                    isQuestFound = true;
                }

                if (!isQuestFound)
                {
                    foreach (var gossipQuestTextData in Host.GetNpcQuestDialogs())
                    {

                        id = gossipQuestTextData.QuestID;
                        isQuestFound = true;
                        // Host.log("Нашел квест " + id);                  
                    }
                }
                //  }

                switch (id)
                {
                    case 47514:
                    case 47512:
                    case 47513:
                        {
                            if (!MyStartAdventure(id, npc))
                                return false;
                        }
                        break;
                    case 51753:
                        {
                            Host.PrepareCompleteQuest(id);
                            if (!Host.CompleteQuest(id))
                                Host.log("Не смог завершить квест" + Host.GetLastError(), Host.LogLvl.Error);
                            return false;
                        }

                    default:
                        {
                            if (!isQuestFound)
                            {
                                if (id == 51975 || id == 51987 || id == 51753)
                                {
                                    Host.PrepareCompleteQuest(id);
                                    if (!Host.CompleteQuest(id))
                                        Host.log("Не смог завершить квест" + Host.GetLastError(), Host.LogLvl.Error);
                                    // Host.log("Надо апи");
                                    //Thread.Sleep(60000);
                                    return false;
                                }
                                Host.log("Не нашел квест у НПС " + id + " " + isQuestFound, Host.LogLvl.Error);

                                Host.SendKeyPress(0x1b);
                                if (id == 50794 || id == 50933 || id == 50538 || id == 51870 || id == 49768)
                                {
                                    if (questFix > 3)
                                    {
                                        questFix = 0;
                                        Host.log("Добавляю в игнор");
                                        Host.QuestStates.QuestState.Add(id);
                                        Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                                    }
                                    questFix++;
                                }

                                //MyMoveFromNpc(npc as Unit);
                                Thread.Sleep(1000);
                                return false;
                            }
                            questFix = 0;
                            var quest = Host.GetQuest(id);
                            if (quest == null)
                                if (!Host.StartQuest(id))
                                {
                                    Host.log("Не смог начать квест " + id + " Всего диалогов у НПС " + Host.GetNpcQuestDialogs().Count + "   " + Host.GetLastError(), Host.LogLvl.Error);
                                    Host.SendKeyPress(0x1b);
                                    if (Host.GetLastError() == ELastError.InvalidParam)
                                    {
                                        MyMoveFromNpc(npc as Unit);
                                    }
                                    else
                                    {

                                    }
                                    Thread.Sleep(500);
                                    return false;
                                }
                        }
                        break;
                }

                if (id == 51770)
                    Host.Wait(45000);

                if (id == 52131)
                    Host.Wait(3000);
                Thread.Sleep(500);
                return true;
            }
            catch (Exception e)
            {
                Host.log(e + "");
                return false;
            }



        }
        int questFix = 0;
        public bool MyStartAdventure(uint id, Entity npc)
        {
            var quest = Host.GetQuest(id);
            if (quest == null)
                if (!Host.StartAdventureJournalQuest(id))
                {
                    Host.log("Не смог начать квест " + id + " Всего диалогов у НПС " + Host.GetNpcQuestDialogs().Count + "   " + Host.GetLastError(), Host.LogLvl.Error);
                    Host.SendKeyPress(0x1b);
                    if (Host.GetLastError() == ELastError.InvalidParam)
                    {
                        MyMoveFromNpc(npc as Unit);
                    }
                    else
                    {

                    }
                    Thread.Sleep(500);
                    return false;
                }

            return true;
        }



        public bool MyComliteQuest(Quest quest)
        {
            try
            {
                var id = quest.Id;
                if (id == 55053)
                {
                    if (Host.MyAllItemsRepair())
                    {
                        NeedActionNpcRepair = true;
                        return false;
                    }
                }


                if (id == 56030 && Host.MapID == 1643)
                {
                    if (!Host.CommonModule.MoveTo(-217.70, -1528.19, 1.44))
                        return false;
                    var npc2 = Host.GetNpcById(139524);
                    Host.MyDialog(npc2, 0);
                    Thread.Sleep(5000);
                    return false;
                }

                if (id == 52746)
                {
                    Host.PrepareCompleteQuest(id);
                    if (!Host.CompleteQuest(id, 0))
                    {
                        Host.log("Не смог завершить квест кампании" + id + " с выбором награды " + 0 + "  " + Host.GetLastError(), Host.LogLvl.Error);
                        Thread.Sleep(6000);
                        Host.SendKeyPress(0x1b);
                        if (Host.GetQuest(id) == null)
                        {
                            Host.QuestStates.QuestState.Add(id);

                            Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                        }
                    }
                    else
                    {
                        Host.log("Завершил квест " + id, Host.LogLvl.Ok);
                    }

                    return false;
                }

                QuestPOI questPoi = null;
                foreach (var questPois in quest.GetQuestPOI())
                {
                    Host.log(questPois.ObjectiveIndex + " ");
                    foreach (var questPoisPoint in questPois.Points)
                    {
                        Host.log(questPoisPoint.X + " " + questPoisPoint.Y);
                    }
                    if (questPois.ObjectiveIndex != -1)
                        continue;
                    if (quest.Id == 6342 && questPois.WorldMapAreaId == 42)
                        continue;
                    questPoi = questPois;
                    break;
                }

                if (quest.Id == 51796)
                {
                    if (Host.Me.Distance(1488.29, -4379.32, 65.16) < 30)
                    {
                        Host.FlyForm();
                        Host.Jump();
                        Host.ForceFlyTo(1516.22, -4406.73, 95.85);
                        Host.Jump();
                        Host.ForceFlyTo(1468.31, -4390.20, 119.69);
                        Host.Jump();
                        Host.ForceFlyTo(1427.39, -4365.66, 75.62);
                    }
                    if (Host.Me.Location.Z < 50)
                    {
                        Host.FlyForm();
                        Host.Jump();
                        Host.ForceFlyTo(1422.44, -4364.15, 25.45);
                        Host.Jump();
                        Host.ForceFlyTo(1330.01, -4379.99, 26.21);
                        Host.Jump();
                        Host.ForceFlyTo(1390.45, -4343.33, 115.40);
                        Host.Jump();
                        Host.ForceFlyTo(1419.54, -4354.76, 79.08);
                        return false;
                    }
                    Host.CanselForm();
                    Host.CommonModule.MyUnmount();
                }

                if (quest.Id == 47512)
                {
                    if (Host.Me.Location.Z < 440)
                    {
                        Host.CommonModule.MoveTo(Host.Me.Location.X + 2, Host.Me.Location.Y + 2, Host.Me.Location.Z);
                    }
                }

                if (quest.Id == 48454)
                {
                    if (Host.Me.Distance(-1859.58, 805.05, 53.78) > 30)
                        if (!Host.CommonModule.MoveTo(new Vector3F(-1859.58, 805.05, 53.78)))
                            return false;
                }


                if (id == 48757)
                    if (!Host.CommonModule.MoveTo(1263.38, -543.17, 33.40))
                        return false;

                if (id == 47186)
                    if (!Host.CommonModule.MoveTo(new Vector3F(1068.09, -486.47, 9.70)))
                        return false;

                if (id == 51243 || id == 51478)
                {
                    if (!Host.CommonModule.MoveTo(-932.67, 1006.92, 321.04))
                        return false;
                }

                /*  if(id == 0)
                  Host.MyUseTaxi(0,)*/
                if (id == 28725)
                    questPoi = null;

                var revardNpcId = quest.CompletionNpcIds[0];

                if (quest.Id == 51675)
                    revardNpcId = 138688;

                if (quest.Id == 13521)
                    revardNpcId = 194105;

                if (quest.Id == 13528)
                    revardNpcId = 194122;

                if (quest.Id == 51691)
                    revardNpcId = 138688;

                if (quest.Id == 29021)
                    revardNpcId = 4141;

                if (quest.Id == 47264)
                    revardNpcId = 121241;
                if (quest.Id == 47130)
                    revardNpcId = 121241;

                if (quest.Id == 51674)
                    revardNpcId = 138867;



                if (quest.Id == 50550)
                {
                    revardNpcId = 138519;
                    Host.Wait(25000);
                }


                //130474
                var revardNpcLoc = MyGetCoordQuestNpc(revardNpcId);

                /*   if (revardNpc != null)
               {
                   if (!Host.CommonModule.MoveTo(revardNpc.Loc, 5, 5))
                       return false;*/



                if (quest.Id == 47314)
                {
                    if (!Host.CommonModule.MoveTo(-851.29, 804.63, 324.37))
                        return false;
                    Host.Wait(40000);
                }

                if (quest.Id == 47513)
                {
                    if (Host.Me.Location.Z < 438 && Host.Me.Distance(-1121.63, 818.27, 436.12) < 5)
                        if (!Host.CommonModule.MoveTo(-1124.88, 809.86, 437.54))
                            return false;

                }


                var npc = Host.GetNpcById(revardNpcId);
                if (questPoi != null && npc == null)
                {

                    var z = Host.GetNavMeshHeight(new Vector3F(questPoi.Points[0].X, questPoi.Points[0].Y, 0));
                    if (z == 0)
                        Host.log("Z координата  = 0 " + questPoi.Points[0].X + "  " + questPoi.Points[0].Y + "  " + questPoi.MapId);
                    if (Host.MapID == 1 && Host.Area.Id == 1637)
                    {
                        Host.log("Точка на другом континенете " + questPoi.MapId + "  " + Host.MapID, Host.LogLvl.Error);
                        Thread.Sleep(1000);
                        Host.FarmModule.farmState = FarmState.Disabled;
                        if ((questPoi.MapId == 1642 || questPoi.MapId == 1643) && Host.MapID == 1)
                        {
                            var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1432.93, -4518.37, 18.40), Host.Me.Location);
                            Host.log(path.Count + "  Путь");
                            foreach (var vector3F in path)
                            {
                                Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                Host.CommonModule.ForceMoveTo2(vector3F);
                            }

                            foreach (var gameObject in Host.GetEntities<GameObject>())
                            {
                                if (gameObject.Id != 323855)
                                    continue;
                                Host.CommonModule.MoveTo(gameObject);
                                Host.CommonModule.MyUnmount();
                                Host.CanselForm();
                                Thread.Sleep(5000);
                                gameObject.Use();
                                Thread.Sleep(2000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
                                Host.Wait(10000);
                            }


                        }
                    }


                    Vector3F locRevard = new Vector3F(questPoi.Points[0].X, questPoi.Points[0].Y, (float)z);
                    if (quest.Id == 24769)
                    {
                        if (!Host.CommonModule.MoveTo(-1287.03, -5566.75, 20.93))
                            return false;
                        if (!Host.CommonModule.ForceMoveTo2(new Vector3F((float)-1295.09, (float)-5576.52, (float)24.46)))
                            return false;
                    }



                    if (quest.Id == 47314)
                    {
                        locRevard.Z = 324;
                        Host.Wait(40000);
                    }

                    if (quest.Id == 54021 || quest.Id == 54012)
                    {
                        Host.FarmModule.farmState = FarmState.Disabled;
                        locRevard.Z = -195;
                    }

                    if (quest.Id == 52139)
                    {
                        locRevard.Z = 443;
                    }

                    if (quest.Id == 46931)
                    {
                        locRevard.Z = 435;
                    }

                    if (quest.Id == 6385)
                    {
                        locRevard.Z = 104;
                    }
                    if (quest.Id == 6385)
                    {
                        locRevard.Z = 104;
                    }

                    if (quest.Id == 28730)
                    {
                        locRevard.Z = 1397;
                    }
                    if (quest.Id == 475)
                    {
                        locRevard.Z = 1318;
                    }

                    if (quest.Id == 14045)
                    {
                        locRevard = new Vector3F(-1453, -3819, 21);
                    }

                    if (quest.Id == 47445)
                    {
                        locRevard.Z = 321;
                    }

                    switch (revardNpcId)
                    {
                        case 125953:
                        case 120740:
                            {
                                Host.log("Необходимо подняться к королю");
                                if (quest.Id == 46930)
                                {
                                    if (Host.Me.Distance2D(locRevard) > 20)
                                        if (!Host.CommonModule.MoveTo(locRevard, 20, 20))
                                            return false;
                                }
                                else
                                {
                                    if (Host.Me.Location.Z < 470)
                                    {
                                        Host.log("Бегу к лифту");
                                        if (!Host.CommonModule.MoveTo(-1126.50, 851.96, 443.32))
                                            return false;
                                        while (Host.Me.Location.Z < 485)
                                        {
                                            Thread.Sleep(2000);
                                            Host.Jump();
                                        }
                                        Host.log("Выхожу из лифта");
                                        Host.Wait(10000);
                                        Host.TurnDirectly(new Vector3F(-1126.19, 842.21, 487.86));
                                        Host.SetMoveStateForClient(true);
                                        Host.MoveForward(true);
                                        Thread.Sleep(2000);
                                        Host.SetMoveStateForClient(false);
                                        Host.MoveForward(false);
                                        if (!Host.CommonModule.ForceMoveTo2(new Vector3F(-1126.19, 842.21, 487.86)))
                                            return false;
                                    }
                                    else
                                    {
                                        Host.log("Бегу к королю");
                                        if (!Host.CommonModule.MoveTo(-1129.67, 805.26, 500.14))
                                            return false;
                                    }
                                }





                            }
                            break;
                        case 122688:
                            {
                                if (!Host.CommonModule.MoveTo(2666.69, 1364.10, 11.17))
                                    return false;
                                Host.MoveTo(2666.94, 1363.68, 11.17);
                            }
                            break;
                        case 138867:
                            {
                                if (!Host.CommonModule.MoveTo(3870.30, 547.09, 134.21))
                                    return false;
                            }
                            break;

                        case 2147759835:
                            {
                                if (Host.Me.Distance2D(locRevard) > 20)
                                {
                                    if (!Host.CommonModule.MoveTo(locRevard, 20, 20))
                                        return false;
                                }
                            }
                            break;

                        default:
                            {
                                var badloc = false;
                                foreach (var vector3F in BadLoc)
                                {
                                    if (vector3F.Distance2D(locRevard) < 20)
                                        badloc = true;
                                }
                                if (!badloc)
                                    if (Host.Me.Distance2D(locRevard) > 20)
                                    {
                                        if (!Host.CommonModule.MoveTo(locRevard, 20, 20))
                                            return false;
                                        else
                                        {
                                            npc = Host.GetNpcById(revardNpcId);
                                            if (npc == null)
                                            {
                                                BadLoc.Add(locRevard);
                                            }
                                        }
                                    }


                                if (quest.Id == 54021 || quest.Id == 54012)
                                {
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                }
                            }
                            break;
                    }


                }
                else
                {
                    if (npc == null)
                    {
                        Host.log("Нет координат " + quest.GetQuestPOI().Count);
                        if (revardNpcLoc != Vector3F.Zero)

                            if (!Host.CommonModule.MoveTo(revardNpcLoc, 20, 20))
                                return false;
                    }

                }



                if (quest.Id == 53370)
                {
                    if (Host.Me.Location.Z > 30 && Host.Me.Distance2D(new Vector3F(-8374.37, 1333.36, 5.22)) < 20)
                    {
                        Host.ForceFlyTo(-8374.37, 1333.36, 5.22);
                    }
                }

                if (quest.Id == 51443)
                    Host.Wait(10000);
                if (quest.Id == 49492)
                    Host.Wait(20000);

                npc = Host.GetNpcById(revardNpcId);

                if (quest.Id == 51057 && npc == null)
                    npc = Host.GetNpcById(136309);

                if (quest.Id == 49919 && npc == null)
                    npc = Host.GetNpcById(132617);

                if (quest.Id == 49922 && npc == null)
                    npc = Host.GetNpcById(132617);



                if (npc != null)
                {
                    switch (npc.Id)
                    {
                        case 125953:
                        case 120740:
                            {
                                Host.log("Необходимо подняться к королю");
                                if (quest.Id == 46930)
                                {
                                    if (!Host.CommonModule.MoveTo(npc, 3, 3))
                                        return false;
                                }
                                else
                                {
                                    if (Host.Me.Location.Z < 470)
                                    {
                                        Host.log("Бегу к лифту");
                                        if (!Host.CommonModule.MoveTo(-1126.50, 851.96, 443.32))
                                            return false;
                                        while (Host.Me.Location.Z < 485)
                                        {
                                            Thread.Sleep(2000);
                                            Host.Jump();
                                        }
                                        Host.log("Захожу в лифт");
                                        Host.Wait(10000);
                                        Host.TurnDirectly(new Vector3F(-1126.19, 842.21, 487.86));
                                        Host.SetMoveStateForClient(true);
                                        Host.MoveForward(true);
                                        Thread.Sleep(2000);
                                        Host.SetMoveStateForClient(false);
                                        Host.MoveForward(false);
                                        if (!Host.CommonModule.ForceMoveTo2(new Vector3F(-1126.19, 842.21, 487.86)))
                                            return false;
                                    }
                                    else
                                    {
                                        Host.log("Бегу к королю");
                                        if (!Host.CommonModule.MoveTo(-1134.96, 805.29, 500.93))
                                            return false;
                                    }
                                }

                            }
                            break;

                        case 122688:
                            {
                                if (!Host.CommonModule.MoveTo(2666.69, 1364.10, 11.17))
                                    return false;
                            }
                            break;

                        default:
                            {
                                if (!Host.CommonModule.MoveTo(npc, 3, 3))
                                    return false;
                            }
                            break;
                    }

                    Thread.Sleep(500);
                    if (quest.Id == 29021)
                    {
                        var result2 = Host.SpellManager.CastSpell(6477, npc);
                        if (result2 != ESpellCastError.SUCCESS)//8613 Skinning
                        {
                            Host.log("Не смог использовать " + "  [" + 6477 + "] " + Host.Me.Distance(npc) + "  " + npc.Id + "   " + Host.GetLastError() + "  " + result2, Host.LogLvl.Error);
                        }
                    }


                    if (Host.Me.Target != npc && npc.Type == EBotTypes.Unit)
                        Host.SetTarget(npc);
                    Thread.Sleep(500);
                    //   Host.log("Открываю диалог");
                    if (!Host.OpenDialog(npc))
                    {

                        Host.log("Не смог начать диалог для завершения квеста с " + npc.Name + " " + revardNpcId + "  " + Host.GetLastError(), Host.LogLvl.Error);
                        Host.log(npc.Guid + "   npc.Guid");
                        Host.log(Host.CurrentInteractionGuid + " CurrentInteractionGuid");
                        Host.CommonModule.MyUnmount();
                        foreach (var entity in Host.GetEntities())
                        {
                            if (entity.Guid == Host.CurrentInteractionGuid)
                            {
                                Host.log("Имя: " + entity.Name);
                                /*  if (Host.Me.Distance(entity) > 2)
                                  Host.CommonModule.MoveTo(entity);*/
                            }
                        }
                        if (Host.GetLastError() == ELastError.ActionNotAllowed)
                        {
                            var safePoint = new List<Vector3F>();

                            /* if(quest.Id == 53372)
                         {
                             Host.log("Необходим релог 2");
                             Host.TerminateGameClient();
                             return false;
                         }*/

                            var xc = Host.Me.Location.X;
                            var yc = Host.Me.Location.Y;

                            var radius = 11;
                            const double a = Math.PI / 16;
                            double u = 0;
                            for (var i = 0; i < 32; i++)
                            {
                                var x1 = xc + radius * Math.Cos(u);
                                var y1 = yc + radius * Math.Sin(u);
                                // log(" " + i + " x:" + x + " y:" + y);
                                u = u + a;
                                var z1 = Host.GetNavMeshHeight(new Vector3F(x1, y1, 0));
                                if (Host.IsInsideNavMesh(new Vector3F((float)x1, (float)y1, (float)z1)))
                                    safePoint.Add(new Vector3F((float)x1, (float)y1, (float)z1));
                            }
                            Host.log("Точкек " + safePoint.Count);
                            if (safePoint.Count > 0)
                            {
                                Vector3F bestPoint = new Vector3F();
                                double bestDist = 0;
                                foreach (var vector3F in safePoint)
                                {
                                    if (npc.Distance(vector3F) > bestDist)
                                    {
                                        bestPoint = vector3F;
                                        bestDist = npc.Distance(vector3F);
                                    }
                                }

                                Host.ComeTo(bestPoint);
                                return false;
                            }
                        }

                    }
                    else
                    {
                        Host.log("Открыл диалог с " + npc.Name, Host.LogLvl.Ok);
                        Host.log("--------------------------------------- GetNpcDialogs " + Host.GetNpcDialogs().Count);

                        foreach (var gossipOptionsData in Host.GetNpcDialogs())
                        {
                            Host.log(gossipOptionsData.Text + " " + " " + gossipOptionsData.Confirm + " " + gossipOptionsData.OptionNPC + "  " + gossipOptionsData.ClientOption);
                        }

                        Host.log("--------------------------------------- GetNpcQuestDialogs " + Host.GetNpcQuestDialogs().Count);
                        foreach (var gossipQuestTextData in Host.GetNpcQuestDialogs())
                        {
                            Host.log(" " + gossipQuestTextData.QuestID + " " + gossipQuestTextData.QuestType + " " + gossipQuestTextData.QuestTitle);

                        }
                    }

                    uint revard = 0;

                    if (Host.Me.Class == EClass.Shaman)
                        if (MyQuestHelp.QuestRevardShaman.ContainsKey(quest.Id))
                            revard = MyQuestHelp.QuestRevardShaman[quest.Id];

                    if (Host.Me.Class == EClass.Hunter)
                        if (MyQuestHelp.QuestRevardHunt.ContainsKey(quest.Id))
                            revard = MyQuestHelp.QuestRevardHunt[quest.Id];

                    if (Host.Me.Class == EClass.Monk)
                        if (MyQuestHelp.QuestRevardMonk.ContainsKey(quest.Id))
                            revard = MyQuestHelp.QuestRevardMonk[quest.Id];

                    if (Host.Me.Class == EClass.Druid)
                        if (MyQuestHelp.QuestRevardDruid.ContainsKey(quest.Id))
                            revard = MyQuestHelp.QuestRevardDruid[quest.Id];

                    if (Host.Me.Class == EClass.Mage)
                        if (MyQuestHelp.QuestRevardMage.ContainsKey(quest.Id))
                            revard = MyQuestHelp.QuestRevardMage[quest.Id];

                    if (Host.Me.Class == EClass.DeathKnight)
                        if (MyQuestHelp.QuestDeathKnight.ContainsKey(quest.Id))
                            revard = MyQuestHelp.QuestDeathKnight[quest.Id];




                    /* if (Host.RequestQuestReward(quest.Id))
                     {
                         Host.log("Получил список наград ", Host.LogLvl.Ok);
                     }
                     else
                     {
                         Host.log("Не смог узнать награду " + Host.GetLastError(), Host.LogLvl.Error);
                     }*/




                    if (Host.QuestRewardOffer == null)
                    {
                        Host.log("QuestRewardOffer  = null");
                    }
                    else
                    {
                        Host.log("AvailableItemIds  " + Host.QuestRewardOffer.AvailableItemIds.Count);
                    }

                    /*  if (Host.QuestRewardOffer != null && Host.QuestRewardOffer.AvailableItemIds != null)
                      {
                          if (quest.Id == 48657 || quest.Id == 47870)
                          {
                              if (Host.QuestRewardOffer.IsRequireItems && Host.QuestRewardOffer.AvailableItemIds.Count == 0)
                              {
                                  if (Host.RequestQuestReward(quest.Id))
                                  {
                                      //...
                                  }
                              }
                          }*/

                    if (!Host.PrepareCompleteQuest(id))
                    {
                        Host.log("Не смог узнать награду " + Host.GetLastError(), Host.LogLvl.Error);
                    }


                    if (Host.QuestRewardOffer != null && Host.QuestRewardOffer.AvailableItemIds != null)
                    {
                        foreach (var templateRewardItem in Host.QuestRewardOffer.AvailableItemIds)
                        {
                            if (Host.GameDB.ItemTemplates.ContainsKey(templateRewardItem))
                            {
                                Host.log("Награда " + Host.GameDB.ItemTemplates[templateRewardItem].GetName() + "[" + templateRewardItem + "]", Host.LogLvl.Error);
                            }
                            else
                            {
                                Host.log("Неизвестная награда " + templateRewardItem, Host.LogLvl.Error);
                            }
                        }
                        if (Host.QuestRewardOffer.AvailableItemIds.Count > 1 && revard == 0)
                        {
                            var rand = Host.RandGenerator.Next(0, Host.QuestRewardOffer.AvailableItemIds.Count);
                            Host.log("Не выбрана награда " + revard + " " + Host.QuestRewardOffer.AvailableItemIds.Count + "  " + rand, Host.LogLvl.Important);
                            revard = Host.QuestRewardOffer.AvailableItemIds[rand];
                            //Thread.Sleep(5000);
                            // return false;
                        }
                    }




                    Thread.Sleep(500);
                    if (!Host.CompleteQuest(id, revard))
                    {
                        Host.log("Не смог завершить квест " + id + " с выбором награды " + revard + "  " + Host.GetLastError(), Host.LogLvl.Error);
                        Thread.Sleep(6000);

                        var zRange = Math.Abs(Host.Me.Location.Z - npc.Location.Z);

                        if (zRange > 6)
                        {
                            var randloc = Host.RandGenerator.Next(-3, 3);
                            Host.CommonModule.MoveTo(Host.Me.Location.X + randloc, Host.Me.Location.Y + randloc, Host.Me.Location.Z);
                            Host.CommonModule.MoveTo(npc, 0);
                        }


                        Host.SendKeyPress(0x1b);
                        if (Host.GetQuest(id) == null)
                        {
                            Host.QuestStates.QuestState.Add(id);

                            Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                        }
                    }
                    else
                    {
                        Host.log("Завершил квест " + id, Host.LogLvl.Ok);
                        if (id == 47441)
                        {
                            Host.Wait(5000);
                            Host.SendKeyPress(0x1b);
                        }
                        Host.QuestStates.QuestState.Add(id);
                        /* var isNewQuest = true;
                         foreach (var questState in Host.QuestStates.QuestState)
                         {
                             if (questState == id)
                             {
                                 //  questState.State = "Выполнен";
                                 isNewQuest = false;
                             }
                         }
                         if (isNewQuest)
                             Host.QuestStates.QuestState.Add(new QuestState
                             {
                                 QuestId = id,
                                 State = "Выполнен"
                             }); */
                        Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                    }
                    Thread.Sleep(500);
                    return false;
                }
                else
                {
                    Host.log("Не нашел завершающего НПС по указанным координатам " + revardNpcId + "  ", Host.LogLvl.Error);




                    if (quest.Id == 47573)
                        Host.CommonModule.MoveTo(3780.09, 2710.15, 74.93);

                    if (quest.Id == 47583)
                        Host.CommonModule.MoveTo(354.35, -4.03, 240.25);

                    if (quest.Id == 49067)
                        Host.CommonModule.MoveTo(2590.31, 479.36, 7.55);
                    if (quest.Id == 49491)
                        revardNpcId = 278452;

                    if (quest.Id == 47583)
                        revardNpcId = 291013;

                    if (quest.Id == 47873)
                        revardNpcId = 282746;


                    if (quest.Id == 50539 || quest.Id == 48315)
                        revardNpcId = 281639;

                    if (quest.Id == 48655 || quest.Id == 48657 || quest.Id == 48656)
                        revardNpcId = 276187;

                    if (quest.Id == 50539 || quest.Id == 48315 || quest.Id == 49491 || quest.Id == 48655 || quest.Id == 48657 || quest.Id == 48656 || quest.Id == 47583 || quest.Id == 47873)
                    {
                        foreach (var gameObject in Host.GetEntities<GameObject>())
                        {
                            if (gameObject.Id != revardNpcId)
                                continue;
                            npc = gameObject;
                        }

                        Host.CommonModule.MoveTo(npc, 3);
                        Host.MyCheckIsMovingIsCasting();
                        if (Host.OpenDialog(npc))
                        {
                            uint revard = 0;

                            if (quest.Id == 48657)
                                revard = 159978;

                            Host.PrepareCompleteQuest(quest.Id);
                            if (!Host.CompleteQuest(quest.Id, revard))
                            {
                                Host.log("Не смог завершить квест " + "с наградой " + revard + " " + Host.GetLastError(), Host.LogLvl.Error);
                            }

                            else
                            {
                                Host.log("Завершил квест");
                                Host.QuestStates.QuestState.Add(id);
                                Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);

                            }
                        }
                        else
                        {
                            Host.CommonModule.MyUnmount();
                            Host.log("Не смог открыть диалог [" + npc.Id + "] " + npc.Name + "  " + Host.GetLastError(), Host.LogLvl.Error);
                        }

                    }

                    completeQuestCount++;
                    if (revardNpcLoc != Vector3F.Zero && Host.Me.Distance(revardNpcLoc) < 400 && completeQuestCount > 20)
                    {
                        if (!Host.CommonModule.MoveTo(revardNpcLoc, 20, 20))
                            return false;
                        npc = Host.GetNpcById(revardNpcId);
                        if (npc == null)
                        {
                            BadLoc.Add(revardNpcLoc);
                        }
                        else
                        {
                            BadLoc.Clear();
                        }
                        completeQuestCount = 0;
                    }
                    if (revardNpcLoc == Vector3F.Zero)
                        BadLoc.Clear();

                }
                return true;
            }
            catch (Exception e)
            {
                Host.log(e + "");
                return false;
            }
        }

        private int completeQuestCount = 0;

        public bool TryFly;



        public bool GroupOffline()
        {
            var result = true;
            foreach (var groupMember in Host.Group.GetMembers())
            {
                if (Host.Me.Name == groupMember.Name)
                    continue;
                if (groupMember.Status != EGroupMemberOnlineStatus.Offline)
                    result = false;
            }

            return result;

        }

        private bool move;

        public virtual bool RunQuest(uint id)
        {
            try
            {
                var quest = Host.GetQuest(id);
                var questTemplate = Host.GameDB.QuestTemplates[id];
                QuestType = ExecuteType.Unknown;
                Host.log("Получаю информацию о квесте: " + questTemplate.LogTitle + "["
                    + id + "]  "
                    + " State:" + quest?.State
                    + "  QuestSortID:" + quest?.Template.QuestSortID
                    + "  StartItem:" + quest?.Template.StartItem
                    + "  QuestInfoID:" + quest?.Template.QuestInfoID
                    + "  Flags:" + quest?.Template.Flags
                    + "  FlagsEx:" + quest?.Template.FlagsEx
                    + ""
                    , Host.LogLvl.Ok);

                if (Host.CommonModule.InFight())
                    return false;


                if (quest == null)
                {
                    Host.log("Беру квест " + questTemplate.LogTitle + "[" + id + "]", Host.LogLvl.Important);

                    if (id == 56030 && !Host.CompleteQuest(56030))
                    {
                        Host.SendKeyPress(0x1b);
                        Thread.Sleep(2000);
                        if (id == 56030 && !Host.CompleteQuest(56030))
                        {
                            Host.log("Нет квеста, нужен релог");
                            Host.TerminateGameClient();
                            Host.GetCurrentAccount().IsAutoLaunch = false;
                            var waitTime = 120000;
                            while (waitTime > 0)
                            {

                                Thread.Sleep(1000);
                                waitTime = waitTime - 1000;
                                Host.log("Ожидаю " + waitTime + "/120000");
                            }
                            Host.GetCurrentAccount().IsAutoLaunch = true;
                        }

                        return false;
                    }

                    if (id == 53372 && Host.Me.Team == ETeam.Horde)
                    {
                        Host.log("Нет квеста Час расплаты, делаю релог");
                        Host.TerminateGameClient();
                        Host.GetCurrentAccount().IsAutoLaunch = false;
                        var waitTime = 60000;
                        while (waitTime > 0)
                        {

                            Thread.Sleep(1000);
                            waitTime = waitTime - 1000;
                            Host.log("Ожидаю " + waitTime + "/120000");
                        }
                        Host.GetCurrentAccount().IsAutoLaunch = true;
                        return false;
                    }

                    if (id == 53370 && Host.Me.Team == ETeam.Alliance)
                    {
                        Host.log("Нет квеста Час расплаты, делаю релог");
                        Host.GetCurrentAccount().IsAutoLaunch = false;
                        var waitTime = 60000;
                        while (waitTime > 0)
                        {

                            Thread.Sleep(1000);
                            waitTime = waitTime - 1000;
                            Host.log("Ожидаю " + waitTime + "/120000");
                        }
                        Host.GetCurrentAccount().IsAutoLaunch = true;
                        return false;
                    }

                    QuestCoordSettings questCoordSettings = null;
                    foreach (var questSettingsQuestCoordSetting in Host.QuestSettings.QuestCoordSettings)
                    {
                        if (questSettingsQuestCoordSetting.QuestId == id)
                        {
                            questCoordSettings = questSettingsQuestCoordSetting;
                            break;
                        }
                    }

                    if (questCoordSettings != null)
                    {
                        var npcLoc = questCoordSettings.Loc;

                        if (npcLoc != null)
                        {
                            var npc = Host.GetNpcById(questCoordSettings.NpcId);
                            if (npc == null)
                            {
                                if (id == 47512)
                                {
                                    if (Host.Area.Id == 8501) //волдун
                                    {
                                        Host.MyUseTaxi(8499, new Vector3F(-1035.45, 758.30, 435.33));
                                        return false;
                                    }
                                }
                                if (id == 47514)
                                {
                                    if (Host.Area.Id == 8500) //Назимир
                                    {
                                        Host.MyUseTaxi(8499, new Vector3F(-1035.45, 758.30, 435.33));
                                        return false;
                                    }
                                }



                                if (id == 52746 || id == 53602 || id == 52444) //Военный фонд[52746]   
                                {
                                    if (Host.Me.Distance(-2137.44, 797.85, 5.93) > 50)
                                        if (!Host.CommonModule.MoveTo(-2137.44, 797.85, 5.93, 5, 5))
                                            return false;
                                }



                                Host.log("Бегу к НПС " + questCoordSettings.NpcId);
                                if (!Host.CommonModule.MoveTo(npcLoc, 5, 5))
                                    return false;
                            }

                            npc = Host.GetNpcById(questCoordSettings.NpcId);
                            if (npc != null)
                            {
                                if (MyApplyQuest(npc, id))
                                    return false;
                            }
                            else
                            {
                                Host.log("Не нашел НПС в указанных координатах " + questCoordSettings.NpcId, Host.LogLvl.Error);
                                if (!Host.CommonModule.MoveTo(npcLoc))
                                    return false;
                                if (id == 49768)
                                {
                                    if (questFix > 3)
                                    {
                                        questFix = 0;
                                        Host.log("Добавляю в игнор");
                                        Host.QuestStates.QuestState.Add(id);
                                        Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                                    }
                                    questFix++;
                                }
                            }
                        }
                        else
                        {
                            Host.log("Не нашел координаты НПС", Host.LogLvl.Error);
                        }
                    }
                    else
                    {
                        Host.log("Не нашел квест ", Host.LogLvl.Error);
                    }
                    Thread.Sleep(10000);
                    return false;
                }

                //    var sw = new Stopwatch();
                //   sw.Start();
                var step = 0;
                for (var index = 0; index < quest.Counts.Length; index++)
                {
                    var questCount = quest.Counts[index];
                    if (questCount == 0)
                        continue;
                    step = step + questCount;
                    Host.log("questCount: " + index + ") " + questCount);
                }


                Host.log("Шаг квеста " + step);


                if (quest.Id == 49227)
                {
                    if (step > 0)
                    {
                        if (Host.Me.Distance(329.33, 3263.19, 99.82) > 150)
                            Host.CommonModule.MoveTo(329.33, 3263.19, 99.82, 20);
                        MyUseSpellClick(128421);
                        if (step == 9)
                            MyComliteQuest(quest);
                        return false;
                    }
                }

                if (quest.Id == 50596)
                {
                    if (!Host.CheckQuestCompleted(50536))
                        RunQuest(50536);
                    return false;
                }

                if (quest.Id == 47316)
                {

                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2121.96, 2885.54, 33.33))
                            return false;
                        Host.MyUseGameObject(271844);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2335.57, 2858.18, 19.13))
                            return false;
                        Host.MyUseGameObject(271844);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2237.15, 3087.29, 55.07))
                            return false;
                        Host.MyUseGameObject(271844);
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(2428.09, 3094.88, 50.42))
                            return false;
                        Host.MyUseGameObject(271844);
                    }
                    if (step == 4)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 55053)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1003.10, -418.18, -282.65)))
                            return false;
                        Host.CommonModule.MyUnmount();
                        if (Host.SpellManager.GetSpell(301991) == null)
                        {
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            var res = Host.SpellManager.CastSpell(301991);
                            if (res != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не удалось использовать сердце  " + res);
                            }
                            Thread.Sleep(5000);
                            while (Host.SpellManager.IsCasting)
                            {
                                Thread.Sleep(5000);

                            }
                        }
                    }

                    return false;
                }

                if (quest.Id == 55094)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1048.03, -192.28, -298.21)))
                            return false;
                    }
                }

                if (quest.Id == 55851)
                {
                    if (step == 0)
                    {
                        if (Host.MapID == 1718)
                        {
                            if (!Host.CommonModule.MoveTo(1007.81, -423.91, -282.28))
                                return false;
                            Host.MyUseGameObject(327527);
                            Thread.Sleep(1000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                        if (Host.MapID == 1642)
                        {
                            if (!Host.CommonModule.MoveTo(-1137.48, 770.38, 433.30, 20))
                                return false;
                            Host.MyUseGameObject(289326);
                            Thread.Sleep(1000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                        if (Host.MapID == 1)
                        {
                            if (!Host.CommonModule.MoveTo(-7079.78, 1239.23, -110.32, 20))
                                return false;
                            Host.CanselForm();
                            Host.CommonModule.MyUnmount();
                            Host.MyUseGameObject(325722);
                            Thread.Sleep(1000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 55533)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(152194);
                        Host.MyDialog(npc, 2);
                        Host.Wait(5000);
                    }
                }

                if (quest.Id == 55374)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(152194);
                        Host.MyDialog(npc, 1);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(3748.05, 4196.73, 890.73, 20))
                            return false;
                        var npc = Host.GetNpcById(151641);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(1000);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 55400)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(151643);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }

                if (quest.Id == 55407)
                {
                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2878.36, 1949.20, -196.36))
                            return false;
                        Thread.Sleep(1000);
                        var res = Host.SpellManager.CastSpell(294595);
                        if (res != ESpellCastError.SUCCESS)
                            Host.log("Не смог использовать склил " + res, Host.LogLvl.Error);
                        Host.Wait(5000);
                        Host.MyCheckIsMovingIsCasting();

                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2835.69, 2005.77, -209.83))
                            return false;
                        Thread.Sleep(1000);
                        var res = Host.SpellManager.CastSpell(294595);
                        if (res != ESpellCastError.SUCCESS)
                            Host.log("Не смог использовать склил " + res, Host.LogLvl.Error);
                        Host.Wait(5000);
                        Host.MyCheckIsMovingIsCasting();

                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2896.02, 2034.26, -200.97))
                            return false;
                        Thread.Sleep(1000);
                        var res = Host.SpellManager.CastSpell(294595);
                        if (res != ESpellCastError.SUCCESS)
                            Host.log("Не смог использовать склил " + res, Host.LogLvl.Error);
                        Host.Wait(5000);
                        Host.MyCheckIsMovingIsCasting();

                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(2778.94, 2034.18, -210.74))
                            return false;
                        Thread.Sleep(1000);
                        var res = Host.SpellManager.CastSpell(294595);
                        if (res != ESpellCastError.SUCCESS)
                            Host.log("Не смог использовать склил " + res, Host.LogLvl.Error);
                        Host.Wait(5000);
                        Host.MyCheckIsMovingIsCasting();

                    }

                    if (step == 4)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 55425)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(151698);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (step == 1 || step == 2)
                    {
                        Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        if (!Host.CommonModule.MoveTo(2762.72, 2039.65, -210.74, 3))
                            return false;
                        if (Host.FarmModule.BestMob == null)
                            Host.FarmModule.BestMob = Host.GetNpcById(151739) as Unit;
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(2812.35, 2008.11, -210.60, 3))
                            return false;
                        Host.MyUseGameObject(324039);
                    }

                    if (step == 4)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 56161)
                {
                    if (Host.MapID == 2215)
                    {
                        if (!Host.CommonModule.MoveTo(-8281.58, 1754.38, 311.96, 3))
                            return false;
                        Host.MyUseGameObject(289522);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (Host.MapID == 1)
                    {
                        Host.MyUseGameObject(289697);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (Host.MapID == 1642)
                    {
                        Host.MyUseGameObject(327526);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (Host.MapID == 1718)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 55481)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(152529);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(1000);
                    }


                    return false;
                }

                if (quest.Id == 57010)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-8359.01, 1754.79, 316.17))
                            return false;
                        var item = Host.MyGetItem(168611);
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5000);
                        Host.ActivateAzeriteEssense(0xC);
                        Thread.Sleep(5000);
                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 55497)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2837.60, 2101.97, -177.90))
                            return false;
                        var npc = Host.GetNpcById(151963);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    if (step == 1)
                    {
                        if (Host.MapID == 1220)
                        {
                            Host.MyUseStone();
                        }

                        if (Host.MapID == 1)
                        {
                            if (Host.Me.Distance(-7096.00, 1302.70, -93.30) < 500)
                            {
                                if (!Host.CommonModule.MoveTo(-7079.78, 1239.23, -110.32, 20))
                                    return false;
                                Host.CanselForm();
                                Host.MyUseGameObject(325722);

                                Thread.Sleep(1000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }

                                return false;
                            }


                            if (!Host.CommonModule.MoveTo(new Vector3F(1432.10, -4519.39, 18.81)))
                                return false;
                            Host.MyUseGameObject(323855);
                            Thread.Sleep(1000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                        if (Host.MapID == 1642)
                        {
                            if (!Host.CommonModule.MoveTo(-1137.48, 770.38, 433.30, 20))
                                return false;
                            Host.MyUseGameObject(289326);
                            Thread.Sleep(1000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                        }

                        if (Host.MapID == 2215)
                        {
                            var npc = Host.GetNpcById(151964);
                            Host.MyDialog(npc, 0);
                            Thread.Sleep(1000);
                        }

                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 55618)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(152194);
                        Host.MyDialog(npc, 5);
                        Thread.Sleep(1000);
                    }

                    return false;
                }

                if (quest.Id == 56429)
                {
                    if (step == 0)
                    {
                        if (!move)
                        {
                            if (!Host.CommonModule.MoveTo(new Vector3F(696.87, -43.10, -182.53)))
                                return false;
                            else
                            {
                                move = true;
                            }
                        }
                        else
                        {
                            if (!Host.CommonModule.MoveTo(new Vector3F(849.24, -14.62, -225.33)))
                                return false;
                            Host.MyUseGameObject(329639);
                        }
                    }



                    return false;
                }


                if (quest.Id == 55092)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(563.36, -158.61, -194.01)))
                            return false;
                        Host.MyUseGameObject(322066);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(607.94, -198.10, -194.25)))
                            return false;
                        Host.MyUseGameObject(322066);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(577.00, -136.06, -184.97)))
                            return false;
                        Host.MyUseGameObject(322066);
                    }
                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(593.07, -112.17, -184.16)))
                            return false;
                        Host.MyUseGameObject(322066);
                    }

                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(694.02, -241.96, -183.48)))
                            return false;
                        Host.MyUseGameObject(322066);
                    }

                    if (step == 5)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 51233)
                {
                    Host.FarmModule.farmState = FarmState.Disabled;
                    if (step == 0)
                    {
                        if (Host.MyGetAura(269564) == null)
                        {
                            var npc = Host.GetNpcById(136683);
                            Host.MyDialog(npc, 1);
                            Host.Wait(5000);
                        }
                        if (Host.MyGetAura(269564) == null)
                            return false;

                        if (!Host.CommonModule.MoveTo(new Vector3F(-182.18, 3380.21, 230.94)))
                            return false;
                        var npc2 = Host.GetNpcById(137613);
                        Host.MyDialog(npc2, 0);
                        Host.Wait(5000);
                    }

                    if (step == 1)
                    {

                        if (!Host.CommonModule.MoveTo(new Vector3F(208.55, 3165.56, 365.60)))
                            return false;
                        Host.Wait(10000);
                    }
                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                }

                if (quest.Id == 51224)
                {
                    Host.FarmModule.farmState = FarmState.Disabled;
                    if (step == 0)
                    {
                        if (Host.MyGetAura(269564) == null)
                        {
                            var npc = Host.GetNpcById(136683);
                            Host.MyDialog(npc, 0);
                            Host.Wait(5000);
                        }

                        if (Host.MyGetAura(269564) == null)
                            return false;
                        if (!Host.CommonModule.MoveTo(new Vector3F(-502.16, 4026.22, 81.48)))
                            return false;
                        MyUseSpellClick(137079);
                        Host.Wait(20000);

                    }

                    if (step == 1)
                    {
                        if (Host.MyGetAura(269564) == null)
                            MyUseSpellClick(137090);
                        if (Host.MyGetAura(269564) == null)
                            return false;
                        if (!Host.CommonModule.MoveTo(new Vector3F(-507.88, 3846.11, 89.64)))
                            return false;
                        MyUseSpellClick(144283);
                        if (Host.MyGetAura(269564) == null)
                            Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        Host.Wait(20000);

                    }

                    if (step == 2)
                    {
                        Host.CanselForm();
                        if (Host.MyGetAura(269564) == null)
                            MyUseSpellClick(137090);
                        if (Host.MyGetAura(269564) == null)
                            return false;
                        if (!Host.CommonModule.MoveTo(new Vector3F(-830.90, 3616.61, 80.36)))
                            return false;

                        MyUseSpellClick(144284);
                        if (Host.MyGetAura(269564) == null)
                            Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        Host.Wait(5000);
                    }

                    if (step == 3)
                    {
                        if (Host.MyGetAura(269564) == null)
                            MyUseSpellClick(137090);
                        if (Host.MyGetAura(269564) == null)
                            return false;
                        if (!Host.CommonModule.MoveTo(new Vector3F(-822.30, 3459.92, 115.79)))
                            return false;


                        Host.Wait(12000);
                        Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                    }


                    return false;
                }

                if (quest.Id == 51916)
                {

                    MyComliteQuest(quest);
                }

                if (quest.Id == 51589)
                {
                    if (Host.MapID == 1642)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(-2174.15, 764.66, 20.92)))
                            return false;
                        var npc = Host.GetNpcById(135690);
                        Host.MyDialog(npc, 4);
                        Host.Wait(10000);
                        return false;
                    }

                    if (Host.MapID == 1642)
                        MyComliteQuest(quest);
                }

                if (quest.Id == 51985)
                {
                    if (Host.MapID == 1643)
                    {
                        if (!Host.CommonModule.MoveTo(-397.19, 4119.56, 3.01))
                            return false;
                        var npc = Host.GetNpcById(139519);
                        Host.MyDialog(npc, 0);
                        Host.Wait(5000);
                        return false;
                    }

                    if (Host.MapID == 1642)
                        MyComliteQuest(quest);
                }

                if (quest.Id == 51234)
                {
                    if (step == 0)
                    {
                        Host.MyUseGameObject(288625);
                        Host.SpellManager.CastSpell(269987);
                        Host.Wait(15000);
                        return false;
                    }

                    if (step == 1)
                    {
                        Host.MyUseGameObject(288624);
                        // Host.SpellManager.CastSpell(269987);
                        Host.Wait(5000);
                        return false;
                    }
                    if (step == 2)
                    {
                        Host.MyUseGameObject(288630);
                        // Host.SpellManager.CastSpell(269987);
                        Host.Wait(5000);
                        return false;
                    }
                    if (step == 3)
                    {
                        var npc = Host.GetNpcById(137397);
                        Host.MyDialog(npc, 0);
                        Host.MyDialog(npc, 1);
                        // Host.SpellManager.CastSpell(269987);
                        Host.Wait(5000);
                        return false;
                    }

                    if (step == 5)
                        MyComliteQuest(quest);

                    return false;
                }


                if (quest.Id == 48400)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-2709.22, 2399.44, 1.39))
                            return false;
                        // Host.Wait(15000);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-2709.22, 2399.44, 1.39))
                            return false;
                        Host.MyUseGameObject(281422);
                        // Host.Wait(15000);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 48317)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-2971.42, 2236.19, 50.13))
                            return false;
                        var npc = Host.GetNpcById(131878);
                        Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step == 1)
                    {
                        Host.MyUseGameObject(280570);
                        Host.Wait(15000);
                        return false;
                    }

                    if (step == 3)
                    {
                        Host.MyUseGameObject(280571);
                        Host.Wait(15000);
                        return false;
                    }

                    if (step == 5)
                    {
                        Host.MyUseGameObject(280572);
                        Host.Wait(15000);
                        return false;
                    }

                    return false;
                }

                if (quest.Id == 50043)
                {
                    if (quest.Counts[0] > 49)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    var npc = Host.GetNpcById(131760);
                    if (npc == null)
                        npc = Host.GetNpcById(131751);
                    if (npc == null)
                        npc = Host.GetNpcById(131755);
                    if (npc == null)
                        npc = Host.GetNpcById(131756);
                    if (npc == null)
                        npc = Host.GetNpcById(131761);
                    if (npc == null)
                        npc = Host.GetNpcById(131758);
                    if (npc != null)
                    {
                        Host.CommonModule.MoveTo(npc);
                        var item = Host.MyGetItem(156596);
                        if (item != null)
                        {
                            Host.MyUseItemAndWait(item);
                            Host.Wait(10000);
                        }
                    }

                    var go = Host.GetNpcById(280497);
                    if (go == null)
                        go = Host.GetNpcById(280493);
                    if (go == null)
                        go = Host.GetNpcById(280501);
                    if (go == null)
                        go = Host.GetNpcById(280491);
                    if (go == null)
                        go = Host.GetNpcById(280493);
                    if (go == null)
                        go = Host.GetNpcById(280495);
                    if (go == null)
                        go = Host.GetNpcById(280496);
                    if (go == null)
                        go = Host.GetNpcById(280490);

                    if (go != null)
                        Host.MyUseGameObject(go.Id);

                    if (Host.Me.Distance(-2776.17, 2136.62, 17.54) > 100 || go == null && npc == null)
                        if (!Host.CommonModule.MoveTo(-2776.17, 2136.62, 17.54))
                            return false;
                    return false;
                }


                if (quest.Id == 47235)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-2692.36, 2108.09, 27.22))
                            return false;
                        Host.MyUseGameObject(280337);
                    }
                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-2783.59, 2079.88, 20.39))
                            return false;
                        Host.MyUseGameObject(280336);
                    }
                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(-2861.75, 2208.15, 4.77))
                            return false;
                        Host.MyUseGameObject(280335);
                    }

                    if (step == 3)
                        MyComliteQuest(quest);

                    return false;
                }
                if (quest.Id == 47329)
                {
                    if (step == 10)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    if (step < 2)
                    {
                        MyUseSpellClick(131799);
                        Host.Wait(10000);
                    }

                    MyUseSpellClick(131707);
                    Host.Wait(10000);
                    return false;
                }

                if (quest.Id == 53602)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(143913);
                        Host.MyDialog(npc, 0);
                        Host.Wait(20000);
                    }
                }

                if (quest.Id == 49126)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2890.32, 24.87, 4.64))
                            return false;
                        if (Host.SpellManager.GetSpell(259742) != null)
                        {
                            Host.SpellManager.CastSpell(259742);
                            Thread.Sleep(4000);
                        }
                        return false;
                    }
                    if (step < 51)
                    {
                        if (Host.MyGetAura(259742) == null)
                        {
                            if (!Host.CommonModule.MoveTo(2890.32, 24.87, 4.64))
                                return false;
                            if (Host.SpellManager.GetSpell(259742) != null)
                            {
                                Host.SpellManager.CastSpell(259742);
                                Thread.Sleep(4000);
                            }
                        }
                        else
                        {
                            if (!Host.CommonModule.MoveTo(2898.95, 57.89, 5.94))
                                return false;
                            if (!Host.CommonModule.MoveTo(2942.48, 115.52, -0.03))
                                return false;
                            if (!Host.CommonModule.MoveTo(2991.53, 121.30, 0.10))
                                return false;
                            if (!Host.CommonModule.MoveTo(2981.48, 74.74, 3.54))
                                return false;
                        }
                    }

                    return false;
                }

                // MyUseSpellClick(138797);
                if (quest.Id == 47581)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(3653.65, 2528.00, 28.07, 10))
                            return false;
                        MyUseSpellClick(138797);
                        // var npc = Host.GetNpcById(281536);
                        // Host.MyUseGameObject(281536);
                        // Host.MyDialog(npc, 0);
                        return false;
                    }

                    MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 47571)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(3400.99, 2491.48, 43.72))
                            return false;
                        var npc = Host.GetNpcById(123063);
                        Host.MyDialog(npc, 0);
                    }
                    return false;
                }

                if (quest.Id == 51057)
                {
                    if (step < 8)
                    {
                        if (Host.Me.Distance(669.92, 4432.89, 0.74) > 100)
                            if (!Host.CommonModule.MoveTo(669.92, 4432.89, 0.74))
                                return false;
                        foreach (var gameObject in Host.GetEntities<GameObject>().OrderBy(i => Host.Me.Distance(i)))
                        {
                            if (gameObject.Id == 289632)
                            {
                                var item = Host.MyGetItem(159774);
                                Host.CommonModule.MoveTo(gameObject, 3);
                                Host.MyUseItemAndWait(item, gameObject);
                                Thread.Sleep(3000);
                                return false;
                            }
                        }

                        if (Host.Me.Distance(597.19, 4400.26, 0.28) > 50)
                            if (!Host.CommonModule.MoveTo(597.19, 4400.26, 0.28))
                                return false;
                        foreach (var gameObject in Host.GetEntities<GameObject>().OrderBy(i => Host.Me.Distance(i)))
                        {
                            if (gameObject.Id == 289632)
                            {
                                var item = Host.MyGetItem(159774);
                                Host.CommonModule.MoveTo(gameObject, 3);
                                Host.MyUseItemAndWait(item, gameObject);
                                Thread.Sleep(3000);
                                return false;
                            }
                        }
                    }

                    if (step == 8)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51056)
                {
                    var item = Host.MyGetItem(159757);
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(792.12, 4235.22, 19.25))
                            return false;

                        Host.MyUseItemAndWait(item);

                    }
                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(915.75, 4248.02, 34.09))
                            return false;

                        Host.MyUseItemAndWait(item);

                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                }

                if (quest.Id == 51055)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(791.51, 4174.40, 15.30, 1))
                            return false;
                        MyUseSpellClick(136434);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(805.21, 4287.50, 13.68, 10))
                            return false;
                        MyUseSpellClick(136435);
                        return false;
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(837.36, 4326.10, 19.63, 10))
                            return false;
                        MyUseSpellClick(136441);
                        return false;
                    }

                    if (step == 3)
                        MyComliteQuest(quest);
                    return false;

                }
                if (quest.Id == 51054)
                {
                    if (step == 0)
                    {
                        MyUseSpellClick(136583);
                        Thread.Sleep(10000);
                    }

                    if (step == 1)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 47870)
                {
                    if (quest.Counts[1] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(371.31, 3507.88, 102.59))
                            return false;
                    }
                    if (quest.Counts[2] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(389.87, 3417.46, 103.74))
                            return false;
                    }
                    if (quest.Counts[3] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(343.16, 3495.87, 100.25))
                            return false;
                    }

                    if (step == 3)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 51053)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(456.60, 4364.84, 8.04))
                            return false;
                        var item = Host.MyGetItem(159747);
                        if (item != null)
                        {

                            Host.MyUseItemAndWait(item);
                            Thread.Sleep(10000);
                        }
                    }

                    MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51668)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(963.89, 3732.79, 59.22, 5))
                            return false;

                        var npc = Host.GetNpcById(138113);
                        var item = Host.MyGetItem(160525);
                        if (item != null && npc != null)
                        {
                            Host.CommonModule.MoveTo(npc, 25);
                            Host.MyUseItemAndWait(item, npc);
                        }
                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;

                }

                if (quest.Id == 48847)
                {
                    if (step < 8)
                    {
                        if (!Host.CommonModule.MoveTo(866.76, 3736.53, 63.86, 20))
                            return false;
                        foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i)))
                        {
                            if (!Host.MainForm.On)
                                return false;
                            step = 0;
                            foreach (var questCount in quest.Counts)
                            {
                                step = step + questCount;
                            }
                            if (step == 8)
                                break;
                            if (entity.Id == 138395)
                            {
                                Host.CommonModule.MoveTo(entity, 3);
                                Host.MyDialog(entity, 0);


                            }
                        }
                    }
                    if (step == 8)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 47585)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(134.28, -265.19, 165.00))
                            return false;
                        var npc = Host.GetNpcById(123117);
                        Host.MyDialog(npc, 0);
                        return false;
                    }
                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-29.47, -28.51, 205.61))
                            return false;
                        var npc = Host.GetNpcById(123113);
                        Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                }



                if (quest.Id == 48846)
                {
                    if (step == 0 && Host.MeGetItemsCount(160499) == 0)
                    {
                        if (!Host.CommonModule.MoveTo(871.43, 3766.24, 65.44))
                            return false;
                        var npc = Host.GetNpcById(127578);
                        if (npc != null)
                        {
                            Host.CommonModule.MoveTo(npc);
                            Thread.Sleep(1000);
                            if (!Host.OpenShop(npc as Unit))
                            {
                                return false;
                            }

                            Thread.Sleep(2000);
                            foreach (var item in Host.GetVendorItems())
                            {
                                if (item.ItemId == 160499)
                                {
                                    item.Buy(1);
                                    break;
                                }
                            }


                        }

                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(849.18, 3738.61, 63.86))
                            return false;
                        var npc = Host.GetNpcById(125862);
                        Host.MyDialog(npc, 0);
                        MyComliteQuest(quest);
                        return false;
                    }

                    return false;

                }

                if (quest.Id == 47261)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1070.98, -8.83, 253.50))
                            return false;
                        var npc = Host.GetNpcById(122009);
                        if (npc != null)
                            Host.MyDialog(npc, 0);

                    }


                }

                if (quest.Id == 47418)
                {
                    if (step == 0)
                    {
                        var item = Host.MyGetItem(147897);
                        Host.MyUseItemAndWait(item);
                        Host.Wait(5000);
                        return false;
                    }
                    if (step == 1)
                        MyComliteQuest(quest);
                }


                if (quest.Id == 47259 || quest.Id == 48527)
                {
                    if (step == 9)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }
                }
                if (quest.Id == 47311)
                {
                    if (step == 12)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }
                }
                if (quest.Id == 47312)
                {
                    if (step == 1)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }
                }

                if (quest.Id == 48405)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1962.45, 556.34, 44.76))
                            return false;
                        var npc = Host.GetNpcById(126005);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Host.Wait(5000);
                        }
                        if (!Host.CommonModule.MoveTo(-1906.13, 599.69, 54.96))
                            return false;
                        return false;
                    }

                    if (step == 1)
                    {
                        var npc = Host.GetNpcById(125996);
                        if (npc != null && Host.FarmModule.BestMob == null)
                            Host.FarmModule.BestMob = npc as Unit;
                    }
                    return false;
                }

                if (quest.Id == 52472)
                {
                    if (quest.Counts[0] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1980.68, 630.09, 25.17))
                            return false;
                        var npc = Host.GetNpcById(134346);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);

                        }
                        return false;
                    }
                }


                if (quest.Id == 48404)
                {
                    while (step < 5)
                    {
                        step = 0;
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                        Thread.Sleep(1000);
                        if (!Host.MainForm.On)
                            return false;
                        /* if (Host.Me.Distance(-1951.15, 809.96, 25.42) > 30)
                             Host.CommonModule.MoveTo(-1951.15, 809.96, 25.42);*/
                        var npc = Host.GetNpcById(126034);
                        if (npc != null)
                        {
                            /*  if (Host.Me.Distance(npc) > 20)
                                  Host.log("Слишком далеко " + Host.Me.Distance(npc));*/
                            MyUseSpellClick(npc);
                            Thread.Sleep(2000);
                            //  Host.CommonModule.MoveTo(-1951.15, 809.96, 25.42);
                        }
                        else
                        {
                            Host.log("Не нашел");
                            if (Host.Me.Distance(-1951.15, 809.96, 25.42) > 20)
                            {
                                Host.CommonModule.MoveTo(-1951.15, 809.96, 25.42);
                                continue;
                            }
                            if (Host.Me.Distance(-1955.75, 617.32, 25.00) > 20)
                            {
                                Host.CommonModule.MoveTo(-1955.75, 617.32, 25.00);
                                continue;
                            }

                        }

                    }
                    MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 48454)
                {

                    MyComliteQuest(quest);
                    return false;
                }


                if (quest.Id == 46927)
                {
                    if (step < 4)
                    {
                        if (Host.Me.Distance(-1901.35, 629.12, 87.72) > 30)
                            if (!Host.CommonModule.MoveTo(-1901.35, 629.12, 87.72))
                                return false;

                        Host.MyUseGameObject(269026);
                        Host.Wait(7000);
                        return false;
                    }
                    if (step == 4)
                        MyComliteQuest(quest);
                    return false;
                }


                if (quest.Id == 46846)
                {
                    if (quest.Counts[0] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1966.35, 763.77, 53.78))
                            return false;
                        var npc = Host.GetNpcById(125485);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);

                        }
                        return false;
                    }
                    if (quest.Counts[1] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1972.45, 694.00, 25.38))
                            return false;
                        var npc = Host.GetNpcById(125489);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);

                        }
                        return false;
                    }
                    if (quest.Counts[2] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1762.59, 932.20, 55.19))
                            return false;
                        var npc = Host.GetNpcById(126009);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);

                        }
                        return false;
                    }

                    if (quest.Counts[0] == 1 && quest.Counts[1] == 1 && quest.Counts[2] == 1)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }
                }


                if (quest.Id == 49125)
                {
                    if (step == 8)
                    {
                        if (!Host.CommonModule.MoveTo(2673.68, 172.44, 1.35))
                            return false;
                        Host.MyUseGameObject(280347);
                        return false;
                    }
                }

                if (quest.Id == 49120)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2597.88, 469.72, 5.41))
                            return false;
                        var npc = Host.GetNpcById(128096);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            while (step == 0)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                Thread.Sleep(2000);
                                step = 0;
                                foreach (var questCount in quest.Counts)
                                {
                                    step = step + questCount;
                                }
                            }
                        }
                        return false;
                    }
                }

                if (quest.Id == 47925)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2158.16, 848.14, 44.53))
                            return false;
                        var npc = Host.GetNpcById(124774);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Host.Wait(5000);
                        }
                        return false;
                    }

                    if (step == 1)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 49067)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2592.31, 475.99, 7.81))
                            return false;
                        var npc = Host.GetNpcById(127961);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Host.Wait(5000);
                        }
                        return false;
                    }
                }

                if (quest.Id == 47924)
                {
                    if (step == 5)
                    {
                        if (!Host.CommonModule.MoveTo(2361.98, 725.30, 2.14))
                            return false;
                        var npc = Host.GetNpcById(124933);

                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Host.Wait(15000);
                        }
                        return false;
                    }

                    if (step == 6)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                }

                if (quest.Id == 49064)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2144.78, 524.19, 18.02))
                            return false;
                        var npc = Host.GetNpcById(127958);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                        }
                        return false;
                    }
                }

                if (quest.Id == 49185)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1986.38, 1375.97, 15.90))
                            return false;
                        var npc = Host.GetNpcById(127961);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 1);
                        }
                        return false;
                    }
                }

                if (quest.Id == 49781)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(134435);
                        if (npc != null)
                        {
                            var result = Host.SpellManager.CastPetSpell(npc.Guid, 264034);
                            if (result != ESpellCastError.SUCCESS)
                                Host.log("Не смог использовать пет скилл 1 " + result);
                            Host.Wait(20000);
                            result = Host.SpellManager.CastPetSpell(npc.Guid, 264107);
                            if (result != ESpellCastError.SUCCESS)
                                Host.log("Не смог использовать пет скилл 2 " + result);
                            Host.Wait(25000);
                            var target = Host.GetNpcById(134436);
                            if (target != null)
                                while ((target as Unit).IsAlive)
                                {
                                    if (!(target as Unit).IsAlive)
                                        break;
                                    if (!Host.MainForm.On)
                                        return false;
                                    result = Host.SpellManager.CastPetSpell(npc.Guid, 264225, target);
                                    if (result != ESpellCastError.SUCCESS)
                                        Host.log("Не смог использовать пет скилл 2 " + result);
                                    Thread.Sleep(2000);

                                }
                            Host.Wait(60000);

                        }

                        npc = Host.GetNpcById(134395);
                        if (npc != null)
                        {
                            if (!Host.CommonModule.MoveTo(1662.79, 2172.70, 69.83))
                                return false;
                            MyUseSpellClick(npc as Unit);
                            Host.Wait(15000);
                        }


                    }


                    return false;
                }

                if (quest.Id == 49780)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1718.72, 2106.31, 54.59))
                            return false;
                        var item = Host.MyGetItem(156480);
                        Host.MyUseItemAndWait(item);
                        return false;
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1753.84, 2142.55, 54.20))
                            return false;
                        var item = Host.MyGetItem(156480);
                        Host.MyUseItemAndWait(item);
                        return false;
                    }
                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(1849.43, 2083.46, 54.75))
                            return false;
                        var item = Host.MyGetItem(156480);
                        Host.MyUseItemAndWait(item);
                        return false;
                    }
                    if (step == 6)
                    {
                        if (!Host.CommonModule.MoveTo(1940.22, 2138.42, 54.81))
                            return false;
                        var item = Host.MyGetItem(156480);
                        Host.MyUseItemAndWait(item);
                        return false;
                    }
                }

                if (quest.Id == 49778)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1665.55, 2176.58, 69.70))
                            return false;
                        var npc = Host.GetNpcById(130930);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Thread.Sleep(10000);
                        }
                        return false;
                    }

                }

                if (quest.Id == 49777)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1591.41, 2316.73, 67.18))
                            return false;
                        var npc = Host.GetNpcById(131135);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                        }
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1759.72, 2325.62, 60.19))
                            return false;
                        var npc = Host.GetNpcById(131129);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                        }
                        return false;
                    }
                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1857.31, 2209.83, 59.32))
                            return false;
                        var npc = Host.GetNpcById(131132);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                        }
                        return false;
                    }
                }

                if (quest.Id == 13562 && quest.State != EQuestState.Complete) //
                {
                    if (!Host.CommonModule.MoveTo(6746.56, 43.66, 48.29))
                        return false;

                    Zone zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 1000);
                    List<uint> farmMobIds = new List<uint>();
                    farmMobIds.Add(194179);
                    Host.FarmModule.SetFarmProps(zone, farmMobIds);
                    //int badRadius = 0;
                    while (Host.MainForm.On
                           && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                           && quest.State != EQuestState.Complete
                           && Host.FarmModule.readyToActions
                           && Host.FarmModule.farmState == FarmState.FarmProps)
                    {
                        if (Host.MyIsNeedRepair())
                            break;
                        Thread.Sleep(100);
                        /* if (host.FarmModule.bestProp == null && host.Me.HpPercents > 80)
                             badRadius++;
                         else
                             badRadius = 0;*/
                        /*   if (host.FarmModule.bestMob != null)
                               badRadius = 0;*/

                        /* if (Host.FarmModule.bestProp == null && Host.Me.HpPercents > 80)
                             badRadius++;
                         else
                             badRadius = 0;
                         if (badRadius > 100)
                         {
                             var findPoint = farmLoc;
                             if (questPoiPoints.Count > 0)
                                 findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                             Host.log("Не могу найти GameObject, подбегаю в центр зоны " + Host.Me.Distance(findPoint));
                             Host.CommonModule.MoveTo(findPoint);
                         }*/
                    }

                    Host.FarmModule.StopFarm();
                    Thread.Sleep(1000);
                    return false;
                }

                if (quest.Id == 13998)
                {
                    if (!Host.CommonModule.MoveTo(-1091.94, -2936.66, 92.47))
                        return false;
                    foreach (var item in Host.ItemManager.GetItems())
                    {
                        if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                            item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                            item.Place == EItemPlace.InventoryItem)
                            if (item.Id == 46789)//Грибы
                            {
                                Host.MyUseItemAndWait(item);
                            }
                    }
                }


                if (quest.Id == 49667)
                {
                    if (step == 1)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                }

                if (quest.Id == 51795)
                {
                    if (Host.MapID == 0)
                    {

                        if (step == 2)
                        {
                            MyComliteQuest(quest);
                            return false;
                        }

                        if (!Host.CommonModule.MoveTo(-8374.12, 1334.61, 5.22))
                            return false;
                        Host.MyCheckIsMovingIsCasting();
                        var npc = Host.GetNpcById(108920);
                        if (npc != null)
                        {
                            if (Host.MyDialog(npc, 1))
                            {
                                while (Host.MapID == 0)
                                {
                                    if (!Host.MainForm.On)
                                        return false;
                                    Thread.Sleep(1000);
                                    Host.log("Ожидаю приглашения " + " " + Host.LFGStatus.Reason, Host.LogLvl.Important);
                                    if (Host.LFGStatus.Reason == ELfgUpdateType.SUSPENDED_QUEUE)
                                    {
                                        Thread.Sleep(2000);

                                        if (!Host.LFGStatus.Proposal.Accept())
                                            Host.log("Не смог принять " + Host.LFGStatus.Reason + "   " + Host.GetLastError(), Host.LogLvl.Error);
                                        while (Host.GameState == EGameState.Ingame)
                                        {
                                            if (!Host.MainForm.On)
                                                return false;
                                            Thread.Sleep(1000);
                                            Host.log("Ожидаю вход " + " " + Host.LFGStatus.Reason + "  " + Host.GameState, Host.LogLvl.Important);
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    while (Host.MapID == 1760)
                    {
                        Host.FarmModule.farmState = FarmState.Disabled;
                        Thread.Sleep(5000);
                        if (!Host.MainForm.On)
                            return false;
                        Host.log("Ожидаю в данже " + Host.LFGStatus.Reason + "  " + Host.Scenario.CurrentStep + "  " + Host.Scenario.ScenarioComplete + " " + Host.Scenario.ScenarioID, Host.LogLvl.Important);
                        WaitTeleport = true;
                        if (Host.ClientAfk)
                            Host.Jump();
                        if (Host.Scenario.CurrentStep == 3591)
                        {

                            /*    var npc = Host.GetNpcById(126795);
                                if (npc != null && Host.Me.Distance(1464.74, 227.69, 61.59) < 50)
                                {
                                    Host.CommonModule.MoveTo(1711.67, 239.60, 62.60);
                                    Host.CommonModule.MoveTo(npc);
                                    MyUseSpellClick(npc);
                                    Host.Wait(30000);
                                }*/

                            if (Host.Me.IsAlive && !Host.Me.IsDeadGhost)
                            {
                                Host.Wait(50000);
                                Host.CommonModule.MoveTo(1711.67, 239.60, 62.60);
                                Host.CommonModule.MoveTo(1693.04, 238.98, 62.60);

                                while (Host.GameState == EGameState.Ingame)
                                {
                                    if (!Host.Me.IsAlive || Host.Me.IsDeadGhost)
                                        return false;
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
                                Host.Wait(15000);
                                return false;

                            }
                        }

                    }

                    return false;
                }

                if (quest.Id == 51796) //Битва за ЛОрдеон
                {
                    if (Host.MapID == 1)
                    {
                        step = 0;
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }

                        if (step == 2)
                        {
                            if (!Host.CommonModule.MoveTo(1424.67, -4357.46, 73.92))
                                return false;

                            MyComliteQuest(quest);
                            return false;
                        }

                        if (!Host.CommonModule.MoveTo(1655.75, -4338.37, 26.36))
                            return false;
                        Host.MyCheckIsMovingIsCasting();
                        var npc = Host.GetNpcById(139093);
                        if (npc != null)
                        {
                            if (Host.MyDialog(npc, "I am ready to go to the Undercity"))
                            {
                                while (Host.MapID == 1)
                                {
                                    if (!Host.MainForm.On)
                                        return false;
                                    Thread.Sleep(1000);
                                    Host.log("Ожидаю приглашения " + " " + Host.LFGStatus.Reason, Host.LogLvl.Important);
                                    if (Host.LFGStatus.Reason == ELfgUpdateType.SUSPENDED_QUEUE)
                                    {
                                        Thread.Sleep(2000);

                                        if (!Host.LFGStatus.Proposal.Accept())
                                            Host.log("Не смог принять " + Host.LFGStatus.Reason + "   " + Host.GetLastError(), Host.LogLvl.Error);
                                        while (Host.GameState == EGameState.Ingame)
                                        {
                                            if (!Host.MainForm.On)
                                                return false;
                                            Thread.Sleep(1000);
                                            Host.log("Ожидаю вход " + " " + Host.LFGStatus.Reason + "  " + Host.GameState, Host.LogLvl.Important);
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    var checkParty = 0;
                    while (Host.MapID == 1760)
                    {
                        Host.FarmModule.farmState = FarmState.Disabled;
                        Thread.Sleep(5000);
                        if (!Host.MainForm.On)
                            return false;
                        Host.log("Ожидаю в данже " + Host.LFGStatus.Reason + "  CurrentStep:" + Host.Scenario.CurrentStep + "  " + Host.Scenario.ScenarioComplete + " " + Host.Scenario.ScenarioID, Host.LogLvl.Important);
                        WaitTeleport = true;

                        if (Host.Scenario.CurrentStep != 3688)
                        {
                            if (GroupOffline())
                            {
                                Host.log("Выхожу из пати, так как все оффлайн " + checkParty, Host.LogLvl.Important);
                                if (!Host.Me.IsDeadGhost && Host.Me.IsAlive)
                                    checkParty++;
                            }
                            if (Host.Group.GetMembers().Count == 1)
                            {

                                /* if (Host.Scenario.CurrentStep == 3654)
                                 {
                                     if (!Host.CommonModule.MoveTo(1700.01, 97.28, -62.20))
                                         return false;
                                     var zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 300);
                                     var farmMobIds = new List<uint>{4571, 5693, 139427, 11048};
                                     Host.FarmModule.SetFarmMobs(zone, farmMobIds);

                                     while (Host.Scenario.CurrentStep == 3654 && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmMobs)
                                     {
                                         Thread.Sleep(100);

                                     }

                                     Host.FarmModule.StopFarm();
                                     Thread.Sleep(1000);
                                 }*/


                                // return false;
                                Host.log("Выхожу из пати, так как остался один " + checkParty, Host.LogLvl.Important);
                                if (!Host.Me.IsDeadGhost && Host.Me.IsAlive)
                                    checkParty++;
                            }


                            if (checkParty > 5)
                            {
                                var result = Host.Group.Leave();
                                if (result != EPartyResult.Ok)
                                {
                                    Host.log("Не удалось выйти из пати " + result + " " + Host.GetLastError(), Host.LogLvl.Error);

                                }
                                Thread.Sleep(10000);
                                return false;
                            }
                        }






                        if (Host.ClientAfk)
                            Host.Jump();
                        if (Host.Scenario.CurrentStep == 3688)
                        {
                            if (Host.Me.IsAlive && !Host.Me.IsDeadGhost)
                            {
                                Host.CommonModule.MoveTo(1711.67, 239.60, 62.60);
                                Host.CommonModule.MoveTo(1693.04, 238.98, 62.60);

                                Thread.Sleep(1000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
                                Host.Wait(20000);
                                return false;

                            }
                        }
                        foreach (var gameObject in Host.GetEntities())
                        {
                            if (gameObject.Id == 131087)
                            {
                                Host.log("  " + gameObject.Name);
                                Host.MoveTo(gameObject.Location);
                            }
                        }
                    }
                    // Thread.Sleep(5000);
                    return false;
                }


                if (quest.Id == 52946)
                {
                    if (step == 0)
                    {
                        if (Host.Me.Location.Z > 130 && Host.Me.Distance2D(new Vector3F(-8231.48, 424.25, 117.83)) < 20)
                        {
                            Host.ForceFlyTo(-8231.48, 424.25, 117.83);
                        }
                        if (Host.Me.Distance(-8231.48, 424.25, 117.83) < 20)
                        {
                            Host.CanselForm();
                            Host.CommonModule.MyUnmount();
                            foreach (var entity in Host.GetEntities<GameObject>())
                            {
                                if (entity.Id == 207695)
                                {
                                    Host.CommonModule.MoveTo(entity);

                                    entity.Use();
                                    Thread.Sleep(1000);
                                    while (Host.GameState != EGameState.Ingame)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Host.CommonModule.MoveTo(new Vector3F(-8231.48, 424.25, 117.83));
                        }
                    }


                    if (Host.Area.Id == 5034)
                    {
                        if (Host.Me.Distance(-8130.18, 220.17, 380.73) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(-9442.05, -936.49, 262.68);
                            Host.Jump();
                            Host.ForceFlyTo(-8130.18, 220.17, 380.73);

                        }
                    }

                    if (Host.Area.Id == 9310)
                    {
                        var steps = 0;
                        foreach (var questCount in quest.Counts)
                        {
                            steps = steps + questCount;
                        }

                        if (steps == 2)
                        {
                            MyComliteQuest(quest);
                        }
                        if (Host.Me.Distance(-7073.26, 1272.43, -92.15) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(-7351.11, 1133.87, 67.91);
                            Host.Jump();
                            Host.ForceFlyTo(-7073.26, 1272.43, -92.15);

                        }
                    }


                    return false;
                }


                if (quest.Id == 53028)//Умирающий мир[53028] 
                {
                    if (Host.Area.Id == 1637)
                    {

                        if (Host.Me.Distance(2035.46, -4360.88, 98.05) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(1510.19, -4408.19, 180.33);
                            Host.Jump();
                            Host.ForceFlyTo(1923.29, -4377.17, 195.51);
                            Host.Jump();
                            Host.ForceFlyTo(2035.46, -4360.88, 98.05);
                        }

                        if (Host.Me.Distance(2035.46, -4360.88, 98.05) < 20)
                        {
                            Host.CanselForm();
                            Host.CommonModule.MyUnmount();
                            foreach (var entity in Host.GetEntities<GameObject>())
                            {
                                if (entity.Id == 207687)
                                {
                                    Host.CommonModule.MoveTo(entity);
                                    entity.Use();
                                    Thread.Sleep(2000);
                                    while (Host.GameState != EGameState.Ingame)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                    }



                    if (Host.Area.Id == 5034)
                    {
                        if (Host.Me.Distance(-8130.18, 220.17, 380.73) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(-9442.05, -936.49, 262.68);
                            Host.Jump();
                            Host.ForceFlyTo(-8130.18, 220.17, 380.73);

                        }
                    }

                    if (Host.Area.Id == 9310)
                    {


                        var steps = 0;
                        foreach (var questCount in quest.Counts)
                        {
                            steps = steps + questCount;
                        }

                        if (steps == 2)
                        {
                            MyComliteQuest(quest);
                        }
                        if (Host.Me.Distance(-7073.26, 1272.43, -92.15) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(-7351.11, 1133.87, 67.91);
                            Host.Jump();
                            Host.ForceFlyTo(-7073.26, 1272.43, -92.15);

                        }
                    }

                    return false;
                }


                if (quest.Id == 50536)
                {
                    if (step == 1)
                    {

                    }


                }

                if (quest.Id == 48854)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1704.13, 1818.43, 12.69, 10))
                            return false;
                        var npc = Host.GetNpcById(127216);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                        }

                        return false;
                    }

                    if (step == 1)
                    {
                        var npc = Host.GetNpcById(127384);
                        if (npc != null)
                        {
                            Host.SpellManager.UseSpellClick(npc as Unit);
                            Thread.Sleep(10000);
                        }
                        else
                        {

                        }

                        return false;
                    }
                }

                if (quest.Id == 48823)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1703.29, 1817.10, 12.65, 20))
                            return false;

                        var npc = Host.GetNpcById(127215);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                        }
                        Thread.Sleep(10000);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1598.24, 1823.70, 16.37, 2))
                            return false;
                        var item = Host.MyGetItem(152727);
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(3000);
                        return false;
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(1588.99, 1762.47, 14.57, 2))
                            return false;
                        var item = Host.MyGetItem(152727);
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(3000);
                        return false;
                    }
                    if (step == 5)
                    {
                        if (!Host.CommonModule.MoveTo(1536.15, 1724.70, 14.59, 2))
                            return false;
                        var item = Host.MyGetItem(152727);
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(3000);
                        return false;
                    }

                    if (step == 7)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 51991)
                {
                    var isRiding = false;
                    foreach (var entity in Host.GetEntities())
                    {
                        if (entity.Id == 139409 && Host.Me.Distance(entity) < 5)
                        {
                            isRiding = true;
                        }
                    }


                    var percent = Host.GetPercent(100, quest.Counts[0]) + Host.GetPercent(34, quest.Counts[1]) + Host.GetPercent(20, quest.Counts[2]);
                    Host.log("Процент " + percent);

                    if (isRiding)
                    {
                        Unit best = null;
                        double dist = double.MaxValue;
                        foreach (var u in Host.GetEntities<Unit>())
                        {
                            if (u.Id == 139772 && Host.GetEntity(Host.ActiveMover).Distance(u) < dist)
                            {
                                best = u;
                                dist = Host.GetEntity(Host.ActiveMover).Distance(u);
                            }
                        }

                        if (best != null)
                        {
                            Host.SetMoveStateForClient(true);
                            Host.SetCurrentTurnPoint(best.Location, true);
                            Thread.Sleep(2000);
                            Host.SetCurrentTurnPoint(Vector3F.Zero, false);
                            Host.SetMoveStateForClient(false);
                        }
                        Thread.Sleep(5000);
                        Entity pet = null;
                        foreach (var entity in Host.GetEntities().OrderBy(i => Host.Me.Distance(i)))
                        {
                            if (entity.Id == 139409)
                            {
                                pet = entity;
                                break;
                            }
                        }

                        if (pet != null)
                        {
                            var res = Host.SpellManager.CastPetSpell(pet.Guid, 274584);
                            if (res != ESpellCastError.SUCCESS)
                                Host.log("Не смог использовать скилл " + res + " " + Host.GetLastError(), Host.LogLvl.Error);
                        }

                        return false;
                    }

                    if (percent >= 98 && !isRiding)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }



                    if (!isRiding && percent <= 98)
                    {
                        if (!Host.CommonModule.MoveTo(1220.44, 4738.00, 71.58))
                            return false;
                        MyUseSpellClick(143377);
                        Thread.Sleep(15000);
                    }


                    return false;

                }

                if (quest.Id == 51211)//Сердце Азерот[51211] 
                {
                    if (Host.Area.Id == 9310)
                    {
                        Host.CanselForm();
                        Host.CommonModule.MyUnmount();
                        if (!Host.MoveTo(-7080.45, 1237.62, -110.92))
                            return false;


                        foreach (var entity in Host.GetEntities<GameObject>())
                        {
                            if (entity.Id == 289521)
                            {
                                Host.CommonModule.MoveTo(entity);
                                Thread.Sleep(2000);
                                entity.Use();
                                Thread.Sleep(2000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                        }
                    }

                    if (Host.Area.Id == 9667)
                    {
                        if (!Host.CommonModule.ForceMoveTo2(new Vector3F(-8329.75, 1754.63, 314.55)))
                            return false;

                        if (step == 3)
                        {
                            MyComliteQuest(quest);
                            return false;
                        }

                        var npc = Host.GetNpcById(136907);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 1);
                        }
                    }
                    return false;
                }

                if (quest.Id == 52428) //Усиление Сердца[52428] 
                {
                    if (Host.MapID == 1929)
                    {
                        if (step == 10)
                        {
                            Host.ForceMoveTo(-8358.08, 1754.23, 314.88);
                            Thread.Sleep(1000);
                            Host.SpellManager.CastSpell(278756);
                            Thread.Sleep(5000);
                            while (step == 10)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                Thread.Sleep(100);
                                step = 0;
                                foreach (var questCount in quest.Counts)
                                {
                                    step = step + questCount;
                                }
                            }
                        }

                        if (step == 11)
                        {
                            Host.ForceMoveTo(-8326.79, 1753.26, 313.61);
                            Thread.Sleep(1000);
                            MyComliteQuest(quest);
                            return false;
                        }

                        foreach (var gameObject in Host.GetEntities<GameObject>())
                        {
                            if (gameObject.Id != 293853)
                                continue;
                            Host.CommonModule.MoveTo(gameObject, 3);
                            Thread.Sleep(1000);
                            Host.SpellManager.CastSpell(275825);
                            Thread.Sleep(5000);
                        }
                    }
                    return false;
                }

                if (quest.Id == 51403)
                {
                    if (Host.MapID == 1929)
                    {
                        foreach (var gameObject in Host.GetEntities<GameObject>())
                        {
                            if (gameObject.Id != 294313)
                                continue;
                            Host.ForceComeTo(gameObject, 3);
                            Thread.Sleep(1000);
                            gameObject.Use();
                            Thread.Sleep(5000);
                        }
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(-8244.30, 1240.03, 5.23)))
                            return false;
                        MyComliteQuest(quest);
                        return false;
                    }

                    return false;
                }

                if (quest.Id == 46727)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(-8376.00, 248.69, 155.35)))
                            return false;
                        Host.Wait(10000);
                        return false;
                    }

                    if (step == 1)
                    {
                        var npc = Host.GetNpcById(139645);
                        if (npc != null)
                        {
                            MyUseSpellClick(npc);
                            Host.Wait(25000);
                        }
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51341)
                {
                    if (step == 1)
                    {
                        if (Host.Me.Distance(132.72, -2710.13, 28.74) > 10)
                        {
                            Host.Wait(10000);
                            return false;
                        }

                        if (!Host.CommonModule.MoveTo(new Vector3F(141.41, -2715.66, 29.19)))
                            return false;
                    }
                }

                if (quest.Id == 51573)
                {
                    if (step == 0)
                    {
                        if (Host.Me.Distance(914.02, 3730.48, 68.50) > 100)
                        {
                            if (!Host.CommonModule.MoveTo(new Vector3F(914.02, 3730.48, 68.50)))
                                return false;

                        }
                        var npc = Host.GetNpcById(126576);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Thread.Sleep(1000);

                        }
                        return false;
                    }

                    if (step == 1)
                    {
                        var convoy = Host.GetNpcById(126576);
                        while (convoy != null)
                        {
                            if (!Host.MainForm.On)
                                return false;
                            Thread.Sleep(1000);
                            step = 0;
                            foreach (var questCount in quest.Counts)
                            {
                                step = step + questCount;
                            }
                            if (step == 2)
                                break;

                            convoy = Host.GetNpcById(126576);
                            if ((convoy as Unit).GetThreats().Count > 0 && Host.FarmModule.BestMob == null)
                            {
                                foreach (var threatItem in (convoy as Unit).GetThreats())
                                {
                                    Host.FarmModule.BestMob = threatItem;
                                    break;
                                }
                            }
                            if (Host.Me.Distance(convoy) > 5 && Host.FarmModule.BestMob == null)
                                Host.CommonModule.MoveToConvoy(convoy, 2);
                            if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null && (convoy as Unit).Target.HpPercents < 99)
                                Host.FarmModule.BestMob = (convoy as Unit).Target;
                            Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + (convoy as Unit).GetThreats().Count);
                        }
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 46728)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(-8281.54, 1326.40, 5.24)))
                            return false;
                        if (Host.Me.Location.Z > 7)
                        {
                            /* Host.CommonModule.MyUnmount();
                             Host.CanselForm();*/
                            Host.ForceFlyTo(-8281.54, 1326.40, 5.24);
                        }

                        var npc = Host.GetNpcById(120590);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);

                            while (Host.GameState != EGameState.Ingame)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                Thread.Sleep(1000);
                            }
                            Host.Wait(100000);

                        }
                        return false;
                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47098)
                {

                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(121239);
                        if (npc != null)
                        {
                            MyUseSpellClick(npc);
                            Host.Wait(5000);
                        }

                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(162.86, -2708.87, 28.88))
                            return false;
                        Host.MyUseGameObject(271938);
                        Host.Wait(25000);
                        return false;
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(151.07, -2713.42, 28.12))
                            return false;
                        Host.MyUseGameObject(290827);
                        return false;
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(101.80, -2665.41, 29.19))
                            return false;
                    }

                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(104.82, -2688.35, 29.19))
                            return false;
                        Host.MyUseGameObject(290126);
                        return false;
                    }

                    if (step == 5)
                    {
                        if (!Host.CommonModule.MoveTo(182.97, -2683.26, 29.36))
                            return false;
                        Host.MyUseGameObject(281902);
                        Host.Wait(10000);
                        return false;
                    }

                    if (step == 6)
                    {
                        if (!Host.CommonModule.MoveTo(110.88, -2653.21, 10.27))
                            return false;

                    }

                    if (step == 6)
                    {
                        Host.FarmModule.farmState = FarmState.Disabled;
                        if (!Host.CommonModule.MoveTo(new Vector3F(242.85, -2813.13, 0.00)))
                            return false;

                    }

                    if (step == 7)
                    {
                        Host.FarmModule.farmState = FarmState.Disabled;
                        if (!Host.CommonModule.MoveTo(new Vector3F(242.85, -2813.13, 0.00)))
                            return false;
                        MyUseSpellClick(124030);
                        Host.Wait(90000);
                        return false;
                    }

                    if (step == 8)
                        MyComliteQuest(quest);

                    return false;
                }


                if (quest.Id == 47099)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1039.09, -597.05, 1.36))
                            return false;
                        Host.Wait(10000);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1122.41, -621.72, 17.53)))
                            return false;
                        Host.Wait(10000);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1179.05, -587.69, 31.50))
                            return false;
                        Host.Wait(10000);
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(1151.93, -473.78, 30.43))
                            return false;
                        Host.Wait(10000);
                    }

                    if (step == 4)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 48533)
                {
                    if (quest.Counts[0] == 8)
                        MyComliteQuest(quest);
                }

                if (quest.Id == 48532)
                {
                    if (quest.Counts[0] == 6)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                }

                if (quest.Id == 48857)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1703.07, 1822.63, 12.85)))
                            return false;
                        var npc = Host.GetNpcById(127212);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(2000);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1536.16, 1912.59, 8.61)))
                            return false;


                        return false;
                    }
                }

                if (quest.Id == 47186)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1135.70, -535.54, 17.53)))
                            return false;
                        var npc = Host.GetNpcById(137066);
                        Host.MyDialog(npc, 0);

                    }
                    return false;
                }

                if (quest.Id == 46729)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1065.77, -473.01, 13.68)))
                            return false;
                        Host.Wait(55000);
                    }



                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(1068.09, -486.47, 9.70)))
                            return false;
                        var npc = Host.GetNpcById(122370);
                        Host.MyDialog(npc, 0);
                        Host.Wait(90000);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 53031) //Воля Вестника[53031]   
                {
                    if (Host.MapID == 1929)
                    {
                        foreach (var gameObject in Host.GetEntities<GameObject>())
                        {
                            if (gameObject.Id != 294538)
                                continue;
                            Host.ForceComeTo(gameObject, 3);
                            Thread.Sleep(1000);
                            gameObject.Use();
                            Thread.Sleep(5000);
                        }
                    }

                    if (Host.Zone.Id == 5166)
                    {
                        Host.Wait(35000);
                        if (!Host.CommonModule.MoveTo(1891.94, -4485.91, 23.43))
                            return false;

                    }

                    if (Host.Zone.Id == 5167)
                    {
                        Host.log("Тест " + Host.Me.Distance(1425.64, -4359.72, 73.92));
                        if (Host.Me.Distance(1425.04, -4360.11, 73.92) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(1900.43, -4542.52, 103.35);
                            Host.Jump();
                            Host.ForceFlyTo(1459.33, -4416.26, 140.95);
                            Host.Jump();
                            Host.ForceFlyTo(1425.04, -4360.11, 73.92);
                        }



                    }

                    if (Host.Me.Distance(1425.04, -4360.11, 73.92) < 20)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }


                    if (step == 1 && Host.MapID == 1)
                    {
                        if (Host.Me.Distance(1444.83, -4500.89, 18.31) < 50)
                            if (!Host.CommonModule.MoveTo(new Vector3F(1564.95, -4401.89, 16.49)))
                                return false;
                        Host.FlyForm();
                        Host.ForceFlyTo(1500.64, -4404.32, 157.39);
                        Host.ForceFlyTo(1442.41, -4382.08, 140.42);
                        Host.ForceFlyTo(1423.19, -4358.88, 73.92);

                    }

                    return false;
                }

                if (quest.Id == 51443) //Поставленная задача[51443]    
                {
                    step = 0;
                    foreach (var questCount in quest.Counts)
                    {
                        step = step + questCount;
                    }

                    if (step == 0)
                    {
                        if (Host.Me.Distance(1590.56, -4383.61, 18.82) > 20)
                        {
                            Host.FlyForm();
                            Host.Jump();
                            Host.ForceFlyTo(1461.25, -4398.24, 130.57);
                            Host.Jump();
                            Host.ForceFlyTo(1590.56, -4383.61, 18.82);
                        }

                        if (Host.Me.Distance(1590.56, -4383.61, 18.82) < 20)
                        {
                            Host.CanselForm();
                            Host.ForceMoveTo(1614.20, -4371.68, 24.63);

                            Host.ForceMoveTo(1614.74, -4366.76, 24.62);

                            Host.TurnDirectly(new Vector3F(1617.39, -4362.65, 24.62));
                            Thread.Sleep(1000);
                            Host.SetMoveStateForClient(true);
                            Host.MoveForward(true);
                            Thread.Sleep(5000);
                            Host.MoveForward(false);
                            Host.SetMoveStateForClient(false);
                            Thread.Sleep(5000);
                            //Host.Wait(000);
                            return false;
                            /*   if (!Host.CommonModule.MoveTo(1622.83, -4374.60, 24.62))
                                   return false;*/
                        }
                    }

                    if (step == 1 && Host.Me.Distance(1577.56, -4455.68, 16.57) > 20)
                    {

                        Host.ForceMoveTo(1625.36, -4366.46, 24.60);
                        Host.ForceMoveTo(1622.26, -4375.56, 24.62);
                        Host.ForceMoveTo(1612.34, -4373.04, 24.63);
                        Host.ForceMoveTo(1594.19, -4382.94, 19.44);

                        /*  if (Host.Me.Distance() < 20)
                              Host.ForceMoveTo(1621.78, -4375.23, 24.62);
                          if (Host.Me.Distance() < 20)
                              Host.ForceMoveTo(1577.56, -4455.68, 16.57);
                          if (Host.Me.Distance() < 20)
                              Host.ForceMoveTo(1577.56, -4455.68, 16.57);*/
                        if (!Host.CommonModule.MoveTo(1569.53, -4426.01, 14.45))
                            return false;
                        Host.ForceMoveTo(1577.56, -4455.68, 16.57);

                    }
                    return false;
                }

                if (quest.Id == 50769) // Побег из Штормграда[50769] 
                {

                    if (step == 1 || step == 2 && Host.MapID == 1)
                    {
                        if (Host.Me.Distance(1576.63, -4451.88, 16.03) < 10)
                            Host.ForceMoveTo(1568.28, -4422.23, 14.90);
                        Thread.Sleep(5000);
                        Host.FlyForm();

                        if (Host.Me.Distance(1405.34, -4359.41, 73.92) > 20)
                        {
                            Thread.Sleep(1000);
                            Host.FlyForm();
                            Thread.Sleep(1000);
                            Host.Jump();
                            Host.ForceFlyTo(1539.88, -4413.84, 114.75);
                            Host.Jump();
                            Host.ForceFlyTo(1458.34, -4388.60, 112.21);
                            Host.Jump();
                            Host.ForceFlyTo(1412.03, -4359.99, 84.42);
                        }
                        else
                        {
                            Host.CanselForm();
                            Host.CommonModule.MyUnmount();
                            foreach (var vehicle in Host.GetEntities<Vehicle>())
                            {
                                if (vehicle.Id != 135211)
                                {
                                    continue;
                                }

                                Host.ForceComeTo(vehicle, 3);
                                Host.SpellManager.UseSpellClick(vehicle);
                                Thread.Sleep(40000);
                                return false;
                            }
                        }
                    }


                    if (step == 3)
                    {
                        if (Host.Me.Distance(-8328.80, 1364.90, 12.94) < 10)
                        {
                            Thread.Sleep(10000);
                            var npc = Host.GetNpcById(134092);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, "Not");
                                Host.MyDialog(npc, "нет");

                                Thread.Sleep(10000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
                                Host.Wait(260000);
                                return false;
                                //Host.Wait(260000);
                            }
                        }

                        if (Host.Me.Distance(-2154.19, 802.68, 5.93) < 40)
                        {
                            MyComliteQuest(quest);
                            return false;
                        }
                    }

                    foreach (var gameObject in Host.GetEntities<GameObject>())
                    {
                        if (gameObject.Id != 289645)
                            continue;
                        if ((gameObject.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) != 0)
                            continue;
                        Host.ForceComeTo(gameObject, 3);
                        Thread.Sleep(1000);
                        if (!gameObject.Use())
                            Host.log("Не смог использовать  " + Host.GetLastError(), Host.LogLvl.Error);
                        Thread.Sleep(5000);
                        if (Host.CanPickupLoot())
                        {
                            if (!Host.PickupLoot())
                            {
                                Host.log("Не смог поднять дроп " + "   " + Host.GetLastError(), Host.LogLvl.Error);
                            }
                        }
                        else
                        {
                            Host.log("Окно лута не открыто " + Host.GetLastError(), Host.LogLvl.Error);
                        }
                    }

                    if (step == 2)
                    {
                        Host.log("Сценарий " + Host.Scenario.CurrentStep);
                        while (Host.Scenario.CurrentStep == 3718)
                        {
                            if (!Host.MainForm.On)
                                return false;
                            Thread.Sleep(5000);
                            Host.log("Ожидаю полет");
                            Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        }

                        if (Host.Scenario.CurrentStep == 3719)
                        {
                            foreach (var gameObject in Host.GetEntities<GameObject>())
                            {
                                if (gameObject.Id != 281481)
                                    continue;
                                Host.CommonModule.MoveTo(gameObject, 3);
                                Thread.Sleep(1000);
                                if (!gameObject.Use())
                                    Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(1000);
                            }

                            foreach (var gameObject in Host.GetEntities<GameObject>())
                            {
                                if (gameObject.Id != 281483)
                                    continue;
                                Host.CommonModule.MoveTo(gameObject, 1);
                                Thread.Sleep(5000);
                            }
                        }

                        if (Host.Scenario.CurrentStep == 3720)
                        {
                            Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                            Host.CanselForm();
                            Thread.Sleep(5000);
                            if (Host.Me.Distance(-8717.08, 1003.23, 45.40) < 30)
                            {
                                Thread.Sleep(10000);
                                Host.ForceMoveTo(-8734.77, 1000.23, 44.15);
                                Host.ForceMoveTo(-8744.32, 985.90, 44.15);
                                Host.ForceMoveTo(-8742.57, 976.91, 45.40);
                                return false;
                            }

                            var npc = Host.GetNpcById(134037);
                            if (npc != null && (npc as Unit).Target != null)
                            {
                                Host.FarmModule.BestMob = (npc as Unit).Target;
                                return false;
                            }
                            if (!Host.CommonModule.MoveTo(-8689.48, 905.75, 53.73))
                                return false;
                        }

                        if (Host.Scenario.CurrentStep == 3721)
                        {
                            if (!Host.CommonModule.MoveTo(-8689.48, 905.75, 53.73))
                                return false;

                            foreach (var gameObject in Host.GetEntities<GameObject>())
                            {
                                if (gameObject.Id != 281508)
                                    continue;
                                Host.ForceComeTo(gameObject, 3);
                                Thread.Sleep(1000);
                                if (!gameObject.Use())
                                    Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(5000);
                            }

                            if (!Host.CommonModule.ForceMoveTo2(new Vector3F(-8673.12, 918.13, 53.73)))
                                return false;
                            var npc = Host.GetNpcById(134120);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, 0);
                                //Host.MyDialog(npc, "Что ты имеешь в виду");
                                while (Host.Scenario.CurrentStep == 3721)
                                {
                                    if (!Host.MainForm.On)
                                        return false;
                                    Thread.Sleep(1000);
                                }
                            }
                        }

                        if (Host.Scenario.CurrentStep == 3722)
                        {
                            var npc = Host.GetNpcById(134120);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, 0);
                                while (Host.Scenario.CurrentStep == 3721)
                                {
                                    if (!Host.MainForm.On)
                                        return false;
                                    Thread.Sleep(1000);
                                }

                            }
                            if (Host.Me.Distance(-8673.31, 917.38, 53.73) < 20)
                            {
                                Host.CommonModule.MoveTo(-8690.47, 905.47, 53.73);
                            }
                            var convoy = Host.GetNpcById(134037);
                            var isuse = false;
                            while (convoy != null)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                if (Host.Scenario.CurrentStep != 3722)
                                    return false;
                                Thread.Sleep(250);
                                convoy = Host.GetNpcById(134037);
                                if ((convoy as Unit).GetThreats().Count > 0 && Host.FarmModule.BestMob == null)
                                {
                                    foreach (var threatItem in (convoy as Unit).GetThreats())
                                    {
                                        Host.FarmModule.BestMob = threatItem;
                                        break;
                                    }
                                }
                                if (Host.Me.Distance(convoy) > 0 && Host.FarmModule.BestMob == null)
                                    Host.CommonModule.MoveToConvoy(convoy, 0);
                                if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null && (convoy as Unit).Target.HpPercents < 99)
                                    Host.FarmModule.BestMob = (convoy as Unit).Target;
                                Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + (convoy as Unit).GetThreats().Count);

                                if (Host.Me.Distance(-8733.11, 871.63, 53.73) < 10 && !isuse && Host.FarmModule.BestMob == null)
                                {
                                    Host.CommonModule.MoveTo(-8742.83, 884.26, 52.82);
                                }

                                if (Host.Me.Distance(-8651.85, 809.60, 44.15) < 10)
                                {
                                    Host.CommonModule.MoveTo(-8648.94, 795.96, 44.15);
                                }

                                foreach (var gameObject in Host.GetEntities<GameObject>())
                                {
                                    if (gameObject.Id != 287550)
                                        continue;
                                    if (Host.Me.Distance(gameObject) > 15)
                                        continue;
                                    if ((gameObject.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) != 0)
                                        continue;
                                    if (isuse)
                                        continue;
                                    Host.ForceComeTo(gameObject, 3);
                                    Thread.Sleep(1000);
                                    if (!gameObject.Use())
                                        Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                                    else
                                    {
                                        isuse = true;
                                    }
                                    Thread.Sleep(5000);
                                }

                                foreach (var gameObject in Host.GetEntities<Unit>())
                                {
                                    if (gameObject.Id != 139948)
                                        continue;
                                    if (Host.Me.Distance(gameObject) > 15)
                                        continue;
                                    /* if (!gameObject.IsUsable)
                                         continue;*/
                                    Host.ForceComeTo(gameObject, 3);
                                    Thread.Sleep(1000);
                                    if (!Host.SpellManager.UseSpellClick(gameObject))
                                        Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                                    Thread.Sleep(5000);
                                }
                            }
                            return false;
                        }

                        if (Host.Scenario.CurrentStep == 3731)
                        {


                            var convoy = Host.GetNpcById(134092);
                            while (convoy != null)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                if (Host.Scenario.CurrentStep != 3731)
                                    return false;
                                Thread.Sleep(100);

                                convoy = Host.GetNpcById(134092);

                                if (Host.Me.Distance(-8290.61, 1386.19, 5.52) < 10)
                                {
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                    Host.Wait(30000);
                                    Host.CommonModule.MoveTo(new Vector3F(-8331.95, 1366.91, 11.89));
                                    Host.Wait(10000);
                                }

                                if (Host.Me.Distance(convoy) > 0 && Host.FarmModule.BestMob == null && Host.FarmModule.farmState != FarmState.Disabled)
                                    Host.CommonModule.MoveToConvoy(convoy, 0);

                                if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null && (convoy as Unit).Target.HpPercents < 99)
                                {
                                    if (Host.Scenario.CurrentStep == 3727)
                                    {

                                    }
                                    else
                                    {
                                        Host.FarmModule.BestMob = (convoy as Unit).Target;
                                    }

                                }

                                Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + (convoy as Unit).GetThreats().Count);

                                if (Host.Me.Distance(-8328.80, 1364.90, 12.94) < 10)
                                {
                                    Thread.Sleep(10000);
                                    var npc = Host.GetNpcById(134092);
                                    if (npc != null)
                                    {
                                        Host.MyDialog(npc, 0);
                                        //  Host.MyDialog(npc, "нет");


                                        Thread.Sleep(10000);
                                        while (Host.GameState != EGameState.Ingame)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        Host.Wait(10000);
                                        Host.CommonModule.ResumeMove();
                                        return false;
                                    }
                                }

                            }

                            return false;
                        }
                        if (Host.Scenario.CurrentStep == 3727 || Host.Scenario.CurrentStep == 3729 || Host.Scenario.CurrentStep == 3730 || Host.Scenario.CurrentStep == 3788)//3730
                        {

                            var convoy = Host.GetNpcById(134038);
                            var dialog1 = false;
                            var dialog2 = false;
                            var dialog3 = false;
                            while (convoy != null)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                if (Host.Scenario.CurrentStep != 3727 && Host.Scenario.CurrentStep != 3729 && Host.Scenario.CurrentStep != 3730 && Host.Scenario.CurrentStep != 3788)
                                    return false;
                                Thread.Sleep(300);
                                if (Host.Scenario.CurrentStep == 3727)
                                {
                                    Host.CanselForm();
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                }
                                else
                                {
                                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                                }

                                convoy = Host.GetNpcById(134038);
                                /* if (Host.GetThreats(convoy as Unit).Count > 0 && Host.FarmModule.BestMob == null)
                                 {
                                     foreach (var threatItem in Host.GetThreats(convoy as Unit))
                                     {
                                         Host.FarmModule.BestMob = threatItem.Obj;
                                         break;
                                     }
                                 }*/
                                if (Host.Me.Distance(convoy) > 0 && Host.FarmModule.BestMob == null)
                                    Host.CommonModule.MoveToConvoy(convoy, 0, 0, true);

                                if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null && (convoy as Unit).Target.HpPercents < 99)
                                {
                                    if (Host.Scenario.CurrentStep == 3727)
                                    {

                                    }
                                    else
                                    {
                                        Host.FarmModule.BestMob = (convoy as Unit).Target;
                                    }

                                }

                                Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + (convoy as Unit).GetThreats().Count);

                                if (Host.Me.Distance(-8522.19, 667.93, 102.71) < 3 && !dialog3)
                                {
                                    Host.ForceMoveTo(-8518.12, 671.02, 103.47);
                                    //  Host.Wait(2000);
                                    Host.SetMoveStateForClient(true);
                                    Host.Wait(1000);
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Host.Wait(1000);

                                    Host.MoveForward(true);
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Thread.Sleep(1000);
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Thread.Sleep(1000);
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Thread.Sleep(1000);
                                    Host.Jump();
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Thread.Sleep(1000);
                                    Host.Jump();
                                    Host.TurnDirectly(new Vector3F(-8503.57, 671.18, 100.69));
                                    Thread.Sleep(1000);
                                    Host.MoveForward(false);
                                    Host.SetMoveStateForClient(false);
                                    Host.CommonModule.MoveTo(-8503.87, 678.35, 96.90);
                                    Host.Wait(30000);
                                }

                                if (Host.Me.Distance(-8492.33, 930.98, 97.34) < 10 && !dialog2)
                                {
                                    Thread.Sleep(20000);
                                    var npc = Host.GetNpcById(134039);
                                    if (npc != null)
                                    {
                                        if (Host.MyDialog(npc, 0))
                                            dialog2 = true;
                                    }
                                }

                                if (Host.Me.Distance(-8536.69, 484.44, 101.62) < 10 && !dialog1)
                                {
                                    var npc = Host.GetNpcById(134038);
                                    if (npc != null)
                                    {
                                        Thread.Sleep(1000);
                                        if (Host.MyDialog(npc, 0))
                                            dialog1 = true;


                                    }
                                }

                                if (Host.Me.Distance(-8500.13, 684.76, 100.82) < 10 && !dialog3)
                                {
                                    Thread.Sleep(10000);
                                    var npc = Host.GetNpcById(134038);
                                    if (npc != null)
                                    {
                                        if (Host.MyDialog(npc, 0))
                                            dialog3 = true;

                                    }
                                }
                            }
                            return false;
                        }


                    }

                    return false;
                }

                if (quest.Id == 46957)
                {

                    var convoy = Host.GetNpcById(132661);
                    while (convoy != null)
                    {
                        if (!Host.MainForm.On)
                            return false;

                        Thread.Sleep(1000);
                        convoy = Host.GetNpcById(132661);
                        if ((convoy as Unit).GetThreats().Count > 0 && Host.FarmModule.BestMob == null)
                        {
                            foreach (var threatItem in (convoy as Unit).GetThreats())
                            {
                                Host.FarmModule.BestMob = threatItem;
                                break;
                            }
                        }
                        if (Host.Me.Distance(convoy) > 3 && Host.FarmModule.BestMob == null)
                            Host.CommonModule.MoveToConvoy(convoy, 2);
                        if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null && (convoy as Unit).Target.HpPercents < 99)
                            Host.FarmModule.BestMob = (convoy as Unit).Target;
                        Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + (convoy as Unit).GetThreats().Count);

                    }

                    return false;
                }

                if (quest.Id == 46930)
                {
                    step = 0;
                    foreach (var questCount in quest.Counts)
                    {
                        step = step + questCount;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-1099.04, 837.87, 487.70))
                            return false;
                        var npc = Host.GetNpcById(135440);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            Host.Wait(10000);
                        }

                    }

                    foreach (var gameObject in Host.GetEntities<Unit>())
                    {
                        if (gameObject.Id != 135438)
                            continue;
                        if (Host.Me.Distance(gameObject) > 15)
                            continue;
                        /* if (!gameObject.IsUsable)
                             continue;*/
                        Host.ForceComeTo(gameObject, 2);
                        Thread.Sleep(1000);
                        if (!Host.SpellManager.UseSpellClick(gameObject))
                            Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                        Thread.Sleep(5000);
                        Host.Wait(45000);
                    }


                    return false;

                }

                if (quest.Id == 46931)
                {

                    step = 0;
                    foreach (var questCount in quest.Counts)
                    {
                        step = step + questCount;
                    }

                    if (step == 1)
                    {
                        if (Host.Me.Location.Z > 450)
                        {
                            if (!Host.CommonModule.MoveTo(-1068.48, 762.54, 482.79))
                                return false;
                            Host.ForceMoveTo(-1039.93, 767.48, 435.34);
                        }


                        if (!Host.CommonModule.MoveTo(-1007.15, 804.84, 437.66))
                            return false;
                        foreach (var gameObject in Host.GetEntities<Unit>())
                        {
                            if (gameObject.Id != 121000)
                                continue;
                            if (Host.Me.Distance(gameObject) > 15)
                                continue;
                            /* if (!gameObject.IsUsable)
                                 continue;*/
                            Host.ForceComeTo(gameObject, 2);
                            Host.CommonModule.MyUnmount();
                            Thread.Sleep(1000);

                            if (!Host.SpellManager.UseSpellClick(gameObject))
                                Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                            Thread.Sleep(5000);
                            //  Thread.Sleep(30000);
                        }
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(-1126.58, 771.91, 433.33))
                            return false;

                    }
                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(-1126.33, 804.30, 437.64))
                            return false;

                    }

                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(-1124.06, 837.84, 433.32))
                            return false;

                    }

                    if (step == 5)
                    {
                        if (!Host.CommonModule.MoveTo(-1089.47, 830.32, 435.34))
                            return false;
                        MyComliteQuest(quest);
                        return false;
                    }

                    var convoy = Host.GetNpcById(135441);
                    while (convoy != null && step == 0)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                        Thread.Sleep(1000);
                        convoy = Host.GetNpcById(135441);
                        if ((convoy as Unit).GetThreats().Count > 0 && Host.FarmModule.BestMob == null)
                        {
                            foreach (var threatItem in (convoy as Unit).GetThreats())
                            {
                                Host.FarmModule.BestMob = threatItem;
                                break;
                            }
                        }
                        if (Host.Me.Distance(convoy) > 3 && Host.FarmModule.BestMob == null)
                            Host.CommonModule.MoveToConvoy(convoy, 2);
                        if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null && (convoy as Unit).Target.HpPercents < 99)
                            Host.FarmModule.BestMob = (convoy as Unit).Target;
                        Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + (convoy as Unit).GetThreats().Count);

                    }
                    return false;
                }

                if (quest.Id == 47313) //Приватный разговор[47313] 
                {
                    step = 0;
                    foreach (var questCount in quest.Counts) step = step + questCount;

                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-965.92, 802.35, 401.04))
                            return false;
                        var npc = Host.GetNpcById(122229);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            return false;
                        }
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(-968.43, 738.10, 368.41))
                            return false;
                        var npc = Host.GetNpcById(123243);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            return false;
                        }
                    }

                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(-932.49, 681.41, 339.80))
                            return false;
                        var npc = Host.GetNpcById(122231);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 0);
                            return false;
                        }
                    }

                    if (step == 6)
                        if (!Host.CommonModule.MoveTo(-863.96, 756.71, 339.80))
                            return false;

                    if (step == 7)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47315)
                {
                    if (step == 0)
                    {
                        MyUseSpellClick(122347);
                    }

                    while (step == 1)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        Thread.Sleep(5000);
                        step = 0;
                        quest = Host.GetQuest(quest.Id);
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                    }

                    if (step == 3)
                    {
                        MyComliteQuest(quest);
                    }
                    return false;
                }

                if (quest.Id == 51357)
                {
                    if (step == 0)
                    {
                        Host.MyUseGameObject(278718);
                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 49676)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2794.64, 2290.96, 114.80))
                            return false;
                        Host.MyUseGameObject(278686);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2848.68, 2357.83, 101.63))
                            return false;
                        Host.MyUseGameObject(278685);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2806.05, 2475.52, 68.01))
                            return false;
                        Host.MyUseGameObject(289311);
                    }

                    if (step == 3)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51364)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2768.88, 2592.97, 42.05))
                            return false;

                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2768.88, 2592.97, 42.05))
                            return false;
                        MyUseSpellClick(137492);
                    }

                    while (step == 2 || step == 3)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        Thread.Sleep(5000);
                        step = 0;
                        quest = Host.GetQuest(quest.Id);
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                    }

                    if (step == 4)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47321)
                {
                    if (step == 6)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                }

                if (quest.Id == 47317)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2548.77, 2967.88, 29.47))
                            return false;
                        Host.MyUseGameObject(271014);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2489.72, 2786.35, 16.18))
                            return false;
                        MyUseSpellClick(122729);
                        Thread.Sleep(2000);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 47322)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2552.12, 2842.26, 20.55))
                            return false;
                        Host.MyUseGameObject(271839);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2506.46, 2948.98, 31.99))
                            return false;
                        Host.MyUseGameObject(271840);
                    }
                    if (step == 4)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47959)
                {

                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2045.50, 2822.21, 50.09))
                            return false;
                        var npc = Host.GetNpcById(122583);
                        if (npc != null)
                            Host.MyDialog(npc, 0);
                        Thread.Sleep(10000);
                    }

                    while (step == 1)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        Thread.Sleep(5000);
                        step = 0;
                        quest = Host.GetQuest(quest.Id);
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 48684)
                {

                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1336.22, 3064.44, 68.25))
                            return false;
                        var npc = Host.GetNpcById(126235);
                        if (npc != null)
                            Host.MyDialog(npc, 1);
                        Thread.Sleep(10000);
                    }

                    while (step == 1)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        Thread.Sleep(5000);
                        step = 0;
                        quest = Host.GetQuest(quest.Id);
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 48896)
                {

                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(282.26, 3098.26, 189.06))
                            return false;
                        var npc = Host.GetNpcById(126235);
                        if (npc != null)
                            Host.MyDialog(npc, 2);
                        Thread.Sleep(10000);
                    }

                    while (step == 1)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        Thread.Sleep(5000);
                        step = 0;
                        quest = Host.GetQuest(quest.Id);
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }
                    }


                    return false;
                }

                if (quest.Id == 48553)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1264.70, 3052.42, 83.51))
                            return false;
                        Host.MyUseGameObject(273992);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1069.91, 3079.94, 81.39))
                            return false;
                        Host.MyUseGameObject(291008);
                    }
                    if (step >= 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 48551)
                {
                    if (step < 10)
                    {
                        var entityList = Host.GetEntities<Unit>();
                        foreach (var entity in entityList.OrderBy(i => Host.Me.Distance(i)))
                        {
                            if (Host.FarmModule.IsBadTarget(entity, Host.ComboRoute.TickTime))
                                continue;
                            if (entity.Id != 126816)
                                continue;
                            Host.CommonModule.MoveTo(entity);
                            var item = Host.MyGetItem(152630);
                            if (item != null && Host.GetAgroCreatures().Count == 0)
                            {
                                Host.MyUseItemAndWait(item, entity);
                                Host.FarmModule.SetBadTarget(entity, 120000);
                            }

                            break;
                        }
                    }
                    if (step >= 10)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 50536)
                {
                    Host.FarmModule.farmState = FarmState.Disabled;
                    if (step == 0)
                    {
                        MyUseSpellClick(134245);
                        Thread.Sleep(2000);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(942.92, 3414.90, 85.14))
                            return false;
                        var npc = Host.GetNpcById(134089);
                        Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(877.02, 3357.50, 85.14))
                            return false;
                        var npc = Host.GetNpcById(134067);
                        Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(931.89, 3306.78, 85.17))
                            return false;
                        var npc = Host.GetNpcById(134090);
                        Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step == 4)
                    {
                        MyComliteQuest(quest);
                        Host.TerminateGameClient();
                    }


                    return false;
                }

                if (quest.Id == 50561)
                {

                    if (!Host.CheckQuestCompleted(50536))
                    {
                        RunQuest(50536);
                        return false;
                    }

                    if (!Host.CheckQuestCompleted(48871))
                    {
                        RunQuest(48871);
                        return false;
                    }

                    if (!Host.CheckQuestCompleted(50535))
                    {
                        RunQuest(50535);
                        return false;
                    }


                }

                if (quest.Id == 48554)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1188.48, 2917.43, 138.75))
                            return false;
                        Host.MyUseGameObject(273995);

                    }
                }

                if (quest.Id == 48887)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(549.14, 3135.16, 121.45))
                            return false;
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(276460);
                        Host.Wait(10000);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 48894)
                {
                    if (!Host.CommonModule.MoveTo(237.23, 3066.59, 192.18))
                        return false;
                    var npc = Host.GetNpcById(127992);
                    if (npc != null)
                        Host.MyDialog(npc, 2);

                    return false;
                }

                if (quest.Id == 50913)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(154.55, 3066.14, 194.21))
                            return false;
                        if (Host.GetAgroCreatures().Count == 0)
                            Host.MyUseGameObject(290773);
                        Thread.Sleep(3000);
                        return false;
                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47874)
                {
                    if (!Host.CommonModule.MoveTo(280.65, 3089.54, 189.06))
                        return false;
                    Host.Wait(30000);

                    MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47716)
                {
                    if (!Host.CommonModule.MoveTo(897.22, 3520.98, 63.86))
                        return false;

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 50770)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(902.63, 3485.83, 65.31))
                            return false;
                        var item = Host.MyGetItem(158678);
                        var npc = Host.GetNpcById(134532);
                        Host.MyUseItemAndWait(item, npc);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(961.46, 3358.70, 85.87))
                            return false;
                        MyComliteQuest(quest);
                    }

                    return false;
                }

                if (quest.Id == 47324)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(906.90, 3500.96, 65.28))
                            return false;
                        MyUseSpellClick(134544);
                        Host.Wait(60000);
                    }
                    return false;
                }

                if (quest.Id == 49334)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1979.32, 4645.48, 53.45))
                            return false;
                        Host.MyUseGameObject(277876);
                        Thread.Sleep(4000);
                    }
                    if (step == 1)
                    {

                        MyComliteQuest(quest);
                    }
                    return false;
                }

                if (quest.Id == 50641)
                {
                    if (step == 10)
                        MyComliteQuest(quest);
                }

                if (quest.Id == 49327)
                {
                    if (step > 3 && step < 6)
                    {
                        if (!Host.CommonModule.MoveTo(2002.48, 4672.74, 53.70, 20))
                            return false;
                        if (Host.GetAgroCreatures().Count == 0)
                            MyUseSpellClick(129076);
                        Thread.Sleep(3000);
                        return false;
                    }
                    if (step == 6)
                    {
                        if (!Host.CommonModule.MoveTo(2000.63, 4719.04, 53.46))
                            return false;
                        if (Host.GetAgroCreatures().Count == 0)
                            Host.MyUseGameObject(277911);
                        Thread.Sleep(3000);
                        return false;
                    }

                    if (step == 7)
                    {
                        if (!Host.CommonModule.MoveTo(1848.21, 4712.58, 53.50))
                            return false;
                        if (Host.GetAgroCreatures().Count == 0)
                            Host.MyUseGameObject(277911);
                        Thread.Sleep(3000);
                        return false;
                    }

                }

                if (quest.Id == 49340)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1949.86, 4809.83, 70.30))
                            return false;
                        var npc = Host.GetNpcById(129519);
                        Host.MyDialog(npc, 1);
                        Host.Wait(10000);
                    }
                }

                if (quest.Id == 50745)
                {
                    if (!Host.CommonModule.MoveTo(2688.00, 3420.25, 69.56))
                        return false;
                    Host.Wait(60000);

                }

                if (quest.Id == 49668)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2844.69, 3235.88, 17.03))
                            return false;
                        var item = Host.MyGetItem(158896);
                        Host.MyUseItemAndWait(item);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2834.51, 3123.45, 17.61))
                            return false;
                        var item = Host.MyGetItem(158896);
                        Host.MyUseItemAndWait(item);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2826.13, 3046.48, 16.87))
                            return false;
                        var item = Host.MyGetItem(158896);
                        Host.MyUseItemAndWait(item);
                    }
                }

                if (quest.Id == 49665)
                {
                    if (quest.Counts[0] < 8)
                    {
                        if (!Host.CommonModule.MoveTo(2846.34, 3499.51, 9.89, 100))
                            return false;
                        if (Host.MyGetItem(159470) == null)
                        {
                            Host.MyUseGameObject(287006);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Entity npc = null;
                            var list = Host.GetEntities();
                            foreach (var entity in list.OrderBy(i => Host.Me.Distance(i)))
                            {
                                if (entity.Id != 130342)
                                    continue;
                                if (Host.FarmModule.IsBadTarget(entity, Host.ComboRoute.TickTime))
                                    continue;
                                npc = entity;
                                break;
                            }

                            if (npc != null)
                            {
                                Host.MyDialog(npc, 0);
                                if (Host.GetAgroCreatures().Count == 0)
                                    Host.FarmModule.SetBadTarget(npc, 20000);
                            }

                        }
                    }

                    if (quest.Counts[0] == 8)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 49003)
                {


                    if (step == 0 || Host.MyGetAura(173426) == null)
                    {
                        if (!Host.CommonModule.MoveTo(2560.73, 3472.73, 200.68))
                        {
                            return false;
                        }

                        var npc = Host.GetNpcById(129763);

                        if (npc != null)
                            Host.MyDialog(npc, 0);
                        MyUseSpellClick(138547);
                        return false;
                    }

                    if (step > 0 && step < 61)
                    {
                        var list = Host.GetEntities();
                        Entity npc = null;
                        foreach (var entity in list.OrderBy(i => Host.GetEntity(Host.ActiveMover).Distance(i)))
                        {
                            if (entity.Id == 135187)
                            {
                                npc = entity;
                                break;
                            }
                        }

                        if (npc != null)
                        {
                            var result = Host.SpellManager.CastSpell(272301, npc, npc.Location);
                            if (result != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не удалось использовать скилл  " + result);
                            }
                            else
                            {
                                Host.log("Использовал ");
                            }
                        }

                        Thread.Sleep(5000);
                    }

                    if (step == 61)
                    {
                        if (Host.Me.Distance(2557.16, 3479.54, 200.68) < 3)
                        {
                            if (!Host.CommonModule.MoveTo(2560.73, 3472.73, 200.68))
                            {
                                return false;
                            }

                            var npc = Host.GetNpcById(129763);

                            if (npc != null)
                                Host.MyDialog(npc, 0);
                            MyUseSpellClick(138547);
                            return false;
                        }
                        var result = Host.SpellManager.CastSpell(265551);
                        if (result != ESpellCastError.SUCCESS)
                        {
                            Host.log("Не удалось использовать скилл  " + result);
                        }
                        else
                        {
                            Host.log("Использовал ");
                        }
                        Thread.Sleep(10000);
                    }

                    if (step == 63)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 50752)
                {
                    Host.FarmModule.farmState = FarmState.Disabled;
                    /* var list = Host.GetEntities();
                     foreach (var entity in list.OrderBy(i => Host.Me.Distance(i)))
                     {
                         if (entity.Id == 135983 && Host.Me.Distance(entity) < 20)
                         {
                             Host.ComeTo(entity);
                             break;
                         }
                     }*/

                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(3045.03, 3285.71, 125.87))
                            return false;
                        Host.CommonModule.UseObject = true;
                        Host.MyUseGameObject(282451);
                        Host.CommonModule.UseObject = false;
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(3121.59, 3265.20, 127.74))
                            return false;
                        Host.CommonModule.UseObject = true;
                        Host.MyUseGameObject(290755);
                        Host.CommonModule.UseObject = false;
                        return false;
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(3144.62, 3401.95, 134.95))
                            return false;
                        Host.CommonModule.UseObject = true;
                        Host.MyUseGameObject(290756);
                        Host.CommonModule.UseObject = false;
                        return false;
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(3223.86, 3316.17, 135.17))
                            return false;
                        Host.CommonModule.UseObject = true;
                        Host.MyUseGameObject(290757);
                        Host.CommonModule.UseObject = false;
                        return false;
                    }

                    if (step == 4)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 50550)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(138411);
                        Host.MyDialog(npc, 0);
                    }

                    if (step == 1)
                    {
                        if (Host.Me.Distance(3087.32, 3152.34, 111.39) > 10)
                            Host.CommonModule.MoveTo(3087.32, 3152.34, 111.39);
                        Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        if (Host.FarmModule.BestMob == null)
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (entity.Id == 134601 && entity.IsAlive)
                                    Host.FarmModule.BestMob = entity;
                            }
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 50904)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(135625);
                        Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (Host.Me.Distance(1240.05, 3761.24, 38.09) < 30)
                            if (!Host.CommonModule.MoveTo(1270.06, 3741.01, 41.13))
                                return false;
                        if (!Host.CommonModule.MoveTo(1219.46, 3673.73, 45.46))
                            return false;
                        MyUseSpellClick(135695);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1320.67, 3619.63, 35.86))
                            return false;
                        MyUseSpellClick(135738);
                    }

                    if (step == 3)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47103)
                {
                    if (!Host.CommonModule.MoveTo(-1037.67, 762.32, 435.34))
                        return false;

                    return false;
                }


                if (quest.Id == 48535)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(122689);
                        Host.MyDialog(npc, 1);
                        Host.Wait(40000);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(510.82, 1184.22, 39.61))
                            return false;
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47105)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(713.31, 990.17, 0.71))
                            return false;
                    }

                    if (step == 1)
                        MyComliteQuest(quest);

                    return false;

                }

                if (quest.Id == 47263)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1117.49, 1055.71, 22.99))
                            return false;
                        Host.Wait(10000);
                        var npc = Host.GetNpcById(131146);
                        Host.MyDialog(npc, 0);
                    }

                    if (step == 1)
                    {
                        if (Host.Me.Distance(842.61, 1376.16, 14.89) > 30)
                            Host.Wait(5000);
                        MyComliteQuest(quest);
                    }

                    return false;
                }

                if (quest.Id == 47188)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(768.59, 1388.02, 19.58, 30, 30))
                            return false;
                        var npc = Host.GetNpcById(121288);
                        Host.MyDialog(npc, 0);
                        var timer = 0;
                        while (step == 0)
                        {
                            if (!Host.MainForm.On)
                                return false;
                            quest = Host.GetQuest(id);
                            step = 0;
                            foreach (var questCount in quest.Counts)
                            {
                                step = step + questCount;
                            }
                            Thread.Sleep(5000);
                            timer++;
                            if (timer > 36)
                                return false;
                        }
                    }

                    if (step == 1)
                    {
                        MyComliteQuest(quest);
                    }

                    return false;
                }

                if (quest.Id == 48669)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(802.96, 1403.25, 19.39))
                            return false;
                        var npc = Host.GetNpcById(126713);
                        Host.MyDialog(npc, 0);
                        while (step == 0)
                        {
                            if (!Host.MainForm.On)
                                return false;
                            quest = Host.GetQuest(id);
                            step = 0;
                            foreach (var questCount in quest.Counts)
                            {
                                step = step + questCount;
                            }
                            Thread.Sleep(5000);
                        }
                    }

                    return false;
                }

                if (quest.Id == 48573)
                {
                    if (quest.Counts[0] > 7)
                    {

                        MyComliteQuest(quest);
                        return false;
                    }
                }


                if (id == 47264)
                {
                    if (quest.Counts[0] > 5)
                    {

                        MyComliteQuest(quest);
                        return false;
                    }
                }

                if (quest.Id == 47241)
                {
                    if (step == 1 || step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1231.10, 1362.19, 21.97))
                            return false;
                        Host.MyUseGameObject(270902);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1317.91, 1473.72, 28.81))
                            return false;
                        Host.MyUseGameObject(271170);
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(1476.72, 1570.94, 36.12))
                            return false;
                        MyUseSpellClick(122094);
                        Thread.Sleep(2000);
                    }

                    if (step == 4 || step == 5)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 49278)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1989.58, 1392.82, 16.94))
                            return false;
                        MyUseSpellClick(134363);
                    }
                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2003.35, 1402.47, 17.66))
                            return false;
                        MyUseSpellClick(128898);
                    }
                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(2002.46, 1301.76, 18.57))
                            return false;
                        MyUseSpellClick(128875);
                    }

                    if (step == 6)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 49440)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1994.17, 1368.71, 16.06))
                            return false;
                        MyUseSpellClick(129223);
                        Thread.Sleep(1000);
                    }
                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(2002.73, 1373.81, 16.06))
                            return false;
                        MyUseSpellClick(129223);
                        Thread.Sleep(1000);
                    }
                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2002.17, 1359.78, 16.06))
                            return false;
                        MyUseSpellClick(129223);
                        Thread.Sleep(1000);
                    }

                    if (step == 3)
                    {
                        var npc = Host.GetNpcById(122795);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(5000);
                    }

                    if (step == 4)
                    {
                        Thread.Sleep(5000);
                        MyComliteQuest(quest);
                    }

                    return false;
                }

                if (quest.Id == 48699)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1909.88, 1740.92, 12.04))
                            return false;
                        var npc = Host.GetNpcById(127128);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(5000);

                    }
                    if (step == 1)
                    {
                        MyComliteQuest(quest);
                    }
                    return false;
                }


                if (quest.Id == 47871)
                {
                    if (step == 3)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }


                }
                if (quest.Id == 48890)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1974.45, 1720.33, 9.93))
                            return false;
                        MyUseSpellClick(126933);
                        Thread.Sleep(1000);
                    }
                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1985.15, 1728.20, 9.96))
                            return false;
                        MyUseSpellClick(126933);
                        Thread.Sleep(1000);
                    }
                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(2004.62, 1748.72, 9.92))
                            return false;
                        MyUseSpellClick(126933);
                        Thread.Sleep(1000);
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(1941.55, 1736.61, 10.24))
                            return false;
                        MyUseSpellClick(126933);
                        Thread.Sleep(1000);
                    }
                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(1918.17, 1786.43, 8.96))
                            return false;
                        MyUseSpellClick(126933);
                        Thread.Sleep(1000);
                    }

                    if (step == 5)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 48801)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2000.09, 1773.38, 11.27))
                            return false;
                        if (!Host.CommonModule.MoveTo(2011.04, 1790.47, 33.94))
                            return false;
                        var npc = Host.GetNpcById(127999);
                        Host.MyDialog(npc, 0);

                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1867.45, 1596.79, 21.55))
                            return false;
                        if (!Host.CommonModule.MoveTo(1885.80, 1593.39, 46.26))
                            return false;
                        var npc = Host.GetNpcById(129380);
                        Host.MyDialog(npc, 0);

                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1800.08, 1804.05, 10.72))
                            return false;
                        if (!Host.CommonModule.MoveTo(1786.67, 1808.81, 33.45))
                            return false;
                        var npc = Host.GetNpcById(129381);
                        Host.MyDialog(npc, 0);

                    }

                    if (step == 3)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47880)
                {
                    if (step == 0)
                    {
                        Host.CommonModule.MoveTo(2390.58 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1366.87 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                        Host.CommonModule.MoveTo(2391.78 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1365.41 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                        Host.CommonModule.MoveTo(2398.78 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1362.48 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                        Host.CommonModule.MoveTo(2400.81 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1368.61 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                        Host.CommonModule.MoveTo(2396.90 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1371.98 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                        Host.CommonModule.MoveTo(2392.22 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1369.14 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                        Host.CommonModule.MoveTo(2397.65 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 1368.09 + Host.RandGenerator.Next(0, 2) - Host.RandGenerator.Next(0, 4), 10.35);
                    }

                    if (step == 1)
                    {
                        Host.MyUseGameObject(272250);
                        Thread.Sleep(20000);
                    }

                    if (step == 2)
                    {
                        Host.CommonModule.MoveTo(2661.50, 1366.62, 10.91);
                        MyComliteQuest(quest);
                    }
                    return false;
                }

                if (quest.Id == 49432)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2308.75, 1367.30, -18.17))
                            return false;
                        Host.MyUseGameObject(278691);
                    }

                    if (step == 1)
                    {
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 47249)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2472.90, 1370.89, -2.05))
                            return false;
                        var npc = Host.GetNpcById(126707);
                        Host.MyDialog(npc, 0);
                        Host.Wait(10000);
                    }

                    if (step == 1)
                    {
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }


                if (quest.Id == 48800)
                {
                    if (quest.Counts[2] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1794.65, 1734.44, 21.35))
                            return false;

                        Thread.Sleep(5000);
                        return false;
                    }

                    if (quest.Counts[0] == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1801.65, 1680.08, 14.80))
                            return false;
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (quest.Counts[1] == 0)
                    {
                        if (Host.Me.Location.Z < 15)
                            Host.CommonModule.MoveTo(Host.Me.Location.X + 3, Host.Me.Location.Y + 3, Host.Me.Location.Z);
                        if (!Host.CommonModule.MoveTo(1946.33, 1821.46, 20.18))
                            return false;
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (step == 3)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 49079)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1906.25, 1756.72, 11.95))
                            return false;
                        var npc = Host.GetNpcById(129378);
                        Host.MyDialog(npc, 0);
                        Host.Wait(30000);
                    }

                    var fix = 0;
                    while (step == 1)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        var item = Host.MyGetItem(153694);
                        var npc = Host.GetNpcById(129395);
                        if (item != null && npc != null)
                            Host.SendKeyPress(0x31);
                        fix++;
                        Host.log("До  отмены квеста " + fix + " 100");
                        if (fix > 100)
                        {
                            Host.log("Отменяю квест");
                            Host.CancelQuest(quest.Id);
                            return false;
                        }
                        Host.Wait(3000);
                        return false;
                    }

                    if (step == 2)
                    {
                        Host.Wait(30000);
                        MyComliteQuest(quest);
                    }

                    return false;
                }

                if (quest.Id == 49081)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1898.30, 1911.40, -126.79))
                            return false;

                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1891.62, 1809.65, -118.72))
                            return false;

                    }

                    return false;
                }

                if (quest.Id == 49082)
                {
                    if (step == 0)
                    {
                        MyUseSpellClick(128291);
                        Host.Wait(20000);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1989.01, 1364.55, 16.07))
                            return false;
                        var npc = Host.GetNpcById(122795);
                        Host.MyDialog(npc, 1);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 49615)
                {
                    if (step == 0)
                    {
                        if (Host.Me.Location.Z < 470)
                        {
                            if (!Host.CommonModule.MoveTo(-1126.50, 851.96, 443.32))
                                return false;
                            while (Host.Me.Location.Z < 485)
                            {
                                Thread.Sleep(1000);
                                Host.Jump();
                            }
                            Host.Wait(7000);
                            if (!Host.CommonModule.MoveTo(-1126.36, 841.51, 487.86))
                                return false;
                        }
                    }
                }

                if (quest.Id == 49489)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-264.70, 357.79, 196.91))
                            return false;
                        MyUseSpellClick(130109);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(-483.30, 202.02, 216.09))
                            return false;
                        MyUseSpellClick(130089);
                    }

                    if (step == 4)
                    {
                        MyComliteQuest(quest);
                    }

                    return false;
                }

                if (quest.Id == 49492)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-202.10, 389.87, 202.19))
                            return false;
                        Host.MyUseGameObject(278536);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-205.01, 334.19, 221.15))
                            return false;
                        Host.MyUseGameObject(278537);
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(-223.43, 280.72, 239.63))
                            return false;
                        MyUseSpellClick(130197);
                    }


                    return false;
                }

                if (quest.Id == 49494)
                {
                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-325.91, 153.04, 257.30))
                            return false;
                        MyComliteQuest(quest);
                        return false;
                    }
                }


                if (quest.Id == 47423)
                {
                    if (step < 9)
                    {
                        Unit npc = null;
                        foreach (var entity in Host.GetEntities<Unit>())
                        {
                            if (Host.FarmModule.IsBadTarget(entity, Host.ComboRoute.TickTime))
                                continue;
                            if (entity.Id != 126586)
                                continue;
                            npc = entity;
                            break;
                        }

                        if (npc == null)
                        {
                            Host.CommonModule.MoveTo(-605.00, 828.09, 291.39);
                        }
                        else
                        {
                            Host.CommonModule.MoveTo(npc);
                            var item = Host.MyGetItem(152627);
                            Host.MyUseItemAndWait(item);
                        }
                    }

                    if (step == 9)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 47433)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-486.95, 750.40, 293.80))
                            return false;
                        var npc = Host.GetNpcById(126564);
                        Host.MyDialog(npc, 0);
                        Thread.Sleep(2000);
                    }

                    if (step == 1)
                    {
                        MyUseSpellClick(126822);
                        Host.Wait(60000);
                    }

                    if (step > 2 && step < 8)
                    {

                    }

                    return false;
                }

                if (quest.Id == 47441)
                {
                    if (step == 8)
                    {
                        if (!Host.CommonModule.MoveTo(-410.81, 1210.07, 320.84))
                            return false;
                        Host.MyUseGameObject(273660);
                        return false;
                    }

                }

                if (quest.Id == 49495)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-366.77, 135.08, 257.20))
                            return false;
                        Host.MyUseGameObject(279349);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-353.14, 132.97, 257.11))
                            return false;
                        Host.MyUseGameObject(279346);
                    }

                    if (step == 3)
                    {
                        if (!Host.CommonModule.MoveTo(-353.14, 132.97, 257.11))
                            return false;
                        Host.MyUseGameObject(279353);
                    }

                    if (step == 4)
                    {
                        if (!Host.CommonModule.MoveTo(-382.37, 162.70, 257.36))
                            return false;
                        Host.MyUseGameObject(279354);
                    }

                    if (step == 5)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 49810)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-706.62, 374.40, 152.76))
                            return false;
                        var item = Host.MyGetItem(155911);

                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5500);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-831.37, 278.62, 174.36))
                            return false;
                        var item = Host.MyGetItem(155911);

                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5500);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 50074)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-698.18, 364.78, 154.32))
                            return false;
                        var item = Host.MyGetItem(156475);

                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5500);
                    }
                }

                if (quest.Id == 50150)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-814.64, 271.98, 175.08))
                            return false;
                        MyUseSpellClick(132628);
                        Thread.Sleep(3500);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-814.64, 271.98, 175.08))
                            return false;
                        MyUseSpellClick(132629);
                        Thread.Sleep(3500);
                    }

                    if (step == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 50252)
                {
                    if (step == 0)
                    {
                        var npc = Host.GetNpcById(130905);
                        Host.MyDialog(npc, 0);
                    }

                    if (step == 1)
                    {
                        var npc = Host.GetNpcById(130929);
                        Host.MyDialog(npc, 0);
                    }

                    return false;
                }

                if (quest.Id == 50268)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-613.56, 284.11, 169.39))
                            return false;
                        MyUseSpellClick(133167);
                        Thread.Sleep(3500);
                    }
                    return false;
                }

                if (quest.Id == 49870)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-711.53, 374.56, 152.95))
                            return false;
                        var item = Host.MyGetItem(156867);

                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5500);
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-998.85, 233.46, 185.31))
                            return false;
                    }

                    if (step == 2)
                    {
                        if (Host.FarmModule.BestMob == null)
                        {
                            var npc = Host.GetNpcById(131555);
                            if (npc != null)
                            {
                                Host.FarmModule.BestMob = npc as Unit;
                            }
                        }
                    }
                    if (step == 3)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 50297)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-820.82, 281.87, 173.80))
                            return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(-820.82, 281.87, 173.80))
                            return false;
                        MyUseSpellClick(133300);
                        Thread.Sleep(3500);
                    }

                    if (step == 3)
                    {
                        MyComliteQuest(quest);
                    }
                    return false;
                }

                if (quest.Id == 46928)
                {
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(-1655.57, 898.33, 82.23))
                            return false;
                        Host.MyUseGameObject(268897);
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (step == 3)
                        MyComliteQuest(quest);

                    return false;
                }


                if (quest.Id == 48753)
                {
                    if (step == 10)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    var farmLoc = new Vector3F(1186.62, 103.94, 11.11);
                    var questPoiPoints = new List<Vector3F>();
                    questPoiPoints.Add(farmLoc);
                    questPoiPoints.Add(new Vector3F(1411.62, 355.89, 13.07));
                    if (!Host.CommonModule.MoveTo(farmLoc))
                        return false;
                    var zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 600);
                    var farmmoblist = new List<uint>();
                    farmmoblist.Add(290136);
                    Host.FarmModule.SetFarmProps(zone, farmmoblist);
                    int badRadius = 0;
                    //int badRadius = 0;
                    while (Host.MainForm.On
                           && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                           && !IsQuestComplite(quest.Id, 0)
                           && Host.FarmModule.readyToActions
                           && Host.FarmModule.farmState == FarmState.FarmProps)
                    {
                        if (Host.MyIsNeedRepair())
                            break;
                        Thread.Sleep(100);
                        if (Host.FarmModule.BestProp == null && Host.Me.HpPercents > 80)
                            badRadius++;
                        else
                            badRadius = 0;
                        if (Host.FarmModule.BestMob != null)
                            badRadius = 0;

                        if (badRadius > 50)
                        {
                            var findPoint = farmLoc;
                            if (questPoiPoints.Count > 0)
                                findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                            Host.log("Не могу найти GameObject, подбегаю в центр зоны " + Host.Me.Distance(findPoint) + "    "/* + questPoiPoints.Count*/);
                            Host.CommonModule.MoveTo(findPoint);
                        }
                    }
                    Host.FarmModule.StopFarm();
                    return false;
                }


                if (quest.Id == 51230)
                {
                    if (step == 10)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    var farmLoc = new Vector3F(-648.05, 459.78, 151.37);
                    var questPoiPoints = new List<Vector3F>();
                    questPoiPoints.Add(farmLoc);
                    questPoiPoints.Add(new Vector3F(-825.11, 387.59, 149.30));
                    if (!Host.CommonModule.MoveTo(farmLoc))
                        return false;
                    var zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 200);
                    var farmmoblist = new List<uint>();
                    farmmoblist.Add(290136);
                    Host.FarmModule.SetFarmProps(zone, farmmoblist);
                    int badRadius = 0;
                    //int badRadius = 0;
                    while (Host.MainForm.On
                           && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                           && !IsQuestComplite(quest.Id, 0)
                           && Host.FarmModule.readyToActions
                           && Host.FarmModule.farmState == FarmState.FarmProps)
                    {
                        if (Host.MyIsNeedRepair())
                            break;
                        Thread.Sleep(100);
                        if (Host.FarmModule.BestProp == null && Host.Me.HpPercents > 80)
                            badRadius++;
                        else
                            badRadius = 0;
                        if (Host.FarmModule.BestMob != null)
                            badRadius = 0;

                        if (badRadius > 50)
                        {
                            var findPoint = farmLoc;
                            if (questPoiPoints.Count > 0)
                                findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                            Host.log("Не могу найти GameObject, подбегаю в центр зоны " + Host.Me.Distance(findPoint) + "    "/* + questPoiPoints.Count*/);
                            Host.CommonModule.MoveTo(findPoint);
                        }
                    }
                    Host.FarmModule.StopFarm();
                    return false;
                }


                if (quest.Id == 51243)
                {
                    if (step == 12)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    var farmLoc = new Vector3F(-572.55, 1309.20, 345.29);
                    var questPoiPoints = new List<Vector3F>();
                    questPoiPoints.Add(farmLoc);
                    questPoiPoints.Add(new Vector3F(-567.53, 1323.61, 345.39));
                    if (!Host.CommonModule.MoveTo(farmLoc))
                        return false;
                    var zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 100);
                    var farmmoblist = new List<uint>();
                    farmmoblist.Add(131522);
                    Host.FarmModule.SetFarmMobs(zone, farmmoblist);
                    int badRadius = 0;
                    //int badRadius = 0;
                    while (Host.MainForm.On
                           && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                           && !IsQuestComplite(quest.Id, 0)
                           && Host.FarmModule.readyToActions
                           && Host.FarmModule.farmState == FarmState.FarmMobs)
                    {
                        if (Host.MyIsNeedRepair())
                            break;
                        Thread.Sleep(100);
                        if (Host.FarmModule.BestMob == null && Host.Me.HpPercents > 80)
                            badRadius++;
                        else
                            badRadius = 0;


                        if (badRadius > 50)
                        {
                            var findPoint = farmLoc;
                            if (questPoiPoints.Count > 0)
                                findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                            Host.log("Не могу найти мобов, подбегаю в центр зоны " + Host.Me.Distance(findPoint) + "    "/* + questPoiPoints.Count*/);
                            Host.CommonModule.MoveTo(findPoint);
                        }
                    }
                    Host.FarmModule.StopFarm();
                    return false;
                }


                if (quest.Id == 48757)
                {
                    if (step == 2)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    if (!Host.CommonModule.MoveTo(new Vector3F(714.19, -234.86, 12.10)))
                        return false;
                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                    var item = Host.MyGetItem(159833);
                    Host.MyUseItemAndWait(item);
                    Host.Wait(10000);
                    return false;
                }

                if (quest.Id == 51478)
                {
                    if (step == 3)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    if (!Host.CommonModule.MoveTo(new Vector3F(-1866.61, 740.45, 53.71)))
                        return false;
                    Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                    var item = Host.MyGetItem(160299);
                    Host.MyUseItemAndWait(item);
                    Host.Wait(10000);
                    return false;
                }


                if (quest.Id == 51313)
                {
                    if (step == 1)
                    {
                        if (Host.Area.Id != 8717)
                        {
                            Host.MyUseTaxi(8717, new Vector3F(1263.38, -543.17, 33.40));
                            return false;
                        }

                        MyComliteQuest(quest);
                        return false;
                    }

                    if (step == 0)
                    {
                        /* if (Host.Area.Id != 8501 && !TryFly)
                        {
                             Host.MyUseTaxi(8501, new Vector3F(2095.05, 2808.83, 56.24));
                             TryFly = true;
                             Host.AutoQuests.HerbQuest = true;
                             if (!Host.CommonModule.MoveTo(new Vector3F(2126.09, 2927.00, 42.51)))
                                 return false;

                         }*/

                        if (!Host.CommonModule.MoveTo(new Vector3F(2395.43, -757.84, 64.78)))
                            return false;
                        var item = Host.MyGetItem(159881);
                        Host.MyUseItemAndWait(item);
                        Host.Wait(30000);
                        return false;
                    }
                }

                if (quest.Id == 51448)
                {
                    if (step == 1)
                    {
                        if (Host.Area.Id != 8499)
                        {
                            Host.MyUseTaxi(8499, new Vector3F(-932.67, 1006.92, 321.04));
                            return false;
                        }

                        MyComliteQuest(quest);
                        return false;
                    }

                    if (step == 0)
                    {
                        if (Host.Area.Id != 8501 && !TryFly)
                        {
                            Host.MyUseTaxi(8501, new Vector3F(2095.05, 2808.83, 56.24));
                            TryFly = true;
                            Host.AutoQuests.HerbQuest = true;
                            if (!Host.CommonModule.MoveTo(new Vector3F(2126.09, 2927.00, 42.51)))
                                return false;

                        }

                        if (!Host.CommonModule.MoveTo(new Vector3F(2126.09, 2927.00, 42.51)))
                            return false;
                        var item = Host.MyGetItem(160252);
                        Host.MyUseItemAndWait(item);
                        Host.Wait(30000);
                        return false;
                    }
                }


                if (quest.Id == 51986)
                {
                    if (Host.MapID == 1643)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(3888.11, 424.88, 130.46));
                        var npc = Host.GetNpcById(138097);
                        if (npc != null)
                        {
                            /*  Host.OpenDialog(npc);
                              Thread.Sleep(1000);*/

                            Host.MyDialog(npc, 1);

                            Thread.Sleep(5000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(10000);
                            /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                             {
                                 Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                             }*/
                        }
                    }

                    if (Host.MapID == 1642)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51984)
                {
                    if (Host.MapID == 1643)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(-217.60, -1526.52, 1.44));
                        var npc = Host.GetNpcById(139524);
                        if (npc != null)
                        {
                            /*  Host.OpenDialog(npc);
                              Thread.Sleep(1000);*/

                            Host.MyDialog(npc, 0);

                            Thread.Sleep(5000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(10000);
                            /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                             {
                                 Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                             }*/
                        }
                    }

                    if (Host.MapID == 1642)
                        MyComliteQuest(quest);

                    return false;
                }


                var curentObjectiveType = EQuestRequirementType.PlayerKills;
                QuestObjective questObjective = new QuestObjective();
                var objectiveindex = -1;

                for (var index = 0; index < (quest?.Template.QuestObjectives).Length; index++)
                {
                    var templateQuestObjective = (quest?.Template.QuestObjectives)[index];
                    if (templateQuestObjective.Type == EQuestRequirementType.AreaTrigger && quest.State == EQuestState.None)
                    {
                        objectiveindex = 0;
                        questObjective = templateQuestObjective;
                        curentObjectiveType = templateQuestObjective.Type;
                    }

                    switch (quest.Id)
                    {
                        case 47247:
                            {
                                if (index == 0)
                                    if (quest?.Counts[index] >= templateQuestObjective.Amount)
                                        continue;

                                if (index == 1)
                                    if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                        continue;
                            }
                            break;

                        case 47573:
                            {
                                if (index == 0)
                                    if (quest?.Counts[index + 2] >= templateQuestObjective.Amount)
                                        continue;

                                if (index == 1)
                                    if (quest?.Counts[index + 2] >= templateQuestObjective.Amount)
                                        continue;
                            }
                            break;

                        case 47871:
                            {
                                if (index == 2)
                                {
                                    if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                        continue;
                                }
                                else
                                {
                                    if (quest?.Counts[index] >= templateQuestObjective.Amount)
                                        continue;
                                }
                            }
                            break;
                        case 47262:
                            {
                                if (quest?.Counts[index + 5] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 47228:
                            {
                                if (quest?.Counts[index + 6] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 49494:
                        case 50702:
                        case 49922:
                            {
                                if (quest?.Counts[index + 2] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 47244:
                            {
                                if (quest?.Counts[index + 4] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 49327:
                            {
                                if (quest?.Counts[index + 3] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;

                        case 54012:
                            {
                                if (quest?.Counts[4] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 49801:
                            {
                                if (quest?.Counts[index + 3] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 48399:
                        case 47925:
                        case 47570:
                        case 50154:
                        case 51663:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 25188:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 50748:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 13558:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;

                        case 49919:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 48871:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 50535:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 48585:
                            {
                                if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;

                        /*  case 49227:
                              {
                              if (quest?.Counts[index + 1] >= templateQuestObjective.Amount)
                                  continue;
                          }
                              break;*/



                        default:
                            {
                                if (quest?.Counts[index] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;

                    }




                    Host.log("Type: " + templateQuestObjective.Type + " Amount:" + templateQuestObjective.Amount + " ObjectID:" + templateQuestObjective.ObjectID + "  индекс " + index, Host.LogLvl.Important);
                    questObjective = templateQuestObjective;
                    objectiveindex = index;
                    curentObjectiveType = templateQuestObjective.Type;
                    break;
                }


                //Сдать квет
                if (objectiveindex == -1)
                {
                    Host.log("Сдаю квест : " + quest.Template.LogTitle + "[" + quest.Id + "]    State:" + quest.State, Host.LogLvl.Important);

                    if (quest.Id == 56063)
                    {
                        if (!Host.CommonModule.MoveTo(504.35, -190.46, -194.29))
                            return false;
                    }

                    if (quest.Id == 51870)
                    {
                        if (Host.Me.Distance(-2155.96, 765.34, 14.67) < 15)
                            if (!Host.CommonModule.MoveTo(new Vector3F(-2139.56, 795.00, 5.93)))
                                return false;
                    }

                    if (quest.Id == 50539)
                    {
                        if (!Host.CommonModule.MoveTo(961.46, 3358.70, 85.87))
                            return false;
                    }

                    if (quest.Id == 6342)
                    {
                        if (!Host.CommonModule.MoveTo(9946.87, 2603.77, 1316.19))
                            return false;
                    }


                    if (quest.Id == 28725)
                    {
                        if (!Host.CommonModule.MoveTo(10747.70, 925.48, 1336.66))
                            return false;
                        return false;
                    }

                    MyComliteQuest(quest);
                    return false;
                }


                //Заполнение формы квестов
                Host.MainForm.SetQuestIdText(quest?.Template.LogTitle + "[" + id + "]");
                if (quest != null)
                {
                    Host.MainForm.SetQuestStateText(curentObjectiveType + "   " + QuestType);
                }
                else
                    Host.MainForm.SetQuestStateText("Не взят");




                if (logQuest)
                    Host.log("Начинаю выполнять:  " + quest.Template.LogTitle + "[" + quest.Id + "] Индекс: " + objectiveindex
                        + " Type:" + questObjective.Type
                             + " Flags:" + questObjective.Flags
                             + " Flags2:" + questObjective.Flags2
                             + " ObjectID:" + questObjective.ObjectID
                             + " ProgressBarWeight:" + questObjective.ProgressBarWeight
                             + " StorageIndex:" + questObjective.StorageIndex

                        , Host.LogLvl.Ok);


                /* for (var index = 0; index < quest.Template.ItemDrops.Length; index++)
                 {
                     var templateItemDrop = quest.Template.ItemDrops[index];
                     Host.log("templateItemDrop " + index + "   " + templateItemDrop + "   /   " + quest.Template.StartItem, Host.LogLvl.Error);
                 }

                 for (var index = 0; index < quest.Template.ItemDropQuantitys.Length; index++)
                 {
                     var templateItemDropQuantity = quest.Template.ItemDropQuantitys[index];
                     Host.log(
                         "templateItemDropQuantity " + index + "   " + templateItemDropQuantity + "   /   " + quest.Template.StartItem, Host.LogLvl.Error);
                 }*/



                switch (curentObjectiveType)
                {

                    case EQuestRequirementType.Monster:
                        {
                            QuestTypeMonster(quest, objectiveindex, questObjective);
                        }
                        break;
                    case EQuestRequirementType.Item:   // флаг 0 флаг2 = 1  - Убить НПС и получить дроп?   1 1 убить босса и получить дроп?
                        {
                            QuestTypeItem(quest, objectiveindex, questObjective);
                        }
                        break;

                    case EQuestRequirementType.GameObject:
                        {
                            QuestTypeGameObject(quest, objectiveindex, questObjective);
                        }
                        break;

                    case EQuestRequirementType.TalkTo:
                        break;

                    case EQuestRequirementType.Currency:
                        break;

                    case EQuestRequirementType.LearnSpell:
                        break;

                    case EQuestRequirementType.MinReputation:
                        break;

                    case EQuestRequirementType.MaxReputation:
                        break;

                    case EQuestRequirementType.Money:
                        break;

                    case EQuestRequirementType.PlayerKills:
                        Host.log("Тип квеста не найден!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + curentObjectiveType, Host.LogLvl.Error);
                        break;

                    case EQuestRequirementType.AreaTrigger:
                        {
                            QuestTypeArea(quest, objectiveindex);
                        }
                        break;
                    case EQuestRequirementType.WinPetBattleAgainsNpc:
                        break;

                    case EQuestRequirementType.DefeatBattlePet:
                        break;

                    case EQuestRequirementType.WinPvpPetBattles:
                        break;

                    case EQuestRequirementType.CriteriaTree:
                        break;

                    case EQuestRequirementType.ProgressBar:
                        {
                            if (quest.Id == 47259)
                                QuestTypeGameObject(quest, 1, questObjective);
                            if (quest.Id == 50178)
                                QuestTypeMonster(quest, 2, questObjective);
                            if (quest.Id == 47647)
                                QuestTypeMonster(quest, 2, questObjective);
                            if (quest.Id == 50805)
                                QuestTypeMonster(quest, 2, questObjective);
                            if (quest.Id == 49406)
                                QuestTypeMonster(quest, 2, questObjective);
                            if (quest.Id == 48852)
                                QuestTypeMonster(quest, 1, questObjective);
                            if (quest.Id == 48934)
                                QuestTypeMonster(quest, 4, questObjective);
                            if (quest.Id == 47996)
                                QuestTypeMonster(quest, 1, questObjective);
                            if (quest.Id == 51689)
                                QuestTypeMonster(quest, 1, questObjective);
                            if (quest.Id == 49918)
                                QuestTypeMonster(quest, 1, questObjective);

                            if (quest.Id == 51991)
                            {
                                var percent = Host.GetPercent(100, quest.Counts[0]) +
                                              Host.GetPercent(34, quest.Counts[1]) + Host.GetPercent(20, quest.Counts[2]);
                                if (percent < 98)
                                    QuestTypeMonster(quest, 3, questObjective);
                            }
                        }

                        break;

                    case EQuestRequirementType.HaveCurrency:
                        break;

                    case EQuestRequirementType.ObtainCurrency:
                        break;

                    default:
                        Host.log("Тип квеста не найден!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + curentObjectiveType, Host.LogLvl.Error);
                        break;



                }
                return false;
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception err)
            {
                Host.log("RunQuest " + err);
                return false;
            }
        }

        public bool MyUseSpellClick(Entity entity)
        {
            try
            {
                if (entity != null)
                {
                    if (entity.Type == EBotTypes.Unit && (entity as Unit).IsSpellClick ||
                        entity.Type == EBotTypes.Vehicle && (entity as Vehicle).IsSpellClick)
                    {
                        if (!Host.CommonModule.MoveTo(entity, 2, 2))
                            return false;
                        Host.CommonModule.MyUnmount();
                        Host.MyCheckIsMovingIsCasting();

                        if (Host.SpellManager.UseSpellClick(entity as Unit))
                        {
                            Host.log("Использовал SpellClick ", Host.LogLvl.Ok);
                            Host.MyCheckIsMovingIsCasting();
                            while (Host.SpellManager.IsChanneling)
                                Thread.Sleep(50);
                            Host.FarmModule.SetBadTarget(entity, 120000);
                            // return true;
                        }
                        else
                        {
                            Host.log("Не смог использовать SpellClick " + Host.GetLastError(), Host.LogLvl.Error);
                            Host.CanselForm();
                            Host.FarmModule.SetBadTarget(entity, 120000);
                        }
                    }
                    else
                    {
                        Host.log("Тип Entity не известен " + entity.Type);
                    }
                }
                else
                {
                    Host.log("Не нашел НПС для использования скила ", Host.LogLvl.Error);
                }
                Thread.Sleep(1000);
                return false;
            }
            catch (Exception e)
            {
                Host.log("" + e);
                return false;
            }
        }


        public bool MyUseSpellClick(uint mobId)
        {
            try
            {
                var unit = Host.GetNpcById(mobId);

                if (unit != null)
                {
                    if (unit.Type == EBotTypes.Unit && (unit as Unit).IsSpellClick ||
                        unit.Type == EBotTypes.Vehicle && (unit as Vehicle).IsSpellClick)
                    {
                        if (mobId == 136434)
                        {
                            Host.CommonModule.MoveTo(unit, 4, 4);
                        }
                        else
                        {
                            Host.CommonModule.MoveTo(unit, 2, 2);
                        }

                        if (Host.Me.Distance(unit) > 6)
                            return false;
                        Host.CommonModule.MyUnmount();
                        Host.MyCheckIsMovingIsCasting();
                        if (Host.SpellManager.UseSpellClick(unit as Unit))
                        {
                            Host.log("Использовал SpellClick " + mobId, Host.LogLvl.Ok);
                            Host.MyCheckIsMovingIsCasting();
                            while (Host.SpellManager.IsChanneling)
                                Thread.Sleep(50);
                        }

                        else
                        {
                            Host.log("Не смог использовать SpellClick " + Host.GetLastError(), Host.LogLvl.Error);
                            Host.CanselForm();
                        }
                    }
                    else
                    {
                        Host.log("Тип Entity не известен " + unit.Type);
                    }


                }
                else
                {
                    Host.log("Не нашел НПС для использования скила ", Host.LogLvl.Error);
                    if (mobId == 122683)
                        Host.CommonModule.MoveTo(862.99, 3107.12, 123.84);

                    if (mobId == 129086)
                        Host.CommonModule.MoveTo(2473.83, 1202.95, 7.28);
                }

                Thread.Sleep(1000);
                return false;
            }

            catch (Exception e)
            {
                Host.log("" + e);
                return false;
            }
        }

        public bool IsQuestComplite(uint id, int index)
        {
            var quest = Host.GetQuest(id);
            if (quest == null)
                return true;
            var curCount = quest.Counts[index];
            if (id == 50641 || id == 50748 || id == 51663 || id == 50154 || id == 49667 || id == 47570 || id == 47925 || id == 48399 || id == 49919 || id == 48871 || id == 50535 || id == 48585
                || id == 49227)
                curCount = quest.Counts[index + 1];

            if (quest.Id == 48527)
                curCount = quest.Counts[index + 3];

            if (quest.Id == 47228)
                curCount = quest.Counts[index + 6];

            if (id == 48573)
            {
                if (quest.Counts[0] > 7)
                    return true;
            }

            if (id == 54012)
            {
                if (quest.Counts[4] > 6)
                    return true;
            }


            if (id == 51675)
            {
                if (quest.Counts[1] > 7)
                    return true;
            }


            if (id == 47264)
            {
                if (quest.Counts[0] > 5)
                    return true;
            }

            if (id == 47247)
            {
                if (index == 1)
                {
                    curCount = quest.Counts[index + 1];
                }
            }

            if (id == 47573 || id == 49922)
            {
                curCount = quest.Counts[index + 2];
            }

            if (id == 47871)
            {
                if (index == 2)
                {
                    curCount = quest.Counts[index + 1];
                }
            }

            if (id == 47311)
            {
                curCount = quest.Counts[index + 1];
            }

            if (id == 47244)
            {
                curCount = quest.Counts[index + 4];
            }
            if (id == 49327 || id == 49801)
            {
                curCount = quest.Counts[index + 3];
            }
            if (id == 50702 || id == 49494 || id == 47312)
            {
                curCount = quest.Counts[index + 2];
            }
            if (id == 47262)
            {
                curCount = quest.Counts[index + 5];
            }

            var needCount = quest.Template.QuestObjectives[index].Amount;
            Host.MainForm.SetQuestStateText(quest.Template.QuestObjectives[index].Type + "(" + index + "): " + curCount + "/" + needCount + "  " + QuestType);
            // Host.log(curCount + "/" + needCount);
            if (curCount >= needCount)
                return true;

            return false;
        }

        public ExecuteType QuestType = ExecuteType.Unknown;
        public bool QuestTypeMonster(Quest quest, int objectiveindex, QuestObjective questObjective)
        {
            try
            {

                var farmLoc = new Vector3F();
                var questPoiPoints = new List<Vector3F>();

                uint additionalId = 0;
                uint additionalId2 = 0;



                if (quest.Id == 25168)
                {
                    additionalId = 39260;
                    additionalId2 = 39261;
                }

                if (quest.Id == 835)
                {
                    additionalId = 3117;
                    additionalId2 = 3118;
                }

                if (quest.Id == 13992)
                {
                    additionalId = 3273;
                    additionalId2 = 3272;
                }
                if (quest.Id == 13945)
                {
                    additionalId = 2009;
                    additionalId2 = 2011;
                }

                if (quest.Id == 13946)
                {
                    additionalId = 2002;
                    additionalId2 = 2003;
                }

                if (quest.Id == 25194)
                {
                    additionalId = 39337;
                }

                if (quest.Id == 50812)
                {
                    additionalId = 135349;
                }

                if (quest.Id == 47574)
                {
                    additionalId = 137167;
                }

                if (quest.Id == 54012)
                {
                    additionalId = 153962;
                }


                var farmMobIds = new List<uint>();

                var sw = new Stopwatch();
                if (Host.AdvancedLog)
                    sw.Start();
                foreach (var monsterGroupMonsterGroup in Host.MonsterGroup.MonsterGroups)
                {
                    if (monsterGroupMonsterGroup.QuestId == quest.Id)
                    {
                        foreach (var u in monsterGroupMonsterGroup.MonstersId)
                        {
                            farmMobIds.Add(u);
                        }
                        break;
                    }
                }
                if (Host.AdvancedLog)
                {
                    Host.log("monsterGroupMonsterGroup   за  " + sw.ElapsedMilliseconds + "   Всего НПС:" + farmMobIds.Count);
                    sw.Stop();
                }
                var step2 = 0;
                foreach (var questCount in quest.Counts)
                {
                    step2 = step2 + questCount;
                }

                if (quest.Id == 56044)
                {
                    if (step2 == 0)
                    {
                        if (!Host.CommonModule.MoveTo(new Vector3F(-2051.89, 954.24, 7.05)))
                        {
                            return false;
                        }

                        var npc = Host.GetNpcById(123000);
                        Host.MyDialog(npc, 3);
                        Thread.Sleep(5000);
                        return false;
                    }

                }

                if (quest.Id == 51888)
                {


                    if (step2 == 0 || Host.MapID == 1642)
                    {
                        Host.CommonModule.MoveTo(-2052.65, 954.43, 7.05);
                        var npc = Host.GetNpcById(123000);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, 1);
                            Host.Wait(25000);

                            return false;
                        }
                    }

                    if (step2 == 1)
                    {
                        if (Host.Scenario.CurrentStep == 3823)
                        {
                            if (!Host.CommonModule.MoveTo(-1939.57, 2180.37, 3.10))
                                return false;
                        }

                        if (Host.Scenario.CurrentStep == 3824 || Host.Scenario.CurrentStep == 3823)
                        {
                            Host.MyUseGameObject(291056);
                            Host.MyUseGameObject(291055);
                        }

                        if (Host.Scenario.CurrentStep == 3825)
                        {
                            if (!Host.CommonModule.MoveTo(-1758.07, 2307.25, 1.45))
                                return false;

                        }

                        if (Host.Scenario.CurrentStep == 3827)
                        {
                            if (!Host.CommonModule.MoveTo(-2004.42, 2511.16, 7.99))
                                return false;

                        }

                        if (Host.Scenario.CurrentStep == 3828)
                        {
                            if (!Host.CommonModule.MoveTo(-2004.42, 2511.16, 7.99))
                                return false;
                            Host.MyUseGameObject(291080);
                            Host.MyUseGameObject(291081);

                        }

                        if (Host.Scenario.CurrentStep == 3829)
                        {
                            if (!Host.CommonModule.MoveTo(-2063.86, 2176.77, 6.45))
                                return false;
                            Host.Wait(30000);
                            return false;

                        }
                    }
                    return false;
                }

                if (quest.Id == 51438)
                {
                    Host.MyUseGameObject(290903);
                    Host.Wait(30000);
                    return false;
                }

                if (quest.Id == 51440)
                {
                    if (quest.Counts[0] == 0)
                    {
                        if (Host.Me.Location.Z < 17)
                        {
                            if (!Host.CommonModule.MoveTo(-183.75, -1522.07, 13.57))
                                return false;
                            if (!Host.CommonModule.MoveTo(-182.18, -1523.26, 13.69))
                                return false;

                            Host.CommonModule.MyUnmount();
                            Host.CanselForm();
                            Host.TurnDirectly(new Vector3F(-178.74, -1525.20, 17.28));
                            Thread.Sleep(2000);
                            var move = new MoveParams
                            {
                                Location = new Vector3F(-176.00, -1527.08, 19.13),
                                IgnoreStuckCheck = true,
                                DoneDist = 15

                            };
                            var i = 0;
                            while (Host.Me.Location.Z < 17)
                            {
                                if (!Host.MainForm.On || i > 5)
                                    return false;
                                Host.MoveTo(move);
                                Thread.Sleep(1000);
                                Host.Jump();
                                i++;
                            }
                        }


                        if (Host.Me.Location.Z > 17)
                        {
                            Host.CommonModule.MoveTo(-166.78, -1528.69, 20.19);
                            var npc = Host.GetNpcById(137798);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, 0);
                                Thread.Sleep(10000);
                                return false;
                            }
                        }

                        return false;

                    }

                    if (quest.Counts[2] == 0)
                    {

                        Host.CommonModule.MoveTo(-141.71, -1492.20, 4.85);
                        var npc = Host.GetNpcById(137807);
                        if (npc != null)
                        {
                            Host.CommonModule.MyUnmount();
                            Host.MyDialog(npc, 0);
                            Thread.Sleep(10000);
                            return false;
                        }
                    }


                    if (quest.Counts[1] == 0)
                    {

                        Host.CommonModule.MoveTo(-257.04, -1467.57, 24.39);
                        var npc = Host.GetNpcById(137800);
                        if (npc != null)
                        {
                            Host.CommonModule.MyUnmount();
                            Host.MyDialog(npc, 0);
                            Thread.Sleep(10000);
                            return false;
                        }
                    }




                    return false;
                }


                if (quest.Id == 51441)
                {
                    if (step2 > 2)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    var rand = Host.RandGenerator.Next(0, 1);

                    if (rand == 0)
                    {
                        Host.CommonModule.MoveTo(-172.79, -1590.28, -1.53, 30, 30);
                        var npc = Host.GetNpcById(137887);
                        var item = Host.MyGetItem(160405);
                        if (npc != null)
                        {
                            Host.CommonModule.MoveTo(npc, 20, 20);
                            Host.MyUseItemAndWait(item, npc);
                        }
                    }
                    else
                    {
                        Host.CommonModule.MoveTo(-112.83, -1480.80, -1.55, 30, 30);
                        var npc = Host.GetNpcById(137887);
                        var item = Host.MyGetItem(160405);
                        if (npc != null)
                        {
                            Host.CommonModule.MoveTo(npc, 20, 20);
                            Host.MyUseItemAndWait(item, npc);
                        }
                    }

                    return false;
                }


                if (quest.Id == 51437)
                {
                    var listEntity = Host.GetEntities<GameObject>();
                    foreach (var npc in listEntity.OrderBy((i => Host.Me.Distance(i))))
                    {

                        if (npc.Id != 289675)
                            continue;
                        if (npc.Distance(-168.41, -1529.56, 20.13) < 10)
                            continue;
                        if (Host.FarmModule.IsBadProp(npc, Host.ComboRoute.TickTime))
                            continue;
                        if ((npc.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                        {
                            Host.MyUseGameObject(npc);
                            Host.FarmModule.SetBadProp(npc, 60000);
                            break;
                        }

                    }

                    if (step2 > 5)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51696)
                {

                    Host.MyUseGameObject(290846);
                    Host.Wait(20000);
                    return false;
                }

                if (quest.Id == 51675)
                {
                    if (step2 > 8)
                    {
                        MyComliteQuest(quest);

                        return false;
                    }
                }

                if (quest.Id == 51691)
                {
                    if (step2 > 5)
                    {
                        MyComliteQuest(quest);

                        return false;
                    }
                    MyUseSpellClick(137922);
                    Thread.Sleep(5000);
                    return false;
                }

                if (quest.Id == 51674)
                {
                    if (step2 > 3)
                    {
                        MyComliteQuest(quest);
                        return false;
                    }

                    if (Host.Me.Distance(3958.75, 460.84, 114.06) > 50)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(3958.75, 460.84, 114.06));
                    }

                    var item = Host.MyGetItem(160565);
                    if (item == null)
                    {
                        var npc = Host.GetNpcById(137894);
                        if (npc != null)
                        {
                            Host.FarmModule.BestMob = npc as Unit;
                            return false;
                        }
                    }
                    else
                    {
                        var npc2 = Host.GetNpcById(138449);
                        if (npc2 != null)
                        {
                            Host.CommonModule.MoveTo(npc2, 25);
                            Host.log(npc2.Location + " ");
                            Host.MyUseItemAndWait(item, npc2);
                            Host.CommonModule.MoveTo(3948.54, 459.05, 113.10);
                        }
                    }


                    return false;
                }

                if (quest.Id == 51536)
                {
                    if (step2 == 0)
                    {
                        Host.MyUseGameObject(290760);
                        Host.Wait(20000);

                    }

                    if (step2 == 1)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(4172.44, 480.64, 21.26));
                        var npc = Host.GetNpcById(138137);
                        Host.MyDialog(npc, 0);
                        Host.Wait(25000);
                    }

                    if (step2 == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 51643)
                {
                    if (step2 == 0)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(4366.01, 436.13, 0.07));
                        MyUseSpellClick(138312);
                    }

                    if (step2 < 41)
                    {
                        var npc = Host.GetNpcById(138259);
                        var pet = Host.GetNpcById(138317);
                        if (npc != null)
                        {
                            var res = Host.SpellManager.CastPetSpell(pet.Guid, 272171, null, npc.Location);
                            if (res != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не смог выстрелить " + res + " " + Host.GetLastError(), Host.LogLvl.Error);
                            }
                        }

                        Thread.Sleep(3000);
                    }

                    if (step2 == 41)
                    {
                        MyComliteQuest(quest);
                    }

                    return false;
                }


                if (quest.Id == 51435)
                {
                    if (step2 == 0)
                    {
                        Host.MyUseGameObject(289572);
                    }

                    return false;
                }

                if (quest.Id == 51436)
                {
                    if (step2 == 0)
                    {
                        Host.MyUseGameObject(291139);
                        Host.Wait(20000);
                    }

                    if (step2 == 1)
                    {
                        var npc = Host.GetNpcById(137675);
                        Host.MyDialog(npc, 3);
                        Host.CommonModule.MoveTo(-161.46, -1487.93, 14.57);
                        Host.Wait(60000);
                    }

                    if (step2 == 2)
                        MyComliteQuest(quest);
                    return false;
                }

                if (quest.Id == 51532)
                {
                    if (Host.MapID == 1642)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(-2174.88, 765.17, 20.92));
                        var npc = Host.GetNpcById(135690);
                        if (npc != null)
                        {
                            /*  Host.OpenDialog(npc);
                              Thread.Sleep(1000);*/

                            Host.MyDialog(npc, 1);

                            Thread.Sleep(5000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(10000);
                            /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                             {
                                 Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                             }*/
                        }
                    }

                    if (Host.MapID == 1643)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51421)
                {
                    if (Host.MapID == 1642)
                    {
                        Host.CommonModule.MoveTo(new Vector3F(-2174.88, 765.17, 20.92));
                        var npc = Host.GetNpcById(135690);
                        if (npc != null)
                        {
                            /*  Host.OpenDialog(npc);
                              Thread.Sleep(1000);*/

                            Host.MyDialog(npc, 3);

                            Thread.Sleep(5000);
                            while (Host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(10000);
                            /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                             {
                                 Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                             }*/
                        }
                    }

                    if (Host.MapID == 1643)
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51340)
                {
                    if (Host.MapID == 1642)
                    {
                        if (step2 == 0)
                        {
                            Host.CommonModule.MoveTo(new Vector3F(-2158.45, 756.58, 14.71));
                            var npc = Host.GetNpcById(143913);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, 1);
                                Thread.Sleep(5000);

                            }
                        }
                        if (step2 == 1)
                        {
                            Host.CommonModule.MoveTo(new Vector3F(-2174.88, 765.17, 20.92));
                            var npc = Host.GetNpcById(135690);
                            if (npc != null)
                            {
                                /*  Host.OpenDialog(npc);
                                  Thread.Sleep(1000);*/

                                Host.MyDialog(npc, 0);

                                Thread.Sleep(5000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(10000);
                                /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                 {
                                     Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                                 }*/
                            }
                        }

                    }

                    if (Host.MapID == 1643)
                        MyComliteQuest(quest);

                    return false;
                }



                if (quest.Id == 51979)
                {
                    var npc = Host.GetNpcById(144630);
                    Host.CommonModule.MoveTo(npc, 2, 2);
                    Thread.Sleep(1000);
                    if (!Host.OpenDialog(npc))
                    {
                        Host.log("Не смог открыть диалог " + Host.GetLastError());
                    }

                    if (Host.GetNpcDialogs().Count > 0)
                    {
                        if (!Host.StartAdventureJournalQuest(51800))
                            Host.log("Не смог начать кампанию " + Host.GetLastError());
                    }
                    else
                    {
                        if (Host.GetQuest(51800) != null)
                        {
                            Host.PrepareCompleteQuest(51800);
                            if (!Host.CompleteQuest(51800, 0))
                            {
                                Host.log("Не смог завершить квест кампании" + 51800 + " с выбором награды " + 0 + "  " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(6000);
                                Host.SendKeyPress(0x1b);
                            }
                            else
                            {
                                Host.log("Завершил квест " + 51800, Host.LogLvl.Ok);
                            }
                        }
                    }


                    Thread.Sleep(3000);
                    return false;
                }

                if (quest.Id == 51803)
                {
                    var npc = Host.GetNpcById(144630);
                    Host.CommonModule.MoveTo(npc, 2, 2);
                    Thread.Sleep(1000);
                    if (!Host.OpenDialog(npc))
                    {
                        Host.log("Не смог открыть диалог " + Host.GetLastError());
                    }

                    if (Host.GetNpcDialogs().Count > 0)
                    {
                        if (!Host.StartAdventureJournalQuest(51802))
                            Host.log("Не смог начать кампанию " + Host.GetLastError());
                    }
                    else
                    {
                        if (Host.GetQuest(51802) != null)
                        {
                            Host.PrepareCompleteQuest(51802);
                            if (!Host.CompleteQuest(51802, 0))
                            {
                                Host.log("Не смог завершить квест кампании" + 51802 + " с выбором награды " + 0 + "  " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(6000);
                                Host.SendKeyPress(0x1b);
                            }
                            else
                            {
                                Host.log("Завершил квест " + 51802, Host.LogLvl.Ok);
                            }
                        }
                    }


                    Thread.Sleep(10000);
                    return false;
                }

                if (quest.Id == 52444)
                {
                    var npc = Host.GetNpcById(144630);
                    Host.CommonModule.MoveTo(npc, 2, 2);
                    Thread.Sleep(1000);
                    if (!Host.OpenDialog(npc))
                    {
                        Host.log("Не смог открыть диалог " + Host.GetLastError());
                    }

                    if (Host.GetNpcDialogs().Count > 0)
                    {
                        if (!Host.StartAdventureJournalQuest(51801))
                            Host.log("Не смог начать кампанию " + Host.GetLastError());
                    }
                    else
                    {
                        if (Host.GetQuest(51801) != null)
                        {
                            Host.PrepareCompleteQuest(51801);
                            if (!Host.CompleteQuest(51801, 0))
                            {
                                Host.log("Не смог завершить квест кампании" + 51801 + " с выбором награды " + 0 + "  " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(6000);
                                Host.SendKeyPress(0x1b);
                            }
                            else
                            {
                                Host.log("Завершил квест " + 51801, Host.LogLvl.Ok);
                            }
                        }
                    }


                    Thread.Sleep(3000);
                    return false;
                }

                if (quest.Id == 46926)
                {
                    farmMobIds.Add(125460);
                    farmMobIds.Add(125458);
                }

                if (quest.Id == 47584)
                {
                    farmMobIds.Add(123095);
                    farmMobIds.Add(123093);
                }

                if (quest.Id == 50178)
                {
                    farmMobIds.Add(132409);
                    farmMobIds.Add(132410);
                    farmMobIds.Add(132412);
                    farmMobIds.Add(123135);
                    farmMobIds.Add(132022);
                }

                if (quest.Id == 50641)
                {
                    farmMobIds.Add(128660);
                    farmMobIds.Add(128662);
                    farmMobIds.Add(128665);
                    farmMobIds.Add(129008);
                    farmMobIds.Add(134321);
                    farmMobIds.Add(134320);
                    farmMobIds.Add(129007);
                    farmMobIds.Add(128661);
                }

                if (quest.Id == 47647)
                {
                    farmMobIds.Add(128299);
                    farmMobIds.Add(128346);
                    farmMobIds.Add(128379);
                    farmMobIds.Add(128453);
                    farmMobIds.Add(128454);
                    farmMobIds.Add(136446);
                    farmMobIds.Add(123358);
                    farmMobIds.Add(128351);
                    farmMobIds.Add(128724);
                    farmMobIds.Add(128723);
                    farmMobIds.Add(136695);
                    //farmMobIds.Add(128356);
                    farmMobIds.Add(128788);
                    farmMobIds.Add(128469);
                    farmMobIds.Add(128755);
                    farmMobIds.Add(128757);
                    farmMobIds.Add(128766);
                    farmMobIds.Add(136691);
                    farmMobIds.Add(128765);
                    farmMobIds.Add(128787);
                    farmMobIds.Add(128789);
                    farmMobIds.Add(128764);
                }

                if (/*quest.Id == 47647 ||*/ quest.Id == 50805 || quest.Id == 49406 || quest.Id == 48852 || quest.Id == 48934 || quest.Id == 47996 || quest.Id == 51689 || quest.Id == 49918)
                {
                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if (!Host.CanAttack(entity, Host.CanSpellAttack))
                            continue;
                        if (farmMobIds.Contains(entity.Id))
                            continue;
                        if (entity.Id == 0)
                            continue;
                        if (entity.Level == 1)
                        {
                            continue;
                        }
                        farmMobIds.Add(entity.Id);
                    }
                }

                Thread.Sleep(1000);
                var checkIndex = objectiveindex;
                if (quest.Id == 871)
                    checkIndex = 0;
                foreach (var questPoi in quest.GetQuestPOI())
                {
                    if (questPoi.ObjectiveIndex == checkIndex)
                    {
                        double bestDist = 999999;
                        foreach (var questPoiPoint in questPoi.Points)
                        {
                            var z = Host.GetNavMeshHeight(new Vector3F(questPoiPoint.X, questPoiPoint.Y, 0));
                            if (quest.Id == 25173 && objectiveindex == 2)
                            {
                                z = 42;
                            }



                            if (questPoiPoint.X == -244.00 && questPoiPoint.Y == -5113.00)
                                continue;
                            if (questPoiPoint.X == -340.00 && questPoiPoint.Y == -3564.00)
                                continue;

                            if (z != 0)
                            {
                                questPoiPoints.Add(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            }



                            if (Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z)) > bestDist)
                                continue;
                            bestDist = Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            farmLoc = new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z);
                        }
                        break;
                    }
                }
                if (farmLoc.X == 0)
                {

                    var nearUnit = Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i));
                    foreach (var entity in nearUnit)
                    {

                        if (entity.Id == questObjective.ObjectID || entity.Id == additionalId)
                        {
                            farmLoc = entity.Location;
                            break;
                        }

                    }
                }
                if (quest.Id == 47647)
                    farmLoc = new Vector3F(768.31, 4273.29, 8.18);

                if (quest.Id == 50805)
                    farmLoc = new Vector3F(3119.19, 3106.52, 111.33);

                if (quest.Id == 48934)
                    farmLoc = new Vector3F(2522.33, 1485.76, 9.51);

                if (quest.Id == 47996)
                    farmLoc = new Vector3F(2355.44, 594.39, 5.25);
                if (quest.Id == 51689)
                    farmLoc = new Vector3F(2680.15, 169.64, 1.35);
                if (quest.Id == 49918)
                    farmLoc = new Vector3F(-1698.84, 1488.26, 120.27);

                if (quest.Id == 56063)
                    farmLoc.Z = -183;



                /*  if (farmLoc.X == 0)
                  {
                      foreach (var myNpcLoc in Host.MyNpcLocss.NpcLocs)
                      {
                          if (myNpcLoc.Id == questObjective.ObjectID || myNpcLoc.Id == additionalId)
                          {
                              farmLoc = myNpcLoc.Loc;
                              break;
                          }

                      }
                  }*/

                if (farmLoc.X == 0)
                {
                    Host.log("НПС не найден ", Host.LogLvl.Error);
                    Thread.Sleep(10000);
                    return false;
                }

                Zone zone = new RoundZone(farmLoc.X, farmLoc.Y, 1000);
                var badRadius = 0;

                //Host.log(Host.Me.Distance(farmLoc) + "");
                //  if (!zone.ObjInZone(Host.Me))

                switch (quest.Id)
                {
                    case 48549:
                        {
                            if (!Host.CommonModule.MoveTo(1401.14, 3332.29, 156.43, 2, 2))
                                return false;
                        }
                        break;
                    case 13580:
                        {
                            if (!Host.CommonModule.MoveTo(5529.57, 449.70, 34.65, 2, 2))
                                return false;
                            Host.MyUseItemAndWait(Host.MyGetItem(quest.Template.StartItem));
                            Thread.Sleep(5000);

                            farmMobIds.Add(34368);
                            farmMobIds.Add(34370);
                            Thread.Sleep(1000);
                            Host.FarmModule.SetFarmMobs(zone, farmMobIds);
                            zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 20);
                            while (!IsQuestComplite(quest.Id, objectiveindex) && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmMobs)
                            {
                                Thread.Sleep(100);

                            }
                            return false;
                        }

                    case 13995:
                        {
                            if (!Host.CommonModule.MoveTo(-1195.10, -2912.08, 117.18, 2, 2))
                                return false;
                            farmMobIds.Add(34635);
                            Thread.Sleep(5000);
                            Host.FarmModule.SetFarmMobs(zone, farmMobIds);

                            while (!IsQuestComplite(quest.Id, objectiveindex) && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmMobs)
                            {
                                Thread.Sleep(100);

                            }
                            return false;
                        }

                    case 13621:
                        {

                            //использовать предмет
                            foreach (var item in Host.ItemManager.GetItems())
                            {
                                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                                    item.Place == EItemPlace.InventoryItem)
                                    if (item.Id == quest.Template.StartItem)
                                    {

                                        if (Host.SpellManager.GetItemCooldown(item) == 0)
                                        {
                                            if (!Host.CommonModule.MoveTo(1424.19, -2000.12, 96.30, 1, 1))
                                                return false;
                                        }
                                        Host.MyUseItemAndWait(item);
                                        Thread.Sleep(25000);
                                        if (!Host.CommonModule.MoveTo(1517.53, -2142.20, 88.82, 1, 1))
                                            return false;
                                        return false;

                                    }

                            }


                            return false;
                        }


                    case 14046:
                        {
                            if (!Host.CommonModule.MoveTo(-1453, -3819, 21, 20, 20))
                                return false;
                        }
                        break;

                    case 28726:
                        {
                            if (!Host.CommonModule.MoveTo(10750.10, 922.50, 1337.19, 20, 20))
                                return false;
                        }
                        break;

                    case 14063:
                        {
                            if (!Host.CommonModule.MoveTo(-1661.08, -4354.01, 4.37))
                                return false;

                        }
                        break;
                    default:
                        {
                            if (Host.Me.Distance2D(farmLoc) > 15)
                                if (!Host.CommonModule.MoveTo(farmLoc, 20, 20))
                                {
                                    if (Host.Me.Distance(farmLoc) > 100)
                                        return false;
                                }


                        }
                        break;
                }


                if (quest.Id == 49141)
                {
                    farmMobIds.Add(290707);
                    Host.FarmModule.SetFarmProps(zone, farmMobIds);
                    //   zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 20);
                    while (!IsQuestComplite(quest.Id, objectiveindex) && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmProps)
                    {
                        Thread.Sleep(100);
                        if (Host.FarmModule.BestProp == null && Host.Me.HpPercents > 50)
                            badRadius++;
                        else
                            badRadius = 0;
                        if (Host.FarmModule.BestMob != null)
                            badRadius = 0;

                        /*   if (Host.FarmModule.bestProp == null && Host.Me.HpPercents > 80)
                               badRadius++;
                           else
                               badRadius = 0;*/
                        if (badRadius > 50)
                        {
                            var findPoint = farmLoc;
                            if (questPoiPoints.Count > 0)
                                findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                            Host.log("Не могу найти GameObject, подбегаю в центр зоны " + Host.Me.Distance(findPoint) + "    " + questPoiPoints.Count);
                            Host.CommonModule.MoveTo(findPoint);
                        }
                    }


                    return false;
                }

                if (quest.Id == 14038)
                {
                    if (!Host.CommonModule.ForceMoveTo(-1456.26, -3966.92, -5.38))
                        return false;
                    foreach (var item in Host.ItemManager.GetItems())
                    {
                        if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                            item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                            item.Place == EItemPlace.InventoryItem)
                            if (item.Id == quest.Template.StartItem)
                            {
                                Host.MyUseItemAndWait(item);
                                return false;

                            }
                    }

                    return false;
                }

                var isSpellOnUnit = false;
                uint mobIdSpell = 0;

                if (quest.Id == 48988)
                {
                    isSpellOnUnit = true;
                    mobIdSpell = 122683;
                }

                if (quest.Id == 29087)
                {
                    isSpellOnUnit = true;
                    mobIdSpell = 52171;
                }

                if (quest.Id == 49348)
                {
                    isSpellOnUnit = true;
                    mobIdSpell = 129086;
                }

                if (quest.Id == 51991)
                {
                    isSpellOnUnit = true;
                    mobIdSpell = 143377;
                }

                if (isSpellOnUnit)
                {
                    MyUseSpellClick(mobIdSpell);
                    return false;
                }



                if (quest.Id == 25188)
                    return false;


                if (quest.Id == 14063)
                {
                    if (!Host.CommonModule.MoveTo(-1661.08, -4354.01, 4.37))
                        return false;

                    Item item = null;
                    foreach (var item1 in Host.ItemManager.GetItems())
                    {
                        if (item1.Id == quest.Template.StartItem)
                            item = item1;
                    }

                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5000);
                        while (!IsQuestComplite(quest.Id, objectiveindex))
                        {
                            Thread.Sleep(5000);
                        }
                    }

                    else
                    {
                        Host.log("Не нашел итем для квеста ", Host.LogLvl.Error);
                    }
                }

                if (quest.Id == 25187)
                {
                    if (!Host.CommonModule.MoveTo(394.30, -4584.43, 76.64))
                        return false;

                    Item item = null;
                    foreach (var item1 in Host.ItemManager.GetItems())
                    {
                        if (item1.Id == quest.Template.StartItem)
                            item = item1;
                    }

                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5000);
                        while (!IsQuestComplite(quest.Id, objectiveindex))
                        {
                            Thread.Sleep(5000);
                        }
                    }

                    else
                    {
                        Host.log("Не нашел итем для квеста ", Host.LogLvl.Error);
                    }
                    return false;
                }

                if (quest.Id == 881)
                {
                    if (!Host.CommonModule.MoveTo(127.60, -2539.83, 91.67))
                        return false;

                    Item item = null;
                    foreach (var item1 in Host.ItemManager.GetItems())
                    {
                        if (item1.Id == quest.Template.StartItem)
                            item = item1;
                    }

                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item);
                        Thread.Sleep(5000);

                    }

                    else
                    {
                        Host.log("Не нашел итем для квеста ", Host.LogLvl.Error);
                    }
                    return false;
                }


                if (quest.Id == 13988)
                {
                    Thread.Sleep(20000);
                    if (!IsQuestComplite(quest.Id, objectiveindex))
                    {
                        var npc = Host.GetNpcById(34631);
                        if (npc == null)
                        {
                            Host.log("Не нашел орла");
                            if (!Host.CommonModule.MoveTo(-1430.30, -3276.65, 201.93))
                                return false;

                            foreach (var item in Host.ItemManager.GetItems())
                            {
                                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                                    item.Place == EItemPlace.InventoryItem)
                                    if (item.Id == 46782)//Тотем тонги
                                    {
                                        Host.MyUseItemAndWait(item);

                                    }
                            }
                        }
                        if (!Host.CommonModule.MoveTo(-1483.08, -3299.27, 210.77, 2, 2))
                            return false;
                        Thread.Sleep(5000);
                    }

                    return false;
                }




                var mobId = questObjective.ObjectID;
                if (quest.Id == 25165)
                    mobId = 3125;
                farmMobIds.Add(Convert.ToUInt32(mobId));

                if (additionalId != 0)
                    farmMobIds.Add(additionalId);
                if (additionalId2 != 0)
                    farmMobIds.Add(additionalId2);


                if (quest.Id == 13523)
                {
                    farmMobIds.Clear();
                    farmMobIds.Add(32890);
                }

                if (quest.Id == 47327)
                {
                    farmMobIds.Add(123775);
                    farmMobIds.Add(123774);
                    farmMobIds.Add(123773);
                }


                if (quest.Id == 49490)
                {
                    farmMobIds.Add(129513);
                    farmMobIds.Add(134156);
                    farmMobIds.Add(128933);
                }




                if (quest.Id == 49493)
                {
                    mobId = 129752;
                    QuestType = ExecuteType.MonsterHunt;
                    MyQuestHelp.MonsterHunt(quest, zone, farmMobIds, objectiveindex, farmLoc, questPoiPoints, Host);
                    return false;
                }

                var isCanAttack = false;
                var findNpc = false;



                if (quest.Id == 13576)
                {
                    farmMobIds.Clear();
                    farmMobIds.Add(32999);
                    isCanAttack = false;
                    findNpc = true;
                }


                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (!entity.IsAlive)
                        continue;
                    if (farmMobIds.Contains(entity.Id))
                    {
                        findNpc = true;
                        //  Host.log(entity.Id + " " + Host.CanAttack(entity, Host.CanSpellAttack));
                        if (Host.CanAttack(entity, Host.CanSpellAttack))
                        {

                            isCanAttack = true;
                        }
                    }
                }

                if (farmMobIds.Contains(37989))
                    isCanAttack = false;

                if (farmMobIds.Contains(3125))//39236
                {
                    isCanAttack = false;
                    findNpc = true;
                }

                if (quest.Id == 13946)
                {
                    isCanAttack = false;
                    findNpc = true;
                }

                if (quest.Id == 13565 && objectiveindex == 1)
                {
                    isCanAttack = false;
                    findNpc = true;
                }

                if (quest.Id == 13523)
                {
                    // mobId = 32890;
                    farmMobIds.Add(32890);
                    isCanAttack = false;
                    findNpc = true;
                }

                if (quest.Id == 14046)
                {
                    // mobId = 32890;
                    //  farmMobIds.Add(32890);
                    isCanAttack = false;
                    findNpc = true;
                }

                if (quest.Id == 49666)
                {
                    isCanAttack = false;
                    findNpc = true;
                    farmMobIds.Add(134560);
                }

                if (quest.Id == 48573)
                {
                    isCanAttack = false;
                    findNpc = true;
                    farmMobIds.Add(126723);
                }

                if (quest.Id == 49078)
                {
                    isCanAttack = false;
                    findNpc = true;
                    farmMobIds.Add(128071);
                }

                if (quest.Id == 48532)
                {
                    isCanAttack = false;
                    findNpc = true;
                    farmMobIds.Add(126627);
                }

                if (quest.Id == 50817)
                {
                    isCanAttack = false;
                    findNpc = true;
                }

                if (quest.Id == 47943)
                {
                    isCanAttack = false;
                    findNpc = true;
                    farmMobIds.Add(123814);
                }


                // Host.log("Можно ли атаковать НПС " + isCanAttack + " " + findNpc);
                if (quest.Id == 47311)
                {
                    farmMobIds.Clear();
                    mobId = 122504;
                    farmMobIds.Add(122504);
                    isCanAttack = true;
                    findNpc = true;
                }

                foreach (var farmMobId in farmMobIds)
                {
                    Host.log("НПС для квеста 1 " + farmMobId);
                }

                if (quest.Id == 50656)
                {
                    farmMobIds.Add(277910);
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 48790)
                {
                    farmMobIds.Add(282631);
                    farmMobIds.Add(282634);
                    farmMobIds.Add(282632);
                    farmMobIds.Add(282633);
                    farmMobIds.Add(282635);
                    farmMobIds.Add(282636);
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }


                if (QuestType == ExecuteType.ItemGatherFromGameObject)
                {
                    MyQuestHelp.ItemGatherFromGameObject(quest, zone, farmMobIds, objectiveindex, farmLoc, questPoiPoints, Host);
                    return false;
                }


                if (!isCanAttack && findNpc)
                {
                    Host.log("Указанного НПС нельзя атаковать " + questObjective.ObjectID);

                    if (quest.Id == 50536)
                    {
                        Host.CanselForm();
                        var npc = Host.GetNpcById(134245);
                        if (npc != null)
                        {
                            Host.CommonModule.MoveTo(npc, 2);
                            if (!Host.SpellManager.UseSpellClick(npc as Unit))
                                Host.log("Не смог использовать спел клик " + Host.GetLastError());
                            Thread.Sleep(5000);
                            return false;
                        }


                    }

                    if (quest.Id == 47320)
                    {
                        if (!Host.CommonModule.MoveTo(2043.82, 2818.25, 50.42))
                            return false;
                        var item = Host.MyGetItem(150759);
                        var npc = Host.GetNpcById(122741);
                        Host.MyUseItemAndWait(item, npc);
                        Host.Wait(5000);
                        return false;
                    }
                    if (quest.Id == 48555)
                    {

                        while (quest.State == EQuestState.None)
                        {
                            Thread.Sleep(100);
                            quest = Host.GetQuest(quest.Id);
                            var step = 0;
                            foreach (var questCount in quest.Counts)
                                step = step + questCount;
                            if (step >= 8)
                                return false;

                            if (!Host.MainForm.On)
                                return false;
                            if (Host.GetAgroCreatures().Count > 0)
                                continue;
                            Host.FarmSpellClick(farmMobIds);
                        }


                        return false;
                    }

                    if (quest.Id == 50739)
                    {

                        while (quest.State == EQuestState.None)
                        {
                            Thread.Sleep(100);
                            quest = Host.GetQuest(quest.Id);
                            var step = 0;
                            foreach (var questCount in quest.Counts)
                                step = step + questCount;
                            if (step >= 7)
                                return false;

                            if (!Host.MainForm.On)
                                return false;
                            if (Host.GetAgroCreatures().Count > 0)
                                continue;
                            Host.FarmSpellClick(farmMobIds);
                            if (Host.GetNpcById(farmMobIds[0]) == null)
                                Host.CommonModule.MoveTo(farmLoc, 20, 20);
                        }


                        return false;
                    }

                    if (quest.Id == 13557 || quest.Id == 51663) //13557 State:None LogTitle:Неожиданная удача 
                    {
                        var dist = 3;
                        farmMobIds.Clear();
                        farmMobIds.Add(194133);
                        farmMobIds.Add(194124);

                        farmMobIds.Add(290749);
                        farmMobIds.Add(290748);
                        Host.FarmModule.SetFarmProps(zone, farmMobIds, Convert.ToInt32(quest.Template.StartItem), dist);

                        while (!IsQuestComplite(quest.Id, objectiveindex) && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmProps)
                        {
                            Thread.Sleep(100);
                            if (Host.FarmModule.BestProp == null && Host.Me.HpPercents > 50)
                                badRadius++;
                            else
                                badRadius = 0;
                            if (Host.FarmModule.BestMob != null)
                                badRadius = 0;

                            /*   if (Host.FarmModule.bestProp == null && Host.Me.HpPercents > 80)
                                   badRadius++;
                               else
                                   badRadius = 0;*/
                            if (badRadius > 50)
                            {
                                var findPoint = farmLoc;
                                if (questPoiPoints.Count > 0)
                                    findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                                Host.log("Не могу найти GameObject, подбегаю в центр зоны " + Host.Me.Distance(findPoint) + "    " + questPoiPoints.Count);
                                Host.CommonModule.MoveTo(findPoint);
                            }
                        }

                        Host.FarmModule.StopFarm();
                        Thread.Sleep(1000);
                        return false;


                    }


                    var useSpellOnCreature = false;
                    uint useSpellOnCreatureSpellId = 0;
                    uint useSpellOnCreatureId = 0;

                    if (quest.Id == 13878)
                    {
                        useSpellOnCreature = true;
                        useSpellOnCreatureId = 34287;
                        useSpellOnCreatureSpellId = 8386;
                    }



                    //Испоьлзовать скилл на НПС
                    if (useSpellOnCreature)
                    {
                        var unit = Host.GetNpcById(useSpellOnCreatureId);
                        if (unit != null && unit.Type == EBotTypes.Unit)
                        {
                            if (!Host.CommonModule.MoveTo(unit, 2, 2))
                                return false;

                            var result = Host.SpellManager.CastSpell(useSpellOnCreatureSpellId, unit);
                            if (result == ESpellCastError.SUCCESS)
                            {
                                Host.log("Использовал useSpellOnCreature " + result, Host.LogLvl.Ok);
                                Host.MyCheckIsMovingIsCasting();
                                while (Host.SpellManager.IsChanneling)
                                    Thread.Sleep(50);
                            }
                            else
                            {
                                Host.log("Не смог использовать useSpellOnCreature " + useSpellOnCreatureSpellId + "  " + Host.GetLastError() + " " + result, Host.LogLvl.Error);
                            }

                        }
                        else
                        {
                            Host.log("Не нашел НПС для использования скила " + useSpellOnCreatureId, Host.LogLvl.Error);
                        }

                        Thread.Sleep(1000);
                        return false;
                    }


                    //Использовать предмет на НПС
                    if (questObjective.Description.Contains("Кровопалые детеныши спасены")
                        || questObjective.Description.Contains("Поймайте Стремительного Когтя")
                        || questObjective.Description.Contains("Собрано образцов яда скорпидов")
                        || quest.Id == 25236
                        || quest.Id == 13946
                        || quest.Id == 13557
                        || quest.Id == 13523
                        || quest.Id == 13576
                        || quest.Id == 13613
                        || quest.Id == 49666
                        || quest.Id == 47130
                        || quest.Id == 48573
                        || quest.Id == 49078
                        || quest.Id == 48532
                        || quest.Id == 50817
                        || quest.Id == 50771
                        || quest.Id == 47924
                        || quest.Id == 49125
                        || quest.Id == 47943
                        || (quest.Id == 13565 && objectiveindex == 1)
                        )
                    {
                        Item item = null;
                        foreach (var item1 in Host.ItemManager.GetItems())
                        {
                            if (item1.Id == quest.Template.StartItem)
                            {
                                item = item1;
                                break;
                            }
                        }
                        if (item != null)
                        {
                            var dist = 5;
                            if (quest.Id == 13613)
                                dist = 0;
                            if (quest.Id == 47130)
                                dist = 0;
                            if (quest.Id == 49078)
                                dist = 20;
                            Host.FarmModule.SetFarmMobs(zone, farmMobIds, Convert.ToInt32(item.Id), dist);

                            while (!IsQuestComplite(quest.Id, objectiveindex) && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmMobs)
                            {
                                Thread.Sleep(100);
                                if (Host.FarmModule.BestMob == null && Host.Me.HpPercents > 80)
                                    badRadius++;
                                else
                                    badRadius = 0;
                                if (badRadius > 100)
                                {
                                    var findPoint = farmLoc;
                                    if (questPoiPoints.Count > 0)
                                        findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                                    Host.log("Не могу найти мобов, подбегаю в центр зоны " + Host.Me.Distance(findPoint));
                                    Host.CommonModule.MoveTo(findPoint);
                                }

                            }

                            Host.FarmModule.StopFarm();
                            Thread.Sleep(1000);
                            return false;
                        }
                        else
                        {
                            Host.log("Не найден указанный итем в инвентаре ", Host.LogLvl.Error);
                        }
                    }


                    //Диалог
                    if (questObjective.Description.Contains("Поговори")
                        || quest.Id == 13518
                        || quest.Id == 13547
                        || quest.Id == 14046
                        || quest.Id == 47959
                        )
                    {

                        var npcId = questObjective.ObjectID;
                        if (quest.Id == 14046)
                            npcId = 3467;
                        var npc = Host.GetNpcById(Convert.ToUInt32(npcId));
                        if (npc != null)
                        {

                            if (!Host.CommonModule.MoveTo(npc))
                                return false;
                            Thread.Sleep(500);


                            if (!Host.OpenDialog(npc))
                            {
                                Host.log("Не смог начать диалог для выбора диалога с " + npc.Name + "[" + npc.Id + "] " + Host.GetLastError(), Host.LogLvl.Error);
                                if (Host.GetLastError() == ELastError.ActionNotAllowed)
                                {
                                    MyMoveFromNpc(npc as Unit);
                                    return false;
                                }
                            }

                            Thread.Sleep(500);
                            foreach (var gossipOptionsData in Host.GetNpcDialogs())
                            {
                                Host.log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  ");
                                if (gossipOptionsData.Text.Contains("принять вызов") || gossipOptionsData.Text.Contains("ready to face") || quest.Id == 13547)
                                {
                                    if (!Host.SelectNpcDialog(gossipOptionsData))
                                        Host.log("Не смог выбрать диалог 1 " + Host.GetLastError(), Host.LogLvl.Error);
                                    return false;
                                }

                                if (quest.Id == 14046 && gossipOptionsData.ClientOption == 1)
                                {
                                    if (!Host.SelectNpcDialog(gossipOptionsData))
                                        Host.log("Не смог выбрать диалог 2 " + Host.GetLastError(), Host.LogLvl.Error);
                                    return false;
                                }

                            }
                            Host.log("Необходим диалог ");
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            Host.log("Не найден НПС для диалога ", Host.LogLvl.Error);
                        }

                    }

                    Thread.Sleep(5000);
                    return false;
                }


                var startItem = quest.Template.StartItem;
                if (quest.Id == 48855 || quest.Id == 49071 || quest.Id == 47577)
                    startItem = 0;

                if (quest.Id == 47577)
                {
                    QuestType = ExecuteType.MonsterHunt;
                    // farmMobIds.Add(142991);
                    farmMobIds.Add(134062);
                    farmMobIds.Add(134068);
                    farmMobIds.Add(134059);
                }

                if (farmMobIds.Count > 0 && startItem == 0)
                {
                    QuestType = ExecuteType.MonsterHunt;
                    MyQuestHelp.MonsterHunt(quest, zone, farmMobIds, objectiveindex, farmLoc, questPoiPoints, Host);
                    QuestType = ExecuteType.Unknown;
                }

                if (QuestType == ExecuteType.Unknown)
                {
                    Host.log("Не найден тип квеста ", Host.LogLvl.Error);
                }

                return false;
            }
            catch (Exception e)
            {
                Host.log(" " + e);

            }
            return false;
        }

        public bool QuestTypeItem(Quest quest, int objectiveindex, QuestObjective questObjective)
        {
            try
            {
                var farmLoc = new Vector3F();
                uint mobId = 0;
                uint addMobId = 0;

                var questPoiPoints = new List<Vector3F>();

                if (quest.Id == 25232)
                {
                    mobId = 3199;
                    addMobId = 3196;
                }

                if (quest.Id == 47499 && objectiveindex == 0)
                    mobId = 128346;

                if (quest.Id == 47499 && objectiveindex == 1)
                    mobId = 128454;

                if (quest.Id == 47499 && objectiveindex == 2)
                    mobId = 136446;


                if (quest.Id == 25176)
                    mobId = 202648;

                if (quest.Id == 47570)
                    mobId = 134250;

                if (quest.Id == 48850)
                    mobId = 135326;


                if (quest.Id == 25178)
                    mobId = 3236;

                if (quest.Id == 50535)
                    mobId = 134121;

                if (quest.Id == 834)
                {

                    mobId = 3290;
                }


                if (quest.Id == 844)
                {
                    mobId = 3244;
                    addMobId = 3246;
                }

                if (quest.Id == 5041)
                {
                    mobId = 175708;

                }
                if (quest.Id == 872)
                    mobId = 3438;

                if (quest.Id == 903)
                    mobId = 3415;

                if (quest.Id == 845)
                {
                    mobId = 3242;
                    addMobId = 44166;
                }
                if (quest.Id == 848)
                {
                    mobId = 3640;

                }
                if (quest.Id == 850)
                {
                    mobId = 3394;
                }

                if (quest.Id == 880)
                {
                    mobId = 3461;
                }

                if (quest.Id == 49677)
                {
                    mobId = 130466;
                }

                if (quest.Id == 48871)
                {
                    mobId = 281608;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 48585)
                {
                    mobId = 273836;
                    addMobId = 273837;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                //Альянс


                if (quest.Id == 28724)//Косяк в базе
                {
                    mobId = 207346;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 47248)
                {
                    mobId = 270991;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                /*  if (quest.Id == 2438)
                  {
                      mobId = 126158;
                      isGameObject = true;
                  }*/

                if (quest.Id == 489)
                {
                    mobId = 1673;

                }

                if (quest.Id == 47871 && objectiveindex == 0)
                {
                    mobId = 124593;

                }

                if (quest.Id == 47871 && objectiveindex == 1)
                {
                    mobId = 272292;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 47871 && objectiveindex == 2)
                {
                    mobId = 272294;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }


                if (quest.Id == 488 && objectiveindex == 0)
                {
                    mobId = 2042;
                    addMobId = 2043;
                }
                if (quest.Id == 488 && objectiveindex == 1)
                {
                    mobId = 1995;
                    addMobId = 1996;
                }

                if (quest.Id == 488 && objectiveindex == 2)
                {
                    mobId = 1998;
                    addMobId = 1999;
                }

                if (quest.Id == 50775)
                {
                    mobId = 135006;
                    addMobId = 136225;
                }


                if (quest.Id == 932)
                {
                    mobId = 2038;

                }

                if (quest.Id == 2459)
                {
                    mobId = 7234;
                }

                if (quest.Id == 483 && objectiveindex == 0)
                {
                    mobId = 2740;

                }
                if (quest.Id == 483 && objectiveindex == 1)
                {
                    mobId = 2739;

                }
                if (quest.Id == 483 && objectiveindex == 2)
                {
                    mobId = 2741;

                }
                if (quest.Id == 483 && objectiveindex == 3)
                {
                    mobId = 2742;

                }

                if (quest.Id == 13521)
                {
                    mobId = 32935;
                }

                if (quest.Id == 851)
                {
                    mobId = 34846;
                }

                if (quest.Id == 48825)
                {
                    mobId = 127225;
                }

                if (quest.Id == 13520)
                {
                    mobId = 194107;

                }

                if (quest.Id == 13598)
                {
                    mobId = 194208;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 47316)
                {
                    mobId = 271844;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }
                if (quest.Id == 48992)
                {
                    mobId = 277285;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 48313)
                {
                    mobId = 273193;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 48315)
                {
                    mobId = 281558;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 49491)
                {
                    mobId = 278453;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 49814)
                {
                    mobId = 279044;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }


                if (quest.Id == 47312)
                {
                    mobId = 139365;
                }

                if (quest.Id == 48993)
                {
                    mobId = 134052;
                }


                if (quest.Id == 48314)
                {
                    mobId = 122153;
                }

                if (quest.Id == 48574)
                {
                    mobId = 126689;
                }

                if (quest.Id == 49801)
                {
                    mobId = 131554;
                }


                var isSpellOnUnit = false;
                if (quest.Id == 13527)
                {
                    isSpellOnUnit = true;

                    mobId = 32975;
                }

                if (quest.Id == 29087)
                {

                    isSpellOnUnit = true;

                    mobId = 52171;
                }

                if (quest.Id == 4021)
                {
                    mobId = 9523;
                }

                if (quest.Id == 51574)
                {
                    mobId = 138107;
                }

                if (quest.Id == 47319)
                {
                    mobId = 122678;
                }

                if (quest.Id == 47491)
                {
                    mobId = 122684;
                }

                var farmMobIds = new List<uint>();

                if (quest.Id == 47321)
                {
                    mobId = 122746;
                }

                if (quest.Id == 48550)
                {
                    mobId = 129276;
                }

                if (quest.Id == 50748)
                {
                    mobId = 129811;
                    farmMobIds.Add(129821);
                    if (Host.Me.Distance(2690.47, 3378.57, 111.81) > 50)
                    {
                        if (!Host.CommonModule.MoveTo(2738.37, 3425.62, 68.21))
                            return false;
                        if (!Host.CommonModule.MoveTo(2690.47, 3378.57, 111.81))
                            return false;
                    }
                }
                if (quest.Id == 51602)
                {
                    mobId = 135311;

                }

                if (quest.Id == 49227)
                {
                    mobId = 128418;

                }

                if (quest.Id == 47939)
                {
                    mobId = 128540;
                    farmMobIds.Add(124635);
                    farmMobIds.Add(135766);

                    farmMobIds.Add(124639);
                    farmMobIds.Add(124638);

                }

                if (quest.Id == 50154)
                {
                    mobId = 126722;
                    farmMobIds.Add(126726);
                    farmMobIds.Add(126725);
                }
                if (quest.Id == 50979)
                {
                    mobId = 136144;
                    farmMobIds.Add(136109);
                }


                if (quest.Id == 48531)
                    mobId = 126645;

                if (quest.Id == 48533)
                    mobId = 126502;

                if (quest.Id == 48534)
                    mobId = 126644;

                if (quest.Id == 49776)
                    mobId = 130720;

                if (quest.Id == 48452)
                    mobId = 126153;

                if (quest.Id == 51439)
                    mobId = 138430;


                if (quest.Id == 48657)
                {
                    mobId = 275099;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }
                if (quest.Id == 49774)
                {
                    mobId = 279293;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 51990)
                {
                    mobId = 270040;
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 49333)
                {
                    mobId = 278193;
                    farmMobIds.Add(278190);
                    farmMobIds.Add(278191);
                    farmMobIds.Add(278192);
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }

                if (quest.Id == 51998)
                {
                    mobId = 133854;
                    farmMobIds.Add(122113);

                }


                if (quest.Id == 47272)
                {
                    mobId = 291236;
                    farmMobIds.Add(291235);
                    farmMobIds.Add(291234);

                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }


                if (quest.Id == 46929 && objectiveindex == 0)
                {

                    mobId = 120900;//, 82.14
                    farmMobIds.Add(139961);
                }
                if (quest.Id == 46929 && objectiveindex == 1)
                {


                    mobId = 121017;
                    farmMobIds.Add(130230);
                    farmMobIds.Add(130242);
                }


                var sw = new Stopwatch();
                if (Host.AdvancedLog)
                    sw.Start();

                foreach (var dropBase in Host.DropBases.Drop)
                {

                    if (questObjective.ObjectID == dropBase.ItemId)
                    {
                        if (dropBase.Type == "object")
                        {

                            QuestType = ExecuteType.ItemGatherFromGameObject;
                        }

                        if (quest.Id == 4021)
                            QuestType = ExecuteType.Unknown;

                        foreach (var u in dropBase.MobsId)
                        {
                            farmMobIds.Add(u);
                            if (mobId == 0)
                                mobId = u;
                        }
                    }
                }

                if (Host.AdvancedLog)
                {
                    Host.log("DropBases   за  " + sw.ElapsedMilliseconds + " мс " + " всего итемов " + Host.DropBases.Drop.Count + " " + questObjective.ObjectID);
                    sw.Stop();
                }




                if (quest.Id == 14034)
                {
                    if (!Host.CommonModule.MoveTo(-1050.91, -3648.82, 23.88))
                        return false;

                    var npc = Host.GetNpcById(34754);
                    if (npc != null)
                    {

                        if (!Host.CommonModule.MoveTo(npc))
                            return false;
                        Thread.Sleep(500);
                        if (!Host.OpenDialog(npc))
                        {
                            Host.log("Не смог начать диалог для выбора диалога с " + npc.Name + "[" + npc.Id + "] " + Host.GetLastError(), Host.LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                MyMoveFromNpc(npc as Unit);
                                return false;
                            }
                        }

                        Thread.Sleep(500);
                        foreach (var gossipOptionsData in Host.GetNpcDialogs())
                        {
                            Host.log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  ");
                            if (!Host.SelectNpcDialog(gossipOptionsData))
                                Host.log("Не смог выбрать диалог " + Host.GetLastError(), Host.LogLvl.Error);
                            return false;


                        }
                        // Host.log("Необходим диалог ");
                        // Thread.Sleep(5000);
                    }
                    else
                    {
                        Host.log("Не найден НПС для диалога ", Host.LogLvl.Error);
                    }
                }


                if (quest.Id == 929)//Использовать предмет в координатах questPoi Flags:3    "Flags2": 1,
                {
                    if (!Host.CommonModule.MoveTo(9859.54, 588.11, 1300.66))
                        return false;
                    var item = Host.MyGetItem(quest.Template.StartItem);
                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item);
                        return false;
                    }
                    else
                    {
                        Host.log("Не нашел итем для квеста ", Host.LogLvl.Error);
                        return false;
                    }
                }

                if (quest.Id == 28729)//Использовать предмет в координатах  questPoi Flags:7    "Flags2": 1,
                {
                    if (!Host.CommonModule.MoveTo(10708.67, 762.91, 1321.20))
                        return false;
                    var item = Host.MyGetItem(quest.Template.StartItem);
                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item);
                        return false;
                    }
                    else
                    {
                        Host.log("Не нашел итем для квеста ", Host.LogLvl.Error);
                        return false;
                    }
                }




                if (mobId == 0)
                {
                    Host.log("Не указан айди моба для квеста " + quest.Id);
                    Thread.Sleep(10000);
                    return false;
                }




                //Thread.Sleep(1000);
                foreach (var questPoi in quest.GetQuestPOI())
                {
                    if (questPoi.ObjectiveIndex == objectiveindex)
                    {
                        double bestDist = 999999;
                        foreach (var questPoiPoint in questPoi.Points)
                        {
                            var z = Host.GetNavMeshHeight(new Vector3F(questPoiPoint.X, questPoiPoint.Y, 0));

                            if (questPoiPoint.X == -1175.00 && questPoiPoint.Y == -4923.00)
                                continue;

                            if (questPoiPoint.X == -1096.00 && questPoiPoint.Y == -2755.00)
                                continue;

                            if (questPoiPoint.X == 2622.00 && questPoiPoint.Y == 2939.00)
                                continue;


                            if (z != 0)
                            {
                                questPoiPoints.Add(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            }
                            if (Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z)) > bestDist)
                                continue;
                            bestDist = Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            farmLoc = new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z);

                            if (Host.AdvancedLog)
                                Host.log("Item POI " + farmLoc);

                        }
                        break;
                    }
                }


                if (farmLoc.X == 0)
                {
                    Host.log("НПС не найден ", Host.LogLvl.Error);
                    Thread.Sleep(10000);
                    return false;
                }


                if (quest.Id == 858)
                    farmLoc.Z = (float)105.12;
                if (quest.Id == 46929 && objectiveindex == 0)
                {
                    farmLoc = new Vector3F(-1725.33, 790.63, 81.79);
                }

                if (quest.Id == 47319)
                {
                    farmLoc = new Vector3F(2056.12, 3007.73, 46.32);
                }

                if (quest.Id == 47319)
                {
                    farmLoc = new Vector3F(2056.12, 3007.73, 46.32);
                }

                Zone zone = new RoundZone(farmLoc.X, farmLoc.Y, 1000);
                var badRadius = 0;


                // Host.log(Host.Me.Distance(farmLoc) + "");

                var dist = 20;
                if (quest.Id == 24625)
                    dist = 50;

                if (quest.Id == 24625)
                    if (!Host.CommonModule.MoveTo(-1336.34, -5183.75, 3.26))
                        return false;



                if (quest.Id == 13558)
                {
                    if (!Host.CommonModule.MoveTo(6455.36, 654.66, 21.64))
                        return false;
                    var gameObject = Host.GetNpcById(194145);
                    if (gameObject != null)
                    {
                        Host.FarmModule.InteractWithProp(gameObject as GameObject);
                    }
                    Thread.Sleep(30000);
                    return false;
                }
                farmMobIds.Add(mobId);

                switch (quest.Id)
                {
                    case 51574:
                        {
                            if (!Host.CommonModule.MoveTo(2041.83, 3014.50, 48.99))
                                return false;
                            while (quest.State == EQuestState.None)
                            {
                                Thread.Sleep(100);
                                quest = Host.GetQuest(quest.Id);
                                var step = 0;
                                foreach (var questCount in quest.Counts)
                                    step = step + questCount;
                                if (step >= 16)
                                    return false;

                                if (!Host.MainForm.On)
                                    return false;
                                if (Host.GetAgroCreatures().Count > 0)
                                    continue;
                                Host.FarmSpellClick(farmMobIds);
                            }

                        }
                        break;
                    default:
                        {
                            if (!Host.CommonModule.MoveTo(farmLoc, dist, dist))
                                return false;
                        }
                        break;
                }

                // if (!zone.ObjInZone(Host.Me))








                if (addMobId != 0)
                    farmMobIds.Add(addMobId);

                foreach (var farmMobId in farmMobIds)
                {
                    Host.log("НПС для квеста 2 " + farmMobId);
                }



                if (isSpellOnUnit)
                {
                    MyUseSpellClick(mobId);
                    return false;
                }

                if (quest.Id == 48533)
                {
                    Host.FarmModule.SetFarmMobs(zone, farmMobIds);

                    while (!IsQuestComplite(quest.Id, objectiveindex) && Host.FarmModule.readyToActions && Host.FarmModule.farmState == FarmState.FarmMobs)
                    {
                        Thread.Sleep(100);

                        if (Host.FarmModule.BestMob == null && Host.Me.HpPercents > 80)
                            badRadius++;
                        else
                            badRadius = 0;
                        if (badRadius > 100)
                        {
                            var findPoint = farmLoc;
                            if (questPoiPoints.Count > 0)
                                findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                            Host.log("Не могу найти мобов, подбегаю в центр зоны " + Host.Me.Distance(findPoint));
                            Host.CommonModule.MoveTo(findPoint);
                        }

                    }

                    Host.FarmModule.StopFarm();
                    Thread.Sleep(1000);
                    return false;
                }

                if (QuestType == ExecuteType.ItemGatherFromGameObject)
                {
                    MyQuestHelp.ItemGatherFromGameObject(quest, zone, farmMobIds, objectiveindex, farmLoc, questPoiPoints, Host);
                    return false;
                }


                if (farmMobIds.Count > 0)
                {
                    QuestType = ExecuteType.ItemGatherFromMonster;
                    MyQuestHelp.ItemGatherFromMonster(quest, zone, farmMobIds, objectiveindex, farmLoc, questPoiPoints, Host);
                    return false;

                }


                return false;
            }
            catch (Exception e)
            {
                Host.log(" " + e);

            }
            return false;
        }

        public bool QuestTypeGameObject(Quest quest, int objectiveindex, QuestObjective questObjective)
        {
            try
            {
                //Получить координаты
                var farmLoc = new Vector3F();
                List<Vector3F> questPoiPoints = new List<Vector3F>();
                foreach (var questPoi in quest.GetQuestPOI())
                {
                    if (questPoi.ObjectiveIndex == objectiveindex)
                    {
                        double bestDist = 999999;

                        foreach (var questPoiPoint in questPoi.Points)
                        {
                            var z = Host.GetNavMeshHeight(new Vector3F(questPoiPoint.X, questPoiPoint.Y, 0));
                            if (Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z)) > bestDist)
                                continue;
                            questPoiPoints.Add(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            bestDist = Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            farmLoc = new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z);
                        }
                        break;
                    }
                }
                if (quest.Id == 47259)
                {
                    farmLoc = new Vector3F(-1051.52, -86.46, 253.64);
                }
                if (farmLoc.X == 0)
                {
                    Host.log("Координаты GameObject не найдены ", Host.LogLvl.Error);
                    return false;
                }

                //Дойти по координатам
                if (Host.Me.Distance2D(farmLoc) > 25)
                    if (!Host.CommonModule.MoveTo(farmLoc, 30, 30))
                        return false;

                //Использовать скилл
                var zone = new RoundZone(farmLoc.X, farmLoc.Y, 1000);

                var farmmoblist = new List<uint>();
                //   if (!zone.ObjInZone(host.Me))



                farmmoblist.Add(Convert.ToUInt32(questObjective.ObjectID));
                if (quest.Id == 47259)
                    farmmoblist[0] = 270918;

                if (farmmoblist.Count == 0)
                {
                    Host.log("Не нашел пропы или фильтр пропов пустой", Host.LogLvl.Error);
                    Host.MainForm.On = false;
                }
                foreach (var i in farmmoblist)
                    Host.log("QuestTypeGameObject " + i);

                var useItem = 0;
                if (quest.Id == 877)
                    useItem = 5068;

                if (useItem == 0)
                {
                    Host.FarmModule.SetFarmProps(zone, farmmoblist);
                }
                else
                {
                    Host.FarmModule.SetFarmProps(zone, farmmoblist, useItem);
                }
                int badRadius = 0;
                while (Host.MainForm.On
                       && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                       && !IsQuestComplite(quest.Id, objectiveindex)
                       && Host.FarmModule.readyToActions
                       && Host.FarmModule.farmState == FarmState.FarmProps)
                {
                    if (Host.MyIsNeedRepair())
                        break;
                    Thread.Sleep(100);
                    if (Host.FarmModule.BestProp == null && Host.Me.HpPercents > 50)
                        badRadius++;
                    else
                        badRadius = 0;
                    if (Host.FarmModule.BestMob != null)
                        badRadius = 0;
                    if (badRadius > 50)
                    {
                        var findPoint = farmLoc;
                        if (questPoiPoints.Count > 0)
                            findPoint = questPoiPoints[Host.RandGenerator.Next(0, questPoiPoints.Count)];
                        Host.log("Не могу найти GameObject, подбегаю в центр зоны " + Host.Me.Distance(findPoint) + "    " + questPoiPoints.Count);
                        Host.CommonModule.MoveTo(findPoint, 20, 20);
                    }
                    /* if (host.FarmModule.bestProp == null && host.Me.HpPercents > 80)
                         badRadius++;
                     else
                         badRadius = 0;*/
                    /*   if (host.FarmModule.bestMob != null)
                           badRadius = 0;*/


                }

                Host.FarmModule.StopFarm();
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Host.log(" " + e);
            }
            return false;
        }

        public bool QuestTypeArea(Quest quest, int objectiveindex)
        {
            try
            {
                if (quest.Id == 14066)
                    if (!Host.CommonModule.MoveTo(-758.11, -3580.36, 93.67, 2, 2))
                        return false;
                    else
                    {
                        return false;
                    }

                if (quest.Id == 13564)
                    if (!Host.CommonModule.MoveTo(6538.76, 242.69, 7.36, 2, 2))
                        return false;
                    else
                    {
                        return false;
                    }

                if (quest.Id == 870)
                    if (!Host.CommonModule.MoveTo(90.22, -1943.69, 80.10, 2, 2))
                        return false;
                    else
                    {
                        return false;
                    }

                var farmLoc = new Vector3F();
                foreach (var questPoi in quest.GetQuestPOI())
                {
                    if (questPoi.ObjectiveIndex == objectiveindex)
                    {
                        double bestDist = 999999;
                        foreach (var questPoiPoint in questPoi.Points)
                        {
                            var z = Host.GetNavMeshHeight(new Vector3F(questPoiPoint.X, questPoiPoint.Y, 0));
                            if (Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z)) > bestDist)
                                continue;
                            bestDist = Host.Me.Distance(new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z));
                            farmLoc = new Vector3F(questPoiPoint.X, questPoiPoint.Y, (float)z);
                        }
                        break;
                    }
                }
                if (farmLoc.X == 0)
                {
                    Host.log("Координаты AreaTrigger не найдены ", Host.LogLvl.Error);
                    return false;
                }

                //Дойти по координатам
                if (!Host.CommonModule.MoveTo(farmLoc, 2, 2))
                    return false;
            }
            catch (Exception e)
            {
                Host.log(" " + e);

            }
            return false;
        }



    }
}