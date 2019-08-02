using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Out.Utility;
using WowAI.Modules;
using WoWBot.Core;


namespace WowAI
{
    internal class MyQuestHelp
    {
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
        public List<Vector3F> BadVector3Fs = new List<Vector3F>();
        public void ItemGatherFromGameObject(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            var badRadius = 0;
            host.FarmModule.SetFarmProps(zone, farmMobIds);
            while (host.MainForm.On
                   && host.ItemManager.GetFreeInventorySlotsCount() >= host.CharacterSettings.InvFreeSlotCount
                   && !host.AutoQuests.IsQuestComplite(quest.Id, objectiveindex)
                   && host.FarmModule.readyToActions
                   && host.FarmModule.farmState == FarmState.FarmProps)
            {
                if (host.MyIsNeedRepair())
                    break;
                Thread.Sleep(100);
                if (host.FarmModule.BestProp == null && host.Me.HpPercents > 50)
                    badRadius++;
                else
                    badRadius = 0;
                if (host.FarmModule.BestMob != null)
                    badRadius = 0;
                if (badRadius > 50)
                {
                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    host.log("Не могу найти GameObject, подбегаю в центр зоны " + host.Me.Distance(findPoint) + "    " + questPoiPoints.Count);
                    host.CommonModule.MoveTo(findPoint, 20, 20);
                }
            }
            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

        public void ItemGatherFromMonster(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            var badRadius = 0;
            host.FarmModule.SetFarmMobs(zone, farmMobIds);

            while (!host.AutoQuests.IsQuestComplite(quest.Id, objectiveindex) && host.FarmModule.readyToActions && host.FarmModule.farmState == FarmState.FarmMobs)
            {
                Thread.Sleep(100);

                if (host.FarmModule.BestMob == null && host.Me.HpPercents > 80)
                    badRadius++;
                else
                    badRadius = 0;
                if (badRadius > 100)
                {
                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                            continue;

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                                continue;
                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                    bad = true;
                            }
                            if (bad)
                                continue;
                            loc = vector3F;
                        }


                    }
                    if (loc != Vector3F.Zero)
                    {
                        host.log("Не могу найти GameObject, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    if (host.Me.Distance(findPoint) > 5)
                    {
                        host.log("Не могу найти мобов, подбегаю в центр зоны " + host.Me.Distance(findPoint));
                        host.CommonModule.MoveTo(findPoint);
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


            while (!host.AutoQuests.IsQuestComplite(quest.Id, objectiveindex) && host.FarmModule.readyToActions && host.FarmModule.farmState == FarmState.FarmMobs)
            {

                Thread.Sleep(100);
                if (host.FarmModule.BestMob == null && host.Me.HpPercents > 80)
                    badRadius++;
                else
                    badRadius = 0;
                if (badRadius > 100)
                {
                    if (quest.Id == 50702)
                    {
                        var npc = host.GetNpcById(134803);
                        if (npc != null)
                            host.CommonModule.MoveTo(npc, 20);
                        badRadius = 0;
                    }

                    var loc = Vector3F.Zero;
                    foreach (var myNpcLoc in host.MyNpcLocss.NpcLocs)
                    {
                        if (!farmMobIds.Contains(myNpcLoc.Id))
                            continue;

                        foreach (var vector3F in myNpcLoc.ListLoc.OrderBy(i => host.Me.Distance2D(i)))
                        {
                            if (host.Me.Distance2D(vector3F) < 50)
                                continue;
                            var bad = false;
                            foreach (var badVector3F in BadVector3Fs)
                            {
                                if (vector3F.Distance(badVector3F) < 50)
                                    bad = true;
                            }
                            if (bad)
                                continue;
                            loc = vector3F;
                        }


                    }
                    if (loc != Vector3F.Zero)
                    {
                        host.log("Не могу найти GameObject, подбегаю к  " + host.Me.Distance(loc) + "    " + loc);
                        if (host.CommonModule.MoveTo(loc, 20, 20))
                        {
                            BadVector3Fs.Add(loc);
                        }
                        continue;
                    }

                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    if (host.Me.Distance(findPoint) > 5 && findPoint != Vector3F.Zero)
                    {

                        host.log("Не могу найти мобов, подбегаю в центр зоны " + host.Me.Distance(findPoint));
                        host.CommonModule.MoveTo(findPoint);
                        badRadius = 0;
                    }

                }
            }

            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

    }
}