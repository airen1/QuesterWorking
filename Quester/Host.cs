using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
using System.Windows.Data;
using Out.Utility;


namespace WowAI
{
    internal partial class Host : Core
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once InconsistentNaming
        public static bool isReleaseVersion = false;

        public Random RandGenerator { get; private set; }
        private const string Version = "v0.14";
        private bool _cfgLoaded;
        public string CfgName = "";
        public string ScriptName = "";
        public string QuestName = "";
        public string DacandaName = "";
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

        public bool NeedWaitAfterCombat;

        public List<Vector3F> CordDacanda = new List<Vector3F>();

        public void ChangeAccount()
        {
            log("Необходима замена");
            var path = AssemblyDirectory + "\\NewLogin.txt";
            if (File.Exists(path))
            {

                Thread.Sleep(RandGenerator.Next(100, 10000));
                var lines = File.ReadAllLines(path);
                var delim = new char[] { ';', ':' };
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
                             DateTime.Now.ToString("hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture) + ":   Заменяю " + GetCurrentAccount().Login + ":" + GetCurrentAccount().Password + " на " + inpstr[0] + ":" + inpstr[1] + Environment.NewLine);
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
            StringBuilder sb = new StringBuilder(size);
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < size; i++)
                sb.Append((char)rnd.Next(97, 123));
            return sb.ToString();
        }

        public string PathNpCjson = "";
        public string PathDropjson = "";
        public string PathQuestSet = "";
        public string PathQuestState = "";
        public string PathMonsterGroup = "";
        public uint StartExp;
        public bool AdvancedLog = false;


        // ReSharper disable once UnusedMember.Global
        public void PluginRun()
        {
            try
            {
                if (GetBotLogin() == "Daredevi1")
                {
                    AdvancedLog = true;
                }

                /* foreach (var gameDbQuestTemplate in GameDB.QuestTemplates)
                 {
                     if (gameDbQuestTemplate.Value.QuestObjectives == null)
                         continue;
                     foreach (var valueQuestObjective in gameDbQuestTemplate.Value.QuestObjectives)
                     {
                         if (valueQuestObjective.Type != EQuestRequirementType.Item)
                             continue;
                        
                         File.AppendAllText("D:\\test1.txt", valueQuestObjective.ObjectID.ToString() + Environment.NewLine);
                         break;
                     }

                 }*/


                ClearLogs(GetCurrentAccount().Name);
                RandGenerator = new Random((int)DateTime.Now.Ticks);
                while (GameState != EGameState.Ingame)
                {
                    log("Ожидаю вход в игру... Status: " + GameState);
                    Thread.Sleep(5000);
                }
                //Thread.Sleep(2000);
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

                if (!Directory.Exists(AssemblyDirectory + "\\Log"))
                    Directory.CreateDirectory(AssemblyDirectory + "\\Log");


                if (isReleaseVersion)
                    PathQuestSet = AssemblyDirectory + "\\Quest\\";
                else
                {
                    PathQuestSet = AssemblyDirectory + "\\Plugins\\Quester\\Quest\\";
                }

                if (!Directory.Exists(PathQuestSet))
                    Directory.CreateDirectory(PathQuestSet);

                if (!Directory.Exists(PathQuestSet))
                    Directory.CreateDirectory(PathQuestSet);

                if (isReleaseVersion)
                    PathQuestState = AssemblyDirectory + "\\QuestState\\";
                else
                {
                    PathQuestState = AssemblyDirectory + "\\Plugins\\Quester\\QuestState\\";
                }

                if (!Directory.Exists(PathQuestState))
                    Directory.CreateDirectory(PathQuestState);


                DateTime lastChanged = File.GetLastWriteTime(AssemblyDirectory + "\\WowAI.dll");

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

                if (isReleaseVersion)
                    PathNpCjson = AssemblyDirectory + "\\npc.json";
                else
                {
                    PathNpCjson = AssemblyDirectory + "\\Plugins\\Quester\\npc.json";
                }

                if (isReleaseVersion)
                    PathMonsterGroup = AssemblyDirectory + "\\monstergroup.json";
                else
                {
                    PathMonsterGroup = AssemblyDirectory + "\\Plugins\\Quester\\monstergroup.json";
                }

                if (isReleaseVersion)
                    PathDropjson = AssemblyDirectory + "\\drop.json";
                else
                {
                    PathDropjson = AssemblyDirectory + "\\Plugins\\Quester\\drop.json";
                }

                if (File.Exists(PathNpCjson))
                {
                    MyNpcLocss = (MyNpcLocs)ConfigLoader.LoadConfig(PathNpCjson, typeof(MyNpcLocs), MyNpcLocss);
                }
                else
                {
                    log("Не найден файл " + PathNpCjson, LogLvl.Error);
                }

                if (AdvancedLog)
                {
                    log("НПС загружены за                              " + sw.ElapsedMilliseconds + " мс всего: " + MyNpcLocss.NpcLocs.Count + " шт.");
                    sw.Restart();
                }

                if (File.Exists(PathDropjson))
                {
                    DropBases = (DropBases)ConfigLoader.LoadConfig(PathDropjson, typeof(DropBases), DropBases);
                }
                else
                {
                    log("Не найден файл " + PathDropjson, LogLvl.Error);
                }



                if (AdvancedLog)
                {

                    log("Drop загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + DropBases.Drop.Count + " шт.");
                    sw.Restart();
                }

                if (File.Exists(PathMonsterGroup))
                {
                    MonsterGroup = (MonsterGroup2)ConfigLoader.LoadConfig(PathMonsterGroup, typeof(MonsterGroup2), MonsterGroup);
                }
                else
                {
                    log("Не найден файл " + PathMonsterGroup, LogLvl.Error);
                }



                if (AdvancedLog)
                {
                    log("MonsterGroup загружен за                              " + sw.ElapsedMilliseconds + " мс всего: " + MonsterGroup.MonsterGroups.Count + " шт.");
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



                var d = GetMainDispatcher(); //получает диспатчер самого бота, будет работать тока в isRelease
                if (d == null)
                {
                    log("Failed to get dispatcher");
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
                LoadScript();
                LoadQuest();
                MainForm.NeedApplyDungeonSettings = true;
                MainForm.NeedApplyQuestSettings = true;

                
                

                MainForm.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {

                            MainForm.Main1.Title = Me?.Name + " " + GetCurrentAccount().ServerName + " " + Version + " " + isReleaseVersion;

                            if (!CharacterSettings.HideQuesterUi)
                            {
                                MainForm.Main1.WindowState = WindowState.Normal;
                                if (CharacterSettings.QuesterLeft > -1)
                                {
                                    MainForm.Main1.Left = CharacterSettings.QuesterLeft;
                                    MainForm.Main1.Top = CharacterSettings.QuesterTop;
                                }
                            }
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


               


                #region TestLog

                if (AdvancedLog)
                {
                    log("Level: " + Me?.Level + " HP: " + Me?.Hp + "/" + Me?.MaxHp +/* " MP: " + Me.Mp + "/" + Me.MaxMp +*/ " Class: " + Me?.Class + "  " + Me.Exp + "  " + Me.NextLevelExp + "  ObjectSize" + Me.ObjectSize + " GetRealmId:" + Me.Guid.GetRealmId() + " GetServerId:" + Me.Guid.GetServerId() + "  ");

                    log(GetTimerInfo(EMirrorTimerType.Breath).InitialValue + "  " + GetTimerInfo(EMirrorTimerType.Breath).MaxValue + " " + GetTimerInfo(EMirrorTimerType.Breath).IsActivated);
                    log("-------------------------------GetQuests------------------------------------------------------");

                    //  Me.PrintDebug();

                    log("----------------------------------------GetEntities" + GetEntities().Count + "-------------------------------------");
                    foreach (var entity in GetEntities<GameObject>())
                    {
                        if (entity.Type == EBotTypes.Player)
                            continue;
                       // log(entity.Name + " " + entity.GameObjectType + "  " + entity.DynamicFlags);
                        
                        /* if(entity.Id != 20000021)
                             continue;*/


                        /*  log(entity.Guid.GetEntry() + "  " + entity.Guid.GetHighGuidType() + "  " + entity.Guid.GetObjectType() + "  Name: " + entity.Name  + "    Type:" + entity.Type 
                          + "  Дистанция: " + Distance(Me.Location.X, Me.Location.Y, Me.Location.Z, entity.Location.X, entity.Location.Y, entity.Location.Z) + "   " + entity.IsSpiritService
                          );*/

                        /*   if(entity.Id == 32641)
                           {
                               if (!ComeTo(entity.Location, 4))
                                   continue;
                               if (!OpenShop(entity))
                                   log("НЕ смог открыть шоп " + GetLastError(), LogLvl.Error);
                           }*/
                        //  Log("Мои координаты: " + Me.Location.X +" " + Me.Location.Y + " " + Me.Location.Z);  
                        //   Log("НПС координаты: " + entity.Location.X +" " +  entity.Location.Y+ " " + entity.Location.Z);
                        //   Log("Дистанция: " +Distance(Me.Location.X, Me.Location.Y, Me.Location.Z, entity.Location.X, entity.Location.Y,entity.Location.Z).ToString());

                    }







                    log("----------------------------------------GetSkills-------------------------------------" + SpellManager.GetSpells().Count);
                    foreach (var skill in SpellManager.GetSpells())
                    {
                        /*  if (NoShowSkill.Contains(skill.Id))
                              continue;*/
                        try
                        {
                            /* if (skill.DescriptionRu == "")
                                 continue;*/
                            if (skill.IsPassive())
                                continue;
                            
                            /*    if (skill.SkillLines.Contains(ESkillType.SKILL_MINING))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_HERBALISM))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_FISHING))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_MOUNTS))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_ALL_GLYPHS))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_ALL_SPECIALIZATIONS))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_COMPANIONS))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_GENERIC_DND))
                                    continue;
                                if (skill.SkillLines.Contains(ESkillType.SKILL_FIRST_AID))
                                    continue;*/


                            //  if (skill.Id == 6795)//(skill.DescriptionRu.Contains("Можно использовать"))// (skill.DescriptionRu.Contains("ед. урона ") || skill.DescriptionRu.Contains("физический урон") || skill.DescriptionRu.Contains("физического урона"))
                            //   {
                            //log(skill.Id + " " + "  " + skill.Name + " IsPassive =  " + skill.IsPassive() + "  RecoveryTime:" + skill.RecoveryTime + "  ChargeRecoveryTime:" + skill.ChargeRecoveryTime + "  ");
                            /*   foreach (var i in skill.SkillLines)
                                   log(i + "  ");*/

                            /*   foreach(var i in skill.GetEffectsData())
                               {
                                   log(i.Effect + "  " + i.Mechanic + "  " + i.ApplyAuraName);
                               }*/
                            /*   log("DescriptionRu: " + skill.DescriptionRu, LogLvl.Important);
                               log("AuraDescriptionRu: " + skill.AuraDescriptionRu + " \n");*/
                            //     }

                        }
                        catch
                        {
                            // log(skill.Id + " " + "  " + skill.Name + "  IsPassive = null" + "  " );
                        }


                    }
                    //   int percent = 220000 / 220000 / 100;
                    //   log(percent + " ghjd");
                    // SpellManager.CastSpell(5487);
                    log("GetAuras " + Me.GetAuras().Count);
                    foreach (var i in Me.GetAuras())
                    {
                       // log(i.SpellId + "   " + i.SpellName + "  " + i.AuraDescriptionRu);  //106829   Облик медведя  5487 аура 5487 
                    }



                    /*   foreach (var item in ItemManager.GetItems())
                       {
                           if (item.Id == 44925)

                           {
                               var target = GetNpcById(194133); 



                               var result = SpellManager.UseItem(item, target);
                               log(result + " ");
                           }
                       }*/

                    foreach (var entity in GetEntities<Unit>())
                    {
                        /* if (entity.Id == 43949)
                         {
                             if(OpenShop(entity))
                                 log("Открыл шоп ");
                             else
                             {
                                 log("не смог открыть шоп " + GetLastError());
                             }
                         }*/
                        /*  if (!entity.IsQuestGiver)
                              continue;*/



                        //  log(entity.Name + "  " + entity.Id + " IsQuestGiver:" + entity.IsQuestGiver + " QuestGiverStatus:" + entity.QuestGiverStatus + " " + entity.TaxiStatus);
                        /*  if (entity.QuestGiverStatus == EQuestGiverStatus.Reward)
                              if (!OpenDialog(entity))
                                  log("Не смог открыть диалог с НПС " + entity.Name + "  " + GetLastError(), LogLvl.Error);*/
                    }

                    log("--------------------------------------- GetNpcDialogs " + GetNpcDialogs().Count);

                    foreach (var gossipOptionsData in GetNpcDialogs())
                    {
                        log(gossipOptionsData.Text + " " + " " + gossipOptionsData.Confirm + " " + gossipOptionsData.OptionNPC);
                    }

                    log("--------------------------------------- GetNpcQuestDialogs " + GetNpcQuestDialogs().Count);
                    foreach (var gossipQuestTextData in GetNpcQuestDialogs())
                    {
                        log(" " + gossipQuestTextData.QuestID + " " + gossipQuestTextData.QuestType + " " + gossipQuestTextData.QuestTitle);
                    }

                    log("---------------------------------------");
                    var questlog = true;
                    if (questlog)
                        foreach (var quest in GetQuests())
                        {
                            if (quest.Template == null)
                            {
                                log(quest.Id + " ");
                                continue;
                            }
                            log(quest.Id + " State:" + quest.State + " LogTitle:" + quest.Template.LogTitle + " QuestType:" + quest.Template.QuestType + " Flags:" + quest.Template.Flags + " FlagsEx" + quest.Template.FlagsEx + " " + quest.Counts.Length, LogLvl.Ok);
                            for (var index = 0; index < quest.Counts.Length; index++)
                            {
                                var questCount = quest.Counts[index];
                                if (questCount == 0)
                                    continue;
                                log("questCount: " + index + ") " + questCount);
                            }
                           
                            foreach (var templateQuestObjective in quest.Template.QuestObjectives)
                            {
                                log(templateQuestObjective.Type + " " + templateQuestObjective.Amount + " " + templateQuestObjective.Description, LogLvl.Important);
                            }

                            if (quest.CompletionNpcIds != null)
                                foreach (var questCompletionNpcId in quest.CompletionNpcIds)
                                {
                                    log("questCompletionNpcId " + questCompletionNpcId);

                                }

                            log("---------------------------------------" + quest.GetQuestPOI().Count);

                            foreach (var questPoi in quest.GetQuestPOI())
                            {

                                log("questPoi Flags:" + questPoi.Flags
                                    + "   Floor:" //+ questPoi.
                                    + "   MapId:" + questPoi.MapId
                                    + "   ObjectiveIndex:" + questPoi.ObjectiveIndex
                                    + "   WorldMapAreaId:" + questPoi.WorldMapAreaId
                                    + "   BlobIndex:" + questPoi.BlobIndex
                                    + "   PlayerConditionId:" + questPoi.PlayerConditionId
                                    + "   Priority:" + questPoi.Priority
                                    + "   QuestObjectId:" + questPoi.QuestObjectId
                                    + "   WorldEffectId:" + questPoi.WorldEffectId
                                    + "   QuestObjectiveId:" + questPoi.QuestObjectiveId

                                    );
                                foreach (var questPoiPoint in questPoi.Points)
                                {
                                    //  log("questPoiPoint " + questPoiPoint.X + " " + questPoiPoint.Y);
                                }
                            }
                        }


                    log(CurrentInteractionGuid + " CurrentInteractionGuid");

                   //CreateNewEditorGpsPoint(Me.Location);
                    //  log(QuestManager.FindQuestSlot(29078) + "  " + QuestManager.GetQuestSlotQuestId(0) + "  " + QuestManager.(29078));


                    /*   log("-----------------------------------------------------------------------------" + GameDB.GameObjectsEntries.Count);
                       foreach (var i in GameDB.GameObjectsEntries)
                       {
                           if (i.Value.ID == 2175320)
                               log(i.Value.ID + "  " + i.Value.Name);

                           if (i.Key == 179965)
                               log(i.Value.ID + "  " + i.Value.Name);
                       }*/
                    log("-----------------------------------------------------------------------------");
                    log("-----------------------------------------------------------------------------");

                    log("----------------------------------------GetItems-------------------------------------");
                    foreach (var item in ItemManager.GetItems())
                    {

                        /*  if (item.Id == 141410)
                          {
                             var res = item.Sell();
                              log("результат " + res + " " + GetLastError());
                          }*/
                        /*  if (item.Id == 46789)//Грибы
                          {
                              log("Использую " + item.Name + " " + item.Place);
                              var result = SpellManager.UseItem(item);
                              if (result != EInventoryResult.OK)
                              {
                                  log("Не смог использовать итем " + item.Name + "  " + result,
                                      LogLvl.Error);
                              }
                          }*/

                        /* if (item.Id == 10327)//Ичияки
                         {
                             log("Использую " + item.Name + item.Place);
                             var result = SpellManager.UseItem(item);
                             if (result != EInventoryResult.OK)
                             {
                                 log("Не смог использовать итем " + item.Name + "  " + result,
                                     LogLvl.Error);
                             }
                         }*/

                        /* if (item.ItemClass == EItemClass.Armor || item.ItemClass == EItemClass.Weapon)
                         {
                             log(item.Id + "   " + item.Name + "  " + item.ItemClass + "  " + item.ItemQuality + "  " + item.Place + " Level:" + item.Level + " RequiredLevel:" + item.RequiredLevel + " " + " " + item.Modifiers + " " + " " + (EEquipmentSlot)item.Cell + " " + item.Cell + " " + item.InventoryType + " " + item.CanEquipItem() + " ");

                         }*/
                        /*  if (item.Place == EItemPlace.Equipment)
                              continue;

                          if (item.SpellId == 0)
                              continue;
                          var isbuff = false;

                          foreach (var effect in item.GetEffectsData())
                          {
                              if (effect.Effect == ESpellEffectName.APPLY_AURA && effect.ApplyAuraName == EAuraType.DUMMY)
                                  isbuff = true;
                          }

                          if (!isbuff)
                              continue;*/


                        /*  foreach (var i in item.SkillLines)
                             log(i.ToString());

                         foreach (var i in item.GetEffectsData())
                             log(i.Effect + "  " + i.Mechanic + "  " + i.ApplyAuraName);*/

                    }

                    /*  if (QuestManager.AcceptQuest(2))
                      {
                          Log("Взял квест " + GetLastError());
                      }
                      else
                      {
                          Log("Не смог взять квест " + GetLastError());
                      }      */



                 //   log(BindPoint.ZoneID + "  " + BindPoint.MapID + "  " + BindPoint.Location);


                    //  ForceMoveTo(1572.03, -4396.03, 15.98);
                    /*   log(Me.Target.GetAuras().Count + "   sdgsg ");
                       foreach (var aura in Me.Target.GetAuras())
                       {
                           log(aura.SpellName + " " + aura.SpellId);
                       }*/


                    // log(Me.GetPet().Name);

                }


               // log(LFGStatus.Joined + "" + Scenario.GetCriterias().Count);

                // SetMoveStateForClient(true);
                /*  TurnDirectly(new Vector3F(-8507.96, 672.98, 93.39));
                  TurnDirectly(new Vector3F(-8507.96, 672.98, 93.39));
                  TurnDirectly(new Vector3F(-8507.96, 672.98, 93.39));*/
                /*  Wait(1000);

                  MoveForward(true);
                  Thread.Sleep(1000);
                  MoveForward(false);
                  SetMoveStateForClient(false);*/



                AddNonUnloadableMesh(600, 31, 31);
                AddNonUnloadableMesh(600, 31, 32);
                AddNonUnloadableMesh(600, 31, 33);
                AddNonUnloadableMesh(600, 32, 31);
                AddNonUnloadableMesh(600, 32, 32);
                AddNonUnloadableMesh(600, 32, 33);
                AddNonUnloadableMesh(600, 33, 31);
                AddNonUnloadableMesh(600, 33, 32);
                AddNonUnloadableMesh(600, 33, 33);
                AddNonUnloadableMesh(600, 34, 31);
                AddNonUnloadableMesh(600, 34, 32);
                AddNonUnloadableMesh(600, 34, 33);
                AddNonUnloadableMesh(600, 32, 34);
                AddNonUnloadableMesh(600, 33, 34);
                AddNonUnloadableMesh(600, 34, 34);

                AddNonUnloadableMesh(571, 34, 22);
                AddNonUnloadableMesh(571, 34, 23);
                AddNonUnloadableMesh(571, 34, 24);
                AddNonUnloadableMesh(571, 35, 22);
                AddNonUnloadableMesh(571, 35, 23);
                AddNonUnloadableMesh(571, 35, 24);
                AddNonUnloadableMesh(571, 36, 22);
                AddNonUnloadableMesh(571, 36, 23);
                AddNonUnloadableMesh(571, 36, 24);
                AddNonUnloadableMesh(571, 34, 21);
                AddNonUnloadableMesh(571, 35, 21);
                AddNonUnloadableMesh(571, 36, 21);


                AddNonUnloadableMesh(1, 36, 16);
                AddNonUnloadableMesh(1, 36, 17);
                AddNonUnloadableMesh(1, 36, 18);
                AddNonUnloadableMesh(1, 37, 16);
                AddNonUnloadableMesh(1, 37, 17);
                AddNonUnloadableMesh(1, 37, 18);
                AddNonUnloadableMesh(1, 38, 16);
                AddNonUnloadableMesh(1, 38, 17);
                AddNonUnloadableMesh(1, 38, 18);
                //Перед 12 данжем
                AddNonUnloadableMesh(1, 35, 32);
                AddNonUnloadableMesh(1, 35, 33);
                AddNonUnloadableMesh(1, 35, 34);
                AddNonUnloadableMesh(1, 36, 32);
                AddNonUnloadableMesh(1, 36, 33);
                AddNonUnloadableMesh(1, 36, 34);
                AddNonUnloadableMesh(1, 37, 32);
                AddNonUnloadableMesh(1, 37, 33);
                AddNonUnloadableMesh(1, 37, 34);
                //12 данж
                AddNonUnloadableMesh(43, 30, 31);
                AddNonUnloadableMesh(43, 30, 32);
                AddNonUnloadableMesh(43, 30, 33);
                AddNonUnloadableMesh(43, 31, 31);
                AddNonUnloadableMesh(43, 31, 32);
                AddNonUnloadableMesh(43, 31, 33);
                AddNonUnloadableMesh(43, 32, 31);
                AddNonUnloadableMesh(43, 32, 32);
                AddNonUnloadableMesh(43, 32, 33);

                //перед 85 данж 
                AddNonUnloadableMesh(646, 29, 29);
                AddNonUnloadableMesh(646, 29, 30);
                AddNonUnloadableMesh(646, 29, 31);
                AddNonUnloadableMesh(646, 30, 29);
                AddNonUnloadableMesh(646, 30, 30);
                AddNonUnloadableMesh(646, 30, 31);
                AddNonUnloadableMesh(646, 31, 29);
                AddNonUnloadableMesh(646, 31, 30);
                AddNonUnloadableMesh(646, 31, 31);
                
                //в 85 данж 
                AddNonUnloadableMesh(725, 29, 29);
                AddNonUnloadableMesh(725, 29, 30);
                AddNonUnloadableMesh(725, 29, 31);
                AddNonUnloadableMesh(725, 30, 29);
                AddNonUnloadableMesh(725, 30, 30);
                AddNonUnloadableMesh(725, 30, 31);
                AddNonUnloadableMesh(725, 31, 29);
                AddNonUnloadableMesh(725, 31, 30);
                AddNonUnloadableMesh(725, 31, 31);

                #endregion

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


                /* foreach (var skill in SkillManager.GetSkills())
                 {
                     Log(skill.Id + " " + skill.Db.mName);
                 }*/

                /*
                                foreach (var entity in GetEntities())
                                {
                                    if (entity.Type == EBotTypes.Player)
                                        continue;
                                    log(Distance(Me.Location.X, Me.Location.Y, Me.Location.Z, entity.Location.X, entity.Location.Y, entity.Location.Z).ToString());
                                    log(entity.Guid + " " + entity.Type);
                                }

                                */
                //  log(Me.Money + "");
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
                if (Me.Level == 110)
                {
                    SendKeyPress(0x1b);
                }

               // CommonModule.MoveTo(1636.14, -4445.57, 17.04);
                GetUpdateInventory();
               
                
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
                            GetUpdateInventory();
                            CompInv();
                            timer2 = 0;
                        }

                        //  MainForm.Draw();
                        MyCheckNPC();
                        MainForm?.SetMe();
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
                            if (string.Compare(GetBotLogin(), "outside", true) == 0)
                            {
                                var tmpCharacter = Me.Name;
                                MainForm.On = false;
                                Thread.Sleep(1000);
                                //Restart();

                                Thread.Sleep(5000);
                                while (GameState != EGameState.CharacterSelect)
                                {
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(5000);
                                /*  foreach (var gameCharacter in CurrentServer.GetCharacters())
                                  {
                                      if (gameCharacter.Name != tmpCharacter)
                                          continue;
                                      gameCharacter.EnterGame();
                                      Thread.Sleep(5000);
                                      while (GameState != EGameState.Ingame)
                                          Thread.Sleep(1000);


                                      MainForm.On = true;
                                      break;
                                  }*/
                            }
                            else
                            {
                                if (Me.Level == 65 && CharacterSettings.Mode == EMode.Questing)//"Выполнение квестов")
                                    GetCurrentAccount().IsAutoLaunch = false;
                                TerminateGameClient();
                            }

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
        }

        public bool NeedRestart = false;
        public int EventInactiveCount;
        public bool EventInactive;



        /*
         *Random  RandomGenerator = new Random();
        public Vector3F GetRandomPoint()
            {
                double angle = 2.0 * Math.PI * RandomGenerator.NextDouble();
                return new Vector3F(Me.Target.Location.X + RandomGenerator.Next(4,9) * Math.Cos(angle), Me.Target.Location.Y + RandomGenerator.Next(4,7) * Math.Sin(angle), Me.Location.Z);
            }

public void PluginRun()
       {

           while (true)
           {
               if (Me.Target != null)
               {
                   var f = GetRandomPoint();
                   ForceMoveToWithLookTo(f, Me.Target.Location);
               }
           }
}
         *
         */
        public void PluginStop()
        {
            try
            {
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
                        MainForm.Close();
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

        private void StopPluginNow()
        {
            try
            {
                var indexOfChar = _path.IndexOf(Ch, StringComparison.Ordinal);
                _path = indexOfChar > 0 ? _path.Substring(indexOfChar) : "Quester";
                log(_path + "\\WowAI.dll");
                StopPlugin(_path + "\\WowAI.dll");
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

        public bool LoadDG()
        {
            if (GetBotLogin() == "Daredevi1" || GetBotLogin() == "alxpro")
            {
                DacandaName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Dacanda.xml";
                // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;

                if (!File.Exists(DacandaName))
                    File.Create(DacandaName);

                var doc = new XmlDocument();
                try
                {
                    doc.Load(DacandaName);
                    _cfgLoaded = true;
                    log("Загружаю Dacanda из файла: " + DacandaName, LogLvl.Ok);
                }
                catch
                {
                    log("Не получилось загрузить Dacanda: " + DacandaName, LogLvl.Error);

                }
            }
            return _cfgLoaded;
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
            // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;

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
                MainForm.Dispatcher.BeginInvoke(new Action(() =>
                {

                    //  MainForm.GridSettings.DataContext = CharacterSettings;
                    //    MainForm.ComboBoxSwitchMode.DataContext = CharacterSettings;
                    /*   MainForm.StackPanelGlobal.DataContext = CharacterSettings;
                       MainForm.listViewProp.DataContext = CharacterSettings;
                       MainForm.GridResource.DataContext = CharacterSettings;
                       MainForm.groupBoxFilterMob.DataContext = CharacterSettings;*/


                }));
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