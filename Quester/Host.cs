using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using WowAI.ComboRoutes;
using WowAI.Modules;
using WowAI.UI;
using System.Diagnostics;
using System.Text;
using WoWBot.Core;
using Out.Internal.Core;
using Newtonsoft.Json.Linq;
using Out.Utility;


namespace WowAI
{
    internal partial class Host : Core
    {
        // ReSharper disable once InconsistentNaming
        public static bool isReleaseVersion = false;

        public Random RandGenerator { get; private set; }
        private const string Version = "v0.18";
        private bool _cfgLoaded;
        public string CfgName = "";
        public string ScriptName = "";
        public string QuestName = "";

        private string _path = AssemblyDirectory;
        private const string Ch = "Quester\\";
        public uint CanSpellAttack = 6603;

        //DPS
        public long TimeInFight;
        public long AllDamage;
        public long KillMobsCount;
        public DateTime TimeWork;
        public int CheckCount;
        public double StartGold;
        public long Startinvgold;

        public enum LogLvl
        {
            Ok, Error, Info, Important
        }



        // ReSharper disable once InconsistentNaming
        public void log(string text, LogLvl type = LogLvl.Info)
        {
            if (CharacterSettings != null)
                if (!CharacterSettings.LogAll)
                    return;

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
                //  if (color == Color.Red)
                //     Log(DateTime.Now.ToString("hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture) + ":   " + Me.Name + "(" + Me.Level + ")" + "   " + text, color, GetCurrentAccount().Name + " Ошибки");

                Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Me.Name + "(" + Me.Level + ")  " + FarmModule?.farmState + "   " /*+ Me.IsAFK + "  " + Me.IsDND + "   " */+ text, color, GetCurrentAccount().Name);
            }

            else
                Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + "Character offline: " + "   " + text, color, GetCurrentAccount().Name);
        }

        private bool _cancelRequested;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once ConvertToAutoProperty
        internal bool cancelRequested
        {
            get { return _cancelRequested; }
            set { _cancelRequested = value; }
        }

        public CharacterSettings CharacterSettings { get; set; } = new CharacterSettings();
        internal DungeonSetting DungeonSettings { get; set; }
        internal QuestSetting QuestSettings { get; set; }
        internal QuestStates QuestStates { get; set; }

        internal DropBases DropBases { get; set; } = new DropBases();
        internal MonsterGroup2 MonsterGroup { get; set; } = new MonsterGroup2();

        internal Main MainForm { get; set; }
        internal ComboRoute ComboRoute { get; set; }
        internal CommonModule CommonModule { get; set; }
        internal FarmModule FarmModule { get; set; }
        internal AutoQuests AutoQuests { get; set; }
        // private Thread FormThread { get; set; }
        public bool FormInitialized { get; set; }
        public MyNpcLocs MyNpcLocss { get; set; } = new MyNpcLocs();
        public MyGameObjectLocs MyGameObjectLocss { get; set; } = new MyGameObjectLocs();

        public bool NeedWaitAfterCombat;



        public void ChangeAccount()
        {
            log("Необходима замена");
            var path = AssemblyDirectory + "\\NewLogin.txt";
            if (File.Exists(path))
            {

                Thread.Sleep(RandGenerator.Next(100, 10000));
                var lines = File.ReadAllLines(path);
                var delim = new[] { ';', ':' };
                string[] inpstr;
                if (lines.Length != 0)
                {
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] != "")
                        {
                            inpstr = lines[i].Split(delim);
                            log("Заменяю " + GetCurrentAccount().Login + " на " + inpstr[0]);
                            File.AppendAllText(AssemblyDirectory + "\\vadlog.txt",
                             DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + @":   Заменяю " + GetCurrentAccount().Login + @":" + GetCurrentAccount().Password + @" на " + inpstr[0] + @":" + inpstr[1] + Environment.NewLine);
                            GetCurrentAccount().Login = inpstr[0];
                            GetCurrentAccount().Password = inpstr[1];
                            lines[i] = string.Empty;
                            File.WriteAllLines(path, lines);
                            GetCurrentAccount().IsAutoLaunch = true;
                            GetCurrentAccount().DoLogin();
                            break;
                        }
                    }
                }
                else
                {
                    log("Закончились аккаунты");
                    GetCurrentAccount().IsAutoLaunch = false;
                }
            }
            else
            {
                log("Файл " + path + " не найден");
                GetCurrentAccount().IsAutoLaunch = false;
            }
        }

        public static string RandomString(int size)
        {
            var sb = new StringBuilder(size);
            var rnd = new Random(DateTime.Now.Millisecond);
            for (var i = 0; i < size; i++)
                sb.Append((char)rnd.Next(97, 123));
            return sb.ToString();
        }

        public string PathNpCjson = "";
        public string PathGameObjectLocs = "";
        public string PathNpCjsonCopy = "";
        public string PathDropjson = "";
        public string PathQuestSet = "";
        public string PathQuestState = "";
        public string PathMonsterGroup = "";
        public uint StartExp;
        public bool AdvancedLog;
        public string PathGps;
        // private Thread formThread { get; set; }
        public class MyQuestBase
        {
            public List<MyQuestBaseItem> MyQuestBases = new List<MyQuestBaseItem>();
        }

        public class MyQuestBaseItem
        {
            public uint Id = 0;
            public uint Level = 0;
            public uint RequiresLevel = 0;
            public uint Side = 0;
            public List<uint> Race = new List<uint>();
            public List<uint> Class = new List<uint>();
            public MyQuestStart QuestStart = new MyQuestStart();
            public MyQuestEnd QuestEnd = new MyQuestEnd();
            public uint PreviousQuest = 0;
        }

        public class MyQuestStart
        {
            public uint QuestStartId = 0;
            public MyUnitType QuestStarType = MyUnitType.Unknown;
        }

        public class MyQuestEnd
        {
            public uint QuestEndId = 0;
            public MyUnitType QuestEndType = MyUnitType.Unknown;
        }

        public enum MyUnitType
        {
            Unknown = -1,
            Item = 0,
            Unit = 1,
            GameObject = 2
        }
        public MyQuestBase MyQuestBases = new MyQuestBase();
        // ReSharper disable once UnusedMember.Global
        public void PluginRun()
        {
            try
            {
                if (GetBotLogin() == "Daredevi1")
                    AdvancedLog = true;

                /* var pathItem = "C:\\AllQuestItem" + ClientType + ".txt";
                 var pathQuest = "C:\\AllQuest" + ClientType + ".txt";
                 if (File.Exists(pathItem))
                     File.Delete(pathItem);
                 if (File.Exists(pathQuest))
                     File.Delete(pathQuest);
                 foreach (var gameDbQuestTemplate in GameDB.QuestTemplates)
                 {
                     if (gameDbQuestTemplate.Value.QuestObjectives != null)
                         foreach (var valueQuestObjective in gameDbQuestTemplate.Value.QuestObjectives)
                         {
                             if (valueQuestObjective.Type == EQuestRequirementType.Item)
                             {
                                 File.AppendAllText(pathItem, valueQuestObjective.ObjectID + Environment.NewLine);
                             }
                         }
                     File.AppendAllText(pathQuest, gameDbQuestTemplate.Value.Id + Environment.NewLine);
                 }*/

                ClearLogs(GetCurrentAccount().Name);
                RandGenerator = new Random((int)DateTime.Now.Ticks);
                while (GameState != EGameState.Ingame)
                {
                    log("Ожидаю вход в игру... Status: " + GameState);
                    Thread.Sleep(5000);
                }
                var sw = new Stopwatch();
                if (AdvancedLog)
                    sw.Start();

                FormInitialized = false;

                if (!Directory.Exists(AssemblyDirectory + "\\Configs"))
                    Directory.CreateDirectory(AssemblyDirectory + "\\Configs");

                if (!Directory.Exists(AssemblyDirectory + "\\Configs\\Default"))
                    Directory.CreateDirectory(AssemblyDirectory + "\\Configs\\Default");

                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");

                if (!Directory.Exists(AssemblyDirectory + "\\Log\\"))
                    Directory.CreateDirectory(AssemblyDirectory + "\\Log\\");


                if (isReleaseVersion)
                    PathQuestSet = AssemblyDirectory + "\\Quest\\";
                else
                    PathQuestSet = AssemblyDirectory + "\\Plugins\\Quester\\Quest\\";

                if (isReleaseVersion)
                    PathGps = AssemblyDirectory + "\\helpGps.db3";
                else
                    PathGps = AssemblyDirectory + "\\Plugins\\Quester\\helpGps.db3";

                if (!Directory.Exists(PathQuestSet))
                    Directory.CreateDirectory(PathQuestSet);



                if (isReleaseVersion)
                    PathQuestState = AssemblyDirectory + "\\QuestState\\";
                else
                    PathQuestState = AssemblyDirectory + "\\Plugins\\Quester\\QuestState\\";

                if (!Directory.Exists(PathQuestState))
                    Directory.CreateDirectory(PathQuestState);


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

                var PathQuestjson = "";

                if (ClientType == EWoWClient.Retail)
                {
                    if (isReleaseVersion)
                        PathQuestjson = AssemblyDirectory + "\\MyQuestBase.json";
                    else
                        PathQuestjson = AssemblyDirectory + "\\Plugins\\Quester\\MyQuestBase.json";
                }

                if (ClientType == EWoWClient.Classic)
                {
                    if (isReleaseVersion)
                        PathQuestjson = AssemblyDirectory + "\\MyQuestBaseClassic.json";
                    else
                        PathQuestjson = AssemblyDirectory + "\\Plugins\\Quester\\MyQuestBaseClassic.json";
                }


                if (ClientType == EWoWClient.Retail)
                {
                    if (isReleaseVersion)
                        PathNpCjson = AssemblyDirectory + "\\npc.json";
                    else
                        PathNpCjson = AssemblyDirectory + "\\Plugins\\Quester\\npc.json";
                }

                if (ClientType == EWoWClient.Classic)
                {
                    if (isReleaseVersion)
                        PathNpCjson = AssemblyDirectory + "\\npcClassic.json";
                    else
                        PathNpCjson = AssemblyDirectory + "\\Plugins\\Quester\\npcClassic.json";
                }

                if (isReleaseVersion)
                    PathNpCjsonCopy = AssemblyDirectory + "\\npcCopy.json";
                else
                    PathNpCjsonCopy = AssemblyDirectory + "\\Plugins\\Quester\\npcCopy.json";

                if (isReleaseVersion)
                    PathMonsterGroup = AssemblyDirectory + "\\monstergroup.json";
                else
                    PathMonsterGroup = AssemblyDirectory + "\\Plugins\\Quester\\monstergroup.json";


                if (ClientType == EWoWClient.Retail)
                {
                    if (isReleaseVersion)
                        PathDropjson = AssemblyDirectory + "\\drop.json";
                    else
                        PathDropjson = AssemblyDirectory + "\\Plugins\\Quester\\drop.json";
                }
                if (ClientType == EWoWClient.Classic)
                {
                    if (isReleaseVersion)
                        PathDropjson = AssemblyDirectory + "\\MyDropBasesClassic.json";
                    else
                        PathDropjson = AssemblyDirectory + "\\Plugins\\Quester\\MyDropBasesClassic.json";
                }

                if (ClientType == EWoWClient.Classic)
                {
                    if (isReleaseVersion)
                        PathGameObjectLocs = AssemblyDirectory + "\\GameObjectClassic.json";
                    else
                        PathGameObjectLocs = AssemblyDirectory + "\\Plugins\\Quester\\GameObjectClassic.json";
                }

                if (ClientType == EWoWClient.Retail)
                {
                    if (isReleaseVersion)
                        PathGameObjectLocs = AssemblyDirectory + "\\GameObject.json";
                    else
                        PathGameObjectLocs = AssemblyDirectory + "\\Plugins\\Quester\\GameObject.json";
                }

                if (File.Exists(PathGameObjectLocs))
                    MyGameObjectLocss = (MyGameObjectLocs)ConfigLoader.LoadConfig(PathGameObjectLocs, typeof(MyGameObjectLocs), MyGameObjectLocss);
                else
                    log("Не найден файл PathNpCjson " + PathNpCjson + "  " + ClientType, LogLvl.Error);

                if (File.Exists(PathNpCjson))
                    MyNpcLocss = (MyNpcLocs)ConfigLoader.LoadConfig(PathNpCjson, typeof(MyNpcLocs), MyNpcLocss);
                else
                    log("Не найден файл PathNpCjson " + PathNpCjson + "  " + ClientType, LogLvl.Error);

                if (AdvancedLog)
                {
                    log("НПС загружены за                              " + sw.ElapsedMilliseconds + " мс всего: " + MyNpcLocss.NpcLocs.Count + " шт.");
                    sw.Restart();
                }

                if (File.Exists(PathDropjson))
                    DropBases = (DropBases)ConfigLoader.LoadConfig(PathDropjson, typeof(DropBases), DropBases);
                else
                    log("Не найден файл PathDropjson " + PathDropjson, LogLvl.Error);

                if (AdvancedLog)
                {
                    log("Drop загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + DropBases.Drop.Count + " шт.");
                    sw.Restart();
                }

                if (File.Exists(PathMonsterGroup))
                    MonsterGroup =
                        (MonsterGroup2)ConfigLoader.LoadConfig(PathMonsterGroup, typeof(MonsterGroup2), MonsterGroup);
                else
                    log("Не найден файл PathMonsterGroup " + PathMonsterGroup, LogLvl.Error);

                if (AdvancedLog)
                {
                    log("MonsterGroup загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + MonsterGroup.MonsterGroups.Count + " шт.");
                    sw.Restart();
                }

                if (File.Exists(PathQuestjson))
                    MyQuestBases =
                        (MyQuestBase)ConfigLoader.LoadConfig(PathQuestjson, typeof(MyQuestBase), MyQuestBases);
                else
                    log("Не найден файл PathQuestjson " + PathQuestjson + ClientType, LogLvl.Error);

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
                    QuestStates = (QuestStates)ConfigLoader.LoadConfig(
                        PathQuestState + Me.Name + "[" + GetCurrentAccount().ServerName + "].json", typeof(QuestStates),
                        QuestStates);



                /*   if (!isReleaseVersion)
                   {
                       formThread = new Thread(() =>
                       {
                           try
                           {
                               SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                               MainForm = new Main();
                               MainForm.Show();
                               FormInitialized = true;
                               Dispatcher.Run();

                           }
                           catch (TaskCanceledException) { }
                           catch (ThreadAbortException) { }
                           catch (Exception error)
                           {
                               Log(error.ToString());
                           }
                       });
                       formThread.SetApartmentState(ApartmentState.STA);
                     //  formThread.IsBackground = true;
                       formThread.Start();
                   }
                   else
                   {
                       var d = GetMainDispatcher();
                       if (d == null)
                       {
                           Log("Failed to get dispatcher");
                           return;
                       }
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
                   }*/



                var d = GetMainDispatcher(); //получает диспатчер самого бота, будет работать тока в isRelease
                if (d == null)
                {
                    log("Failed to get dispatcher");
                    return;
                }

                //Main.Host = this;
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
                    Thread.Sleep(10);

                if (AdvancedLog)
                {
                    log("Инициализация формы завершена                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }

                MainForm.Host = this;


                if (!LoadSettingsForQp())
                    return;
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

                LoadQuest();
                if (AdvancedLog)
                {
                    log("LoadQuest                              " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }
                /*   MainForm.NeedApplyDungeonSettings = true;
                   MainForm.NeedApplyQuestSettings = true;*/

                MainForm.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            MainForm.Main1.Title = Me?.Name + " " + GetCurrentAccount().ServerName + " " + Version + " " + isReleaseVersion;
                            // if (!CharacterSettings.HideQuesterUi)
                            //  {
                            MainForm.Main1.WindowState = WindowState.Normal;
                            if (CharacterSettings.QuesterLeft > -1)
                            {
                                MainForm.Main1.Left = CharacterSettings.QuesterLeft;
                                MainForm.Main1.Top = CharacterSettings.QuesterTop;
                            }
                            //  }
                        }
                        catch (Exception err)
                        {
                            log(err.ToString());
                        }
                    }
                ));
                if (AdvancedLog)
                {
                    log("Форма 2                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Restart();
                }



                CommonModule = new CommonModule();
                CommonModule.Start(this);
                FarmModule = new FarmModule();
                FarmModule.Start(this);
                AutoQuests = new AutoQuests();
                AutoQuests.Start(this);



                if (AdvancedLog)
                {
                    log("Запуск потоков                               " + sw.ElapsedMilliseconds + " мс");
                    sw.Stop();
                }


                var timer = 0;
                var timer2 = 0;
                var timerDps = 0;
                TimeInFight = 0;

                float lastX = 0;
                float lastY = 0;
                float lastZ = 0;
                if (Me != null)
                {
                    lastX = Me.Location.X;
                    lastY = Me.Location.Y;
                    lastZ = Me.Location.Z;
                }

                if (Me != null)
                {
                    StartGold = Me.Money;
                    StartExp = Me.Exp;
                }


                TimeWork = DateTime.Now;

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





                if (Me.Name == "")
                {
                    log("Нет ника, перезапустите клиент");
                    MainForm.On = false;
                }
                GetStartInventory();
                if (Me.Level == 110 && CharacterSettings.Mode == EMode.Questing)
                {
                    SendKeyPress(0x1b);
                }

                // CommonModule.MoveTo(1636.14, -4445.57, 17.04);
                //  GetUpdateInventory();
                if (CharacterSettings.Mode == EMode.Questing)
                    SetCTMMovement(true);

                if (ClientType == EWoWClient.Classic)
                {
                    if (Me.Class == EClass.Hunter && Me.GetPet() != null)
                        SetCTMMovement(true);
                }







                //Jump();
                while (!cancelRequested)
                {
                    Thread.Sleep(100);

                    //    if(/*ClientAfk ||*/ Me.IsAFK || Me.IsDND)
                    //      log("ClientAfk: " /*+ ClientAfk */ + "  IsAFK: " +  Me.IsAFK + "  IsDND: " + Me.IsDND, LogLvl.Error);

                    if (GameState == EGameState.LoadingGameWorld)
                        AutoQuests.IsNeedWaitAfterLoading = true;


                    /*  if (Me.Target != null)
                      {
                          log(Me.Target.GetAuras().Count + "");
                          foreach (var aura in Me.Target.GetAuras())
                          {
                              log(aura.SpellId + "  " );
                          }
                      }*/


                    timer++;
                    timer2++;
                    timerDps++;

                    if (GameState == EGameState.Offline)
                        StopPluginNow();

                    if (MainForm != null && timer > 30 && Me != null /*&& !MainForm.IsToggle*/)
                    {
                        if (timer2 > 100)
                        {
                            //GetUpdateInventory();
                            //CompInv();
                            timer2 = 0;
                        }
                        MainForm?.SetMe();
                        //  MainForm.Draw();
                        MyCheckNPC();

                        timer = 0;
                    }

                    if (GameState == EGameState.Ingame && timerDps > 10)
                    {
                        if (CommonModule.InFight())
                            TimeInFight++;
                        timerDps = 0;
                    }
                    if (MainForm != null && MainForm.NeedApplySettings)
                        ApplySettings();
                    if (MainForm != null && MainForm.NeedApplyDungeonSettings)
                        ApplyDungeonSettings();
                    if (MainForm != null && MainForm.NeedApplyQuestSettings)
                        ApplyQuestSettings();



                    if (MainForm.On)
                        CheckCount++;
                    else
                        CheckCount = 0;

                    if (FarmModule.BestMob != null)
                        NeedWaitAfterCombat = true;

                    if (CommonModule.InFight())
                    {
                        CheckCount = 0;

                    }


                    if (Me != null)
                    {
                        if (Me.IsMoving)
                            CheckCount = 0;
                        if (SpellManager.IsCasting)
                            CheckCount = 0;
                        foreach (var aura in Me.GetAuras())
                        {
                            if (aura.SpellId == 15007) //15007   
                            {
                                CheckCount = 0;
                                break;
                            }
                        }
                    }


                    if (AutoQuests.WaitTeleport)
                        CheckCount = 0;


                    if (CharacterSettings.Mode == EMode.FarmResource)//"Сбор ресурсов")
                        CheckCount = 0;




                    if (CheckCount > 2200)
                    {
                        if (Me.Distance(lastX, lastY, lastZ) < 3)
                        {
                            log("Застрял, нет передвижения более 5 минут. Перезапускаю ", LogLvl.Error);

                            if (Me.Level == 65 && CharacterSettings.Mode == EMode.Questing)//"Выполнение квестов")
                                GetCurrentAccount().IsAutoLaunch = false;
                            TerminateGameClient();


                            EventInactiveCount++;
                            if (EventInactiveCount > 1)
                                EventInactive = true;
                        }
                        lastX = Me.Location.X;
                        lastY = Me.Location.Y;
                        lastZ = Me.Location.Z;
                        CheckCount = 0;
                    }

                }


            }

            catch (ThreadAbortException)
            {

            }
            catch (Exception err)
            {

                log("Ошибка запуска " + err);
            }
            finally
            {
                log("Main Stop");
            }

        }

        public bool NeedRestart = false;
        public int EventInactiveCount;
        public bool EventInactive;




        public void PluginStop()
        {
            try
            {
                SetMoveStateForClient(false);
                SetCurrentTurnPoint(Vector3F.Zero, false);
                cancelRequested = true;
                Thread.Sleep(500);
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log(err.ToString());
            }

            try
            {
                CommonModule?.Stop();
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log(err.ToString());
            }

            try
            {
                FarmModule?.Stop();
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log(err.ToString());
            }

            try
            {
                AutoQuests?.Stop();
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log(err.ToString());
            }


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
            catch (Exception err)
            {
                log(err.ToString());
            }

            try
            {
                if (Me != null)
                {
                    CancelMoveTo();
                }
                /*   if (!isReleaseVersion)
                       FormThread?.Abort();*/
                log("Aborted!");
            }
            catch (ThreadAbortException)
            {
                if (Me != null)
                {
                    CancelMoveTo();
                }
            }
            catch (Exception err)
            {
                if (Me != null)
                {
                    CancelMoveTo();
                }
                log(err.ToString());
            }


        }

        public void StopPluginNow()
        {
            try
            {
                var indexOfChar = _path.IndexOf(Ch, StringComparison.Ordinal);
                _path = indexOfChar > 0 ? _path.Substring(indexOfChar) : "Quester";
                log(_path + "\\WowAI.dll");
                //  StopPlugin(_path + "\\WowAI.dll");
                cancelRequested = true;
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log(err.ToString());
            }
        }

        private void LoadComboRoute()
        {
            //Мой класс
            try
            {
                if (Me == null) return;
                switch (Me.Class)
                {
                    case EClass.DeathKnight:
                        ComboRoute = new DefaultComboRoute(this);
                        break;

                    default:
                        ComboRoute = new DefaultComboRoute(this);
                        break;
                }
            }
            catch (Exception err)
            {
                log(err.ToString());
            }
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


        public bool LoadQuest()
        {

            QuestName = PathQuestSet + CharacterSettings.Quest;
            // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;

            var doc = new XmlDocument();
            try
            {
                doc.Load(QuestName);
                _cfgLoaded = true;
                log("Загружаю квест из файла: " + QuestName, LogLvl.Ok);
            }
            catch
            {
                log("Не получилось загрузить квест: " + QuestName, LogLvl.Error);

            }

            return _cfgLoaded;
        }

        private bool LoadScript()
        {

            ScriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\" + CharacterSettings.Script;

            var doc = new XmlDocument();
            try
            {
                doc.Load(ScriptName);
                _cfgLoaded = true;
                log("Загружаю скрипт из файла: " + ScriptName, LogLvl.Ok);
            }
            catch
            {
                log("Не получилось загрузить скрипт: " + ScriptName, LogLvl.Error);
            }

            return _cfgLoaded;
        }

        public string FileName = "";

        private bool LoadSettingsForQp()
        {
            try
            {
                MainForm.NeedUpdate = true;

                if (Me != null)
                    if (isReleaseVersion)
                        CfgName = AssemblyDirectory + "\\Configs\\" + Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
                    else
                        CfgName = AssemblyDirectory + "\\Plugins\\Quester\\Configs\\" + Me.Name + "[" + GetCurrentAccount().ServerName + "].json";

                if (File.Exists(CfgName))
                {
                    CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                    _cfgLoaded = true;
                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                    FileName = Me.Name + "[" + GetCurrentAccount().ServerName + "].json";
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
                        // папка с файлами
                        foreach (var file in dir.GetFiles())
                        {
                            if (file.Name.Contains(Me.Class.ToString()))
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
                                _cfgLoaded = true;
                                log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                                FileName = file.Name;
                                return true;
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
                    _cfgLoaded = true;
                    log("Загружаю настройки из файла: " + CfgName, LogLvl.Ok);
                    FileName = "Default.xml";
                }
            }
            catch (Exception err)
            {
                log(err.ToString());
            }
            return _cfgLoaded;
        }

        private void ApplyQuestSettings()
        {
            try
            {

                if (CharacterSettings.Quest == "" || CharacterSettings.Quest.Contains("Не выбрано"))
                    return;
                QuestName = PathQuestSet + CharacterSettings.Quest;
                // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;
                log("Применяю квест: " + QuestName, LogLvl.Ok);

                var reader = new XmlSerializer(typeof(QuestSetting));
                using (var fs = File.Open(QuestName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    QuestSettings = (QuestSetting)reader.Deserialize(fs);

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
                    return;
                ScriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\" + CharacterSettings.Script;
                // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;
                log("Применяю скрипт: " + ScriptName, LogLvl.Ok);

                var reader = new XmlSerializer(typeof(DungeonSetting));
                using (var fs = File.Open(ScriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    DungeonSettings = (DungeonSetting)reader.Deserialize(fs);

                MainForm.InitFromDungeonSettings();
                MainForm.NeedApplyDungeonSettings = false;




            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log("Ошибка загрузки: " + err);
            }
        }




        internal static class ConfigLoader
        {
            public static object LoadConfig(string path, Type targetType, object def)
            {
                if (!File.Exists(path))
                    File.WriteAllText(path, JObject.FromObject(def).ToString());

                var obj = JObject.Parse(File.ReadAllText(path)).ToObject(targetType);
                // (obj as CharacterSettings).InitAfterLoad();
                return obj;
            }

            public static void SaveConfig(string path, object def)
            {
                File.WriteAllText(path, JObject.FromObject(def).ToString());
            }
        }

        private void ApplySettings()
        {
            try
            {
                log("Применяю настройки: " + CfgName, LogLvl.Ok);

                /*var reader = new XmlSerializer(typeof(CharacterSettings));
                                using (var fs = File.Open(CfgName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                    CharacterSettings = (CharacterSettings)reader.Deserialize(fs);*/
                CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(CfgName, typeof(CharacterSettings), CharacterSettings);
                LoadComboRoute();

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
        }
    }
}