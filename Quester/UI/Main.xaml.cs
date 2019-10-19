using Out.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
//using ComboBox = System.Windows.Controls.ComboBox;
//using TextBox = System.Windows.Controls.TextBox;
using WoWBot.Core;
using static WowAI.Host;
// ReSharper disable PossibleInvalidOperationException
// ReSharper disable SpecifyACultureInStringConversionExplicitly


namespace WowAI.UI
{
    /*  public class EnumToItemsSource : MarkupExtension
      {
          private readonly Type _type;

          public EnumToItemsSource(Type type)
          {
              _type = type;
          }

          public override object ProvideValue(IServiceProvider serviceProvider)
          {
              return _type.GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>()).Select(x => x.Description).ToList();
          }
      }


      public class EnumConverter : IValueConverter
      {
          public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
          {
              if (value == null) return "";
              foreach (Enum one in Enum.GetValues(parameter as Type))
              {
                  if (value.Equals(one))
                      return GetDescription(one);
              }
              return "";
          }

          public object ConvertBack(object value, Type targetType,
              object parameter, CultureInfo culture)
          {
              if (value == null) return null;
              foreach (Enum one in Enum.GetValues(parameter as Type))
              {
                  if (value.ToString() == GetDescription(one))
                      return one;
              }
              return null;
          }

          private string GetDescription(Enum one)
          {
              DescriptionAttribute[] da =
                 (DescriptionAttribute[])(one.GetType().
                        GetField(one.ToString()).
                               GetCustomAttributes(typeof(DescriptionAttribute), false));
              return da.Length > 0 ? da[0].Description : one.ToString();

          }
      }


      */

    public partial class Main
    {
        public Main()
        {
            try
            {
                On = true;
                NeedApplySettings = false;
                SettingsNeedSave = false;
                InitializeComponent();
                DataContext = this;
                ComboBoxDungeonScript.SelectionChanged += ComboBoxDungeonScript_SelectionChanged;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        private void ComboBoxDungeonScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!Host.FormInitialized)
                return;
            if (Host.CharacterSettings == null)
                return;
            if (ComboBoxDungeonScript.SelectedItem == null)
                return;
            Host.CharacterSettings.Script = ComboBoxDungeonScript.SelectedItem.ToString();
            if (ComboBoxDungeonAction.SelectedIndex == 0)
                CollectionDungeonCoord.RemoveAll();
            NeedApplyDungeonSettings = true;
        }

        public bool NeedApplySettings;
        public bool NeedApplyDungeonSettings;
        public bool NeedApplyQuestSettings;

        public bool On;

        public bool SettingsNeedSave;
        private XmlDocument _doc;
        public bool IsToggle;
        public bool IsToggleDone = true;
        public bool NeedUpdate = true;
        internal Host Host;



        private ObservableCollection<QuestCoordSettings> _collectionQuestSettings;
        private ObservableCollection<PropSettings> _collectionPropSettings;
        private ObservableCollection<EventSettings> _collectionEvents;
        private ObservableCollection<SkillTable> _collectionAllSkill;
        private ObservableCollection<BuffTable> _collectionAllBuff;
        private ObservableCollection<SkillSettings> _collectionActiveSkill;
        private ObservableCollection<ItemSettings> _collectionInvItems;
        private ObservableCollection<MobsSettings> _collectionMobs;
        private ObservableCollection<DungeonCoordSettings> _collectionDungeonCoord;
        private ObservableCollection<PetSettings> _collectionPetSettings;
        private ObservableCollection<IgnoreQuest> _collectionIgnoreQuest;
        private ObservableCollection<MultiZone> _collectionMultiZone;
        private ObservableCollection<AukSettings> _collectionaAukSettingses;
        private ObservableCollection<NpcForAction> _collectionNpcForActions;
        private ObservableCollection<RegenItems> _collectionRegenItems;
        private ObservableCollection<ItemGlobal> _collectionItemGlobals;
        private ObservableCollection<GameObjectIgnore> _gameObjectIgnores;
        private ObservableCollection<ScriptSchedule> _scriptSchedules;
        private ObservableCollection<LearnSkill> _learnSkill;
        private ObservableCollection<AdvancedEquipWeapon> _collectionAdvancedEquipsWeapon;
        private ObservableCollection<AdvancedEquipArmor> _collectionAdvancedEquipArmors;
        private ObservableCollection<AdvancedEquipStat> _collectionAdvancedEquipStats;
        private ObservableCollection<LearnTalent> _collectionLearnTalents;

        public ObservableCollection<LearnTalent> CollectionLearnTalents
        {
            get
            {
                if (_collectionLearnTalents == null)
                    _collectionLearnTalents = new ObservableCollection<LearnTalent>();
                return _collectionLearnTalents;
            }
        }

        public ObservableCollection<AdvancedEquipStat> CollectionAdvancedEquipStats
        {
            get
            {
                if (_collectionAdvancedEquipStats == null)
                    _collectionAdvancedEquipStats = new ObservableCollection<AdvancedEquipStat>();
                return _collectionAdvancedEquipStats;
            }
        }

        public ObservableCollection<AdvancedEquipArmor> CollectionAdvancedEquipArmors
        {
            get
            {
                if (_collectionAdvancedEquipArmors == null)
                    _collectionAdvancedEquipArmors = new ObservableCollection<AdvancedEquipArmor>();
                return _collectionAdvancedEquipArmors;
            }
        }

        public ObservableCollection<AdvancedEquipWeapon> CollectionAdvancedEquipsWeapon
        {
            get
            {
                if (_collectionAdvancedEquipsWeapon == null)
                    _collectionAdvancedEquipsWeapon = new ObservableCollection<AdvancedEquipWeapon>();
                return _collectionAdvancedEquipsWeapon;
            }
        }


        public ObservableCollection<LearnSkill> CollectionLearnSkill
        {
            get
            {
                if (_learnSkill == null)
                    _learnSkill = new ObservableCollection<LearnSkill>();
                return _learnSkill;
            }
        }

        private ObservableCollection<EquipAuc> _equipAucs;
        public ObservableCollection<EquipAuc> CollectionEquipAuc
        {
            get
            {
                if (_equipAucs == null)
                    _equipAucs = new ObservableCollection<EquipAuc>();
                return _equipAucs;
            }
        }

        public ObservableCollection<ScriptSchedule> CollectionScriptSchedules
        {
            get
            {
                if (_scriptSchedules == null)
                    _scriptSchedules = new ObservableCollection<ScriptSchedule>();
                return _scriptSchedules;
            }
        }

        public ObservableCollection<AukSettings> CollectionAukSettingses
        {
            get
            {
                if (_collectionaAukSettingses == null)
                    _collectionaAukSettingses = new ObservableCollection<AukSettings>();
                return _collectionaAukSettingses;
            }
        }

        public ObservableCollection<GameObjectIgnore> CollectionGameObjectIgnores
        {
            get
            {
                if (_gameObjectIgnores == null)
                    _gameObjectIgnores = new ObservableCollection<GameObjectIgnore>();
                return _gameObjectIgnores;
            }
        }
        public ObservableCollection<QuestCoordSettings> CollectionQuestSettings
        {
            get
            {
                if (_collectionQuestSettings == null)
                    _collectionQuestSettings = new ObservableCollection<QuestCoordSettings>();
                return _collectionQuestSettings;
            }
        }

        public ObservableCollection<ItemGlobal> CollectionItemGlobals
        {
            get
            {
                if (_collectionItemGlobals == null)
                    _collectionItemGlobals = new ObservableCollection<ItemGlobal>();
                return _collectionItemGlobals;
            }
        }

        public ObservableCollection<RegenItems> CollectionRegenItems
        {
            get
            {
                if (_collectionRegenItems == null)
                    _collectionRegenItems = new ObservableCollection<RegenItems>();
                return _collectionRegenItems;
            }
        }

        public ObservableCollection<NpcForAction> CollectionNpcForActions
        {
            get
            {
                if (_collectionNpcForActions == null)
                    _collectionNpcForActions = new ObservableCollection<NpcForAction>();
                return _collectionNpcForActions;
            }
        }



        public ObservableCollection<MultiZone> CollectionMultiZone
        {
            get
            {
                if (_collectionMultiZone == null)
                    _collectionMultiZone = new ObservableCollection<MultiZone>();
                return _collectionMultiZone;
            }
        }

        public ObservableCollection<PropSettings> CollectionProps
        {
            get
            {
                if (_collectionPropSettings == null)
                    _collectionPropSettings = new ObservableCollection<PropSettings>();
                return _collectionPropSettings;
            }
        }



        public ObservableCollection<IgnoreQuest> CollectionIgnoreQuest
        {
            get
            {
                if (_collectionIgnoreQuest == null)
                    _collectionIgnoreQuest = new ObservableCollection<IgnoreQuest>();
                return _collectionIgnoreQuest;
            }
        }


        public ObservableCollection<PetSettings> CollectionPetSettings
        {
            get
            {
                if (_collectionPetSettings == null)
                    _collectionPetSettings = new ObservableCollection<PetSettings>();
                return _collectionPetSettings;
            }
        }

        public ObservableCollection<EventSettings> CollectionEventSettings
        {
            get
            {
                if (_collectionEvents == null)
                    _collectionEvents = new ObservableCollection<EventSettings>();
                return _collectionEvents;
            }
        }

        public ObservableCollection<DungeonCoordSettings> CollectionDungeonCoord
        {
            get
            {
                if (_collectionDungeonCoord == null)
                    _collectionDungeonCoord = new ObservableCollection<DungeonCoordSettings>();
                return _collectionDungeonCoord;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {

            }
        }

        public ObservableCollection<MobsSettings> CollectionMobs
        {
            get
            {
                if (_collectionMobs == null)
                    _collectionMobs = new ObservableCollection<MobsSettings>();
                return _collectionMobs;
            }
        }

        public ObservableCollection<SkillSettings> CollectionActiveSkill
        {
            get
            {
                if (_collectionActiveSkill == null)
                    _collectionActiveSkill = new ObservableCollection<SkillSettings>();
                return _collectionActiveSkill;
            }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public ObservableCollection<ItemSettings> CollectionInvItems
        {
            get
            {
                if (_collectionInvItems == null)
                    _collectionInvItems = new ObservableCollection<ItemSettings>();
                return _collectionInvItems;
            }

        }

        public ObservableCollection<BuffTable> CollectionAllBuff
        {
            get
            {
                if (_collectionAllBuff == null)
                    _collectionAllBuff = new ObservableCollection<BuffTable>();
                return _collectionAllBuff;
            }

        }

        public ObservableCollection<SkillTable> CollectionAllSkill
        {
            get
            {
                if (_collectionAllSkill == null)
                    _collectionAllSkill = new ObservableCollection<SkillTable>();
                return _collectionAllSkill;
            }

        }





        //---------------------------------------------------------------------------------Start Status Block------------------------------------------------------------------------

        #region Status Block



        public void SetQuestIdText(string s)
        {
            try
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() => { QuestId.Content = s; });
            }
            catch (TaskCanceledException)
            {
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetQuestStateText(string s)
        {
            try
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() => { QuestState.Content = "State: " + s; });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetBestMobText(string s)
        {
            try
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() => { BestMob.Content = "BestMob: " + s; });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetBestPropText(string s)
        {
            try
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() => { BestProp.Content = "BestProp: " + s; });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetFarmModuleText(string s)
        {
            try
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() => { FarmState.Content = "FarmState: " + s; });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetFarmModuleToolTipt(string s)
        {
            try
            {
                if (Dispatcher != null)
                    Dispatcher.Invoke(() => { FarmState.ToolTip = s; });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }




        /* public void SetIsMovementSuspendedText(string s)
         {
             try
             {
                 Dispatcher.Invoke(() =>
                 {
                     IsMoveToNow.Content =  s;
                 });
             }
             catch (TaskCanceledException)
             {
             }
             catch (Exception e)
             {
                 Host.log(e.ToString());
             }
         }*/




        public void SetMe()
        {
            try
            {

                long invgold = 0;
                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        //  Host.log(item.GetSellPrice() + " " + item.Count + " " + item.Name + " " + item.Id);
                        // invgold = invgold + item.GetSellPrice() * item.Count;
                    }
                }

                var isMoveToNow = "IsMoveToNow: " + Host.CommonModule?.IsMoveToNow + "   Moving: " + Host.Me.IsMoving + "   Susp: " + Host.CommonModule?.IsMoveSuspended();





                /*  if (Host.CharacterSettings.Mode == EMode.Script)
                  {
                      questId = "Скрипт " + Host.AutoQuests.ScriptStopwatch.Elapsed.Minutes + " min, " +
                                        Host.AutoQuests.ScriptStopwatch.Elapsed.Seconds + " sec  Speed:" + Host.Me.RunSpeed + " " + Host.Me.MovementFlagExtra + " " + Host.Me.MovementFlags + " " + Host.Me.SwimBackSpeed +
                                        " " + Host.Me.SwimSpeed;
                  }*/



                var allTime = DateTime.Now - Host.TimeWork;
                var labelKillMobs = "";
                if (allTime.TotalMinutes > 0 && Host.TimeInFight != 0)
                {
                    labelKillMobs = "Убито мобов: " + Host.KillMobsCount + "(" + Math.Round(Host.KillMobsCount / allTime.TotalMinutes, 3) + ") " +
                                           " ДПС: " + Host.AllDamage / Host.TimeInFight + " (" + (Host.FarmModule.TickTime - Host.TimeAttack) + ")";
                }

                var formattedTimeSpan = $"{allTime.Hours:D2} hr, {allTime.Minutes:D2} min, {allTime.Seconds:D2} sec";

                var labelTimeWork = "Время работы: " + formattedTimeSpan + " (" + Host.CheckCount + ") " + "   : " + Host.FarmModule.DeadCount + "/" + Host.FarmModule.DeadCountInPvp;

                var labelGameState = Host.GameState + " " + " Map:" + Host.MapID + " Area:" + Host.Area.Id + " Zone:" + Host.Zone.Id + " Смертей: " +
                                         Host.FarmModule.DeadCount + "/" + Host.FarmModule.DeadCountInPvp;
                var textboxMeCoord = Host.Me.Location.ToString();


                var meTarget = "[" + Host.GetAgroCreatures().Count + "/" + Host.FarmModule.MobsWithDropCount() + "/" + Host.FarmModule.MobsWithSkinCount() + "]Цель: Не выбрана" + Host.SpellManager.CurrentAutoRepeatSpellId;

                if (Host.Me.Target != null && Host.IsExists(Host.Me.Target))
                {
                    meTarget = "[" + Host.GetAgroCreatures().Count + "/" +
                               Host.FarmModule.MobsWithDropCount() + "/" + Host.FarmModule.MobsWithSkinCount() + "]"/* + Host.Me.HasInArc(Math.PI, Host.Me.Target) + Host.Me.GetAngle(Host.Me.Target)*/

                              + " Цель:" +
                               Host.Me.Target.Name +
                               "[" + Host.Me.Target.Id + "]Dist:" +
                               Math.Round(Host.Me.Distance(Host.Me.Target));

                }
                var alternatePoint = EPowerType.AlternatePower;
                if (Host.Me.Class == EClass.Druid || Host.Me.Class == EClass.Rogue)
                    alternatePoint = EPowerType.ComboPoints;
                if (Host.Me.Class == EClass.Monk)
                    alternatePoint = EPowerType.Chi;

                double tempGold = Host.Me.Money;
                var gold = tempGold / 10000;
                var startGold = Host.StartGold / 10000;
                invgold = invgold - Host.Startinvgold;
                var doubleGold = Convert.ToDouble(invgold) / 10000;

                var labelGold = "Золото: " + Math.Round(gold, 2) + " г. [" +
                                     Math.Round(gold - startGold, 2) + " г. + " + Math.Round(doubleGold, 2) +
                                     " г.] Всего: " + Math.Round((gold - startGold) + doubleGold, 2) + " г.  ";


                var labelGoldInHour = "";
                if (allTime.TotalHours > 0 && allTime.TotalDays > 0)
                {
                    var goldinday = Math.Round(((gold - startGold) /*+ doubleGold*/) / allTime.TotalDays, 2);
                    labelGoldInHour = "Золота в час / день: " +
                                              Math.Round(((gold - startGold) /*+ doubleGold*/) / allTime.TotalHours, 2) +
                                              " / " +
                                              goldinday + " Шанс смерти " + Host.GetChanceDeath() + "%";






                }
                var meMaxHp = Host.Me.MaxHp;
                var meHp = Host.Me.Hp;

                var maxPower = Host.Me.GetMaxPower(Host.Me.PowerType);
                var power = Host.Me.GetPower(Host.Me.PowerType);

                var powerType = Host.Me.PowerType;
                var maxPowerAlt = Host.Me.GetMaxPower(alternatePoint);
                var powerAlt = Host.Me.GetPower(alternatePoint);
                if (Host.ClientType == EWoWClient.Classic)
                {
                    maxPowerAlt = Host.MyTotalInvSlot();
                    powerAlt = Host.MyFreeInvSlots();
                }

                var exp = "Опыта" + Host.MyExps.CalcExpGain(Host.Me) + " в мин: " + Host.MyExps.CalcAverageExp() + "  До левел апа: " + Host.MyExps.CalcMinToLevelUp(Host.Me) + " мин";

                // CheckSkill();
                if (Dispatcher != null)
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            LabelExp.Content = exp;
                            IsMoveToNow.Content = isMoveToNow;

                            LabelKillMobs.Content = labelKillMobs;
                            LabelTimeWork.Content = labelTimeWork;
                            LabelGameState.Content = labelGameState;
                            TextboxMeCoord.Text = textboxMeCoord;

                            LabelGoldInHour.Content = labelGoldInHour;
                            LabelGold.Content = labelGold;

                            /*   if (SettingsNeedSave)
                               GridSettings.Margin = new Thickness(0, 25, 0, 0);*/

                            ProgressBarMeHp.Maximum = meMaxHp;
                            ProgressBarMeHp.Value = meHp;
                            var formattedString = $"{ProgressBarMeHp.Value}/{ProgressBarMeHp.Maximum}";
                            TextBlockHp.Text = formattedString;

                            ProgressBarMeMp.Foreground = Brushes.Aqua;
                            ProgressBarMeMp.Maximum = maxPower;
                            ProgressBarMeMp.Value = power;

                            formattedString = $"{ProgressBarMeMp.Value}/{ProgressBarMeMp.Maximum}";
                            TextBlockMp.Text = powerType + ": " + formattedString;

                            ProgressBarMeEnergy.Maximum = maxPowerAlt;
                            ProgressBarMeEnergy.Value = powerAlt;
                            formattedString = $"{ProgressBarMeEnergy.Value}/{ProgressBarMeEnergy.Maximum}";
                            TextBlockMeEnergy.Text = formattedString;


                            if (Host.Me.Target != null && Host.IsExists(Host.Me.Target))
                            {
                                ProgressBarTargetHp.Maximum = Host.Me.Target.MaxHp;
                                ProgressBarTargetHp.Value = Host.Me.Target.Hp;
                                formattedString = $"{ProgressBarTargetHp.Value}/{ProgressBarTargetHp.Maximum}";
                            }

                            TextBlockTargetHp.Text = formattedString;

                            MeTarget.Content = meTarget;


                            if (sw.ElapsedMilliseconds > 0)
                                Host.log("SetMe " + sw.ElapsedMilliseconds + " mc", LogLvl.Error);
                        }
                        catch (Exception e)
                        {
                            Host.log(e.ToString());
                        }
                    });
            }
            catch (TaskCanceledException)
            {
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        //--------------------------------------------------------------------------------------------End Status Block-----------------------------------------------------------------

        #endregion




        //---------------------------------------------------------------------------------Start Function-------------------------------------------------------------------------

        #region Function

        /// <summary>
        /// Поиск текста в комбобокс
        /// </summary>
        /// <param name="cb">Комбобокс</param>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public bool FindComboBoxItems(ComboBox cb, int value)
        {
            try
            {
                for (var j = 0; j < cb.Items.Count; j++)
                {
                    // MessageBox.Show(ComboBoxKey.Items[j].ToString());
                    if (cb.Items[j].ToString().Contains(value.ToString()))
                        return true;
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
            return false;
        }


        /// <summary>
        /// Возвращает Id скила из комбобокс
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        private int GetSkillIdFromCombobox(string skillId)
        {
            var startIndex = skillId.IndexOf("[", StringComparison.Ordinal);
            var endIndex = skillId.IndexOf("]", StringComparison.Ordinal);
            var leght = endIndex - startIndex + 1;
            var tempId = Convert.ToInt32(skillId.Substring(startIndex + 1, leght - 2));
            return tempId;
        }



        //-----------------------------------------------------------------------------------End Function---------------------------------------------------------------------------

        #endregion





        //--------------------------------------------------------------------------------Start Buttons-----------------------------------------------------------------------

        #region Buttons

        /// <summary>
        /// Кнопка 0-100
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUpCharacterMinHp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var textbox = new TextBox();
                //Персонаж вверх
                if (Equals(sender, ButtonUpMeMaxHp))
                    textbox = TextBoxMeMaxHp;
                if (Equals(sender, ButtonUpMeMaxMp))
                    textbox = TextBoxMeMaxMp;
                if (Equals(sender, ButtonUpMeMinHp))
                    textbox = TextBoxMeMinHp;
                if (Equals(sender, ButtonUpMeMinMp))
                    textbox = TextBoxMeMinMp;
                //Цель вверх
                if (Equals(sender, ButtonUpTargetMaxHp))
                    textbox = TextBoxTargetMaxHp;
                if (Equals(sender, ButtonUpTargetMinHp))
                    textbox = TextBoxTargetMinHp;
                //Дистанция вверх
                if (Equals(sender, ButtonUpMaxDist))
                    textbox = TextBoxMaxDist;
                if (Equals(sender, ButtonUpMinDist))
                    textbox = TextBoxMinDist;
                //АОЕ вверх
                if (Equals(sender, ButtonUpAoeMax))
                    textbox = TextBoxAoeMax;
                if (Equals(sender, ButtonUpAoeMin))
                    textbox = TextBoxAoeMin;
                if (Equals(sender, ButtonUpAoeRadius))
                    textbox = TextBoxAoeRadius;
                //левел вверх
                if (Equals(sender, ButtonUpMaxLevel))
                    textbox = TextBoxMaxLevel;
                if (Equals(sender, ButtonUpMinLevel))
                    textbox = TextBoxMinLevel;

                if (Convert.ToInt32(textbox.Text) > 99) return;
                textbox.Text = (Convert.ToInt32(textbox.Text) + 1).ToString();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Кнопка 100- 0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDownCharacterMinHp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var textbox = new TextBox();
                // Персонаж вниз
                if (Equals(sender, ButtonDownMeMaxHp))
                    textbox = TextBoxMeMaxHp;
                if (Equals(sender, ButtonDownMeMaxMp))
                    textbox = TextBoxMeMaxMp;
                if (Equals(sender, ButtonDownMeMinHp))
                    textbox = TextBoxMeMinHp;
                if (Equals(sender, ButtonDownMeMinMp))
                    textbox = TextBoxMeMinMp;
                //Цель вниз
                if (Equals(sender, ButtonDownTargetMaxHp))
                    textbox = TextBoxTargetMaxHp;
                if (Equals(sender, ButtonDownTargetMinHp))
                    textbox = TextBoxTargetMinHp;
                //Дистанция вниз
                if (Equals(sender, ButtonDownMaxDist))
                    textbox = TextBoxMaxDist;
                if (Equals(sender, ButtonDownMinDist))
                    textbox = TextBoxMinDist;
                //АОЕ вниз
                if (Equals(sender, ButtonDownAoeMax))
                    textbox = TextBoxAoeMax;
                if (Equals(sender, ButtonDownAoeMin))
                    textbox = TextBoxAoeMin;
                if (Equals(sender, ButtonDownAoeRadius))
                    textbox = TextBoxAoeRadius;
                // level down
                if (Equals(sender, ButtonDownMaxLevel))
                    textbox = TextBoxMaxLevel;
                if (Equals(sender, ButtonDownMinLevel))
                    textbox = TextBoxMinLevel;

                if (Convert.ToInt32(textbox.Text) == 0) return;
                textbox.Text = (Convert.ToInt32(textbox.Text) - 1).ToString();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Кнопка приоритета скилов 0-9
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonUpPriotity_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt32(TextBoxPriority.Text) > 8) return;
            TextBoxPriority.Text = (Convert.ToInt32(TextBoxPriority.Text) + 1).ToString();
        }

        /// <summary>
        /// Кнопка приоритета скилов 9-0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDownPriority_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt32(TextBoxPriority.Text) == 0) return;
            TextBoxPriority.Text = (Convert.ToInt32(TextBoxPriority.Text) - 1).ToString();
        }

        /// <summary>
        /// Закрывает интерфес
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                /*  if (MessageBox.Show("Do you want close application?", "Question", MessageBoxButton.YesNo) ==
                      MessageBoxResult.Yes)
                  {*/
                Host.CancelMoveTo();
                On = false;
                Host.CancelRequested = true;
                /* }
                 else e.Cancel = true;*/
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }


        public void buttonOnOff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonOnOff.Content.ToString() == "Включить")
                {
                    On = true;
                    ButtonOnOff.Content = "Выключить";
                }
                else
                {
                    On = false;
                    Host.AutoQuests?.ScriptStopwatch.Stop();
                    Host.CancelMoveTo();
                    Host.FarmModule.BestMob = null;
                    Host.FarmModule.BestProp = null;
                    // Host.FarmModule.farmState = Modules.FarmState.Disabled;                  
                    ButtonOnOff.Content = "Включить";
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception exception) { Host.log(exception.ToString()); }
        }

        //------------------------------------------------------------------EndButtons-----------------------------------------------------------------------

        #endregion





        //-------------------------------------------------------------------------------Start Events-----------------------------------------------------------------------

        #region Events

        /*  private readonly SolidColorBrush _colorWhite = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _color = new SolidColorBrush(Color.FromArgb(0xFF, 0x36, 0x39, 0x3E));//FF36393E*/
        //  private void DatagridActiveQuest_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        //  {
        //e.Row.Background = _color;
        //e.Row.Foreground = _colorWhite;
        //  }


        private void CheckBoxBaseDist_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonDownMaxDist.IsEnabled = false;
                ButtonUpMaxDist.IsEnabled = false;
                ButtonDownMinDist.IsEnabled = false;
                ButtonUpMinDist.IsEnabled = false;
                TextBoxMinDist.IsEnabled = false;
                TextBoxMaxDist.IsEnabled = false;
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void CheckBoxBaseDist_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonDownMaxDist.IsEnabled = true;
                ButtonUpMaxDist.IsEnabled = true;
                ButtonDownMinDist.IsEnabled = true;
                ButtonUpMinDist.IsEnabled = true;
                TextBoxMinDist.IsEnabled = true;
                TextBoxMaxDist.IsEnabled = true;
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void RadioButtonAoeMe_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButtonAoeTarget.IsChecked = true;
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void Main1_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Main1.Width = 300;
                Main1.Height = 510;

            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void comboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                ComboBoxInvItems.Items.Clear();

                if (Host.Me == null)
                    return;
                foreach (var item in Host.ItemManager.GetItems())
                    ComboBoxInvItems.Items.Add("[" + item.Id + "]" + item.Name + " " + item.NameRu);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        /* private void ComboBoxTargetEffect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
             try
             {
                // ComboBoxNotTargetEffect.Items.Clear();
                 ComboBoxIsTargetEffect.Items.Clear();
                  if (Host.Me.Target == null)
                      return;


                  foreach (var abnormalStatuse in Host.Me.Target.GetAuras())
                  {
                     // ComboBoxNotTargetEffect.Items.Add("[" + abnormalStatuse.SpellId + "]" + abnormalStatuse.SpellName);
                      ComboBoxIsTargetEffect.Items.Add("[" + abnormalStatuse.SpellId + "]" + abnormalStatuse.SpellName);
                  }
             }
             catch (ThreadAbortException)
             {
             }
             catch (Exception err)
             {

                 Host.log(err.ToString());
             }
         }*/

        /* private void ComboBoxMeEffect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
             try
             {
                 // Host.log("[");
                 //ComboBoxNotMeEffect.Items.Clear();
                 //ComboBoxIsMeEffect.Items.Clear();
                  foreach (var abnormalStatuse in Host.Me.GetAuras())
                  {
                    //  ComboBoxNotMeEffect.Items.Add("[" + abnormalStatuse.SpellId + "]" + abnormalStatuse.SpellName);
                    //  ComboBoxIsMeEffect.Items.Add("[" + abnormalStatuse.SpellId + "]" + abnormalStatuse.SpellName);
                  }
             }
             catch (ThreadAbortException)
             {
             }
             catch (Exception err)
             {

                 Host.log(err.ToString());
             }
         }*/

        private void ComboBoxSwitchMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Host == null)
                    return;
                switch (Host.GetBotLogin())
                {
                    case "zawww":
                        break;
                    case "zawww2":
                        break;
                    case "Daredevi1":
                        break;
                    default:
                        return;

                }

                if (ComboBoxSwitchMode == null || ComboBoxSwitchMode.SelectedIndex == -1)
                    return;
                Host.log(ComboBoxSwitchMode.SelectedIndex + "  " + Host.CharacterSettings.Mode);
                Host.CharacterSettings.Mode = (Mode)ComboBoxSwitchMode.SelectedIndex;
                Host.log(ComboBoxSwitchMode.SelectedIndex + "  " + Host.CharacterSettings.Mode);
            }
            catch (Exception err)
            {
                Host?.log(err.ToString());
            }
        }



        /// <summary>
        /// Ограничения для приоритетов 0-9
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                var ctrl = sender as TextBox;
                e.Handled = "0123456789".IndexOf(e.Text, StringComparison.Ordinal) < 0; //только цифры
                if (ctrl != null)
                    ctrl.MaxLength = 1; //длина текста в текстбоксе
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Ограничения для тексбокса 0-100
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxCharacterMinHp_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                var ctrl = sender as TextBox;
                e.Handled = "0123456789".IndexOf(e.Text, StringComparison.Ordinal) < 0; //только цифры
                if (ctrl != null) ctrl.MaxLength = 3; //длина текста в текстбоксе
                if (ctrl != null && Convert.ToInt32(ctrl.Text) > 99)
                {
                    ctrl.Text = "100";
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }


        /// <summary>
        /// Событие на изменение размера интерфейса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main1_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            /* try
             {
                 if (IsToggle)
                 {
                     // MessageBox.Show("sdfdsf");
                     GridSettings.Width = Main1.ActualWidth;

                     if (Main1.ActualHeight > 55) GridSettings.Height = Main1.ActualHeight - 55;
                     else
                     {
                         GridSettings.Height = 0;
                     }
                 }
                 else
                 {
                     GridSettings.Width = 0;
                     GridSettings.Height = 0;
                 }
             }
             catch (ThreadAbortException)
             {
             }
             catch (Exception exception)
             {
                 Host.log(exception.ToString());
             }*/
        }

        private void ListViewAllSkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataGridAllSkill.SelectedIndex == -1)
                    return;
                DataGridActiveSkill.SelectedIndex = -1;
                ButtonAddSkill.IsEnabled = true;
                ButtonChangeSkill.IsEnabled = false;
                ButtonDelSkill.IsEnabled = false;

                var skill = DataGridAllSkill.SelectedItem as SkillTable;
                if (skill != null)
                {
                    var spell = Host.SpellManager.GetSpell(skill.Id);
                    GroupBoxSettingsSkill.Header = "Настройки " + skill.Name + "[" + skill.Id + "]" + "  " + spell?.DescriptionRu;
                    TextBoxMinDist.Text =
                        Host.SpellManager.GetSpell(skill.Id).GetMinCastRange().ToString(
                            CultureInfo.InvariantCulture);
                    TextBoxMaxDist.Text =
                         Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange().ToString(
                            CultureInfo.InvariantCulture);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        //------------------------------------------------------------------EndEvents-----------------------------------------------------------------------

        #endregion




        //---------------------------------------------------------------------------Start Settings Change-------------------------------------------------------------------------

        #region Настройки Сохранение/Изменение/Загрузка

        /// <summary>
        /// Сброс настроек скилов по умолчанию
        /// </summary>
        private void ResetSettingSkillDefault()
        {
            try
            {
                //сброс настроек по умолчанию
                TextBoxPriority.Text = "0";
                //Персонаж
                TextBoxMeMinHp.Text = "0";
                TextBoxMeMaxHp.Text = "100";
                TextBoxMeMinMp.Text = "0";
                TextBoxMeMaxMp.Text = "100";
                //Цель
                TextBoxTargetMinHp.Text = "0";
                TextBoxTargetMaxHp.Text = "100";
                //Дистанция
                TextBoxMaxDist.Text = "20";
                TextBoxMinDist.Text = "0";
                CheckBoxBaseDist.IsChecked = true;
                CheckBoxMoveDist.IsChecked = true;
                //Аое
                RadioButtonAoeMe.IsChecked = true;
                TextBoxAoeRadius.Text = "5";
                TextBoxAoeMin.Text = "0";
                TextBoxAoeMax.Text = "100";
                //эффекты
                CheckBoxSelfTarget.IsChecked = false;
                CheckBoxUseInFight.IsChecked = true;
                //ComboBoxNotMeEffect.SelectedIndex = -1;
                TextBoxNotMeEffect.Text = "0";
                TextBoxNotTargetEffect.Text = "0";

                TextBoxIsMeEffect.Text = "0";
                TextBoxIsTargetEffect.Text = "0";
                // ComboBoxNotTargetEffect.SelectedIndex = -1;
                // ComboBoxIsTargetEffect.SelectedIndex = -1;
                // ComboBoxIsMeEffect.SelectedIndex = -1;
                TextBoxAlternatePowerLess.Text = "0";
                TextBoxAlternatePowerMore.Text = "0";
                //Левел
                TextBoxMinLevel.Text = "1";
                TextBoxMaxLevel.Text = "100";
                TextBoxNotTargetId.Text = "0";


                CheckBoxUseInPvp.IsChecked = false;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }

        }


        private void CreateNewQuestSettings()
        {
            try
            {

                Host.QuestSettings.QuestCoordSettings.Clear();
                foreach (var dungeonCoordSettingse in CollectionQuestSettings)
                {
                    var item = new QuestCoordSettings
                    {
                        Run = dungeonCoordSettingse.Run,
                        QuestId = dungeonCoordSettingse.QuestId,
                        QuestName = dungeonCoordSettingse.QuestName,
                        NpcId = dungeonCoordSettingse.NpcId,
                        Loc = dungeonCoordSettingse.Loc,
                        State = dungeonCoordSettingse.State
                    };
                    Host.QuestSettings.QuestCoordSettings.Add(item);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception error)
            {
                Host.log(error.ToString());
            }
        }


        private void CreateNewDungeonSettings()
        {
            try
            {

                Host.DungeonSettings.DungeonCoordSettings.Clear();
                var i = 0;
                foreach (var dungeonCoordSettingse in CollectionDungeonCoord)
                {
                    var item = new DungeonCoordSettings
                    {
                        Id = i,
                        Action = dungeonCoordSettingse.Action,
                        Attack = dungeonCoordSettingse.Attack,
                        Loc = dungeonCoordSettingse.Loc,
                        MobId = dungeonCoordSettingse.MobId,
                        PropId = dungeonCoordSettingse.PropId,
                        Pause = dungeonCoordSettingse.Pause,
                        ItemId = dungeonCoordSettingse.ItemId,
                        Com = dungeonCoordSettingse.Com,
                        AreaId = dungeonCoordSettingse.AreaId,
                        MapId = dungeonCoordSettingse.MapId,
                        SkillId = dungeonCoordSettingse.SkillId,
                        PluginPath = dungeonCoordSettingse.PluginPath,
                        QuestId = dungeonCoordSettingse.QuestId,
                        QuestAction = dungeonCoordSettingse.QuestAction
                    };
                    Host.DungeonSettings.DungeonCoordSettings.Add(item);
                    i++;
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception error)
            {
                Host.log(error.ToString());
            }
        }



        private void CreateNewSettings()
        {
            try
            {
                Host.CharacterSettings.CheckRepairInCity = CheckBoxCheckRepairInCity.IsChecked.Value;
                Host.CharacterSettings.LearnAllSpell = CheckBoxLearnAllSpell.IsChecked.Value;
                Host.CharacterSettings.CraftConjured = CheckBoxCraftConjured.IsChecked.Value;
                Host.CharacterSettings.CraftConjuredHp = Convert.ToInt32(TextBoxCraftConjuredHp.Text);
                Host.CharacterSettings.CraftConjuredMp = Convert.ToInt32(TextBoxCraftConjuredMp.Text);
                Host.CharacterSettings.AdvancedFight = CheckBoxAdvancedFight.IsChecked.Value;
                Host.CharacterSettings.RandomDistForAttack = CheckBoxRandomDistForAttack.IsChecked.Value;
                Host.CharacterSettings.RandomDistForAttackCount = Convert.ToInt32(TextBoxRandomDistForAttackCount.Text);
                Host.CharacterSettings.UseArrow = CheckBoxUseArrow.IsChecked.Value;
                Host.CharacterSettings.RunRun = CheckBoxRunRun.IsChecked.Value;
                Host.CharacterSettings.RunRunPercent = Convert.ToInt32(TextBoxRunRunPercent.Text);

                Host.CharacterSettings.UseArrowId = Convert.ToUInt32(TextBoxUseArrowId.Text);
                Host.CharacterSettings.TwoWeapon = CheckBoxTwoWeapon.IsChecked.Value;
                Host.CharacterSettings.AttackMobForDrop = CheckBoxAttackMobForDrop.IsChecked.Value;
                Host.CharacterSettings.UseStoneIfStuck = CheckBoxUseStoneIsStuck.IsChecked.Value;
                Host.CharacterSettings.AdvancedEquipUseShield = CheckBoxAdvancedEquipShield.IsChecked.Value;
                Host.CharacterSettings.AdvancedEquip = CheckBoxAdvancedEquip.IsChecked.Value;
                Host.CharacterSettings.AutoEquip = CheckBoxAutoEquip.IsChecked.Value;
                Host.CharacterSettings.CheckRepairAndSell = CheckBoxCheckRepairAndSell.IsChecked.Value;
                Host.CharacterSettings.WaitSixMin = CheckBoxWaitSixMin.IsChecked.Value;
                Host.CharacterSettings.QuesterLeft = Main1.Left;
                Host.CharacterSettings.QuesterTop = Main1.Top;
                Host.CharacterSettings.DebuffDeath = CheckBoxDebuffDeath.IsChecked.Value;
                Host.CharacterSettings.UseFilterMobs = CheckBoxUseFilterMobs.IsChecked.Value;
                Host.CharacterSettings.FightIfMobs = CheckBoxFightIfMobs.IsChecked.Value;
                Host.CharacterSettings.StopQuesting = CheckBoxStopQuesting.IsChecked.Value;
                Host.CharacterSettings.StopQuestingLevel = Convert.ToInt32(TextBoxStopQuestingLevel.Text);
                Host.CharacterSettings.UnmountMoveFail = CheckBoxUnmountMoveFail.IsChecked.Value;

                Host.CharacterSettings.FindBestPoint = CheckBoxFindBestPoint.IsChecked.Value;
                Host.CharacterSettings.CheckAuk = CheckBoxCheckAuk.IsChecked.Value;
                Host.CharacterSettings.LogAll = CheckBoxAllLog.IsChecked.Value;
                Host.CharacterSettings.FreeInvCountForAuk = Convert.ToInt32(TextBoxFreeInvCountForAuk.Text);
                Host.CharacterSettings.SummonBattlePet = CheckBoxSummonBattlePet.IsChecked.Value;
                Host.CharacterSettings.BattlePetNumber = ComboBoxSummonBattlePetNumber.SelectedIndex;
                Host.CharacterSettings.UseMultiZone = CheckBoxMultiZone.IsChecked.Value;
                Host.CharacterSettings.AoeFarm = CheckBoxAoeFarm.IsChecked.Value;
                Host.CharacterSettings.AoeMobsCount = Convert.ToInt32(TextBoxAoeMobsCount.Text);
                Host.CharacterSettings.FreeInvCountForAukId = Convert.ToUInt32(TextBoxFreeInvCountForAukId.Text);
                Host.CharacterSettings.FormForFight = ComboBoxFormForFight.Text;
                Host.CharacterSettings.FormForMove = ComboBoxFormForMove.Text;
                Host.CharacterSettings.LaunchScript = CheckBoxLaunchSkript.IsChecked.Value;
                Host.CharacterSettings.UseMountMyLoc = CheckBoxUseSMountMyLoc.IsChecked.Value;
                Host.CharacterSettings.RunQuestHerbalism = CheckBoxRunQuestHerbalism.IsChecked.Value;

                Host.CharacterSettings.RepairCount = Convert.ToInt32(TextBoxRepairCount.Text);
                Host.CharacterSettings.CheckRepair = CheckBoxCheckRepair.IsChecked.Value;
                Host.CharacterSettings.InvFreeSlotCount = Convert.ToInt32(TextBoxFreeInvCount.Text);
                Host.CharacterSettings.UseStoneForSellAndRepair = CheckBoxUseStoneForSellAndRepair.IsChecked.Value;
                Host.CharacterSettings.GatherLocX = Convert.ToSingle(TextBoxGatherLocX.Text);
                Host.CharacterSettings.GatherLocY = Convert.ToSingle(TextBoxGatherLocY.Text);
                Host.CharacterSettings.GatherLocZ = Convert.ToSingle(TextBoxGatherLocZ.Text);
                Host.CharacterSettings.GatherLocMapId = Convert.ToInt32(TextBoxGatherLocMapId.Text);
                Host.CharacterSettings.GatherLocAreaId = Convert.ToInt32(TextBoxGatherLocAreaId.Text);
                Host.CharacterSettings.GatherRadius = Convert.ToInt32(TextBoxGatherLocRadius.Text);
                Host.CharacterSettings.CheckBoxAttackForSitMount = CheckBoxAttackForSitMount.IsChecked.Value;
                Host.CharacterSettings.FarmLocX = Convert.ToSingle(TextBoxFarmLocX.Text);
                Host.CharacterSettings.FarmLocY = Convert.ToSingle(TextBoxFarmLocY.Text);
                Host.CharacterSettings.FarmLocZ = Convert.ToSingle(TextBoxFarmLocZ.Text);
                Host.CharacterSettings.FarmLocMapId = Convert.ToInt32(TextBoxFarmLocMapId.Text);
                Host.CharacterSettings.FarmLocAreaId = Convert.ToUInt32(TextBoxFarmLocAreaId.Text);
                Host.CharacterSettings.FarmRadius = Convert.ToInt32(TextBoxFarmLocRadius.Text);
                Host.CharacterSettings.WorldQuest = CheckBoxWorldQuest.IsChecked.Value;
                Host.CharacterSettings.MountLocX = Convert.ToSingle(TextBoxMountLocX.Text);
                Host.CharacterSettings.MountLocY = Convert.ToSingle(TextBoxMountLocY.Text);
                Host.CharacterSettings.MountLocZ = Convert.ToSingle(TextBoxMountLocZ.Text);
                Host.CharacterSettings.MountLocMapId = Convert.ToInt32(TextBoxMountLocMapId.Text);
                Host.CharacterSettings.MountLocAreaId = Convert.ToInt32(TextBoxMountLocAreaId.Text);
                Host.CharacterSettings.RunScriptFromBegin = CheckBoxRunScriptfrombegin.IsChecked.Value;
                Host.CharacterSettings.AukRun = CheckBoxAukRun.IsChecked.Value;
                Host.CharacterSettings.AukAreaId = Convert.ToInt32(TextBoxAukLocAreaId.Text);
                Host.CharacterSettings.AukMapId = Convert.ToInt32(TextBoxAukLocMapId.Text);
                Host.CharacterSettings.AukLocX = Convert.ToSingle(TextBoxAukLocX.Text);
                Host.CharacterSettings.AukLocY = Convert.ToSingle(TextBoxAukLocY.Text);
                Host.CharacterSettings.AukLocZ = Convert.ToSingle(TextBoxAukLocZ.Text);
                Host.CharacterSettings.SummonMount = CheckBoxSummonMount.IsChecked.Value;
                Host.CharacterSettings.ScriptReverse = CheckBoxScriptReverse.IsChecked.Value;
                Host.CharacterSettings.UseWhistleForSellAndRepair = CheckBoxUseWhistleForSellAndRepair.IsChecked.Value;
                Host.CharacterSettings.LogScriptAction = CheckBoxLogScript.IsChecked.Value;
                Host.CharacterSettings.LogSkill = CheckBoxLogSkill.IsChecked.Value;
                Host.CharacterSettings.ScriptScheduleEnable = CheckBoxScriptScheduleEnable.IsChecked.Value;
                Host.CharacterSettings.ForceMoveScriptEnable = CheckBoxForceMoveScriptEnable.IsChecked.Value;
                Host.CharacterSettings.ForceMoveScriptDist = Convert.ToInt32(TextBoxForceMoveScriptDist.Text);
                Host.CharacterSettings.PetRegen = Convert.ToInt32(TextBoxPetRegen.Text);
                Host.CharacterSettings.FightIfHPLess = CheckBoxFightIfHpLess.IsChecked.Value;
                Host.CharacterSettings.FightIfHPLessCount = Convert.ToInt32(TextBoxFightIfHpLessCount.Text);
                Host.CharacterSettings.UseFly = CheckBoxUseFly.IsChecked.Value;
                Host.CharacterSettings.Skinning = CheckBoxSkining.IsChecked.Value;
                Host.CharacterSettings.NoAttackOnMount = CheckBoxNoAttackOnMount.IsChecked.Value;
                Host.CharacterSettings.FightForSell = CheckBoxFightForSell.IsChecked.Value;
                Host.CharacterSettings.GatherResouceScript = CheckBoxGatherResourceScript.IsChecked.Value;
                Host.CharacterSettings.GatherRadiusScript = Convert.ToInt32(TextBoxGatherRadiusScript.Text);
                Host.CharacterSettings.EquipItemStat = Convert.ToInt32(TextBoxEquipStateWeapon.Text);
                Host.CharacterSettings.Attack = CheckBoxAttack.IsChecked.Value;
                Host.CharacterSettings.AttackRadius = Convert.ToInt32(TextBoxAttackRadius.Text);
                Host.CharacterSettings.HpRegen = Convert.ToInt32(TextBoxHpRegen.Text);
                Host.CharacterSettings.MpRegen = Convert.ToInt32(TextBoxMpRegen.Text);
                Host.CharacterSettings.UseRegen = CheckBoxUseRegen.IsChecked.Value;
                Host.CharacterSettings.UseDash = CheckBoxUseDash.IsChecked.Value;
                Host.CharacterSettings.UseStoneForLearnSpell = CheckBoxUseStoneForLearnSpell.IsChecked.Value;
                Host.CharacterSettings.Zrange = Convert.ToInt32(TextBoxFarmZRange.Text);
                Host.CharacterSettings.KillRunaways = CheckBoxKillRunaways.IsChecked.Value;
                Host.CharacterSettings.Mode = (Mode)ComboBoxSwitchMode.SelectedIndex;
                Host.CharacterSettings.CheckSellAndRepairScript = CheckBoxCheckSellAndRepairScript.IsChecked.Value;
                Host.CharacterSettings.AlternateAuk = CheckBoxAlternateAuk.IsChecked.Value;
                Host.CharacterSettings.ReturnToCenter = CheckBoxReturnToCenter.IsChecked.Value;
                Host.CharacterSettings.PickUpLoot = CheckBoxPickUpLoot.IsChecked.Value;
                Host.CharacterSettings.IgnoreMob = Convert.ToInt32(TextBoxIgnoreMob.Text);
                Host.CharacterSettings.ResPetInCombat = CheckBoxResPetInCombat.IsChecked.Value;
                Host.CharacterSettings.ResPetMeMp = Convert.ToInt32(TextBoxResPetMeMp.Text);

                Host.CharacterSettings.StoneLoc = new Vector3F(Convert.ToSingle(TextBoxStoneLocX.Text), Convert.ToSingle(TextBoxStoneLocY.Text), Convert.ToSingle(TextBoxStoneLocZ.Text));
                Host.CharacterSettings.StoneAreaId = Convert.ToUInt32(TextBoxStoneLocAreaId.Text);
                Host.CharacterSettings.StoneMapId = Convert.ToInt32(TextBoxStoneLocMapId.Text);
                Host.CharacterSettings.StoneRegister = CheckBoxRegisterStone.IsChecked.Value;
                Host.CharacterSettings.MaxItemQuality = (EItemQuality)ComboBoxAdvancedEquipQuality.SelectedIndex;
                //  Host.CharacterSettings.Mode = ComboBoxSwitchMode.Text;

                //Farm
                /*    Host.CharacterSettings.FarmLoc = new Vector3F(Convert.ToSingle(textBoxFarmLocX.Text),
                        Convert.ToSingle(textBoxFarmLocY.Text), Convert.ToSingle(textBoxFarmLocZ.Text));
                    Host.CharacterSettings.FarmRadius = Convert.ToInt32(textBoxFarmLocRadius.Text);*/
                //Gather
                /*  Host.CharacterSettings.GatherLoc = new Vector3F(Convert.ToSingle(textBoxGatherLocX.Text),
                      Convert.ToSingle(textBoxGatherLocY.Text), Convert.ToSingle(textBoxGatherLocZ.Text));*/
                Host.CharacterSettings.KillMobFirst = CheckBoxKillMobsFirst.IsChecked.Value;

                Host.CharacterSettings.RandomJump = CheckBoxRandomJump.IsChecked.Value;
                // Host.CharacterSettings.GatherRadius = Convert.ToInt32(textBoxGatherLocRadius.Text);

                Host.CharacterSettings.AukTime = ComboBoxAukTimeClassic.SelectedIndex;

                Host.CharacterSettings.SendMail = CheckBoxSendMail.IsChecked.Value;
                Host.CharacterSettings.SendMailName = TextBoxSendMailName.Text;
                Host.CharacterSettings.Pvp = CheckBoxPvp.IsChecked.Value;

                Host.CharacterSettings.SendMailLocAreaId = Convert.ToInt32(TextBoxSendMailLocAreaId.Text);
                Host.CharacterSettings.SendMailLocMapId = Convert.ToInt32(TextBoxSendMailLocMapId.Text);
                Host.CharacterSettings.SendMailLocX = Convert.ToSingle(TextBoxSendMailLocX.Text);
                Host.CharacterSettings.SendMailLocY = Convert.ToSingle(TextBoxSendMailLocY.Text);
                Host.CharacterSettings.SendMailLocZ = Convert.ToSingle(TextBoxSendMailLocZ.Text);
                Host.CharacterSettings.ChangeTargetInCombat = CheckBoxChangeTargetInCombat.IsChecked.Value;

                try
                {
                    //  Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2    " + textBoxCheckAukStartTime.Text);
                    //   Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2   " + textBoxCheckAukEndTime.Text);



                    Host.CharacterSettings.SendMailStartTime = TimeSpan.Parse(TextBoxSendMailStartTime.Text);
                    Host.CharacterSettings.SendMailStopTime = TimeSpan.Parse(TextBoxSendMailEndTime.Text);

                    //  Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2  " + textBoxCheckAukStartTime.Text);
                    //  Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2  " + textBoxCheckAukEndTime.Text);
                }
                catch (Exception e)
                {
                    Host.log(e + "");
                }

                //  
                Host.CharacterSettings.AukTime = ComboBoxAukTime.SelectedIndex;
                Host.CharacterSettings.Script = ComboBoxDungeonScript.Text;
                Host.CharacterSettings.Quest = ComboBoxQuestSet.Text;

                try
                {
                    //Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2    " + textBoxCheckAukStartTime.Text);
                    //Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2   " + textBoxCheckAukEndTime.Text);

                    Host.CharacterSettings.CheckAukInTimeRange = CheckBoxCheckAukTime.IsChecked.Value;
                    Host.CharacterSettings.StartAukTime = TimeSpan.Parse(TextBoxCheckAukStartTime.Text);
                    Host.CharacterSettings.EndAukTime = TimeSpan.Parse(TextBoxCheckAukEndTime.Text);
                    //Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2  " + textBoxCheckAukStartTime.Text);
                    //Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2  " + textBoxCheckAukEndTime.Text);
                }
                catch (Exception e)
                {
                    Host.log(e + "");
                }


                Host.CharacterSettings.AdvancedEquipArmors.Clear();
                Host.CharacterSettings.AdvancedEquipsWeapon.Clear();
                Host.CharacterSettings.LearnSkill.Clear();
                Host.CharacterSettings.EquipAucs.Clear();
                Host.CharacterSettings.ScriptSchedules.Clear();
                Host.CharacterSettings.AukSettingses.Clear();
                Host.CharacterSettings.GameObjectIgnores.Clear();
                Host.CharacterSettings.MyItemGlobals.Clear();
                Host.CharacterSettings.RegenItemses.Clear();
                Host.CharacterSettings.IgnoreQuests.Clear();
                Host.CharacterSettings.NpcForActionSettings.Clear();
                Host.CharacterSettings.SkillSettings.Clear();
                Host.CharacterSettings.ItemSettings.Clear();
                Host.CharacterSettings.MobsSettings.Clear();
                Host.CharacterSettings.MultiZones.Clear();
                Host.CharacterSettings.EventSettings.Clear();
                Host.CharacterSettings.PropssSettings.Clear();
                Host.CharacterSettings.AdvancedEquipStats.Clear();
                Host.CharacterSettings.PetSettings.Clear();
                Host.CharacterSettings.LearnTalents.Clear();

                foreach (var collectionLearnTalent in CollectionLearnTalents)
                {
                    Host.CharacterSettings.LearnTalents.Add(collectionLearnTalent);
                }

                foreach (var collectionAdvancedEquipStat in CollectionAdvancedEquipStats)
                    Host.CharacterSettings.AdvancedEquipStats.Add(collectionAdvancedEquipStat);

                foreach (var collectionAdvancedEquipArmor in CollectionAdvancedEquipArmors)
                    Host.CharacterSettings.AdvancedEquipArmors.Add(collectionAdvancedEquipArmor);

                foreach (var collectionAdvancedEquip in CollectionAdvancedEquipsWeapon)
                    Host.CharacterSettings.AdvancedEquipsWeapon.Add(collectionAdvancedEquip);

                foreach (var learnSkill in CollectionLearnSkill)
                    Host.CharacterSettings.LearnSkill.Add(learnSkill);


                foreach (var colectionEquipAuc in CollectionEquipAuc)
                    Host.CharacterSettings.EquipAucs.Add(colectionEquipAuc);


                foreach (var collectionScriptSchedule in CollectionScriptSchedules)
                    Host.CharacterSettings.ScriptSchedules.Add(collectionScriptSchedule);


                foreach (var collectionAukSettingse in CollectionAukSettingses)
                    Host.CharacterSettings.AukSettingses.Add(collectionAukSettingse);


                foreach (var collectionGameObjectIgnore in CollectionGameObjectIgnores)
                    Host.CharacterSettings.GameObjectIgnores.Add(collectionGameObjectIgnore);

                foreach (var collectionItemGlobal in CollectionItemGlobals)
                    Host.CharacterSettings.MyItemGlobals.Add(collectionItemGlobal);


                foreach (var collectionRegenItem in CollectionRegenItems)
                    Host.CharacterSettings.RegenItemses.Add(collectionRegenItem);



                foreach (var collectionNpcForAction in CollectionNpcForActions)
                    Host.CharacterSettings.NpcForActionSettings.Add(collectionNpcForAction);



                foreach (var collectionIgnoreQuests in CollectionIgnoreQuest)
                    Host.CharacterSettings.IgnoreQuests.Add(collectionIgnoreQuests);

                foreach (var collectionPet in CollectionPetSettings)
                    Host.CharacterSettings.PetSettings.Add(collectionPet);


                foreach (var collectionEvent in CollectionEventSettings)
                    Host.CharacterSettings.EventSettings.Add(collectionEvent);


                foreach (var collectionInvItem in CollectionInvItems)
                    Host.CharacterSettings.ItemSettings.Add(collectionInvItem);

                foreach (var collectionProp in CollectionProps)
                    Host.CharacterSettings.PropssSettings.Add(collectionProp);

                foreach (var multiZone in CollectionMultiZone)
                    Host.CharacterSettings.MultiZones.Add(multiZone);

                foreach (var collectionMobs in CollectionMobs)
                    Host.CharacterSettings.MobsSettings.Add(collectionMobs);

                foreach (var r in CollectionActiveSkill)
                    Host.CharacterSettings.SkillSettings.Add(r);
                Host.CommonModule.SetEquip();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception error)
            {
                Host.log(error.ToString());
            }
        }


        private void buttonAddSkill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridAllSkill.SelectedIndex == -1)
                    return;
                if (Convert.ToInt32(TextBoxMaxDist.Text) < 1)
                {
                    // ReSharper disable once RedundantNameQualifier
                    System.Windows.MessageBox.Show("Максимальная дистанция скила не может быть меньше 1", "Ошибка", MessageBoxButton.OK);
                    return;
                }



                var path = DataGridAllSkill.SelectedItem as SkillTable;
                var tempNotTargetEffect = Convert.ToInt32(TextBoxNotTargetEffect.Text);
                var tempNotMeEffect = Convert.ToInt32(TextBoxNotMeEffect.Text);
                var tempIsMeEffect = Convert.ToInt32(TextBoxIsMeEffect.Text);

                var tempIsTargetEffect = Convert.ToInt32(TextBoxIsTargetEffect.Text);

                if (path != null)
                    CollectionActiveSkill.Add(new SkillSettings
                    {
                        Checked = true,
                        Id = Convert.ToUInt32(path.Id),
                        MeMaxMp = Convert.ToInt32(TextBoxMeMaxMp.Text),
                        MeMaxHp = Convert.ToInt32(TextBoxMeMaxHp.Text),
                        MeMinHp = Convert.ToInt32(TextBoxMeMinHp.Text),
                        MeMinMp = Convert.ToInt32(TextBoxMeMinMp.Text),
                        Name = path.Name,
                        Priority = Convert.ToInt32(TextBoxPriority.Text),

                        TargetMaxHp = Convert.ToInt32(TextBoxTargetMaxHp.Text),
                        TargetMinHp = Convert.ToInt32(TextBoxTargetMinHp.Text),
                        BaseDist = CheckBoxBaseDist.IsChecked != null && CheckBoxBaseDist.IsChecked.Value,
                        MaxDist = Convert.ToInt32(TextBoxMaxDist.Text),
                        MinDist = Convert.ToInt32(TextBoxMinDist.Text),
                        MoveDist = CheckBoxMoveDist.IsChecked != null && CheckBoxMoveDist.IsChecked.Value,
                        AoeMax = Convert.ToInt32(TextBoxAoeMax.Text),
                        AoeMe = RadioButtonAoeMe.IsChecked != null && RadioButtonAoeMe.IsChecked.Value,
                        AoeMin = Convert.ToInt32(TextBoxAoeMin.Text),
                        AoeRadius = Convert.ToInt32(TextBoxAoeRadius.Text),
                        SelfTarget = CheckBoxSelfTarget.IsChecked != null && CheckBoxSelfTarget.IsChecked.Value,
                        NotTargetEffect = tempNotTargetEffect,
                        NotMeEffect = tempNotMeEffect,
                        IsMeEffect = tempIsMeEffect,
                        IsTargetEffect = tempIsTargetEffect,
                        MinMeLevel = Convert.ToInt32(TextBoxMinLevel.Text),
                        MaxMeLevel = Convert.ToInt32(TextBoxMaxLevel.Text),
                        CombatElementCountLess = Convert.ToInt32(TextBoxAlternatePowerLess.Text),
                        CombatElementCountMore = Convert.ToInt32(TextBoxAlternatePowerMore.Text),
                        UseInFight = CheckBoxUseInFight.IsChecked != null && CheckBoxUseInFight.IsChecked.Value,
                        TargetId = Convert.ToInt32(TextBoxTargetId.Text),
                        NotTargetId = Convert.ToInt32(TextBoxNotTargetId.Text),
                        UseInPVP = CheckBoxUseInPvp.IsChecked.Value,
                        PetMaxHp = Convert.ToInt32(TextBoxPetMaxHp.Text),
                        PetMinHp = Convert.ToInt32(TextBoxPetMinHp.Text),
                    }

                    );
                //сброс настроек по умолчанию
                ResetSettingSkillDefault();
                GroupBoxSettingsSkill.Header = "Настройки";
                ButtonAddSkill.IsEnabled = false;

            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        /// <summary>
        /// Событие на выбор элемента в листе активных скилов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewActiveSkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataGridActiveSkill.SelectedIndex == -1)
                    return;

                var skill = DataGridActiveSkill.SelectedItem as SkillSettings;
                DataGridAllSkill.SelectedIndex = -1;
                ButtonChangeSkill.IsEnabled = true;
                ButtonDelSkill.IsEnabled = true;
                ButtonAddSkill.IsEnabled = false;
                if (skill != null)
                {
                    var spell = Host.SpellManager.GetSpell(skill.Id);
                    GroupBoxSettingsSkill.Header = "Настройки " + skill.Name + "[" + skill.Id + "] " + spell?.DescriptionRu;
                    TextBoxPriority.Text = skill.Priority.ToString();
                    TextBoxMeMinHp.Text = skill.MeMinHp.ToString();
                    TextBoxMeMaxHp.Text = skill.MeMaxHp.ToString();
                    TextBoxMeMinMp.Text = skill.MeMinMp.ToString();
                    TextBoxMeMaxMp.Text = skill.MeMaxMp.ToString();
                    TextBoxTargetMinHp.Text = skill.TargetMinHp.ToString();
                    TextBoxTargetMaxHp.Text = skill.TargetMaxHp.ToString();
                    //Дист
                    TextBoxMinDist.Text = skill.MinDist.ToString();
                    TextBoxMaxDist.Text = skill.MaxDist.ToString();
                    CheckBoxMoveDist.IsChecked = skill.MoveDist;
                    CheckBoxBaseDist.IsChecked = skill.BaseDist;
                    //Аое
                    RadioButtonAoeMe.IsChecked = skill.AoeMe;
                    TextBoxAoeRadius.Text = skill.AoeRadius.ToString();
                    TextBoxAoeMax.Text = skill.AoeMax.ToString();
                    TextBoxAoeMin.Text = skill.AoeMin.ToString();
                    //
                    CheckBoxSelfTarget.IsChecked = skill.SelfTarget;
                    CheckBoxUseInFight.IsChecked = skill.UseInFight;
                    //мин макс
                    TextBoxMinLevel.Text = skill.MinMeLevel.ToString();
                    TextBoxMaxLevel.Text = skill.MaxMeLevel.ToString();

                    TextBoxNotTargetEffect.Text = skill.NotTargetEffect.ToString();
                    TextBoxNotMeEffect.Text = skill.NotMeEffect.ToString();
                    TextBoxIsTargetEffect.Text = skill.IsTargetEffect.ToString();
                    TextBoxIsMeEffect.Text = skill.IsMeEffect.ToString();

                    TextBoxAlternatePowerMore.Text = skill.CombatElementCountMore.ToString();
                    TextBoxAlternatePowerLess.Text = skill.CombatElementCountLess.ToString();

                    TextBoxTargetId.Text = skill.TargetId.ToString();
                    TextBoxPetMaxHp.Text = skill.PetMaxHp.ToString();
                    TextBoxPetMinHp.Text = skill.PetMinHp.ToString();
                    TextBoxNotTargetId.Text = skill.NotTargetId.ToString();
                    //int items;
                    //"Эффекты
                    /*  if (skill.IsMeEffect != 0)
                      {
                          items =
                              ComboBoxIsMeEffect.Items.Add("[" + skill.IsMeEffect + "]" + Host.GameDB.DBAbnormalStatusInfos[skill.IsMeEffect].Name);
                          ComboBoxIsMeEffect.SelectedIndex = items;
                      }
                      else
                      {
                          ComboBoxIsMeEffect.SelectedIndex = -1;
                      }
                      */
                    /*   if (skill.NotMeEffect != 0)
                       {
                           items =
                               ComboBoxNotMeEffect.Items.Add("[" + skill.NotMeEffect + "]" +Host.GameDB.DBAbnormalStatusInfos[skill.NotMeEffect].Name);
                           ComboBoxNotMeEffect.SelectedIndex = items;
                       }
                       else
                       {
                           ComboBoxNotMeEffect.SelectedIndex = -1;
                       }*/
                    /*  if (skill.NotTargetEffect != 0)
                      {
                          items =
                              ComboBoxNotTargetEffect.Items.Add("[" + skill.NotTargetEffect + "]" +
                                                                Host.GameDB.DBAbnormalStatusInfos[
                                                                    skill.NotTargetEffect].Name);
                          ComboBoxNotTargetEffect.SelectedIndex = items;
                      }
                      else
                      {
                          ComboBoxNotTargetEffect.SelectedIndex = -1;
                      }*/

                    /*   if (skill.IsTargetEffect != 0)
                       {
                           items =
                               ComboBoxIsTargetEffect.Items.Add("[" + skill.IsTargetEffect + "]" +
                                                                Host.GameDB.DBAbnormalStatusInfos[skill.IsTargetEffect]
                                                                    .Name);
                           ComboBoxIsTargetEffect.SelectedIndex = items;
                       }
                       else
                       {
                           ComboBoxIsTargetEffect.SelectedIndex = -1;
                       }*/
                    //Элементы


                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }


        public void MyGetTalent()
        {
            try
            {
                if (Host.ClientType != EWoWClient.Classic)
                    return;
                LabelTalent.Content = "AvailablePoints: " + Host.TalentTree.AvailablePoints + Environment.NewLine;
                foreach (var characterSettingsLearnTalent in Host.CharacterSettings.LearnTalents)
                {
                    LabelTalent.Content += characterSettingsLearnTalent.Name + "[" + characterSettingsLearnTalent.Id + "]  ";
                    foreach (var talentSpell in Host.TalentTree.GetAllTalents())
                    {
                        if (talentSpell.ID != characterSettingsLearnTalent.Id)
                            continue;
                        LabelTalent.Content += talentSpell.GetCurrentRank() + "/" + talentSpell.GetMaxRank() + "  " + talentSpell.CanLearn() + Environment.NewLine;
                    }
                }


            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        public void subItem1_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (!IsToggleDone)
                    return;

                IsToggleDone = false;

                if (!IsToggle)
                {

                    IsToggle = true;
                    Main1.Height = 800;
                    Main1.Width = 1400 + 300;

                    /*Main1.Height = 750;
                    Main1.Width = 1294 + 300;
                    GridSettings.Width = Main1.ActualWidth - 300;
                    GridMain.Width = 300;
                    GridMain.HorizontalAlignment = HorizontalAlignment.Right;
                    if (Main1.ActualHeight > 55)
                        GridSettings.Height = Main1.ActualHeight - 55;
                    else
                        GridSettings.Height = 0;

                    GridSettings.Margin = new Thickness(0, 25, 0, 0);*/

                    CollectionAllBuff.RemoveAll();
                    foreach (var buff in Host.ItemManager.GetItems())
                    {
                        /*  if (buff.Place == EItemPlace.Equipment)
                              continue;*/

                        if (buff.SpellId == 0)
                            continue;
                        /*  var isbuff = false;
                          Host.log(buff.NameRu);
                          foreach (var effect in buff.GetEffectsData())
                          {
                              Host.log(effect.Effect + " " + effect.ApplyAuraName);
                              if (effect.Effect == ESpellEffectName.APPLY_AURA && effect.ApplyAuraName == EAuraType.DUMMY)
                                  isbuff = true;
                          }*/

                        /* if (!isbuff)
                             continue;*/
                        var item = new BuffTable(buff.Name, buff.Id, buff.SpellId);
                        if (CollectionAllBuff.Any(collectionInvItem => buff.Id == collectionInvItem.ItemId))
                            continue;
                        CollectionAllBuff.Add(item);
                    }
                    //  Host.log(CollectionAllBuff.Count + "  ");

                    CollectionAllSkill.RemoveAll();
                    foreach (var skill in Host.SpellManager.GetSpells())
                    {
                        if (Host.NoShowSkill.Contains(skill.Id))
                            continue;
                        /*  if (skill.SkillLines.Contains(ESkillType.SKILL_MINING))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_HERBALISM))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_ALL_GLYPHS))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_ALL_SPECIALIZATIONS))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_COMPANIONS))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_GENERIC_DND))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_FISHING))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_MOUNTS))
                              continue;
                          if (skill.SkillLines.Contains(ESkillType.SKILL_FIRST_AID))
                              continue;*/




                        try
                        {

                            uint level = 0;
                            if (Host.GameDB.SpellInfoEntries.ContainsKey(skill.Id))
                                level = Host.GameDB.SpellInfoEntries[skill.Id].SpellLevel;
                            if (!skill.IsPassive())
                            {
                                /* Host.log(skill.Name + " " + skill.Id + " " );
                                 foreach (var skillSkillLine in skill.SkillLines)
                                 {
                                     Host.log(skillSkillLine + "");
                                 }*/

                                CollectionAllSkill.Add(new SkillTable(skill.Name, skill.Id, level));
                            }

                        }
                        catch
                        {
                            // ignored
                        }

                        MyGetTalent();
                    }
                }
                else
                {
                    //ComboBoxSwitchMode.Visibility = Visibility.Visible;
                    IsToggle = false;
                    /* GridSettings.Margin = new Thickness(0, 25, 0, 0);
                     GridSettings.Height = 0;
                     GridSettings.Width = 0;*/

                    // if (Main1.Width > 300 || Main1.Height > 400)
                    // {
                    Main1.Width = 300;
                    Main1.Height = 510;
                    // }

                }
                IsToggleDone = true;
            }
            catch (Exception exception)
            {
                IsToggleDone = true;
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Кнопка загрузить настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoadSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var initDir = AssemblyDirectory + "\\Plugins\\Quester\\Configs";
                if (isReleaseVersion)
                    initDir = AssemblyDirectory + "\\Configs";

                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    InitialDirectory = initDir,// Host.AssemblyDirectory + "\\Configs",
                    Filter = @"json files (*.json)|*.json|All files|*.*",
                    RestoreDirectory = true
                };
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                if (Path.GetExtension(openFileDialog.FileName) != ".json")
                    return;
                Host.CfgName = openFileDialog.FileName;
                Host.FileName = openFileDialog.SafeFileName;
                // CharacterSettings characterSettings;
                Host.CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(Host.CfgName, typeof(CharacterSettings), Host.CharacterSettings);


                //  InitFromSettings();
                NeedApplySettings = true;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Кнопка сохранить настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var initDir = AssemblyDirectory + "\\Plugins\\Quester\\Configs";
                if (isReleaseVersion)
                    initDir = AssemblyDirectory + "\\Configs";

                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                {
                    InitialDirectory = initDir,//Host.AssemblyDirectory + "\\Configs",
                    Filter = @"json files (*.json)|*.json|All files|*.*",
                    FileName = Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "]"
                };
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                SettingsNeedSave = false;
                CreateNewSettings();
                Host.CfgName = saveFileDialog.FileName;
                Host.FileName = Path.GetFileName(saveFileDialog.FileName);
                // var writer = new XmlSerializer(typeof(CharacterSettings));

                /*   using (var fs = File.Open(Host.CfgName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                   {
                       writer.Serialize(fs, Host.CharacterSettings);


                   }*/
                //  Host.log(Host.CharacterSettings.PropssSettings.Count + "  Save");
                ConfigLoader.SaveConfig(Host.CfgName, Host.CharacterSettings);
                //  Host.log(Host.CharacterSettings.PropssSettings.Count + "  Save 2");
                _doc = new XmlDocument();
                try
                {
                    // _doc.Load(Host.CfgName);
                }
                catch
                {
                    SettingsNeedSave = true;

                    return;
                }
                NeedApplySettings = true;
                SettingsNeedSave = false;
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Кнопка редактирования активных скилов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonChangeSkill_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (DataGridActiveSkill.SelectedIndex == -1)
                    return;

                var tempNotTargetEffect = Convert.ToInt32(TextBoxNotTargetEffect.Text);
                var tempNotMeEffect = Convert.ToInt32(TextBoxNotMeEffect.Text);
                var tempIsMeEffect = Convert.ToInt32(TextBoxIsMeEffect.Text);
                var tempIsTargetEffect = Convert.ToInt32(TextBoxIsTargetEffect.Text);

                var activeskill = DataGridActiveSkill.SelectedItem as SkillSettings;
                if (activeskill != null)
                {
                    for (var i = 0; i < CollectionActiveSkill.Count; i++)
                    {
                        if (CollectionActiveSkill[i] == activeskill)
                            CollectionActiveSkill[i] = new SkillSettings
                            {
                                Checked = true,
                                Id = activeskill.Id,
                                MeMaxMp = Convert.ToInt32(TextBoxMeMaxMp.Text),
                                MeMaxHp = Convert.ToInt32(TextBoxMeMaxHp.Text),
                                MeMinHp = Convert.ToInt32(TextBoxMeMinHp.Text),
                                MeMinMp = Convert.ToInt32(TextBoxMeMinMp.Text),
                                Name = activeskill.Name,
                                Priority = Convert.ToInt32(TextBoxPriority.Text),

                                TargetMaxHp = Convert.ToInt32(TextBoxTargetMaxHp.Text),
                                TargetMinHp = Convert.ToInt32(TextBoxTargetMinHp.Text),
                                BaseDist = CheckBoxBaseDist.IsChecked != null && CheckBoxBaseDist.IsChecked.Value,
                                MaxDist = Convert.ToInt32(TextBoxMaxDist.Text),
                                MinDist = Convert.ToInt32(TextBoxMinDist.Text),
                                MoveDist = CheckBoxMoveDist.IsChecked != null && CheckBoxMoveDist.IsChecked.Value,
                                AoeMax = Convert.ToInt32(TextBoxAoeMax.Text),
                                AoeMe = RadioButtonAoeMe.IsChecked != null && RadioButtonAoeMe.IsChecked.Value,
                                AoeMin = Convert.ToInt32(TextBoxAoeMin.Text),
                                AoeRadius = Convert.ToInt32(TextBoxAoeRadius.Text),
                                SelfTarget = CheckBoxSelfTarget.IsChecked != null && CheckBoxSelfTarget.IsChecked.Value,
                                NotMeEffect = tempNotMeEffect,
                                NotTargetEffect = tempNotTargetEffect,
                                IsMeEffect = tempIsMeEffect,
                                IsTargetEffect = tempIsTargetEffect,
                                MinMeLevel = Convert.ToInt32(TextBoxMinLevel.Text),
                                MaxMeLevel = Convert.ToInt32(TextBoxMaxLevel.Text),
                                CombatElementCountMore = Convert.ToInt32(TextBoxAlternatePowerMore.Text),
                                CombatElementCountLess = Convert.ToInt32(TextBoxAlternatePowerLess.Text),
                                UseInFight = CheckBoxUseInFight.IsChecked != null && CheckBoxUseInFight.IsChecked.Value,
                                TargetId = Convert.ToInt32(TextBoxTargetId.Text),
                                UseInPVP = CheckBoxUseInPvp.IsChecked.Value,
                                PetMinHp = Convert.ToInt32(TextBoxPetMinHp.Text),
                                PetMaxHp = Convert.ToInt32(TextBoxPetMaxHp.Text),
                                NotTargetId = Convert.ToInt32(TextBoxNotTargetId.Text)
                            };
                    }
                }
                GroupBoxSettingsSkill.Header = "Настройки";
                ButtonChangeSkill.IsEnabled = false;
                ButtonDelSkill.IsEnabled = false;
                ResetSettingSkillDefault();

            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        public void InitFromQuestSettings()
        {
            try
            {
                CheckBoxAutoEquip.Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        CollectionQuestSettings.RemoveAll();
                        foreach (var dungeonCoordSetting in Host.QuestSettings.QuestCoordSettings)
                        {
                            var name = dungeonCoordSetting.QuestName;
                            var minLevel = 0;
                            var level = 0;
                            if (Host.GameDB.QuestTemplates.ContainsKey(dungeonCoordSetting.QuestId))
                            {
                                if (dungeonCoordSetting.QuestName == "")
                                    name = Host.GameDB.QuestTemplates[dungeonCoordSetting.QuestId].LogTitle;
                                minLevel = Host.GameDB.QuestTemplates[dungeonCoordSetting.QuestId].MinLevel;
                                level = Host.GameDB.QuestTemplates[dungeonCoordSetting.QuestId].QuestLevel;
                            }

                            CollectionQuestSettings.Add(new QuestCoordSettings
                            {
                                Run = dungeonCoordSetting.Run,
                                NpcId = dungeonCoordSetting.NpcId,
                                QuestName = name,
                                QuestId = dungeonCoordSetting.QuestId,
                                Loc = dungeonCoordSetting.Loc,
                                State = dungeonCoordSetting.State,
                                MinLevel = minLevel,
                                Level = level,
                            });
                        }
                    }
                    catch (Exception err)
                    {
                        Host.log(err.ToString());
                    }
                });
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void InitFromDungeonSettings()
        {
            try
            {
                CheckBoxAutoEquip.Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        CollectionDungeonCoord.RemoveAll();

                        CollectionDungeonCoord =
                            new ObservableCollection<DungeonCoordSettings>(
                                Host.DungeonSettings.DungeonCoordSettings);
                        foreach (var dungeonCoordSetting in Host.DungeonSettings.DungeonCoordSettings)
                        {
                            CollectionDungeonCoord.Add(new DungeonCoordSettings
                            {
                                Id = dungeonCoordSetting.Id,
                                Action = dungeonCoordSetting.Action,
                                Attack = dungeonCoordSetting.Attack,
                                Loc = dungeonCoordSetting.Loc,
                                MobId = dungeonCoordSetting.MobId,
                                PropId = dungeonCoordSetting.PropId,
                                Pause = dungeonCoordSetting.Pause,
                                ItemId = dungeonCoordSetting.ItemId,
                                Com = dungeonCoordSetting.Com,
                                AreaId = dungeonCoordSetting.AreaId,
                                MapId = dungeonCoordSetting.MapId,
                                SkillId = dungeonCoordSetting.SkillId,
                                PluginPath = dungeonCoordSetting.PluginPath,
                                QuestId = dungeonCoordSetting.QuestId,
                                QuestAction = dungeonCoordSetting.QuestAction
                            });
                        }
                    }
                    catch (Exception err)
                    {
                        Host.log(err.ToString());
                    }
                });
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        /// <summary>
        /// Обновляет интерфейс из characterSettings ------------------------------------------------------------------------------------------------------------------------
        /// </summary>
        public void InitFromSettings()
        {
            try
            {
                CheckBoxAutoEquip.Dispatcher?.Invoke(() =>
                {
                    try
                    {
                        LabelProfile.Content = Host.FileName;
                        //Настройки
                        CheckBoxLearnAllSpell.IsChecked = Host.CharacterSettings.LearnAllSpell;
                        CheckBoxCheckRepairInCity.IsChecked = Host.CharacterSettings.CheckRepairInCity;
                        //Общее
                        CheckBoxCraftConjured.IsChecked = Host.CharacterSettings.CraftConjured;
                        TextBoxCraftConjuredHp.Text = Host.CharacterSettings.CraftConjuredHp.ToString();
                        TextBoxCraftConjuredMp.Text = Host.CharacterSettings.CraftConjuredMp.ToString();
                        CheckBoxAdvancedFight.IsChecked = Host.CharacterSettings.AdvancedFight;
                        CheckBoxRunRun.IsChecked = Host.CharacterSettings.RunRun;
                        TextBoxRunRunPercent.Text = Host.CharacterSettings.RunRunPercent.ToString();
                        CheckBoxUseArrow.IsChecked = Host.CharacterSettings.UseArrow;
                        TextBoxUseArrowId.Text = Host.CharacterSettings.UseArrowId.ToString();
                        CheckBoxTwoWeapon.IsChecked = Host.CharacterSettings.TwoWeapon;
                        CheckBoxPvp.IsChecked = Host.CharacterSettings.Pvp;
                        CheckBoxChangeTargetInCombat.IsChecked = Host.CharacterSettings.ChangeTargetInCombat;
                        CheckBoxAttackMobForDrop.IsChecked = Host.CharacterSettings.AttackMobForDrop;
                        CheckBoxUseStoneIsStuck.IsChecked = Host.CharacterSettings.UseStoneIfStuck;
                        CheckBoxAdvancedEquipShield.IsChecked = Host.CharacterSettings.AdvancedEquipUseShield;
                        // CheckBoxHideQuesterUI.IsChecked = Host.CharacterSettings.HideQuesterUi;
                        CheckBoxAutoEquip.IsChecked = Host.CharacterSettings.AutoEquip;
                        CheckBoxAdvancedEquip.IsChecked = Host.CharacterSettings.AdvancedEquip;
                        CheckBoxRunQuestHerbalism.IsChecked = Host.CharacterSettings.RunQuestHerbalism;
                        CheckBoxUseFilterMobs.IsChecked = Host.CharacterSettings.UseFilterMobs;
                        CheckBoxCheckRepairAndSell.IsChecked = Host.CharacterSettings.CheckRepairAndSell;
                        CheckBoxUnmountMoveFail.IsChecked = Host.CharacterSettings.UnmountMoveFail;
                        CheckBoxWaitSixMin.IsChecked = Host.CharacterSettings.WaitSixMin;
                        CheckBoxFightIfMobs.IsChecked = Host.CharacterSettings.FightIfMobs;
                        CheckBoxLaunchSkript.IsChecked = Host.CharacterSettings.LaunchScript;
                        TextBoxMpRegen.Text = Host.CharacterSettings.MpRegen.ToString();
                        TextBoxHpRegen.Text = Host.CharacterSettings.HpRegen.ToString();
                        CheckBoxUseRegen.IsChecked = Host.CharacterSettings.UseRegen;
                        CheckBoxMultiZone.IsChecked = Host.CharacterSettings.UseMultiZone;
                        CheckBoxRunScriptfrombegin.IsChecked = Host.CharacterSettings.RunScriptFromBegin;
                        CheckBoxAllLog.IsChecked = Host.CharacterSettings.LogAll;
                        CheckBoxUseSMountMyLoc.IsChecked = Host.CharacterSettings.UseMountMyLoc;
                        CheckBoxCheckRepair.IsChecked = Host.CharacterSettings.CheckRepair;
                        TextBoxRepairCount.Text = Host.CharacterSettings.RepairCount.ToString();
                        TextBoxFreeInvCountForAuk.Text = Host.CharacterSettings.FreeInvCountForAuk.ToString();
                        CheckBoxCheckAuk.IsChecked = Host.CharacterSettings.CheckAuk;
                        CheckBoxUseStoneForSellAndRepair.IsChecked = Host.CharacterSettings.UseStoneForSellAndRepair;
                        ComboBoxSummonBattlePetNumber.SelectedIndex = Host.CharacterSettings.BattlePetNumber;
                        TextBoxFreeInvCountForAukId.Text = Host.CharacterSettings.FreeInvCountForAukId.ToString();
                        CheckBoxWorldQuest.IsChecked = Host.CharacterSettings.WorldQuest;
                        CheckBoxDebuffDeath.IsChecked = Host.CharacterSettings.DebuffDeath;
                        CheckBoxAoeFarm.IsChecked = Host.CharacterSettings.AoeFarm;
                        TextBoxAoeMobsCount.Text = Host.CharacterSettings.AoeMobsCount.ToString();
                        CheckBoxAttackForSitMount.IsChecked = Host.CharacterSettings.CheckBoxAttackForSitMount;
                        CheckBoxSummonBattlePet.IsChecked = Host.CharacterSettings.SummonBattlePet;
                        CheckBoxStopQuesting.IsChecked = Host.CharacterSettings.StopQuesting;
                        CheckBoxRandomDistForAttack.IsChecked = Host.CharacterSettings.RandomDistForAttack;
                        TextBoxRandomDistForAttackCount.Text = Host.CharacterSettings.RandomDistForAttackCount.ToString();
                        TextBoxStopQuestingLevel.Text = Host.CharacterSettings.StopQuestingLevel.ToString();
                        CheckBoxAlternateAuk.IsChecked = Host.CharacterSettings.AlternateAuk;
                        if (Host.CharacterSettings.FormForFight == "Не использовать")
                            ComboBoxFormForFight.SelectedIndex = 0;
                        if (Host.CharacterSettings.FormForFight == "Облик медведя")
                            ComboBoxFormForFight.SelectedIndex = 1;
                        if (Host.CharacterSettings.FormForFight == "Облик кошки")
                            ComboBoxFormForFight.SelectedIndex = 2;
                        if (Host.CharacterSettings.FormForFight == "Облик лунного совуха")
                            ComboBoxFormForFight.SelectedIndex = 3;

                        if (Host.CharacterSettings.FormForMove == "Не использовать")
                            ComboBoxFormForMove.SelectedIndex = 0;
                        if (Host.CharacterSettings.FormForMove == "Облик медведя")
                            ComboBoxFormForMove.SelectedIndex = 1;
                        if (Host.CharacterSettings.FormForMove == "Облик кошки")
                            ComboBoxFormForMove.SelectedIndex = 2;
                        if (Host.CharacterSettings.FormForMove == "Походный облик")
                            ComboBoxFormForMove.SelectedIndex = 3;
                        ComboBoxAukTimeClassic.SelectedIndex = Host.CharacterSettings.AukTimeClassic;
                        CheckBoxKillMobsFirst.IsChecked = Host.CharacterSettings.KillMobFirst;
                        ComboBoxAukTime.SelectedIndex = Host.CharacterSettings.AukTime;
                        CheckBoxUseStoneForLearnSpell.IsChecked = Host.CharacterSettings.UseStoneForLearnSpell;
                        TextBoxPetRegen.Text = Host.CharacterSettings.PetRegen.ToString();
                        CheckBoxSendMail.IsChecked = Host.CharacterSettings.SendMail;
                        TextBoxSendMailName.Text = Host.CharacterSettings.SendMailName;
                        CheckBoxCheckSellAndRepairScript.IsChecked = Host.CharacterSettings.CheckSellAndRepairScript;
                        TextBoxSendMailLocX.Text = Host.CharacterSettings.SendMailLocX.ToString();
                        TextBoxSendMailLocY.Text = Host.CharacterSettings.SendMailLocY.ToString();
                        TextBoxSendMailLocZ.Text = Host.CharacterSettings.SendMailLocZ.ToString();
                        TextBoxSendMailLocMapId.Text = Host.CharacterSettings.SendMailLocMapId.ToString();
                        TextBoxSendMailLocAreaId.Text = Host.CharacterSettings.SendMailLocAreaId.ToString();
                        try
                        {
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                            TextBoxSendMailStartTime.Text = Host.CharacterSettings.SendMailStartTime.ToString("hh':'mm");
                            TextBoxSendMailEndTime.Text = Host.CharacterSettings.SendMailStopTime.ToString("hh':'mm");

                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                        }
                        catch (Exception e)
                        {
                            Host.log(e + " ");
                        }

                        CheckBoxKillRunaways.IsChecked = Host.CharacterSettings.KillRunaways;
                        CheckBoxResPetInCombat.IsChecked = Host.CharacterSettings.ResPetInCombat;
                        TextBoxResPetMeMp.Text = Host.CharacterSettings.ResPetMeMp.ToString();
                        CheckBoxFightForSell.IsChecked = Host.CharacterSettings.FightForSell;

                        CheckBoxReturnToCenter.IsChecked = Host.CharacterSettings.ReturnToCenter;
                        TextBoxFreeInvCount.Text = Host.CharacterSettings.InvFreeSlotCount.ToString();
                        CheckBoxUseFly.IsChecked = Host.CharacterSettings.UseFly;
                        CheckBoxScriptReverse.IsChecked = Host.CharacterSettings.ScriptReverse;
                        CheckBoxUseWhistleForSellAndRepair.IsChecked = Host.CharacterSettings.UseWhistleForSellAndRepair;
                        try
                        {
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                            TextBoxCheckAukStartTime.Text = Host.CharacterSettings.StartAukTime.ToString("hh':'mm");
                            TextBoxCheckAukEndTime.Text = Host.CharacterSettings.EndAukTime.ToString("hh':'mm");
                            CheckBoxCheckAukTime.IsChecked = Host.CharacterSettings.CheckAukInTimeRange;
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                        }
                        catch (Exception e)
                        {
                            Host.log(e + " ");
                        }

                        CheckBoxAukRun.IsChecked = Host.CharacterSettings.AukRun;
                        TextBoxAukLocX.Text = Host.CharacterSettings.AukLocX.ToString();
                        TextBoxAukLocY.Text = Host.CharacterSettings.AukLocY.ToString();
                        TextBoxAukLocZ.Text = Host.CharacterSettings.AukLocZ.ToString();
                        TextBoxAukLocMapId.Text = Host.CharacterSettings.AukMapId.ToString();
                        TextBoxAukLocAreaId.Text = Host.CharacterSettings.AukAreaId.ToString();

                        TextBoxGatherLocX.Text = Host.CharacterSettings.GatherLocX.ToString();
                        TextBoxGatherLocY.Text = Host.CharacterSettings.GatherLocY.ToString();
                        TextBoxGatherLocZ.Text = Host.CharacterSettings.GatherLocZ.ToString();
                        TextBoxGatherLocMapId.Text = Host.CharacterSettings.GatherLocMapId.ToString();
                        TextBoxGatherLocAreaId.Text = Host.CharacterSettings.GatherLocAreaId.ToString();
                        TextBoxGatherLocRadius.Text = Host.CharacterSettings.GatherRadius.ToString();
                        CheckBoxRandomJump.IsChecked = Host.CharacterSettings.RandomJump;
                        TextBoxFarmLocX.Text = Host.CharacterSettings.FarmLocX.ToString();
                        TextBoxFarmLocY.Text = Host.CharacterSettings.FarmLocY.ToString();
                        TextBoxFarmLocZ.Text = Host.CharacterSettings.FarmLocZ.ToString();
                        TextBoxFarmLocMapId.Text = Host.CharacterSettings.FarmLocMapId.ToString();
                        TextBoxFarmLocAreaId.Text = Host.CharacterSettings.FarmLocAreaId.ToString();
                        TextBoxFarmLocRadius.Text = Host.CharacterSettings.FarmRadius.ToString();

                        TextBoxStoneLocX.Text = Host.CharacterSettings.StoneLoc.X.ToString();
                        TextBoxStoneLocY.Text = Host.CharacterSettings.StoneLoc.Y.ToString();
                        TextBoxStoneLocZ.Text = Host.CharacterSettings.StoneLoc.Z.ToString();
                        TextBoxStoneLocMapId.Text = Host.CharacterSettings.StoneMapId.ToString();
                        TextBoxStoneLocAreaId.Text = Host.CharacterSettings.StoneAreaId.ToString();
                        CheckBoxRegisterStone.IsChecked = Host.CharacterSettings.StoneRegister;

                        TextBoxMountLocX.Text = Host.CharacterSettings.MountLocX.ToString();
                        TextBoxMountLocY.Text = Host.CharacterSettings.MountLocY.ToString();
                        TextBoxMountLocZ.Text = Host.CharacterSettings.MountLocZ.ToString();
                        TextBoxMountLocAreaId.Text = Host.CharacterSettings.MountLocAreaId.ToString();
                        TextBoxMountLocMapId.Text = Host.CharacterSettings.MountLocMapId.ToString();

                        CheckBoxLogScript.IsChecked = Host.CharacterSettings.LogScriptAction;
                        CheckBoxLogSkill.IsChecked = Host.CharacterSettings.LogSkill;

                        CheckBoxForceMoveScriptEnable.IsChecked = Host.CharacterSettings.ForceMoveScriptEnable;
                        TextBoxForceMoveScriptDist.Text = Host.CharacterSettings.ForceMoveScriptDist.ToString();
                        CheckBoxSummonMount.IsChecked = Host.CharacterSettings.SummonMount;
                        CheckBoxFightIfHpLess.IsChecked = Host.CharacterSettings.FightIfHPLess;
                        TextBoxFightIfHpLessCount.Text = Host.CharacterSettings.FightIfHPLessCount.ToString();

                        CheckBoxSkining.IsChecked = Host.CharacterSettings.Skinning;
                        CheckBoxNoAttackOnMount.IsChecked = Host.CharacterSettings.NoAttackOnMount;
                        CheckBoxScriptScheduleEnable.IsChecked = Host.CharacterSettings.ScriptScheduleEnable;
                        CheckBoxGatherResourceScript.IsChecked = Host.CharacterSettings.GatherResouceScript;
                        TextBoxGatherRadiusScript.Text = Host.CharacterSettings.GatherRadiusScript.ToString();

                        CheckBoxAttack.IsChecked = Host.CharacterSettings.Attack;
                        TextBoxAttackRadius.Text = Host.CharacterSettings.AttackRadius.ToString();

                        CheckBoxUseDash.IsChecked = Host.CharacterSettings.UseDash;

                        TextBoxFarmZRange.Text = Host.CharacterSettings.Zrange.ToString();
                        CheckBoxFindBestPoint.IsChecked = Host.CharacterSettings.FindBestPoint;


                        if (Host.CharacterSettings.Mode == Mode.Questing)
                            ComboBoxSwitchMode.SelectedIndex = 0;
                        if (Host.CharacterSettings.Mode == Mode.FarmMob)
                            ComboBoxSwitchMode.SelectedIndex = 1;
                        if (Host.CharacterSettings.Mode == Mode.FarmResource)
                            ComboBoxSwitchMode.SelectedIndex = 2;
                        if (Host.CharacterSettings.Mode == Mode.Script)
                            ComboBoxSwitchMode.SelectedIndex = 3;
                        if (Host.CharacterSettings.Mode == Mode.OnlyAttack)
                            ComboBoxSwitchMode.SelectedIndex = 4;
                        if (Host.CharacterSettings.Mode == Mode.QuestingClassic)
                            ComboBoxSwitchMode.SelectedIndex = 5;


                        ComboBoxAdvancedEquipQuality.Items.Clear();

                        foreach (var value in Enum.GetValues(typeof(EItemQuality)))
                        {
                            //  Host.log(value.ToString());
                            if (!ComboBoxAdvancedEquipQuality.Items.Contains(value.ToString()))
                                ComboBoxAdvancedEquipQuality.Items.Add(value.ToString());
                        }


                        foreach (var item in ComboBoxAdvancedEquipQuality.Items)
                        {
                            if (item.ToString() == Host.CharacterSettings.MaxItemQuality.ToString())
                                ComboBoxAdvancedEquipQuality.SelectedItem = item;
                        }





                        if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                        {
                            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");
                            foreach (var file in dir.GetFiles())
                            {
                                ComboBoxDungeonScript.Items.Add(file.Name);
                            }
                        }

                        foreach (var item in ComboBoxDungeonScript.Items)
                        {
                            if (item.ToString() == Host.CharacterSettings.Script)
                            {
                                ComboBoxDungeonScript.SelectedItem = item;
                            }
                        }


                        if (Directory.Exists(Host.PathQuestSet))
                        {
                            var dir = new DirectoryInfo(Host.PathQuestSet);
                            foreach (var file in dir.GetFiles())
                            {
                                ComboBoxQuestSet.Items.Add(file.Name);
                            }
                        }
                        foreach (var item in ComboBoxQuestSet.Items)
                        {
                            if (item.ToString() == Host.CharacterSettings.Quest)
                            {
                                ComboBoxQuestSet.SelectedItem = item;
                            }
                        }






                        CheckBoxPickUpLoot.IsChecked = Host.CharacterSettings.PickUpLoot;
                        TextBoxIgnoreMob.Text = Host.CharacterSettings.IgnoreMob.ToString();


                        TextBoxEquipStateWeapon.Text = Host.CharacterSettings.EquipItemStat.ToString();



                        //Умения
                        CollectionActiveSkill.RemoveAll();
                        CollectionInvItems.RemoveAll();
                        CollectionMobs.RemoveAll();
                        CollectionEventSettings.RemoveAll();
                        CollectionPetSettings.RemoveAll();
                        CollectionIgnoreQuest.RemoveAll();
                        CollectionProps.RemoveAll();
                        CollectionMultiZone.RemoveAll();
                        CollectionNpcForActions.RemoveAll();
                        CollectionRegenItems.RemoveAll();
                        CollectionItemGlobals.RemoveAll();
                        CollectionGameObjectIgnores.RemoveAll();
                        CollectionAukSettingses.RemoveAll();
                        CollectionScriptSchedules.RemoveAll();
                        CollectionEquipAuc.RemoveAll();
                        CollectionLearnSkill.RemoveAll();
                        CollectionAdvancedEquipsWeapon.RemoveAll();
                        CollectionAdvancedEquipArmors.RemoveAll();
                        CollectionAdvancedEquipStats.RemoveAll();
                        CollectionLearnTalents.RemoveAll();

                        foreach (var characterSettingsLearnTalent in Host.CharacterSettings.LearnTalents)
                        {
                            CollectionLearnTalents.Add(characterSettingsLearnTalent);
                        }

                        foreach (var characterSettingsAdvancedEquipStat in Host.CharacterSettings.AdvancedEquipStats)
                        {
                            CollectionAdvancedEquipStats.Add(characterSettingsAdvancedEquipStat);
                        }

                        foreach (var characterSettingsAdvancedEquipArmor in Host.CharacterSettings.AdvancedEquipArmors)
                            CollectionAdvancedEquipArmors.Add(characterSettingsAdvancedEquipArmor);

                        foreach (var characterSettingsAdvancedEquip in Host.CharacterSettings.AdvancedEquipsWeapon)
                            CollectionAdvancedEquipsWeapon.Add(characterSettingsAdvancedEquip);

                        foreach (var learnSkill in Host.CharacterSettings.LearnSkill)
                            CollectionLearnSkill.Add(learnSkill);

                        foreach (var characterSettingsEquipAuc in Host.CharacterSettings.EquipAucs)
                            CollectionEquipAuc.Add(characterSettingsEquipAuc);

                        foreach (var characterSettingsScriptSchedule in Host.CharacterSettings.ScriptSchedules)
                            CollectionScriptSchedules.Add(characterSettingsScriptSchedule);

                        foreach (var characterSettingsAukSettingse in Host.CharacterSettings.AukSettingses)
                            CollectionAukSettingses.Add(characterSettingsAukSettingse);

                        foreach (var characterSettingsGameObjectIgnore in Host.CharacterSettings.GameObjectIgnores)
                            CollectionGameObjectIgnores.Add(characterSettingsGameObjectIgnore);

                        foreach (var characterSettingsMyItemGlobal in Host.CharacterSettings.MyItemGlobals)
                            CollectionItemGlobals.Add(characterSettingsMyItemGlobal);

                        foreach (var characterSettingsRegenItemse in Host.CharacterSettings.RegenItemses)
                            CollectionRegenItems.Add(characterSettingsRegenItemse);

                        foreach (var characterSettingsNpcForActionSetting in Host.CharacterSettings.NpcForActionSettings)
                            CollectionNpcForActions.Add(characterSettingsNpcForActionSetting);

                        foreach (var ignoreQuestsSettingse in Host.CharacterSettings.IgnoreQuests)
                            CollectionIgnoreQuest.Add(ignoreQuestsSettingse);

                        foreach (var petSettingse in Host.CharacterSettings.PetSettings)
                            CollectionPetSettings.Add(petSettingse);

                        foreach (var characterSettingsEventSetting in Host.CharacterSettings.EventSettings)
                            CollectionEventSettings.Add(characterSettingsEventSetting);

                        foreach (var characterSettingsMultiZone in Host.CharacterSettings.MultiZones)
                            CollectionMultiZone.Add(characterSettingsMultiZone);

                        foreach (var mobsSettingse in Host.CharacterSettings.MobsSettings)
                            CollectionMobs.Add(mobsSettingse);

                        foreach (var propSettingse in Host.CharacterSettings.PropssSettings)
                            CollectionProps.Add(propSettingse);

                        foreach (var itemSettingse in Host.CharacterSettings.ItemSettings)
                            CollectionInvItems.Add(itemSettingse);

                        foreach (var i in Host.CharacterSettings.SkillSettings)
                            CollectionActiveSkill.Add(i);
                        Host.CommonModule?.SetEquip();
                    }
                    catch (Exception err)
                    {
                        Host.log(err.ToString());
                    }
                });
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        /// <summary>
        /// Кнопка удалить скилл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDelSkill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridActiveSkill.SelectedIndex != -1)
                {
                    CollectionActiveSkill.RemoveAt(DataGridActiveSkill.SelectedIndex);
                    ButtonDelSkill.IsEnabled = false;
                    ButtonChangeSkill.IsEnabled = false;
                    ResetSettingSkillDefault();
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        #endregion



        private void buttonGatherLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*  Host.CharacterSettings.GatherLocX = Host.Me.Location.X;
                  Host.CharacterSettings.GatherLocY = Host.Me.Location.Y;
                  Host.CharacterSettings.GatherLocZ = Host.Me.Location.Z;*/
                TextBoxGatherLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxGatherLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxGatherLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxGatherLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
                TextBoxGatherLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        /// <summary>
        /// Добавить итем в список инвентаря
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxInvItems.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(ComboBoxInvItems.Text));


                /*  if (_collectionInvItems == null)
              {
                  _collectionInvItems = new ObservableCollection<ItemSettings>();
                   listViewInvItems.ItemsSource = _collectionInvItems;
              }*/
                /* if (CollectionInvItems.Any(collectionInvItem => tempId == collectionInvItem.Id))
                     return;*/

                Item item = null;
                foreach (var i in Host.ItemManager.GetItems())
                {
                    if (i.Id != tempId)
                        continue;
                    item = i;

                }
                if (item == null)
                    return;


                CollectionInvItems.Add(new ItemSettings
                {
                    Id = tempId,
                    Name = item.Name,
                    Use = (EItemUse)ComboBoxInvItemsUse.SelectedIndex,
                    //  EnableSale = Host.GameDB.item_info[tempId].mIsSellable,

                    MeLevel = Convert.ToInt32(TextBoxInvMeLevel.Text),
                    Class = item.ItemClass,
                    Quality = item.ItemQuality,
                    AreaId = Convert.ToUInt32(TextBoxItemsLocAreaId1.Text),
                    MaxCount = Convert.ToUInt32(TextBoxItemsMaxCount.Text),
                    MinCount = Convert.ToUInt32(TextBoxItemsMinCount.Text),
                    MapId = Convert.ToInt32(TextBoxItemsLocMapId1.Text),
                    NpcId = Convert.ToUInt32(TextBoxItemsNpcId.Text),
                    Loc = new Vector3F(Convert.ToDouble(TextBoxItemsLocX1.Text), Convert.ToDouble(TextBoxItemsLocY1.Text), Convert.ToDouble(TextBoxItemsLocZ1.Text)),
                });
                //  Host.log(CollectionInvItems.Count + " " + Host.CharacterSettings.ItemSettings.Count);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }




        private void buttonAddAllInvItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                /*   if (_collectionInvItems == null)
               {
                   _collectionInvItems = new ObservableCollection<ItemSettings>();
                      listViewInvItems.ItemsSource = _collectionInvItems;
               }*/

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (CollectionInvItems.Any(collectionInvItem => item.Id == collectionInvItem.Id))
                        continue;

                    CollectionInvItems.Add(new ItemSettings
                    {
                        Id = item.Id,

                        Name = item.Name,
                        Use = (EItemUse)ComboBoxInvItemsUse.SelectedIndex,

                        //EnableSale = item.Db.mIsSellable,
                        //      SellPrice = item.GetSellPrice(),
                        MeLevel = Convert.ToInt32(TextBoxInvMeLevel.Text),
                        Class = item.ItemClass,
                        Quality = item.ItemQuality
                    });
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void comboBoxMobs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxMobs.Items.Clear();
                if (Host.Me == null)
                    return;
                foreach (var creature in Host.GetEntities<Unit>())
                {
                    /* if (creature.Type != EBotTypes.Unit)
                         continue;*/
                    if (!Host.CanAttack(creature, Host.CanSpellAttack))
                        continue;
                    /*  if (!Host.CachedDbNpcInfos.ContainsKey(creature.Id))
                         continue;*/
                    if (!ComboBoxMobs.Items.Contains("[" + creature.Id + "]" + creature.Name))
                        ComboBoxMobs.Items.Add("[" + creature.Id + "]" + creature.Name);
                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxMobs.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(ComboBoxMobs.Text));



                if (CollectionMobs.Any(collectionMobs => tempId == collectionMobs.Id))
                    return;

                Unit creature = null;
                foreach (var obj in Host.GetEntities<Unit>())
                {
                    if (obj.Id != tempId)
                        continue;
                    creature = obj;
                    break;
                }

                if (creature == null)
                    return;
                CollectionMobs.Add(new MobsSettings
                {
                    Id = tempId,
                    Name = creature.Name,
                    Level = creature.Level,
                    Priority = 0,

                    //  Level = Host.GameDB.DBNpcInfos[tempId].Level
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        private void buttonAddAllMobs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var creature in Host.GetEntities<Unit>())
                {

                    if (!Host.CanAttack(creature, Host.CanSpellAttack))
                        continue;
                    /*if (!Host.CachedDbNpcInfos.ContainsKey(creature.Id))
                        continue;*/
                    if (CollectionMobs.Any(collectionMobs => creature.Id == collectionMobs.Id))
                        continue;

                    CollectionMobs.Add(new MobsSettings
                    {
                        Id = creature.Id,
                        Name = creature.Name,
                        Priority = 0,
                        Level = creature.Level

                    });
                }
            }
            catch (Exception exception)
            {

                Host.log(exception.ToString());
            }
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (ComboBoxDungeonAction.SelectedIndex < 1)
                    return;

                var mobId = 0;
                var propId = 0;
                var itemId = 0;
                var skillId = 0;
                if (ComboBoxDungeonMob.SelectedIndex > 0)
                    mobId = GetSkillIdFromCombobox(ComboBoxDungeonMob.Text);
                if (ComboBoxDungeonProp.SelectedIndex > 0)
                    propId = GetSkillIdFromCombobox(ComboBoxDungeonProp.Text);
                if (ComboBoxDungeonItem.SelectedIndex > 0)
                    itemId = GetSkillIdFromCombobox(ComboBoxDungeonItem.Text);
                if (ComboBoxDungeonSkill.SelectedIndex > 0)
                    skillId = GetSkillIdFromCombobox(ComboBoxDungeonSkill.Text);

                /*   if (ComboBoxDungeonAction.SelectedIndex == 3 || ComboBoxDungeonAction.SelectedIndex == 4 ||
                       ComboBoxDungeonAction.SelectedIndex == 6)*/
                var tempLoc = Host.Me.Location;

                QuestAction tempQuestAction = QuestAction.Apply;
                if (ComboBoxDungeonQuestAction.SelectedIndex == 2)
                    tempQuestAction = QuestAction.Run;
                if (ComboBoxDungeonQuestAction.SelectedIndex == 3)
                    tempQuestAction = QuestAction.Complete;




                CollectionDungeonCoord.Add(new DungeonCoordSettings
                {
                    Id = CollectionDungeonCoord.Count,
                    Action = ComboBoxDungeonAction.Text,
                    Loc = tempLoc,
                    MobId = mobId,
                    PropId = Convert.ToUInt32(propId),
                    Attack = CheckBoxAttackMobs.IsChecked != null && CheckBoxAttackMobs.IsChecked.Value,
                    MapId = Convert.ToInt32(TextBoxDungeonLocMapId.Text),
                    AreaId = Convert.ToUInt32(TextBoxDungeonLocAreaId.Text),
                    ItemId = itemId,
                    SkillId = Convert.ToUInt32(skillId),
                    Pause = Convert.ToInt32(TextBoxScriptPause.Text),
                    PluginPath = ComboBoxDungeonPlugin.SelectedItem.ToString(),
                    QuestId = Convert.ToUInt32(TextBoxDungeonQuestId.Text),
                    QuestAction = tempQuestAction,
                    Com = TextBoxDungeonCom.Text
                });

                ComboBoxDungeonMob.SelectedIndex = 0;
                ComboBoxDungeonProp.SelectedIndex = 0;
                ComboBoxDungeonSkill.SelectedIndex = 0;

                ComboBoxDungeonItem.SelectedIndex = 0;
                //   CheckBoxAttackMobs.IsChecked = false;
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void comboBoxDungeonMob_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxDungeonMob.Items.Clear();
                ComboBoxDungeonMob.Items.Add("Не выбрано");



            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void comboBoxDungeonMob2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxDungeonProp.Items.Clear();
                ComboBoxDungeonProp.Items.Add("Не выбрано");
                if (Host.Me == null)
                    return;


                foreach (var prop in Host.GetEntities<GameObject>())
                {

                    if (!ComboBoxDungeonProp.Items.Contains("[" + prop.Id + "]" + prop.Name))
                        ComboBoxDungeonProp.Items.Add("[" + prop.Id + "]" + prop.Name);
                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }



        private void ComboBoxDungeonAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (CheckBoxAttackMobs == null)
                return;

            CheckBoxAttackMobs.Visibility = Visibility.Hidden;
            StackPanelDungeonMob.Visibility = Visibility.Hidden;
            StackPanelDungeonProp.Visibility = Visibility.Hidden;

            StackPanelDungeonItem.Visibility = Visibility.Hidden;



            if (ComboBoxDungeonAction.SelectedIndex == 3)
            {
                CheckBoxAttackMobs.Visibility = Visibility.Visible;
                StackPanelDungeonItem.Visibility = Visibility.Visible;

            }

            if (ComboBoxDungeonAction.SelectedIndex == 4)
            {
                StackPanelDungeonMob.Visibility = Visibility.Visible;
                StackPanelDungeonItem.Visibility = Visibility.Visible;

            }

            if (ComboBoxDungeonAction.SelectedIndex == 6)
            {
                StackPanelDungeonProp.Visibility = Visibility.Visible;
                StackPanelDungeonItem.Visibility = Visibility.Visible;

            }

            if (ComboBoxDungeonAction.SelectedIndex == 1 || ComboBoxDungeonAction.SelectedIndex == 2)
            {

                StackPanelDungeonItem.Visibility = Visibility.Visible;

            }
        }

        private void buttonDungeonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridScript.SelectedIndex == -1)
                return;
            if (ComboBoxDungeonAction.SelectedIndex < 1)
                return;
            var dungeon = DataGridScript.SelectedItem as DungeonCoordSettings;
            if (dungeon != null)
            {
                for (var i = 0; i < CollectionDungeonCoord.Count; i++)
                {
                    if (dungeon == CollectionDungeonCoord[i])
                    {


                        var mobId = 0;
                        var propId = 0;
                        var itemId = 0;
                        var skillId = 0;
                        if (ComboBoxDungeonSkill.SelectedIndex > 0)
                            skillId = GetSkillIdFromCombobox(ComboBoxDungeonSkill.Text);

                        if (ComboBoxDungeonMob.SelectedIndex > 0)
                            mobId = GetSkillIdFromCombobox(ComboBoxDungeonMob.Text);
                        if (ComboBoxDungeonProp.SelectedIndex > 0)
                            propId = GetSkillIdFromCombobox(ComboBoxDungeonProp.Text);
                        if (ComboBoxDungeonItem.SelectedIndex > 0)
                            itemId = GetSkillIdFromCombobox(ComboBoxDungeonItem.Text);

                        var tempLoc = new Vector3F();
                        if (ComboBoxDungeonAction.SelectedIndex == 3 || ComboBoxDungeonAction.SelectedIndex == 4 ||
                            ComboBoxDungeonAction.SelectedIndex == 6)
                            tempLoc = Host.Me.Location;

                        QuestAction tempQuestAction = QuestAction.Apply;
                        if (ComboBoxDungeonQuestAction.SelectedIndex == 2)
                            tempQuestAction = QuestAction.Run;
                        if (ComboBoxDungeonQuestAction.SelectedIndex == 3)
                            tempQuestAction = QuestAction.Complete;

                        CollectionDungeonCoord[i] = new DungeonCoordSettings
                        {
                            Id = i,
                            Action = ComboBoxDungeonAction.Text,
                            Loc = tempLoc,
                            MobId = mobId,
                            PropId = Convert.ToUInt32(propId),
                            Attack = CheckBoxAttackMobs.IsChecked != null && CheckBoxAttackMobs.IsChecked.Value,
                            Pause = Convert.ToInt32(TextBoxScriptPause.Text),
                            ItemId = itemId,
                            Com = TextBoxDungeonCom.Text,
                            SkillId = Convert.ToUInt32(skillId),
                            MapId = Convert.ToInt32(TextBoxDungeonLocMapId.Text),
                            AreaId = Convert.ToUInt32(TextBoxDungeonLocAreaId.Text),
                            QuestId = Convert.ToUInt32(TextBoxDungeonQuestId.Text),
                            QuestAction = tempQuestAction
                        };

                        ComboBoxDungeonMob.SelectedIndex = 0;
                        ComboBoxDungeonProp.SelectedIndex = 0;
                        ComboBoxDungeonSkill.SelectedIndex = 0;
                        ComboBoxDungeonItem.SelectedIndex = 0;
                        CheckBoxAttackMobs.IsChecked = false;
                    }
                }
            }

        }

        private void comboBoxDungeonItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxDungeonItem.Items.Clear();
                ComboBoxDungeonItem.Items.Add("Не выбрано");



                /*   foreach (var item in Host.ItemManager.GetItems())
                   {
                        if (!Host.GameDB.DBPropInfos.ContainsKey(prop.Id))
                             continue;
                       if (!comboBoxDungeonItem.Items.Contains("[" + item.Id + "]" + item.Db.mItemName))
                           comboBoxDungeonItem.Items.Add("[" + item.Id + "]" + item.Db.mItemName);
                   }*/

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }


        private void buttonDungeonSaveScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                {
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\",
                    Filter = @"xml files (*.xml)|*.xml|All files|*.*",
                    FileName = ""
                };
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                SettingsNeedSave = false;
                CreateNewDungeonSettings();
                Host.ScriptName = saveFileDialog.FileName;



                var writer = new XmlSerializer(typeof(DungeonSetting));

                using (var fs = File.Open(Host.ScriptName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    writer.Serialize(fs, Host.DungeonSettings);


                }

                _doc = new XmlDocument();
                try
                {
                    _doc.Load(Host.ScriptName);
                }
                catch
                {
                    SettingsNeedSave = true;
                    return;
                }
                NeedApplyDungeonSettings = true;
                SettingsNeedSave = false;
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }


        private void buttonDungeonLocFill_Click(object sender, RoutedEventArgs e)
        {
            TextBoxDungeonLocX.Text = Host.Me.Location.X.ToString();
            TextBoxDungeonLocY.Text = Host.Me.Location.Y.ToString();
            TextBoxDungeonLocZ.Text = Host.Me.Location.Z.ToString();
            TextBoxDungeonLocMapId.Text = Host.MapID.ToString();
            TextBoxDungeonLocAreaId.Text = Host.Area.Id.ToString();

        }

        private void ButtonUpDungeon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridScript.SelectedIndex < 1)
                    return;

                //  while (Mouse.LeftButton == MouseButtonState.Pressed)
                //  {
                Host.log("1312");
                var index = DataGridScript.SelectedIndex;
                var buf = CollectionDungeonCoord[index];
                CollectionDungeonCoord[index] = CollectionDungeonCoord[index - 1];
                CollectionDungeonCoord[index - 1] = buf;
                DataGridScript.SelectedIndex = index - 1;
                //  }


            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonDownDungeon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridScript.SelectedIndex == -1)
                    return;

                if (DataGridScript.SelectedIndex > CollectionDungeonCoord.Count - 2)
                    return;
                var index = DataGridScript.SelectedIndex;
                var buf = CollectionDungeonCoord[index];
                CollectionDungeonCoord[index] = CollectionDungeonCoord[index + 1];
                CollectionDungeonCoord[index + 1] = buf;
                DataGridScript.SelectedIndex = index + 1;
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxEventType.SelectedIndex == -1 || ComboBoxEventAction.SelectedIndex == -1)
                    return;
                var action = EventsAction.NotSellected;
                var type = EventsType.NotSellected;

                if (ComboBoxEventAction.SelectedIndex == 0)
                    action = EventsAction.Log;
                if (ComboBoxEventAction.SelectedIndex == 1)
                    action = EventsAction.ExitGame;
                if (ComboBoxEventAction.SelectedIndex == 2)
                    action = EventsAction.PlaySound;
                if (ComboBoxEventAction.SelectedIndex == 3)
                    action = EventsAction.Pause;
                if (ComboBoxEventAction.SelectedIndex == 4)
                    action = EventsAction.ShowGameClient;
                if (ComboBoxEventAction.SelectedIndex == 5)
                    action = EventsAction.ShowQuester;

                if (ComboBoxEventType.SelectedIndex == 0)
                    type = EventsType.Inactivity;
                if (ComboBoxEventType.SelectedIndex == 1)
                    type = EventsType.Death;
                if (ComboBoxEventType.SelectedIndex == 2)
                    type = EventsType.DeathPlayer;
                if (ComboBoxEventType.SelectedIndex == 3)
                    type = EventsType.ChatMessage;
                if (ComboBoxEventType.SelectedIndex == 4)
                    type = EventsType.Gm;
                if (ComboBoxEventType.SelectedIndex == 5)
                    type = EventsType.AttackPlayer;
                if (ComboBoxEventType.SelectedIndex == 6)
                    type = EventsType.PartyInvite;
                if (ComboBoxEventType.SelectedIndex == 7)
                    type = EventsType.ClanInvite;
                if (ComboBoxEventType.SelectedIndex == 8)
                    type = EventsType.GmServer;



                CollectionEventSettings.Add(new EventSettings
                {
                    ActionEvent = action,
                    TypeEvents = type,
                    SoundFile = TextBox.Text,
                    Pause = Convert.ToInt32(TextBoxCopy.Text),
                });

            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        private void comboBoxInvItems1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                ComboBoxInvItems1.Items.Clear();

                if (Host.Me == null)
                    return;

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (!item.IsSoulBound)
                        continue;
                    if (Host.GameDB.SpellInfoEntries.ContainsKey(item.SpellId))
                    {
                        var spellDb = Host.GameDB.SpellInfoEntries[item.SpellId];
                        if (spellDb.Effects != null)
                            foreach (var spellDbEffect in spellDb.Effects)
                            {
                                if (spellDbEffect.Value != null)
                                    foreach (var spellEffectInfo in spellDbEffect.Value)
                                    {
                                        if (spellEffectInfo?.ApplyAuraName == EAuraType.MOUNTED)
                                            ComboBoxInvItems1.Items.Add("[" + item.Id + "]" + item.Name);
                                    }
                            }
                    }
                }

                foreach (var item in Host.SpellManager.GetSpells())
                {
                    if (Host.GameDB.SpellInfoEntries.ContainsKey(item.Id))
                    {
                        var spellDb = Host.GameDB.SpellInfoEntries[item.Id];
                        if (spellDb.Effects != null)
                            foreach (var spellDbEffect in spellDb.Effects)
                            {
                                if (spellDbEffect.Value != null)
                                    foreach (var spellEffectInfo in spellDbEffect.Value)
                                    {
                                        if (spellEffectInfo?.ApplyAuraName == EAuraType.MOUNTED)
                                            ComboBoxInvItems1.Items.Add("[" + item.Id + "]" + item.Name);
                                    }
                            }
                    }

                    if (item.IsPartOfSkillLine(777))
                        ComboBoxInvItems1.Items.Add("[" + item.Id + "]" + item.Name);
                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxInvItems1.SelectedIndex == -1)
                    return;


                var tempId = GetSkillIdFromCombobox(ComboBoxInvItems1.Text);
                SpellInfo mount = null;
                EMountType mountType = EMountType.Spell;



                foreach (var i in Host.SpellManager.GetSpells())
                {
                    if (!Host.GameDB.SpellInfoEntries.ContainsKey(i.Id))
                        continue;
                    var spellDb = Host.GameDB.SpellInfoEntries[i.Id];



                    if (i.Id == tempId)
                    {
                        if (spellDb.Effects != null)
                        {
                            foreach (var spellDbEffect in spellDb.Effects)
                            {
                                if (spellDbEffect.Value == null)
                                    continue;
                                foreach (var spellEffectInfo in spellDbEffect.Value)
                                {
                                    if (spellEffectInfo?.ApplyAuraName != EAuraType.MOUNTED)
                                        continue;
                                    mount = spellDb;
                                    mountType = EMountType.Spell;
                                }

                            }
                        }
                    }




                    if (i.IsPartOfSkillLine(777))
                    {
                        if (i.Id == tempId)
                        {
                            mount = spellDb;
                            mountType = EMountType.Spell;
                        }
                    }
                }

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (Host.GameDB.SpellInfoEntries.ContainsKey(item.SpellId))
                    {
                        var spellDb = Host.GameDB.SpellInfoEntries[item.SpellId];
                        if (spellDb.Effects != null)
                            foreach (var spellDbEffect in spellDb.Effects)
                            {
                                if (spellDbEffect.Value != null)
                                    foreach (var spellEffectInfo in spellDbEffect.Value)
                                    {
                                        if (spellEffectInfo?.ApplyAuraName == EAuraType.MOUNTED)
                                        {
                                            mount = spellDb;
                                            mountType = EMountType.Item;
                                        }
                                    }
                            }
                    }
                }

                if (mount == null)
                    return;

                if (CollectionPetSettings.Any(collectionInvItem => tempId == collectionInvItem.Id))
                    return;

                CollectionPetSettings.Add(new PetSettings
                {
                    Id = tempId,
                    Name = mount.SpellName,
                    Type = "Mount",
                    MountType = mountType
                    // Name = Host.GameDB.DBPetInfos[tempId].LocalName,
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }







        private void button4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Host.Me.Target == null)
                {
                    Host.log("Нет цели ", LogLvl.Error);
                    return;
                }

                if (Host.GetNpcQuestDialogs().Count == 0)
                {
                    Host.log("Нет квестов в диалоге");
                }

                foreach (var gossipOptionsData in Host.GetNpcQuestDialogs())
                {
                    if (CollectionQuestSettings.Any(collectionInvItem => gossipOptionsData.QuestID == collectionInvItem.QuestId))
                        continue;

                    Host.log(gossipOptionsData.QuestID + " " + gossipOptionsData.QuestTitle + " ");

                    var item = new QuestCoordSettings
                    {
                        Run = true,
                        QuestId = gossipOptionsData.QuestID,
                        QuestName = gossipOptionsData.QuestTitle,
                        NpcId = Host.Me.Target.Id,
                        Loc = Host.Me.Target.Location
                    };


                    if (ListViewQuest.SelectedIndex != -1)
                    {
                        if (ListViewQuest.SelectedIndex + 1 > ListViewQuest.Items.Count - 1)
                        {
                            CollectionQuestSettings.Add(item);
                        }
                        else
                        {
                            CollectionQuestSettings.Insert(ListViewQuest.SelectedIndex + 1, item);
                        }

                    }
                    else
                    {
                        CollectionQuestSettings.Add(item);
                    }

                    ListViewQuest.ScrollIntoView(item);


                }


                /* if (textBoxQuestId.Text == "0" || textBoxNpcId.Text == "0")
                     return;



                 if (CollectionQuestSettings.Any(collectionInvItem => Convert.ToInt32(textBoxQuestId) == collectionInvItem.QuestId))
                     return;

                 var questId = Convert.ToUInt32(textBoxQuestId.Text);
                 var state = "Не известно";
                 if (Host.GetQuest(questId) != null)
                     state = Host.GetQuest(questId).State.ToString();

                 CollectionQuestSettings.Add(new QuestCoordSettings
                 {
                     QuestId = questId,
                     NpcId = Convert.ToUInt32(textBoxNpcId.Text),
                     State = state
                     //  Name = Host.GameDB.quest[tempId].mName,
                 });*/
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }







        private void textBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    InitialDirectory = AssemblyDirectory + "\\Configs",
                    Filter = @"wav files (*.wav)|*.wav|All files|*.*",
                    RestoreDirectory = true
                };
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                if (Path.GetExtension(openFileDialog.FileName) != ".wav")
                    return;
                TextBox.Text = openFileDialog.FileName;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonClearAllMobs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CollectionMobs.RemoveAll();
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonResetStat_Click(object sender, RoutedEventArgs e)
        {
            double tempGold = Host.Me.Money;
            Host.StartGold = tempGold / 10000;
            Host.Startinvgold = 0;


            Host.TimeWork = DateTime.Now;
            Host.KillMobsCount = 0;
            Host.AllDamage = 0;
            Host.TimeInFight = 0;
        }

        private void comboBoxProp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxProp.Items.Clear();
                if (Host.Me == null)
                    return;
                foreach (var prop in Host.GetEntities<GameObject>())
                {
                    if (prop.GameObjectType == EGameObjectType.GatheringNode || prop.GameObjectType == EGameObjectType.Chest)
                    {
                        if (!ComboBoxProp.Items.Contains("[" + prop.Id + "]" + prop.Name))
                            ComboBoxProp.Items.Add("[" + prop.Id + "]" + prop.Name);
                    }


                }


            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void buttonAddProp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxProp.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(ComboBoxProp.Text));



                if (CollectionProps.Any(collectionprop => tempId == collectionprop.Id))
                    return;


                GameObject gogAtherNode = null;
                foreach (var i in Host.GetEntities<GameObject>())
                {
                    /*  if (i.GameObjectType != EGameObjectType.GatheringNode)
                          continue;*/

                    if (i.Id == tempId)
                        gogAtherNode = i;
                }
                if (gogAtherNode == null)
                    return;

                CollectionProps.Add(new PropSettings
                {
                    Id = tempId,
                    Name = gogAtherNode.Name,
                    Priority = 0,


                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonPropsDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewProp.SelectedIndex != -1)
                    CollectionProps.RemoveAt(ListViewProp.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonClearAllProps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CollectionProps.RemoveAll();
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonAddAllProps_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var prop in Host.GetEntities<GameObject>())
                {
                    /*  if (prop.GameObjectType != EGameObjectType.GatheringNode)
                          continue;*/
                    if (CollectionProps.Any(collectionprop => prop.Id == collectionprop.Id))
                        continue;

                    CollectionProps.Add(new PropSettings
                    {
                        Id = prop.Id,
                        Name = prop.Name,
                        Priority = 0,

                    });
                }
            }
            catch (Exception exception)
            {

                Host.log(exception.ToString());
            }
        }



        private void buttonAddMultiZone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tempLoc = Host.Me.Location;

                var listMob = new List<uint>();
                var delim = new[] { ';', ':', ',' };
                string[] inpstr;
                inpstr = TextBoxMultiZoneListMob.Text.Split(delim);
                for (var index = 0; index < inpstr.Length; index++)
                {

                    var s = inpstr[index];
                    if (s == "")
                        continue;
                    s = s.Trim();
                    Host.log(s);
                    if (s == "")
                        continue;
                    listMob.Add(Convert.ToUInt32(s));

                }


                CollectionMultiZone.Add(new MultiZone
                {
                    Id = CollectionMultiZone.Count,
                    Loc = tempLoc,
                    ChangeByTime = CheckBoxChangeTime.IsChecked.Value,
                    Time = Convert.ToInt32(TextBoxMultiZoneTime.Text),
                    Radius = Convert.ToInt32(TextBoxMultiZoneRadius.Text),
                    //ChangeByDeathPlayer = CheckBoxChangeDeathByPlayer.IsChecked.Value,
                    // CountDeathByPlayer = Convert.ToInt32(textBoxCountDeathByPlayer.Text),
                    ListMobs = listMob,
                    UseFilter = CheckBoxUseFilterMultiZone.IsChecked.Value,
                    ChangeByLevel = CheckBoxChangeByLevel.IsChecked.Value,

                    MapId = Convert.ToInt32(TextBoxFarmMultiZoneMapId.Text),
                    AreaId = Convert.ToUInt32(TextBoxFarmMultiZoneAreaId.Text),
                    MinLevel = Convert.ToInt32(TextBoxMultiZoneMinLevel.Text),
                    MaxLevel = Convert.ToInt32(TextBoxMultiZoneMaxLevel.Text),
                    ChangeByPlayer = CheckBoxChangeByPlayer.IsChecked.Value,
                    TimePlayer = Convert.ToInt32(TextBoxChangeByPlayerTime.Text)
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonDelMultiZone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridMultiZone.SelectedIndex != -1)
                    CollectionMultiZone.RemoveAt(DataGridMultiZone.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        private void buttonMultiFarmEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridMultiZone.SelectedIndex == -1)
                return;

            var multiZone = DataGridMultiZone.SelectedItem as MultiZone;
            if (multiZone != null)
            {
                for (var i = 0; i < CollectionMultiZone.Count; i++)
                {
                    if (multiZone == CollectionMultiZone[i])
                    {

                        var tempLoc = new Vector3F(Convert.ToSingle(TextBoxMultiZoneX.Text), Convert.ToSingle(TextBoxMultiZoneY.Text), Convert.ToSingle(TextBoxMultiZoneZ.Text));


                        CollectionMultiZone[i] = new MultiZone()
                        {
                            Id = i,
                            Loc = tempLoc,
                            ChangeByTime = CheckBoxChangeTime.IsChecked.Value,
                            Time = Convert.ToInt32(TextBoxMultiZoneTime.Text),
                            Radius = Convert.ToInt32(TextBoxMultiZoneRadius.Text),
                            //  ChangeByDeathPlayer = CheckBoxChangeDeathByPlayer.IsChecked.Value,
                            //  CountDeathByPlayer = Convert.ToInt32(textBoxCountDeathByPlayer.Text),


                        };

                        ComboBoxDungeonMob.SelectedIndex = 0;
                        ComboBoxDungeonProp.SelectedIndex = 0;

                        ComboBoxDungeonItem.SelectedIndex = 0;
                        CheckBoxAttackMobs.IsChecked = false;
                    }
                }
                ButtonMultiZoneLocFill.IsEnabled = false;
            }

        }

        private void buttonFarmLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxFarmLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxFarmLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxFarmLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxFarmLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxFarmLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void buttonMultiZoneLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxMultiZoneX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxMultiZoneY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxMultiZoneZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxFarmMultiZoneMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxFarmMultiZoneAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }



        private void buttonDungeonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBoxDungeonLocAreaId.Text == "0" && TextBoxDungeonLocMapId.Text == "0")
                {
                    // ReSharper disable once RedundantNameQualifier
                    System.Windows.MessageBox.Show("Необходимо указать AreaId и MapId", "Ошибка", MessageBoxButton.OK);
                    return;
                }


                var initDir = AssemblyDirectory + "\\Plugins\\Quester\\Configs";
                if (isReleaseVersion)
                    initDir = AssemblyDirectory + "\\Configs";

                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    InitialDirectory = initDir,// Host.AssemblyDirectory + "\\Configs",
                    Filter = @"xml files (*.xml)|*.xml|All files|*.*",
                    RestoreDirectory = true
                };
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                if (Path.GetExtension(openFileDialog.FileName) != ".xml")
                    return;
                //  Host.CfgName = openFileDialog.FileName;
                // Host.FileName = openFileDialog.SafeFileName;
                Host.log(openFileDialog.FileName + "   " + openFileDialog.SafeFileName);
                var lines = File.ReadAllLines(openFileDialog.FileName);

                var delim = new[] { '"', ':' };
                string[] inpstr;

                for (var i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("Vector3 X"))
                        continue;
                    // Host.log(lines[i]);


                    inpstr = lines[i].Split(delim);


                    CollectionDungeonCoord.Add(new DungeonCoordSettings
                    {
                        Id = CollectionDungeonCoord.Count,
                        Action = "Бежать на точку",
                        Loc = new Vector3F(Convert.ToSingle(inpstr[1]), Convert.ToSingle(inpstr[3]), Convert.ToSingle(inpstr[5])),
                        MobId = 0,
                        PropId = 0,
                        Attack = CheckBoxAttackMobs.IsChecked != null && CheckBoxAttackMobs.IsChecked.Value,

                        ItemId = 0,
                        AreaId = Convert.ToUInt32(TextBoxDungeonLocAreaId.Text),
                        MapId = Convert.ToInt32(TextBoxDungeonLocMapId.Text)
                    });

                }

            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void comboBoxDungeonScript_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (ComboBoxDungeonScript.Items != null)
                {
                    if (ComboBoxDungeonScript.Items.Count > 0)
                        ComboBoxDungeonScript?.Items.Clear();
                    if (ComboBoxDungeonScriptCopy.Items.Count > 0)
                        ComboBoxDungeonScriptCopy?.Items.Clear();

                }

                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                {
                    var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");
                    // папка с файлами
                    ComboBoxDungeonScript.Items.Add("Не выбрано");
                    foreach (var file in dir.GetFiles())
                    {
                        ComboBoxDungeonScript.Items.Add(file.Name);
                        ComboBoxDungeonScriptCopy.Items.Add(file.Name);
                    }
                }
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }



        private void buttonDungeonDelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CollectionDungeonCoord.RemoveAll();
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        private void buttonAddNpc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var obj in Host.GetEntities<Unit>())
                {
                    if (CollectionNpcForActions.Any(collectionprop => obj.Id == collectionprop.Id))
                        continue;

                    if (obj.IsVendor || obj.IsArmorer || obj.IsAuctioner || obj.IsBanker || obj.IsTaxi)
                    {
                        CollectionNpcForActions.Add(new NpcForAction
                        {
                            Use = true,
                            Id = obj.Id,
                            Name = obj.Name,
                            Loc = obj.Location,
                            AreaId = Host.Area.Id,
                            MapId = Host.MapID,
                            IsArmorer = obj.IsArmorer,
                            IsVendor = obj.IsVendor,
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        private void buttonScriptSavePointMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonScriptSavePointMove.Content.ToString() == "Запись точек в движении(выкл)")
                {
                    // On = true;
                    On = false;
                    Host.AutoQuests.SavePointMove = true;
                    ButtonScriptSavePointMove.Content = "Запись точек в движении(вкл)";
                    ButtonOnOff.IsEnabled = false;
                }
                else
                {
                    Host.AutoQuests.SavePointMove = false;
                    ButtonOnOff.IsEnabled = true;
                    //  On = false;
                    //  Host.NeedRestart = true;

                    // Host.CancelMoveTo();
                    // Host.FarmModule.farmState = Modules.FarmState.Disabled;                  
                    ButtonScriptSavePointMove.Content = "Запись точек в движении(выкл)";
                }


            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonAddSkillAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var s in Host.SpellManager.GetSpells())
                {
                    if (s.IsPassive())
                        continue;
                    if (s.DescriptionRu == "")
                        continue;
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (s.GetMaxCastRange() == 0)
                        continue;

                    if (s.DescriptionRu.Contains("ед. урона") || s.DescriptionRu.Contains("физический урон") || s.DescriptionRu.Contains("физического урона"))
                    {

                        /* Host.log(s.Id + " " + "  " + s.Name + " IsPassive =  " + s.IsPassive() + "  ");
                         Host.log("DescriptionRu: " + s.DescriptionRu, LogLvl.Important);
                         Host.log("AuraDescriptionRu: " + s.AuraDescriptionRu + " \n");*/
                        if (CollectionActiveSkill.Any(collectionInvItem => s.Id == collectionInvItem.Id))
                            continue;

                        var cd = s.ChargeRecoveryTime + s.RecoveryTime;
                        var priority = 0;
                        if (cd > 0)
                            priority = 1;
                        if (cd > 5000)
                            priority = 2;
                        if (cd > 10000)
                            priority = 3;

                        if (s.Id == 116)//Ледяная стрела
                            priority = 1;

                        CollectionActiveSkill.Add(new SkillSettings
                        {
                            Checked = true,
                            Id = s.Id,
                            MeMaxMp = Convert.ToInt32(TextBoxMeMaxMp.Text),
                            MeMaxHp = Convert.ToInt32(TextBoxMeMaxHp.Text),
                            MeMinHp = Convert.ToInt32(TextBoxMeMinHp.Text),
                            MeMinMp = Convert.ToInt32(TextBoxMeMinMp.Text),
                            Name = s.Name,
                            Priority = priority,

                            TargetMaxHp = Convert.ToInt32(TextBoxTargetMaxHp.Text),
                            TargetMinHp = Convert.ToInt32(TextBoxTargetMinHp.Text),
                            BaseDist = CheckBoxBaseDist.IsChecked != null && CheckBoxBaseDist.IsChecked.Value,
                            MaxDist = Convert.ToInt32(s.GetMaxCastRange()),
                            MinDist = Convert.ToInt32(s.GetMinCastRange()),
                            MoveDist = CheckBoxMoveDist.IsChecked != null && CheckBoxMoveDist.IsChecked.Value,
                            AoeMax = Convert.ToInt32(TextBoxAoeMax.Text),
                            AoeMe = RadioButtonAoeMe.IsChecked != null && RadioButtonAoeMe.IsChecked.Value,
                            AoeMin = Convert.ToInt32(TextBoxAoeMin.Text),
                            AoeRadius = Convert.ToInt32(TextBoxAoeRadius.Text),
                            SelfTarget = CheckBoxSelfTarget.IsChecked != null && CheckBoxSelfTarget.IsChecked.Value,
                            /*   NotTargetEffect = tempNotTargetEffect,
                               NotMeEffect = tempNotMeEffect,
                               IsMeEffect = tempIsMeEffect,
                               IsTargetEffect = tempIsTargetEffect,*/
                            MinMeLevel = Convert.ToInt32(TextBoxMinLevel.Text),
                            MaxMeLevel = Convert.ToInt32(TextBoxMaxLevel.Text),

                            UseInFight = CheckBoxUseInFight.IsChecked != null && CheckBoxUseInFight.IsChecked.Value,

                            UseInPVP = CheckBoxUseInPvp.IsChecked.Value
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonAddBuff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridAllBuff.SelectedIndex == -1)
                    return;


                var tempNotMeEffect = Convert.ToUInt32(TextBoxNotMeEffect1.Text);

                var tempIsMeEffect = Convert.ToUInt32(TextBoxIsMeEffect1.Text);


                if (DataGridAllBuff.SelectedItem is BuffTable path)
                    CollectionRegenItems.Add(new RegenItems
                    {
                        Checked = true,
                        ItemId = path.ItemId,
                        SpellId = path.SkillId,
                        MeMaxMp = Convert.ToInt32(TextBoxMeMaxMp1.Text),
                        MeMaxHp = Convert.ToInt32(TextBoxMeMaxHp1.Text),
                        MeMinHp = Convert.ToInt32(TextBoxMeMinHp1.Text),
                        MeMinMp = Convert.ToInt32(TextBoxMeMinMp1.Text),
                        Name = path.ItemName,
                        Priority = Convert.ToInt32(TextBoxPriority1.Text),
                        IsMeEffect = tempIsMeEffect,
                        NotMeEffect = tempNotMeEffect,
                        // WaitEndBuff = CheckBoxWaitEndBuff.IsChecked.Value,
                        InFight = CheckBoxUseInFight1.IsChecked != null && CheckBoxUseInFight1.IsChecked.Value,
                        // UseSkillAfterSkill = CheckBoxUseSkillAfterSkill.IsChecked.Value,
                        // UseSkillAfterSkillId = Convert.ToUInt32(TextBoxUseSkillAfterSkillId.Text),
                        // UseKeyAfterItem = CheckBoxUseKeyAfterItem.IsChecked.Value,
                        //  UseKeyAfterItemKey = ComboBoxUseKeyAfterItemKey.SelectedIndex,
                        //  Level = Convert.ToInt32(TextBoxRegenItemLevel.Text)

                    }

                    );
                //сброс настроек по умолчанию
                // ResetSettingSkillDefault();
                GroupBoxSettingsSkill1.Header = "Настройки";
                ButtonAddBuff.IsEnabled = false;

            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ListViewAllItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DataGridAllBuff.SelectedIndex == -1)
                    return;
                DataGridActiveSkill.SelectedIndex = -1;

                ButtonAddBuff.IsEnabled = true;

                ButtonDelBuff.IsEnabled = false;
                var skill = DataGridAllBuff.SelectedItem as BuffTable;
                if (skill != null)
                {
                    /*  GroupBoxSettingsSkill.Header = "Настройки " + skill.Name + "[" + skill.Id + "]";
                      TextBoxMinDist.Text =
                          Host.SpellManager.GetSpell(skill.Id).GetMinCastRange().ToString(
                              CultureInfo.InvariantCulture);
                      TextBoxMaxDist.Text =
                           Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange().ToString(
                              CultureInfo.InvariantCulture);*/
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonDelBuff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridAllBuff.Dispatcher?.Invoke(() =>
                {
                    if (DataGridRegenItems.SelectedIndex != -1)
                    {
                        CollectionRegenItems.RemoveAt(DataGridRegenItems.SelectedIndex);
                        ButtonDelBuff.IsEnabled = false;
                    }
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ListViewActiveItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridRegenItems.SelectedIndex == -1)
                return;


            DataGridAllBuff.SelectedIndex = -1;

            ButtonDelBuff.IsEnabled = true;
            ButtonAddBuff.IsEnabled = false;
            if (DataGridRegenItems.SelectedItem is RegenItems skill)
            {
                GroupBoxSettingsSkill1.Header = "Настройки " + skill.Name + "[" + skill.ItemId + "]";
                TextBoxPriority1.Text = skill.Priority.ToString();
                CheckBoxUseInFight1.IsChecked = skill.InFight;

                TextBoxMeMinHp1.Text = skill.MeMinHp.ToString();
                TextBoxMeMaxHp1.Text = skill.MeMaxHp.ToString();
                TextBoxMeMinMp1.Text = skill.MeMinMp.ToString();
                TextBoxMeMaxMp1.Text = skill.MeMaxMp.ToString();
                TextBoxNotMeEffect1.Text = skill.IsMeEffect.ToString();
                TextBoxIsMeEffect1.Text = skill.IsMeEffect.ToString();



            }
        }

        private void buttonFarmLocFill1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxMountLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxMountLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxMountLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxMountLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxMountLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void comboBoxDungeonSkill_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxDungeonSkill.Items.Clear();
                ComboBoxDungeonSkill.Items.Add("Не выбрано");
                if (Host.Me == null)
                    return;


                foreach (var skill in Host.SpellManager.GetSpells())
                {
                    if (skill.SkillLines.Contains(186))//   SKILL_MINING 
                        continue;
                    if (skill.SkillLines.Contains(182))//SKILL_HERBALISM                      
                        continue;
                    if (skill.SkillLines.Contains(810))
                        continue;
                    if (skill.SkillLines.Contains(934))
                        continue;
                    if (skill.SkillLines.Contains(778))
                        continue;
                    if (skill.SkillLines.Contains(183))
                        continue;
                    if (skill.SkillLines.Contains(356))
                        continue;
                    if (skill.SkillLines.Contains(777))
                        continue;
                    if (skill.SkillLines.Contains(129))
                        continue;
                    if (skill.IsPassive())
                        continue;

                    /* if (item.Db.Target != ESpellTarget.Self)
                         continue;*/
                    if (!ComboBoxDungeonSkill.Items.Contains("[" + skill.Id + "]" + skill.Name))
                        ComboBoxDungeonSkill.Items.Add("[" + skill.Id + "]" + skill.Name);
                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void comboBoxInvItemsGlobalClass_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in Enum.GetValues(typeof(EItemClass)))
            {
                //  Host.log(value.ToString());
                if (!ComboBoxInvItemsGlobalClass.Items.Contains(value.ToString()))
                    ComboBoxInvItemsGlobalClass.Items.Add(value.ToString());
            }
        }

        private void comboBoxInvItemsGlobalQuality_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in Enum.GetValues(typeof(EItemQuality)))
            {
                //  Host.log(value.ToString());
                if (!ComboBoxInvItemsGlobalQuality.Items.Contains(value.ToString()))
                    ComboBoxInvItemsGlobalQuality.Items.Add(value.ToString());
            }
        }

        private void buttonAddItemGlobal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxInvItemsGlobalQuality.SelectedIndex == -1 || ComboBoxInvItemsGlobalClass.SelectedIndex == -1)
                    return;


                CollectionItemGlobals.Add(new ItemGlobal
                {
                    Class = (EItemClass)Enum.Parse(typeof(EItemClass), ComboBoxInvItemsGlobalClass.Text),
                    Quality = (EItemQuality)Enum.Parse(typeof(EItemQuality), ComboBoxInvItemsGlobalQuality.Text),
                    ItemLevel = Convert.ToUInt32(TextBoxItemLevel.Text)
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }





        private void comboBoxQuestSet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (ComboBoxQuestSet.Items != null)
                {
                    if (ComboBoxQuestSet.Items.Count > 0)
                        ComboBoxQuestSet?.Items.Clear();
                }

                if (Directory.Exists(Host.PathQuestSet))
                {
                    var dir = new DirectoryInfo(Host.PathQuestSet);
                    // папка с файлами
                    foreach (var file in dir.GetFiles())
                    {
                        ComboBoxQuestSet.Items.Add(file.Name);
                    }
                }
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void buttonSaveQuest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                {
                    InitialDirectory = Host.PathQuestSet,
                    Filter = @"xml files (*.xml)|*.xml|All files|*.*",
                    FileName = ""
                };
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                SettingsNeedSave = false;
                CreateNewQuestSettings();
                Host.QuestName = saveFileDialog.FileName;



                var writer = new XmlSerializer(typeof(QuestSetting));

                using (var fs = File.Open(Host.QuestName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    writer.Serialize(fs, Host.QuestSettings);


                }

                _doc = new XmlDocument();
                try
                {
                    _doc.Load(Host.QuestName);
                }
                catch
                {
                    SettingsNeedSave = true;
                    return;
                }
                NeedApplyQuestSettings = true;
                SettingsNeedSave = false;
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonUpQuest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewQuest.SelectedIndex < 1)
                    return;

                //  while (Mouse.LeftButton == MouseButtonState.Pressed)
                //  {
                Host.log("1312");
                var index = ListViewQuest.SelectedIndex;
                var buf = CollectionQuestSettings[index];
                CollectionQuestSettings[index] = CollectionQuestSettings[index - 1];
                CollectionQuestSettings[index - 1] = buf;
                ListViewQuest.SelectedIndex = index - 1;
                //  }


            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonDownQuest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewQuest.SelectedIndex == -1)
                    return;
                if (ListViewQuest.SelectedIndex > CollectionQuestSettings.Count - 2)
                    return;
                var index = ListViewQuest.SelectedIndex;
                var buf = CollectionQuestSettings[index];
                CollectionQuestSettings[index] = CollectionQuestSettings[index + 1];
                CollectionQuestSettings[index + 1] = buf;
                ListViewQuest.SelectedIndex = index + 1;
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonAddResourceIgnore_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var gameObject in Host.GetEntities<GameObject>())
                {
                    /* if (gameObject.GameObjectType != EGameObjectType.GatheringNode)
                         continue;*/
                    CollectionGameObjectIgnores.Add(new GameObjectIgnore
                    {
                        Id = gameObject.Id,
                        Name = gameObject.Name,
                        Ignore = true,
                        Loc = gameObject.Location
                    });
                }



            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonResourceIgnore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridResourceIgnore.SelectedIndex != -1)
                    CollectionGameObjectIgnores.RemoveAt(DataGridResourceIgnore.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ComboBoxAuk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxAuk.Items.Clear();
                if (Host.Me == null)
                    return;


                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.IsSoulBound)
                        continue;
                    if (!ComboBoxAuk.Items.Contains("[" + item.Id + "]" + item.Name))
                        ComboBoxAuk.Items.Add("[" + item.Id + "]" + item.Name);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private string GetSkillIdFromComboboxName(string skillId)
        {
            //  var startIndex = skillId.IndexOf("[", StringComparison.Ordinal);
            var endIndex = skillId.IndexOf("]", StringComparison.Ordinal);
            // var leght = endIndex - startIndex + 1;
            var tempId = skillId.Substring(endIndex + 1);
            return tempId;
        }

        private void ButtonAddProp1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxAuk.SelectedIndex == -1)
                    return;


                //   var tempByte = GetSkillIdFromComboboxByte(comboBoxProp.Text);
                var tempId = GetSkillIdFromCombobox(ComboBoxAuk.Text);
                var tempname = GetSkillIdFromComboboxName(ComboBoxAuk.Text);


                /* if (CollectionAukSettingses.Any(collectionprop => tempId == collectionprop.Id))
                     return;*/




                CollectionAukSettingses.Add(new AukSettings
                {
                    Id = Convert.ToInt32(tempId),
                    Name = tempname,
                    MaxPrice = Convert.ToUInt64(TextBoxMaxPrice.Text),
                    Disscount = Convert.ToUInt64(TextBoxDiscount.Text),
                    Level = Convert.ToInt32(TextBoxLevel.Text),
                    MaxCount = Convert.ToUInt32(TextBoxMaxCount.Text)
                    /*  Count = Convert.ToInt32(TextBoxAucCount.Text),
                      MaxPrixe = Convert.ToInt32(TextBoxAucMaxPrice.Text),
                      MinPrice = Convert.ToInt32(TextBoxAucMinPrice.Text),
                      FixPrice = CheckBoxFixPrice.IsChecked.Value,
                      FixPriceCount = Convert.ToInt32(TextBoxFixPriceCount.Text)*/

                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonAukDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridAuk.SelectedIndex != -1)
                    CollectionAukSettingses.RemoveAt(DataGridAuk.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonAukLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxAukLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxAukLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxAukLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxAukLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxAukLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }




        private void comboBoxDungeonPlugin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxDungeonPlugin.Items.Clear();
                ComboBoxDungeonPlugin.Items.Add("Не выбрано");
                if (Host.Me == null)
                    return;

                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\"))
                {
                    Host.log(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\");
                    var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\");
                    // папка с файлами
                    foreach (var directoryInfo in dir.GetDirectories())
                    {
                        foreach (var fileInfo in directoryInfo.GetFiles())
                        {
                            if (!fileInfo.Name.Contains(".dll"))
                                continue;
                            ComboBoxDungeonPlugin.Items.Add(directoryInfo.Name + "\\" + fileInfo.Name);
                        }
                    }


                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void buttonDungeonSheduleDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridScriptSchedule.SelectedIndex != -1)
                    CollectionScriptSchedules.RemoveAt(DataGridScriptSchedule.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonScriptScheduleAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CollectionScriptSchedules.Add(new ScriptSchedule
                {
                    ScriptStartTime = TimeSpan.Parse(TextBoxScriptScheduleStartTime.Text),
                    ScriptStopTime = TimeSpan.Parse(TextBoxScriptSheduleEndTime.Text),
                    ScriptName = ComboBoxDungeonScriptCopy.Text,
                    Reverse = CheckBoxScriptReverseShedule.IsChecked != null && CheckBoxScriptReverseShedule.IsChecked.Value
                });

            }
            catch (Exception exception)
            {
                Host.log(exception + " ");
            }
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            Host.AutoQuests.Continue = true;
            Dispatcher?.Invoke(() => { ButtonContinue.IsEnabled = false; });
        }

        private void comboBoxBuyAuc_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (EEquipmentSlot value in Enum.GetValues(typeof(EEquipmentSlot)))
            {
                /* if (value == EEquipmentSlot.Ranged)
                     continue;*/
                //  Host.log(value.ToString());
                if (!ComboBoxBuyAuc.Items.Contains(value.ToString()))
                    ComboBoxBuyAuc.Items.Add(value.ToString());
            }
        }



        private void buttonAddEquipAuc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxBuyAuc.SelectedIndex == -1)
                    return;


                CollectionEquipAuc.Add(new EquipAuc()
                {
                    Slot = (EEquipmentSlot)Enum.Parse(typeof(EEquipmentSlot), ComboBoxBuyAuc.Text),

                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonSendMailLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxSendMailLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxSendMailLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxSendMailLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxSendMailLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxSendMailLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void ButtonItemsLocFill1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxItemsLocX1.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxItemsLocY1.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxItemsLocZ1.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxItemsLocMapId1.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxItemsLocAreaId1.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void ButtonLearnLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxLearnLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxLearnLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxLearnLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxLearnLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxLearnLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void ButtonInvItemsDel2_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridLearn.SelectedIndex != -1)
                    CollectionLearnSkill.RemoveAt(DataGridLearn.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonLearnSkillAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = "Неизвестно";
                var id = Convert.ToUInt32(TextBoxLearnSkillId.Text);
                if (Host.GameDB.SpellInfoEntries.ContainsKey(id))
                    name = Host.GameDB.SpellInfoEntries[id].SpellName;


                CollectionLearnSkill.Add(new LearnSkill
                {
                    Id = Convert.ToUInt32(TextBoxLearnSkillId.Text),
                    Name = name,
                    Level = Convert.ToUInt32(TextBoxLearnSkillLevel.Text),
                    Price = Convert.ToUInt32(TextBoxLearnPrice.Text),
                    AreaId = Convert.ToUInt32(TextBoxLearnLocAreaId.Text),
                    MapId = Convert.ToInt32(TextBoxLearnLocMapId.Text),
                    NpcId = Convert.ToUInt32(TextBoxLearnNpcId.Text),
                    Loc = new Vector3F(Convert.ToDouble(TextBoxLearnLocX.Text), Convert.ToDouble(TextBoxLearnLocY.Text), Convert.ToDouble(TextBoxLearnLocZ.Text)),
                });
                //  Host.log(CollectionInvItems.Count + " " + Host.CharacterSettings.ItemSettings.Count);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonIgnoreQuestAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBoxIgnoreQuestId.Text == "Id")
                    return;


                var tempId = Convert.ToUInt32(TextBoxIgnoreQuestId.Text);


                if (CollectionIgnoreQuest.Any(collectionInvItem => tempId == collectionInvItem.Id))
                    return;

                if (!Host.GameDB.QuestTemplates.ContainsKey(tempId))
                {
                    // ReSharper disable once RedundantNameQualifier
                    System.Windows.MessageBox.Show("Нет такого квеста");
                    return;
                }


                CollectionIgnoreQuest.Add(new IgnoreQuest
                {
                    Id = tempId,
                    Name = Host.GameDB.QuestTemplates[tempId].LogTitle,
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ComboBoxAdvancedEquipWeapon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in Enum.GetValues(typeof(EItemSubclassWeapon)))
            {
                //  Host.log(value.ToString());
                if (!ComboBoxAdvancedEquipWeapon.Items.Contains(value.ToString()))
                    ComboBoxAdvancedEquipWeapon.Items.Add(value.ToString());
            }
        }

        private void ButtonAddAdvancedEquip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxAdvancedEquipWeapon.SelectedIndex == -1)
                    return;

                CollectionAdvancedEquipsWeapon.Add(new AdvancedEquipWeapon
                {
                    WeaponType = (EItemSubclassWeapon)Enum.Parse(typeof(EItemSubclassWeapon), ComboBoxAdvancedEquipWeapon.Text),
                    Use = true
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonDelAdvancedEquip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridEquip.SelectedIndex != -1)
                    CollectionAdvancedEquipsWeapon.RemoveAt(DataGridEquip.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ComboBoxAdvancedEquipArmor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach (var value in Enum.GetValues(typeof(EItemSubclassArmor)))
                {
                    //  Host.log(value.ToString());
                    if (!ComboBoxAdvancedEquipArmor.Items.Contains(value.ToString()))
                        ComboBoxAdvancedEquipArmor.Items.Add(value.ToString());
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonAddAdvancedEquipArmor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxAdvancedEquipArmor.SelectedIndex == -1)
                    return;

                CollectionAdvancedEquipArmors.Add(new AdvancedEquipArmor
                {
                    ArmorType = (EItemSubclassArmor)Enum.Parse(typeof(EItemSubclassArmor), ComboBoxAdvancedEquipArmor.Text),
                    Use = true
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonDelAdvancedEquipArmor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridEquipArmor.SelectedIndex != -1)
                    CollectionAdvancedEquipArmors.RemoveAt(DataGridEquipArmor.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonStoneLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxStoneLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                TextBoxStoneLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                TextBoxStoneLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                TextBoxStoneLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                TextBoxStoneLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void ComboBoxAdvancedEquipStat_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                foreach (var value in Enum.GetValues(typeof(EItemModType)))
                {
                    //  Host.log(value.ToString());
                    if (!ComboBoxAdvancedEquipStat.Items.Contains(value.ToString()))
                        ComboBoxAdvancedEquipStat.Items.Add(value.ToString());
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonAddAdvancedEquipStat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxAdvancedEquipStat.SelectedIndex == -1)
                    return;
                CollectionAdvancedEquipStats.Add(new AdvancedEquipStat
                {
                    StatType = (EItemModType)Enum.Parse(typeof(EItemModType), ComboBoxAdvancedEquipStat.Text),
                    Coef = 1
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ButtonCheckCoef_Click(object sender, RoutedEventArgs e)
        {
            TextBlockEquipCoef.Text = "Одето" + Environment.NewLine;

            foreach (EEquipmentSlot value in Enum.GetValues(typeof(EEquipmentSlot)))
            {
                Item item = null;
                foreach (var item1 in Host.ItemManager.GetItems())
                {
                    if (item1.Place != EItemPlace.Equipment)
                        continue;
                    if (item1.Cell != (int)value)
                        continue;
                    /* if (Host.CommonModule.GetItemEPlayerPartsType(item1.InventoryType) != value)
                         continue;*/
                    item = item1;
                    break;
                }

                if (item == null)
                {
                    TextBlockEquipCoef.Text += value + " Пусто" + Environment.NewLine;
                }
                else
                {
                    var canequip = true;
                    if (item.ItemClass == EItemClass.Weapon)
                    {
                        if ((Host.GetProficiency(EItemClass.Weapon) & (1 << (int)item.Template.GetSubClass())) == 0)
                            canequip = false;
                    }

                    if (item.ItemClass == EItemClass.Armor)
                    {
                        if ((Host.GetProficiency(EItemClass.Armor) & (1 << (int)item.Template.GetSubClass())) == 0)
                            canequip = false;
                    }
                    TextBlockEquipCoef.Text += value + ": " + item.Name + "[" + item.Id + "] ["
                                               + item.InventoryType + "] ["
                                               + Host.CommonModule.GetItemEPlayerPartsType(item.InventoryType) + "]   "
                                               + item.RequiredLevel + "]   ["
                                               + item.ItemQuality + "]   ["
                                               + item.CanEquipItem() + "]   ["
                                               + canequip + "]   ["
                                               + Math.Round(Host.CommonModule.GetCoef(item), 2) + Environment.NewLine;
                }


            }

            TextBlockInvCoef.Text = "В инвентаре:" + Environment.NewLine;
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                    item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                    item.Place != EItemPlace.InventoryItem && item.Place != EItemPlace.Equipment)
                    continue;
                if (item.ItemClass != EItemClass.Armor && item.ItemClass != EItemClass.Weapon)
                    continue;

                if (item.Place == EItemPlace.Equipment)
                {

                }
                else
                {
                    var canequip = true;
                    if (item.ItemClass == EItemClass.Weapon)
                    {
                        if ((Host.GetProficiency(EItemClass.Weapon) & (1 << (int)item.Template.GetSubClass())) == 0)
                            canequip = false;
                    }

                    if (item.ItemClass == EItemClass.Armor)
                    {
                        if ((Host.GetProficiency(EItemClass.Armor) & (1 << (int)item.Template.GetSubClass())) == 0)
                            canequip = false;
                    }

                    TextBlockInvCoef.Text += item.Name + "[" + item.Id + "] ["
                                             + item.InventoryType + "] ["
                                             + Host.CommonModule.GetItemEPlayerPartsType(item.InventoryType) + "]   ["
                                             + item.RequiredLevel + "]   ["
                                             + item.ItemQuality + "]   ["
                                             + item.CanEquipItem() + "]   ["
                                             + canequip + "]   "
                                             + Math.Round(Host.CommonModule.GetCoef(item), 2) + Environment.NewLine;
                }

            }
        }

        private void comboBoxAdvancedEquipQuality_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ComboBoxAdvancedEquipQuality.Items.Clear();

                foreach (var value in Enum.GetValues(typeof(EItemQuality)))
                {
                    //  Host.log(value.ToString());
                    if (!ComboBoxAdvancedEquipQuality.Items.Contains(value.ToString()))
                        ComboBoxAdvancedEquipQuality.Items.Add(value.ToString());
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ComboBoxLearnTalant_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                ComboBoxLearnTalent.Items.Clear();

                if (Host.Me == null)
                    return;
                foreach (var item in Host.TalentTree.GetAllTalents())
                    ComboBoxLearnTalent.Items.Add("[" + item.ID + "]" + item.Name + " " + item.NameRu);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {

                Host.log(err.ToString());
            }
        }

        private void ButtonLearnTalentAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxLearnTalent.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(ComboBoxLearnTalent.Text));



                TalentSpell item = null;
                foreach (var i in Host.TalentTree.GetAllTalents())
                {
                    if (i.ID != tempId)
                        continue;
                    item = i;
                }
                if (item == null)
                    return;

                CollectionLearnTalents.Add(new LearnTalent
                {
                    Id = tempId,
                    Name = item.Name,
                    Level = Convert.ToUInt32(TextBoxLearnTalentLevel.Text)
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }
    }



    #region Class



    public class QuestTable
    {
        public QuestTable(string questType, int id, string isCompleted, string isFailed, string location = "Не известно")
        {
            QuestType = questType;
            Id = id;
            IsCompleted = isCompleted;
            IsFailed = isFailed;
            Location = location;
        }

        public string QuestType { get; set; }
        public int Id { get; set; }
        public string IsCompleted { get; set; }
        public string IsFailed { get; set; }
        public string Location { get; set; }
    }

    public class SkillTable
    {
        public SkillTable(string name, uint id, uint level)
        {
            Name = name;
            Id = id;

            Level = level;

        }
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint Level { get; set; }

    }

    public class BuffTable
    {
        public BuffTable(string itemname, uint itemid, uint skillid/*, string skillname*/)
        {
            ItemName = itemname;
            ItemId = itemid;
            SkillId = skillid;
            // SkillName = skillname;
        }

        public string ItemName { get; set; }
        public uint ItemId { get; set; }
        //  public string SkillName { get; set; }
        public uint SkillId { get; set; }

        //  public int Level { get; set; }
    }

    /*  public class ActiveSkillTable
      {
          public bool Checked { get; set; }
          public string Name { get; set; }
          public int Id { get; set; }
          public ESkillDeckTacticSlotType Type { get; set; }
          public int Priority { get; set; }

          public ActiveSkillTable(bool Checked, string name, int id, ESkillDeckTacticSlotType type, int priority)
          {
              this.Checked = Checked;
              Name = name;
              Id = id;
              Type = type;
              Priority = priority;
          }
      }*/

    #endregion
}
