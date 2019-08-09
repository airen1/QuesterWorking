using System;
using System.Collections.Generic;
using System.Threading;
using WowAI.Modules;
using WowAI.UI;
using WoWBot.Core;


namespace WowAI.ComboRoutes
{
    internal class DefaultComboRoute : ComboRoute
    {
        public DefaultComboRoute(Host host) : base(host)
        {
            DeckSkills = new List<SkillSettings>();
            var copySkillSettings = new List<SkillSettings>(host.CharacterSettings.SkillSettings);


            copySkillSettings.Sort(delegate (SkillSettings ss1, SkillSettings ss2)
            { return ss1.Priority.CompareTo(ss2.Priority); });

            copySkillSettings.Reverse();

            foreach (var i in copySkillSettings)
            {
                // host.log(i.Id.ToString());
                DeckSkills.Add(i);
            }
        }

        public override bool UseSkills()
        {
           // host.log("Атака------------------------------------------------");
            foreach (var i in DeckSkills)
            {
                if (host.IsAlive(host.FarmModule.BestMob))
                {
//host.log(i.Id + "  " + i.Name);
                    swUseSkill.Restart();
                    if (AttackWithSkill(i))
                    {
                        //  swUseSkill.Reset();
                        return true;
                    }
                }
            }


            return base.UseSkills();
        }

        public override void FightFarmPropsTick()
        {
            GlobalCheckBestMob();
            if ((host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && ((host.Me.HpPercents > 75) || (host.GetAgroCreatures().Count > 0)))
                host.FarmModule.BestMob = host.FarmModule.GetBestAgroMob();

            if (host.CommonModule.IsMoveSuspended() && (host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0 && MobsWithSkinCount() == 0)
                host.CommonModule.ResumeMove();

            if (host.FarmModule.BestMob == null)
            {

                for (var i = 0; i < FarmPropIds.Count; i++)
                {

                    if (host.FarmModule.BestMob != null || host.CommonModule.IsMoveSuspended())
                        break;

                    host.FarmModule.BestProp = host.FarmModule.GetNearestPropInZone(FarmPropIds[i], true);


                    if (host.FarmModule.BestProp == null && host.CharacterSettings.Mode == UI.EMode.FarmResource)//"Сбор ресурсов")
                    {
                        host.FarmModule.BestProp = host.FarmModule.GetNearestPropInZone(FarmPropIds[i], false);
                        if (host.FarmModule.BestProp == null)
                            host.log("bestprop = null");
                        else
                            host.log(host.FarmModule.BestProp.Id + "");
                    }

                    if (host.FarmModule.BestProp == null)
                        continue;

                    //Проверяем путь к этому пропу
                    /*  var mobPreventDoodadFarm = host.FarmModule.CheckBestMob(BestProp);
                      if (mobPreventDoodadFarm != null)
                      {
                          BestMob = mobPreventDoodadFarm;
                          break;
                      }*/

                    host.CommonModule.MoveTo(host.FarmModule.BestProp.Location, 3);

                    if (host.FarmModule.BestProp != null && host.FarmModule.BestMob == null)
                    {

                        host.FarmModule.InteractWithProp(host.FarmModule.BestProp);
                    }

                    Thread.Sleep(100);


                    if (SpecialItems != null && host.GetAgroCreatures().Count == 0)
                    {

                        for (var j = 0; j < SpecialItems.Length; j++)
                        {
                            // host.log("teldjf + " + SpecialItems.Length);
                            foreach (var item in host.ItemManager.GetItems())
                            {
                                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                                    item.Place == EItemPlace.InventoryItem)
                                    if (item.Id == SpecialItems[j])
                                    {

                                        host.log("Использую спец итем " + item.Name + " " + item.Place + "  на " + host.FarmModule.BestProp.Guid);
                                        host.MyUseItemAndWait(item, host.FarmModule.BestProp);
                                        host.FarmModule.SetBadProp(host.FarmModule.BestProp, 300000);
                                        /*    var result = host.SpellManager.UseItem(item, host.FarmModule.bestProp);
                                            if (result != EInventoryResult.OK)
                                            {
                                                host.log("Не смог использовать спец итем " + item.Name + "  " + result, Host.LogLvl.Error);
                                                Thread.Sleep(5000);
                                            }*/

                                    }
                            }
                            //todo
                        }
                    }
                }
            }
            FarmRoute();
        }


        public override void FightFarmMobsTick()
        {
            GlobalCheckBestMob();
            //  host.log(host.FarmModule.bestMob?.Name);
            if (host.CommonModule.IsMoveSuspended()
                && (host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob))
                && !host.CommonModule.InFight()
                && host.FarmModule.GetBestAgroMob() == null
                && MobsWithDropCount() == 0
                && MobsWithSkinCount() == 0
                )
                host.CommonModule.ResumeMove();
            if ((host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && ((/*host.Me.MpPercents > 40 &&*/ host.Me.HpPercents > 75) || (host.GetAgroCreatures().Count > 0)))
            {
                if (!host.NeedWaitAfterCombat)
                {
                    host.FarmModule.BestMob = host.FarmModule.GetBestMob(true);

                    if (host.FarmModule.BestMob == null)
                        host.FarmModule.BestMob = host.FarmModule.GetBestMob(false);

                    /*  if (host.FarmModule.bestMob == null)
                          host.log("null");*/

                }

            }
            FarmRoute();
        }


        public override void FightFarmDisabledTick()
        {
            if (host.CharacterSettings.Mode == EMode.Questing)
                return;
            if (host.CharacterSettings.DebuffDeath)
            {
                var debuf = host.MyGetAura(15007);
                if (debuf != null)
                {
                    Thread.Sleep(5000);
                   // host.log("Ожидаю " + debuf.SpellName + " " + debuf.Remaining);
                    return;
                }
            }


            if (host.IsAlive() && !host.Me.IsDeadGhost)
            {
                if (host.CharacterSettings.FightIfHPLess)
                    if (host.Me.HpPercents < host.CharacterSettings.FightIfHPLessCount)
                    {
                        host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                        if (host.GetAgroCreatures().Count > 0)
                        {
                            if (host.Me.MountId != 0)
                                host.CommonModule.MyUnmount();
                        }
                    }


                GlobalCheckBestMob();

                host.FarmModule.BestMob = GetHightPrioritiMob();
                if (host.CharacterSettings.GatherResouceScript && host.CharacterSettings.Mode == UI.EMode.Script && host.AutoQuests.EnableFarmProp)
                {
                    if (!host.AutoQuests.NeedActionNpcRepair && !host.AutoQuests.NeedActionNpcSell && !host.NeedAuk)
                    {
                        host.FarmModule.BestProp = host.FarmModule.GetBestPropInZone(false);
                        if (host.FarmModule.BestProp != null && host.IsExists(host.FarmModule.BestProp))
                            host.CommonModule.SuspendMove();
                        if (host.FarmModule.BestMob == null && host.FarmModule.BestProp != null)
                        {
                            host.log("Нашел ресурс " + host.FarmModule.BestProp.Name + "  дист " + host.Me.Distance(host.FarmModule.BestProp));
                            if (host.CharacterSettings.FindBestPoint)
                                host.AutoQuests.NeedFindBestPoint = true;
                            host.CancelMoveTo();
                            Thread.Sleep(100);
                            host.CommonModule.ForceMoveTo(host.FarmModule.BestProp.Location, 3);
                            /*  if ((host.FarmModule.bestMob == null || !host.IsAlive(host.FarmModule.bestMob)) && ((host.Me.HpPercents > 20) || (host.GetAgroCreatures().Count > 0)))
                                  host.FarmModule.bestMob = host.FarmModule.GetBestAgroMob();*/

                            host.FarmModule.InteractWithProp(host.FarmModule.BestProp);
                        }
                    }
                    
                    if (host.CommonModule.IsMoveSuspended() /*&& (host.FarmModule.bestMob == null || !host.IsAlive(host.FarmModule.bestMob)) */&& host.FarmModule.BestProp == null /*&& !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0*/)
                        host.CommonModule.ResumeMove();


                }

                //if ((host.FarmModule.bestMob == null || !host.IsAlive(host.FarmModule.bestMob)) && ((/*host.Me.MpPercents > 40 &&*/ host.Me.HpPercents > 75) || (host.GetAgroCreatures().Count > 0)))
                //{

                //    if (!host.NeedWaitAfterCombat)
                //    {
                //        host.FarmModule.bestMob = host.FarmModule.GetBestMob(true);
                //        /*  if (host.FarmModule.bestMob == null)
                //              host.log("null");*/
                //        if (host.FarmModule.bestMob == null)
                //            host.FarmModule.bestMob = host.FarmModule.GetBestMob(false);
                //    }
                //}
            }

            if (host.CommonModule.IsMoveSuspended() && (host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && host.FarmModule.BestProp == null /*&& !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0*/)
                host.CommonModule.ResumeMove();

            FarmRoute();
        }

        public Unit  GetHightPrioritiMob()
        {
            if (host.GetBotLogin() == "deathstar")
            {
                if (host.Me.GetThreats().Count > 1)
                    return null;
            }

            foreach(var unit in host.GetEntities<Unit>())
            {
                var zRange = Math.Abs(host.Me.Location.Z - unit.Location.Z);
                
                if (zRange > host.CharacterSettings.Zrange)
                    continue;


                if (!host.IsAlive(unit))
                    continue;
                if (host.FarmModule.IsBadTarget(unit, host.ComboRoute.TickTime))
                    continue;
                if (host.FarmModule.IsImmuneTarget(unit))
                    continue;

                if (host.ComboRoute.SpecialItems != null)
                {
                    if (host.ComboRoute.SpecialItems?.Length == 0)
                        if (!host.CanAttack(unit, host.CanSpellAttack))
                            continue;
                }
                else
                {
                    if (!host.CanAttack(unit, host.CanSpellAttack))
                        continue;
                }

                foreach (var characterSettingsMobsSetting in host.CharacterSettings.MobsSettings)
                {
                    if (unit.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                        if (characterSettingsMobsSetting.Priority == 2)
                            return unit;
                }


            }
            return null;
        }

        public int IsNeedFight;
        public override void FightAttackOnlyAgroTick()
        {
            GlobalCheckBestMob();


            if (host.CharacterSettings.NoAttackOnMount)
            {
                if (host.Me.MountId == 0)
                {
                    if ((host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && ((/*host.Me.MpPercents > 10 &&*/ host.Me.HpPercents > 20) || (host.GetAgroCreatures().Count > 0)))
                        host.FarmModule.BestMob = host.FarmModule.GetBestAgroMob();
                }
                else
                {
                    if (host.GetAgroCreatures().Count > 0)
                    {
                        if (host.Me.IsMoving)
                            IsNeedFight = 0;
                        else
                            IsNeedFight++;

                        if (IsNeedFight > 50)
                        {
                            host.log("Нет движения, отзываю маунта");
                            host.CancelMoveTo();
                            host.CommonModule.MyUnmount();
                            IsNeedFight = 0;
                        }
                    }


                }
            }
            else
            {
                if ((host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && ((/*host.Me.MpPercents > 10 &&*/ host.Me.HpPercents > 20) || (host.GetAgroCreatures().Count > 0)))
                    host.FarmModule.BestMob = host.FarmModule.GetBestAgroMob();
            }



            if (host.FarmModule.BestMob != null && host.IsExists(host.FarmModule.BestMob) && host.IsAlive(host.FarmModule.BestMob))
                host.CommonModule.SuspendMove();

            if (host.CharacterSettings.GatherResouceScript && host.CharacterSettings.Mode == UI.EMode.Script && host.AutoQuests.EnableFarmProp)
            {
                host.FarmModule.BestProp = host.FarmModule.GetBestPropInZone(false);

                if (host.FarmModule.BestProp != null && host.IsExists(host.FarmModule.BestProp))
                    host.CommonModule.SuspendMove();

                if (host.FarmModule.BestMob == null && host.FarmModule.BestProp != null)
                {
                    host.log("Нашел ресурс " + host.FarmModule.BestProp.Name + "  дист " + host.Me.Distance(host.FarmModule.BestProp));
                    if (host.CharacterSettings.FindBestPoint)
                        host.AutoQuests.NeedFindBestPoint = true;
                    host.CancelMoveTo();
                    Thread.Sleep(100);
                    host.CommonModule.ForceMoveTo(host.FarmModule.BestProp, 3);
                    if ((host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && ((host.Me.HpPercents > 20) || (host.GetAgroCreatures().Count > 0)))
                        host.FarmModule.BestMob = host.FarmModule.GetBestAgroMob();

                    host.FarmModule.InteractWithProp(host.FarmModule.BestProp);
                }

                if (host.CommonModule.IsMoveSuspended() && (host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && host.FarmModule.BestProp == null && !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0)
                    host.CommonModule.ResumeMove();

                /*  if (host.CharacterSettings.NoAttack)
                  {
                      if (host.CommonModule.IsMoveSuspended() && host.FarmModule.bestProp == null && MobsWithDropCount() == 0)
                          host.CommonModule.ResumeMove();
                  }*/
            }

            if (host.CommonModule.IsMoveSuspended() && (host.FarmModule.BestMob == null || !host.IsAlive(host.FarmModule.BestMob)) && !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0 && MobsWithSkinCount() == 0)
                host.CommonModule.ResumeMove();
            FarmRoute();
        }
    }
}
