using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows;
using Out.Internal.Core;
using WoWBot.Core;
using WowAI.UI;
using Out.Utility;

namespace WowAI.Modules
{
    internal class CommonModule : Module
    {
        public GpsBase GpsBase = new GpsBase();
        public override void Stop()
        {
            try
            {
                Host.onUserNavMeshPreMoveFull -= NavMeshPreMoveFull;
                Host.onUserNavMeshPreMove -= NavMeshPreMove;
                Host.onMoveTick -= MyonMoveTick;
                Host.onCastFailedMessage -= MyonCastFailedMessage;
                Host.onSpellDamage -= MyonSpellDamage;
                
                base.Stop();
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }
        public override void Start(Host host)
        {
            try
            {
                base.Start(host);
                ZonesGps = new GpsBase();
                if (!File.Exists(Host.PathGps))
                {
                    Host.log("Не найден файл " + Host.PathGps);
                    Host.StopPluginNow();
                }
                GpsBase.LoadDataBase(Host.PathGps);
                host.onUserNavMeshPreMoveFull += NavMeshPreMoveFull;
                Host.onUserNavMeshPreMove += NavMeshPreMove;
                Host.onMoveTick += MyonMoveTick;
                Host.onCastFailedMessage += MyonCastFailedMessage;
                Host.onSpellDamage += MyonSpellDamage;
            }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }

        public void MyonSpellDamage(WowGuid casterGuid, WowGuid targetGuid, uint spellID, EAttackerStateFlags flags, int damage)
        {
            var target = Host.GetEntity(targetGuid);
            var caster = Host.GetEntity(casterGuid);
            if (casterGuid == Host.Me.Guid)
            {
                Host.AllDamage = Host.AllDamage + damage;
            }
            if (Host.AdvancedLog)
                Host.Log(caster.Name + "->" + target.Name + "[" + spellID + "][" + damage + "]" + " [" + flags + "]", "Damage");
        }

        public void MyonCastFailedMessage(uint spellId, ESpellCastError reason, int failedArg1, int failedArg2)
        {
            try
            {
                if (Host.AdvancedLog)
                    Host.log(spellId + " " + reason + " " + failedArg1 + " " + failedArg2);
                if (spellId == 131476 && reason == ESpellCastError.NOT_FISHABLE)
                {
                    Host.SetMoveStateForClient(true);
                    Host.TurnLeft(true);
                    Thread.Sleep(500);
                    Host.TurnLeft(false);
                    Host.SetMoveStateForClient(false);
                    Host.AutoQuests.StartWait = false;
                }
                if (spellId == 131476 && reason == ESpellCastError.TOO_SHALLOW)
                {
                    Host.SetMoveStateForClient(true);
                    Host.TurnLeft(true);
                    Thread.Sleep(500);
                    Host.TurnLeft(false);
                    Host.SetMoveStateForClient(false);
                    Host.AutoQuests.StartWait = false;
                }
            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        public void MyUnmount()
        {
            try
            {
                if (!Host.IsAlive(Host.Me) || Host.Me.MountId == 0)
                    return;
                foreach (var s in Host.Me.GetAuras())
                    if (s.IsPartOfSkillLine(777))
                        if (!s.Cancel())
                            Host.log("Не удалось отозвать маунта " + Host.GetLastError(), Host.LogLvl.Error);
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public Aura MyGetAura(uint id)
        {
            try
            {
                foreach (var aura in Host.Me.GetAuras())
                    if (aura.SpellId == id)
                        return aura;
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
            return null;
        }


        public void MySitMount(Vector3F loc)
        {
            try
            {
                if (!Host.IsOutdoors)
                    return;
                if (Host.MyGetAura(269564) != null)
                    return;
                if (Host.MyGetAura(267254) != null)
                    return;
                if (Host.MyGetAura(263851) != null)
                    return;
                if (Host.Me.Distance(loc) < 2)
                    return;
                if (Host.FarmModule.BestMob != null)
                    return;
                if (Host.CharacterSettings.Mode == EMode.Questing)
                {
                    if (Host.AutoQuests.BestQuestId == 47880)
                        return;
                    if (Host.Me.Distance(loc) < 50)
                        return;
                }
                //   Host.log("Маунт " + Host.CharacterSettings.CheckBoxAttackForSitMount + " " + Host.GetThreats(Host.Me).Count + " " + Host.Me.MountId);
                if (Host.CharacterSettings.Mode == EMode.Script)
                {

                    if (Host.CharacterSettings.CheckBoxAttackForSitMount && Host.Me.GetThreats().Count > 0 && Host.Me.MountId == 0)
                    {
                        //  Host.log("Атака");
                        Host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        return;
                    }
                    if (Host.FarmModule.farmState == FarmState.AttackOnlyAgro)
                    {
                        foreach (var entity in Host.GetEntities<Unit>())
                        {
                            if (Host.Me.Level > 10 && entity.Level == 1)
                                continue;
                            if (Host.Me.Distance(entity) > 50)
                                continue;
                            if (!entity.IsAlive)
                                continue;
                            if (!Host.CanAttack(entity, Host.CanSpellAttack))
                                continue;
                            return;
                        }
                    }
                }


                if (Host.MapID == 1643 && Host.AutoQuests.BestQuestId == 47098)
                    return;
                if (Host.MapID == 1904 || Host.MapID == 1929)
                {
                    return;
                }
                if (!Host.IsAlive(Host.Me) || _moveFailCount > 2 || Host.FarmModule.farmState == FarmState.FarmMobs || Host.Me.IsDeadGhost)
                    return;

                if (Host.Me.Class == EClass.Druid)
                {
                    if (Host.CharacterSettings.FormForMove != "Не использовать")
                    {
                        MyUnmount();
                        var formId = 0;
                        if (Host.CharacterSettings.FormForMove == "Облик медведя")
                            formId = 5487;
                        if (Host.CharacterSettings.FormForMove == "Облик кошки")
                            formId = 768;
                        if (Host.CharacterSettings.FormForMove == "Походный облик")
                            formId = 783;

                        foreach (var aura in Host.Me.GetAuras())
                        {
                            if (aura.SpellId == formId)
                            {
                                if (Host.CharacterSettings.UseDash && formId == 768) //Кошка, ускорение
                                {

                                    //106898 Тревожный рев
                                    //1850 Порыв
                                    var listDash = new List<uint> { 1850, 106898 };
                                    var isNeedDash = true;
                                    foreach (var i in listDash)
                                    {
                                        if (MyGetAura(i) != null)
                                        {
                                            isNeedDash = false;
                                        }
                                    }
                                    if (isNeedDash)
                                    {
                                        foreach (var i in listDash)
                                        {
                                            if (Host.SpellManager.GetSpellCooldown(i) != 0)
                                                continue;
                                            if (!Host.SpellManager.IsSpellReady(i))
                                                continue;
                                            if (Host.SpellManager.CheckCanCast(i, Host.Me) != ESpellCastError.SUCCESS)
                                                continue;
                                            var resultForm = Host.SpellManager.CastSpell(i);
                                            if (resultForm != ESpellCastError.SUCCESS)
                                            {
                                                Host.log("Не удалось использовать ускорение " + i + "  " + Host.SpellManager.CheckCanCast(i, Host.Me) + "  " + resultForm, Host.LogLvl.Error);
                                            }
                                            else
                                                if (Host.CharacterSettings.LogSkill)
                                                Host.log("Использовал ускорение " + Host.SpellManager.GetSpellCooldown(i) + "    " + Host.SpellManager.CheckCanCast(i, Host.Me) + "  " + i, Host.LogLvl.Ok);

                                            while (Host.SpellManager.IsCasting)
                                                Thread.Sleep(100);
                                            break;
                                        }
                                    }
                                }

                                if (formId == 783 && Host.Me.GetThreats().Count == 0)
                                {
                                    if (Host.Me.RunSpeed < 13 && Host.Me.SwimSpeed < 9)
                                    {
                                        Host.log("Отменяю форму так как скорость " + Host.Me.RunSpeed + "   " + Host.Me.MovementFlags);
                                        Host.CanselForm();
                                        Thread.Sleep(500);
                                        break;
                                    }
                                }
                                return;
                            }

                        }

                        Host.CanselForm();
                        foreach (var spell in Host.SpellManager.GetSpells())
                        {
                            if (spell.Id == formId)
                            {
                                var i = 0;
                                while (Host.SpellManager.CheckCanCast(spell.Id, Host.Me) != ESpellCastError.SUCCESS)
                                {
                                    if (!Host.MainForm.On)
                                        return;
                                    Thread.Sleep(100);
                                    i++;
                                    if (i > 10)
                                        break;
                                }
                                var resultForm = Host.SpellManager.CastSpell(spell.Id, Host.Me);
                                if (resultForm != ESpellCastError.SUCCESS)
                                {
                                    if (Host.AdvancedLog)
                                        Host.log("Не удалось поменять форму 1 " + spell.Name + "  " + resultForm, Host.LogLvl.Error);
                                }


                                while (Host.SpellManager.IsCasting)
                                    Thread.Sleep(100);
                                return;
                            }
                        }
                        return;
                    }
                }

                if (!Host.IsAlive(Host.Me) || _moveFailCount > 2 || InFight() || Host.FarmModule.farmState == FarmState.FarmMobs || Host.Me.MountId != 0 || Host.Me.IsDeadGhost)
                    return;





                Spell mountSpell = null;

                foreach (var s in Host.SpellManager.GetSpells())
                {
                    if (!s.SkillLines.Contains(777))
                        continue;
                    var IsNeedMount = false;
                    foreach (var i in Host.CharacterSettings.PetSettings)
                    {
                        if (i.Type != "Mount")
                            continue;
                        if (i.Id != s.Id)
                            continue;
                        IsNeedMount = true;
                    }
                    if (!IsNeedMount)
                        continue;
                    mountSpell = s;
                    break;
                }
                if (mountSpell == null)
                    return;

                if (Host.Me.Class == EClass.Druid)
                {
                    Host.log("Снимаю облик для использования маунта ", Host.LogLvl.Important);
                    Host.CanselForm();
                }
                Host.CancelMoveTo();
                Host.MyCheckIsMovingIsCasting();
                Thread.Sleep(500);
                var result = Host.SpellManager.CastSpell(mountSpell.Id);

                if (result != ESpellCastError.SUCCESS)
                {
                    Host.log("Не удалось призвать маунта " + mountSpell.Name + "  " + result, Host.LogLvl.Error);

                }
                else
                    Host.log("Призвал маунта", Host.LogLvl.Ok);

                while (Host.SpellManager.IsCasting)
                    Thread.Sleep(100);
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        public bool InFight()
        {
            try
            {
                if (Host.GetAgroCreatures().Count == 0)
                    return false;
                foreach (var i in Host.GetAgroCreatures())
                {
                    if (i == null)
                        continue;
                    if (!Host.IsAlive(i))
                        continue;
                    return true;
                }
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
            return false;
        }

        public bool AttackPlayer = false;
        public bool EventAttackPlayer;


        private void NavMeshPreMove(Vector3F point)
        {

            if (Host.FarmModule.BestMob != null && Host.FarmModule.BestMob.HpPercents < 100)
                return;

            if (Host.FarmModule.farmState == FarmState.Disabled)
                return;
            if (Host.MapID == 1904)
                return;


            if (Host.CharacterSettings.Attack)
            {
                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (Host.CharacterSettings.Mode == EMode.Questing)
                        break;
                    if (Host.GetAgroCreatures().Count > 0)
                        break;
                    if (Host.Me.Level > 10 && entity.Level == 1)
                        continue;

                    if (Host.Me.Distance(entity) > Host.CharacterSettings.AttackRadius)
                        continue;

                    var zRange = Math.Abs(Host.Me.Location.Z - entity.Location.Z);

                    if (zRange > 10)
                        continue;

                    if (!Host.CanAttack(entity, Host.CanSpellAttack))
                        continue;
                    if (!Host.IsAlive(entity))
                        continue;
                    if (Host.FarmModule.IsBadTarget(entity, Host.ComboRoute.TickTime))
                        continue;
                    if (Host.FarmModule.IsImmuneTarget(entity))
                        continue;
                    if (Host.CharacterSettings.Mode != EMode.Questing)
                    {

                        if (entity.Victim != null && entity.Victim != Host.Me && entity.Victim != Host.Me.GetPet())
                            continue;
                    }

                    if (Host.CharacterSettings.UseFilterMobs)
                    {
                        var mobsIgnore = false;
                        foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                        {
                            if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                if (characterSettingsMobsSetting.Priority == 1)
                                    mobsIgnore = true;
                        }
                        if (mobsIgnore)
                            continue;
                    }

                    if (entity != Host.FarmModule.BestMob)
                    {
                        Host.FarmModule.BestMob = entity;

                        Host.CancelMoveTo();
                        Host.CommonModule.SuspendMove();
                        if (Host.CharacterSettings.LogScriptAction)
                            Host.log("Перехват: " + Host.FarmModule.BestMob.Name + " дист:" + Host.Me.Distance(entity) + " всего:" + Host.GetAgroCreatures().Count + "  IsAlive:" + Host.IsAlive(entity) + " HP:" + entity.Hp, Host.LogLvl.Error);

                    }
                    return;
                }
            }

        }

        public bool UseObject = false;

        private void MyonMoveTick(Vector3F loc)
        {
            if (Host.CharacterSettings.Mode == EMode.Questing)
            {
                var list = Host.GetEntities();
                foreach (var entity in list.OrderBy(i => Host.Me.Distance(i)))
                {
                    if (entity.Id == 135983 && Host.Me.Distance(entity) < 15 && !Host.CommonModule.IsMovementSuspended && !UseObject)
                    {
                        Host.CommonModule.SuspendMove();
                        Host.ComeTo(entity);
                        Host.CommonModule.ResumeMove();
                        break;
                    }
                }

                if (Host.FarmModule.BestMob != null && Host.FarmModule.BestMob.HpPercents < 100)
                    return;

                if (Host.FarmModule.farmState == FarmState.Disabled)
                    return;

                if (Host.CharacterSettings.NoAttackOnMount)
                    if (Host.Me.MountId != 0)
                        return;

                foreach (var agromob in Host.GetAgroCreatures())
                {
                    if (Host.GetAgroCreatures().Contains(Host.FarmModule.BestMob) && Host.IsAlive(Host.FarmModule.BestMob))
                        return;

                    var zRange = Math.Abs(Host.Me.Location.Z - agromob.Location.Z);

                    if (zRange > 10)
                        continue;
                    if (!Host.IsAlive(agromob))
                        continue;


                    Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();

                    Host.CancelMoveTo();
                    Host.CommonModule.SuspendMove();
                    Host.log("Агр: " + Host.FarmModule.BestMob.Name + " дист:" + Host.Me.Distance(agromob) + " всего:" + Host.GetAgroCreatures().Count, Host.LogLvl.Error);
                    return;
                }

            }

            if (Host.Me.Class == EClass.DeathKnight && !Host.Me.IsInCombat && Host.IsAlive() && Host.Me.MountId == 0 && !Host.Me.IsDeadGhost)
            {

                var skill = Host.SpellManager.GetSpell(48265);
                if (Host.SpellManager.IsSpellReady(skill))
                {
                    var result = Host.SpellManager.CastSpell(skill.Id);
                    if (result != ESpellCastError.SUCCESS)
                        Host.log("Не смог использовать скилл ускорения" + result, Host.LogLvl.Error);
                }
            }

        }
        private void NavMeshPreMoveFull(Vector3F[] points)
        {
            try
            {
                if (Host.FarmModule.BestMob != null && Host.FarmModule.BestMob.HpPercents < 100)
                    return;

                if (Host.FarmModule.farmState == FarmState.Disabled)
                    return;

                if (Host.MapID == 1904)
                    return;

                if (Host.CharacterSettings.Attack)
                {
                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        var needbreak = false;
                        switch (entity.Id)
                        {
                            case 153962:
                                {

                                }
                                break;
                            case 153960:
                                {

                                }
                                break;
                            default:
                                {
                                    if (Host.CharacterSettings.Mode == EMode.Questing)
                                        needbreak = true;
                                }
                                break;
                        }
                        if (needbreak)
                            break;
                        /*  if (Host.Me.Target != null && Host.Me.Target.HpPercents < 100)
                              break;*/
                        if (Host.GetAgroCreatures().Count > 0)
                            break;
                        if (Host.Me.Level > 10 && entity.Level == 1)
                            continue;
                        if (Host.Me.Distance(entity) > Host.CharacterSettings.AttackRadius)
                            continue;
                        if (Host.CharacterSettings.Mode != EMode.Questing)
                        {

                            if (entity.Victim != null && entity.Victim != Host.Me && entity.Victim != Host.Me.GetPet())
                                continue;
                        }

                        var zRange = Math.Abs(Host.Me.Location.Z - entity.Location.Z);

                        if (zRange > 10)
                            continue;

                        if (!Host.CanAttack(entity, Host.CanSpellAttack))
                            continue;
                        if (!Host.IsAlive(entity))
                            continue;
                        if (Host.FarmModule.IsBadTarget(entity, Host.ComboRoute.TickTime))
                            continue;
                        if (Host.FarmModule.IsImmuneTarget(entity))
                            continue;


                        if (Host.CharacterSettings.UseFilterMobs)
                        {
                            var mobsIgnore = false;
                            foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                            {
                                if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                    if (characterSettingsMobsSetting.Priority == 1)
                                        mobsIgnore = true;
                            }
                            if (mobsIgnore)
                                continue;

                            /*  foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                              {
                                  if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                      if (characterSettingsMobsSetting.Priority == 2)
                                          return entity;
                              }
                              if (Host.FarmModule.farmState == FarmState.Disabled)
                                  return null;*/
                        }
                        if (entity != Host.FarmModule.BestMob)
                        {
                            Host.FarmModule.BestMob = entity;

                            Host.CancelMoveTo();
                            Host.CommonModule.SuspendMove();
                            if (Host.CharacterSettings.LogScriptAction)
                                Host.log(
                                "Перехват: " + Host.FarmModule.BestMob.Name + " дист:" + Host.Me.Distance(entity) +
                                " всего:" + Host.GetAgroCreatures().Count + "  IsAlive:" + Host.IsAlive(entity) +
                                " HP:" + entity.Hp, Host.LogLvl.Error);
                        }
                        return;
                    }
                }


                if (Host.CharacterSettings.NoAttackOnMount)
                {
                    if (Host.Me.MountId != 0)
                    {
                        return;
                    }
                }

                var sw = new Stopwatch();
                sw.Start();

                foreach (var agromob in Host.GetAgroCreatures())
                {
                    if (Host.GetAgroCreatures().Contains(Host.FarmModule.BestMob) && Host.IsAlive(Host.FarmModule.BestMob))
                        return;

                    var zRange = Math.Abs(Host.Me.Location.Z - agromob.Location.Z);

                    if (zRange > 10)
                        continue;
                    if (!Host.IsAlive(agromob))
                        continue;


                    Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();

                    Host.CancelMoveTo();
                    Host.CommonModule.SuspendMove();
                    Host.log("Агр: " + Host.FarmModule.BestMob.Name + " дист:" + Host.Me.Distance(agromob) + " всего:" + Host.GetAgroCreatures().Count, Host.LogLvl.Error);
                    return;
                }
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }


        private void UseItemsMountAndPet()
        {
            try
            {
                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                        {
                            if (item.Id == characterSettingsItemSetting.Id && characterSettingsItemSetting.Use == 5)
                            {
                                Host.CommonModule.SuspendMove();
                                Host.CommonModule.MyUnmount();
                                Host.log("Использую " + item.Name);
                                var result = Host.SpellManager.UseItem(item);
                                if (result != EInventoryResult.OK)
                                {
                                    Host.log("Не смог использовать итем " + item.Name + "  " + result,
                                        Host.LogLvl.Error);
                                }

                                while (Host.SpellManager.IsCasting)
                                    Thread.Sleep(100);
                                Host.CommonModule.ResumeMove();
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        public EEquipmentSlot GetItemEPlayerPartsType(EInventoryType type)
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
                    return EEquipmentSlot.MainHand;
                case EInventoryType.Shield:
                    return EEquipmentSlot.OffHand;

                case EInventoryType.Ranged:
                    return EEquipmentSlot.MainHand;
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
                    return EEquipmentSlot.MainHand;
                    /*  case EInventoryType.Quiver:
                          break;*/
                    /*  case EInventoryType.Relic:
                          break;   */

            }
            return EEquipmentSlot.Ranged;
        }

        private void EquipBestArmorAndWeapon()
        {
            try
            {
                if (!Host.Me.IsAlive)
                    return;
                if (Host.Me.IsDeadGhost)
                    return;
                var equipCells = new Dictionary<EEquipmentSlot, Item>();
                //  Host.log("Тест  EquipBestArmorAndWeapon");

                foreach (EEquipmentSlot value in Enum.GetValues(typeof(EEquipmentSlot)))
                {
                    if (value == EEquipmentSlot.Ranged)
                        continue;
                    equipCells.Add(value, null);
                    // Host.log(value.ToString());
                }

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Equipment)
                        if (item.ItemClass == EItemClass.Armor || item.ItemClass == EItemClass.Weapon)
                        {

                            if (item.Id == 68743)//Прочный плащ пехотинца
                                continue;

                            /* if (item.Db.ClassReq.Count > 0 && !item.Db.ClassReq.Contains((host.Me as Avatar).GameClass))
                          continue;*/
                            if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                                continue;

                            if (item.RequiredLevel > Host.Me.Level)
                                continue;

                            var itemEquipType = GetItemEPlayerPartsType(item.InventoryType);
                            /* if (item.ItemClass == EItemClass.Armor)
                             {
                                 if (!item.CanEquipItem())
                                     Host.log(item.Name + "  " + itemEquipType + "  " + (EItemSubclassArmor)item.ItemSubClass + "   " + item.InventoryType, Host.LogLvl.Error);
                                 else
                                 {
                                     Host.log(item.Name + "  " + itemEquipType + "  " + (EItemSubclassArmor)item.ItemSubClass + "   " + item.InventoryType, Host.LogLvl.Important);
                                 }
                             }*/

                            if (item.ItemClass == EItemClass.Weapon && !WeaponType.Contains((EItemSubclassWeapon)item.ItemSubClass))
                                continue;
                            if (item.ItemClass == EItemClass.Armor && !ArmorType.Contains((EItemSubclassArmor)item.ItemSubClass))
                                continue;



                            if (item.ItemClass == EItemClass.Weapon)
                            {
                                if (Host.CharacterSettings.EquipItemStat != 0)
                                {
                                    if (!item.ItemStatType.Contains((EItemModType)Host.CharacterSettings.EquipItemStat))
                                        continue;
                                }
                            }


                            if (!WeaponAndShield)
                                if (itemEquipType == EEquipmentSlot.OffHand)
                                    continue;
                            /* if (itemEquipType == EEquipmentSlot.Unk17)
                                 continue;*/
                            /*   if (item.ItemClass == EItemClass.Weapon)
                               {
                                   if (!item.CanEquipItem())
                                       Host.log(item.Name + "  " + itemEquipType + "  " + (EItemSubclassWeapon)item.ItemSubClass + "   " + item.InventoryType, Host.LogLvl.Error);
                                   else
                                   {
                                       Host.log(item.Name + "  " + itemEquipType + "  " + (EItemSubclassWeapon)item.ItemSubClass + "   " + item.InventoryType, Host.LogLvl.Important);
                                   }
                               }*/



                            if (equipCells[itemEquipType] == null)
                            {
                                equipCells[itemEquipType] = item;
                            }
                            else
                            {
                                double bestCoef = 0;
                                double curCoef = 0;

                                bestCoef = equipCells[itemEquipType].Level;

                                curCoef = item.Level;
                                if (bestCoef < curCoef)
                                    equipCells[itemEquipType] = item;
                            }
                        }
                }


                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Equipment)
                        if (item.ItemClass == EItemClass.Armor && (EItemSubclassArmor)item.ItemSubClass == EItemSubclassArmor.MISCELLANEOUS)
                        {
                            if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                                continue;

                            if (item.RequiredLevel > Host.Me.Level)
                                continue;
                            if (item.InventoryType != EInventoryType.Finger)
                                continue;
                            if (equipCells.ContainsValue(item))
                                continue;

                            if (equipCells[EEquipmentSlot.Finger2] == null)
                            {
                                equipCells[EEquipmentSlot.Finger2] = item;
                            }
                            else
                            {
                                double bestCoef = 0;
                                double curCoef = 0;

                                bestCoef = equipCells[EEquipmentSlot.Finger2].Level;

                                curCoef = item.Level;
                                if (bestCoef < curCoef)
                                    equipCells[EEquipmentSlot.Finger2] = item;
                            }
                        }
                }

                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Equipment)
                        if (item.ItemClass == EItemClass.Armor && (EItemSubclassArmor)item.ItemSubClass == EItemSubclassArmor.MISCELLANEOUS)
                        {
                            if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                                continue;
                            if (item.RequiredLevel > Host.Me.Level)
                                continue;
                            if (item.InventoryType != EInventoryType.Trinket)
                                continue;
                            if (equipCells.ContainsValue(item))
                                continue;
                            if (equipCells[EEquipmentSlot.Trinket2] == null)
                            {
                                equipCells[EEquipmentSlot.Trinket2] = item;
                            }
                            else
                            {
                                double bestCoef = 0;
                                double curCoef = 0;

                                bestCoef = equipCells[EEquipmentSlot.Trinket2].Level;

                                curCoef = item.Level;
                                if (bestCoef < curCoef)
                                    equipCells[EEquipmentSlot.Trinket2] = item;
                            }
                        }
                }


                if (equipCells[EEquipmentSlot.MainHand].InventoryType != EInventoryType.TwoHandedWeapon)
                {
                    foreach (var item in Host.ItemManager.GetItems())
                    {
                        if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                            item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                            item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Equipment)
                            if (item.ItemClass == EItemClass.Armor || item.ItemClass == EItemClass.Weapon)
                            {

                                if (item.Id == 68743)//Прочный плащ пехотинца
                                    continue;

                                if (!item.CanEquipItem() && item.Place != EItemPlace.Equipment)
                                    continue;

                                if (item.RequiredLevel > Host.Me.Level)
                                    continue;

                                var itemEquipType = GetItemEPlayerPartsType(item.InventoryType);

                                if (item.ItemClass != EItemClass.Weapon)
                                    continue;
                                if (item.InventoryType == EInventoryType.TwoHandedWeapon)
                                    continue;
                                if (item.ItemClass == EItemClass.Weapon && !WeaponType.Contains((EItemSubclassWeapon)item.ItemSubClass))
                                    continue;

                                if (itemEquipType != EEquipmentSlot.MainHand)
                                    continue;



                                if (item.ItemClass == EItemClass.Weapon)
                                {
                                    if (Host.CharacterSettings.EquipItemStat != 0)
                                    {
                                        if (!item.ItemStatType.Contains((EItemModType)Host.CharacterSettings.EquipItemStat))
                                            continue;
                                    }
                                }



                                if (equipCells[EEquipmentSlot.MainHand] == item)
                                    continue;
                                if (equipCells[EEquipmentSlot.OffHand] == null)
                                {
                                    equipCells[EEquipmentSlot.OffHand] = item;
                                }
                                else
                                {
                                    double bestCoef = 0;
                                    double curCoef = 0;

                                    bestCoef = equipCells[EEquipmentSlot.OffHand].Level;

                                    curCoef = item.Level;
                                    if (bestCoef < curCoef)
                                        equipCells[EEquipmentSlot.OffHand] = item;
                                }


                            }
                    }
                }

                /*    Host.log("-----------------------------------------");
                   foreach (var equipCell in equipCells)
                   {

                       Host.log(equipCell.Key + " " + equipCell.Value?.Name + "  " + equipCell.Value?.Place + "  " + "  ");
                   }*/

                foreach (var b in equipCells.Keys.ToList())
                {
                    if (equipCells[b] != null && equipCells[b].Place != EItemPlace.Equipment)
                    {
                        if (equipCells[b].Equip())
                        {
                            Host.log("Одеваю " + equipCells[b].InventoryType + "  best item = " + equipCells[b].Name, Host.LogLvl.Ok);
                            Thread.Sleep(Host.RandGenerator.Next(555, 1555));
                        }
                        else
                        {
                            Host.log("Error. Can't equip " + equipCells[b].InventoryType + "  " + b + "  best item = " +
                                        equipCells[b].Name + ". Reason = " + Host.GetLastError(),
                                        Host.LogLvl.Error);
                            Thread.Sleep(Host.RandGenerator.Next(555, 1555));
                        }
                    }
                }

            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }



        public bool Equip = true;





        private void ActionEvent(EventSettings events)
        {

            try
            {
                if (events.ActionEvent == CharacterSettings.EventsAction.Log)
                {
                    Host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Host.Me.Name + "(" + Host.Me.Level + ")" + " Внимание сработало событие " + events.TypeEvents, "События");
                }

                if (events.ActionEvent == CharacterSettings.EventsAction.ShowGameClient)
                {
                    //host.ShowGameClient();
                }
                if (events.ActionEvent == CharacterSettings.EventsAction.ShowQuester)
                {
                    Host.MainForm.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Host.MainForm.Main1.WindowState = WindowState.Normal;
                    }));
                }

                if (events.ActionEvent == CharacterSettings.EventsAction.Pause)
                {
                    Host.MainForm.On = false;
                    Thread.Sleep(events.Pause);
                    Host.MainForm.On = true;
                }

                if (events.ActionEvent == CharacterSettings.EventsAction.ExitGame)
                {
                    Host.GetCurrentAccount().IsAutoLaunch = false;
                    // host.CloseGameClient();

                }
                if (events.ActionEvent == CharacterSettings.EventsAction.PlaySound)
                {
                    if (File.Exists(events.SoundFile))
                    {
                        var sp = new SoundPlayer(events.SoundFile);
                        sp.Play();
                    }
                    else
                    {
                        Host.log("Файл не найден " + events.SoundFile);
                    }
                }
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        public class MyObstacle
        {
            public Vector3F Loc;
            public int hight;
            public int radius;
        }

        public List<MyObstacle> MyObstacles = new List<MyObstacle>
        {
            new MyObstacle{Loc = new Vector3F(-1383.33, -5153.40, 7.56), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(2063.42, 2956.96, 37.55), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(519.97, -163.17, -194.33), radius = 12, hight = 12},
            new MyObstacle{Loc = new Vector3F(4039.70, 372.60, 69.14), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-661.77, 89.38, 274.51), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-896.24, -193.22, 222.11), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(1770.84, -4512.96, 27.43), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-213.00, 396.73, 201.68), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(2452.94, 827.52, 10.89), radius = 1, hight = 1},
            new MyObstacle{Loc = new Vector3F(2240.47, 4345.53, 37.74), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(2220.45, 4345.64, 38.29), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(2896.73, 2534.25, 61.64), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(1475.53, 1596.20, 45.40), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(2047.28, 1666.26, 22.94), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(1012.61, 1382.93, 22.30), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(-1431.77, 1261.11, 193.38), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(2658.31, 3391.74, 133.48), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(993.22, 3239.45, 92.20), radius = 2, hight = 2},
            new MyObstacle{Loc = new Vector3F(-915.82, 1235.41, 320.08), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-910.73, 1230.04, 320.40), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-912.35, 1222.49, 319.68), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-924.24, 1240.08, 319.71), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(-920.18, 1238.42, 319.87), radius = 4, hight = 4},
            new MyObstacle{Loc = new Vector3F(2751.12, 3330.07, 64.17), radius = 8, hight = 8},
            new MyObstacle{Loc = new Vector3F(1941.12, 1838.47, 21.27), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1942.03, 1837.25, 22.13), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(562.28, -262.14, -193.66), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(552.66, -255.10, -193.98), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(565.61, -262.10, -194.02), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(572.10, -263.93, -193.22), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(584.38, -265.50, -194.47), radius = 5, hight = 5},
            new MyObstacle{Loc = new Vector3F(602.19, -255.42, -194.26), radius = 5, hight = 5},
            new MyObstacle{Loc = new Vector3F(556.18, -255.87, -194.62), radius = 5, hight = 5},
            new MyObstacle{Loc = new Vector3F(561.91, -257.68, -195.36), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1698.78, 85.40, -62.29), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1695.71, 78.76, -62.29), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1696.49, 82.19, -62.16), radius = 3, hight = 3},

            new MyObstacle{Loc = new Vector3F(1725.81, 95.17, -61.95), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1729.45, 98.78, -61.95), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1731.94, 101.25, -61.94), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1735.93, 105.13, -61.94), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1738.70, 108.33, -61.87), radius = 3, hight = 3},

            new MyObstacle{Loc = new Vector3F(1757.61, 129.05, -62.29), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1759.42, 133.02, -62.29), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1764.51, 141.31, -62.30), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1765.49, 148.31, -62.30), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1768.04, 153.55, -62.30), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1764.04, 145.33, -62.30), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1765.78, 136.52, -62.30), radius = 3, hight = 3},
            new MyObstacle{Loc = new Vector3F(1642.75, 66.45, -61.62), radius = 5, hight = 5},

            new MyObstacle{Loc = new Vector3F(1646.98, 72.97, -62.18), radius = 5, hight = 5},
            new MyObstacle{Loc = new Vector3F(1648.42, 66.85, -62.18), radius = 5, hight = 5},
        };

        public List<uint> NoObstacle = new List<uint>
        {
            271556,
            271170,
            271557,
            231074,
            282637,
        };

        public void ModuleTick()
        {
            try
            {
                if (Host.GameState == EGameState.Ingame && Host.IsAlive(Host.Me) && Host.Me.Level > 0)
                {

                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if (Host.GetVar(entity, "obstacle") != null)
                            continue;
                        if (entity.Id == 131789 || entity.Id == 131753 || entity.Id == 135875 || entity.Id == 142484 || entity.Id == 142485 || entity.Id == 142478
                            || entity.Id == 142360 || entity.Id == 135119 || entity.Id == 143875 || entity.Id == 141780 || entity.Id == 135388 || entity.Id == 143482)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(entity.Location.X, entity.Location.Y, entity.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + entity.CollisionHeight + " " + entity.CollisionScale + " " + entity.ObjectSize + " " + entity.ObjectSize2 + " " + entity.Scale);
                                Host.BuildQuad(entity.Location.X, entity.Location.Y, 0, 0, 0, entity);
                                //  Host.AddObstacle(new Vector3F(entity.Location.X, entity.Location.Y, entity.Location.Z), 30, 30);
                                Host.SetVar(entity, "obstacle", true);
                            }
                        }
                    }

                    foreach (var gameObject in Host.GetEntities<GameObject>())
                    {
                        if (Host.GetVar(gameObject, "obstacle") != null)
                            continue;
                        if (NoObstacle.Contains(gameObject.Id))
                            continue;
                        if (gameObject.GameObjectType == EGameObjectType.Mailbox)//Почтовый ящик
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                //  Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 1, 1);
                                Host.SetVar(gameObject, "obstacle", true);
                            }
                        }

                        /*  if (gameObject.GameObjectType == EGameObjectType.Generic)
                          {
                              if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                              {
                                  // Host.log("Ставлю обстакл " + gameObject.Name + " " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                  Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 3, 3);
                                  Host.SetVar(gameObject, "obstacle", true);
                              }
                          }*/

                        if (gameObject.Name == "Barricade")
                        {
                            // Host.log("Проверяю обстаклы " + Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)));
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                //  Host.log("Ставлю обстакл");
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 8, 8);
                                Host.SetVar(gameObject, "obstacle", true);
                            }
                        }
                    }

                    foreach (var myObstacle in MyObstacles)
                    {
                        if (Host.Me.Distance(myObstacle.Loc) < 150)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(myObstacle.Loc)))
                            {
                                //  Host.log("Ставлю обстакл ");
                                Host.AddObstacle(myObstacle.Loc, myObstacle.hight, myObstacle.radius);
                            }
                        }
                    }



                    if (!InFight())
                    {
                        UseItemsMountAndPet();


                        if (Equip && Host.CharacterSettings.AutoEquip)
                            EquipBestArmorAndWeapon();
                    }
                }


                foreach (var eventSetting in Host.CharacterSettings.EventSettings)
                {
                    if (eventSetting.TypeEvents == CharacterSettings.EventsType.Death && Host.ComboRoute.EventDeath)
                    {
                        ActionEvent(eventSetting);
                        Host.ComboRoute.EventDeath = false;
                    }

                    if (eventSetting.TypeEvents == CharacterSettings.EventsType.DeathPlayer && Host.ComboRoute.EventDeath && Host.CommonModule.AttackPlayer)
                    {
                        ActionEvent(eventSetting);
                        Host.ComboRoute.EventDeath = false;
                    }


                    if (eventSetting.TypeEvents == CharacterSettings.EventsType.Inactivity && Host.EventInactive)
                    {
                        ActionEvent(eventSetting);
                        Host.EventInactiveCount = 0;
                        Host.EventInactive = false;
                    }


                    if (eventSetting.TypeEvents == CharacterSettings.EventsType.AttackPlayer && Host.CommonModule.EventAttackPlayer)
                    {
                        ActionEvent(eventSetting);
                        EventAttackPlayer = false;
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        //Загрузка зоны
        public GpsBase ZonesGps;



        private bool _isMovementSuspended;
        private bool _isMoveToNow;

        private bool IsMovementSuspended
        {
            get { return _isMovementSuspended; }
            set
            {
                _isMovementSuspended = value;
                // host.MainForm.SetIsMovementSuspendedText(_isMovementSuspended.ToString());
            }
        }

        public bool IsMoveToNow
        {
            get { return _isMoveToNow; }
            set
            {
                _isMoveToNow = value;
                // host.MainForm.SetIsMoveToNowText(_isMoveToNow.ToString());
            }
        }

        public bool IsMoveSuspended()
        {
            return IsMovementSuspended;
        }

        public void SuspendMove()
        {
            //  if (IsMoveToNow)
            Host.CancelMoveTo();
            IsMovementSuspended = true;
        }

        public void ResumeMove()
        {
            IsMovementSuspended = false;
        }

        public int _moveFailCount;

        private void CheckMoveFailed(bool result)
        {
            try
            {
                if (Host.GetTimerInfo(EMirrorTimerType.Breath).IsActivated)
                {
                    Host.log("Я под водой " + Host.GetTimerInfo(EMirrorTimerType.Breath).SpellId + "  " + Host.GetTimerInfo(EMirrorTimerType.Breath).Scale + " " + Host.GetTimerInfo(EMirrorTimerType.Breath).InitialValue + " " + Host.GetTimerInfo(EMirrorTimerType.Breath).MaxValue);
                    if (Host.Area.Id == 17 && Host.Zone.Id == 391)
                    {
                        Host.Ascend(true);
                        Thread.Sleep(2000);
                        Host.Ascend(false);
                    }

                    if (Host.MapID == 600 && Host.Area.Id == 4196)
                    {
                        Host.Ascend(true);
                        Thread.Sleep(2000);
                        Host.Ascend(false);
                    }
                }


                if (!result)
                {
                    var le = Host.GetLastError();
                    if (le != ELastError.Movement_MoveCanceled && le != ELastError.Movement_AnotherMoveCalled && le != ELastError.NotIngame)
                    {
                        _moveFailCount++;
                        Host.log("Застрял: " + Host.GetLastError() + "(" + _moveFailCount + ")" + " " + Host.Me.MovementFlagExtra + " " + Host.Me.MovementFlags + " " + Host.Me.SwimBackSpeed +
                            " " + Host.Me.SwimSpeed, Host.LogLvl.Error);
                        if (Host.Me.MovementFlags == EMovementFlag.Pending_root)
                        {
                            _moveFailCount = 0;
                            return;
                        }

                        while ((Host.Me.MovementFlags & EMovementFlag.DisableGravity) == EMovementFlag.DisableGravity && (Host.Me.MovementFlags & EMovementFlag.Root) == EMovementFlag.Root)
                        {
                            if (!Host.MainForm.On)
                                return;
                            if (Host.MyGetAura(269564) != null)
                                break;
                            if (Host.MyGetAura(245831) != null)
                                break;
                            if (Host.MapID == 1718)
                                break;
                            Host.log("Ожидаю возврата движения " + Host.Me.MovementFlags);
                            Thread.Sleep(5000);

                        }
                    }
                    else
                    {
                        if (Host.Area.Id == 141 && Host.MapID == 1 && Host.Zone.Id == 262)
                        {

                        }
                        else
                        {
                            _moveFailCount = 0;
                        }
                    }
                }
                else
                    _moveFailCount = 0;



                if (_moveFailCount > 0)
                {
                    // Host.log("Прыжок");
                    Host.SetMoveStateForClient(true);
                    Host.MoveForward(true);
                    Host.SetMoveStateForClient(false);
                    Host.Jump();
                    Host.SetMoveStateForClient(true);
                    Host.MoveForward(false);
                    Host.SetMoveStateForClient(false);
                    //  Host.log("Прыгнул");
                    if (Host.Me.MovementFlags == EMovementFlag.Swimming)
                    {
                        Host.CancelMoveTo();
                        if (Host.RandGenerator.Next(0, 2) == 0)
                            Host.Descend(true);
                        else
                            Host.Ascend(true);
                        Thread.Sleep(2000);
                        Host.Descend(false);
                        Host.Ascend(false);
                    }
                }

                if (_moveFailCount > 2)
                {
                    var safePoint = new List<Vector3F>();
                    var xc = Host.Me.Location.X;
                    var yc = Host.Me.Location.Y;

                    var radius = 3;
                    const double a = Math.PI / 16;
                    double u = 0;
                    for (var i = 0; i < 32; i++)
                    {
                        var x1 = xc + radius * Math.Cos(u);
                        var y1 = yc + radius * Math.Sin(u);
                        // log(" " + i + " x:" + x + " y:" + y);
                        u = u + a;
                        var z1 = Host.GetNavMeshHeight(new Vector3F(x1, y1, 0));
                        if (Host.IsInsideNavMesh(new Vector3F((float)x1, (float)y1, (float)z1)))
                            safePoint.Add(new Vector3F((float)x1, (float)y1, (float)z1));
                    }
                    Host.log("Пытаюсь обойти препядствие " + safePoint.Count);
                    if (safePoint.Count > 0)
                    {
                        var bestPoint = safePoint[Host.RandGenerator.Next(safePoint.Count)];
                        Host.ComeTo(bestPoint);
                    }
                }
                if (_moveFailCount > 3 && Host.CharacterSettings.UnmountMoveFail)
                    MyUnmount();

                if (_moveFailCount > 4 && _moveFailCount < 10)
                {
                    Host.CancelMoveTo();
                    Host.SetMoveStateForClient(true);
                    Host.MoveBackward(true);
                    Thread.Sleep(2000);
                    Host.MoveBackward(false);
                    Host.SetMoveStateForClient(false);
                }

                if (_moveFailCount > 4)
                {
                    Host.CancelMoveTo();
                    Host.SetMoveStateForClient(true);
                    if (Host.RandGenerator.Next(0, 2) == 0)
                        Host.StrafeLeft(true);
                    else
                        Host.StrafeRight(true);
                    Thread.Sleep(2000);
                    Host.StrafeRight(false);
                    Host.StrafeLeft(false);
                    Host.SetMoveStateForClient(false);
                }

                if (_moveFailCount > 10)
                {
                    if (Host.Me.IsDeadGhost)
                        Host.ComboRoute.DeathTime = DateTime.Now;
                }
                //  if (Host.CharacterSettings.Mode != EMode.Questing)
                if (_moveFailCount > 20 || Host.Me.Distance(1477.98, 1591.57, 39.66) < 5)
                {
                    if (Host.MapID != 1643)
                    {
                        Host.CanselForm();
                        Host.CancelMoveTo();
                        Host.MyCheckIsMovingIsCasting();
                        Thread.Sleep(2000);
                        foreach (var item in Host.ItemManager.GetItems())
                        {

                            if (item.Id == 6948)
                            {
                                if (Host.SpellManager.GetItemCooldown(item) != 0)
                                {
                                    Host.log("Камень в КД " + Host.SpellManager.GetItemCooldown(item));
                                    break;
                                }
                                var result2 = Host.SpellManager.UseItem(item);
                                if (result2 != EInventoryResult.OK)
                                {
                                    Host.log("Не удалось использовать камень " + item.Name + " " + result2 + " " + Host.GetLastError(), Host.LogLvl.Error);
                                }
                                else
                                {
                                    Host.log("Использовал камень ", Host.LogLvl.Ok);
                                }
                                Host.MyCheckIsMovingIsCasting();
                                while (Host.SpellManager.IsChanneling)
                                    Thread.Sleep(50);
                                Thread.Sleep(5000);
                                while (Host.GameState != EGameState.Ingame)
                                {
                                    Thread.Sleep(200);
                                }
                                Thread.Sleep(10000);
                                break;
                            }
                        }
                    }

                }

                /* if (_moveFailCount > 1)
                 {
                     if (host.Me.Mount != null)
                         host.Me.Unmount();
                     Thread.Sleep(1000);
                 }
                 if (_moveFailCount > 1)
                     host.Jump();

                 if (_moveFailCount > 5 && _moveFailCount < 9)
                 {
                     host.CancelMoveTo();
                     host.MoveBackward(true);
                     Thread.Sleep(1000);
                     host.MoveBackward(false);
                 }
                 if (_moveFailCount > 5)
                 {
                     host.CancelMoveTo();
                     if (host.RandGenerator.Next(0, 2) == 0)
                         host.MoveLeft(true);
                     else
                         host.MoveRight(true);
                     Thread.Sleep(1000);
                     host.MoveRight(false);
                     host.MoveLeft(false);
                 }
                 if (_moveFailCount > 15)
                 {
                     host.EmergencyEscape();
                 }*/

                /* if (_moveFailCount == 20)
                 {
                     host.log("Релог", Host.LogLvl.Error);
                     if ((string.Compare(host.GetBotLogin(), "outside", true) == 0))
                     {
                         var tmpCharacter = host.Me.Name;
                         host.MainForm.On = false;
                         Thread.Sleep(1000);
                         host.Restart();

                         Thread.Sleep(5000);
                         while (host.GameState != EGameState.CharacterSelect)
                         {
                             Thread.Sleep(1000);
                         }
                         Thread.Sleep(5000);
                         foreach (var gameCharacter in host.CurrentServer.GetCharacters())
                         {
                             if (gameCharacter.Name != tmpCharacter)
                                 continue;
                             gameCharacter.EnterGame();
                             Thread.Sleep(5000);
                             while (host.GameState != EGameState.Ingame)
                                 Thread.Sleep(1000);


                             host.MainForm.On = true;
                             break;
                         }
                     }
                     else
                     {
                         host.CloseGameClient();
                     }

                 }*/
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        public bool ForceMoveTo2(Vector3F loc, double dist = 1, bool useMount = true)
        {
            try
            {

                // if (host.Me.Distance(loc.X, loc.Y, loc.Z) > 50)

                if (useMount)
                    MySitMount(loc);
                else
                {
                    MyUnmount();
                }
                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;
                var doneDist = Host.Me.RunSpeed / 5.0;
                //  Host.log("Начал бег в " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + "/" + doneDist);
                var result = Host.ForceComeTo(loc, dist, doneDist);
                // Host.log("Закончил бег в " + loc + "  дист: " + Host.Me.Distance(loc));
                // 

                //  
                CheckMoveFailed(result);
                /* if (!result)
                     host.log(host.GetLastError().ToString());*/
                return result;
            }
            catch (ThreadAbortException)
            {
                IsMoveToNow = false;
                return false;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }

        public bool ForceMoveTo2(Entity obj, double dist = 1)
        {
            try
            {

                // if (host.Me.Distance(loc.X, loc.Y, loc.Z) > 50)
                MySitMount(obj.Location);

                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;

                //  Host.log("Начал бег в " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + "/" + doneDist);
                var result = Host.ForceComeTo(obj, dist, Host.Me.RunSpeed / 5.0);
                //  Host.log("Закончил бег в " + loc + "  дист: " + Host.Me.Distance(loc));
                // 

                //  
                CheckMoveFailed(result);
                /* if (!result)
                     host.log(host.GetLastError().ToString());*/
                return result;
            }
            catch (ThreadAbortException)
            {
                IsMoveToNow = false;
                return false;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }


        public bool MoveTo(double x, double y, double z, double dist = 1)
        {
            if (IsMovementSuspended)
                return false;
            try
            {
                if (Host.CharacterSettings.Mode == EMode.FarmMob)// "Убийство мобов")
                    if (Host.Me.Distance(x, y, z) > 500)
                        Host.FarmModule.farmState = FarmState.Disabled;
                //  if (host.Me.Distance(x, y, z) > 50)
                MySitMount(new Vector3F(x, y, z));


                IsMoveToNow = true;


                if (Host.GetNavMeshHeight(new Vector3F(x, y, 0)) == 0 && Host.Me.Distance(x, y, z) > 300)
                {
                    /* var x0 = Host.Me.Location.X;
                     var y0 = Host.Me.Location.Y;
                     var x1 = x;
                     var y1 = y;
                     var distance = Host.Me.Distance(x, y, z) - 400;
                     var expectedDistance = Host.Me.Distance(x, y, z) - distance;

                     //находим длину исходного отрезка
                     var dx = x1 - x0;
                     var dy = y1 - y0;
                     var l = Math.Sqrt(dx * dx + dy * dy);
                     //находим направляющий вектор
                     var dirX = dx / l;
                     var dirY = dy / l;
                     //умножаем направляющий вектор на необх длину
                     dirX *= expectedDistance;
                     dirY *= expectedDistance;
                     //находим точку
                     var resX = dirX + x0;
                     var resY = dirY + y0;
                     var z1 = Host.GetNavMeshHeight(new Vector3F(resX, resY, 0));
                     if (z1 > 0)
                     {
                         Host.log("Точка в мешах. Дист:" + Host.Me.Distance(resX, resY, z1));
                         CheckMoveFailed(Host.ComeTo(resX, resY, z1, 50, 50));
                         return false;
                     }
                     else
                     {
                         Host.log("Точка вне мешей " + Host.Me.Distance(resX, resY, z1) + " " + expectedDistance);
                         CheckMoveFailed(Host.ComeTo(resX, resY, z1, 250, 250));
                         return false;
                     }*/
                    var path = Host.GetServerPath(Host.Me.Location, new Vector3F(x, y, z));
                    for (var i = 0; i < path.Path.Count - 1; i++)
                    {
                        if (Host.Me.Distance(path.Path[i]) < 50)
                            continue;
                        if (!Host.Me.IsAlive)
                            return false;
                        if (!Host.MainForm.On)
                            return false;
                        if (!Host.ComeTo(path.Path[i], dist, Host.Me.RunSpeed / 5.0))
                        {
                            CheckMoveFailed(false);
                            return false;
                        }

                    }
                }

                var result = Host.ComeTo(x, y, z, dist, Host.Me.RunSpeed / 5.0);

                CheckMoveFailed(result);
                /*   if (!result)
                       host.log(host.GetLastError().ToString());*/
                return result;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }

        public bool MoveToBadLoc(Vector3F loc)
        {
            if (loc.Distance(1798.27, -4363.27, 102.85) < 20 && Host.Me.Distance(1567.82, -4400.66, 16.20) < 30)
            {
                Host.FlyForm();
                Host.ForceFlyTo(1565.31, -4401.46, 175.71);
                Host.ForceFlyTo(1783.75, -4323.89, 155.91);
                Host.ForceFlyTo(1802.62, -4366.94, 102.61);
            }

            if (Host.Me.Distance(-1130.89, 805.10, 500.08) < 20 && Host.Me.Location.Z > 480 && loc.Distance(-1130.89, 805.10, 500.08) > 30 && Host.AutoQuests.BestQuestId != 46930)
            {
                Host.log("Прыгаю вниз");
                if (!Host.ForceComeTo(-1100.62, 795.14, 497.08))
                    return false;
                if (!Host.ForceComeTo(-1087.30, 765.32, 487.73))
                    return false;
                if (!Host.ForceComeTo(-1046.30, 769.34, 435.33))
                    return false;
                return true;
            }

            // Host.log(loc.Distance(9835.40, 1558.26, 1292.05) + "  ");
            if (Host.Area.Id == 141 && Host.MapID == 1 && Host.Zone.Id == 262 && loc.Distance(9835.40, 1558.26, 1292.05) > 300)
            {

                Host.log("Пытаюсь выбраться из обители");
                if (!Host.ComeTo(9857.85, 1564.07, 1329.17))
                    return false;
            }




            if (loc.Distance(-835.04, -3729.01, 26.28) < 5 && Host.Me.Distance(-835.04, -3729.01, 26.28) > 5)
            {
                if (!Host.ComeTo(-843.43, -3736.47, 20.43))
                    return false;
                if (!Host.ForceComeTo(-849.96, -3732.85, 20.13))
                    return false;
                if (!Host.ForceComeTo(-840.81, -3724.28, 26.31))
                    return false;
                if (!Host.ForceComeTo(-837.63, -3728.02, 26.32))
                    return false;
                return true;
            }

            if (loc.Distance(-835.04, -3729.01, 26.28) > 5 && Host.Me.Distance(-835.04, -3729.01, 26.28) < 5)
            {

                if (!Host.ForceComeTo(-845.29, -3737.67, 21.42))
                    return false;

                return true;
            }



            if (loc.Distance(-1604.49, -4274.51, 9.50) < 15 && Host.Me.Distance(-1604.49, -4274.51, 9.50) > 15)
            {
                if (!Host.ComeTo(-1624.81, -4267.99, 6.87))
                    return false;
                if (!Host.ForceComeTo(-1625.89, -4274.09, 6.83))
                    return false;
                if (!Host.ForceComeTo(-1620.86, -4278.05, 9.87))
                    return false;
                if (!Host.ForceComeTo(-1616.36, -4274.60, 9.47))
                    return false;
                if (!Host.ForceComeTo(-1612.12, -4277.92, 9.50))
                    return false;
                return true;
            }

            if (loc.Distance(-1604.49, -4274.51, 9.50) > 15 && Host.Me.Distance(-1604.49, -4274.51, 9.50) < 15)
            {
                if (!Host.ComeTo(-1612.12, -4277.92, 9.50))
                    return false;
                if (!Host.ForceComeTo(-1616.36, -4274.60, 9.47))
                    return false;
                if (!Host.ForceComeTo(-1620.86, -4278.05, 9.87))
                    return false;
                if (!Host.ForceComeTo(-1625.89, -4274.09, 6.83))
                    return false;
                if (!Host.ForceComeTo(-1624.81, -4267.99, 6.87))
                    return false;
                return true;
            }
            /* -1624.81, -4267.99, 6.87
                 - 1625.89, -4274.09, 6.83
                 - 1620.86, -4278.05, 9.87
                 - 1616.36, -4274.60, 9.47
                 - 1612.12, -4277.92, 9.50




                 - 1604.49, -4274.51, 9.50*/

            if (Host.Me.Distance(9838.60, 438.81, 1317.18) < 8 && Host.Me.Distance(loc) > 20)
            {
                Host.log("Путь вниз");
                if (!Host.ComeTo(9834.55, 436.94, 1317.18))
                    return false;
                if (!Host.ForceComeTo(9837.37, 425.16, 1317.18))
                    return false;
                if (!Host.ForceComeTo(9826.38, 422.55, 1312.48))
                    return false;
                if (!Host.ForceComeTo(9822.03, 435.22, 1307.79))
                    return false;
                return true;
            }


            if (Host.Me.Distance(9845.00, 441.00, 1318.00) > 10 && loc.Distance(9845.00, 441.00, 1318.00) < 5 && Host.Me.Location.Z < 1316)
            {
                Host.log("Путь наверх");
                if (!Host.ComeTo(9822.09, 434.21, 1307.79))
                    return false;
                if (!Host.ForceComeTo(9826.54, 422.23, 1312.51))
                    return false;
                if (!Host.ForceComeTo(9837.74, 425.34, 1317.18))
                    return false;
                if (!Host.ForceComeTo(9834.92, 436.79, 1317.18))
                    return false;
                return true;
            }

            if (Host.Me.Distance(-245.88, -5102.19, 41.35) < 10 && Host.Me.Location.Z > 40)
            {
                if (!Host.ForceComeTo(-238.38, -5093.90, 41.35))
                    return false;
                if (!Host.ForceComeTo(-230.41, -5094.95, 41.35))
                    return false;
                if (!Host.ForceComeTo(-228.79, -5112.90, 34.07))
                    return false;
                if (!Host.ForceComeTo(-253.49, -5114.52, 34.07))
                    return false;
                return true;
            }

            if (loc.Distance(-839.00, -4893.00, 23.58) < 5)
            {
                if (!Host.ComeTo(-831.63, -4902.63, 19.81))
                    return false;
                if (!Host.ForceComeTo(new Vector3F((float)-837.42, (float)-4894.77, (float)23.47)))
                    return false;
                return true;
            }

            if (loc.Distance(1287.00, -4342.00, 34.08) < 5)
            {
                if (!Host.ComeTo(1303.27, -4336.21, 32.94))
                    return false;
                if (!Host.ForceComeTo(new Vector3F((float)1291.81, (float)-4338.69, (float)34.03)))
                    return false;
                return true;
            }



            if (loc.Distance(-769.15, -4948.53, 22.93) < 5)
            {
                if (!Host.ComeTo(-820.83, -4929.59, 20.04))
                    return false;
                if (!Host.ForceComeTo(new Vector3F((float)-798.74, (float)-4937.86, (float)22.25)))
                    return false;
                return true;
            }

            if (Host.Me.Distance(-1316.92, -5602.15, 23.72) < 5 && loc.Distance(-1316.92, -5602.15, 23.72) > 10)
                if (!Host.ForceComeTo(new Vector3F((float)-1287.11, (float)-5566.97, (float)20.93)))
                    return false;

            if (Host.Me.Distance(-839.31, -4892.87, 23.51) < 5 && loc.Distance(-839.31, -4892.87, 23.51) > 10)
                if (!Host.ForceComeTo(new Vector3F((float)-832.40, (float)-4902.90, (float)19.83)))
                    return false;

            if (Host.Me.Distance(-772.62, -4947.32, 22.88) < 5 && loc.Distance(-772.62, -4947.32, 22.88) > 10)
                if (!Host.ForceComeTo(new Vector3F((float)-821.92, (float)-4929.19, (float)20.03)))
                    return false;

            if (Host.Me.Distance(1288.26, -4338.71, 34.03) < 5 && loc.Distance(1288.26, -4338.71, 34.03) > 10)
                if (!Host.ForceComeTo(new Vector3F((float)1303.32, (float)-4336.79, (float)32.94)))
                    return false;

            return true;
        }

        public bool MoveToConvoy(Entity obj, double dist = 1, bool force = false)
        {
            try
            {
                if (!Host.MainForm.On)
                    return false;


                IsMoveToNow = true;


                var req = new MoveParams()
                {
                    Location = obj.Location,
                    Obj = null,
                    Dist = dist,
                    DoneDist = Host.Me.RunSpeed / 5.0,
                    UseNavCall = true,
                    NoWaitResult = true,
                    IgnoreStuckCheck = true
                };
                var result = Host.MoveTo(req);

                // CheckMoveFailed(result);             
                return result;
            }
            catch (ThreadAbortException)
            {
                IsMoveToNow = false;
                return false;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }

        public bool MoveTo(Vector3F loc, double dist = 1, double doneDist = 0.5, bool UseMount = true)
        {
            if (IsMovementSuspended)
                return false;
            try
            {

                if (!MyUseGps(loc, UseMount))
                    return false;
                // if (host.Me.Distance(loc.X, loc.Y, loc.Z) > 50)
                if (UseMount)
                    MySitMount(loc);

                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;

                if (!MoveToBadLoc(loc))
                {
                    CheckMoveFailed(false);
                    return false;
                }

                // if (Host.CharacterSettings.Mode == EMode.Questing || Host.AutoQuests.HerbQuest)
                if (Host.GetNavMeshHeight(new Vector3F(loc.X, loc.Y, 0)) == 0 && Host.Me.Distance(loc) > 300)
                {
                     if (Host.AutoQuests.BestQuestId == 50751)
                     {
                         if (Host.Me.Distance(loc) > 1800)
                         {
                             Host.MyUseTaxi(8501, new Vector3F(2034.23, 4810.68, 71.18));

                             return false;
                         }
                     }

                    if (Host.AutoQuests.BestQuestId == 50703 && Host.GetQuest(50703) != null)
                        if (Host.Area.Id == 8501) //волдун
                        {
                            Host.MyUseTaxi(8499, new Vector3F(-1035.45, 758.30, 435.33));
                            return false;
                        }

                    var path = Host.GetServerPath(Host.Me.Location, loc);
                    if (path == null
                        || Host.Me.Distance(-976.23, 1200.57, 283.16) < 80
                        || Host.Me.Distance(-972.04, 1121.50, 241.91) < 80
                        || Host.Me.Distance(-1036.92, 1130.74, 189.23) < 80
                        || path.Path.Count < 10)
                    {
                        Host.log("Не нашел путь " + Host.Me.Distance(loc) + " " + loc);

                        // loc.Z = 0;
                        // Host.log("Бегу по серверным мешам " + Host.Me.Location + " в " + loc);
                        var x0 = Host.Me.Location.X;
                        var y0 = Host.Me.Location.Y;
                        var x1 = loc.X;
                        var y1 = loc.Y;
                        var distance = Host.Me.Distance(loc) - 400;
                        var expectedDistance = Host.Me.Distance(loc) - distance;

                        //находим длину исходного отрезка
                        var dx = x1 - x0;
                        var dy = y1 - y0;
                        var l = Math.Sqrt(dx * dx + dy * dy);
                        //находим направляющий вектор
                        var dirX = dx / l;
                        var dirY = dy / l;
                        //умножаем направляющий вектор на необх длину
                        dirX *= expectedDistance;
                        dirY *= expectedDistance;
                        //находим точку
                        var resX = dirX + x0;
                        var resY = dirY + y0;
                        var z = Host.GetNavMeshHeight(new Vector3F(resX, resY, 0));
                        if (z > 0)
                        {
                            Host.log("Точка в мешах. Дист:" + Host.Me.Distance(resX, resY, z));
                            CheckMoveFailed(Host.ComeTo(resX, resY, z, 50, 50));
                            return false;
                        }
                        else
                        {
                            Host.log("Точка вне мешей " + Host.Me.Distance(resX, resY, z) + " " + expectedDistance);
                            CheckMoveFailed(Host.ComeTo(resX, resY, z, 250, 250));
                            return false;
                        }
                        // loc.Z = 116;
                    }
                    Host.log("Бегу по серверным мешам " + Host.Me.Location + " в " + loc + "  всего точек " + path.Path.Count);
                    foreach (var vector3F in path.Path)
                    {
                        //  Host.log(vector3F + " дистанция:" + Host.Me.Distance(vector3F));
                    }

                    for (var i = 0; i < path.Path.Count - 2; i++)
                    {
                        if (!Host.IsInsideNavMesh(path.Path[i]))
                            continue;
                        if (Host.Me.Distance(path.Path[i]) < 200)
                            continue;
                        if (Host.FarmModule.BestMob != null)
                            return false;
                        if (!Host.Me.IsAlive)
                            return false;
                        if (!Host.MainForm.On)
                            return false;
                         Host.log("Начал бег");
                        if (!Host.ComeTo(path.Path[i], dist, doneDist))
                        {
                                Host.log("Закончил бег");
                            CheckMoveFailed(false);
                            return false;
                        }

                       // return false;
                        //   Host.log("Закончил бег");
                    }

                    return false;
                }



                doneDist = Host.Me.RunSpeed / 6.0;
                if (loc.Distance(-986.00, -3797.00, 0.11) < 5)
                    loc.Z = (float)5.2;
                if (Host.Me.Distance(713.81, 3128.34, 133.02) < 30 && Host.Me.Distance(loc) > 300)
                    Host.MoveTo(749.13, 3099.93, 133.11);

                Host.log("Начал бег в " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + "/" + doneDist + "  " + Host.GetNavMeshHeight(new Vector3F(loc.X, loc.Y, 0)));
                var result = Host.ComeTo(loc, dist, doneDist);
                Host.log("Закончил бег в " + loc + "  дист: " + Host.Me.Distance(loc));

                CheckMoveFailed(result);
                /* if (!result)
                     host.log(host.GetLastError().ToString());*/
                return result;
            }
            catch (ThreadAbortException)
            {
                IsMoveToNow = false;
                return false;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }

        public bool MyUseGps(Vector3F loc, bool UseMount = true)
        {
            var pointNeatMe = false;
            var pointNearDest = false;
            foreach (var allGpsPoint in GpsBase.GetAllGpsPoints())
            {
                if (Host.Me.Distance(allGpsPoint.X, allGpsPoint.Y, allGpsPoint.Z) < 20)
                    pointNeatMe = true;
                if (loc.Distance(allGpsPoint.X, allGpsPoint.Y, allGpsPoint.Z) < 20)
                    pointNearDest = true;
            }

            if (Host.MyGetAura(269564) != null)
                UseMount = false;
            if (pointNearDest && pointNeatMe)
            {

                var path = GpsBase.GetPath(loc, Host.Me.Location);
                if (path.Count > 1)
                {
                    Host.log("Нашел путь по ГПС " + path.Count);
                    foreach (var vector3F in path)
                    {
                        if (!Host.Me.IsAlive)
                            return false;
                        if (!Host.MainForm.On)
                            return false;
                        if (!ForceMoveTo2(vector3F, 1, UseMount))
                            return false;
                    }
                }
            }

            return true;
        }

        public bool MoveTo(Entity obj, double dist = 1)
        {
            if (IsMovementSuspended)
                return false;
            try
            {
                if (!MyUseGps(obj.Location, false))
                    return false;

                IsMoveToNow = true;
                if (!MoveToBadLoc(obj.Location))
                    return false;


                var result = Host.ComeTo(obj, dist, Host.Me.RunSpeed / 5.0);

                CheckMoveFailed(result);
                /*  if (!result)
                      host.log(host.GetLastError().ToString());*/
                return result;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }

        public bool ForceMoveTo(double x, double y, double z, double dist = 1)
        {

            var result = Host.ComeTo(x, y, z, dist, Host.Me.RunSpeed / 5.0);

            CheckMoveFailed(result);
            return result;
        }

        public bool ForceMoveTo(Vector3F loc, double dist = 1)
        {
            MySitMount(loc);

            Host.log("Начал бег");

            var result = Host.ComeTo(loc, dist, Host.Me.RunSpeed / 5.0);

            Host.log("Закончил бег");
            CheckMoveFailed(result);
            return result;
        }

        public MoveParams MoveParams = new MoveParams();

        public bool ForceMoveTo(Entity obj, double dist = 0.5, double doneDist = 0.5, bool mount = true)
        {
            if (mount)
            {
                MySitMount(obj.Location);
            }

            if (!MyUseGps(obj.Location))
                return false;
            // doneDist = Host.Me.RunSpeed / 5.0;
            //  Host.log("Начал бег в " + obj.Location + "  дист: " + Host.Me.Distance(obj.Location) + "   dist: " + dist + "/" + doneDist);
            if (obj.Location.Distance(-247.85, -5120.16, 42.72) < 3 && Host.Me.Location.Z < 40)
            {
                if (!Host.MoveTo(-253.73, -5113.94, 34.07))
                    return false;
                if (!Host.ForceComeTo(-228.80, -5112.67, 34.07))
                    return false;
                if (!Host.ForceComeTo(-228.80, -5094.90, 41.35))
                    return false;
                if (!Host.ForceComeTo(-243.63, -5094.32, 41.35))
                    return false;
            }

            bool result;
            if (Host.CharacterSettings.Mode == EMode.Script)
            {
                Host.log("Бегу без учета застреваний");
                MoveParams.Obj = obj;
                MoveParams.Dist = dist;
                MoveParams.DoneDist = doneDist;
                MoveParams.IgnoreStuckCheck = true;
                MoveParams.ForceRandomJumps = false;
                MoveParams.UseNavCall = true;

                result = Host.MoveTo(MoveParams);
                Host.log("Добежал");
            }
            else
            {
                result = Host.ComeTo(obj, dist, doneDist);
            }
            CheckMoveFailed(result);
            return result;
        }



        public bool StopCommonModule;

        public List<EItemSubclassWeapon> WeaponType = new List<EItemSubclassWeapon>();
        public List<EItemSubclassArmor> ArmorType = new List<EItemSubclassArmor>();
        public bool WeaponAndShield;


        public override void Run(CancellationToken ct)
        {
            try
            {
                switch (Host.Me.Class)
                {
                    case EClass.None:
                        break;
                    case EClass.Warrior:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.AXE2, EItemSubclassWeapon.SWORD2, EItemSubclassWeapon.MACE2 };
                        }
                        break;
                    case EClass.Paladin:
                        break;
                    case EClass.Hunter:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.BOW, EItemSubclassWeapon.GUN };
                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.CLOTH, EItemSubclassArmor.MAIL, EItemSubclassArmor.MISCELLANEOUS };
                        }
                        break;
                    case EClass.Rogue:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.DAGGER, EItemSubclassWeapon.SWORD };
                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.LEATHER, EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS };
                        }
                        break;
                    case EClass.Priest:
                        break;
                    case EClass.DeathKnight:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.AXE2, EItemSubclassWeapon.SWORD2, EItemSubclassWeapon.POLEARM, EItemSubclassWeapon.MACE2 };
                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.PLATE, EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS };
                        }
                        break;
                    case EClass.Shaman:
                        {
                            //  WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.STAFF, EItemSubclassWeapon.POLEARM, EItemSubclassWeapon.MACE2 };
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.AXE, EItemSubclassWeapon.DAGGER, EItemSubclassWeapon.MACE };
                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.MAIL, EItemSubclassArmor.SHIELD, EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS };
                            WeaponAndShield = true;
                        }
                        break;
                    case EClass.Mage:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.STAFF };
                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.CLOTH, EItemSubclassArmor.MISCELLANEOUS };
                        }
                        break;
                    case EClass.Warlock:
                        break;
                    case EClass.Monk:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.STAFF, EItemSubclassWeapon.POLEARM };

                            if (Host.Me.TalentSpecId == 269)
                            {
                                WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.AXE, EItemSubclassWeapon.SWORD, EItemSubclassWeapon.FIST_WEAPON };
                            }

                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER, EItemSubclassArmor.MISCELLANEOUS };
                        }
                        break;
                    case EClass.Druid:
                        {
                            WeaponType = new List<EItemSubclassWeapon>() { EItemSubclassWeapon.STAFF, EItemSubclassWeapon.POLEARM, EItemSubclassWeapon.MACE2 };
                            ArmorType = new List<EItemSubclassArmor>() { EItemSubclassArmor.CLOTH, EItemSubclassArmor.LEATHER, EItemSubclassArmor.MISCELLANEOUS };
                        }
                        break;
                    case EClass.DemonHunter:
                        break;
                }
                StopCommonModule = false;
                while (!Host.cancelRequested && !ct.IsCancellationRequested)
                {
                    base.Run(ct);
                    Thread.Sleep(5000);
                    // continue;
                    if ((Host.MainForm.On) && (Host.GameState == EGameState.Ingame))
                        ModuleTick();
                }
                StopCommonModule = true;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception error)
            {
                Host.log(error.ToString());
            }
            finally
            {
                Host.log("CommonModule Stop");
            }
        }
    }
}