using Out.Internal.Core;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using WoWBot.Core;
using WowAI;

namespace WowAI.Module
{
    internal partial class AutoQuests
    {
        private void Mode_86()
        {
            try
            {
                if (Host.CharacterSettings.RunQuestHerbalism)
                    if (!CheckHerbalism())
                        return;

                if (Host.CharacterSettings.PikPocket && Host.CharacterSettings.PikPocketMapId == 189 && Host.MapID == 189)
                {

                    if (Host.Me.Distance(220.64, -303.54, 18.53) < 500)
                        Host.FarmModule.IsWing1 = true;
                    else
                        Host.FarmModule.IsWing1 = false;

                    if (Host.FarmModule.IsWing1)
                    {
                        var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing1.xml";
                        Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                        var reader = new XmlSerializer(typeof(DungeonSetting));
                        using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                            Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                    }
                    else
                    {
                        var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing2.xml";
                        Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                        var reader = new XmlSerializer(typeof(DungeonSetting));
                        using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                            Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                    }


                }

                ScriptStopwatch = new Stopwatch();
                ScriptStopwatch.Start();


                var mobsStart = Host.KillMobsCount;
                double tempGold = Host.Me.Money;
                var goldStart = tempGold / 10000;
                long startinvgold = 0;

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        //   log(item.Id + "  "+ item.Name);
                        //  startinvgold = startinvgold + item.GetSellPrice() * item.Count;
                    }
                }


                double bestPoint = 999999;
                var bestpoint = new DungeonCoordSettings();
                var lastPoint = new DungeonCoordSettings();
                DungeonCoordSettings checkPoint = null;
                var reverse = false;
                if (Host.CharacterSettings.ScriptScheduleEnable)
                {
                    foreach (var characterSettingsScriptSchedule in Host.CharacterSettings.ScriptSchedules)
                    {
                        if (!DateTime.Now.TimeOfDay.IsBetween(characterSettingsScriptSchedule.ScriptStartTime,
                            characterSettingsScriptSchedule.ScriptStopTime))
                            continue;

                        if (ShedulePlugin == characterSettingsScriptSchedule.ScriptName)
                            break;


                        var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\" +
                                         characterSettingsScriptSchedule.ScriptName;
                        // ScriptName = AssemblyDirectory + "\\Script\\" + CharacterSettings.Script;
                        Host.log("Применяю скрипт: " + scriptName + " Текущее время: " + DateTime.Now + " Время запуска " + characterSettingsScriptSchedule.ScriptStartTime + " Время окончания" +
                            characterSettingsScriptSchedule.ScriptStopTime, LogLvl.Ok);

                        var reader = new XmlSerializer(typeof(DungeonSetting));
                        using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                            Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                        ShedulePlugin = characterSettingsScriptSchedule.ScriptName;
                        if (characterSettingsScriptSchedule.Reverse)
                            reverse = true;
                        break;
                    }

                    //  Host.log("Выбрал скрипт " + shedulePlugin);
                }

                var scriptActionList = Host.DungeonSettings.DungeonCoordSettings;

                if (Host.CharacterSettings.ScriptReverse || reverse)
                    scriptActionList.Reverse();

                if (!Host.CharacterSettings.RunScriptFromBegin)
                    foreach (var dungeonSettingsScriptCoordSetting in scriptActionList)
                    {
                        if (dungeonSettingsScriptCoordSetting.Action != "Бежать на точку")
                            continue;
                        if (checkPoint == null)
                        {
                            checkPoint = dungeonSettingsScriptCoordSetting;
                        }

                        lastPoint = dungeonSettingsScriptCoordSetting;
                        if (dungeonSettingsScriptCoordSetting.AreaId != Host.Area.Id)
                            continue;
                        if (dungeonSettingsScriptCoordSetting.MapId != Host.MapID)
                            continue;

                        if (Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc) < bestPoint)
                        {
                            bestPoint = Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc);
                            bestpoint = dungeonSettingsScriptCoordSetting;
                        }
                    }


                if (bestpoint == lastPoint) // если лучшая точка последняя, то начать сначала
                {
                    bestpoint = scriptActionList[0];
                    bestPoint = 999999;
                }

                if (checkPoint != null && checkPoint == bestpoint)
                {
                    bestpoint = scriptActionList[0];
                    bestPoint = 999999;
                }

                if (Host.CharacterSettings.LogScriptAction)
                    Host.log("Начал выполение скрипта " + Host.Me.Name + "  " + ScriptStopwatch.ElapsedMilliseconds,
                        LogLvl.Ok);

                for (var index = 0; index < scriptActionList.Count; index++)
                {
                    while (Host.GameState != EGameState.Ingame)
                    {
                        if (Host.GameState == EGameState.Offline)
                            return;
                        if (!Host.MainForm.On)
                            return;
                        Thread.Sleep(100);
                    }

                    while (!Host.CheckCanUseGameActions())
                    {
                        if (Host.GameState == EGameState.Offline)
                            return;
                        if (!Host.MainForm.On)
                            return;
                        Thread.Sleep(100);
                    }

                    while (Host.CommonModule.IsMoveSuspended())
                    {
                        if (Host.GameState == EGameState.Offline)
                            return;
                        if (!Host.MainForm.On)
                            return;
                        Thread.Sleep(100);
                    }

                    if (Host.CharacterSettings.CheckSellAndRepairScript)
                    {
                        if (Host.GetAgroCreatures().Count == 0)
                            if (NeedActionNpcSell || NeedActionNpcRepair || Host.IsNeedAuk() || Host.MyIsNeedSell() ||
                                Host.MyIsNeedRepair())
                                break;
                    }

                    if (!Host.MainForm.On)
                        break;
                    if (!Host.IsAlive(Host.Me))
                        break;
                    if (Host.Me.IsDeadGhost)
                        break;

                    if (Host.CharacterSettings.SendMail && !Send &&
                        Host.MapID == Host.CharacterSettings.SendMailLocMapId &&
                        DateTime.Now.TimeOfDay.IsBetween(Host.CharacterSettings.SendMailStartTime,
                            Host.CharacterSettings.SendMailStopTime))
                    {
                        while (!Send)
                        {
                            Thread.Sleep(1000);
                            if (!Host.MainForm.On)
                                return;
                            if (!Host.CommonModule.MoveTo(Host.CharacterSettings.SendMailLocX,
                                Host.CharacterSettings.SendMailLocY, Host.CharacterSettings.SendMailLocZ, 10))
                                continue;

                            GameObject mailBox = null;
                            foreach (var gameObject in Host.GetEntities<GameObject>().OrderBy(i => Host.Me.Distance(i)))
                            {
                                if (gameObject.GameObjectType != EGameObjectType.Mailbox)
                                    continue;
                                mailBox = gameObject;
                                break;
                            }

                            var result = ((double)Host.Me.Money / 100) * 99;
                            if (result < 5000000)
                            {
                                result = Host.Me.Money - 5000000;
                                if (result < 0)
                                    result = 0;
                            }

                            var itemList = new List<Item>();


                            foreach (var item in Host.ItemManager.GetItems())
                            {
                                if (itemList.Count >= 12)
                                    break;
                                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                                    item.Place == EItemPlace.InventoryItem)
                                {
                                    foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                                    {
                                        if (item.Id == characterSettingsItemSetting.Id &&
                                            characterSettingsItemSetting.Use == EItemUse.SendByMail)
                                        {
                                            itemList.Add(item);
                                            break;
                                        }
                                    }
                                }
                            }


                            if (mailBox != null)
                            {
                                var next = false;
                                if (itemList.Count > 0)
                                    next = true;
                                Host.ForceComeTo(mailBox, 2);
                                Host.MyCheckIsMovingIsCasting();
                                Thread.Sleep(1000);
                                if (!Host.OpenMailbox(mailBox))
                                    Host.log("Не удалось открыть ящик " + Host.GetLastError(), LogLvl.Error);
                                else
                                    Host.log("Открыл ящик", LogLvl.Ok);
                                Thread.Sleep(2000);
                                if (!Host.SendMail(Host.CharacterSettings.SendMailName, "123", "",
                                    Convert.ToInt64(result), itemList))
                                    Host.log(
                                        "Не смог отправить письмо " + Convert.ToInt64(result) + "   " +
                                        Host.GetLastError(), LogLvl.Error);
                                else
                                {
                                    Host.log("Отправил письмо " + Convert.ToInt64(result), LogLvl.Ok);
                                }

                                if (next)
                                {
                                    Thread.Sleep(5000);
                                    continue;
                                }

                                Send = true;
                            }
                        }
                    }

                    if (NeedFindBestPoint)
                    {
                        Host.log("Ищу новую ближайшую точку");
                        foreach (var dungeonSettingsScriptCoordSetting in scriptActionList)
                        {
                            if (dungeonSettingsScriptCoordSetting.Action != "Бежать на точку")
                                continue;
                            if (dungeonSettingsScriptCoordSetting.AreaId != Host.Area.Id)
                                continue;
                            if (dungeonSettingsScriptCoordSetting.MapId != Host.MapID)
                                continue;

                            if (Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc.X,
                                    dungeonSettingsScriptCoordSetting.Loc.Y, dungeonSettingsScriptCoordSetting.Loc.Z)
                                /* Host.FarmModule.GetDistToMobFromMech(dungeonSettingsScriptCoordSetting.Loc)*/ <
                                bestPoint)
                            {
                                // host.log("Нашел " + dungeonSettingsScriptCoordSetting.Id);
                                bestPoint = Host.Me.Distance(dungeonSettingsScriptCoordSetting.Loc.X,
                                    dungeonSettingsScriptCoordSetting.Loc.Y, dungeonSettingsScriptCoordSetting.Loc.Z);
                                bestpoint = dungeonSettingsScriptCoordSetting;
                            }
                        }

                        NeedFindBestPoint = false;
                    }


                    var dungeon = scriptActionList[index];
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (bestPoint != 999999 && bestpoint != dungeon)
                    {
                        continue;
                    }
                    else
                    {
                        bestPoint = 999999;
                    }

                    if (!Host.MyOpenTaxyRoute())
                        continue;

                    if (Host.CharacterSettings.LogScriptAction)
                        Host.log(dungeon.Id + ")Выполняю действие: " + dungeon.Action + " координаты:" + dungeon.Loc + "  Attack:" + dungeon.Attack + " дист:" + Host.Me.Distance(dungeon.Loc), LogLvl.Important);

                    if (Host.CharacterSettings.PikPocket)
                    {
                        if (!Host.MyOpenLockedChest())
                            return;
                        Host.AdvancedInvisible();
                    }


                    switch (dungeon.Action)
                    {
                        case "Загрузить профиль":
                            {
                                Host.LoadSettingsForQp(dungeon.Com);
                                Host.ApplySettings();
                                return;
                            }


                        case "Использовать портал":
                            {
                                foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i)))
                                {
                                    if (!entity.IsSpellClick)
                                        continue;
                                    if (Host.Me.Distance(entity) > 3)
                                        Host.CommonModule.MoveTo(entity, 2);
                                    Host.MyUseSpellClick(entity.Id);
                                    Thread.Sleep(1000);
                                    while (Host.GameState != EGameState.Ingame)
                                    {
                                        if (!Host.MainForm.On)
                                            return;
                                        Thread.Sleep(500);
                                    }

                                    break;
                                }
                            }
                            break;

                        case "Купить нитки":
                            {
                                if (Host.MeGetItemsCount(159959) < dungeon.Pause)
                                {
                                    while (Host.Me.Distance(-1075, 780, 435) > 20)
                                    {
                                        if (!Host.MainForm.On)
                                            return;
                                        Host.CommonModule.MoveTo(-1075, 780, 435, 3);
                                        Thread.Sleep(1000);
                                    }

                                    var npc = Host.GetNpcById(141928);
                                    if (npc != null)
                                    {
                                        Host.CommonModule.MoveTo(npc, 3);
                                        Thread.Sleep(2000);
                                        if (!Host.OpenShop(npc as Unit))
                                            Host.log("Не смог открыть диалог " + npc.Name + " " + Host.GetLastError(),
                                                LogLvl.Error);
                                        Thread.Sleep(2000);
                                        foreach (var item in Host.GetVendorItems())
                                        {
                                            if (item.ItemId != 159959)
                                                continue;
                                            if (!Host.GameDB.ItemTemplates.ContainsKey(item.ItemId))
                                            {
                                                Host.log("Не нашел в базе" + item.ItemId);
                                                continue;
                                            }

                                            var itemTemplate = Host.GameDB.ItemTemplates[item.ItemId];
                                            Host.log("Предметы на продажу: " + item.ItemId + " " + item.Price + " " +
                                                     itemTemplate.GetName() + " " + itemTemplate.GetMaxStackSize() +
                                                     item.Quantity);
                                            var needcount = dungeon.Pause - Host.MeGetItemsCount(159959);
                                            var needSlot = needcount / itemTemplate.GetMaxStackSize() + 1;
                                            var needGold = itemTemplate.GetMaxStackSize() * item.Price;
                                            Host.log("Нужно " + needcount + " шт. " + needSlot + " слот " +
                                                     (needGold / 10000f).ToString("F2") + " г.");


                                            while (needcount > 0)
                                            {
                                                if (Host.ItemManager.GetFreeInventorySlotsCount() < 11)
                                                {
                                                    Host.log(
                                                        "Нет свободных слотов " +
                                                        Host.ItemManager.GetFreeInventorySlotsCount(), LogLvl.Error);
                                                    break;
                                                }

                                                if (Host.Me.Money < Convert.ToUInt64(needGold))
                                                {
                                                    Host.log("Не хватает денег " + Host.Me.Money + "/" + needGold);
                                                    break;
                                                }

                                                Thread.Sleep(100);
                                                if (!Host.MainForm.On)
                                                    return;
                                                var buyCount = Math.Min(Convert.ToInt32(needcount),
                                                    Convert.ToInt32(itemTemplate.GetMaxStackSize()));
                                                var res = item.Buy(buyCount);
                                                if (res != EBuyResult.Success)
                                                {
                                                    Host.log(
                                                        "Не смог купить " + itemTemplate.GetName() + "  " + buyCount + " " +
                                                        res + " " + Host.GetLastError(), LogLvl.Error);
                                                    Thread.Sleep(10000);
                                                    break;
                                                }

                                                Thread.Sleep(2000);
                                                needcount = dungeon.Pause - Host.MeGetItemsCount(159959);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "Крафт":
                            {
                                CraftRecept craft;

                                switch (dungeon.SkillId)
                                {
                                    case 257099:
                                        {
                                            craft = new CraftRecept
                                            {
                                                Id = 257099,
                                                CraftIngridients = new List<CraftIngridient>
                                        {
                                            new CraftIngridient {Id = 152576, Count = 17},
                                            new CraftIngridient {Id = 159959, Count = 10}
                                        }
                                            };
                                        }
                                        break;

                                    default:
                                        {
                                            Host.log("Неизветный рецепт крафта ", LogLvl.Error);
                                            continue;
                                        }
                                }

                                while (Host.MeGetItemsCount(craft.CraftIngridients[0].Id) > craft.CraftIngridients[0].Count)
                                {
                                    Thread.Sleep(100);
                                    if (Host.ItemManager.GetFreeInventorySlotsCount() < 3)
                                    {
                                        Host.log("Необходима продажа ", LogLvl.Important);
                                        Host.MyMoveToSell(true);
                                    }

                                    Host.CanselForm();
                                    if (Host.Me.MountId != 0)
                                    {
                                        Host.CommonModule.MyUnmount();
                                        Thread.Sleep(2000);
                                    }

                                    var craftSpell = Host.SpellManager.GetSpell(craft.Id);
                                    var count = Host.MeGetItemsCount(craft.CraftIngridients[0].Id) /
                                                craft.CraftIngridients[0].Count;
                                    Host.log("Можно скрафтить " + craftSpell.Name + "[" + craftSpell.Id + "] " + count +
                                             " шт.");
                                    if (!Host.MainForm.On)
                                        return;
                                    if (Host.Me.Team == ETeam.Horde)
                                    {
                                        while (Host.Me.Distance(-891.93, 975.31, 321.12) > 20)
                                        {
                                            if (!Host.MainForm.On)
                                                return;
                                            Host.CommonModule.MoveTo(-891.93, 975.31, 321.12);
                                            Thread.Sleep(1000);
                                        }

                                        if (Host.MeGetItemsCount(craft.CraftIngridients[1].Id) <
                                            craft.CraftIngridients[1].Count)
                                        {
                                            var npc = Host.GetNpcById(141609);
                                            if (npc != null)
                                            {
                                                Host.CommonModule.MoveTo(npc, 3);
                                                if (!Host.OpenShop(npc as Unit))
                                                    Host.log("Не смог открыть диалог " + npc.Name);
                                                foreach (var item in Host.GetVendorItems())
                                                {
                                                    if (item.ItemId != craft.CraftIngridients[1].Id)
                                                        continue;
                                                    if (!Host.GameDB.ItemTemplates.ContainsKey(item.ItemId))
                                                    {
                                                        Host.log("Не нашел в базе" + item.ItemId);
                                                        continue;
                                                    }

                                                    var itemTemplate = Host.GameDB.ItemTemplates[item.ItemId];
                                                    Host.log("Предметы на продажу: " + item.ItemId + " " + item.Price +
                                                             " " + itemTemplate.GetName() + " " +
                                                             itemTemplate.GetMaxStackSize() + item.Quantity);
                                                    var needcount =
                                                        count * craft.CraftIngridients[1].Count -
                                                        Host.MeGetItemsCount(craft.CraftIngridients[1].Id);
                                                    var needSlot = needcount / itemTemplate.GetMaxStackSize() + 1;
                                                    var needGold = itemTemplate.GetMaxStackSize() * item.Price;
                                                    Host.log("Нужно " + needcount + " шт. " + needSlot + " слот " +
                                                             (needGold / 10000f).ToString("F2") + " г.");


                                                    while (needcount > 0)
                                                    {
                                                        if (Host.ItemManager.GetFreeInventorySlotsCount() < 11)
                                                        {
                                                            Host.log(
                                                                "Нет свободных слотов " +
                                                                Host.ItemManager.GetFreeInventorySlotsCount(),
                                                                LogLvl.Error);
                                                            break;
                                                        }

                                                        if (Host.Me.Money < Convert.ToUInt64(needGold))
                                                        {
                                                            Host.log("Не хватает денег " + Host.Me.Money + "/" + needGold);
                                                            break;
                                                        }

                                                        Thread.Sleep(100);
                                                        if (!Host.MainForm.On)
                                                            return;
                                                        var buyCount = Math.Min(Convert.ToInt32(needcount),
                                                            Convert.ToInt32(itemTemplate.GetMaxStackSize()));
                                                        var res = item.Buy(buyCount);
                                                        if (res != EBuyResult.Success)
                                                        {
                                                            Host.log(
                                                                "Не смог купить " + itemTemplate.GetName() + "  " +
                                                                buyCount + " " + res + " " + Host.GetLastError(),
                                                                LogLvl.Error);
                                                            Thread.Sleep(10000);
                                                            break;
                                                        }

                                                        Thread.Sleep(2000);
                                                        needcount = count * craft.CraftIngridients[1].Count -
                                                                    Host.MeGetItemsCount(craft.CraftIngridients[1].Id);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var res = Host.SpellManager.CastSpell(craftSpell.Id);
                                            if (res != ESpellCastError.SUCCESS)
                                            {
                                                Host.log(
                                                    "Не смог скрафтить " + craftSpell.Name + "  " + " " + res + " " +
                                                    Host.GetLastError(), LogLvl.Error);
                                                Thread.Sleep(10000);
                                            }

                                            while (Host.SpellManager.IsCasting)
                                                Thread.Sleep(100);
                                            // крафт
                                        }
                                    }

                                    Thread.Sleep(500);
                                }

                                Host.MyMoveToSell(true);
                            }
                            break;

                        case "Ремонт":
                            {
                                if (Host.GetBotLogin() == "wowklausvovot")
                                {
                                }
                                else
                                {
                                    Host.MyUseStone(true);
                                    if (Host.Me.Team == ETeam.Horde)
                                        if (Host.Area.Id != 1637)
                                        {
                                            Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }

                                    if (Host.Me.Team == ETeam.Alliance)
                                        if (Host.Area.Id != 1519)
                                        {
                                            Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }
                                }

                                Host.MyMoveToRepair();
                            }
                            break;
                        case "Почта":
                            {
                                Host.MyUseStone(true);
                                if (Host.Me.Team == ETeam.Horde)
                                    if (Host.Area.Id != 1637)
                                    {
                                        Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                if (Host.Me.Team == ETeam.Alliance)
                                    if (Host.Area.Id != 1519)
                                    {
                                        Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                Host.MyMail();
                            }
                            break;
                        case "Аукцион":
                            {
                                Host.MyUseStone(true);
                                if (Host.Me.Team == ETeam.Horde)
                                    if (Host.Area.Id != 1637)
                                    {
                                        Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }

                                if (Host.Me.Team == ETeam.Alliance)
                                    if (Host.Area.Id != 1519)
                                    {
                                        Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                        Thread.Sleep(5000);
                                        continue;
                                    }


                                Host.Auk();
                            }
                            break;

                        case "Включить сбор шкуры":
                            {
                                EnableSkinning = true;
                            }
                            break;
                        case "Выключить сбор шкуры":
                            {
                                EnableSkinning = false;
                            }
                            break;

                        case "Включить сбор":
                            {
                                EnableFarmProp = true;
                            }
                            break;
                        case "Выключить сбор":
                            {
                                EnableFarmProp = false;
                            }
                            break;
                        case "Рыбалка":
                            {
                                if (Host.Me.Distance(dungeon.Loc) > 15)
                                {
                                    Host.log("Нахожусь далеко от точки для рыбалки, начинаю скрипт заново");
                                    return;
                                }

                                var isTimer = false;
                                var timer = new Stopwatch();
                                if (dungeon.Pause != 0)
                                {
                                    isTimer = true;
                                    timer.Start();
                                }

                                var state = Host.FarmModule.FarmState;
                                Host.FarmModule.FarmState = FarmState.Disabled;
                                Host.onGameObjectCustomAnim += CheckGo;
                                WaitTeleport = true;
                                while (true)
                                {
                                    Thread.Sleep(100);
                                    if (Host.Me.GetThreats().Count > 0)
                                    {
                                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                        continue;
                                    }

                                    if (!Host.MainForm.On)
                                        break;
                                    Host.CanselForm();
                                    StartWait = true;
                                    if (isTimer)
                                    {
                                        if (timer.Elapsed.TotalSeconds > dungeon.Pause)
                                            break;
                                    }

                                    Host.MyCheckIsMovingIsCasting();
                                    var result = Host.SpellManager.CastSpell(131474);
                                    if (result != ESpellCastError.SUCCESS)
                                    {
                                        Host.log("Не удалось закинуть удочку" + result, LogLvl.Error);
                                        break;
                                    }

                                    else
                                    {
                                        for (var i = 0; i < 220; i++)
                                        {
                                            if (!Host.MainForm.On)
                                                break;
                                            if (!StartWait)
                                                break;
                                            Thread.Sleep(100);
                                        }
                                    }

                                    Thread.Sleep(Host.RandGenerator.Next(500, 1000));
                                    if (Host.CanPickupLoot())
                                        Host.PickupLoot();
                                    Thread.Sleep(Host.RandGenerator.Next(100, 500));

                                    if (Host.CharacterSettings.ScriptScheduleEnable)
                                    {
                                        foreach (var characterSettingsScriptSchedule in Host.CharacterSettings
                                            .ScriptSchedules)
                                        {
                                            if (!DateTime.Now.TimeOfDay.IsBetween(
                                                characterSettingsScriptSchedule.ScriptStartTime,
                                                characterSettingsScriptSchedule.ScriptStopTime))
                                                continue;

                                            if (ShedulePlugin == characterSettingsScriptSchedule.ScriptName)
                                                break;


                                            return;
                                        }
                                    }

                                    if (Host.MyIsNeedSell() || Host.MyIsNeedRepair())
                                    {
                                        NeedActionNpcSell = true;
                                        NeedActionNpcRepair = true;
                                        return;
                                    }
                                }

                                Host.onGameObjectCustomAnim -= CheckGo;
                                Host.FarmModule.FarmState = state;
                                WaitTeleport = false;
                            }
                            break;
                        case "Запустить другой плагин":
                            {
                                Host.CommonModule.MyUnmount();
                                Host.LaunchPlugin(dungeon.PluginPath);
                                Thread.Sleep(2000);
                                while (Host.IsPluginLaunched(dungeon.PluginPath))
                                {
                                    Thread.Sleep(1000);
                                    Host.log("Ожидаю выполения плагина " + dungeon.PluginPath);
                                    if (!Host.MainForm.On)
                                        return;
                                }
                                Host.log("Плагин завершил работу, продолжаю выполнение");
                            }
                            break;

                        case "Выключить плагин":
                            {
                                Host.CancelRequested = true;
                                Host.StopPluginNow();
                            }
                            break;
                        case "Проверить инвентарь":
                            {
                                if (Host.MyIsNeedSell() || Host.MyIsNeedRepair())
                                {
                                    NeedActionNpcSell = true;
                                    NeedActionNpcRepair = true;
                                    return;
                                }
                            }
                            break;

                        case "Выбрать диалог":
                            {
                                if (Host.MapID == 43 && Host.Area.Id == 718)
                                {
                                    var npc = Host.GetNpcById(3678);
                                    if (npc != null)
                                    {
                                        Host.CommonModule.ForceMoveTo(npc.Location);
                                        Thread.Sleep(500);

                                        if (!Host.OpenDialog(npc))
                                        {
                                            Host.log(
                                                "Не смог начать диалог для выбора диалога с " + npc.Name + "[" + npc.Id +
                                                "] " + Host.GetLastError(), LogLvl.Error);
                                            if (Host.GetLastError() == ELastError.ActionNotAllowed)
                                            {
                                                Host.MySendKeyEsc();
                                            }
                                        }

                                        Thread.Sleep(500);
                                        var isFindDialog = false;
                                        foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                        {
                                            if (gossipOptionsData.Text.Contains("Нужен ру перевод") ||
                                                gossipOptionsData.Text.Contains("event begin"))
                                            {
                                                isFindDialog = true;
                                                if (!Host.SelectNpcDialog(gossipOptionsData))
                                                    Host.log("Не смог выбрать диалог " + Host.GetLastError(),
                                                        LogLvl.Error);
                                            }

                                            Host.log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                                                     gossipOptionsData.ClientOption + "  ");
                                        }

                                        if (!isFindDialog)
                                        {
                                            Host.log("Необходим диалог ");
                                            Thread.Sleep(5000);
                                        }
                                    }
                                    else
                                    {
                                        Host.log("Не найден НПС ", LogLvl.Error);
                                    }
                                }
                                else
                                {
                                    Host.log("Выбрать диалог, находимся не в той зоне " + Host.MapID + " " + Host.Area.Id,
                                        LogLvl.Error);
                                }
                            }
                            break;


                        case "Использовать скилл":
                            {
                                if (!dungeon.Attack)
                                    Host.FarmModule.FarmState = FarmState.Disabled;
                                else
                                    Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;

                                var skill = Host.SpellManager.GetSpell(dungeon.SkillId);
                                if (skill.Name == "Stealth")
                                {
                                    var next = false;
                                    foreach (var aura in Host.Me.GetAuras())
                                    {
                                        if (aura.SpellName == "Stealth")
                                        {
                                            aura.Cancel();
                                            next = true;
                                        }
                                    }

                                    if (next)
                                        continue;
                                }


                                if (skill != null)
                                {
                                    var isMoon = false;
                                    if (skill.Id == 5215)
                                    {
                                        if (Host.MyGetAura(skill.Id) != null)
                                            continue;
                                    }


                                    if (skill.Id == 18960 || skill.Id == 193753)
                                    {
                                        Host.CanselForm();
                                        Host.CommonModule.MyUnmount();

                                        //Вывести из данжа группу
                                        if (Host.Group.GetMembers().Count > 0)
                                        {
                                            //var allMove = false;
                                            if (ScriptStopwatch.Elapsed.Minutes > 2 && ScriptStopwatch.Elapsed.Minutes < 6)
                                            {
                                                while (ScriptStopwatch.Elapsed.Minutes < 5 && Host.MainForm.On)
                                                {
                                                    Thread.Sleep(10000);
                                                    Host.log("Ожидаю до 5 минут " + ScriptStopwatch.Elapsed.Minutes + " " +
                                                             ScriptStopwatch.Elapsed.Seconds);
                                                }
                                            }
                                        }

                                        Thread.Sleep(1000);
                                    }

                                    if (!Host.MainForm.On)
                                        return;

                                    if (skill.Id == 18960 && Host.MapID == 1 && Host.Area.Id == 493)
                                    {
                                        isMoon = true;
                                    }

                                    var isNeedWaitKd = false;
                                    if (skill.Id == 193753)
                                        isNeedWaitKd = true;
                                    if (skill.Id == 193753 && Host.MapID == 1540 && Host.Area.Id == 7979)
                                    {
                                        isMoon = true;
                                    }


                                    Host.MyCheckIsMovingIsCasting();
                                    while (Host.SpellManager.IsChanneling)
                                        Thread.Sleep(50);
                                    /* while (!Host.CheckCanUseGameActions() && Host.Me.IsAlive)
                                             Thread.Sleep(50);*/
                                    //   Host.IsRegen = true;
                                    if (isNeedWaitKd)
                                    {
                                        if (Host.SpellManager.GetSpellCooldown(dungeon.SkillId) != 0)
                                        {
                                            while (Host.SpellManager.GetSpellCooldown(dungeon.SkillId) != 0)
                                            {
                                                Thread.Sleep(1000);
                                                Host.log("Ожидаю кд " +
                                                         Host.SpellManager.GetSpellCooldown(dungeon.SkillId));
                                            }
                                        }
                                    }

                                    if (isMoon || isNeedWaitKd)
                                    {
                                        Thread.Sleep(500);
                                    }

                                    var result2 = Host.SpellManager.CastSpell(dungeon.SkillId);
                                    if (result2 != ESpellCastError.SUCCESS)
                                        Host.log(
                                            "Не удалось использовать скилл из скрипта " + skill.Name + "  " + result2 +
                                            "   " + Host.GetLastError(), LogLvl.Error);
                                    else
                                    {
                                        if (Host.CharacterSettings.LogScriptAction)
                                            Host.log("Использовал скилл из скрипта " + skill.Name, LogLvl.Ok);
                                        Thread.Sleep(200);
                                    }

                                    Host.MyCheckIsMovingIsCasting();

                                    while (Host.SpellManager.IsChanneling)
                                    {
                                        Thread.Sleep(50);
                                        //  Host.log("Использовал скилл из скрипта IsChanneling " + skill.Name, Host.LogLvl.Ok);
                                    }


                                    if (skill.Id == 18960)
                                        Thread.Sleep(5000);

                                    if (skill.Id == 193753)
                                        Thread.Sleep(5000);

                                    while (Host.GameState != EGameState.Ingame)
                                        Thread.Sleep(200);

                                    if (isMoon)
                                    {
                                        if (Host.Area.Id == 493 && Host.MapID == 1)
                                        {
                                            Host.log("После телепорта из лунной поляны оказались там же, использую камень");
                                            foreach (var item in Host.ItemManager.GetItems())
                                            {
                                                if (item.Id == 6948)
                                                {
                                                    if (Host.SpellManager.GetItemCooldown(item) != 0)
                                                    {
                                                        Host.log("Камень в КД " + Host.SpellManager.GetItemCooldown(item));
                                                        break;
                                                    }

                                                    var result = Host.SpellManager.UseItem(item);
                                                    if (result != EInventoryResult.OK)
                                                    {
                                                        Host.log(
                                                            "Не удалось использовать камень " + item.Name + " " + result +
                                                            " " + Host.GetLastError(), LogLvl.Error);
                                                    }
                                                    else
                                                    {
                                                        Host.log("Использовал камень ", LogLvl.Ok);
                                                    }

                                                    Host.MyCheckIsMovingIsCasting();
                                                    while (Host.SpellManager.IsChanneling)
                                                        Thread.Sleep(50);
                                                    Thread.Sleep(5000);
                                                    while (Host.GameState != EGameState.Ingame)
                                                    {
                                                        Thread.Sleep(200);
                                                    }

                                                    Thread.Sleep(1000);
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    //    Host.IsRegen = false;
                                }
                                else
                                {
                                    Host.log("Скилл не найден на панели", LogLvl.Error);
                                }
                            }
                            break;

                        case "Сбросить все данжи":
                            {
                                while (true)
                                {
                                    if (!Host.MainForm.On)
                                        return;
                                    Host.log("Пытаюсь сбросить данж");
                                    if (!Host.ResetInstances())
                                    {
                                        Host.log(
                                            "Не удалось сбросить данжи " + Host.GetLastError() + "   " + Host.GameState,
                                            LogLvl.Error);
                                        Thread.Sleep(10000);
                                        if (!Host.ResetInstances())
                                        {
                                            Host.log(
                                                "Не удалось сбросить данжи " + Host.GetLastError() + "   " + Host.GameState,
                                                LogLvl.Error);
                                            Thread.Sleep(10000);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            break;

                        case "Пауза":
                            {
                                if (!dungeon.Attack)
                                    Host.FarmModule.FarmState = FarmState.Disabled;
                                else
                                    Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                var delay = dungeon.Pause;

                                Thread.Sleep(delay);
                            }
                            break;
                        case "Пауза 2":
                            {
                                Host.MainForm.Dispatcher.Invoke(() => { Host.MainForm.ButtonContinue.IsEnabled = true; });
                                Continue = false;
                                WaitTeleport = true;


                                while (Host.MainForm.On)
                                {
                                    Thread.Sleep(1000);
                                    if (Continue)
                                        break;
                                }

                                WaitTeleport = false;
                            }
                            break;

                        case "Продажа через Телепорт":
                            {
                                /*  if (host.Me.Pet != null)
                                          host.Me.Pet.Dismiss();
                                      WaitTeleport = true;
                                      var teleport = host.Me.GetItem(50169);
                                      while (host.GetItemCooldownTime(teleport.Id) != 0)
                                          Thread.Sleep(5000);

                                      WaitTeleport = false;
                                      if (host.GetItemCooldownTime(teleport.Id) == 0)
                                      {
                                          Thread.Sleep(1000);
                                          while (host.Me.IsMoving)
                                              Thread.Sleep(50);

                                          if (host.UseItem(teleport.Id, EItemUseType.UseByItemUniqId))
                                          {
                                              host.log("Использовал телепорт для продажи ");
                                              Thread.Sleep(7000);
                                              while (host.Me.IsCastingSkill)
                                                  Thread.Sleep(1000);
                                              while (host.GameState != EGameState.Ingame)
                                                  Thread.Sleep(100);
                                              Thread.Sleep(100);
                                              host.CommonModule.LoadCurrentZoneMesh(host.Me.Location.X, host.Me.Location.Y);

                                          }
                                          else
                                          {
                                              host.log("Не удалось использовать телепорт 1" + host.GetLastError(), Host.LogLvl.Error);
                                              Thread.Sleep(1000);

                                              while (host.GameState != EGameState.Ingame)
                                                  Thread.Sleep(100);
                                              Thread.Sleep(1000);

                                          }

                                      }



                                      if (host.WorldMapType == EWorldMapType.Field)
                                      {
                                          if (host.CharacterSettings.Mode == "Данж.(п)")
                                              if (host.WorldMapType == EWorldMapType.Field)
                                                  foreach (var fieldChannel in host.GetFieldChannels())
                                                      if (fieldChannel.ChannelId == 2 && fieldChannel != host.CurrentFieldChannel)
                                                      {
                                                          if (!fieldChannel.Transfer())
                                                              host.log("Не смог поменять канал " + host.GetLastError(), Host.LogLvl.Error);
                                                          break;
                                                      }

                                          host.CommonModule.LoadCurrentZoneMesh(host.Me.Location.X, host.Me.Location.Y);
                                          host.log("Необходима продажа", Host.LogLvl.Important);
                                          host.FarmModule.farmState = FarmState.Disabled;


                                          host.FarmModule.farmState = FarmState.AttackOnlyAgro;*/
                            }


                            break;
                        case "Вход в данж":
                            {
                                var mapid = Host.MapID;
                                while (mapid == Host.MapID)
                                {
                                    Thread.Sleep(100);
                                    if (!Host.MainForm.On)
                                        return;
                                    foreach (var gameObject in Host.GetEntities<GameObject>()
                                        .OrderBy(i => Host.Me.Distance(i)))
                                    {
                                        if (gameObject.GameObjectType != EGameObjectType.DungeonDifficulty)
                                            continue;
                                        Host.log("Бегу к входу " + gameObject.Name + " " + gameObject.GameObjectType);
                                        Host.CommonModule.MoveTo(gameObject, 0);
                                        Thread.Sleep(1000);
                                        Host.SetMoveStateForClient(true);
                                        Host.MoveForward(true);
                                        Thread.Sleep(2000);
                                        Host.MoveForward(false);
                                        Host.SetMoveStateForClient(false);
                                        while (Host.GameState != EGameState.Ingame)
                                        {
                                            if (!Host.MainForm.On)
                                                return;
                                            Thread.Sleep(1000);
                                        }

                                        Host.CommonModule.MoveTo(Host.Me.Location, 1);
                                        break;
                                    }
                                }
                            }
                            break;
                        case "Выход из данжа":
                            {
                                var mapid = Host.MapID;
                                while (mapid == Host.MapID)
                                {
                                    Thread.Sleep(100);
                                    if (!Host.MainForm.On)
                                        return;
                                    foreach (var gameObject in Host.GetEntities<GameObject>()
                                        .OrderBy(i => Host.Me.Distance(i)))
                                    {
                                        if (gameObject.GameObjectType != EGameObjectType.DungeonDifficulty)
                                            continue;
                                        Host.log("Бегу к выходу " + gameObject.Name + " " + gameObject.GameObjectType);
                                        Host.CommonModule.MoveTo(gameObject, 0);
                                        Thread.Sleep(1000);
                                        Host.SetMoveStateForClient(true);
                                        Host.MoveForward(true);
                                        Thread.Sleep(2000);
                                        Host.MoveForward(false);
                                        Host.SetMoveStateForClient(false);
                                        while (Host.GameState != EGameState.Ingame)
                                        {
                                            if (!Host.MainForm.On)
                                                return;
                                            Thread.Sleep(1000);
                                        }

                                        Host.CommonModule.MoveTo(Host.Me.Location, 1);
                                        break;
                                    }
                                }
                            }
                            break;

                        case "Бежать на точку":
                            {
                                if (dungeon.MapId != Host.MapID)
                                {
                                    Host.log("Точка на другом континенете " + dungeon.MapId + "  " + Host.MapID,
                                        LogLvl.Error);
                                    Thread.Sleep(1000);
                                    Host.FarmModule.FarmState = FarmState.Disabled;
                                    if (dungeon.MapId == 209 && Host.MapID == 0)
                                    {
                                        if (Host.Me.Distance(-3893.50, -603.81, 5.39) > 300)
                                            if (!Host.MyUseTaxi(11, new Vector3F(-3893.50, -603.81, 5.39)))
                                                return;

                                        if (Host.Me.Distance(-3893.50, -603.81, 5.39) > 5)
                                            if (!Host.CommonModule.MoveTo(-3893.50, -603.81, 5.39))
                                                return;

                                        var transport = Host.MyGetTransportById(176231);
                                        if (transport != null && !transport.IsMoving && Host.Me.Distance(transport) < 50)
                                        {
                                            Host.CommonModule.ForceMoveTo2(transport.Location, 8);
                                            Host.CommonModule.ForceMoveTo2(new Vector3F(-3899.22, -580.95, 6.10), 1);
                                            while (Host.MapID == 0)
                                            {
                                                Host.log("Плыву " + Host.MapID);
                                                if (!Host.MainForm.On)
                                                    return;
                                                Thread.Sleep(1000);
                                            }

                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                                if (!Host.MainForm.On)
                                                    return;
                                            }
                                            transport = Host.MyGetTransportById(176231);
                                            while (transport.IsMoving)
                                            {
                                                Host.log("Плыву " + transport.IsMoving);
                                                if (!Host.MainForm.On)
                                                    return;
                                                Thread.Sleep(1000);
                                            }
                                            Host.log("Доплыл");
                                            Host.CommonModule.ForceMoveTo2(new Vector3F(-4005.96, -4726.72, 5.09), 1);
                                        }
                                        else
                                        {
                                            Host.log("Нет транспорта");
                                        }
                                        return;
                                    }


                                    if (dungeon.MapId == 209 && Host.MapID == 1)
                                    {
                                        if (Host.Me.Distance(-6793.53, -2891.10, 8.88) > 300)
                                            if (!Host.MyUseTaxi(440, new Vector3F(-6793.53, -2891.10, 8.88)))
                                                return;
                                        if (!Host.CommonModule.MoveTo(-6793.53, -2891.10, 8.88))
                                            return;
                                        Host.MyMoveForvard(2000);
                                        return;
                                    }


                                    if (dungeon.MapId == 189 && Host.MapID == 0)
                                    {
                                        if (Host.CharacterSettings.PikPocket)
                                            if (Host.Me.IsAlive)
                                            {
                                                if (!Host.CommonModule.MoveTo(new Vector3F(2866.74, -821.74, 160.33), 0))
                                                    return;
                                                var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing1.xml";
                                                Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                                                var reader = new XmlSerializer(typeof(DungeonSetting));
                                                using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                                            }
                                            else
                                            {
                                                if (Host.FarmModule.IsWing1)
                                                {
                                                    if (!Host.CommonModule.MoveTo(new Vector3F(2866.74, -821.74, 160.33), 0)) // 2884.65, -836.56, 160.33 данж 2
                                                        return;
                                                    var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing1.xml";
                                                    Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                                                    var reader = new XmlSerializer(typeof(DungeonSetting));
                                                    using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                                                }
                                                else
                                                {
                                                    if (!Host.CommonModule.MoveTo(new Vector3F(2884.65, -836.56, 160.33),
                                                        0)) //  данж 2
                                                        return;
                                                    var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing2.xml";
                                                    Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                                                    var reader = new XmlSerializer(typeof(DungeonSetting));
                                                    using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                                                }
                                            }


                                        Host.MyMoveForvard(2000);
                                        return;
                                    }

                                    if (dungeon.MapId == 1643 && Host.MapID == 0)
                                    {
                                        var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(-9077.94, 891.76, 68.04), Host.Me.Location);
                                        Host.log(path.Count + "  Путь");
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                        }

                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 323845)
                                                continue;
                                            Host.CommonModule.MoveTo(gameObject, 1);
                                            Host.CommonModule.MyUnmount();
                                            Host.CanselForm();
                                            Thread.Sleep(5000);
                                            gameObject.Use();
                                            Thread.Sleep(2000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }

                                            Host.MyWait(10000);
                                        }

                                        if (String.Compare(Host.GetBotLogin(), "zawww", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            if (index != 0)
                                                index -= 1;
                                            continue;
                                        }

                                        return;
                                    }

                                    if (dungeon.MapId == 1643 && Host.MapID == 1642)
                                    {
                                        if (Host.Me.Distance(-2174.64, 766.01, 20.92) > 20)
                                            Host.CommonModule.MoveTo(-2138.24, 797.57, 5.93);

                                        Host.CommonModule.MyUnmount();
                                        Host.CommonModule.MoveTo(new Vector3F(-2174.64, 766.01, 20.92), 1, false);

                                        var npc = Host.GetNpcById(135690);
                                        if (npc != null)
                                        {
                                            /*  Host.OpenDialog(npc);
                                                  Thread.Sleep(1000);*/
                                            switch (Host.GetBotLogin())
                                            {
                                                case "deathstar":
                                                    {
                                                        Host.MyDialog(npc, 8);
                                                    }
                                                    break;
                                                default:
                                                    {
                                                        Host.MyDialog(npc, 6);
                                                    }
                                                    break;
                                            }

                                            Thread.Sleep(5000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }

                                            Thread.Sleep(10000);
                                            /* foreach (var gossipOptionsData in Host.GetNpcDialogs())
                                                 {
                                                     Host.log(gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                                                 }*/
                                        }

                                        if (String.Compare(Host.GetBotLogin(), "zawww", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            if (index != 0)
                                                index -= 1;
                                            continue;
                                        }

                                        return;
                                    }

                                    if ((dungeon.MapId == 1642 || dungeon.MapId == 1643 || dungeon.MapId == 1718) &&
                                        Host.MapID == 1)
                                    {
                                        var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1432.93, -4518.37, 18.40),
                                            Host.Me.Location);
                                        Host.log(path.Count + "  Путь");
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F);
                                        }

                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 323855)
                                                continue;
                                            Host.CommonModule.MoveTo(gameObject, 1);
                                            Host.CommonModule.MyUnmount();
                                            Host.CanselForm();
                                            Thread.Sleep(5000);
                                            gameObject.Use();
                                            Thread.Sleep(2000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }

                                            Host.MyWait(10000);
                                        }

                                        if (String.Compare(Host.GetBotLogin(), "zawww", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            Host.log("Повторяю действие");
                                            if (index != 0)
                                                index -= 1;
                                            continue;
                                        }

                                        return;
                                    }

                                    if (dungeon.MapId == 1718 && Host.MapID == 1642)
                                    {
                                        if (!Host.CommonModule.MoveTo(-1132.67, 772.36, 433.32))
                                            return;
                                        foreach (var gameObject in Host.GetEntities<GameObject>())
                                        {
                                            if (gameObject.Id != 327526)
                                                continue;
                                            Host.CommonModule.MoveTo(gameObject, 1);
                                            Host.CommonModule.MyUnmount();
                                            Host.CanselForm();
                                            Thread.Sleep(5000);
                                            gameObject.Use();
                                            Thread.Sleep(2000);
                                            while (Host.GameState != EGameState.Ingame)
                                            {
                                                Thread.Sleep(1000);
                                            }

                                            Host.MyWait(10000);
                                        }
                                    }


                                    if (index != 0)
                                        index -= 1;
                                }

                                if (dungeon.AreaId != Host.Area.Id ||
                                    (Host.Me.Distance(dungeon.Loc) > 600 && Host.ClientType == EWoWClient.Retail))
                                {
                                    if (Host.Me.Distance(dungeon.Loc) < 200)
                                    {
                                    }
                                    else
                                    {
                                        switch (Host.Area.Id)
                                        {
                                            case 8721:
                                                {
                                                }
                                                break;
                                            default:
                                                {
                                                    Host.log("Точка в другой зоне " + dungeon.Loc + "  " + Host.Area.Id,
                                                        LogLvl.Error);
                                                    if (Host.Me.Distance(-217.74, -1554.51, 2.73) < 100)
                                                    {
                                                        Host.MyUseTaxi(dungeon.AreaId, new Vector3F(1397.26, 2043.43, 264.55));
                                                        return;
                                                    }

                                                    Host.MyUseTaxi(dungeon.AreaId, dungeon.Loc);
                                                    Host.MyUseTaxi(dungeon.AreaId, dungeon.Loc);
                                                    if (Host.GetBotLogin() != "deathstar")
                                                        if (index != 0)
                                                            index -= 1;
                                                }
                                                break;
                                        }
                                    }
                                }


                                /* if (!Host.IsInsideNavMesh(dungeon.Loc) && Host.Me.Distance(dungeon.Loc) < 50)
                                     {
                                         Host.log(index + ")Точка вне мешей. пропускаю " + dungeon.Loc);
                                         continue;
                                     }*/

                                if (!dungeon.Attack)
                                    Host.FarmModule.FarmState = FarmState.Disabled;
                                else
                                    Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;

                                if (Host.Me.Distance(dungeon.Loc) > 150)
                                    Host.FarmModule.FarmState = FarmState.Disabled;
                                //  Host.log("Бегу на точку " + dungeon.Loc + "  " + "  " + dungeon.Attack + " дист:" + Host.Me.Distance(dungeon.Loc));
                                while (Host.Me.Distance2D(dungeon.Loc) > 3 && Host.IsAlive(Host.Me) && Host.MainForm.On && !Host.Me.IsDeadGhost)
                                {
                                    Host.log("Бегу на точку  2     " + dungeon.Loc + "  " + "  " + dungeon.Attack +
                                             " дист:" + Host.Me.Distance(dungeon.Loc) + Host.IsAlive(Host.Me) + " " +
                                             Host.Me.IsDeadGhost);
                                    Thread.Sleep(10);
                                    Host.AdvancedInvisible();
                                    if (!Host.MainForm.On)
                                        break;
                                    if (!Host.CheckCanUseGameActions())
                                    {
                                        Thread.Sleep(100);
                                        continue;
                                    }

                                    if (Host.GameState != EGameState.Ingame)
                                    {
                                        Thread.Sleep(100);
                                        continue;
                                    }

                                    while (Host.FarmModule.BestMob != null || Host.FarmModule.BestProp != null)
                                        Thread.Sleep(100);

                                    while (Host.SpellManager.IsCasting)
                                        Thread.Sleep(100);

                                    if (dungeon.Attack)
                                    {
                                        while (Host.CommonModule.IsMoveSuspended())
                                            Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        if (Host.FarmModule.FarmState != FarmState.Disabled)
                                        {
                                            while (Host.CommonModule.IsMoveSuspended())
                                                Thread.Sleep(100);
                                            if (Host.GetAgroCreatures().Count == 0)
                                                Host.FarmModule.FarmState = FarmState.Disabled;
                                        }
                                    }

                                    if (Host.CheckPikPocket())
                                    {
                                        Host.AdvancedInvisible();
                                        //  Host.log("Жду");
                                        while (Host.CommonModule.IsMoveSuspended())
                                            Thread.Sleep(100);
                                        // Host.log("Жду2");
                                        Thread.Sleep(500);
                                        Host.AdvancedInvisible();
                                        if (!Host.CheckInvisible())
                                        {
                                            Host.AdvancedInvisible();
                                            Host.log("Нет инвиза");
                                            continue;
                                        }
                                    }

                                    if (NeedFindBestPoint)
                                    {
                                        break;
                                    }

                                    if (Host.CommonModule.IsMoveSuspended())
                                        continue;
                                    if (Host.CharacterSettings.ForceMoveScriptEnable)
                                    {
                                        if (Host.Me.Distance(dungeon.Loc) < Host.CharacterSettings.ForceMoveScriptDist)
                                        {
                                            Host.CommonModule.MySitMount(dungeon.Loc);
                                            Host.log("Вошел в ForceComeTo");


                                            if (!Host.CommonModule.ForceMoveTo2(dungeon.Loc))
                                            {
                                                Host.CommonModule.MyUseGps(dungeon.Loc);
                                                /* if (Host.GetLastError() != ELastError.Movement_MoveCanceled)
                                                         Host.log("Не смог добежать до точки " + Host.GetLastError() + "  " + Host.Me.Distance(dungeon.Loc), Host.LogLvl.Error);*/
                                            }
                                            else
                                            {
                                                Host.CommonModule.MoveFailCount = 0;
                                                break;
                                            }

                                            Host.log("Вышел из ForceComeTo");
                                            if (Host.Me.Distance2D(dungeon.Loc) > 4 && Host.MainForm.On &&
                                                !Host.CommonModule.IsMoveSuspended())
                                            {
                                                Host.CommonModule.MoveTo(dungeon.Loc, 1);
                                            }

                                            if (Host.CommonModule.MoveFailCount > 1)
                                                break;
                                            continue;
                                        }
                                    }
                                    //   host.log("тест 2");

                                    if (Host.MainForm.On && !Host.CommonModule.IsMoveSuspended())
                                    {
                                        if (Host.CharacterSettings.PikPocket && !Host.CheckInvisible())
                                        {
                                            Host.AdvancedInvisible();
                                            Host.log("Нет инвиза");
                                            continue;
                                        }

                                        if (Host.CommonModule.MoveTo(dungeon.Loc, 1))
                                            break;
                                    }

                                    if (Host.CharacterSettings.PikPocket)
                                    {
                                        if (!Host.CheckInvisible())
                                        {
                                            Host.AdvancedInvisible();
                                            Host.log("Нет инвиза");
                                        }
                                    }
                                    else
                                    {
                                        if (Host.CommonModule.MoveFailCount > 1)
                                        {
                                            if (Host.Me.Distance(dungeon.Loc) < 20)
                                                break;
                                        }

                                        if (Host.CommonModule.MoveFailCount > 20)
                                            break;
                                    }
                                }
                            }
                            break;

                        case "Отбиться от мобов":
                            {
                                /*  if (host.WorldMapType == EWorldMapType.Dungeon && host.Me.Pet == null)
                                          host.CommonModule.SummonPet();*/
                                //   host.log("Отбиваюсь от мобов");
                                Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                                Thread.Sleep(100);
                                Host.CommonModule.SuspendMove();
                                //  var timerfix = 0;
                                while (Host.GetAgroCreatures().Count > 0 && Host.IsAlive(Host.Me) &&
                                       Host.FarmModule.FarmState == FarmState.AttackOnlyAgro)
                                {
                                    Thread.Sleep(100);
                                    /*  timerfix++;
                                          if (timerfix > 1500)
                                              host.FarmModule.farmState = FarmState.Disabled;*/
                                }

                                Host.FarmModule.BestMob = null;
                                if (Host.CharacterSettings.LogScriptAction)
                                    Host.log("Отбился от мобов");
                                //   timerfix = 0;
                                while (Host.CommonModule.IsMoveSuspended() && Host.IsAlive(Host.Me))
                                {
                                    Thread.Sleep(100);
                                    /*  timerfix++;
                                          if (timerfix > 1500)
                                              break;*/
                                }

                                Host.CommonModule.ResumeMove();
                                if (Host.CharacterSettings.LogScriptAction)
                                    Host.log("Собрал лут");

                                if (Host.CharacterSettings.FindBestPoint)
                                    Host.AutoQuests.NeedFindBestPoint = true;
                            }
                            break;
                        case "Остановить скрипт":
                            {
                                Host.MainForm.On = false;
                                return;
                            }

                        case "Фарм пропов":
                            {
                                Host.MyUseGameObject(dungeon.PropId);

                                Host.log("Фарм пропов " + dungeon.PropId);
                                Host.CommonModule.MoveTo(dungeon.Loc, 1);
                                var zone = new RoundZone(Host.Me.Location.X, Host.Me.Location.Y, 120);
                                var farmproplist = new List<uint> { dungeon.PropId };
                                Host.FarmModule.SetFarmProps(zone, farmproplist);
                                while (Host.MainForm.On
                                       && Host.IsAlive(Host.Me)
                                       && Host.CharacterSettings.Mode == Mode.Script
                                       && Host.FarmModule.ReadyToActions
                                       && Host.FarmModule.FarmState == FarmState.FarmProps)
                                {
                                    Thread.Sleep(100); //213032
                                    if (!Host.CommonModule.InFight())
                                        if (!Host.IsPropExitis(dungeon.PropId))
                                            Host.FarmModule.StopFarm();
                                }

                                Host.FarmModule.StopFarm();
                                Host.CommonModule.SuspendMove();
                                Thread.Sleep(100);
                                while (Host.CommonModule.IsMoveSuspended() && Host.IsAlive(Host.Me))
                                    Thread.Sleep(100);
                                Host.log("Пропов с Id " + dungeon.PropId + " нет");
                            }
                            break;

                        case "Фарм мобов":
                            {
                                /*  if (host.WorldMapType == EWorldMapType.Dungeon && host.Me.Pet == null)
                                          host.CommonModule.SummonPet();
                                      host.log("Фарм мобов до смерти " + dungeon.MobId);
                                      host.CommonModule.MoveTo(dungeon.Loc);
                                      var zone = new RoundZone(dungeon.Loc.X, dungeon.Loc.Y, 40);
                                      var farmmoblist = new List<int> { dungeon.MobId };
                                      host.FarmModule.SetFarmMobs(zone, farmmoblist);
                                      while (host.MainForm.On
                                             && host.IsAlive(host.Me)
                                             && host.CharacterSettings.Mode == "Данж.(п)"
                                             && host.FarmModule.readyToActions
                                             && host.FarmModule.farmState == FarmState.FarmMobs)
                                      {
                                          Thread.Sleep(100);
                                          if (!host.CommonModule.InFight())
                                              if (!host.IsBossAlive(dungeon.MobId))
                                                  host.FarmModule.StopFarm();
                                      }

                                      host.FarmModule.StopFarm();
                                      host.CommonModule.SuspendMove();
                                      Thread.Sleep(100);
                                      while (host.CommonModule.IsMoveSuspended() && host.IsAlive(host.Me))
                                          Thread.Sleep(100);
                                      host.log("Убил " + dungeon.MobId);*/
                            }
                            break;
                        case "Эквип на ауке":
                            {
                                var npc = Host.MyMoveToAuction();
                                if (npc == null)
                                {
                                    Host.log("Нет НПС для аука", LogLvl.Error);
                                    Thread.Sleep(5000);
                                    return;
                                }

                                Host.MyCheckIsMovingIsCasting();
                                if (!Host.OpenAuction(npc))
                                    Host.log("Не смог открыть диалог для аука " + Host.GetLastError(), LogLvl.Error);
                                else
                                {
                                    Host.log("Открыл диалог для аука", LogLvl.Ok);
                                }

                                Thread.Sleep(3000);
                                foreach (var characterSettingsEquipAuc in Host.CharacterSettings.EquipAucs)
                                {
                                    if (IsFindItem(characterSettingsEquipAuc.Name, characterSettingsEquipAuc.Stat1,
                                        characterSettingsEquipAuc.Stat2))
                                    {
                                        Host.log("Нашел в инвентаре " + characterSettingsEquipAuc.Name + " " +
                                                 characterSettingsEquipAuc.Slot);
                                        _equipCells.Add(characterSettingsEquipAuc.Slot);
                                    }
                                }

                                foreach (var characterSettingsEquipAuc in Host.CharacterSettings.EquipAucs)
                                {
                                    if (_equipCells.Contains(characterSettingsEquipAuc.Slot))
                                        continue;
                                    var req = new AuctionSearchRequest
                                    {
                                        MaxReturnItems = 50,
                                        SearchText = characterSettingsEquipAuc.Name,
                                        ExactMatch = true,
                                        SortType = EAuctionSortType.PriceAsc
                                    };
                                    Host.log("Ищу на ауке " + characterSettingsEquipAuc.Name + " " +
                                             characterSettingsEquipAuc.Slot);
                                    var aucItems = Host.GetAuctionBuyList(req);
                                    if (aucItems == null || aucItems.Count == 0)
                                    {
                                        Host.log("Ничего не нашел");
                                        continue;
                                    }

                                    foreach (var aucItem in aucItems)
                                    {
                                        if (aucItem.BuyoutPrice == 0)
                                            continue;
                                        if (aucItem.BuyoutPrice > characterSettingsEquipAuc.MaxPrice)
                                        {
                                            Host.log("Цена1: " + aucItem.ItemId + " " + aucItem.BuyoutPrice);
                                            Host.log("Цена2: " + aucItem.ItemId + " " + characterSettingsEquipAuc.MaxPrice);
                                            continue;
                                        }

                                        if (characterSettingsEquipAuc.Level != 0)
                                            if (characterSettingsEquipAuc.Level != aucItem.ItemLevel)
                                            {
                                                Host.log("Не подходит уровень");
                                                continue;
                                            }


                                        if (characterSettingsEquipAuc.Stat1 != 0)
                                        {
                                            if (aucItem.ItemStatType == null || aucItem.ItemStatType.Count == 0)
                                            {
                                                Host.log("Нет Информации о стате " + aucItem.ItemStatType?.Count);
                                                continue;
                                            }

                                            if (!aucItem.ItemStatType.Contains(
                                                (EItemModType)characterSettingsEquipAuc.Stat1))
                                            {
                                                Host.log("Нет стата " + characterSettingsEquipAuc.Stat1);
                                                continue;
                                            }
                                        }

                                        if (characterSettingsEquipAuc.Stat2 != 0)
                                        {
                                            if (aucItem.ItemStatType == null || aucItem.ItemStatType.Count == 0)
                                            {
                                                Host.log("Нет Информации о стате " + aucItem.ItemStatType?.Count);
                                                continue;
                                            }

                                            if (!aucItem.ItemStatType.Contains(
                                                (EItemModType)characterSettingsEquipAuc.Stat2))
                                            {
                                                Host.log("Нет стата " + characterSettingsEquipAuc.Stat2);
                                                continue;
                                            }
                                        }

                                        Host.log("Покупаю " + characterSettingsEquipAuc.Name + "[" + aucItem.ItemId +
                                                 "]   " + "  " + aucItem.BuyoutPrice + " " + aucItem.ItemLevel);
                                        _equipCells.Add(characterSettingsEquipAuc.Slot);
                                        Thread.Sleep(2000);
                                        var result = aucItem.MakeBuyout();
                                        if (result == EAuctionHouseError.Ok)
                                        {
                                            Host.log("Выкупил ");
                                            _equipCells.Add(characterSettingsEquipAuc.Slot);
                                            Thread.Sleep(5000);
                                        }
                                        else
                                        {
                                            Host.log("Не смог выкупить " + result + " " + Host.GetLastError(),
                                                LogLvl.Error);
                                            Thread.Sleep(10000);
                                        }

                                        break;
                                    }
                                }

                                Host.log("Все покупки завершены ");
                                Host.MainForm.On = false;
                                return;
                            }

                        case "Токен": //В плагин квестер добавить в раздел скрипта строчку с проверкой:
                            {
                                if (Host.MyGetItem(122284) != null)
                                {
                                    if (!Host.ActivateWowTokenToBalance())
                                        Host.log(
                                            "Не смог активировать токен на баланс " + Host.GetLastError(),
                                            LogLvl.Error);
                                    else
                                        Host.log("Активировал токен на баланс ", LogLvl.Ok);

                                    Thread.Sleep(10000);
                                    return;
                                }

                                if (_notGoldToken)
                                {
                                    Host.log("Не хватает голды на токен или баланс > 90");
                                    Thread.Sleep(10000);
                                    continue;
                                }

                                var balance = Host.GetBattleNetBalance();
                                Host.log("Баланс " + balance + " $");
                                if (balance < 930000) //если баланс близард <90$ <Core.GetBattleNetBalance()> (в центах)
                                {
                                    if (Host.Me.Team == ETeam.Horde)
                                        if (Host.Area.Id != 1637)
                                        {
                                            Host.log("Нахожусь не в оргримаре " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }

                                    if (Host.Me.Team == ETeam.Alliance)
                                        if (Host.Area.Id != 1519)
                                        {
                                            Host.log("Нахожусь не в штормвинде " + Host.Area.Id + " " + Host.Area.ZoneName);
                                            Thread.Sleep(5000);
                                            continue;
                                        }

                                    var path = Host.CommonModule.GpsBase.GetPath(new Vector3F(1635, -4445, 17),
                                        Host.Me.Location);
                                    if (Host.Me.Team == ETeam.Horde)
                                        if (Host.Me.Distance(1654.84, -4350.49, 26.35) < 50 ||
                                            Host.Me.Distance(1573.36, -4437.08, 16.05) < 50)
                                        {
                                            if (Host.CharacterSettings.AlternateAuk)
                                            {
                                                path = Host.CommonModule.GpsBase.GetPath(
                                                    new Vector3F(2065.83, -4668.45, 32.52), Host.Me.Location);
                                            }

                                            foreach (var vector3F in path)
                                            {
                                                Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                                Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                            }
                                        }

                                    if (Host.Me.Team == ETeam.Alliance)
                                    {
                                        path = Host.CommonModule.GpsBase.GetPath(new Vector3F(-8816.10, 660.36, 98.01),
                                            Host.Me.Location);
                                        foreach (var vector3F in path)
                                        {
                                            Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                                            Host.CommonModule.ForceMoveTo2(vector3F, 1, false);
                                        }
                                    }

                                    //Проверка НПС
                                    Unit npc = null;
                                    foreach (var entity in Host.GetEntities<Unit>())
                                    {
                                        if (!entity.IsAuctioner)
                                            continue;
                                        if (entity.Id == 44868)
                                            continue;
                                        if (entity.Id == 44865)
                                            continue;
                                        if (entity.Id == 44866)
                                            npc = entity;
                                        if (entity.Id == 8719)
                                            npc = entity;
                                        if (entity.Id == 46640)
                                            npc = entity;
                                    }

                                    if (npc == null)
                                    {
                                        Host.log("Нет НПС для аука", LogLvl.Error);
                                        Thread.Sleep(5000);
                                        return;
                                    }

                                    Host.log("Выбран " + npc.Name + " " + npc.Id);
                                    Host.CommonModule.MoveTo(npc, 3);
                                    Host.MyCheckIsMovingIsCasting();
                                    if (!Host.OpenAuction(npc))
                                        Host.log("Не смог открыть диалог для аука " + Host.GetLastError(),
                                            LogLvl.Error);
                                    else
                                    {
                                        Host.log("Открыл диалог для аука", LogLvl.Ok);
                                    }

                                    Thread.Sleep(3000);
                                    var priceToken = Host.GetAuctionWowTokenPrice();
                                    Host.log("Money " + Host.Me.Money + " GetAuctionWowTokenPrice: " + priceToken);
                                    if (Host.Me.Money > priceToken + 20000000
                                    ) //если у меня голды больше N суммы (N определить подойдя на аук и проверив цену токена (если такое возможно), или если не получится, то в ручную указывать)
                                    {
                                        if (!Host.BuyAuctionWowToken()
                                        ) //купить вов токен <ulong Core.GetAuctionWowTokenPrice()>      <bool Core.BuyAuctionWowToken()>
                                        {
                                            Host.log("Не смог купить токен " + Host.GetLastError());
                                            Thread.Sleep(10000);
                                            return;
                                        }
                                        else
                                        {
                                            Thread.Sleep(
                                                60000); //подождать минуту (токен бывает идет долго и не сразу появляется на почте)
                                            Host.MyMail();


                                            if (Host.MyGetItem(122284) != null)
                                            {
                                                var time = Host.GetCurrentAccount().Premium.ToUniversalTime() -
                                                           DateTime.UtcNow;
                                                Host.log("Осталось " + time.Days + " дней " + time.Hours + " часов");
                                                if (time.Days < 2 && !_buySubs)
                                                {
                                                    if (!Host.ActivateWowTokenToSubscription())
                                                        Host.log(
                                                            "Не смог активировать токен на подписку " + Host.GetLastError(),
                                                            LogLvl.Error);
                                                    else
                                                        Host.log("Активировал токен на подписку ", LogLvl.Ok);
                                                    _buySubs = true;
                                                }
                                                else
                                                {
                                                    if (!Host.ActivateWowTokenToBalance())
                                                        Host.log(
                                                            "Не смог активировать токен на баланс " + Host.GetLastError(),
                                                            LogLvl.Error);
                                                    else
                                                        Host.log("Активировал токен на баланс ", LogLvl.Ok);
                                                }


                                                Thread.Sleep(10000);
                                            }

                                            // пойти к почте забрать токен
                                            // удалить оставшееся письмо после токена<Mail.Delete>
                                            // активировать токен на баланс близард < Core.ActivateWowTokenToBalance() >
                                            // если получится потом можно еще сделать
                                            // если время подписки меньше N времени
                                            // сделать все тоже самое, но только в конце активировать токен в подписку<Core.ActivateWowTokenToSubscription()> 
                                        }
                                    }
                                    else
                                    {
                                        Host.log("Не хватает голды на токен");
                                        _notGoldToken = true;
                                    }
                                }
                                else
                                {
                                    File.AppendAllText(Host.AssemblyDirectory + "\\Token.txt",
                                        DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + @" " +
                                        Host.GetCurrentAccount().Login + @"  " + Host.GetCurrentAccount().Name +
                                        Environment.NewLine);

                                    _notGoldToken = true;
                                }
                            }
                            break;

                        case "Приручить питомца":
                            {
                                if (!RecivePet())
                                    // if (index != 0)
                                    index -= 1;

                            }
                            break;

                        case "Выполнить квест":
                            {
                                while (true)
                                {
                                    Thread.Sleep(100);
                                    if (!Host.MainForm.On)
                                        return;

                                    var listQuest = new MyQuestHelpClass.MyQuest(dungeon.QuestId, dungeon.QuestAction);
                                    Host.log(listQuest.Id + " " + listQuest.QuestAction);
                                    if (Host.CheckQuestCompleted(listQuest.Id))
                                        break;

                                    /*  if (!listQuest.Race.Contains(ERace.None))
                                              if (!listQuest.Race.Contains(Host.Me.Race))
                                                  continue;

                                          if (!listQuest.Class.Contains(EClass.None))
                                              if (!listQuest.Class.Contains(Host.Me.Class))
                                                  continue;*/

                                    if (listQuest.QuestAction == QuestAction.Apply && Host.GetQuest(listQuest.Id) != null)
                                        break;


                                    var quest = Host.GetQuest(listQuest.Id);
                                    if (listQuest.QuestAction == QuestAction.Run && GetQuestIndex(quest) == -1)
                                        break;

                                    if (listQuest.QuestAction == QuestAction.Complete && GetQuestIndex(quest) != -1)
                                        break;

                                    Host.log(listQuest.Id + " " + listQuest.QuestAction);
                                    if (listQuest.QuestAction == QuestAction.Apply)
                                    {
                                        ApplyQuestClassic(listQuest);
                                        return;
                                    }

                                    if (listQuest.QuestAction == QuestAction.Run)
                                    {
                                        RunQuestClassic(listQuest);
                                        return;
                                    }

                                    if (listQuest.QuestAction == QuestAction.Complete)
                                    {
                                        ComliteQuestClassic(listQuest);
                                        return;
                                    }

                                    Thread.Sleep(1000);
                                }
                            }
                            break;
                        case "Положить нитки на склад":
                            {
                                foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i)))
                                {
                                    if (!entity.IsBanker)
                                        continue;
                                    Host.CommonModule.MoveTo(entity, 4);
                                    Host.MyCheckIsMovingIsCasting();
                                    Thread.Sleep(2000);
                                    if (!Host.OpenBank(entity))
                                        Host.log("Не смог открыть банк " + Host.GetLastError(), LogLvl.Error);
                                    Thread.Sleep(2000);
                                    foreach (var item in Host.ItemManager.GetItems())
                                    {
                                        if (item.Id != 159959)
                                            continue;
                                        if (item.MoveToBank())
                                        {
                                            Host.log("Положил в банк", LogLvl.Ok);
                                        }
                                        else
                                        {
                                            Host.log("Не смог положить в банк " + Host.GetLastError(), LogLvl.Error);
                                            Thread.Sleep(5000);
                                            break;
                                        }
                                    }

                                    break;
                                }
                            }
                            break;
                        case "Прыжок":
                            {
                                Host.Jump();
                                Thread.Sleep(1000);
                            }
                            break;
                        case "Перезапустить окно":
                            {
                                Host.TerminateGameClient();
                            }
                            break;

                    }
                }

                double tempGold2 = Host.Me.Money;
                var gold = tempGold2 / 10000;
                // var gold = Host.Me.Money / 10000;
                var formattedTimeSpan = $"{ScriptStopwatch.Elapsed.Hours:D2} hr, {ScriptStopwatch.Elapsed.Minutes:D2} min, {ScriptStopwatch.Elapsed.Seconds:D2} sec";
                long invgold = 0;

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        //   log(item.Id + "  "+ item.Name);
                        //    invgold = invgold + item.GetSellPrice() * item.Count;
                    }
                }

                invgold -= startinvgold;
                var doubleGold = Convert.ToDouble(invgold) / 10000;
                // Host.log("Тест " + scriptStopwatch.Elapsed.Minutes + " " + scriptStopwatch.Elapsed.Seconds);

                if (Host.CharacterSettings.WaitSixMin)
                    if (ScriptStopwatch.Elapsed.Minutes > 2 && ScriptStopwatch.Elapsed.Minutes < 7)
                    {
                        while (ScriptStopwatch.Elapsed.Minutes < 7 && Host.MainForm.On)
                        {
                            Thread.Sleep(10000);
                            Host.log("Ожидаю до 6 минут " + ScriptStopwatch.Elapsed.Minutes + " " +
                                     ScriptStopwatch.Elapsed.Seconds + " " + ScriptStopwatch.Elapsed.TotalSeconds);
                        }
                    }


                Host.log(
                    "Скрипт выполнен за " + formattedTimeSpan + ". Мобов убито: " + (Host.KillMobsCount - mobsStart) +
                    " Золота получено: " + Math.Round(gold - goldStart, 2) + "г. + " + Math.Round(doubleGold, 2) +
                    " г. Всего: " + Math.Round((gold - goldStart) + doubleGold, 2) + " г. " + Host.Me.Name,
                    LogLvl.Ok);

                Host.Log(
                    "Скрипт выполнен за " + formattedTimeSpan + ". Мобов убито: " + (Host.KillMobsCount - mobsStart) +
                    " Золота получено: " + Math.Round(gold - goldStart, 2) + "г. + " + Math.Round(doubleGold, 2) +
                    " г. Всего: " + Math.Round((gold - goldStart) + doubleGold, 2) + " г. " + Host.Me.Name
                    , "Статистика Данжа");


                //  host.Log("После прохода стало " + invgold + "(" + (invgold - startinvgold ) + ")", "Статистика Данжа");
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        public void CheckGo(GameObject go, int anim)
        {
            if (go.OwnerGuid == Host.Me.Guid && go.Name == "Fishing Bobber")
            {
                Thread.Sleep(Host.RandGenerator.Next(100, 300));
                go.Use();
                StartWait = false;
            }
        }
    }
}