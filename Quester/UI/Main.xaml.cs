using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;
using System.Runtime.InteropServices;
using Application = System.Windows.Application;
using Out.Internal.Core;
using WoWBot.Core;
using System.Windows.Markup;
using System.Windows.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using static WowAI.Host;
using Out.Utility;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using System.Windows.Shapes;

namespace WowAI.UI
{
    public static class Helper
    {
        public static void RemoveAll(this IList list)
        {
            while (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
    }
    public enum EMode
    {
        [Description("Квестинг")]
        Questing,
        [Description("Фарм мобов")]
        FarmMob,
        [Description("Фарм ресурсов")]
        FarmResource,
        [Description("Скрипт")]
        Script,
        [Description("Отбивание")]
        OnlyAttack,
       
        /*  [Description("Пати фарм")]
          PartyFarm*/

    }



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
                CollectionActiveSkill.RemoveAll();
                CollectionInvItems.RemoveAll();
                CollectionAllSkill.RemoveAll();
                CollectionAllBuff.RemoveAll();
                CollectionMobs.RemoveAll();
                CollectionDungeonCoord.RemoveAll();
                CollectionEventSettings.RemoveAll();
                CollectionPetSettings.RemoveAll();
                CollectionProps.RemoveAll();
                CollectionMultiZone.RemoveAll();
                CollectionQuestSettings.RemoveAll();
                CollectionMyQuests.RemoveAll();
                CollectionNpcForActions.RemoveAll();
                CollectionMyBuffs.RemoveAll();
                CollectionItemGlobals.RemoveAll();
                comboBoxDungeonScript.SelectionChanged += ComboBoxDungeonScript_SelectionChanged;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        /*  public class ViewModel : INotifyPropertyChanged
          {
              private ObservableCollection<DungeonCoordSettings> _collection;
              public ObservableCollection<DungeonCoordSettings> CollectionDungeonCoord
              {
                  get { return _collection; }
                  set
                  {
                      _collection = value;
                      OnPropertyChanged("Collection");
                  }
              }

              public ViewModel()
              {
                  BackgroundWorker bw = new BackgroundWorker();
                  bw.DoWork += DoWork;
                  bw.RunWorkerAsync();
              }

              private void DoWork(object sender, DoWorkEventArgs e)
              {
                  List<DungeonCoordSettings> emps = new List<DungeonCoordSettings>();
                  for (int i = 0; i < 10000; i++)
                  {
                      emps.Add(new DungeonCoordSettings() { Id = i + 1 });
                  }

                  Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                  {
                      CollectionDungeonCoord = new ObservableCollection<DungeonCoordSettings>(emps);
                  }));
              }

              public event PropertyChangedEventHandler PropertyChanged;
              protected void OnPropertyChanged(string propertyName)
              {
                  if (PropertyChanged != null)
                      PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
              }
          }*/

        private void ComboBoxDungeonScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!Host.FormInitialized)
                return;
            if (Host.CharacterSettings == null)
                return;
            if (comboBoxDungeonScript.SelectedItem == null)
                return;
            Host.CharacterSettings.Script = comboBoxDungeonScript.SelectedItem.ToString();
            if (ComboBoxDungeonAction.SelectedIndex == 0)
                CollectionDungeonCoord.RemoveAll();
            NeedApplyDungeonSettings = true;
        }

        public bool NeedApplySettings;
        public bool NeedApplyDungeonSettings;
        public bool NeedApplyQuestSettings;
        public bool NeedApplyDacandaSettings;
        public bool On;

        public bool SettingsNeedSave;
        private XmlDocument _doc;
        public bool IsToggle;
        public bool IsToggleDone = true;
        public bool NeedUpdate = true;
        internal Host Host;

        private List<Quest> _collectionQuests = new List<Quest>();
        public ObservableCollection<MyQuestTemplate1> collectionQuestTemplate = new ObservableCollection<MyQuestTemplate1>();


        private List<LFGStatus> _collectionLfg = new List<LFGStatus>();
        private List<LFGProposal> _collectionLfgProposals = new List<LFGProposal>();
        private IList<Unit> _collectionEntities = new List<Unit>();
        private List<Scenario> _collectionScenarios = new List<Scenario>();
        private List<ScenarioCriteria> _collectionCriterias = new List<ScenarioCriteria>();
        private List<Item> _collectionItems = new List<Item>();
        private List<Aura> _collectionAuras = new List<Aura>();
        private List<Aura> _collectionAurasTarget = new List<Aura>();
        private List<Spell> _collectionSpell = new List<Spell>();




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
        private ObservableCollection<IgnoreMB> _collectionIgnoreMb;
        private ObservableCollection<IgnoreQuest> _collectionIgnoreQuest;
        private ObservableCollection<MultiZone> _collectionMultiZone;
        private ObservableCollection<AukSettings> _collectionaAukSettingses;
        private ObservableCollection<MyEntity> _collectionMyQuests;

        private ObservableCollection<NpcForAction> _collectionNpcForActions;
        private ObservableCollection<MyBuff> _collectionMyBuffs;
        private ObservableCollection<ItemGlobal> _collectionItemGlobals;
        private ObservableCollection<GameObjectIgnore> _gameObjectIgnores;
        private ObservableCollection<ScriptSchedule> _scriptSchedules;

        private ObservableCollection<EquipAuc> _EquipAucs;
        public ObservableCollection<EquipAuc> CollectionEquipAuc
        {
            get
            {
                if (_EquipAucs == null)
                    _EquipAucs = new ObservableCollection<EquipAuc>();
                return _EquipAucs;
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

        public ObservableCollection<MyBuff> CollectionMyBuffs
        {
            get
            {
                if (_collectionMyBuffs == null)
                    _collectionMyBuffs = new ObservableCollection<MyBuff>();
                return _collectionMyBuffs;
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

        public ObservableCollection<MyEntity> CollectionMyQuests
        {
            get
            {
                if (_collectionMyQuests == null)
                    _collectionMyQuests = new ObservableCollection<MyEntity>();
                return _collectionMyQuests;
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

        public ObservableCollection<IgnoreMB> CollectionIgnoreMb
        {
            get
            {
                if (_collectionIgnoreMb == null)
                    _collectionIgnoreMb = new ObservableCollection<IgnoreMB>();
                return _collectionIgnoreMb;
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

        public class MyEntity
        {


            public WowGuid Guid { get; set; }
            public string Name { get; set; }
            public EMovementFlag MovementFlag { get; set; }
            public Vector3F Location { get; set; }
            public EBotTypes Type { get; set; }

            public uint GetEntry { get; set; }
            public EHighGuidType GetHighGuidType { get; set; }
            public EObjectType GetObjectType { get; set; }
            public double MeDist { get; set; }

            public bool CanAttack { get; set; }
            public string GameObject { get; set; }
            public List<ushort> SkillTyoe { get; set; }
            public uint SkillId { get; set; }
            public string SkillName { get; set; }
            public bool IsTrader { get; set; }

            public double Angle { get; set; }
            /*  event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
              {
                  add
                  {
                      throw new NotImplementedException();
                  }

                  remove
                  {
                      throw new NotImplementedException();
                  }
              }*/
        }



        public void UpdateQuest()
        {
            try
            {

                CollectionMyQuests.RemoveAll();
                var list = Host.GetEntities();
                foreach (var e in list.OrderBy(i => Host.Me.Distance(i)))
                {
                    MyEntity item = new MyEntity();
                    /*    if (e.Type == EBotTypes.Player)
                            continue;*/
                    if (CheckBoxCanAttack.IsChecked.Value)
                        if (!Host.CanAttack(e, Host.CanSpellAttack))
                            continue;

                    if (CheckBoxCanSbor.IsChecked.Value)
                    {
                        if (e.Type != EBotTypes.GameObject)
                            continue;
                        if ((e as GameObject).GameObjectType != EGameObjectType.GatheringNode)
                            continue;
                    }

                    var gameObject = "";
                    List<ushort> skilltype = new List<ushort>();
                    if (e.Type == EBotTypes.GameObject)
                    {
                        gameObject = (e as GameObject).GameObjectType.ToString();
                        //skilltype = (e as GameObject).RequiredGatheringSkill;
                    }
                    var trader = false;
                    if (e.Type == EBotTypes.Unit)
                    {
                        skilltype.Add((e as Unit).RequiredLootSkill);
                        trader = (e as Unit).IsVendor;
                    }


                    var loc = e.Location;

                    var skill = Host.SpellManager.GetGatheringSpell(e);
                    uint skillid = 0;
                    var skillname = "null";
                    if (skill != null)
                    {
                        skillid = skill.Id;
                        skillname = skill.Name;
                    }





                    item = new MyEntity
                    {
                        // Guid = e.Guid,
                        Name = e.Name,
                        MovementFlag = e.MovementFlags,
                        Location = loc,
                        Type = e.Type,
                        GetEntry = e.Guid.GetEntry(),
                        GetHighGuidType = e.Guid.GetHighGuidType(),
                        GetObjectType = e.Guid.GetObjectType(),
                        MeDist = Host.Me.Distance(loc),
                        SkillTyoe = skilltype,
                        GameObject = gameObject,
                        CanAttack = Host.CanAttack(e, Host.CanSpellAttack),
                        SkillId = skillid,
                        SkillName = skillname,
                        IsTrader = trader,
                        Angle = e.Rotation.Y


                    };
                    CollectionMyQuests.Add(item);
                }

                groupBox3.Header = "Debug " + CollectionMyQuests.Count();
                // Host.log(item.Index + "");
            }


            catch (TaskCanceledException) { }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        //---------------------------------------------------------------------------------Start Status Block------------------------------------------------------------------------

        #region Status Block

        public void SetTitle(string s)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    // LabelVersion.Content = s;
                    Main1.Title = s;
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }






        public void SetQuestIdText(string s)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    QuestId.Content = "Quest(" + Host.AutoQuests.ListQuest.Count + ") :" + s;
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

        public void SetQuestStateText(string s)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    QuestState.Content = "State: " + s;
                });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }






        public void FlashWindow()
        {
            var helper = new FlashWindowHelper(Application.Current);
            new Task(() =>
            {
                while (true)
                {
                    try
                    {

                        Thread.Sleep(5000);
                        // Flashes the window and taskbar 5 times and stays solid 
                        // colored until user focuses the main window
                        Dispatcher.Invoke(() => helper.FlashApplicationWindow());
                        Thread.Sleep(5000);
                        // Cancels the flash at any time
                        Dispatcher.Invoke(() => helper.StopFlashing());
                    }
                    catch (Exception e)
                    {
                        Host.log(e.ToString());
                    }
                }
            }).Start();
        }


        public void CheckSkill()
        {
            try
            {
                // Host.log("-------------------------------------------------------");
                foreach (var s in CollectionActiveSkill)
                {

                    bool notUse = false;
                    foreach (var spell in Host.SpellManager.GetSpells())
                    {
                        if (s.Id == 108853 && Host.Me.Level > 9 && Host.SpellManager.GetSpell(116) != null)
                        {
                            notUse = false;
                            break;
                        }

                        if (spell.Id == s.Id)
                        {
                            notUse = true;
                            break;
                        }

                    }
                    // Host.log(s.Id + "  " + s.Name + "  " + s.Checked + " // "+ notUse);
                    if (!notUse)
                    {
                        s.Checked = false;
                        foreach (var characterSettingsSkillSetting in Host.CharacterSettings.SkillSettings)
                        {
                            if (s.Id == characterSettingsSkillSetting.Id)
                            {
                                characterSettingsSkillSetting.Checked = false;
                            }
                        }
                    }
                    else
                    {
                        s.Checked = true;
                        foreach (var characterSettingsSkillSetting in Host.CharacterSettings.SkillSettings)
                        {
                            if (s.Id == characterSettingsSkillSetting.Id)
                            {
                                characterSettingsSkillSetting.Checked = true;
                            }
                        }
                    }
                }
                //  ListViewActiveSkill.DataContext = Host.CharacterSettings;
                ListViewActiveSkill.Items.Refresh();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }



        public void Draw()
        {
            Dispatcher.Invoke(() =>
            {

                myGrid.Children.Clear();
                Point(Host.Me);
                var myLine = new Line();
                var myLine2 = new Line();
                myLine.Stroke = Brushes.LightSteelBlue;
                myLine2.Stroke = Brushes.LightSteelBlue;
                myLine.X1 = 300; //150;
                myLine.Y1 = 300; //0;
                myLine.X2 = 150;
                myLine.Y2 = 300;

                myLine2.X1 = Host.Me.Location.X; //0;
                myLine2.Y1 = Host.Me.Location.Y; // 150;
                myLine2.X2 = Host.Me.Location.X - 150;
                myLine2.Y2 = Host.Me.Location.Y - 150;
                //  myLine.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                //     myLine.VerticalAlignment = VerticalAlignment.Center;
                //  myLine2.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 1;
                myLine.ToolTip = myLine.X1 + "," + myLine.Y1;
                // myGrid.Children.Add(myLine);
                // myLine2.StrokeThickness = 1;

                // myGrid.Children.Add(myLine2);


                Ellipse myEllipse = new Ellipse();

                // Create a SolidColorBrush with a red color to fill the 
                // Ellipse with.
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();

                // Describes the brush's color using RGB values. 
                // Each value has a range of 0-255.
                // mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                //   myEllipse.Fill = mySolidColorBrush;
                myEllipse.StrokeThickness = 1;
                myEllipse.Stroke = Brushes.LightSteelBlue;

                // Set the width and height of the Ellipse.
                //  myEllipse.Width = 200 * Masshtab;
                //  myEllipse.Height = 200* Masshtab;

                // Add the Ellipse to the StackPanel.
                // myGrid.Children.Add(myEllipse);


                // Host.log(Host.Me.Location + "  ");



                foreach (var entity in Host.GetEntities())
                {
                    /* if (entity.Id != 9406)
                         continue;*/
                    //  Host.log(entity.Id + "  " + entity.Name + "  " + entity.Location);
                    Point(entity);

                }

                foreach (var quest in Host.GetQuests())
                {
                    foreach (var questPoi in quest.GetQuestPOI())
                    {
                        if (questPoi.ObjectiveIndex != 0)
                            continue;
                        foreach (var questPoiPoint in questPoi.Points)
                        {
                            Vector3F point2 = new Vector3F(questPoiPoint.X, questPoiPoint.Y, 0);
                            Point(point2);
                        }
                    }
                }


            });
        }

        private float offsetX;
        private float offsetY;
        private int Masshtab = 1;

        public void Point(Vector3F entity)
        {
            Dispatcher.Invoke(() =>
            {

                Point point = new Point(entity.X, entity.Y);

                /*  if (entity.Type == EBotTypes.Self)
                  {
                      point = new Point(0, 0);
                      offsetX = entity.Location.X;
                      offsetY = entity.Location.Y;
                  }
                  else
                  {*/
                point = new Point((entity.X - offsetX) * Masshtab, (entity.Y - offsetY) * Masshtab);
                //  }


                Ellipse elipse = new Ellipse();
                var name = "";


                /*  if (entity.Type == EBotTypes.Npc)
                      name = (entity as Npc).Name;*/
                // name = name + "(" + entity.Type + ")";
                elipse.Width = 4;
                elipse.Height = 4;

                elipse.StrokeThickness = 2;
                elipse.Stroke = Brushes.DarkMagenta;



                elipse.Margin = new Thickness(point.X - 2, point.Y - 2, 0, 0);
                elipse.ToolTip = name + "   " + entity.X + ", " + entity.Y;
                myGrid.Children.Add(elipse);

            });
        }


        public void Point(Entity entity)
        {

            var point = new Point(entity.Location.X, entity.Location.Y);

            if (entity == Host.Me)
            {
                point = new Point(0, 0);
                offsetX = entity.Location.X;
                offsetY = entity.Location.Y;
            }
            else
            {
                point = new Point((entity.Location.X - offsetX) * Masshtab, (entity.Location.Y - offsetY) * Masshtab);
            }


            Ellipse elipse = new Ellipse();
            var name = "";


            //  if (entity.Type == EBotTypes.Unit)
            name = entity.Name;


            name = name + "(" + entity.Type + ")";
            elipse.Width = 4;
            elipse.Height = 4;

            elipse.StrokeThickness = 2;
            elipse.Stroke = Brushes.Green;

            if (entity.Type == EBotTypes.Player)
                elipse.Stroke = Brushes.Blue;

            if (entity.Type == EBotTypes.Pet)
                elipse.Stroke = Brushes.Aqua;

            if (entity.Type == EBotTypes.GameObject)
                elipse.Stroke = Brushes.Yellow;

            if (entity == Host.Me)
            {
                elipse.Stroke = Brushes.Red;
                /* elipse.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                 elipse.VerticalAlignment = VerticalAlignment.Center;*/
            }
            elipse.Margin = new Thickness(point.X - 2, point.Y - 2, 0, 0);
            elipse.ToolTip = name + "   " + entity.Location.X + ", " + entity.Location.Y + " " + entity.Location.Z;
            myGrid.Children.Add(elipse);
        }


        /// <summary>
        /// Обновляет интерфейс с информацией о персонаже
        /// </summary>
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
                        //   Host.log(item.GetSellPrice() + " " + item.Count + " " + item.Name + " " + item.Id);
                        //  invgold = invgold + item.GetSellPrice() * item.Count;
                    }

                }

                Dispatcher.Invoke(() =>
                {
                    try
                    {


                        if (CheckBoxAutoUpdate.IsChecked.Value)
                            UpdateQuest();

                        CheckSkill();
                        //   Host.log(Host.CharacterSettings.Mode + "   " + ComboBoxSwitchMode.SelectedValue);
                        //  Host.log(CheckBoxHideQuesterUI.IsChecked + "  " + Host.CharacterSettings.HideQuesterUi);
                        var sw = new Stopwatch();
                        sw.Start();
                        //  Main1.
                        //Прогрессбар



                        IsMoveToNow.Content = "IsMoveToNow: " + Host.CommonModule.IsMoveToNow + " Moving: " + Host.Me.IsMoving + " Susp: " + Host.CommonModule.IsMoveSuspended() + Host.Me.RunSpeed;
                        //FarmState.Content = "FarmState: " + Host.FarmModule.farmState + "  " + Host.FarmModule.readyToActions;

                        if (Host.FarmModule.BestMob != null)
                            BestMob.Content = "BestMob: " + Host.FarmModule.BestMob.Name + "[" + Host.FarmModule.BestMob.Id + "]" + " [" + Host.Me.Distance(Host.FarmModule.BestMob).ToString("F2") + "]" + "(" + (Host.ComboRoute.TickTime - Host.ComboRoute._timeAttack) + ")";
                        else
                            BestMob.Content = "BestMob: null";

                        if (Host.FarmModule.BestProp != null)
                        {
                            BestProp.Content = "BestProp: " + Host.FarmModule.BestProp.Name + "[" + Host.FarmModule.BestProp.Id + "]" + " [" + Host.Me.Distance(Host.FarmModule.BestProp).ToString("F2") + "]";
                        }
                        else
                        {
                            BestProp.Content = "BestProp: null";
                        }

                        var quest = Host.GetQuest(Host.AutoQuests.BestQuestId);
                        if (quest != null)
                            QuestId.Content = "Q(" + Host.AutoQuests.ListQuest.Count + "): " + quest.Template.LogTitle + " [" + quest.Id + "] " + quest.State;
                        else
                        {
                            if (Host.AutoQuests.BestQuestId != 0)
                            {
                                var questTemplate = Host.GameDB.QuestTemplates[Host.AutoQuests.BestQuestId];
                                QuestId.Content = "Q(" + Host.AutoQuests.ListQuest.Count + "): " + questTemplate.LogTitle + " [" + questTemplate.Id + "] Не взят";
                            }
                            else
                            {
                                QuestId.Content = "Q(" + Host.AutoQuests.ListQuest.Count + "): null";
                            }
                        }
                        if (Host.CharacterSettings.Mode == EMode.Script)
                        {
                            QuestId.Content = "Скрипт " + Host.AutoQuests.scriptStopwatch.Elapsed.Minutes + " min, " +
                                              Host.AutoQuests.scriptStopwatch.Elapsed.Seconds + " sec  Speed:" + Host.Me.RunSpeed + " " + Host.Me.MovementFlagExtra + " " + Host.Me.MovementFlags + " " + Host.Me.SwimBackSpeed +
                                              " " + Host.Me.SwimSpeed;
                        }


                        if (SettingsNeedSave)
                            GridSettings.Margin = new Thickness(0, 25, 0, 0);

                        ProgressBarMeHp.Maximum = Host.Me.MaxHp;
                        ProgressBarMeHp.Value = Host.Me.Hp;
                        var formattedString = $"{ProgressBarMeHp.Value}/{ProgressBarMeHp.Maximum}";
                        TextBlockHp.Text = formattedString;



                        ProgressBarMeMp.Foreground = Brushes.Aqua;
                        ProgressBarMeMp.Maximum = Host.Me.GetMaxPower(Host.Me.PowerType);
                        ProgressBarMeMp.Value = Host.Me.GetPower(Host.Me.PowerType);






                        formattedString = $"{ProgressBarMeMp.Value}/{ProgressBarMeMp.Maximum}";
                        TextBlockMp.Text = Host.Me.PowerType + ": " + formattedString;

                        var alternatePoint = EPowerType.AlternatePower;
                        if (Host.Me.Class == EClass.Druid || Host.Me.Class == EClass.Rogue)
                            alternatePoint = EPowerType.ComboPoints;
                        if (Host.Me.Class == EClass.Monk)
                            alternatePoint = EPowerType.Chi;
                        ProgressBarMeEnergy.Maximum = Host.Me.GetMaxPower(alternatePoint);
                        ProgressBarMeEnergy.Value = Host.Me.GetPower(alternatePoint);
                        formattedString = $"{ProgressBarMeEnergy.Value}/{ProgressBarMeEnergy.Maximum}";
                        TextBlockMeEnergy.Text = formattedString;



                        FarmState.Content = "FarmState: " + Host.FarmModule.farmState + " " +
                                            Host.FarmModule.readyToActions + "  " + Host.Me.VictimGuid.IsEmpty() + " " + Host.SpellManager.IsCasting + " " + Host.CurrentInteractionGuid;
                        //DPS
                        /* if (Host.TimeInFight != 0)
                        {
                            LabelDPS.Content = "ДПС: " + Host.AllDamage / Host.TimeInFight;
                        }*/
                        var allTime = DateTime.Now - Host.TimeWork;

                        //   LabelTimeInFight.Content = "Время в бою: " + Host.TimeInFight;

                        var formattedTimeSpan =
                    $"{allTime.Hours:D2} hr, {allTime.Minutes:D2} min, {allTime.Seconds:D2} sec";
                        LabelTimeWork.Content = "Время работы: " + formattedTimeSpan + " (" + Host.CheckCount + ") " + "Умер: " + Host.ComboRoute.DeadCount + "/" + Host.ComboRoute.DeadCountInPVP;

                        //   LabelAllDamage.Content = "Всего урона: " + Host.AllDamage;



                        if (allTime.TotalMinutes > 0 && Host.TimeInFight != 0)
                        {
                            LabelKillMobs.Content = "Убито мобов: " + Host.KillMobsCount + "(" + Math.Round(Host.KillMobsCount / allTime.TotalMinutes, 3) + ") " +
                                                    " ДПС: " + Host.AllDamage / Host.TimeInFight + " ";
                        }


                        /* if (allTime.TotalMinutes > 0)
                             LabelDPSKillMobsForMin.Content = "Убито мобов в минуту: " + Math.Round(Host.KillMobsCount / allTime.TotalMinutes, 3);*/



                        // LabelTimeNotWork.Content = "Время бездействия: " + Host.CheckCount;

                        double tempGold = Host.Me.Money;
                        double gold = tempGold / 10000;

                        double startGold = Host.StartGold / 10000;


                        invgold = invgold - Host.Startinvgold;
                        double doubleGold = Convert.ToDouble(invgold) / 10000;
                        //   Host.log(gold + "  " + Host.StartGold + "  " + startGold);
                        LabelGold.Content = "Золото: " + Math.Round(gold, 2) + " г. [" +
                                            Math.Round(gold - startGold, 2) + " г. + " + Math.Round(doubleGold, 2) +
                                            " г.] Всего: " + Math.Round((gold - startGold) + doubleGold, 2) + " г.";

                        if (allTime.TotalHours > 0 && allTime.TotalDays > 0)
                        {
                            var goldinday = Math.Round(((gold - startGold) /*+ doubleGold*/) / allTime.TotalDays, 2);
                            LabelGoldInHour.Content = "Золота в час / день: " +
                                                      Math.Round(((gold - startGold) /*+ doubleGold*/) / allTime.TotalHours, 2) +
                                                      " / " +
                                                      goldinday;

                            if (Host.CharacterSettings.Price == 0)
                            {
                                LabelPrice.Content = "Необходимо указать курс";
                            }
                            else
                            {
                                LabelPrice.Content = textBoxValuta.Text + " в день / 30 дней " + Math.Round(((((gold - startGold) / allTime.TotalDays) / Host.CharacterSettings.PriceKK) * Host.CharacterSettings.Price), 2) +
                                                     " / " +
                                                     Math.Round(((((gold - startGold) / allTime.TotalDays) / Host.CharacterSettings.PriceKK) * Host.CharacterSettings.Price) * 30, 2);
                            }


                        }



                        //  if (allTime.TotalDays > 0)
                        // LabelGoldInDay.Content = "Золота в день: " + Math.Round(((gold - Host.StartGold) + doubleGold) / allTime.TotalDays, 2);

                        /* if (Host.CharacterSettings.Mode != "Выполнение квестов")
                         {
                             QuestState.Visibility = Visibility.Collapsed;
                             IsMoveToNow.Visibility = Visibility.Collapsed;
                             IsMovementSuspended.Visibility = Visibility.Collapsed;
                         }
                         else
                         {
                             QuestId.Visibility = Visibility.Visible;
                             QuestState.Visibility = Visibility.Visible;
                             IsMoveToNow.Visibility = Visibility.Visible;
                             IsMovementSuspended.Visibility = Visibility.Visible;
                         }
                         */
                        LabelGameState.Content = "GameState: " + Host.GameState + " " + " MapID:" + Host.MapID + " " + Host.Area.Id + "  " + Host.Area.AreaName + "/" + Host.Area.ZoneName + " " + Host.Zone.Id + "  " + Host.Zone.ZoneName + "/" + Host.Zone.AreaName +
                                                  " " + " Смертей: " +
                                                 Host.ComboRoute.DeadCount + "/" + Host.ComboRoute.DeadCountInPVP;

                        //   LabelWorldType.Content = "WorldType: " + Host.WorldMapType + " " + Host.WorldMapId;

                        //Таргет
                        if (Host.Me.Target != null && Host.IsExists(Host.Me.Target))
                        {
                            MeTarget.Content = "[" + Host.GetAgroCreatures().Count + "/" +
                                               Host.ComboRoute.MobsWithDropCount() + "/" + Host.ComboRoute.MobsWithSkinCount() + "] "/* + Host.Me.HasInArc(Math.PI, Host.Me.Target) + Host.Me.GetAngle(Host.Me.Target)*/ +
                                               " " +
                                              " Цель: " +
                                               Host.Me.Target?.Name + " " +
                                               " [" + Host.Me.Target?.Id + "] MeDist:" +
                                               Math.Round(Host.Me.Distance(Host.Me.Target)) + " / Loc" + Host.Me.Target.Location;
                            if (Host.Me.Target != null && Host.IsExists(Host.Me.Target))
                            {
                                var target = Host.Me.Target as Unit;
                                if (target != null)
                                {
                                    target = Host.Me.Target as Unit;
                                    ProgressBarTargetHp.Maximum = target.MaxHp;
                                    ProgressBarTargetHp.Value = target.Hp;
                                }

                                if (Host.Me.Target.Type == EBotTypes.Player)
                                {
                                    target = Host.Me.Target as Player;
                                    ProgressBarTargetHp.Maximum = target.MaxHp;
                                    ProgressBarTargetHp.Value = target.Hp;
                                }

                                formattedString = $"{ProgressBarTargetHp.Value}/{ProgressBarTargetHp.Maximum}";
                                TextBlockTargetHp.Text = formattedString;
                            }
                        }
                        else
                            MeTarget.Content = "[" + Host.GetAgroCreatures().Count + "/" +
                                               Host.ComboRoute.MobsWithDropCount() + "/" + Host.ComboRoute.MobsWithSkinCount() + "]Цель: Не выбрана";

                        //Координаты
                        if (Host.AdvancedLog)
                            TextboxMeCoord.Text = Host.Me.Location.ToString() /*+ "  /   " + Host.GetNavMeshHeight(Host.Me.Location)*/;
                        else
                        {
                            TextboxMeCoord.Text = Host.Me.Location.ToString();
                        }
                        if (sw.ElapsedMilliseconds > 2)
                            Host.log("SetMe " + sw.ElapsedMilliseconds + " mc", Host.LogLvl.Error);
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
                Host.cancelRequested = true;
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

        /// <summary>
        /// Включить и выключить квестинг
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    Host.NeedRestart = true;
                    Host.AutoQuests.scriptStopwatch.Stop();
                    Host.CancelMoveTo();
                    //  Host.log("Отменяю движение");
                    // Host.FarmModule.farmState = Modules.FarmState.Disabled;                  
                    ButtonOnOff.Content = "Включить";
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

        private void ComboBoxPassive1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                TextBoxPriority.Text = "0";
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }


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
                Main1.Height = 450;

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

                comboBoxInvItems.Items.Clear();

                if (Host.Me == null)
                    return;
                foreach (var item in Host.ItemManager.GetItems())
                    comboBoxInvItems.Items.Add("[" + item.Id + "]" + item.Name);
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

        private void ComboBoxSwitchMode_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            try
            {
                /* if (Host != null)
                     if (ComboBoxSwitchMode != null)
                         Host.CharacterSettings.Mode = ComboBoxSwitchMode.Text;*/
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
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
                if (ctrl != null) ctrl.MaxLength = 1; //длина текста в текстбоксе
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

            try
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
            }
        }

        private void ListViewAllSkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ListViewAllSkill.SelectedIndex == -1)
                    return;
                ListViewActiveSkill.SelectedIndex = -1;
                ButtonAddSkill.IsEnabled = true;
                ButtonChangeSkill.IsEnabled = false;
                ButtonDelSkill.IsEnabled = false;
                var skill = ListViewAllSkill.SelectedItem as SkillTable;
                if (skill != null)
                {
                    GroupBoxSettingsSkill.Header = "Настройки " + skill.Name + "[" + skill.Id + "]";
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



                CheckBoxUseInPVP.IsChecked = false;
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
                var i = 0;
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
                        PluginPath = dungeonCoordSettingse.PluginPath
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

        private bool CheckMultizone()
        {
            if (radioButtonMultiZone.IsChecked.Value)
            {
                radioButtonMultiZone.IsChecked = true;
                radioButtonOneZone.IsChecked = false;
                return true;
            }
            else
            {
                radioButtonMultiZone.IsChecked = false;
                radioButtonOneZone.IsChecked = true;
                return false;
            }
        }

        /// <summary>
        /// Создает настройки по умолчанию
        /// </summary>
        private void CreateNewSettings()
        {
            try
            {
                //Настройки-----------

                //Общее
                Host.CharacterSettings.HideQuesterUi = CheckBoxHideQuesterUI.IsChecked.Value;
                Host.CharacterSettings.AutoEquip = CheckBoxAutoEquip.IsChecked.Value;
                Host.CharacterSettings.CheckRepairAndSell = CheckBoxCheckRepairAndSell.IsChecked.Value;
                Host.CharacterSettings.WaitSixMin = CheckBoxWaitSixMin.IsChecked.Value;
                Host.CharacterSettings.QuesterLeft = Main1.Left;
                Host.CharacterSettings.QuesterTop = Main1.Top;
                Host.CharacterSettings.DebuffDeath = CheckBoxDebuffDeath.IsChecked.Value;
                Host.CharacterSettings.UseFilterMobs = CheckBoxUseFilterMobs.IsChecked.Value;
                Host.CharacterSettings.FightIfMobs = CheckBoxFightIfMobs.IsChecked.Value;
                Host.CharacterSettings.StopQuesting = CheckBoxStopQuesting.IsChecked.Value;
                Host.CharacterSettings.StopQuestingLevel = Convert.ToInt32(textBoxStopQuestingLevel.Text);
                Host.CharacterSettings.UnmountMoveFail = CheckBoxUnmountMoveFail.IsChecked.Value;
                Host.CharacterSettings.Valuta = textBoxValuta.Text;
                Host.CharacterSettings.Price = Convert.ToSingle(textBoxPrice.Text);
                Host.CharacterSettings.PriceKK = Convert.ToInt32(textBoxPriceKK.Text);
                Host.CharacterSettings.FindBestPoint = CheckBoxFindBestPoint.IsChecked.Value;
                Host.CharacterSettings.CheckAuk = CheckBoxCheckAuk.IsChecked.Value;
                Host.CharacterSettings.LogAll = CheckBoxAllLog.IsChecked.Value;
                Host.CharacterSettings.FreeInvCountForAuk = Convert.ToInt32(textBoxFreeInvCountForAuk.Text);
                Host.CharacterSettings.SummonBattlePet = CheckBoxSummonBattlePet.IsChecked.Value;
                Host.CharacterSettings.BattlePetNumber = ComboBoxSummonBattlePetNumber.SelectedIndex;
                Host.CharacterSettings.UseMultiZone = CheckMultizone();
                Host.CharacterSettings.AoeFarm = CheckBoxAOEFarm.IsChecked.Value;
                Host.CharacterSettings.AoeMobsCount = Convert.ToInt32(textBoxAOEMobsCount.Text);
                Host.CharacterSettings.FreeInvCountForAukId = Convert.ToUInt32(textBoxFreeInvCountForAukId.Text);
                Host.CharacterSettings.FormForFight = ComboBoxFormForFight.Text;
                Host.CharacterSettings.FormForMove = ComboBoxFormForMove.Text;
                Host.CharacterSettings.LaunchScript = CheckBoxLaunchSkript.IsChecked.Value;
                Host.CharacterSettings.UseMountMyLoc = CheckBoxUseSMountMyLoc.IsChecked.Value;
                Host.CharacterSettings.RunQuestHerbalism = CheckBoxRunQuestHerbalism.IsChecked.Value;
                Host.CharacterSettings.QuestMode = ComboBoxSwitchQuestMode.SelectedIndex;
                Host.CharacterSettings.RepairCount = Convert.ToInt32(textBoxRepairCount.Text);
                Host.CharacterSettings.CheckRepair = CheckBoxCheckRepair.IsChecked.Value;
                Host.CharacterSettings.InvFreeSlotCount = Convert.ToInt32(textBoxFreeInvCount.Text);
                Host.CharacterSettings.UseStoneForSellAndRepair = CheckBoxUseStoneForSellAndRepair.IsChecked.Value;
                Host.CharacterSettings.GatherLocX = Convert.ToSingle(textBoxGatherLocX.Text);
                Host.CharacterSettings.GatherLocY = Convert.ToSingle(textBoxGatherLocY.Text);
                Host.CharacterSettings.GatherLocZ = Convert.ToSingle(textBoxGatherLocZ.Text);
                Host.CharacterSettings.GatherLocMapId = Convert.ToInt32(textBoxGatherLocMapId.Text);
                Host.CharacterSettings.GatherLocAreaId = Convert.ToInt32(textBoxGatherLocAreaId.Text);
                Host.CharacterSettings.GatherRadius = Convert.ToInt32(textBoxGatherLocRadius.Text);
                Host.CharacterSettings.CheckBoxAttackForSitMount = CheckBoxAttackForSitMount.IsChecked.Value;
                Host.CharacterSettings.FarmLocX = Convert.ToSingle(textBoxFarmLocX.Text);
                Host.CharacterSettings.FarmLocY = Convert.ToSingle(textBoxFarmLocY.Text);
                Host.CharacterSettings.FarmLocZ = Convert.ToSingle(textBoxFarmLocZ.Text);
                Host.CharacterSettings.FarmLocMapId = Convert.ToInt32(textBoxFarmLocMapId.Text);
                Host.CharacterSettings.FarmLocAreaId = Convert.ToUInt32(textBoxFarmLocAreaId.Text);
                Host.CharacterSettings.FarmRadius = Convert.ToInt32(textBoxFarmLocRadius.Text);
                Host.CharacterSettings.WorldQuest = CheckBoxWorldQuest.IsChecked.Value;
                Host.CharacterSettings.MountLocX = Convert.ToSingle(textBoxMountLocX.Text);
                Host.CharacterSettings.MountLocY = Convert.ToSingle(textBoxMountLocY.Text);
                Host.CharacterSettings.MountLocZ = Convert.ToSingle(textBoxMountLocZ.Text);
                Host.CharacterSettings.MountLocMapId = Convert.ToInt32(textBoxMountLocMapId.Text);
                Host.CharacterSettings.MountLocAreaId = Convert.ToInt32(textBoxMountLocAreaId.Text);

                Host.CharacterSettings.AukRun = CheckBoxAukRun.IsChecked.Value;
                Host.CharacterSettings.AukAreaId = Convert.ToInt32(textBoxAukLocAreaId.Text);
                Host.CharacterSettings.AukMapId = Convert.ToInt32(textBoxAukLocMapId.Text);
                Host.CharacterSettings.AukLocX = Convert.ToSingle(textBoxAukLocX.Text);
                Host.CharacterSettings.AukLocY = Convert.ToSingle(textBoxAukLocY.Text);
                Host.CharacterSettings.AukLocZ = Convert.ToSingle(textBoxAukLocZ.Text);
                Host.CharacterSettings.SummonMount = CheckBoxSummonMount.IsChecked.Value;
                Host.CharacterSettings.ScriptReverse = CheckBoxScriptReverse.IsChecked.Value;
                Host.CharacterSettings.UseWhistleForSellAndRepair = CheckBoxUseWhistleForSellAndRepair.IsChecked.Value;
                Host.CharacterSettings.LogScriptAction = CheckBoxLogScript.IsChecked.Value;
                Host.CharacterSettings.LogSkill = CheckBoxLogSkill.IsChecked.Value;
                Host.CharacterSettings.ScriptScheduleEnable = CheckBoxScriptScheduleEnable.IsChecked.Value;
                Host.CharacterSettings.ForceMoveScriptEnable = CheckBoxForceMoveScriptEnable.IsChecked.Value;
                Host.CharacterSettings.ForceMoveScriptDist = Convert.ToInt32(textBoxForceMoveScriptDist.Text);

                Host.CharacterSettings.FightIfHPLess = CheckBoxFightIfHPLess.IsChecked.Value;
                Host.CharacterSettings.FightIfHPLessCount = Convert.ToInt32(textBoxFightIfHPLessCount.Text);

                Host.CharacterSettings.Skinning = CheckBoxSkining.IsChecked.Value;
                Host.CharacterSettings.NoAttackOnMount = CheckBoxNoAttackOnMount.IsChecked.Value;

                Host.CharacterSettings.GatherResouceScript = CheckBoxGatherResourceScript.IsChecked.Value;
                Host.CharacterSettings.GatherRadiusScript = Convert.ToInt32(textBoxGatherRadiusScript.Text);
                Host.CharacterSettings.EquipItemStat = Convert.ToInt32(textBoxEquipStateWeapon.Text);
                Host.CharacterSettings.Attack = CheckBoxAttack.IsChecked.Value;
                Host.CharacterSettings.AttackRadius = Convert.ToInt32(textBoxAttackRadius.Text);

                Host.CharacterSettings.UseDash = CheckBoxUseDash.IsChecked.Value;

                Host.CharacterSettings.Zrange = Convert.ToInt32(textBoxFarmZRange.Text);

                Host.CharacterSettings.Mode = (EMode)ComboBoxSwitchMode.SelectedIndex;

                Host.CharacterSettings.AlternateAuk = CheckBoxAlternateAuk.IsChecked.Value;

                Host.CharacterSettings.PickUpLoot = CheckBoxPickUpLoot.IsChecked.Value;
                Host.CharacterSettings.IgnoreMob = Convert.ToInt32(textBoxIgnoreMob.Text);
                //  Host.CharacterSettings.Mode = ComboBoxSwitchMode.Text;

                //Farm
                /*    Host.CharacterSettings.FarmLoc = new Vector3F(Convert.ToSingle(textBoxFarmLocX.Text),
                        Convert.ToSingle(textBoxFarmLocY.Text), Convert.ToSingle(textBoxFarmLocZ.Text));
                    Host.CharacterSettings.FarmRadius = Convert.ToInt32(textBoxFarmLocRadius.Text);*/
                //Gather
                /*  Host.CharacterSettings.GatherLoc = new Vector3F(Convert.ToSingle(textBoxGatherLocX.Text),
                      Convert.ToSingle(textBoxGatherLocY.Text), Convert.ToSingle(textBoxGatherLocZ.Text));*/



                // Host.CharacterSettings.GatherRadius = Convert.ToInt32(textBoxGatherLocRadius.Text);



                Host.CharacterSettings.SendMail = CheckBoxSendMail.IsChecked.Value;
                Host.CharacterSettings.SendMailName = textBoxSendMailName.Text;


                Host.CharacterSettings.SendMailLocAreaId = Convert.ToInt32(textBoxSendMailLocAreaId.Text);
                Host.CharacterSettings.SendMailLocMapId = Convert.ToInt32(textBoxSendMailLocMapId.Text);
                Host.CharacterSettings.SendMailLocX = Convert.ToSingle(textBoxSendMailLocX.Text);
                Host.CharacterSettings.SendMailLocY = Convert.ToSingle(textBoxSendMailLocY.Text);
                Host.CharacterSettings.SendMailLocZ = Convert.ToSingle(textBoxSendMailLocZ.Text);

                try
                {
                    //  Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2    " + textBoxCheckAukStartTime.Text);
                    //   Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2   " + textBoxCheckAukEndTime.Text);

                    

                    Host.CharacterSettings.SendMailStartTime = TimeSpan.Parse(textBoxSendMailStartTime.Text);
                    Host.CharacterSettings.SendMailStopTime = TimeSpan.Parse(textBoxSendMailEndTime.Text);

                    //  Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2  " + textBoxCheckAukStartTime.Text);
                    //  Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2  " + textBoxCheckAukEndTime.Text);
                }
                catch (Exception e)
                {
                    Host.log(e + "");
                }

                //  
                Host.CharacterSettings.AukTime = ComboBoxAukTime.SelectedIndex;
                Host.CharacterSettings.Script = comboBoxDungeonScript.Text;
                Host.CharacterSettings.Quest = comboBoxQuestSet.Text;

                try
                {
                    //  Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2    " + textBoxCheckAukStartTime.Text);
                    //   Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2   " + textBoxCheckAukEndTime.Text);

                    Host.CharacterSettings.CheckAukInTimeRange = CheckBoxCheckAukTime.IsChecked.Value;

                    Host.CharacterSettings.StartAukTime = TimeSpan.Parse(textBoxCheckAukStartTime.Text);
                    Host.CharacterSettings.EndAukTime = TimeSpan.Parse(textBoxCheckAukEndTime.Text);

                    //  Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   2  " + textBoxCheckAukStartTime.Text);
                    //  Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   2  " + textBoxCheckAukEndTime.Text);
                }
                catch (Exception e)
                {
                    Host.log(e + "");
                }


                Host.CharacterSettings.EquipAucs.Clear();
                foreach (var collectionScriptSchedule in CollectionEquipAuc)
                {
                    var item = new EquipAuc
                    {
                        Slot = collectionScriptSchedule.Slot,
                        Level = collectionScriptSchedule.Level,
                        MaxPrice = collectionScriptSchedule.MaxPrice,
                        Name= collectionScriptSchedule.Name,
                        Stat1 = collectionScriptSchedule.Stat1,
                        Stat2 = collectionScriptSchedule.Stat2
                    };
                    Host.CharacterSettings.EquipAucs.Add(item);
                }

                Host.CharacterSettings.ScriptSchedules.Clear();
                foreach (var collectionScriptSchedule in CollectionScriptSchedules)
                {
                    var item = new ScriptSchedule
                    {
                        ScriptStopTime = collectionScriptSchedule.ScriptStopTime,
                        ScriptName = collectionScriptSchedule.ScriptName,
                        ScriptStartTime = collectionScriptSchedule.ScriptStartTime,
                        Reverse = collectionScriptSchedule.Reverse
                    };
                    Host.CharacterSettings.ScriptSchedules.Add(item);
                }

                Host.CharacterSettings.AukSettingses.Clear();
                foreach (var collectionAukSettingse in CollectionAukSettingses)
                {
                    var item = new AukSettings
                    {
                        Id = collectionAukSettingse.Id,
                        Name = collectionAukSettingse.Name,
                        Disscount = collectionAukSettingse.Disscount,
                        Level = collectionAukSettingse.Level,
                        MaxPrice = collectionAukSettingse.MaxPrice,
                        MaxCount = collectionAukSettingse.MaxCount
                    };
                    Host.CharacterSettings.AukSettingses.Add(item);
                }


                //Farm
                Host.CharacterSettings.GameObjectIgnores.Clear();
                foreach (var collectionGameObjectIgnore in CollectionGameObjectIgnores)
                {
                    var item = new GameObjectIgnore
                    {
                        Name = collectionGameObjectIgnore.Name,
                        Id = collectionGameObjectIgnore.Id,
                        Ignore = collectionGameObjectIgnore.Ignore,
                        Loc = collectionGameObjectIgnore.Loc
                    };
                    Host.CharacterSettings.GameObjectIgnores.Add(item);
                }


                Host.CharacterSettings.MyItemGlobals.Clear();
                foreach (var collectionItemGlobal in CollectionItemGlobals)
                {
                    var item = new ItemGlobal
                    {
                        Quality = collectionItemGlobal.Quality,
                        Class = collectionItemGlobal.Class,
                        ItemLevel = collectionItemGlobal.ItemLevel
                    };
                    Host.CharacterSettings.MyItemGlobals.Add(item);
                }

                Host.CharacterSettings.MyBuffSettings.Clear();
                foreach (var collectionMyBuff in CollectionMyBuffs)
                {
                    var item = new MyBuff
                    {
                        ItemId = collectionMyBuff.ItemId,
                        ItemName = collectionMyBuff.ItemName,
                        SkillId = collectionMyBuff.SkillId
                    };
                    Host.CharacterSettings.MyBuffSettings.Add(item);
                }


                Host.CharacterSettings.NpcForActionSettings.Clear();

                foreach (var collectionNpcForAction in CollectionNpcForActions)
                {
                    var item = new NpcForAction
                    {
                        Id = collectionNpcForAction.Id,
                        Name = collectionNpcForAction.Name,
                        Use = collectionNpcForAction.Use,
                        IsArmorer = collectionNpcForAction.IsArmorer,
                        IsCharmed = collectionNpcForAction.IsCharmed,
                        IsServiceProvider = collectionNpcForAction.IsServiceProvider,
                        IsBanker = collectionNpcForAction.IsBanker,
                        IsTaxi = collectionNpcForAction.IsTaxi,
                        IsVendor = collectionNpcForAction.IsVendor,
                        IsTabardDesigner = collectionNpcForAction.IsTabardDesigner,
                        IsGossip = collectionNpcForAction.IsGossip,
                        IsCritter = collectionNpcForAction.IsCritter,
                        MapId = collectionNpcForAction.MapId,
                        IsAuctioner = collectionNpcForAction.IsAuctioner,
                        IsTrainer = collectionNpcForAction.IsTrainer,
                        IsInnkeeper = collectionNpcForAction.IsInnkeeper,
                        IsTurning = collectionNpcForAction.IsTurning,
                        IsSpiritGuide = collectionNpcForAction.IsSpiritGuide,
                        IsTotem = collectionNpcForAction.IsTotem,
                        AreaId = collectionNpcForAction.AreaId,
                        IsSpiritService = collectionNpcForAction.IsSpiritService,
                        IsTapped = collectionNpcForAction.IsTapped,
                        IsBattleMaster = collectionNpcForAction.IsBattleMaster,
                        IsSpiritHealer = collectionNpcForAction.IsSpiritHealer,
                        Loc = collectionNpcForAction.Loc
                    };

                    Host.CharacterSettings.NpcForActionSettings.Add(item);
                }


                Host.CharacterSettings.IgnoreQuests.Clear();
                foreach (var collectionIgnoreQuests in CollectionIgnoreQuest)
                {
                    var pet = new IgnoreQuest
                    {
                        Id = collectionIgnoreQuests.Id,
                        Name = collectionIgnoreQuests.Name
                    };
                    Host.CharacterSettings.IgnoreQuests.Add(pet);
                }



                Host.CharacterSettings.IgnoreMB.Clear();
                foreach (var collectionIgnoreMb in CollectionIgnoreMb)
                {
                    var pet = new IgnoreMB
                    {
                        Id = collectionIgnoreMb.Id,
                        Name = collectionIgnoreMb.Name
                    };
                    Host.CharacterSettings.IgnoreMB.Add(pet);
                }

                Host.CharacterSettings.PetSettings.Clear();
                foreach (var collectionPet in CollectionPetSettings)
                {
                    var pet = new PetSettings
                    {
                        Id = collectionPet.Id,
                        Name = collectionPet.Name,
                        Type = collectionPet.Type
                    };
                    Host.CharacterSettings.PetSettings.Add(pet);
                }

                Host.CharacterSettings.EventSettings.Clear();
                foreach (var collectionEvent in CollectionEventSettings)
                {
                    var item = new EventSettings
                    {
                        ActionEvent = collectionEvent.ActionEvent,
                        TypeEvents = collectionEvent.TypeEvents,
                        SoundFile = collectionEvent.SoundFile,
                        Pause = collectionEvent.Pause
                    };
                    Host.CharacterSettings.EventSettings.Add(item);
                }

                Host.CharacterSettings.ItemSettings.Clear();

                foreach (var collectionInvItem in CollectionInvItems)
                {
                    var item = new ItemSettings
                    {
                        Id = collectionInvItem.Id,
                        Name = collectionInvItem.Name,

                        Use = collectionInvItem.Use,
                        Quality = collectionInvItem.Quality,
                        Class = collectionInvItem.Class,
                        EnableSale = collectionInvItem.EnableSale,
                        SellPrice = collectionInvItem.SellPrice,
                        MeLevel = collectionInvItem.MeLevel
                    };
                    Host.CharacterSettings.ItemSettings.Add(item);
                }

                //Мобы
                Host.CharacterSettings.PropssSettings.Clear();

                foreach (var collectionProp in CollectionProps)
                {
                    var item = new PropSettings
                    {
                        Id = collectionProp.Id,
                        Name = collectionProp.Name,
                        Priority = collectionProp.Priority,

                    };
                    Host.CharacterSettings.PropssSettings.Add(item);
                }

                Host.CharacterSettings.MultiZones.Clear();

                foreach (var multiZone in CollectionMultiZone)
                {
                    var item = new MultiZone
                    {
                        Id = multiZone.Id,
                        ChangeByTime = multiZone.ChangeByTime,
                        Loc = multiZone.Loc,
                        Radius = multiZone.Radius,
                        Time = multiZone.Time,
                        ChangeByDeathPlayer = multiZone.ChangeByDeathPlayer,
                        CountDeathByPlayer = multiZone.CountDeathByPlayer
                    };
                    Host.CharacterSettings.MultiZones.Add(item);
                }

                Host.CharacterSettings.MobsSettings.Clear();

                foreach (var collectionMobs in CollectionMobs)
                {
                    var item = new MobsSettings
                    {
                        Id = collectionMobs.Id,
                        Name = collectionMobs.Name,
                        Priority = collectionMobs.Priority,
                        Level = collectionMobs.Level
                    };
                    Host.CharacterSettings.MobsSettings.Add(item);
                }



                //Скилы
                Host.CharacterSettings.SkillSettings.Clear();




                foreach (var r in CollectionActiveSkill)
                {
                    var skill = new SkillSettings
                    {
                        Checked = r.Checked,
                        Id = r.Id,
                        MeMaxHp = r.MeMaxHp,
                        MeMaxMp = r.MeMaxMp,
                        MeMinMp = r.MeMinMp,
                        MeMinHp = r.MeMinHp,
                        Name = r.Name,
                        Priority = r.Priority,

                        TargetMaxHp = r.TargetMaxHp,
                        TargetMinHp = r.TargetMinHp,
                        BaseDist = r.BaseDist,
                        MaxDist = r.MaxDist,
                        MinDist = r.MinDist,
                        MoveDist = r.MoveDist,
                        AoeMax = r.AoeMax,
                        AoeRadius = r.AoeRadius,
                        AoeMin = r.AoeMin,
                        AoeMe = r.AoeMe,
                        SelfTarget = r.SelfTarget,
                        NotTargetEffect = r.NotTargetEffect,
                        NotMeEffect = r.NotMeEffect,
                        IsMeEffect = r.IsMeEffect,
                        IsTargetEffect = r.IsTargetEffect,
                        MinLevel = r.MinLevel,
                        MaxLevel = r.MaxLevel,
                        CombatElementCountLess = r.CombatElementCountLess,
                        CombatElementCountMore = r.CombatElementCountMore,

                        UseInFight = r.UseInFight,

                        UseInPVP = r.UseInPVP,
                        TargetId = r.TargetId

                    };
                    Host.CharacterSettings.SkillSettings.Add(skill);
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

        /// <summary>
        /// Кнопка добавить скилл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAddSkill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewAllSkill.SelectedIndex == -1)
                    return;
                if (Convert.ToInt32(TextBoxMaxDist.Text) < 1)
                {
                    System.Windows.MessageBox.Show("Максимальная дистанция скила не может быть меньше 1", "Ошибка", MessageBoxButton.OK);
                    return;
                }



                var path = ListViewAllSkill.SelectedItem as SkillTable;

                var tempNotTargetEffect = 0;
                //  if (ComboBoxNotTargetEffect.SelectedIndex != -1)
                tempNotTargetEffect = Convert.ToInt32(TextBoxNotTargetEffect.Text);

                var tempNotMeEffect = 0;
                // if (ComboBoxNotMeEffect.SelectedIndex != -1)
                tempNotMeEffect = Convert.ToInt32(TextBoxNotMeEffect.Text);// GetSkillIdFromCombobox(ComboBoxNotMeEffect.Text);

                var tempIsMeEffect = 0;
                //  if (ComboBoxIsMeEffect.SelectedIndex != -1)
                tempIsMeEffect = Convert.ToInt32(TextBoxIsMeEffect.Text);// GetSkillIdFromCombobox(ComboBoxIsMeEffect.Text);

                var tempIsTargetEffect = 0;
                //  if (ComboBoxIsTargetEffect.SelectedIndex != -1)
                tempIsTargetEffect = Convert.ToInt32(TextBoxIsTargetEffect.Text);// GetSkillIdFromCombobox(ComboBoxIsTargetEffect.Text);
                // if(ComboBoxCombatEllementLess.SelectedIndex != -1)




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
                        MinLevel = Convert.ToInt32(TextBoxMinLevel.Text),
                        MaxLevel = Convert.ToInt32(TextBoxMaxLevel.Text),
                        CombatElementCountLess = Convert.ToInt32(TextBoxAlternatePowerLess.Text),
                        CombatElementCountMore = Convert.ToInt32(TextBoxAlternatePowerMore.Text),
                        UseInFight = CheckBoxUseInFight.IsChecked != null && CheckBoxUseInFight.IsChecked.Value,
                        TargetId = Convert.ToInt32(TextBoxTargetId.Text),
                        UseInPVP = CheckBoxUseInPVP.IsChecked.Value
                    }

                    );
                //сброс настроек по умолчанию
                ResetSettingSkillDefault();
                GroupBoxSettingsSkill.Header = "Настройки";
                ButtonAddSkill.IsEnabled = false;
                //   ListViewActiveSkill.Items.Refresh();
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
                if (ListViewActiveSkill.SelectedIndex == -1)
                    return;

                var skill = ListViewActiveSkill.SelectedItem as SkillSettings;
                ListViewAllSkill.SelectedIndex = -1;
                ButtonChangeSkill.IsEnabled = true;
                ButtonDelSkill.IsEnabled = true;
                ButtonAddSkill.IsEnabled = false;
                if (skill != null)
                {
                    GroupBoxSettingsSkill.Header = "Настройки " + skill.Name + "[" + skill.Id + "]";
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
                    TextBoxMinLevel.Text = skill.MinLevel.ToString();
                    TextBoxMaxLevel.Text = skill.MaxLevel.ToString();

                    TextBoxNotTargetEffect.Text = skill.NotTargetEffect.ToString();
                    TextBoxNotMeEffect.Text = skill.NotMeEffect.ToString();
                    TextBoxIsTargetEffect.Text = skill.IsTargetEffect.ToString();
                    TextBoxIsMeEffect.Text = skill.IsMeEffect.ToString();

                    TextBoxAlternatePowerMore.Text = skill.CombatElementCountMore.ToString();
                    TextBoxAlternatePowerLess.Text = skill.CombatElementCountLess.ToString();

                    TextBoxTargetId.Text = skill.TargetId.ToString();
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



        public class MyQuestTemplate1
        {
            public uint Id { get; set; }
            public uint RewardFactionFlags { get; set; }
            public int[] RewardFactionCapIns { get; set; }
            public int[] RewardFactionValues { get; set; }
            public int[] RewardFactionOverrides { get; set; }
            public uint[] RewardFactionIDs { get; set; }
            public uint QuestTurnInPortrait { get; set; }
            public uint[] RewardCurrencyIDs { get; set; }
            public uint PortraitGiverMount { get; set; }
            public uint RewardNumSkillUps { get; set; }
            public uint RewardSkillLineID { get; set; }
            public uint RewardArenaPoints { get; set; }
            public uint RewardTitle { get; set; }
            public int POIPriorityWod { get; set; }
            public float POIy { get; set; }
            public uint QuestGiverPortrait { get; set; }
            public float POIx { get; set; }
            public uint[] RewardCurrencyCounts { get; set; }
            public uint SoundTurnIn { get; set; }
            public string QuestTurnTargetName { get; set; }
            public string QuestTurnTextWindow { get; set; }
            public string QuestGiverTargetName { get; set; }
            public string QuestGiverTextWindow { get; set; }
            public string AreaDescription { get; set; }
            public string QuestDescription { get; set; }
            public uint SoundAccept { get; set; }
            public string LogDescription { get; set; }
            public QuestObjective[] QuestObjectives { get; set; }
            public int Expansion { get; set; }
            public int QuestRewardID { get; set; }
            public long AllowableRaces { get; set; }
            public uint AreaGroupID { get; set; }
            public uint TimeAllowed { get; set; }
            public string LogTitle { get; set; }
            public string QuestCompletionLog { get; set; }
            public uint POIContinent { get; set; }
            public uint[] RewardChoiceItemQuantits { get; set; }
            public int RewardMoney { get; set; }
            public float RewardXPMultiplier { get; set; }
            public uint RewardXPDifficulty { get; set; }
            public uint RewardNextQuest { get; set; }
            public uint SuggestedGroupNum { get; set; }
            public EQuestInfo QuestInfoID { get; set; }
            public uint RewardMoneyDifficulty { get; set; }
            public EQuestSort QuestSortID { get; set; }
            public int QuestMaxScalingLevel { get; set; }
            public uint QuestPackageID { get; set; }
            public int QuestLevel { get; set; }
            public EQuestType QuestType { get; set; }
            public KeyValuePair<int, bool> QuestID { get; set; }
            public bool HasData { get; set; }
            public int MinLevel { get; set; }
            public uint[] RewardChoiceItemDiplayIDs { get; set; }
            public float RewardMoneyMultiplier { get; set; }
            public uint[] RewardDisplaySpell { get; set; }
            public uint[] RewardChoiceItemIDs { get; set; }
            public uint[] ItemDropQuantitys { get; set; }
            public uint[] ItemDrops { get; set; }
            public uint[] RewardAmounts { get; set; }
            public uint[] RewardItems { get; set; }
            public EQuestFlags2 FlagsEx { get; set; }
            public uint RewardBonusMoney { get; set; }
            public EQuestFlags Flags { get; set; }
            public uint RewardArtifactCategoryID { get; set; }
            public float RewardArtifactXPMultiplier { get; set; }
            public uint RewardArtifactXPDifficulty { get; set; }
            public float RewardKillHonor { get; set; }
            public uint RewardHonor { get; set; }
            public uint RewardSpell { get; set; }
            public uint StartItem { get; set; }
        }

        /// <summary>
        /// Открывает настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void subItem1_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (!IsToggleDone)
                    return;

                IsToggleDone = false;

                if (!IsToggle)
                {

                    _collectionQuests = Host.GetQuests();

                  

                    DataGridQuest.ItemsSource = _collectionQuests;


                    DataGridQuestTemplate.ItemsSource = collectionQuestTemplate;

                    _collectionLfg.Add(Host.LFGStatus);
                    DataGridLfg.ItemsSource = _collectionLfg;

                    _collectionLfgProposals.Add(Host.LFGStatus.Proposal);
                    DataGridLfg_Copy.ItemsSource = _collectionLfgProposals;

                    _collectionEntities = Host.GetEntities<Unit>();
                    DataGridEntity.ItemsSource = _collectionEntities;

                    _collectionScenarios.Add(Host.Scenario);
                    DataGridScenario.ItemsSource = _collectionScenarios;

                    _collectionCriterias = Host.Scenario.GetCriterias();
                    DataGridScenarioCriteria.ItemsSource = _collectionCriterias;

                    _collectionItems = Host.ItemManager.GetItems();
                    DataGridItem.ItemsSource = _collectionItems;

                    _collectionAuras = Host.Me.GetAuras();
                    DataGridBuff.ItemsSource = _collectionAuras;

                    _collectionSpell = Host.SpellManager.GetSpells();
                    DataGridSkill.ItemsSource = _collectionSpell;

                    if (Host.Me.Target != null)
                    {
                        _collectionAurasTarget = Host.Me.Target.GetAuras();
                        DataGridBuffTarget.ItemsSource = _collectionAurasTarget;
                    }

                    // if (Host.Me?.ClassType != EClass.Assassin)
                    //  GroupBoxMarkofDeathSettings.Visibility = Visibility.Hidden;
                    IsToggle = true;
                    Main1.Height = 750;
                    Main1.Width = 1294 + 300;
                    GridSettings.Width = Main1.ActualWidth - 300;
                    GridMain.Width = 300;
                    GridMain.HorizontalAlignment = HorizontalAlignment.Right;
                    if (Main1.ActualHeight > 55)
                        GridSettings.Height = Main1.ActualHeight - 55;
                    else
                        GridSettings.Height = 0;

                    GridSettings.Margin = new Thickness(0, 25, 0, 0);

                    UpdateQuest();
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
                        /* if (Host.NoShowSkill.Contains(skill.Id))
                             continue;*/
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


                        /* if (skill.Id == 3365)//3365   Opening 
                             continue;
                         if (skill.Id == 6233)//6233   Closing 
                             continue;
                         if (skill.Id == 6246)//6246   Closing 
                             continue;
                         if (skill.Id == 0)//3365   Opening 
                             continue;
                         if (skill.Id == 0)//3365   Opening 
                             continue;
                         if (skill.Id == 0)//3365   Opening 
                             continue;
                         if (skill.Id == 0)//3365   Opening 
                             continue;
                         if (skill.Id == 0)//3365   Opening 
                             continue;
                         if (skill.Id == 0)//3365   Opening 
                             continue;*/

                        try
                        {
                            if (!skill.IsPassive())
                                CollectionAllSkill.Add(new SkillTable(skill.Name, skill.Id, skill.IsPassive()));
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    //ComboBoxSwitchMode.Visibility = Visibility.Visible;
                    IsToggle = false;
                    GridSettings.Margin = new Thickness(0, 25, 0, 0);
                    GridSettings.Height = 0;
                    GridSettings.Width = 0;

                    // if (Main1.Width > 300 || Main1.Height > 400)
                    // {
                    Main1.Width = 300;
                    Main1.Height = 450;
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
                var initDir = Host.AssemblyDirectory + "\\Plugins\\Quester\\Configs";
                if (Host.isReleaseVersion)
                    initDir = Host.AssemblyDirectory + "\\Configs";

                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = initDir,// Host.AssemblyDirectory + "\\Configs",
                    Filter = @"json files (*.json)|*.json|All files|*.*",
                    RestoreDirectory = true
                };
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                if (System.IO.Path.GetExtension(openFileDialog.FileName) != ".json")
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
                var initDir = Host.AssemblyDirectory + "\\Plugins\\Quester\\Configs";
                if (Host.isReleaseVersion)
                    initDir = Host.AssemblyDirectory + "\\Configs";

                var saveFileDialog = new SaveFileDialog
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
                Host.FileName = System.IO.Path.GetFileName(saveFileDialog.FileName);
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

                if (ListViewActiveSkill.SelectedIndex == -1) return;

                var tempNotTargetEffect = 0;
                // if (ComboBoxNotTargetEffect.SelectedIndex != -1)
                tempNotTargetEffect = Convert.ToInt32(TextBoxNotTargetEffect.Text);//GetSkillIdFromCombobox(ComboBoxNotTargetEffect.Text);

                var tempNotMeEffect = 0;
                // if (ComboBoxNotMeEffect.SelectedIndex != -1)
                tempNotMeEffect = Convert.ToInt32(TextBoxNotMeEffect.Text);// GetSkillIdFromCombobox(ComboBoxNotMeEffect.Text);

                var tempIsMeEffect = 0;
                //  if (ComboBoxIsMeEffect.SelectedIndex != -1)
                tempIsMeEffect = Convert.ToInt32(TextBoxIsMeEffect.Text);//GetSkillIdFromCombobox(ComboBoxIsMeEffect.Text);

                var tempIsTargetEffect = 0;
                // if (ComboBoxIsTargetEffect.SelectedIndex != -1)
                tempIsTargetEffect = Convert.ToInt32(TextBoxIsTargetEffect.Text);//GetSkillIdFromCombobox(ComboBoxIsTargetEffect.Text);

                var activeskill = ListViewActiveSkill.SelectedItem as SkillSettings;
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
                                MinLevel = Convert.ToInt32(TextBoxMinLevel.Text),
                                MaxLevel = Convert.ToInt32(TextBoxMaxLevel.Text),
                                CombatElementCountMore = Convert.ToInt32(TextBoxAlternatePowerMore.Text),
                                CombatElementCountLess = Convert.ToInt32(TextBoxAlternatePowerLess.Text),
                                UseInFight = CheckBoxUseInFight.IsChecked != null && CheckBoxUseInFight.IsChecked.Value,
                                TargetId = Convert.ToInt32(TextBoxTargetId.Text),
                                UseInPVP = CheckBoxUseInPVP.IsChecked.Value

                            };
                    }
                }
                GroupBoxSettingsSkill.Header = "Настройки";
                ButtonChangeSkill.IsEnabled = false;
                ButtonDelSkill.IsEnabled = false;
                ResetSettingSkillDefault();
                //  ListViewActiveSkill.Items.Refresh();
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
                CheckBoxHideQuesterUI.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        CollectionQuestSettings.RemoveAll();
                        foreach (var dungeonCoordSetting in Host.QuestSettings.QuestCoordSettings)
                        {
                            var name = dungeonCoordSetting.QuestName;
                            var minLevel = 0;
                            var Level = 0;
                            if (Host.GameDB.QuestTemplates.ContainsKey(dungeonCoordSetting.QuestId))
                            {
                                if (dungeonCoordSetting.QuestName == "")
                                    name = Host.GameDB.QuestTemplates[dungeonCoordSetting.QuestId].LogTitle;
                                minLevel = Host.GameDB.QuestTemplates[dungeonCoordSetting.QuestId].MinLevel;
                                Level = Host.GameDB.QuestTemplates[dungeonCoordSetting.QuestId].QuestLevel;
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
                                Level = Level,


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
                CheckBoxHideQuesterUI.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        CollectionDungeonCoord.RemoveAll();

                        CollectionDungeonCoord = new ObservableCollection<DungeonCoordSettings>(Host.DungeonSettings.DungeonCoordSettings);
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
                                PluginPath = dungeonCoordSetting.PluginPath

                            });
                        }
                        //  ListViewDangeonFunction.ItemsSource = CollectionDungeonCoord;
                        // Host.log(CollectionDungeonCoord.Count + " " + ListViewDangeonFunction.Items.Count);
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
                CheckBoxHideQuesterUI.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        LabelProfile.Content = Host.FileName;
                        //Настройки


                        //Общее
                        CheckBoxHideQuesterUI.IsChecked = Host.CharacterSettings.HideQuesterUi;
                        CheckBoxAutoEquip.IsChecked = Host.CharacterSettings.AutoEquip;
                        CheckBoxRunQuestHerbalism.IsChecked = Host.CharacterSettings.RunQuestHerbalism;
                        CheckBoxUseFilterMobs.IsChecked = Host.CharacterSettings.UseFilterMobs;
                        CheckBoxCheckRepairAndSell.IsChecked = Host.CharacterSettings.CheckRepairAndSell;
                        CheckBoxUnmountMoveFail.IsChecked = Host.CharacterSettings.UnmountMoveFail;
                        CheckBoxWaitSixMin.IsChecked = Host.CharacterSettings.WaitSixMin;
                        CheckBoxFightIfMobs.IsChecked = Host.CharacterSettings.FightIfMobs;
                        CheckBoxLaunchSkript.IsChecked = Host.CharacterSettings.LaunchScript;
                        if (Host.CharacterSettings.UseMultiZone)
                        {
                            radioButtonMultiZone.IsChecked = true;
                            radioButtonOneZone.IsChecked = false;
                        }
                        else
                        {
                            radioButtonMultiZone.IsChecked = false;
                            radioButtonOneZone.IsChecked = true;
                        }
                        CheckBoxAllLog.IsChecked = Host.CharacterSettings.LogAll;
                        CheckBoxUseSMountMyLoc.IsChecked = Host.CharacterSettings.UseMountMyLoc;
                        CheckBoxCheckRepair.IsChecked = Host.CharacterSettings.CheckRepair;
                        textBoxRepairCount.Text = Host.CharacterSettings.RepairCount.ToString();
                        textBoxFreeInvCountForAuk.Text = Host.CharacterSettings.FreeInvCountForAuk.ToString();
                        CheckBoxCheckAuk.IsChecked = Host.CharacterSettings.CheckAuk;
                        CheckBoxUseStoneForSellAndRepair.IsChecked = Host.CharacterSettings.UseStoneForSellAndRepair;
                        ComboBoxSummonBattlePetNumber.SelectedIndex = Host.CharacterSettings.BattlePetNumber;
                        textBoxFreeInvCountForAukId.Text = Host.CharacterSettings.FreeInvCountForAukId.ToString();
                        CheckBoxWorldQuest.IsChecked = Host.CharacterSettings.WorldQuest;
                        CheckBoxDebuffDeath.IsChecked = Host.CharacterSettings.DebuffDeath;
                        CheckBoxAOEFarm.IsChecked = Host.CharacterSettings.AoeFarm;
                        textBoxAOEMobsCount.Text = Host.CharacterSettings.AoeMobsCount.ToString();
                        CheckBoxAttackForSitMount.IsChecked = Host.CharacterSettings.CheckBoxAttackForSitMount;
                        CheckBoxSummonBattlePet.IsChecked = Host.CharacterSettings.SummonBattlePet;
                        CheckBoxStopQuesting.IsChecked = Host.CharacterSettings.StopQuesting;
                        textBoxStopQuestingLevel.Text = Host.CharacterSettings.StopQuestingLevel.ToString();
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

                        ComboBoxAukTime.SelectedIndex = Host.CharacterSettings.AukTime;


                        CheckBoxSendMail.IsChecked = Host.CharacterSettings.SendMail;
                        textBoxSendMailName.Text = Host.CharacterSettings.SendMailName;

                        textBoxSendMailLocX.Text = Host.CharacterSettings.SendMailLocX.ToString();
                        textBoxSendMailLocY.Text = Host.CharacterSettings.SendMailLocY.ToString();
                        textBoxSendMailLocZ.Text = Host.CharacterSettings.SendMailLocZ.ToString();
                        textBoxSendMailLocMapId.Text = Host.CharacterSettings.SendMailLocMapId.ToString();
                        textBoxSendMailLocAreaId.Text = Host.CharacterSettings.SendMailLocAreaId.ToString();
                        try
                        {
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                            textBoxSendMailStartTime.Text = Host.CharacterSettings.SendMailStartTime.ToString("hh':'mm");
                            textBoxSendMailEndTime.Text = Host.CharacterSettings.SendMailStopTime.ToString("hh':'mm");
                           
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                        }
                        catch (Exception e)
                        {
                            Host.log(e + " ");
                        }



                        textBoxValuta.Text = Host.CharacterSettings.Valuta;
                        textBoxPrice.Text = Host.CharacterSettings.Price.ToString();
                        textBoxPriceKK.Text = Host.CharacterSettings.PriceKK.ToString();

                        textBoxFreeInvCount.Text = Host.CharacterSettings.InvFreeSlotCount.ToString();

                        CheckBoxScriptReverse.IsChecked = Host.CharacterSettings.ScriptReverse;
                        CheckBoxUseWhistleForSellAndRepair.IsChecked = Host.CharacterSettings.UseWhistleForSellAndRepair;
                        try
                        {
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                            textBoxCheckAukStartTime.Text = Host.CharacterSettings.StartAukTime.ToString("hh':'mm");
                            textBoxCheckAukEndTime.Text = Host.CharacterSettings.EndAukTime.ToString("hh':'mm");
                            CheckBoxCheckAukTime.IsChecked = Host.CharacterSettings.CheckAukInTimeRange;
                            // Host.log(Host.CharacterSettings.StartAukTime.ToString() + "   ");
                            // Host.log(Host.CharacterSettings.EndAukTime.ToString() + "   ");
                        }
                        catch (Exception e)
                        {
                            Host.log(e + " ");
                        }

                        CheckBoxAukRun.IsChecked = Host.CharacterSettings.AukRun;
                        textBoxAukLocX.Text = Host.CharacterSettings.AukLocX.ToString();
                        textBoxAukLocY.Text = Host.CharacterSettings.AukLocY.ToString();
                        textBoxAukLocZ.Text = Host.CharacterSettings.AukLocZ.ToString();
                        textBoxAukLocMapId.Text = Host.CharacterSettings.AukMapId.ToString();
                        textBoxAukLocAreaId.Text = Host.CharacterSettings.AukAreaId.ToString();

                        textBoxGatherLocX.Text = Host.CharacterSettings.GatherLocX.ToString();
                        textBoxGatherLocY.Text = Host.CharacterSettings.GatherLocY.ToString();
                        textBoxGatherLocZ.Text = Host.CharacterSettings.GatherLocZ.ToString();
                        textBoxGatherLocMapId.Text = Host.CharacterSettings.GatherLocMapId.ToString();
                        textBoxGatherLocAreaId.Text = Host.CharacterSettings.GatherLocAreaId.ToString();
                        textBoxGatherLocRadius.Text = Host.CharacterSettings.GatherRadius.ToString();

                        textBoxFarmLocX.Text = Host.CharacterSettings.FarmLocX.ToString();
                        textBoxFarmLocY.Text = Host.CharacterSettings.FarmLocY.ToString();
                        textBoxFarmLocZ.Text = Host.CharacterSettings.FarmLocZ.ToString();
                        textBoxFarmLocMapId.Text = Host.CharacterSettings.FarmLocMapId.ToString();
                        textBoxFarmLocAreaId.Text = Host.CharacterSettings.FarmLocAreaId.ToString();
                        textBoxFarmLocRadius.Text = Host.CharacterSettings.FarmRadius.ToString();

                        textBoxMountLocX.Text = Host.CharacterSettings.MountLocX.ToString();
                        textBoxMountLocY.Text = Host.CharacterSettings.MountLocY.ToString();
                        textBoxMountLocZ.Text = Host.CharacterSettings.MountLocZ.ToString();
                        textBoxMountLocAreaId.Text = Host.CharacterSettings.MountLocAreaId.ToString();
                        textBoxMountLocMapId.Text = Host.CharacterSettings.MountLocMapId.ToString();

                        CheckBoxLogScript.IsChecked = Host.CharacterSettings.LogScriptAction;
                        CheckBoxLogSkill.IsChecked = Host.CharacterSettings.LogSkill;

                        CheckBoxForceMoveScriptEnable.IsChecked = Host.CharacterSettings.ForceMoveScriptEnable;
                        textBoxForceMoveScriptDist.Text = Host.CharacterSettings.ForceMoveScriptDist.ToString();
                        CheckBoxSummonMount.IsChecked = Host.CharacterSettings.SummonMount;
                        CheckBoxFightIfHPLess.IsChecked = Host.CharacterSettings.FightIfHPLess;
                        textBoxFightIfHPLessCount.Text = Host.CharacterSettings.FightIfHPLessCount.ToString();

                        CheckBoxSkining.IsChecked = Host.CharacterSettings.Skinning;
                        CheckBoxNoAttackOnMount.IsChecked = Host.CharacterSettings.NoAttackOnMount;
                        CheckBoxScriptScheduleEnable.IsChecked = Host.CharacterSettings.ScriptScheduleEnable;
                        CheckBoxGatherResourceScript.IsChecked = Host.CharacterSettings.GatherResouceScript;
                        textBoxGatherRadiusScript.Text = Host.CharacterSettings.GatherRadiusScript.ToString();

                        CheckBoxAttack.IsChecked = Host.CharacterSettings.Attack;
                        textBoxAttackRadius.Text = Host.CharacterSettings.AttackRadius.ToString();

                        CheckBoxUseDash.IsChecked = Host.CharacterSettings.UseDash;

                        textBoxFarmZRange.Text = Host.CharacterSettings.Zrange.ToString();
                        CheckBoxFindBestPoint.IsChecked = Host.CharacterSettings.FindBestPoint;


                        if (Host.CharacterSettings.Mode == EMode.Questing)
                            ComboBoxSwitchMode.SelectedIndex = 0;
                        if (Host.CharacterSettings.Mode == EMode.FarmMob)
                            ComboBoxSwitchMode.SelectedIndex = 1;
                        if (Host.CharacterSettings.Mode == EMode.FarmResource)
                            ComboBoxSwitchMode.SelectedIndex = 2;
                        if (Host.CharacterSettings.Mode == EMode.Script)
                            ComboBoxSwitchMode.SelectedIndex = 3;
                        if (Host.CharacterSettings.Mode == EMode.OnlyAttack)
                            ComboBoxSwitchMode.SelectedIndex = 4;
                       





                        /*
                                                if (Host.CharacterSettings.Mode == "Данж.(п)")
                                                {
                                                    foreach (var item in ComboBoxSwitchMode.Items)
                                                    {
                                                        if (item.ToString() == "Данж.(п)")
                                                        {
                                                            ComboBoxSwitchMode.SelectedItem = item;
                                                        }
                                                    }
                                                }
                                                */



                        // comboBoxDungeonScript.SelectedIndex = 0;
                        if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                        {
                            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");
                            foreach (var file in dir.GetFiles())
                            {
                                comboBoxDungeonScript.Items.Add(file.Name);
                            }
                        }
                        foreach (var item in comboBoxDungeonScript.Items)
                        {
                            if (item.ToString() == Host.CharacterSettings.Script)
                            {
                                comboBoxDungeonScript.SelectedItem = item;
                            }
                        }


                        if (Directory.Exists(Host.PathQuestSet))
                        {
                            var dir = new DirectoryInfo(Host.PathQuestSet);
                            foreach (var file in dir.GetFiles())
                            {
                                comboBoxQuestSet.Items.Add(file.Name);
                            }
                        }
                        foreach (var item in comboBoxQuestSet.Items)
                        {
                            if (item.ToString() == Host.CharacterSettings.Quest)
                            {
                                comboBoxQuestSet.SelectedItem = item;
                            }
                        }



                        ComboBoxSwitchQuestMode.SelectedIndex = Host.CharacterSettings.QuestMode;


                        CheckBoxPickUpLoot.IsChecked = Host.CharacterSettings.PickUpLoot;
                        textBoxIgnoreMob.Text = Host.CharacterSettings.IgnoreMob.ToString();


                        textBoxEquipStateWeapon.Text = Host.CharacterSettings.EquipItemStat.ToString();
                        //Умения
                        CollectionActiveSkill.RemoveAll();
                        CollectionInvItems.RemoveAll();

                        CollectionMobs.RemoveAll();
                        CollectionEventSettings.RemoveAll();
                        CollectionPetSettings.RemoveAll();
                        CollectionIgnoreMb.RemoveAll();
                        CollectionIgnoreQuest.RemoveAll();
                        CollectionProps.RemoveAll();
                        CollectionMultiZone.RemoveAll();
                        CollectionNpcForActions.RemoveAll();
                        CollectionMyBuffs.RemoveAll();

                        CollectionItemGlobals.RemoveAll();
                        CollectionGameObjectIgnores.RemoveAll();

                        CollectionAukSettingses.RemoveAll();
                        CollectionScriptSchedules.RemoveAll();
                        CollectionEquipAuc.RemoveAll();

                        foreach (var characterSettingsEquipAuc in Host.CharacterSettings.EquipAucs)
                        {
                            CollectionEquipAuc.Add(new EquipAuc
                            {
                                Slot = characterSettingsEquipAuc.Slot,
                                Name = characterSettingsEquipAuc.Name,
                                MaxPrice = characterSettingsEquipAuc.MaxPrice,
                                Level = characterSettingsEquipAuc.Level,
                                Stat1 = characterSettingsEquipAuc.Stat1,
                                Stat2 = characterSettingsEquipAuc.Stat2
                            });
                        }

                        foreach (var characterSettingsScriptSchedule in Host.CharacterSettings.ScriptSchedules)
                        {
                            CollectionScriptSchedules.Add(new ScriptSchedule
                            {
                                ScriptName = characterSettingsScriptSchedule.ScriptName,
                                ScriptStopTime = characterSettingsScriptSchedule.ScriptStopTime,
                                ScriptStartTime = characterSettingsScriptSchedule.ScriptStartTime,
                                Reverse = characterSettingsScriptSchedule.Reverse
                            });
                        }

                        foreach (var characterSettingsAukSettingse in Host.CharacterSettings.AukSettingses)
                        {
                            CollectionAukSettingses.Add(new AukSettings
                            {
                                Id = characterSettingsAukSettingse.Id,
                                Name = characterSettingsAukSettingse.Name,
                                Level = characterSettingsAukSettingse.Level,
                                Disscount = characterSettingsAukSettingse.Disscount,
                                MaxPrice = characterSettingsAukSettingse.MaxPrice,
                                MaxCount = characterSettingsAukSettingse.MaxCount
                            });
                        }

                        foreach (var characterSettingsGameObjectIgnore in Host.CharacterSettings.GameObjectIgnores)
                        {
                            CollectionGameObjectIgnores.Add(new GameObjectIgnore
                            {
                                Id = characterSettingsGameObjectIgnore.Id,
                                Name = characterSettingsGameObjectIgnore.Name,
                                Ignore = characterSettingsGameObjectIgnore.Ignore,
                                Loc = characterSettingsGameObjectIgnore.Loc
                            });
                        }

                        foreach (var characterSettingsMyItemGlobal in Host.CharacterSettings.MyItemGlobals)
                        {
                            CollectionItemGlobals.Add(new ItemGlobal
                            {
                                Quality = characterSettingsMyItemGlobal.Quality,
                                Class = characterSettingsMyItemGlobal.Class,
                                ItemLevel = characterSettingsMyItemGlobal.ItemLevel
                            });
                        }

                        foreach (var characterSettingsMyBuffSetting in Host.CharacterSettings.MyBuffSettings)
                        {
                            CollectionMyBuffs.Add(new MyBuff
                            {
                                SkillId = characterSettingsMyBuffSetting.SkillId,
                                ItemName = characterSettingsMyBuffSetting.ItemName,
                                ItemId = characterSettingsMyBuffSetting.ItemId
                            });
                        }

                        foreach (var characterSettingsNpcForActionSetting in Host.CharacterSettings.NpcForActionSettings)
                        {
                            CollectionNpcForActions.Add(new NpcForAction
                            {
                                AreaId = characterSettingsNpcForActionSetting.AreaId,
                                Id = characterSettingsNpcForActionSetting.Id,
                                IsArmorer = characterSettingsNpcForActionSetting.IsArmorer,
                                IsAuctioner = characterSettingsNpcForActionSetting.IsAuctioner,
                                IsBanker = characterSettingsNpcForActionSetting.IsBanker,
                                IsBattleMaster = characterSettingsNpcForActionSetting.IsBattleMaster,
                                IsCharmed = characterSettingsNpcForActionSetting.IsCharmed,
                                IsCritter = characterSettingsNpcForActionSetting.IsCritter,
                                IsGossip = characterSettingsNpcForActionSetting.IsGossip,
                                IsInnkeeper = characterSettingsNpcForActionSetting.IsInnkeeper,
                                IsServiceProvider = characterSettingsNpcForActionSetting.IsServiceProvider,
                                IsSpiritGuide = characterSettingsNpcForActionSetting.IsSpiritGuide,
                                IsSpiritHealer = characterSettingsNpcForActionSetting.IsSpiritHealer,
                                IsSpiritService = characterSettingsNpcForActionSetting.IsSpiritService,
                                IsTabardDesigner = characterSettingsNpcForActionSetting.IsTabardDesigner,
                                IsTapped = characterSettingsNpcForActionSetting.IsTapped,
                                IsTaxi = characterSettingsNpcForActionSetting.IsTaxi,
                                IsTotem = characterSettingsNpcForActionSetting.IsTotem,
                                IsTrainer = characterSettingsNpcForActionSetting.IsTrainer,
                                IsTurning = characterSettingsNpcForActionSetting.IsTurning,
                                IsVendor = characterSettingsNpcForActionSetting.IsVendor,
                                Name = characterSettingsNpcForActionSetting.Name,
                                Use = characterSettingsNpcForActionSetting.Use,
                                Loc = characterSettingsNpcForActionSetting.Loc,
                                MapId = characterSettingsNpcForActionSetting.MapId
                            });
                        }

                        foreach (var ignoreQuestsSettingse in Host.CharacterSettings.IgnoreQuests)
                        {
                            CollectionIgnoreQuest.Add(new IgnoreQuest
                            {
                                Id = ignoreQuestsSettingse.Id,
                                Name = ignoreQuestsSettingse.Name
                            });
                        }

                        foreach (var ignoreMbSettingse in Host.CharacterSettings.IgnoreMB)
                        {
                            CollectionIgnoreMb.Add(new IgnoreMB
                            {
                                Id = ignoreMbSettingse.Id,
                                Name = ignoreMbSettingse.Name
                            });
                        }

                        foreach (var petSettingse in Host.CharacterSettings.PetSettings)
                        {
                            CollectionPetSettings.Add(new PetSettings
                            {
                                Id = petSettingse.Id,
                                Name = petSettingse.Name,
                                Type = petSettingse.Type
                            });
                        }

                        foreach (var characterSettingsEventSetting in Host.CharacterSettings.EventSettings)
                        {
                            CollectionEventSettings.Add(new EventSettings
                            {
                                ActionEvent = characterSettingsEventSetting.ActionEvent,
                                TypeEvents = characterSettingsEventSetting.TypeEvents,
                                SoundFile = characterSettingsEventSetting.SoundFile,
                                Pause = characterSettingsEventSetting.Pause
                            });
                        }

                        foreach (var characterSettingsMultiZone in Host.CharacterSettings.MultiZones)
                        {
                            CollectionMultiZone.Add(new MultiZone
                            {
                                Id = characterSettingsMultiZone.Id,
                                ChangeByTime = characterSettingsMultiZone.ChangeByTime,
                                Loc = characterSettingsMultiZone.Loc,
                                Radius = characterSettingsMultiZone.Radius,
                                Time = characterSettingsMultiZone.Time,
                                ChangeByDeathPlayer = characterSettingsMultiZone.ChangeByDeathPlayer,
                                CountDeathByPlayer = characterSettingsMultiZone.CountDeathByPlayer
                            });
                        }

                        //Мобы
                        foreach (var mobsSettingse in Host.CharacterSettings.MobsSettings)
                        {
                            CollectionMobs.Add(new MobsSettings
                            {
                                Id = mobsSettingse.Id,
                                Level = mobsSettingse.Level,
                                Name = mobsSettingse.Name,
                                //  Name = Host.GameDB.npc_origin[mobsSettingse.Id]?.mMonsterName,
                                Priority = mobsSettingse.Priority,

                                // Level = Host.GameDB.npc_origin[mobsSettingse.Id].Level
                            });
                        }

                        foreach (var propSettingse in Host.CharacterSettings.PropssSettings)
                        {
                            CollectionProps.Add(new PropSettings
                            {
                                Id = propSettingse.Id,
                                Name = propSettingse.Name,
                                //  Name = Host.GameDB.npc_origin[propSettingse.Id]?.mMonsterName,
                                Priority = propSettingse.Priority,

                            });
                        }


                        //Предметы
                        foreach (var itemSettingse in Host.CharacterSettings.ItemSettings)
                        {
                            CollectionInvItems.Add(new ItemSettings
                            {
                                Id = itemSettingse.Id,
                                //   Name = Host.GameDB.item_info[itemSettingse.Id]?.mItemName,                               
                                Use = itemSettingse.Use,
                                Name = itemSettingse.Name,
                                Class = itemSettingse.Class,
                                Quality = itemSettingse.Quality,
                                EnableSale = itemSettingse.EnableSale,
                                SellPrice = itemSettingse.SellPrice,
                                MeLevel = itemSettingse.MeLevel
                            });
                        }

                        //Активные умения
                        foreach (var i in Host.CharacterSettings.SkillSettings)
                        {

                            CollectionActiveSkill.Add(new SkillSettings
                            {

                                Checked = i.Checked,
                                Name = i.Name,
                                Id = i.Id,

                                Priority = i.Priority,
                                MeMinHp = i.MeMinHp,
                                MeMaxHp = i.MeMaxHp,
                                MeMinMp = i.MeMinMp,
                                MeMaxMp = i.MeMaxMp,
                                TargetMaxHp = i.TargetMaxHp,
                                TargetMinHp = i.TargetMinHp,
                                BaseDist = i.BaseDist,
                                MaxDist = i.MaxDist,
                                MinDist = i.MinDist,
                                MoveDist = i.MoveDist,
                                AoeMax = i.AoeMax,
                                AoeRadius = i.AoeRadius,
                                AoeMin = i.AoeMin,
                                AoeMe = i.AoeMe,
                                SelfTarget = i.SelfTarget,
                                NotTargetEffect = i.NotTargetEffect,
                                NotMeEffect = i.NotMeEffect,
                                IsMeEffect = i.IsMeEffect,
                                IsTargetEffect = i.IsTargetEffect,
                                MinLevel = i.MinLevel,
                                MaxLevel = i.MaxLevel,
                                CombatElementCountMore = i.CombatElementCountMore,
                                CombatElementCountLess = i.CombatElementCountLess,

                                UseInFight = i.UseInFight,
                                TargetId = i.TargetId,

                                UseInPVP = i.UseInPVP

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
        /// Кнопка удалить скилл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDelSkill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ListViewActiveSkill.Dispatcher.Invoke(() =>
                {
                    if (ListViewActiveSkill.SelectedIndex != -1)
                    {
                        CollectionActiveSkill.RemoveAt(ListViewActiveSkill.SelectedIndex);
                        ButtonDelSkill.IsEnabled = false;
                        ButtonChangeSkill.IsEnabled = false;
                        ResetSettingSkillDefault();
                    }
                });
                //    ListViewActiveSkill.Items.Refresh();
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
                textBoxGatherLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                textBoxGatherLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                textBoxGatherLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                textBoxGatherLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
                textBoxGatherLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
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
                if (comboBoxInvItems.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(comboBoxInvItems.Text));


                /*  if (_collectionInvItems == null)
              {
                  _collectionInvItems = new ObservableCollection<ItemSettings>();
                   listViewInvItems.ItemsSource = _collectionInvItems;
              }*/
                if (CollectionInvItems.Any(collectionInvItem => tempId == collectionInvItem.Id))
                    return;

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
                    Use = comboBoxInvItemsUse.SelectedIndex,
                    //  EnableSale = Host.GameDB.item_info[tempId].mIsSellable,
                    //   SellPrice = item.GetSellPrice(),
                    MeLevel = Convert.ToInt32(textBoxInvMeLevel.Text),
                    Class = item.ItemClass,
                    Quality = item.ItemQuality
                });
                //  Host.log(CollectionInvItems.Count + " " + Host.CharacterSettings.ItemSettings.Count);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        /// <summary>
        /// Удалить итем  из списка инвентаря
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonInvItemsDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewInvItems.SelectedIndex != -1)
                    CollectionInvItems.RemoveAt(listViewInvItems.SelectedIndex);
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
                        Use = comboBoxInvItemsUse.SelectedIndex,

                        //EnableSale = item.Db.mIsSellable,
                        //      SellPrice = item.GetSellPrice(),
                        MeLevel = Convert.ToInt32(textBoxInvMeLevel.Text),
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
                comboBoxMobs.Items.Clear();
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
                    if (!comboBoxMobs.Items.Contains("[" + creature.Id + "]" + creature.Name))
                        comboBoxMobs.Items.Add("[" + creature.Id + "]" + creature.Name);
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
                if (comboBoxMobs.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(comboBoxMobs.Text));



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

        private void buttonInvItemsDel1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewMobs.SelectedIndex != -1)
                    CollectionMobs.RemoveAt(listViewMobs.SelectedIndex);
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

        public void CheckPrivateFunction()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    Host.log("Проверяю приватные функции. Пользователь " + Host.GetBotLogin());

                    if (Host.GetBotLogin() == "alex" || Host.GetBotLogin() == "Zakkk" || (string.Compare(Host.GetBotLogin(), "outside", true) == 0) || Host.GetBotLogin() == "Daredevi1")
                    {
                        Host.log("Доступно:");
                        Host.log("Фарм элиток");
                        ComboBoxSwitchMode.Items.Add("Фарм элиток");

                        Host.log("Данж.(п)");
                        ComboBoxSwitchMode.Items.Add("Данж.(п)");
                        TabItemDungeonP.Visibility = Visibility.Visible;
                        Main1.Activate();
                        //AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"
                        if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                        {
                            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");
                            // папка с файлами
                            foreach (FileInfo file in dir.GetFiles())
                            {
                                comboBoxDungeonScript.Items.Add(file.Name);
                            }
                        }
                    }
                    if (Host.GetBotLogin() == "alex" || Host.GetBotLogin() == "Zakkk" || Host.GetBotLogin() == "Daredevi1")
                    {
                        Host.log("Доступно:");
                        Host.log("Заслон");
                        ComboBoxSwitchMode.Items.Add("Заслон");


                        //AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"                     
                    }



                });
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
                if (comboBoxDungeonMob.SelectedIndex > 0)
                    mobId = GetSkillIdFromCombobox(comboBoxDungeonMob.Text);
                if (comboBoxDungeonProp.SelectedIndex > 0)
                    propId = GetSkillIdFromCombobox(comboBoxDungeonProp.Text);
                if (comboBoxDungeonItem.SelectedIndex > 0)
                    itemId = GetSkillIdFromCombobox(comboBoxDungeonItem.Text);
                if (comboBoxDungeonSkill.SelectedIndex > 0)
                    skillId = GetSkillIdFromCombobox(comboBoxDungeonSkill.Text);

                var tempLoc = new Vector3F();
                /*   if (ComboBoxDungeonAction.SelectedIndex == 3 || ComboBoxDungeonAction.SelectedIndex == 4 ||
                       ComboBoxDungeonAction.SelectedIndex == 6)*/
                tempLoc = Host.Me.Location;






                CollectionDungeonCoord.Add(new DungeonCoordSettings
                {
                    Id = CollectionDungeonCoord.Count,
                    Action = ComboBoxDungeonAction.Text,
                    Loc = tempLoc,
                    MobId = mobId,
                    PropId = propId,
                    Attack = CheckBoxAttackMobs.IsChecked != null && CheckBoxAttackMobs.IsChecked.Value,
                    MapId = Convert.ToInt32(textBoxDungeonLocMapId.Text),
                    AreaId = Convert.ToUInt32(textBoxDungeonLocAreaId.Text),
                    ItemId = itemId,
                    SkillId = Convert.ToUInt32(skillId),
                    Pause = Convert.ToInt32(textBoxScriptPause.Text),
                    PluginPath = comboBoxDungeonPlugin.SelectedItem.ToString()
                });

                comboBoxDungeonMob.SelectedIndex = 0;
                comboBoxDungeonProp.SelectedIndex = 0;
                comboBoxDungeonSkill.SelectedIndex = 0;

                comboBoxDungeonItem.SelectedIndex = 0;
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
                comboBoxDungeonMob.Items.Clear();
                comboBoxDungeonMob.Items.Add("Не выбрано");
                if (Host.Me == null)
                    return;


                foreach (var creature in Host.GetEntities<Unit>())
                {
                    /*   if (creature.Type != EBotTypes.Npc)
                           continue;*/

                    /* if (!Host.CachedDbNpcInfos.ContainsKey(creature.Id))
                        continue;*/
                    /* if (!comboBoxDungeonMob.Items.Contains("[" + creature.Id + "]" + creature?.Db.mMonsterName))
                         comboBoxDungeonMob.Items.Add("[" + creature.Id + "]" + creature?.Db.mMonsterName);*/
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

        private void comboBoxDungeonMob2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                comboBoxDungeonProp.Items.Clear();
                comboBoxDungeonProp.Items.Add("Не выбрано");
                if (Host.Me == null)
                    return;


                /*  foreach (var prop in Host.GetEntities<Prop>())
                  {

                      if (!comboBoxDungeonProp.Items.Contains("[" + prop.Id + "]" + prop.Db.mName))
                          comboBoxDungeonProp.Items.Add("[" + prop.Id + "]" + prop.Db.mName);
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

        private void buttonInvItemsDel2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewDangeonFunction.SelectedIndex != -1)
                    CollectionDungeonCoord.RemoveAt(ListViewDangeonFunction.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
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
            if (ListViewDangeonFunction.SelectedIndex == -1)
                return;
            if (ComboBoxDungeonAction.SelectedIndex < 1)
                return;
            var dungeon = ListViewDangeonFunction.SelectedItem as DungeonCoordSettings;
            if (dungeon != null)
            {
                for (int i = 0; i < CollectionDungeonCoord.Count; i++)
                {
                    if (dungeon == CollectionDungeonCoord[i])
                    {


                        var mobId = 0;
                        var propId = 0;
                        var itemId = 0;
                        var skillId = 0;
                        if (comboBoxDungeonSkill.SelectedIndex > 0)
                            skillId = GetSkillIdFromCombobox(comboBoxDungeonSkill.Text);

                        if (comboBoxDungeonMob.SelectedIndex > 0)
                            mobId = GetSkillIdFromCombobox(comboBoxDungeonMob.Text);
                        if (comboBoxDungeonProp.SelectedIndex > 0)
                            propId = GetSkillIdFromCombobox(comboBoxDungeonProp.Text);
                        if (comboBoxDungeonItem.SelectedIndex > 0)
                            itemId = GetSkillIdFromCombobox(comboBoxDungeonItem.Text);

                        var tempLoc = new Vector3F();
                        if (ComboBoxDungeonAction.SelectedIndex == 3 || ComboBoxDungeonAction.SelectedIndex == 4 ||
                            ComboBoxDungeonAction.SelectedIndex == 6)
                            tempLoc = Host.Me.Location;



                        CollectionDungeonCoord[i] = new DungeonCoordSettings
                        {
                            Id = i,
                            Action = ComboBoxDungeonAction.Text,
                            Loc = tempLoc,
                            MobId = mobId,
                            PropId = propId,
                            Attack = CheckBoxAttackMobs.IsChecked != null && CheckBoxAttackMobs.IsChecked.Value,
                            Pause = Convert.ToInt32(textBoxScriptPause.Text),
                            ItemId = itemId,
                            Com = textBoxDungeonCom.Text,
                            SkillId = Convert.ToUInt32(skillId),
                            MapId = Convert.ToInt32(textBoxDungeonLocMapId.Text),
                            AreaId = Convert.ToUInt32(textBoxDungeonLocAreaId.Text)
                        };

                        comboBoxDungeonMob.SelectedIndex = 0;
                        comboBoxDungeonProp.SelectedIndex = 0;
                        comboBoxDungeonSkill.SelectedIndex = 0;
                        comboBoxDungeonItem.SelectedIndex = 0;
                        CheckBoxAttackMobs.IsChecked = false;
                    }
                }
            }

        }

        private void comboBoxDungeonItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                comboBoxDungeonItem.Items.Clear();
                comboBoxDungeonItem.Items.Add("Не выбрано");
                if (Host.Me == null)
                    return;


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
                var saveFileDialog = new SaveFileDialog
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

        private void buttonDungeonLoadScript_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListViewDangeonFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ListViewDangeonFunction.SelectedIndex == -1)
                    return;
                //   Host.log("тест");
                var df = ListViewDangeonFunction.SelectedItem as DungeonCoordSettings;

                if (df != null)
                {

                    foreach (var item in ComboBoxDungeonAction.Items)
                    {
                        //   Host.log(item + "   " + df.Action);
                        if (item.ToString().Contains(df.Action))
                        {
                            ComboBoxDungeonAction.SelectedItem = item;
                            //   Host.log("Выбираю");
                        }
                    }



                    CheckBoxAttackMobs.IsChecked = df.Attack;
                    //int items;
                    if (df.MobId != 0)
                    {
                        /* items =
                             comboBoxDungeonMob.Items.Add("[" + df.MobId + "]" +
                                                          Host.GameDB.npc_origin[df.MobId].mMonsterName);*/
                        //  comboBoxDungeonMob.SelectedIndex = items;
                    }
                    if (df.PropId != 0)
                    {
                        /* items =
                             comboBoxDungeonProp.Items.Add("[" + df.PropId + "]" +
                                                           Host.GameDB.npc_origin[df.PropId].mMonsterName);*/
                        //  comboBoxDungeonProp.SelectedIndex = items;
                    }

                    if (df.ItemId != 0)
                    {
                        /*   items =
                               comboBoxDungeonItem.Items.Add("[" + df.ItemId + "]" +
                                                             Host.GameDB.item_info[df.ItemId].mItemName);
                           comboBoxDungeonItem.SelectedIndex = items;*/
                    }

                    textBoxDungeonLocX.Text = df.Loc.X.ToString();
                    textBoxDungeonLocY.Text = df.Loc.Y.ToString();
                    textBoxDungeonLocZ.Text = df.Loc.Z.ToString();
                    textBoxScriptPause.Text = df.Pause.ToString();
                    textBoxDungeonLocAreaId.Text = df.AreaId.ToString();
                    textBoxDungeonLocMapId.Text = df.MapId.ToString();
                    textBoxDungeonCom.Text = df.Com;
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonDungeonLocFill_Click(object sender, RoutedEventArgs e)
        {
            textBoxDungeonLocX.Text = Host.Me.Location.X.ToString();
            textBoxDungeonLocY.Text = Host.Me.Location.Y.ToString();
            textBoxDungeonLocZ.Text = Host.Me.Location.Z.ToString();
            textBoxDungeonLocMapId.Text = Host.MapID.ToString();
            textBoxDungeonLocAreaId.Text = Host.Area.Id.ToString();

        }

        private void ButtonUpDungeon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewDangeonFunction.SelectedIndex < 1)
                    return;

                //  while (Mouse.LeftButton == MouseButtonState.Pressed)
                //  {
                Host.log("1312");
                var index = ListViewDangeonFunction.SelectedIndex;
                var buf = CollectionDungeonCoord[index];
                CollectionDungeonCoord[index] = CollectionDungeonCoord[index - 1];
                CollectionDungeonCoord[index - 1] = buf;
                ListViewDangeonFunction.SelectedIndex = index - 1;
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
                if (ListViewDangeonFunction.SelectedIndex == -1)
                    return;
                // Host.log(ListViewDangeonFunction.SelectedIndex + "  " + CollectionDungeonCoord.Count );
                if (ListViewDangeonFunction.SelectedIndex > CollectionDungeonCoord.Count - 2)
                    return;
                var index = ListViewDangeonFunction.SelectedIndex;
                var buf = CollectionDungeonCoord[index];
                CollectionDungeonCoord[index] = CollectionDungeonCoord[index + 1];
                CollectionDungeonCoord[index + 1] = buf;
                ListViewDangeonFunction.SelectedIndex = index + 1;
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
                if (comboBoxEventType.SelectedIndex == -1 || comboBoxEventAction.SelectedIndex == -1)
                    return;
                CharacterSettings.EventsAction Action = CharacterSettings.EventsAction.NotSellected;
                CharacterSettings.EventsType Type = CharacterSettings.EventsType.NotSellected;

                if (comboBoxEventAction.SelectedIndex == 0)
                    Action = CharacterSettings.EventsAction.Log;
                if (comboBoxEventAction.SelectedIndex == 1)
                    Action = CharacterSettings.EventsAction.ExitGame;
                if (comboBoxEventAction.SelectedIndex == 2)
                    Action = CharacterSettings.EventsAction.PlaySound;
                if (comboBoxEventAction.SelectedIndex == 3)
                    Action = CharacterSettings.EventsAction.Pause;
                if (comboBoxEventAction.SelectedIndex == 4)
                    Action = CharacterSettings.EventsAction.ShowGameClient;
                if (comboBoxEventAction.SelectedIndex == 5)
                    Action = CharacterSettings.EventsAction.ShowQuester;

                if (comboBoxEventType.SelectedIndex == 0)
                    Type = CharacterSettings.EventsType.Inactivity;
                if (comboBoxEventType.SelectedIndex == 1)
                    Type = CharacterSettings.EventsType.Death;
                if (comboBoxEventType.SelectedIndex == 2)
                    Type = CharacterSettings.EventsType.DeathPlayer;
                if (comboBoxEventType.SelectedIndex == 3)
                    Type = CharacterSettings.EventsType.ChatMessage;
                if (comboBoxEventType.SelectedIndex == 4)
                    Type = CharacterSettings.EventsType.GMAssHer;
                if (comboBoxEventType.SelectedIndex == 5)
                    Type = CharacterSettings.EventsType.AttackPlayer;
                if (comboBoxEventType.SelectedIndex == 6)
                    Type = CharacterSettings.EventsType.PartyInvite;
                if (comboBoxEventType.SelectedIndex == 7)
                    Type = CharacterSettings.EventsType.ClanInvite;
                if (comboBoxEventType.SelectedIndex == 8)
                    Type = CharacterSettings.EventsType.GMServer;



                CollectionEventSettings.Add(new EventSettings
                {
                    ActionEvent = Action,
                    TypeEvents = Type,
                    SoundFile = textBox.Text,
                    Pause = Convert.ToInt32(textBox_Copy.Text),
                });

            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonEventsDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewEvents.SelectedIndex != -1)
                    CollectionEventSettings.RemoveAt(listViewEvents.SelectedIndex);
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

                comboBoxInvItems1.Items.Clear();

                if (Host.Me == null)
                    return;
                foreach (var item in Host.SpellManager.GetSpells())
                {
                    if (item.IsPartOfSkillLine(777))
                        comboBoxInvItems1.Items.Add("[" + item.Id + "]" + item.Name);
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
                if (comboBoxInvItems1.SelectedIndex == -1)
                    return;


                var tempId = GetSkillIdFromCombobox(comboBoxInvItems1.Text);
                Spell Mount = null;
                foreach (var i in Host.SpellManager.GetSpells())
                {
                    if (i.IsPartOfSkillLine(777))
                    {
                        if (i.Id == tempId)
                        {
                            Mount = i;
                        }
                    }
                }
                if (Mount == null)
                    return;

                if (CollectionPetSettings.Any(collectionInvItem => tempId == collectionInvItem.Id))
                    return;

                CollectionPetSettings.Add(new PetSettings
                {
                    Id = tempId,
                    Name = Mount.Name,
                    Type = "Mount"
                    // Name = Host.GameDB.DBPetInfos[tempId].LocalName,
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonPetDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewPet.SelectedIndex != -1)
                    CollectionPetSettings.RemoveAt(listViewPet.SelectedIndex);
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


                    if (listViewQuest.SelectedIndex != -1)
                    {
                        if (listViewQuest.SelectedIndex + 1 > listViewQuest.Items.Count - 1)
                        {
                            CollectionQuestSettings.Add(item);
                        }
                        else
                        {
                            CollectionQuestSettings.Insert(listViewQuest.SelectedIndex + 1, item);
                        }

                    }
                    else
                    {
                        CollectionQuestSettings.Add(item);
                    }

                    listViewQuest.ScrollIntoView(item);


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



        private void buttonIgnoreQuestDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewQuest.SelectedIndex != -1)
                    CollectionQuestSettings.RemoveAt(listViewQuest.SelectedIndex);
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
                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = Host.AssemblyDirectory + "\\Configs",
                    Filter = @"wav files (*.wav)|*.wav|All files|*.*",
                    RestoreDirectory = true
                };
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                if (System.IO.Path.GetExtension(openFileDialog.FileName) != ".wav")
                    return;
                textBox.Text = openFileDialog.FileName;
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
                comboBoxProp.Items.Clear();
                if (Host.Me == null)
                    return;
                foreach (var prop in Host.GetEntities<GameObject>())
                {
                    if (prop.GameObjectType != EGameObjectType.GatheringNode)
                        continue;

                    if (!comboBoxProp.Items.Contains("[" + prop.Id + "]" + prop.Name))
                        comboBoxProp.Items.Add("[" + prop.Id + "]" + prop.Name);
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
                if (comboBoxProp.SelectedIndex == -1)
                    return;


                var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(comboBoxProp.Text));



                if (CollectionProps.Any(collectionprop => tempId == collectionprop.Id))
                    return;


                GameObject GOGAtherNode = null;
                foreach (var i in Host.GetEntities<GameObject>())
                {
                    if (i.GameObjectType != EGameObjectType.GatheringNode)
                        continue;

                    if (i.Id == tempId)
                        GOGAtherNode = i;
                }
                if (GOGAtherNode == null)
                    return;

                CollectionProps.Add(new PropSettings
                {
                    Id = tempId,
                    Name = GOGAtherNode.Name,
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
                if (listViewProp.SelectedIndex != -1)
                    CollectionProps.RemoveAt(listViewProp.SelectedIndex);
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
                    if (prop.GameObjectType != EGameObjectType.GatheringNode)
                        continue;
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
                var tempLoc = new Vector3F();

                tempLoc = Host.Me.Location;



                CollectionMultiZone.Add(new MultiZone
                {
                    Id = CollectionMultiZone.Count,
                    Loc = tempLoc,
                    ChangeByTime = CheckBoxChangeTime.IsChecked.Value,
                    Time = Convert.ToInt32(textBoxMultiZoneTime.Text),
                    Radius = Convert.ToInt32(textBoxMultiZoneRadius.Text),
                    ChangeByDeathPlayer = CheckBoxChangeDeathByPlayer.IsChecked.Value,
                    CountDeathByPlayer = Convert.ToInt32(textBoxCountDeathByPlayer.Text)
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
                if (listViewMultiZone.SelectedIndex != -1)
                    CollectionMultiZone.RemoveAt(listViewMultiZone.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }



        private void buttonMultiFarmEdit_Click(object sender, RoutedEventArgs e)
        {
            if (listViewMultiZone.SelectedIndex == -1)
                return;

            var multiZone = listViewMultiZone.SelectedItem as MultiZone;
            if (multiZone != null)
            {
                for (int i = 0; i < CollectionMultiZone.Count; i++)
                {
                    if (multiZone == CollectionMultiZone[i])
                    {

                        var tempLoc = new Vector3F(Convert.ToSingle(textBoxMultiZoneX.Text), Convert.ToSingle(textBoxMultiZoneY.Text), Convert.ToSingle(textBoxMultiZoneZ.Text));


                        CollectionMultiZone[i] = new MultiZone()
                        {
                            Id = i,
                            Loc = tempLoc,
                            ChangeByTime = CheckBoxChangeTime.IsChecked.Value,
                            Time = Convert.ToInt32(textBoxMultiZoneTime.Text),
                            Radius = Convert.ToInt32(textBoxMultiZoneRadius.Text),
                            ChangeByDeathPlayer = CheckBoxChangeDeathByPlayer.IsChecked.Value,
                            CountDeathByPlayer = Convert.ToInt32(textBoxCountDeathByPlayer.Text)
                        };

                        comboBoxDungeonMob.SelectedIndex = 0;
                        comboBoxDungeonProp.SelectedIndex = 0;

                        comboBoxDungeonItem.SelectedIndex = 0;
                        CheckBoxAttackMobs.IsChecked = false;
                    }
                }
                buttonMultiZoneLocFill.IsEnabled = false;
            }

        }

        private void listViewMultiZone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (listViewMultiZone.SelectedIndex == -1)
                    return;

                var df = listViewMultiZone.SelectedItem as MultiZone;

                if (df != null)
                {
                    CheckBoxChangeTime.IsChecked = df.ChangeByTime;
                    CheckBoxChangeDeathByPlayer.IsChecked = df.ChangeByDeathPlayer;
                    textBoxMultiZoneTime.Text = df.Time.ToString();
                    textBoxCountDeathByPlayer.Text = df.CountDeathByPlayer.ToString();
                    textBoxMultiZoneX.Text = df.Loc.X.ToString();
                    textBoxMultiZoneY.Text = df.Loc.Y.ToString();
                    textBoxMultiZoneZ.Text = df.Loc.Z.ToString();
                    textBoxMultiZoneRadius.Text = df.Radius.ToString();
                    buttonMultiZoneLocFill.IsEnabled = true;
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }

        }
        private void buttonFarmLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBoxFarmLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                textBoxFarmLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                textBoxFarmLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                textBoxFarmLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                textBoxFarmLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
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
                textBoxMultiZoneX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                textBoxMultiZoneY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                textBoxMultiZoneZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }


        private void buttonUpdateEntity_Click(object sender, RoutedEventArgs e)
        {
            UpdateQuest();
        }

        private void ListViewDangeonFunction1_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListViewDebugEntity.SelectedIndex == -1)
                return;

            var df = ListViewDebugEntity.SelectedItem as MyEntity;
            if (df != null)
            {
                Host.AutoQuests.NeedGebugMoveLoc = df.Location;
                Host.AutoQuests.NeedDebugMove = true;
                // Host.MoveTo(df.Location);
            }
        }

        private void buttonDungeonImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textBoxDungeonLocAreaId.Text == "0" && textBoxDungeonLocMapId.Text == "0")
                {
                    System.Windows.MessageBox.Show("Необходимо указать AreaId и MapId", "Ошибка", MessageBoxButton.OK);
                    return;
                }


                var initDir = Host.AssemblyDirectory + "\\Plugins\\Quester\\Configs";
                if (Host.isReleaseVersion)
                    initDir = Host.AssemblyDirectory + "\\Configs";

                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = initDir,// Host.AssemblyDirectory + "\\Configs",
                    Filter = @"xml files (*.xml)|*.xml|All files|*.*",
                    RestoreDirectory = true
                };
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                if (System.IO.Path.GetExtension(openFileDialog.FileName) != ".xml")
                    return;
                //  Host.CfgName = openFileDialog.FileName;
                // Host.FileName = openFileDialog.SafeFileName;
                Host.log(openFileDialog.FileName + "   " + openFileDialog.SafeFileName);
                var lines = File.ReadAllLines(openFileDialog.FileName);

                var delim = new char[] { '"', ':' };
                string[] inpstr;

                for (var i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("Vector3 X"))
                        continue;
                    // Host.log(lines[i]);


                    inpstr = lines[i].Split(delim);
                    // Host.log(inpstr[1] + "   " +inpstr[3] +"   " + inpstr[5]);

                    CollectionDungeonCoord.Add(new DungeonCoordSettings
                    {
                        Id = CollectionDungeonCoord.Count,
                        Action = "Бежать на точку",
                        Loc = new Vector3F(Convert.ToSingle(inpstr[1]), Convert.ToSingle(inpstr[3]), Convert.ToSingle(inpstr[5])),
                        MobId = 0,
                        PropId = 0,
                        Attack = CheckBoxAttackMobs.IsChecked != null && CheckBoxAttackMobs.IsChecked.Value,

                        ItemId = 0,
                        AreaId = Convert.ToUInt32(textBoxDungeonLocAreaId.Text),
                        MapId = Convert.ToInt32(textBoxDungeonLocMapId.Text)
                    });

                }
                // ListViewDangeonFunction.Items.Refresh();

                /*   var reader = new XmlSerializer(typeof(GathererProfile));
                   GathererProfile Import = new GathererProfile();
                   using (var fs = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                   {
                       Import = (GathererProfile)reader.Deserialize(fs);
                   }
                   */
                // Host.log(Import.Vectors3.Count + "   ");
                // CharacterSettings characterSettings;
                // Host.CharacterSettings = (CharacterSettings)ConfigLoader.LoadConfig(Host.CfgName, typeof(CharacterSettings), Host.CharacterSettings);
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
                if (comboBoxDungeonScript.Items != null)
                {
                    if (comboBoxDungeonScript.Items.Count > 0)
                        comboBoxDungeonScript?.Items?.Clear();
                    if (comboBoxDungeonScript_Copy.Items.Count > 0)
                        comboBoxDungeonScript_Copy?.Items?.Clear();

                }

                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\"))
                {
                    var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\");
                    // папка с файлами
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        comboBoxDungeonScript.Items.Add(file.Name);
                        comboBoxDungeonScript_Copy.Items.Add(file.Name);
                    }
                }
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private void myGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {

                Masshtab++;
                // Host.log("Вверх" + Masshtab);
            }

            else
            {

                if (Masshtab > 1)
                    Masshtab--;
                // Host.log("Вниз" + Masshtab);
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
                            IsAuctioner = obj.IsAuctioner,
                            IsBanker = obj.IsBanker,
                            IsBattleMaster = obj.IsBattleMaster,
                            IsCharmed = obj.IsCharmed,
                            IsCritter = obj.IsCritter,
                            IsGossip = obj.IsGossip,
                            IsInnkeeper = obj.IsInnkeeper,
                            IsServiceProvider = obj.IsServiceProvider,
                            IsSpiritGuide = obj.IsSpiritGuide,
                            IsSpiritHealer = obj.IsSpiritHealer,
                            IsSpiritService = obj.IsSpiritService,
                            IsTabardDesigner = obj.IsTabardDesigner,
                            IsTapped = obj.IsTapped,
                            IsTaxi = obj.IsTaxi,
                            IsTotem = obj.IsTotem(),
                            IsTrainer = obj.IsTrainer,
                            IsTurning = obj.IsTurning,


                        });

                    }
                }
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonDelNpc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewNpcForAction.SelectedIndex != -1)
                    CollectionNpcForActions.RemoveAt(ListViewNpcForAction.SelectedIndex);
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
                if (buttonScriptSavePointMove.Content.ToString() == "Запись точек в движении(выкл)")
                {
                    // On = true;
                    On = false;
                    Host.AutoQuests.SavePointMove = true;
                    buttonScriptSavePointMove.Content = "Запись точек в движении(вкл)";
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
                    buttonScriptSavePointMove.Content = "Запись точек в движении(выкл)";
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
                    if (s.GetMaxCastRange() == 0)
                        continue;

                    if (s.DescriptionRu.Contains("ед. урона") || s.DescriptionRu.Contains("физический урон") || s.DescriptionRu.Contains("физического урона"))
                    {

                        /* Host.log(s.Id + " " + "  " + s.Name + " IsPassive =  " + s.IsPassive() + "  ");
                         Host.log("DescriptionRu: " + s.DescriptionRu, LogLvl.Important);
                         Host.log("AuraDescriptionRu: " + s.AuraDescriptionRu + " \n");*/
                        if (CollectionActiveSkill.Any(collectionInvItem => s.Id == collectionInvItem.Id))
                            continue; ;

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
                            MinLevel = Convert.ToInt32(TextBoxMinLevel.Text),
                            MaxLevel = Convert.ToInt32(TextBoxMaxLevel.Text),

                            UseInFight = CheckBoxUseInFight.IsChecked != null && CheckBoxUseInFight.IsChecked.Value,

                            UseInPVP = CheckBoxUseInPVP.IsChecked.Value
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
                if (ListViewAllItems.SelectedIndex == -1)
                    return;




                var path = ListViewAllItems.SelectedItem as BuffTable;

                /*    var tempNotTargetEffect = 0;
                    if (ComboBoxNotTargetEffect.SelectedIndex != -1)
                        tempNotTargetEffect = GetSkillIdFromCombobox(ComboBoxNotTargetEffect.Text);

                    var tempNotMeEffect = 0;
                    if (ComboBoxNotMeEffect.SelectedIndex != -1)
                        tempNotMeEffect = GetSkillIdFromCombobox(ComboBoxNotMeEffect.Text);

                    var tempIsMeEffect = 0;
                    if (ComboBoxIsMeEffect.SelectedIndex != -1)
                        tempIsMeEffect = GetSkillIdFromCombobox(ComboBoxIsMeEffect.Text);

                    var tempIsTargetEffect = 0;
                    if (ComboBoxIsTargetEffect.SelectedIndex != -1)
                        tempIsTargetEffect = GetSkillIdFromCombobox(ComboBoxIsTargetEffect.Text);*/
                // if(ComboBoxCombatEllementLess.SelectedIndex != -1)




                if (path != null)
                    CollectionMyBuffs.Add(new MyBuff
                    {
                        SkillId = path.SkillId,

                        ItemId = path.ItemId,

                        ItemName = path.ItemName,

                    }

                    );

                //сброс настроек по умолчанию
                // ResetSettingSkillDefault();
                GroupBoxSettingsSkill1.Header = "Настройки";
                ButtonAddBuff.IsEnabled = false;
                //   ListViewActiveSkill.Items.Refresh();
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
                if (ListViewAllItems.SelectedIndex == -1)
                    return;
                ListViewActiveSkill.SelectedIndex = -1;

                ButtonAddBuff.IsEnabled = true;

                ButtonDelBuff.IsEnabled = false;
                var skill = ListViewAllItems.SelectedItem as BuffTable;
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
                ListViewAllItems.Dispatcher.Invoke(() =>
                {
                    if (ListViewActiveItems.SelectedIndex != -1)
                    {
                        CollectionMyBuffs.RemoveAt(ListViewActiveItems.SelectedIndex);
                        ButtonDelBuff.IsEnabled = false;


                    }
                });
                //    ListViewActiveSkill.Items.Refresh();
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ListViewActiveItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewActiveItems.SelectedIndex == -1)
                return;


            ListViewAllItems.SelectedIndex = -1;

            ButtonDelBuff.IsEnabled = true;
            ButtonAddBuff.IsEnabled = false;
        }

        private void buttonFarmLocFill1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBoxMountLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                textBoxMountLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                textBoxMountLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                textBoxMountLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                textBoxMountLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
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
                comboBoxDungeonSkill.Items.Clear();
                comboBoxDungeonSkill.Items.Add("Не выбрано");
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
                    if (!comboBoxDungeonSkill.Items.Contains("[" + skill.Id + "]" + skill.Name))
                        comboBoxDungeonSkill.Items.Add("[" + skill.Id + "]" + skill.Name);
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
                if (!comboBoxInvItemsGlobalClass.Items.Contains(value.ToString()))
                    comboBoxInvItemsGlobalClass.Items.Add(value.ToString());
            }
        }

        private void comboBoxInvItemsGlobalQuality_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in Enum.GetValues(typeof(EItemQuality)))
            {
                //  Host.log(value.ToString());
                if (!comboBoxInvItemsGlobalQuality.Items.Contains(value.ToString()))
                    comboBoxInvItemsGlobalQuality.Items.Add(value.ToString());
            }
        }

        private void buttonAddItemGlobal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxInvItemsGlobalQuality.SelectedIndex == -1 || comboBoxInvItemsGlobalClass.SelectedIndex == -1)
                    return;


                //   var tempId = Convert.ToUInt32(GetSkillIdFromCombobox(comboBoxInvItems.Text));


                /*  if (_collectionInvItems == null)
              {
                  _collectionInvItems = new ObservableCollection<ItemSettings>();
                   listViewInvItems.ItemsSource = _collectionInvItems;
              }*/
                /*  if (Host.CharacterSettings.MyItemGlobals.Any(collectionInvItem => tempId == collectionInvItem.Id))
                      return;*/

                /*  Item item = null;
                  foreach (var i in Host.ItemManager.GetItems())
                  {
                      if (i.Id != tempId)
                          continue;
                      item = i;

                  }*/
                /*  if (item == null)
                      return;*/
                CollectionItemGlobals.Add(new ItemGlobal
                {
                    Class = (EItemClass)Enum.Parse(typeof(EItemClass), comboBoxInvItemsGlobalClass.Text),
                    Quality = (EItemQuality)Enum.Parse(typeof(EItemQuality), comboBoxInvItemsGlobalQuality.Text),
                    ItemLevel = Convert.ToUInt32(textBoxItemLevel.Text)
                });
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void buttonInvItemsGlobalDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listViewInvItemsGlobal.SelectedIndex != -1)
                    CollectionItemGlobals.RemoveAt(listViewInvItemsGlobal.SelectedIndex);
            }
            catch (Exception exception)
            {
                Host.log(exception.ToString());
            }
        }

        private void ListViewNpcForAction_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListViewNpcForAction.SelectedIndex == -1)
                return;

            var df = ListViewNpcForAction.SelectedItem as NpcForAction;
            if (df != null)
            {
                Host.AutoQuests.NeedGebugMoveLoc = df.Loc;
                Host.AutoQuests.NeedDebugMove = true;
                // Host.MoveTo(df.Location);
            }
        }

        private void comboBoxQuestSet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (comboBoxQuestSet.Items != null)
                {
                    if (comboBoxQuestSet.Items.Count > 0)
                        comboBoxQuestSet?.Items?.Clear();
                }

                if (Directory.Exists(Host.PathQuestSet))
                {
                    var dir = new DirectoryInfo(Host.PathQuestSet);
                    // папка с файлами
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        comboBoxQuestSet.Items.Add(file.Name);
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
                var saveFileDialog = new SaveFileDialog
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
                if (listViewQuest.SelectedIndex < 1)
                    return;

                //  while (Mouse.LeftButton == MouseButtonState.Pressed)
                //  {
                Host.log("1312");
                var index = listViewQuest.SelectedIndex;
                var buf = CollectionQuestSettings[index];
                CollectionQuestSettings[index] = CollectionQuestSettings[index - 1];
                CollectionQuestSettings[index - 1] = buf;
                listViewQuest.SelectedIndex = index - 1;
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
                if (listViewQuest.SelectedIndex == -1)
                    return;
                // Host.log(ListViewDangeonFunction.SelectedIndex + "  " + CollectionDungeonCoord.Count );
                if (listViewQuest.SelectedIndex > CollectionQuestSettings.Count - 2)
                    return;
                var index = listViewQuest.SelectedIndex;
                var buf = CollectionQuestSettings[index];
                CollectionQuestSettings[index] = CollectionQuestSettings[index + 1];
                CollectionQuestSettings[index + 1] = buf;
                listViewQuest.SelectedIndex = index + 1;
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
                    if (gameObject.GameObjectType != EGameObjectType.GatheringNode)
                        continue;
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
                    MaxPrice = Convert.ToUInt64(textBoxMaxPrice.Text),
                    Disscount = Convert.ToUInt64(textBoxDiscount.Text),
                    Level = Convert.ToInt32(textBoxLevel.Text),
                    MaxCount = Convert.ToUInt32(textBoxMaxCount.Text)
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

        private void ButtonAddAllProps1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonAukLocFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textBoxAukLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                textBoxAukLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                textBoxAukLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                textBoxAukLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                textBoxAukLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }
        private ObservableCollection<QuestTemplate> _collectionQuestTemplates = new ObservableCollection<QuestTemplate>();
        private void DataGridQuest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {



                 if (_collectionQuestTemplates.Count > 0)
                      Host.log(_collectionQuestTemplates[0].LogTitle);

                   if (DataGridQuest.Items.Count > 1)
                       Host.log((DataGridQuest.Items[0] as QuestTemplate).LogTitle);

                //_collectionQuestTemplates.Clear();

                if (DataGridQuest.SelectedItem is Quest item)
                {
                    var t = item.Template;
                    collectionQuestTemplate.Add(
                        new MyQuestTemplate1
                        {
                            Id = t.Id,
                            AllowableRaces = t.AllowableRaces,
                            AreaDescription = t.AreaDescription,
                            QuestID = t.QuestID,
                            LogTitle = t.LogTitle,
                            AreaGroupID = t.AreaGroupID,
                            Expansion = t.Expansion,
                            Flags = t.Flags,
                            FlagsEx = t.FlagsEx,
                            HasData = t.HasData,
                            ItemDropQuantitys = t.ItemDropQuantitys,
                            ItemDrops = t.ItemDrops,
                            LogDescription = t.LogDescription,
                            MinLevel = t.MinLevel,
                            POIContinent = t.POIContinent,
                            POIPriorityWod = t.POIPriorityWod,
                            POIx = t.POIx,
                            POIy = t.POIy,
                            PortraitGiverMount = t.PortraitGiverMount,
                            QuestCompletionLog = t.QuestCompletionLog,
                            QuestDescription = t.QuestDescription,
                            QuestGiverPortrait = t.QuestGiverPortrait,
                            QuestGiverTargetName = t.QuestGiverTargetName,
                            QuestGiverTextWindow = t.QuestGiverTextWindow,
                            QuestInfoID = t.QuestInfoID,
                            QuestLevel = t.QuestLevel,
                            QuestMaxScalingLevel = t.QuestMaxScalingLevel,
                            QuestObjectives = t.QuestObjectives,
                            QuestPackageID = t.QuestPackageID,
                            QuestRewardID = t.QuestRewardID,
                            QuestSortID = t.QuestSortID,
                            QuestTurnInPortrait = t.QuestTurnInPortrait,
                            QuestTurnTargetName = t.QuestTurnTargetName,
                            QuestTurnTextWindow = t.QuestTurnTextWindow,
                            QuestType = t.QuestType,
                            RewardAmounts = t.RewardAmounts,
                            RewardArenaPoints = t.RewardArenaPoints,
                            RewardArtifactCategoryID = t.RewardArtifactCategoryID,
                            RewardArtifactXPDifficulty = t.RewardArtifactXPDifficulty,
                            RewardArtifactXPMultiplier = t.RewardArtifactXPMultiplier,
                            RewardBonusMoney = t.RewardBonusMoney,
                            RewardChoiceItemDiplayIDs = t.RewardChoiceItemDisplayIDs,
                            RewardChoiceItemIDs = t.RewardChoiceItemIDs,
                            RewardChoiceItemQuantits = t.RewardChoiceItemQuantitys,
                            RewardCurrencyCounts = t.RewardCurrencyCounts,
                            RewardCurrencyIDs = t.RewardCurrencyIDs,
                            RewardDisplaySpell = t.RewardDisplaySpell,
                            RewardFactionCapIns = t.RewardFactionCapIns,
                            RewardFactionFlags = t.RewardFactionFlags,
                            RewardFactionIDs = t.RewardFactionIDs,
                            RewardFactionOverrides = t.RewardFactionOverrides,
                            RewardFactionValues = t.RewardFactionValues,
                            RewardHonor = t.RewardHonor,
                            RewardItems = t.RewardItems,
                            RewardKillHonor = t.RewardKillHonor,
                            RewardMoney = t.RewardMoney,
                            RewardMoneyDifficulty = t.RewardMoneyDifficulty,
                            RewardMoneyMultiplier = t.RewardMoneyMultiplier,
                            RewardNextQuest = t.RewardNextQuest,
                            RewardNumSkillUps = t.RewardNumSkillUps,
                            RewardSkillLineID = t.RewardSkillLineID,
                            RewardSpell = t.RewardSpell,
                            RewardTitle = t.RewardTitle,
                            RewardXPDifficulty = t.RewardXPDifficulty,
                            RewardXPMultiplier = t.RewardXPMultiplier,
                            SoundAccept = t.SoundAccept,
                            SoundTurnIn = t.SoundTurnIn,
                            StartItem = t.StartItem,
                            SuggestedGroupNum = t.SuggestedGroupNum,
                            TimeAllowed = t.TimeAllowed

                        });



                    DataGridQuestTemplate.ItemsSource = collectionQuestTemplate;

                    /*  foreach(var i in _collectionCount)
                          Host.log(i.Id + " " + i.LogTitle);*/

                }

                //  DataGridQuestTemplate.ItemsSource = _collectionQuestTemplates;


                Host.log(collectionQuestTemplate.Count() + "  " + DataGridQuestTemplate.ItemsSource + "  " + DataGridQuestTemplate.Items.Count);
                Host.log(collectionQuestTemplate.Count() + "  " + DataGridQuest.ItemsSource + "  " + DataGridQuest.Items.Count);
            }
            catch (Exception exception)
            {
                Host.log(exception + "");
            }
        }

        private void DataGridQuestTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void comboBoxDungeonPlugin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                comboBoxDungeonPlugin.Items.Clear();
                comboBoxDungeonPlugin.Items.Add("Не выбрано");
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
                            comboBoxDungeonPlugin.Items.Add(directoryInfo.Name + "\\" + fileInfo.Name);
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
                    ScriptStartTime = TimeSpan.Parse(textBoxScriptScheduleStartTime.Text),
                    ScriptStopTime = TimeSpan.Parse(textBoxScriptSheduleEndTime.Text),
                    ScriptName = comboBoxDungeonScript_Copy.Text,
                    Reverse = CheckBoxScriptReverseShedule.IsChecked.Value
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
            Dispatcher.Invoke(() =>
            {
                ButtonContinue.IsEnabled = false;
            });
        }

        private void DataGridQuest_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                /*Type myType = e.AddedCells[0].Item.GetType();

                foreach (MemberInfo mi in myType.GetMembers())
                {
                     Host.log(mi.DeclaringType + " " + mi.MemberType + " " + mi.Name);
                }

                var property = myType.GetProperty(e.AddedCells[0].Column.Header.ToString());
                if (property == null)
                    Host.log("nulll");
                else
                {
                    var obj = property.GetValue(e.AddedCells[0].Item);
                    if (obj == null)
                        Host.log("nulll 2");
                    else
                    {
                        Host.log("Тест " + obj.ToString());
                        DataGrid1QuestLocation.ItemsSource = obj as IEnumerable;
                        Host.log("Тест2 " + DataGrid1QuestLocation.ItemsSource);

                        if(obj is QuestTemplate)
                        {
                            Host.log("Тет 33 ");
                            var col = new ObservableCollection<QuestTemplate>();
                            col.Add(obj as QuestTemplate);
                            DataGrid1QuestLocation.ItemsSource = col;
                        }
                    }
                }*/


            }
            catch (Exception err)
            {
                Host.log(err + "");
            }
        }

        private void DataGrid1QuestLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridQuestCounts_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void DataGridQuestCounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void comboBoxBuyAuc_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (EEquipmentSlot value in Enum.GetValues(typeof(EEquipmentSlot)))
            {
                if (value == EEquipmentSlot.Ranged)
                    continue;
                //  Host.log(value.ToString());
                if (!comboBoxBuyAuc.Items.Contains(value.ToString()))
                    comboBoxBuyAuc.Items.Add(value.ToString());
            }
        }

        private void buttonEquipAucDel_Click(object sender, RoutedEventArgs e)
        {
            
                try
                {
                    if (DataGridEquipAuction.SelectedIndex != -1)
                        CollectionEquipAuc.RemoveAt(DataGridEquipAuction.SelectedIndex);
                }
                catch (Exception exception)
                {
                    Host.log(exception.ToString());
                }
            
        }

        private void buttonAddEquipAuc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxBuyAuc.SelectedIndex == -1)
                    return;


                CollectionEquipAuc.Add(new EquipAuc()
                {
                    Slot = (EEquipmentSlot)Enum.Parse(typeof(EEquipmentSlot), comboBoxBuyAuc.Text),
                   
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
                textBoxSendMailLocX.Text = Convert.ToInt32(Host.Me.Location.X).ToString();
                textBoxSendMailLocY.Text = Convert.ToInt32(Host.Me.Location.Y).ToString();
                textBoxSendMailLocZ.Text = Convert.ToInt32(Host.Me.Location.Z).ToString();
                textBoxSendMailLocMapId.Text = Convert.ToInt32(Host.MapID).ToString();
                textBoxSendMailLocAreaId.Text = Convert.ToInt32(Host.Area.Id).ToString();
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }
    }

    public class FlashWindowHelper
    {
        private IntPtr mainWindowHWnd;
        private Application theApp;

        public FlashWindowHelper(Application app)
        {
            this.theApp = app;
        }

        public void FlashApplicationWindow()
        {
            InitializeHandle();
            Flash(this.mainWindowHWnd, 5);
        }

        public void StopFlashing()
        {
            InitializeHandle();

            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(this.mainWindowHWnd, FLASHW_STOP, uint.MaxValue, 0);
                FlashWindowEx(ref fi);
            }
        }

        private void InitializeHandle()
        {
            if (this.mainWindowHWnd == IntPtr.Zero)
            {
                // Delayed creation of Main Window IntPtr as Application.Current passed in to ctor does not have the MainWindow set at that time
                var mainWindow = this.theApp.MainWindow;
                this.mainWindowHWnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;

        /// <summary>
        /// Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        /// Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="hwnd">The handle of the Window to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, count, 0);

                return FlashWindowEx(ref fi);
            }

            return false;
        }

        /// <summary>
        /// A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater
        {
            get { return Environment.OSVersion.Version.Major >= 5; }
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
        public SkillTable(string name, uint id, bool isPassive)
        {
            Name = name;
            Id = id;
            IsPassive = isPassive;
        }

        public string Name { get; set; }
        public uint Id { get; set; }
        public bool IsPassive { get; set; }

        //  public int Level { get; set; }
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
