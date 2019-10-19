using Out.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WoWBot.Core;

namespace WowAI.Module
{
    internal partial class FarmModule
    {
        public int MobsWithSkinCount()
        {
            var result = 0;
            try
            {
                if (!Host.CharacterSettings.Skinning)
                    return 0;
                if (Host.SpellManager.GetSpell(8613) == null && Host.SpellManager.GetSpell(8617) == null)
                    return 0;

                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (!entity.IsSkinnable)
                        continue;
                    if (Host.Me.Distance(entity) > 55)
                        continue;
                    if (!Host.SkinUnits.Contains(entity))
                        continue;
                    if (Host.GetVar(entity, "skinFailed") == null)
                        result++;
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e) { Host.log(e.ToString()); }
            return result;
        }

        private void PickupSkinRoute()
        {
            try
            {
                if (!Host.CharacterSettings.Skinning)
                    return;
                if (Host.AutoQuests == null)
                    return;
                if (!Host.AutoQuests.EnableSkinning)
                    return;
                if (!Host.Me.IsAlive)
                    return;

                if (Host.GetAgroCreatures().Count != 0)
                    return;
                var mobsWithDropLoot = new List<Unit>();
                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (!Host.IsExists(entity))
                        continue;
                    if (Host.Me.Distance(entity) > 55)
                        continue;
                    if (!entity.IsSkinnable)
                        continue;
                    if (!Host.SkinUnits.Contains(entity))
                        continue;
                    if (Host.SpellManager.GetGatheringSpell(entity) == null)
                        continue;
                    if (Host.GetVar(entity, "skinFailed") == null)
                        mobsWithDropLoot.Add(entity);
                }

                if (mobsWithDropLoot.Count > 0)
                    Host.CommonModule.SuspendMove();

                foreach (var unit in mobsWithDropLoot.OrderBy(i => Host.Me.Distance(i)))
                {
                    if (!Host.MainForm.On)
                        return;

                    if (Host.Me.Target != unit)
                        Host.SetTarget(unit);
                    if (Host.MyIsNeedRegen() && Host.Me.Distance(unit) > 5)
                        return;
                    if (Host.CheckPathForMob(unit) != null)
                        return;
                    if (Host.Me.Distance(unit) > 2)
                        if (!Host.CommonModule.ForceMoveTo(unit, 2, false))
                            if (!Host.CommonModule.ForceMoveTo(unit, 2, false))
                            {
                                Host.SetVar(unit, "skinFailed", true);
                                continue;
                            }

                    Host.TurnDirectly(unit);
                    Host.CommonModule.MyUnmount();
                    Host.MyCheckIsMovingIsCasting();
                    if (!Host.IsExists(unit))
                        continue;
                    if (Host.GetAgroCreatures().Count != 0)
                        return;
                    var skillSkinning = Host.SpellManager.GetGatheringSpell(unit);
                    if (!Host.SpellManager.IsSpellReady(skillSkinning))
                    {
                        Host.log("Шкуродер не готов " + skillSkinning);
                        return;
                    }

                    Thread.Sleep(500);
                    var result = Host.SpellManager.CastSpell(skillSkinning.Id, unit);
                    Thread.Sleep(500);
                    if (result != ESpellCastError.SUCCESS)
                    {
                        Thread.Sleep(1000);
                        result = Host.SpellManager.CastSpell(skillSkinning.Id, unit);
                        Host.log(
                            "Не смог снять шкуру скилл: " + skillSkinning.Name + "[" + skillSkinning.Id + "] Dist: " +
                            Host.Me.Distance(unit) + "  Name: " + unit.Name + "[" + unit.Id + "]   " + Host.GetLastError() +
                            "  " + result, LogLvl.Error);
                        Host.SetVar(unit, "skinFailed", true);
                    }

                    while (Host.SpellManager.IsCasting)
                        Thread.Sleep(100);

                    Thread.Sleep(1000);
                    Host.SetVar(unit, "skinFailed", true);

                    if (Host.GetAgroCreatures().Count != 0)
                        return;

                    var waitTill = DateTime.UtcNow.AddSeconds(3);
                    while (waitTill > DateTime.UtcNow)
                    {
                        Thread.Sleep(100);
                        if (!Host.MainForm.On)
                            return;
                        if (unit.IsLootable)
                            break;
                    }

                    if (!unit.IsLootable)
                        continue;

                    if (!Host.CanPickupLoot())
                        if (!Host.OpenLoot(unit))
                            Host.log("Не смог открыть лут " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError(), LogLvl.Error);

                    if (Host.CanPickupLoot())
                    {
                        if (!Host.PickupLoot())
                        {
                            Host.log("Не смог поднять дроп " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError(), LogLvl.Error);
                            Host.SetVar(unit, "pickFailed", true);
                        }
                        else
                        {

                            if (Host.FarmModule.MobsWithDropCount() < 2)
                                Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        Host.log("Окно лута не открыто 2 " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError(), LogLvl.Error);
                        Host.SetVar(unit, "pickFailed", true);
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

        public int MobsWithDropCount()
        {
            var result = 0;
            try
            {
                if (!Host.CharacterSettings.PickUpLoot)
                    return 0;
                foreach (var unit in Host.GetEntities<Unit>())
                {
                    if (!Host.IsExists(unit))
                        continue;
                    if (!unit.IsLootable)
                        continue;
                    if (Host.Me.Distance(unit) > 55)
                        continue;
                    if (Host.Me.MovementFlags != EMovementFlag.Swimming)
                    {
                        if (Host.Me.Distance(unit) > 3)
                            if (!Host.CommonModule.CheckPathForLoc(Host.Me.Location, unit.Location))
                                continue;
                    }

                    if (Host.GetVar(unit, "pickFailed") == null)
                        result++;
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e) { Host.log(e.ToString()); }
            return result;
        }

        private void PickupDropRoute()
        {
            try
            {
                if (!Host.Me.IsAlive)
                    return;
                if (!Host.CharacterSettings.PickUpLoot)
                    return;
                if (Host.GetAgroCreatures().Count != 0)
                    return;

                if (Host.MapID == 2207)
                {
                    foreach (var entity in Host.GetEntities())
                    {
                        if (entity.Id == 21273 && Host.Me.Distance(entity) < 20)
                        {
                            Host.CommonModule.ForceMoveTo(entity, 0);
                        }
                    }
                }

                var mobsWithDropLoot = new List<Unit>();
                foreach (var unit in Host.GetEntities<Unit>())
                {
                    if (!Host.IsExists(unit))
                        continue;
                    if (!unit.IsLootable)
                        continue;
                    if (Host.Me.Distance(unit) > 55)
                        continue;
                    if (Host.Me.MovementFlags != EMovementFlag.Swimming)
                    {
                        if (Host.Me.Distance(unit) > 3)
                            if (!Host.CommonModule.CheckPathForLoc(Host.Me.Location, unit.Location))
                                continue;
                    }
                    if (Host.MyIsNeedRegen() && Host.Me.Distance(unit) > 5)
                        continue;

                    if (Host.GetVar(unit, "pickFailed") == null)
                        mobsWithDropLoot.Add(unit);
                }

                if (mobsWithDropLoot.Count > 0)
                    Host.CommonModule.SuspendMove();

                foreach (var unit in mobsWithDropLoot.OrderBy(i => Host.Me.Distance(i)))
                {

                    if (!Host.MainForm.On)
                        return;
                    if (!Host.IsExists(unit))
                        continue;
                    if (!unit.IsLootable)
                        continue;
                    if (Host.MyIsNeedRegen() && Host.Me.Distance(unit) > 5)
                        return;
                    if (Host.CheckPathForMob(unit) != null)
                        return;
                    if (Host.SpellManager.CurrentAutoRepeatSpellId != 0)
                        Host.SpellManager.CancelAutoRepeatSpell();
                    // Host.log("Бегу за дропом CurrentAutoRepeatSpellId:" + Host.CurrentAutoRepeatSpellId);
                    if (Host.FarmModule.BestMob != null || Host.GetAgroCreatures().Count > 0)
                        return;
                    if (Host.Me.Distance(unit) > 2)
                    {
                        if (!Host.CommonModule.ForceMoveTo(unit, 2, false))
                        {
                            Thread.Sleep(1000);
                            if (Host.FarmModule.BestMob != null || Host.GetAgroCreatures().Count > 0)
                                return;
                            Host.log("Бегу за дропом 2 ");
                            if (!Host.CommonModule.ForceMoveTo(unit, 2, false))
                            {
                                Host.log("Добавляю лут в игнор, так как не смог добежать");
                                Host.SetVar(unit, "pickFailed", true);
                                continue;
                            }
                        }
                    }

                    Host.MyCheckIsMovingIsCasting();
                    if (!Host.IsExists(unit))
                        continue;
                    if (!unit.IsLootable)
                        continue;

                    if (Host.GetAgroCreatures().Count != 0)
                        return;
                    if (!Host.CanPickupLoot())
                        if (!Host.OpenLoot(unit))
                            Host.log("Не смог открыть лут " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError() + "  Всего лута: " + mobsWithDropLoot.Count, LogLvl.Error);
                    Thread.Sleep(300);
                    if (Host.CanPickupLoot())
                    {
                        if (!Host.PickupLoot())
                        {
                            Host.log("Не смог поднять дроп " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError(), LogLvl.Error);
                            Host.SetVar(unit, "pickFailed", true);
                        }
                        else
                        {
                            Host.log("Поднял дроп " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError(), LogLvl.Ok);
                            Thread.Sleep(500);
                            if (Host.CharacterSettings.Mode == Mode.Questing && Host.AutoQuests.BestQuestId == 13523 &&
                                unit.Id == 32890)
                            {
                                var item = Host.MyGetItem(44975);
                                if (item != null)
                                    Host.MyUseItemAndWait(item, unit);
                            }

                            if (Host.CharacterSettings.Mode == Mode.Questing && Host.AutoQuests.BestQuestId == 13576 &&
                                unit.Id == 32999)
                            {
                                var item = Host.MyGetItem(44959);
                                if (item != null)
                                    Host.MyUseItemAndWait(item, unit);
                            }

                            if (Host.CharacterSettings.Mode == Mode.Questing && Host.AutoQuests.BestQuestId == 49666 &&
                                unit.Id == 134560)
                            {
                                var item = Host.MyGetItem(158884);
                                if (item != null)
                                    Host.MyUseItemAndWait(item, unit);
                            }

                            if (Host.CharacterSettings.Mode == Mode.Questing && Host.AutoQuests.BestQuestId == 49666 &&
                                unit.Id == 134558)
                            {
                                var item = Host.MyGetItem(158884);
                                if (item != null)
                                    Host.MyUseItemAndWait(item, unit);
                            }

                            if (Host.CharacterSettings.Mode == Mode.Questing && Host.AutoQuests.BestQuestId == 48573 &&
                                unit.Id == 126723)
                            {
                                var item = Host.MyGetItem(152596);
                                if (item != null)
                                    Host.MyUseItemAndWait(item, unit);
                            }

                            if (unit.Id == 134059 || unit.Id == 134062 || unit.Id == 134068)
                                if (Host.CharacterSettings.Mode == Mode.Questing &&
                                    Host.AutoQuests.BestQuestId == 47577)
                                {
                                    var item = Host.MyGetItem(160585);
                                    if (item != null)
                                        Host.MyUseItemAndWait(item, unit);
                                }

                            if (Host.ClientType == EWoWClient.Retail)
                                if (Host.FarmModule.MobsWithDropCount() < 2)
                                    Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        Host.log("Окно лута не открыто 1 " + Host.Me.Distance(unit) + "  " + unit.Name + "   " + Host.GetLastError() + "  Всего лута: " + mobsWithDropLoot.Count, LogLvl.Error);
                        if (MobsWithDropCount() == 1)
                        {
                            Host.log("Добавляю лут в игнор, так как не смог открыть");
                            Host.SetVar(unit, "pickFailed", true);
                        }

                        if (Host.ClientType == EWoWClient.Classic)
                            Host.CommonModule.ForceMoveTo(new Vector3F(Host.Me.Location.X + 2, Host.Me.Location.Y, Host.Me.Location.Z));
                        // host.SendKeyPress(0x1B);
                    }

                    if (Host.GetAgroCreatures().Count != 0)
                        break;
                    if (!Host.CharacterSettings.Skinning)
                        continue;

                    var waitTill = DateTime.UtcNow.AddSeconds(1);
                    while (waitTill > DateTime.UtcNow)
                    {
                        Thread.Sleep(100);
                      //  Host.log("Проверяю есть ли шкура " + MobsWithSkinCount());
                        if (!Host.MainForm.On)
                            return;
                        if (MobsWithSkinCount() > 0)
                            break;
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception err) { Host.log(err.ToString()); }
        }
    }
}