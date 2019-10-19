using Out.Internal.Core;
using System;
using System.Linq;
using System.Threading;
using WoWBot.Core;

namespace WowAI
{
    internal partial class Host
    {
        internal bool MyIsNeedRepair()
        {
            try
            {
                if (!CharacterSettings.CheckRepair)
                    return false;
                if (MapID == 1904)
                    return false;
                if (MapID == 1220 && CharacterSettings.Mode == Mode.Questing)
                    return false;
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Equipment)
                        continue;
                    if (item.MaxDurability == 0)
                        continue;
                    if (item.Durability < CharacterSettings.RepairCount)
                    {
                        log("Нужен ремонт " + item.Name + "  " + item.Durability + "/" + item.MaxDurability, LogLvl.Important);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return false;
        }

        internal bool MyAllItemsRepair()
        {
            try
            {
                if (CharacterSettings.PikPocket)
                    return false;
                if (!MyIsNeedRepair())
                {
                    if (!CharacterSettings.CheckRepairInCity)
                    {
                        log("Выключен ремонт в городе");
                        return false;
                    }
                       
                    var findArmorer = false;
                    foreach (var entity in GetEntities<Unit>())
                    {
                        switch (entity.Id)
                        {
                            case 32639:
                            case 127837:
                                continue;
                        }

                        if (!entity.IsArmorer) 
                            continue;
                        log("Нашел IsArmorer " + entity.Name + "[" + entity.Id + "] Dist:" + Me.Distance(entity), LogLvl.Important);
                        findArmorer = true;
                    }

                    if (!findArmorer)
                    {
                        log("Не нашел НПС для ремонта в городе");
                        return false;
                    }
                       
                }
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Equipment)
                        continue;
                    if (item.MaxDurability == 0)
                        continue;
                    if (item.Durability >= item.MaxDurability-2)
                        continue;
                    log("Ремонтируюсь, так как в городе " + item.Name + "  " + item.Durability + "/" + item.MaxDurability, LogLvl.Important);
                    return true;
                }
                log("Ремонт в городе не нужен");
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return false;
        }

        private Unit MyGetArmorer()
        {
            foreach (var npc in GetEntities<Unit>().OrderBy(i => Me.Distance(i)))
            {
                if (!npc.IsArmorer)
                    continue;
                return npc;
            }
            return null;
        }

        internal bool MyMoveToRepair()
        {
            try
            {
                if (!MyAllItemsRepair())
                    return true;
                if (CharacterSettings.SummonMount && IsOutdoors)
                {
                    var mountSell = SpellManager.GetSpell(61447); //Тундровый мамонт путешественника
                    if (mountSell == null)
                        mountSell = SpellManager.GetSpell(61425); //Тундровый мамонт путешественника

                    if (mountSell != null)
                    {
                        if (CharacterSettings.UseMountMyLoc)
                        {
                        }
                        else
                        {
                            if (Math.Abs(CharacterSettings.MountLocX) > 0)
                                if (!MoveTo(CharacterSettings.MountLocX, CharacterSettings.MountLocY, CharacterSettings.MountLocZ))
                                    return false;
                        }

                        CommonModule.MyUnmount();
                        CanselForm();
                        CancelMoveTo();
                        Thread.Sleep(500);
                        MyCheckIsMovingIsCasting();
                        var result = SpellManager.CastSpell(mountSell.Id);

                        if (result != ESpellCastError.SUCCESS)
                        {
                            log("Не удалось призвать маунта " + mountSell.Name + "  " + result, LogLvl.Error);
                            return false;
                        }
                        else
                            log("Призвал маунта", LogLvl.Ok);

                        Thread.Sleep(2000);
                        while (SpellManager.IsCasting)
                            Thread.Sleep(100);
                        Thread.Sleep(2000);
                        foreach (var npc in GetEntities<Unit>())
                        {
                            if (npc.Id == 32641 || npc.Id == 32639)
                            {
                                Thread.Sleep(1000);
                                if (!OpenShop(npc))
                                {
                                    log("Не смог открыть шоп 5 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(),
                                        LogLvl.Error);
                                    if (InteractionObject != null)
                                        log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id +
                                            "  " + Me.Distance(InteractionObject.Location) + " " +
                                            CurrentInteractionGuid);
                                    else
                                    {
                                        log("InteractionNpc = null " + CurrentInteractionGuid);
                                    }

                                    Thread.Sleep(5000);
                                    /* if (GetLastError() != ELastError.ActionNotAllowed)
                                     {
                                         return false;
                                     }*/
                                }
                                else
                                {
                                    log("Открыл шоп");
                                }

                                Thread.Sleep(1000);
                                if (CharacterSettings.CheckRepair)
                                    if (!ItemManager.RepairAllItems())
                                    {
                                        log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                        /* if (GetLastError() == ELastError.NoItemForRepair)
                                             return true;*/
                                        Thread.Sleep(10000);
                                        //  return false;
                                    }
                                    else
                                    {
                                        log("Отремонтировал ", LogLvl.Ok);
                                    }

                                Thread.Sleep(1000);

                                MySellItems();
                                MyBuyItems();
                                CommonModule.MyUnmount();
                                AutoQuests.NeedActionNpcSell = false;
                                AutoQuests.NeedActionNpcRepair = false;
                                return true;
                            }
                        }
                    }
                }


                var armorer = MyGetArmorer();
                if (armorer == null)
                {
                    var vendor = FindNpcForActionArmored();
                    if (vendor == null)
                    {
                        double bestDist = 9999999;
                        MyNpcLoc npcLoc = null;
                        foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                        {
                            if (_badNpcForSell.Contains(myNpcLoc.Id))
                                continue;
                            if (IsBadNpcLocs.Contains(myNpcLoc))
                                continue;
                            if (!myNpcLoc.IsArmorer)
                                continue;
                            if (Me.Distance(myNpcLoc.Loc) > bestDist)
                                continue;
                            bestDist = Me.Distance(myNpcLoc.Loc);
                            npcLoc = myNpcLoc;
                        }

                        if (npcLoc != null)
                        {
                            log("Выбрал нпс " + npcLoc.Id);
                            if (!CommonModule.MoveTo(npcLoc.Loc, 10))
                                return false;
                            var listUnit2 = GetEntities<Unit>();

                            foreach (var npc in listUnit2.OrderBy(i => Me.Distance(i)))
                            {
                                if (!npc.IsArmorer)
                                    continue;
                                if (!CommonModule.MoveTo(npc, 3))
                                    return false;
                                CanselForm();
                                Thread.Sleep(1000);
                                if (!OpenShop(npc))
                                {
                                    log("Не смог открыть шоп 2 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(), LogLvl.Error);
                                    if (InteractionObject != null)
                                        log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + "  " + CurrentInteractionGuid);
                                    else
                                    {
                                        log("InteractionNpc = null " + CurrentInteractionGuid);
                                    }
                                    Thread.Sleep(5000);
                                }
                                else
                                {
                                    log("Открыл шоп");
                                }

                                foreach (var gossipOptionsData in GetNpcDialogs())
                                {
                                    if (gossipOptionsData.Text.Contains("buy from you"))
                                        SelectNpcDialog(gossipOptionsData);
                                    log(gossipOptionsData.Text);
                                }

                                Thread.Sleep(1000);
                                if (CharacterSettings.CheckRepair)
                                    if (!ItemManager.RepairAllItems())
                                    {
                                        log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                        Thread.Sleep(10000);
                                    }
                                    else
                                    {
                                        log("Отремонтировал ", LogLvl.Ok);
                                    }

                                MySellItems();
                                MyBuyItems();
                                return true;
                            }
                        }
                        IsBadNpcLocs.Add(npcLoc);
                        log("Не указаны координаты для ремонта", LogLvl.Error);
                        Thread.Sleep(10000);
                        return false;
                    }

                    log("Выбран НПС для ремонта  " + vendor.Name, LogLvl.Ok);

                    if (vendor.AreaId != Area.Id || Me.Distance(vendor.Loc) > 1000)
                    {
                        if (!MyUseTaxi(vendor.AreaId, vendor.Loc))
                            return false;
                    }

                    if (!CommonModule.MoveTo(vendor.Loc, 10))
                        return false;
                }


                var listUnit = GetEntities<Unit>();

                foreach (var npc in listUnit.OrderBy(i => Me.Distance(i)))
                {
                    if (!npc.IsArmorer)
                        continue;
                    if (!CommonModule.MoveTo(npc, 3))
                        return false;
                    CanselForm();
                    Thread.Sleep(1000);
                    if (CurrentInteractionGuid == npc.Guid)
                    {
                        if (InteractionObject != null)
                            log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                        else
                        {
                            log("InteractionNpc = null " + CurrentInteractionGuid);
                        }
                    }
                    else
                    {
                        if (!OpenShop(npc))
                        {
                            if (GetLastError() == ELastError.TooFarDistance)
                                CommonModule.MoveTo(npc, 0);
                            log("Не смог открыть шоп 6 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(),
                                LogLvl.Error);
                            if (InteractionObject != null)
                                log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                            else
                            {
                                log("InteractionNpc = null " + CurrentInteractionGuid);
                            }
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            log("Открыл шоп");
                        }
                    }


                    foreach (var gossipOptionsData in GetNpcDialogs())
                    {
                        if (gossipOptionsData.Text.Contains("I need to repair"))
                        {
                            SelectNpcDialog(gossipOptionsData);
                            Thread.Sleep(1000);
                        }
                    }

                    Thread.Sleep(1000);
                    if (CharacterSettings.CheckRepair)
                        if (!ItemManager.RepairAllItems())
                        {
                            log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                            Thread.Sleep(10000);
                        }
                        else
                        {
                            log("Отремонтировал ", LogLvl.Ok);
                            Thread.Sleep(2000);
                        }

                    MySellItems();
                    MyBuyItems();
                    return true;
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }

            return false;
        }
    }
}