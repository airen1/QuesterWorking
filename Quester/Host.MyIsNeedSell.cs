using Out.Internal.Core;
using System;
using System.Linq;
using System.Threading;
using WoWBot.Core;

namespace WowAI
{
    internal partial class Host
    {
        public NpcForAction FindNpcForActionVendor()
        {
            double bestDist = 999999;
            NpcForAction bestNpc = null;
            try
            {
                foreach (var npc in CharacterSettings.NpcForActionSettings)
                {
                    if (npc.MapId != MapID)
                    {
                        continue;
                    }

                    if (npc.AreaId != Area.Id)
                    {
                        continue;
                    }

                    if (!npc.IsVendor)
                    {
                        continue;
                    }

                    if (Me.Distance(npc.Loc) < bestDist)
                    {
                        bestNpc = npc;
                        bestDist = Me.Distance(npc.Loc);
                    }
                }

                if (bestNpc == null)
                {
                    foreach (var npc in CharacterSettings.NpcForActionSettings)
                    {
                        if (npc.MapId != MapID)
                        {
                            continue;
                        }

                        if (!npc.IsVendor)
                        {
                            continue;
                        }

                        if (Me.Distance(npc.Loc) < bestDist)
                        {
                            bestNpc = npc;
                            bestDist = Me.Distance(npc.Loc);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }

            return bestNpc;
        }

        internal bool MyIsNeedSell()
        {
            try
            {
                if (!CharacterSettings.CheckRepairAndSell)
                {
                    return false;
                }

                if (MyGetFreeSlot() <= CharacterSettings.InvFreeSlotCount)
                {
                    log("Необходима продажа " + MyFreeInvSlots() + "/" + MyTotalInvSlot(), LogLvl.Important);
                    return true;
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }

            return false;
        }

        internal bool MyMoveToSell(bool noMount = false)
        {
            try
            {
                var mountSell = SpellManager.GetSpell(61447); //Тундровый мамонт путешественника
                if (mountSell == null)
                {
                    mountSell = SpellManager.GetSpell(61425); //Тундровый мамонт путешественника
                }

                if (noMount)
                {
                    mountSell = null;
                }

                if (mountSell != null && IsOutdoors)
                {
                    if (CharacterSettings.UseMountMyLoc)
                    {
                    }
                    else
                    {
                        if (Math.Abs(CharacterSettings.MountLocX) > 0)
                        {
                            if (!MoveTo(CharacterSettings.MountLocX, CharacterSettings.MountLocY, CharacterSettings.MountLocZ))
                            {
                                return false;
                            }
                        }
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
                    {
                        log("Призвал маунта", LogLvl.Ok);
                    }

                    Thread.Sleep(2000);
                    while (SpellManager.IsCasting)
                    {
                        Thread.Sleep(100);
                    }

                    Thread.Sleep(2000);
                    foreach (var npc in GetEntities<Unit>())
                    {
                        if (npc.Id == 32641 || npc.Id == 32639)
                        {
                            Thread.Sleep(1000);
                            if (!OpenShop(npc))
                            {
                                log("Не смог открыть шоп 7 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(),
                                    LogLvl.Error);
                                if (InteractionObject != null)
                                {
                                    log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id +
                                        "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                                }
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


                            Thread.Sleep(1000);
                            if (CharacterSettings.PikPocket)
                            {
                                foreach (var spell in Me.GetAuras())
                                {
                                    if (spell.SpellName == "Stealth")
                                    {
                                        spell.Cancel();
                                    }
                                }
                            }
                            MySellItems();
                            MyBuyItems();

                            Thread.Sleep(1000);
                            if (CharacterSettings.CheckRepair && npc.IsArmorer)
                            {
                                if (!ItemManager.RepairAllItems())
                                {
                                    log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                    if (GetLastError() == ELastError.NoItemForRepair)
                                    {
                                        return true;
                                    }

                                    Thread.Sleep(10000);
                                    return false;
                                }
                                else
                                {
                                    AutoQuests.NeedActionNpcRepair = false;
                                    log("Отремонтировал ", LogLvl.Ok);
                                }
                               
                            }

                            AutoQuests.NeedActionNpcSell = false;
                            
                            CommonModule.MyUnmount();
                            return true;
                        }
                    }
                }


                var armorer = MyGetArmorer();

                if (armorer == null)
                {
                    var vendor = FindNpcForActionVendor();
                    if (vendor == null)
                    {
                        double bestDist = 9999999;
                        MyNpcLoc npcLoc = null;
                        foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                        {
                            if (myNpcLoc.MapId != MapID)
                                continue;
                            if (_badNpcForSell.Contains(myNpcLoc.Id))
                            {
                                continue;
                            }

                            if (IsBadNpcLocs.Contains(myNpcLoc))
                            {
                                continue;
                            }

                            if (!myNpcLoc.IsArmorer)
                            {
                                continue;
                            }

                            if (Me.Team == ETeam.Horde && myNpcLoc.Team == ETeam.Alliance)
                            {
                                continue;
                            }

                            if (Me.Team == ETeam.Alliance && myNpcLoc.Team == ETeam.Horde)
                            {
                                continue;
                            }

                            if (Area.Id == 215) //Плато Красного облака
                            {
                                if (myNpcLoc.Id == 10380)
                                {
                                    continue;
                                }
                            }

                            if (Me.Race == ERace.Tauren)
                            {
                                if (myNpcLoc.Id == 3539)
                                    continue;
                                if (myNpcLoc.Id == 3537)
                                    continue;
                            }

                            if (CharacterSettings.Mode == Mode.QuestingClassic)
                            {
                                if (myNpcLoc.Id == 4188)
                                    continue;
                            }

                            if (Me.Distance(myNpcLoc.Loc) > bestDist)
                            {
                                continue;
                            }

                            bestDist = Me.Distance(myNpcLoc.Loc);
                            npcLoc = myNpcLoc;
                        }

                        if (npcLoc != null)
                        {
                            log("Выбрал нпс 2 " + npcLoc.Id);
                            if (!CommonModule.MoveTo(npcLoc.Loc, 10))
                            {
                                return false;
                            }

                            var listUnit2 = GetEntities<Unit>();

                            foreach (var npc in listUnit2.OrderBy(i => Me.Distance(i)))
                            {
                                if (!npc.IsArmorer)
                                {
                                    continue;
                                }

                                if (!CommonModule.MoveTo(npc, 3))
                                {
                                    return false;
                                }

                                CanselForm();
                                if (CharacterSettings.PikPocket)
                                {
                                    foreach (var spell in Me.GetAuras())
                                    {
                                        if (spell.SpellName == "Stealth")
                                        {
                                            spell.Cancel();
                                        }
                                    }
                                }
                                Thread.Sleep(1000);
                                if (!OpenShop(npc))
                                {
                                    log("Не смог открыть шоп 3 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(),
                                        LogLvl.Error);
                                    if (InteractionObject != null)
                                    {
                                        log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id +
                                            "  " + Me.Distance(InteractionObject.Location) + "  " +
                                            CurrentInteractionGuid);
                                    }
                                    else
                                    {
                                        log("InteractionNpc = null " + CurrentInteractionGuid);
                                    }

                                    Thread.Sleep(5000);
                                    if (GetLastError() == ELastError.TooFarDistance)
                                    {
                                        log("подбегаю поближе");
                                        CommonModule.MoveTo(npc, 0);
                                        return false;
                                    }
                                }


                                Thread.Sleep(1000);
                                foreach (var gossipOptionsData in GetNpcDialogs())
                                {
                                    if (gossipOptionsData.Text.Contains("buy from you"))
                                    {
                                        SelectNpcDialog(gossipOptionsData);
                                    }

                                    log(gossipOptionsData.Text);
                                }

                                MySellItems();
                                Thread.Sleep(1000);
                                if (CharacterSettings.CheckRepair && npc.IsArmorer)
                                {
                                    if (!ItemManager.RepairAllItems())
                                    {
                                        log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                        if (GetLastError() == ELastError.NoItemForRepair)
                                        {
                                            return true;
                                        }

                                        Thread.Sleep(10000);
                                        return false;
                                    }
                                    else
                                    {
                                        AutoQuests.NeedActionNpcRepair = false;
                                        log("Отремонтировал ", LogLvl.Ok);
                                    }
                               
                                }
                                MyBuyItems();
                                Thread.Sleep(1000);
                                if (CharacterSettings.CheckRepair && npc.IsArmorer)
                                {
                                    if (!ItemManager.RepairAllItems())
                                    {
                                        log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                        if (GetLastError() == ELastError.NoItemForRepair)
                                        {
                                            return true;
                                        }

                                        Thread.Sleep(10000);
                                        return false;
                                    }
                                    else
                                    {
                                        AutoQuests.NeedActionNpcRepair = false;
                                        log("Отремонтировал ", LogLvl.Ok);
                                    }
                               
                                }
                                return true;
                            }

                            IsBadNpcLocs.Add(npcLoc);
                            log("Не указаны координаты бакалейщика " + npcLoc.Id, LogLvl.Error);
                        }

                        Thread.Sleep(10000);
                        return false;
                    }


                    log("Выбран НПС для продажи " + vendor.Name, LogLvl.Ok);

                    if (vendor.AreaId != Area.Id || Me.Distance(vendor.Loc) > 1000)
                    {
                        if (!MyUseTaxi(vendor.AreaId, vendor.Loc))
                        {
                            return false;
                        }
                    }

                    if (!CommonModule.MoveTo(vendor.Loc, 5))
                    {
                        return false;
                    }
                }

                if (CharacterSettings.PikPocket)
                {
                    foreach (var spell in Me.GetAuras())
                    {
                        if (spell.SpellName == "Stealth")
                        {
                            spell.Cancel();
                        }
                    }
                }

                var listUnit = GetEntities<Unit>();

                foreach (var npc in listUnit.OrderBy(i => Me.Distance(i)))
                {
                    if (armorer != null)
                    {
                        if (!npc.IsArmorer)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!npc.IsVendor)
                        {
                            continue;
                        }
                    }

                    if (!CommonModule.MoveTo(npc.Location, 3))
                    {
                        return false;
                    }

                    CanselForm();
                    Thread.Sleep(1000);
                    if (!OpenShop(npc))
                    {
                        if (GetLastError() == ELastError.TooFarDistance)
                        {
                            CommonModule.MoveTo(npc, 0);
                        }

                        log("Не смог открыть шоп 4 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(), LogLvl.Error);
                        if (InteractionObject != null)
                        {
                            log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " +
                                Me.Distance(InteractionObject.Location) + "  " + CurrentInteractionGuid);
                        }
                        else
                        {
                            log("InteractionNpc = null " + CurrentInteractionGuid);
                        }

                        Thread.Sleep(5000);

                    }

                    Thread.Sleep(1000);

                    MySellItems();
                    if (CharacterSettings.CheckRepair && npc.IsArmorer)
                    {
                        if (!ItemManager.RepairAllItems())
                        {
                            log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                            if (GetLastError() == ELastError.NoItemForRepair)
                            {
                                return true;
                            }

                            Thread.Sleep(10000);
                            return false;
                        }
                        else
                        {
                            AutoQuests.NeedActionNpcRepair = false;
                            log("Отремонтировал ", LogLvl.Ok);
                        }
                               
                    }
                    MyBuyItems();
                    if (CharacterSettings.CheckRepair && npc.IsArmorer)
                    {
                        if (!ItemManager.RepairAllItems())
                        {
                            log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                            if (GetLastError() == ELastError.NoItemForRepair)
                            {
                                return true;
                            }

                            Thread.Sleep(10000);
                            return false;
                        }
                        else
                        {
                            AutoQuests.NeedActionNpcRepair = false;
                            log("Отремонтировал ", LogLvl.Ok);
                        }
                               
                    }
                    return true;
                }
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception e)
            {
                log(e.ToString());
                return false;
            }

            return false;
        }

        private void MySellItems()
        {
            try
            {
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 && item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 && item.Place != EItemPlace.InventoryItem)
                    {
                        continue;
                    }

                    if (item.GetSellPrice() == 0)
                    {
                        continue;
                    }

                    if (GetBotLogin() == "zawww2")
                    {
                        if (Me.Level > 54 && Me.Class == EClass.Paladin && CharacterSettings.Mode == Mode.FarmMob && item.ItemClass == EItemClass.Armor)
                        {
                            if ((EItemSubclassArmor)item.ItemSubClass == EItemSubclassArmor.PLATE || (EItemSubclassArmor)item.ItemSubClass == EItemSubclassArmor.MISCELLANEOUS)
                            {
                                var inta = false;
                                var stamina = false;
                                foreach (var @sbyte in item.GetStats())
                                {
                                    if (@sbyte.Key == EItemModType.INTELLECT)
                                        inta = true;
                                    if (@sbyte.Key == EItemModType.STAMINA)
                                        stamina = true;
                                }

                                if (inta && stamina)
                                {
                                    continue;
                                }
                            }
                        }

                        if (Me.Level > 54 && Me.Class == EClass.Mage && CharacterSettings.Mode == Mode.FarmMob && item.ItemClass == EItemClass.Armor)
                        {
                            if ((EItemSubclassArmor)item.ItemSubClass == EItemSubclassArmor.MISCELLANEOUS)
                            {
                                var shadow = false;

                                foreach (var @sbyte in item.GetStats())
                                {
                                    if (@sbyte.Key == EItemModType.SHADOW_RESISTANCE)
                                        shadow = true;
                                }

                                if (shadow)
                                {
                                    continue;
                                }
                            }
                        }

                    }

                    foreach (var characterSettingsMyItemGlobal in CharacterSettings.MyItemGlobals)
                    {
                        if (item.ItemClass == characterSettingsMyItemGlobal.Class && item.ItemQuality == characterSettingsMyItemGlobal.Quality)
                        {
                            var isNosell = false;
                            if (CharacterSettings.Mode == Mode.QuestingClassic)
                            {

                                foreach (var quest in GetQuests())
                                {
                                    if (quest.Template == null)
                                        continue;
                                    if (quest.Template.QuestObjectives == null)
                                        continue;
                                    foreach (var templateQuestObjective in quest.Template.QuestObjectives)
                                    {
                                        if (item.Id != templateQuestObjective.ObjectID)
                                            continue;
                                        isNosell = true;
                                        log("Не продаю так как " + item.Name + "[" + item.Id + "] Цена:" + (item.GetSellPrice() / 10000f).ToString("F2") + " находится в списке предметов для квеста" + quest.Id, LogLvl.Important);
                                    }
                                }
                            }



                            foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
                            {
                                if (item.Id != characterSettingsItemSetting.Id)
                                {
                                    continue;
                                }

                                if (characterSettingsItemSetting.Use == EItemUse.Nothing
                                    || characterSettingsItemSetting.Use == EItemUse.PutToWarehouse
                                    || characterSettingsItemSetting.Use == EItemUse.Sell
                                    || characterSettingsItemSetting.Use == EItemUse.DoNotSell
                                    || characterSettingsItemSetting.Use == EItemUse.Use
                                    || characterSettingsItemSetting.Use == EItemUse.SendByMail
                                    || characterSettingsItemSetting.Use == EItemUse.FoodPet
                                    || characterSettingsItemSetting.Use == EItemUse.NotEquip
                                    || characterSettingsItemSetting.Use == EItemUse.Buy)
                                {
                                    isNosell = true;
                                    log("Не продаю так как " + item.Name + "[" + item.Id + "] Цена:" + (item.GetSellPrice() / 10000f).ToString("F2") + " находится в списке предметов " + characterSettingsItemSetting.Use, LogLvl.Important);
                                }
                            }

                            foreach (var characterSettingsRegenItemse in CharacterSettings.RegenItemses)
                            {
                                if (item.Id != characterSettingsRegenItemse.ItemId)
                                {
                                    continue;
                                }

                                isNosell = true;
                                log("Не продаю так как " + item.Name + "[" + item.Id + "] Цена:" + (item.GetSellPrice() / 10000f).ToString("F2") + " находится в списке предметов для регена " + characterSettingsRegenItemse.Checked, LogLvl.Important);
                            }

                            if (characterSettingsMyItemGlobal.ItemLevel != 0)
                            {
                                if (item.Level > characterSettingsMyItemGlobal.ItemLevel)
                                {
                                    log("Не продаю так как уровень " + item.Name + "[" + item.Id + "] Цена:" + (item.GetSellPrice() / 10000f).ToString("F2") + " не подходит " + item.Level + "/" + characterSettingsMyItemGlobal.ItemLevel, LogLvl.Important);
                                    isNosell = true;
                                }

                            }

                            if (!isNosell)
                            {
                                log("Продаю " + item.Name + "  [" + item.Id + "]  Цена:" + (item.GetSellPrice() / 10000f).ToString("F2"), LogLvl.Ok);
                                var result = item.Sell();
                                if (result == ESellResult.Success)
                                {
                                    continue;
                                }

                                log("Не смог продать  " + item.Name + "[" + item.Id + "] Цена:" + (item.GetSellPrice() / 10000f).ToString("F2") + "  " + result + " " + " " + GetLastError(), LogLvl.Error);
                                Thread.Sleep(5000);
                                if (GetLastError() == ELastError.CantFindNpc)
                                {
                                    return;
                                }

                                if (GetLastError() == ELastError.DialongWithNpcNotOpened)
                                {
                                    MySendKeyEsc();
                                    return;
                                }
                            }
                        }
                    }
                }

                //Обычная продажа
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 && item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 && item.Place != EItemPlace.InventoryItem)
                    {
                        continue;
                    }

                    if (item.GetSellPrice() == 0)
                    {
                        continue;
                    }

                    foreach (var itemSettingse in CharacterSettings.ItemSettings)
                    {
                        if (itemSettingse.Id == item.Id && itemSettingse.Use == EItemUse.Sell && Me.Level > itemSettingse.MeLevel && MeGetItemsCount(item.Id) > itemSettingse.MinCount)
                        {
                            log("Продаю " + item.Name + "  " + item.Id + "  Цена:" + item.GetSellPrice() + " " + MeGetItemsCount(item.Id) + " " + itemSettingse.MinCount, LogLvl.Ok);
                            var result = item.Sell();
                            if (result != ESellResult.Success)
                            {
                                log("Не смог продать  " + item.Name + "[" + item.Id + "] Цена:" + item.GetSellPrice() + "  " + result + " " + " " + GetLastError(), LogLvl.Error);
                                Thread.Sleep(5000);
                            }
                            if (itemSettingse.MinCount > 0)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }


            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log("SellItems2: " + err);
            }
        }
    }
}