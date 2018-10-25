using System.Collections.Generic;
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
            {52428, 159906},//Vest of the Champion[159906]
           // {13576, 52587},
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
            {47247, 155339}
        };


        public void ItemGatherFromGameObject(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            int badRadius = 0;
            host.FarmModule.SetFarmProps(zone, farmMobIds);
            //int badRadius = 0;
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

                /*   if (Host.FarmModule.bestProp == null && Host.Me.HpPercents > 80)
                       badRadius++;
                   else
                       badRadius = 0;*/
                if (badRadius > 50)
                {
                    var findPoint = farmLoc;
                    if (questPoiPoints.Count > 0)
                        findPoint = questPoiPoints[host.RandGenerator.Next(0, questPoiPoints.Count)];
                    host.log("Не могу найти GameObject, подбегаю в центр зоны " + host.Me.Distance(findPoint) + "    " + questPoiPoints.Count);
                    host.CommonModule.MoveTo(findPoint);
                }


            }

            host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

        public void ItemGatherFromMonster(Quest quest, Zone zone, List<uint> farmMobIds, int objectiveindex, Vector3F farmLoc, List<Vector3F> questPoiPoints, Host host)
        {
            int badRadius = 0;
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
            int badRadius = 0;
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

    }
}