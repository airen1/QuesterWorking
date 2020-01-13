using Out.Internal.Core;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using WowAI.Module;
using WowAI.UI;
using WoWBot.Core;


namespace WowAI
{
    internal partial class Host : Core
    {
        public static bool isReleaseVersion = true;

        private const string Ch = "Quester\\";
        public const uint CanSpellAttack = 6603;
        private const string Version = "v0.34";
        public Random RandGenerator { get; private set; }
        public string CfgName = "";
        public string ScriptName = "";
        public string QuestName = "";
        private string _path = AssemblyDirectory;
        public long TimeInFight;
        public long AllDamage;
        public long KillMobsCount;
        public DateTime TimeWork;
        public int CheckCount;
        public double StartGold;
        public long Startinvgold;
        internal CharacterSettings CharacterSettings { get; set; } = new CharacterSettings();
        internal DungeonSetting DungeonSettings { get; set; }
        internal QuestSetting QuestSettings { get; set; }
        internal QuestStates QuestStates { get; set; }
        internal DropBases DropBases { get; set; } = new DropBases();
        internal MyTaxyNode MyTaxyNode { get; set; } = new MyTaxyNode();
        internal MonsterGroup2 MonsterGroup { get; set; } = new MonsterGroup2();
        internal Main MainForm { get; set; }
        internal CommonModule CommonModule { get; set; }
        internal FarmModule FarmModule { get; set; }
        internal AutoQuests AutoQuests { get; set; }
        public bool FormInitialized { get; set; }
        public MyNpcLocs MyNpcLocss { get; set; } = new MyNpcLocs();
        public MyGameObjectLocs MyGameObjectLocss { get; set; } = new MyGameObjectLocs();
        public bool NeedWaitAfterCombat;
        private string _pathNpCjson = "";
        private string _pathGameObjectLocs = "";
        private string _pathNpCjsonCopy = "";
        private string _pathDropjson = "";
        public string PathQuestSet = "";
        public string PathQuestState = "";
        public string PathTaxyNode = "";
        private string _pathMonsterGroup = "";
        public bool AdvancedLog;
        public string PathGps;
        public int EventInactiveCount;
        public bool EventInactive;
        public string PathGpsCustom;
        public string FileName = "";
        public int StartLevel = -1;
        public int EndLevel = -1;
        public MyQuestBase MyQuestBases = new MyQuestBase();
        private int _badBestMobHp;
        public long TimeAttack;
        private WowGuid _badSid;

        internal bool CancelRequested { get; set; }

        public void log(string text, LogLvl type = LogLvl.Info)
        {
            try
            {
                if (CharacterSettings != null)
                {
                    if (!CharacterSettings.LogAll)
                    {
                        return;
                    }
                }

                var color = new Color();
                switch (type)
                {
                    case LogLvl.Ok:
                        color = Color.Green;
                        break;
                    case LogLvl.Error:
                        color = Color.Red;
                        break;
                    case LogLvl.Info:
                        color = Color.DarkGray;
                        break;
                    case LogLvl.Important:
                        color = Color.DarkOrange;
                        break;
                }

                if (Me != null)
                {
                    Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Me?.Name + "(" + Me?.Level + ")  " + FarmModule?.FarmState + "   " + text, color, GetCurrentAccount().Name);
                }
                else
                {
                    Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + "Character offline: " + "   " + text, color, GetCurrentAccount().Name);
                }
            }
            catch (Exception)
            {
                //igrored
            }
        }

        public MyExp MyExps = new MyExp();

        public void PluginRun()
        {
            try
            {
                if (GetBotLogin() == "Daredevi1")
                {
                    AdvancedLog = true;
                }

               
                //var pathItem = "C:\\AllQuestItem" + ClientType + ".txt";
                //var pathQuest = "C:\\AllQuest" + ClientType + ".txt";
                //if (File.Exists(pathItem))
                //    File.Delete(pathItem);
                //if (File.Exists(pathQuest))
                //    File.Delete(pathQuest);
                //foreach (var gameDbQuestTemplate in GameDB.QuestTemplates)
                //{
                //    if (gameDbQuestTemplate.Value.QuestObjectives != null)
                //        foreach (var valueQuestObjective in gameDbQuestTemplate.Value.QuestObjectives)
                //        {
                //            if (valueQuestObjective.Type == EQuestRequirementType.Item)
                //            {
                //                File.AppendAllText(pathItem, valueQuestObjective.ObjectID + Environment.NewLine);
                //            }
                //        }
                //    File.AppendAllText(pathQuest, gameDbQuestTemplate.Value.Id + Environment.NewLine);
                //}

                ClearLogs(GetCurrentAccount().Name);
                RandGenerator = new Random((int)DateTime.Now.Ticks);
                while (GameState != EGameState.Ingame)
                {
                    log("Ожидаю вход в игру... Status: " + GameState);
                    Thread.Sleep(5000);
                }
                var sw = new Stopwatch();
                if (AdvancedLog)
                {
                    sw.Start();
                }

                FormInitialized = false;

                if (!Directory.Exists(AssemblyDirectory + "\\Configs"))
                {
                    Directory.CreateDirectory(AssemblyDirectory + "\\Configs");
                }

                if (!Directory.Exists(AssemblyDirectory + "\\Configs\\Default"))
                {
                    Directory.CreateDirectory(AssemblyDirectory + "\\Configs\\Default");
                }

                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");
                }



                if (!Directory.Exists(AssemblyDirectory + "\\Log\\"))
                {
                    Directory.CreateDirectory(AssemblyDirectory + "\\Log\\");
                }

                if (isReleaseVersion)
                {
                    PathQuestSet = AssemblyDirectory + "\\Quest\\";
                    if (!Directory.Exists(AssemblyDirectory + "\\TaxyNodes"))
                    {
                        Directory.CreateDirectory(AssemblyDirectory + "\\TaxyNodes\\");
                    }
                    PathTaxyNode = AssemblyDirectory + "\\TaxyNodes\\" + Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
                    if (ClientType == EWoWClient.Classic)
                    {
                        PathGps = AssemblyDirectory + "\\helpGpsClassic_" + MapID + ".db3";

                    }

                    else
                    {
                        PathGps = AssemblyDirectory + "\\helpGps_" + MapID + ".db3";
                    }
                    PathGpsCustom = AssemblyDirectory + "\\helpGpsCustom.db3";
                    PathQuestState = AssemblyDirectory + "\\QuestState\\";
                }
                else
                {
                    PathQuestSet = AssemblyDirectory + "\\Plugins\\Quester\\Quest\\";
                    if (!Directory.Exists(AssemblyDirectory + "\\Plugins\\Quester\\TaxyNodes"))
                    {
                        Directory.CreateDirectory(AssemblyDirectory + "\\Plugins\\Quester\\TaxyNodes\\");
                    }
                    PathTaxyNode = AssemblyDirectory + "\\Plugins\\Quester\\TaxyNodes\\" + Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
                    if (ClientType == EWoWClient.Classic)
                    {

                        PathGps = AssemblyDirectory + "\\Plugins\\Quester\\helpGpsClassic_" + MapID + ".db3";
                    }
                    else
                    {
                        PathGps = AssemblyDirectory + "\\Plugins\\Quester\\helpGps_" + MapID + ".db3";
                    }

                    PathGpsCustom = AssemblyDirectory + "\\Plugins\\Quester\\helpGpsCustom.db3";
                    PathQuestState = AssemblyDirectory + "\\Plugins\\Quester\\QuestState\\";
                }

                if (!Directory.Exists(PathQuestSet))
                {
                    Directory.CreateDirectory(PathQuestSet);
                }

                if (!Directory.Exists(PathQuestState))
                {
                    Directory.CreateDirectory(PathQuestState);
                }

                var lastChanged = File.GetLastWriteTime(AssemblyDirectory + "\\WowAI.dll");
                log("Версия обновлена: " + lastChanged);


                // CharacterSettings = new CharacterSettings();
                DungeonSettings = new DungeonSetting();
                QuestSettings = new QuestSetting();
                QuestStates = new QuestStates();

                if (AdvancedLog)
                {
                    log("Поиск путей                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

               
                var pathQuestjson = "";

                switch (ClientType)
                {
                    case EWoWClient.Retail when isReleaseVersion:
                        pathQuestjson = AssemblyDirectory + "\\MyQuestBase.json";
                        break;
                    case EWoWClient.Retail:
                        pathQuestjson = AssemblyDirectory + "\\Plugins\\Quester\\MyQuestBase.json";
                        break;
                    case EWoWClient.Classic when isReleaseVersion:
                        pathQuestjson = AssemblyDirectory + "\\MyQuestBaseClassic.json";
                        break;
                    case EWoWClient.Classic:
                        pathQuestjson = AssemblyDirectory + "\\Plugins\\Quester\\MyQuestBaseClassic.json";
                        break;
                    case EWoWClient.Unknown:
                        break;
                    case EWoWClient.PTR:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                switch (ClientType)
                {
                    case EWoWClient.Retail when isReleaseVersion:
                        _pathNpCjson = AssemblyDirectory + "\\npc.json";
                        break;
                    case EWoWClient.Retail:
                        _pathNpCjson = AssemblyDirectory + "\\Plugins\\Quester\\npc.json";
                        break;
                    case EWoWClient.Classic when isReleaseVersion:
                        _pathNpCjson = AssemblyDirectory + "\\npcClassic.json";
                        break;
                    case EWoWClient.Classic:
                        _pathNpCjson = AssemblyDirectory + "\\Plugins\\Quester\\npcClassic.json";
                        break;
                    case EWoWClient.Unknown:
                        break;
                    case EWoWClient.PTR:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                if (isReleaseVersion)
                {
                    _pathNpCjsonCopy = AssemblyDirectory + "\\npcCopy.json";
                }
                else
                {
                    _pathNpCjsonCopy = AssemblyDirectory + "\\Plugins\\Quester\\npcCopy.json";
                }

                if (isReleaseVersion)
                {
                    _pathMonsterGroup = AssemblyDirectory + "\\monstergroup.json";
                }
                else
                {
                    _pathMonsterGroup = AssemblyDirectory + "\\Plugins\\Quester\\monstergroup.json";
                }

                switch (ClientType)
                {
                    case EWoWClient.Retail when isReleaseVersion:
                        _pathDropjson = AssemblyDirectory + "\\drop.json";
                        break;
                    case EWoWClient.Retail:
                        _pathDropjson = AssemblyDirectory + "\\Plugins\\Quester\\drop.json";
                        break;
                    case EWoWClient.Classic when isReleaseVersion:
                        _pathDropjson = AssemblyDirectory + "\\MyDropBasesClassic.json";
                        break;
                    case EWoWClient.Classic:
                        _pathDropjson = AssemblyDirectory + "\\Plugins\\Quester\\MyDropBasesClassic.json";
                        break;
                    case EWoWClient.Unknown:
                        break;
                    case EWoWClient.PTR:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (ClientType)
                {
                    case EWoWClient.Classic when isReleaseVersion:
                        _pathGameObjectLocs = AssemblyDirectory + "\\GameObjectClassic.json";
                        break;
                    case EWoWClient.Classic:
                        _pathGameObjectLocs = AssemblyDirectory + "\\Plugins\\Quester\\GameObjectClassic.json";
                        break;
                    case EWoWClient.Retail when isReleaseVersion:
                        _pathGameObjectLocs = AssemblyDirectory + "\\GameObject.json";
                        break;
                    case EWoWClient.Retail:
                        _pathGameObjectLocs = AssemblyDirectory + "\\Plugins\\Quester\\GameObject.json";
                        break;
                    case EWoWClient.Unknown:
                        break;
                    case EWoWClient.PTR:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                MyTaxyNode = (MyTaxyNode)ConfigLoader.LoadConfig(PathTaxyNode, typeof(MyTaxyNode), MyTaxyNode);
                /* foreach (var myTaxyNode in MyTaxyNode.MyTaxyNodes)
                 {
                     log(myTaxyNode.Id + " " + myTaxyNode.Name);
                 }*/

                if (File.Exists(_pathGameObjectLocs))
                {
                    MyGameObjectLocss = (MyGameObjectLocs)ConfigLoader.LoadConfig(_pathGameObjectLocs, typeof(MyGameObjectLocs), MyGameObjectLocss);
                }
                else
                {
                    log("Не найден файл PathNpCjson " + _pathGameObjectLocs + "  " + ClientType, LogLvl.Error);
                }

                if (File.Exists(_pathNpCjson))
                {
                    var pathnpcdefault = "";
                    if (ClientType == EWoWClient.Classic && isReleaseVersion)
                        pathnpcdefault = AssemblyDirectory + "\\npcClassicDefault.json";

                    if (ClientType == EWoWClient.Classic && !isReleaseVersion)
                        pathnpcdefault = AssemblyDirectory + "\\Plugins\\Quester\\npcClassicDefault.json";


                    var mynpclocdefault = (MyNpcLocs)ConfigLoader.LoadConfig(pathnpcdefault, typeof(MyNpcLocs), MyNpcLocss);

                    // log(mynpclocdefault.NpcLocs.Count +" jkl ");
                    MyNpcLocss = (MyNpcLocs)ConfigLoader.LoadConfig(_pathNpCjson, typeof(MyNpcLocs), MyNpcLocss);

                    foreach (var myNpcLoc in mynpclocdefault.NpcLocs)
                    {
                        if (MyNpcLocss.NpcLocs.Any(i => i.Id == myNpcLoc.Id))
                            continue;

                        MyNpcLocss.NpcLocs.Add(myNpcLoc);
                        /*if (GetBotLogin() == "Daredevi1")
                             log("Добавляю");*/
                    }

                    /* foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                     {
                         for (var index = 0; index < myNpcLoc.ListLoc.Count; index++)
                         {
                             var vector3F = myNpcLoc.ListLoc[index];
                             if (double.IsNaN(vector3F.X))
                             {
                                 log("Удаляю кривую координату");
                                 myNpcLoc.ListLoc.RemoveAt(index);
                                 //index = index - 1;
                             }
                         }
                     }*/
                }

                else
                {
                    var pathnpcdefault = "";
                    if (ClientType == EWoWClient.Classic && isReleaseVersion)
                        pathnpcdefault = AssemblyDirectory + "\\npcClassicDefault.json";

                    if (ClientType == EWoWClient.Classic && !isReleaseVersion)
                        pathnpcdefault = AssemblyDirectory + "\\Plugins\\Quester\\npcClassicDefault.json";

                    MyNpcLocss = (MyNpcLocs)ConfigLoader.LoadConfig(pathnpcdefault, typeof(MyNpcLocs), MyNpcLocss);
                    log("Не найден файл PathNpCjson " + _pathNpCjson + "  " + ClientType, LogLvl.Error);
                }

                if (AdvancedLog)
                {
                    log("НПС загружены за                              " + sw.ElapsedMilliseconds + " мс всего: " + MyNpcLocss.NpcLocs.Count + " шт.");
                    sw.Restart();
                }

                if (File.Exists(_pathDropjson))
                {
                    DropBases = (DropBases)ConfigLoader.LoadConfig(_pathDropjson, typeof(DropBases), DropBases);
                }
                else
                {
                    log("Не найден файл PathDropjson " + _pathDropjson, LogLvl.Error);
                }

                if (AdvancedLog)
                {
                    log("Drop загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + DropBases.Drop.Count + " шт.");
                    sw.Restart();
                }

                if (ClientType == EWoWClient.Retail)
                {
                    if (File.Exists(_pathMonsterGroup))
                    {
                        MonsterGroup = (MonsterGroup2)ConfigLoader.LoadConfig(_pathMonsterGroup, typeof(MonsterGroup2), MonsterGroup);
                    }
                    else
                    {
                        log("Не найден файл PathMonsterGroup " + _pathMonsterGroup, LogLvl.Error);
                    }
                }


                if (AdvancedLog)
                {
                    log("MonsterGroup загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + MonsterGroup.MonsterGroups.Count + " шт.");
                    sw.Restart();
                }

                if (GetBotLogin() == "Daredevi1")
                {

                }
                else
                {
                    if (File.Exists(pathQuestjson))
                    {
                        MyQuestBases = (MyQuestBase)ConfigLoader.LoadConfig(pathQuestjson, typeof(MyQuestBase), MyQuestBases);
                    }
                    else
                    {
                        log("Не найден файл PathQuestjson " + pathQuestjson + ClientType, LogLvl.Error);
                    }
                }


                if (AdvancedLog)
                {
                    log("MyQuestBases загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + MyQuestBases.MyQuestBases.Count + " шт.");
                    sw.Restart();
                }


                if (File.Exists(PathQuestState + Me.Name + "[].json"))
                {
                    QuestStates = (QuestStates)ConfigLoader.LoadConfig(PathQuestState + Me.Name + "[" + CurrentServer.Name + "].json", typeof(QuestStates), QuestStates);
                    File.Delete(PathQuestState + Me.Name + "[].json");
                    ConfigLoader.SaveConfig(PathQuestState + Me.Name + "[" + GetCurrentAccount().ServerName + "].json", QuestStates);
                }
                else
                {
                    QuestStates = (QuestStates)ConfigLoader.LoadConfig(PathQuestState + Me.Name + "[" + GetCurrentAccount().ServerName + "].json", typeof(QuestStates), QuestStates);
                }

                //if (!isReleaseVersion)
                //{
                //    formThread = new Thread(() =>
                //    {
                //        try
                //        {
                //            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                //            MainForm = new Main();
                //            MainForm.Show();
                //            FormInitialized = true;
                //            Dispatcher.Run();
                //        }
                //        catch (TaskCanceledException) { }
                //        catch (ThreadAbortException) { }
                //        catch (Exception error)
                //        {
                //            Log(error.ToString());
                //        }
                //    });
                //    formThread.SetApartmentState(ApartmentState.STA);
                //    //  formThread.IsBackground = true;
                //    formThread.Start();
                //}
                //else
                //{
                //    var d = GetMainDispatcher();
                //    if (d == null)
                //    {
                //        Log("Failed to get dispatcher");
                //        return;
                //    }
                //    d.Invoke(() =>
                //    {
                //        try
                //        {
                //            MainForm = new Main();
                //            MainForm.Show();
                //            FormInitialized = true;
                //        }
                //        catch (Exception e)
                //        {
                //            log(e + "");
                //        }
                //    });
                //}



                var d = GetMainDispatcher(); //получает диспатчер самого бота, будет работать тока в isRelease
                if (d == null)
                {
                    log("Failed to get dispatcher");
                    return;
                }

                // Main.Host = this;

                d.Invoke(() =>
                {
                    try
                    {
                        MainForm = new Main();
                        MainForm.Show();

                        FormInitialized = true;
                    }
                    catch (Exception e)
                    {
                        log(e + "");
                    }
                });

                if (AdvancedLog)
                {
                    log("Диспатчер                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                while (!FormInitialized)
                {
                    Thread.Sleep(10);
                }

                if (AdvancedLog)
                {
                    log("Инициализация формы завершена                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }


                MainForm.Host = this;

                if (!LoadSettingsForQp())
                {
                    return;
                }

                if (AdvancedLog)
                {
                    log("Загрузка настроек                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }



                ApplySettings();
                if (AdvancedLog)
                {
                    log("ApplySettings                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                LoadScript();
                if (AdvancedLog)
                {
                    log("LoadScript                              " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                if (ClientType == EWoWClient.Retail)
                {
                    LoadQuest();
                    if (AdvancedLog)
                    {
                        log("LoadQuest                              " + sw.ElapsedMilliseconds + " мс");
                        sw.Restart();
                    }
                }



                MainForm.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (ClientType == EWoWClient.Classic)
                        {
                            MainForm.ComboBoxAukTime.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            MainForm.ComboBoxAukTimeClassic.Visibility = Visibility.Collapsed;
                        }
                        MainForm.Main1.Title = Me?.Name + " " + GetCurrentAccount().ServerName + " " + Version + " " + isReleaseVersion;
                        if (CharacterSettings.QuesterLeft > -1)
                        {
                            MainForm.Main1.Left = CharacterSettings.QuesterLeft;
                            MainForm.Main1.Top = CharacterSettings.QuesterTop;
                        }
                    }
                    catch (Exception err)
                    {
                        log(err.ToString());
                    }
                }));
                if (AdvancedLog)
                {
                    log("Форма 2                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                CommonModule = new CommonModule();
                CommonModule.Start(this);
                if (AdvancedLog)
                {
                    log("CommonModule                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                FarmModule = new FarmModule();
                FarmModule.Start(this);
                if (AdvancedLog)
                {
                    log("FarmModule                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                AutoQuests = new AutoQuests();
                AutoQuests.Start(this);
                if (AdvancedLog)
                {
                    log("AutoQuests                              " + sw.ElapsedMilliseconds + " мс");
                    sw.Stop();
                }


                var timer = 11;
                var timerDps = 0;
                TimeInFight = 0;
                var lastLoc = Me.Location;
                var checkMoveLoc = Me.Location;
                StartGold = Me.Money;
                TimeWork = DateTime.Now;
                var checkMove = 0;

                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        //  log(item.Id + "  "+ item.Name);
                        /* if(item.GetSellPrice() == null)
                             continue;*/
                        //  Startinvgold = Startinvgold + item.GetSellPrice() * item.Count;
                    }

                }



                GetStartInventory();
                SetCTMMovement(CharacterSettings.Mode == Mode.Questing);
                if (ClientType == EWoWClient.Classic)
                {
                    SetCTMMovement(true);
                }

                /* if (GetBotLogin() == "Armin")
                     SetBetaCTMMovement(true);*/


                MyExps.TimeWork = DateTime.Now;

                while (!CancelRequested)
                {
                    Thread.Sleep(100);
                    if (GameState == EGameState.LoadingGameWorld)
                    {
                        AutoQuests.IsNeedWaitAfterLoading = true;
                    }

                    timer++;
                    timerDps++;

                    if (GameState == EGameState.Offline)
                    {
                        StopPluginNow();
                    }

                    if (MainForm != null && timer > 5)
                    {
                        MainForm?.SetMe();
                        MyCheckNpc();
                        timer = 0;
                        if (GetBotLogin() == "Daredevi1")
                        {
                            CommonModule.MyDraw();
                        }
                    }

                    CheckAttack();
                    if (GameState == EGameState.Ingame && timerDps > 10)
                    {
                        if (CommonModule.InFight() || CheckInvisible())
                        {
                            TimeInFight++;
                        }

                        timerDps = 0;
                    }

                    if (MainForm != null && MainForm.NeedApplySettings)
                    {
                        ApplySettings();
                    }

                    if (MainForm != null && MainForm.NeedApplyDungeonSettings)
                    {
                        ApplyDungeonSettings();
                    }

                    if (MainForm != null && MainForm.NeedApplyQuestSettings)
                    {
                        ApplyQuestSettings();
                    }

                    if (ClientAfk)
                    {
                        Jump();
                    }

                    if (Me.StandState == EStandState.Sit && MainForm.On)
                    {
                        if (!MyIsNeedRegen() && CharacterSettings.UseRegen)
                        {
                            ChangeStandState(EStandState.Stand);
                        }
                    }

                    if (MainForm.On)
                    {
                        CheckCount++;
                    }
                    else
                    {
                        CheckCount = 0;
                    }

                    if (FarmModule.BestMob != null && Me.IsInCombat)
                    {
                        if (!CharacterSettings.PikPocket)
                        {
                            NeedWaitAfterCombat = true;
                        }
                    }


                    if (CommonModule.InFight())
                    {
                        CheckCount = 0;
                    }

                    if (Me.IsInFlight)
                    {
                        CheckCount = 0;
                    }

                    if (Me != null && CharacterSettings.Mode != Mode.QuestingClassic)
                    {
                        if (Me.IsMoving)
                        {
                            checkMove++;
                            //   log("Move " +checkMove );
                            if (checkMove > 30)
                            {
                                if (Me.Distance(checkMoveLoc) < 3)
                                {
                                    log("Застрял, нет передвижения более 30 секунд ", LogLvl.Error);
                                    CancelMoveTo();

                                }
                                checkMoveLoc = Me.Location;
                                checkMove = 0;
                            }


                            CheckCount = 0;
                        }
                        else
                        {
                            checkMove = 0;
                        }

                        if (SpellManager.IsCasting)
                        {
                            CheckCount = 0;
                        }

                        foreach (var aura in Me.GetAuras())
                        {
                            if (aura.SpellId != 15007)
                            {
                                continue;
                            }

                            CheckCount = 0;
                            break;
                        }
                    }

                    if (AutoQuests.WaitTeleport)
                    {
                        CheckCount = 0;
                    }

                    if (CharacterSettings.Mode == Mode.FarmResource)
                    {
                        CheckCount = 0;
                    }

                    if (CheckCount > 3200)
                    {
                        if (Me.Distance(lastLoc) < 3)
                        {
                            log("Застрял, нет передвижения более 5 минут. Перезапускаю ", LogLvl.Error);
                            TerminateGameClient();
                            EventInactiveCount++;
                            if (EventInactiveCount > 1)
                            {
                                EventInactive = true;
                            }
                        }
                        lastLoc = Me.Location;
                        CheckCount = 0;

                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { log("Ошибка запуска " + err); }
            finally { log("Main Stop"); }
        }

        public void PluginStop()
        {
            try
            {
                MyDelBigObstacle(true);
                foreach (var myObstacleDic in DicObstaclePic)
                {
                    log("Удаляю обстакл " + myObstacleDic.Value.Id);
                    RemoveObstacle(myObstacleDic.Value.Id);
                }
            }
            catch (Exception e) { log(e.ToString()); }

            try
            {
                SetMoveStateForClient(false);
                SetCurrentTurnPoint(Vector3F.Zero, false);
                CancelRequested = true;
                Thread.Sleep(500);
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { log(err.ToString()); }

            try
            {
                CommonModule?.Stop();
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { log(err.ToString()); }

            try
            {
                FarmModule?.Stop();
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { log(err.ToString()); }

            try
            {
                AutoQuests?.Stop();
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { log(err.ToString()); }


            try
            {
                MainForm?.Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        MainForm?.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
            catch (TaskCanceledException) { }
            catch (ThreadAbortException) { }
            catch (Exception err) { log(err.ToString()); }

            try
            {
                /*   if (!isReleaseVersion)
                        FormThread?.Abort();*/
                log("Aborted!");
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log(err.ToString());
            }
            finally
            {
                if (Me != null)
                {
                    CancelMoveTo();
                }
            }


        }

        public void StopPluginNow()
        {
            try
            {
                var indexOfChar = _path.IndexOf(Ch, StringComparison.Ordinal);
                _path = indexOfChar > 0 ? _path.Substring(indexOfChar) : "Quester";
                log(_path + "\\WowAI.dll");
                //StopPlugin(_path + "\\WowAI.dll");
                CancelRequested = true;
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { log(err.ToString()); }
        }

        internal static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private void LoadQuest()
        {
            QuestName = PathQuestSet + CharacterSettings.Quest;
            var doc = new XmlDocument();
            try
            {
                doc.Load(QuestName);
                log("Загружаю квест из файла: " + QuestName, LogLvl.Ok);
            }
            catch { log("Не получилось загрузить квест: " + QuestName, LogLvl.Error); }
        }

        private void LoadScript()
        {
            ScriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\" + CharacterSettings.Script;
            var doc = new XmlDocument();
            try
            {
                doc.Load(ScriptName);
                log("Загружаю скрипт из файла: " + ScriptName, LogLvl.Ok);
            }
            catch { log("Не получилось загрузить скрипт: " + ScriptName, LogLvl.Error); }
        }

        public bool LoadSettingsForQp(string cfg = "")
        {
            try
            {
                MainForm.NeedUpdate = true;
                if (cfg != "")
                {
                    if (isReleaseVersion)
                    {
                        CfgName = AssemblyDirectory + "\\Configs\\Default\\" + cfg + ".json";
                    }
                    else
                    {
                        CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\Default\\" + cfg + ".json";
                    }

                    CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);

                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                    FileName = cfg + ".json";
                    return true;
                }

                if (Me != null)
                {
                    if (isReleaseVersion)
                    {
                        CfgName = AssemblyDirectory + "\\Configs\\" + Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
                    }
                    else
                    {
                        CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\" + Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
                    }
                }

                if (File.Exists(CfgName))
                {
                    CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                    FileName = Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
                    return true;
                }
                else
                {
                    var path = AssemblyDirectory + "\\Configs\\Default\\";
                    if (!isReleaseVersion)
                    {
                        path = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\Default\\";
                    }
                    if (Directory.Exists(path))
                    {
                        var dir = new DirectoryInfo(path);
                        foreach (var file in dir.GetFiles())
                        {
                            if (Me != null && file.Name.Contains(Me.Class.ToString()))
                            {
                                if (!file.Name.Contains("Level"))
                                {
                                    continue;
                                }
                                var index = file.Name.IndexOf("[", StringComparison.Ordinal) + 1;
                                var lastIndex = file.Name.LastIndexOf("-", StringComparison.Ordinal);
                                var leght = lastIndex - index;
                                // log(index + " до " + lastIndex);
                                var startLevel = Convert.ToInt32(file.Name.Substring(index, leght));
                                index = file.Name.IndexOf("-", StringComparison.Ordinal) + 1;
                                lastIndex = file.Name.LastIndexOf("]", StringComparison.Ordinal);
                                leght = lastIndex - index;
                                // log(index + " до " + lastIndex);
                                var endLevel = Convert.ToInt32(file.Name.Substring(index, leght));
                                // log(startLevel + " до " + endLevel);
                                if (Me.Level >= startLevel && Me.Level <= endLevel)
                                {
                                    StartLevel = startLevel;
                                    EndLevel = endLevel;
                                    if (isReleaseVersion)
                                    {
                                        CfgName = AssemblyDirectory + "\\Configs\\Default\\" + file.Name;
                                    }
                                    else
                                    {
                                        CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\Default\\" + file.Name;
                                    }
                                    CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                                    FileName = file.Name;
                                    return true;
                                }
                            }
                        }



                        foreach (var file in dir.GetFiles())
                        {
                            if (Me != null && (file.Name.Contains(Me.Class.ToString())&& file.Name.Contains("json") && !file.Name.Contains("Level")))
                            {
                                if (isReleaseVersion)
                                {
                                    CfgName = AssemblyDirectory + "\\Configs\\Default\\" + file.Name;
                                }
                                else
                                {
                                    CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\Default\\" + file.Name;
                                }

                                CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                                log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                                FileName = file.Name;
                                return true;
                            }
                        }

                        if (Me.Level < 21)
                        {
                            path = AssemblyDirectory + "\\Configs\\QuestDefault\\";
                            if (!isReleaseVersion)
                            {
                                path = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\QuestDefault\\";
                            }
                            dir = new DirectoryInfo(path);

                            foreach (var file in dir.GetFiles())
                            {
                                if (Me != null && file.Name.Contains(Me.Class.ToString()) && file.Name.Contains(Me.Race.ToString()) && !file.Name.Contains("Level") && file.Name.Contains("json"))
                                {
                                    if (isReleaseVersion)
                                    {
                                        CfgName = AssemblyDirectory + "\\Configs\\QuestDefault\\" + file.Name;
                                    }
                                    else
                                    {
                                        CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\QuestDefault\\" + file.Name;
                                    }
                                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                                    CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);

                                    FileName = file.Name;
                                    return true;
                                }
                            }
                        }
                    }


                    log("Не получилось загрузить настройки: " + CfgName + " Загружаю стандартные", LogLvl.Important);


                    if (isReleaseVersion)
                    {
                        CfgName = AssemblyDirectory + "\\Configs\\Default\\Default.json";
                    }
                    else
                    {
                        CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\Default\\Default.json";
                    }

                    CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                    FileName = "Default.xml";
                    return true;
                }
            }
            catch (Exception err)
            {
                log(err.ToString());
                return false;
            }
        }

        private void ApplyQuestSettings()
        {
            try
            {
                if (CharacterSettings.Quest == "" || CharacterSettings.Quest.Contains("Не выбрано"))
                {
                    return;
                }

                QuestName = PathQuestSet + CharacterSettings.Quest;
                log("Применяю квест: " + QuestName, LogLvl.Ok);
                var reader = new XmlSerializer(typeof(QuestSetting));
                using (var fs = File.Open(QuestName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    QuestSettings = (QuestSetting)reader.Deserialize(fs);
                }

                MainForm.InitFromQuestSettings();
                MainForm.NeedApplyQuestSettings = false;
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log("Ошибка загрузки: " + err);
            }
        }

        private void ApplyDungeonSettings()
        {
            try
            {
                if (CharacterSettings.Script == "" || CharacterSettings.Script.Contains("Не выбрано"))
                {
                    return;
                }

                ScriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\" + CharacterSettings.Script;
                log("Применяю скрипт: " + ScriptName, LogLvl.Ok);
                var reader = new XmlSerializer(typeof(DungeonSetting));
                using (var fs = File.Open(ScriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                }

                MainForm.InitFromDungeonSettings();
                MainForm.NeedApplyDungeonSettings = false;
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log("Ошибка загрузки: " + err);
            }
        }

        public void ApplySettings()
        {
            try
            {
                log("Применяю настройки: " + CfgName, LogLvl.Ok);
                CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                MainForm.InitFromSettings();
                MainForm.NeedApplySettings = false;
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log("Ошибка загрузки: " + err);
            }
            ApplyDungeonSettings();
            ApplyQuestSettings();
            FarmModule?.LoadSkill();
        }
    }
}