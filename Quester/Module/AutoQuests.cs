using WowAI.UI;
using Out.Internal.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Navigation;
using WoWBot.Core;
using Out.Utility;
using WowAI.ComboRoutes;

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
        private readonly List<int> _ignoreMonsterBookQuests = new List<int>();
        private int _checkId;
        private int _checkTry;
        private bool logQuest = true;
        public uint BestQuestId = 0;
        public MyQuestHelp MyQuestHelp = new MyQuestHelp();
        public override void Start(Host host)
        {
            ListQuest = new List<uint>();
            base.Start(host);
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Stop()
        {
            base.Stop();
        }

        public bool NeedDebugMove = false;
        public Vector3F NeedGebugMoveLoc = new Vector3F();
        public bool NeedActionNpcSell = false;
        public bool NeedActionNpcRepair = false;


        public bool StopQuestModule;
        public List<MultiZone> IgnoreMultiZones = new List<MultiZone>();
        public bool NeedChangeMultizone = true;
        public MultiZone bestMultizone = new MultiZone();
        public double bestDist = 9999999;

        public bool SavePointMove = false;
        public Vector3F PreViliousPoint = new Vector3F();

        public List<uint> IsMapidDungeon = new List<uint> { 600, 43 };

        public int StartLevel;

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

                    /*  if (StartLevel != Host.Me.Level)
                      {
                          Host.log("Обновляю скилы ", Host.LogLvl.Important);
                          Host.ComboRoute = new DefaultComboRoute(Host);
                      }*/

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



                    //  Host.CommonModule.LoadCurrentZoneMesh(Host.Me.Location.X, Host.Me.Location.Y);

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

                            if(Host.GetBotLogin() == "deathstar")
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


                        if (Host.CharacterSettings.CheckAuk)
                        {
                            var count = Host.MeGetItemsCount(Host.CharacterSettings.FreeInvCountForAukId);
                            if (count > Host.CharacterSettings.FreeInvCountForAuk)
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


                                Host.Auk();
                                Host.Mail();
                                Host.Auk();
                                Host.Mail();
                                continue;
                            }
                        }





                        //   if(host.CharacterSettings.Mode > )
                        //host.log("Выбран режим " + host.CharacterSettings.Mode);








                        switch (Host.CharacterSettings.Mode)
                        {
                            #region Квест

                            case EMode.Questing:// "Выполнение квестов": //Квест
                                {
                                    /* if (Host.Me.Race != ERace.Troll)
                                     {
                                         Host.log(" " + Host.Me.Race);
                                         Thread.Sleep(5000);
                                         continue;
                                     }*/
                                    if (Host.Me.Class != EClass.Druid && Host.Me.Class != EClass.Hunter && Host.Me.Class != EClass.Monk && Host.Me.Class != EClass.Shaman)
                                    {
                                        Host.log("Класс не поддверживается  " + Host.Me.Class, Host.LogLvl.Error);
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

                                        if (Host.Zone.Id == 9830 && Host.MapID == 1950 && Host.Area.Id == 9830)
                                        {
                                            Host.log("Необходимо выбраться с корабля");
                                            //  Host.ForceMoveTo(1304.85, 2453.96, 187.69);
                                            // Host.ForceMoveTo(1265.68, 2456.16, 187.73);
                                            Host.MyUseStone();
                                            Host.log("Необходим перезапуск после пролога ");
                                            Thread.Sleep(15000);
                                            Host.TerminateGameClient();
                                            Thread.Sleep(1000);
                                            continue;
                                        }

                                        var sw = new Stopwatch();
                                        if (Host.AdvancedLog)
                                            sw.Start();

                                        /* if (!CheckAvalibleQuest())
                                             continue;*/
                                        //  Thread.Sleep(500);
                                        ListQuest.Clear();
                                        uint nextQuestId = 0;
                                        foreach (var questCoordSetting in Host.QuestSettings.QuestCoordSettings)
                                        {
                                            if (!questCoordSetting.Run)
                                                continue;

                                            if (Host.QuestStates.QuestState.Contains(questCoordSetting.QuestId))
                                            {
                                                // Host.MainForm.CollectionQuestSettings[0].State = "Выполнен";
                                                continue;
                                            }

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

                                                    if (quest?.Counts[index] >= templateQuestObjective.Amount)
                                                        continue;

                                                    //  Host.log("Type: " + templateQuestObjective.Type + " Amount:" + templateQuestObjective.Amount + " ObjectID:" + templateQuestObjective.ObjectID);

                                                    objectiveindex = index;

                                                    break;
                                                }
                                                if (objectiveindex == -1)
                                                {
                                                    ListQuest.Add(questCoordSetting.QuestId);
                                                }
                                                else
                                                {
                                                    //  ListQuest.Add(questCoordSetting.QuestId);
                                                    ListQuest.Insert(0, questCoordSetting.QuestId);
                                                }
                                                //

                                                continue;
                                            }

                                            if (questTemplate.MinLevel == 1)
                                            {
                                                minlevel = questTemplate.MinLevel;
                                            }
                                            else
                                            {
                                                if (Host.Me.Race == ERace.Troll)
                                                {
                                                    minlevel = questTemplate.MinLevel + 1;
                                                    if (Host.Me.Level > 18)
                                                        minlevel = questTemplate.MinLevel;
                                                }


                                                if (Host.Me.Race == ERace.NightElf && Host.Me.Level > 3)
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

                                            if (Host.Me.Level < minlevel)
                                                continue;


                                            if (nextQuestId == questCoordSetting.QuestId)
                                            {
                                                nextQuestId = questTemplate.RewardNextQuest;
                                                continue;
                                            }

                                            Host.log(questTemplate.LogTitle + "[" + questCoordSetting.QuestId + "]    MinLevel:" + questTemplate.MinLevel + "/" + minlevel + "    RewardNextQuest:" + questTemplate.RewardNextQuest + "    QuestLevel:" + questTemplate.QuestLevel + " " + Host.Me.Distance(questCoordSetting.Loc), Host.LogLvl.Important);

                                            nextQuestId = questTemplate.RewardNextQuest;
                                            /*   var nextQuestAvalible = false;



                                               if (!nextQuestAvalible)
                                                   continue;*/

                                            ListQuest.Add(questCoordSetting.QuestId);
                                        }

                                        foreach (var quest in Host.GetQuests())
                                        {
                                            if (quest.Id == 13557)//13557 State:None LogTitle:Неожиданная удача 
                                                ListQuest.Insert(0, quest.Id);
                                            if (quest.Id == 26447)//26447 State:None LogTitle:Демонические планы 
                                                ListQuest.Add(quest.Id);
                                            if (quest.Id == 2)//2 State: None LogTitle:Коготь гиппогрифа
                                                ListQuest.Add(quest.Id);
                                            if (quest.Id == 1918) //1918 State:None LogTitle:Оскверненная стихия 
                                                ListQuest.Add(quest.Id);

                                        }

                                        /*  foreach (var quest in Host.GetQuests())
                                          {

                                              var objectiveindex = -1;

                                              for (var index = 0; index < (quest?.Template.QuestObjectives).Length; index++)
                                              {
                                                  var templateQuestObjective = (quest?.Template.QuestObjectives)[index];
                                                  if (quest?.Counts[index] >= templateQuestObjective.Amount)
                                                      continue;
                                                  Host.log("Type: " + templateQuestObjective.Type + " Amount:" + templateQuestObjective.Amount + " ObjectID:" + templateQuestObjective.ObjectID);

                                                  objectiveindex = index;

                                                  break;
                                              }
                                              if (objectiveindex == -1)
                                              {
                                                  ListQuest.Add(quest.Id);

                                                  continue;
                                              }


                                              ListQuest.Insert(0, quest.Id);

                                          }*/


                                        if (Host.AdvancedLog)
                                        {
                                            Host.log("Квесты добавлены за                               " + sw.ElapsedMilliseconds + " мс. Всего квестов " + ListQuest.Count);
                                            foreach (var i in ListQuest)
                                            {
                                                Host.log(i + " " + Host.GameDB.QuestTemplates[i].LogTitle);
                                            }
                                        }


                                        //поиск квестов в локации
                                        for (var i = ListQuest.Count - 1; i > -1; i--)
                                        {
                                            /*  switch (ListQuest[i])
                                              {

                                              }*/



                                            if (_ignoreQuest.Contains(ListQuest[i]))
                                            {
                                                ListQuest.Remove(ListQuest[i]);
                                                continue;
                                            }

                                            var ignoreQuest = false;
                                            foreach (var characterSettingsIgnoreQuest in Host.CharacterSettings.IgnoreQuests)
                                            {
                                                if (characterSettingsIgnoreQuest.Id == ListQuest[i])
                                                {
                                                    ListQuest.Remove(ListQuest[i]);
                                                    ignoreQuest = true;
                                                    break;
                                                }
                                            }
                                            if (ignoreQuest)
                                                continue;



                                        }


                                        /*   foreach (var quest in Host.QuestSettings.QuestCoordSettings)
                                           {
                                               if (quest.State != "Выполнен")
                                                   ListQuest.Add(quest.QuestId);
                                           }*/

                                        if (Host.AdvancedLog)
                                        {

                                            Host.log("Квесты обновлены за                               " + sw.ElapsedMilliseconds + " мс");
                                            Host.log("Квестов можно выполнить: " + ListQuest.Count);

                                        }

                                        if (ListQuest.Contains(50755) && Host.GetQuest(50755) != null)
                                            RunQuest(50755);
                                        if (ListQuest.Contains(51829) && Host.GetQuest(51829) != null)
                                            RunQuest(51829);



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
                                                if (quest.Id == 13998 && quest.State != EQuestState.Complete)
                                                {
                                                    objectiveindex = 30;
                                                }

                                                if (quest.Id == 13562 && quest.State != EQuestState.Complete)
                                                {
                                                    objectiveindex = 30;
                                                }
                                                for (var index = 0; index < (quest?.Template.QuestObjectives).Length; index++)
                                                {
                                                    var templateQuestObjective = (quest?.Template.QuestObjectives)[index];
                                                    if (templateQuestObjective.Type == EQuestRequirementType.AreaTrigger && quest.State == EQuestState.None)
                                                        objectiveindex = 0;

                                                    if (quest?.Counts[index] >= templateQuestObjective.Amount)
                                                        continue;
                                                    // Host.log("Type: " + templateQuestObjective.Type + " Amount:" + templateQuestObjective.Amount + " ObjectID:" + templateQuestObjective.ObjectID);

                                                    objectiveindex = index;

                                                    break;
                                                }
                                                // Thread.Sleep(500);
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
                                                    //Thread.Sleep(5000);
                                                    foreach (var questPois in quest.GetQuestPOI())
                                                    {
                                                        if (questPois.ObjectiveIndex != -1)
                                                            continue;
                                                        questPoi = questPois;
                                                        break;
                                                    }
                                                }
                                                if (objectiveindex == -1 && quest.Id == 28726)
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
                                                }
                                                if (objectiveindex == -1 && questPoi != null && Host.DistanceNoZ(Host.Me.Location.X, Host.Me.Location.Y, questPoi.Points[0].X, questPoi.Points[0].Y) < 90)
                                                {
                                                    Host.log("Сдаю квест поблизости " + objectiveindex + "  " + quest.Id, Host.LogLvl.Ok);
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
                                                        Host.log("Беру квест поблизости " + questSet.QuestId, Host.LogLvl.Ok);
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
                                                        Host.log("Беру квест поблизости " + questSet.QuestId, Host.LogLvl.Ok);
                                                        BestQuestId = questSet.QuestId;
                                                        MyApplyQuest(npc, questSet.QuestId); // взял квест
                                                        Thread.Sleep(1000);
                                                        isNeedApply = true;
                                                        break;
                                                    }
                                                }

                                                //  }
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
                                            if (!RunQuest(quest))
                                                break;
                                        }


                                        if (ListQuest.Count == 0)
                                        {
                                            Host.log("Нет квестов");

                                            if (Host.Me.Level > 100)
                                            {
                                                Host.log("Отключаюсь");
                                                Host.MainForm.On = false;
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

                            #endregion

                            #region Фарм 

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

                            #endregion

                            #region Сбор

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

                            #endregion

                            #region Данж.(п)

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
                                        // if (host.WorldMapType != EWorldMapType.Dungeon)
                                        // {
                                        if (NeedActionNpcSell || NeedActionNpcRepair)
                                            break;
                                        if (Host.CharacterSettings.CheckAuk)
                                        {
                                            var count = Host.MeGetItemsCount(Host.CharacterSettings.FreeInvCountForAukId);
                                            if (count > Host.CharacterSettings.FreeInvCountForAuk)
                                            {
                                                break;
                                            }
                                        }

                                        if (IsMapidDungeon.Contains(Host.MapID) && Host.CharacterSettings.CheckRepairAndSell && Host.CharacterSettings.MountLocMapId != 0 && Host.CharacterSettings.MountLocMapId == Host.MapID)
                                            if (Host.ItemManager.GetFreeInventorySlotsCount() <= Host.CharacterSettings.InvFreeSlotCount || Host.MyIsNeedRepair())
                                            {
                                                // Host.CommonModule.LoadCurrentZoneMesh(Host.Me.Location.X, Host.Me.Location.Y);
                                                break;
                                            }


                                        Mode_86();
                                        // }

                                    }
                                }
                                break;

                            #endregion

                            case EMode.OnlyAttack:
                                {
                                    while (Host.GameState == EGameState.Ingame && Host.CharacterSettings.Mode == EMode.OnlyAttack)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                                break;


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


        /*  public bool CheckAvalibleQuest()
          {
              foreach (var entity in Host.GetEntities<Unit>())
              {
                  if (!entity.IsQuestGiver)
                      continue;
                  if (entity.QuestGiverStatus == EQuestGiverStatus.Available)
                  {
                      MyApplyQuest(entity);
                      return false;
                  }
              }

              return true;
          }*/

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




        public bool WaitTeleport = false;

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

        public bool NeedFindBestPoint = false;


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



                    AddMech(core);

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


        public void AddMech(Core core)
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

                foreach (var dungeonSettingsScriptCoordSetting in Host.DungeonSettings.DungeonCoordSettings)
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
                    bestpoint = Host.DungeonSettings.DungeonCoordSettings[0];
                    bestPoint = 999999;
                }
                if (checkPoint != null && checkPoint == bestpoint)
                {
                    bestpoint = Host.DungeonSettings.DungeonCoordSettings[0];
                    bestPoint = 999999;
                }

                if (Host.CharacterSettings.LogScriptAction)
                    Host.log("Начал выполение скрипта " + Host.Me.Name + "  " + scriptStopwatch.ElapsedMilliseconds, Host.LogLvl.Ok);

                for (var index = 0; index < Host.DungeonSettings.DungeonCoordSettings.Count; index++)
                {

                    while (Host.GameState != EGameState.Ingame)
                        Thread.Sleep(100);
                    while (Host.CommonModule.IsMoveSuspended())
                        Thread.Sleep(100);
                    if (!Host.MainForm.On)
                        break;
                    if (!Host.IsAlive(Host.Me))
                        break;

                    if (NeedFindBestPoint)
                    {
                        Host.log("Ищу новую ближайшую точку");
                        foreach (var dungeonSettingsScriptCoordSetting in Host.DungeonSettings.DungeonCoordSettings)
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


                    var dungeon = Host.DungeonSettings.DungeonCoordSettings[index];
                    if (bestPoint != 999999 && bestpoint != dungeon)
                    {
                        continue;
                    }
                    else
                    {
                        bestPoint = 999999;
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


                                    while (Host.Me.IsMoving)
                                        Thread.Sleep(50);
                                    while (Host.SpellManager.IsCasting)
                                        Thread.Sleep(50);
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
                                    while (Host.Me.IsMoving)
                                    {
                                        Thread.Sleep(50);
                                        //  Host.log("Использовал скилл из скрипта IsMoving " + skill.Name, Host.LogLvl.Ok);
                                    }

                                    while (Host.SpellManager.IsCasting)
                                    {
                                        Thread.Sleep(50);
                                        // Host.log("Использовал скилл из скрипта IsCasting  " + skill.Name, Host.LogLvl.Ok);
                                    }

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
                                                    while (Host.Me.IsMoving)
                                                        Thread.Sleep(50);
                                                    while (Host.SpellManager.IsCasting)
                                                        Thread.Sleep(50);
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


                                        var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(-9005.38, 870.93, 29.62), Host.Me.Location);
                                        Host.log(path.Count + "  Путь");
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F);
                                        }

                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 287164)
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

                                    if (dungeon.MapId == 1642 && Host.MapID == 1)
                                    {//

                                        var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1792.34, -4333.80, -10.76), Host.Me.Location);
                                        Host.log(path.Count + "  Путь");
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F);
                                        }



                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 293551)
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

                                    return;
                                }

                                if (dungeon.AreaId != Host.Area.Id || Host.Me.Distance(dungeon.Loc) > 600)
                                {
                                    Host.log("Точка в другой зоне ", Host.LogLvl.Error);
                                    Host.MyUseTaxi(dungeon.AreaId, dungeon.Loc);

                                    return;
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
                                while (Host.Me.Distance(new Vector3F(dungeon.Loc.X, dungeon.Loc.Y, dungeon.Loc.Z)) > 3 && Host.IsAlive(Host.Me) && Host.MainForm.On && !Host.Me.IsDeadGhost)
                                {
                                    //  Host.log("Бегу на точку  2     " + dungeon.Loc + "  " + "  " + dungeon.Attack + " дист:" + Host.Me.Distance(dungeon.Loc));
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
                                            //  Host.log("Вошел в ForceComeTo");
                                            if (!Host.CommonModule.ForceMoveTo2(dungeon.Loc, 1, Host.Me.RunSpeed / 5.0))
                                            {

                                                /* if (Host.GetLastError() != ELastError.Movement_MoveCanceled)
                                                     Host.log("Не смог добежать до точки " + Host.GetLastError() + "  " + Host.Me.Distance(dungeon.Loc), Host.LogLvl.Error);*/
                                            }
                                            else
                                            {
                                                Host.CommonModule._moveFailCount = 0;
                                                break;
                                            }
                                            // Host.log("Вышел из ForceComeTo");
                                            if (Host.Me.Distance(dungeon.Loc) > 4 && Host.MainForm.On && !Host.CommonModule.IsMoveSuspended())
                                            {
                                                //      host.log("test");
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
                                        if (Host.CommonModule.MoveTo(dungeon.Loc))
                                            break;
                                    }


                                    if (Host.CommonModule._moveFailCount > 1)
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
                Host.log("Тест " + scriptStopwatch.Elapsed.Minutes + " " + scriptStopwatch.Elapsed.Seconds);
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

        private bool MyApplyQuest(Entity npc, uint id)
        {

            switch (npc.Id)
            {
                case 122688:
                    {
                        if (!Host.CommonModule.MoveTo(2666.69, 1364.10, 11.17))
                            return false;
                    }
                    break;


                default:
                    {
                        if (!Host.CommonModule.MoveTo(npc, 2, 2))
                            return false;
                    }
                    break;
            }



            if (npc.Guid == Host.CurrentInteractionGuid)
            {
                Host.log("Диалог уже открыт ", Host.LogLvl.Ok);
            }
            else
            {
                Thread.Sleep(500);
                if (!Host.OpenDialog(npc))
                {
                    Host.log("Не смог начать диалог для начала квеста с " + npc.Name + " " + Host.GetLastError() + " CurrentInteractionGuid:" + Host.CurrentInteractionGuid + " IsAlive:" + Host.Me.IsAlive, Host.LogLvl.Error);
                    Host.log(npc.Guid + "   npc.Guid");
                    Host.log(Host.CurrentInteractionGuid + " CurrentInteractionGuid");

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
                    Host.log("Нашел квест " + id);
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
                //  id = gossipQuestTextData.QuestID;
                //break;
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
                default:
                    {
                        if (!isQuestFound)
                        {
                            Host.log("Не нашел квест у НПС " + id + " " + isQuestFound, Host.LogLvl.Error);
                            Host.SendKeyPress(0x1b);
                            //MyMoveFromNpc(npc as Unit);
                            Thread.Sleep(1000);
                            return false;
                        }

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


            if (id == 52131)
                Host.Wait(60000);
            Thread.Sleep(500);
            return true;
        }

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
            var id = quest.Id;
            QuestPOI questPoi = null;
            foreach (var questPois in quest.GetQuestPOI())
            {
                if (questPois.ObjectiveIndex != -1)
                    continue;
                if (quest.Id == 6342 && questPois.WorldMapAreaId == 42)
                    continue;
                questPoi = questPois;
                break;
            }

            /*  if(id == 0)
                  Host.MyUseTaxi(0,)*/
            if (id == 28725)
                questPoi = null;

            var revardNpcId = quest.CompletionNpcIds[0];

            if (quest.Id == 13521)
                revardNpcId = 194105;

            if (quest.Id == 13528)
                revardNpcId = 194122;

            if (quest.Id == 29021)
                revardNpcId = 4141;

            if (quest.Id == 47264)
                revardNpcId = 121241;
            if (quest.Id == 47130)
                revardNpcId = 121241;



            if (quest.Id == 50550)
            {
                revardNpcId = 138519;
                Host.Wait(20000);
            }


            //130474
            var revardNpc = MyGetLocNpcById(revardNpcId);

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




            var npc = Host.GetNpcById(revardNpcId);
            if (questPoi != null && npc == null)
            {
                var z = Host.GetNavMeshHeight(new Vector3F(questPoi.Points[0].X, questPoi.Points[0].Y, 0));
                if (z == 0)
                    Host.log("Z координата  = 0");
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
                    case 120740:
                        {
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
                                    if (!Host.CommonModule.MoveTo(-1126.50, 851.96, 443.32))
                                        return false;
                                    while (Host.Me.Location.Z < 485)
                                    {
                                        Thread.Sleep(2000);
                                        Host.Jump();
                                    }
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
                        }
                        break;
                    default:
                        {
                            if (Host.Me.Distance2D(locRevard) > 20)
                                if (!Host.CommonModule.MoveTo(locRevard, 20, 20))
                                    return false;
                        }
                        break;
                }


            }

            if (quest.Id == 51443)
                Host.Wait(10000);
            if (quest.Id == 49492)
                Host.Wait(20000);


            if (npc != null)
            {
                switch (npc.Id)
                {
                    case 120740:
                        {
                            if (quest.Id == 46930)
                            {
                                if (!Host.CommonModule.MoveTo(npc, 3, 3))
                                    return false;
                            }
                            else
                            {
                                if (Host.Me.Location.Z < 470)
                                {
                                    if (!Host.CommonModule.MoveTo(-1126.50, 851.96, 443.32))
                                        return false;
                                    while (Host.Me.Location.Z < 485)
                                    {
                                        Thread.Sleep(2000);
                                        Host.Jump();
                                    }
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

                    Host.log("Не смог начать диалог для завершения квеста с " + npc.Name + " " + revardNpc.Id + "  " + Host.GetLastError(), Host.LogLvl.Error);
                    Host.log(npc.Guid + "   npc.Guid");
                    Host.log(Host.CurrentInteractionGuid + " CurrentInteractionGuid");

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
                }

                uint revard = 0;

                if (Host.Me.Class == EClass.Druid)
                {

                    if (MyQuestHelp.QuestRevardDruid.ContainsKey(quest.Id))
                    {
                        revard = MyQuestHelp.QuestRevardDruid[quest.Id];
                    }
                    /* if (quest.Id == 835)
                         revard = 4938;
                     if (quest.Id == 872)
                         revard = 59543;
                     if (quest.Id == 850)
                         revard = 59552;
                     if (quest.Id == 13998)
                         revard = 59545;
                     if (quest.Id == 28726)
                         revard = 5393;
                     if (quest.Id == 2159)
                         revard = 2070;
                     if (quest.Id == 28727)
                         revard = 5405;
                     if (quest.Id == 13520)
                         revard = 52609;
                     if (quest.Id == 13554)
                         revard = 52631;
                     if (quest.Id == 13529)
                         revard = 52595;
                     if (quest.Id == 13563)
                         revard = 52599;
                     if (quest.Id == 25192)
                         revard = 53382;
                     if (quest.Id == 895)
                         revard = 49446;
                     if (quest.Id == 865)
                         revard = 59581;
                     if (quest.Id == 851)
                         revard = 59567;
                     if (quest.Id == 855)
                         revard = 59583;                
                     if (quest.Id == 852)
                         revard = 59585;
                     if (quest.Id == 899)
                         revard = 59540;
                     if (quest.Id == 13621)
                         revard = 56644;*/

                }

                if (Host.Me.Class == EClass.Hunter)
                {
                    if (quest.Id == 52428)
                        revard = 159907;
                    if (quest.Id == 53372)
                        revard = 163524;
                }

                if (Host.Me.Class == EClass.Monk)
                {
                    if (quest.Id == 52428)
                        revard = 159906;
                    if (quest.Id == 53372)
                        revard = 163528;
                }

                if (Host.Me.Class == EClass.Shaman)
                {
                    if (quest.Id == 52428)
                        revard = 159907;
                    if (quest.Id == 53372)
                        revard = 163529;
                }


                foreach (var templateRewardItem in quest.Template.RewardItems)
                {
                    Host.log("Награда " + templateRewardItem, Host.LogLvl.Error);
                }

                Thread.Sleep(500);
                if (!Host.CompleteQuest(id, revard))
                {
                    Host.log("Не смог завершить квест " + id + " с выбором награды " + revard + "  " + Host.GetLastError(), Host.LogLvl.Error);
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
                    Host.QuestStates.QuestState.Add(id);
                    /*  var isNewQuest = true;
                      foreach (var questState in Host.QuestStates.QuestState)
                      {
                          if (questState == id)
                          {
                            //  questState.State = "Выполнен";
                              isNewQuest = false;
                          }
                      }
                      if(isNewQuest)
                          Host.QuestStates.QuestState.Add(new QuestState
                          {
                              QuestId = id,
                              State = "Выполнен"
                          });*/
                    Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                }
                Thread.Sleep(500);
                return false;
            }
            else
            {
                Host.log("Не нашел завершающего НПС по указанным координатам " + revardNpcId, Host.LogLvl.Error);

                if (quest.Id == 49491)
                    revardNpcId = 278452;

                if (quest.Id == 50539 || quest.Id == 48315)
                    revardNpcId = 281639;

                if (quest.Id == 50539 || quest.Id == 48315 || quest.Id == 49491)
                {
                    foreach (var gameObject in Host.GetEntities<GameObject>())
                    {
                        if (gameObject.Id != revardNpcId)
                            continue;
                        npc = gameObject;
                    }

                    Host.CommonModule.MoveTo(npc, 3);
                    while (Host.Me.IsMoving)
                    {
                        if (!Host.MainForm.On)
                            return false;
                        Thread.Sleep(100);
                    }
                    if (Host.OpenDialog(npc))
                    {
                        if (!Host.CompleteQuest(quest.Id, 0))
                        {
                            Host.log("Не смог завершить квест " + Host.GetLastError(), Host.LogLvl.Error);
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
                        Host.log("Не смог открыть диалог [" + npc.Id + "] " + npc.Name + "  " + Host.GetLastError(), Host.LogLvl.Error);
                    }

                }


            }
            return true;
        }



        /// <summary>
        /// Выполнение квеста
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
                foreach (var questCount in quest.Counts)
                {
                    step = step + questCount;
                }
                Host.log("Шаг квеста " + step);
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

                if (quest.Id == 13998 && quest.State != EQuestState.Complete)
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

                if (quest.Id == 51796 && quest.State != EQuestState.Complete) //Битва за ЛОрдеон
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
                        while (Host.Me.IsMoving)
                            Thread.Sleep(100);
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
                        if (Host.Scenario.CurrentStep == 3688)
                        {
                            if (Host.Me.IsAlive && !Host.Me.IsDeadGhost)
                            {
                                Host.CommonModule.MoveTo(1711.67, 239.60, 62.60);
                                Host.CommonModule.MoveTo(1693.04, 238.98, 62.60);

                                while (Host.GameState == EGameState.Ingame)
                                {
                                    Thread.Sleep(1000);
                                }
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

                if (quest.Id == 53028 && quest.State != EQuestState.Complete)//Умирающий мир[53028] 
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

                if (quest.Id == 51211 && quest.State != EQuestState.Complete)//Сердце Азерот[51211] 
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


                        var count = 0;
                        foreach (var questCount in quest.Counts)
                        {
                            if (questCount == 1)
                                count++;
                        }

                        if (count == 3)
                        {
                            MyComliteQuest(quest);
                            return false;
                        }

                        var npc = Host.GetNpcById(136907);
                        if (npc != null)
                        {
                            Host.MyDialog(npc, "What does Azeroth want of me");
                            Host.MyDialog(npc, "Что Азерот хочет от меня");

                            Host.Wait(45000);
                        }

                        /* foreach (var entity in Host.GetEntities<GameObject>())
                         {
                             if (entity.Id == 293847)
                             {
                                 Host.CommonModule.MoveTo(entity);
                                 entity.Use();
                                 Thread.Sleep(2000);
                                 while (Host.GameState != EGameState.Ingame)
                                 {
                                     Thread.Sleep(1000);
                                 }
                             }
                         }*/
                    }

                    //   Thread.Sleep(5000);
                    return false;
                }

                if (quest.Id == 52428 && quest.State != EQuestState.Complete) //Усиление Сердца[52428] 
                {
                    if (Host.MapID == 1929)
                    {
                        step = 0;
                        foreach (var questCount in quest.Counts)
                        {
                            step = step + questCount;
                        }

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
                            //  Host.Wait(60000);
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
                            Host.ForceComeTo(gameObject, 3);
                            Thread.Sleep(1000);
                            Host.SpellManager.CastSpell(275825);
                            Thread.Sleep(5000);
                        }
                    }
                    return false;
                }

                if (quest.Id == 53031 && quest.State != EQuestState.Complete) //Воля Вестника[53031]   
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
                        MyComliteQuest(quest);

                    return false;
                }

                if (quest.Id == 51443 && quest.State != EQuestState.Complete) //Поставленная задача[51443]    
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
                            Host.Wait(50000);
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

                if (quest.Id == 50769 && quest.State != EQuestState.Complete) // Побег из Штормграда[50769] 
                {

                    step = 0;
                    foreach (var questCount in quest.Counts)
                    {
                        step = step + questCount;
                    }


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
                                /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                                 if (!m.PickUp())
                                 {*/

                                Host.log("Не смог поднять дроп " + "   " + Host.GetLastError(), Host.LogLvl.Error);

                                //   }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            Host.log("Окно лута не открыто " + Host.GetLastError(), Host.LogLvl.Error);

                        }
                    }

                    if (step == 2)
                    {


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
                                Host.ForceComeTo(gameObject, 3);
                                Thread.Sleep(1000);
                                if (!gameObject.Use())
                                    Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(1000);
                            }

                            foreach (var gameObject in Host.GetEntities<GameObject>())
                            {
                                if (gameObject.Id != 281483)
                                    continue;
                                Host.ForceComeTo(gameObject, 1);
                                Thread.Sleep(1000);
                                /*  if (!gameObject.Use())
                                      Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);*/
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

                            if (!Host.CommonModule.MoveTo(-8673.12, 918.13, 53.73))
                                return false;
                            var npc = Host.GetNpcById(134120);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, "What do");
                                Host.MyDialog(npc, "Что ты имеешь в виду");

                                while (Host.Scenario.CurrentStep == 3721)
                                {
                                    Thread.Sleep(1000);
                                }

                            }


                        }

                        if (Host.Scenario.CurrentStep == 3722)
                        {
                            if (Host.Me.Distance(-8673.31, 917.38, 53.73) < 20)
                            {
                                Host.ForceMoveTo(-8690.47, 905.47, 53.73);

                            }
                            var convoy = Host.GetNpcById(134037);
                            var isuse = false;
                            while (convoy != null)
                            {
                                if (!Host.MainForm.On)
                                    return false;
                                if (Host.Scenario.CurrentStep != 3722)
                                    return false;
                                Thread.Sleep(1000);
                                convoy = Host.GetNpcById(134037);
                                if (Host.GetThreats(convoy as Unit).Count > 0 && Host.FarmModule.BestMob == null)
                                {
                                    foreach (var threatItem in Host.GetThreats(convoy as Unit))
                                    {
                                        Host.FarmModule.BestMob = threatItem.Obj;
                                        break;
                                    }
                                }
                                if (Host.Me.Distance(convoy) > 2 && Host.FarmModule.BestMob == null)
                                    Host.CommonModule.ForceMoveTo2(convoy.Location, 2);
                                if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null)
                                    Host.FarmModule.BestMob = (convoy as Unit).Target;
                                Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + Host.GetThreats(convoy as Unit).Count);

                                if (Host.Me.Distance(-8733.11, 871.63, 53.73) < 10 && !isuse && Host.FarmModule.BestMob == null)
                                {
                                    Host.ForceMoveTo(-8742.83, 884.26, 52.82);
                                }

                                if (Host.Me.Distance(-8651.85, 809.60, 44.15) < 10)
                                {
                                    Host.ForceMoveTo(-8648.94, 795.96, 44.15);
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

                            if (Host.Me.Distance(-8673.31, 917.38, 53.73) < 20)
                            {
                                Host.ForceMoveTo(-8690.47, 905.47, 53.73);

                            }

                            if (!Host.CommonModule.MoveTo(-8747.72, 889.91, 52.82))
                                return false;
                            foreach (var gameObject in Host.GetEntities<GameObject>())
                            {
                                if (gameObject.Id != 287550)
                                    continue;
                                Host.ForceComeTo(gameObject, 3);
                                Thread.Sleep(1000);
                                if (!gameObject.Use())
                                    Host.log("Не смог использовать " + Host.GetLastError(), Host.LogLvl.Error);
                                Thread.Sleep(5000);
                            }

                            if (Host.MyGetAura(264422) != null)
                            {
                                if (Host.Me.Distance(-8747.72, 889.91, 52.82) < 20)
                                {
                                    //   Host.ForceMoveTo();
                                    if (!Host.CommonModule.MoveTo(-8698.15, 898.79, 53.73))
                                        return false;
                                }



                            }

                            /*  if (Host.Me.Distance(-8698.15, 898.79, 53.73) < 20)
                              {
                                  if (!Host.CommonModule.MoveTo(-8698.15, 898.79, 53.73))
                                      return false;
                              }*/
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
                                /* if (Host.GetThreats(convoy as Unit).Count > 0 && Host.FarmModule.BestMob == null)
                                 {
                                     foreach (var threatItem in Host.GetThreats(convoy as Unit))
                                     {
                                         Host.FarmModule.BestMob = threatItem.Obj;
                                         break;
                                     }
                                 }*/
                                if (Host.Me.Distance(convoy) > 1.5 && Host.FarmModule.BestMob == null)
                                    Host.ComeTo(convoy.Location);

                                if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null)
                                {
                                    if (Host.Scenario.CurrentStep == 3727)
                                    {

                                    }
                                    else
                                    {
                                        Host.FarmModule.BestMob = (convoy as Unit).Target;
                                    }

                                }

                                Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + Host.GetThreats(convoy as Unit).Count);

                                if (Host.Me.Distance(-8328.80, 1364.90, 12.94) < 10)
                                {
                                    Thread.Sleep(10000);
                                    var npc = Host.GetNpcById(134092);
                                    if (npc != null)
                                    {
                                        Host.MyDialog(npc, "Not");
                                        Host.MyDialog(npc, "нет");

                                        while (Host.GameState == EGameState.Ingame)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        Thread.Sleep(10000);
                                        while (Host.GameState != EGameState.Ingame)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        Host.Wait(30000);
                                        return false;
                                        //Host.Wait(260000);
                                    }
                                }





                            }
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
                                Thread.Sleep(100);
                                if (Host.Scenario.CurrentStep == 3727)
                                {
                                    Host.CanselForm();
                                    Host.FarmModule.farmState = FarmState.Disabled;
                                    /* if (Host.Me.Class == EClass.Hunter)
                                     {
                                         var needUnsummon = false;                                      
                                         foreach (var entity in Host.GetEntities<Unit>())
                                         {
                                             if (entity.Owner == Host.Me)
                                                 needUnsummon = true;


                                         }

                                         if (needUnsummon)
                                         {
                                             var pet = Host.SpellManager.CastSpell(2641);
                                             if (pet == ESpellCastError.SUCCESS)
                                             {
                                                 Host.log("Отозвал питомца", Host.LogLvl.Ok);
                                             }
                                             else
                                             {
                                                 Host.log("Не удалось отозвать питомца " + pet, Host.LogLvl.Error);
                                             }
                                             while (Host.SpellManager.IsCasting)
                                             {
                                                 Thread.Sleep(100);
                                             }
                                         }
                                     }*/
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
                                if (Host.Me.Distance(convoy) > 1.5 && Host.FarmModule.BestMob == null)
                                    Host.ForceComeTo(convoy.Location);

                                if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null)
                                {
                                    if (Host.Scenario.CurrentStep == 3727)
                                    {

                                    }
                                    else
                                    {
                                        Host.FarmModule.BestMob = (convoy as Unit).Target;
                                    }

                                }

                                Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + Host.GetThreats(convoy as Unit).Count);

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
                                    Host.Wait(30000);
                                }

                                if (Host.Me.Distance(-8492.33, 930.98, 97.34) < 10 && !dialog2)
                                {
                                    Thread.Sleep(20000);
                                    var npc = Host.GetNpcById(134039);
                                    if (npc != null)
                                    {
                                        if (Host.MyDialog(npc, "ready"))
                                            dialog2 = true;
                                        if (Host.MyDialog(npc, "готов"))
                                            dialog2 = true;
                                    }
                                }

                                if (Host.Me.Distance(-8536.69, 484.44, 101.62) < 10 && !dialog1)
                                {
                                    var npc = Host.GetNpcById(134038);
                                    if (npc != null)
                                    {
                                        Thread.Sleep(1000);
                                        if (Host.MyDialog(npc, "Let"))
                                            dialog1 = true;
                                        if (Host.MyDialog(npc, "Идем"))
                                            dialog1 = true;
                                    }
                                }

                                if (Host.Me.Distance(-8500.13, 684.76, 100.82) < 10 && !dialog3)
                                {
                                    Thread.Sleep(10000);
                                    var npc = Host.GetNpcById(134038);
                                    if (npc != null)
                                    {
                                        if (Host.MyDialog(npc, "will"))
                                            dialog3 = true;
                                        if (Host.MyDialog(npc, "идти дальше"))
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
                        if (Host.GetThreats(convoy as Unit).Count > 0 && Host.FarmModule.BestMob == null)
                        {
                            foreach (var threatItem in Host.GetThreats(convoy as Unit))
                            {
                                Host.FarmModule.BestMob = threatItem.Obj;
                                break;
                            }
                        }
                        if (Host.Me.Distance(convoy) > 3 && Host.FarmModule.BestMob == null)
                            Host.ForceComeTo(convoy.Location, 2);
                        if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null)
                            Host.FarmModule.BestMob = (convoy as Unit).Target;
                        Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + Host.GetThreats(convoy as Unit).Count);

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
                            Host.MyDialog(npc, "Take");
                            Host.MyDialog(npc, "Отведи");
                            Host.Wait(90000);
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
                        if (Host.GetThreats(convoy as Unit).Count > 0 && Host.FarmModule.BestMob == null)
                        {
                            foreach (var threatItem in Host.GetThreats(convoy as Unit))
                            {
                                Host.FarmModule.BestMob = threatItem.Obj;
                                break;
                            }
                        }
                        if (Host.Me.Distance(convoy) > 3 && Host.FarmModule.BestMob == null)
                            Host.ForceComeTo(convoy.Location, 2);
                        if ((convoy as Unit).Target != null && Host.FarmModule.BestMob == null)
                            Host.FarmModule.BestMob = (convoy as Unit).Target;
                        Host.log("Конвой " + convoy.Name + " " + (convoy as Unit).Target?.Name + "  " + Host.GetThreats(convoy as Unit).Count);

                    }
                    return false;
                }

                if (quest.Id == 47313 && quest.State != EQuestState.Complete) //Приватный разговор[47313] 
                {

                    step = 0;
                    foreach (var questCount in quest.Counts)
                    {
                        step = step + questCount;
                    }

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
                    {
                        if (!Host.CommonModule.MoveTo(-863.96, 756.71, 339.80))
                            return false;

                    }

                    if (step == 7)
                    {
                        MyComliteQuest(quest);
                    }

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
                        MyComliteQuest(quest);
                    return false;
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
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1069.91, 3079.94, 81.39))
                            return false;
                        Host.MyUseGameObject(291008);
                    }
                    if (step == 4)
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

                    return false;
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
                        var npc = Host.GetNpcById(129519);
                        Host.MyDialog(npc, 1);
                        Host.Wait(140000);
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
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(2560.73, 3472.73, 200.68))
                        {
                            return false;
                        }

                        var npc = Host.GetNpcById(129763);
                        if (npc != null)
                            Host.MyDialog(npc, 0);
                        return false;
                    }

                    if (step > 0 && step < 61)
                    {
                        var list = Host.GetEntities();
                        Entity npc = null;
                        foreach (var entity in list.OrderBy(i => Host.Me.Distance(i)))
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
                        Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        if (Host.FarmModule.BestMob == null)
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (entity.Id == 134601 && entity.IsAlive)
                                    Host.FarmModule.BestMob = entity;
                            }
                    }

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
                        var npc = Host.GetNpcById(121288);
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
                    if (step > 15)
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
                    if (step == 0)
                    {
                        if (!Host.CommonModule.MoveTo(1794.65, 1734.44, 21.35))
                            return false;
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (step == 1)
                    {
                        if (!Host.CommonModule.MoveTo(1801.65, 1680.08, 14.80))
                            return false;
                        Thread.Sleep(5000);
                        return false;
                    }

                    if (step == 2)
                    {
                        if (!Host.CommonModule.MoveTo(1946.33, 1821.46, 20.18))
                            return false;
                        Thread.Sleep(5000);
                        return false;
                    }
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

                    if (step == 1)
                    {
                        var item = Host.MyGetItem(153694);
                        var npc = Host.GetNpcById(129395);
                        if (item != null && npc != null)
                            Host.SendKeyPress(0x31);
                        Host.Wait(5000);
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
                        if (Host.CommonModule.MoveTo(-410.81, 1210.07, 320.84))
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

                        case 47262:
                            {
                                if (quest?.Counts[index + 5] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;
                        case 49494:
                        case 50702:
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

                        case 49801:
                            {
                                if (quest?.Counts[index + 3] >= templateQuestObjective.Amount)
                                    continue;
                            }
                            break;

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
                    Host.MainForm.SetQuestStateText(curentObjectiveType.ToString() + "   " + QuestType);
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
                        if (Host.SpellManager.UseSpellClick(entity as Unit))
                        {
                            Host.log("Использовал SpellClick ", Host.LogLvl.Ok);
                            while (Host.Me.IsMoving)
                                Thread.Sleep(50);
                            while (Host.SpellManager.IsCasting)
                                Thread.Sleep(50);
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
                Thread.Sleep(5000);
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
                        Host.CommonModule.MoveTo(unit, 2, 2);
                        if (Host.Me.Distance(unit) > 6)
                            return false;
                        while (Host.Me.IsMoving)
                            Thread.Sleep(50);
                        if (Host.SpellManager.UseSpellClick(unit as Unit))
                        {
                            Host.log("Использовал SpellClick ", Host.LogLvl.Ok);
                            while (Host.Me.IsMoving)
                                Thread.Sleep(50);
                            while (Host.SpellManager.IsCasting)
                                Thread.Sleep(50);
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
            if (id == 50641 || id == 50748 || id == 51663 || id == 50154)
                curCount = quest.Counts[index + 1];

            if (id == 48573)
            {
                curCount = quest.Counts[index];
                var count = 0;
                foreach (var questCount in quest.Counts)
                {
                    count = count + questCount;
                }

                if (count > 15)
                    return true;
            }

            if (id == 47247)
            {
                if (index == 1)
                {
                    curCount = quest.Counts[index + 1];
                }
            }

            if (id == 47244)
            {
                curCount = quest.Counts[index + 4];
            }
            if (id == 49327 || id == 49801)
            {
                curCount = quest.Counts[index + 3];
            }
            if (id == 50702 || id == 49494)
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

                if (quest.Id == 46926)
                {
                    farmMobIds.Add(125460);
                    farmMobIds.Add(125458);
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
                                    return false;
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


                // Host.log("Можно ли атаковать НПС " + isCanAttack + " " + findNpc);

                foreach (var farmMobId in farmMobIds)
                {
                    Host.log("НПС для квеста " + farmMobId);
                }


                if (!isCanAttack && findNpc)
                {
                    Host.log("Указанного НПС нельзя атаковать " + questObjective.ObjectID);



                    if (quest.Id == 47320)
                    {
                        if (!Host.CommonModule.MoveTo(2043.82, 2818.25, 50.42))
                            return false;
                        var item = Host.MyGetItem(150759);
                        var npc = Host.GetNpcById(122741);
                        Host.MyUseItemAndWait(item, npc);
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
                                while (Host.Me.IsMoving)
                                    Thread.Sleep(50);
                                while (Host.SpellManager.IsCasting)
                                    Thread.Sleep(50);
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


                if (farmMobIds.Count > 0 && quest.Template.StartItem == 0)
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
                var isGameObject = false;
                var questPoiPoints = new List<Vector3F>();

                if (quest.Id == 25232)
                {
                    mobId = 3199;
                    addMobId = 3196;
                }

                if (quest.Id == 25176)
                    mobId = 202648;

                if (quest.Id == 25178)
                    mobId = 3236;

                if (quest.Id == 834)
                {
                    isGameObject = true;
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
                    isGameObject = true;
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
                    isGameObject = true;
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


                //Альянс


                if (quest.Id == 28724)//Косяк в базе
                {
                    mobId = 207346;
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
                    isGameObject = true;
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
                    isGameObject = true;
                }
                if (quest.Id == 483 && objectiveindex == 1)
                {
                    mobId = 2739;
                    isGameObject = true;
                }
                if (quest.Id == 483 && objectiveindex == 2)
                {
                    mobId = 2741;
                    isGameObject = true;
                }
                if (quest.Id == 483 && objectiveindex == 3)
                {
                    mobId = 2742;
                    isGameObject = true;
                }

                if (quest.Id == 13521)
                {
                    mobId = 32935;
                }

                if (quest.Id == 851)
                {
                    mobId = 34846;
                }

                if (quest.Id == 13520)
                {
                    mobId = 194107;
                    isGameObject = true;
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

                uint spellid = 0;
                var isSpellOnUnit = false;
                if (quest.Id == 13527)
                {
                    isSpellOnUnit = true;
                    spellid = 62113;
                    mobId = 32975;
                }

                if (quest.Id == 29087)
                {

                    isSpellOnUnit = true;
                    spellid = 62113;
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

                if (quest.Id == 50154)
                {
                    mobId = 126722;
                    farmMobIds.Add(126726);
                    farmMobIds.Add(126725);
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
                            isGameObject = true;
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
                    Host.log("НПС не найден ", Host.LogLvl.Error);
                    Thread.Sleep(10000);
                    return false;
                }


                if (quest.Id == 858)
                    farmLoc.Z = (float)105.12;


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
                    Host.log("НПС для квеста " + farmMobId);
                }



                if (isSpellOnUnit)
                {
                    MyUseSpellClick(mobId);
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
                    Host.log("Координаты GameObject не найдены ", Host.LogLvl.Error);
                    return false;
                }

                //Дойти по координатам
                if (!Host.CommonModule.MoveTo(farmLoc, 30, 30))
                    return false;

                //Использовать скилл
                var zone = new RoundZone(farmLoc.X, farmLoc.Y, 1000);

                var farmmoblist = new List<uint>();
                //   if (!zone.ObjInZone(host.Me))



                farmmoblist.Add(Convert.ToUInt32(questObjective.ObjectID));


                if (farmmoblist.Count == 0)
                {
                    Host.log("Не нашел пропы или фильтр пропов пустой", Host.LogLvl.Error);
                    Host.MainForm.On = false;
                }

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
                //int badRadius = 0;
                while (Host.MainForm.On
                       && Host.ItemManager.GetFreeInventorySlotsCount() >= Host.CharacterSettings.InvFreeSlotCount
                       && !IsQuestComplite(quest.Id, objectiveindex)
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