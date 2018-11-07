using System;
using System.Collections.Generic;
using System.Threading;
using WowAI.Modules;
using WoWBot.Core;
using Out.Internal.Core;
using System.Linq;
using System.Diagnostics;
using WowAI.UI;

namespace WowAI.ComboRoutes
{
    internal class ComboRoute
    {
        internal Host host;

        public long TickTime;

        public List<SkillSettings> DeckSkills;

        public ComboRoute(Host host)
        {
            this.host = host;
        }

        public FarmState FarmState
        {
            get
            {
                return host.FarmModule.farmState;
            }
            set
            {
                host.FarmModule.farmState = value;
            }
        }

        internal int[] SpecialItems
        {
            get
            {
                return host.FarmModule.specialItems;
            }
        }

        internal float SpecialDist
        {
            get
            {
                return host.FarmModule.specialDist;
            }
        }

        internal List<uint> FarmMobsIds
        {
            get
            {
                return host.FarmModule.farmMobsIds;
            }
        }

        internal List<uint> FarmPropIds
        {
            get
            {
                return host.FarmModule.farmPropIds;
            }
        }

        internal Zone FarmZone
        {
            get
            {
                return host.FarmModule.farmZone;
            }
        }

        /*  public Unit BestMob
          {
              get
              {
                  return host.FarmModule.bestMob;
              }
              set
              {
                  host.FarmModule.bestMob = value;
              }
          }*/

        /* public Prop BestProp
         {
             get
             {
                 return host.FarmModule.bestProp;
             }
             set
             {
                 host.FarmModule.bestProp = value;
             }
         }*/




        public int DeadCount;
        public int DeadCountInPVP;

        public DateTime DeathTime = new DateTime();

        public bool EventDeath;
        public virtual void CheckWeDie()
        {
            try
            {
                if (!host.IsAlive() || host.Me.IsDeadGhost)
                {
                    if (host.CommonModule.AttackPlayer)
                    {
                        DeadCountInPVP++;
                    }
                    else
                    {
                        DeadCount++;
                    }

                    host.log("Умер " + host.IsAlive() + " " + host.Me.IsDeadGhost);
                    EventDeath = true;



                    host.FarmModule.BestMob = null;
                    host.FarmModule.BestProp = null;
                    FarmState = FarmState.Disabled;
                    host.CancelMoveTo();

                    if (!host.IsAlive())
                        Thread.Sleep(3000);



                    if (!host.Me.IsDeadGhost)
                        if (!host.ReturnToRevivalBasePoint())
                        {
                            host.log("IsAlive: " + host.IsAlive(host.Me) + " Воскрешение не удачно " + host.GetLastError(), Host.LogLvl.Error);
                        }
                        else
                        {
                            Thread.Sleep(2000);
                            while (host.GameState != EGameState.Ingame)
                                Thread.Sleep(1000);
                            host.CancelMoveTo();
                            Thread.Sleep(5000);
                        }


                    if (host.MapID == 1760 || host.MapID == 1904)
                    {
                        Thread.Sleep(30000);
                        return;
                    }



                    host.CommonModule.ResumeMove();
                    host.log("Ищу свой труп " + host.ReviveCorpseInfo.IsValid + "  " + host.ReviveCorpseInfo.Location + "  " + host.MapID);




                    if (host.ReviveCorpseInfo.Location.X == 0)
                        return;


                    DateTime begin = DateTime.Now;
                    DateTime end = DeathTime;
                    TimeSpan rez = begin - end;
                    Thread.Sleep(5000);
                    host.log("Последняя смерть " + rez.TotalMinutes);
                    if (rez.TotalMinutes < 5)
                    {
                        if (!host.ReturnToRevivalBasePoint())
                        {
                            host.log("IsAlive: " + host.IsAlive(host.Me) + "Воскрешение не удачно " + host.GetLastError(), Host.LogLvl.Error);
                        }
                        else
                        {
                            while (host.GameState != EGameState.Ingame)
                                Thread.Sleep(1000);
                            host.CancelMoveTo();
                            Thread.Sleep(5000);
                            foreach (var i in host.GetEntities<Unit>())
                            {
                                if (i.IsSpiritService)
                                {
                                    host.CommonModule.MoveTo(i, 2);
                                    Thread.Sleep(2000);
                                    if (host.ReviveWithNpc(i))
                                    {

                                    }
                                    else
                                    {
                                        host.log("Воскрешение на кладбище не успешно 1 " + host.GetLastError(), Host.LogLvl.Error);
                                    }
                                    Thread.Sleep(60 * 1000);
                                    if (host.CharacterSettings.DebuffDeath)
                                        while (host.GetThreats(host.Me).Count == 0 && host.MainForm.On)
                                        {
                                            Thread.Sleep(5000);
                                            if (host.ClientAfk)
                                                host.Jump();
                                            var debuf = false;
                                            foreach (var aura in host.Me.GetAuras())
                                            {
                                                if (aura.SpellId == 15007)//15007   
                                                    debuf = true;
                                            }
                                            if (!debuf)
                                                break;
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!host.CommonModule.MoveTo(host.ReviveCorpseInfo.Location, 10, 10))
                            return;
                        Thread.Sleep(3000);
                        if (!host.ReviveWithCorpse())
                        {
                            host.log("Воскрешение возле трупа не успешно  возвращаюсь на кладбище" + host.GetLastError(), Host.LogLvl.Error);
                            Thread.Sleep(10000);
                            while (host.GameState != EGameState.Ingame)
                            {
                                Thread.Sleep(1000);
                            }

                            if (!host.ReturnToRevivalBasePoint())
                            {
                                host.log("IsAlive: " + host.IsAlive(host.Me) + "Воскрешение не удачно " + host.GetLastError(), Host.LogLvl.Error);
                            }
                            else
                            {
                                while (host.GameState != EGameState.Ingame)
                                    Thread.Sleep(1000);
                                host.CancelMoveTo();
                                Thread.Sleep(5000);
                                foreach (var i in host.GetEntities<Unit>())
                                {
                                    if (i.IsSpiritService)
                                    {
                                        host.CommonModule.MoveTo(i, 2);
                                        Thread.Sleep(2000);
                                        if (host.ReviveWithNpc(i))
                                        {

                                        }
                                        else
                                        {
                                            host.log("Воскрешение на кладбище не успешно 2 " + host.GetLastError(), Host.LogLvl.Error);
                                        }
                                        Thread.Sleep(60 * 1000);
                                        if (host.CharacterSettings.DebuffDeath)
                                            while (host.GetThreats(host.Me).Count == 0 && host.MainForm.On)
                                            {
                                                Thread.Sleep(5000);
                                                if (host.ClientAfk)
                                                    host.Jump();
                                                var debuf = false;
                                                foreach (var aura in host.Me.GetAuras())
                                                {
                                                    if (aura.SpellId == 15007)
                                                        debuf = true;
                                                }
                                                if (!debuf)
                                                    break;
                                            }
                                    }
                                }
                            }
                            // return;
                        }
                    }





                    DeathTime = DateTime.Now;


                    Thread.Sleep(2000);


                    FarmState = FarmState.Disabled;
                    if (host.CharacterSettings.Mode == EMode.Questing)
                        FarmState = FarmState.AttackOnlyAgro;
                }


            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }


        public virtual void PrepareForFight()
        {
            CheckWeDie();
        }

        public virtual void Fight()
        {

            try
            {
                TickTime = host.GetUnixTime();
                // host.log(TickTime + " тик");
                if (!host.IsExists(host.FarmModule.BestMob))
                    host.FarmModule.BestMob = null;
                // if (bestMob != null && (host.farmModule.IsBadTarget(bestMob, tickTime) || host.farmModule.IsImmuneTarget(bestMob) || host.IsInPeaceZone(bestMob)))
                //    bestMob = null;

                switch (FarmState)
                {
                    case FarmState.Disabled:
                        {
                            FightFarmDisabledTick();
                        }
                        break;

                    case FarmState.FarmMobs:
                        FightFarmMobsTick();
                        break;

                    case FarmState.FarmProps:
                        FightFarmPropsTick();
                        break;

                    case FarmState.AttackOnlyAgro:
                        FightAttackOnlyAgroTick();
                        break;
                }
            }
            catch (ThreadAbortException)
            { }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }
        public Stopwatch swUseSkill = new Stopwatch();
        internal virtual bool AttackWithSkill(SkillSettings skill)
        {
            try
            {
                /* if (host.Me.GetAngle(BestMob) > 20 && host.Me.GetAngle(BestMob) < 340)
                     //  {
                     host.TurnDirectly(BestMob);*/

                /*  if (host.Me.ClassType == EClass.Berserker)
                      host.SetCamRotation(host.Me.Rotation.Y);*/
                //   }
                UseSpecialSkills(SpecialDist);
                return host.FarmModule.UseSkillAndWait(skill);
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception e)
            {
                host.log(e.ToString());
                return false;
            }
        }


        public Aura MyGetAura(uint id)
        {
            try
            {
                foreach (var i in host.Me.GetAuras())
                {
                    if (i.SpellId == id)
                        return i;
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                host.log(e.ToString());

            }
            return null;
        }

        public virtual void UseBuffItems()
        {
            try
            {
                foreach (var mybuff in host.CharacterSettings.MyBuffSettings)
                {
                    var aura = MyGetAura(mybuff.SkillId);
                    if (aura == null || aura?.Remaining < 60000)
                    {
                        Item buffItem = null;
                        foreach (var item in host.ItemManager.GetItems())
                        {
                            if (item.Id == mybuff.ItemId)
                            {
                                buffItem = item;
                                break;
                            }
                        }
                        if (buffItem != null)
                        {
                            host.CommonModule.SuspendMove();
                            host.CommonModule.MyUnmount();
                            var result = host.SpellManager.UseItem(buffItem);
                            if (result != EInventoryResult.OK)
                            {
                                host.log("Не смог использовать итем для бафа " + buffItem.Name + "  " + result, Host.LogLvl.Error);
                            }
                            while (host.SpellManager.IsCasting)
                                Thread.Sleep(100);
                            host.CommonModule.ResumeMove();
                        }
                    }
                }


                if (host.GetThreats(host.Me).Count == 0)
                {
                    var auraSkill = MyGetAura(3714);
                    if (auraSkill == null)
                    {
                        Spell buffItem = null;
                        foreach (var item in host.SpellManager.GetSpells())
                        {
                            if (item.Id == 3714)
                            {
                                buffItem = item;
                                break;
                            }
                        }
                        if (buffItem != null)
                        {
                            host.CommonModule.SuspendMove();
                            // host.CommonModule.MyUnmount();
                            while (host.Me.IsMoving)
                            {
                                Thread.Sleep(1000);
                            }
                            var result = host.SpellManager.CastSpell(buffItem.Id);
                            if (result != ESpellCastError.SUCCESS)
                            {
                                host.log("Не смог использовать скилл для бафа " + buffItem.Name + "  " + result, Host.LogLvl.Error);
                            }
                            while (host.SpellManager.IsCasting)
                                Thread.Sleep(100);
                            host.CommonModule.ResumeMove();
                        }
                    }
                }





            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }

        public virtual void UseRegenItems()
        {
            if (host.MyGetAura(269824) != null)//стан
                return;


            if (host.GetThreats().Count == 0 && host.Me.HpPercents < 50)
            {
                var regenSpel = host.SpellManager.GetSpell(18562);
                if (regenSpel != null)
                {
                    if (host.SpellManager.CheckCanCast(regenSpel.Id, host.Me) != ESpellCastError.SUCCESS)
                        return;
                    host.CommonModule.SuspendMove();
                    try
                    {

                        while (host.SpellManager.IsCasting)
                            Thread.Sleep(200);
                        while (host.Me.IsMoving)
                            Thread.Sleep(200);
                        host.CanselForm();
                        host.CommonModule.MyUnmount();
                        while (host.SpellManager.IsCasting)
                            Thread.Sleep(200);
                        while (host.Me.IsMoving)
                            Thread.Sleep(200);
                        var result = host.SpellManager.CastSpell(regenSpel.Id, host.Me);
                        if (result != ESpellCastError.SUCCESS)
                        {
                            host.log("Не смог использовать скилл регена " + result + " " + host.GetLastError(), Host.LogLvl.Error);
                            Thread.Sleep(2000);

                        }
                        while (host.SpellManager.IsCasting)
                            Thread.Sleep(200);
                    }
                    finally
                    {
                        host.CommonModule.ResumeMove();
                    }
                }

            }

            if (host.GetAgroCreatures().Count == 0 && host.Me.HpPercents < 80)
            {
                var regenSpel = host.SpellManager.GetSpell(8936);
                if (regenSpel != null)
                {
                    if (host.SpellManager.CheckCanCast(regenSpel.Id, host.Me) != ESpellCastError.SUCCESS)
                        return;
                    host.CommonModule.SuspendMove();
                    try
                    {                    
                        while (host.SpellManager.IsCasting)
                            Thread.Sleep(200);
                        while (host.Me.IsMoving)
                            Thread.Sleep(200);
                        var result = host.SpellManager.CastSpell(regenSpel.Id, host.Me);
                        if (result != ESpellCastError.SUCCESS)
                        {
                            host.log("Не смог использовать скилл регена " + result + " " + host.GetLastError(), Host.LogLvl.Error);
                            Thread.Sleep(2000);

                        }
                        while (host.SpellManager.IsCasting)
                            Thread.Sleep(200);
                    }
                    finally
                    {
                        host.CommonModule.ResumeMove();
                    }
                }

            }
        }

        private int _failMoveUseSpecialSkill = 0;

        public virtual void UseSpecialSkills(float specialDist)
        {
            try
            {
                if (FarmState == FarmState.FarmProps)
                    return;
                if (host.AutoQuests.BestQuestId == 13523)//:  Власть над приливами[13523
                    return;
                if (host.AutoQuests.BestQuestId == 13576)//:  13576 State:None LogTitle:Взаимная помощь 
                    return;
                if (host.AutoQuests.BestQuestId == 48573)//: :  Жизни кроколисков[48573
                    return;

                if (host.AutoQuests.BestQuestId == 49078 && host.Me.Target?.Id != 128071)
                    return;

                if (SpecialItems != null)
                {
                    for (var i = 0; i < SpecialItems?.Length; i++)
                    {
                        switch (host.AutoQuests.BestQuestId)
                        {
                            case 47130:
                                {
                                    if (host.GetAgroCreatures().Count > 0)
                                        continue;
                                }
                                break;

                            case 49666:
                                {
                                    if (host.Me.Target == null || host.Me.Target.IsAlive)
                                        continue;
                                }
                                break;
                            default:
                                {
                                    if (host.Me.Target == null || !host.Me.Target.IsAlive)
                                        continue;
                                }
                                break;
                        }


                        if (host.Me.Distance(host.Me.Target) > specialDist)
                            if (!host.CommonModule.ForceMoveTo(host.Me.Target, specialDist, specialDist + 2))
                            {
                                if (_failMoveUseSpecialSkill > 4)
                                {
                                    //Плохая цель
                                    host.log("Плохая цель 4 :" + host.FarmModule.BestMob.Name, Host.LogLvl.Error);
                                    host.FarmModule.SetBadTarget(host.FarmModule.BestMob, 60000);
                                    host.FarmModule.BestMob = null;
                                    _failMoveUseSpecialSkill = 0;
                                    // host.SetTarget(null);
                                    return;
                                }
                                _failMoveUseSpecialSkill++;

                                continue;
                            }
                            else
                            {
                                _failMoveUseSpecialSkill = 0;
                            }

                        host.TurnDirectly(host.Me.Target);
                        Item spItem = host.MyGetItem(Convert.ToUInt32(SpecialItems[i]));

                        if (spItem != null)
                        {
                            if (host.SpellManager.GetItemCooldown(spItem.Id) > 0)
                            {
                                /*   if (host.AdvancedLog)
                                       host.log(spItem.Name + " UseSpecialSkills GetItemCooldown:" + host.SpellManager.GetItemCooldown(spItem.Id));*/
                                continue;
                            }
                            else
                            {
                                while (host.Me.IsMoving)
                                    Thread.Sleep(50);
                                while (host.SpellManager.IsCasting)
                                    Thread.Sleep(50);
                                while (host.SpellManager.IsChanneling)
                                    Thread.Sleep(50);

                                Thread.Sleep(500);
                                EInventoryResult result;
                                switch (spItem.Id)
                                {
                                    case 153012:
                                        {
                                            result = host.SpellManager.UseItem(spItem, host.Me.Target);
                                        }
                                        break;
                                    default:
                                        {
                                            result = host.SpellManager.UseItem(spItem);
                                        }
                                        break;
                                }


                                if (result == EInventoryResult.OK)
                                {
                                    host.log("Использую " + spItem.Name + "[" + spItem.Id + "] " + spItem.Place,
                                        Host.LogLvl.Ok);
                                    host.FarmModule.SetBadTarget(host.FarmModule.BestMob, 120000);
                                    host.FarmModule.BestMob = null;
                                }

                                else
                                {
                                    host.log("Не получилось использовать " + spItem.Name + "[" + SpecialItems[i] + "]  " + result + "  " + host.GetLastError() + "  " + host.FarmModule.BestMob.Guid, Host.LogLvl.Error);
                                    host.FarmModule.SetBadTarget(host.FarmModule.BestMob, 120000);
                                    host.FarmModule.BestMob = null;
                                }

                                Thread.Sleep(1000);
                                while (host.Me.IsMoving)
                                    Thread.Sleep(50);
                                while (host.SpellManager.IsCasting)
                                    Thread.Sleep(50);
                                while (host.SpellManager.IsChanneling)
                                    Thread.Sleep(50);
                            }
                        }
                        else
                        {
                            host.log("Не нашел указанный айтем " + SpecialItems[i]);
                        }




                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }



        public virtual bool UseSkills()
        {
            return false;
        }

        public int MobsWithDropCount()
        {
            var result = 0;
            try
            {
                if (!host.CharacterSettings.PickUpLoot)
                    return 0;
                foreach (var m in host.GetEntities<Unit>())
                {

                    if (!m.IsLootable)
                        continue;
                    if (host.Me.Distance(m) > 55)
                        continue;
                    if (host.GetVar(m, "pickFailed") == null)
                        result++;
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
            return result;
        }

        public int MobsWithSkinCount()
        {
            var result = 0;
            try
            {
                if (!host.CharacterSettings.Skinning)
                    return 0;

                if (host.SpellManager.GetSpell(8613) == null &&
                    host.SpellManager.GetSpell(8617) == null
                    )
                    return 0;

                foreach (var m in host.GetEntities<Unit>())
                {

                    if (!m.IsSkinnable)
                        continue;
                    if (host.Me.Distance(m) > 55)
                        continue;
                    if (host.GetVar(m, "skinFailed") == null)
                        result++;
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
            return result;
        }

        public virtual void PickupSkinRoute()
        {
            try
            {
                if (!host.CharacterSettings.Skinning)
                    return;

                /*   uint skillSkinning = 0;
                   if (host.SpellManager.GetSpell(8613) != null)
                   {
                       skillSkinning = 8613;
                   }
                   if (host.SpellManager.GetSpell(8617) != null)
                   {
                       skillSkinning = 8617;
                   }

                   if (skillSkinning == 0)
                       return;*/

                if (!host.Me.IsAlive)
                    return;
                var mobsWithDropLoot = new List<Unit>();
                if (host.GetAgroCreatures().Count == 0 && host.IsAlive())
                {
                    foreach (var m in host.GetEntities<Unit>())
                    {
                        if (!host.IsExists(m))
                            continue;
                        if (host.Me.Distance(m) > 55)
                            continue;
                        if (!m.IsSkinnable)
                            continue;
                        /*   if (host.CharacterSettings.UseFilterDrop) //Подбирать только из списка
                               if (!host.CharacterSettings.DroplistInt.Contains(m.Id))
                                   continue;

                           if (host.CharacterSettings.UseFilterNotDrop) //Не подбирать из списка
                               if (host.CharacterSettings.DroplistInt.Contains(m.Id))
                                   continue;*/
                        if (host.SpellManager.GetGatheringSpell(m) == null)
                            continue;


                        if (host.GetVar(m, "skinFailed") == null)
                            mobsWithDropLoot.Add(m);
                    }

                    if (mobsWithDropLoot.Count > 0)
                        host.CommonModule.SuspendMove();

                    foreach (var m in mobsWithDropLoot.OrderBy(i => host.Me.Distance(i)))
                    {
                        if (!host.MainForm.On)
                            return;
                        //  host.log("Come to " + m.Name + " for drop pickup.");
                        if (!host.IsExists(m))
                            continue;
                        if (host.Me.Target != m)
                            host.SetTarget(m);
                        if (!host.CommonModule.ForceMoveTo(m, 2, 0.5, false))
                            if (!host.CommonModule.ForceMoveTo(m, 2, 0.5, false))
                            {
                                host.SetVar(m, "skinFailed", true);
                                continue;
                            }

                        host.CommonModule.MyUnmount();

                        while (host.Me.IsMoving)
                            Thread.Sleep(10);

                        if (!host.IsExists(m))
                            continue;

                        var skillSkinning = host.SpellManager.GetGatheringSpell(m);

                        if (!host.SpellManager.IsSpellReady(skillSkinning))
                        {
                            host.log("Шкуродер не готов " + skillSkinning);
                            return;
                        }
                        if (host.GetAgroCreatures().Count != 0)
                            return;
                        var result = host.SpellManager.CastSpell(skillSkinning.Id);
                        Thread.Sleep(500);

                        if (result != ESpellCastError.SUCCESS)//8613 Skinning
                        {
                            /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                             if (!m.PickUp())
                             {*/

                            host.log("Не смог снять шкуру скилл: " + skillSkinning.Name + "[" + skillSkinning.Id + "] Dist: " + host.Me.Distance(m) + "  Name: " + m.Name + "[" + m.Id + "]   " + host.GetLastError() + "  " + result, Host.LogLvl.Error);
                            host.SetVar(m, "skinFailed", true);
                            //   }
                        }
                        while (host.SpellManager.IsCasting)
                            Thread.Sleep(100);


                        Thread.Sleep(400);
                        host.SetVar(m, "skinFailed", true);

                        if (host.GetAgroCreatures().Count != 0)
                            return;

                        if (!m.IsLootable)
                            continue;

                        if (!host.CanPickupLoot())
                            if (!host.OpenLoot(m))
                                host.log("Не смог открыть лут " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);

                        if (host.CanPickupLoot())
                        {
                            if (!host.PickupLoot())
                            {
                                /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                                 if (!m.PickUp())
                                 {*/

                                host.log("Не смог поднять дроп " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);
                                host.SetVar(m, "pickFailed", true);
                                //   }
                            }
                            else
                            {
                                if (host.ComboRoute.MobsWithDropCount() < 2)
                                    Thread.Sleep(500);
                            }
                        }
                        else
                        {
                            host.log("Окно лута не открыто 2 " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);
                            host.SetVar(m, "pickFailed", true);
                        }

                        //  Thread.Sleep(1000);
                    }




                    /**/
                }

            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                host.log(err.ToString());
            }
        }

        public virtual void PickupDropRoute()
        {
            try
            {
                if (!host.Me.IsAlive)
                    return;
                if (!host.CharacterSettings.PickUpLoot)
                    return;
                var mobsWithDropLoot = new List<Unit>();
                if (host.GetAgroCreatures().Count == 0 && host.IsAlive())
                {
                    foreach (var m in host.GetEntities<Unit>())
                    {
                        if (!host.IsExists(m))
                            continue;
                        if (host.Me.Distance(m) > 55)
                            continue;
                        if (!m.IsLootable)
                            continue;
                        /*   if (host.CharacterSettings.UseFilterDrop) //Подбирать только из списка
                               if (!host.CharacterSettings.DroplistInt.Contains(m.Id))
                                   continue;

                           if (host.CharacterSettings.UseFilterNotDrop) //Не подбирать из списка
                               if (host.CharacterSettings.DroplistInt.Contains(m.Id))
                                   continue;*/

                        if (host.GetVar(m, "pickFailed") == null)
                            mobsWithDropLoot.Add(m);
                    }

                    if (mobsWithDropLoot.Count > 0)
                        host.CommonModule.SuspendMove();

                    foreach (var m in mobsWithDropLoot.OrderBy(i => host.Me.Distance(i)))
                    {
                        //  host.log("Come to " + m.Name + " for drop pickup.");
                        if (!host.MainForm.On)
                            return;
                        if (!host.IsExists(m))
                            continue;
                        if (!m.IsLootable)
                            continue;
                        if (!host.CommonModule.ForceMoveTo(m, 2, 0.5, false))
                            if (!host.CommonModule.ForceMoveTo(m, 2, 0.5, false))
                            {
                                host.SetVar(m, "pickFailed", true);
                                continue;
                            }


                        while (host.Me.IsMoving)
                            Thread.Sleep(100);

                        if (!host.IsExists(m))
                            continue;


                        if (!m.IsLootable)
                            continue;

                        if (host.GetAgroCreatures().Count != 0)
                            return;
                        if (!host.CanPickupLoot())
                            if (!host.OpenLoot(m))
                                host.log("Не смог открыть лут " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);
                        Thread.Sleep(300);
                        if (host.CanPickupLoot())
                        {
                            if (!host.PickupLoot())
                            {
                                /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                                 if (!m.PickUp())
                                 {*/

                                host.log("Не смог поднять дроп " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);
                                host.SetVar(m, "pickFailed", true);
                                //   }
                            }
                            else
                            {
                                if (host.CharacterSettings.Mode == EMode.Questing && host.AutoQuests.BestQuestId == 13523 && m.Id == 32890)
                                {
                                    var item = host.MyGetItem(44975);
                                    if (item != null)
                                        host.MyUseItemAndWait(item, m);
                                }

                                if (host.CharacterSettings.Mode == EMode.Questing && host.AutoQuests.BestQuestId == 13576 && m.Id == 32999)
                                {
                                    var item = host.MyGetItem(44959);
                                    if (item != null)
                                        host.MyUseItemAndWait(item, m);
                                }

                                if (host.CharacterSettings.Mode == EMode.Questing && host.AutoQuests.BestQuestId == 49666 && m.Id == 134560)
                                {
                                    var item = host.MyGetItem(158884);
                                    if (item != null)
                                        host.MyUseItemAndWait(item, m);
                                }
                                if (host.CharacterSettings.Mode == EMode.Questing && host.AutoQuests.BestQuestId == 49666 && m.Id == 134558)
                                {
                                    var item = host.MyGetItem(158884);
                                    if (item != null)
                                        host.MyUseItemAndWait(item, m);
                                }

                                if (host.CharacterSettings.Mode == EMode.Questing && host.AutoQuests.BestQuestId == 48573 && m.Id == 126723)
                                {
                                    var item = host.MyGetItem(152596);
                                    if (item != null)
                                        host.MyUseItemAndWait(item, m);
                                }

                                if (host.ComboRoute.MobsWithDropCount() < 2)
                                    Thread.Sleep(500);
                            }
                        }
                        else
                        {
                            // host.log("Окно лута не открыто 1 " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);
                            host.SetVar(m, "pickFailed", true);
                        }

                        if (host.GetAgroCreatures().Count != 0)
                            break;
                    }




                    /**/
                }

            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                host.log(err.ToString());
            }
        }


        private int _badBestMobHp;
        public long _timeAttack;
        private WowGuid badSid;

        public virtual void FarmRoute()
        {
            try
            {
                // host.log("FarmRoute");
                if (host.NeedWaitAfterCombat && host.FarmModule.BestMob == null)
                {

                    //host.log("Ожидаю после боя", Host.LogLvl.Important);
                    /* if (host.Me.Class == EClass.Druid)
                     {
                         host.CanselForm();
                     }*/
                    Thread.Sleep(300);
                    host.NeedWaitAfterCombat = false;
                }
                UseRegenItems();
                UseBuffItems();
                PickupDropRoute();
                PickupSkinRoute();
                //  PickupDropRoute();
                if (host.FarmModule.BestMob != null)
                {
                    //  host.log(_timeAttack + "  " + _badBestMobHp + "  " +TickTime + "  " + (TickTime - _timeAttack));
                    if (badSid != host.FarmModule.BestMob.Guid)
                    {
                        badSid = host.FarmModule.BestMob.Guid;
                        _badBestMobHp = host.FarmModule.BestMob.HpPercents;
                        _timeAttack = host.GetUnixTime();
                    }

                    if (_badBestMobHp == 0 && badSid == host.FarmModule.BestMob.Guid)
                    {
                        _badBestMobHp = host.FarmModule.BestMob.HpPercents;
                        _timeAttack = host.GetUnixTime();
                    }

                    if (host.Me.IsMoving || host.CommonModule.InFight())
                    {
                        _badBestMobHp = host.FarmModule.BestMob.HpPercents;
                        _timeAttack = host.GetUnixTime();
                    }




                    if (_badBestMobHp != host.FarmModule.BestMob.HpPercents && badSid == host.FarmModule.BestMob.Guid)
                    {
                        _badBestMobHp = host.FarmModule.BestMob.HpPercents;
                        _timeAttack = host.GetUnixTime();
                    }
                    else
                    {
                        if (_timeAttack + host.CharacterSettings.IgnoreMob < TickTime && host.IsAlive(host.Me))
                        {
                            host.log("Плохая цель :" + host.FarmModule.BestMob.Name, Host.LogLvl.Error);
                            host.FarmModule.SetBadTarget(host.FarmModule.BestMob, 60000);
                            host.CommonModule.ResumeMove();
                            host.FarmModule.BestMob = null;

                        }
                    }
                }
                else
                {
                    _badBestMobHp = 0;
                    _timeAttack = host.GetUnixTime();
                }

                // host.log("ХП "+_badBestMobHp + " время " + _timeAttack + " время2 " + TickTime);

                /* if (host.Me.Target != BestMob && host.IsAlive(BestMob))
                     host.SetTarget(BestMob);*/
                // host.log("FarmRoute2");
                if (host.IsAlive(host.FarmModule.BestMob))
                    UseSkills();

            }
            catch (ThreadAbortException) { }
            catch (Exception error)
            {
                host.log(error.ToString());
            }
        }

        public void GlobalCheckBestMob()
        {
            try
            {
                if (!host.IsExists(host.FarmModule.BestMob))
                    host.FarmModule.BestMob = null;
                if (host.FarmModule.BestMob != null && !host.IsAlive(host.FarmModule.BestMob))
                    host.FarmModule.BestMob = null;
                if (host.FarmModule.BestMob != null && !host.CanAttack(host.FarmModule.BestMob, host.CanSpellAttack))
                    host.FarmModule.BestMob = null;
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                host.log(e.ToString());
            }
        }

        public virtual void FightFarmPropsTick()
        {

        }

        public virtual void FightFarmMobsTick()
        {
        }

        public virtual void FightFarmDisabledTick()
        {

        }

        public virtual void FightAttackOnlyAgroTick()
        {

        }

    }
}
