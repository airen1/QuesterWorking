using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WowAI.Properties;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Documents;
using Out.Internal.Core;
using WoWBot.Core;
using WowAI.UI;
using Out.Utility;

namespace WowAI.Modules
{
    internal class CommonModule : Module
    {
        public override void Stop()
        {
            try
            {
                Host.onUserNavMeshPreMoveFull -= NavMeshPreMoveFull;
                Host.onUserNavMeshPreMove -= NavMeshPreMove;
                Host.onMoveTick -= MyonMoveTick;
                Host.onCastFailedMessage -= MyonCastFailedMessage;
                // host.onCreatureAttacked -= MyCreatureAttacked;
                // host.onSkillLaunched -= MeSkillLaunched;
                // host.onChatNotify -= OnChatNotify;
                // host.onPartyInvite -= MyonPartyInvite;
                // host.onGuildInvite -= MyonGuildInvite;
                //  host.onFoundGameMaster -= MyonFoundGameMaster;
                base.Stop();
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }
        public GpsBase GpsBase = new GpsBase();
        public override void Start(Host host)
        {
            try
            {
                base.Start(host);
                //_navUpdate = host.GetUnixTime();
                //Грузим базу с многоугольниками, описывающими зоны
                ZonesGps = new GpsBase();
                GpsBase.LoadDataBase(Resources.helpGps);
                /*   var point1 = GpsBase.AddPoint(1654, -4351, 26, "");
                   var point2 = GpsBase.AddPoint(1625, -4366, 24, "");
                   var point3 = GpsBase.AddPoint(1622, -4375, 24, "");
                   var point4 = GpsBase.AddPoint(1612, -4373, 24, "выход");
                   var point5 = GpsBase.AddPoint(1602, -4378, 20, "");
                   var point6 = GpsBase.AddPoint(1609, -4415, 14, "Вход на аук");
                   var point7 = GpsBase.AddPoint(1635, -4445, 17, "Аук");
                   GpsBase.AddLink(point1, point2);
                   GpsBase.AddLink(point2, point3);
                   GpsBase.AddLink(point3, point4);
                   GpsBase.AddLink(point4, point5);
                   GpsBase.AddLink(point5, point6);
                   GpsBase.AddLink(point6, point7);*/



                //ZonesGps.LoadDataBase(Resources.zones);
                //  LoadCurrentZoneMesh(host.Me.Location.X, host.Me.Location.Y);
                // host.onSkillLaunched += MeSkillLaunched;
                // host.onCreatureAttacked += MyCreatureAttacked;
                host.onUserNavMeshPreMoveFull += NavMeshPreMoveFull;
                Host.onUserNavMeshPreMove += NavMeshPreMove;
                Host.onMoveTick += MyonMoveTick;
                Host.onCastFailedMessage += MyonCastFailedMessage;
                //  host.onChatNotify += OnChatNotify;
                //  host.onPartyInvite += MyonPartyInvite;
                // host.onGuildInvite += MyonGuildInvite;
                // host.onFoundGameMaster += MyonFoundGameMaster;
            }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }

        public void MyonCastFailedMessage(uint SpellID, ESpellCastError Reason, int FailedArg1, int FailedArg2)
        {
            try
            {
                Host.log(SpellID + " " + Reason + " " + FailedArg1 + " " + FailedArg2);
                if (SpellID == 131476 && Reason == ESpellCastError.NOT_FISHABLE)
                {
                    Host.SetMoveStateForClient(true);
                    Host.TurnLeft(true);
                    Thread.Sleep(500);
                    Host.TurnLeft(false);
                    Host.SetMoveStateForClient(false);
                    Host.AutoQuests.StartWait = false;
                }
                if (SpellID == 131476 && Reason == ESpellCastError.TOO_SHALLOW)
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


        public void MyonFoundGameMaster(string name, Vector3F pos)
        {
            try
            {
                Host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Host.Me.Name + "(" + Host.Me.Level + ")" + " Внимание сработало событие GM " + name + " дист " + Host.Me.Distance(pos), "События");

                foreach (var eventSetting in Host.CharacterSettings.EventSettings)
                {
                    if (eventSetting.TypeEvents == CharacterSettings.EventsType.GMServer)
                    {
                        ActionEvent(eventSetting);
                        if (eventSetting.ActionEvent == CharacterSettings.EventsAction.Log)
                        {
                            Host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + Host.Me.Name + "(" + Host.Me.Level + ")" + " Внимание сработало событие GM " + name + " дист " + Host.Me.Distance(pos), "События");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        /* public void MyonPartyInvite(string inviterName, EPartyType partyType)
         {
             foreach (var eventSetting in host.CharacterSettings.EventSettings)
             {
                 if (eventSetting.TypeEvents == CharacterSettings.EventsType.PartyInvite)
                 {
                     ActionEvent(eventSetting);
                     if (eventSetting.ActionEvent == CharacterSettings.EventsAction.Log)
                     {
                         host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + host.Me.Name + "(" + host.Me.Level + ")" + " Внимание сработало событие " + inviterName + " пригласил в " + partyType, "События");
                     }
                 }
             }
         }*/

        /*public void MyonGuildInvite(string guildName, string inviterName)
        {
            if (host.CharacterSettings.MyClan && guildName == host.CharacterSettings.MyClanName)
            {
                Thread.Sleep(5000);
                host.AcceptGuildInvite();
            }
               

            foreach (var eventSetting in host.CharacterSettings.EventSettings)
            {
                if (eventSetting.TypeEvents == CharacterSettings.EventsType.ClanInvite)
                {
                    ActionEvent(eventSetting);
                    if (eventSetting.ActionEvent == CharacterSettings.EventsAction.Log)
                    {
                        host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + host.Me.Name + "(" + host.Me.Level + ")" + " Внимание сработало событие " + inviterName + " пригласил в " + guildName, "События");
                    }
                }
            }
        }*/



        public void MyUnmount()
        {
            try
            {
                if (!Host.IsAlive(Host.Me) || Host.Me.MountId == 0)
                    return;

                foreach (var s in Host.Me.GetAuras())
                {
                    if (s.IsPartOfSkillLine(777))
                    {
                        if (!s.Cancel())
                            Host.log("Не удалось отозвать маунта " + Host.GetLastError(), Host.LogLvl.Error);

                    }

                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        DateTime _lastMountTime = DateTime.Now - TimeSpan.FromMinutes(1);



        public Aura MyGetAura(uint id)
        {
            try
            {
                foreach (var aura in Host.Me.GetAuras())
                {
                    if (aura.SpellId == id)
                        return aura;
                }

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
                /* if (_lastMountTime.AddMinutes(1) > DateTime.Now)//"Сбор ресурсов")
                     return;
                 */
                if (Host.Me.Distance(loc) < 2)
                    return;
                if (Host.FarmModule.BestMob != null)
                    return;
                if (Host.CharacterSettings.Mode == EMode.Questing)
                {
                    if (Host.AutoQuests.BestQuestId == 47880)
                        return;
                }

                if (Host.CharacterSettings.Mode == EMode.Script)
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

                if(Host.MapID == 1643 && Host.AutoQuests.BestQuestId == 47098)
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
                                    List<uint> listDash = new List<uint> { 1850, 106898 };
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

                                if (formId == 783 && Host.GetThreats().Count == 0)
                                {
                                    if (Host.Me.RunSpeed < 13 && Host.Me.SwimSpeed < 9)
                                    {
                                        Host.log("Отменяю форму так как скорость " + Host.Me.RunSpeed + "   " + Host.Me.MovementFlags);
                                        Host.CanselForm();
                                        Thread.Sleep(500);
                                        break;
                                    }
                                }


                                //   Host.log("Нашел ауру");
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
                                    Host.log("Не удалось поменять форму 1 " + spell.Name + "  " + resultForm, Host.LogLvl.Error);
                                }
                                else
                                    Host.log("Поменял форму " + spell.Name, Host.LogLvl.Ok);

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
                /*   var myMount = new List<Mount>();
                   if (host.Me.Mount != null || host.Me.GetMounts().Count <= 0)
                       return;

                   var randMount = host.RandGenerator.Next(0, host.Me.GetMounts().Count);
                   foreach (var i in host.Me.GetMounts())
                       if (i.Type == EBotTypes.Mount)
                           myMount.Add(i);

                   if (myMount[randMount].UseMount())
                   {
                       // host.log("Призываю маунта №" + randMount + " " + myMount[randMount].Db.LocalName);
                       Thread.Sleep(2100);
                       while (host.Me.IsCastingSkill)
                       {
                           Thread.Sleep(100);
                       }
                       _lastMountTime = DateTime.Now;
                   }
                   else
                   {
                       host.log("Не удалось призвать маунта: " + host.GetLastError(), Host.LogLvl.Error);
                   }*/
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        /// <summary>
        /// Я в бою?
        /// </summary>
        /// <returns></returns>
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

        private readonly List<uint> _obstacle = new List<uint>();


        /*   private void OnChatNotify(EChatChannel channel, string talker, string message)
           {
               if (channel == EChatChannel.Whisper || channel == EChatChannel.Normal)
               {
                   foreach (var eventSetting in host.CharacterSettings.EventSettings)
                   {
                       if (eventSetting.TypeEvents == CharacterSettings.EventsType.ChatMessage)
                       {
                           ActionEvent(eventSetting);
                           if (eventSetting.ActionEvent == CharacterSettings.EventsAction.Log)
                           {
                               host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + host.Me.Name + "(" + host.Me.Level + ")" + " Внимание сработало событие чата " + talker + ": " + message, "События");
                           }
                       }
                   }
               }
           }*/


        //private void MeSkillLaunched(Creature obj, Creature objTarget, int skillId, Vector3F location, Vector3F targetLocation)
        //{
        //    try
        //    {
        //        if (!host.CharacterSettings.ShiftAoe)
        //            return;
        //        if (obj?.Type != EBotTypes.Npc)
        //            return;
        //        if (!obj.IsEnemy)
        //            return;
        //        if ((obj as Npc).Db.Grade == ENPCGradeType.Normal && obj.Id != 125029)
        //            return;

        //        //Скилы в данже 20
        //        if (skillId == 9001811)
        //            return;
        //        if (skillId == 9001031)
        //            return;
        //        if (skillId == 100652)
        //            return;

        //        /*   host.Log(host.CachedDbNpcInfos[obj.Id].LocalName + " " + host.CachedDbNpcInfos[obj.Id].Grade + " кастует " + host.CahedDbNpcSkillInfos[skillId].LocalName + " на " + objTarget?.Name);
        //           host.Log("location: " + location + "       targetLocation: " + targetLocation + "          Me.Location: " + host.Me.Location + "  Дистанция:  " +host.Me.Distance(targetLocation.X, targetLocation.Y, targetLocation.Z));
        //           host.Log("ApplyMoment: " + host.CahedDbNpcSkillInfos[skillId].ApplyMoment +
        //                     " ApplyingType: " + host.CahedDbNpcSkillInfos[skillId].ApplyingType +
        //                     " FiringTime: " + host.CahedDbNpcSkillInfos[skillId].FiringTime +
        //                     " ShapeRadiusMax: " + host.CahedDbNpcSkillInfos[skillId].ShapeRadiusMax 
        //                    // " ShapeRadiusMin  " + host.CahedDbNpcSkillInfos[skillId].ShapeRadiusMin
        //                );
        //           host.Log("---------------------------------------------------------------------------------------");*/

        //        if (host.GameDB.DBNpcSkillInfos[skillId].ApplyingType == EApplyingType.SelfArea && host.Me.Distance(obj) < host.GameDB.DBNpcSkillInfos[skillId].ShapeRadiusMax
        //            || host.GameDB.DBNpcSkillInfos[skillId].ApplyingType == EApplyingType.Area && host.Me.Distance(targetLocation.X, targetLocation.Y, targetLocation.Z) < host.GameDB.DBNpcSkillInfos[skillId].ShapeRadiusMax
        //            )
        //        {
        //            if (host.Me.ClassType == EClass.Berserker && host.Me.Energy < 20)
        //                return;
        //            if (host.Me.ClassType == EClass.Assassin && host.Me.Energy < 25)
        //                return;
        //            if (host.Me.ClassType == EClass.Ranger && host.Me.Energy < 35)
        //                return;
        //            if (host.Me.ClassType == EClass.Mage && host.Me.Energy < 35)
        //                return;
        //            if (host.Me.ClassType == EClass.Mystic && host.Me.Energy < 35)
        //                return;
        //            if (host.Me.Energy < 10)
        //                return;
        //            var delayAoe = Math.Round(host.GameDB.DBNpcSkillInfos[skillId].ApplyMoment * 1000);
        //            //  var percent = delayAoe * 0.50;
        //            int resdelayAoe = 0/*Convert.ToInt32(delayAoe - percent)*/;

        //            if (delayAoe > host.CharacterSettings.DelayAoe)
        //                resdelayAoe = Convert.ToInt32(delayAoe - host.CharacterSettings.DelayAoe);
        //            else
        //                resdelayAoe = 0;

        //            NpcLaunchAoeAttack = true;
        //            //  host.Log("АОЕ!!!!!!!  " + resdelayAoe + " " + skillId);
        //            Thread.Sleep(resdelayAoe);

        //            host.StartVoluntaryAction();
        //            Thread.Sleep(300);
        //            host.EndVoluntaryAction();
        //            host.TurnDirectly(host.Me.Target);
        //            host.SetCamRotation(host.Me.Rotation.Y);

        //            ShapeRadiusMax = host.GameDB.DBNpcSkillInfos[skillId].ShapeRadiusMax;
        //            FiringTime = host.GameDB.DBNpcSkillInfos[skillId].FiringTime;

        //            NpcLaunchAoeAttack = false;

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        host.log(e.ToString());
        //    }
        //}

        // public Creature AttackerEscort;

        public bool AttackPlayer = false;
        //public DateTime AttackPlayerTime;
        public bool EventAttackPlayer;
        //private void MyCreatureAttacked(AttackInfo info)
        //{
        //    try
        //    {
        //        //  string attacklog = "";
        //        if(info.Attacker == null)
        //            return;

        //        // attacklog = attacklog + info.Attacker.Name + " атакует ";
        //        //  host.log(attacklog);

        //        //Вражеская фракци
        //        if (info.Attacker.Type == EBotTypes.Player && host.FarmModule.bestMob?.Type != EBotTypes.Player)
        //            foreach (var attackTargetInfo in info.AttackTargetInfo)
        //            {
        //                if (attackTargetInfo.Target == host.Me)
        //                {
        //                    if (host.GetPlayerFraction(info.Attacker) != host.GetPlayerFraction(host.Me) && host.CharacterSettings.Mode != "Данж(п)")
        //                    {
        //                        host.log("Нападение игрока " + info.Attacker.Name);
        //                        host.FarmModule.bestMob = info.Attacker;
        //                        host.SetTarget(info.Attacker);
        //                        host.CancelMoveTo();
        //                        host.CommonModule.SuspendMove();
        //                        AttackPlayer = true;
        //                        EventAttackPlayer = true;
        //                        AttackPlayerTime = DateTime.Now;
        //                    }

        //                }
        //            }
        //        //Проверка на бой
        //        if (info.Attacker != null)
        //        {
        //            host.SetVar(info.Attacker, "InFight", true);
        //            host.SetVar(info.Attacker, "Time", info.Time);
        //        }
        //        foreach (var attackTargetInfo in info.AttackTargetInfo)
        //        {
        //            if (attackTargetInfo.Target != null)
        //            {
        //                host.SetVar(attackTargetInfo.Target, "InFight", true);
        //                host.SetVar(attackTargetInfo.Target, "Time", info.Time);
        //            }
        //        }

        //        if (host.Me.Escortor != null)
        //        {
        //            foreach (var attackTargetInfo in info.AttackTargetInfo)
        //            {
        //                if (attackTargetInfo.Target == host.Me.Escortor)
        //                {
        //                    AttackerEscort = info.Attacker;
        //                    break;
        //                }
        //            }
        //        }

        //        //  string attacklog = "";
        //        if (info.Attacker != host.Me)
        //            return;

        //        //   attacklog = "DPS: " + host.AllDamage/host.TimeInFight + " ";

        //        //  attacklog = attacklog + info.Attacker.Name + " атакует ";
        //        foreach (var attackTargetInfo in info.AttackTargetInfo)
        //        {
        //            if (attackTargetInfo.Target == host.Me)
        //                return;
        //            //  attacklog = attacklog + attackTargetInfo.Target.Name + "  AttackChance: " + attackTargetInfo.AttackChance;
        //            foreach (var attackDamageInfo in attackTargetInfo.AttackDamageInfo)
        //            {
        //                // attacklog = attacklog + " и наносит " + attackDamageInfo.Damage + " по " + attackDamageInfo.Type;
        //                host.AllDamage = host.AllDamage + attackDamageInfo.Damage;
        //            }
        //        }
        //        //  attacklog = attacklog + " SkillId: " + info.SkillId + " " /*+ host.CachedDbSkillInfos[info.SkillId].LocalName*/;
        //        //attacklog = attacklog + " время: " + info.Time;

        //        //  host.Log(attacklog);
        //    }
        //    catch (Exception e)
        //    {
        //        host.log(e.ToString());
        //    }
        //}

        //private long _navUpdate;

        private Vector3F ObstaclePoint = new Vector3F();
        private void NavMeshPreMove(Vector3F point)
        {

            if (Host.FarmModule.BestMob != null && Host.FarmModule.BestMob.HpPercents < 100)
                return;

            if (Host.FarmModule.farmState == FarmState.Disabled)
                return;
            if (Host.MapID == 1904)
                return;
            ObstaclePoint = point;
            //   Host.log("Тест 2");
            if (Host.CharacterSettings.Attack)
            {
                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (Host.CharacterSettings.Mode == EMode.Questing)
                        break;
                    /*if (Host.Me.Target != null && Host.Me.Target.HpPercents < 100)
                        break;*/
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
            }
        }
        private void NavMeshPreMoveFull(Vector3F[] points)
        {
            try
            {



                /*  if (host.GetUnixTime() < _navUpdate + 1000)
                      return true;*/
                // _navUpdate = host.GetUnixTime();
                if (Host.FarmModule.BestMob != null && Host.FarmModule.BestMob.HpPercents < 100)
                    return;

                if (Host.FarmModule.farmState == FarmState.Disabled)
                    return;
              
                if (Host.MapID == 1904)
                    return;
                //  Host.log("Тест");

                /*  if (Host.CharacterSettings.Mode == EMode.Script)//"Данж.(п)")
                      return;*/

                /*   switch (host.Me.ConditionPhase)
                   {
                       case EConditionPhase.Spirit:
                           return true;
                       case EConditionPhase.Dead:
                           return true;
                       case EConditionPhase.Alive:
                           break;
                       case EConditionPhase.Stealth:
                           break;
                       case EConditionPhase.All:
                           break;
                       default:
                           throw new ArgumentOutOfRangeException();
                   }*/

                /*  if (host.Me.Escortor != null && host.GetAgroCreatures().Count == 0)
                  {
                      foreach (var attackerEscortor in host.GetCreatures())
                      {
                          if (host.Me.GetItemsCount(50318) > 0)
                              break;

                          if (attackerEscortor.IsEnemy
                              && host.IsAlive(attackerEscortor)
                              && host.Me.Distance(attackerEscortor) < 10
                              && host.GetAgroCreatures().Count == 0
                              && host.FarmModule.bestMob == null)
                          {
                              host.FarmModule.bestMob = attackerEscortor;
                              host.SetTarget(attackerEscortor);
                              host.CancelMoveTo();
                              host.CommonModule.SuspendMove();
                              break;
                          }
                      }
                  }*/
                if (Host.CharacterSettings.Attack)
                {
                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if (Host.CharacterSettings.Mode == EMode.Questing)
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
                    if (Host.GetAgroCreatures().Contains(Host.FarmModule.BestMob)
                        && Host.IsAlive(Host.FarmModule.BestMob))
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


                //if (host.GetAgroCreatures().Count == 0)
                //{
                //    var interceptionlist = new List<Creature>();
                //    foreach (var point in points)
                //    {
                //        if (host.Me.Distance(point.X, point.Y, point.Z) > 30)
                //            break;

                //        foreach (var interception in host.GetCreatures())
                //        {
                //            if (!host.IsExists(interception))
                //                continue;
                //            var zRange = Math.Abs(host.Me.Location.Z - interception.Location.Z);

                //            if (zRange > 10)
                //                continue;
                //            if (interception.Type != EBotTypes.Npc)
                //                continue;

                //            if (host.Me.Distance(interception) > 30)
                //                continue;
                //            if (interception.Id == 370007)
                //                continue;
                //            if (interception.Id == 201040)
                //                continue;
                //            if (interception.Id == 611502)
                //                continue;
                //            if (host.CheckBuff(2000881, interception)) //баф на иммунитет
                //                continue;
                //            if (interceptionlist.Contains(interception))
                //                continue;
                //            if (!interception.IsEnemy)
                //                continue;
                //            if (!host.IsAlive(interception))
                //                continue;
                //            if (interception.HpPercents < 100)
                //                continue;

                //            if (!interception.IsVisible)
                //                continue;

                //            if (host.Me.Target != null && host.Me.Target.HpPercents < 100)
                //                break;
                //            if (host.FarmModule.bestMob != null && host.Me.Distance(host.FarmModule.bestMob) < host.Me.Distance(interception))
                //                continue;

                //            if (interception == host.FarmModule.bestMob)
                //                continue;


                //            if (host.GetVar(interception, "InFight") != null)
                //            {
                //                var infight = (bool)(host.GetVar(interception, "InFight"));
                //                if (infight)
                //                {
                //                    //  host.log(interception.Name + " В бою!!", Host.LogLvl.Error);
                //                    continue;
                //                }
                //            }


                //            if ((interception as Npc).Db.Grade == ENPCGradeType.Elite)
                //                continue;

                //            if ((interception as Npc).Db.AggressiveType == EAggressiveType.Aggressive
                //                && host.Distance(point.X, point.Y, point.Z, interception.Location.X, interception.Location.Y, interception.Location.Z) <
                //                (interception as Npc).Db.AudibleRange + 1)
                //            {
                //                //  host.log("Нас могут перехватить: " + interception.Name + "[" + interception.Sid + "]" + " дист:" + host.Me.Distance(interception), Host.LogLvl.Important);
                //                interceptionlist.Add(interception);
                //            }
                //        }
                //    }


                //    //Найти ближайшего моба из тех кто может ссагрится
                //    double finalDist = 999999;
                //    Creature finalMob = null;


                //    foreach (var bestinterception in interceptionlist)
                //        if (finalDist > host.Me.Distance(bestinterception))
                //        {
                //            finalDist = host.Me.Distance(bestinterception);
                //            finalMob = bestinterception;
                //        }

                //    if (finalMob != null && finalMob != host.FarmModule.bestMob)
                //    {
                //        host.FarmModule.bestMob = finalMob;
                //        host.CommonModule.SuspendMove();
                //        host.CancelMoveTo();
                //        host.SetTarget(host.FarmModule.bestMob);
                //        //  host.log("Ближайший моб из перехвата: " + finalMob.Name + " дист:" + host.Me.Distance(finalMob), Host.LogLvl.Error);
                //    }
                //}
                // host.log("Мобов проверил за                               " + sw.ElapsedMilliseconds + " мс");
            }
            catch (Exception err)
            {
                Host.log(err.ToString());
            }
            return;
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
                    return EEquipmentSlot.MainHand;
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
                var equipCells = new Dictionary<EEquipmentSlot, Item>();
                // host.log("Тест  EquipBestArmorAndWeapon");

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
                            /* if (itemEquipType == EEquipmentSlot.Unk17)
                                 continue;*/
                            //  Host.log(item.Name + "  " + itemEquipType);
                            if (equipCells[itemEquipType] == null)
                                equipCells[itemEquipType] = item;
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

                /* foreach (var equipCell in equipCells)
                 {

                     Host.log(equipCell.Key + " " + equipCell.Value?.Name + "  " + equipCell.Value?.Place + "  " + equipCell.Value?.Place);
                 }*/
                //  Host.log("-----------------------------------------");
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
                            Host.log("Error. Can't equip " + equipCells[b].InventoryType + "  best item = " +
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



        private bool FindGmAssHer()
        {
            /* foreach (var creature in host.GetCreatures())
             {
                 if (creature.Type == EBotTypes.Npc)
                     continue;
                 if (creature.Name.IndexOf("ass", StringComparison.CurrentCultureIgnoreCase) != -1
                     || creature.Name.IndexOf("gm", StringComparison.CurrentCultureIgnoreCase) != -1)
                 {
                     host.Log(DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) + ":   " + host.Me.Name + "(" + host.Me.Level + ")" + " Внимание сработало событие на ГМ: " + creature.Name, "События");
                     return true;
                 }
             }*/
            return false;
        }

        public void SummonPet()
        {
            /* if (!host.IsAlive(host.Me))
                 return;
             if (host.Me.Pet == null)
             {
                 foreach (var characterSettingsPetSetting in host.CharacterSettings.PetSettings)
                 {
                     foreach (var pet in host.Me.GetPets())
                     {
                         if (pet.Id != characterSettingsPetSetting.Id)
                             continue;
                         if (pet.Fatigue > pet.Db.FatigueMax / 2)
                         {
                             if (pet.Use())
                             {
                                 host.log("Призываю " + pet.Db.LocalName + " Бодрость: " + pet.Fatigue);
                             }
                             else
                             {
                                 host.log("Не удалось призвать " + pet.Db.LocalName + " Бодрость: " + pet.Fatigue + "  " + host.GetLastError(), Host.LogLvl.Error);
                             }
                         }
                     }
                 }
             }*/
        }

        public void ModuleTick()
        {
            try
            {
                if (Host.GameState == EGameState.Ingame && Host.IsAlive(Host.Me) && Host.Me.Level > 0)
                {

                    foreach (var gameObject in Host.GetEntities<GameObject>())
                    {
                        if (gameObject.Id == 303217)//Почтовый ящик
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 3, 3);
                            }
                        }

                        if (gameObject.Id == 202589)//Почтовый ящик
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 3, 3);
                            }
                        }
                        if (gameObject.Id == 142109)//Почтовый ящик
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 3, 3);
                            }
                        }

                        if (gameObject.Id == 31407)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 5, 5);
                            }
                        }
                        if (gameObject.Id == 289694)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 5, 5);
                            }
                        }

                        if (gameObject.Id == 195146)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            }
                        }

                        if (gameObject.Id == 31573)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            }
                        }

                        if (gameObject.Id == 31578)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            }
                        }

                        if (gameObject.Id == 204135)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 2, 2);
                            }
                        }

                        if (gameObject.Id == 31411)
                        {
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл " + gameObject.CollisionHeight + " " + gameObject.CollisionScale + " " + gameObject.ObjectSize + " " + gameObject.ObjectSize2 + " " + gameObject.Scale);
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 5, 5);
                            }
                        }

                        if (gameObject.Name == "Barricade")
                        {
                            // Host.log("Проверяю обстаклы " + Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)));
                            if (Host.IsInsideNavMesh(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z)))
                            {
                                Host.log("Ставлю обстакл");
                                Host.AddObstacle(new Vector3F(gameObject.Location.X, gameObject.Location.Y, gameObject.Location.Z), 8, 8);
                            }
                        }
                    }

                    if (Host.Me.Distance(1475.53, 1596.20, 45.40) < 100)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(1475.53, 1596.20, 45.40)))
                        {
                            Host.log("Ставлю обстакл ");
                            Host.AddObstacle(new Vector3F(1475.53, 1596.20, 45.40), 8, 8);
                        }
                    }

                    if (Host.Me.Distance(2047.28, 1666.26, 22.94) < 100)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(2047.28, 1666.26, 22.94)))
                        {
                            Host.log("Ставлю обстакл ");
                            Host.AddObstacle(new Vector3F(2047.28, 1666.26, 22.94), 8, 8);
                        }
                    }

                    if (Host.Me.Distance(1012.61, 1382.93, 22.30) < 100)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(1012.61, 1382.93, 22.30)))
                        {
                            Host.log("Ставлю обстакл ");
                            Host.AddObstacle(new Vector3F(1012.61, 1382.93, 22.30), 8, 8);
                        }
                    }
                    if (Host.Me.Distance(-1431.77, 1261.11, 193.38) < 100)
                    {
                        if (Host.IsInsideNavMesh(new Vector3F(-1431.77, 1261.11, 193.38)))
                        {
                            Host.log("Ставлю обстакл ");
                            Host.AddObstacle(new Vector3F(-1431.77, 1261.11, 193.38), 8, 8);
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
                    if (eventSetting.TypeEvents == CharacterSettings.EventsType.GMAssHer && FindGmAssHer())
                    {
                        ActionEvent(eventSetting);
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
        private string _curMesh = "";

        internal void LoadCurrentZoneMesh(double x, double y)
        {
            try
            {
                // var sw = new Stopwatch();
                //  sw.Start();
                foreach (var p in ZonesGps.GetAllGpsPolygons())
                {
                    if (p.PointInZone(x, y))
                    {
                        if (_curMesh != p.Name)
                        {
                            _curMesh = p.Name;
                            var meshName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Quester\\Meshes\\" + _curMesh + ".nav";
                            if (File.Exists(meshName))
                            {
                                Host.LoadNavMesh(meshName);
                                Host.log("Load zone " + meshName, Host.LogLvl.Ok);
                            }
                            else
                                Host.log("Не нашел файл " + meshName, Host.LogLvl.Error);
                        }
                        break;
                    }
                }
                // host.log("Загрузка зоны                                               " + sw.ElapsedMilliseconds);
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public string GetZoneByCoords(double x, double y)
        {
            foreach (var p in ZonesGps.GetAllGpsPolygons())
                if (p.PointInZone(x, y))
                    return p.Name;
            return "";
        }


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
                        while ((Host.Me.MovementFlags & EMovementFlag.DisableGravity) == EMovementFlag.DisableGravity && (Host.Me.MovementFlags & EMovementFlag.Root) == EMovementFlag.Root)
                        {
                            Host.log("Ожидаю возврата движения " + Host.Me.MovementFlags);
                            Thread.Sleep(5000);
                            _moveFailCount = 0;
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


                if (_moveFailCount > 2)
                {
                    if (Host.MapID != 600)
                    {
                        if (Host.Me.Distance(-253.54, -5126.02, 29.17) > 50)
                        {
                            var obstacleRadius = 1;
                            if (_moveFailCount > 7)
                                obstacleRadius = 2;
                            if (Host.Me.Distance(ObstaclePoint) < 6)
                            {
                                Host.log("Ставлю обстакл " + obstacleRadius + "   " + Host.Me.Distance(ObstaclePoint));
                                Host.AddObstacle(ObstaclePoint, obstacleRadius, 10);
                            }
                            else
                            {
                                Host.log("Ставлю обстакл " + obstacleRadius + "   " + Host.Me.Distance(ObstaclePoint));
                                Host.AddObstacle(Host.Me.Location, obstacleRadius, 10);
                            }
                        }
                    }
                }
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
                    if(Host.Me.IsDeadGhost)
                    Host.ComboRoute.DeathTime = DateTime.Now;
                }

                if (_moveFailCount > 20 || Host.Me.Distance(1477.98, 1591.57, 39.66) < 5)
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


        public bool ForceMoveTo2(Vector3F loc, double dist = 1, double doneDist = 0.5)
        {
            try
            {

                // if (host.Me.Distance(loc.X, loc.Y, loc.Z) > 50)
                MySitMount(loc);

                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;
                doneDist = Host.Me.RunSpeed / 5.0;
                //  Host.log("Начал бег в " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + "/" + doneDist);
                var result = Host.ForceComeTo(loc, dist, doneDist);
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

        public bool ForceMoveTo2(Entity obj, double dist = 1, double doneDist = 0.5)
        {
            try
            {

                // if (host.Me.Distance(loc.X, loc.Y, loc.Z) > 50)
                MySitMount(obj.Location);

                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;
                doneDist = Host.Me.RunSpeed / 5.0;
                //  Host.log("Начал бег в " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + "/" + doneDist);
                var result = Host.ForceComeTo(obj, dist, doneDist);
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


        public bool MoveTo(double x, double y, double z, double dist = 1, double doneDist = 0.5)
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
                doneDist = Host.Me.RunSpeed / 5.0;



                var result = Host.ComeTo(x, y, z, dist, doneDist);

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

            if (Host.Me.Distance(-1130.89, 805.10, 500.08) < 20 && Host.Me.Location.Z > 480 && loc.Distance(-1130.89, 805.10, 500.08) > 30 && Host.AutoQuests.BestQuestId != 46930)
            {
                Host.log("Прыгаю вниз");
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

        public bool MoveTo(Vector3F loc, double dist = 1, double doneDist = 0.5)
        {
            if (IsMovementSuspended)
                return false;
            try
            {
                MyUseGps(loc);
                // if (host.Me.Distance(loc.X, loc.Y, loc.Z) > 50)
                MySitMount(loc);

                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;

                if (!MoveToBadLoc(loc))
                {
                    CheckMoveFailed(false);
                    return false;
                }

                if (Host.CharacterSettings.Mode == EMode.Questing || Host.AutoQuests.HerbQuest)
                    if (Host.GetNavMeshHeight(new Vector3F(loc.X, loc.Y, 0)) == 0 && Host.Me.Distance(loc) > 300)
                    {
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
                    }



                doneDist = Host.Me.RunSpeed / 6.0;
                if (loc.Distance(-986.00, -3797.00, 0.11) < 5)
                    loc.Z = (float)5.2;
                if (Host.Me.Distance(713.81, 3128.34, 133.02) < 30 && Host.Me.Distance(loc) > 300)
                    Host.MoveTo(749.13, 3099.93, 133.11);

                // Host.log("Начал бег в " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + "/" + doneDist + "  " + Host.GetNavMeshHeight(new Vector3F(loc.X, loc.Y, 0)));
                var result = Host.ComeTo(loc, dist, doneDist);
                //   Host.log("Закончил бег в " + loc + "  дист: " + Host.Me.Distance(loc));
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

        public void MyUseGps(Vector3F loc)
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

            if (pointNearDest && pointNeatMe)
            {
                var path = GpsBase.GetPath(loc, Host.Me.Location);
                foreach (var vector3F in path)
                {
                 /*   if(Host.FarmModule.farmState == FarmState.AttackOnlyAgro && Host.GetThreats().Count > 0)
                        return;*/
                    if(!Host.Me.IsAlive)
                        return;
                    if (!Host.MainForm.On)
                        return;
                    Host.log(path.Count + "  Путь " + Host.Me.Distance(vector3F));
                    ForceMoveTo2(vector3F);
                }
            }

           /* if (Host.Me.Distance(loc) > 300)
            {
                var path = Host.GetServerPath(Host.Me.Location, loc);
                Host.log("Слишком далеко, использую GetServerPath из " + Host.Me.Location + " в " + loc + " Путь:" + path.Path.Count);


                foreach (var vector3F in path.Path)
                {
                    Host.log(path.Path.Count + "  Путь " + Host.Me.Distance(vector3F));
                    ForceMoveTo2(vector3F);
                }
            }*/

        }

        public bool MoveTo(Entity obj, double dist = 1, double doneDist = 0.5)
        {
            if (IsMovementSuspended)
                return false;
            try
            {

                IsMoveToNow = true;
                if (!MoveToBadLoc(obj.Location))
                    return false;

                doneDist = Host.Me.RunSpeed / 5.0;
                var result = Host.ComeTo(obj, dist, doneDist);

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

        public bool ForceMoveTo(double x, double y, double z, double dist = 1, double doneDist = 0.5)
        {
            doneDist = Host.Me.RunSpeed / 5.0;
            var result = Host.ComeTo(x, y, z, dist, doneDist);

            CheckMoveFailed(result);
            return result;
        }

        public bool ForceMoveTo(Vector3F loc, double dist = 1, double doneDist = 0.5)
        {
            MySitMount(loc);
            doneDist = Host.Me.RunSpeed / 5.0;
            // Host.log("Начал бег");
            bool result;
            /*  if (Host.CharacterSettings.Mode == EMode.Script)
              {
                  Host.log("Бегу без учета застреваний");
                  MoveParams.Location = loc;
                  MoveParams.Obj = null;
                  MoveParams.Dist = dist;
                  MoveParams.DoneDist = doneDist;
                  MoveParams.IgnoreStuckCheck = true;
                  MoveParams.ForceRandomJumps = false;
                  MoveParams.UseNavCall = true;
                  result = Host.MoveTo(MoveParams);
              }
              else
              {*/
            result = Host.ComeTo(loc, dist, doneDist);
            //  }


            // Host.log("Закончил бег");
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
            }
            else
            {
                result = Host.ComeTo(obj, dist, doneDist);
            }
            CheckMoveFailed(result);
            return result;
        }



        public bool StopCommonModule;
        public override void Run(CancellationToken ct)
        {
            try
            {
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