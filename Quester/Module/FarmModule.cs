using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;
using System.Windows.Forms.VisualStyles;
using Out.Internal.Core;
using Out.Utility;
using WoWBot.Core;
using WowAI.UI;

namespace WowAI.Modules
{
    internal enum FarmState
    {
        Disabled, // Выключено отбивание
        FarmMobs,
        FarmProps,
        AttackOnlyAgro, // Перехватывать агро мобов на пути
        DefenseOnlyAgro, // Защищатся от  агро мобов
        EvadeOnlyAgro // обходить агро мобов
    }

    internal class FarmModule : Module
    {
        public override void Start(Host host)
        {
            base.Start(host);

            farmState = FarmState.Disabled;
            if (host.CharacterSettings.Mode == EMode.Questing)
                farmState = FarmState.AttackOnlyAgro;
        }

        public override void Stop()
        {
            base.Stop();
        }



        public bool StopFarmModule;

        public override void Run(CancellationToken ct)
        {
            try
            {
                StopFarmModule = false;
                while (!Host.cancelRequested && !ct.IsCancellationRequested)
                {
                    base.Run(ct);
                    Thread.Sleep(100);
                    // continue;
                    if (Host.Me == null)
                        continue;
                    if ((Host.MainForm.On) && (Host.GameState == EGameState.Ingame))
                    {
                        // if (farmState == FarmState.FarmMobs || farmState == FarmState.FarmProps || bestMob != null)
                        /* if (host.Me.Mount != null)
                             host.Me.Unmount();*/
                        Host.ComboRoute.PrepareForFight();
                        Host.ComboRoute.Fight();
                    }
                }
                StopFarmModule = true;
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
                Host.log("FarmModule Stop");
            }
        }


        internal bool readyToActions
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

        internal int[] specialItems;
        internal float specialDist;
        internal int[] specialSkills;

        internal Zone farmZone = new RoundZone(0, 0, 0);
        internal List<uint> farmMobsIds = new List<uint>();

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
                            AutoAttackStart = false;
                            //  host.MainForm.SetBestMobText(value.Name + "[" + value.Guid.GetEntry() + "]" + " [" + host.Me.Distance(value).ToString("F2") + "]");
                            _bestMob = value;
                        }
                        else
                        {
                            //  host.MainForm.SetBestMobText("null");
                            _bestMob = null;
                        }
                    }

                }
                catch
                {
                    _bestMob = null;
                }
            }
        }

        internal List<uint> farmPropIds = new List<uint>();
        private GameObject _bestProp;

        internal GameObject BestProp
        {
            get
            {
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
                            // host.MainForm.SetBestPropText(value.Name + "[" + value.Id + "]" + "[" + host.Me.Distance(value).ToString("F2") + "]");
                            _bestProp = value;
                        }
                        else
                        {
                            // host.MainForm.SetBestPropText("null");
                            _bestProp = null;
                        }
                    }

                }
                catch
                {
                    _bestProp = null;
                }
            }
        }

        private FarmState _farmState;
        internal FarmState farmState
        {
            get
            {
                return _farmState;
            }
            set
            {
                _farmState = value;
                //  host.MainForm.SetFarmModuleText(_farmState.ToString());
            }
        }


        public void StopFarm()
        {
            try
            {
                BestMob = null;
                specialSkills = null;
                specialItems = null;
                farmState = FarmState.AttackOnlyAgro;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetFarmMobs(Zone zone, List<uint> ids, int[] UseSpecialItem)
        {
            try
            {
                if (!Host.IsAlive())
                    return;
                specialSkills = null;
                specialItems = null;
                BestMob = null;
                lock (farmMobsIds)
                    farmMobsIds = new List<uint>(ids);
                lock (farmZone)
                    farmZone = zone;
                specialItems = UseSpecialItem;
                farmState = FarmState.FarmMobs;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetFarmMobs(Zone zone, List<uint> ids, int UseSpecialItem = 0, float dist = 0)
        {
            try
            {
                if (!Host.IsAlive())
                    return;
                specialSkills = null;
                specialItems = null;
                BestMob = null;
                lock (farmMobsIds)
                    farmMobsIds = new List<uint>(ids);
                lock (farmZone)
                    farmZone = zone;
                if (UseSpecialItem > 0)
                {
                    specialItems = new[] { UseSpecialItem };
                    specialDist = dist;
                }

                else
                    specialItems = null;
                farmState = FarmState.FarmMobs;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        public void SetFarmProps(Zone zone, List<uint> ids, int useSpecialItem = 0, float dist = 0)
        {
            try
            {
                if (!Host.IsAlive())
                    return;
                specialSkills = null;
                specialItems = null;
                BestProp = null;
                lock (farmPropIds)
                    farmPropIds = new List<uint>(ids);
                lock (farmZone)
                    farmZone = zone;
                if (useSpecialItem > 0)
                    specialItems = new[] { useSpecialItem };
                else
                    specialItems = null;
                farmState = FarmState.FarmProps;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }


        public int GatherCount = 0;
        internal bool InteractWithProp(GameObject prop)
        {
            try
            {
                Host.log("Проп " + prop.Name + " " + prop.GameObjectType + " " + prop.RequiredGatheringSkill);
                foreach (var @ushort in prop.RequiredGatheringSkill)
                {
                    Host.log("Скилы для сбора " + @ushort);
                }
                /*  if (host.GetAgroCreatures().Count > 0)
                      return false;*/
                while (Host.SpellManager.IsCasting || Host.Me.IsMoving)
                {
                    Thread.Sleep(100);
                    Host.log(Host.SpellManager.IsCasting + "    " + Host.Me.IsMoving);
                }
                    

                if (Host.Me.Distance(prop) > 6)
                {
                    Host.log("Слишком далеко " + Host.Me.Distance(prop), Host.LogLvl.Error);
                    if (Host.CommonModule._moveFailCount > 3)
                    {
                        SetBadProp(prop, 300000);
                    }

                    return false;
                }

                if (Host.CharacterSettings.Mode == EMode.Questing)
                {
                    /* uint spellId = 0;
                     if (prop.Id == 3189 || prop.Id == 3190 || prop.Id == 3192 || prop.Id == 3236 || prop.Id == 3290 || prop.Id == 175708 || prop.Id == 3640 || prop.Id == 207346
                         || prop.Id == 1673 || prop.Id == 195240 || prop.Id == 195007)
                     {
                         spellId = 6478;
                     }

                     if (prop.Id == 202648 || prop.Id == 194107 || prop.Id == 194179)
                         spellId = 6247;

                     if (prop.Id == 195074 || prop.Id == 194208 || prop.Id == 194263)
                         spellId = 3365;
                     if (prop.Id == 126158 || prop.Id == 195211 || prop.Id == 194145)
                         spellId = 6477;

                     if (spellId == 0)
                     {
                         Host.log("Нет скила для сбора " + prop.Name + " [" + prop.Id + "] " + prop.RequiredGatheringSkill, Host.LogLvl.Error);
                         return false;
                     }*/


                    if (Host.Me.MountId != 0)
                    {
                        Host.log("Отзываю маунта для сбора");
                        Host.CommonModule.MyUnmount();
                    }

                    Host.CanselForm();
                    while (Host.Me.IsMoving)
                    {
                        Thread.Sleep(100);
                    }
                    if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                    {
                        if (!prop.Use())
                        {
                            Host.log("Не смог собрать " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", Host.LogLvl.Error);
                            Thread.Sleep(1000);
                            if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                            {
                                if (!prop.Use())
                                {
                                    Host.log("Не смог собрать скилом 2 " + +Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", Host.LogLvl.Error);

                                }
                            }
                            SetBadProp(prop, 300000);
                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }
                    /*   var result2 = Host.SpellManager.CastSpell(spellId, prop);
                       Thread.Sleep(500);
                       if (result2 != ESpellCastError.SUCCESS)//8613 Skinning
                       {


                           Host.log("Не смог собрать скилом " + "  [" + spellId + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  " + result2, Host.LogLvl.Error);
                           SetBadProp(prop, 300000);
                           //   }
                       }*/

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
                            SetBadProp(prop, 300000);
                            Host.log("Не смог поднять дроп " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), Host.LogLvl.Error);
                            Host.SetVar(prop, "pickFailed", true);
                            //   }
                        }
                        else
                        {
                            SetBadProp(prop, 300000);
                            GatherCount = 0;
                            if (Host.ComboRoute.MobsWithDropCount() < 2)
                                Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        Host.log("Окно лута не открыто " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), Host.LogLvl.Error);
                        if (Host.CharacterSettings.FightIfMobs && farmState == FarmState.Disabled)
                            farmState = FarmState.AttackOnlyAgro;
                        // if (GatherCount > 1)
                        if (Host.GetAgroCreatures().Count == 0)
                            SetBadProp(prop, 60000);
                        GatherCount++;
                    }
                    return true;

                }

                var skillGather = Host.SpellManager.GetGatheringSpell(prop);
                if (skillGather == null)
                {
                    Host.log("Нет скила для сбора " + prop.Name + " " + prop.Id + " " + prop.RequiredGatheringSkill, Host.LogLvl.Error);
                    SetBadProp(prop, 300000);
                    return false;
                }
                else
                {
                    Host.log("Скилл для сбора " + skillGather.Id + "  " + skillGather.Name);
                }
                /* uint skillId = 0;

                 switch (prop.RequiredGatheringSkill)
                 {
                     case ESkillType.SKILL_MINING:
                         {
                             skillId = 195122;
                         }
                         break;
                     case ESkillType.SKILL_HERBALISM:
                         {
                             skillId = 195114;
                         }
                         break;

                     default:
                         {
                             host.log("Сбор не поддерживается " + prop.RequiredGatheringSkill);
                             return false;
                         }
                         break;
                 }*/
                /*   if (host.GetAgroCreatures().Count != 0)
                       return false;*/

                var isNeedUnmount = true;

                foreach (var aura in Host.Me.GetAuras())
                {
                    if (aura.SpellId == 209563)
                        isNeedUnmount = false;
                    if (aura.SpellId == 267560)
                        isNeedUnmount = false;
                    /*    if (aura.SpellId == 134359 && prop.RequiredGatheringSkill.Contains(182))
                            isNeedUnmount = false;*/
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
                    /*  if (i.SpellId == 5487)//Облик медведя
                          i.Cancel();
                      if (i.SpellId == 768)//Облик кошки
                          i.Cancel();
                      if (i.SpellId == 24858)//Облик лунного совуха    
                          i.Cancel();*/
                    if (i.SpellId == 783 && skillGather.Id == 265835) //Походный облик
                        isNeedChangeForm = false;
                    if (i.SpellId == 783 && skillGather.Id == 265831) //Походный облик
                        isNeedChangeForm = false;
                    if (i.SpellId == 783 && skillGather.Id == 2366) //Походный облик
                        isNeedChangeForm = false;
                }


                if (isNeedChangeForm)
                    Host.CanselForm();

                while (Host.Me.IsMoving)
                {
                    Thread.Sleep(100);
                }

                if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                {
                    if (!prop.Use())
                    {
                        Host.log("Не смог собрать скилом " + skillGather.Name + "  [" + skillGather.Id + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", Host.LogLvl.Error);
                        Thread.Sleep(1000);
                        if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) == 0)
                        {
                            if (!prop.Use())
                            {
                                Host.log("Не смог собрать скилом 2 " + skillGather.Name + "  [" + skillGather.Id + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  ", Host.LogLvl.Error);

                            }
                        }
                        SetBadProp(prop, 300000);
                    }
                }
                /*  var result = Host.SpellManager.CastSpell(skillGather.Id, prop);
                  Thread.Sleep(500);*/
                /*  if (result != ESpellCastError.SUCCESS)//8613 Skinning
                  {

                      Host.log("Не смог собрать скилом " + skillGather.Name + "  [" + skillGather.Id + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  " + result, Host.LogLvl.Error);
                      Thread.Sleep(1000);
                      result = Host.SpellManager.CastSpell(skillGather.Id, prop);
                      Thread.Sleep(500);
                      if (result != ESpellCastError.SUCCESS) //8613 Skinning
                      {
                          Host.log("Не смог собрать скилом 2 " + skillGather.Name + "  [" + skillGather.Id + "] " + Host.Me.Distance(prop) + "  " + prop.Id + "   " + Host.GetLastError() + "  " + result, Host.LogLvl.Error);
                      }

                      SetBadProp(prop, 300000);
                      //   }
                  }*/

                while (Host.SpellManager.IsCasting)
                    Thread.Sleep(100);
                Thread.Sleep(500);

                /*  if (!prop.IsLootable)
                      continue;*/

                /*if (!host.CanPickupLoot())
                    if (!host.OpenLoot())
                        host.log("Не смог открыть лут " + host.Me.Distance(m) + "  " + m.Name + "   " + host.GetLastError(), Host.LogLvl.Error);
                */
                if (Host.CanPickupLoot())
                {
                    if (!Host.PickupLoot())
                    {
                        /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                         if (!m.PickUp())
                         {*/
                        SetBadProp(prop, 300000);
                        Host.log("Не смог поднять дроп " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), Host.LogLvl.Error);
                        Host.SetVar(prop, "pickFailed", true);
                        //   }
                    }
                    else
                    {
                        SetBadProp(prop, 300000);
                        GatherCount = 0;
                        if (Host.ComboRoute.MobsWithDropCount() < 2)
                            Thread.Sleep(500);
                    }
                }
                else
                {
                    Host.log("Окно лута не открыто " + Host.Me.Distance(prop) + "  " + prop.Name + "   " + Host.GetLastError(), Host.LogLvl.Error);
                    if (Host.CharacterSettings.FightIfMobs && farmState == FarmState.Disabled)
                        farmState = FarmState.AttackOnlyAgro;
                    if (GatherCount > 1)
                        SetBadProp(prop, 300000);
                    GatherCount++;
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


        internal int NearTargetCount(Unit target, int range)
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



        private bool IsCreatureOccupierMeOrNull(Entity obj)
        {
            /*  if (obj.Occupier != null)
              {
                  if (obj.Occupier.Type == EBotTypes.Player)
                      if (obj.Occupier != host.Me)
                          return false;
              }*/
            return true;
        }

        private int _attackMoveFailCount;
        public bool AutoAttackStart = false;


        internal bool UseSkillAndWait(uint id, Entity target = null)
        {
            try
            {
                while (Host.Me.IsMoving)
                    Thread.Sleep(50);
                while (Host.SpellManager.IsCasting)
                    Thread.Sleep(50);
                while (Host.SpellManager.IsChanneling)
                    Thread.Sleep(50);


                var result2 = Host.SpellManager.CastSpell(id, target);
                if (result2 != ESpellCastError.SUCCESS)
                    Host.log("Не удалось использовать скилл  " + id + "  " + result2 + "   " + Host.GetLastError(), Host.LogLvl.Error);
                else
                {

                    Host.log("Использовал скилл  " + id, Host.LogLvl.Ok);
                    Thread.Sleep(200);
                }

                while (Host.Me.IsMoving)
                    Thread.Sleep(50);
                while (Host.SpellManager.IsCasting)
                    Thread.Sleep(50);
                while (Host.SpellManager.IsChanneling)
                    Thread.Sleep(50);

                if (result2 == ESpellCastError.SUCCESS)
                    return true;
                return false;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }


        //Использование скилов
        internal bool UseSkillAndWait(SkillSettings skill, bool selfTarget = false, bool suspendMovements = false)
        {
            try
            {
                var useskilllog = Host.CharacterSettings.LogSkill;

                if (Host.GetAgroCreatures().Count == 0 && (Host.ComboRoute.MobsWithDropCount() + Host.ComboRoute.MobsWithSkinCount()) > 0)
                    return false;

                if (!skill.Checked)
                    return false;

                if (!Host.SpellManager.IsSpellReady(skill.Id))
                {
                    // if (useskilllog) Host.log("Скилл не готов [" + skill.Id + "] " + skill.Name, Host.LogLvl.Error);
                    return false;
                }

       

                //17130 ураган
                //18313 меч
                List<uint> listArea = new List<uint>{17130, 18313};
                foreach (var areaTrigger in Host.GetEntities<AreaTrigger>())
                {
                    if (listArea.Contains(areaTrigger.Id) && Host.Me.Distance(areaTrigger) < 4)
                    {
                        var safePoint = new List<Vector3F>();
                        var xc = Host.Me.Location.X;
                        var yc = Host.Me.Location.Y;

                        var radius = 5;
                        const double a = Math.PI / 16;
                        double u = 0;
                        for (var i = 0; i < 32; i++)
                        {
                            var x1 = xc + radius * Math.Cos(u);
                            var y1 = yc + radius * Math.Sin(u);
                            // log(" " + i + " x:" + x + " y:" + y);
                            u = u + a;
                           
                            var z1 = Host.GetNavMeshHeight(new Vector3F(x1, y1, 0));
                            var nextpoint = false;
                            foreach (var trigger in Host.GetEntities<AreaTrigger>())
                            {
                                if(!listArea.Contains(trigger.Id))
                                    continue;
                                if (trigger.Distance(x1, y1, z1) < 4)
                                    nextpoint = true;
                            }
                            if(nextpoint)
                                continue;

                            if (Host.IsInsideNavMesh(new Vector3F((float)x1, (float)y1, (float)z1)))
                                safePoint.Add(new Vector3F((float)x1, (float)y1, (float)z1));
                        }
                        Host.log("Пытаюсь отойти от тригера" + safePoint.Count);
                        if (safePoint.Count > 0)
                        {
                            var bestPoint = safePoint[Host.RandGenerator.Next(safePoint.Count)];
                            Host.ForceMoveToWithLookTo(bestPoint, Host.Me.Target.Location); 
                        }

                    }
                }

                if (BestMob.Id == 122666)
                {
                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if(!entity.IsAlive)
                            continue;
                        if(entity.Id != 126654)
                            continue;
                        BestMob = entity;
                        
                    }
                }

                if (BestMob.Id == 131522 && Host.GetThreats().Count == 0)
                {
                    if (Host.SpellManager.GetSpell(8921) != null && Host.Me.Distance(BestMob) < 40)
                        Host.SpellManager.CastSpell(8921);
                }


                if (Host.GetAgroCreatures().Count > 0 && !Host.GetAgroCreatures().Contains(BestMob) || (Host.GetAgroCreatures().Count > 1))
                    BestMob = GetBestAgroMob();

                if (Host.Me.MountId != 0)
                {
                    Host.log("Отзываю маунта для боя");
                    Host.CommonModule.MyUnmount();
                }

                if (Host.Me.Class == EClass.Hunter)
                {
                    var needsummon = true;
                    var needrevive = false;
                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if (entity.Owner != Host.Me)
                            continue;
                        needsummon = false;
                        if (!entity.IsAlive)
                            needrevive = true;
                    }

                    if (needrevive)
                    {
                        /*  var pet = Host.SpellManager.CastSpell(982);
                          if (pet == ESpellCastError.SUCCESS)
                          {
                              Host.log("Воскрешаю питомца", Host.LogLvl.Ok);
                          }
                          else
                          {
                              Host.log("Не удалось воскресить питомца " + pet, Host.LogLvl.Error);
                          }
                          Thread.Sleep(2000);
                          while (Host.SpellManager.IsCasting)
                          {
                              Thread.Sleep(100);
                          }*/

                    }
                    if (needsummon && Host.CharacterSettings.SummonBattlePet)
                    {
                        uint petSkill = 0;
                        switch (Host.CharacterSettings.BattlePetNumber)
                        {
                            case 0:
                                {
                                    petSkill = 883;
                                }
                                break;
                            case 1:
                                {
                                    petSkill = 83242;
                                }
                                break;
                            case 2:
                                {
                                    petSkill = 83243;
                                }
                                break;
                            case 3:
                                {
                                    petSkill = 83244;
                                }
                                break;
                            case 4:
                                {
                                    petSkill = 83245;
                                }
                                break;
                        }

                        if (petSkill == 0)
                        {
                            Host.log("Неизвестный питомец " + Host.CharacterSettings.BattlePetNumber, Host.LogLvl.Error);
                        }
                        else
                        {
                            var pet = Host.SpellManager.CastSpell(petSkill);
                            if (pet == ESpellCastError.SUCCESS)
                            {
                                Host.log("Призвал питомца", Host.LogLvl.Ok);
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                if (Host.PetTameFailureReason == EPetTameFailureReason.Dead)
                                {
                                    Thread.Sleep(1000);
                                    Host.log("Pet = null");
                                    var pet2 = Host.SpellManager.CastSpell(982);
                                    if (pet2 == ESpellCastError.SUCCESS)
                                    {
                                        Host.log("Воскрешаю питомца", Host.LogLvl.Ok);
                                    }
                                    else
                                    {
                                        Host.log("Не удалось воскресить питомца " + pet2, Host.LogLvl.Error);
                                    }
                                    Thread.Sleep(2000);
                                    while (Host.SpellManager.IsCasting)
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                else
                                {
                                    Host.log(Host.Me.GetPet().IsAlive + " " + Host.Me.GetPet().Name);
                                }
                                Host.log("Не удалось призвать питомца " + pet, Host.LogLvl.Error);
                            }
                            while (Host.SpellManager.IsCasting)
                            {
                                Thread.Sleep(100);
                            }
                        }


                    }
                }

                if (Host.Me.Class == EClass.Druid)
                {
                    if (Host.CharacterSettings.FormForFight != "Не используется")
                    {
                        var formId = 0;
                        if (Host.CharacterSettings.FormForFight == "Облик медведя")
                            formId = 5487;
                        if (Host.CharacterSettings.FormForFight == "Облик кошки")
                            formId = 768;
                        if (Host.CharacterSettings.FormForFight == "Походный облик")
                            formId = 783;
                        if (Host.CharacterSettings.FormForFight == "Облик лунного совуха")
                            formId = 24858;


                        var isNeedAura = false;
                        foreach (var aura in Host.Me.GetAuras())
                        {
                            if (aura.SpellId == formId)
                            {
                                //   Host.log("Нашел ауру");
                                isNeedAura = true;
                            }
                        }

                        if (!isNeedAura)
                        {
                            Host.CanselForm();
                            foreach (var spell in Host.SpellManager.GetSpells())
                            {
                                if (spell.Id == formId)
                                {
                                    if (Host.SpellManager.CheckCanCast(spell.Id, Host.Me) != ESpellCastError.SUCCESS)
                                    {
                                        if (useskilllog) Host.log("CheckCanCast сообщает что скилл не готов [" + spell.Id + "] " + spell.Name + " " + Host.SpellManager.CheckCanCast(spell.Id, Host.Me), Host.LogLvl.Error);
                                        return false;
                                    }

                                    var resultForm = Host.SpellManager.CastSpell(spell.Id);
                                    if (resultForm != ESpellCastError.SUCCESS)
                                    {
                                        Host.log("Не удалось поменять форму " + spell.Name + "  " + resultForm, Host.LogLvl.Error);
                                    }
                                    else
                                    if (useskilllog)
                                        Host.log("Поменял форму " + spell.Name, Host.LogLvl.Ok);

                                    while (Host.SpellManager.IsCasting)
                                        Thread.Sleep(100);
                                }
                            }
                        }
                    }
                }



                if (BestMob == null)
                    return false;

                /* if (host.SpellManager.HasGlobalCooldown(skill.Id))
                     return false;*/

                if (Host.Me.HpPercents < 65 && !Host.CommonModule.InFight())
                {
                    Thread.Sleep(500);
                    return true;
                }

                //Пассивки


                if (BestMob.Id == 39072)
                {
                    if (BestMob.HpPercents > 0 && BestMob.HpPercents < 90 && Host.Me.Distance(BestMob) > 2)
                        Thread.Sleep(1300);
                }



                //Ожидание АОЕ атаки


                if (Host.Me.Target != BestMob)
                {
                    if (BestMob.Id == 26631 && Host.CanAttack(BestMob, Host.CanSpellAttack))
                    {

                    }
                    else
                    {
                        // Host.log("Выбираю цель");  26631
                        if (!Host.SetTarget(BestMob))
                            Host.log("Не смог выбрать цель " + Host.GetLastError(), Host.LogLvl.Error);
                        // Host.log("Выбрал цель");
                    }

                }
                //Цель
                if (Host.Me.Target == null)
                {
                    if (BestMob.Id == 26631 && Host.CanAttack(BestMob, Host.CanSpellAttack))
                    {

                    }
                    else
                    {
                        if (useskilllog)
                            Host.log("Нет цели [" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                }




                if (BestMob.Type == EBotTypes.Player)
                {
                    if (Host.Me.Distance(BestMob) > 30)
                    {
                        BestMob = null;
                        if (useskilllog)
                            Host.log("Цель - игрок " + skill.Name + "  " + skill.Id);
                        return true;
                    }
                }



                /*  if (skill.UseInPVP)
                      if (host.Me.Target.Type != EBotTypes.Player)
                          return false;*/



                if (Host.DistanceNoZ(Host.Me.Location.X, Host.Me.Location.Y, BestMob.Location.X, BestMob.Location.Y) < 2)
                    if (Host.Me.Distance(BestMob) > 10)
                    {
                        Host.log("Плохая цель 2:" + BestMob.Name);
                        SetBadTarget(BestMob, 60000);
                        BestMob = null;
                        _attackMoveFailCount = 0;

                        return true;
                    }

                /*  if (!IsCreatureOccupierMeOrNull(bestMob) && !host.GetAgroCreatures().Contains(bestMob))
                  {
                      host.log(bestMob.Occupier.Name + " ссагрил цель :" + bestMob.Name);
                      SetBadTarget(bestMob, 60000);
                      bestMob = null;
                      _attackMoveFailCount = 0;
                      host.SetTarget(null);
                      return true;
                  }*/



                /*  if (!host.CanUseSkill(skill.Id))
                  {
                      if (useskilllog) host.log("Ядро сообщает что скилл не готов [" + skill.Id + "] " + skill.Name + "   " + host.GetLastError());


                      return false;
                  }*/


                /*  DBSkillInfo dbSkillInfo = null;
                  if (host.GameDB.DBSkillInfos.ContainsKey(skill.Id))
                      dbSkillInfo = host.GameDB.DBSkillInfos[skill.Id];*/


                //Проверка настроек из интерфейса

                //Использовать на себя
                if (skill.SelfTarget)
                    selfTarget = true;
                Entity target = BestMob;
                if (selfTarget)
                    target = Host.Me;

                /*  if (skill.Id == 106832 || skill.Id == 213764)
                  {
                      target = null;
                  }*/

                var preResult = Host.SpellManager.CheckCanCast(skill.Id, target);

                if (preResult != ESpellCastError.SUCCESS)
                {
                    switch (preResult)
                    {
                        case ESpellCastError.LINE_OF_SIGHT:
                            {
                                if (useskilllog)
                                    Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                  Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                  , Host.LogLvl.Important);
                                if (Host.Me.Distance(BestMob) <= 2)
                                    Host.FarmModule.SetImmuneTarget(BestMob);
                                else if (Host.Me.Distance(BestMob) <= 5)
                                    Host.CommonModule.ForceMoveTo(BestMob, 2);
                                else if (Host.Me.Distance(BestMob) <= 10)
                                    Host.CommonModule.ForceMoveTo(BestMob, 5);
                                else if (Host.Me.Distance(BestMob) < 20)
                                    Host.CommonModule.ForceMoveTo(BestMob, 10);
                                else
                                    Host.CommonModule.ForceMoveTo(BestMob, 19);
                            }
                            break;
                        case ESpellCastError.OUT_OF_RANGE:
                            {
                                if (useskilllog)
                                    Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                 Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                 , Host.LogLvl.Important);
                                /*  if (Host.Me.Distance(bestMob) <= 2)
                                      Host.FarmModule.SetImmuneTarget(bestMob);
                                  else if (Host.Me.Distance(bestMob) <= 5)
                                      Host.CommonModule.ForceMoveTo(bestMob, 2);
                                  else if (Host.Me.Distance(bestMob) <= 10)
                                      Host.CommonModule.ForceMoveTo(bestMob, 5);
                                  else if (Host.Me.Distance(bestMob) < 20)
                                      Host.CommonModule.ForceMoveTo(bestMob, 10);
                                  else
                                      Host.CommonModule.ForceMoveTo(bestMob, 19);*/

                            }
                            break;
                        case ESpellCastError.UNIT_NOT_INFRONT:
                            {
                                if (useskilllog)
                                    Host.log("Плохой угол, поворачиваюсь UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) + "   " + Host.Me.GetAngle(BestMob));
                                Host.TurnDirectly(BestMob);
                                Thread.Sleep(100);
                                if (useskilllog)
                                    Host.log("Плохой угол, повернулся UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) + "   " + Host.Me.GetAngle(BestMob));
                                // Host.CommonModule.ForceMoveTo(bestMob, Host.Me.Distance(bestMob) - 1);
                            }
                            break;

                        case ESpellCastError.NO_POWER:
                            {
                                /*  Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                           Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.bestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                       , Host.LogLvl.Important);*/
                                return false;
                            }

                        case ESpellCastError.NOT_READY:
                            {
                                /*  Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                           Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                        , Host.LogLvl.Important);*/
                                return false;
                            }
                        case ESpellCastError.NOT_SHAPESHIFT:
                            {
                                if (skill.Id == 8936 || skill.Id == 18562 || skill.Id == 774) // [18562] Быстрое восстановление [774] Омоложение
                                {
                                    var isNeedCanselForm = true;
                                    foreach (var aura in Host.Me.GetAuras())
                                    {
                                        if (aura.SpellId == 69369)
                                            isNeedCanselForm = false;

                                        if (aura.SpellId == 24858 && skill.Id == 8936)//Облик лунного совуха    
                                            isNeedCanselForm = false;
                                    }
                                    if (isNeedCanselForm)
                                    {
                                        if (useskilllog)
                                            Host.log("Снимаю облик для использования скила " + skill.Id + "[" + skill.Name + "]", Host.LogLvl.Important);
                                        Host.CanselForm();
                                        Thread.Sleep(200);
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                if (useskilllog)
                                    Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                    Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                    , Host.LogLvl.Important);
                            }
                            return false;
                    }

                }
                // Host.log("Пытаюсь использовать скилл  " + skill.Name + skill.Id + "   " + Host.SpellManager.HasGlobalCooldown(skill.Id) + "  " + Host.SpellManager.IsSpellReady(skill.Id) + "   Время " + Host.ComboRoute.swUseSkill.ElapsedMilliseconds);

                if (Host.CommonModule.InFight() && !skill.UseInFight)
                {
                    if (useskilllog)
                        Host.log("Скилл используется только в бою " + skill.Name + "[" + skill.Id + "]");
                    return false;
                }


                var alternatePower = EPowerType.AlternatePower;
                if (Host.Me.Class == EClass.Druid)
                    alternatePower = EPowerType.ComboPoints;
                if (Host.Me.Class == EClass.Monk)
                    alternatePower = EPowerType.Chi;

                if (skill.CombatElementCountLess != 0)
                    if (Host.Me.GetPower(alternatePower) > skill.CombatElementCountLess)
                    {
                        if (useskilllog)
                            Host.log("Кол-во AlternatePower больше нужного " + skill.CombatElementCountLess + "/" + Host.Me.GetPower(alternatePower) + " [" + skill.Id + "] " + skill.Name);
                        return false;
                    }


                if (skill.CombatElementCountMore != 0)
                    if (Host.Me.GetPower(alternatePower) < skill.CombatElementCountMore)
                    {
                        if (useskilllog)
                            Host.log("Кол-во AlternatePower меньше нужного " + skill.CombatElementCountMore + "/" + Host.Me.GetPower(alternatePower) + " [" + skill.Id + "] " + skill.Name);
                        return false;
                    }




                //Персонаж ХП и МП
                if (Host.Me.HpPercents < skill.MeMinHp || Host.Me.HpPercents > skill.MeMaxHp)
                {
                    if (useskilllog)
                        Host.log("HP персонажа не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                    return false;
                }

                if (skill.TargetId != 0)
                {
                    if (skill.TargetId != BestMob.Id)
                    {
                        if (useskilllog)
                            Host.log("ID цели не соответсвует условиям " + skill.TargetId + " != " + BestMob.Id + "[" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                }


                decimal Power = Host.Me.GetPower(Host.Me.PowerType);
                decimal MaxPower = Host.Me.GetMaxPower(Host.Me.PowerType);

                if (Power != 0)
                    if (MaxPower != 0)
                    {
                        var percent = Power * 100 / MaxPower;

                        if (percent <= skill.MeMinMp || percent > skill.MeMaxMp)
                        {
                            if (useskilllog)
                                Host.log("MP персонажа не соответсвует условиям " + Host.Me.GetPower(Host.Me.PowerType) + " / " + Host.Me.GetMaxPower(Host.Me.PowerType) + "     " + percent + "/" + skill.MeMinMp + "/" + skill.MeMaxMp + " [" + skill.Id + "] " + skill.Name);
                            return false;
                        }
                    }




                //Эффекта нет на цели
                var findEffect = false;
                if (skill.NotTargetEffect != 0)
                {
                    foreach (var abnormalStatuse in BestMob.GetAuras())
                        if (abnormalStatuse.SpellId == skill.NotTargetEffect)
                            findEffect = true;
                    if (findEffect)
                    {
                        if (useskilllog)
                            Host.log("Нашел эффект на цели " + skill.NotTargetEffect + " [" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                }

                //Эффекта нет на персонаже
                // findEffect = false;
                if (skill.NotMeEffect != 0)
                {
                    foreach (var abnormalStatuse in Host.Me.GetAuras())
                        if (abnormalStatuse.SpellId == skill.NotMeEffect)
                            findEffect = true;
                    if (findEffect)
                    {
                        if (useskilllog)
                            Host.log("Нашел эффект на персонаже " + skill.NotMeEffect + " [" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                }

                //есть эффект на цели
                if (skill.IsTargetEffect != 0)
                {
                    foreach (var abnormalStatuse in BestMob.GetAuras())
                        if (abnormalStatuse.SpellId == skill.IsTargetEffect)
                            findEffect = true;
                    if (!findEffect)
                    {
                        if (useskilllog)
                            Host.log("Не нашел эффект на цели " + skill.IsTargetEffect + " [" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                }

                //есть эффект на персе
                if (skill.IsMeEffect != 0)
                {
                    foreach (var abnormalStatuse in Host.Me.GetAuras())
                        if (abnormalStatuse.SpellId == skill.IsMeEffect)
                        {
                            findEffect = true;
                            //  Host.log("нашел ", Host.LogLvl.Ok);
                        }

                    if (!findEffect)
                    {
                        if (useskilllog)
                            Host.log("Не нашел эффект на персонаже " + skill.IsMeEffect + " [" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                }

                //ХП цели
                if (BestMob?.HpPercents < skill.TargetMinHp || BestMob?.HpPercents > skill.TargetMaxHp)
                {
                    if (useskilllog)
                        Host.log("HP цели не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                    return false;
                }



                if (BestMob?.Hp == 0)
                    _attackMoveFailCount++;

                if (skill.MoveDist)
                    if (!selfTarget)
                    {

                        if (Host.Me.Distance(BestMob) >= skill.MaxDist)
                        {
                            if (useskilllog)
                                Host.log("Подбегаю что бы использовать [" + skill.Id + "] " + skill.Name + "  дист: " + Host.Me.Distance(BestMob) + "  " + skill.MaxDist);
                            /* if (host.Me.Distance(bestMob) > 40)
                             {
                                 host.CommonModule.ForceMoveTo(bestMob, 40, 40);
                                 if (useskilllog)
                                     host.log("Подбежал 1");
                                 return true;
                             }*/


                            if (Host.Me.Distance(BestMob) >= skill.MaxDist)
                            {
                                if (useskilllog)
                                    Host.log("Начинаю бег " + "  дист: " + Host.Me.Distance(BestMob));
                                if (!Host.CommonModule.ForceMoveTo(BestMob, skill.MaxDist - 1, skill.MaxDist - 1))
                                {
                                    if (Host.GetLastError() != ELastError.Movement_MoveCanceled)
                                        Host.log("Ошибка передвижения к цели: " + Host.GetLastError() + "(" + _attackMoveFailCount + ")", Host.LogLvl.Error);
                                }
                                if (useskilllog)
                                    Host.log("Закончил бег " + "  дист: " + Host.Me.Distance(BestMob));
                                Host.CancelMoveTo();
                                if (Host.GetLastError() == ELastError.Movement_MovePossibleFullStop)
                                    _attackMoveFailCount++;
                                else
                                    _attackMoveFailCount = 0;

                                return true;
                            }

                        }


                    }

                //Дистанция Подбегание
                if (_attackMoveFailCount > 5)
                {
                    //Плохая цель
                    Host.log("Плохая цель 3 :" + BestMob.Name);
                    SetBadTarget(BestMob, 60000);
                    BestMob = null;
                    _attackMoveFailCount = 0;
                    // host.SetTarget(null);
                    return true;
                }

                if (Host.Me.GetAngle(BestMob) > 20 && Host.Me.GetAngle(BestMob) < 340)
                {
                    if (useskilllog)
                        Host.log("Плохой угол, поворачиваюсь к мобу " + Host.Me.Distance(BestMob) + "  " + Host.Me.GetAngle(BestMob));
                    Host.TurnDirectly(BestMob);
                    Thread.Sleep(100);
                    if (useskilllog)
                        Host.log("Плохой угол, повернулся к мобу " + Host.Me.Distance(BestMob) + "  " + Host.Me.GetAngle(BestMob));
                }


                //Дистанция - максимальная
                if (Host.Me.Distance(BestMob) > skill.MaxDist + 1)
                    if (!selfTarget)
                    {
                        if (useskilllog)
                            Host.log("Максимальаня дистанция не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                        return false;
                    }
                //Дистанция минимальная
                if (Host.Me.Distance(BestMob) < skill.MinDist)
                    if (!selfTarget)
                    {
                        if (useskilllog)
                            Host.log("Минимальная дистанция не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                        return false;
                    }

                //Аое
                Unit aoeTarget = null;
                if (skill.AoeMe)
                    aoeTarget = Host.Me; //Рядом со мной
                else
                    aoeTarget = BestMob;
                if (NearTargetCount(aoeTarget, skill.AoeRadius) < skill.AoeMin ||
                    NearTargetCount(aoeTarget, skill.AoeRadius) > skill.AoeMax)
                {
                    if (useskilllog)
                        Host.log("Кол-во целей не соответсвует условиям [" + skill.Id + "] " + skill.Name + "   " + NearTargetCount(aoeTarget, skill.AoeRadius) + " " + skill.AoeMin + "/" + skill.AoeMax);
                    return false;
                }









                if ( /*!selfTarget && bestMob != null && */!Host.IsAlive(BestMob))
                {
                    Host.log("Моб умер " + skill.Id + " " + skill.Name);
                    return true;
                }







                /*  if(!AutoAttackStart)
                  {
                      var auto = host.SpellManager.GetSpell(75);
                      if(auto != null)
                      {
                          var res = host.SpellManager.CastSpell(auto.Id);
                          host.log("Авто атака " + res, Host.LogLvl.Important);
                          AutoAttackStart = true;
                      }

                  }
                  */

                var result = new ESpellCastError();









                if (useskilllog)
                    Host.log("Пытаюсь использовать: [" + skill.Id + "] " + skill.Name + "   Время " + Host.ComboRoute.swUseSkill.ElapsedMilliseconds, Host.LogLvl.Important);


                Host.CancelMoveTo();

                while (Host.SpellManager.IsCasting || Host.Me.IsMoving || Host.SpellManager.IsChanneling || Host.SpellManager.HasGlobalCooldown(skill.Id))
                {
                    if (!Host.Me.IsAlive)
                        return false;
                    if (!BestMob.IsAlive)
                    {
                        Host.SpellManager.CancelSpellCast();
                        Host.SpellManager.CancelSpellChanneling();
                    }
                    // host.log("IsCasting: " + host.SpellManager.IsCasting + "  IsMoving:" + host.Me.IsMoving);
                    Thread.Sleep(50);
                }




                result = Host.SpellManager.CastSpell(skill.Id, target/*, host.Me.Target.Location*/);
                // Thread.Sleep(1000);
                //использую скилл
                while (Host.SpellManager.IsCasting || Host.Me.IsMoving || Host.SpellManager.IsChanneling)
                {
                    if (!Host.Me.IsAlive)
                        return false;
                    if (!BestMob.IsAlive)
                    {
                        Host.SpellManager.CancelSpellCast();
                        Host.SpellManager.CancelSpellChanneling();
                    }
                    //  host.log("IsCasting: " + host.SpellManager.IsCasting + "  IsMoving:" + host.Me.IsMoving);
                    Thread.Sleep(50);
                }

                if (result != ESpellCastError.SUCCESS)
                {
                    var le = Host.GetLastError();

                    switch (result)
                    {
                        case ESpellCastError.LINE_OF_SIGHT:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " + "   " + result + "  на " + target.Name + "   " +
                                  Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                  , Host.LogLvl.Error);
                                if (Host.Me.Distance(BestMob) <= 2)
                                    Host.FarmModule.SetImmuneTarget(BestMob);
                                else if (Host.Me.Distance(BestMob) <= 5)
                                    Host.CommonModule.ForceMoveTo(BestMob, 2);
                                else if (Host.Me.Distance(BestMob) <= 15)
                                    Host.CommonModule.ForceMoveTo(BestMob, 5);
                                else if (Host.Me.Distance(BestMob) < 20)
                                    Host.CommonModule.ForceMoveTo(BestMob, 10);
                                else
                                    Host.CommonModule.ForceMoveTo(BestMob, 19);
                            }
                            break;
                        case ESpellCastError.OUT_OF_RANGE:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " + "   " + result + "  на " + target.Name + "   " +
                                 Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                 , Host.LogLvl.Error);
                                if (Host.Me.Distance(BestMob) <= 2)
                                    Host.FarmModule.SetImmuneTarget(BestMob);
                                else if (Host.Me.Distance(BestMob) <= 5)
                                    Host.CommonModule.ForceMoveTo(BestMob, 2);
                                else if (Host.Me.Distance(BestMob) <= 10)
                                    Host.CommonModule.ForceMoveTo(BestMob, 5);
                                else if (Host.Me.Distance(BestMob) < 20)
                                    Host.CommonModule.ForceMoveTo(BestMob, 10);
                                else
                                    Host.CommonModule.ForceMoveTo(BestMob, 19);

                            }
                            break;
                        case ESpellCastError.UNIT_NOT_INFRONT:
                            {
                                Host.log("Плохой угол, поворачиваюсь UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) + "   " + Host.Me.GetAngle(BestMob));
                                Host.TurnDirectly(BestMob);
                                Host.log("Плохой угол, повернулся UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) + "   " + Host.Me.GetAngle(BestMob));
                            }
                            break;

                        case ESpellCastError.NO_POWER:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " + "   " + result + "  на " + target.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                     , Host.LogLvl.Error);
                                return false;
                            }

                        case ESpellCastError.NOT_READY:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " + "   " + result + "  на " + target.Name + "   " +
                                          Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                       , Host.LogLvl.Error);
                                return false;
                            }


                        default:
                            {

                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " + "   " + result + "  на " + target.Name + "   " +
                                    Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                    , Host.LogLvl.Error);
                            }
                            break;
                    }




                    if (BestMob?.HpPercents == 100 && le != ELastError.Unknown)
                        _attackMoveFailCount++;

                    return false;
                    // Thread.Sleep(3000);
                }
                else
                {
                    _attackMoveFailCount = 0;
                    if (useskilllog)
                        Host.log("Использую скилл: [" + skill.Id + "] " + skill.Name + " на " + target.Name + "   Время " + Host.ComboRoute.swUseSkill.ElapsedMilliseconds, Host.LogLvl.Ok);
                    Host.ComboRoute.swUseSkill.Reset();
                    if (skill.Id == 198013)
                        Thread.Sleep(3000);
                    return false;
                }

            }
            catch (ThreadAbortException) { return false; }
            catch (Exception e)
            {

                Host.log(e.ToString());
                return false;
            }
        }

        internal Entity GetNearestCreatureInZone(int Id, Zone zone)
        {
            try
            {
                double minDist = 999999;
                Entity bestCreature = null;
                foreach (var obj in Host.GetEntities<Entity>())
                {

                    if (obj.Guid.GetEntry() == Id && zone.ObjInZone(obj))
                    {
                        if (minDist > Host.Me.Distance(obj))
                        {
                            minDist = Host.Me.Distance(obj);
                            bestCreature = obj;
                        }
                    }
                }
                return bestCreature;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return null;
            }
        }

        internal Entity GetNearestCreatureInZone(int Id)
        {
            double minDist = 999999;
            Entity bestCreature = null;
            foreach (var obj in Host.GetEntities<Entity>())
            {

                if (obj.Guid.GetEntry() == Id && farmZone.ObjInZone(obj))
                {
                    if (minDist > Host.Me.Distance(obj))
                    {
                        minDist = Host.Me.Distance(obj);
                        bestCreature = obj;
                    }
                }
            }
            return bestCreature;
        }

        internal GameObject GetNearestPropInZone(uint id, bool mobInsideMesh)
        {
            try
            {


                /* if (host.CharacterSettings.Mode == "Сбор ресурсов" || host.CharacterSettings.Mode == "Сбор Dacanda")
                 {
                     if (host.GetBotLogin() == "alxpro" || host.GetBotLogin() == "Daredevi1")
                         return GetBestPropInZone(true);
                 }
                 */
                // mobInsideMesh = true;
                double minDist = 999999;
                GameObject bestProp = null;
                foreach (var prop in Host.GetEntities<GameObject>())
                {
                  /*  if (Host.CharacterSettings.Mode != EMode.Questing)
                        if (prop.GameObjectType != EGameObjectType.GatheringNode)
                            continue;*/
                    if (!Host.IsExists(prop))
                        continue;
                    if (IsBadProp(prop, Host.ComboRoute.TickTime))
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
                    Host.log(mobInsideMesh + "  " + id + "  " + farmZone.ObjInZone(prop));
                    if (!farmZone.ObjInZone(prop))
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

        public double GetDistancePoint(GameObject prop)
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

        internal GameObject GetBestPropInZone(bool mobInsideMesh)
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
                    if (prop.GameObjectType != EGameObjectType.GatheringNode)
                        continue;
                    if (!Host.IsExists(prop))
                        continue;
                    /*    if (!farmPropIds.Contains(prop.Id))
                            continue;*/
                    if (IsBadProp(prop, Host.ComboRoute.TickTime))
                        continue;
                    if ((prop.DynamicFlags & EGameObjectDynamicFlags.NO_INTERACT) != 0)
                        continue;
                    double bestdist = 999999;
                    if (Host.CharacterSettings.Mode == EMode.Script)
                    {
                        if (Host.Me.Distance(prop) > Host.CharacterSettings.GatherRadiusScript)
                            continue;
                        bestdist = GetDistancePoint(prop);
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

        internal bool CircleIntersects(double x, double y, double R, double AX, double AY, double BX, double BY)
        {
            try
            {
                var L = Math.Sqrt(Math.Pow((BX - AX), 2) + Math.Pow((BY - AY), 2));
                // единичный вектор отрезка AB 
                var Xv = (BX - AX) / L;
                var Yv = (BY - AY) / L;
                var Xd = (AX - x);
                var Yd = (AY - y);
                var b = 2 * (Xd * Xv + Yd * Yv);
                var c = Xd * Xd + Yd * Yd - R * R;
                var c4 = c + c;
                c4 += c4;
                var D = b * b - c4;
                if (D < 0) return false; // нет корней, нет пересечений

                D = Math.Sqrt(D);
                var l1 = (-b + D) * 0.5;
                var l2 = (-b - D) * 0.5;
                var intersects1 = ((l1 >= 0.0) && (l1 <= L));
                var intersects2 = ((l2 >= 0.0) && (l2 <= L));
                var intersects = intersects1 || intersects2;
                return intersects;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
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

        public void SetImmuneTarget(Entity obj)
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
                        return (Host.Distance(immuneX, immuneY, immuneZ, obj.Location.X, obj.Location.Y, obj.Location.Z) < 3);
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

        /// <summary>
        /// Получить лучшего агро моба
        /// </summary>
        /// <param name="mobInsideMesh"></param>
        /// <returns></returns>
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
                    /*  if (obj.Type != EBotTypes.Npc)
                          continue;*/
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

        internal double GetDistToMobFromMech(Vector3F loc)
        {
            double allDist = 0;
            var test = Host.GetSmoothPath(Host.Me.Location, loc);
            for (var i = 0; i < test.Path.Count; i++)
            {
                double dist = 0;
                if (i + 1 < test.Path.Count)
                    dist = Host.Distance(test.Path[i].X, test.Path[i].Y, test.Path[i].Z, test.Path[i + 1].X,
                    test.Path[i + 1].Y, test.Path[i + 1].Z);
                allDist = allDist + dist;
            }
            // Host.log(allDist.ToString());
            return allDist;
        }
        /*  internal double GetDistToMobFromMech(Prop prop)
          {
              double allDist = 0;
              var test = host.GetSmoothPath(host.Me.Location, prop.Location);
              for (var i = 0; i < test.Path.Count; i++)
              {
                  double dist = 0;
                  if (i + 1 < test.Path.Count)
                      dist = host.Distance(test.Path[i].X, test.Path[i].Y, test.Path[i].Z, test.Path[i + 1].X,
                      test.Path[i + 1].Y, test.Path[i + 1].Z);
                  allDist = allDist + dist;
              }
              // host.log(allDist.ToString());
              return allDist;
          }
  */

        /* public bool CheckEliteInradius(Creature creature)
         {
             foreach (var elitecreature in host.GetCreatures())
             {
                 if (elitecreature.Type != EBotTypes.Npc)
                     continue;
                 if ((elitecreature as Npc).Db.Grade != ENPCGradeType.Elite)
                     continue;
                 if (!elitecreature.IsEnemy)
                     continue;
                 if (elitecreature.Distance(creature) < 20)
                     return true;
             }
             return false;
         }*/



        /// <summary>
        /// Получить лучшего моба
        /// </summary>
        /// <param name="mobInsideMesh"></param>
        /// <returns></returns>
        internal Unit GetBestMob(bool mobInsideMesh = false)
        {
            try
            {
                //   Host.log("Ищу моба " + mobInsideMesh);
                double bestDist = 999999;
                Unit bestMob = null;
                var aggroMobs = Host.GetAgroCreatures();

                foreach (var obj in Host.GetEntities<Unit>())
                {
                    if (Host.ComboRoute.SpecialItems != null)
                    {
                        if (Host.ComboRoute.SpecialItems?.Length == 0)
                            if (!Host.CanAttack(obj, Host.CanSpellAttack))
                                continue;
                    }
                    else
                    {
                        if (!Host.CanAttack(obj, Host.CanSpellAttack))
                            continue;
                    }
                    //  Host.log("Тест");

                    if (Host.CharacterSettings.Mode != EMode.Questing)
                    {

                        if (obj.Victim != null && obj.Victim != Host.Me && obj.Victim != Host.Me.GetPet())
                            continue;
                    }

                    if (Host.CharacterSettings.Mode == EMode.Questing)
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

                        if ((!farmZone.ObjInZone(obj) || !farmMobsIds.Contains(obj.Guid.GetEntry())) && !aggroMobs.Contains(obj))
                            continue;
                    }







                    /*     if (obj.Occupier != null)// проверка на агр
                             if (obj.Occupier != host.Me)
                                 continue;*/

                    if (mobInsideMesh)
                        if (!Host.IsInsideNavMesh(obj.Location)/* && host.GetBotLogin() == "Daredevi1"*/)
                        {
                            /* if (host.FindNearPointInRadiusNoZ(obj.Location.X, obj.Location.Y, 5))
                                 continue;
                               host.log("Добавляю точку");
                               host.CreateNewEditorGpsPoint(obj.Location, (obj as Npc).Db.LocalName + "[" + obj.Sid + "]");*/
                            continue;// в мешах   
                        }



                    var zRange = Math.Abs(Host.Me.Location.Z - obj.Location.Z);
                    if (mobInsideMesh)
                        zRange = 0;
                    if (zRange > Host.CharacterSettings.Zrange)
                        continue;


                    if (!Host.IsAlive(obj))
                        continue;
                    if (IsBadTarget(obj, Host.ComboRoute.TickTime))
                        continue;
                    if (IsImmuneTarget(obj))
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

                    /* if (bestDist < Host.Me.Distance(obj))
                         continue;*/


                    if (Host.CharacterSettings.Mode == EMode.FarmMob || Host.CharacterSettings.Mode == EMode.FarmResource || Host.CharacterSettings.Mode == EMode.Script)
                    {
                        if (Host.CharacterSettings.UseFilterMobs)
                        {
                            var mobsIgnore = false;
                            foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                            {
                                if (obj.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                    if (characterSettingsMobsSetting.Priority == 1)
                                        mobsIgnore = true;
                            }
                            if (mobsIgnore)
                                continue;

                            foreach (var characterSettingsMobsSetting in Host.CharacterSettings.MobsSettings)
                            {
                                if (obj.Guid.GetEntry() == characterSettingsMobsSetting.Id)
                                    if (characterSettingsMobsSetting.Priority == 2)
                                        return obj;
                            }
                            if (Host.FarmModule.farmState == FarmState.Disabled)
                                return null;
                        }

                        if ((!farmZone.ObjInZone(obj) || !farmMobsIds.Contains(obj.Guid.GetEntry())) && !aggroMobs.Contains(obj))
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


                    // bestDist = Host.Me.Distance(obj);
                    //bestDist = GetDistToMobFromMech(obj);
                    bestMob = obj;
                }

                //Если bestMob != null - это наш лучший моб. Но нужно еще проверить путь к нему теперь.
                /*   var finalMob = CheckBestMob(bestMob);
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

        /// <summary>
        /// Проверка лучшего моба
        /// </summary>
        /// <param name="best"></param>
        /// <returns></returns>
      /*  internal Npc CheckBestMob(Npc best)
        {
            try
            {
                if (best == null)
                    return null;
                var finalResult = new List<Npc>();
                if (best.Type != EBotTypes.Prop)
                    finalResult.Add((Creature)best);

                foreach (var obj in host.GetEntities<Npc>())
                {

                    if (!obj.IsVisible)//Не видно
                        continue;
                    if (obj.Type != EBotTypes.Npc)//не нпс
                        continue;
                    var zRange = Math.Abs(host.Me.Location.Z - obj.Location.Z);
                    if (zRange > 10)
                        continue;
                    if (!host.IsAlive(obj))
                        continue;
                    if (!obj.IsEnemy)
                        continue;

                    if (!host.CheckBuff(2000581, obj)
                        && !IsBadTarget(obj, host.ComboRoute.TickTime) // не в списке плохих
                        && (obj as Npc).Db.AggressiveType == EAggressiveType.Aggressive //агресивный                
                        && host.FarmModule.CircleIntersects(obj.Location.X, obj.Location.Y, 4, host.Me.Location.X, host.Me.Location.Y, best.Location.X, best.Location.Y))// может ссагрится
                    {
                        finalResult.Add(obj);
                    }
                }

                double finalDist = 999999;
                Creature finalMob = null;
                foreach (var obj in finalResult)
                {
                    if (finalDist > host.Me.Distance(obj))
                    {
                        // finalDist = GetDistToMobFromMech(obj);
                        finalDist = host.Me.Distance(obj);
                        finalMob = obj;
                    }
                }
                return finalMob;
            }
            catch (Exception e)
            {
                host.log(e.ToString());
                return null;
            }
        }*/


    }
}
