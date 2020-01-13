using Out.Internal.Core;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Out.Navigation;
using WoWBot.Core;
using WoWBot.WoWNetwork;

namespace WowAI.Module
{
    internal partial class FarmModule
    {
        private readonly Stopwatch _swUseSkill = new Stopwatch();

        private void UseSkills()
        {
            if (Host.CharacterSettings.Mode == Mode.QuestingClassic || Host.CharacterSettings.AdvancedFight)
            {
                if (BestMob != null && !Host.Me.IsInCombat && Host.ClientType == EWoWClient.Classic && Host.AutoQuests?.Convoy == null)
                {
                    Host.CommonModule.SuspendMove();
                    if (Host.Me.Distance(BestMob) > 40)
                    {
                        Host.CommonModule.ForceMoveTo(BestMob, 40);
                        return;
                    }

                    if (BestMob.GetReactionTo(Host.Me) != EReputationRank.Neutral)
                    {
                        var mobsAgrIfMeAttack = new List<Unit>();
                        var mobsAgrIfMeRun = new List<Unit>();
                        foreach (var entity in Host.GetEntities<Unit>())
                        {
                            if (entity == BestMob)
                            {
                                continue;
                            }

                            if (!entity.IsAlive)
                            {
                                continue;
                            }

                            if (entity.Owner != null)
                            {
                                continue;
                            }

                            if (entity.MinionGuid == BestMob.Guid)
                            {
                                continue;
                            }

                            if (entity.Type == EBotTypes.Player)
                            {
                                continue;
                            }

                            if (entity.Type == EBotTypes.Pet)
                            {
                                continue;
                            }

                            if (entity.GetReactionTo(Host.Me) == EReputationRank.Neutral)
                            {
                                continue;
                            }

                            if (entity.GetReactionTo(Host.Me) == EReputationRank.Friendly)
                            {
                                continue;
                            }
                            Vector3F hitPos = Vector3F.Zero;
                            Triangle tri = new Triangle();
                            if (Host.Raycast(BestMob.Location, entity.Location, ref hitPos, ref tri))
                            {
                                continue;
                            }

                            if (entity.Distance(BestMob) < 10)
                            {
                                mobsAgrIfMeAttack.Add(entity);
                            }

                            if (BestMob.Distance(entity) < Host.GetAggroRadius(entity))
                            {
                                mobsAgrIfMeRun.Add(entity);
                            }
                        }

                        if (mobsAgrIfMeAttack.Count > 0)
                        {
                            Host.log("При атаке ссагрится " + mobsAgrIfMeAttack.Count + " ", LogLvl.Important);
                            foreach (var unit in mobsAgrIfMeAttack)
                            {
                                Host.log(unit.Name + " " + unit.Id + " " + unit.Class + " dist" + unit.Distance(BestMob), LogLvl.Important);
                            }
                        }

                        if (mobsAgrIfMeRun.Count > 0)
                        {
                            Host.log("При движении  ссагрится " + mobsAgrIfMeRun.Count + " ", LogLvl.Important);
                            foreach (var unit in mobsAgrIfMeRun)
                            {
                                Host.log(unit.Name + " " + unit.Id + " " + unit.Class + " dist" + unit.Distance(BestMob) + "  " + Host.GetAggroRadius(unit) + "  " + unit.GetReactionTo(Host.Me), LogLvl.Important);
                            }
                        }


                        if (mobsAgrIfMeRun.Count > 0 && BestMob.Class != EClass.Mage)
                        {
                            if (Host.Me.Distance(BestMob) > 40)
                            {
                                Host.CommonModule.ForceMoveTo(BestMob, 40);
                            }

                            if (BestMob != null && BestMob.Target == null)
                            {
                                var point = Host.Me.Location;
                                Host.CommonModule.ForceMoveTo(BestMob, 20);
                                var radius = Convert.ToInt32(Host.GetAggroRadius(BestMob) - 3);
                                for (var i = radius; i > 1; i -= 2)
                                {
                                    if (Host.Me.Distance(BestMob) < i)
                                    {
                                        continue;
                                    }

                                    if (Host.Me.IsInCombat)
                                    {
                                        break;
                                    }

                                    if (BestMob.IsInCombat)
                                    {
                                        break;
                                    }

                                    if (BestMob.Target != null)
                                    {
                                        break;
                                    }

                                    if (!Host.MainForm.On)
                                    {
                                        return;
                                    }

                                    Host.CommonModule.ForceMoveTo(BestMob, i);
                                    Thread.Sleep(300);
                                }

                                if (Host.Me.IsInCombat)
                                {
                                    Host.CommonModule.ForceMoveTo(point, 2);
                                    Host.TurnDirectly(BestMob);
                                    if (Host.Me.Distance(BestMob) > 5)
                                    {
                                        Thread.Sleep(1000);
                                    }

                                    return;
                                }
                            }


                        }
                    }

                    if (BestMob != null && Host.Raycast(Host.Me, BestMob))
                    {
                        Host.log("Не вижу моба, ищу точку");
                        if (!Host.Me.IsInCombat)
                        {
                            var range = Math.Min(Host.Me.Distance(BestMob), 30);
                            var point = Host.CalcNoLosPoint(BestMob.Location, range);
                            if (point != Vector3F.Zero)
                            {
                                Host.CommonModule.ForceMoveTo(point, 0);
                                Thread.Sleep(500);
                            }
                        }
                    }
                }
            }


            foreach (var skillSettingse in _deckSkills)
            {
                if (!skillSettingse.Checked)
                {
                    continue;
                }

                _swUseSkill.Restart();
                if (AttackWithSkill(skillSettingse))
                {
                    return;
                }
            }
        }

        private bool AttackWithSkill(SkillSettings skill)
        {
            try
            {
                UseSpecialSkills(SpecialDist);
                return Host.FarmModule.UseSkillAndWait(skill);
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }

        private SkillSettings MyGetMaxLevelSkill(SkillSettings skill)
        {
            var result = skill;
            if (!Host.GameDB.SpellInfoEntries.ContainsKey(skill.Id))
            {
                return skill;
            }

            uint bestLevel = 0;
            var bestId = skill.Id;
            foreach (var spell in Host.SpellManager.GetSpells())
            {
                if (spell.Name == skill.Name && Host.GameDB.SpellInfoEntries.ContainsKey(spell.Id))
                {
                    var spellInfo = Host.GameDB.SpellInfoEntries[spell.Id];
                    if (spellInfo.SpellLevel > bestLevel)
                    {
                        bestLevel = spellInfo.SpellLevel;
                        bestId = spell.Id;
                    }
                }
            }

            if (result.Id != bestId)
            {
                // Host.log("Нашел скилл " + skill.Name + " с уровенем " + bestLevel + "  " + result.NotTargetEffect + " " + result.Id);
                if (result.IsMeEffect == result.Id)
                {
                    result.IsMeEffect = Convert.ToInt32(bestId);
                }

                if (result.IsTargetEffect == result.Id)
                {
                    result.IsTargetEffect = Convert.ToInt32(bestId);
                }

                if (result.NotMeEffect == result.Id)
                {
                    result.NotMeEffect = Convert.ToInt32(bestId);
                }

                if (result.NotTargetEffect == result.Id)
                {
                    result.NotTargetEffect = Convert.ToInt32(bestId);
                }

                result.Id = bestId;
            }
            else
            {
                //  Host.log("Не нашел скилов с более высоким уровнем " + skill.Name + "  " + bestLevel);
            }

            return result;
        }

        private bool UseSkillQuest()
        {
            if (BestMob?.Id == 122666)
            {
                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (!entity.IsAlive)
                    {
                        continue;
                    }

                    if (entity.Id != 126654)
                    {
                        continue;
                    }

                    BestMob = entity;
                }
            }

            if (BestMob?.Id == 124547)
            {
                if (Host.SpellManager.GetSpell(267934) != null && BestMob.HpPercents > 90)
                {
                    Host.SpellManager.CastSpell(267934);
                }
            }

            if (BestMob?.Id == 126038)
            {
                if (Host.SpellManager.GetSpell(267934) != null && BestMob.HpPercents > 90)
                {
                    Host.SpellManager.CastSpell(267934);
                }
            }


            if (BestMob?.Id == 131522 && Host.Me.GetThreats().Count == 0)
            {
                if (Host.SpellManager.GetSpell(8921) != null && Host.Me.Distance(BestMob) < 40)
                {
                    Host.SpellManager.CastSpell(8921);
                }
            }


            if (BestMob?.Id == 123461)
            {
                var item = Host.ItemManager.GetItemById(151363);
                if (item != null)
                {
                    Host.CommonModule.MoveTo(BestMob, 15);
                    Host.MyUseItemAndWait(item, BestMob);
                }
            }

            if (BestMob?.Id == 130219)
            {
                var item = Host.ItemManager.GetItemById(155458);
                if (item != null)
                {
                    Host.MyUseItemAndWait(item, BestMob);
                    BestMob = null;
                    return false;
                }
            }

            if (BestMob?.Id == 131285)
            {
                var use = true;
                foreach (var aura in BestMob.GetAuras())
                {
                    if (aura.SpellId == 263088)
                    {
                        use = false;
                    }
                }

                if (use && Host.Me.Distance(BestMob) < 10)
                {
                    var item = Host.ItemManager.GetItemById(156528);
                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item, BestMob);
                    }
                }
            }

            if (BestMob?.Id == 127253)
            {
                var use = true;
                foreach (var aura in BestMob.GetAuras())
                {
                    if (aura.SpellId == 252804)
                    {
                        use = false;
                    }
                }

                if (use)
                {
                    var item = Host.ItemManager.GetItemById(153483);
                    if (item != null)
                    {
                        Host.MyUseItemAndWait(item, BestMob);
                    }
                }
            }

            if (BestMob?.Id == 126502)
            {
                var item = Host.ItemManager.GetItemById(152572);
                if (item != null)
                {
                    var use = true;
                    foreach (var aura in BestMob.GetAuras())
                    {
                        if (aura.SpellId == 251286)
                        {
                            use = false;
                        }
                    }

                    if (use)
                    {
                        Host.CommonModule.ForceMoveTo(BestMob, 30);
                        if (Host.Me.Distance(BestMob) < 40 && !Host.SpellManager.IsCasting)
                        {
                            Host.MyUseItemAndWait(item, BestMob);
                        }
                    }
                }
            }

            if (BestMob?.Id == 126702)
            {
                var item = Host.ItemManager.GetItemById(152610);
                if (item != null)
                {
                    if (Host.Me.Distance(BestMob) > 10)
                    {
                        Host.CommonModule.ForceMoveTo(BestMob, 50);
                        if (Host.Me.Distance(BestMob) < 60 && !Host.SpellManager.IsCasting)
                        {
                            Host.MyUseItemAndWait(item, BestMob);
                            Host.CommonModule.ForceMoveTo(BestMob, 10);
                        }
                    }
                }
            }

            if (BestMob?.Id == 132189)
            {
            }
            else
            {
                if (Host.ClientType == EWoWClient.Retail)
                {
                    if (Host.GetAgroCreatures().Count > 0 && !Host.GetAgroCreatures().Contains(BestMob) ||
                        (Host.GetAgroCreatures().Count > 1))
                    {
                        BestMob = GetBestAgroMob();
                    }
                }
            }


            if (BestMob?.Id == 127298)
            {
                foreach (var entity in Host.GetEntities<Unit>())
                {
                    if (!entity.IsAlive)
                    {
                        continue;
                    }

                    if (entity.Id != 127407)
                    {
                        continue;
                    }

                    BestMob = entity;
                    break;
                }
            }

            if (Host.MyGetAura(267456) != null)
            {
                return false;
            }

            if (Host.MyGetAura(45438) != null)
            {
                return false;
            }

            if (Host.CharacterSettings.Mode == Mode.Questing)
            {
                if (Host.AutoQuests?.BestQuestId == 49126)
                {
                    if (Host.MyGetAura(259742) != null)
                    {
                        var npc = Host.GetNpcById(131613);
                        if (npc != null)
                        {
                            Host.SpellManager.CastPetSpell(npc.Guid, 259747);
                            Thread.Sleep(1500);
                            Host.SpellManager.CastPetSpell(npc.Guid, 259769);
                            Thread.Sleep(3000);
                        }

                        return false;
                    }
                }

                if (Host.AutoQuests?.BestQuestId == 47311)
                {
                    if (_useQuestSpell < DateTime.UtcNow)
                    {
                        _useQuestSpell = DateTime.UtcNow.AddSeconds(5);
                        var questSpell = Host.SpellManager.GetSpell(245398);
                        if (questSpell != null)
                        {
                            if (Host.SpellManager.GetSpellCooldown(questSpell) == 0 &&
                                Host.SpellManager.CheckCanCast(questSpell.Id, Host.Me.Target) ==
                                ESpellCastError.SUCCESS)
                            {
                                var res = Host.SpellManager.CastSpell(questSpell.Id);
                                if (res != ESpellCastError.SUCCESS)
                                {
                                    Host.log("Не смог использовать скилл для квеста " + questSpell.Name + "[" + questSpell.Id + "] " + res + " " + Host.GetLastError(), LogLvl.Error);
                                }
                            }
                        }
                    }
                }

                if (Host.AutoQuests?.BestQuestId == 48996)
                {
                    if (_useQuestSpell < DateTime.UtcNow)
                    {
                        _useQuestSpell = DateTime.UtcNow.AddSeconds(5);
                        var questSpell = Host.SpellManager.GetSpell(272331);
                        if (questSpell != null)
                        {
                            if (Host.SpellManager.GetSpellCooldown(questSpell) == 0 &&
                                Host.SpellManager.CheckCanCast(questSpell.Id, Host.Me.Target) ==
                                ESpellCastError.SUCCESS)
                            {
                                var res = Host.SpellManager.CastSpell(questSpell.Id);
                                if (res != ESpellCastError.SUCCESS)
                                {
                                    Host.log(
                                        "Не смог использовать скилл для квеста " + questSpell.Name + "[" +
                                        questSpell.Id + "] " + res + " " + Host.GetLastError(), LogLvl.Error);
                                }
                            }
                        }
                    }
                }
            }

            if (Host.MyGetAura(267254) != null)
            {
                if (BestMob != null)
                {
                    Host.CommonModule.MoveTo(BestMob, 3);
                    var pet = Host.GetNpcById(135825);
                    if (pet != null)
                    {
                        Host.SpellManager.CastPetSpell(pet.Guid, 267206);
                        Thread.Sleep(1000);
                    }
                }
                return false;
            }
            return true;
        }


        private bool UseSkillAndWait(SkillSettings skill, bool selfTarget = false)
        {
            try
            {
                if (!Host.Me.IsAlive)
                {
                    return true;
                }

                if (Host.ClientType == EWoWClient.Classic)
                {
                    skill = MyGetMaxLevelSkill(skill);
                }

                var useskilllog = Host.CharacterSettings.LogSkill;

                if (BestMob == null)
                {
                    useskilllog = false;
                }

                if (!skill.Checked || Host.SpellManager.GetSpell(skill.Id) == null)
                {
                    return false;
                }

                if (Host.CharacterSettings.Pvp)
                {
                    if (BestMob?.Type == EBotTypes.Player)
                    {
                        if (Host.Me.Distance(BestMob) > 40)
                        {
                            BestMob = null;
                            if (useskilllog)
                            {
                                Host.log("Цель - игрок убегает " + skill.Name + "  " + skill.Id);
                            }
                            return true;
                        }
                    }
                }
                else
                {
                    if (BestMob?.Type == EBotTypes.Player && !Host.CharacterSettings.Pvp)
                    {
                        BestMob = null;
                        if (useskilllog)
                        {
                            Host.log("Цель - игрок " + skill.Name + "  " + skill.Id);
                        }
                        return true;
                    }
                }


                if (Host.ClientType == EWoWClient.Classic && BestMob != null)
                {
                    if (IsBadTarget(BestMob))
                    {
                        return true;
                    }
                }

                if (Host.CharacterSettings.ChangeTargetInCombat)
                {
                    foreach (var entity in Host.GetEntities<Unit>())
                    {
                        if (!entity.IsAlive)
                            continue;
                        if (Host.FarmModule.BestMob == null)
                            break;
                        if (entity.CreatorGuid != Host.FarmModule.BestMob.Guid)
                            continue;
                        if (entity.IsTotem())
                            BestMob = entity;
                    }

                    if (Host.GetAgroCreatures().Count > 1)
                    {
                        var hight = GetHightPrioritiMob();
                        if (hight != null)
                            BestMob = hight;
                    }
                }

                if (Host.SpellManager.CurrentAutoRepeatSpellId == skill.Id)
                {
                    return true;
                }

                if (Host.ClientType == EWoWClient.Classic)
                {
                    if (!Host.SpellManager.IsSpellReady(skill.Id))
                    {
                        return false;
                    }
                }

                if (useskilllog)
                {
                    Host.log("----------------------------АТАКА " + skill.Name + " " + skill.Id + "-----------------------------" + "CurrentAutoRepeatSpellId " +
                             Host.SpellManager.CurrentAutoRepeatSpellId + " " + Host.SpellManager.AutoAttackingGuid);
                }

                if (Host.MyIsNeedRegen() && !Host.CommonModule.InFight() && skill.UseInFight)
                {
                    Thread.Sleep(100);
                    return false;
                }


                if (!UseSkillQuest())
                {
                    return false;
                }

                if (!Host.SpellManager.IsSpellReady(skill.Id) || Host.SpellManager.GetSpellCooldown(skill.Id) > 0)
                {
                    if (useskilllog) Host.log("Скилл не готов [" + skill.Id + "] " + skill.Name + " " + Host.SpellManager.GetSpellCooldown(skill.Id), LogLvl.Error);
                    return false;
                }


                //17130 ураган
                //18313 меч
                var listArea = new List<uint> { 17130, 18313, 17129, 3282 };
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
                            u += a;

                            var z1 = Host.GetNavMeshHeight(new Vector3F(x1, y1, 0));
                            var nextpoint = false;
                            foreach (var trigger in Host.GetEntities<AreaTrigger>())
                            {
                                if (!listArea.Contains(trigger.Id))
                                {
                                    continue;
                                }

                                if (trigger.Distance(x1, y1, z1) < 4)
                                {
                                    nextpoint = true;
                                }
                            }

                            if (nextpoint)
                            {
                                continue;
                            }

                            if (Host.IsInsideNavMesh(new Vector3F((float)x1, (float)y1, (float)z1)))
                            {
                                safePoint.Add(new Vector3F((float)x1, (float)y1, (float)z1));
                            }
                        }

                        Host.log("Пытаюсь отойти от тригера" + safePoint.Count);
                        if (safePoint.Count > 0 && Host.Me.Target != null)
                        {
                            Host.SetCTMMovement(false);
                            var bestPoint = safePoint[Host.RandGenerator.Next(safePoint.Count)];
                            Host.ForceMoveToWithLookTo(bestPoint, Host.Me.Target.Location);
                            Host.ForceMoveTo(bestPoint);
                            Host.SetCTMMovement(true);
                        }
                    }
                }


                if (Host.Me.MountId != 0 && BestMob != null)
                {
                    Host.log("Отзываю маунта для боя");
                    Host.CommonModule.MyUnmount();
                }

                switch (Host.Me.Class)
                {
                    case EClass.DeathKnight:
                        {
                            var needsummon = true;
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (entity.Owner != Host.Me)
                                {
                                    continue;
                                }

                                needsummon = false;
                            }

                            if (needsummon && Host.CharacterSettings.SummonBattlePet)
                            {
                                var pet = Host.SpellManager.CastSpell(46584);
                                if (pet == ESpellCastError.SUCCESS)
                                {
                                    Host.log("Призвал питомца", LogLvl.Ok);
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    Host.log("Не удалось призвать питомца " + pet, LogLvl.Error);
                                }

                                while (Host.SpellManager.IsCasting)
                                {
                                    Thread.Sleep(100);
                                }
                            }

                            break;
                        }

                    case EClass.Mage:
                        {
                            var needsummon = true;
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (entity.Owner != Host.Me)
                                {
                                    continue;
                                }

                                needsummon = false;
                            }

                            if (needsummon && Host.CharacterSettings.SummonBattlePet)
                            {
                                var pet = Host.SpellManager.CastSpell(31687);
                                if (pet == ESpellCastError.SUCCESS)
                                {
                                    Host.log("Призвал питомца", LogLvl.Ok);
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    Host.log("Не удалось призвать питомца " + pet, LogLvl.Error);
                                }

                                while (Host.SpellManager.IsCasting)
                                {
                                    Thread.Sleep(100);
                                }
                            }


                            if (BestMob != null && Host.CharacterSettings.Mode == Mode.QuestingClassic && Host.GetAgroCreatures().Count == 2 && BestMob.HpPercents > 50)
                            {
                                var polymorf = Host.SpellManager.GetSpell(118);
                                if (polymorf != null)
                                {
                                    foreach (var agroCreature in Host.GetAgroCreatures())
                                    {
                                        if (BestMob == agroCreature)
                                            continue;

                                        var needbreak = false;
                                        foreach (var aura in agroCreature.GetAuras())
                                        {
                                            if (aura.SpellId == 118)
                                                needbreak = true;
                                        }
                                        if (needbreak)
                                            break;
                                        if (agroCreature.GetCreatureType() == ECreatureType.Humanoid ||

                                            agroCreature.GetCreatureType() == ECreatureType.Beast)
                                        {
                                            if (agroCreature.HpPercents > 80)
                                            {
                                                if (UseSkillAndWait(polymorf.Id, agroCreature))
                                                    Thread.Sleep(500);
                                            }
                                            break;
                                        }

                                    }
                                }
                               
                            }
                            break;
                        }

                    case EClass.Hunter:
                    case EClass.Warlock:
                        {
                            if (skill.Name == "Drain Soul")
                            {
                                if (Host.MyGetItem(6265) != null && Host.MyGetItem(6265).Count > 3)
                                    return false;
                            }
                            if (BestMob != null)
                            {
                                if (Host.Me.Class == EClass.Hunter)
                                {
                                    if (Host.Me.GetPet() == null)
                                    {
                                        if (Host.Me.Distance(BestMob) < 7 && Host.Me.Distance(BestMob) > 3)
                                        {
                                            Host.CommonModule.ForceMoveTo(BestMob);
                                        }
                                    }
                                    else
                                    {
                                        if (!Host.Me.GetPet().IsAlive)
                                        {
                                            if (Host.Me.Distance(BestMob) < 7 && Host.Me.Distance(BestMob) > 3)
                                            {
                                                Host.CommonModule.ForceMoveTo(BestMob);
                                            }
                                        }
                                    }
                                }


                                if (Host.Me.GetPet() != null && Host.Me.Target != null)
                                {
                                    if (Host.Me.GetPet().IsAlive)
                                    {
                                        if (Host.GetAgroCreatures().Count > 1)
                                        {
                                            var needReturnPet = true;
                                            foreach (var agroCreature in Host.GetAgroCreatures())
                                            {
                                                if (agroCreature.Target == Host.Me)
                                                {
                                                    needReturnPet = false;
                                                }

                                                if (agroCreature.Target == Host.Me && Host.Me.GetPet().Target != agroCreature)
                                                {
                                                    Host.SpellManager.UsePetAction(Host.Me.GetPet().Guid, 2, EActiveStates.ACT_COMMAND, agroCreature);
                                                    if (useskilllog)
                                                    {
                                                        Host.log("Отправляю питомца на агра");
                                                    }

                                                    Thread.Sleep(1000);

                                                    break;
                                                }
                                            }


                                            if (needReturnPet)
                                            {
                                                if (Host.Me.GetPet().Target != BestMob && Host.Me.Target == BestMob && Host.Me.Distance(BestMob) < 45 && BestMob.IsAlive)
                                                {
                                                    if (Host.CharacterSettings.Mode == Mode.QuestingClassic)
                                                    {
                                                        if (Host.GetAgroCreatures().Count != 0)
                                                        {
                                                            Host.SpellManager.UsePetAction(Host.Me.GetPet().Guid, 2, EActiveStates.ACT_COMMAND, BestMob);
                                                            if (useskilllog)
                                                            {
                                                                Host.log("Отправляю питомца в атаку");
                                                            }

                                                            Thread.Sleep(1000);
                                                            return true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Host.SpellManager.UsePetAction(Host.Me.GetPet().Guid, 2, EActiveStates.ACT_COMMAND, BestMob);
                                                        if (useskilllog)
                                                        {
                                                            Host.log("Отправляю питомца в атаку 2 ");
                                                        }

                                                        Thread.Sleep(1000);
                                                        // return true;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Host.Me.GetPet().Target != BestMob && Host.Me.Target == BestMob && Host.Me.Distance(BestMob) < 40 && BestMob.IsAlive)
                                            {
                                                Host.SpellManager.UsePetAction(Host.Me.GetPet().Guid, 2, EActiveStates.ACT_COMMAND, BestMob);
                                                if (useskilllog)
                                                {
                                                    Host.log("Отправляю питомца в атаку 2 ");
                                                }

                                                Thread.Sleep(1000);
                                                // return true;
                                            }
                                        }
                                    }


                                    if (Host.Me.Class == EClass.Hunter)
                                    {
                                        if (Host.Me.Distance(BestMob) <= skill.MinDist + 1 && BestMob.Victim == Host.Me.GetPet())
                                        {
                                            Host.SpellManager.CancelAutoRepeatSpell();

                                            double range = 30;
                                            if (Host.Me.Distance(BestMob) < range)
                                            {
                                                range = Host.Me.Distance(BestMob);
                                            }

                                            if (range < 10)
                                            {
                                                range = 30;
                                            }

                                            var point = Host.CalcNoLosPoint(BestMob.Location, range);
                                            if (point != Vector3F.Zero)
                                            {
                                                Host.CommonModule.ForceMoveTo(point, 0);
                                                Thread.Sleep(500);
                                            }
                                            else
                                            {
                                                Host.SetMoveStateForClient(true);
                                                var rand = Host.RandGenerator.Next(0, 2);
                                                if (rand == 0)
                                                {
                                                    Host.StrafeLeft(true);
                                                }
                                                else
                                                {
                                                    Host.StrafeRight(true);
                                                }

                                                var sw = new Stopwatch();
                                                sw.Start();

                                                while (Host.Me.Distance(BestMob) <= skill.MinDist + 3 && BestMob.Victim == Host.Me.GetPet() && Host.IsAlive(BestMob))
                                                {
                                                    Thread.Sleep(100);
                                                    if (useskilllog)
                                                    {
                                                        Host.log("отбегаю " + rand + "  " + sw.ElapsedMilliseconds + " / 5000");
                                                    }

                                                    if (sw.ElapsedMilliseconds > 5000)
                                                    {
                                                        break;
                                                    }
                                                }

                                                Host.StrafeLeft(false);
                                                Host.StrafeRight(false);
                                                Host.SetMoveStateForClient(false);
                                            }

                                            Host.SpellManager.StopAutoAttack();
                                        }
                                    }
                                }
                            }

                            break;
                        }

                    case EClass.Druid when BestMob != null:
                        {
                            if (Host.CharacterSettings.FormForFight != "Не используется")
                            {
                                var formId = 0;
                                if (Host.CharacterSettings.FormForFight == "Облик медведя")
                                {
                                    formId = 5487;
                                }

                                if (Host.CharacterSettings.FormForFight == "Облик кошки")
                                {
                                    formId = 768;
                                }

                                if (Host.CharacterSettings.FormForFight == "Походный облик")
                                {
                                    formId = 783;
                                }

                                if (Host.CharacterSettings.FormForFight == "Облик лунного совуха")
                                {
                                    formId = 24858;
                                }

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
                                            if (Host.SpellManager.CheckCanCast(spell.Id, Host.Me) !=
                                                ESpellCastError.SUCCESS)
                                            {
                                                if (useskilllog)
                                                {
                                                    Host.log(
                                                        "CheckCanCast сообщает что скилл не готов [" + spell.Id + "] " +
                                                        spell.Name + " " +
                                                        Host.SpellManager.CheckCanCast(spell.Id, Host.Me),
                                                        LogLvl.Error);
                                                }

                                                return false;
                                            }

                                            var resultForm = Host.SpellManager.CastSpell(spell.Id);
                                            if (resultForm != ESpellCastError.SUCCESS)
                                            {
                                                if (Host.Me.Target == null)
                                                {
                                                    Host.log("Target null");
                                                }
                                                else
                                                {
                                                    Host.log("Цель: " + Host.Me.Target.Name);
                                                }

                                                if (Host.AdvancedLog)
                                                {
                                                    Host.log("Не удалось поменять форму " + spell.Name + "  " + resultForm,
                                                        LogLvl.Error);
                                                }
                                            }


                                            while (Host.SpellManager.IsCasting)
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        }

                    case EClass.Warrior:
                        {
                            if (skill.Id == 2764)
                            {
                                var use = false;
                                foreach (var item in Host.ItemManager.GetItems())
                                {
                                    if (item.Place != EItemPlace.Equipment)
                                    {
                                        continue;
                                    }

                                    if (item.InventoryType != EInventoryType.Thrown)
                                    {
                                        continue;
                                    }

                                    use = true;
                                }

                                if (!use)
                                {
                                    return false;
                                }
                            }
                        }
                        break;
                }


                var spellInstant = IsSpellInstant(skill.Id);

                if (BestMob?.Id == 151639)
                {
                    if (Host.Me.Distance(BestMob) < 10 && spellInstant && !Host.Me.IsMoving)
                    {
                        UpdateRandomMoveTimes();
                        double angle = Host.Me.Rotation.Y;
                        if (RandDirLeft)
                        {
                            angle += Math.PI / 2f;
                        }
                        else
                        {
                            angle -= Math.PI / 2f;
                        }

                        var pos = new Vector3F(BestMob.Location.X + GetRandomNumber(2.5, 5) * Math.Cos(angle),
                            BestMob.Location.Y + GetRandomNumber(2.5, 5) * Math.Sin(angle), Host.Me.Location.Z);
                        new Task(() =>
                        {
                            Host.SetCTMMovement(false);
                            Host.ForceMoveToWithLookTo(pos, Host.Me.Target.Location);
                            Host.SetCTMMovement(true);
                        }).Start();
                    }
                }

                if (BestMob?.Id == 39072)
                {
                    if (BestMob.HpPercents > 0 && BestMob.HpPercents < 90 && Host.Me.Distance(BestMob) > 2)
                    {
                        Thread.Sleep(1300);
                    }
                }


                if (Host.Me.Target != BestMob && BestMob != null)
                {
                    if (BestMob.Id == 26631 && Host.CanAttack(BestMob, Host.CanSpellAttack))
                    {
                    }
                    else
                    {
                        if (!Host.SetTarget(BestMob))
                        {
                            Host.log("Не смог выбрать цель " + Host.GetLastError(), LogLvl.Error);
                        }
                    }
                }


                /*  if (!host.CanUseSkill(skill.Id))
                  {
                      if (useskilllog) host.log("Ядро сообщает что скилл не готов [" + skill.Id + "] " + skill.Name + "   " + host.GetLastError());


                      return false;
                  }*/

                if (skill.SelfTarget)
                {
                    selfTarget = true;
                }

                Entity target = BestMob;
                if (selfTarget)
                {
                    target = Host.Me;
                }

                if (skill.Id == 197835)
                {
                    target = null;
                }

                if (skill.Id == 63560)
                {
                    target = null;
                }

                if (skill.Id == 45438)
                {
                    target = null;
                }

                if (skill.Id == 11426)
                {
                    target = null;
                }

                if (skill.Id == 12472)
                {
                    target = null;
                }

                if (skill.Id == 192081)
                {
                    target = Host.Me;
                }

                if (skill.Name.Contains("Mend Pet"))
                {
                    target = null;
                }

                if (skill.Name.Contains("Life Tap"))
                {
                    target = null;
                }

                if (skill.Name.Contains("Health Funnel"))
                {
                    target = Host.Me.GetPet();
                }

                if (skill.Name.Contains("Intimidation"))
                {
                    target = Host.Me.GetPet();
                }

                if (skill.Name.Contains("Bestial Wrath"))
                {
                    target = Host.Me.GetPet();
                }

                if (BestMob == null)
                {
                    if (!Host.CommonModule.InFight() && skill.UseInFight)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Host.CommonModule.InFight() && !skill.UseInFight)
                    {
                        return false;
                    }
                }

                if (skill.Id == 5019)
                {
                    var use = false;
                    foreach (var item in Host.ItemManager.GetItems())
                    {
                        if (item.Place != EItemPlace.Equipment)
                        {
                            continue;
                        }

                        if (item.ItemClass != EItemClass.Weapon)
                        {
                            continue;
                        }

                        if (item.ItemSubClass == 19)
                        {
                            use = true;
                        }
                    }

                    if (!use)
                    {
                        return false;
                    }
                }

                //Персонаж ХП и МП
                if (Host.Me.HpPercents < skill.MeMinHp || Host.Me.HpPercents > skill.MeMaxHp)
                {
                    if (useskilllog)
                    {
                        Host.log("HP персонажа не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                    }

                    return false;
                }

                //Эффекта нет на цели
                var findEffect = false;
                if (skill.NotTargetEffect != 0)
                {
                    if (Host.GameDB.SpellInfoEntries.ContainsKey(Convert.ToUInt32(skill.NotTargetEffect)))
                    {
                        var buff = Host.GameDB.SpellInfoEntries[Convert.ToUInt32(skill.NotTargetEffect)];
                        if (BestMob != null)
                        {
                            foreach (var aura in BestMob.GetAuras())
                            {
                                if (aura.SpellName == buff.SpellName)
                                {
                                    findEffect = true;
                                }
                            }
                        }
                    }
                    if (BestMob != null)
                    {
                        foreach (var abnormalStatuse in BestMob.GetAuras())
                        {
                            if (abnormalStatuse.SpellId == skill.NotTargetEffect)
                            {
                                findEffect = true;
                            }
                        }
                    }

                    if (findEffect)
                    {
                        if (useskilllog)
                        {
                            Host.log("Нашел эффект на цели " + skill.NotTargetEffect + " [" + skill.Id + "] " +
                                     skill.Name);
                        }

                        return false;
                    }
                }

                //Эффекта нет на персонаже
                // findEffect = false;
                if (skill.NotMeEffect != 0)
                {
                    if (Host.ClientType == EWoWClient.Classic)
                    {
                        if (Host.GameDB.SpellInfoEntries.ContainsKey(Convert.ToUInt32(skill.NotMeEffect)))
                        {
                            var buff = Host.GameDB.SpellInfoEntries[Convert.ToUInt32(skill.NotMeEffect)];
                            foreach (var aura in Host.Me.GetAuras())
                            {
                                if (aura.SpellName == buff.SpellName)
                                {
                                    if (aura.Remaining == 0)
                                    {
                                        findEffect = true;
                                    }
                                    else
                                    {
                                        if (aura.Remaining > 2000)
                                        {
                                            findEffect = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var aura in Host.Me.GetAuras())
                    {
                        if (aura.SpellId == skill.NotMeEffect)
                        {
                            if (aura.Remaining == 0)
                            {
                                findEffect = true;
                            }
                            else
                            {
                                if (aura.Remaining > 2000)
                                {
                                    findEffect = true;
                                }
                            }
                        }
                    }


                    if (findEffect)
                    {
                        if (useskilllog)
                        {
                            Host.log("Нашел эффект на персонаже " + skill.NotMeEffect + " [" + skill.Id + "] " +
                                     skill.Name);
                        }

                        return false;
                    }
                }

                //есть эффект на цели
                if (BestMob != null)
                {
                    if (skill.IsTargetEffect != 0)
                    {
                        if (Host.ClientType == EWoWClient.Classic)
                        {
                            if (Host.GameDB.SpellInfoEntries.ContainsKey(Convert.ToUInt32(skill.IsTargetEffect)))
                            {
                                var buff = Host.GameDB.SpellInfoEntries[Convert.ToUInt32(skill.IsTargetEffect)];
                                foreach (var aura in BestMob.GetAuras())
                                {
                                    if (aura.SpellName == buff.SpellName)
                                    {
                                        findEffect = true;
                                    }
                                }
                            }
                        }

                        foreach (var abnormalStatuse in BestMob.GetAuras())
                        {
                            if (abnormalStatuse.SpellId == skill.IsTargetEffect)
                            {
                                findEffect = true;
                            }
                        }

                        if (!findEffect)
                        {
                            if (useskilllog)
                            {
                                Host.log("Не нашел эффект на цели " + skill.IsTargetEffect + " [" + skill.Id + "] " +
                                         skill.Name);
                            }

                            return false;
                        }
                    }
                }


                //есть эффект на персе
                if (skill.IsMeEffect != 0)
                {
                    if (Host.GameDB.SpellInfoEntries.ContainsKey(Convert.ToUInt32(skill.IsMeEffect)))
                    {
                        var buff = Host.GameDB.SpellInfoEntries[Convert.ToUInt32(skill.IsMeEffect)];
                        foreach (var aura in Host.Me.GetAuras())
                        {
                            if (aura.SpellName == buff.SpellName)
                            {
                                if (aura.Remaining == 0)
                                {
                                    findEffect = true;
                                }
                                else
                                {
                                    if (aura.Remaining > 2000)
                                    {
                                        findEffect = true;
                                    }
                                }
                            }
                        }
                    }


                    foreach (var aura in Host.Me.GetAuras())
                    {
                        if (aura.SpellId == skill.IsMeEffect)
                        {
                            if (aura.Remaining == 0)
                            {
                                findEffect = true;
                            }
                            else
                            {
                                if (aura.Remaining > 2000)
                                {
                                    findEffect = true;
                                }
                            }
                        }
                    }

                    if (!findEffect)
                    {
                        if (useskilllog)
                        {
                            Host.log("Не нашел эффект на персонаже " + skill.IsMeEffect + " [" + skill.Id + "] " +
                                     skill.Name);
                        }

                        return false;
                    }
                }


                var preResult = Host.SpellManager.CheckCanCast(skill.Id, target);
                if (preResult != ESpellCastError.SUCCESS)
                {
                    switch (preResult)
                    {
                        case ESpellCastError.LINE_OF_SIGHT:
                            {
                                if (useskilllog)
                                {
                                    Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " +
                                             "   " + preResult + "  на " + target?.Name + "   " +
                                             Host.Me.Distance(Host.Me.Target) + "/" +
                                             Host.Me.Distance(Host.FarmModule.BestMob) + BestMob.Id + "/" +
                                             Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                        , LogLvl.Important);
                                }

                                if (Host.Me.Distance(BestMob) <= 2)
                                {
                                    Host.FarmModule.SetImmuneTarget(BestMob);
                                }
                                else if (Host.Me.Distance(BestMob) <= 5)
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 2);
                                }
                                else if (Host.Me.Distance(BestMob) <= 10)
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 5);
                                }
                                else if (Host.Me.Distance(BestMob) < 20)
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 10);
                                }
                                else
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 19);
                                }

                                return true;
                            }

                        case ESpellCastError.OUT_OF_RANGE:
                            {
                                if (useskilllog)
                                {
                                    Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " +
                                             "   " + preResult + "  на " + target?.Name + "   " +
                                             Host.Me.Distance(Host.Me.Target) + "/" +
                                             Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager
                                                 .GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                        , LogLvl.Important);
                                }
                            }
                            break;
                        case ESpellCastError.UNIT_NOT_INFRONT:
                            {
                                if (useskilllog)
                                {
                                    Host.log("Плохой угол, поворачиваюсь UNIT_NOT_INFRONT дист:" +
                                             Host.Me.Distance(BestMob) + "   " + Host.Me.GetAngle(BestMob));
                                }
                                //  Host.ForceMoveToWithLookTo(new Vector3F(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z), BestMob.Location);
                                Host.TurnDirectly(BestMob.Location);
                                Thread.Sleep(100);
                                if (useskilllog)
                                {
                                    Host.log("Плохой угол, повернулся UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) +
                                             "   " + Host.Me.GetAngle(BestMob.Location));
                                }
                                // Host.CommonModule.ForceMoveTo(bestMob, Host.Me.Distance(bestMob) - 1);
                            }
                            break;

                        case ESpellCastError.NO_POWER:
                            {
                                /*  Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                               Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                           , Host.LogLvl.Important);*/
                                return false;
                            }

                        case ESpellCastError.NOT_READY:
                            {
                                /* Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " + "   " + preResult + "  на " + target.Name + "   " +
                                              Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                           , LogLvl.Important);*/
                                return false;
                            }

                        case ESpellCastError.NOT_SHAPESHIFT:
                            {
                                if (Host.ClientType == EWoWClient.Classic)
                                {
                                    if (useskilllog)
                                    {
                                        Host.log("Снимаю облик для использования скила " + skill.Id + "[" + skill.Name + "]", LogLvl.Important);
                                    }

                                    Host.CanselForm();
                                    Thread.Sleep(1000);
                                }

                                if (skill.Id == 8936 || skill.Id == 18562 || skill.Id == 774) // [18562] Быстрое восстановление [774] Омоложение
                                {
                                    var isNeedCanselForm = true;
                                    foreach (var aura in Host.Me.GetAuras())
                                    {
                                        if (aura.SpellId == 69369)
                                        {
                                            isNeedCanselForm = false;
                                        }

                                        if (aura.SpellId == 24858 && skill.Id == 8936) //Облик лунного совуха    
                                        {
                                            isNeedCanselForm = false;
                                        }
                                    }

                                    if (isNeedCanselForm)
                                    {
                                        if (useskilllog)
                                        {
                                            Host.log("Снимаю облик для использования скила " + skill.Id + "[" + skill.Name + "]", LogLvl.Important);
                                        }

                                        Host.CanselForm();
                                        Thread.Sleep(200);
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                if (useskilllog)
                                {
                                    Host.log("Не смог использовать скилл: " + " [" + skill.Id + "] " + skill.Name + " " +
                                             "   " + preResult + "  на " + target?.Name + "   " +
                                             Host.Me.Distance(Host.Me.Target) + "/" +
                                             Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager
                                                 .GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                        , LogLvl.Important);
                                }
                            }
                            return false;
                    }
                }
                // Host.log("Пытаюсь использовать скилл  " + skill.Name +"[" + skill.Id + "]   " + Host.SpellManager.HasGlobalCooldown(skill.Id) + "  " + Host.SpellManager.IsSpellReady(skill.Id) + "   Время " + Host.ComboRoute.swUseSkill.ElapsedMilliseconds + " Приоритет: " + skill.Priority);


                var alternatePower = EPowerType.AlternatePower;
                switch (Host.Me.Class)
                {
                    case EClass.Druid:
                    case EClass.Rogue:
                        alternatePower = EPowerType.ComboPoints;
                        break;
                    case EClass.Monk:
                        alternatePower = EPowerType.Chi;
                        break;
                }

                if (skill.CombatElementCountLess != 0)
                {
                    if (Host.Me.GetPower(alternatePower) > skill.CombatElementCountLess)
                    {
                        if (useskilllog)
                        {
                            Host.log("Кол-во AlternatePower больше нужного " + skill.CombatElementCountLess + "/" + Host.Me.GetPower(alternatePower) + " [" + skill.Id + "] " + skill.Name);
                        }

                        return false;
                    }
                }

                if (skill.CombatElementCountMore != 0)
                {
                    if (Host.Me.GetPower(alternatePower) < skill.CombatElementCountMore)
                    {
                        if (useskilllog)
                        {
                            Host.log("Кол-во AlternatePower меньше нужного " + skill.CombatElementCountMore + "/" +
                                     Host.Me.GetPower(alternatePower) + " [" + skill.Id + "] " + skill.Name);
                        }

                        return false;
                    }
                }

                if (Host.Me.GetPet() != null)
                //  if (skill.Name.Contains("Mend Pet") || skill.Name.Contains("Health Funnel"))
                {
                    if (Host.Me.GetPet().HpPercents < skill.PetMinHp || Host.Me.GetPet().HpPercents > skill.PetMaxHp)
                    {
                        if (useskilllog)
                        {
                            Host.log("HP питомца не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                        }

                        return false;
                    }
                }




                if (skill.TargetId != 0)
                {
                    if (skill.TargetId != BestMob.Id)
                    {
                        if (useskilllog)
                        {
                            Host.log("ID цели не соответсвует условиям " + skill.TargetId + " != " + BestMob.Id + "[" +
                                     skill.Id + "] " + skill.Name);
                        }

                        return false;
                    }
                }

                if (skill.NotTargetId != 0)
                {
                    if (skill.NotTargetId == BestMob.Id)
                    {
                        if (useskilllog)
                        {
                            Host.log("ID цели не соответсвует условиям " + skill.NotTargetId + " != " + BestMob.Id +
                                     "[" + skill.Id + "] " + skill.Name);
                        }

                        return false;
                    }
                }

                if (Host.Me.Level < skill.MinMeLevel || Host.Me.Level > skill.MaxMeLevel)
                {
                    if (useskilllog)
                    {
                        Host.log("Уровень персонажа не соответсвует условиям [" + skill.Id + "] " + skill.Name + " " +
                                 Host.Me.Level + "<>" + skill.MinMeLevel + " " + skill.MaxMeLevel);
                    }

                    return false;
                }


                decimal power = Host.Me.GetPower(Host.Me.PowerType);
                decimal maxPower = Host.Me.GetMaxPower(Host.Me.PowerType);
                var percent = power * 100 / maxPower;
                if (power != 0)
                {
                    if (maxPower != 0)
                    {
                        if (percent <= skill.MeMinMp || percent > skill.MeMaxMp)
                        {
                            if (useskilllog)
                            {
                                Host.log("MP персонажа не соответсвует условиям " +
                                         Host.Me.GetPower(Host.Me.PowerType) + " / " +
                                         Host.Me.GetMaxPower(Host.Me.PowerType) + "     " + Host.MeMpPercent() + "/" +
                                         skill.MeMinMp + "/" + skill.MeMaxMp + " [" + skill.Id + "] " + skill.Name);
                            }

                            return false;
                        }
                    }
                }



                //ХП цели
                if (BestMob?.HpPercents < skill.TargetMinHp || BestMob?.HpPercents > skill.TargetMaxHp)
                {
                    if (useskilllog)
                    {
                        Host.log("HP цели не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                    }

                    return false;
                }


                if (BestMob?.Hp == 0)
                {
                    _attackMoveFailCount++;
                }

                if (skill.MoveDist)
                {
                    if (!selfTarget)
                    {
                        var dist = skill.MaxDist;
                        if (!Host.Me.IsInCombat && Host.CharacterSettings.RandomDistForAttack)
                        {
                            var rand = Host.RandGenerator.Next(0, Host.CharacterSettings.RandomDistForAttackCount);
                            if (dist - rand > skill.MinDist)
                            {
                                dist -= rand;
                            }
                        }

                        if (Host.Me.Distance(BestMob) >= dist)
                        {
                            if (useskilllog)
                            {
                                Host.log("Подбегаю что бы использовать [" + skill.Id + "] " + skill.Name + "  дист: " + Host.Me.Distance(BestMob) + "  " + dist);
                            }

                            if (Host.Me.Distance(BestMob) >= dist)
                            {
                                if (useskilllog)
                                {
                                    Host.log("Начинаю бег " + "  дист: " + Host.Me.Distance(BestMob));
                                }

                                if (!Host.CommonModule.ForceMoveTo(BestMob, dist - 1))
                                {
                                    if (Host.GetLastError() != ELastError.Movement_MoveCanceled)
                                    {
                                        Host.log("Ошибка передвижения к цели: " + Host.GetLastError() + "(" + _attackMoveFailCount + ")", LogLvl.Error);
                                    }
                                }

                                if (useskilllog)
                                {
                                    Host.log("Закончил бег " + "  дист: " + Host.Me.Distance(BestMob));
                                }

                                if (!spellInstant)
                                {
                                    Host.CancelMoveTo();
                                }

                                if (Host.GetLastError() == ELastError.Movement_MovePossibleFullStop)
                                {
                                    _attackMoveFailCount++;
                                }
                                else
                                {
                                    _attackMoveFailCount = 0;
                                }

                                /* if (!Host.CommonModule.InFight() && Host.CharacterSettings.Mode != Mode.QuestingClassic && Host.Me.Class != EClass.Hunter && !Host.CharacterSettings.KillMobFirst)
                                 {
                                     Host.log("Плохая цель 33 :" + BestMob.Name);
                                     BestMob = GetBestMob();
                                 }*/

                                return true;
                            }
                        }
                    }
                }

                //Дистанция Подбегание
                if (_attackMoveFailCount > 10)
                {
                    Host.log("Плохая цель 3 :" + BestMob.Name);
                    SetBadTarget(BestMob, 60000);
                    BestMob = null;
                    Host.Evade = true;
                    _attackMoveFailCount = 0;
                    return true;
                }

                if (Host.Me.GetAngle(BestMob) > 20 && Host.Me.GetAngle(BestMob) < 340)
                {
                    if (useskilllog)
                    {
                        Host.log("Плохой угол, поворачиваюсь к мобу " + Host.Me.Distance(BestMob) + "  " +
                                 Host.Me.GetAngle(BestMob));
                    }

                    Host.TurnDirectly(BestMob.Location);
                    Thread.Sleep(100);
                    if (useskilllog)
                    {
                        Host.log("Плохой угол, повернулся к мобу " + Host.Me.Distance(BestMob) + "  " +
                                 Host.Me.GetAngle(BestMob));
                    }
                }


                //Дистанция - максимальная
                if (Host.Me.Distance(BestMob) > skill.MaxDist + 1)
                {
                    if (!selfTarget)
                    {
                        if (useskilllog)
                        {
                            Host.log("Максимальаня дистанция не соответсвует условиям [" + skill.Id + "] " +
                                     skill.Name);
                        }

                        return false;
                    }
                }

                //Дистанция минимальная
                if (Host.Me.Distance(BestMob) < skill.MinDist)
                {
                    if (!selfTarget)
                    {
                        if (useskilllog)
                        {
                            Host.log("Минимальная дистанция не соответсвует условиям [" + skill.Id + "] " + skill.Name);
                        }

                        return false;
                    }
                }

                //Аое
                Unit aoeTarget;
                if (skill.AoeMe)
                {
                    aoeTarget = Host.Me; //Рядом со мной
                }
                else
                {
                    aoeTarget = BestMob;
                }

                if (NearTargetCount(aoeTarget, skill.AoeRadius) < skill.AoeMin ||
                    NearTargetCount(aoeTarget, skill.AoeRadius) > skill.AoeMax)
                {
                    if (useskilllog)
                    {
                        Host.log("Кол-во целей не соответсвует условиям [" + skill.Id + "] " + skill.Name + "   " + NearTargetCount(aoeTarget, skill.AoeRadius) + " " + skill.AoeMin + "/" + skill.AoeMax);
                    }

                    return false;
                }


                if ( /*!selfTarget && bestMob != null && */BestMob != null && !Host.IsAlive(BestMob))
                {
                    if (useskilllog)
                    {
                        Host.log("Моб умер " + skill.Id + " " + skill.Name);
                    }

                    return true;
                }


                switch (Host.Me.Class)
                {
                    default:
                        {
                            if (BestMob != null)
                            {
                                if (skill.Id != 921)
                                {
                                    if (Host.SpellManager.AutoAttackingGuid == WowGuid.Zero && Host.Me.Distance(Host.FarmModule.BestMob) < 4)
                                    {
                                        if (useskilllog)
                                        {
                                            Host.log("Начинаю авто атаку " + Host.SpellManager.AutoAttackingGuid);
                                        }

                                        if (!Host.SpellManager.StartAutoAttack(Host.FarmModule.BestMob))
                                        {
                                            Host.CommonModule.ForceMoveTo(BestMob, 2);
                                            Host.log("Авто атака " + Host.GetLastError() + " " + Host.SpellManager.AutoAttackingGuid, LogLvl.Error);
                                        }
                                        else
                                        {
                                            if (useskilllog)
                                            {
                                                Host.log("Авто атака" + Host.SpellManager.AutoAttackingGuid);
                                            }
                                        }
                                    }
                                }



                                if (skill.Id == 6603 && Host.SpellManager.AutoAttackingGuid != WowGuid.Zero && Host.CharacterSettings.KillRunaways)
                                {
                                    if (Host.Me.Distance(BestMob) > 3)
                                    {
                                        Host.log("Моб убегает " + _attackMoveFailCount);
                                        if (Host.GetAgroCreatures().Count > 1)
                                        {
                                            BestMob = GetBestMob();
                                        }
                                        else
                                        {
                                            Host.CommonModule.ForceMoveTo(BestMob.Location, 2);
                                            _attackMoveFailCount++;
                                        }

                                    }
                                    return false;
                                }
                            }

                            if (skill.Id == Host.SpellManager.CurrentAutoRepeatSpellId)
                            {
                                return false;
                            }
                        }
                        break;
                }

                if (skill.Id == 6603)
                {
                    return false;
                }

                if (skill.Id == 275699)
                {
                    var use = false;
                    foreach (var aura in BestMob.GetAuras())
                    {
                        if (aura.SpellId == 194310 && aura.StackOrCharges > 3)
                        {
                            use = true;
                        }
                    }

                    if (!use)
                    {
                        return false;
                    }
                }

                if (BestMob == null)
                {
                    if (skill.Name.Contains("Aspect of the Hawk") && Host.SpellManager.GetSpell(5118) != null)
                    {
                        return false;
                    }
                }

                ESpellCastError result;


                if (useskilllog)
                {
                    Host.log(
                        "Пытаюсь использовать: [" + skill.Id + "] " + skill.Name + "   Время " +
                        Host.FarmModule._swUseSkill.ElapsedMilliseconds + " Приоритет:" + skill.Priority,
                        LogLvl.Important);
                }

                if (!spellInstant)
                {
                    Host.CancelMoveTo();
                }

                Host.MyCheckIsMovingIsCasting(spellInstant);
                while (Host.SpellManager.IsCasting || Host.SpellManager.IsChanneling || Host.SpellManager.HasGlobalCooldown(skill.Id))
                {
                    if (!Host.Me.IsAlive)
                    {
                        return false;
                    }

                    if (!BestMob.IsAlive)
                    {
                        Host.SpellManager.CancelSpellCast();
                        Host.SpellManager.CancelSpellChanneling();
                    }
                    // host.log("IsCasting: " + host.SpellManager.IsCasting + "  IsMoving:" + host.Me.IsMoving);
                    Thread.Sleep(50);
                }

                if (Host.CharacterSettings.Mode == Mode.Questing)
                {
                    if (Host.SpellManager.GetSpell(301690) != null)
                    {
                        if (Host.SpellManager.CheckCanCast(301690, Host.Me.Target) == ESpellCastError.SUCCESS)
                        {
                            result = Host.SpellManager.CastSpell(301690);
                            if (result != ESpellCastError.SUCCESS)
                            {
                                Host.log("Не смог использовать скил 301690 " + result + " " + Host.GetLastError(),
                                    LogLvl.Error);
                            }

                            return false;
                        }
                    }
                }


                if (Host.Me.Class == EClass.Mage || Host.Me.Class == EClass.Warlock || Host.Me.Class == EClass.Priest)
                {
                    if (Host.SpellManager.CurrentAutoRepeatSpellId == 5019 && skill.Id != 5019)
                    {
                        Host.SpellManager.CancelAutoRepeatSpell();
                        Thread.Sleep(500);
                    }
                }
                var checkCast = true;
                new Task(() =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        while (checkCast)
                        {
                            Thread.Sleep(100);
                            if (Host.Me.GetAngle(BestMob) > 10 && Host.Me.GetAngle(BestMob) < 350)
                            {
                                if (useskilllog)
                                {
                                    Host.log("Плохой угол, поворачиваюсь к мобу " + Host.Me.Distance(BestMob) + "  " + Host.Me.GetAngle(BestMob));
                                }

                                Host.TurnDirectly(BestMob.Location);
                                Thread.Sleep(100);
                                if (useskilllog)
                                {
                                    Host.log("Плохой угол, повернулся к мобу " + Host.Me.Distance(BestMob) + "  " + Host.Me.GetAngle(BestMob));
                                }
                            }
                        }
                    }).Start();

                /*  if (BestMob != null)
                  {
                      if (Host.Me.Class == EClass.Rogue)
                      {
                          var unikSpell = Host.SpellManager.GetSpell(1766);
                          if (unikSpell != null && BestMob.IsCasting && Host.Me.Distance(BestMob) < 3 && Host.Me.IsInCombat && BestMob.HpPercents < 99)
                          {
                              if (Host.AdvancedLog)
                                  Host.log("Пытаюсь использовать пинок");
                              if (UseSkillAndWait(unikSpell.Id, BestMob))
                                  return true;
                          }
                      }
                  }*/



                result = Host.SpellManager.CastSpell(skill.Id, target);
                // Thread.Sleep(1000);
                //использую скилл
                while (Host.SpellManager.IsCasting || Host.SpellManager.IsChanneling)
                {
                    if (!Host.Me.IsAlive)
                    {
                        return false;
                    }

                    if (BestMob != null && !BestMob.IsAlive)
                    {
                        Host.SpellManager.CancelSpellCast();
                        Host.SpellManager.CancelSpellChanneling();
                    }

                    //  host.log("IsCasting: " + host.SpellManager.IsCasting + "  IsMoving:" + host.Me.IsMoving);
                    Thread.Sleep(50);
                }

                checkCast = false;
                if (result != ESpellCastError.SUCCESS)
                {
                    var le = Host.GetLastError();

                    switch (result)
                    {
                        case ESpellCastError.LINE_OF_SIGHT:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " +
                                         "   " + result + "  на " + target?.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(target) + "/" +
                                         Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                    , LogLvl.Error);
                                if (Host.Me.Distance(BestMob) <= 2)
                                {
                                    Host.FarmModule.SetImmuneTarget(BestMob);
                                }
                                else if (Host.Me.Distance(BestMob) <= 5)
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 2);
                                }
                                else if (Host.Me.Distance(BestMob) <= 15)
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 5);
                                }
                                else if (Host.Me.Distance(BestMob) < 20)
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 10);
                                }
                                else
                                {
                                    Host.CommonModule.ForceMoveTo(BestMob, 19);
                                }

                                return true;
                            }

                        case ESpellCastError.OUT_OF_RANGE:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " +
                                         "   " + result + "  на " + target?.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(target) + "/" +
                                         Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                    , LogLvl.Error);

                                switch (skill.Id)
                                {
                                    case 1978:
                                    case 3044:
                                    case 75:
                                        {
                                            if (Host.Me.Distance(target) > 30)
                                            {
                                                if (Host.Me.Distance(BestMob) <= 2)
                                                {
                                                    Host.FarmModule.SetImmuneTarget(BestMob);
                                                }
                                                else if (Host.Me.Distance(BestMob) <= 5)
                                                {
                                                    Host.CommonModule.ForceMoveTo(BestMob, 2);
                                                }
                                                else if (Host.Me.Distance(BestMob) <= 10)
                                                {
                                                    Host.CommonModule.ForceMoveTo(BestMob, 5);
                                                }
                                                else if (Host.Me.Distance(BestMob) < 20)
                                                {
                                                    Host.CommonModule.ForceMoveTo(BestMob, 10);
                                                }
                                                else
                                                {
                                                    Host.CommonModule.ForceMoveTo(BestMob, 19);
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            if (Host.Me.Distance(BestMob) <= 2)
                                            {
                                                Host.FarmModule.SetImmuneTarget(BestMob);
                                            }
                                            else if (Host.Me.Distance(BestMob) <= 5)
                                            {
                                                Host.CommonModule.ForceMoveTo(BestMob, 2);
                                            }
                                            else if (Host.Me.Distance(BestMob) <= 10)
                                            {
                                                Host.CommonModule.ForceMoveTo(BestMob, 5);
                                            }
                                            else if (Host.Me.Distance(BestMob) < 20)
                                            {
                                                Host.CommonModule.ForceMoveTo(BestMob, 10);
                                            }
                                            else
                                            {
                                                Host.CommonModule.ForceMoveTo(BestMob, 19);
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case ESpellCastError.UNIT_NOT_INFRONT:
                            {
                                Host.log("Плохой угол, поворачиваюсь UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) +
                                         "   " + Host.Me.GetAngle(BestMob));
                                // Host.ForceMoveToWithLookTo(new Vector3F(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z), BestMob.Location);
                                Host.TurnDirectly(BestMob.Location);
                                Host.log("Плохой угол, повернулся UNIT_NOT_INFRONT дист:" + Host.Me.Distance(BestMob) +
                                         "   " + Host.Me.GetAngle(BestMob));
                            }
                            break;

                        case ESpellCastError.NO_POWER:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " +
                                         "   " + result + "  на " + target?.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" +
                                         Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager
                                             .GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                    , LogLvl.Error);
                                return false;
                            }

                        case ESpellCastError.NOT_READY:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " +
                                         "   " + result + "  на " + target?.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" +
                                         Host.Me.Distance(Host.FarmModule.BestMob) + "/" + Host.SpellManager
                                             .GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target)
                                    , LogLvl.Error);
                                return false;
                            }

                        case ESpellCastError.NOT_STANDING:
                            {
                                Host.ChangeStandState(EStandState.Stand);
                                // Host.MoveTo(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z);
                                return true;
                            }

                        case ESpellCastError.INTERRUPTED:
                        case ESpellCastError.TARGETS_DEAD:
                            break;

                        case ESpellCastError.SPELLOK_EVADE:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " +
                                         "   " + result + "  на " + target?.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(target) + "/" +
                                         Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target) + "    " + Host.SpellManager.CurrentAutoRepeatSpellId + "  " + Host.SpellManager.AutoAttackingGuid
                                    , LogLvl.Error);
                                SetBadTarget(BestMob, 60000);
                                BestMob = null;
                                Host.Evade = true;
                                
                            }
                            break;

                        default:
                            {
                                Host.log("Не смог использовать скилл: " + le + " [" + skill.Id + "] " + skill.Name + " " +
                                         "   " + result + "  на " + target?.Name + "   " +
                                         Host.Me.Distance(Host.Me.Target) + "/" + Host.Me.Distance(target) + "/" +
                                         Host.SpellManager.GetSpell(skill.Id).GetMaxCastRange(Host.Me.Target) + "    " + Host.SpellManager.CurrentAutoRepeatSpellId + "  " + Host.SpellManager.AutoAttackingGuid
                                    , LogLvl.Error);
                                if (Host.Me.Class == EClass.Hunter)
                                {
                                    if (Host.Me.Distance(target) > 8 && Host.SpellManager.AutoAttackingGuid != WowGuid.Zero)
                                    {
                                        Host.log("Отменяю авто атаку");
                                        Host.SpellManager.StopAutoAttack();
                                        Host.SpellManager.CancelAutoRepeatSpell();
                                    }
                                }
                            }
                            break;
                    }


                    if (BestMob?.HpPercents == 100 && le != ELastError.Unknown)
                    {
                        _attackMoveFailCount++;
                    }

                    Thread.Sleep(500);
                    return false;
                }
                else
                {
                    _attackMoveFailCount = 0;
                    if (useskilllog)
                    {
                        Host.log("Использую скилл: [" + skill.Id + "] " + skill.Name + " на " + target?.Name + "   Время " + Host.FarmModule._swUseSkill.ElapsedMilliseconds + "    " + Host.SpellManager.CurrentAutoRepeatSpellId + "  ", LogLvl.Ok);
                    }

                    Host.FarmModule._swUseSkill.Reset();
                    if (selfTarget)
                    {
                        Thread.Sleep(1000);
                    }
                    if (skill.NotTargetEffect != 0)
                    {
                        Thread.Sleep(1500);

                    }

                    switch (skill.Id)
                    {
                        case 78:
                            Thread.Sleep(400);
                            break;
                        case 198013: //Пронзающий взгляд
                            Thread.Sleep(3000);
                            break;
                        case 921: //Обшаривание карманов
                            {
                                var waitTill = DateTime.UtcNow.AddSeconds(1);
                                while (waitTill > DateTime.UtcNow)
                                {
                                    Thread.Sleep(100);
                                    if (!Host.MainForm.On)
                                    {
                                        return false;
                                    }

                                    if (Host.CanPickupLoot())
                                    {
                                        break;
                                    }
                                }
                                if (Host.CanPickupLoot())
                                {
                                    Host.PickupLoot();
                                }
                            }
                            return false;
                        case 5384://Притвориться мертвым
                            {
                                Thread.Sleep(3000);
                                while (Host.MyGetAura(5384) != null)
                                {
                                    Thread.Sleep(1000);
                                    if (!Host.MainForm.On)
                                    {
                                        return true;
                                    }

                                    var needStand = true;
                                    foreach (var entity in Host.GetEntities<Unit>())
                                    {
                                        if (!entity.IsAlive)
                                        {
                                            continue;
                                        }

                                        if (entity.GetReactionTo(Host.Me) == EReputationRank.Neutral)
                                        {
                                            continue;
                                        }

                                        if (entity.Distance(Host.Me) < skill.MaxDist)
                                        {
                                            needStand = false;
                                        }
                                    }
                                    if (needStand)
                                    {
                                        break;
                                    }
                                }
                                break;
                            }
                    }

                    while (Host.SpellManager.IsCasting)
                    {
                        Thread.Sleep(100);
                    }
                    return true;
                }
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }

        private void UseSpecialSkills(float specialDist)
        {
            try
            {
                if (FarmState == FarmState.FarmProps)
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 48576)
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 13523) //:  Власть над приливами[13523
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 13576) //:  13576 State:None LogTitle:Взаимная помощь 
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 48573) //: :  Жизни кроколисков[48573
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 49078 && Host.Me.Target?.Id != 128071)
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 47943 && Host.Me.Target?.Id != 123814)
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 48532)
                {
                    var use = false;
                    foreach (var entity in Host.GetEntities())
                    {
                        if (Host.Me.Distance(entity) > 10)
                        {
                            continue;
                        }

                        if (entity.Id == 126610 || entity.Id == 126627)
                        {
                            use = true;
                        }
                    }

                    if (!use)
                    {
                        return;
                    }
                }

                if (Host.AutoQuests?.BestQuestId == 47622)
                {
                    var use = false;
                    foreach (var entity in Host.GetEntities())
                    {
                        if (Host.Me.Distance(entity) > 10)
                        {
                            continue;
                        }

                        if (entity.Id == 123121 || entity.Id == 123116)
                        {
                            use = true;
                        }
                    }

                    if (!use)
                    {
                        return;
                    }
                }


                if (Host.AutoQuests?.BestQuestId == 50771 && Host.FarmModule.BestMob?.Id != 135080)
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 47924 && Host.FarmModule.BestMob?.Id != 124949)
                {
                    return;
                }

                if (Host.AutoQuests?.BestQuestId == 49125 && Host.FarmModule.BestMob?.Id != 138536)
                {
                    return;
                }

                if (SpecialItems != null)
                {
                    for (var i = 0; i < SpecialItems?.Length; i++)
                    {
                        switch (Host.AutoQuests?.BestQuestId)
                        {
                            case 47130:
                                {
                                    if (Host.GetAgroCreatures().Count > 0)
                                    {
                                        continue;
                                    }
                                }
                                break;

                            case 49666:
                                {
                                    if (Host.Me.Target == null || Host.Me.Target.IsAlive)
                                    {
                                        continue;
                                    }
                                }
                                break;
                            default:
                                {
                                    if (Host.Me.Target == null || !Host.Me.Target.IsAlive)
                                    {
                                        continue;
                                    }
                                }
                                break;
                        }


                        if (Host.Me.Distance(Host.Me.Target) > specialDist)
                        {
                            if (!Host.CommonModule.ForceMoveTo(Host.Me.Target, specialDist))
                            {
                                if (_failMoveUseSpecialSkill > 4)
                                {
                                    //Плохая цель
                                    Host.log("Плохая цель 4 :" + Host.FarmModule.BestMob.Name, LogLvl.Error);
                                    Host.FarmModule.SetBadTarget(Host.FarmModule.BestMob, 30000);
                                    Host.FarmModule.BestMob = null;
                                    _failMoveUseSpecialSkill = 0;
                                    Host.Evade = true;
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
                        }

                        Host.TurnDirectly(Host.Me.Target.Location);
                        var spItem = Host.MyGetItem(Convert.ToUInt32(SpecialItems[i]));

                        if (spItem != null)
                        {
                            if (Host.SpellManager.GetItemCooldown(spItem.Id) <= 0)
                            {
                                Host.MyCheckIsMovingIsCasting();
                                while (Host.SpellManager.IsChanneling)
                                {
                                    Thread.Sleep(50);
                                }

                                Thread.Sleep(500);
                                EInventoryResult result;
                                switch (spItem.Id)
                                {
                                    case 153012:
                                        {
                                            result = Host.SpellManager.UseItem(spItem, Host.Me.Target);
                                        }
                                        break;
                                    case 151763:
                                        {
                                            result = Host.SpellManager.UseItem(spItem, Host.Me.Target);
                                        }
                                        break;
                                    case 160559:
                                        {
                                            result = Host.SpellManager.UseItem(spItem, Host.Me.Target);
                                        }
                                        break;

                                    case 152610:
                                        {
                                            result = Host.SpellManager.UseItem(spItem, Host.Me.Target);
                                        }
                                        break;
                                    default:
                                        {
                                            result = Host.SpellManager.UseItem(spItem);
                                        }
                                        break;
                                }


                                if (result == EInventoryResult.OK)
                                {
                                    Host.log("Использую " + spItem.Name + "[" + spItem.Id + "] " + spItem.Place,
                                        LogLvl.Ok);
                                    Host.FarmModule.SetBadTarget(Host.FarmModule.BestMob, 60000);
                                    Host.FarmModule.BestMob = null;
                                }
                                else
                                {
                                    Host.log(
                                        "Не получилось использовать 2 " + spItem.Name + "[" + SpecialItems[i] + "]  " +
                                        result + "  " + Host.GetLastError() + "  " + Host.FarmModule.BestMob.Guid,
                                        LogLvl.Error);
                                    Host.FarmModule.SetBadTarget(Host.FarmModule.BestMob, 60000);
                                    Host.FarmModule.BestMob = null;
                                }

                                Thread.Sleep(1000);
                                Host.MyCheckIsMovingIsCasting();
                                while (Host.SpellManager.IsChanneling)
                                {
                                    Thread.Sleep(50);
                                }
                            }
                            else
                            {
                                /*   if (host.AdvancedLog)
                                       host.log(spItem.Name + " UseSpecialSkills GetItemCooldown:" + host.SpellManager.GetItemCooldown(spItem.Id));*/
                            }
                        }
                        else
                        {
                            Host.log("Не нашел указанный айтем " + SpecialItems[i]);
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
        }

        internal bool UseSkillAndWait(uint id, Entity target = null, bool suspendMove = true)
        {
            try
            {
                var spell = Host.SpellManager.GetSpell(id);
                if (spell == null)
                {
                    return false;
                }

                if (!Host.SpellManager.IsSpellReady(spell))
                {
                    return false;
                }

                if (Host.SpellManager.GetSpellCooldown(id) != 0)
                {
                    return false;
                }

                if (suspendMove)
                {
                    Host.MyCheckIsMovingIsCasting();
                }

                while (Host.SpellManager.IsChanneling)
                {
                    Thread.Sleep(50);
                }

                var preResult = Host.SpellManager.CheckCanCast(id, target);
                if (preResult != ESpellCastError.SUCCESS)
                {
                    switch (preResult)
                    {
                        case ESpellCastError.NOT_MOUNTED:
                            {
                                Host.CommonModule.MyUnmount();
                            }
                            break;

                        case ESpellCastError.NOT_STANDING:
                            {
                                Host.ChangeStandState(EStandState.Stand);
                                // Host.MoveTo(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z);
                            }
                            break;

                        default:
                            {
                                Host.log("Не смог использовать скилл " + spell.Name + "[" + id + "] " + preResult + " " + Host.GetLastError(), LogLvl.Important);
                                return false;
                            }
                    }
                }

                var result = Host.SpellManager.CastSpell(id, target);
                if (result != ESpellCastError.SUCCESS)
                {

                    switch (result)
                    {
                        case ESpellCastError.NOT_STANDING:
                            {
                                Host.ChangeStandState(EStandState.Stand);
                                // Host.MoveTo(Host.Me.Location.X + 1, Host.Me.Location.Y, Host.Me.Location.Z);
                            }
                            break;

                        default:
                            {
                                Host.log("Не смог использовать скилл " + spell.Name + "[" + id + "] " + result + " " + Host.GetLastError(), LogLvl.Error);
                                return false;
                            }
                    }
                }
                else
                {

                    Host.log("Использовал скилл  " + spell.Name + "[" + id + "]", LogLvl.Ok);
                    Thread.Sleep(200);
                }
                if (suspendMove)
                {
                    Host.MyCheckIsMovingIsCasting();
                }

                while (Host.SpellManager.IsChanneling)
                {
                    Thread.Sleep(50);
                }

                if (result == ESpellCastError.SUCCESS)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
                return false;
            }
        }
    }
}