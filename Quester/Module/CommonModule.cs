using Out.Internal.Core;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Out.Navigation;
using WoWBot.Core;

namespace WowAI.Module
{
    internal partial class CommonModule : Module
    {
        public GpsBase GpsBase = new GpsBase();
        private readonly GpsBase _gpsBaseCustom = new GpsBase();

        public bool AttackPlayer;
        private bool _eventAttackPlayer;
        public bool UseObject = false;
        private bool _isGroupInvite;
        public bool IsChatMessage;
        private DateTime _nextEquip = DateTime.MinValue;
        public bool EventPlayerInZone;

        public override void Run(CancellationToken ct)
        {
            try
            {
                SetEquip();
                while (!Host.CancelRequested && !ct.IsCancellationRequested)
                {
                    base.Run(ct);
                    Thread.Sleep(500);
                    if (Host.MainForm.On && Host.GameState == EGameState.Ingame && Host.CheckCanUseGameActions())
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        ModuleTick();

                        if (sw.ElapsedMilliseconds > 20 && Host.GetBotLogin() == "Daredevi1")
                        {
                            Host.log("CommonModule " + sw.ElapsedMilliseconds, LogLvl.Error);
                        }
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception error) { Host.log(error.ToString()); }
            finally { Host.log("CommonModule Stop"); }
        }

        public void MyDraw()
        {
            var drawList = new List<DrawObject>();
            foreach (var entity in Host.GetEntities<Unit>())
            {
                if (entity.Type == EBotTypes.Player || entity.Type == EBotTypes.Pet)
                {
                    continue;
                }

                if (entity.IsTotem())
                {
                    continue;
                }

                if (!Host.CanAttack(entity, Host.CanSpellAttack))
                {
                    continue;
                }

                if (!Host.IsAlive(entity))
                {
                    continue;
                }

                if (Host.FarmModule.IsBadTarget(entity, Host.FarmModule.TickTime))
                {
                    continue;
                }

                if (Host.FarmModule.IsImmuneTarget(entity))
                {
                    continue;
                }

                if (entity.GetReactionTo(Host.Me) == EReputationRank.Neutral)
                {
                    continue;
                }

                if (entity.HpPercents < 100)
                {
                    continue;
                }

                if (entity.Target != null)
                {
                    if (entity.Target != Host.Me && entity.Target != Host.Me.GetPet())
                    {
                        continue;
                    }
                }

                if (Host.CharacterSettings.UseFilterMobs)
                {
                    var mobsIgnore = false;
                    foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                    {
                        if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                        {
                            if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                            {
                                mobsIgnore = true;
                            }
                        }
                    }

                    if (mobsIgnore)
                    {
                        continue;
                    }
                }

                var radius = Host.GetAggroRadius(entity);
                if (Host.Me.Distance(-249.04, -5110.56, 25.24) < 100)
                {
                    radius = 10;
                }

                drawList.Add(Host.MyDrawRadius(entity.Location, radius));

            }
            Host.DrawCustom(drawList);
        }

        public override void Start(Host host)
        {
            try
            {
                base.Start(host);
                if (!File.Exists(Host.PathGps))
                {
                    Host.log("Не найден файл " + Host.PathGps);
                    Host.StopPluginNow();
                }
                GpsBase.LoadDataBase(Host.PathGps);
                _gpsBaseCustom.LoadDataBase(Host.PathGpsCustom);
                Host.onMoveTick += MyonMoveTick;
                Host.onGroupInvite += MyOnGroupInvite;
                Host.onChatMessage += MyOnChatMessage;
                Host.onDamageLog += MyonDamageLog;
            }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }

        public override void Stop()
        {
            try
            {
                Host.onMoveTick -= MyonMoveTick;
                Host.onCastFailedMessage -= MyonCastFailedMessage;
                Host.onChatMessage -= MyOnChatMessage;
                Host.onGroupInvite -= MyOnGroupInvite;
                Host.onDamageLog -= MyonDamageLog;

                base.Stop();
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        private void MyonMoveTick(int index, List<Vector3F> path)
        {
            var sw = new Stopwatch();
            sw.Start();
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

                if (Host.FarmModule.BestMob != null && Host.FarmModule.BestMob.HpPercents < 100)
                {
                    if (Host.ClientType == EWoWClient.Classic)
                    {
                        if (Host.FarmModule.BestMob.Target != Host.Me && Host.FarmModule.BestMob.Target != Host.Me.GetPet() && Host.FarmModule.BestMob.Target != Host.FarmModule.BestMob)
                        {
                            Host.log("Моба ссагрил другой игрок 1 " + Host.FarmModule.BestMob.Target.Name);
                            Host.FarmModule.SetBadTarget(Host.FarmModule.BestMob, 10000);
                            Host.FarmModule.BestMob = null;
                            Host.CancelMoveTo();
                        }

                    }
                    return;
                }

                if (Host.CharacterSettings.NoAttackOnMount)
                {
                    if (Host.Me.MountId != 0)
                    {
                        return;
                    }
                }

                //Ускорение для некоторых классов
                if (!Host.Me.IsInCombat && Host.IsAlive() && Host.Me.MountId == 0 && !Host.Me.IsDeadGhost)
                {
                    if (Host.Me.Class == EClass.Rogue)
                    {
                        foreach (var spell in Host.SpellManager.GetSpells())
                        {
                            if (spell.Name != "Sprint")
                            {
                                continue;
                            }

                            if (!Host.SpellManager.IsSpellReady(spell))
                            {
                                continue;
                            }

                            var result = Host.SpellManager.CastSpell(spell.Id);
                            if (result != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не смог использовать скилл ускорения" + result, LogLvl.Error);
                            }

                            break;
                        }
                    }

                    if (Host.Me.Class == EClass.DeathKnight)
                    {

                        var skill = Host.SpellManager.GetSpell(48265);
                        if (Host.SpellManager.IsSpellReady(skill))
                        {
                            var result = Host.SpellManager.CastSpell(skill.Id);
                            if (result != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не смог использовать скилл ускорения" + result, LogLvl.Error);
                            }
                        }
                    }
                    if (Host.CharacterSettings.UseDash)
                    {
                        if (Host.FarmModule.BestMob == null)
                        {
                            if (Host.FarmModule.FarmState == FarmState.AttackOnlyAgro || Host.FarmModule.FarmState == FarmState.Disabled)
                            {
                                if (Host.Me.Class == EClass.Hunter)
                                {
                                    var skill = Host.SpellManager.GetSpell(5118);
                                    if (skill != null && Host.SpellManager.IsSpellReady(skill) && Host.MyGetAura(5118) == null)
                                    {
                                        var result = Host.SpellManager.CastSpell(skill.Id);
                                        if (result != ESpellCastError.SUCCESS)
                                        {
                                            Host.log("Не смог использовать скилл ускорения" + result, LogLvl.Error);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                //Обкрадывание
                if (Host.CharacterSettings.Mode == Mode.Script && Host.CharacterSettings.PikPocket)
                {
                    Host.AdvancedInvisible();
                    if (Host.CharacterSettings.PikPocketMapId == Host.MapID)
                    {
                        if (Host.CheckPikPocket())
                        {
                            foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => GetPath(Host.Me.Location, i.Location)))
                            {
                                if (Host.Me.Distance(entity) > Host.CharacterSettings.AttackRadius)
                                {
                                    continue;
                                }

                                if (Host.DictionaryMove.ContainsKey(entity.Guid))
                                {
                                    continue;
                                }

                                if (Host.GetAgroCreatures().Count > 0)
                                {
                                    break;
                                }
                                /*if (Host.Me.Level > 3 && entity.Level == 1)
   continue;*/
                                if (entity.GetCreatureType() != ECreatureType.Humanoid && entity.GetCreatureType() != ECreatureType.Undead)
                                {
                                    continue;
                                }
                                /* if (entity.Id == 1711)
    continue;
if (entity.Id == 7269)
    continue;
if (entity.Id == 8095)
    continue;
if (entity.Id == 7246)
    continue;*/
                                if (entity.IsMoving)
                                {
                                    continue;
                                }

                                var zRange = Math.Abs(Host.Me.Location.Z - entity.Location.Z);

                                if (zRange > 10)
                                {
                                    continue;
                                }

                                if (!Host.CanAttack(entity, Host.CanSpellAttack))
                                {
                                    continue;
                                }

                                if (!Host.IsAlive(entity))
                                {
                                    continue;
                                }

                                if (Host.ListGuidPic.ContainsKey(entity.Guid))
                                {
                                    if (DateTime.Now < Host.ListGuidPic[entity.Guid])
                                    {
                                        continue;
                                    }
                                }
                                /* if (Host.FarmModule.IsBadTarget(entity, Host.ComboRoute.TickTime))
                                     continue;
                                 if (Host.FarmModule.IsImmuneTarget(entity))
                                     continue;*/


                                if (Host.CharacterSettings.UseFilterMobs)
                                {
                                    var mobsIgnore = false;
                                    foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                    {
                                        if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                        {
                                            if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                                            {
                                                mobsIgnore = true;
                                            }
                                        }
                                    }

                                    if (mobsIgnore)
                                    {
                                        continue;
                                    }

                                    /*  foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                      {
                                          if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                              if (characterSettingsMobsSetting.Priority == 2)
                                                  return entity;
                                      }
                                      if (Host.FarmModule.farmState == FarmState.Disabled)
                                          return null;*/
                                }

                                if (Host.FarmModule.BestMob != null)
                                {
                                    if (Host.FarmModule.BestMob.IsMoving)
                                    {
                                        Host.FarmModule.BestMob = null;
                                        Host.CancelMoveTo();
                                        Host.CommonModule.SuspendMove();
                                        return;
                                    }
                                    if (Host.Me.Distance(Host.FarmModule.BestMob) < Host.Me.Distance(entity))
                                    {
                                        return;
                                    }
                                }

                                if (entity != Host.FarmModule.BestMob)
                                {
                                    Host.FarmModule.BestMob = entity;
                                    Host.CancelMoveTo();
                                    Host.CommonModule.SuspendMove();
                                    if (Host.CharacterSettings.LogScriptAction)
                                    {
                                        Host.log("Перехват22: " + Host.FarmModule.BestMob.Name + "  " + Host.FarmModule.BestMob.Id + " дист:" + Host.Me.Distance(entity) +
                                            " всего:" + Host.GetAgroCreatures().Count + "  IsAlive:" + Host.IsAlive(entity) +
                                            " HP:" + entity.Hp + " " + Host.FarmModule.BestMob.Guid, LogLvl.Error);
                                    }
                                }
                                return;
                            }
                        }

                    }
                }



                if (Host.FarmModule.FarmState == FarmState.Disabled || Host.MapID == 1904)
                {
                    return;
                }






                //Перехват по пути
                if (Host.ClientType == EWoWClient.Classic)
                {
                    if (Host.ClientType == EWoWClient.Classic && Host.FarmModule.BestMob != null)
                    {
                        if (Host.FarmModule.IsBadTarget(Host.FarmModule.BestMob))
                        {
                            return;
                        }
                    }

                    if (Host.CharacterSettings.KillMobFirst || Host.CharacterSettings.Mode == Mode.QuestingClassic)
                    {
                        if (index > 0)
                        {
                            index -= 1;
                        }

                        for (var a = index; a < path.Count - 1; a++)
                        {
                            var fromVector3F = path[a];
                            var toVector3F = path[a + 1];
                            if (Host.Me.Distance(fromVector3F) > 100)
                            {
                                continue;
                            }

                            foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => fromVector3F.Distance(i.Location)))
                            {
                                if (entity.Type == EBotTypes.Player || entity.Type == EBotTypes.Pet)
                                {
                                    continue;
                                }

                                if (entity.IsTotem())
                                {
                                    continue;
                                }

                                if (!Host.CanAttack(entity, Host.CanSpellAttack))
                                {
                                    continue;
                                }

                                if (!Host.IsAlive(entity))
                                {
                                    continue;
                                }

                                if (Host.FarmModule.IsBadTarget(entity, Host.FarmModule.TickTime))
                                {
                                    continue;
                                }

                                if (Host.FarmModule.IsImmuneTarget(entity))
                                {
                                    continue;
                                }

                                if (entity.GetReactionTo(Host.Me) == EReputationRank.Neutral)
                                {
                                    continue;
                                }

                                if (entity.HpPercents < 100)
                                {
                                    continue;
                                }

                                if (entity.Target != null)
                                {
                                    if (entity.Target != Host.Me && entity.Target != Host.Me.GetPet())
                                    {
                                        continue;
                                    }
                                }



                                var zRange = Math.Abs(fromVector3F.Z - entity.Location.Z);
                                if (zRange > 5)
                                {
                                    continue;
                                }

                                if (Host.ClientType == EWoWClient.Classic)
                                {
                                    Vector3F hitPos = Vector3F.Zero;
                                    Triangle tri = new Triangle();
                                    if (Host.Raycast(fromVector3F, entity.Location, ref hitPos, ref tri))
                                    {
                                        continue;
                                    }
                                }

                                if (Host.CharacterSettings.UseFilterMobs)
                                {
                                    var mobsIgnore = false;
                                    foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                    {
                                        if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                        {
                                            if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                                            {
                                                mobsIgnore = true;
                                            }
                                        }
                                    }

                                    if (mobsIgnore)
                                    {
                                        continue;
                                    }
                                }

                                var radius = Host.GetAggroRadius(entity);
                                if (Host.Me.Distance(-249.04, -5110.56, 25.24) < 100)
                                {
                                    radius = 10;
                                }

                                if (!CircleIntersects(entity.Location.X, entity.Location.Y, radius, fromVector3F.X, fromVector3F.Y, toVector3F.X, toVector3F.Y))
                                {
                                    continue;
                                }

                                if (Host.FarmModule.BestMob != null)
                                {
                                    if (Host.Me.Distance(Host.FarmModule.BestMob) < Host.Me.Distance(entity))
                                    {
                                        break;
                                    }
                                }

                                if (entity != Host.FarmModule.BestMob)
                                {
                                    Host.FarmModule.BestMob = entity;
                                    Host.CommonModule.SuspendMove();
                                    Host.log("Атакую моба на пути: " + Host.FarmModule.BestMob.Name + "[" + Host.FarmModule.BestMob.Id + "] дист:" + Host.Me.Distance(entity) +
                                        " всего:" + Host.GetAgroCreatures().Count + "  GetAggroRadius:" +
                                        Host.GetAggroRadius(entity) + "  zRange " + zRange + "  " + Host.FarmModule.BestMob.GetReactionTo(Host.Me), LogLvl.Error);
                                }
                                return;
                            }
                        }
                    }

                }

                if (Host.CharacterSettings.Mode == Mode.Questing)
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
                }


                foreach (var agromob in Host.GetAgroCreatures())
                {
                    if (Host.GetAgroCreatures().Contains(Host.FarmModule.BestMob) && Host.IsAlive(Host.FarmModule.BestMob))
                    {
                        return;
                    }

                    var zRange = Math.Abs(Host.Me.Location.Z - agromob.Location.Z);

                    if (zRange > 10)
                    {
                        continue;
                    }

                    if (!Host.IsAlive(agromob))
                    {
                        continue;
                    }

                    if (Host.Me.Distance(agromob) > 15)
                    {
                        continue;
                    }

                    /* if (Host.GetBotLogin() == "zaww")
                     {
                         if (Host.FarmModule.IsBadTarget(agromob, Host.FarmModule.TickTime))
                             continue;
                         if (Host.FarmModule.IsImmuneTarget(agromob))
                             continue;
                     }*/


                    Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();

                    Host.CancelMoveTo();
                    Host.CommonModule.SuspendMove();
                    Host.log("Агр: " + Host.FarmModule.BestMob.Name + " дист:" + Host.Me.Distance(agromob) + " всего:" + Host.GetAgroCreatures().Count, LogLvl.Error);
                    return;
                }

                if (Host.CharacterSettings.Attack && Host.CharacterSettings.Mode == Mode.Script)
                {
                    if (Host.FarmModule.MobsWithDropCount() + Host.FarmModule.MobsWithSkinCount() == 0)
                    {
                        foreach (var entity in Host.GetEntities<Unit>().OrderBy(i => Host.Me.Distance(i)))
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
                                        if (Host.CharacterSettings.Mode == Mode.Questing)
                                        {
                                            needbreak = true;
                                        }
                                    }
                                    break;
                            }

                            if (needbreak)
                            {
                                break;
                            }

                            if (Host.ClientType == EWoWClient.Classic)
                            {
                                if (entity.Target != null)
                                {
                                    if (entity.Target != Host.Me && entity.Target != Host.Me.GetPet())
                                    {
                                        Host.log("Моба ссагрил другой игрок 2 " + entity.Target.Name);
                                        continue;
                                    }
                                }
                            }
                            /*  if (Host.Me.Target != null && Host.Me.Target.HpPercents < 100)
                                  break;*/
                            if (Host.GetAgroCreatures().Count > 0)
                            {
                                break;
                            }

                            if (Host.Me.Level > 3 && entity.Level == 1)
                            {
                                continue;
                            }

                            if (Host.Me.Distance(entity) > Host.CharacterSettings.AttackRadius)
                            {
                                continue;
                            }

                            if (entity.Type == EBotTypes.Player || entity.Type == EBotTypes.Pet)
                            {
                                continue;
                            }

                            var zRange = Math.Abs(Host.Me.Location.Z - entity.Location.Z);

                            if (zRange > 10)
                            {
                                continue;
                            }

                            if (!Host.CanAttack(entity, Host.CanSpellAttack))
                            {
                                continue;
                            }

                            if (!Host.IsAlive(entity))
                            {
                                continue;
                            }

                            if (Host.FarmModule.IsBadTarget(entity, Host.FarmModule.TickTime))
                            {
                                continue;
                            }

                            if (Host.FarmModule.IsImmuneTarget(entity))
                            {
                                continue;
                            }

                            if (Host.CharacterSettings.UseFilterMobs)
                            {
                                var mobsIgnore = false;
                                foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                {
                                    if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                    {
                                        if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                                        {
                                            mobsIgnore = true;
                                        }
                                    }
                                }

                                if (mobsIgnore)
                                {
                                    continue;
                                }

                                /*  foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                                  {
                                      if (entity.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                          if (characterSettingsMobsSetting.Priority == 2)
                                              return entity;
                                  }
                                  if (Host.FarmModule.farmState == FarmState.Disabled)
                                      return null;*/
                            }

                            if (Host.FarmModule.BestMob != null)
                            {
                                if (Host.Me.Distance(Host.FarmModule.BestMob) < Host.Me.Distance(entity))
                                {
                                    return;
                                }
                            }

                            if (entity != Host.FarmModule.BestMob)
                            {
                                Host.FarmModule.BestMob = entity;

                                Host.CancelMoveTo();
                                Host.CommonModule.SuspendMove();
                                if (Host.CharacterSettings.FindBestPoint)
                                {
                                    Host.AutoQuests.NeedFindBestPoint = true;
                                }
                                if (Host.CharacterSettings.LogScriptAction)
                                {
                                    Host.log(
                                        "Attack: " + Host.FarmModule.BestMob.Name + " дист:" + Host.Me.Distance(entity) +
                                        " всего:" + Host.GetAgroCreatures().Count + "  IsAlive:" + Host.IsAlive(entity) +
                                        " HP:" + entity.Hp + " " + Host.FarmModule.BestMob.Guid, LogLvl.Error);
                                }
                            }

                            return;
                        }
                    }
                }


            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
            finally
            {
                if (sw.ElapsedMilliseconds > 20 && Host.GetBotLogin() == "Daredevi1")
                {
                    Host.log("MoveTick: " + sw.ElapsedMilliseconds, LogLvl.Error);
                }
            }

        }

        private void MyonDamageLog(SpellDamageLog log)
        {
            var targetEntity = Host.GetEntity(log.TargetGuid);

            switch (log.EnvironmentType)
            {
                case EEnvironmentDamage.Fire:
                    {
                        if (targetEntity == Host.Me)
                        {
                            Host.CommonModule.ForceMoveTo(Host.Me.Location.X + 4, Host.Me.Location.Y, Host.Me.Location.Z, 0);
                        }
                    }
                    break;
                case EEnvironmentDamage.Fall:
                    break;
                default:
                    {
                        // Host.log(targetEntity?.Name + " " + log.EnvironmentType);
                    }
                    break;
            }

            try
            {
                var target = Host.GetEntity(log.TargetGuid);
                var caster = Host.GetEntity(log.CasterGuid);


                if (log.CasterGuid == Host.Me.Guid)
                {
                    Host.AllDamage += log.Damage;
                }

                if (target?.Guid == Host.Me.Guid && log.CasterGuid != Host.Me.Guid)
                {
                    if (caster?.Type == EBotTypes.Player && log.Damage > 0)
                    {
                        _eventAttackPlayer = true;
                        AttackPlayer = true;
                        if (Host.AdvancedLog)
                        {
                            if (Host.GameDB.SpellInfoEntries.ContainsKey(log.SpellID))
                            {
                                Host.log(caster.Name + "->" + target.Name + "->" + " [" + log.Type + "]" + Host.GameDB.SpellInfoEntries[log.SpellID].SpellName + "[" + log.SpellID + "]   [" + log.Damage + "]", LogLvl.Error);
                            }
                            else
                            {
                                Host.log(caster.Name + "->" + target.Name + "->" + " [" + log.Type + "]" + "Неизвестный спел[" + log.SpellID + "]   [" + log.Damage + "]", LogLvl.Error);
                            }
                        }

                        if (Host.CharacterSettings.Pvp)
                        {
                            if (Host.FarmModule.BestMob != caster)
                            {
                                Host.log("Атакует игрок");
                                if (Host.FarmModule.BestMob == null)
                                {
                                    Host.FarmModule.BestMob = caster as Unit;
                                }
                                else
                                {
                                    if (Host.FarmModule.BestMob.HpPercents > 30)
                                        Host.FarmModule.BestMob = caster as Unit;
                                }


                            }

                        }
                    }
                }

                if (!Host.AdvancedLog)
                {
                    return;
                }

                if (Host.GameDB.SpellInfoEntries.ContainsKey(log.SpellID))
                {
                    Host.Log(caster?.Name + "->" + target?.Name + "->" + " [" + log.Type + "]" + Host.GameDB.SpellInfoEntries[log.SpellID].SpellName + "[" + log.SpellID + "]   [" + log.Damage + "]", "Damage");
                }
                else
                {
                    Host.Log(caster?.Name + "->" + target?.Name + "->" + " [" + log.Type + "]" + "Неизвестный спел[" + log.SpellID + "]   [" + log.Damage + "]", "Damage");
                }
                // Host.log(log.Crit + " " + log.CritRollMade + " " + log.CritRollNeeded);
            }
            catch (Exception e)
            {
                Host.log(e + " ");
            }




        }

        private void MyOnChatMessage(string senderName, EChatMessageType type, string text, string receiver)
        {
            if (type == EChatMessageType.Whisper || type == EChatMessageType.Whisper2 || type == EChatMessageType.WhisperInform || type == EChatMessageType.Say)
            {
                if (Host.Area.Id == 1637 || Host.Area.Id == 1497)
                    return;
                IsChatMessage = true;
                Host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Host.Me.Name + "(" + Host.Me.Level + ")" + senderName + " [" + type + "]:" + text, "События");
            }
        }

        public void MyOnGroupInvite(string inviterName, WowGuid inviterGuid)
        {
            Host.log("Инвайт в группу от " + inviterName);
            _isGroupInvite = true;
            Thread.Sleep(Host.RandGenerator.Next(2, 5) * 1000);
            Host.log("Отказываюсь от пати ", LogLvl.Ok);
            Host.Group.AcceptInvite(false);
        }



        private void MyonCastFailedMessage(uint spellId, ESpellCastError reason, int failedArg1, int failedArg2)
        {
            try
            {

                if (Host.AdvancedLog)
                {
                    Host.log(spellId + " " + reason + " " + failedArg1 + " " + failedArg2);
                }

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
                {
                    return;
                }

                foreach (var s in Host.Me.GetAuras())
                {
                    if (s.SpellInfo.Effects != null)
                    {
                        foreach (var spellInfoEffect in s.SpellInfo.Effects)
                        {
                            foreach (var spellEffectInfo in spellInfoEffect.Value)
                            {
                                if (spellEffectInfo.ApplyAuraName == EAuraType.MOUNTED)
                                {
                                    if (!s.Cancel())
                                    {
                                        Host.log("Не удалось отозвать маунта " + Host.GetLastError(), LogLvl.Error);
                                    }
                                }
                            }
                        }
                    }

                    if (s.IsPartOfSkillLine(777))
                    {
                        if (!s.Cancel())
                        {
                            Host.log("Не удалось отозвать маунта " + Host.GetLastError(), LogLvl.Error);
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

        public void MySitMount(Vector3F loc)
        {
            try
            {
                if (!Host.IsOutdoors)
                {
                    return;
                }

                if (Host.MyGetAura(269564) != null)
                {
                    return;
                }

                if (Host.MyGetAura(267254) != null)
                {
                    return;
                }

                if (Host.MyGetAura(263851) != null)
                {
                    return;
                }

                if (Host.Me.Distance(loc) < 2)
                {
                    return;
                }

                if (Host.FarmModule.BestMob != null)
                {
                    return;
                }

                if (Host.ClientType == EWoWClient.Classic)
                {
                    if (Host.Me.Distance(loc) < 50)
                    {
                        return;
                    }
                }
                if (Host.CharacterSettings.Mode == Mode.Questing)
                {
                    if (Host.AutoQuests.BestQuestId == 47880)
                    {
                        return;
                    }

                    if (Host.Me.Distance(loc) < 50)
                    {
                        return;
                    }
                }
                //   Host.log("Маунт " + Host.CharacterSettings.CheckBoxAttackForSitMount + " " + Host.GetThreats(Host.Me).Count + " " + Host.Me.MountId);
                if (Host.CharacterSettings.Mode == Mode.Script)
                {
                    if (Host.CharacterSettings.CheckBoxAttackForSitMount && Host.Me.GetThreats().Count > 0 && Host.Me.MountId == 0)
                    {
                        //  Host.log("Атака");
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        return;
                    }

                    if (Host.FarmModule.FarmState == FarmState.AttackOnlyAgro)
                    {
                        foreach (var entity in Host.GetEntities<Unit>())
                        {
                            if (Host.Me.Level > 10 && entity.Level == 1)
                            {
                                continue;
                            }
                            if (Host.Me.Distance(entity) > 50)
                            {
                                continue;
                            }
                            if (!entity.IsAlive)
                            {
                                continue;
                            }
                            if (!Host.CanAttack(entity, Host.CanSpellAttack))
                            {
                                continue;
                            }

                            return;
                        }
                    }
                }


                if (Host.MapID == 1643 && Host.AutoQuests.BestQuestId == 47098)
                {
                    return;
                }

                if (Host.MapID == 1904 || Host.MapID == 1929)
                {
                    return;
                }
                if (!Host.IsAlive(Host.Me) || MoveFailCount > 2 || Host.FarmModule.FarmState == FarmState.FarmMobs || Host.Me.IsDeadGhost)
                {
                    return;
                }

                if (Host.Me.Class == EClass.Druid)
                {
                    if (Host.CharacterSettings.FormForMove != "Не использовать")
                    {
                        MyUnmount();
                        var formId = 0;
                        if (Host.CharacterSettings.FormForMove == "Облик медведя")
                        {
                            formId = 5487;
                        }

                        if (Host.CharacterSettings.FormForMove == "Облик кошки")
                        {
                            formId = 768;
                        }

                        if (Host.CharacterSettings.FormForMove == "Походный облик")
                        {
                            formId = 783;
                        }

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
                                        if (Host.MyGetAura(i) != null)
                                        {
                                            isNeedDash = false;
                                        }
                                    }
                                    if (isNeedDash)
                                    {
                                        foreach (var i in listDash)
                                        {
                                            if (Host.SpellManager.GetSpellCooldown(i) != 0)
                                            {
                                                continue;
                                            }

                                            if (!Host.SpellManager.IsSpellReady(i))
                                            {
                                                continue;
                                            }

                                            if (Host.SpellManager.CheckCanCast(i, Host.Me) != ESpellCastError.SUCCESS)
                                            {
                                                continue;
                                            }

                                            var resultForm = Host.SpellManager.CastSpell(i);
                                            if (resultForm != ESpellCastError.SUCCESS)
                                            {
                                                Host.log("Не удалось использовать ускорение " + i + "  " + Host.SpellManager.CheckCanCast(i, Host.Me) + "  " + resultForm, LogLvl.Error);
                                            }
                                            else
                                            {
                                                if (Host.CharacterSettings.LogSkill)
                                                {
                                                    Host.log("Использовал ускорение " + Host.SpellManager.GetSpellCooldown(i) + "    " + Host.SpellManager.CheckCanCast(i, Host.Me) + "  " + i, LogLvl.Ok);
                                                }
                                            }


                                            while (Host.SpellManager.IsCasting)
                                            {
                                                Thread.Sleep(100);
                                            }

                                            break;
                                        }
                                    }
                                }
                                if (Host.ClientType == EWoWClient.Retail)
                                {
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
                                    {
                                        return;
                                    }

                                    Thread.Sleep(100);
                                    i++;
                                    if (i > 10)
                                    {
                                        break;
                                    }
                                }
                                var resultForm = Host.SpellManager.CastSpell(spell.Id, Host.Me);
                                if (resultForm != ESpellCastError.SUCCESS)
                                {
                                    if (Host.AdvancedLog)
                                    {
                                        Host.log("Не удалось поменять форму 1 " + spell.Name + "  " + resultForm, LogLvl.Error);
                                    }
                                }


                                while (Host.SpellManager.IsCasting)
                                {
                                    Thread.Sleep(100);
                                }

                                return;
                            }
                        }
                        return;
                    }
                }

                if (!Host.IsAlive(Host.Me) || MoveFailCount > 2 || InFight() || Host.FarmModule.FarmState == FarmState.FarmMobs || Host.Me.MountId != 0 || Host.Me.IsDeadGhost)
                {
                    return;
                }

                Spell mountSpell = null;

                foreach (var s in Host.SpellManager.GetSpells())
                {
                    /* if (!s.SkillLines.Contains(777))
                         continue;*/
                    var isNeedMount = false;
                    foreach (var i in Host.CharacterSettings.PetSettings)
                    {
                        if (i.Type != "Mount")
                        {
                            continue;
                        }

                        if (i.Id != s.Id)
                        {
                            continue;
                        }

                        if (i.MountType != EMountType.Spell)
                        {
                            continue;
                        }

                        isNeedMount = true;
                    }
                    if (!isNeedMount)
                    {
                        continue;
                    }

                    mountSpell = s;
                    break;
                }
                if (mountSpell != null)
                {
                    if (Host.Me.Class == EClass.Druid)
                    {
                        Host.log("Снимаю облик для использования маунта ", LogLvl.Important);
                        Host.CanselForm();
                    }
                    SuspendMove();
                    Host.MyCheckIsMovingIsCasting();
                    Thread.Sleep(500);
                    for (var i = 0; i < 3; i++)
                    {
                        var result = Host.SpellManager.CastSpell(mountSpell.Id);
                        if (result != ESpellCastError.SUCCESS)
                        {
                            Host.log("Не удалось призвать маунта " + mountSpell.Name + "  " + result, LogLvl.Error);
                            SuspendMove();
                            Host.MyCheckIsMovingIsCasting();
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Host.log("Призвал маунта", LogLvl.Ok);
                            break;
                        }
                    }


                    while (Host.SpellManager.IsCasting)
                    {
                        Thread.Sleep(100);
                    }

                    return;
                }

                Item mountItem = null;
                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (!item.IsSoulBound)
                    {
                        continue;
                    }

                    var isNeedMount = false;
                    foreach (var i in Host.CharacterSettings.PetSettings)
                    {
                        if (i.Type != "Mount")
                        {
                            continue;
                        }

                        if (i.Id != item.Id)
                        {
                            continue;
                        }

                        if (i.MountType != EMountType.Item)
                        {
                            continue;
                        }

                        isNeedMount = true;
                    }
                    if (!isNeedMount)
                    {
                        continue;
                    }

                    mountItem = item;
                    break;
                }

                if (mountItem != null)
                {
                    Host.MyUseItemAndWait(mountItem);
                }




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
                {
                    return false;
                }

                foreach (var i in Host.GetAgroCreatures())
                {
                    if (i == null)
                    {
                        continue;
                    }

                    if (!Host.IsAlive(i))
                    {
                        continue;
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
            return false;
        }

        public double GetPath(Vector3F from, Vector3F to)
        {
            var pathToMob = Host.GetSmoothPath2(from, to, new Vector3F(0.01, 2, 0.01), 0.5f, 0.1f, TryGetMidPoints);
            if (pathToMob == null || pathToMob.Path == null || pathToMob.InternalResult <= 0)
            {
                return double.MaxValue;
            }
            double pathLen = 0;
            for (int i = 1; i < pathToMob.Path.Count; i++)
            {
                var d = pathToMob.Path[i - 1].Distance(pathToMob.Path[i]);
                if (d > 5)
                {
                    return double.MaxValue;
                }

                pathLen += d;
            }
            return pathLen;



            /*  double result = 0;
              var path = Host.GetSmoothPath(from, to);
              if (path.Path.Count < 2)
                  return 9999;
              for (var index = 0; index < path.Path.Count - 1; index++)
              {
                  var vector3F = path.Path[index];
                  var vector3F2 = path.Path[index + 1];
                  result += vector3F.Distance(vector3F2);
              }
              return result;*/
        }

        internal bool CircleIntersects(double x, double y, double r, double ax, double ay, double bx, double @by)
        {
            try
            {
                var l = Math.Sqrt(Math.Pow((bx - ax), 2) + Math.Pow((@by - ay), 2));
                // единичный вектор отрезка AB 
                var xv = (bx - ax) / l;
                var yv = (@by - ay) / l;
                var xd = (ax - x);
                var yd = (ay - y);
                var b = 2 * (xd * xv + yd * yv);
                var c = xd * xd + yd * yd - r * r;
                var c4 = c + c;
                c4 += c4;
                var d = b * b - c4;
                if (d < 0)
                {
                    return false; // нет корней, нет пересечений
                }

                d = Math.Sqrt(d);
                var l1 = (-b + d) * 0.5;
                var l2 = (-b - d) * 0.5;
                var intersects1 = ((l1 >= 0.0) && (l1 <= l));
                var intersects2 = ((l2 >= 0.0) && (l2 <= l));
                var intersects = intersects1 || intersects2;
                return intersects;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }

        public void MyUseItemFromSettings()
        {
            try
            {
                


                foreach (var item in Host.ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                        item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                        item.Place != EItemPlace.InventoryItem)
                    {
                        continue;
                    }

                    foreach (var characterSettingsItemSetting in Host.CharacterSettings.ItemSettings)
                    {
                        if (item.Id == characterSettingsItemSetting.Id && characterSettingsItemSetting.Use == EItemUse.Del && Host.MeGetItemsCount(item.Id) > characterSettingsItemSetting.MinCount)
                        {
                            Host.log("Удаляю предмет " + item.Name + " " + Host.MeGetItemsCount(item.Id) + "/" + characterSettingsItemSetting.MinCount);
                            if (!item.Destroy())
                            {
                                Host.log("Не смог удалить " + Host.GetLastError(), LogLvl.Error);
                            }
                            return;
                        }

                        if (item.Id == characterSettingsItemSetting.Id && characterSettingsItemSetting.Use == EItemUse.Use)
                        {
                            Host.CommonModule.SuspendMove();
                            Host.CommonModule.MyUnmount();
                            Host.log("Использую " + item.Name);
                            var result = Host.SpellManager.UseItem(item);
                            if (result != EInventoryResult.OK)
                            {
                                Host.log("Не смог использовать итем " + item.Name + "  " + result,
                                    LogLvl.Error);
                            }

                            while (Host.SpellManager.IsCasting)
                            {
                                Thread.Sleep(100);
                            }

                            Host.CommonModule.ResumeMove();
                            if (Host.CanPickupLoot())
                            {
                                if (!Host.PickupLoot())
                                {
                                    Host.log("Не смог поднять дроп " + "   " + Host.GetLastError(), LogLvl.Error);
                                }
                            }
                            return;
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

        private void ActionEvent(EventSettings events)
        {

            try
            {
                switch (events.ActionEvent)
                {
                    case EventsAction.Log:
                        Host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Host.Me.Name + "(" + Host.Me.Level + ")" + " Внимание сработало событие " + events.TypeEvents, "События");
                        break;
                    case EventsAction.ShowGameClient:
                        // Host.ShowGameClient();
                        break;
                    case EventsAction.ShowQuester:
                        Host.MainForm.Dispatcher?.BeginInvoke(new Action(() =>
                        {
                            Host.MainForm.Main1.WindowState = WindowState.Normal;
                        }));
                        break;
                    case EventsAction.Pause:
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            Host.MainForm.On = false;
                            while (sw.ElapsedMilliseconds < events.Pause)
                            {
                                Thread.Sleep(10000);
                                Host.log("Пауза " + events.TypeEvents + " " + sw.ElapsedMilliseconds + "/" + events.Pause);
                            }
                            Host.MainForm.On = true;
                        }

                        break;
                    case EventsAction.ExitGame:
                        {
                            Host.GetCurrentAccount().IsAutoLaunch = false;
                            Host.TerminateGameClient();
                            if (events.Pause != 0)
                            {
                                Thread.Sleep(events.Pause);
                                Host.GetCurrentAccount().IsAutoLaunch = true;
                            }
                        }
                        break;
                    case EventsAction.PlaySound when File.Exists(events.SoundFile):
                        {
                            using (var sp = new SoundPlayer(events.SoundFile))
                            {
                                sp.Play();
                            }
                            break;
                        }
                    case EventsAction.PlaySound:
                        Host.log("Файл не найден " + events.SoundFile);
                        break;
                    case EventsAction.NotSellected:
                        break;
                }
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
        }

        private class MyObstacle
        {
            public Vector3F Loc;
            public int Hight;
            public int Radius;
        }

        private readonly List<MyObstacle> _myObstacles = new List<MyObstacle>
        {
            new MyObstacle{Loc = new Vector3F(-1383.33, -5153.40, 7.56), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(2063.42, 2956.96, 37.55), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(519.97, -163.17, -194.33), Radius = 12, Hight = 12},
            new MyObstacle{Loc = new Vector3F(4039.70, 372.60, 69.14), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-661.77, 89.38, 274.51), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-896.24, -193.22, 222.11), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(1770.84, -4512.96, 27.43), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-213.00, 396.73, 201.68), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(2452.94, 827.52, 10.89), Radius = 1, Hight = 1},
            new MyObstacle{Loc = new Vector3F(2240.47, 4345.53, 37.74), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(2220.45, 4345.64, 38.29), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(2896.73, 2534.25, 61.64), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(1475.53, 1596.20, 45.40), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(2047.28, 1666.26, 22.94), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(1012.61, 1382.93, 22.30), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(-1431.77, 1261.11, 193.38), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(2658.31, 3391.74, 133.48), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(993.22, 3239.45, 92.20), Radius = 2, Hight = 2},
            new MyObstacle{Loc = new Vector3F(-915.82, 1235.41, 320.08), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-910.73, 1230.04, 320.40), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-912.35, 1222.49, 319.68), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-924.24, 1240.08, 319.71), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(-920.18, 1238.42, 319.87), Radius = 4, Hight = 4},
            new MyObstacle{Loc = new Vector3F(2751.12, 3330.07, 64.17), Radius = 8, Hight = 8},
            new MyObstacle{Loc = new Vector3F(1941.12, 1838.47, 21.27), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1942.03, 1837.25, 22.13), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(562.28, -262.14, -193.66), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(552.66, -255.10, -193.98), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(565.61, -262.10, -194.02), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(572.10, -263.93, -193.22), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(584.38, -265.50, -194.47), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(602.19, -255.42, -194.26), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(556.18, -255.87, -194.62), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(561.91, -257.68, -195.36), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1698.78, 85.40, -62.29), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1695.71, 78.76, -62.29), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1696.49, 82.19, -62.16), Radius = 3, Hight = 3},

            new MyObstacle{Loc = new Vector3F(1725.81, 95.17, -61.95), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1729.45, 98.78, -61.95), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1731.94, 101.25, -61.94), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1735.93, 105.13, -61.94), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1738.70, 108.33, -61.87), Radius = 3, Hight = 3},

            new MyObstacle{Loc = new Vector3F(1757.61, 129.05, -62.29), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1759.42, 133.02, -62.29), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1764.51, 141.31, -62.30), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1765.49, 148.31, -62.30), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1768.04, 153.55, -62.30), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1764.04, 145.33, -62.30), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1765.78, 136.52, -62.30), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(1642.75, 66.45, -61.62), Radius = 5, Hight = 5},

            new MyObstacle{Loc = new Vector3F(1646.98, 72.97, -62.18), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(1648.42, 66.85, -62.18), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(2105.98, -4594.81, 51.35), Radius = 3, Hight = 3},
            new MyObstacle{Loc = new Vector3F(-73.06, -4321.12, 65.09), Radius = 5, Hight = 5},

            //Клаус
            new MyObstacle{Loc = new Vector3F(-7386.92, -2012.50464, -270.947723), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7383.396, -2010.20667, -270.032867), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7379.32666, -2008.588, -268.578461), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7375.28467, -2009.18469, -266.4157), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7371.25146, -2009.78015, -265.433319), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7367.17725, -2009.92944, -264.943329), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7363.4126, -2010.06726, -266.006317), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7359.558, -2011.53271, -267.2973), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7356.819, -2014.29175, -267.308563), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7356.69238, -2018.46252, -267.49408), Radius = 5, Hight = 5},
            new MyObstacle{Loc = new Vector3F(-7357.839, -2022.04175, -267.883942), Radius = 5, Hight = 5},
            //----

        };

        private readonly List<uint> _noObstacle = new List<uint>
        {
            271556,
            271170,
            271557,
            231074,
            282637,
        };


        private void LearnTalant()
        {
            if (Host.TalentTree.AvailablePoints <= 0)
            {
                return;
            }

            foreach (var characterSettingsLearnTalent in Host.CharacterSettings.LearnTalents)
            {
                foreach (var talentSpell in Host.TalentTree.GetAllTalents())
                {
                    if (talentSpell.ID != characterSettingsLearnTalent.Id)
                    {
                        continue;
                    }

                    if (talentSpell.GetCurrentRank() >= characterSettingsLearnTalent.Level)
                    {
                        continue;
                    }

                    if (!talentSpell.CanLearn())
                    {
                        Host.log("Нельзя выучить талант " + talentSpell.Name, LogLvl.Important);
                        continue;
                    }

                    if (talentSpell.GetCurrentRank() == talentSpell.GetMaxRank())
                    {
                        Host.log("Максимальный ранг " + talentSpell.Name, LogLvl.Important);
                        continue;
                    }

                    if (!talentSpell.Learn())
                    {
                        Host.log("Не удалось выучить талант " + talentSpell.Name + " " + Host.GetLastError(), LogLvl.Error);
                    }
                    Host.log("Выучил талант " + talentSpell.Name);
                    return;
                }
            }
        }

        private void CheckObstacle()
        {
            try
            {
                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (Host.GetVar(entity, "obstacle") != null)
                    {
                        continue;
                    }

                    if (entity.Id == 131789 || entity.Id == 131753 || entity.Id == 135875 || entity.Id == 142484 || entity.Id == 142485 || entity.Id == 142478
                        || entity.Id == 142360 || entity.Id == 135119 || entity.Id == 143875 || entity.Id == 141780 || entity.Id == 135388 || entity.Id == 143482)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(entity.Location.X, entity.Location.Y, entity.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 1 " + entity.CollisionHeight + " " + entity.CollisionScale + " " + entity.ObjectSize + " " + entity.ObjectSize2 + " " + entity.Scale);
                            Host.BuildQuad(entity.Location.X, entity.Location.Y, 0, 0, 0, entity);
                            //  Host.AddObstacle(new Vector3F(entity.Location.X, entity.Location.Y, entity.Location.Z), 30, 30);
                            Host.SetVar(entity, "obstacle", true);
                        }
                    }
                }

                foreach (var gameObject in Host.GetEntities<GameObject>())
                {
                    if (Host.GetVar(gameObject, "obstacle") != null)
                    {
                        continue;
                    }

                    if (_noObstacle.Contains(gameObject.Id))
                    {
                        continue;
                    }

                    if (gameObject.Id == 173017)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z - 3), 4, 5);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173206)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 3, 3);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173203)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 1814)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 1, 1);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 1815 || gameObject.Id == 1816 || gameObject.Id == 1811)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 1, 1);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 178573)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 1, 1);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 177026)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 4, 4);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 177014)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 4, 4);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 18089)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 4, 4);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 141852)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 3, 3);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173217)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 5, 3);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173199)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 5, 3);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 31579)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173063)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 6, 6);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173147)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2.5, 2.5);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 31411)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2.5, 2.5);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 173197)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Id == 31576)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.GameObjectType == EGameObjectType.Mailbox)//Почтовый ящик
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 1, 1);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.GameObjectType == EGameObjectType.SpellFocus)//Почтовый ящик
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 2 " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }

                    if (gameObject.Name == "Barricade")
                    {
                        // Host.log("Проверяю обстаклы " + Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)));
                        if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                        {
                            Host.log("Ставлю обстакл 3");
                            Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 8, 8);
                            Host.SetVar(gameObject, "obstacle", true);
                        }
                    }
                }

                foreach (var myObstacle in _myObstacles)
                {
                    if (Host.Me.Distance(myObstacle.Loc) < 150)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(myObstacle.Loc)))
                        {
                            Host.log("Ставлю обстакл 4");
                            var loc = new Vector3F(myObstacle.Loc.X, myObstacle.Loc.Y, myObstacle.Loc.Z - 2);
                            Host.AddObstacle(loc, myObstacle.Hight + 1, myObstacle.Radius + 5);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Host.log(e + "");
            }
        }

        private void ModuleTick()
        {
            try
            {
                if (Host.ClientType == EWoWClient.Retail)
                {
                    CheckObstacle();
                }

                if (Host.Me.Distance(-1265.70, 52.15, 127.31) < 150)
                {
                    if (Host.IsInsideNavMesh(new Vector3F(-1265.71, 51.59, 127.38)))
                    {
                        Host.log("Ставлю обстакл 5");
                        var loc = new Vector3F(-1265.70, 52.15, 127.31);
                        Host.AddObstacle(loc, 3, 3);
                    }
                }

                if (Host.Me.Distance(-1226.71, 81.82, 130.60) < 150)
                {
                    if (Host.IsInsideNavMesh(new Vector3F(-1226.71, 81.82, 130.60)))
                    {
                        Host.log("Ставлю обстакл 5");
                        var loc = new Vector3F(-1226.71, 81.82, 130.60);
                        Host.AddObstacle(loc, 3, 3);
                    }
                }

                if (Host.Me.Distance(-4844.50, -865.31, 501.91) < 150)
                {
                    if (Host.IsInsideNavMesh(new Vector3F(-4844.50, -865.31, 501.91)))
                    {
                        Host.log("Ставлю обстакл 5");
                        var loc = new Vector3F(-4844.50, -865.31, 501.91);
                        Host.AddObstacle(loc, 2, 2);
                    }
                }

                if (Host.Me.Distance(-1255.61, 52.50, 126.96) < 150)
                {
                    if (Host.IsInsideNavMesh(new Vector3F(-1255.61, 52.50, 126.96)))
                    {
                        Host.log("Ставлю обстакл 5");
                        var loc = new Vector3F(-1255.61, 52.50, 126.96);
                        Host.AddObstacle(loc, 2, 2);
                    }
                }


                /* if (Host.Me.Distance(-73.46, -4322.14, 65.32) < 150)
                 {
                     if (Host.IsInsideNavMesh(new Vector3F(-73.46, -4322.14, 65.32)))
                     {
                         Host.log("Ставлю обстакл 5");
                         var loc = new Vector3F(-73.46, -4322.14, 65.32 - 2);
                         Host.AddObstacle(loc, 5, 5);
                     }
                 }

                 if (Host.Me.Distance(-64.69, -4319.42, 63.12) < 150)
                 {
                     if (Host.IsInsideNavMesh(new Vector3F(-64.69, -4319.42, 63.12)))
                     {
                         Host.log("Ставлю обстакл 5");
                         var loc = new Vector3F(-64.69, -4319.42, 63.12- 2);
                         Host.AddObstacle(loc, 5, 5);
                     }
                 }*/



                if (Host.RunRun)
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
                                if (Host.MyGetAura(17) == null && Host.MyGetAura(6788) == null)
                                {
                                    Host.FarmModule.UseSkillAndWait(17, Host.Me, false);//Щит
                                }

                                if (Host.MyGetAura(139) == null)
                                {
                                    Host.FarmModule.UseSkillAndWait(139, Host.Me, false);//Обновление
                                }

                                Host.FarmModule.UseSkillAndWait(586, Host.Me, false);//Уход в тень

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
                                var listRejuvenation = new List<uint> { 1058, 774 };
                                foreach (var u in listRejuvenation)
                                {
                                    if (Host.MyGetAura(u) != null)
                                    {
                                        break;
                                    }

                                    if (Host.MyGetAura(u) == null && Host.SpellManager.GetSpell(u) != null)
                                    {
                                        Host.FarmModule.UseSkillAndWait(u, Host.Me, false);//Обновление 
                                        break;
                                    }

                                }

                            }
                            break;
                        case EClass.DemonHunter:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (!Host.Me.IsInCombat)
                    {
                        Host.MyDelBigObstacle(true);
                        Host.log("Убежал");
                        Host.RunRun = false;
                        Host.CancelMoveTo();
                        if (Host.Me.IsAlive)
                        {
                            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        }
                    }
                }

                if (Host.CharacterSettings.Mode == Mode.Script && Host.CharacterSettings.PikPocket)
                {
                    Host.ObstaclePic();
                }

                /* if (Host.CharacterSettings.Mode == Mode.QuestingClassic)
                 {
                     foreach (var entity in Host.GetEntities<Unit>())
                     {
                         if(entity.Id == 832)
                             Host.MyBigObstacleAdd(entity.Location, WowGuid.Zero);
                     }
                 }*/

                if (Host.FarmModule.ReadyToActions)
                {
                    if (!InFight())
                    {
                        if (DateTime.UtcNow > _nextEquip)
                        {
                            _nextEquip = DateTime.UtcNow.AddSeconds(5);

                            if (Equip && Host.CharacterSettings.AutoEquip)
                            {
                                EquipBestArmorAndWeapon();
                                if (Host.CharacterSettings.Mode == Mode.QuestingClassic && Host.MyTotalInvSlot() == 16 && Host.MyGetFreeSlot() <= 4)
                                {
                                    foreach (var item in Host.ItemManager.GetItems())
                                    {
                                        if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                                            item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                                            item.Place != EItemPlace.InventoryItem)
                                        {
                                            continue;
                                        }

                                        if (item.ItemClass == EItemClass.Armor || item.ItemClass == EItemClass.Weapon)
                                        {
                                            if (item.ItemQuality == EItemQuality.Poor)
                                            {
                                                if (item.GetSellPrice() < 10)
                                                {
                                                    if (!item.Destroy())
                                                    {
                                                        Host.log("Не удалось удалить " + Host.GetLastError(), LogLvl.Error);
                                                        Thread.Sleep(5000);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            if (Host.ClientType == EWoWClient.Classic)
                            {
                                LearnTalant();
                            }

                            if (Host.ClientType == EWoWClient.Classic)
                            {
                                Host.MyLearnPetSpell();
                            }
                        }
                    }
                }




                foreach (var eventSetting in Host.CharacterSettings.EventSettings)
                {
                    switch (eventSetting.TypeEvents)
                    {
                        case EventsType.PartyInvite when _isGroupInvite:
                        case EventsType.ChatMessage when IsChatMessage:
                        case EventsType.Death when Host.FarmModule.EventDeath:
                        case EventsType.DeathPlayer when Host.FarmModule.EventDeathPlayer:
                        case EventsType.Inactivity when Host.EventInactive:
                        case EventsType.AttackPlayer when _eventAttackPlayer:
                        case EventsType.PlayerInZone when EventPlayerInZone:
                            ActionEvent(eventSetting);
                            break;
                        case EventsType.NotSellected:
                            break;
                        case EventsType.Gm:
                            break;
                        case EventsType.ClanInvite:
                            break;
                        case EventsType.GmServer:
                            break;
                    }
                }

                Host.FarmModule.EventDeathPlayer = false;
                _isGroupInvite = false;
                IsChatMessage = false;
                EventPlayerInZone = false;
                Host.FarmModule.EventDeath = false;
                Host.FarmModule.EventDeath = false;
                Host.EventInactiveCount = 0;
                Host.EventInactive = false;
                _eventAttackPlayer = false;
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }



    }
}