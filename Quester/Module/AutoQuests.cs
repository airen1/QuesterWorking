using Out.Internal.Core;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WoWBot.Core;
using WowAI;
using WoWBot.WoWNetwork;

namespace WowAI.Module
{
    internal partial class AutoQuests : Module
    {

        private List<uint> _listQuest;
        public MyQuestHelpClass MyQuestHelps = new MyQuestHelpClass();

        public List<MyQuestHelpClass.MyQuest> ListQuestClassic = new List<MyQuestHelpClass.MyQuest>();
        private string _state;
        public bool NeedActionNpcSell;
        public bool NeedActionNpcRepair;
        public List<MultiZone> IgnoreMultiZones = new List<MultiZone>();
        public bool NeedChangeMultizone = true;
        public MultiZone BestMultizone;
        public bool SavePointMove = false;
        public Vector3F PreViliousPoint;
        public List<uint> IsMapidDungeon = new List<uint>
        {
            600, 43, 725, 1175, 557, 2207, 543
        };
        public List<Vector3F> BadLoc = new List<Vector3F>();

        private uint _bestQuestId;
        internal uint BestQuestId
        {
            get => _bestQuestId;
            set
            {
                try
                {
                    if (_bestQuestId != value)
                    {
                        var quest = Host.GetQuest(value);
                        if (quest != null)
                        {
                            if (Host.ClientType == EWoWClient.Classic)
                            {
                                Host.MainForm.SetQuestIdText("Quest(" + Host.AutoQuests.ListQuestClassic.Count + ") :" + quest.Template.LogTitle + "[" + value + "] " + _state);
                            }
                            else
                            {
                                Host.MainForm.SetQuestIdText("Quest(" + Host.AutoQuests._listQuest.Count + ") :" + quest.Template.LogTitle + "[" + value + "] " + _state);
                            }
                        }
                        else
                        {
                            if (Host.GameDB.QuestTemplates.ContainsKey(value))
                            {
                                if (Host.ClientType == EWoWClient.Classic)
                                {
                                    Host.MainForm.SetQuestIdText("Quest(" + Host.AutoQuests.ListQuestClassic.Count + ") :" + Host.GameDB.QuestTemplates[value].LogTitle + "[" + value + "] " + _state);
                                }
                                else
                                {
                                    Host.MainForm.SetQuestIdText("Quest(" + Host.AutoQuests._listQuest.Count + ") :" + Host.GameDB.QuestTemplates[value].LogTitle + "[" + value + "] " + _state);
                                }

                            }

                            else
                            {
                                if (Host.ClientType == EWoWClient.Classic)
                                {
                                    Host.MainForm.SetQuestIdText("Quest(" + Host.AutoQuests.ListQuestClassic.Count + ") : Квест не известен " + "[" + value + "] " + _state);
                                }
                                else
                                {
                                    Host.MainForm.SetQuestIdText("Quest(" + Host.AutoQuests._listQuest.Count + ") : Квест не известен " + "[" + value + "] " + _state);
                                }
                            }

                        }

                        _bestQuestId = value;
                    }
                }
                catch (Exception e)
                {
                    Host.log("Ошибка " + e);
                    _bestQuestId = 0;
                }
            }
        }
        public string ShedulePlugin = "";
        public bool EnableFarmProp = true;
        public bool EnableSkinning = true;
        public bool Continue;
        int _questFix;
        public bool WaitTeleport;
        public bool NeedFindBestPoint;
        public bool IsNeedWaitAfterLoading;
        public Stopwatch ScriptStopwatch = new Stopwatch();
        public bool HerbQuest;
        public bool StartWait;
        public bool Send;
        private bool _buySubs;
        private readonly List<EEquipmentSlot> _equipCells = new List<EEquipmentSlot>();
        private bool _notGoldToken;
        private int _fixBadDialog;

        public bool TryFly;
        private bool _move;
        private int _cancelQuestFix;
        private int _completeQuestCount;
        public ExecuteType QuestType = ExecuteType.Unknown;
       

        public override void Start(Host host)
        {
            _listQuest = new List<uint>();
            base.Start(host);
        }

        public override void Stop()
        {
            Host.CancelMoveTo();
            base.Stop();
        }

        private bool CheckLevelForChangeConfig()
        {
            if (Host.StartLevel == -1 || Host.EndLevel == -1)
                return false;
            if (Host.Me.Level >= Host.StartLevel && Host.Me.Level <= Host.EndLevel)
                return false;
            Host.LoadSettingsForQp();
            Host.ApplySettings();
            return true;
        }

        public override void Run(CancellationToken ct)
        {
            try
            {
                if (Host.ClientType == EWoWClient.Classic)
                    FillQuestClassic();
                while (!Host.CancelRequested && !ct.IsCancellationRequested)
                {
                    base.Run(ct);
                    Thread.Sleep(100);

                    if (SavePointMove)
                    {
                        Host.MainForm.Dispatcher.Invoke(() =>
                        {
                            if (Host.Me.Distance(PreViliousPoint) > Convert.ToInt32(Host.MainForm.TextBoxScriptSavePointMoveDist.Text))
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

                    if (!Host.Check())
                        continue;
                    if (Host.CommonModule.InFight())
                        continue;
                    if (Host.CommonModule.IsMoveSuspended())
                        continue;
                    if (Host.MyIsNeedRegen())
                        continue;

                    if (Host.CharacterSettings.DebuffDeath)
                    {
                        var debuf = Host.MyGetAura(15007);
                        if (debuf != null)
                        {
                            Host.log("Ожидаю " + debuf.SpellName + " " + debuf.Remaining + "  " + debuf.Duraction);
                            Thread.Sleep(5000);
                            continue;
                        }
                    }

                    if (NeedActionNpcRepair || NeedActionNpcSell)
                    {
                        if (!Host.CharacterSettings.FightForSell)
                            Host.FarmModule.FarmState = FarmState.Disabled;

                        if (Host.CharacterSettings.UseStoneForSellAndRepair)
                            Host.MyUseStone();

                        if (Host.CharacterSettings.UseWhistleForSellAndRepair)
                            Host.MyUseStone2();

                        if (Host.CharacterSettings.PikPocket)
                        {
                            if (!Host.MyOpenLockedChest())
                                continue;

                            if (Host.MapID == 209)
                            {
                                if (!Host.CommonModule.MoveTo(1210.42, 842.17, 8.91))
                                    continue;
                                Host.MyMoveForvard(2000);
                                continue;
                            }
                        }


                        if (NeedActionNpcSell)
                        {
                            if (!Host.MyMoveToSell())
                            {
                                Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                continue;
                            }
                            NeedActionNpcSell = false;
                        }

                        if (NeedActionNpcRepair)
                        {
                            if (!Host.MyMoveToRepair())
                            {
                                Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                continue;
                            }
                            NeedActionNpcRepair = false;
                        }
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        continue;
                    }

                    if (Host.MyIsNeedBuy())
                    {
                        if (!Host.CharacterSettings.FightForSell)
                            Host.FarmModule.FarmState = FarmState.Disabled;
                        Host.MyBuyItems(false);
                        continue;
                    }

                    //Продажа
                    if (Host.GetAgroCreatures().Count == 0 && !IsMapidDungeon.Contains(Host.MapID))
                    {
                        if (Host.MyIsNeedSell() || Host.MyIsNeedRepair())
                        {
                            NeedActionNpcSell = true;
                            NeedActionNpcRepair = true;
                            continue;
                        }
                    }

                    if (Host.IsNeedAuk())
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

                        if (Host.GetBotLogin() == "deathstar")
                            Host.MyMail();

                        Host.Auk();
                        Host.MyMail();
                        Host.Auk();
                        Host.MyMail();
                        continue;
                    }

                    if (Host.ClientType == EWoWClient.Classic)
                    {
                        if (Host.MyIsNeedLearnSpell())
                        {
                            Host.MyLearnSpell();
                            continue;
                        }
                        if (!EquipAuc())
                            continue;
                    }

                    if (Host.CharacterSettings.StoneRegister)
                    {
                        if (Host.CharacterSettings.StoneLoc.Distance(Host.BindPoint.Location) > 200)
                        {
                            Host.log("Нужно зарегистрировать точку для камня Dist:" + Host.CharacterSettings.StoneLoc.Distance(Host.BindPoint.Location) + "  " + Host.BindPoint.MapID + " " + Host.MapID);

                            if (Host.CharacterSettings.StoneAreaId != Host.Area.Id || Host.Me.Distance(Host.CharacterSettings.StoneLoc) > 1000)
                            {
                                if (!Host.MyUseTaxi(Host.CharacterSettings.StoneAreaId, Host.CharacterSettings.StoneLoc))
                                    continue;
                            }

                            if (!Host.CommonModule.MoveTo(Host.CharacterSettings.StoneLoc, 10))
                                continue;
                            Unit npc = null;
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (!entity.IsInnkeeper)
                                    continue;
                                npc = entity;
                            }

                            if (npc != null)
                            {
                                if (Host.Me.Distance(npc) > 2)
                                    Host.CommonModule.MoveTo(npc, 2);
                                Host.MyOpenDialog(npc);
                                foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                {
                                    Host.log(gossipOptionsData.OptionNPC + " " + gossipOptionsData.Text);
                                    if (gossipOptionsData.OptionNPC != EGossipOptionIcon.Interact2)
                                        continue;
                                    Host.SelectNpcDialog(gossipOptionsData);
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                            }
                            continue;
                        }
                    }

                    if (!Host.MyOpenTaxyRoute())
                        continue;

                    if (Host.MyAllItemsRepair())
                    {
                        Host.MyMoveToSell();
                        Host.MyMoveToRepair();
                        continue;
                    }

                    if (IsNeedWaitAfterLoading)
                    {
                        Thread.Sleep(1000);
                        IsNeedWaitAfterLoading = false;
                    }

                    switch (Host.CharacterSettings.Mode)
                    {
                        case Mode.Questing:// "Выполнение квестов": //Квест
                            {

                                if (Host.ClientType == EWoWClient.Classic)
                                    continue;
                                if (Host.CharacterSettings.StopQuesting && Host.Me.Level >= Host.CharacterSettings.StopQuestingLevel)
                                {
                                    Host.MainForm.On = false;
                                    continue;
                                }

                                if (Host.Check() && !Host.CommonModule.InFight() && Host.FarmModule.ReadyToActions && !Host.CommonModule.IsMoveSuspended())
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


                                            if (Host.Me.Distance(8381.02, 987.79, 29.61) < 50)
                                                Host.MyUseTaxi(148, new Vector3F((float)7459.90, (float)-326.56, (float)8.09));
                                            else
                                            {
                                                Host.CommonModule.MoveTo(new Vector3F(9944.84, 2608.60, 1316.28), 1);

                                                continue;
                                            }

                                        }

                                    }

                                    if (Host.Zone.Id == 9830 && Host.MapID == 1950 && Host.Area.Id == 9830)
                                    {

                                        Host.SetCTMMovement(true);
                                        if (!Host.CommonModule.MoveTo(new Vector3F(1262.36, 2454.28, 187.83), 1))
                                            continue;
                                        var npc = Host.GetNpcById(112565);
                                        Host.MyDialog(npc, 2);
                                        Thread.Sleep(10000);
                                        Host.SetCTMMovement(false);

                                        while (true)
                                        {
                                            if (!Host.MainForm.On)
                                                break;
                                            Thread.Sleep(1000);
                                            Host.log("Ожидаю приглашения " + " " + Host.LFGStatus.Reason, LogLvl.Important);
                                            if (Host.LFGStatus.Reason == ELfgUpdateType.SUSPENDED_QUEUE)
                                            {
                                                Thread.Sleep(2000);

                                                if (!Host.LFGStatus.Proposal.Accept())
                                                    Host.log("Не смог принять " + Host.LFGStatus.Reason + "   " + Host.GetLastError(), LogLvl.Error);
                                                while (Host.GameState == EGameState.Ingame)
                                                {
                                                    if (!Host.MainForm.On)
                                                        break;
                                                    Thread.Sleep(1000);
                                                    Host.log("Ожидаю вход " + " " + Host.LFGStatus.Reason + "  " + Host.GameState, LogLvl.Important);
                                                }
                                                break;
                                            }
                                        }
                                        continue;
                                    }


                                    if (Host.Zone.Id == 9826 && Host.MapID == 1949 && Host.Area.Id == 9826)
                                    {
                                        Host.log("Необходимо выбраться с корабля");
                                        Host.MyUseStone();
                                        Host.log("Необходим перезапуск после пролога ", LogLvl.Error);
                                        Thread.Sleep(15000);
                                        Host.TerminateGameClient();
                                        Thread.Sleep(1000);
                                        continue;
                                    }

                                    if (Host.MapID == 1)
                                    {
                                        if (Host.CharacterSettings.Quest.Contains("OrdaBFA") && Host.Me.Level != 110 && Host.Me.Level != 120)
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
                                                Host.CommonModule.MoveTo(gameObject, 1);
                                                Host.CommonModule.MyUnmount();
                                                Host.CanselForm();
                                                Thread.Sleep(5000);
                                                gameObject.Use();
                                                Thread.Sleep(2000);
                                                while (Host.GameState != EGameState.Ingame)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                Host.MyWait(10000);
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
                                           48588,
                                           47756,
                                           48093,
                                          // 50080,
                                           49315,
                                           47527
                                        };

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



                                    _listQuest.Clear();
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

                                            for (var index = 0; index < (quest.Template.QuestObjectives).Length; index++)
                                            {

                                                var templateQuestObjective = (quest.Template.QuestObjectives)[index];
                                                Host.log("Type1: " + templateQuestObjective.Type
                                                                  + " Amount:" + templateQuestObjective.Amount
                                                                  + " ObjectID:" + templateQuestObjective.ObjectID
                                                                  + "StorageIndex" + templateQuestObjective.StorageIndex);
                                                if (templateQuestObjective.StorageIndex < 0)
                                                    continue;
                                                if (quest.Counts[templateQuestObjective.StorageIndex] >= templateQuestObjective.Amount)
                                                    continue;


                                                objectiveindex = index;
                                                break;
                                            }

                                            switch (quest.Id)
                                            {
                                                case 47959:
                                                    {
                                                        if (!Host.CheckQuestCompleted(47959) || !Host.CheckQuestCompleted(50755))
                                                            _listQuest.Add(questCoordSetting.QuestId);
                                                    }
                                                    break;
                                                case 47324:
                                                    {
                                                        _listQuest.Add(questCoordSetting.QuestId);
                                                    }
                                                    break;
                                                case 48684:
                                                    {
                                                        _listQuest.Add(questCoordSetting.QuestId);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        if (objectiveindex == -1)
                                                            _listQuest.Add(questCoordSetting.QuestId);
                                                        else
                                                            _listQuest.Insert(0, questCoordSetting.QuestId);
                                                    }
                                                    break;
                                            }
                                            continue;
                                        }

                                        if (Host.Me.Level < minlevel)
                                            continue;

                                        if (nextQuestId == questCoordSetting.QuestId)
                                        {
                                            nextQuestId = questTemplate.RewardNextQuest;
                                            continue;
                                        }

                                        nextQuestId = questTemplate.RewardNextQuest;
                                        _listQuest.Add(questCoordSetting.QuestId);
                                    }



                                    if (Host.AdvancedLog)
                                    {
                                        Host.log("Квесты добавлены за                               " + sw.ElapsedMilliseconds + " мс. Всего квестов " + _listQuest.Count, LogLvl.Important);
                                        for (var index = 0; index < _listQuest.Count; index++)
                                        {
                                            var i = _listQuest[index];
                                            Host.log(Host.GameDB.QuestTemplates[i].LogTitle + "[" + i + "]");
                                            if (index > 2)
                                                break;
                                        }
                                    }

                                    for (var i = _listQuest.Count - 1; i > -1; i--)
                                    {

                                        if (Host.CharacterSettings.IgnoreQuests.Any(item => _listQuest[i] == item.Id))
                                            _listQuest.Remove(_listQuest[i]);
                                    }


                                    if (Host.AdvancedLog)
                                    {
                                        Host.log("Квесты обновлены за                               " + sw.ElapsedMilliseconds + " мс");
                                        Host.log("Квестов можно выполнить: " + _listQuest.Count + "/" + Host.QuestSettings.QuestCoordSettings.Count);
                                    }






                                    //Сдать все квесты
                                    //Взять все доступные квесты
                                    var isNeedApply = false;
                                    foreach (var i in _listQuest)
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
                                            for (var index = 0; index < (quest.Template.QuestObjectives).Length; index++)
                                            {
                                                var templateQuestObjective = (quest.Template.QuestObjectives)[index];
                                                if (templateQuestObjective.Type == EQuestRequirementType.AreaTrigger && quest.State == EQuestState.None)
                                                    objectiveindex = 0;
                                                if (templateQuestObjective.StorageIndex < 0)
                                                    continue;

                                                Host.log(quest.Template.LogTitle + "[" + quest.Id + "] Type2: " + templateQuestObjective.Type + " Amount:" + templateQuestObjective.Amount + " ObjectID:" + templateQuestObjective.ObjectID + "  StorageIndex:" + templateQuestObjective.StorageIndex);
                                                if (quest.Counts[templateQuestObjective.StorageIndex] >= templateQuestObjective.Amount)
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

                                            if (quest.Id == 50561)
                                                continue;
                                            if (quest.Id == 49955)
                                                continue;
                                            if (quest.Id == 49956)
                                                continue;
                                            if (objectiveindex == -1 && questPoi != null && Host.MyDistanceNoZ(Host.Me.Location.X, Host.Me.Location.Y, questPoi.Points[0].X, questPoi.Points[0].Y) < 90)
                                            {
                                                Host.log("Сдаю квест поблизости " + objectiveindex + "  " + quest.Template.LogTitle + "[" + quest.Id + "]", LogLvl.Ok);

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
                                                    if (questSet.QuestId == 51985 || questSet.QuestId == 51233 || questSet.QuestId == 51057 || questSet.QuestId == 47499)
                                                        continue;

                                                    Host.log("Беру квест поблизости у NPC " + npc.Name + "[" + npc.Id + "]  " + Host.GameDB.QuestTemplates[questSet.QuestId].LogTitle + "[" + questSet.QuestId + "]", LogLvl.Ok);

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
                                                    Host.log("Беру квест поблизости у GO " + npc.Name + "[" + npc.Id + "]  " + Host.GameDB.QuestTemplates[questSet.QuestId].LogTitle + "[" + questSet.QuestId + "]", LogLvl.Ok);

                                                    MyApplyQuest(npc, questSet.QuestId); // взял квест
                                                    Thread.Sleep(1000);
                                                    isNeedApply = true;
                                                    break;
                                                }
                                            }
                                            if (npc != null && npc.Type == EBotTypes.Vehicle)
                                            {
                                                if ((npc as Vehicle).QuestGiverStatus == EQuestGiverStatus.Available)
                                                {
                                                    Host.log("Беру квест поблизости у Vehicle " + npc.Name + "[" + npc.Id + "]  " + Host.GameDB.QuestTemplates[questSet.QuestId].LogTitle + "[" + questSet.QuestId + "]", LogLvl.Ok);

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
                                    foreach (var quest in _listQuest)
                                    {
                                        if ((!Host.FarmModule.ReadyToActions) & (Host.MainForm.On))
                                            break;

                                        if ((Host.CommonModule.IsMoveSuspended()) & (Host.MainForm.On))
                                            break;

                                        if (!RunQuest(quest))
                                            break;
                                    }


                                    if (_listQuest.Count == 0)
                                    {
                                        Host.log("Нет квестов");

                                        if (Host.CharacterSettings.LaunchScript)
                                        {
                                            Host.CharacterSettings.Mode = Mode.Script;
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
                                            if (!Host.CommonModule.MoveTo(new Vector3F((float)-742.13, (float)-2217.77, (float)16.17), 1))
                                                continue;
                                            Host.GetCurrentAccount().IsAutoLaunch = false;
                                            Host.log("Выключаю окно игры 1 ");
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



                                        if (mobs)
                                        {
                                            if (!Host.CommonModule.MoveTo(coord, 20))
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
                                                   && meLevel == Host.Me.Level
                                                   && Host.FarmModule.ReadyToActions
                                                   && Host.FarmModule.FarmState == FarmState.FarmMobs
                                            )
                                            {
                                                if (Host.MyIsNeedRepair())
                                                    break;
                                                if (Host.MyIsNeedSell())
                                                    break;
                                                if (Host.FarmModule.BestMob == null && Host.Me.HpPercents > 80)
                                                    badRadius++;
                                                else
                                                    badRadius = 0;
                                                if (badRadius > 100)
                                                    Host.CommonModule.MoveTo(coord, 2);
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

                        case Mode.FarmMob:// "Убийство мобов": //ФАрм
                            {
                                MyModeFarmMob();
                            }

                            break;

                        case Mode.FarmResource://"Сбор ресурсов": //Сбор
                            {
                                if (Host.MainForm.On && Host.Check())
                                {
                                    Host.log("Сбор");
                                    if (Math.Abs(Host.CharacterSettings.GatherLocX) < 1)
                                    {
                                        Host.log("Не выбраны координаты зоны ", LogLvl.Error);
                                        Host.MainForm.On = false;
                                        continue;

                                    }

                                    if (Host.CharacterSettings.GatherLocAreaId != Host.Area.Id || Host.CharacterSettings.GatherLocMapId != Host.MapID)
                                    {
                                        Host.log("Зона фарма находится в другой зоне 2", LogLvl.Error);
                                        Host.MainForm.On = false;
                                        continue;
                                    }


                                    var zone = new RoundZone(Host.CharacterSettings.GatherLocX, Host.CharacterSettings.GatherLocY, Host.CharacterSettings.GatherRadius);
                                    Host.log(Host.CharacterSettings.GatherLocX + "    " + Host.CharacterSettings.GatherLocY + "   " + Host.CharacterSettings.GatherRadius);
                                    var farmmoblist = new List<uint>();
                                    //   if (!zone.ObjInZone(host.Me))
                                    if (!Host.CommonModule.MoveTo(Host.CharacterSettings.GatherLocX, Host.CharacterSettings.GatherLocY, Host.CharacterSettings.GatherLocZ, 25))
                                        break;

                                    foreach (var dbPropInfo in Host.CharacterSettings.PropssSettings)
                                        farmmoblist.Add(dbPropInfo.Id);


                                    if (farmmoblist.Count == 0)
                                    {
                                        Host.log("Не нашел пропы или фильтр пропов пустой", LogLvl.Error);
                                        Host.MainForm.On = false;
                                    }
                                    Host.FarmModule.SetFarmProps(zone, farmmoblist);
                                    //int badRadius = 0;
                                    while (Host.MainForm.On

                                           && Host.CharacterSettings.Mode == Mode.FarmResource// "Сбор ресурсов"
                                           && Host.FarmModule.ReadyToActions
                                           && Host.FarmModule.FarmState == FarmState.FarmProps)
                                    {
                                        if (Host.MyIsNeedRepair())
                                            break;
                                        if (Host.MyIsNeedSell())
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

                        case Mode.Script:// "Данж.(п)":
                            {
                                while (Host.MainForm.On && Host.Check() && Host.CharacterSettings.Mode == Mode.Script)
                                {
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
                                    if (!IsMapidDungeon.Contains(Host.MapID))
                                        if (Host.Me.GetThreats().Count == 0)
                                            if (NeedActionNpcSell || NeedActionNpcRepair || Host.IsNeedAuk() || Host.MyIsNeedSell() || Host.MyIsNeedRepair())
                                                break;
                                    Mode_86();
                                }
                            }
                            break;


                        case Mode.OnlyAttack:
                            {
                                while (Host.GameState == EGameState.Ingame && Host.CharacterSettings.Mode == Mode.OnlyAttack && Host.MainForm.On)
                                    Thread.Sleep(1000);
                            }
                            break;

                        case Mode.QuestingClassic:
                            {
                                QuestingClassic();
                            }
                            break;
                    }

                }

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

        private void MyGetMyltiZone()
        {
            double bestDist = 9999999;
            foreach (var characterSettingsMultiZone in Host.CharacterSettings.MultiZones)
            {
                if (IgnoreMultiZones.Contains(characterSettingsMultiZone))
                {
                    Host.log("Зона " + characterSettingsMultiZone.Id + " находится в списке игнора");
                    continue;
                }

                if (characterSettingsMultiZone.ChangeByLevel)
                {
                    if (Host.Me.Level <= characterSettingsMultiZone.MaxLevel && Host.Me.Level >= characterSettingsMultiZone.MinLevel)
                    {
                        BestMultizone = characterSettingsMultiZone;
                        bestDist = Host.Me.Distance(characterSettingsMultiZone.Loc);
                        NeedChangeMultizone = false;
                    }
                    Host.log("Зона " + characterSettingsMultiZone.Id + " меняется по уровню " + Host.Me.Level + " " + characterSettingsMultiZone.MinLevel + "-" + characterSettingsMultiZone.MaxLevel);
                    continue;
                }

                if (Host.Me.Distance(characterSettingsMultiZone.Loc) < bestDist)
                {
                    BestMultizone = characterSettingsMultiZone;
                    bestDist = Host.Me.Distance(characterSettingsMultiZone.Loc);
                    NeedChangeMultizone = false;
                }
            }
        }

        private void MyModeFarmMob()
        {
            var farmmoblist = new List<uint>();

            //Заполнить список мобов

            if (Host.CharacterSettings.UseMultiZone)
            {

                if (NeedChangeMultizone)
                {
                    MyGetMyltiZone();
                }

                if (BestMultizone == null)
                {
                    IgnoreMultiZones.Clear();
                    NeedChangeMultizone = true;
                    Host.log("Не нашел зону, очищаю список " + NeedChangeMultizone + "  " + IgnoreMultiZones.Count, LogLvl.Important);
                    return;
                }
                else
                {
                    Host.log(IgnoreMultiZones.Count + ")Выбрал зону: " + BestMultizone.Id, LogLvl.Ok);
                }

                if (BestMultizone.UseFilter)
                {
                    Host.log("Использую фильтр из мультизоны");
                    farmmoblist = BestMultizone.ListMobs;
                }
                else
                {
                    if (Host.CharacterSettings.UseFilterMobs)
                    {
                        foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                        {
                            if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                                continue;
                            farmmoblist.Add(characterSettingsMobsSetting.Id);
                        }
                    }
                }
                Host.log("Фарм мобов " + BestMultizone.Id);



                if (Math.Abs(BestMultizone.Loc.X) < 1)
                {
                    Host.log("Не выбраны координаты зоны ", LogLvl.Error);
                    Host.MainForm.On = false;
                }


            }
            else
            {
                if (Host.CharacterSettings.UseFilterMobs)
                {
                    foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                    {
                        if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                            continue;
                        farmmoblist.Add(characterSettingsMobsSetting.Id);
                    }
                }
            }

            if (farmmoblist.Count > 0)
                Host.log("Выбранно: " + farmmoblist.Count + " мобов для фарма");
            else
                Host.log("Не нашел мобов или фильтр мобов пустой", LogLvl.Error);


            //Получить зону

            Zone zone;

            if (Host.CharacterSettings.UseMultiZone)
            {
                if (BestMultizone.UsePoligon)
                    zone = BestMultizone.PolygoneZone;
                else
                    zone = new RoundZone(BestMultizone.Loc.X, BestMultizone.Loc.Y, BestMultizone.Radius);
            }
            else
            {
                if (Host.CharacterSettings.UsePoligon)
                    zone = Host.CharacterSettings.PolygoneZone;
                else
                    zone = new RoundZone(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmRadius);
            }
            Host.DrawZones(new List<Zone> { zone });

            //Бег в зону

            if (Host.CharacterSettings.UseMultiZone)
            {
                if (BestMultizone.MapId != Host.MapID)
                {
                    Host.log("Зона фарма находится на другом континенте 1 " + BestMultizone.AreaId + " / " + Host.MapID, LogLvl.Error);
                    Host.MainForm.On = false;
                    return;
                }

                if (BestMultizone.AreaId != Host.Area.Id || Host.Me.Distance(BestMultizone.Loc) > 1000)
                {
                    if (!Host.MyUseTaxi(BestMultizone.AreaId, BestMultizone.Loc))
                        return;
                    Host.log("Зона фарма находится в другой зоне 1 " + BestMultizone.AreaId + " / " + Host.Area.Id, LogLvl.Error);
                }
                if (!zone.ObjInZone(Host.Me))
                {
                    if (!zone.PointInZone(BestMultizone.Loc.X, BestMultizone.Loc.Y))
                    {
                        Host.log("Точка " + BestMultizone.Loc + " вне зоны " + BestMultizone.Id);
                        var findPoint = zone.GetRandomPoint();
                        var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, Host.GetNavMeshHeight(new Vector3F(findPoint.X, findPoint.Y, Host.Me.Location.Z)));
                        if (!Host.CommonModule.MyUseGpsCustom(vectorPoint))
                            return;
                        if (!Host.CommonModule.MoveTo(vectorPoint, 1))
                            return;
                    }
                    if (!Host.CommonModule.MyUseGpsCustom(BestMultizone.Loc))
                        return;
                    if (!Host.CommonModule.MoveTo(BestMultizone.Loc, 1))
                        return;
                }
            }
            else
            {
                if (Host.CharacterSettings.FarmLocMapId != Host.MapID)
                {
                    Host.log("Зона фарма находится на другом континенте 1 " + Host.CharacterSettings.FarmLocMapId + " / " + Host.MapID, LogLvl.Error);
                    Host.MainForm.On = false;
                    return;
                }

                if (Host.CharacterSettings.FarmLocAreaId != Host.Area.Id || Host.Me.Distance(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ) > 1000)
                {
                    if (!Host.MyUseTaxi(Host.CharacterSettings.FarmLocAreaId, new Vector3F(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ)))
                        return;
                    Host.log("Зона фарма находится в другой зоне 1 " + Host.CharacterSettings.FarmLocAreaId + " / " + Host.Area.Id, LogLvl.Error);
                }
                if (!zone.ObjInZone(Host.Me))
                {
                    Host.log("Бегу в зону");
                    var zoneloc = new Vector3F(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ);
                    if (Host.CharacterSettings.UsePoligon)
                    {
                        if (!Host.CommonModule.MyUseGpsCustom(new Vector3F(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ)))
                            return;
                        if (Host.MainForm.On && !Host.CommonModule.MoveTo(new Vector3F(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ), 10))
                            return;
                    }
                    else
                    {
                        if (!Host.CommonModule.MyUseGpsCustom(zoneloc))
                            return;
                        if (Host.MainForm.On && !Host.CommonModule.MoveTo(zoneloc, 1))
                            return;
                    }
                }

            }

            //фарм мобов


            if (!Host.CharacterSettings.UseMultiZone)
            {

                if (Host.CharacterSettings.AoeFarm)
                {
                    AoeFarm(farmmoblist, zone as RoundZone);
                }
                else
                {
                    var badRadius = 0;
                    Host.FarmModule.SetFarmMobs(zone, farmmoblist);
                    while (Host.MainForm.On && Host.CharacterSettings.Mode == Mode.FarmMob && Host.FarmModule.ReadyToActions && Host.FarmModule.FarmState == FarmState.FarmMobs)
                    {
                        if (Host.MyIsNeedRepair())
                            break;
                        if (Host.MyIsNeedBuy())
                            break;
                        if (Host.MyIsNeedSell())
                            break;

                        if (CheckLevelForChangeConfig())
                        {
                            Thread.Sleep(3000);
                            break;
                        }

                        if (Host.GetEntities<Player>().Count > 0)
                            Host.CommonModule.EventPlayerInZone = true;

                        if (Host.FarmModule.BestMob == null)
                            badRadius++;
                        else
                            badRadius = 0;

                        if (Host.FarmModule.MobsWithDropCount() + Host.FarmModule.MobsWithSkinCount() > 0)
                            badRadius = 0;
                        if (Host.MyIsNeedRegen())
                            badRadius = 0;

                        if (badRadius > 50)
                        {
                            if (Host.CharacterSettings.ReturnToCenter)
                            {
                                var centerZone = new RoundZone(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, 10);
                                var findPoint = centerZone.GetRandomPoint();
                                var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, Host.CharacterSettings.FarmLocZ);
                                Host.log("Не могу найти Monster, подбегаю в центр зоны " + Host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                                if (Host.Me.Distance(vectorPoint) > 20)
                                    Host.CommonModule.MoveTo(vectorPoint, 2);
                            }
                            else
                            {
                                var findPoint = zone.GetRandomPoint();
                                var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, Host.GetNavMeshHeight(new Vector3F(findPoint.X, findPoint.Y, Host.Me.Location.Z)));
                                Host.log("Не могу найти Monster, бегаю по зоне " + Host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                                Host.CommonModule.MoveTo(vectorPoint, 20);
                            }
                        }
                        Thread.Sleep(100);
                    }
                    Host.FarmModule.StopFarm();
                    Thread.Sleep(1000);
                }
            }
            else
            {
                var farmTimeSecond = BestMultizone.Time;
                var farmTime = 0;
                // var countDeathOnSpot = Host.ComboRoute.DeadCountInPVP;
                var badRadius = 0;
                Host.FarmModule.SetFarmMobs(zone, farmmoblist);
                if (BestMultizone.ChangeByPlayer)
                {
                    if (Host.GetEntities<Player>().Count > 0)
                    {
                        Host.log("Меняю зону по времени из за игрока в зоне " + " / " + (BestMultizone.TimePlayer * 1000), LogLvl.Important);
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        IgnoreMultiZones.Add(BestMultizone);
                        BestMultizone = null;
                        NeedChangeMultizone = true;
                        return;
                    }
                }
                var swPlayer = new Stopwatch();
                while (Host.MainForm.On
                       && Host.CharacterSettings.Mode == Mode.FarmMob//"Убийство мобов"
                       && Host.FarmModule.ReadyToActions
                       && Host.FarmModule.FarmState == FarmState.FarmMobs)
                {
                    Thread.Sleep(1000);

                    if (Host.MyIsNeedRepair())
                        break;

                    if (Host.MyIsNeedBuy())
                        break;

                    if (CheckLevelForChangeConfig())
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                    if (Host.GetEntities<Player>().Count > 0)
                        Host.CommonModule.EventPlayerInZone = true;

                    if (Host.MyIsNeedSell())
                        break;



                    if (Host.FarmModule.BestMob == null)
                        badRadius++;
                    else
                        badRadius = 0;

                    if (Host.FarmModule.MobsWithDropCount() + Host.FarmModule.MobsWithSkinCount() > 0)
                        badRadius = 0;

                    if (Host.MyIsNeedRegen())
                        badRadius = 0;

                    if (badRadius > 50)
                    {

                        if (Host.CharacterSettings.ReturnToCenter)
                        {
                            var centerZone = new RoundZone(BestMultizone.Loc.X, BestMultizone.Loc.Y, 10);
                            var findPoint = centerZone.GetRandomPoint();
                            var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, BestMultizone.Loc.Z);
                            Host.log("Не могу найти Monster, подбегаю в центр зоны " + Host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                            if (Host.Me.Distance(vectorPoint) > 20)
                                Host.CommonModule.MoveTo(vectorPoint, 2);

                        }
                        else
                        {
                            var findPoint = zone.GetRandomPoint();
                            var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, Host.GetNavMeshHeight(new Vector3F(findPoint.X, findPoint.Y, Host.Me.Location.Z)));
                            Host.log("Не могу найти Monster, бегаю по зоне " + Host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                            Host.CommonModule.MoveTo(vectorPoint, 20);
                        }

                    }

                    farmTime++;
                    if (BestMultizone.ChangeByTime && !Host.CommonModule.InFight() && farmTime > farmTimeSecond)
                    {
                        Host.log("Меняю зону по времени " + farmTime + " / " + farmTimeSecond, LogLvl.Important);
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        IgnoreMultiZones.Add(BestMultizone);
                        BestMultizone = null;
                        NeedChangeMultizone = true;
                        break;
                    }

                    if (BestMultizone.ChangeByLevel && !Host.CommonModule.InFight() && Host.Me.Level > BestMultizone.MaxLevel)
                    {
                        Host.log("Меняю зону по уровню " + BestMultizone.MinLevel + " / " + BestMultizone.MaxLevel, LogLvl.Important);
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        IgnoreMultiZones.Add(BestMultizone);
                        NeedChangeMultizone = true;
                    }

                    if (BestMultizone.ChangeByPlayer)
                    {
                        if (Host.GetEntities<Player>().Count > 0)
                        {
                            if (!swPlayer.IsRunning)
                                swPlayer.Restart();
                        }
                        else
                        {
                            swPlayer.Stop();
                        }

                        if (swPlayer.ElapsedMilliseconds > (BestMultizone.TimePlayer * 1000))
                        {
                            Host.log("Меняю зону по времени из за игрока " + swPlayer.ElapsedMilliseconds + " / " + (BestMultizone.TimePlayer * 1000), LogLvl.Important);
                            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                            IgnoreMultiZones.Add(BestMultizone);
                            BestMultizone = null;
                            NeedChangeMultizone = true;
                            break;
                        }
                    }
                    /* if (BestMultizone.ChangeByDeathPlayer && (Host.ComboRoute.DeadCountInPVP - countDeathOnSpot) > BestMultizone.CountDeathByPlayer)
                     {
                         Host.log("Меняю зону по смерти от игроков " + (Host.ComboRoute.DeadCountInPVP - countDeathOnSpot) + " / " + BestMultizone.CountDeathByPlayer);
                         Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                         IgnoreMultiZones.Add(BestMultizone);
                         NeedChangeMultizone = true;
                     }*/
                }
                Host.FarmModule.StopFarm();
                Thread.Sleep(1000);
            }

        }

        private Item GetEquipItem(EItemSubclassWeapon itemSubclassWeapon)
        {
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.ItemClass != EItemClass.Weapon)
                    continue;
                if (item.Place != EItemPlace.Equipment)
                    continue;
                if (item.ItemSubClass == (int)itemSubclassWeapon)
                    return item;
            }
            return null;
        }

        private Item GetEquipItem(EItemSubclassArmor itemSubclassArmor, EInventoryType inventoryType)
        {
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.ItemClass != EItemClass.Armor)
                    continue;
                if (item.Place != EItemPlace.Equipment)
                    continue;
                if (item.InventoryType != inventoryType)
                    continue;
                if (item.ItemSubClass == (int)itemSubclassArmor)
                    return item;
            }
            return null;
        }

        private bool _needEquipAuc = true;
        private int _needEqupAucLevel;
        public bool EquipAuc()
        {
            // return true;
            try
            {
                if (!_needEquipAuc)
                {
                    if (_needEqupAucLevel == Host.Me.Level)
                        return true;
                }

                if (!Host.CharacterSettings.AdvancedEquip)
                    return true;

                if (Host.Me.Team == ETeam.Horde)
                    if (Host.Area.Id != 1637)
                        return true;

                if (Host.Me.Team == ETeam.Alliance)
                    if (Host.Area.Id != 1519)
                        return true;

                var needAuc = false;
                foreach (var advancedEquipWeapon in Host.CharacterSettings.AdvancedEquipsWeapon)
                {
                    if (advancedEquipWeapon.BuyAuc)
                    {
                        foreach (var item in Host.ItemManager.GetItems())
                        {
                            if (item.Place != EItemPlace.Equipment)
                                continue;
                            if (item.ItemClass != EItemClass.Weapon)
                                continue;
                            Host.log(item.NameRu + " " + item.Level + " " + item.RequiredLevel + "  " + Host.CommonModule.GetCoef(item));
                        }
                        needAuc = true;
                    }
                }

                foreach (var advancedEquipWeapon in Host.CharacterSettings.AdvancedEquipArmors)
                {
                    if (advancedEquipWeapon.BuyAuc)
                    {
                        foreach (var item in Host.ItemManager.GetItems())
                        {
                            if (item.Place != EItemPlace.Equipment)
                                continue;
                            if (item.ItemClass != EItemClass.Armor)
                                continue;
                            Host.log(item.NameRu + " " + item.Level + " " + item.RequiredLevel + "  " + Host.CommonModule.GetCoef(item));
                        }
                        needAuc = true;
                    }
                }

                if (!needAuc)
                    return true;


                if (Host.IsNewMailsAvailable)
                {
                    Host.log("Надо принять почту " + Host.IsNewMailsAvailable);
                    Host.MyMail();
                    return false;
                }

                if (Host.Me.Team == ETeam.Horde && Host.Area.Id == 1637 || Host.Me.Team == ETeam.Alliance && Host.Area.Id != 1519)
                {
                    Host.log("Бегу на аук");
                    Unit npc = null;
                    if (Host.Me.Team == ETeam.Horde)
                    {
                        if (!Host.CommonModule.MoveTo(1668.77, -4459.48, 18.84))
                            return false;
                        foreach (var entity in Host.GetEntities<Unit>())
                        {
                            if (!entity.IsAuctioner)
                                continue;
                            if (entity.Id == 8724)
                                npc = entity;
                        }
                    }

                    if (Host.Me.Team == ETeam.Alliance)
                    {
                        if (!Host.CommonModule.MoveTo(-8813.21, 662.48, 95.42))
                            return false;
                        foreach (var entity in Host.GetEntities<Unit>())
                        {
                            if (!entity.IsAuctioner)
                                continue;
                            if (entity.Id == 15659)
                                npc = entity;
                        }
                    }

                    if (npc == null)
                    {
                        Host.log("Нет НПС для аука", LogLvl.Error);
                        Thread.Sleep(5000);
                        return true;
                    }
                    Host.log("Выбран " + npc.Name + " " + npc.Id);
                    if (Host.ClientType == EWoWClient.Classic)
                    {
                        Host.CommonModule.MoveTo(npc, 4);
                    }

                    Host.MyCheckIsMovingIsCasting();
                    if (!Host.OpenAuction(npc))
                        Host.log("Не смог открыть диалог для аука " + Host.GetLastError(), LogLvl.Error);
                    else
                        Host.log("Открыл диалог для аука", LogLvl.Ok);
                    Thread.Sleep(3000);



                    foreach (var advancedEquipWeapon in Host.CharacterSettings.AdvancedEquipsWeapon)
                    {
                        if (!advancedEquipWeapon.BuyAuc)
                            continue;
                        var filter = new AuctionClassFilter
                        {
                            ItemClass = EItemClass.Weapon,
                            SubFilters = new AuctionSubClassFilter[1]
                        };

                        filter.SubFilters[0] = new AuctionSubClassFilter
                        {
                            ItemSubclass = (int)advancedEquipWeapon.WeaponType,
                            InvTypeMask = EInventoryTypeMask.All
                        };

                        var listFilter = new List<AuctionClassFilter> { filter };
                        var req = new AuctionSearchRequest
                        {
                            MaxReturnItems = 50,
                            SortType = EAuctionSortType.PriceAsc,
                            Filters = listFilter
                        };

                        Host.log("Ищу на ауке " + advancedEquipWeapon.WeaponType);

                        var aucItems = Host.GetAuctionBuyList(req);
                        if (aucItems == null || aucItems.Count == 0)
                        {
                            Host.log("Ничего не нашел");
                            return true;
                        }

                        var equipedItem = GetEquipItem(advancedEquipWeapon.WeaponType);
                        if (equipedItem == null)
                        {
                            Host.log("Нет одетого " + advancedEquipWeapon.WeaponType);
                        }
                        else
                        {
                            Host.log("Одето: " + equipedItem.NameRu + " " + equipedItem.Level + " " + equipedItem.RequiredLevel + " coef: " + Host.CommonModule.GetCoef(equipedItem), LogLvl.Ok);

                        }

                        var buyItems = new List<AuctionItem>();
                        double bestCoef = 0;
                        foreach (var aucItem in aucItems)
                        {
                            if (aucItem.BuyoutPrice == 0)
                                continue;
                            if (!Host.GameDB.ItemTemplates.ContainsKey(aucItem.ItemId))
                                continue;
                            var itemdb = Host.GameDB.ItemTemplates[aucItem.ItemId];
                            if (Host.Me.Level < itemdb.GetBaseRequiredLevel())
                            {
                               // Host.log("Уровень не подходит " + Host.Me.Level + "/" + itemdb.GetBaseRequiredLevel());
                                continue;
                            }
                              
                            if (aucItem.BuyoutPrice > advancedEquipWeapon.MaxPrice)
                            {
                                Host.log("Слишком дорого " + aucItem.BuyoutPrice + "/" + advancedEquipWeapon.MaxPrice);
                                continue;
                            }

                            var rangeCoef = Host.CommonModule.GetCoef(equipedItem) + advancedEquipWeapon.CoefRange - Host.CommonModule.GetCoef(aucItem);

                            if (rangeCoef > 0)
                            {
                                Host.log("Меньший коэф " + rangeCoef + "/" + Host.CommonModule.GetCoef(aucItem) + "/" + Host.CommonModule.GetCoef(equipedItem));
                                continue;
                            }

                            if (aucItem.BuyoutPrice > Host.Me.Money)
                                continue;
                            if (Host.CommonModule.GetCoef(aucItem) > bestCoef)
                                bestCoef = Host.CommonModule.GetCoef(aucItem);
                            buyItems.Add(aucItem);
                            Host.log("Можно купить " + itemdb.GetName() + "[" + aucItem.ItemId + "]  Price " + aucItem.BuyoutPrice + "  coef: " + Host.CommonModule.GetCoef(aucItem), LogLvl.Important);
                        }

                        ulong bestPrice = 999999;
                        AuctionItem bestItem = null;
                        foreach (var auctionItem in buyItems)
                        {
                            if (Host.CommonModule.GetCoef(auctionItem) < bestCoef)
                                continue;
                            if (auctionItem.BuyoutPrice < bestPrice)
                            {
                                bestPrice = auctionItem.BuyoutPrice;
                                bestItem = auctionItem;
                            }
                        }

                        if (bestItem != null)
                        {
                            Host.log("Найден лучший предмет " + bestItem.Template.GetNameRu() + " " + bestItem.BuyoutPrice + " " + bestCoef, LogLvl.Ok);
                            var result = bestItem.MakeBuyout();
                            if (result == EAuctionHouseError.Ok)
                            {
                                Host.log("Выкупил ", LogLvl.Ok);
                                //equipCells.Add(characterSettingsEquipAuc.Slot);
                                Thread.Sleep(5000);
                            }
                            else
                            {
                                Host.log("Не смог выкупить " + result + " " + Host.GetLastError(),
                                    LogLvl.Error);
                                Thread.Sleep(10000);
                            }
                        }
                        else
                        {
                            Host.log("Лучший предмет не найден ", LogLvl.Important);
                        }

                        Thread.Sleep(3000);
                        break;
                    }

                    foreach (var advancedEquipArmor in Host.CharacterSettings.AdvancedEquipArmors)
                    {
                        if (!advancedEquipArmor.BuyAuc)
                            continue;

                        switch (advancedEquipArmor.ArmorType)
                        {
                            case EItemSubclassArmor.MAIL:
                            case EItemSubclassArmor.LEATHER:
                            case EItemSubclassArmor.PLATE:
                            case EItemSubclassArmor.CLOTH:
                                {
                                    var dictionaryMaskTypes = new Dictionary<EInventoryTypeMask, EInventoryType>()
                                    {
                                        {EInventoryTypeMask.Head, EInventoryType.Head},
                                        {EInventoryTypeMask.Body, EInventoryType.Body},//Рубашка
                                        {EInventoryTypeMask.Chest, EInventoryType.Chest},//Грудь
                                        {EInventoryTypeMask.Waist, EInventoryType.Waist},//Ремень
                                        {EInventoryTypeMask.Legs, EInventoryType.Legs},//Штаны
                                        {EInventoryTypeMask.Feet, EInventoryType.Feet},//сапоги
                                        {EInventoryTypeMask.Wrists, EInventoryType.Wrists},//Запястья
                                        {EInventoryTypeMask.Hands, EInventoryType.Hands},//руки
                                        {EInventoryTypeMask.Cloak, EInventoryType.Cloak},//Плащ

                                    };

                                    foreach (var dictionaryMaskType in dictionaryMaskTypes)
                                    {
                                        if (!Host.MainForm.On)
                                            return false;
                                        var filter = new AuctionClassFilter
                                        {
                                            ItemClass = EItemClass.Armor,
                                            SubFilters = new AuctionSubClassFilter[1]
                                        };
                                        filter.SubFilters[0] = new AuctionSubClassFilter
                                        {
                                            ItemSubclass = (int)advancedEquipArmor.ArmorType,
                                            InvTypeMask = dictionaryMaskType.Key
                                        };



                                        var listFilter = new List<AuctionClassFilter> { filter };
                                        var req = new AuctionSearchRequest
                                        {
                                            MaxReturnItems = 50,
                                            SortType = EAuctionSortType.PriceAsc,
                                            Filters = listFilter
                                        };

                                        Host.log("Ищу на ауке " + advancedEquipArmor.ArmorType + " " + dictionaryMaskType);

                                        var aucItems = Host.GetAuctionBuyList(req);
                                        if (aucItems == null || aucItems.Count == 0)
                                        {
                                            Host.log("Ничего не нашел");
                                            continue;
                                        }

                                        var equipedItem = GetEquipItem(advancedEquipArmor.ArmorType, dictionaryMaskType.Value);
                                        if (equipedItem == null)
                                        {
                                            Host.log("Нет одетого " + advancedEquipArmor.ArmorType);
                                        }
                                        else
                                        {
                                            Host.log("Одето: " + equipedItem?.NameRu + " " + equipedItem.Level + " " + equipedItem.RequiredLevel + " coef: " + Host.CommonModule.GetCoef(equipedItem), LogLvl.Ok);

                                        }

                                        var buyItems = new List<AuctionItem>();
                                        double bestCoef = 0;
                                        foreach (var aucItem in aucItems)
                                        {
                                            if (aucItem.BuyoutPrice == 0)
                                                continue;
                                            if (!Host.GameDB.ItemTemplates.ContainsKey(aucItem.ItemId))
                                                continue;
                                            var itemdb = Host.GameDB.ItemTemplates[aucItem.ItemId];
                                            if (Host.Me.Level < itemdb.GetBaseRequiredLevel())
                                                continue;
                                            if (aucItem.BuyoutPrice > advancedEquipArmor.MaxPrice)
                                            {
                                                Host.log("Слишком дорого " + aucItem.BuyoutPrice + "/" + advancedEquipArmor.MaxPrice);
                                                continue;
                                            }
                                            var rangeCoef = Host.CommonModule.GetCoef(equipedItem) + advancedEquipArmor.CoefRange - Host.CommonModule.GetCoef(aucItem);

                                            if (rangeCoef > 0)
                                            {
                                                Host.log("Меньший коэф " + rangeCoef + "/" + Host.CommonModule.GetCoef(aucItem) + "/" + Host.CommonModule.GetCoef(equipedItem));
                                                continue;
                                            }

                                            /* var rangeCoef = Host.CommonModule.GetCoef(equipedItem) - Host.CommonModule.GetCoef(aucItem);
                                             if (rangeCoef > advancedEquipArmor.CoefRange + 1)
                                             {
                                                 Host.log("Меньший коэф " + rangeCoef + "/" + advancedEquipArmor.CoefRange);
                                                 continue;
                                             }*/

                                            if (aucItem.BuyoutPrice > Host.Me.Money)
                                                continue;
                                            if (Host.CommonModule.GetCoef(aucItem) > bestCoef)
                                                bestCoef = Host.CommonModule.GetCoef(aucItem);
                                            buyItems.Add(aucItem);
                                            Host.log("Можно купить " + itemdb.GetName() + "[" + aucItem.ItemId + "]  Price " + aucItem.BuyoutPrice + "  coef: " + Host.CommonModule.GetCoef(aucItem), LogLvl.Important);
                                        }

                                        ulong bestPrice = 999999;
                                        AuctionItem bestItem = null;
                                        foreach (var auctionItem in buyItems)
                                        {
                                            if (Host.CommonModule.GetCoef(auctionItem) < bestCoef)
                                                continue;
                                            if (auctionItem.BuyoutPrice < bestPrice)
                                            {
                                                bestPrice = auctionItem.BuyoutPrice;
                                                bestItem = auctionItem;
                                            }
                                        }

                                        if (bestItem != null)
                                        {
                                            Host.log("Найден лучший предмет " + bestItem.Template.GetNameRu() + " " + bestItem.BuyoutPrice + " " + bestCoef, LogLvl.Ok);
                                            var result = bestItem.MakeBuyout();
                                            if (result == EAuctionHouseError.Ok)
                                            {
                                                Host.log("Выкупил ", LogLvl.Ok);
                                                //equipCells.Add(characterSettingsEquipAuc.Slot);
                                                Thread.Sleep(5000);
                                            }
                                            else
                                            {
                                                Host.log("Не смог выкупить " + result + " " + Host.GetLastError(),
                                                    LogLvl.Error);
                                                Thread.Sleep(10000);
                                            }
                                        }
                                        else
                                        {
                                            Host.log("Лучший предмет не найден ", LogLvl.Important);
                                        }
                                        Thread.Sleep(3000);
                                    }



                                }
                                break;
                            case EItemSubclassArmor.MISCELLANEOUS:
                                break;

                            case EItemSubclassArmor.COSMETIC:
                                break;
                            case EItemSubclassArmor.SHIELD:
                                break;
                            case EItemSubclassArmor.LIBRAM:
                                break;
                            case EItemSubclassArmor.IDOL:
                                break;
                            case EItemSubclassArmor.TOTEM:
                                break;
                            case EItemSubclassArmor.SIGIL:
                                break;
                            case EItemSubclassArmor.RELIC:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }


                    }
                    while (Host.IsNewMailsAvailable)
                    {
                        Thread.Sleep(1000);
                        if (!Host.MainForm.On)
                            return false;
                        Host.MyMail();
                    }

                    _needEqupAucLevel = Host.Me.Level;
                    _needEquipAuc = false;
                }


            }
            catch (Exception e)
            {
                Host.log(e + " ");
            }



            return true;
        }

        public void AoeFarm(List<uint> farmmoblist, RoundZone zone)
        {
            var aoeMobsCount = Host.CharacterSettings.AoeMobsCount;
            var isFind = true;
            var aoeTimer = 0;

            while (Host.GetAgroCreatures().Count < aoeMobsCount && isFind && Host.IsAlive(Host.Me) && Host.MainForm.On && zone.ObjInZone(Host.Me))
            {
                Host.log(" Ищу мобов для АОЕ");
                Thread.Sleep(100);
                Host.FarmModule.FarmState = FarmState.Disabled;
                var listMob = Host.GetEntities<Entity>();


                if (Host.GetAgroCreatures().Count == 0 && Host.Me.HpPercents > 80)
                    aoeTimer++;
                else
                    aoeTimer = 0;

                if (aoeTimer > 50)
                {
                    Host.CommonModule.MoveTo(Host.CharacterSettings.FarmLocX, Host.CharacterSettings.FarmLocY, Host.CharacterSettings.FarmLocZ, 5);

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
                    if (!Host.CommonModule.ForceMoveTo(creature, 1))
                    {
                        Thread.Sleep(100);
                        isFind = false;
                    }
                    break;

                }
            }


            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
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
                            step += iCount;
                        }

                        if (step < 12)
                            return true;
                        if (step == 12)
                        {
                            Host.PrepareCompleteQuest(48755);
                            if (!Host.CompleteQuest(48755))
                            {
                                Host.log("Не смог завершить квест " + Host.GetLastError(), LogLvl.Error);
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
                            step += iCount;
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
                            step += iCount;
                        }

                        if (step < 12)
                            return true;
                        if (step == 12)
                        {
                            Host.PrepareCompleteQuest(51452);
                            if (!Host.CompleteQuest(51452))
                            {
                                Host.log("Не смог завершить квест " + Host.GetLastError(), LogLvl.Error);
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
                            step += iCount;
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
                                        step += iCount;
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
                if ((uint)Host.Me.Race == u)
                    return true;
            return false;
        }

        public bool CheckQuestClass(List<uint> clas)
        {
            if (clas.Count == 0)
                return true;
            foreach (var u in clas)
                if ((uint)Host.Me.Class == u)
                    return true;

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

    }
}