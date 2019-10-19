using Out.Internal.Core;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WoWBot.Core;

namespace WowAI.Module
{
    internal partial class FarmModule : Module
    {
        public bool EventDeath;
        public bool EventDeathPlayer;
        public bool IsWing1 = true;
        public long TickTime;
        private List<SkillSettings> _deckSkills;
        public int IsNeedFight;
        DateTime _nextUsePottion = DateTime.MinValue;
        private int _attackMoveFailCount;
        DateTime _useQuestSpell = DateTime.MinValue;
        internal bool ReadyToActions
        {
            get
            {
                if (!Host.MainForm.On)
                    return false;
                if (Host.IsAlive(Host.Me) && !Host.Me.IsDeadGhost && !Host.Me.IsInFlight)
                {
                    return true;
                }
                return false;
            }
        }
        public bool RandDirLeft;
        public DateTime NextRandomMovesDirChange = DateTime.UtcNow;
        internal int[] SpecialItems;
        internal float SpecialDist;
        internal int[] SpecialSkills;

        internal Zone FarmZone = new RoundZone(0, 0, 0);
        internal List<uint> FarmMobsIds = new List<uint>();
        internal List<uint> FactionIds = new List<uint>();
        private int _gatherCount;
        private Unit _bestMob;
        internal Unit BestMob
        {
            get
            {
                if (!Host.IsExists(_bestMob))
                    _bestMob = null;
                return _bestMob;
            }
            set
            {
                try
                {
                    if (_bestMob != value)
                    {
                        if (value != null && Host.IsExists(value))
                        {
                            Host.MainForm.SetBestMobText(value.Name + "[" + value.Id + "]");
                            _bestMob = value;
                        }
                        else
                        {
                            _bestMob = null;
                            Host.MainForm.SetBestMobText("");
                        }
                    }
                }
                catch
                {
                    _bestMob = null;
                }
            }
        }

        internal List<uint> FarmPropIds = new List<uint>();
        private GameObject _bestProp;

        internal GameObject BestProp
        {
            get
            {
                if (!Host.IsExists(_bestProp))
                    _bestProp = null;
                return _bestProp;
            }
            set
            {
                try
                {
                    if (_bestProp != value)
                    {
                        if (value != null && Host.IsExists(value))
                        {
                            Host.MainForm.SetBestPropText(value.Name + "[" + value.Id + "]");
                            _bestProp = value;
                        }
                        else
                        {
                            _bestProp = null;
                            Host.MainForm.SetBestPropText("");
                        }
                    }

                }
                catch
                {
                    _bestProp = null;
                }
            }
        }
        private int _failMoveUseSpecialSkill;
        private FarmState _farmState;
        internal FarmState FarmState
        {
            get => _farmState;
            set
            {
                _farmState = value;
                Host.MainForm.SetFarmModuleText(_farmState.ToString());
                var s = "";
                foreach (var farmMobsId in FarmMobsIds)
                {
                    s += farmMobsId + Environment.NewLine;
                }
                foreach (var farmMobsId in FarmPropIds)
                {
                    s += farmMobsId + Environment.NewLine;
                }
                Host.MainForm.SetFarmModuleToolTipt(s);
            }
        }


        public override void Start(Host host)
        {
            base.Start(host);
            FarmState = FarmState.Disabled;
            if (host.CharacterSettings.Mode == Mode.Questing)
                FarmState = FarmState.AttackOnlyAgro;
            if (host.ClientType == EWoWClient.Classic)
                FarmState = FarmState.AttackOnlyAgro;
            LoadSkill();
        }

        public void LoadSkill()
        {
            _deckSkills = new List<SkillSettings>();
            var copySkillSettings = new List<SkillSettings>(Host.CharacterSettings.SkillSettings);
            copySkillSettings.Sort(delegate (SkillSettings ss1, SkillSettings ss2)
                { return ss1.Priority.CompareTo(ss2.Priority); });
            copySkillSettings.Reverse();
            foreach (var i in copySkillSettings)
                _deckSkills.Add(i);
        }

        public override void Stop()
        {
            Host.CancelMoveTo();
            base.Stop();
        }

        public override void Run(CancellationToken ct)
        {
            try
            {
                while (!Host.CancelRequested && !ct.IsCancellationRequested)
                {
                    base.Run(ct);
                    Thread.Sleep(100);
                    if (Host.MainForm.On && Host.GameState == EGameState.Ingame)
                    {
                        PrepareForFight();
                        if (ReadyToActions)
                            Fight();
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception error) { Host.log(error.ToString()); }
            finally { Host.log("FarmModule Stop"); }
        }


        private void Fight()
        {
            try
            {
                TickTime = Host.GetUnixTime();
                if (!Host.IsExists(Host.FarmModule.BestMob))
                    Host.FarmModule.BestMob = null;

                switch (FarmState)
                {
                    case FarmState.Disabled:
                        FightFarmDisabledTick();
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
                Host.log(e.ToString());
            }
        }

        private void FightFarmPropsTick()
        {
            GlobalCheckBestMob();
            if ((Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && ((Host.Me.HpPercents > 75) || (Host.GetAgroCreatures().Count > 0)))
                Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();

            if (Host.CommonModule.IsMoveSuspended() && (Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && !Host.CommonModule.InFight() && Host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0 && MobsWithSkinCount() == 0)
                Host.CommonModule.ResumeMove();

            if (Host.FarmModule.BestMob == null)
            {
                for (var i = 0; i < FarmPropIds.Count; i++)
                {
                    if (Host.MyIsNeedRegen())
                        break;

                    if (Host.FarmModule.BestMob != null || Host.CommonModule.IsMoveSuspended())
                        break;

                    Host.FarmModule.BestProp = Host.FarmModule.GetNearestPropInZone(FarmPropIds[i], true);


                    if (Host.FarmModule.BestProp == null && Host.CharacterSettings.Mode == Mode.FarmResource)//"Сбор ресурсов")
                    {
                        Host.FarmModule.BestProp = Host.FarmModule.GetNearestPropInZone(FarmPropIds[i], false);
                        if (Host.FarmModule.BestProp == null)
                            Host.log("bestprop = null");
                        else
                            Host.log(Host.FarmModule.BestProp.Id + "");
                    }

                    if (Host.FarmModule.BestProp == null)
                        continue;

                    Host.CommonModule.MoveTo(Host.FarmModule.BestProp.Location, 3);

                    if (Host.FarmModule.BestProp != null && Host.FarmModule.BestMob == null)
                    {
                        var needWait = false;
                        if (Host.FarmModule.BestProp.Id == 128308 || Host.FarmModule.BestProp.Id == 128403)
                        {
                            needWait = true;
                        }
                        Host.FarmModule.InteractWithProp(Host.FarmModule.BestProp);
                        if (needWait)
                            Thread.Sleep(3000);
                    }

                    Thread.Sleep(100);


                    if (SpecialItems != null && Host.GetAgroCreatures().Count == 0)
                    {

                        for (var j = 0; j < SpecialItems.Length; j++)
                        {
                            // host.log("teldjf + " + SpecialItems.Length);
                            foreach (var item in Host.ItemManager.GetItems())
                            {
                                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                                    item.Place == EItemPlace.InventoryItem)
                                    if (item.Id == SpecialItems[j])
                                    {

                                        Host.log("Использую спец итем " + item.Name + " " + item.Place + "  на " + Host.FarmModule.BestProp.Guid);
                                        Host.MyUseItemAndWait(item, Host.FarmModule.BestProp);
                                        Host.FarmModule.SetBadProp(Host.FarmModule.BestProp, 60000);
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

        private void FightFarmMobsTick()
        {
            GlobalCheckBestMob();
            //  host.log(host.FarmModule.bestMob?.Name);
            if (Host.CommonModule.IsMoveSuspended()
                && (Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob))
                && !Host.CommonModule.InFight()
                && Host.FarmModule.GetBestAgroMob() == null
                && MobsWithDropCount() == 0
                && MobsWithSkinCount() == 0)
                Host.CommonModule.ResumeMove();


            if ((Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob))
                && (!Host.MyIsNeedRegen() || Host.GetAgroCreatures().Count > 0)
                && (MobsWithDropCount() + MobsWithSkinCount()) == 0 || Host.GetAgroCreatures().Count > 0)
            {
                if (!Host.NeedWaitAfterCombat)
                {
                    Host.FarmModule.BestMob = Host.FarmModule.GetBestMob(true);
                    if (Host.FarmModule.BestMob == null)
                        Host.FarmModule.BestMob = Host.FarmModule.GetBestMob();
                    /*  if (host.FarmModule.bestMob == null)
                          host.log("null");*/
                }

            }
            FarmRoute();
        }

        private void FightFarmDisabledTick()
        {
            Host.CallPet();
            if (Host.CharacterSettings.Mode == Mode.Questing)
                return;
            if (Host.CharacterSettings.Mode == Mode.QuestingClassic)
                return;
            if (Host.CharacterSettings.DebuffDeath)
            {
                var debuf = Host.MyGetAura(15007);
                if (debuf != null)
                {
                    Thread.Sleep(5000);
                    // host.log("Ожидаю " + debuf.SpellName + " " + debuf.Remaining);
                    return;
                }
            }


            if (Host.IsAlive() && !Host.Me.IsDeadGhost)
            {
                if (Host.CharacterSettings.FightIfHPLess)
                    if (Host.Me.HpPercents < Host.CharacterSettings.FightIfHPLessCount)
                    {
                        Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        if (Host.GetAgroCreatures().Count > 0)
                        {
                            if (Host.Me.MountId != 0)
                                Host.CommonModule.MyUnmount();
                        }
                    }


                GlobalCheckBestMob();

                if (Host.CharacterSettings.GatherResouceScript && Host.CharacterSettings.Mode == Mode.Script && Host.AutoQuests.EnableFarmProp)
                {
                    if (!Host.AutoQuests.NeedActionNpcRepair && !Host.AutoQuests.NeedActionNpcSell && !Host.NeedAuk)
                    {
                        Host.FarmModule.BestProp = Host.FarmModule.GetBestPropInZone(false);
                        if (Host.FarmModule.BestProp != null && Host.IsExists(Host.FarmModule.BestProp))
                            Host.CommonModule.SuspendMove();
                        if (Host.FarmModule.BestMob == null && Host.FarmModule.BestProp != null)
                        {
                            Host.log("Нашел ресурс " + Host.FarmModule.BestProp.Name + "  дист " + Host.Me.Distance(Host.FarmModule.BestProp));
                            if (Host.CharacterSettings.FindBestPoint)
                                Host.AutoQuests.NeedFindBestPoint = true;
                            Host.CancelMoveTo();
                            Thread.Sleep(100);
                            Host.CommonModule.ForceMoveTo(Host.FarmModule.BestProp, 3);
                            /*  if ((host.FarmModule.bestMob == null || !host.IsAlive(host.FarmModule.bestMob)) && ((host.Me.HpPercents > 20) || (host.GetAgroCreatures().Count > 0)))
                                  host.FarmModule.bestMob = host.FarmModule.GetBestAgroMob();*/

                            Host.FarmModule.InteractWithProp(Host.FarmModule.BestProp);
                        }
                    }

                    if (Host.CommonModule.IsMoveSuspended() /*&& (host.FarmModule.bestMob == null || !host.IsAlive(host.FarmModule.bestMob)) */&& Host.FarmModule.BestProp == null /*&& !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0*/)
                        Host.CommonModule.ResumeMove();


                }


                if (Host.FarmModule.BestMob != null && Host.CharacterSettings.Mode == Mode.Script)
                {
                    if (Host.CheckPikPocket())
                    {
                        // host.ObstaclePic();
                        Host.log("Бегу к мобу");
                        Host.CommonModule.SuspendMove();
                        Host.SetTarget(Host.FarmModule.BestMob);

                        var safePoint = new List<Vector3F>();
                        var xc = Host.FarmModule.BestMob.Location.X;
                        var yc = Host.FarmModule.BestMob.Location.Y;

                        var radius = 3;
                        var bestPoint = new Vector3F(Vector3F.Zero);
                        double bestDist = 99999;
                        const double a = Math.PI / 16;
                        double u = 0;
                        for (var i = 0; i < 32; i++)
                        {
                            var x1 = xc + radius * Math.Cos(u);
                            var y1 = yc + radius * Math.Sin(u);
                            // log(" " + i + " x:" + x + " y:" + y);
                            u += a;

                            var z1 = Host.FarmModule.BestMob.Location.Z;
                            var next = false;
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (entity == Host.FarmModule.BestMob)
                                    continue;
                                if (entity == Host.Me)
                                    continue;

                                if (entity.Distance(x1, y1, z1) < 3)
                                    next = true;
                            }

                            if (next)
                                continue;
                            if (Host.IsInsideNavMesh(new Vector3F(x1, y1, z1)))
                            {
                                var vector = new Vector3F(x1, y1, z1);
                                if (Host.CommonModule.GetPath(Host.Me.Location, vector) < bestDist)
                                {
                                    bestDist = Host.CommonModule.GetPath(Host.Me.Location, vector);
                                    bestPoint = vector;
                                    safePoint.Add(new Vector3F(x1, y1, z1));
                                }
                            }
                        }

                        Host.log("Пытаюсь обойти препядствие " + safePoint.Count);
                        if (safePoint.Count > 0)
                        {
                            // var bestPoint = safePoint[host.RandGenerator.Next(safePoint.Count)];
                            Host.log("Бегу в " + bestPoint + " " + Host.IsInsideNavMesh(bestPoint));
                            if (!Host.CommonModule.ForceMoveTo(bestPoint))
                                return;
                        }
                        else
                        {
                            if (!Host.CommonModule.ForceMoveTo(Host.FarmModule.BestMob, 4.5))
                                return;
                        }


                        Host.MyCheckIsMovingIsCasting();
                        var res = Host.SpellManager.CastSpell(921, Host.FarmModule.BestMob);
                        Host.KillMobsCount++;
                        if (res != ESpellCastError.SUCCESS)
                        {
                            Host.log("Не удалось обокрасть " + res + " " + Host.GetLastError(), LogLvl.Error);
                            if (res == ESpellCastError.OUT_OF_RANGE)
                            {
                                if (!Host.CommonModule.ForceMoveTo(Host.FarmModule.BestMob, 4.2))
                                    return;
                                Host.MyCheckIsMovingIsCasting();
                                res = Host.SpellManager.CastSpell(921, Host.FarmModule.BestMob);
                                if (res != ESpellCastError.SUCCESS)
                                    Host.log("Не удалось обокрасть 2 " + res + " " + Host.GetLastError(), LogLvl.Error);
                            }
                        }
                        var waitTill = DateTime.UtcNow.AddSeconds(1);
                        while (waitTill > DateTime.UtcNow)
                        {
                            Thread.Sleep(100);
                            if (!Host.MainForm.On)
                                return;
                            if (Host.CanPickupLoot())
                                break;
                        }
                        if (Host.CanPickupLoot())
                            Host.PickupLoot();
                        // host.FarmModule.SetBadTarget(host.FarmModule.BestMob, 360000);
                        if (Host.ListGuidPic.ContainsKey(Host.FarmModule.BestMob.Guid))
                        {
                            Host.ListGuidPic[Host.FarmModule.BestMob.Guid] = DateTime.Now.AddMinutes(6);
                        }
                        else
                        {
                            Host.ListGuidPic.Add(Host.FarmModule.BestMob.Guid, DateTime.Now.AddMinutes(6));
                        }
                        Host.FarmModule.BestMob = null;
                        Host.CommonModule.ResumeMove();
                    }


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

            if (Host.CommonModule.IsMoveSuspended() && (Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && Host.FarmModule.BestProp == null /*&& !host.CommonModule.InFight() && host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0*/)
                Host.CommonModule.ResumeMove();
            if (Host.ClientType == EWoWClient.Retail)
                FarmRoute();
        }

        private void FightAttackOnlyAgroTick()
        {
            GlobalCheckBestMob();


            if (Host.CharacterSettings.NoAttackOnMount)
            {
                if (Host.Me.MountId == 0)
                {
                    if ((Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && (!Host.MyIsNeedRegen() || (Host.GetAgroCreatures().Count > 0)))
                        Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();
                }
                else
                {
                    if (Host.GetAgroCreatures().Count > 0)
                    {
                        if (Host.Me.IsMoving)
                            IsNeedFight = 0;
                        else
                            IsNeedFight++;

                        if (IsNeedFight > 50)
                        {
                            Host.log("Нет движения, отзываю маунта");
                            Host.CancelMoveTo();
                            Host.CommonModule.MyUnmount();
                            IsNeedFight = 0;
                        }
                    }


                }
            }
            else
            {
                if ((Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && (!Host.MyIsNeedRegen() || (Host.GetAgroCreatures().Count > 0)))
                    Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();
            }



            if (Host.FarmModule.BestMob != null && Host.IsExists(Host.FarmModule.BestMob) && Host.IsAlive(Host.FarmModule.BestMob))
                Host.CommonModule.SuspendMove();

            if (Host.CharacterSettings.GatherResouceScript && Host.CharacterSettings.Mode == Mode.Script && Host.AutoQuests.EnableFarmProp)
            {
                Host.FarmModule.BestProp = Host.FarmModule.GetBestPropInZone(false);

                if (Host.FarmModule.BestProp != null && Host.IsExists(Host.FarmModule.BestProp))
                    Host.CommonModule.SuspendMove();

                if (Host.FarmModule.BestMob == null && Host.FarmModule.BestProp != null)
                {
                    Host.log("Нашел ресурс " + Host.FarmModule.BestProp.Name + "  дист " + Host.Me.Distance(Host.FarmModule.BestProp));
                    if (Host.CharacterSettings.FindBestPoint)
                        Host.AutoQuests.NeedFindBestPoint = true;
                    Host.CancelMoveTo();
                    Thread.Sleep(100);
                    Host.CommonModule.ForceMoveTo(Host.FarmModule.BestProp, 3);
                    if ((Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && (!Host.MyIsNeedRegen() || (Host.GetAgroCreatures().Count > 0)))
                        Host.FarmModule.BestMob = Host.FarmModule.GetBestAgroMob();

                    Host.FarmModule.InteractWithProp(Host.FarmModule.BestProp);
                }

                if (Host.CommonModule.IsMoveSuspended() && (Host.FarmModule.BestMob == null
                                                            || !Host.IsAlive(Host.FarmModule.BestMob)) && Host.FarmModule.BestProp == null && !Host.CommonModule.InFight() && Host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0)
                    Host.CommonModule.ResumeMove();

            }

            if (Host.CommonModule.IsMoveSuspended() && (Host.FarmModule.BestMob == null || !Host.IsAlive(Host.FarmModule.BestMob)) && !Host.CommonModule.InFight() && Host.FarmModule.GetBestAgroMob() == null && MobsWithDropCount() == 0 && MobsWithSkinCount() == 0)
                Host.CommonModule.ResumeMove();
            FarmRoute();
        }

        private void FarmRoute()
        {
            try
            {
                if (Host.CharacterSettings.RunRun || Host.CharacterSettings.Mode == Mode.QuestingClassic)
                {
                    if (Host.GetChanceDeath() > Host.CharacterSettings.RunRunPercent)
                    {
                        double bestDist = 99999999;
                        Vector3F bestPoint = Vector3F.Zero;
                        foreach (var myNpcLoc in Host.MyNpcLocss.NpcLocs)
                        {
                            if (!myNpcLoc.IsTaxy && !myNpcLoc.IsArmorer)
                                continue;
                            foreach (var vector3F in myNpcLoc.ListLoc)
                            {
                                if (Host.Me.Distance(vector3F) < bestDist)
                                {
                                    bestDist = Host.Me.Distance(bestPoint);
                                    bestPoint = vector3F;
                                }
                            }
                        }

                        if (bestPoint != Vector3F.Zero)
                        {
                            Host.log("Сваливаю " + Host.Me.Distance(bestPoint));
                            Host.FarmModule.BestMob = null;
                            Host.FarmModule.FarmState = FarmState.Disabled;
                            Host.RunRun = true;
                            Host.CommonModule.ForceMoveTo(bestPoint);
                            Host.FarmModule.FarmState = FarmState.AttackOnlyAgro;
                            return;
                        }
                    }
                }

                if (Host.NeedWaitAfterCombat && Host.FarmModule.BestMob == null)
                {

                    var waitTill = DateTime.UtcNow.AddSeconds(2);
                    while (waitTill > DateTime.UtcNow)
                    {
                        Thread.Sleep(100);
                        Host.log("Ожидаю после боя " + MobsWithDropCount() + " " + Host.GetAgroCreatures().Count, LogLvl.Important);
                        if (!Host.MainForm.On)
                            return;
                        if (MobsWithDropCount() > 0)
                            break;
                        if (Host.GetAgroCreatures().Count > 0)
                            break;
                    }
                    Host.NeedWaitAfterCombat = false;
                }

                Host.CallPet();
                Host.FoodPet();
                PickupDropRoute();
                PickupSkinRoute();
                MyUsePossion();
                UseBuffItems();



                // host.log("ХП "+_badBestMobHp + " время " + _timeAttack + " время2 " + TickTime);
                UseSkills();
            }
            catch (ThreadAbortException) { }
            catch (Exception error)
            {
                Host.log(error.ToString());
            }
        }

        private Item MyGetPossionItem()
        {
            foreach (var item in Host.ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 || item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 || item.Place == EItemPlace.InventoryItem)
                {
                    if (item.Id == 6947 || item.Id == 6949 || item.Id == 6950 || item.Id == 8926 || item.Id == 8927 || item.Id == 8928)
                        return item;
                }

            }
            return null;
            // return Host.ItemManager.GetItems().FirstOrDefault(item => item.Id == 6947 && item.Place >= EItemPlace.InventoryItem && item.Place <= EItemPlace.Bag4);
        }

        public void MyUsePossion()
        {

            if (Host.Me.IsInCombat)
                return;
            if (Host.Me.Class != EClass.Rogue)
                return;
            var weaponMainHand = Host.ItemManager.GetItems().FirstOrDefault(item => item.Place == EItemPlace.Equipment && item.Cell == (byte)EEquipmentSlot.MainHand);
            var weaponOffHand = Host.ItemManager.GetItems().FirstOrDefault(item => item.Place == EItemPlace.Equipment && item.Cell == (byte)EEquipmentSlot.OffHand);
            if (weaponMainHand != null)
            {
                var enchId = weaponMainHand.GetEnchantmentId(EEnchantmentSlot.Temp);
                var enchCharges = weaponMainHand.GetEnchantmentCharges(EEnchantmentSlot.Temp);
                if (enchId != 0)
                    return;

                var itemToCast = MyGetPossionItem();
                if (itemToCast != null)
                {
                    Host.CommonModule.SuspendMove();
                    Host.MyCheckIsMovingIsCasting();
                    var res = Host.SpellManager.UseItem(itemToCast, weaponMainHand);
                    while (Host.Me.IsCasting)
                    {
                        Thread.Sleep(100);
                    }
                    if (res != EInventoryResult.OK)
                    {
                        Host.log("Не удалось использовать яд " + itemToCast.Name + " " + res + " " + Host.GetLastError(), LogLvl.Error);
                    }
                    Host.CommonModule.ResumeMove();
                }
            }

            if (weaponOffHand != null)
            {
                var enchId = weaponOffHand.GetEnchantmentId(EEnchantmentSlot.Temp);
                var enchCharges = weaponOffHand.GetEnchantmentCharges(EEnchantmentSlot.Temp);
                if (enchId != 0)
                    return;

                var itemToCast = MyGetPossionItem();
                if (itemToCast != null)
                {
                    Host.CommonModule.SuspendMove();
                    Host.MyCheckIsMovingIsCasting();
                    var res = Host.SpellManager.UseItem(itemToCast, weaponOffHand);
                    while (Host.Me.IsCasting)
                    {
                        Thread.Sleep(100);
                    }
                    if (res != EInventoryResult.OK)
                    {
                        Host.log("Не удалось использовать яд " + itemToCast.Name + " " + res + " " + Host.GetLastError(), LogLvl.Error);
                    }
                    Host.CommonModule.ResumeMove();
                }
            }



        }

        private void GlobalCheckBestMob()
        {
            try
            {
                if (!Host.IsExists(Host.FarmModule.BestMob))
                    Host.FarmModule.BestMob = null;
                if (Host.FarmModule.BestMob != null && !Host.IsAlive(Host.FarmModule.BestMob))
                    Host.FarmModule.BestMob = null;
                if (Host.FarmModule.BestMob != null && !Host.CanAttack(Host.FarmModule.BestMob, Host.CanSpellAttack))
                    Host.FarmModule.BestMob = null;
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        private void UseRegenSpell(Spell regenSpel)
        {
            if (regenSpel != null)
            {
                Host.CommonModule.SuspendMove();
                if (Host.SpellManager.CheckCanCast(regenSpel.Id, Host.Me) != ESpellCastError.SUCCESS)
                    return;
                try
                {

                    Host.MyCheckIsMovingIsCasting();
                    Host.CanselForm();
                    Host.CommonModule.MyUnmount();
                    Host.MyCheckIsMovingIsCasting();
                    var result = Host.SpellManager.CastSpell(regenSpel.Id, Host.Me);
                    if (result != ESpellCastError.SUCCESS)
                    {
                        Host.log("Не смог использовать скилл регена " + result + " " + Host.GetLastError(), LogLvl.Error);
                        Thread.Sleep(2000);

                    }
                    while (Host.SpellManager.IsCasting)
                        Thread.Sleep(200);
                }
                finally
                {
                    Host.CommonModule.ResumeMove();
                }
            }
        }

        private void UseBuffItems()
        {
            try
            {
                if (!Host.Me.IsAlive)
                    return;
                if (Host.Me.IsDeadGhost)
                    return;
                if (Host.CheckUnderWather())
                    return;
                if (Host.MyGetAura(269824) != null)//стан
                    return;

                foreach (var regenItemse in Host.CharacterSettings.RegenItemses)
                {
                    if (Host.Me.IsInCombat && !regenItemse.InFight)
                        continue;

                    if (Host.MeMpPercent() <= regenItemse.MeMinMp || Host.MeMpPercent() > regenItemse.MeMaxMp)
                    {
                        //  Host.log("MP персонажа не соответсвует условиям [" + characterSettingsRegenSkillSettingse.ItemId + "] " + characterSettingsRegenSkillSettingse.Name);
                        continue;
                    }

                    if (Host.Me.HpPercents < regenItemse.MeMinHp || Host.Me.HpPercents > regenItemse.MeMaxHp)
                    {
                        // host.log("HP персонажа не соответсвует условиям [" + characterSettingsRegenSkillSettingse.ItemId + "] " + characterSettingsRegenSkillSettingse.Name);
                        continue;
                    }

                    var findEffect = false;
                    if (regenItemse.NotMeEffect != 0)
                    {
                        foreach (var abnormalStatuse in Host.Me.GetAuras())
                            if (abnormalStatuse.SpellId == regenItemse.NotMeEffect)
                                findEffect = true;

                        if (findEffect)
                        {
                            // host.log("Нашел эффект на персонаже " + characterSettingsRegenSkillSettingse.NotMeEffect);
                            continue;
                        }

                    }

                    if (regenItemse.IsMeEffect != 0)
                    {
                        foreach (var abnormalStatuse in Host.Me.GetAuras())
                            if (abnormalStatuse.SpellId == regenItemse.IsMeEffect)
                            {
                                findEffect = true;
                                //  Host.log("нашел ", Host.LogLvl.Ok);
                            }

                        if (!findEffect)
                        {
                            //  host.log("Не нашел эффект на персонаже " + characterSettingsRegenSkillSettingse.IsMeEffect);
                            continue;
                        }

                    }

                    var aura = Host.MyGetAura(regenItemse.SpellId);
                    if (aura != null && aura.Remaining >= 60000)
                        continue;

                    var buffItem = Host.MyGetItem(regenItemse.ItemId);

                    if (buffItem == null)
                        continue;
                    if (Host.SpellManager.GetItemCooldown(buffItem.Id) > 0)
                        continue;
                    Host.CommonModule.SuspendMove();
                    Host.MyUseItemAndWait(buffItem, Host.Me);
                    aura = Host.MyGetAura(regenItemse.SpellId);
                    if (aura != null && aura.Remaining < 30000)
                    {
                        while (true)
                        {
                            Thread.Sleep(1000);
                            if (!Host.MainForm.On)
                                break;
                            aura = Host.MyGetAura(regenItemse.SpellId);
                            if (aura == null)
                                break;
                        }
                    }

                    if (Host.Me.StandState == EStandState.Sit)
                    {
                        Host.ChangeStandState(EStandState.Stand);
                        // Host.MoveTo(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z);
                    }

                    Host.CommonModule.ResumeMove();
                    Thread.Sleep(1000);
                }

                if (Host.CommonModule.InFight() && Host.Me.HpPercents < 50 && !Host.Me.IsMounted)
                {

                    if (_nextUsePottion > DateTime.UtcNow)
                    {

                    }
                    else
                    {
                        _nextUsePottion = DateTime.UtcNow.AddSeconds(Host.RandGenerator.Next(5, 15));
                        Item buffItem = null;
                        foreach (var item in Host.ItemManager.GetItems())
                        {
                            if (item.Id == 152494)
                            {
                                buffItem = item;
                                break;
                            }
                        }
                        if (buffItem != null && Host.SpellManager.GetItemCooldown(buffItem) == 0)
                        {
                            Host.CommonModule.SuspendMove();

                            var isNeedUnmount = true;

                            if (buffItem.Id == 152813)
                                isNeedUnmount = false;

                            if (isNeedUnmount)
                                Host.CommonModule.MyUnmount();

                            var result = Host.SpellManager.UseItem(buffItem);
                            if (result != EInventoryResult.OK)
                            {
                                Host.log("Не смог использовать итем для регена " + buffItem.Name + "  " + result, LogLvl.Error);
                            }
                            while (Host.SpellManager.IsCasting)
                                Thread.Sleep(100);
                            Host.CommonModule.ResumeMove();
                        }
                    }

                }

                if (!Host.Me.IsInCombat)
                {
                    if (Host.Me.Class == EClass.Mage && Host.CharacterSettings.CraftConjured)
                    {
                        var listFood = new Dictionary<uint, uint>
                        {
                            {190336, 113509},
                            {6129, 1487},
                            {990, 1114},
                            {597, 1113},
                            {587, 5349},

                        };
                        var listWater = new Dictionary<uint, uint>
                        {
                            {10138, 8077},
                            {6127, 3772},
                            {5506, 2136},
                            {5505, 2288},
                            {5504, 5350},

                        };

                        if (Host.MeMpPercent() < Host.CharacterSettings.CraftConjuredMp)
                        {
                            foreach (var u in listWater)
                            {
                                if (Host.SpellManager.GetSpell(u.Key) == null)
                                    continue;
                                Host.CommonModule.SuspendMove();
                                try
                                {
                                    if (Host.MeGetItemsCount(u.Value) < 2)
                                    {
                                        Host.FarmModule.UseSkillAndWait(u.Key);
                                        Thread.Sleep(500);
                                    }
                                    var item = Host.MyGetItem(u.Value);
                                    if (item != null)
                                    {
                                        Host.MyUseItemAndWait(item);
                                        while (Host.MeMpPercent() < 90)
                                        {
                                            Thread.Sleep(100);
                                            if (!Host.MainForm.On)
                                                return;
                                            if (Host.Me.IsInCombat)
                                                return;
                                        }
                                        if (Host.Me.StandState == EStandState.Sit)
                                        {
                                            Host.ChangeStandState(EStandState.Stand);
                                            //  Host.MoveTo(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z);
                                        }
                                        break;
                                    }
                                }
                                finally
                                {
                                    Host.CommonModule.ResumeMove();
                                }
                            }
                        }

                        if (Host.Me.HpPercents < Host.CharacterSettings.CraftConjuredHp)
                        {
                            foreach (var u in listFood)
                            {
                                if (Host.SpellManager.GetSpell(u.Key) == null)
                                    continue;
                                Host.CommonModule.SuspendMove();
                                try
                                {
                                    if (Host.MeGetItemsCount(u.Value) < 2)
                                    {
                                        Host.FarmModule.UseSkillAndWait(u.Key);
                                        Thread.Sleep(500);
                                    }

                                    var item = Host.MyGetItem(u.Value);
                                    if (item != null)
                                    {
                                        Host.MyUseItemAndWait(item);
                                        while (Host.Me.HpPercents < 90)
                                        {
                                            Thread.Sleep(100);
                                            if (!Host.MainForm.On)
                                                return;
                                            if (Host.Me.IsInCombat)
                                                return;
                                        }
                                        if (Host.Me.StandState == EStandState.Sit)
                                        {
                                            Host.ChangeStandState(EStandState.Stand);
                                            //  Host.MoveTo(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z);
                                        }
                                    }
                                    break;
                                }
                                finally
                                {
                                    Host.CommonModule.ResumeMove();
                                }
                            }
                        }
                    }
                }


                if (Host.ClientType == EWoWClient.Classic)
                    return;

                if (!Host.Me.IsInCombat && Host.Me.HpPercents < 80)
                    UseRegenSpell(Host.SpellManager.GetSpell(8004));

                if (Host.Me.GetThreats().Count == 0 && Host.Me.HpPercents < 50)
                    UseRegenSpell(Host.SpellManager.GetSpell(18562));

                if (Host.Me.GetThreats().Count == 0 && Host.Me.HpPercents < 80)
                {
                    var regenSpel = Host.SpellManager.GetSpell(8936);
                    if (regenSpel != null)
                    {
                        if (Host.SpellManager.CheckCanCast(regenSpel.Id, Host.Me) != ESpellCastError.SUCCESS)
                            return;
                        Host.CommonModule.SuspendMove();
                        try
                        {
                            var needChangeForm = true;
                            foreach (var i in Host.Me.GetAuras())
                            {
                                if (i.SpellId == 24858)//Облик лунного совуха    
                                    needChangeForm = false;
                            }
                            if (needChangeForm)
                                Host.CanselForm();
                            Host.MyCheckIsMovingIsCasting();
                            var result = Host.SpellManager.CastSpell(regenSpel.Id, Host.Me);
                            if (result != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не смог использовать скилл регена " + result + " " + Host.GetLastError(), LogLvl.Error);
                                Thread.Sleep(2000);

                            }
                            while (Host.SpellManager.IsCasting)
                                Thread.Sleep(200);
                        }
                        finally
                        {
                            Host.CommonModule.ResumeMove();
                        }
                    }
                }
                /* if (host.Me.GetThreats().Count == 0)//Морозная аура, нахера?
                 {
                     var auraSkill = host.MyGetAura(3714);
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
                             host.MyCheckIsMovingIsCasting();
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
                 }*/

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void StopFarm()
        {
            try
            {
                BestMob = null;
                BestProp = null;
                SpecialSkills = null;
                SpecialItems = null;
                if (Host.FarmModule.FarmState != FarmState.Disabled)
                    FarmState = FarmState.AttackOnlyAgro;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        public void SetFarmMobs(Zone zone, List<uint> ids, int useSpecialItem = 0, float dist = 0)
        {
            try
            {
                if (!Host.IsAlive())
                    return;
                SpecialSkills = null;
                SpecialItems = null;
                BestMob = null;
                lock (FarmMobsIds)
                    FarmMobsIds = new List<uint>(ids);
                lock (FarmZone)
                    FarmZone = zone;
                Host.DrawZones(new List<Zone> { zone });
                if (useSpecialItem > 0)
                {
                    SpecialItems = new[] { useSpecialItem };
                    SpecialDist = dist;
                }

                else
                    SpecialItems = null;
                FarmState = FarmState.FarmMobs;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetFarmProps(Zone zone, List<uint> ids, int useSpecialItem = 0)
        {
            try
            {
                if (!Host.IsAlive())
                    return;
                SpecialSkills = null;
                SpecialItems = null;
                BestProp = null;
                lock (FarmPropIds)
                    FarmPropIds = new List<uint>(ids);
                lock (FarmZone)
                    FarmZone = zone;
                Host.DrawZones(new List<Zone> { zone });
                if (useSpecialItem > 0)
                    SpecialItems = new[] { useSpecialItem };
                else
                    SpecialItems = null;
                FarmState = FarmState.FarmProps;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        public bool InteractWithProp(GameObject prop)
        {
            try
            {
                Host.log("GameObject:" + prop.Name + " " + prop.GameObjectType);
                foreach (var @ushort in prop.RequiredGatheringSkill)
                {
                    Host.log("Скилы для сбора " + @ushort);
                }
                Host.MyCheckIsMovingIsCasting();



                if (Host.Me.Distance2D(prop.Location) > 6)
                {
                    Host.log("Слишком далеко " + Host.Me.Distance(prop), LogLvl.Error);
                    if (Host.CommonModule.MoveFailCount > 3)
                    {
                        SetBadProp(prop, 60000);
                    }

                    return false;
                }

                if (Host.CharacterSettings.Mode == Mode.Questing)
                {
                    if (Host.Me.MountId != 0)
                    {
                        Host.log("Отзываю маунта для сбора");
                        Host.CommonModule.MyUnmount();
                    }

                    Host.CanselForm();
                    Host.MyCheckIsMovingIsCasting();
                    if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                    {
                        if (!prop.Use())
                        {
                            Host.log("Не смог собрать " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", LogLvl.Error);
                            Thread.Sleep(1000);
                            if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                            {
                                if (!prop.Use())
                                {
                                    Host.log("Не смог собрать скилом 2 " + +Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", LogLvl.Error);

                                }
                            }
                            SetBadProp(prop, 60000);
                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }


                    while (Host.SpellManager.IsCasting)
                        Thread.Sleep(100);
                    Thread.Sleep(1000);
                    if (Host.CanPickupLoot())
                    {
                        if (!Host.PickupLoot())
                        {
                            /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                             if (!m.PickUp())
                             {*/
                            SetBadProp(prop, 60000);
                            Host.log("Не смог поднять дроп " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), LogLvl.Error);
                            Host.SetVar(prop, "pickFailed", true);
                            //   }
                        }
                        else
                        {
                            SetBadProp(prop, 60000);
                            _gatherCount = 0;
                            if (Host.FarmModule.MobsWithDropCount() < 2)
                                Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        Host.log("Окно лута не открыто " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), LogLvl.Error);
                        if (Host.CharacterSettings.FightIfMobs && FarmState == FarmState.Disabled)
                            FarmState = FarmState.AttackOnlyAgro;
                        if (Host.GetAgroCreatures().Count == 0)
                            SetBadProp(prop, 60000);
                        _gatherCount++;
                    }
                    return true;

                }

                var skillGather = Host.SpellManager.GetGatheringSpell(prop);
                if (skillGather == null)
                {
                    Host.log("Нет скила для сбора " + prop.Name + " " + prop.Id + " " + prop.RequiredGatheringSkill, LogLvl.Error);
                    SetBadProp(prop, 60000);
                    return false;
                }
                else
                {
                    Host.log("Скилл для сбора " + skillGather.Id + "  " + skillGather.Name);
                }



                var isNeedUnmount = true;

                foreach (var aura in Host.Me.GetAuras())
                {
                    if (aura.SpellId == 209563)
                        isNeedUnmount = false;
                    if (aura.SpellId == 267560)
                        isNeedUnmount = false;
                    if (aura.SpellId == 134359 && skillGather.Id == 265835)
                        isNeedUnmount = false;
                    if (aura.SpellId == 134359 && skillGather.Id == 265831)
                        isNeedUnmount = false;
                }



                if (isNeedUnmount)
                    if (Host.Me.MountId != 0)
                    {
                        Host.log("Отзываю маунта для сбора");
                        Host.CommonModule.MyUnmount();
                    }

                var isNeedChangeForm = true;
                foreach (var i in Host.Me.GetAuras())
                {
                    if (i.SpellId == 783 && skillGather.Id == 265835) //Походный облик
                        isNeedChangeForm = false;
                    if (i.SpellId == 783 && skillGather.Id == 265831) //Походный облик
                        isNeedChangeForm = false;
                    if (i.SpellId == 783 && skillGather.Id == 2366) //Походный облик
                        isNeedChangeForm = false;
                }


                if (isNeedChangeForm)
                    Host.CanselForm();

                Host.MyCheckIsMovingIsCasting();

                if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                {
                    if (!prop.Use())
                    {
                        Host.log("Не смог собрать скилом " + skillGather.Name + "  [" + skillGather.Id + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", LogLvl.Error);
                        Thread.Sleep(1000);
                        if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                        {
                            if (!prop.Use())
                            {
                                Host.log("Не смог собрать скилом 2 " + skillGather.Name + "  [" + skillGather.Id + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", LogLvl.Error);

                            }
                        }
                        SetBadProp(prop, 60000);
                    }
                }

                while (Host.SpellManager.IsCasting)
                    Thread.Sleep(100);
                Thread.Sleep(500);


                if (Host.CanPickupLoot())
                {
                    if (!Host.PickupLoot())
                    {
                        SetBadProp(prop, 60000);
                        Host.log("Не смог поднять дроп " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), LogLvl.Error);
                        Host.SetVar(prop, "pickFailed", true);
                    }
                    else
                    {
                        SetBadProp(prop, 60000);
                        _gatherCount = 0;
                        if (Host.FarmModule.MobsWithDropCount() < 2)
                            Thread.Sleep(500);
                    }
                }
                else
                {
                    Host.log("Окно лута не открыто " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), LogLvl.Error);
                    if (Host.CharacterSettings.FightIfMobs && FarmState == FarmState.Disabled)
                        FarmState = FarmState.AttackOnlyAgro;
                    if (_gatherCount > 1)
                        SetBadProp(prop, 60000);
                    _gatherCount++;
                }

                return false;
            }
            catch (ThreadAbortException) { return false; }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }

        private int NearTargetCount(Unit target, int range)
        {
            var i = 0;
            try
            {
                foreach (var creature in Host.GetAgroCreatures())
                {
                    if (creature == null)
                        continue;
                    if (!Host.IsAlive(creature))
                        continue;
                    if (target.Distance(creature) > range)
                        continue;
                    i++;
                }
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
            return i;
        }


        private bool IsSpellInstant(uint id)
        {
            var spell = Host.SpellManager.GetSpell(id);
            if (spell == null)
                return true;
            return spell.CastTime == 0;
        }


        private void UpdateRandomMoveTimes()
        {
            if (NextRandomMovesDirChange < DateTime.UtcNow)
            {
                NextRandomMovesDirChange = DateTime.UtcNow.AddSeconds(Host.RandGenerator.Next(5, 20));
                RandDirLeft = !RandDirLeft;
            }
        }

        private double GetRandomNumber(double minimum, double maximum)
        {
            return Host.RandGenerator.NextDouble() * (maximum - minimum) + minimum;
        }

        public bool IsBadTarget(Unit bestMob)
        {
            if (bestMob.Target == Host.Me)
                return false;
            if (bestMob.Target == Host.Me.GetPet())
                return false;
            if (bestMob.Target == null)
                return false;
            foreach (var groupMember in Host.Group.GetMembers())
            {
                if (bestMob.Target.Name == groupMember.Name)
                    return false;
            }
            BestMob = null;
            Host.CancelMoveTo();
            if (Host.SpellManager.CurrentAutoRepeatSpellId != 0)
                Host.SpellManager.CancelAutoRepeatSpell();
            return true;
        }

        private Unit GetHightPrioritiMob()
        {
            if (Host.GetBotLogin() == "deathstar")
            {
                if (Host.Me.GetThreats().Count > 1)
                    return null;
            }

            foreach (var unit in Host.GetAgroCreatures())
            {
                var zRange = Math.Abs(Host.Me.Location.Z - unit.Location.Z);

                if (zRange > Host.CharacterSettings.Zrange)
                    continue;

                if (!Host.IsAlive(unit))
                    continue;
                if (Host.FarmModule.IsBadTarget(unit, Host.FarmModule.TickTime))
                    continue;
                if (Host.FarmModule.IsImmuneTarget(unit))
                    continue;

                if (Host.FarmModule.SpecialItems != null)
                {
                    if (Host.FarmModule.SpecialItems.Length == 0)
                        if (!Host.CanAttack(unit, Host.CanSpellAttack))
                            continue;
                }
                else
                {
                    if (!Host.CanAttack(unit, Host.CanSpellAttack))
                        continue;
                }

                foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                {
                    if (unit.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                        if (characterSettingsMobsSetting.Priority == Priority.VeryHight)
                            return unit;
                }
            }

            foreach (var unit in Host.GetAgroCreatures())
            {
                var zRange = Math.Abs(Host.Me.Location.Z - unit.Location.Z);

                if (zRange > Host.CharacterSettings.Zrange)
                    continue;

                if (!Host.IsAlive(unit))
                    continue;
                if (Host.FarmModule.IsBadTarget(unit, Host.FarmModule.TickTime))
                    continue;
                if (Host.FarmModule.IsImmuneTarget(unit))
                    continue;

                if (Host.FarmModule.SpecialItems != null)
                {
                    if (Host.FarmModule.SpecialItems.Length == 0)
                        if (!Host.CanAttack(unit, Host.CanSpellAttack))
                            continue;
                }
                else
                {
                    if (!Host.CanAttack(unit, Host.CanSpellAttack))
                        continue;
                }

                foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                {
                    if (unit.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                        if (characterSettingsMobsSetting.Priority == Priority.Hight)
                            return unit;
                }
            }
            return null;
        }

        private GameObject GetNearestPropInZone(uint id, bool mobInsideMesh)
        {
            try
            {
                double minDist = 999999;
                GameObject bestProp = null;
                foreach (var prop in Host.GetEntities<GameObject>())
                {

                    if (!Host.IsExists(prop))
                        continue;
                    if (IsBadProp(prop, Host.FarmModule.TickTime))
                        continue;
                    if (prop.Id != id)
                        continue;
                    if (prop.Distance(-349.34, -3545.83, 31.01) < 10)
                        continue;
                    if (prop.Distance(-345.80, -3562.13, 31.24) < 10)
                        continue;
                    if (prop.Distance(-236.23, -5084.25, 25.78) < 10)
                        continue;
                    if (prop.Distance(-269.50, -5095.31, 25.24) < 10)
                        continue;

                    if (prop.Id == 194107)
                    {
                        /* if(prop.Location.Z < -10)
                             continue;*/
                    }

                    if (prop.Id == 194107)
                    {
                        if (prop.Location.Z < -5)
                            continue;
                    }

                    /* if (prop.Distance(-349.00, -3546.00, 31.17) < 10)
                         continue;*/



                    // Host.log(mobInsideMesh + "  " + id + "  " + farmZone.ObjInZone(prop));

                    /*if (mobInsideMesh)
                            if (!host.IsInsideNavMesh(prop.Location))
                            {
                                if (host.FindNearPointInRadiusNoZ(prop.Location.X, prop.Location.Y, 0.2))
                                    continue;
                                //  host.log("Добавляю точку");
                                host.CreateNewEditorGpsPoint(prop.Location, prop.Name + "[" + prop.Sid + "]");
                                continue;
                            }*/
                    if (!mobInsideMesh)
                    {
                        if (minDist < Host.Me.Distance(prop))
                            continue;
                    }
                    else
                    {
                        if (minDist < GetDistToMobFromMech(prop.Location))
                            continue;
                    }


                    if (!FarmZone.ObjInZone(prop))
                        continue;





                    minDist = Host.Me.Distance(prop);
                    bestProp = prop;

                }
                return bestProp;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return null;
            }
        }

        private double GetDistancePoint(GameObject prop)
        {
            try
            {
                double bestDist = 999999;
                foreach (var dungeonSettingsDungeonCoordSetting in Host.DungeonSettings.DungeonCoordSettings)
                {
                    if (prop.Distance(dungeonSettingsDungeonCoordSetting.Loc) < bestDist)
                        bestDist = prop.Distance(dungeonSettingsDungeonCoordSetting.Loc);
                }


                return bestDist;

            }
            catch (Exception e)
            {
                Host.log(e + "");
                return 999999;
            }
        }

        private GameObject GetBestPropInZone(bool mobInsideMesh)
        {
            try
            {
                if (Host.Me.IsInFlight)
                    return null;
                //  host.log("  GetBestPropInZone");
                // mobInsideMesh = true;
                double minDist = 999999;
                GameObject bestProp = null;
                foreach (var prop in Host.GetEntities<GameObject>())
                {
                    /* if (prop.GameObjectType != EGameObjectType.GatheringNode)
                         continue;*/
                    if (!Host.IsExists(prop))
                        continue;
                    /*    if (!farmPropIds.Contains(prop.Id))
                            continue;*/
                    if (IsBadProp(prop, Host.FarmModule.TickTime))
                        continue;
                    if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) != 0)
                        continue;
                    if (Host.CharacterSettings.Mode == Mode.Script)
                    {
                        if (Host.Me.Distance(prop) > Host.CharacterSettings.GatherRadiusScript)
                            continue;
                        var bestdist = GetDistancePoint(prop);
                        if (bestdist > Host.CharacterSettings.GatherRadiusScript)
                            continue;

                        var mobsIgnore = false;
                        foreach (var characterSettingsMobsSetting in Host.CharacterSettings.PropssSettings)
                        {
                            if (prop.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                if (characterSettingsMobsSetting.Priority == 0)
                                    mobsIgnore = true;
                        }
                        if (!mobsIgnore)
                            continue;

                        var ignore = false;
                        foreach (var characterSettingsGameObjectIgnore in Host.CharacterSettings.GameObjectIgnores)
                        {
                            if (characterSettingsGameObjectIgnore.Id == prop.Id &&
                                characterSettingsGameObjectIgnore.Ignore &&
                                prop.Distance(characterSettingsGameObjectIgnore.Loc) < 10)
                                ignore = true;
                        }

                        if (ignore)
                            continue;
                    }


                    /* if (mobInsideMesh)
                         if (!host.IsInsideNavMesh(prop.Location))
                         {
                             if (host.FindNearPointInRadiusNoZ(prop.Location.X, prop.Location.Y, 0.2))
                                 continue;
                             //  host.log("Добавляю точку");
                             host.CreateNewEditorGpsPoint(prop.Location, prop.Name + "[" + prop.Sid + "]");
                             continue;
                         }*/
                    if (!mobInsideMesh)
                    {
                        if (minDist < Host.Me.Distance(prop))
                            continue;
                    }
                    else
                    {
                        if (minDist < GetDistToMobFromMech(prop.Location))
                            continue;
                    }

                    /*   if (!farmZone.ObjInZone(prop))
                           continue;*/





                    minDist = Host.Me.Distance(prop);
                    bestProp = prop;

                }
                return bestProp;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return null;
            }
        }

        public void SetBadProp(GameObject obj, int addTime)
        {
            //  Host.log("бад " + addTime);
            if (obj != null)
            {
                BestProp = null;
                Host.SetVar(obj, "Intersect", null);
                Host.SetVar(obj, "bad", Host.GetUnixTime() + addTime);
                //  Host.log((Host.GetUnixTime() + addTime) + " бад ");
            }
        }

        public bool IsBadProp(GameObject obj, long time)
        {
            if (Host.GetVar(obj, "bad") != null)
            {
                var badTime = (long)(Host.GetVar(obj, "bad"));
                var result = time < badTime;
                //  Host.log(badTime + "badTime");
                // Host.log(time  + " бад " + result);
                return result;
            }
            return false;
        }

        public void SetBadTarget(Entity obj, int addTime)
        {

            if (obj != null)
            {
                Host.SetVar(obj, "Intersect", null);
                Host.SetVar(obj, "bad", Host.GetUnixTime() + addTime);
            }
        }

        public bool IsBadTarget(Entity obj, long time)
        {
            if (Host.GetVar(obj, "bad") != null)
            {
                var badTime = (long)(Host.GetVar(obj, "bad"));
                var result = time < badTime;
                return result;
            }
            return false;
        }

        private void SetImmuneTarget(Entity obj)
        {
            if (obj != null)
            {
                Host.SetVar(obj, "Intersect", null);
                Host.SetVar(obj, "immune", true);
                Host.SetVar(obj, "immuneX", obj.Location.X);
                Host.SetVar(obj, "immuneY", obj.Location.Y);
                Host.SetVar(obj, "immuneZ", obj.Location.Z);
            }
        }

        public bool IsImmuneTarget(Entity obj)
        {
            try
            {
                if (Host.GetVar(obj, "immune") != null)
                {
                    var isImmune = (bool)(Host.GetVar(obj, "immune"));
                    if (isImmune)
                    {
                        var immuneX = (double)(Host.GetVar(obj, "immuneX"));
                        var immuneY = (double)(Host.GetVar(obj, "immuneY"));
                        var immuneZ = (double)(Host.GetVar(obj, "immuneZ"));
                        return (Host.MyDistance(immuneX, immuneY, immuneZ, obj.Location.X, obj.Location.Y, obj.Location.Z) < 3);
                    }
                    return false;
                }
                return false;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }

        internal Unit GetBestAgroMob(bool mobInsideMesh = false)
        {
            try
            {
                double bestDist = 999999;
                Unit bestMob = null;
                var aggroMobs = Host.GetAgroCreatures();
                foreach (var obj in Host.GetEntities<Unit>())
                {
                    if (obj == null)
                        continue;

                    if (!Host.CanAttack(obj, Host.CanSpellAttack))
                        continue;
                    if (obj.Type == EBotTypes.Player)
                        continue;
                    if (obj.Type == EBotTypes.Pet)
                        continue;
                    if (!Host.IsAlive(obj))
                        continue;

                    if (!aggroMobs.Contains(obj))
                        continue;
                    /*  if (IsBadTarget(obj, host.ComboRoute.TickTime))
                          continue;*/
                    if (obj.Id == 27483)//динозавр
                        return obj;
                    if (!(bestDist > Host.Me.Distance(obj)))
                        continue;

                    bestDist = Host.Me.Distance(obj);
                    bestMob = obj;
                }

                /*    var finalMob = CheckBestMob(bestMob);
                    while (finalMob != bestMob)
                    {
                        bestMob = finalMob;
                        finalMob = CheckBestMob(bestMob);
                    }*/
                return bestMob;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return null;
            }
        }

        private double GetDistToMobFromMech(Vector3F loc)
        {
            double allDist = 0;
            var test = Host.GetSmoothPath(Host.Me.Location, loc);
            for (var i = 0; i < test.Path.Count; i++)
            {
                double dist = 0;
                if (i + 1 < test.Path.Count)
                    dist = Host.MyDistance(test.Path[i].X, test.Path[i].Y, test.Path[i].Z, test.Path[i + 1].X,
                    test.Path[i + 1].Y, test.Path[i + 1].Z);
                allDist += dist;
            }
            // Host.log(allDist.ToString());
            return allDist;
        }

        private Unit GetBestMob(bool mobInsideMesh = false)
        {
            try
            {
                double bestDist = 999999;
                Unit bestMob = null;
                var aggroMobs = Host.GetAgroCreatures();
                foreach (var obj in Host.GetEntities<Unit>())
                {
                    if (obj.Id == 126702 && obj.Location.Z < 28)
                        continue;
                    if (obj.Type == EBotTypes.Player)
                        continue;
                    if (obj.Type == EBotTypes.Pet)
                        continue;
                    if (obj.IsTotem())
                        continue;

                    if (Host.FarmModule.SpecialItems != null)
                    {
                        if (Host.FarmModule.SpecialItems?.Length == 0)
                            if (!Host.CanAttack(obj, Host.CanSpellAttack))
                                continue;
                    }
                    else
                    {
                        if (!Host.CanAttack(obj, Host.CanSpellAttack))
                            continue;
                    }


                    if (Host.CharacterSettings.Mode != Mode.Questing)
                        if (obj.Victim != null && obj.Victim != Host.Me && obj.Victim != Host.Me.GetPet())
                            continue;

                    if (Host.CharacterSettings.Mode == Mode.Questing)
                    {
                        if (obj.Distance(-349.34, -3545.83, 31.01) < 10)
                            continue;
                        if (obj.Distance(9993.67, 1398.21, 1282.94) < 10)
                            continue;
                        if (obj.Distance(9894.74, 1554.01, 1278.95) < 20)
                            continue;
                        if (obj.Distance(9882.24, 1526.17, 1275.57) < 20)
                            continue;
                        if (obj.Distance(9874.21, 1514.17, 1257.61) < 20)
                            continue;
                        if (obj.Distance(9892.94, 1483.57, 1277.97) < 20)
                            continue;
                        if (obj.Distance(9883.57, 1580.49, 1285.17) < 20)
                            continue;
                        if (obj.Distance(9844.51, 1555.41, 1290.95) < 20)
                            continue;
                        if (obj.Distance(9872.90, 1459.87, 1276.42) < 20)
                            continue;
                        if (obj.Distance(9862.75, 1580.74, 1287.43) < 20)
                            continue;
                        if (obj.Distance(9844.93, 1590.39, 1310.87) < 20)
                            continue;
                        if (obj.Distance(9815.89, 1565.21, 1294.96) < 20)
                            continue;
                        if (obj.Distance(9851.38, 1522.00, 1258.31) < 20)
                            continue;
                        if (obj.Distance(9873.51, 1492.11, 1258.34) < 20)
                            continue;
                        if (obj.Distance(9860.81, 1504.84, 1270.48) < 20)
                            continue;
                        if (obj.Distance(9776.68, 1560.15, 1266.34) < 20)
                            continue;
                        if (obj.Distance(776.45, 1654.16, 1.57) < 20)
                            continue;

                        if ((!FarmZone.ObjInZone(obj) || !FarmMobsIds.Contains(obj.Guid.GetEntry())) && !aggroMobs.Contains(obj))
                            continue;
                    }

                    if (Host.CharacterSettings.Mode == Mode.QuestingClassic)
                    {
                        if (!FarmZone.ObjInZone(obj) && !aggroMobs.Contains(obj))
                            continue;
                        if (FarmMobsIds.Count == 0)
                        {
                            if (!FarmZone.ObjInZone(obj))
                                continue;
                            var levelRange = Math.Abs(Host.Me.Level - obj.Level);

                            if (levelRange > 7)
                                continue;
                        }
                        else
                        {
                            if (!CheckFactionId(obj) && !aggroMobs.Contains(obj))
                                continue;
                        }

                    }



                    if (mobInsideMesh)
                        if (!Host.IsInsideNavMesh(obj.Location))
                            continue; // в мешах   



                    var zRange = Math.Abs(Host.Me.Location.Z - obj.Location.Z);
                    if (Host.Me.MovementFlags == EMovementFlag.Swimming)
                        zRange = 0;
                    if (zRange > Host.CharacterSettings.Zrange)
                        continue;


                    if (!Host.IsAlive(obj))
                        continue;
                    if (IsBadTarget(obj, Host.FarmModule.TickTime))
                        continue;
                    if (IsImmuneTarget(obj))
                        continue;
                    if (Host.Me.MovementFlags != EMovementFlag.Swimming)
                        if (!Host.CommonModule.CheckPathForLoc(Host.Me.Location, obj.Location))
                            continue;

                    if (!mobInsideMesh)
                    {
                        if (bestDist < Host.Me.Distance(obj))
                            continue;
                    }
                    else
                    {
                        if (bestDist < GetDistToMobFromMech(obj.Location))
                            continue;
                    }



                    if (Host.CharacterSettings.Mode == Mode.FarmMob || Host.CharacterSettings.Mode == Mode.FarmResource || Host.CharacterSettings.Mode == Mode.Script)
                    {
                        if (Host.CharacterSettings.UseFilterMobs)
                        {
                            var mobsIgnore = false;
                            foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                            {
                                if (obj.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                    if (characterSettingsMobsSetting.Priority == Priority.Ignore)
                                        mobsIgnore = true;
                            }
                            if (mobsIgnore)
                                continue;

                            foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                            {
                                if (obj.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                    if (characterSettingsMobsSetting.Priority == Priority.Hight)
                                        return obj;
                            }
                            if (Host.FarmModule.FarmState == FarmState.Disabled)
                                return null;
                            if ((!FarmZone.ObjInZone(obj) || !FarmMobsIds.Contains(obj.Guid.GetEntry())) && !aggroMobs.Contains(obj))
                                continue;
                        }
                        else
                        {
                            if (Host.CharacterSettings.UseMultiZone)
                            {
                                if (Host.AutoQuests.BestMultizone.UseFilter)
                                {
                                    if ((!FarmZone.ObjInZone(obj) || !FarmMobsIds.Contains(obj.Guid.GetEntry())) && !aggroMobs.Contains(obj))
                                        continue;
                                }
                                else
                                {
                                    if (!FarmZone.ObjInZone(obj))
                                        continue;
                                }
                            }
                            else
                            {
                                if (!FarmZone.ObjInZone(obj))
                                    continue;
                            }


                        }


                    }

                    if (Host.ClientType == EWoWClient.Classic && BestMob != null)
                    {
                        if (IsBadTarget(BestMob))
                            continue;
                    }


                    if (!mobInsideMesh)
                    {
                        bestDist = Host.Me.Distance(obj);
                    }
                    else
                    {
                        bestDist = GetDistToMobFromMech(obj.Location);
                    }
                    bestMob = obj;
                }
                return bestMob;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return null;
            }
        }

        private bool CheckFactionId(Unit unit)
        {
            if (unit == null)
                return false;
            if (FarmMobsIds.Contains(unit.Id))
                return true;
            if (FactionIds.Contains(unit.FactionId))
                return true;
            return false;

        }
    }
}
