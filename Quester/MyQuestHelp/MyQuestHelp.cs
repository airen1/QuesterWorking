using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Out.Utility;
using WoWBot.Core;

// ReSharper disable once CheckNamespace
namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public List<MyQuest> ListQuestsDurotarClassic = new List<MyQuest>
        {
            new MyQuest(4641, QuestAction.Apply),
            new MyQuest(4641, QuestAction.Complete),
            new MyQuest(788, QuestAction.Apply),
            new MyQuest(788, QuestAction.Run),
            new MyQuest(788, QuestAction.Complete),
            new MyQuest(789, QuestAction.Apply),
            new MyQuest(3086, QuestAction.Apply, ERace.None, EClass.Mage),
            new MyQuest(3086, QuestAction.Complete, ERace.None, EClass.Mage),
            new MyQuest(3082, QuestAction.Apply, ERace.Troll, EClass.Hunter),
            new MyQuest(3082, QuestAction.Complete, ERace.Troll, EClass.Hunter),
            new MyQuest(3065 , QuestAction.Apply, ERace.Troll, EClass.Warrior),
            new MyQuest(3065 , QuestAction.Complete, ERace.Troll, EClass.Warrior),
            new MyQuest(2383 , QuestAction.Apply, ERace.Orc, EClass.Warrior),
            new MyQuest(2383 , QuestAction.Complete, ERace.Orc, EClass.Warrior),
            new MyQuest(3090 , QuestAction.Apply, ERace.Orc, EClass.Warlock),
            new MyQuest(3090 , QuestAction.Complete, ERace.Orc, EClass.Warlock),

            new MyQuest(790, QuestAction.Apply),
            new MyQuest(790, QuestAction.Run),
            new MyQuest(789, QuestAction.Run),
            new MyQuest(790, QuestAction.Complete),
            new MyQuest(804, QuestAction.Apply),
            new MyQuest(804, QuestAction.Complete),
            new MyQuest(789, QuestAction.Complete),
            new MyQuest(792, QuestAction.Apply),
            new MyQuest(4402, QuestAction.Apply),
            new MyQuest(5441, QuestAction.Apply),
            new MyQuest(4402, QuestAction.Run),
            new MyQuest(5441, QuestAction.Run),
            new MyQuest(792, QuestAction.Run),
            new MyQuest(4402, QuestAction.Complete),
            new MyQuest(792, QuestAction.Complete),
            new MyQuest(794, QuestAction.Apply),
            new MyQuest(5441, QuestAction.Complete),
            new MyQuest(6394, QuestAction.Apply),
            new MyQuest(6394, QuestAction.Run),
            new MyQuest(794, QuestAction.Run),
            new MyQuest(794, QuestAction.Complete),
            new MyQuest(805 , QuestAction.Apply),
            new MyQuest(6394, QuestAction.Complete),
            new MyQuest(2161, QuestAction.Apply),
            new MyQuest(786, QuestAction.Apply),
            new MyQuest(805, QuestAction.Complete),

            new MyQuest(808, QuestAction.Apply),
            new MyQuest(823, QuestAction.Apply),
            new MyQuest(826, QuestAction.Apply),
            new MyQuest(818, QuestAction.Apply),
            new MyQuest(817, QuestAction.Apply),

            new MyQuest(823, QuestAction.Complete),
            new MyQuest(806, QuestAction.Apply),

            new MyQuest(2161, QuestAction.Complete),
            new MyQuest(784, QuestAction.Apply),
            new MyQuest(837, QuestAction.Apply),
            new MyQuest(815, QuestAction.Apply),
            new MyQuest(791, QuestAction.Apply),
            new MyQuest(791, QuestAction.Run),
            new MyQuest(784, QuestAction.Run),
           // new MyQuest(830, QuestAction.Apply),
            
            new MyQuest(784, QuestAction.Complete),

            new MyQuest(825, QuestAction.Apply),
            new MyQuest(837, QuestAction.Apply),
            new MyQuest(791, QuestAction.Complete),

            new MyQuest(825, QuestAction.Run),
            new MyQuest(818, QuestAction.Run),
            new MyQuest(837, QuestAction.Run, 0),
            new MyQuest(837, QuestAction.Run, 1),
            new MyQuest(825, QuestAction.Complete),
            new MyQuest(786, QuestAction.Run),
            new MyQuest(786, QuestAction.Complete),
            new MyQuest(818, QuestAction.Complete),
            new MyQuest(815, QuestAction.Run),
            new MyQuest(817, QuestAction.Run),
            new MyQuest(826, QuestAction.Run),
            new MyQuest(808, QuestAction.Run),

            new MyQuest(808, QuestAction.Complete),
            new MyQuest(826, QuestAction.Complete),
            new MyQuest(817, QuestAction.Complete),
            new MyQuest(815, QuestAction.Complete),

            new MyQuest(837, QuestAction.Run, 2),
            new MyQuest(837, QuestAction.Run, 3),
            new MyQuest(816, QuestAction.Apply),
            new MyQuest(834, QuestAction.Apply),
            new MyQuest(834, QuestAction.Run),
            new MyQuest(834, QuestAction.Complete),
            new MyQuest(835, QuestAction.Apply),
            new MyQuest(837, QuestAction.Complete),
            new MyQuest(6062, QuestAction.Apply, ERace.None, EClass.Hunter),
            new MyQuest(6062, QuestAction.Run, ERace.None,EClass.Hunter),
            new MyQuest(6062, QuestAction.Complete, ERace.None,EClass.Hunter),
            new MyQuest(6083, QuestAction.Apply, ERace.None, EClass.Hunter),
            new MyQuest(6083, QuestAction.Run, ERace.None,EClass.Hunter),
            new MyQuest(6083, QuestAction.Complete, ERace.None,EClass.Hunter),
            new MyQuest(6082, QuestAction.Apply, ERace.None, EClass.Hunter),
            new MyQuest(6082, QuestAction.Run, ERace.None,EClass.Hunter),
            new MyQuest(6082, QuestAction.Complete, ERace.None,EClass.Hunter),
            new MyQuest(6081, QuestAction.Apply, ERace.None,EClass.Hunter),
          //  new MyQuest(812, QuestAction.Apply),
            new MyQuest(6081, QuestAction.Complete, ERace.None,EClass.Hunter),
           // new MyQuest(812, QuestAction.Run),
          //  new MyQuest(812, QuestAction.Complete),
        };

        public List<MyQuest> ListQuestsEchoIsles = new List<MyQuest>
        {
            new MyQuest(24764, QuestAction.Apply, ERace.Troll, EClass.Druid),
            new MyQuest(24764, QuestAction.Complete, ERace.Troll, EClass.Druid),
            new MyQuest(24765, QuestAction.Apply, ERace.Troll, EClass.Druid),
            new MyQuest(24765, QuestAction.Run, ERace.Troll, EClass.Druid),
            new MyQuest(24765, QuestAction.Complete, ERace.Troll, EClass.Druid),
            new MyQuest(24767, QuestAction.Apply, ERace.Troll, EClass.Druid),
            new MyQuest(24767, QuestAction.Run, ERace.Troll, EClass.Druid),//Type:Item Amount: 6 ObjectID: 50222 Flags: 0 Flags2: 1 Description:
            new MyQuest(24767, QuestAction.Complete, ERace.Troll, EClass.Druid),
            new MyQuest(24768, QuestAction.Apply, ERace.Troll, EClass.Druid),
            new MyQuest(24768, QuestAction.Run, ERace.Troll, EClass.Druid),
            new MyQuest(24768, QuestAction.Complete, ERace.Troll, EClass.Druid),
            new MyQuest(24769, QuestAction.Apply, ERace.Troll, EClass.Druid),
            new MyQuest(24769, QuestAction.Run, ERace.Troll, EClass.Druid),
            new MyQuest(24769, QuestAction.Complete, ERace.Troll, EClass.Druid),

            new MyQuest(24758, QuestAction.Apply, ERace.Troll, EClass.Shaman),
            new MyQuest(24758, QuestAction.Complete, ERace.Troll, EClass.Shaman),
            new MyQuest(24759, QuestAction.Apply, ERace.Troll, EClass.Shaman),
            new MyQuest(24759, QuestAction.Run, ERace.Troll, EClass.Shaman),
            new MyQuest(24759, QuestAction.Complete, ERace.Troll, EClass.Shaman),
            new MyQuest(24761, QuestAction.Apply, ERace.Troll, EClass.Shaman),
            new MyQuest(24761, QuestAction.Run, ERace.Troll, EClass.Shaman),
            new MyQuest(24761, QuestAction.Complete, ERace.Troll, EClass.Shaman),
            new MyQuest(24762, QuestAction.Apply, ERace.Troll, EClass.Shaman),
            new MyQuest(24762, QuestAction.Run, ERace.Troll, EClass.Shaman),
            new MyQuest(24762, QuestAction.Complete, ERace.Troll, EClass.Shaman),
            new MyQuest(24763, QuestAction.Apply, ERace.Troll, EClass.Shaman),
            new MyQuest(24763, QuestAction.Run, ERace.Troll, EClass.Shaman),
            new MyQuest(24763, QuestAction.Complete, ERace.Troll, EClass.Shaman),

            new MyQuest(25064, QuestAction.Apply),
            new MyQuest(25037, QuestAction.Apply),
            new MyQuest(25064, QuestAction.Complete),
            new MyQuest(25037, QuestAction.Run),//Type:Item Amount: 5 ObjectID: 52080 Flags: 0 Flags2: 1 Description: 
            new MyQuest(25037, QuestAction.Complete),
            new MyQuest(24622, QuestAction.Apply),
            new MyQuest(24622, QuestAction.Complete),
            new MyQuest(24623, QuestAction.Apply),
            new MyQuest(24624, QuestAction.Apply),
            new MyQuest(24625, QuestAction.Apply),
            new MyQuest(24623, QuestAction.Run),//Type:Monster Amount: 12 ObjectID: 39157 Flags: 0 Flags2: 0 Description: Bloodtalon Hatchlings rescued
            new MyQuest(24625, QuestAction.Run), //Type:Item Amount: 1 ObjectID: 50018 Flags: 1 Flags2: 1 Description: 
            new MyQuest(24624, QuestAction.Run),//Type:Monster Amount: 8 ObjectID: 37961 Flags: 0 Flags2: 0 Description: 
            new MyQuest(24623, QuestAction.Complete),
            new MyQuest(24624, QuestAction.Complete),
            new MyQuest(24625, QuestAction.Complete),
        };


        public class MyQuestInfoRun
        {
            public MyQuestInfoRun(RoundZone zone)
            {
                Zone = zone;
            }
            public MyQuestInfoRun(RoundZone zone, List<uint> farmIds)
            {
                Zone = zone;
                FarmIds = farmIds;
            }
            public RoundZone Zone;
            public List<uint> FarmIds = new List<uint>();
        }

        public class MyQuestInfo
        {
            public MyQuestInfo(uint id, Vector3F loc)
            {
                Id = id;
                Loc = loc;
            }
            public MyQuestInfo(uint id)
            {
                Id = id;
                Loc = new Vector3F(Vector3F.Zero);
            }
            public uint Id;
            public Vector3F Loc;
        }


        public class MyQuest
        {
            public MyQuest(uint id, QuestAction questAction, ERace race, EClass _class, int index)
            {
                Id = id;
                Race.Add(race);
                Class.Add(_class);
                QuestAction = questAction;
                Team = ETeam.Other;
                Index = index;
            }

            public MyQuest(uint id, QuestAction questAction, int index)
            {
                Id = id;
                Race.Add(ERace.None);
                Class.Add(EClass.None);
                QuestAction = questAction;
                Team = ETeam.Other;
                Index = index;
            }
            public MyQuest(uint id, QuestAction questAction, ERace race, EClass _class)
            {
                Id = id;
                Race.Add(race);
                Class.Add(_class);
                QuestAction = questAction;
                Team = ETeam.Other;
                Index = -2;
            }



            public MyQuest(uint id, QuestAction questAction, List<ERace> race, List<EClass> _class)
            {
                Id = id;
                Race = race;
                Class = _class;
                QuestAction = questAction;
                Team = ETeam.Other;
                Index = -2;
            }

            public MyQuest(uint id, QuestAction questAction)
            {
                Id = id;
                Race.Add(ERace.None);
                Class.Add(EClass.None);
                QuestAction = questAction;
                Team = ETeam.Other;
                Index = -2;
            }

            public MyQuest(QuestAction questAction, int level, Vector3F loc)
            {
                Race.Add(ERace.None);
                Class.Add(EClass.None);
                QuestAction = questAction;
                Level = level;
                Loc = loc;
            }

            public MyQuest(QuestAction questAction, int level, Zone grindZone)
            {
                QuestAction = questAction;
                Level = level;
                GrindZone = grindZone;
                Race.Add(ERace.None);
                Class.Add(EClass.None);
                Team = ETeam.Other;
            }

            public MyQuest(uint id, QuestAction questAction, string flyPath, Vector3F loc)
            {
                Id = id;
                Race.Add(ERace.None);
                Class.Add(EClass.None);
                QuestAction = questAction;
                FlyPath = flyPath;
                Loc = loc;
            }

            public uint Id;
            public List<ERace> Race = new List<ERace>();
            public List<EClass> Class = new List<EClass>();
            public ETeam Team;
            public QuestAction QuestAction;
            public int Index;
            public Zone GrindZone;
            public int Level;
            public string FlyPath;
            public Vector3F Loc;

        }

        public Dictionary<uint, Tuple<EQuestReq, uint>> QuestReq = new Dictionary<uint, Tuple<EQuestReq, uint>>
        {
            // {830, new Tuple<EQuestReq, uint>(EQuestReq.NeedItem, 4882) }
            {812, new Tuple<EQuestReq, uint>(EQuestReq.QuestCompleted, 813) }
        };


        //Награды
        public Dictionary<uint, uint> QuestRevardDruid = new Dictionary<uint, uint>
        {
            {835, 4938},
            {872, 59543},
            {850, 59552},
            {13998, 59545},
            {28726, 5393},
            {2159, 2070},
            {28727, 5405},
            {13520, 52609},
            {13554, 52631},
            {13529, 52595},
            {13563, 52599},
            {25192, 53382},
            {895, 49446},
            {865, 59581},
            {851, 59567},
            {855, 59583},
            {852, 59585},
            {899, 59540},
            {13621, 56644},
            {13523, 55127},
            {13558, 52596},
            {13584, 52634},
            {13578, 55133},
            {13580, 52598},
            {13587, 52614},
            {53372, 163527},
            {49676, 159983},
            {47322, 155412},
            {48550, 155403},
            {48554, 155392},
            {48993, 159979},
            {49005, 161270},
            {48996, 155405},
            {50561, 159991},
            {49334, 155457},
            {50550, 155440},
            {50702, 155399},
            {47130, 155319},
            {47247, 155339},
            {47249, 155365},
            {48800, 159075},
            {49081, 155326},
            {49492, 159763},
            {50297, 159147},
            {53370, 163536},
            {51364, 161189},
            {49667, 160020},
            {50980, 160008},
            {46929, 155305},
            {56063, 170275},
            {55053, 170518},
            {55481, 169482},
            {52428, 159906},
            {49980, 159087},
            {47528, 159111},

        };

        public Dictionary<uint, uint> QuestRevardHunt = new Dictionary<uint, uint>
        {
            {49676, 159980},
            {47322, 155411},
            {48550, 155402},
            {48554, 155395},
            {48993, 159979},
            {49005, 161272},
            {48996, 155405},
            {50561, 159990},
            {49334, 155457},
            {50550, 160034},
            {50702, 155399},
            {47130, 155322},
            {47247, 155338},
            {47249, 155348},
            {48800, 159075},
            {49081, 155326},
            {49492, 159763},
            {50297, 159148},
            {55481, 169482},
            {52428, 159907},
            {53372, 163524},
            {56063, 163867},
        };

        public Dictionary<uint, uint> QuestRevardMonk = new Dictionary<uint, uint>
        {
            {49676, 159983},
            {47322, 155412},
            {48550, 155403},
            {48554, 155392},
            {48993, 159979},
            {49005, 161270},
            {48996, 155405},
            {50561, 159991},
            {49334, 155457},
            {50550, 160032},
            {50702, 155399},
            {47130, 155319},
            {47247, 155339},
            {47249, 155365},
            {48800, 159075},
            {49081, 155314},
            {49492, 159763},
            {50297, 159147},
            {55481, 169482},
            {52428, 159906},
            {53372, 163528},
            {49667, 159995},
            {56063, 163871},
            {55053, 170518},
            {49980, 159087},
            {47528, 159111},
        };

        public Dictionary<uint, uint> QuestRevardShaman = new Dictionary<uint, uint>
        {
            {53372, 163529},
            {49676, 159980},
            {47322, 155411},
            {48550, 155402},
            {48554, 159975},
            {48993, 159979},
            {49005, 161272},
            {48996, 155408},
            {50561, 159990},
            {49334, 155457},
            {50550, 155440},
            {50702, 155398},
            {47130, 155322},
            {47247, 155338},
            {47249, 155351},
            {48800, 159075},
            {49081, 155313},
            {49492, 159763},
            {50297, 159148},
            {50980, 160007},
            {48400, 155310},
            {55481, 169482},
            {52428, 159907},
            {49667, 159994},
            {56063, 170275},
            {55053, 170518},
            {46929, 155305},
            {51364, 161188},
         };

        public Dictionary<uint, uint> QuestRevardMonk2 = new Dictionary<uint, uint>
        {
            {47587, 159121},
            {51668, 155441},
            {51364, 161192},
            {49676, 159983},
            {47322, 155412},
            {48550, 155403},
            {48554, 155392},
            {48993, 159979},
            {49005, 161270},
            {48996, 155405},
            {50561, 159991},
            {49334, 155457},
            {50550, 155417},
            {50702, 155399},
            {47130, 155319},
            {47247, 155339},
            {47249, 155344},
            {48800, 159075},
            {49081, 155314},
            {49492, 159763},
            {50297, 159147},
            {55481, 169482},
            {52428, 159906},
            {53372, 163528},
            {49667, 159995},
            {56063, 163871},
            {55053, 170518},
            {49980, 159084},
            {47528, 159105},
        };

        public Dictionary<uint, uint> QuestRevardMage = new Dictionary<uint, uint>
        {
            {52428, 159908},
            {53372, 163530},
            {48996, 155407},
            {50702, 155397},
        };

        public Dictionary<uint, uint> QuestDeathKnight = new Dictionary<uint, uint>
        {
           {48550, 155400},
           {48993, 159979},
           {49005, 161274},
           {50561, 159984},
           {49667, 159992},
           {50980, 160005},
           {47247, 159050},
           {48800, 158969},
           {46929, 155268},

        };


        //Фарм
        public void MonsterHuntClassic(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Host host, double z)
        {
            var badRadius = 0;
            foreach (var farmMobId in farmMobIds)
            {
                host.log("Мобы для квеста  " + farmMobId);
                var factionIds = new List<uint>();
                switch (quest.Id)
                {
                    case 826:
                        break;
                    case 837:
                        break;
                    case 784:
                        break;
                    case 789:
                        break;
                    default:
                        {
                            /*  foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                              {
                                  if (farmMobId == myNpcLoc.Id)
                                  {
                                      factionIds.Add(myNpcLoc.FactionId);
                                      break;
                                  }
                              }*/
                        }
                        break;

                }
                host.FarmModule.FactionIds = factionIds;
            }

            host.FarmModule.SetFarmMobs(zone, farmMobIds);
            while (!host.AutoQuests.IsQuestCompliteClassic(quest.Id, objectiveindex) && host.FarmModule.ReadyToActions && host.FarmModule.FarmState == FarmState.FarmMobs)
            {
                Thread.Sleep(100);
                if (host.MyIsNeedSell())
                {
                    break;
                }

                if (host.MyIsNeedRepair())
                {
                    break;
                }

                if (host.MyIsNeedBuy())
                {
                    break;
                }

                if (host.FarmModule.BestMob == null && !host.MyIsNeedRegen())
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                if (badRadius > 50)
                {
                    if (zone.ZoneType == EZoneType.Circle)
                    {
                        if ((zone as RoundZone).Radius < 50)
                        {
                            continue;
                        }
                    }
                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                        {
                            continue;
                        }

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                            {
                                continue;
                            }

                            if (!zone.PointInZone(vector3F.X, vector3F.Y))
                            {
                                continue;
                            }

                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                {
                                    bad = true;
                                }
                            }
                            if (bad)
                            {
                                continue;
                            }

                            if (!host.CommonModule.CheckPathForLoc(host.Me.Location, vector3F))
                            {
                                continue;
                            }

                            loc = vector3F;
                        }
                    }

                    if (loc != Vector3F.Zero)
                    {


                        host.log("Не могу найти Monster 1, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = zone.GetRandomPoint();
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (z == 0)
                    {
                        z = host.Me.Location.Z;
                    }

                    var path = host.GetSmoothPath(host.Me.Location, new Vector3F(findPoint.X, findPoint.Y, z));
                    if (path.Path.Count > 100)
                    {
                        continue;
                    }

                    if (!host.CommonModule.CheckPathForLoc(host.Me.Location, new Vector3F(findPoint.X, findPoint.Y, z)))
                    {
                        continue;
                    }

                    var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, z);
                    host.log("Не могу найти Monster 2, подбегаю в центр зоны " + host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                    host.CommonModule.MoveTo(vectorPoint, 20);

                }
            }
            host.FarmModule.StopFarm();
            Thread.Sleep(1000);

            if (host.CharacterSettings.Mode != Mode.QuestingClassic)
            {
                return;
            }

            if (host.FarmModule.FarmState == FarmState.Disabled)
            {
                return;
            }

            if (quest.Id == 835 || quest.Id == 1525 || quest.Id == 257 || quest.Id ==258 || quest.Id ==2499)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            if (zone.ZoneType != EZoneType.Circle)
            {
                return;
            }

            host.MainForm.SetQuestIdText("Grind 20 min");
            zone = new RoundZone((zone as RoundZone).X, (zone as RoundZone).Y, 200);

            host.FarmModule.SetFarmMobs(zone, new List<uint>());
            while (sw.ElapsedMilliseconds < 1200000 && host.FarmModule.ReadyToActions && host.FarmModule.FarmState == FarmState.FarmMobs)
            {
                Thread.Sleep(100);

                if (host.MyIsNeedSell())
                {
                    break;
                }

                if (host.MyIsNeedRepair())
                {
                    break;
                }

                if (host.MyIsNeedBuy())
                {
                    break;
                }

                if (host.FarmModule.BestMob == null && !host.MyIsNeedRegen())
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                host.MainForm.SetQuestStateText(sw.Elapsed.Minutes + ":" + sw.Elapsed.Seconds);
                if (badRadius > 50)
                {
                    if (zone.ZoneType == EZoneType.Circle)
                    {
                        if ((zone as RoundZone).Radius < 50)
                        {
                            continue;
                        }
                    }
                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                        {
                            continue;
                        }

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                            {
                                continue;
                            }

                            if (!zone.PointInZone(vector3F.X, vector3F.Y))
                            {
                                continue;
                            }

                            if (!host.CommonModule.CheckPathForLoc(host.Me.Location, vector3F))
                            {
                                continue;
                            }

                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                {
                                    bad = true;
                                }
                            }
                            if (bad)
                            {
                                continue;
                            }

                            loc = vector3F;
                        }
                    }

                    if (loc != Vector3F.Zero)
                    {
                        host.log("Не могу найти Monster 1, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = zone.GetRandomPoint();
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (z == 0)
                    {
                        z = host.Me.Location.Z;
                    }

                    var path = host.GetSmoothPath(host.Me.Location, new Vector3F(findPoint.X, findPoint.Y, z));
                    if (path.Path.Count > 100)
                    {
                        continue;
                    }

                    if (!host.CommonModule.CheckPathForLoc(host.Me.Location, new Vector3F(findPoint.X, findPoint.Y, z)))
                    {
                        continue;
                    }

                    var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, z);
                    host.log("Не могу найти Monster 2, подбегаю в центр зоны " + host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                    host.CommonModule.MoveTo(vectorPoint, 20);

                }
            }
            host.FarmModule.StopFarm();
            Thread.Sleep(1000);

        }

        public void ItemGatherFromGameObjectClassic(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Host host, double z)
        {
            var badRadius = 0;
            host.FarmModule.SetFarmProps(zone, farmMobIds);
            while (host.MainForm.On && !host.AutoQuests.IsQuestCompliteClassic(quest.Id, objectiveindex) && host.FarmModule.ReadyToActions && host.FarmModule.FarmState == FarmState.FarmProps)
            {
                if (host.MyIsNeedRepair())
                {
                    break;
                }

                if (host.MyIsNeedSell())
                {
                    break;
                }

                if (host.MyIsNeedBuy())
                {
                    break;
                }

                Thread.Sleep(100);

                if (host.FarmModule.BestProp == null && !host.MyIsNeedRegen())
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                if (host.FarmModule.BestMob != null)
                {
                    badRadius = 0;
                }

                if (badRadius > 50)
                {
                    if (zone.ZoneType == EZoneType.Circle)
                    {
                        if (((RoundZone)zone).Radius < 50)
                        {
                            continue;
                        }
                    }

                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyGameObjectLocss.GameObjectLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                        {
                            continue;
                        }

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                            {
                                continue;
                            }

                            if (!zone.PointInZone(vector3F.X, vector3F.Y))
                            {
                                continue;
                            }

                            if (!host.CommonModule.CheckPathForLoc(host.Me.Location, vector3F))
                            {
                                continue;
                            }

                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                {
                                    bad = true;
                                }
                            }
                            if (bad)
                            {
                                continue;
                            }

                            loc = vector3F;
                        }
                    }

                    if (loc != Vector3F.Zero)
                    {
                        host.log("Не могу найти GameObject 1, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = zone.GetRandomPoint();
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (z == 0)
                    {
                        z = host.Me.Location.Z;
                    }

                    var path = host.GetSmoothPath(host.Me.Location, new Vector3F(findPoint.X, findPoint.Y, z));
                    if (path.Path.Count > 100)
                    {
                        continue;
                    }

                    var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, z);
                    if (!host.CommonModule.CheckPathForLoc(host.Me.Location, vectorPoint))
                    {
                        continue;
                    }
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (host.GetNavMeshHeight(vectorPoint) != 0)
                    {
                        host.log("Не могу найти GameObject, подбегаю в центр зоны " + host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                        host.CommonModule.MoveTo(vectorPoint, 20);
                    }
                }
            }
            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

        public List<Vector3F> BadVector3Fs = new List<Vector3F>();
        public void ItemGatherFromGameObject(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            var badRadius = 0;
            host.FarmModule.SetFarmProps(zone, farmMobIds);
            while (host.MainForm.On

                   && !host.AutoQuests.IsQuestComplite(quest.Id, objectiveindex)
                   && host.FarmModule.ReadyToActions
                   && host.FarmModule.FarmState == FarmState.FarmProps)
            {
                if (host.MyIsNeedRepair())
                {
                    break;
                }

                if (host.MyIsNeedSell())
                {
                    break;
                }

                Thread.Sleep(100);
                if (host.FarmModule.BestProp == null && host.Me.HpPercents > 50)
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                if (host.FarmModule.BestMob != null)
                {
                    badRadius = 0;
                }

                if (badRadius > 50)
                {
                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                    {
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    }

                    host.log("Не могу найти GameObject, подбегаю в центр зоны " + host.Me.Distance(findPoint) + "    " + questPoiPoints.Count);
                    host.CommonModule.MoveTo(findPoint, 20);
                }
            }
            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

        public void ItemGatherFromMonster(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            var badRadius = 0;
            host.FarmModule.SetFarmMobs(zone, farmMobIds);

            while (!host.AutoQuests.IsQuestComplite(quest.Id, objectiveindex) && host.FarmModule.ReadyToActions && host.FarmModule.FarmState == FarmState.FarmMobs)
            {
                Thread.Sleep(100);

                if (host.FarmModule.BestMob == null && host.Me.HpPercents > 80)
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                if (badRadius > 100)
                {
                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                        {
                            continue;
                        }

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                            {
                                continue;
                            }

                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                {
                                    bad = true;
                                }
                            }
                            if (bad)
                            {
                                continue;
                            }

                            loc = vector3F;
                        }


                    }
                    if (loc != Vector3F.Zero)
                    {
                        host.log("Не могу найти GameObject, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                    {
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    }

                    if (host.Me.Distance(findPoint) > 5)
                    {
                        host.log("Не могу найти мобов, подбегаю в центр зоны " + host.Me.Distance(findPoint));
                        host.CommonModule.MoveTo(findPoint, 1);
                    }
                }

            }

            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

        public void MonsterHunt(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            var badRadius = 0;
            host.FarmModule.SetFarmMobs(zone, farmMobIds);


            while (!host.AutoQuests.IsQuestComplite(quest.Id, objectiveindex) && host.FarmModule.ReadyToActions && host.FarmModule.FarmState == FarmState.FarmMobs)
            {

                Thread.Sleep(100);
                if (quest.Id == 49378 && host.MyGetAura(255988) == null)
                {
                    break;
                }

                if (host.FarmModule.BestMob == null && host.Me.HpPercents > 80)
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                if (badRadius > 100)
                {
                    if (quest.Id == 50702)
                    {
                        var npc = host.GetNpcById(134803);
                        if (npc != null)
                        {
                            host.CommonModule.MoveTo(npc, 20);
                        }

                        badRadius = 0;
                    }

                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                        {
                            continue;
                        }

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                            {
                                continue;
                            }

                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                {
                                    bad = true;
                                }
                            }
                            if (bad)
                            {
                                continue;
                            }

                            loc = vector3F;
                        }


                    }
                    if (loc != Vector3F.Zero)
                    {
                        host.log("Не могу найти GameObject, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                    {
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    }

                    if (host.Me.Distance(findPoint) > 5 && findPoint != Vector3F.Zero)
                    {

                        host.log("Не могу найти мобов, подбегаю в центр зоны " + host.Me.Distance(findPoint));
                        host.CommonModule.MoveTo(findPoint, 1);
                        badRadius = 0;
                    }

                }

            }

            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

    }
}