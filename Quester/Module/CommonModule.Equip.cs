using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WoWBot.Core;

namespace WowAI.Module
{
    internal partial class CommonModule
    {
        public bool Equip = true;
        public List<EItemSubclassWeapon> WeaponType = new List<EItemSubclassWeapon>();
        public List<EItemSubclassArmor> ArmorType = new List<EItemSubclassArmor>();
        private Dictionary<EItemModType, double> StatCoefs = new Dictionary<EItemModType, double>();
        private bool _weaponAndShield;
        private bool _twoWeapon;

        public EEquipmentSlot GetItemEPlayerPartsType(EInventoryType type)
        {
            try
            {
                switch (type)
                {
                    /*  case EInventoryType.NonEquip:
                      break;*/
                    case EInventoryType.Head:
                        return EEquipmentSlot.Head;
                    case EInventoryType.Neck:
                        return EEquipmentSlot.Neck;
                    case EInventoryType.Shoulders:
                        return EEquipmentSlot.Shoulders;

                    case EInventoryType.Body:
                        return EEquipmentSlot.Body;
                    case EInventoryType.Chest:
                        return EEquipmentSlot.Chest;

                    case EInventoryType.Waist:
                        return EEquipmentSlot.Waist;
                    case EInventoryType.Legs:
                        return EEquipmentSlot.Legs;
                    case EInventoryType.Feet:
                        return EEquipmentSlot.Feet;
                    case EInventoryType.Wrists:
                        return EEquipmentSlot.Wrists;
                    case EInventoryType.Hands:
                        return EEquipmentSlot.Hands;
                    case EInventoryType.Finger:
                        return EEquipmentSlot.Finger1;
                    case EInventoryType.Trinket:
                        return EEquipmentSlot.Trinket1;

                    case EInventoryType.Weapon:
                        {
                            /* if (Host.CharacterSettings.TwoWeapon)
                                 {
                                     return EEquipmentSlot.OffHand;
                                 }*/
                            return EEquipmentSlot.MainHand;
                        }


                    case EInventoryType.Shield:
                        return EEquipmentSlot.OffHand;

                    case EInventoryType.Ranged:
                        return Host.ClientType == EWoWClient.Classic ? EEquipmentSlot.Ranged : EEquipmentSlot.MainHand;

                    case EInventoryType.Cloak:
                        return EEquipmentSlot.Cloak;
                    case EInventoryType.TwoHandedWeapon:
                        return EEquipmentSlot.MainHand;
                    case EInventoryType.Bag:
                        break;
                    case EInventoryType.Tabard:
                        return EEquipmentSlot.Tabard;
                    case EInventoryType.Robe:
                        return EEquipmentSlot.Chest;
                    case EInventoryType.MainHandWeapon:
                        return EEquipmentSlot.MainHand;
                    case EInventoryType.OffHandWeapon:
                        return EEquipmentSlot.OffHand;
                    case EInventoryType.Holdable:
                        return EEquipmentSlot.OffHand;
                    /*  case EInventoryType.Ammo:
                      break;*/
                    /*  case EInventoryType.Thrown:
                      break;*/
                    case EInventoryType.RangedRight:
                        return EEquipmentSlot.Ranged;
                        /*  case EInventoryType.Quiver:
                                  break;*/
                        /*  case EInventoryType.Relic:
                                  break;   */
                }

                return EEquipmentSlot.Ranged;
            }
            catch (Exception e)
            {
                Host.log(e + "");
                return EEquipmentSlot.Ranged;
            }
        }

        private int MyInventoryBagCount()
        {
            var count = 0;
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.InventoryBag)
                {
                    count++;
                }
            }

            return count;
        }

        private void EquipBestBug(EItemClass itemClass)
        {
            Item equipItemMinBag = null;
            uint equipItemMinBagCount = 999;
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.InventoryType != EInventoryType.Bag)
                {
                    continue;
                }

                if (item.Place != EItemPlace.InventoryBag)
                {
                    continue;
                }

                if (item.Type != EBotTypes.Bag)
                {
                    continue;
                }

                var equip = true;
                foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                {
                    if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                    {
                        equip = false;
                    }
                }

                if (!equip)
                {
                    continue;
                }

                if (item.ItemClass != itemClass)
                {
                    continue;
                }

                if (((Bag)item).BagSize < equipItemMinBagCount)
                {
                    equipItemMinBag = item;
                    equipItemMinBagCount = (item as Bag).BagSize;
                    // Host.log(equipItemMinBag.Name + "[" + equipItemMinBagCount + "]     " );
                }
            }

            Item invItemMinBag = null;
            uint invItemMinBagCount = 0;
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.InventoryType != EInventoryType.Bag)
                {
                    continue;
                }

                if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 && item.Place != EItemPlace.Bag3 &&
                    item.Place != EItemPlace.Bag4 && item.Place != EItemPlace.InventoryItem)
                {
                    continue;
                }

                if (item.Type != EBotTypes.Bag)
                {
                    continue;
                }

                if (item.ItemClass != itemClass)
                {
                    continue;
                }

                var equip = true;
                foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                {
                    if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                    {
                        equip = false;
                    }
                }

                if (!equip)
                {
                    continue;
                }

                if (((Bag)item).BagSize > invItemMinBagCount)
                {
                    invItemMinBag = item;
                    invItemMinBagCount = (item as Bag).BagSize;
                    //Host.log(invItemMinBag.Name + "[" + invItemMinBagCount + "]     " );
                }
            }

            if (equipItemMinBag != null && invItemMinBag != null)
            {
                if (invItemMinBagCount > equipItemMinBagCount)
                {
                    Host.log(invItemMinBag.Name + "[" + invItemMinBagCount + "]     " + equipItemMinBag.Name + "[" + equipItemMinBagCount + "]");
                    if (invItemMinBag.Place != EItemPlace.InventoryItem)
                    {
                        Host.log("Необходимо перенести сумку");
                        foreach (var item in Host.ItemManager.GetItems())
                        {
                            if (item.Place != EItemPlace.InventoryItem)
                            {
                                continue;
                            }

                            item.SwapItem(invItemMinBag);
                            break;
                        }
                    }
                    else
                    {
                        if (invItemMinBag.SwapItem(equipItemMinBag))
                        {
                            Host.log("Одеваю " + invItemMinBag.InventoryType + "  best item = " + invItemMinBag.Name,LogLvl.Ok);
                        }
                        else
                        {
                            Host.log("Error. Can't equip " + invItemMinBag.InventoryType + "  " + "  best item = " +invItemMinBag.Name + ". Reason = " + Host.GetLastError(), LogLvl.Error);
                        }
                    }
                    Thread.Sleep(Host.RandGenerator.Next(555, 1555));
                }
            }
        }

        private void CheckArrow()
        {
            if (!Host.CharacterSettings.UseArrow)
            {
                return;
            }

            if (Host.Me.AmmoID != Host.CharacterSettings.UseArrowId)
            {
                if (Host.MeGetItemsCount(Host.CharacterSettings.UseArrowId) == 0)
                {
                    Host.log("Нет стрел");
                    return;
                }

                if (!Host.SetAmmoID(Host.CharacterSettings.UseArrowId))
                {
                    Host.log("Не смог устанвоить стрелы " + Host.CharacterSettings.UseArrowId + "   " + Host.Me.AmmoID + "  " + Host.GetLastError(), LogLvl.Error);
                    Thread.Sleep(5000);
                }
            }
        }

        public double GetCoef(AuctionItem item)
        {
            double coef = 0;
            try
            {
                if (item.Template.GetClass() == EItemClass.Weapon)
                {
                    coef = item.Template.GetDamagePerSecond();
                    foreach (var s in item.GetStats())
                    {
                        if (StatCoefs.ContainsKey(s.Key))
                        {
                            coef += StatCoefs[s.Key] * s.Value;
                        }
                    }
                }

                if (item.Template.GetClass() == EItemClass.Armor)
                {
                    coef = item.Template.GetArmorDefence() * Host.CharacterSettings.EquipArmorCoef;
                    foreach (var s in item.GetStats())
                    {
                        if (StatCoefs.ContainsKey(s.Key))
                        {
                            coef += StatCoefs[s.Key] * s.Value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Host.log("" + e);
            }

            return coef;
        }

        public double GetCoef(Item item)
        {
            double coef = 0;
            try
            {
                if (item == null)
                {
                    return 0;
                }

                if (item.ItemClass == EItemClass.Weapon)
                {
                    coef = item.Template.GetDamagePerSecond();
                    foreach (var s in item.GetStats())
                    {
                        if (StatCoefs.ContainsKey(s.Key))
                        {
                            coef += StatCoefs[s.Key] * s.Value;
                        }
                    }
                }

                if (item.ItemClass == EItemClass.Armor)
                {
                    coef = item.Template.GetArmorDefence() * Host.CharacterSettings.EquipArmorCoef;
                    foreach (var s in item.GetStats())
                    {
                        if (StatCoefs.ContainsKey(s.Key))
                        {
                            coef += StatCoefs[s.Key] * s.Value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Host.log("" + e);
            }

            return coef;
        }


        public void GetBestTwoArmor()
        {
            if (Host.Me.Class != EClass.Rogue)
            {
                return;
            }

            Item mainHand = null;
            Item offHand = null;
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.Place != EItemPlace.Equipment)
                {
                    continue;
                }

                if (item.Cell == 15)
                {
                    mainHand = item;
                }

                if (item.Cell == 16)
                {
                    offHand = item;
                }
            }
            if (mainHand == null || offHand == null)
            {
                return;
            }

            if (offHand.InventoryType == EInventoryType.OffHandWeapon)
            {
                return;
            }

            if (mainHand.InventoryType == EInventoryType.MainHandWeapon)
            {
                return;
            }

            if (mainHand.Template.GetDamagePerSecond() < offHand.Template.GetDamagePerSecond())
            {
                if (!mainHand.SwapItem(offHand))
                {
                    Host.log("Не удалось поменять местами оружие " + mainHand.Name + " " + offHand.Name, LogLvl.Error);
                }
            }
        }

        private void EquipBestArmorAndWeapon()
        {
            try
            {
                if (!Host.Me.IsAlive)
                {
                    return;
                }

                if (Host.Me.IsDeadGhost)
                {
                    return;
                }

                var equipCells = new Dictionary<EEquipmentSlot, Item>();

                CheckArrow();

                foreach (EEquipmentSlot value in Enum.GetValues(typeof(EEquipmentSlot)))
                {
                    if (Host.ClientType == EWoWClient.Retail)
                    {
                        if (value == EEquipmentSlot.Ranged)
                        {
                            continue;
                        }
                    }

                    equipCells.Add(value, null);
                }

                //Сумки
                if (Host.ClientType == EWoWClient.Classic)
                {
                    if (MyInventoryBagCount() < 4)
                    {
                        foreach (var item in Host.ItemManager.GetItems())
                        {
                            if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                                item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                                item.Place != EItemPlace.InventoryItem)
                            {
                                continue;
                            }

                            if (item.InventoryType != EInventoryType.Bag || !item.IsBag)
                            {
                                continue;
                            }

                            var equip = true;
                            foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                            {
                                if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                                {
                                    equip = false;
                                }
                            }

                            if (!equip)
                            {
                                continue;
                            }

                            if (item.Equip())
                            {
                                Host.log("Одеваю " + item.InventoryType + "  best item = " + item.Name,
                                    LogLvl.Ok);
                            }
                            else
                            {
                                Host.log(
                                    "Error. Can't equip " + item.InventoryType + "  " + "  best item = " + item.Name +
                                    ". Reason = " + Host.GetLastError(), LogLvl.Error);
                            }

                            Thread.Sleep(Host.RandGenerator.Next(555, 1555));
                            break;
                        }
                    }
                    else
                    {
                        if (Host.Me.Class == EClass.Hunter)
                        {
                            EquipBestBug(EItemClass.Quiver);
                        }

                        EquipBestBug(EItemClass.Container);
                    }
                }



                //  Host.log("Тест  EquipBestArmorAndWeapon 2");
                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 && item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 && item.Place != EItemPlace.InventoryItem && item.Place != EItemPlace.Equipment)
                    {
                        continue;
                    }

                    if (item.ItemClass != EItemClass.Armor && item.ItemClass != EItemClass.Weapon)
                    {
                        continue;
                    }

                    if (item.Template == null)
                    {
                        continue;
                    }

                    if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                    {
                        continue;
                    }

                    if (item.RequiredLevel > Host.Me.Level)
                    {
                        continue;
                    }

                    if (item.Place != EItemPlace.Equipment)
                    {
                        if (item.ItemQuality > Host.CharacterSettings.MaxItemQuality)
                        {
                            continue;
                        }
                    }

                    var equip = true;
                    foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                    {
                        if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                        {
                            equip = false;
                        }
                    }

                    if (!equip)
                    {
                        continue;
                    }

                    var itemEquipType = GetItemEPlayerPartsType(item.InventoryType);

                    if (itemEquipType == EEquipmentSlot.Cloak)
                    {
                    }
                    else
                    {
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            if (item.ItemClass == EItemClass.Weapon)
                            {
                                if ((Host.GetProficiency(EItemClass.Weapon) & (1 << (int)item.Template.GetSubClass())) == 0)
                                {
                                    continue;
                                }
                            }

                            if (item.ItemClass == EItemClass.Armor)
                            {
                                if ((Host.GetProficiency(EItemClass.Armor) & (1 << (int)item.Template.GetSubClass())) == 0)
                                {
                                    continue;
                                }
                            }
                        }


                        if (item.ItemClass == EItemClass.Weapon && !WeaponType.Contains((EItemSubclassWeapon)item.ItemSubClass))
                        {
                            continue;
                        }

                        if (item.ItemClass == EItemClass.Armor && !ArmorType.Contains((EItemSubclassArmor)item.ItemSubClass))
                        {
                            continue;
                        }
                    }


                    if (Host.ClientType == EWoWClient.Retail)
                    {
                        if (item.ItemClass == EItemClass.Weapon)
                        {
                            if (Host.CharacterSettings.EquipItemStat != 0)
                            {
                                if (!item.ItemStatType.Contains((EItemModType)Host.CharacterSettings.EquipItemStat))
                                {
                                    continue;
                                }
                            }
                        }
                    }


                    if (!_weaponAndShield)
                    {
                        if (itemEquipType == EEquipmentSlot.OffHand)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (itemEquipType == EEquipmentSlot.OffHand && item.ItemSubClass == (int)EItemSubclassArmor.SHIELD)
                        {
                            var next = false;
                            foreach (var item1 in Host.ItemManager.GetItems())
                            {
                                if (item1.Place != EItemPlace.Equipment)
                                {
                                    continue;
                                }

                                if (item1.InventoryType == EInventoryType.TwoHandedWeapon)
                                {
                                    next = true;
                                }
                            }

                            if (next)
                            {
                                continue;
                            }
                        }
                    }


                    if (equipCells[itemEquipType] == null)
                    {
                        equipCells[itemEquipType] = item;
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            Host.SetVar(equipCells[itemEquipType], "coef", GetCoef(item));
                        }
                    }
                    else
                    {
                        double bestCoef = equipCells[itemEquipType].Level;
                        double curCoef = item.Level;

                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            bestCoef = GetCoef(equipCells[itemEquipType]);
                            curCoef = GetCoef(item);
                        }

                        if (bestCoef < curCoef)
                        {
                            equipCells[itemEquipType] = item;
                            Host.SetVar(equipCells[itemEquipType], "coef", bestCoef);
                        }
                    }
                }

                // Finger 2
                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                        item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                        item.Place != EItemPlace.InventoryItem && item.Place != EItemPlace.Equipment)
                    {
                        continue;
                    }

                    if (item.ItemClass != EItemClass.Armor || (EItemSubclassArmor)item.ItemSubClass != EItemSubclassArmor.MISCELLANEOUS)
                    {
                        continue;
                    }

                    if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                    {
                        continue;
                    }

                    var equip = true;
                    foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                    {
                        if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                        {
                            equip = false;
                        }
                    }

                    if (!equip)
                    {
                        continue;
                    }

                    if (item.RequiredLevel > Host.Me.Level)
                    {
                        continue;
                    }

                    if (item.InventoryType != EInventoryType.Finger)
                    {
                        continue;
                    }

                    if (equipCells.ContainsValue(item))
                    {
                        continue;
                    }

                    if (item.ItemQuality > Host.CharacterSettings.MaxItemQuality)
                    {
                        continue;
                    }

                    if (equipCells[EEquipmentSlot.Finger2] == null)
                    {
                        equipCells[EEquipmentSlot.Finger2] = item;
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            Host.SetVar(equipCells[EEquipmentSlot.Finger2], "coef", GetCoef(item));
                        }
                    }
                    else
                    {
                        double bestCoef = equipCells[EEquipmentSlot.Finger2].Level;
                        double curCoef = item.Level;
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            bestCoef = GetCoef(equipCells[EEquipmentSlot.Finger2]);
                            curCoef = GetCoef(item);
                        }

                        if (bestCoef < curCoef)
                        {
                            equipCells[EEquipmentSlot.Finger2] = item;
                            Host.SetVar(equipCells[EEquipmentSlot.Finger2], "coef", bestCoef);
                        }
                    }
                }

                //  Trinket2
                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                        item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                        item.Place != EItemPlace.InventoryItem && item.Place != EItemPlace.Equipment)
                    {
                        continue;
                    }

                    if (item.ItemClass != EItemClass.Armor ||
                        (EItemSubclassArmor)item.ItemSubClass != EItemSubclassArmor.MISCELLANEOUS)
                    {
                        continue;
                    }

                    if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                    {
                        continue;
                    }

                    if (item.RequiredLevel > Host.Me.Level)
                    {
                        continue;
                    }

                    if (item.InventoryType != EInventoryType.Trinket)
                    {
                        continue;
                    }

                    if (equipCells.ContainsValue(item))
                    {
                        continue;
                    }

                    if (item.ItemQuality > Host.CharacterSettings.MaxItemQuality)
                    {
                        continue;
                    }

                    var equip = true;
                    foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                    {
                        if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                        {
                            equip = false;
                        }
                    }

                    if (!equip)
                    {
                        continue;
                    }

                    if (equipCells[EEquipmentSlot.Trinket2] == null)
                    {
                        equipCells[EEquipmentSlot.Trinket2] = item;
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            Host.SetVar(equipCells[EEquipmentSlot.Trinket2], "coef", GetCoef(item));
                        }
                    }
                    else
                    {
                        double bestCoef = equipCells[EEquipmentSlot.Trinket2].Level;
                        double curCoef = item.Level;
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            bestCoef = GetCoef(equipCells[EEquipmentSlot.Trinket2]);
                            curCoef = GetCoef(item);
                        }

                        if (bestCoef < curCoef)
                        {
                            equipCells[EEquipmentSlot.Trinket2] = item;
                            Host.SetVar(equipCells[EEquipmentSlot.Trinket2], "coef", bestCoef);
                        }
                    }
                }

                //    Host.log("Тест  EquipBestArmorAndWeapon 5");
                if (_twoWeapon)
                {
                    if (equipCells[EEquipmentSlot.MainHand]?.InventoryType != EInventoryType.TwoHandedWeapon)
                    {
                        foreach (var item in Host.ItemManager.GetItems())
                        {
                            if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                                item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                                item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Equipment)
                            {
                                if (item.ItemClass == EItemClass.Armor || item.ItemClass == EItemClass.Weapon)
                                {
                                    if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                                    {
                                        continue;
                                    }

                                    if (item.Place != EItemPlace.Equipment)
                                    {
                                        if (item.ItemQuality > Host.CharacterSettings.MaxItemQuality)
                                        {
                                            continue;
                                        }
                                    }

                                    if (item.RequiredLevel > Host.Me.Level)
                                    {
                                        continue;
                                    }

                                    var itemEquipType = GetItemEPlayerPartsType(item.InventoryType);

                                    if (item.ItemClass != EItemClass.Weapon)
                                    {
                                        continue;
                                    }

                                    if (item.InventoryType == EInventoryType.TwoHandedWeapon)
                                    {
                                        continue;
                                    }

                                    if (item.ItemClass == EItemClass.Weapon && !WeaponType.Contains((EItemSubclassWeapon)item.ItemSubClass))
                                    {
                                        continue;
                                    }

                                    var equip = true;
                                    foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                                    {
                                        if (characterSettingsItemSetting.Id == item.Id && characterSettingsItemSetting.Use == EItemUse.NotEquip)
                                        {
                                            equip = false;
                                        }
                                    }

                                    if (!equip)
                                    {
                                        continue;
                                    }

                                    if (Host.ClientType == EWoWClient.Retail)
                                    {
                                        if (itemEquipType != EEquipmentSlot.MainHand)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (item.InventoryType == EInventoryType.Weapon && itemEquipType == EEquipmentSlot.MainHand)
                                        {

                                        }
                                        else
                                        {
                                            if (itemEquipType != EEquipmentSlot.OffHand)
                                            {
                                                continue;
                                            }
                                        }
                                    }


                                    if (item.ItemClass == EItemClass.Weapon)
                                    {
                                        if (Host.CharacterSettings.EquipItemStat != 0)
                                        {
                                            if (!item.ItemStatType.Contains((EItemModType)Host.CharacterSettings.EquipItemStat))
                                            {
                                                continue;
                                            }
                                        }
                                    }


                                    if (equipCells[EEquipmentSlot.MainHand] == item)
                                    {
                                        continue;
                                    }

                                    if (equipCells[EEquipmentSlot.OffHand] == null)
                                    {
                                        equipCells[EEquipmentSlot.OffHand] = item;
                                    }
                                    else
                                    {
                                        double bestCoef = equipCells[EEquipmentSlot.OffHand].Level;
                                        double curCoef = item.Level;

                                        if (Host.ClientType == EWoWClient.Classic)
                                        {
                                            bestCoef = GetCoef(equipCells[EEquipmentSlot.OffHand]);
                                            curCoef = GetCoef(item);
                                        }


                                        if (bestCoef < curCoef)
                                        {
                                            equipCells[itemEquipType] = item;
                                            Host.SetVar(equipCells[itemEquipType], "coef", bestCoef);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }



                //  Host.log("Тест  EquipBestArmorAndWeapon 6");
                foreach (var b in equipCells.Keys.ToList())
                {
                    if (equipCells[b] != null && equipCells[b].Place != EItemPlace.Equipment)
                    {
                        if (equipCells[b].Equip())
                        {
                            Host.log("Одеваю " + equipCells[b].InventoryType + "  best item = " + equipCells[b].Name, LogLvl.Ok);
                            Thread.Sleep(Host.RandGenerator.Next(555, 1555));
                        }
                        else
                        {
                            Host.log("Error. Can't equip " + equipCells[b].InventoryType + "  " + b + "  best item = " + equipCells[b].Name + ". Reason = " + Host.GetLastError(),LogLvl.Error);
                            Thread.Sleep(Host.RandGenerator.Next(555, 1555));
                        }
                    }
                }

                GetBestTwoArmor();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetEquip()
        {
            switch (Host.Me.Class)
            {
                case EClass.None:
                    break;
                case EClass.Warrior:
                    {
                        WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.AXE, EItemSubclassWeapon.SWORD, EItemSubclassWeapon.AXE2 };
                        ArmorType = new List<EItemSubclassArmor>()
                    {
                        EItemSubclassArmor.PLATE, EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER,
                        EItemSubclassArmor.MAIL, EItemSubclassArmor.MISCELLANEOUS
                    };
                        if (Host.Me.Race == ERace.Troll)
                        {
                            WeaponType = new List<EItemSubclassWeapon>()
                            {EItemSubclassWeapon.AXE, EItemSubclassWeapon.DAGGER};
                            ArmorType = new List<EItemSubclassArmor>()
                        {
                            EItemSubclassArmor.PLATE, EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER,
                            EItemSubclassArmor.MAIL, EItemSubclassArmor.SHIELD, EItemSubclassArmor.MISCELLANEOUS
                        };
                            _weaponAndShield = true;
                        }
                    }
                    break;
                case EClass.Paladin:
                    {
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            /* WeaponType = new List<EItemSubclassWeapon>(){EItemSubclassWeapon.MACE2, EItemSubclassWeapon.AXE2, EItemSubclassWeapon.SWORD2};
                             ArmorType = new List<EItemSubclassArmor>(){EItemSubclassArmor.PLATE, EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER,EItemSubclassArmor.MAIL, EItemSubclassArmor.MISCELLANEOUS};*/
                            if (Host.GetBotLogin() == "easymoney")
                            {
                                WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.MACE };
                                ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.PLATE, EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER, EItemSubclassArmor.MAIL, EItemSubclassArmor.MISCELLANEOUS, EItemSubclassArmor.SHIELD };
                                _weaponAndShield = true;
                            }
                        }
                    }
                    break;
                case EClass.Hunter:
                    {
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.BOW, EItemSubclassWeapon.AXE };
                            ArmorType = new List<EItemSubclassArmor>()
                        {
                            EItemSubclassArmor.LEATHER, EItemSubclassArmor.CLOTH, EItemSubclassArmor.MAIL,
                            EItemSubclassArmor.MISCELLANEOUS,
                        };
                            if (Host.Me.Race == ERace.Tauren)
                            {
                                WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.GUN };
                            }
                        }
                        else
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.BOW, EItemSubclassWeapon.GUN };
                            ArmorType = new List<EItemSubclassArmor>()
                            {EItemSubclassArmor.CLOTH, EItemSubclassArmor.MAIL, EItemSubclassArmor.MISCELLANEOUS};
                        }
                    }
                    break;
                case EClass.Rogue:
                    {
                        WeaponType = new List<EItemSubclassWeapon>()
                        {EItemSubclassWeapon.DAGGER, EItemSubclassWeapon.SWORD};
                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.LEATHER, EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS};
                    }
                    break;
                case EClass.Priest:
                    {
                        WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.WAND, EItemSubclassWeapon.MACE };
                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS};
                    }
                    break;
                case EClass.DeathKnight:
                    {
                        WeaponType = new List<EItemSubclassWeapon>()
                    {
                        EItemSubclassWeapon.AXE2, EItemSubclassWeapon.SWORD2, EItemSubclassWeapon.POLEARM,
                        EItemSubclassWeapon.MACE2
                    };
                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.PLATE, EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS};
                    }
                    break;
                case EClass.Shaman:
                    {
                        //  WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.STAFF, EItemSubclassWeapon.POLEARM, EItemSubclassWeapon.MACE2 };
                        WeaponType = new List<EItemSubclassWeapon>()
                        {EItemSubclassWeapon.AXE, EItemSubclassWeapon.DAGGER, EItemSubclassWeapon.MACE};
                        ArmorType = new List<EItemSubclassArmor>()
                    {
                        EItemSubclassArmor.MAIL, EItemSubclassArmor.SHIELD, EItemSubclassArmor.CLOTH,
                        EItemSubclassArmor.MISCELLANEOUS
                    };
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            WeaponType = new List<EItemSubclassWeapon>()
                            {EItemSubclassWeapon.AXE, EItemSubclassWeapon.DAGGER, EItemSubclassWeapon.MACE};
                            ArmorType = new List<EItemSubclassArmor>()
                        {
                            EItemSubclassArmor.LEATHER, EItemSubclassArmor.SHIELD, EItemSubclassArmor.CLOTH,
                            EItemSubclassArmor.MISCELLANEOUS
                        };
                        }

                        _weaponAndShield = true;
                    }
                    break;
                case EClass.Mage:
                    {
                        WeaponType = new List<EItemSubclassWeapon>()
                    {
                        EItemSubclassWeapon.STAFF, EItemSubclassWeapon.WAND, EItemSubclassWeapon.MACE,
                        EItemSubclassWeapon.DAGGER
                    };
                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS};
                    }
                    break;
                case EClass.Warlock:
                    {
                        WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.STAFF };
                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS};
                    }
                    break;
                case EClass.Monk:
                    {
                        WeaponType = new List<EItemSubclassWeapon>()
                        {EItemSubclassWeapon.STAFF, EItemSubclassWeapon.POLEARM};

                        if (Host.Me.TalentSpecId == 269)
                        {
                            WeaponType = new List<EItemSubclassWeapon>()
                            {EItemSubclassWeapon.AXE, EItemSubclassWeapon.SWORD, EItemSubclassWeapon.FIST_WEAPON};
                            _twoWeapon = true;
                        }

                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER, EItemSubclassArmor.MISCELLANEOUS};
                    }
                    break;
                case EClass.Druid:
                    {
                        WeaponType = new List<EItemSubclassWeapon>()
                        {EItemSubclassWeapon.STAFF, EItemSubclassWeapon.POLEARM, EItemSubclassWeapon.MACE2};
                        ArmorType = new List<EItemSubclassArmor>()
                        {EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER, EItemSubclassArmor.MISCELLANEOUS};
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            ArmorType = new List<EItemSubclassArmor>()
                            {EItemSubclassArmor.LEATHER, EItemSubclassArmor.MISCELLANEOUS};
                        }
                    }
                    break;
                case EClass.DemonHunter:
                    break;
            }

            if (Host.CharacterSettings.AdvancedEquip)
            {
                WeaponType.Clear();
                ArmorType.Clear();
                StatCoefs.Clear();
                foreach (var advancedEquipWeapon in Host.CharacterSettings.AdvancedEquipsWeapon)
                {
                    if (!advancedEquipWeapon.Use)
                    {
                        continue;
                    }

                    WeaponType.Add(advancedEquipWeapon.WeaponType);
                }

                foreach (var characterSettingsAdvancedEquipArmor in Host.CharacterSettings.AdvancedEquipArmors)
                {
                    if (!characterSettingsAdvancedEquipArmor.Use)
                    {
                        continue;
                    }

                    ArmorType.Add(characterSettingsAdvancedEquipArmor.ArmorType);
                }

                _weaponAndShield = Host.CharacterSettings.AdvancedEquipUseShield;
                _twoWeapon = Host.CharacterSettings.TwoWeapon;

                foreach (var characterSettingsAdvancedEquipStat in Host.CharacterSettings.AdvancedEquipStats)
                {
                    StatCoefs.Add(characterSettingsAdvancedEquipStat.StatType, characterSettingsAdvancedEquipStat.Coef);
                }
            }
        }
    }
}