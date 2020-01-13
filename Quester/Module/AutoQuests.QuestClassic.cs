using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Out.Internal.Core;
using Out.Utility;
using WoWBot.Core;
using WoWBot.WoWNetwork;
using static WowAI.MyQuestHelpClass;

namespace WowAI.Module
{
    internal partial class AutoQuests
    {
        private int _skipQuest;
        public Entity Convoy;

        private void ApplyQuestClassic(MyQuest myQuest)
        {
            try
            {
                if (Host.GameDB.QuestTemplates.ContainsKey(myQuest.Id))
                {
                    Host.log("Беру квест " + Host.GameDB.QuestTemplates[myQuest.Id]?.LogTitle + "[" + myQuest.Id + "]" + myQuest.QuestAction, LogLvl.Ok);
                }
                else
                {
                    Host.log("Беру квест " + "[" + myQuest.Id + "]" + myQuest.QuestAction, LogLvl.Ok);
                }

                _state = "Apply";
                BestQuestId = myQuest.Id;



                switch (myQuest.Id)
                {
                    case 307:
                        Host.FarmModule.FarmState = FarmState.Disabled;
                        break;
                    case 2519:
                        {

                            if (Host.Area.Id != 1657)
                            {
                                if (!Host.CommonModule.MoveTo(8783.94, 965.74, 30.21))
                                    return;
                                if (!Host.CommonModule.MoveTo(8810.84, 971.63, 32.22))
                                    return;
                                Thread.Sleep(2000);
                                return;
                            }
                        }
                        break;

                    case 963:
                        {
                            Host.FarmModule.FarmState = FarmState.Disabled;
                            if (!CheckPath2())
                            {
                                return;
                            }

                            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        }
                        break;

                    case 433:
                        {
                            if (!CheckPath())
                            {
                                return;
                            }
                        }
                        break;

                    case 830:
                        {
                            var item = Host.MyGetItem(4882);
                            if (item != null)
                            {
                                Host.CommonModule.MoveTo(new Vector3F(-224.93, -5118.06, 49.32), 1);
                                foreach (var gameObject in Host.GetEntities<GameObject>())
                                {
                                    if (gameObject.Id != 3239)
                                    {
                                        continue;
                                    }

                                    if (!Host.CommonModule.MoveTo(gameObject, 1))
                                    {
                                        return;
                                    }

                                    Thread.Sleep(1000);
                                    Host.MyUseItemAndWait(item, gameObject);
                                    Thread.Sleep(1000);
                                    if (Host.CanPickupLoot())
                                    {
                                        if (!Host.PickupLoot())
                                        {
                                            Host.log("Не смог поднять дроп " + "   " + Host.GetLastError(), LogLvl.Error);
                                        }
                                    }

                                    Thread.Sleep(2000);
                                    item = Host.MyGetItem(4881);
                                    Host.MyUseItemAndWait(item, Host.Me);
                                    Thread.Sleep(1000);
                                    Host.StartQuest(myQuest.Id);
                                }
                            }
                            else
                            {
                                Host.log("Нет ключа");
                                item = Host.MyGetItem(4881);
                                if (item != null)
                                {
                                    Host.MyUseItemAndWait(item, Host.Me);
                                    Thread.Sleep(1000);
                                    Host.StartQuest(myQuest.Id);
                                }
                                else
                                {
                                    Host.log("Надо выбить ключ");

                                    if (Host.Me.Distance(-247.09, -5109.54, 41.35) > 3)
                                    {
                                        if (!Host.CommonModule.MoveTo(-247.09, -5109.54, 41.35))
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                            return;
                        }

                    case 819:
                        {
                            if (Host.Me.Distance(257.75, -3029.97, 97.61) > 5)
                            {
                                if (!Host.CommonModule.MoveTo(257.75, -3029.97, 97.61, 5))
                                {
                                    return;
                                }
                            }

                            var item = Host.MyGetItem(4926);
                            if (item == null)
                            {
                                Host.MyUseGameObject(3238);
                            }
                            else
                            {
                                Host.MyUseItemAndWait(item, Host.Me);
                                Thread.Sleep(1000);
                                Host.StartQuest(myQuest.Id);
                            }

                            Thread.Sleep(1000);
                            return;
                        }

                    case 781:
                        {
                            if (Host.Me.Distance(-3104.67, -1201.28, 85.11) > 2)
                            {
                                if (!Host.CommonModule.MoveTo(-3104.67, -1201.28, 85.11))
                                {
                                    return;
                                }
                            }

                            var item = Host.MyGetItem(4851);
                            if (item != null)
                            {
                                Host.MyUseItemAndWait(item, Host.Me);
                                Thread.Sleep(1000);
                                Host.StartQuest(myQuest.Id);
                            }
                            else
                            {
                                Thread.Sleep(2000);
                                Host.MyUseGameObject(3076);
                                Thread.Sleep(5000);
                                if (!Host.StartQuest(myQuest.Id))
                                {
                                    Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                                }
                            }

                            return;
                        }
                }


                var applyNpcId = new MyQuestInfo(0, new Vector3F(Vector3F.Zero));
                if (MyQuestHelps.MyQuestInfosApplyCustom.ContainsKey(myQuest.Id))
                {
                    applyNpcId = MyQuestHelps.MyQuestInfosApplyCustom[myQuest.Id];
                }
                else
                {
                    if (MyQuestHelps.MyQuestInfosApply.ContainsKey(myQuest.Id))
                    {
                        applyNpcId = MyQuestHelps.MyQuestInfosApply[myQuest.Id];
                    }
                }


                if (applyNpcId.Id == 0)
                {
                    foreach (var myQuestBaseItem in Host.MyQuestBases.MyQuestBases)
                    {
                        if (myQuestBaseItem.Id == myQuest.Id)
                        {
                            applyNpcId.Id = myQuestBaseItem.QuestStart.QuestStartId;
                        }
                    }
                }


                if (applyNpcId.Id == 0 && MyQuestHelps.QuestTableLua.ContainsKey(myQuest.Id))
                {
                    var lua = MyQuestHelps.QuestTableLua[myQuest.Id];
                    if (lua.Start.ObjLua.Count == 1)
                    {
                        foreach (var u in lua.Start.ObjLua)
                        {
                            applyNpcId.Id = u.Value;
                        }
                    }
                    if (lua.Start.UnitLua.Count == 1)
                    {
                        foreach (var u in lua.Start.UnitLua)
                        {
                            applyNpcId.Id = u.Value;
                        }
                    }
                    else
                    {
                        Host.log("Неписей для принятия квеста " + lua.Start.UnitLua.Count);
                        double bestDist = 99999999999;
                        uint bestId = 0;
                        foreach (var u in lua.Start.UnitLua)
                        {
                            if (Host.Me.Distance(Host.MyGetCoordQuestNpc(u.Value, this)) < bestDist)
                            {
                                bestDist = Host.Me.Distance(Host.MyGetCoordQuestNpc(u.Value, this));
                                bestId = u.Value;
                            }
                        }

                        if (bestId != 0)
                        {
                            applyNpcId.Id = bestId;
                        }
                    }
                }


                if (applyNpcId.Id == 0)
                {
                    Host.log("Нет НПС для принятия квеста");
                    return;
                }

                if (applyNpcId.Loc == Vector3F.Zero)
                {
                    Host.log("Нет координат, пытаюсь найти в базе");
                    applyNpcId.Loc = Host.MyGetCoordQuestNpc(applyNpcId.Id, this);
                }

                var npc = Host.GetNpcById(applyNpcId.Id);

                if (myQuest.Id == 5481 || myQuest.Id == 749)
                {
                    var loc = Host.MyGetCoordQuestNpc(applyNpcId.Id, this);
                    if (loc != Vector3F.Zero)
                    {
                        applyNpcId.Loc = Vector3F.Zero;
                    }

                }

                if (applyNpcId.Loc != Vector3F.Zero)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (applyNpcId.Loc.Z == 0)
                    {
                        applyNpcId.Loc.Z = Host.MyGetCoordQuestNpc(applyNpcId.Id, this).Z;
                    }
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (applyNpcId.Loc.Z == 0)
                    {
                        applyNpcId.Loc.Z = Convert.ToSingle(Host.GetNavMeshHeight(applyNpcId.Loc));
                    }

                    if (npc == null)
                    {
                        if (Host.Me.Distance(applyNpcId.Loc) > 51)
                        {
                            if (!Host.CommonModule.MoveTo(applyNpcId.Loc, 50))
                            {
                                return;
                            }
                        }
                    }

                    npc = Host.GetNpcById(applyNpcId.Id);
                    if (npc == null)
                    {
                        if (Host.Me.Distance(applyNpcId.Loc) > 11)
                        {
                            Host.CommonModule.MoveTo(applyNpcId.Loc, 10);
                        }
                    }
                }


                npc = Host.GetNpcById(applyNpcId.Id);
                //Дошли до расположения НПС
                if (npc == null)
                {
                    Host.log("Не нашли НПС в указанных координатах " + applyNpcId.Id + "  " + applyNpcId.Loc);
                    Thread.Sleep(1000);

                    if (myQuest.Id == 5481 || myQuest.Id == 749 || myQuest.Id == 487)
                    {
                        var loc = Host.MyGetCoordQuestNpc(applyNpcId.Id, this);
                        if (loc == Vector3F.Zero)
                        {
                            BadLoc.Clear();
                        }

                        if (Host.CommonModule.MoveTo(loc, 20))
                        {
                            npc = Host.GetNpcById(applyNpcId.Id);
                            if (npc != null)
                            {
                                return;
                            }

                            BadLoc.Add(loc);
                        }
                    }
                    return;
                }

                if (Host.Me.Distance(applyNpcId.Loc) > 4)
                {
                    if (!Host.CommonModule.MoveTo(npc, 4))
                    {
                        return;
                    }
                }

                if (myQuest.Id == 4727)
                {
                    Thread.Sleep(1000);
                    Host.MyUseGameObject(176196);
                    Thread.Sleep(5000);
                    if (!Host.StartQuest(myQuest.Id))
                    {
                        Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                        if (Host.GetLastError() == ELastError.ActionNotAllowed)
                        {
                            Host.log("Выключаю окно игры так как баг в апи");

                            // Host.TerminateGameClient();
                        }
                    }
                }





                if (!Host.MyOpenDialog(npc))
                {
                    if (myQuest.Id == 4728)
                    {
                        Host.log("Выключаю окно игры так как баг в апи");
                        //Host.TerminateGameClient();
                    }
                    if (myQuest.Id == 4725 && Host.GetAgroCreatures().Count == 0)
                    {
                        Host.log("Выключаю окно игры так как баг в апи");
                        //Host.TerminateGameClient();
                    }
                    if (myQuest.Id == 930)
                    {
                        Host.log("Выключаю окно игры так как баг в апи");
                        //Host.TerminateGameClient();
                    }
                    return;
                }

                foreach (var gossipOptionsData in Host.GetNpcDialogs())
                {
                    Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + " " +
                             gossipOptionsData.Confirm + " " + gossipOptionsData.OptionCost + " " +
                             gossipOptionsData.OptionFlags + " " + gossipOptionsData.OptionNPC);
                }

                var isQuestFound = false;
                foreach (var gossipQuestTextData in Host.GetNpcQuestDialogs())
                {
                    if (gossipQuestTextData.QuestID == myQuest.Id)
                    {
                        isQuestFound = true;
                    }

                    Host.log("QuestID: " + gossipQuestTextData.QuestID
                                         + "  QuestTitle:" + gossipQuestTextData.QuestTitle
                                         + " QuestType:" + gossipQuestTextData.QuestType
                                         + " QuestLevel:" + gossipQuestTextData.QuestLevel
                                         + " Repeatable:" + gossipQuestTextData.Repeatable
                                         + " QuestMaxScalingLevel:" + gossipQuestTextData.QuestMaxScalingLevel);
                }

                if (!isQuestFound)
                {
                    Thread.Sleep(1000);
                    Host.log("Не нашел квест " + _skipQuest + "/5");
                    if (myQuest.Id == 1641)
                    {
                        var item = Host.MyGetItem(6775);
                        if (item == null)
                        {
                            Host.PrepareCompleteQuest(myQuest.Id);
                            if (!Host.CompleteQuest(myQuest.Id))
                            {
                                Host.log("Не смог завершить квест " + myQuest.Id + " " + Host.GetLastError(), LogLvl.Error);
                            }
                        }
                        else
                        {
                            Host.MyUseItemAndWait(item);
                            Thread.Sleep(1000);
                            if (!Host.StartQuest(1642))
                            {
                                Host.log("Не удалось взять квест " + 1642);
                            }
                            else
                            {
                                Host.QuestStates.QuestState.Add(myQuest.Id);
                                Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                            }
                        }


                        return;
                    }

                    Host.MySendKeyEsc();
                    _skipQuest++;

                    if (_skipQuest > 5)
                    {
                        Host.QuestStates.QuestState.Add(myQuest.Id);
                        Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                        _skipQuest = 0;
                    }
                    return;
                }

                var quest = Host.GetQuest(myQuest.Id);
                if (quest == null)
                {
                    if (!Host.StartQuest(myQuest.Id))
                    {
                        Host.log("Не смог начать квест " + myQuest.Id + " Всего диалогов у НПС " + Host.GetNpcQuestDialogs().Count + "   " + Host.GetLastError(), LogLvl.Error);
                    }

                    Host.MySendKeyEsc();
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        private void ComliteQuestClassic(MyQuest myQuest)
        {
            try
            {
                _state = "Complete";
                BestQuestId = myQuest.Id;
                var quest = Host.GetQuest(myQuest.Id);
                Host.log("Сдаю квест " + Host.GameDB.QuestTemplates[myQuest.Id]?.LogTitle + "[" + myQuest.Id + "]" + " State:" + quest?.State, LogLvl.Ok);
                if (quest?.State == EQuestState.Fail)
                {
                    if (Host.CancelQuest(quest.Id))
                        Host.log("Не удалось отменить квест  " + myQuest.Id);
                    FillQuestClassic();
                    return;
                }
                switch (myQuest.Id)
                {


                    case 6344:
                        {

                            if (Host.Me.Distance(8784.10, 966.53, 30.21) > 300)
                            {
                                if (Host.Area.Id == 141 || Host.Area.Id == 1657)
                                {
                                    if (Host.Area.Id == 1657)//добежать до дерева
                                    {
                                        if (!Host.CommonModule.MoveTo(9944.84, 2608.60, 1316.28))
                                        {
                                            return;
                                        }

                                        if (!Host.CommonModule.MoveTo(9948.55, 2642.74, 1316.87))
                                        {
                                            return;
                                        }

                                        Thread.Sleep(2000);
                                        return;
                                    }


                                    /*  if (Host.Me.Distance(8381.02, 987.79, 29.61) < 50)
                                      {
                                          Host.MyUseTaxi(148, new Vector3F((float)7459.90, (float)-326.56, (float)8.09));
                                      }
                                      else
                                      {
                                          Host.CommonModule.MoveTo(new Vector3F(9944.84, 2608.60, 1316.28), 1);
    
                                          continue;
                                      }*/
                                    return;
                                }
                            }

                        }
                        break;
                    case 5931:
                        {
                            if (Host.Area.Id == 493)
                            {
                                Host.MyUseStone();
                                return;
                            }
                            break;
                        }

                    case 2041:
                        {
                            if (Host.Me.Race == ERace.NightElf || Host.Me.Race == ERace.Gnome || Host.Me.Race == ERace.Dwarf)
                            {
                                if (!CheckPathReverse())
                                    return;
                            }

                            if (Host.Area.Id != 1519)
                            {
                                Host.MyUseTaxi(1519, new Vector3F(-8391.01, 634.76, 94.89));
                                return;
                            }
                        }
                        break;

                    case 5922 when Host.Area.Id == 1638:
                        Host.FarmModule.UseSkillAndWait(18960);
                        return;
                    case 5921 when Host.Area.Id == 1657:
                        Host.FarmModule.UseSkillAndWait(18960);
                        return;

                    case 307:
                        Host.FarmModule.FarmState = FarmState.Disabled;
                        break;
                    case 6062:
                    case 6083:
                    case 6082:
                        {
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (entity.Charmer != Host.Me)
                                {
                                    continue;
                                }

                                Host.log("Есть пет");
                                Host.SpellManager.UsePetAction(entity.Guid, 3, EActiveStates.ACT_COMMAND);
                                Thread.Sleep(2000);
                                return;
                            }

                            break;
                        }

                    case 1526:
                        {
                            var item = Host.MyGetItem(6655);
                            if (item != null)
                            {
                                if (!Host.CommonModule.MoveTo(-244.49, -4020.22, 187.30))
                                {
                                    return;
                                }

                                foreach (var gameObject in Host.GetEntities<GameObject>())
                                {
                                    if (gameObject.Id != 61934)
                                    {
                                        continue;
                                    }

                                    if (!gameObject.Use())
                                    {
                                        Host.log("Не смог использовать GO " + Host.GetLastError());
                                    }
                                }

                                if (!Host.CompleteQuest(myQuest.Id))
                                {
                                    Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                                }

                                Thread.Sleep(1000);
                                return;
                            }


                            item = Host.MyGetItem(6636);
                            if (item != null)
                            {
                                if (!Host.CommonModule.MoveTo(-257.42, -3984.49, 169.41))
                                {
                                    return;
                                }

                                Host.MyUseItemAndWait(item);
                                Thread.Sleep(5000);
                                return;
                            }

                            var npcQuestShaman = Host.GetNpcById(5893);
                            if (npcQuestShaman != null)
                            {
                                if (!Host.CommonModule.MoveTo(-243.80, -4014.37, 187.30))
                                {
                                    return;
                                }

                                Host.FarmModule.BestMob = npcQuestShaman as Unit;
                                Thread.Sleep(5000);
                                return;
                            }

                            break;
                        }

                    case 5727:
                        {
                            if (Host.MyGetItem(14544) != null && quest.State != EQuestState.Complete)
                            {
                                if (!Host.CommonModule.MoveTo(1801.39, -4375.40, -17.51, 5))
                                {
                                    return;
                                }

                                var npc2 = Host.GetNpcById(3216);
                                if (npc2 != null)
                                {
                                    if (!Host.MyOpenDialog(npc2))
                                    {
                                        return;
                                    }

                                    for (var i = 0; i < 5; i++)
                                    {
                                        foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                        {
                                            Host.SelectNpcDialog(gossipOptionsData);
                                            Thread.Sleep(000);
                                            Host.log(gossipOptionsData.OptionNPC + " " + gossipOptionsData.ClientOption);
                                        }
                                    }
                                }

                                return;
                            }
                        }
                        break;
                    case 1524:
                    case 1525:
                        {
                            if (Host.Me.Location.Z < 83)
                            {
                                if (!Host.CommonModule.MoveTo(-255.76, -3898.19, 82.51))
                                {
                                    return;
                                }

                                if (Host.Me.Distance(-255.76, -3898.19, 82.51) < 5)
                                {
                                    Host.CommonModule.ForceMoveTo2(new Vector3F(-266.46, -3898.74, 91.95));
                                }
                            }
                        }
                        break;
                }


                QuestPOI questPoi = null;

                foreach (var questPois in quest.GetQuestPOI())
                {
                    if (questPois.ObjectiveIndex != -1)
                    {
                        continue;
                    }

                    questPoi = questPois;
                    break;
                }

                var revardNpcId = quest.CompletionNpcIds[0];


                if (revardNpcId == 0)
                {
                    foreach (var myQuestBaseItem in Host.MyQuestBases.MyQuestBases)
                    {
                        if (myQuestBaseItem.Id == myQuest.Id)
                        {
                            revardNpcId = myQuestBaseItem.QuestStart.QuestStartId;
                        }
                    }
                }

                if (revardNpcId == 0)
                {
                    Host.log("Нет НПС для сдачи квеста");
                    return;
                }

                var revardNpcLoc = Host.MyGetCoordQuestNpc(revardNpcId, this);

                var npc = Host.GetNpcById(revardNpcId);

                if (questPoi != null && npc == null)
                {
                    var badloc = false;
                    foreach (var vector3F in BadLoc)
                    {
                        if (vector3F.Distance2D(revardNpcLoc) < 20)
                        {
                            badloc = true;
                        }
                    }

                    if (!badloc)
                    {
                        if (Host.Me.Distance2D(revardNpcLoc) > 20)
                        {
                            if (!Host.CommonModule.MoveTo(revardNpcLoc, 23))
                            {
                                return;
                            }

                            npc = Host.GetNpcById(revardNpcId);
                            if (npc == null)
                            {
                                BadLoc.Add(revardNpcLoc);
                            }
                        }
                    }
                }
                else
                {
                    npc = Host.GetNpcById(revardNpcId);
                    if (npc == null)
                    {
                        MyQuestInfo complNpcId = new MyQuestInfo(0, new Vector3F(Vector3F.Zero));
                        if (MyQuestHelps.MyQuestInfosCompleteCustom.ContainsKey(myQuest.Id))
                        {
                            complNpcId = MyQuestHelps.MyQuestInfosCompleteCustom[myQuest.Id];
                        }
                        else
                        {
                            if (MyQuestHelps.MyQuestInfosComplete.ContainsKey(myQuest.Id))
                            {
                                complNpcId = MyQuestHelps.MyQuestInfosComplete[myQuest.Id];
                            }
                        }
                        if (Host.ClientType == EWoWClient.Retail)
                        {
                            Host.log("Нет координат " + quest.GetQuestPOI().Count);
                        }

                        if (revardNpcLoc != Vector3F.Zero)
                        {
                            if (Host.Me.Distance(revardNpcLoc) > 23)
                            {
                                if (!Host.CommonModule.MoveTo(revardNpcLoc, 22))
                                {
                                    return;
                                }
                            }
                        }

                        npc = Host.GetNpcById(revardNpcId);
                        if (npc == null)
                        {
                            if (complNpcId.Loc != Vector3F.Zero)
                            {
                                var locNpc = complNpcId.Loc;
                                // ReSharper disable once CompareOfFloatsByEqualityOperator
                                if (locNpc.Z == 0)
                                {
                                    locNpc.Z = Convert.ToSingle(Host.GetNavMeshHeight(locNpc));
                                }

                                if (Host.Me.Distance(revardNpcLoc) > 21)
                                {
                                    if (!Host.CommonModule.MoveTo(locNpc, 21))
                                    {
                                        return;
                                    }
                                }
                            }
                        }

                    }
                }

                npc = Host.GetNpcById(revardNpcId);
                if (npc != null)
                {
                    switch (npc.Id)
                    {
                        default:
                            {
                                if (Host.Me.Distance(npc) > 4)
                                {
                                    if (!Host.CommonModule.MoveTo(npc, 4))
                                    {
                                        return;
                                    }
                                }
                            }
                            break;
                    }

                    Thread.Sleep(500);
                    if (myQuest.Id == 1786)
                    {
                        var item = Host.MyGetItem(6866);
                        if (item != null)
                        {
                            Host.MyUseItemAndWait(item, npc);
                            Thread.Sleep(2000);
                        }

                    }
                    if (!Host.MyOpenDialog(npc))
                    {
                        return;
                    }

                    uint revard = 0;


                    /* if (Host.QuestRewardOffer == null)
                     {
                         //Host.log("QuestRewardOffer  = null");
                     }
                     else
                     {
                         Host.log("AvailableItemIds  " + Host.QuestRewardOffer.AvailableItemIds.Count);
                     }*/


                    if (!Host.PrepareCompleteQuest(quest.Id))
                    {
                        Host.log("Не смог узнать награду " + Host.GetLastError(), LogLvl.Error);
                    }


                    if (Host.QuestRewardOffer != null && Host.QuestRewardOffer.AvailableItemIds != null)
                    {
                        /* if (Host.QuestRewardOffer != null && Host.QuestRewardOffer.AvailableItemIds.Count > 1)
                         {
                             Host.log("[" + quest.Id + "]Quest offer " + Host.QuestRewardOffer.AvailableItemIds.Count + " rewards, while we dont select any. Will get depends on our ilvl and prefered stats");
                            // var classCfg = Host.GetClassConfig();
                             var allRewards = Host.QuestRewardOffer.AvailableItemIds.ToList();
                             uint reward = 0;
                             foreach (var ids in allRewards)
                             {
                                 if (!Host.GameDB.ItemTemplates.ContainsKey(ids))
                                     continue;
                                 var item = Host.GameDB.ItemTemplates[ids];
                                 for (uint i = 0; i < 10; i++)
                                 {
                                     var stat = (EItemModType)item.GetItemStatType(i);
                                     if (stat == classCfg.PrimaryModifier)
                                     {
                                         if (reward == 0)
                                             reward = ids;
                                         var slot = Host.CommonModule.GetItemEPlayerPartsType(item.GetInventoryType());

                                         var bonusData = new ItemBonusData();
                                         bonusData.Init(item);
                                         uint targetItemiLvl = Item.GetItemLevel(item, bonusData, Host.Me.Level, 0, 0, 0, 0, 0, false);
                                         uint ourItemiLvl = 0;
                                         var ourItem = Host.ItemManager.GetItems().FirstOrDefault(e => e.Place == EItemPlace.Equipment && e.Cell == (byte)slot);
                                         if (ourItem != null)
                                             ourItemiLvl = ourItem.Level;
                                         uint ourItemiLvl2 = ourItem.Level;
                                         if (slot == EEquipmentSlot.Finger1 || slot == EEquipmentSlot.Trinket1)
                                         {
                                             ourItem = Host.ItemManager.GetItems().FirstOrDefault(e => e.Place == EItemPlace.Equipment && e.Cell == (byte)((uint)slot + 1));
                                             if (ourItem != null)
                                                 ourItemiLvl2 = ourItem.Level;
                                         }
                                         if (ourItemiLvl < targetItemiLvl || ourItemiLvl2 < targetItemiLvl || !ourItem.ItemStatType.Contains(classCfg.PrimaryModifier))
                                             reward = ids;
                                     }
                                 }
                             }

                             if (revard == 0 && reward != 0)
                                 Host.log("[" + quest.Id + "]Auto select reward: " + reward);
                             if (reward == 0)
                             {
                                 Host.log("[" + quest.Id + "]Cant select reward itself. Please fix it");
                                 return false;
                             }
                         }*/


                        foreach (var templateRewardItem in Host.QuestRewardOffer.AvailableItemIds)
                        {
                            if (Host.GameDB.ItemTemplates.ContainsKey(templateRewardItem))
                            {
                                var item = Host.GameDB.ItemTemplates[templateRewardItem];
                                if (Host.CommonModule.GetItemEPlayerPartsType(item.GetInventoryType()) == EEquipmentSlot.Ranged)
                                {
                                    continue;
                                }
                                /* Host.log(
    "Награда " + item.GetName() + "[" + templateRewardItem + "]" +
    item.GetBaseItemLevel() + "  " +
    Host.CommonModule.GetItemEPlayerPartsType(item.GetInventoryType()),
    LogLvl.Important);*/
                                for (uint i = 0; i < 10; i++)
                                {
                                    if (item.GetItemStatType(i) == -1)
                                    {
                                        continue;
                                    }

                                    Host.log(i + ") " + (EItemModType)item.GetItemStatType(i));
                                }

                                /* foreach (var item1 in Host.ItemManager.GetItems())
                                 {
                                     if (item1.Place != EItemPlace.Equipment)
                                         continue;
                                     if (Host.CommonModule.GetItemEPlayerPartsType(item.GetInventoryType()) != Host.CommonModule.GetItemEPlayerPartsType(item1.InventoryType))
                                         continue;
                                     Host.log("Одето " + item1.Name + "[" + item1.Id + "] " + item1.Level);
                                 }*/

                                if (Host.ClientType == EWoWClient.Retail)
                                {
                                    if (item.GetClass() == EItemClass.Weapon &&
                                        !Host.CommonModule.WeaponType.Contains((EItemSubclassWeapon)item.GetSubClass())
                                    )
                                    {
                                        continue;
                                    }

                                    if (item.GetClass() == EItemClass.Armor &&
                                        !Host.CommonModule.ArmorType.Contains((EItemSubclassArmor)item.GetSubClass()))
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (Host.CharacterSettings.AdvancedEquip)
                                    {
                                        if (item.GetClass() == EItemClass.Weapon &&
                                            !Host.CommonModule.WeaponType.Contains(
                                                (EItemSubclassWeapon)item.GetSubClass()))
                                        {
                                            continue;
                                        }

                                        if (item.GetClass() == EItemClass.Armor &&
                                            !Host.CommonModule.ArmorType.Contains(
                                                (EItemSubclassArmor)item.GetSubClass()))
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (item.GetClass() == EItemClass.Weapon)
                                        {
                                            if ((Host.GetProficiency(EItemClass.Weapon) &
                                                 (1 << (int)item.GetSubClass())) == 0)
                                            {
                                                continue;
                                            }
                                        }

                                        if (item.GetClass() == EItemClass.Armor)
                                        {
                                            if ((Host.GetProficiency(EItemClass.Armor) &
                                                 (1 << (int)item.GetSubClass())) == 0)
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }


                                if (revard == 0)
                                {
                                    revard = templateRewardItem;
                                }
                            }
                            else
                            {
                                Host.log("Неизвестная награда " + templateRewardItem, LogLvl.Error);
                            }
                        }


                        /*if (Host.QuestRewardOffer.AvailableItemIds.Count > 0)
                            if (revard != 0)
                                Host.log("Выбранный предмет записан в боте " + revard);*/


                        if (Host.QuestRewardOffer.AvailableItemIds.Count > 1 && revard == 0)
                        {
                            var rand = Host.RandGenerator.Next(0, Host.QuestRewardOffer.AvailableItemIds.Count);
                            Host.log(
                                "Не выбрана награда " + revard + " " + Host.QuestRewardOffer.AvailableItemIds.Count +
                                "  " + rand, LogLvl.Important);
                            revard = Host.QuestRewardOffer.AvailableItemIds[rand];
                            //Thread.Sleep(5000);
                            // return false;
                        }
                    }

                    if (quest.Id == 957)
                        revard = 5604;


                    Thread.Sleep(500);
                    if (!Host.CompleteQuest(quest.Id, revard))
                    {
                        Host.log(
                            "Не смог завершить квест " + myQuest.Id + " с выбором награды " + revard + "  " +
                            Host.GetLastError(), LogLvl.Error);
                        Thread.Sleep(6000);

                        var zRange = Math.Abs(Host.Me.Location.Z - npc.Location.Z);
                        if (zRange > 6)
                        {
                            var randloc = Host.RandGenerator.Next(-3, 3);
                            Host.CommonModule.MoveTo(Host.Me.Location.X + randloc, Host.Me.Location.Y + randloc,
                                Host.Me.Location.Z);
                            Host.CommonModule.MoveTo(npc, 0);
                        }

                        Host.MySendKeyEsc();
                        if (Host.GetQuest(myQuest.Id) == null)
                        {
                            Host.QuestStates.QuestState.Add(myQuest.Id);
                            Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                        }
                    }
                    else
                    {
                        //Host.log("Завершил квест " + myQuest.Id, LogLvl.Ok);

                        Host.QuestStates.QuestState.Add(myQuest.Id);
                        Host.ConfigLoader.SaveConfig(Host.PathQuestState + Host.Me.Name + "[" + Host.GetCurrentAccount().ServerName + "].json", Host.QuestStates);
                    }

                    Thread.Sleep(500);
                }
                else
                {
                    Host.log("Не нашел завершающего НПС по указанным координатам " + revardNpcId + "  ", LogLvl.Error);

                    if (myQuest.Id == 749)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(2908);
                        Thread.Sleep(5000);
                        var go = Host.GetNpcById(2908);
                        if (go != null)
                        {
                            if (!Host.CompleteQuest(myQuest.Id))
                            {
                                Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                                if (Host.GetLastError() == ELastError.ActionNotAllowed && Host.Me.Distance(go) < 3)
                                {
                                    Host.log("Выключаю окно игры так как баг в апи");
                                    //Host.TerminateGameClient();
                                }
                            }
                        }

                    }

                    if (myQuest.Id == 4812)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(175524);
                        Thread.Sleep(5000);
                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                Host.log("Выключаю окно игры так как баг в апи");
                                //Host.TerminateGameClient();
                            }
                        }
                    }



                    if (myQuest.Id == 1003)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(17185);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                Host.log("Выключаю окно игры так как баг в апи");
                                //Host.TerminateGameClient();
                            }
                        }
                    }

                    if (myQuest.Id == 1002)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(17184);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                Host.log("Выключаю окно игры так как баг в апи");
                                //Host.TerminateGameClient();
                            }
                        }
                    }

                    if (myQuest.Id == 983)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(17182);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                Host.log("Выключаю окно игры так как баг в апи");
                                //Host.TerminateGameClient();
                            }
                        }
                    }

                    if (myQuest.Id == 1001)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(17183);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                Host.log("Выключаю окно игры так как баг в апи");
                                //Host.TerminateGameClient();
                            }
                        }
                    }

                    if (myQuest.Id == 37)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(55);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                            {
                                Host.log("Выключаю окно игры так как баг в апи");
                                //Host.TerminateGameClient();
                            }
                        }
                    }

                    if (myQuest.Id == 419)
                    {
                        Thread.Sleep(1000);

                        Host.MyUseGameObject(2059);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                        }
                    }

                    if (myQuest.Id == 250)
                    {
                        Thread.Sleep(1000);

                        Host.MyUseGameObject(257);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                        }
                    }


                    if (myQuest.Id == 45)
                    {
                        Thread.Sleep(1000);
                        Host.MyUseGameObject(56);

                        if (!Host.CompleteQuest(myQuest.Id))
                        {
                            Host.log("Не смог завершить квест " + myQuest.Id + " с выбором награды " + "  " + Host.GetLastError(), LogLvl.Error);
                        }
                    }

                    if (myQuest.Id == 1517)
                    {
                        Host.CommonModule.MoveTo(-875.79, -4291.79, 73.25);
                        var item = Host.MyGetItem(6635);
                        Host.MyUseItemAndWait(item);
                    }

                    if (myQuest.Id == 751)
                    {
                        var loc = Host.MyGetCoordQuestNpc(revardNpcId, this);
                        if (loc == Vector3F.Zero)
                        {
                            BadLoc.Clear();
                        }

                        if (Host.CommonModule.MoveTo(loc, 20))
                        {
                            npc = Host.GetNpcById(revardNpcId);
                            if (npc != null)
                            {
                                return;
                            }

                            BadLoc.Add(loc);
                        }
                        return;
                    }

                    _completeQuestCount++;
                    if (revardNpcLoc != Vector3F.Zero && Host.Me.Distance(revardNpcLoc) < 400 && _completeQuestCount > 20)
                    {
                        var loc = Host.MyGetCoordQuestNpc(revardNpcId, this);
                        if (loc == Vector3F.Zero)
                        {
                            BadLoc.Clear();
                        }

                        if (Host.CommonModule.MoveTo(loc, 20))
                        {
                            npc = Host.GetNpcById(revardNpcId);
                            if (npc != null)
                            {
                                return;
                            }

                            BadLoc.Add(loc);
                        }
                        return;
                        /* if (!Host.CommonModule.MoveTo(revardNpcLoc, 20))
                         {
                             return;
                         }

                         npc = Host.GetNpcById(revardNpcId);
                         if (npc == null)
                         {
                             BadLoc.Add(revardNpcLoc);
                         }
                         else
                         {
                             BadLoc.Clear();
                         }
                         */
                        // _completeQuestCount = 0;
                    }

                    /* if (revardNpcLoc == Vector3F.Zero)
                     {
                         BadLoc.Clear();
                     }*/
                }
            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        private void RunQuestClassic(MyQuest myQuest)
        {
            try
            {
                _state = "Run";
                BestQuestId = myQuest.Id;

                var quest = Host.GetQuest(myQuest.Id);
                var index = GetQuestIndex(quest);
                if (myQuest.Index != -2)
                {
                    index = myQuest.Index;
                }
                if (quest?.State == EQuestState.Fail && myQuest.Id != 2118)
                {
                    if (Host.CancelQuest(quest.Id))
                        Host.log("Не удалось отменить квест  " + myQuest.Id);
                    FillQuestClassic();
                    return;
                }

                if (myQuest.Id == 2118 && quest == null)
                {
                    FillQuestClassic();
                    return;
                }

                Host.log("Выполняю квест " + Host.GameDB.QuestTemplates[myQuest.Id]?.LogTitle + "[" + myQuest.Id + "]" + myQuest.QuestAction + " Index:" + index + " myQuest.Index:" + myQuest.Index + " State:" + quest.State, LogLvl.Ok);


                switch (myQuest.Id)
                {
                    case 2520:
                        {
                            if (Host.Me.Distance(9641.18, 2522.29, 1331.57) > 3)
                            {
                                if (!Host.CommonModule.MoveTo(9641.18, 2522.29, 1331.57))
                                {
                                    return;
                                }
                            }
                            var item = Host.MyGetItem(8155);
                            if (item != null)
                            {


                                Host.MyUseItemAndWait(item);

                            }
                            return;
                        }
                        break;


                    case 5929:
                    case 5930:
                        {
                            if (Host.Me.Distance(8071.88, -2289.20, 496.87) > 3)
                            {
                                if (!Host.CommonModule.MoveTo(8071.88, -2289.20, 496.87))
                                {
                                    return;
                                }
                            }

                            var npc = Host.GetNpcById(11956);
                            if (npc != null)
                            {
                                Host.MyDialog(npc, 0);
                            }

                            return;
                        }

                    case 1719:
                        {
                            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                            if (Host.Me.Distance(-1678.80, -4328.79, 2.42) > 1)
                            {
                                if (!Host.CommonModule.MoveTo(-1678.80, -4328.79, 2.42))
                                {
                                    return;
                                }
                            }

                            var npc = Host.GetNpcById(6238);
                            if (npc != null && ((Unit)npc).IsAlive)
                            {
                                Host.FarmModule.BestMob = npc as Unit;
                            }

                            return;
                        }

                    case 1667:
                        {
                            if (Host.Me.Distance(-9764.00, -1562.79, 41.44) > 3)
                            {
                                if (!Host.CommonModule.MoveTo(-9764.00, -1562.79, 41.44))
                                {
                                    return;
                                }
                            }

                            var item = Host.MyGetItem(6783);
                            if (item != null)
                            {
                                var go = Host.GetNpcById(85563);
                                if (go != null)
                                {
                                    Host.CommonModule.MoveTo(go, 1);
                                    Host.MyUseItemAndWait(item, go);
                                }
                            }

                            return;
                        }

                    case 957:
                        {
                            if (Host.Me.Distance(5631.62, 166.14, 32.26) > 3)
                            {
                                if (!Host.CommonModule.MoveTo(5631.62, 166.14, 32.26))
                                {
                                    return;
                                }
                            }

                            var item = Host.MyGetItem(5338);
                            if (item != null)
                            {
                                var go = Host.GetNpcById(16393);
                                if (go != null)
                                {
                                    Host.CommonModule.MoveTo(go, 2);
                                    Host.MyUseItemAndWait(item, go);
                                }
                            }

                            return;
                        }


                    case 435:
                        ConvoyClassic(myQuest);
                        return;
                    case 6002 when index == 0:
                        {
                            var npc = Host.GetNpcById(12144);
                            if (npc != null)
                            {

                                Host.MyDialog(npc, 1);
                                return;
                            }

                            if (!Host.CommonModule.MoveTo(-2498.48, -1631.96, 91.72))
                            {
                                return;
                            }

                            var item = Host.MyGetItem(15710);
                            Host.MyUseItemAndWait(item);
                            Thread.Sleep(5000);
                            return;
                        }

                    case 6001 when index == 0:
                        {
                            var npc = Host.GetNpcById(12144);
                            if (npc != null)
                            {

                                Host.MyDialog(npc, 0);
                                return;
                            }

                            if (!Host.CommonModule.MoveTo(6326.39, 94.40, 21.65))
                            {
                                return;
                            }

                            var item = Host.MyGetItem(15208);
                            Host.MyUseItemAndWait(item);
                            Thread.Sleep(5000);
                            return;
                        }

                    case 2518:
                        {
                            var npc = Host.GetUnitById(7319);
                            if (npc != null)
                            {
                                Host.FarmModule.BestMob = npc;
                            }
                            if (Host.Me.Distance(10942.92, 1419.34, 1308.86) > 100)
                                Host.CommonModule.MoveTo(10942.92, 1419.34, 1308.86, 1);
                            npc = Host.GetUnitById(7319);
                            if (npc != null)
                            {
                                Host.FarmModule.BestMob = npc;
                            }
                            else
                            {
                                if (Host.Me.Distance(10952.11, 1786.92, 1321.62) > 3)
                                    Host.CommonModule.MoveTo(10952.11, 1786.92, 1321.62, 1);
                            }
                            Thread.Sleep(5000);

                            return;
                        }

                    case 2561:
                        {
                            if (Host.Me.Distance(9834.66, 1496.08, 1258.79) > 3)
                                Host.CommonModule.MoveTo(9834.66, 1496.08, 1258.79, 1);

                            var npc = Host.GetUnitById(7318);
                            if (npc != null)
                            {
                                Host.FarmModule.BestMob = npc;
                            }
                            Thread.Sleep(5000);

                            return;
                        }

                    case 4811 when index == 0 && !Host.CommonModule.MoveTo(6276.46, -172.67, 53.62, 2):
                    case 87 when index == 0 && !Host.CommonModule.MoveTo(-9746.21, 93.01, 12.81, 2):
                    case 2288 when index == 0 && !Host.CommonModule.MoveTo(-5611.31, -4188.39, 389.14, 2):
                    case 7383 when index == 0 && !Host.CommonModule.MoveTo(10674.97, 1858.48, 1324.26, 2):
                    case 933 when index == 0 && !Host.CommonModule.MoveTo(9554.38, 1655.66, 1299.37, 2):
                    case 744 when index == 0 && !Host.CommonModule.MoveTo(-710.40, -738.40, 19.81, 30):
                    // case 92 when index == 2 && !Host.CommonModule.MoveTo(-9117.30, -2608.50, 115, 30):
                    case 354 when index == 0 && !Host.CommonModule.MoveTo(2893.9, 1000.7, 110.01, 30):
                    case 929 when index == 0 && !Host.CommonModule.MoveTo(9860.09, 588.73, 1300.66, 1):
                    case 921 when index == 0 && !Host.CommonModule.MoveTo(10708.25, 763.45, 1321.26, 1):
                    case 743 when index == 0 && !Host.CommonModule.MoveTo(-2651.34, -1253.13, 15.71, 25):
                    case 429 when index == 1 && !Host.CommonModule.MoveTo(1353.43, 1044.50, 52.45, 25):
                    case 437 when index == 1 && !Host.CommonModule.MoveTo(1067.35, 1543.09, 28.30, 25):
                    case 1525 when index == 0 && !Host.CommonModule.MoveTo(231.9, -3021.6, 136.87, 25):
                    case 765 when index == 0 && !Host.CommonModule.MoveTo(-1761.20, -1281.20, 113.46, 25):
                    case 745 when !Host.CommonModule.MoveTo(-2679.87, -744.04, -5.81, 5):
                    case 3376 when index == 0 && !Host.CommonModule.MoveTo(-2940.00, -1266.20, 72.53, 30):
                    case 381 when index == 0 && !Host.CommonModule.MoveTo(1821.22, 1439.93, 84.10, 10):
                    case 816 when !Host.CommonModule.MoveTo(984.89, -3869.08, 18.00, 30):
                    case 2139 when !Host.CommonModule.MoveTo(6700.79, -437.35, 75.54, 5):
                    case 825 when !Host.CommonModule.MoveTo(-185.49, -5277.38, -4.95, 50):
                    case 818 when !Host.CommonModule.MoveTo(91.39, -5140.20, 0.34):
                        return;
                    case 11:
                        {
                            if (Host.Me.Distance(-9999.60, 623.00, 39.11) > 400)
                            {
                                if (!Host.CommonModule.MoveTo(-10153.40, 957.46, 37.03))
                                {
                                    return;
                                }
                            }

                            break;
                        }

                    case 2206:
                        {
                            if (Host.Me.Distance(-9993.14, -100.75, 24.52) > 10)
                            {
                                if (!Host.CommonModule.MoveTo(-9993.14, -100.75, 24.52))
                                {
                                    return;
                                }
                            }

                            var npc = Host.GetNpcById(6846);
                            if (npc != null && ((Unit)npc).IsAlive)
                            {
                                Host.FarmModule.FarmState = FarmState.Disabled;
                                Host.AdvancedInvisible();
                                Host.CommonModule.MoveTo(npc, 3);
                                Host.MyCheckIsMovingIsCasting();
                                var res = Host.SpellManager.CastSpell(921, npc);

                                if (res != ESpellCastError.SUCCESS)
                                {
                                    Host.log("Не удалось обокрасть " + res + " " + Host.GetLastError(), LogLvl.Error);
                                    if (res == ESpellCastError.OUT_OF_RANGE)
                                    {
                                        if (!Host.CommonModule.ForceMoveTo(npc, 4.2))
                                        {
                                            return;
                                        }

                                        Host.MyCheckIsMovingIsCasting();
                                        res = Host.SpellManager.CastSpell(921, Host.FarmModule.BestMob);
                                        if (res != ESpellCastError.SUCCESS)
                                        {
                                            Host.log("Не удалось обокрасть 2 " + res + " " + Host.GetLastError(), LogLvl.Error);
                                        }
                                    }
                                }
                                var waitTill = DateTime.UtcNow.AddSeconds(1);
                                while (waitTill > DateTime.UtcNow)
                                {
                                    Thread.Sleep(100);
                                    if (!Host.MainForm.On)
                                    {
                                        return;
                                    }

                                    if (Host.CanPickupLoot())
                                    {
                                        break;
                                    }
                                }
                                if (Host.CanPickupLoot())
                                {
                                    Host.PickupLoot();
                                }

                                if (!Host.CommonModule.MoveTo(-9965.24, -198.16, 23.28))
                                {
                                    return;
                                }

                                foreach (var spell in Host.Me.GetAuras())
                                {
                                    if (spell.SpellName == "Stealth")
                                    {
                                        spell.Cancel();
                                    }
                                }

                                Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                            }
                            return;
                        }

                    case 1858:
                        {
                            if (Host.Me.Distance(1630.68, -4439.80, 15.52) > 10)
                            {
                                if (!Host.CommonModule.MoveTo(1630.68, -4439.80, 15.52))
                                {
                                    return;
                                }
                            }

                            var item = Host.MyGetItem(7208);
                            var itemTarget = Host.MyGetItem(7209);

                            if (itemTarget != null && !itemTarget.IsLocked)
                            {
                                Host.MyUseItemAndWait(itemTarget);
                                return;
                            }

                            if (item != null)
                            {
                                foreach (var spell in Host.Me.GetAuras())
                                {
                                    if (spell.SpellName == "Stealth")
                                    {
                                        spell.Cancel();
                                    }
                                }
                                Thread.Sleep(1000);
                                var res = Host.SpellManager.UseItem(item, itemTarget);
                                if (res != EInventoryResult.OK)
                                {
                                    Host.log("Не удалось открыть сумку " + res + " " + Host.GetLastError(), LogLvl.Error);
                                }

                                Thread.Sleep(5000);
                                return;
                            }

                            var npc = Host.GetNpcById(6466);
                            if (npc != null && ((Unit)npc).IsAlive)
                            {
                                Host.AdvancedInvisible();
                                Host.CommonModule.MoveTo(npc, 3);
                                Host.MyCheckIsMovingIsCasting();
                                var res = Host.SpellManager.CastSpell(921, npc);

                                if (res != ESpellCastError.SUCCESS)
                                {
                                    Host.log("Не удалось обокрасть " + res + " " + Host.GetLastError(), LogLvl.Error);
                                    if (res == ESpellCastError.OUT_OF_RANGE)
                                    {
                                        if (!Host.CommonModule.ForceMoveTo(npc, 4.2))
                                        {
                                            return;
                                        }

                                        Host.MyCheckIsMovingIsCasting();
                                        res = Host.SpellManager.CastSpell(921, Host.FarmModule.BestMob);
                                        if (res != ESpellCastError.SUCCESS)
                                        {
                                            Host.log("Не удалось обокрасть 2 " + res + " " + Host.GetLastError(), LogLvl.Error);
                                        }
                                    }
                                }
                                var waitTill = DateTime.UtcNow.AddSeconds(1);
                                while (waitTill > DateTime.UtcNow)
                                {
                                    Thread.Sleep(100);
                                    if (!Host.MainForm.On)
                                    {
                                        return;
                                    }

                                    if (Host.CanPickupLoot())
                                    {
                                        break;
                                    }
                                }
                                if (Host.CanPickupLoot())
                                {
                                    Host.PickupLoot();
                                }
                            }
                            Thread.Sleep(3000);
                            return;
                        }

                    case 408 when myQuest.Index == 2 && !Host.CommonModule.MoveTo(3043.88, 648.38, 59.06):
                    case 806 when !Host.CommonModule.MoveTo(876.72, -4224.71, -11.53):
                        return;
                    case 808:
                        {
                            if (Host.Me.Distance(-1295.66, -5540.79, 14.78) > 50)
                            {
                                if (!Host.CommonModule.MoveTo(-1329.96, -5536.41, 3.63))
                                {
                                    return;
                                }
                            }

                            Host.CommonModule.MoveTo(-1288.36, -5534.87, 15.24, 3);
                            Thread.Sleep(1000);
                            break;
                        }

                    case 784 when myQuest.Index == 2:
                        {
                            if (!Host.CommonModule.MoveTo(-248.04, -5092.39, 41.35))
                            {
                                return;
                            }

                            Thread.Sleep(5000);
                            break;
                        }

                    case 5624:
                        {
                            var spell1 = Host.SpellManager.GetSpell(2052);
                            var spell2 = Host.SpellManager.GetSpell(1243);
                            if (spell1 == null || spell2 == null)
                            {
                                Host.log("Необходимо выучить скилы 2052 или 1243");
                                return;
                            }

                            if (!Host.CommonModule.MoveTo(-9512.12, -136.83, 59.79))
                            {
                                return;
                            }

                            var npc = Host.GetNpcById(12423);
                            if (npc != null)
                            {
                                Host.TurnDirectly(npc.Location);
                                Thread.Sleep(2000);
                                Host.SpellManager.CastSpell(spell1.Id, npc);
                                Thread.Sleep(4000);
                                Host.SpellManager.CastSpell(spell2.Id, npc);
                                Thread.Sleep(3000);
                            }

                            return;
                        }

                    case 5648:
                        {
                            var spell1 = Host.SpellManager.GetSpell(2052);
                            var spell2 = Host.SpellManager.GetSpell(1243);
                            if (spell1 == null || spell2 == null)
                            {
                                Host.log("Необходимо выучить скилы 2052 или 1243");
                                return;
                            }

                            if (!Host.CommonModule.MoveTo(173.45, -4765.16, 14.22))
                            {
                                return;
                            }

                            var npc = Host.GetNpcById(12430);
                            if (npc != null)
                            {
                                Host.TurnDirectly(npc.Location);
                                Thread.Sleep(2000);
                                Host.SpellManager.CastSpell(spell1.Id, npc);
                                Thread.Sleep(4000);
                                Host.SpellManager.CastSpell(spell2.Id, npc);
                                Thread.Sleep(3000);
                            }

                            return;
                        }

                    case 6395 when Host.MyGetItem(16333) != null:
                        {
                            if (!Host.CommonModule.MoveTo(1878.95, 1624.73, 94.54, 2))
                            {
                                return;
                            }

                            var go = Host.GetNpcById(178090);
                            var item = Host.MyGetItem(16333);
                            Host.MyUseItemAndWait(item, go);
                            Thread.Sleep(2000);
                            return;
                        }

                    case 6395 when Host.MyGetItem(16333) == null:
                        {
                            if (!Host.CommonModule.MoveTo(1992.75, 1376.44, 62.50, 2))
                            {
                                return;
                            }

                            var npc = Host.GetUnitById(1919);
                            if (npc != null)
                            {
                                Host.FarmModule.BestMob = npc;
                            }
                            Thread.Sleep(5000);
                            return;
                        }
                }


                var questObjective = new QuestObjective();
                foreach (var templateQuestObjective in quest.Template.QuestObjectives)
                {
                    if (templateQuestObjective.StorageIndex == index)
                    {
                        questObjective = templateQuestObjective;
                    }
                }


                Host.log("Type:" + questObjective.Type
                                 + " Amount: " + questObjective.Amount
                                 + " ObjectID: " + questObjective.ObjectID
                                 + " Flags: " + questObjective.Flags
                                 + " Flags2: " + questObjective.Flags2
                                 + " Description: " + questObjective.Description
                                 + " StartItem: " + quest.Template.StartItem
                    , LogLvl.Important);

                foreach (var questPoi in quest.GetQuestPOI())
                {
                    Host.log("ObjectiveIndex: " + questPoi.ObjectiveIndex
                                                + " Flags " + questPoi.Flags
                                                + " BlobIndex: " + questPoi.BlobIndex
                                                + " MapId: " + questPoi.MapId
                                                + " PlayerConditionId: " + questPoi.PlayerConditionId
                                                + " Priority: " + questPoi.Priority
                                                + " WorldEffectId: " + questPoi.WorldEffectId
                                                + " WorldMapAreaId: " + questPoi.WorldMapAreaId
                                                + " QuestObjectId: " + questPoi.QuestObjectId
                                                + " QuestObjectiveId: " + questPoi.QuestObjectiveId);
                    foreach (var questPoiPoint in questPoi.Points)
                    {
                        Host.log(questPoiPoint.X + ", " + questPoiPoint.Y + ", " + Host.GetNavMeshHeight(new Vector3F(questPoiPoint.X, questPoiPoint.Y, Host.Me.Location.Z)));
                        // Host.CommonModule.MoveTo(questPoiPoint.X, questPoiPoint.Y, Host.GetNavMeshHeight(new Vector3F(questPoiPoint.X, questPoiPoint.Y, Host.Me.Location.Z)));
                    }
                }


                List<ZonePoint> zonePoints = new List<ZonePoint>();

                foreach (var questPoi in quest.GetQuestPOI())
                {
                    if (questPoi.ObjectiveIndex != index)
                    {
                        continue;
                    }

                    foreach (var questPoiPoint in questPoi.Points)
                    {
                        zonePoints.Add(new ZonePoint(questPoiPoint.X, questPoiPoint.Y));
                    }
                }

                Zone poly = new RectangleZone(0, 0, 0, 0);
                var farmMobIds = new List<uint>();
                var keyInfoRun = new Tuple<uint, int>(myQuest.Id, index);

                //  Host.log("Ищу " + keyInfoRun.Item1 + " " + keyInfoRun.Item2);

                if (MyQuestHelps.MyQuestInfosRun.ContainsKey(keyInfoRun) || MyQuestHelps.MyQuestInfosRunCustom.ContainsKey(keyInfoRun))
                {
                    //  Host.log("Нашел файл " + myQuest.Id);
                    if (MyQuestHelps.MyQuestInfosRunCustom.ContainsKey(keyInfoRun))
                    {
                        var myQuestInfoRun = MyQuestHelps.MyQuestInfosRunCustom[keyInfoRun];
                        poly = myQuestInfoRun.Zone;
                        farmMobIds = myQuestInfoRun.FarmIds;
                        Host.DrawZones(new List<Zone> { poly });
                    }
                    else
                    {
                        var myQuestInfoRun = MyQuestHelps.MyQuestInfosRun[keyInfoRun];
                        poly = myQuestInfoRun.Zone;
                        farmMobIds = myQuestInfoRun.FarmIds;
                        Host.DrawZones(new List<Zone> { poly });
                    }
                }
                else
                {
                    if (zonePoints.Count == 1)
                    {
                        poly = new RoundZone(zonePoints[0].X, zonePoints[0].Y, 20);
                        Host.DrawZones(new List<Zone> { poly });
                    }
                    else
                    {
                        if (zonePoints.Count > 2)
                        {
                            poly = new PolygonZone(zonePoints);

                            Host.DrawZones(new List<Zone> { poly });
                        }
                    }
                }

                if (myQuest.Id == 3741)
                {
                    poly = new PolygonZone(new List<ZonePoint>
                    {
                        new ZonePoint(-9304.15, -1947.66),
                        new ZonePoint(-9267.76, -2126.48),
                        new ZonePoint(-9298.37, -2379.79),
                        new ZonePoint(-9404.88, -2405.44),
                        new ZonePoint(-9418.85, -2142.98),
                        new ZonePoint(-9343.99, -1942.44),

                    });
                    Host.DrawZones(new List<Zone> { poly });
                }

                if (myQuest.Id == 816)
                {
                    poly = new PolygonZone(new List<ZonePoint>
                    {
                        new ZonePoint(1238.35, -3951.97),
                        new ZonePoint(1236.31, -3900.48),
                        new ZonePoint(1161.67, -3892.37),
                        new ZonePoint(998.51, -3792.25),
                        new ZonePoint(731.37, -3760.16),
                        new ZonePoint(724.88, -3806.51),
                        new ZonePoint(926.44, -3835.07),
                        new ZonePoint(1004.57, -3899.08)
                    });
                    Host.DrawZones(new List<Zone> { poly });
                }


                if (questObjective.Type == EQuestRequirementType.GameObject)
                {
                    farmMobIds.Add(Convert.ToUInt32(questObjective.ObjectID));
                    QuestType = ExecuteType.ItemGatherFromGameObject;
                }


                if (questObjective.Type == EQuestRequirementType.Item)
                {
                    var sw = new Stopwatch();
                    if (Host.AdvancedLog)
                    {
                        sw.Start();
                    }

                    foreach (var dropBase in Host.DropBases.Drop)
                    {
                        if (questObjective.ObjectID == dropBase.ItemId)
                        {
                            if (dropBase.Type == "object")
                            {
                                QuestType = ExecuteType.ItemGatherFromGameObject;
                            }

                            if (dropBase.Type == "npc")
                            {
                                switch (myQuest.Id)
                                {
                                    case 5481:
                                        QuestType = ExecuteType.ItemGatherFromGameObject;
                                        break;

                                    case 315:
                                        break;
                                    default:
                                        QuestType = ExecuteType.ItemGatherFromMonster;
                                        break;
                                }
                            }

                            foreach (var u in dropBase.MobsId)
                            {
                                if (u != 0)
                                {
                                    if (!farmMobIds.Contains(u))
                                    {
                                        farmMobIds.Add(u);
                                    }
                                }
                            }
                        }
                    }
                    if (myQuest.Id == 5482 || myQuest.Id == 4681)
                    {
                        QuestType = ExecuteType.ItemGatherFromGameObject;
                    }

                    if (myQuest.Id == 2459 && myQuest.Index == 1)
                    {
                        farmMobIds.Add(7234);
                        QuestType = ExecuteType.ItemGatherFromMonster;
                    }
                    if (Host.AdvancedLog)
                    {
                        if (sw.ElapsedMilliseconds > 0)
                        {
                            Host.log("DropBases 2 за  " + sw.ElapsedMilliseconds + " мс " + " всего предметов " + Host.DropBases.Drop.Count + " " + questObjective.ObjectID);
                        }

                        sw.Stop();
                    }
                }

                if (questObjective.Type == EQuestRequirementType.Monster && questObjective.Description == "")
                {
                    foreach (var myNpcLoc in Host.MyNpcLocss.NpcLocs)
                    {
                        if (myNpcLoc.Id == questObjective.ObjectID)
                        {
                            if (!farmMobIds.Contains(myNpcLoc.Id))
                            {
                                farmMobIds.Add(myNpcLoc.Id);
                            }
                        }
                    }

                    if (!farmMobIds.Contains(Convert.ToUInt32(questObjective.ObjectID)))
                    {
                        farmMobIds.Add(Convert.ToUInt32(questObjective.ObjectID));
                    }
                }

                double z = 0;
                if (poly.ZoneType == EZoneType.Rectangle && farmMobIds.Count > 0)
                {
                    Host.log("Не найдена зона пытаюсь построить");
                    List<Vector3F> dots = new List<Vector3F>();
                    foreach (var farmMobId in farmMobIds)
                    {
                        Host.log("Обьект: " + farmMobId);
                        if (questObjective.Type != EQuestRequirementType.GameObject)
                        {
                            var npcLoc = Host.MyGetLocNpcById(farmMobId);
                            if (npcLoc != null)
                            {
                                foreach (var vector3F in npcLoc.ListLoc)
                                {
                                    dots.Add(vector3F);
                                    z = vector3F.Z;
                                }
                            }
                        }

                        if (questObjective.Type != EQuestRequirementType.Monster)
                        {
                            var goLoc = Host.MyGetLocGameOjectById(farmMobId);
                            if (goLoc != null)
                            {
                                foreach (var vector3F in goLoc.ListLoc)
                                {
                                    dots.Add(vector3F);
                                    z = vector3F.Z;
                                }
                            }
                        }
                    }

                    Host.log("Всего точек " + dots.Count);
                    int totalX = 0, totalY = 0;
                    foreach (var p in dots)
                    {
                        totalX += Convert.ToInt32(p.X);
                        totalY += Convert.ToInt32(p.Y);
                    }

                    int centerX = totalX / dots.Count;
                    int centerY = totalY / dots.Count;
                    double maxDist = 0;

                    foreach (var vector3F in dots)
                    {
                        if (Host.MyDistanceNoZ(vector3F.X, vector3F.Y, centerX, centerY) > maxDist)
                        {
                            maxDist = Host.MyDistanceNoZ(vector3F.X, vector3F.Y, centerX, centerY);
                        }
                    }

                    poly = new RoundZone(centerX, centerY, 50);
                    if (maxDist > 50)
                    {
                        poly = new RoundZone(centerX, centerY, maxDist);
                    }

                    Host.DrawZones(new List<Zone> { poly });
                }

                if (quest.Id == 76)
                {
                    if (!Host.CommonModule.MoveTo(-9091, -566, 58))
                    {
                        return;
                    }

                    return;
                }

                switch (poly.ZoneType)
                {
                    case EZoneType.Circle:
                        {
                            if (!poly.ObjInZone(Host.Me))
                            {
                                Vector2F randPoint = new Vector2F(((RoundZone)poly).X, ((RoundZone)poly).Y);

                                // ReSharper disable once CompareOfFloatsByEqualityOperator
                                if (z == 0)
                                {
                                    if (questObjective.Type != EQuestRequirementType.GameObject)
                                    {
                                        foreach (var farmMobId in farmMobIds)
                                        {
                                            var npcLoc = Host.MyGetLocNpcById(farmMobId);
                                            if (npcLoc == null)
                                            {
                                                Host.log("Не нашел координаты моба " + farmMobId);
                                                continue;
                                            }


                                            foreach (var vector3F in npcLoc.ListLoc.OrderBy(i => new Vector3F(((RoundZone)poly).X, ((RoundZone)poly).Y, 0).Distance2D(i)))
                                            {
                                                if (double.IsNaN(vector3F.X))
                                                {
                                                    continue;
                                                }

                                                z = vector3F.Z;
                                                if (Host.AdvancedLog)
                                                {
                                                    Host.log("Нашел Z у ближайшего моба " + z + "  " + Host.IsInsideNavMesh(new Vector3F(randPoint.X, randPoint.Y, z)));
                                                }

                                                break;
                                            }
                                        }
                                        if (QuestType == ExecuteType.ItemGatherFromGameObject)
                                        {
                                            foreach (var farmMobId in farmMobIds)
                                            {
                                                var npcLoc = Host.MyGetLocGameOjectById(farmMobId);
                                                if (npcLoc == null)
                                                {
                                                    continue;
                                                }

                                                foreach (var vector3F in npcLoc.ListLoc.OrderBy(i => new Vector3F(((RoundZone)poly).X, ((RoundZone)poly).Y, 0).Distance2D(i)))
                                                {
                                                    if (double.IsNaN(vector3F.X))
                                                    {
                                                        continue;
                                                    }

                                                    if (Host.AdvancedLog)
                                                    {
                                                        Host.log("Нашел Z у ближайшего GO");
                                                    }

                                                    z = vector3F.Z;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {

                                        foreach (var farmMobId in farmMobIds)
                                        {
                                            var npcLoc = Host.MyGetLocGameOjectById(farmMobId);
                                            if (npcLoc == null)
                                            {
                                                continue;
                                            }

                                            foreach (var vector3F in npcLoc.ListLoc.OrderBy(i => new Vector3F(((RoundZone)poly).X, ((RoundZone)poly).Y, 0).Distance2D(i)))
                                            {
                                                if (double.IsNaN(vector3F.X))
                                                {
                                                    continue;
                                                }

                                                if (Host.AdvancedLog)
                                                {
                                                    Host.log("Нашел Z у ближайшего GO");
                                                }

                                                z = vector3F.Z;
                                                break;
                                            }
                                        }
                                    }
                                }

                                /*  if(z != 0 && !Host.IsInsideNavMesh(new Vector3F(randPoint.X, randPoint.Y, z)))
                                      z = Host.GetNavMeshHeight(new Vector3F(randPoint.X, randPoint.Y, z));*/
                                if (myQuest.Id == 418 && myQuest.Index == 0)
                                {
                                    z = 320;
                                }

                                Host.log("Бегу в зону " + randPoint + ", " + z);
                                // ReSharper disable once CompareOfFloatsByEqualityOperator
                                if (z == 0)
                                {
                                    z = Host.GetNavMeshHeight(new Vector3F(randPoint.X, randPoint.Y, Host.Me.Location.Z));
                                    if (Host.AdvancedLog)
                                    {
                                        Host.log("Нашел Z у ближайшего меша");
                                    }
                                }

                                // ReSharper disable once CompareOfFloatsByEqualityOperator
                                if (z == 0)
                                {
                                    Host.log("Не найден маршрут " + questObjective.Type + " ");
                                    return;
                                }
                                Host.CommonModule.MoveTo((poly as RoundZone).X, (poly as RoundZone).Y, z, 10);
                                return;
                            }
                        }

                        break;
                    case EZoneType.Rectangle:
                        {
                            Host.log("Не найдена зона");
                            Thread.Sleep(10000);
                            return;
                        }

                    case EZoneType.Polygon:
                        {
                            if (!poly.ObjInZone(Host.Me))
                            {
                                var randPoint = poly.GetRandomPoint();


                                Host.log("Бегу в зону " + randPoint);
                                Host.CommonModule.MoveTo(randPoint.X, randPoint.Y,
                                    Host.GetNavMeshHeight(new Vector3F(randPoint.X, randPoint.Y, Host.Me.Location.Z)));
                                return;
                            }
                        }
                        break;
                    default:
                        {
                            Host.log("Зона не известна " + poly.Type);
                        }
                        break;
                }


                if (questObjective.Type == EQuestRequirementType.AreaTrigger)
                {
                    if (quest.Id == 62)
                    {
                        if (!Host.CommonModule.MoveTo(-9838.65, 126.71, 4.97))
                        {
                            return;
                        }

                        return;
                    }

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (z == 0)
                    {
                        z = Host.GetNavMeshHeight(new Vector3F((poly as RoundZone).X, (poly as RoundZone).Y,
                            Host.Me.Location.Z));
                    }

                    Host.CommonModule.MoveTo((poly as RoundZone).X, (poly as RoundZone).Y, z, 0);
                    return;
                }

                if (questObjective.Type == EQuestRequirementType.Item && quest.Template.StartItem == 0 || questObjective.Type == EQuestRequirementType.GameObject)
                {
                    if (farmMobIds.Count == 0)
                    {
                        if (myQuest.Id == 384 && myQuest.Index == 1)
                        {
                            if (!Host.CommonModule.MoveTo(-5598.68, -529.60, 399.65))
                            {
                                return;
                            }

                            var npc = Host.GetNpcById(1247);
                            if (npc != null)
                            {
                                Host.CommonModule.MoveTo(npc, 1);
                            }

                            Host.MyOpenDialog(npc);
                            foreach (var gossipOptionsData in Host.GetNpcDialogs())
                            {
                                if (gossipOptionsData.OptionNPC == EGossipOptionIcon.Vendor)
                                {
                                    Host.SelectNpcDialog(gossipOptionsData);
                                }
                            }

                            foreach (var item in Host.GetVendorItems())
                            {
                                if (item.ItemId == 2894)
                                {
                                    var res = item.Buy(1);
                                    if (res != EBuyResult.Success)
                                    {
                                        Host.log("Не удалось купить " + res);
                                    }

                                    Thread.Sleep(2000);
                                    return;
                                }
                            }
                        }

                        if (myQuest.Id == 375 && myQuest.Index == 1)
                        {
                            if (!Host.CommonModule.MoveTo(2258.44, 276.81, 34.52))
                            {
                                return;
                            }

                            var npc = Host.GetNpcById(2118);
                            if (npc != null)
                            {
                                Host.CommonModule.MoveTo(npc, 1);
                            }

                            Host.OpenShop(npc as Unit);
                            //Host.MyOpenDialog(npc);
                            //foreach (var gossipOptionsData in Host.GetNpcDialogs())
                            //{
                            //    if (gossipOptionsData.OptionNPC == EGossipOptionIcon.Vendor)
                            //        Host.SelectNpcDialog(gossipOptionsData);
                            //}

                            foreach (var item in Host.GetVendorItems())
                            {
                                if (item.ItemId == 2320)
                                {
                                    var res = item.Buy(1);
                                    if (res != EBuyResult.Success)
                                    {
                                        Host.log("Не удалось купить " + res);
                                    }

                                    Thread.Sleep(2000);
                                    return;
                                }
                            }
                        }

                        Host.log("Не найден НПС " + questObjective.ObjectID);
                        return;
                    }

                    if (quest.Id == 3524)
                    {
                        QuestType = ExecuteType.ItemGatherFromGameObject;
                    }

                    if (quest.Id == 315)
                    {
                        QuestType = ExecuteType.ItemGatherFromGameObject;
                        farmMobIds.Add(276);
                    }


                    if (QuestType == ExecuteType.ItemGatherFromMonster)
                    {
                        MyQuestHelps.MonsterHuntClassic(quest, poly, farmMobIds, index, Host, z);
                        return;
                    }


                    if (QuestType == ExecuteType.ItemGatherFromGameObject)
                    {
                        MyQuestHelps.ItemGatherFromGameObjectClassic(quest, poly, farmMobIds, index, Host, z);
                        return;
                    }

                    MyQuestHelps.MonsterHuntClassic(quest, poly, farmMobIds, index, Host, z);
                    return;
                }

                if (questObjective.Type == EQuestRequirementType.Monster && questObjective.Description.Contains("Speak"))
                {
                    var npc = Host.GetNpcById(Convert.ToUInt32(questObjective.ObjectID));
                    if (npc == null)
                    {
                        Host.log("Не найден НПС для квеста ");
                        return;
                    }

                    Host.MyDialog(npc);
                    return;
                }

                if (questObjective.Type == EQuestRequirementType.Monster && questObjective.Description.Contains("Find"))
                {
                    var npc = Host.GetNpcById(Convert.ToUInt32(questObjective.ObjectID));
                    if (npc == null)
                    {
                        Host.log("Не найден НПС для квеста ");
                        return;
                    }

                    Host.MyDialog(npc);
                    return;
                }

                if (questObjective.Type == EQuestRequirementType.Monster && questObjective.Description == "")
                {
                    if (farmMobIds.Count == 0)
                    {
                        Host.log("Не найден НПС " + questObjective.ObjectID);
                        return;
                    }

                    MyQuestHelps.MonsterHuntClassic(quest, poly, farmMobIds, index, Host, z);
                    return;
                }

                if (questObjective.Type == EQuestRequirementType.Monster && quest.Template.StartItem != 0 || questObjective.Type == EQuestRequirementType.Item && quest.Template.StartItem != 0)
                {
                    Host.log("Использовать предмет на моба");
                    if (quest.Id == 4812)
                    {
                        if (!Host.CommonModule.MoveTo(6410.06, 466.36, 8.12))
                        {
                            return;
                        }
                    }

                    if (quest.Id == 4762)
                    {
                        if (!Host.CommonModule.MoveTo(7236.56, -382.78, -1.52))
                        {
                            return;
                        }

                        Host.CommonModule.ForceMoveTo2(new Vector3F(7224.04, -386.01, -1.52));
                    }

                    var npc = Host.GetUnitByIdInZone(Convert.ToUInt32(questObjective.ObjectID), false, true, poly);

                    foreach (var farmMobId in farmMobIds)
                    {
                        if (npc == null)
                        {
                            npc = Host.GetUnitByIdInZone(farmMobId, false, true, poly);
                        }
                    }

                    if (npc == null)
                    {
                        npc = Host.GetUnitByIdInZone(Convert.ToUInt32(questObjective.ObjectID), false, false, poly);
                    }

                    var item = Host.MyGetItem(quest.Template.StartItem);
                    float range = 2;

                    if (quest.Template.StartItem == 7586)
                    {
                        if (item == null)
                        {
                            Host.CancelQuest(2118);
                            FillQuestClassic();
                            return;
                        }
                    }

                    if (Host.GameDB.SpellInfoEntries.ContainsKey(item.SpellId))
                    {
                        var spell = Host.GameDB.SpellInfoEntries[item.SpellId];
                        Host.log(spell.SpellName + " " + spell.Id + " " + spell.RangeEntry.MaxRangeFriend + " " + spell.RangeEntry.MaxRangeHostile + " " + spell.RangeEntry.MinRangeFriend + " " +
                                 spell.RangeEntry.MinRangeHostile);
                        range = spell.RangeEntry.MaxRangeFriend - 1;
                    }



                    if (npc != null)
                    {
                        Host.FarmModule.FarmState = FarmState.Disabled;
                        Host.CommonModule.MoveTo(npc, range);
                        Host.MyUseItemAndWait(Host.MyGetItem(quest.Template.StartItem), npc);
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        Host.FarmModule.SetBadTarget(npc, 30000);
                    }
                    else
                    {
                        Host.MyUseItemAndWait(Host.MyGetItem(quest.Template.StartItem));

                        if (quest.Id == 4762)
                        {
                            Host.CommonModule.ForceMoveTo2(new Vector3F(7236.56, -382.78, -1.52));
                        }

                        var loc = Host.MyGetLocNpcById(Convert.ToUInt32(questObjective.ObjectID));
                        foreach (var farmMobId in farmMobIds)
                        {
                            if (loc == null)
                            {
                                Host.log("Не нашел НПС 2  " + farmMobId);
                                loc = Host.MyGetLocNpcById(farmMobId);
                            }
                        }

                        Host.log("Не нашел НПС " + questObjective.ObjectID);
                        if (loc != null)
                        {
                            var randloc = new List<Vector3F>();
                            foreach (var vector3F in loc.ListLoc)
                            {
                                if (!poly.PointInZone(vector3F.X, vector3F.Y))
                                {
                                    continue;
                                }

                                randloc.Add(vector3F);
                            }

                            if (randloc.Count > 0)
                            {
                                Host.CommonModule.MoveTo(randloc[Host.RandGenerator.Next(0, randloc.Count)], 1);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        private bool RecivePet()
        {
            if (Host.Me.Class == EClass.Hunter)
            {
                if (Host.Me.Race == ERace.Troll || Host.Me.Race == ERace.Orc)
                {
                    /* if (Host.Me.GetPet() == null)
                         Host.CallPet();*/
                    if (Host.PetTameFailureReason == EPetTameFailureReason.NoPetAvailable && Host.Me.GetPet() == null && Host.SpellManager.GetSpell(6991) != null)
                    {
                        Host.log("Нет пета, надо приручить");
                        if (!Host.CommonModule.MoveTo(1183.47, -4377.72, 26.31, 10))
                        {
                            return false;
                        }

                        RoundZone zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 200);
                        var npc = Host.GetUnitByIdInZone(3100, false, true, zone);
                        if (npc != null)
                        {
                            Host.log("Выбрал " + npc.Id);
                            Host.FarmModule.FarmState = FarmState.Disabled;
                            Host.CommonModule.MoveTo(npc, 25);
                            Thread.Sleep(2000);
                            Host.SpellManager.CastSpell(1515, npc);
                            Thread.Sleep(2000);
                            while (Host.SpellManager.IsCasting)
                            {
                                Thread.Sleep(1000);
                            }
                            while (Host.SpellManager.IsChanneling)
                            {
                                Thread.Sleep(1000);
                            }

                            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                            Host.CallPet();
                        }
                        return false;
                    }

                    if (Host.Me.Level > 9 && Host.SpellManager.GetSpell(6991) == null)
                    {
                        var hunterQuest = new List<MyQuest>
                            {
                                new  MyQuest(6062, QuestAction.Apply, ERace.None, EClass.None),
                                new  MyQuest(6062, QuestAction.Run, ERace.None, EClass.None, 0),
                                new  MyQuest(6062, QuestAction.Complete, ERace.None,EClass.None),
                                new  MyQuest(6083, QuestAction.Apply, ERace.None, EClass.None),
                                new  MyQuest(6083, QuestAction.Run, ERace.None, EClass.None, 0),
                                new  MyQuest(6083, QuestAction.Complete, ERace.None,EClass.None),
                                new  MyQuest(6082, QuestAction.Apply, ERace.None, EClass.None),
                                new  MyQuest(6082, QuestAction.Run, ERace.None, EClass.None, 0),
                                new  MyQuest(6082, QuestAction.Complete, ERace.None,EClass.None),
                                new  MyQuest(6081, QuestAction.Apply, ERace.None, EClass.None),
                                new  MyQuest(6081, QuestAction.Complete, ERace.None,EClass.None)
                            };

                        for (var index = 0; index < hunterQuest.Count; index++)
                        {
                            var myQuest = hunterQuest[index];


                            if (Host.CheckQuestCompleted(myQuest.Id) || Host.QuestStates.QuestState.Contains(myQuest.Id))
                            {
                                hunterQuest.RemoveAt(index);
                                index -= 1;
                                continue;
                            }

                            if (myQuest.QuestAction == QuestAction.Apply && Host.GetQuest(myQuest.Id) != null)
                            {
                                hunterQuest.RemoveAt(index);
                                index -= 1;
                                continue;
                            }

                            if (myQuest.QuestAction == QuestAction.Run && Host.GetQuest(myQuest.Id) != null && IsQuestCompliteClassic(myQuest.Id, myQuest.Index))
                            {
                                hunterQuest.RemoveAt(index);
                                index -= 1;
                            }
                        }

                        foreach (var myQuest in hunterQuest)
                        {
                            if (myQuest.QuestAction == QuestAction.Apply)
                            {
                                ApplyQuestClassic(myQuest);
                            }

                            if (myQuest.QuestAction == QuestAction.Run)
                            {
                                RunQuestClassic(myQuest);
                            }

                            if (myQuest.QuestAction == QuestAction.Complete)
                            {
                                ComliteQuestClassic(myQuest);
                            }

                            return false;
                        }
                        return false;
                    }

                    /* if (Host.Me.GetPet() == null)
                         return false;*/

                }
            }

            return true;
        }

        private bool needLearn = false;

        private void QuestingClassic()
        {
            try
            {
                if (Host.Me.Level >= 18 && Host.Me.Level <= 19 && Host.Me.Team == ETeam.Alliance)
                {


                    switch (Host.Me.Class)
                    {
                        case EClass.None:
                            break;
                        case EClass.Warrior:
                            break;
                        case EClass.Paladin:
                            break;
                        case EClass.Hunter:
                            break;
                        case EClass.Rogue:
                            break;
                        case EClass.Priest:
                            {
                                if (Host.Area.Id == 38 || Host.Area.Id == 1537 || needLearn)//Гномы
                                {
                                    if (Host.SpellManager.GetSpell(970) == null)
                                    {
                                        needLearn = true;
                                        Host.FarmModule.FarmState = FarmState.Disabled;
                                        if (Host.Area.Id != 1537)
                                            Host.MyUseTaxi(1537, new Vector3F(-4615.31, -916.47, 501.06));
                                        Host.CommonModule.MoveTo(-4615.31, -916.47, 501.06);
                                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                        return;
                                    }
                                    else
                                    {
                                        needLearn = false;
                                    }
                                }

                                if (Host.Area.Id == 44 || Host.Area.Id == 1519 || needLearn)//шторм
                                {
                                    if (Host.SpellManager.GetSpell(970) == null)
                                    {
                                        needLearn = true;
                                        Host.FarmModule.FarmState = FarmState.Disabled;
                                        if (Host.Area.Id != 1519)
                                            Host.MyUseTaxi(1519, new Vector3F(-8518.75, 861.26, 109.84));
                                        Host.CommonModule.MoveTo(-8518.75, 861.26, 109.84);
                                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                        return;
                                    }
                                    else
                                    {
                                        needLearn = false;
                                    }
                                }

                            }
                            break;
                        case EClass.DeathKnight:
                            break;
                        case EClass.Shaman:
                            break;
                        case EClass.Mage:
                            break;
                        case EClass.Warlock:
                            break;
                        case EClass.Monk:
                            break;
                        case EClass.Druid:
                            {
                                if (Host.Area.Id == 44 || Host.Area.Id == 1519 || needLearn)//шторм
                                {
                                    if (Host.SpellManager.GetSpell(8925) == null)
                                    {
                                        needLearn = true;
                                        Host.FarmModule.FarmState = FarmState.Disabled;
                                        if (Host.Area.Id != 1519)
                                            Host.MyUseTaxi(1519, new Vector3F(-8774.69, 1095.24, 92.54));
                                        Host.CommonModule.MoveTo(-8774.69, 1095.24, 92.54);
                                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                        return;
                                    }
                                    else
                                    {
                                        needLearn = false;
                                    }
                                }
                            }
                            break;
                        case EClass.DemonHunter:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }


                }


                if (Host.CharacterSettings.StopQuesting && Host.Me.Level >= Host.CharacterSettings.StopQuestingLevel)
                {
                    Host.MainForm.On = false;
                    return;
                }

                if (!RecivePet())
                {
                    return;
                }

                for (var index = 0; index < ListQuestClassic.Count; index++)
                {

                    if (ListQuestClassic[index].Id == 5655)
                    {
                        ListQuestClassic[index].Id = 5654;
                    }

                    var listQuest = ListQuestClassic[index];

                    if (listQuest.Id == 746)//Дворфийские делишки
                    {
                        ListQuestClassic.RemoveAt(index);
                        index -= 1;
                        continue;
                    }

                    if (listQuest.Id == 754 || listQuest.Id == 756 || listQuest.Id == 759 || listQuest.Id == 760 || listQuest.Id == 758)//Очищение колодца Заиндевевшего Копыта
                    {
                        ListQuestClassic.RemoveAt(index);
                        index -= 1;
                        continue;
                    }



                    if (listQuest.QuestAction == QuestAction.UseFlyPath && index == 0)
                    {
                        Host.log(index + "  " + Host.CheckQuestCompleted(listQuest.Id) + " " + Host.QuestStates.QuestState.Contains(Convert.ToUInt32(listQuest.Id)));
                        if (Host.Me.Class == EClass.Warrior && listQuest.Id == 246)
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }

                        if (listQuest.Id == 436 && Host.MapID == 0 && Host.Area.Id == 38)
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }

                        if (listQuest.Id == 3524 && Host.MapID == 1 && Host.Area.Id == 148)
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }



                        if (Host.Me.Distance(listQuest.Loc) > 1000 && Host.GetQuest(listQuest.Id) != null || Host.CheckQuestCompleted(listQuest.Id) || Host.QuestStates.QuestState.Contains(Convert.ToUInt32(listQuest.Id)))
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                        }
                        continue;
                    }

                    if (listQuest.QuestAction == QuestAction.SetHs)
                    {
                        if (Host.BindPoint.Location.Distance2D(listQuest.Loc) < 10 || Host.CheckQuestCompleted(Convert.ToUInt32(listQuest.Level)) || Host.QuestStates.QuestState.Contains(Convert.ToUInt32(listQuest.Level)))
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                        }
                        continue;
                    }

                    if (listQuest.QuestAction == QuestAction.UseHs)
                    {
                        if ((Host.Me.Location.Distance2D(listQuest.Loc) > 100 && index == 0) || Host.SpellManager.GetItemCooldown(6948) != 0 || Host.CheckQuestCompleted(Convert.ToUInt32(listQuest.Level)) || Host.QuestStates.QuestState.Contains(Convert.ToUInt32(listQuest.Level)))
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                        }
                        continue;
                    }

                    if (listQuest.QuestAction == QuestAction.GetFp)
                    {
                        if (listQuest.Level == 436)
                        {
                            if (Host.Area.Id == 11 || Host.Area.Id == 38)
                            {
                                ListQuestClassic.RemoveAt(index);
                                index -= 1;
                                continue;
                            }

                        }

                        if (Host.CheckQuestCompleted(Convert.ToUInt32(listQuest.Level))
                            || Host.Me.Distance2D(listQuest.Loc) < 50
                            || ((listQuest.Level == 246 && Host.MapID == 1))
                            || Host.QuestStates.QuestState.Contains(Convert.ToUInt32(listQuest.Level))
                            || Host.MyTaxyNode.MyTaxyNodes.Any(i => i.Loc.Distance2D(listQuest.Loc) < 30)
                            )
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                        }
                        continue;
                    }


                    if (listQuest.QuestAction == QuestAction.Grind)
                    {
                        if (Host.Me.Level >= listQuest.Level)
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }
                    }

                    if (listQuest.QuestAction == QuestAction.Apply || listQuest.QuestAction == QuestAction.Complete || listQuest.QuestAction == QuestAction.Run)
                    {
                        if (Host.CharacterSettings.IgnoreQuests.Any(a => listQuest.Id == a.Id))
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }
                    }



                    if (Host.CheckQuestCompleted(listQuest.Id) || Host.QuestStates.QuestState.Contains(listQuest.Id))
                    {
                        ListQuestClassic.RemoveAt(index);
                        index -= 1;
                        continue;
                    }



                    if (!listQuest.Race.Contains(ERace.None))
                    {
                        if (!listQuest.Race.Contains(Host.Me.Race))
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }
                    }

                    if (!listQuest.Class.Contains(EClass.None))
                    {
                        if (!listQuest.Class.Contains(Host.Me.Class))
                        {
                            ListQuestClassic.RemoveAt(index);
                            index -= 1;
                            continue;
                        }
                    }



                    if (listQuest.QuestAction == QuestAction.Apply && Host.GetQuest(listQuest.Id) != null)
                    {
                        ListQuestClassic.RemoveAt(index);
                        index -= 1;
                        continue;
                    }

                    if (listQuest.QuestAction == QuestAction.Run && Host.GetQuest(listQuest.Id) != null && IsQuestCompliteClassic(listQuest.Id, listQuest.Index))
                    {
                        ListQuestClassic.RemoveAt(index);
                        index -= 1;
                    }
                }

                /*  Host.log("Всего действий " + ListQuestClassic.Count + " ");
                  if (Host.AdvancedLog)
                  {
                      for (var index = 0; index < ListQuestClassic.Count; index++)
                      {
                          var myQuest = ListQuestClassic[index];
                          Host.log(index + ")" + myQuest.Id + " " + myQuest.QuestAction + " " + myQuest.Index + "  " + Host.CheckQuestCompleted(myQuest.Id), LogLvl.Important);
                          if (index > 10)
                              break;
                      }
                  }*/

                for (var index = 0; index < ListQuestClassic.Count; index++)
                {
                    var listQuest = ListQuestClassic[index];

                    if (listQuest.QuestAction == QuestAction.Apply)
                    {
                        if (MyQuestHelps.QuestReq.ContainsKey(listQuest.Id))
                        {
                            if (MyQuestHelps.QuestReq[listQuest.Id].Item1 == EQuestReq.QuestCompleted)
                            {
                                if (Host.QuestStates.QuestState.Contains(MyQuestHelps.QuestReq[listQuest.Id].Item2))
                                {
                                    continue;
                                }
                            }

                            if (MyQuestHelps.QuestReq[listQuest.Id].Item1 == EQuestReq.NeedItem)
                            {
                                if (Host.MyGetItem(MyQuestHelps.QuestReq[listQuest.Id].Item2) == null)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    Host.log("Выполняю " + listQuest.Id + " " + listQuest.QuestAction + " " + listQuest.Index + " " + listQuest.Level + "  " + Host.Me.Distance(listQuest.Loc) + "  " + Host.Me.Location.Distance2D(listQuest.Loc), LogLvl.Important);
                    if (listQuest.QuestAction == QuestAction.GetFp)
                    {
                        GetFpClassic(listQuest);
                    }

                    if (listQuest.QuestAction == QuestAction.UseHs)
                    {
                        Host.MyUseStone();
                    }

                    if (listQuest.QuestAction == QuestAction.SetHs)
                    {
                        SetHsClassic(listQuest);
                    }

                    if (listQuest.QuestAction == QuestAction.UseFlyPath)
                    {
                        UseFlyPathQuestClassic(listQuest);
                    }

                    if (listQuest.QuestAction == QuestAction.Grind)
                    {
                        GrindQuestClassic(listQuest);
                    }

                    if (listQuest.QuestAction == QuestAction.Apply)
                    {
                        ApplyQuestClassic(listQuest);
                    }

                    if (listQuest.QuestAction == QuestAction.Run)
                    {
                        RunQuestClassic(listQuest);
                    }

                    if (listQuest.QuestAction == QuestAction.Complete)
                    {
                        if (listQuest.Id == 806)
                        {
                            if (GetQuestIndex(Host.GetQuest(listQuest.Id)) != -1)
                            {
                                RunQuestClassic(listQuest);
                                return;
                            }
                        }

                        ComliteQuestClassic(listQuest);
                    }

                    Thread.Sleep(1000);
                    return;
                }

                if (ListQuestClassic.Count == 0)
                {
                    Host.log("Нет квестов", LogLvl.Important);
                    Host.log("Отключаюсь", LogLvl.Important);
                    Host.MainForm.On = false;
                }
            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        private void GetFpClassic(MyQuest myQuest)
        {
            Host.log("Дистанция до FP " + Host.Me.Distance(myQuest.Loc) + "  " + myQuest.Level);

            if (myQuest.Level == 353)
            {
                if (!CheckPath())
                {
                    return;
                }
            }

            if (myQuest.Level == 436 && Host.Area.Id == 148)
            {
                if (Host.Me.Distance(6424.35, 817.08, 5.49) > 2)
                {
                    if (!Host.CommonModule.MoveTo(6424.35, 817.08, 5.49))
                    {
                        return;
                    }
                }



                var transport = Host.MyGetTransportById(176310);
                if (transport != null && !transport.IsMoving && Host.Me.Distance(transport) < 50)
                {
                    Host.CommonModule.ForceMoveTo2(new Vector3F(6409.37, 829.39, 6.10));
                    Thread.Sleep(30000);
                    while (Host.MapID == 1)
                    {
                        Host.log("Плыву " + Host.MapID);
                        if (!Host.MainForm.On)
                        {
                            return;
                        }

                        Thread.Sleep(1000);
                    }

                    while (Host.GameState != EGameState.Ingame)
                    {
                        Thread.Sleep(1000);
                        if (!Host.MainForm.On)
                        {
                            return;
                        }
                    }

                    transport = Host.MyGetTransportById(176310);
                    while (transport.IsMoving)
                    {
                        Host.log("Еду " + transport.IsMoving);
                        if (!Host.MainForm.On)
                        {
                            return;
                        }

                        Thread.Sleep(1000);
                    }
                    Host.log("Доплыл");
                    Host.CommonModule.ForceMoveTo2(new Vector3F(-3725.61, -584.17, 6.26));

                }
                else
                {
                    Thread.Sleep(1000);
                    Host.log("Нет транспорта");
                }


                return;

            }


            if (Host.Me.Distance2D(myQuest.Loc) > 20)
            {
                Host.FarmModule.FarmState = FarmState.Disabled;
                foreach (var myNpcLoc in Host.MyNpcLocss.NpcLocs)
                {
                    if (!myNpcLoc.IsTaxy)
                    {
                        continue;
                    }

                    foreach (var vector3F in myNpcLoc.ListLoc)
                    {
                        if (vector3F.Distance2D(myQuest.Loc) < 10)
                        {
                            myQuest.Loc.Z = vector3F.Z;
                        }
                    }
                }

                if (myQuest.Loc.Z == 0)
                {
                    myQuest.Loc.Z = Convert.ToSingle(Host.GetNavMeshHeight(myQuest.Loc));
                }

                if (myQuest.Loc.Distance2D(new Vector3F(-8835.70, 489.70, 77.90)) < 50)
                {
                    myQuest.Loc.Z = 108;
                }

                if (!Host.CommonModule.MoveTo(myQuest.Loc, 1))
                {
                    return;
                }

                Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
            }
        }

        private void SetHsClassic(MyQuest myQuest)
        {
            if (!(myQuest.Loc.Distance2D(Host.BindPoint.Location) > 200))
            {
                return;
            }

            Host.log("Нужно зарегистрировать точку для камня Dist:" + myQuest.Loc.Distance2D(Host.BindPoint.Location) + "  " + Host.BindPoint.MapID + " " + Host.MapID + "  " + myQuest.Level);

            myQuest.Loc.Z = Convert.ToSingle(Host.GetNavMeshHeight(myQuest.Loc));
            if (!Host.CommonModule.MoveTo(myQuest.Loc, 10))
            {
                return;
            }

            Unit npc = null;
            foreach (var entity in Host.GetEntities<Unit>())
            {
                if (!entity.IsInnkeeper)
                {
                    continue;
                }

                npc = entity;
            }

            if (npc == null)
            {
                return;
            }

            if (Host.Me.Distance(npc) > 2)
            {
                Host.CommonModule.MoveTo(npc, 4);
            }

            Host.MyCheckIsMovingIsCasting();
            Host.MyOpenDialog(npc);
            foreach (var gossipOptionsData in Host.GetNpcDialogs())
            {
                Host.log(gossipOptionsData.OptionNPC + " " + gossipOptionsData.Text);
                if (gossipOptionsData.OptionNPC != EGossipOptionIcon.Interact2)
                {
                    continue;
                }

                Host.SelectNpcDialog(gossipOptionsData);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }

        private void UseFlyPathQuestClassic(MyQuest myQuest)
        {

            if (myQuest.Id == 3524)
            {
                if (Host.Area.Id == 1657)
                {
                    if (Host.Me.Distance(8784.10, 966.53, 30.21) > 300)
                    {
                        if (Host.Area.Id == 141 || Host.Area.Id == 1657)
                        {
                            if (Host.Area.Id == 1657)//добежать до дерева
                            {
                                if (!Host.CommonModule.MoveTo(9944.84, 2608.60, 1316.28))
                                {
                                    return;
                                }

                                if (!Host.CommonModule.MoveTo(9948.55, 2642.74, 1316.87))
                                {
                                    return;
                                }

                                Thread.Sleep(2000);
                                return;
                            }


                            /*  if (Host.Me.Distance(8381.02, 987.79, 29.61) < 50)
                              {
                                  Host.MyUseTaxi(148, new Vector3F((float)7459.90, (float)-326.56, (float)8.09));
                              }
                              else
                              {
                                  Host.CommonModule.MoveTo(new Vector3F(9944.84, 2608.60, 1316.28), 1);

                                  continue;
                              }*/
                            return;
                        }
                    }
                    return;
                }
            }

            if (myQuest.Id == 436)
            {
                if (Host.Area.Id == 148)
                {


                    if (Host.Me.Distance(6424.35, 817.08, 5.49) > 2)
                    {
                        if (!Host.CommonModule.MoveTo(6424.35, 817.08, 5.49))
                        {
                            return;
                        }
                    }



                    var transport = Host.MyGetTransportById(176310);
                    if (transport != null && !transport.IsMoving && Host.Me.Distance(transport) < 50)
                    {
                        Host.CommonModule.ForceMoveTo2(new Vector3F(6409.37, 829.39, 6.10));
                        Thread.Sleep(30000);
                        while (Host.MapID == 1)
                        {
                            Host.log("Плыву " + Host.MapID);
                            if (!Host.MainForm.On)
                            {
                                return;
                            }

                            Thread.Sleep(1000);
                        }

                        while (Host.GameState != EGameState.Ingame)
                        {
                            Thread.Sleep(1000);
                            if (!Host.MainForm.On)
                            {
                                return;
                            }
                        }

                        transport = Host.MyGetTransportById(176310);
                        while (transport.IsMoving)
                        {
                            Host.log("Еду " + transport.IsMoving);
                            if (!Host.MainForm.On)
                            {
                                return;
                            }

                            Thread.Sleep(1000);
                        }
                        Host.log("Доплыл");
                        Host.CommonModule.ForceMoveTo2(new Vector3F(-3725.61, -584.17, 6.26));

                    }
                    else
                    {
                        Thread.Sleep(1000);
                        Host.log("Нет транспорта");
                    }


                    return;

                }
            }

            if (Host.Me.Distance2D(myQuest.Loc) > 20)
            {
                if (myQuest.Loc.Z == 0)
                {
                    myQuest.Loc.Z = Convert.ToSingle(Host.GetNavMeshHeight(myQuest.Loc));
                }

                if (myQuest.Loc.Distance2D(new Vector3F(-8835.70, 489.70, 77.90)) < 50)
                {
                    myQuest.Loc.Z = 108;
                }

                if (!Host.CommonModule.MoveTo(myQuest.Loc, 1))
                {
                    return;
                }
            }

            foreach (var entity in Host.GetEntities<Unit>())
            {
                if (!entity.IsTaxi)
                {
                    continue;
                }

                if (!Host.CommonModule.MoveTo(entity, 2))
                {
                    return;
                }

                Thread.Sleep(2000);
                if (!Host.OpenTaxi(entity))
                {
                    Host.log("Не смог использовать такси " + entity.Name + "  " + Host.GetLastError(), LogLvl.Error);
                    Thread.Sleep(2000);
                    if (Host.GetLastError() != ELastError.ActionNotAllowed)
                    {
                        Thread.Sleep(10000);
                    }
                }

                Thread.Sleep(2000);

                foreach (var gossipOptionsData in Host.GetNpcDialogs())
                {
                    if (gossipOptionsData.OptionNPC == EGossipOptionIcon.Taxi)
                    {
                        Host.SelectNpcDialog(gossipOptionsData);
                    }

                    Host.log(gossipOptionsData.Text + " " + gossipOptionsData.OptionNPC + " " + gossipOptionsData.ClientOption);
                }
                Host.MyNodeListFill();
                TaxiNode node = null;
                foreach (var canLandNode in Host.TaxiNodesData.CanLandNodes)
                {
                    Host.log(canLandNode.Id + "  " + canLandNode.Name + " " + canLandNode.MapId + "  " + canLandNode.Cost + "   " + canLandNode.Location + "  ");
                    if (canLandNode.Name != myQuest.FlyPath)
                    {
                        continue;
                    }

                    Host.log("Нашел");
                    node = canLandNode;
                }

                if (node == null)
                {
                    return;
                }

                var result = Host.UseTaxi(node.Id);
                Thread.Sleep(1000);
                if (result != ETaxiError.Ok)
                {
                    Host.log("Ошибка перелета " + result, LogLvl.Error);
                }

                while (Host.Me.IsInFlight)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void GrindQuestClassic(MyQuest myQuest)
        {
            var farmmoblist = new List<uint>();
            if (!myQuest.GrindZone.ObjInZone(Host.Me))
            {
                Host.log("Бегу в зону");
                var point = myQuest.GrindZone.GetRandomPoint();
                if (Host.MainForm.On && !Host.CommonModule.MoveTo(new Vector3F(point.X, point.Y, Host.GetNavMeshHeight(new Vector3F(point.X, point.Y, Host.Me.Location.Z))), 20))
                {
                    return;
                }
            }

            var badRadius = 0;
            Host.FarmModule.SetFarmMobs(myQuest.GrindZone, farmmoblist);
            while (Host.MainForm.On && Host.FarmModule.ReadyToActions && Host.FarmModule.FarmState == FarmState.FarmMobs && Host.Me.Level < myQuest.Level)
            {
                if (Host.MyIsNeedRepair())
                {
                    break;
                }

                if (Host.MyIsNeedBuy())
                {
                    break;
                }

                if (Host.MyIsNeedSell())
                {
                    break;
                }

                if (Host.FarmModule.BestMob == null && !Host.MyIsNeedRegen() &&
                    (Host.FarmModule.MobsWithDropCount() + Host.FarmModule.MobsWithSkinCount()) == 0)
                {
                    badRadius++;
                }
                else
                {
                    badRadius = 0;
                }

                if (badRadius > 50)
                {
                    var findPoint = myQuest.GrindZone.GetRandomPoint();
                    var vectorPoint = new Vector3F(findPoint.X, findPoint.Y, Host.GetNavMeshHeight(new Vector3F(findPoint.X, findPoint.Y, Host.Me.Location.Z)));
                    Host.log("Не могу найти Monster, подбегаю в центр зоны " + Host.Me.Distance(vectorPoint) + "    " + vectorPoint);
                    Host.CommonModule.MoveTo(vectorPoint, 20);
                }


                Thread.Sleep(100);
            }

            Host.FarmModule.StopFarm();
            Thread.Sleep(1000);
        }

        private int GetQuestIndex(Quest quest)
        {
            if (quest == null)
            {
                Host.log("Ошибка квеста");
                Thread.Sleep(10000);
                return 0;
            }

            if (quest.Id == 6062 && quest.State != EQuestState.Complete)
            {
                return 0;
            }

            if (quest.Id == 6083 && quest.State != EQuestState.Complete)
            {
                return 0;
            }

            if (quest.Id == 6082 && quest.State != EQuestState.Complete)
            {
                return 0;
            }

            var index = -1;
            foreach (var templateQuestObjective in quest.Template.QuestObjectives)
            {
                if (templateQuestObjective.StorageIndex == -1)
                {
                    continue;
                }

                if (quest.Counts[templateQuestObjective.StorageIndex] >= templateQuestObjective.Amount)
                {
                    continue;
                }

                index = templateQuestObjective.StorageIndex;
                break;
            }

            return index;
        }

        public bool CheckPathReverse()
        {
            if (/*Host.Area.Id == 1519
                || Host.Area.Id == 12
                || Host.Area.Id == 40
                || Host.Area.Id == 10
                ||*/ Host.Area.Id == 1537)
            {
                if (!Host.CommonModule.MoveTo(-4838.90, -1318.94, 501.87))
                {
                    return false;
                }

                Thread.Sleep(1000);
                Host.MyMoveForvard(3000);
                return false;
            }

            if (Host.MapID == 369)
            {
                if (Host.Me.Distance(12.23, 8.29, -3.94) < 150)
                {
                    if (Host.Me.Distance(12.23, 8.29, -3.94) > 2)
                    {
                        if (!Host.CommonModule.MoveTo(12.23, 8.29, -3.94))
                        {
                            return false;
                        }
                    }

                    var transport = Host.MyGetTransportById(176081);
                    if (transport != null && !transport.IsMoving && Host.Me.Distance(transport) < 50)
                    {
                        Host.CommonModule.ForceMoveTo2(new Vector3F(5.46, 8.17, -3.46));
                        Thread.Sleep(30000);
                        transport = Host.MyGetTransportById(176081);
                        while (transport.IsMoving)
                        {
                            Host.log("Еду " + transport.IsMoving);
                            if (!Host.MainForm.On)
                            {
                                return false;
                            }

                            Thread.Sleep(1000);
                        }
                        Host.log("Доплыл");


                    }
                    else
                    {
                        Thread.Sleep(1000);
                        Host.log("Нет транспорта");
                    }
                }
                if (Host.Me.Distance(67.98, 2490.67, -4.30) < 150)
                {
                    Host.CommonModule.MoveTo(67.98, 2490.67, -4.30);
                    Thread.Sleep(1000);
                    Host.MyMoveForvard(3000);
                }
                return false;
            }




            if (Host.Area.Id == 1519)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckPath()
        {
            if (Host.Area.Id == 1519
                || Host.Area.Id == 12
                || Host.Area.Id == 40
                || Host.Area.Id == 10
                || Host.Area.Id == 44)
            {
                if (!Host.CommonModule.MoveTo(-8355.27, 525.69, 91.80))
                {
                    return false;
                }

                Thread.Sleep(1000);
                Host.MyMoveForvard(3000);
                return false;
            }

            if (Host.MapID == 369)
            {
                if (Host.Me.Distance(12.52, 2490.42, -4.00) < 150)
                {
                    if (Host.Me.Distance(12.52, 2490.42, -4.00) > 2)
                    {
                        if (!Host.CommonModule.MoveTo(12.52, 2490.42, -4.00))
                        {
                            return false;
                        }
                    }

                    var transport = Host.MyGetTransportById(176081);
                    if (transport != null && !transport.IsMoving && Host.Me.Distance(transport) < 50)
                    {
                        Host.CommonModule.ForceMoveTo2(new Vector3F(4.56, 2490.49, -3.27));
                        Thread.Sleep(30000);
                        transport = Host.MyGetTransportById(176081);
                        while (transport.IsMoving)
                        {
                            Host.log("Еду " + transport.IsMoving);
                            if (!Host.MainForm.On)
                            {
                                return false;
                            }

                            Thread.Sleep(1000);
                        }
                        Host.log("Доплыл");


                    }
                    else
                    {
                        Thread.Sleep(1000);
                        Host.log("Нет транспорта");
                    }
                }
                if (Host.Me.Distance(64.41, 9.30, -4.30) < 150)
                {
                    Host.CommonModule.MoveTo(65.54, 10.14, -4.30);
                    Thread.Sleep(1000);
                    Host.MyMoveForvard(3000);
                }
                return false;
            }



            if (Host.Area.Id == 1)
            {
                return true;
            }

            if (Host.Area.Id == 1537)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckPath2()
        {
            if (Host.Area.Id == 148 && Host.MapID == 1)
            {
                return true;
            }

            if (Host.MapID == 0)
            {
                if (Host.Me.Distance(-3729.47, -588.09, 6.35) > 10)
                {
                    if (!Host.CommonModule.MoveTo(-3729.47, -588.09, 6.35))
                    {
                        return false;
                    }
                }

                var transport = Host.MyGetTransportById(176310);
                if (transport != null && !transport.IsMoving && Host.Me.Distance(transport) < 50)
                {
                    // Host.CommonModule.ForceMoveTo2(transport.Location, 8);
                    Host.CommonModule.ForceMoveTo2(new Vector3F(-3714.73, -571.17, 6.10));
                    while (Host.MapID == 0)
                    {
                        Host.log("Плыву " + Host.MapID);
                        if (!Host.MainForm.On)
                        {
                            return false;
                        }

                        Thread.Sleep(1000);
                    }

                    while (Host.GameState != EGameState.Ingame)
                    {
                        Thread.Sleep(1000);
                        if (!Host.MainForm.On)
                        {
                            return false;
                        }
                    }
                    transport = Host.MyGetTransportById(176310);
                    while (transport.IsMoving)
                    {
                        Host.log("Плыву " + transport.IsMoving);
                        if (!Host.MainForm.On)
                        {
                            return false;
                        }

                        Thread.Sleep(1000);
                    }
                    Host.log("Доплыл");
                    Host.CommonModule.ForceMoveTo2(new Vector3F(6423.47, 818.95, 5.55));
                }
                else
                {
                    Host.log("Нет транспорта");
                }

                return false;
            }

            return false;
        }

        private void ConvoyClassic(MyQuest myQuest)
        {
            try
            {
                var quest = Host.GetQuest(myQuest.Id);
                uint npcId = 0;
                if (MyQuestHelps.QuestTableLua.ContainsKey(myQuest.Id))
                {
                    var lua = MyQuestHelps.QuestTableLua[myQuest.Id];
                    if (lua.Start.UnitLua.Count == 1)
                    {
                        foreach (var u in lua.Start.UnitLua)
                        {
                            npcId = u.Value;
                        }
                    }
                }

                if (npcId == 0)
                {
                    Host.log("Не нашел нпс для конвоя по квесту " + myQuest.Id);
                    return;
                }


                Convoy = Host.GetNpcById(npcId);
                while (Convoy != null && quest.State != EQuestState.Complete)
                {
                    Thread.Sleep(2000);
                    if (!Host.MainForm.On)
                    {
                        return;
                    }

                    if (Host.Me.Distance(Convoy) > 2 && Host.FarmModule.BestMob == null)
                    {
                        Host.CommonModule.MoveToConvoy(Convoy);
                    }

                    Host.log("Конвой " + Convoy.Name + " " + (Convoy as Unit)?.Target?.Name + "  " + ((Unit)Convoy).GetThreats().Count);
                    Convoy = Host.GetNpcById(npcId);
                    quest = Host.GetQuest(myQuest.Id);
                }

                Convoy = null;

            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        public bool IsQuestCompliteClassic(uint id, int index)
        {

            var quest = Host.GetQuest(id);

            if (quest == null)
            {
                return true;
            }
            /* if (quest.State == EQuestState.Fail)
{
    if (!Host.CancelQuest(quest.Id))
    {
        Host.log("Не смог отменить квест " + id + " " + Host.GetLastError(), LogLvl.Error);
        Thread.Sleep(5000);
    }

    return false;
}*/
            if (quest.Id == 3365)
                return true;


            if (quest.State == EQuestState.Complete || quest.State == EQuestState.Fail)
            {
                return true;
            }

            if (quest.Id == 1688 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 2520 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 2561 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 6062 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 6083 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 6082 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 435 && quest.State != EQuestState.Complete) //конвой
            {
                return false;
            }

            if (quest.Id == 5930 && quest.State != EQuestState.Complete)
            {
                return false;
            }
            if (quest.Id == 5929 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 6002 && quest.State != EQuestState.Complete)
            {
                return false;
            }
            if (quest.Id == 6001 && quest.State != EQuestState.Complete)
            {
                return false;
            }

            if (quest.Id == 5727)
            {
                return Host.MyGetItem(14544) != null;
            }
            //Host.log(id + " " + index);
            if (quest.Id == 1719 && index == 1)
            {
                if (quest.State == EQuestState.NoCounterComplete)
                {
                    return true;
                }
            }


            /* if (quest.Id == 62 && quest.State == EQuestState.NoCounterNone)
                 return true;
             if (quest.Id == 76 && quest.State == EQuestState.NoCounterNone)
                 return true;*/
            //
            var curCount = quest.Counts[index];


            if (quest.Template.QuestObjectives[index].Type == EQuestRequirementType.AreaTrigger)
            {
                if (quest.State == EQuestState.NoCounterNone || quest.State == EQuestState.NoCounterComplete)
                {
                    return true;
                }
            }

            var needCount = quest.Template.QuestObjectives[index].Amount;
            var type = quest.Template.QuestObjectives[index].Type;


            Host.MainForm.SetQuestStateText(type + "(" + index + "): " + curCount + "/" + needCount + "  " + QuestType);
            if (curCount >= needCount)
            {
                return true;
            }

            return false;
        }

        private void FillQuestClassic()
        {
            ListQuestClassic.Clear();

            if (Host.ClientType == EWoWClient.Classic)
            {
                switch (Host.Me.Race)
                {
                    case ERace.None:
                        break;
                    case ERace.Human:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest15HumanWarrior;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest512HumanWarrior);
                                    }
                                    break;
                                case EClass.Paladin:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest15HumanPaladin;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest512HumanPaladin);
                                    }

                                    break;
                                case EClass.Hunter:
                                    break;
                                case EClass.Rogue:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest15HumanRogue;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest512HumanRogue);
                                    }
                                    break;
                                case EClass.Priest:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest15HumanPriest;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest512HumanPriest);
                                    }
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    break;
                                case EClass.Mage:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest15HumanMage;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest512HumanMage);
                                    }
                                    break;
                                case EClass.Warlock:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest15HumanWarlock;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest512HumanWarlock);
                                    }
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1213HumanLochModanDarkshore);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1217Darkshore);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1718HumanDwarfGnomeLochModan);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest18HumanIronforgeRedridge);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1820RedridgeMountains);
                        }

                        break;
                    case ERace.Orc:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollWarrior;
                                    break;
                                case EClass.Paladin:
                                    break;
                                case EClass.Hunter:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollHunter;
                                    break;
                                case EClass.Rogue:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollRogue;
                                    break;
                                case EClass.Priest:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollPriest;
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollShaman;
                                    break;
                                case EClass.Mage:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollMage;
                                    break;
                                case EClass.Warlock:

                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollWarlock;
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1215TheBarrens);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1516StonetalonMountains);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest16TheBarrens);
                            if (Host.GetBotLogin() == "Daredevi1")
                            {
                                ListQuestClassic.AddRange(MyQuestHelps.Quest16GotoTBRest);
                                ListQuestClassic.AddRange(MyQuestHelps.Quest1620TheBarrens);
                                ListQuestClassic.AddRange(MyQuestHelps.Quest20TbPrep);
                                ListQuestClassic.AddRange(MyQuestHelps.Quest2023AshenvaleSouthBarrens);
                                ListQuestClassic.AddRange(MyQuestHelps.Quest2325StonetalonMountains);
                            }

                        }
                        break;
                    case ERace.Dwarf:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeWarrior;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeWarrior);
                                    }
                                    break;
                                case EClass.Paladin:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomePaladin;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomePaladin);
                                    }
                                    break;
                                case EClass.Hunter:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeHunter;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeHunter);
                                    }
                                    break;
                                case EClass.Rogue:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeRogue;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeRogue);
                                    }
                                    break;
                                case EClass.Priest:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomePriest;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomePriest);
                                    }
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    break;
                                case EClass.Mage:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeMage;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeMage);
                                    }
                                    break;
                                case EClass.Warlock:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeWarlock;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeWarlock);
                                    }
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        break;
                    case ERace.NightElf:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest112NightElfWarrior;
                                    }
                                    break;
                                case EClass.Paladin:
                                    break;
                                case EClass.Hunter:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest112NightElfHunter;
                                    }
                                    break;
                                case EClass.Rogue:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest112NightElfRogue;
                                    }
                                    break;
                                case EClass.Priest:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest112NightElfPriest;
                                    }
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    break;
                                case EClass.Mage:
                                    break;
                                case EClass.Warlock:
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest112NightElfDruid;
                                    }
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1217Darkshore);



                            ListQuestClassic.AddRange(MyQuestHelps.Quest17NightElfWetlandsRun);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1718NightElfLochModan);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest18NightElfDwarfGnomeIronforgeRedridge);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1820RedridgeMountains);
                        }
                        break;
                    case ERace.Undead:
                        {
                            ListQuestClassic = MyQuestHelps.Quest15Undead;
                            ListQuestClassic.AddRange(MyQuestHelps.Quest510Undead);
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    ListQuestClassic.AddRange(MyQuestHelps.Quest1012UndeadWarrior);
                                    break;
                                case EClass.Paladin:
                                    break;
                                case EClass.Hunter:
                                    break;
                                case EClass.Rogue:
                                    ListQuestClassic.AddRange(MyQuestHelps.Quest1012UndeadRogue);
                                    break;
                                case EClass.Priest:
                                    ListQuestClassic.AddRange(MyQuestHelps.Quest1012UndeadPriest);
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    break;
                                case EClass.Mage:
                                    ListQuestClassic.AddRange(MyQuestHelps.Quest1012UndeadMage);
                                    break;
                                case EClass.Warlock:
                                    ListQuestClassic.AddRange(MyQuestHelps.Quest1012UndeadWarlock);
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1215UndeadUCOrgriXR);

                        }
                        break;
                    case ERace.Tauren:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    ListQuestClassic = MyQuestHelps.Quest112TaurenWarrior;

                                    break;
                                case EClass.Paladin:
                                    break;
                                case EClass.Hunter:
                                    ListQuestClassic = MyQuestHelps.Quest112TaurenHunter;
                                    break;
                                case EClass.Rogue:
                                    break;
                                case EClass.Priest:
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    ListQuestClassic = MyQuestHelps.Quest112TaurenShaman;
                                    break;
                                case EClass.Mage:
                                    break;
                                case EClass.Warlock:
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    ListQuestClassic = MyQuestHelps.Quest112TaurenDruid;
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1215TheBarrens);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1516StonetalonMountains);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest16TheBarrens);
                            if (Host.GetBotLogin() == "Daredevi1")
                            {
                                ListQuestClassic.AddRange(MyQuestHelps.Quest16GotoTBRest);
                                ListQuestClassic.AddRange(MyQuestHelps.Quest1620TheBarrens);
                            }
                        }
                        break;
                    case ERace.Gnome:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeWarrior;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeWarrior);
                                    }
                                    break;
                                case EClass.Paladin:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomePaladin;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomePaladin);
                                    }
                                    break;
                                case EClass.Hunter:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeHunter;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeHunter);
                                    }
                                    break;
                                case EClass.Rogue:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeRogue;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeRogue);
                                    }
                                    break;
                                case EClass.Priest:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomePriest;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomePriest);
                                    }
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    break;
                                case EClass.Mage:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeMage;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeMage);
                                    }
                                    break;
                                case EClass.Warlock:
                                    {
                                        ListQuestClassic = MyQuestHelps.Quest16DwarfGnomeWarlock;
                                        ListQuestClassic.AddRange(MyQuestHelps.Quest612DwarfGnomeWarlock);
                                    }
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1217Darkshore);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1718HumanDwarfGnomeLochModan);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest18NightElfDwarfGnomeIronforgeRedridge);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1820RedridgeMountains);

                        }
                        break;
                    case ERace.Troll:
                        {
                            switch (Host.Me.Class)
                            {
                                case EClass.None:
                                    break;
                                case EClass.Warrior:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollWarrior;
                                    break;
                                case EClass.Paladin:
                                    break;
                                case EClass.Hunter:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollHunter;
                                    break;
                                case EClass.Rogue:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollRogue;
                                    break;
                                case EClass.Priest:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollPriest;
                                    break;
                                case EClass.DeathKnight:
                                    break;
                                case EClass.Shaman:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollShaman;

                                    break;
                                case EClass.Mage:
                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollMage;
                                    break;
                                case EClass.Warlock:

                                    ListQuestClassic = MyQuestHelps.Quest112OrcTrollWarlock;
                                    break;
                                case EClass.Monk:
                                    break;
                                case EClass.Druid:
                                    break;
                                case EClass.DemonHunter:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            ListQuestClassic.AddRange(MyQuestHelps.Quest1215TheBarrens);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest1516StonetalonMountains);
                            ListQuestClassic.AddRange(MyQuestHelps.Quest16TheBarrens);
                            if (Host.GetBotLogin() == "Daredevi1")
                            {
                                ListQuestClassic.AddRange(MyQuestHelps.Quest16GotoTBRest);
                                ListQuestClassic.AddRange(MyQuestHelps.Quest1620TheBarrens);
                            }


                        }
                        break;
                    case ERace.Goblin:
                        break;
                    case ERace.BloodElf:
                        break;
                    case ERace.Draenei:
                        break;
                    case ERace.FelOrc:
                        break;
                    case ERace.Naga:
                        break;
                    case ERace.Broken:
                        break;
                    case ERace.Skeleton:
                        break;
                    case ERace.Vrykul:
                        break;
                    case ERace.Tuskarr:
                        break;
                    case ERace.ForestTroll:
                        break;
                    case ERace.Taunka:
                        break;
                    case ERace.NorthrendSkeleton:
                        break;
                    case ERace.IceTroll:
                        break;
                    case ERace.Worgen:
                        break;
                    case ERace.Gilnean:
                        break;
                    case ERace.PandarenNeutral:
                        break;
                    case ERace.PandarenAlliance:
                        break;
                    case ERace.PandarenHorde:
                        break;
                    case ERace.NightBorne:
                        break;
                    case ERace.HighMountainTauren:
                        break;
                    case ERace.VoidElf:
                        break;
                    case ERace.LightForgedDraenei:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            else
            {
                switch (Host.Me.Team)
                {
                    case ETeam.Horde:
                        {
                            if (Host.Me.Race == ERace.Troll || Host.Me.Race == ERace.Orc)
                            {
                                foreach (var myQuest in MyQuestHelps.ListQuestsEchoIsles)
                                {
                                    ListQuestClassic.Add(myQuest);
                                }
                            }
                        }
                        break;
                    case ETeam.Alliance:
                        {

                        }
                        break;
                    case ETeam.Other:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


        }
    }
}