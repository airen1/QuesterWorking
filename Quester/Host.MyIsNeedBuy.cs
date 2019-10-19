using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WoWBot.Core;

namespace WowAI
{
    internal partial class Host
    {
        internal bool MyIsNeedBuy()
        {
            foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
            {
                if (characterSettingsItemSetting.Use != EItemUse.Buy)
                    continue;
                if (MeGetItemsCount(characterSettingsItemSetting.Id, true, true) >= characterSettingsItemSetting.MinCount)
                    continue;
                if (Me.Level < characterSettingsItemSetting.MeLevel)
                    continue;
                if (characterSettingsItemSetting.BuyPricePerOne != 0)
                    if (Me.Money < characterSettingsItemSetting.BuyPricePerOne * characterSettingsItemSetting.MinCount)
                        continue;

                log("Необходимо купить " + characterSettingsItemSetting.Name + " " + MeGetItemsCount(characterSettingsItemSetting.Id, true, true) + "/" + characterSettingsItemSetting.MinCount, LogLvl.Important);
                return true;
            }
            return false;
        }

        public void MyBuyItems(bool check = true)
        {
            try
            {
                log("Проверяю покупку");
                var listItem = new Dictionary<uint, List<ItemSettings>>();
                foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
                {
                    if (characterSettingsItemSetting.Use != EItemUse.Buy)
                        continue;
                    if (Me.Level < characterSettingsItemSetting.MeLevel)
                        continue;
                    if (characterSettingsItemSetting.BuyPricePerOne != 0)
                        if (Me.Money < characterSettingsItemSetting.BuyPricePerOne * characterSettingsItemSetting.MinCount)
                            continue;
                    if (MeGetItemsCount(characterSettingsItemSetting.Id, true) > characterSettingsItemSetting.MaxCount)
                        continue;
                    if (listItem.ContainsKey(characterSettingsItemSetting.Id))
                    {
                        listItem[characterSettingsItemSetting.Id].Add(characterSettingsItemSetting);
                    }
                    else
                    {
                        listItem.Add(characterSettingsItemSetting.Id, new List<ItemSettings> { characterSettingsItemSetting });
                    }
                }

                foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
                {
                    if (Me.Level < characterSettingsItemSetting.MeLevel)
                        continue;
                    if (characterSettingsItemSetting.BuyPricePerOne != 0)
                    {
                        if (Me.Money < characterSettingsItemSetting.BuyPricePerOne * characterSettingsItemSetting.MinCount)
                            continue;
                    }
                    if (check)
                    {
                        // log("Дистанция до НПС который продает " + characterSettingsItemSetting.Name + " " + Me.Distance(characterSettingsItemSetting.Loc));
                        if (Me.Distance(characterSettingsItemSetting.Loc) > 200)
                        {
                            continue;
                        }

                    }
                    if (characterSettingsItemSetting.Use == EItemUse.Buy)
                    {
                        while (MeGetItemsCount(characterSettingsItemSetting.Id, true, true) < characterSettingsItemSetting.MaxCount && FarmModule.ReadyToActions)
                        {
                            if (!check)
                                if (CharacterSettings.UseStoneForSellAndRepair)
                                    MyUseStone();

                            var count = characterSettingsItemSetting.MaxCount - MeGetItemsCount(characterSettingsItemSetting.Id, true, true);
                            log("Надо купить " + characterSettingsItemSetting.Name + " [" + characterSettingsItemSetting.Id + "]" + count + " в наличии " + MeGetItemsCount(characterSettingsItemSetting.Id, true, true));
                            Thread.Sleep(1000);
                            if (!MainForm.On)
                            {
                                log(MainForm.On + "");
                                return;
                            }


                            Entity npc = null;


                            if (listItem.ContainsKey(characterSettingsItemSetting.Id))
                            {
                                foreach (var itemSettingse in listItem[characterSettingsItemSetting.Id].OrderBy(i => Me.Distance(i.Loc)))
                                {
                                    npc = GetNpcById(itemSettingse.NpcId);
                                    if (npc == null)
                                    {
                                        if (Me.Distance(itemSettingse.Loc) > 1000)
                                            if (!MyUseTaxi(itemSettingse.AreaId, itemSettingse.Loc))
                                                return;
                                        CommonModule.MoveTo(itemSettingse.Loc, 1);
                                    }

                                    npc = GetNpcById(itemSettingse.NpcId);
                                    break;
                                }
                            }
                            else
                            {
                                npc = GetNpcById(characterSettingsItemSetting.NpcId);
                                if (npc == null)
                                {
                                    if (Me.Distance(characterSettingsItemSetting.Loc) > 1000)
                                        if (!MyUseTaxi(characterSettingsItemSetting.AreaId, characterSettingsItemSetting.Loc))
                                            return;
                                    CommonModule.MoveTo(characterSettingsItemSetting.Loc, 1);
                                }

                            }


                            if (npc != null)
                            {
                                if (Me.Distance(npc) > 3)
                                    CommonModule.MoveTo(npc, 1);
                                Thread.Sleep(1000);
                                MyCheckIsMovingIsCasting();
                                if (CurrentInteractionGuid != npc.Guid)
                                    if (!OpenShop(npc as Unit))
                                        log("Не смог открыть диалог с нпс " + npc.Name + "  " + GetLastError(), LogLvl.Error);

                                Thread.Sleep(1000);
                                if (GetVendorItems().Count == 0)
                                {
                                    log("Нет предметов на продажу " + npc.Name);
                                }

                                MySellItems();
                                foreach (var item in GetVendorItems())
                                {
                                    if (item.ItemId == characterSettingsItemSetting.Id && ItemManager.GetFreeInventorySlotsCount() > 1)
                                    {
                                        if (item.Price > Convert.ToInt64(Me.Money))
                                        {
                                            log("Не хватает золота " + item.Price + " /" + Convert.ToInt64(Me.Money), LogLvl.Error);
                                            return;
                                        }
                                        /* if (item.StackCount < count)
                                             count = item.StackCount;
                                         if (count == 0)
                                             count = 1;*/
                                        log("Покупаю " + count + "   " + item.StackCount + "  " + GameDB.ItemTemplates[item.ItemId]?.GetMaxStackSize());
                                        var result = item.Buy(item.StackCount);
                                        if (result != EBuyResult.Success)
                                            log("Не удалось купить " + result, LogLvl.Error);
                                        else
                                        {
                                            log("Купил", LogLvl.Ok);
                                        }
                                        Thread.Sleep(1000);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                log("Нет НПС для покупки " + characterSettingsItemSetting.NpcId);
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                log(e + "");
            }
        }
    }
}