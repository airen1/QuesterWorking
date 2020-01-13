using Out.Internal.Core;
using Out.Navigation;
using Out.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WowAI.Module;
using WoWBot.Core;

namespace WowAI
{
    static class TimeSpanExtensions
    {
        static public bool IsBetween(this TimeSpan time, TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime == startTime)
            {
                return true;
            }

            if (endTime < startTime)
            {
                return time <= endTime || time >= startTime;
            }

            return time >= startTime && time <= endTime;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal partial class Host
    {
        public DrawObject MyDrawRadius(Vector3F loc, double radius)
        {
            var drawRadius = new DrawObject
            {
                Mode = EDrawMode.Line,
                Color = Color.Red,
                Type = EDrawObject.Circle,
                Points = new List<Vector3F> { loc },
                LineWidth = 1,
                Radius = Convert.ToSingle(radius)
            };
            return drawRadius;
        }

        public DrawObject MyDrawPoint(Vector3F loc)
        {
            var drawRadius = new DrawObject
            {
                Mode = EDrawMode.Fill,
                Color = Color.Blue,
                Type = EDrawObject.Circle,
                Points = new List<Vector3F> { loc },
                Radius = 0.1f,
                LineWidth = 1,

            };
            return drawRadius;
        }


        public static bool IsInRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck >= startDate && dateToCheck <= endDate;
        }

        public void MyCheckIsMovingIsCasting(bool waitMove = true)
        {
            var fixMove = 0;
            while (SpellManager.IsCasting || Me.IsMoving)
            {
                if (!waitMove)
                {
                    return;
                }

                if (Me.IsMoving)
                {
                    fixMove++;
                    if (fixMove > 5)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        log("Ожидаю остановки " + fixMove + "   " + waitMove + "  " + SpellManager.IsCasting + "    " + Me.IsMoving);
                    }
                }

                Thread.Sleep(100);
                if (!Me.IsAlive)
                {
                    return;
                }

                if (!MainForm.On)
                {
                    return;
                }

                
                   
                if (fixMove > 50)
                {
                    SetMoveStateForClient(true);
                    if (RandGenerator.Next(0, 2) == 0)
                    {
                        StrafeLeft(true);
                    }
                    else
                    {
                        StrafeRight(true);
                    }

                    Thread.Sleep(1000);
                    StrafeRight(false);
                    StrafeLeft(false);
                    SetMoveStateForClient(false);
                    CancelMoveTo();
                    fixMove = 0;
                }
            }
        }

        public void MyWait(int time)
        {
            var waitTime = time;
            while (waitTime > 0)
            {
                if (!MainForm.On)
                {
                    return;
                }

                Thread.Sleep(1000);
                waitTime -= 1000;
                log("Ожидаю " + waitTime + "/" + time);
            }
        }



        public void FlyForm()
        {
            CancelMoveTo();
            Thread.Sleep(1000);
            if (Me.Class == EClass.Druid)
            {
                if (MyGetAura(783) == null)
                {
                    CanselForm();
                }

                while (MyGetAura(783) == null)
                {
                    if (!MainForm.On)
                    {
                        return;
                    }

                    foreach (var spell in SpellManager.GetSpells())
                    {
                        if (spell.Id == 783)
                        {
                            var resultForm = SpellManager.CastSpell(spell.Id);
                            if (resultForm != ESpellCastError.SUCCESS)
                            {
                                if (AdvancedLog)
                                {
                                    log("Не удалось поменять форму " + spell.Name + "  " + resultForm, LogLvl.Error);
                                }

                                if (resultForm == ESpellCastError.NOT_MOUNTED)
                                {
                                    CommonModule.MyUnmount();
                                }
                            }

                            while (SpellManager.IsCasting)
                            {
                                Thread.Sleep(100);
                            }

                            Thread.Sleep(2000);
                        }
                    }
                }
            }



            while (Me.Class != EClass.Druid)
            {
                Thread.Sleep(1000);
                if (Me.MountId != 0)
                {
                    return;
                }

                Spell mountSpell = null;
                foreach (var s in SpellManager.GetSpells())
                {
                    if (!s.SkillLines.Contains(777))
                    {
                        continue;
                    }

                    if (s.Id != 32297)
                    {
                        continue;
                    }

                    mountSpell = s;
                    break;
                }
                if (mountSpell == null)
                {
                    continue;
                }

                CancelMoveTo();
                Thread.Sleep(500);
                MyCheckIsMovingIsCasting();
                var result = SpellManager.CastSpell(mountSpell.Id);

                if (result != ESpellCastError.SUCCESS)
                {
                    log("Не удалось призвать маунта " + mountSpell.Name + "  " + result, LogLvl.Error);
                }
                else
                {
                    log("Призвал маунта", LogLvl.Ok);
                    while (SpellManager.IsCasting)
                    {
                        Thread.Sleep(100);
                    }

                    return;
                }
            }
            Jump();
            Thread.Sleep(1000);
            Jump();
        }

        public Unit MyMoveToAuction()
        {
            if (ClientType == EWoWClient.Classic)
            {
                if (Me.Team == ETeam.Horde)
                {

                    if (Area.Id == 440)
                    {
                        if (!CommonModule.MoveTo(-7237, -3805, -1))
                        {
                            return null;
                        }

                        foreach (var entity in GetEntities<Unit>())
                        {
                            if (!entity.IsAuctioner)
                            {
                                continue;
                            }

                            if (entity.Id == 8661)
                            {
                                return entity;
                            }
                        }
                    }

                    if (Area.Id == 1497)
                    {
                        if (!CommonModule.MoveTo(1647.97, 256.77, -56.87))
                        {
                            return null;
                        }

                        foreach (var entity in GetEntities<Unit>())
                        {
                            if (!entity.IsAuctioner)
                            {
                                continue;
                            }

                            if (entity.Id == 15683)
                            {
                                return entity;
                            }
                        }
                    }

                    if (Area.Id == 1637)
                    {
                        if (!CommonModule.MoveTo(1668.77, -4459.48, 18.84))
                        {
                            return null;
                        }

                        foreach (var entity in GetEntities<Unit>())
                        {
                            if (!entity.IsAuctioner)
                            {
                                continue;
                            }

                            if (entity.Id == 8724)
                            {
                                return entity;
                            }
                        }
                    }

                    if (Area.Id == 1638)
                    {
                        if (!CommonModule.MoveTo(-1201.17, 110.89, 134.80))
                        {
                            return null;
                        }

                        foreach (var entity in GetEntities<Unit>())
                        {
                            if (!entity.IsAuctioner)
                            {
                                continue;
                            }

                            if (entity.Id == 8722)
                            {
                                return entity;
                            }
                        }
                    }

                }

                if (Me.Team == ETeam.Alliance)
                {
                    if (!CommonModule.MoveTo(-8813.21, 662.48, 95.42))
                    {
                        return null;
                    }

                    foreach (var entity in GetEntities<Unit>())
                    {
                        if (!entity.IsAuctioner)
                        {
                            continue;
                        }

                        if (entity.Id == 15659)
                        {
                            return entity;
                        }
                    }
                }

                return null;
            }

            if (Me.Team == ETeam.Horde)
            {
                if (Area.Id != 1637)
                {
                    log("Нахожусь не в оргримаре " + Area.Id + " " + Area.ZoneName);
                    Thread.Sleep(5000);
                    return null;
                }
            }

            if (Me.Team == ETeam.Alliance)
            {
                if (Area.Id != 1519)
                {
                    log("Нахожусь не в штормвинде " + Area.Id + " " + Area.ZoneName);
                    Thread.Sleep(5000);
                    return null;
                }
            }

            Unit npc = null;
            if (ClientType == EWoWClient.Retail)
            {
                var path = CommonModule.GpsBase.GetPath(new Vector3F(1635, -4445, 17), Me.Location);
                if (Me.Team == ETeam.Horde)
                {
                    if (Me.Distance(1654.84, -4350.49, 26.35) < 50 || Me.Distance(1573.36, -4437.08, 16.05) < 50)
                    {
                        if (CharacterSettings.AlternateAuk)
                        {
                            path = CommonModule.GpsBase.GetPath(new Vector3F(2065.83, -4668.45, 32.52), Me.Location);
                        }
                        foreach (var vector3F in path)
                        {
                            log(path.Count + "  Путь " + Me.Distance(vector3F));
                            CommonModule.ForceMoveTo2(vector3F, 1, false);
                        }
                    }
                }

                if (Me.Team == ETeam.Alliance)
                {
                    path = CommonModule.GpsBase.GetPath(new Vector3F(-8816.10, 660.36, 98.01), Me.Location);
                    foreach (var vector3F in path)
                    {
                        log(path.Count + "  Путь " + Me.Distance(vector3F));
                        CommonModule.ForceMoveTo2(vector3F, 1, false);
                    }
                }

                //Проверка НПС

                foreach (var entity in GetEntities<Unit>())
                {
                    if (!entity.IsAuctioner)
                    {
                        continue;
                    }

                    if (entity.Id == 44868)
                    {
                        continue;
                    }

                    if (entity.Id == 44865)
                    {
                        continue;
                    }

                    if (entity.Id == 44866)
                    {
                        npc = entity;
                    }

                    if (entity.Id == 8719)
                    {
                        npc = entity;
                    }

                    if (entity.Id == 46640)
                    {
                        npc = entity;
                    }
                }
            }

            if (ClientType == EWoWClient.Classic)
            {
                if (Me.Team == ETeam.Horde)
                {
                    if (!CommonModule.MoveTo(1668.77, -4459.48, 18.84))
                    {
                        return null;
                    }

                    foreach (var entity in GetEntities<Unit>())
                    {
                        if (!entity.IsAuctioner)
                        {
                            continue;
                        }

                        if (entity.Id == 8724)
                        {
                            npc = entity;
                        }
                    }
                }

                if (Me.Team == ETeam.Alliance)
                {
                    if (!CommonModule.MoveTo(-8813.21, 662.48, 95.42))
                    {
                        return null;
                    }

                    foreach (var entity in GetEntities<Unit>())
                    {
                        if (!entity.IsAuctioner)
                        {
                            continue;
                        }

                        if (entity.Id == 15659)
                        {
                            npc = entity;
                        }
                    }
                }
            }

            if (npc != null)
            {
                log("Выбран " + npc.Name + " " + npc.Id);
                CommonModule.MoveTo(npc, ClientType == EWoWClient.Classic ? 4 : 3);
                return npc;
            }

            log("Нет НПС для аука", LogLvl.Error);
            Thread.Sleep(5000);
            return null;

        }

        public void Auk()
        {
            if (!MainForm.On)
            {
                return;
            }

            var npc = MyMoveToAuction();
            if (npc == null)
            {
                log("Нет НПС для аука", LogLvl.Error);
                Thread.Sleep(5000);
                return;
            }

            log("Выбран " + npc.Name + " " + npc.Id);

            MyCheckIsMovingIsCasting();
            if (!OpenAuction(npc))
            {
                log("Не смог открыть диалог для аука " + GetLastError(), LogLvl.Error);
            }
            else
            {
                log("Открыл диалог для аука", LogLvl.Ok);
            }
            Thread.Sleep(3000);
            //Продажа
            AutoQuests.WaitTeleport = true;
            SellAll();
            AutoQuests.WaitTeleport = false;
            Thread.Sleep(2000);
        }

        public bool NeedAuk;

        public bool MyUseStone(bool auk = false)
        {
            if (Me.Distance(BindPoint.Location) < 300 && BindPoint.MapID == MapID)
            {
                return true;
            }
            Thread.Sleep(2000);
            while (GetAgroCreatures().Count > 0)
            {
                if (!MainForm.On)
                {
                    return false;
                }

                Thread.Sleep(1000);
            }

            if (auk)
            {
                if (Area.Id == 1637 || Area.Id == 1519)
                {
                    return true;
                }
            }

            foreach (var item in ItemManager.GetItems())
            {
                if (item.Id == 6948 || item.Id == 8690)
                {
                    if (SpellManager.GetItemCooldown(item) != 0)
                    {

                        log("Камень в КД " + SpellManager.GetItemCooldown(item));
                        break;
                    }

                    if (CharacterSettings.AukRun && auk)
                    {
                        if (MapID == CharacterSettings.AukMapId)
                        {
                            NeedAuk = true;
                            try
                            {
                                if (!CommonModule.MoveTo(CharacterSettings.AukLocX, CharacterSettings.AukLocY, CharacterSettings.AukLocZ))
                                {
                                    return false;
                                }
                            }
                            finally
                            {
                                NeedAuk = false;
                            }
                        }
                    }
                    FarmModule.FarmState = FarmState.AttackOnlyAgro;
                    if (GetAgroCreatures().Count != 0)
                    {
                        return false;
                    }

                    CommonModule.MyUnmount();
                    CanselForm();
                    MyCheckIsMovingIsCasting();

                    var result = SpellManager.UseItem(item);
                    if (result != EInventoryResult.OK)
                    {
                        log("Не удалось использовать камень " + item.Name + " " + result + " " + GetLastError(), LogLvl.Error);
                        return false;
                    }
                    else
                    {
                        log("Использовал камень ", LogLvl.Ok);
                    }
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsCasting)
                    {
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(5000);
                    while (GameState != EGameState.Ingame)
                    {
                        Thread.Sleep(200);
                    }
                    Thread.Sleep(1000);
                    return true;
                }
            }

            return false;
        }

        public bool MyUseStone2()
        {
            if (Me.Distance(BindPoint.Location) < 100 && BindPoint.MapID == MapID)
            {
                return true;
            }
            Thread.Sleep(2000);
            while (GetAgroCreatures().Count > 0)
            {
                if (!MainForm.On)
                {
                    return false;
                }

                Thread.Sleep(1000);
            }

            foreach (var item in ItemManager.GetItems())
            {
                if (item.Id == 141605)
                {
                    FarmModule.FarmState = FarmState.AttackOnlyAgro;
                    while (SpellManager.GetItemCooldown(item) != 0)
                    {
                        if (!MainForm.On)
                        {
                            return false;
                        }

                        Thread.Sleep(5000);
                        log("Свисток в КД " + SpellManager.GetItemCooldown(item));
                        //  break;
                    }


                    FarmModule.FarmState = FarmState.AttackOnlyAgro;
                    if (GetAgroCreatures().Count != 0)
                    {
                        return false;
                    }

                    CommonModule.MyUnmount();
                    CanselForm();
                    MyCheckIsMovingIsCasting();

                    var result = SpellManager.UseItem(item);
                    if (result != EInventoryResult.OK)
                    {
                        log("Не удалось использовать свисток " + item.Name + " " + result + " " + GetLastError(), LogLvl.Error);
                        return false;
                    }
                    else
                    {
                        log("Использовал свисток ", LogLvl.Ok);
                    }
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsChanneling)
                    {
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(5000);
                    while (GameState != EGameState.Ingame)
                    {
                        Thread.Sleep(200);
                    }
                    Thread.Sleep(1000);
                    return true;
                }
            }

            return false;
        }

        public void CanselForm()
        {
            foreach (var i in Me.GetAuras())
            {
                if (i.SpellId == 5487)//Облик медведя
                {
                    i.Cancel();
                }

                if (i.SpellId == 768)//Облик кошки
                {
                    i.Cancel();
                }

                if (i.SpellId == 24858)//Облик лунного совуха    
                {
                    i.Cancel();
                }

                if (i.SpellId == 783)//Походный облик
                {
                    i.Cancel();
                }
            }
        }

        public long GetUnixTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public void FarmSpellClick(List<uint> farmMobIds)
        {
            Thread.Sleep(100);
            if (!MainForm.On)
            {
                return;
            }

            if (GetAgroCreatures().Count > 0)
            {
                return;
            }

            var entitylist = GetEntities();
            Entity needEntity = null;
            foreach (var entity in entitylist.OrderBy(i => Me.Distance(i)))
            {
                if (!farmMobIds.Contains(entity.Id))
                {
                    continue;
                }

                if (Me.Distance(entity) < 10)
                {
                    continue;
                }

                if (FarmModule.IsBadTarget(entity, FarmModule.TickTime))
                {
                    continue;
                }

                needEntity = entity;
                break;
            }

            if (needEntity != null)
            {
                MyUseSpellClick(needEntity);
            }
        }

        internal Unit GetUnitByIdInZone(uint id, bool checkBad, bool needsAlive, Zone zone)
        {
            try
            {
                var listEntity = GetEntities<Unit>();
                foreach (var npc in listEntity.OrderBy((i => Me.Distance(i))))
                {
                    if (!zone.ObjInZone(npc))
                    {
                        continue;
                    }

                    if (!checkBad)
                    {
                        if (FarmModule.IsBadTarget(npc, FarmModule.TickTime))
                        {
                            continue;
                        }
                    }

                    if (needsAlive)
                    {
                        if (!npc.IsAlive)
                        {
                            continue;
                        }
                    }
                    if (npc.Id == id)
                    {
                        return npc;
                    }

                    if (npc.Guid.GetEntry() == id)
                    {
                        return npc;
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return null;
        }


        public Unit GetUnitById(uint id, bool checkAlive = false)
        {
            foreach (var entity in GetEntities<Unit>().OrderBy(i => Me.Distance(i)))
            {
                if (checkAlive)
                {
                    if (!entity.IsAlive)
                    {
                        continue;
                    }
                }

                if (entity.Id == id)
                {
                    return entity;
                }
            }

            return null;
        }

        internal Entity GetNpcById(uint id)
        {
            try
            {
                var listEntity = GetEntities();
                foreach (var npc in listEntity.OrderBy((i => Me.Distance(i))))
                {
                    if (npc.Distance2D(new Vector3F(-2054.99, 753.27, 7.13)) < 10 && id == 126034)
                    {
                        continue;
                    }

                    if (npc.Distance(2475.74, 1130.51, 5.97) < 10)
                    {
                        continue;
                    }

                    if (npc.Id == 138449 && npc.Distance(3899.11, 405.34, 148.84) < 5)
                    {
                        continue;
                    }

                    if (npc.Id == id)
                    {
                        return npc;
                    }

                    if (npc.Guid.GetEntry() == id)
                    {
                        return npc;
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return null;
        }

        public void MyLearnPetSpell()
        {
            if (Me.Class != EClass.Hunter)
            {
                return;
            }

            if (!CharacterSettings.LearnPetSpell)
            {
                return;
            }

            var pet = Me.GetPet();
            if (pet == null)
                return;
            foreach (var spell in SpellManager.GetSpells())
            {

                if (spell.IsPetSpell)
                {
                    continue;
                }

                if (spell.Id == 7370 && pet.Family != ECreatureFamily.Boar)
                {
                    continue;
                }

                if (spell.Id == 24604 && pet.Family != ECreatureFamily.Wolf)
                {
                    continue;
                }

                if (spell.Id == 24450 && pet.Family != ECreatureFamily.Cat)
                {
                    continue;
                }

                if (!spell.GetEffects().Exists(e => e.Effect == ESpellEffectName.LEARN_SPELL))
                {
                    continue;
                }

                if (!spell.CanLearnPetSpell() || spell.GetTPCost() > pet.AvailableTrainingPoints)
                {
                    continue;
                }

                var res = SpellManager.CastSpell(spell.Id);
                if (res != ESpellCastError.SUCCESS)
                {
                    log("Не удалось изучить  " + +spell.Id + " " + spell.Name + "[" + spell.SpellSubName + "] " + spell.GetTPCost() + "\\" + pet.AvailableTrainingPoints + " " + res + " " + GetLastError(), LogLvl.Error);
                }
                else
                {
                    log("Изучил  " + " " + spell.Name + "[" + spell.SpellSubName + "] ", LogLvl.Ok);
                }
            }
        }

        public bool IsBossAlive(uint id)
        {
            foreach (var entity in GetEntities<Unit>())
            {
                if (entity.Id == id && entity.IsAlive)
                    return true;
            }

            return false;
        }

        public Transport MyGetTransportById(int id)
        {
            foreach (var transport in GetEntities<Transport>())
            {
                if (transport.Id == id)
                {
                    return transport;
                }
            }
            return null;
        }

        internal Item MyGetItem(uint id)
        {
            foreach (var item in ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 || item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 || item.Place == EItemPlace.InventoryItem)
                {
                    if (item.Id == id)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private uint _badTaxyLoc;
        private string _badTaxyError = "";
        DateTime _nextUseTaxy = DateTime.MinValue;

        public bool MyOpenNodes()
        {

            foreach (var characterSettingsTaxyNodeSettingse in CharacterSettings.TaxyNodeSettingses)
            {

                if (MyTaxyNode.MyTaxyNodes.Any(i => i.Id == characterSettingsTaxyNodeSettingse.Id))
                    continue;
                MyUseStone();
                if (Area.Id == 1519)
                {
                    if (MyAllItemsRepair())
                    {
                        MyMoveToSell();
                        MyMoveToRepair();
                        return false;
                    }
                }
                log("Необходимо открыть " + characterSettingsTaxyNodeSettingse.Name + " [" + characterSettingsTaxyNodeSettingse.Id + "] дист: " + Me.Distance(characterSettingsTaxyNodeSettingse.Loc) + "  " + Area.Id);
                if (characterSettingsTaxyNodeSettingse.Id == 6)//Ironforge
                {
                    if (Area.Id != 1537)//Ironforge
                    {
                        AutoQuests.CheckPath();
                        return false;
                    }
                }

                if (characterSettingsTaxyNodeSettingse.Id == 8)//Thelsamar, Loch Modan
                {
                    if (Area.Id == 1519 || Area.Id == 12)//StormwindCity
                    {
                        MyUseTaxi(1537, new Vector3F(-4821.78, -1155.44, 502.21), false);//Ironforge
                        return false;
                    }
                }

                if (characterSettingsTaxyNodeSettingse.Id == 16)//Arathi
                {
                    if (Area.Id == 1519 || Area.Id == 12 || Area.Id == 1537)//StormwindCity
                    {
                        MyUseTaxi(320, new Vector3F(-1240.53, -2515.11, 22.16), false);//Ironforge
                        return false;
                    }
                }

                if (characterSettingsTaxyNodeSettingse.Id == 14 || characterSettingsTaxyNodeSettingse.Id == 43)//Southshore, Hillsbrad
                {
                    if (Area.Id == 1519)//StormwindCity
                    {
                        MyUseTaxi(45, new Vector3F(-1240, -2515, 22), false);//ArathiHighlands
                        return false;
                    }
                }

                if (characterSettingsTaxyNodeSettingse.Id == 7)//Southshore, Hillsbrad
                {
                    if (Area.Id == 1519)//StormwindCity
                    {
                        MyUseTaxi(287, new Vector3F(-711.48, -515.48, 26.11), false);//ArathiHighlands
                        return false;
                    }
                }


                if (!CommonModule.MoveToCheckMap(characterSettingsTaxyNodeSettingse.Loc, 5, true, characterSettingsTaxyNodeSettingse.MapId))
                    return false;
                if (!CommonModule.MoveTo(characterSettingsTaxyNodeSettingse.Loc, 5))
                    return false;

                foreach (var entity in GetEntities<Unit>())
                {
                    if (!entity.IsTaxi)
                        continue;
                    if (!CommonModule.MoveTo(entity, 2))
                    {
                        return false;
                    }

                    Thread.Sleep(500);
                    if (!OpenTaxi(entity))
                    {
                        log("Не смог открыть диалог с " + entity.Name + " TaxiStatus:" + entity.TaxiStatus + " IsTaxi:" + entity.IsTaxi + "  " + GetLastError(), LogLvl.Error);
                    }

                    if (MyTaxyNode.MyTaxyNodes.Any(i => i.Id == characterSettingsTaxyNodeSettingse.Id))
                    {

                    }
                    else
                    {
                        MyTaxyNode.MyTaxyNodes.Add(new TaxyNodeSettings
                        {
                            Id = characterSettingsTaxyNodeSettingse.Id,
                            MapId = characterSettingsTaxyNodeSettingse.MapId,
                            Loc = characterSettingsTaxyNodeSettingse.Loc,
                            Name = characterSettingsTaxyNodeSettingse.Name
                        });
                    }


                    MyNodeListFill();
                }

                return false;
            }



            return true;
        }




        private TaxiNode MyGetBestNode(Vector3F loc)
        {

            double bestDist = 9999999;
            TaxiNode bestNode = null;
            foreach (var node in GetallNodesOnMyMap())
            {
                if (node.Id == 1839)//Настрондир  1220  0  81.2824630737305  1839
                {
                    continue;
                }

                if (node.Id == 2161)//Настрондир  1220  0  81.2824630737305  1839
                {
                    continue;
                }

                if (node.Id == 2078)//Настрондир  1220  0  81.2824630737305  1839
                {
                    continue;
                }

                if (node.Id == 2073 && CharacterSettings.Mode != Mode.Questing)//Throne Room, Zuldazar  1642  0  37.4401321411133  2073
                {
                    continue;
                }

                if (node.Id == 2116 && CharacterSettings.Mode != Mode.Questing)//Disabled   Quest Path 6698: Horde Embassy, Zuldazar -> Throne Room, Zuldazar  1642  0  270.530822753906  2116
                {
                    continue;
                }

                if (node.Id == 2015 && CharacterSettings.Mode != Mode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
                {
                    continue;
                }

                if (node.Id == 1961 && CharacterSettings.Mode != Mode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
                {
                    continue;
                }

                if (node.Id == 2080 && CharacterSettings.Mode != Mode.Questing && Me.Team == ETeam.Horde)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2062 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2273 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2114)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2144 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2112 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 1642 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2153 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2147 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2145 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2157 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2148 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2129 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2012 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 1962 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2110 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2112 && CharacterSettings.Mode != Mode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                {
                    continue;
                }

                if (node.Id == 2274 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2275 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2091 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2107 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2135 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2127 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2108 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2057 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2056 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2033 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2093 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2090 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2156 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2105 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2054 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2282 && CharacterSettings.Mode != Mode.Questing)
                {
                    continue;
                }

                if (node.Id == 2144)
                {
                    continue;
                }

                if (node.Id == 2110)
                {
                    continue;
                }

                if (node.Id == 36)
                {
                    continue;
                }

                if (node.Name.Contains("Transport"))
                {
                    continue;
                }

                if (node.Id == 22)//Убрать
                {
                    continue;
                }

                if (node.Id == 1)//Northshire Abbey
                {
                    continue;
                }

                if (Area.Id == 1537 && node.Id == 74)
                {
                    continue;
                }

                if (Area.Id == 1537 && node.Id == 75)
                {
                    continue;
                }

                var next = false;

                if (node.Id == 76)
                {
                    if (Me.Team == ETeam.Alliance)
                        continue;
                }

                foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                {
                    if (!myNpcLoc.IsTaxy)
                    {
                        continue;
                    }

                    if (myNpcLoc.Team == ETeam.Other)
                    {
                        continue;
                    }

                    if (myNpcLoc.ListLoc == null || myNpcLoc.ListLoc.Count == 0)
                    {
                        continue;
                    }

                    if (node.Location.Distance(myNpcLoc.ListLoc[0]) > 20)
                    {
                        continue;
                    }

                    if (Me.Team == ETeam.Horde && myNpcLoc.Team == ETeam.Alliance)
                    {
                        log("Вражеское такси " + myNpcLoc.Id + " " + node.Name);
                        next = true;
                    }

                    if (Me.Team == ETeam.Alliance && myNpcLoc.Team == ETeam.Horde)
                    {
                        log("Вражеское такси " + myNpcLoc.Id + " " + node.Name);
                        next = true;
                    }
                }
                if (next)
                {
                    continue;
                }

                if (node.Location.Distance(loc) > 4000)
                {
                    // log("Слишком далеко " + node.Location.Distance(loc) + " " + node.Name);
                    continue;
                }


                if (Me.Distance(loc) > 10)
                {
                    log("Открытых такси " + MyTaxyNode.MyTaxyNodes.Count + "  " + node.Name + "[" + node.Id + "] MapId:" + node.MapId);
                    if (MyTaxyNode.MyTaxyNodes.Count > 0)
                    {
                        next = true;
                        foreach (var listOpenNode in MyTaxyNode.MyTaxyNodes)
                        {
                            if (listOpenNode.Id == node.Id)
                            {
                                next = false;
                            }
                        }

                        if (next)
                        {
                            log("Не нашел среди открытых " + node.Name + "[" + node.Id + "] MapId:" + node.MapId, LogLvl.Error);
                            continue;
                        }
                    }
                }

                var dist = CalcPathDistanceServer(node.Location, loc);
                log(node.Name + "[" + node.Id + "] MapId:" + node.MapId + " Дист: " + dist);
                /* if (dist == 0)
                     continue;*/
                if (dist < bestDist)
                {
                    bestNode = node;
                    bestDist = dist;
                }
            }

            return bestNode;
        }

        // public List<TaxiNode> ListOpenNodes = new List<TaxiNode>();
        public uint MapIdOpenNodes = 9999999;

        public void MyNodeListFill()
        {
            MapIdOpenNodes = MapID;
            foreach (var canLandNode in TaxiNodesData.CanLandNodes)
            {
                if (MyTaxyNode.MyTaxyNodes.Any(i => i.Id == canLandNode.Id))
                    continue;

                // ListOpenNodes.Add(canLandNode);
                MyTaxyNode.MyTaxyNodes.Add(new TaxyNodeSettings
                {
                    Id = canLandNode.Id,
                    MapId = canLandNode.MapId,
                    Loc = canLandNode.Location,
                    Name = canLandNode.Name
                });

            }
            ConfigLoader.SaveConfig(PathTaxyNode, MyTaxyNode);
            log("Всего открытых таксинод " + MyTaxyNode.MyTaxyNodes.Count);
        }

        internal bool MyUseTaxi(uint areaId, Vector3F loc, bool fail = true)
        {
            try
            {
                if (!CharacterSettings.UseFly)
                {
                    return true;
                }

                while (Me.IsInCombat)
                {
                    if (!MainForm.On)
                    {
                        return false;
                    }

                    Thread.Sleep(1000);
                    log("Ожидаю конца боя " + Me.IsInCombat);
                }

                if (fail)
                    if (_badTaxyLoc == areaId)
                    {
                        if (_nextUseTaxy > DateTime.UtcNow)
                        {
                            log("Не лечу потому что в прошлый раз " + _badTaxyError);
                            return true;
                        }
                    }

                _badTaxyLoc = 0;
                _badTaxyError = "";
                _nextUseTaxy = DateTime.UtcNow.AddMinutes(RandGenerator.Next(2, 3));
                if (Me.GetThreats().Count > 0)
                {
                    FarmModule.FarmState = FarmState.AttackOnlyAgro;
                    return false;
                }

                var needArea = GetAreaById(areaId);

                if (needArea == null)
                {
                    log("Не нашел зону с айди " + areaId, LogLvl.Error);
                    Thread.Sleep(10000);
                    _badTaxyLoc = areaId;
                    _badTaxyError = "Не нашел зону с айди " + areaId;
                    return true;
                }
                log("Нужно в зону " + areaId + "    " + needArea.AreaName + "  " + Me.Distance(loc), LogLvl.Important);


                TaxiNode bestNode = MyGetBestNode(Me.Location);
                if (bestNode == null)
                {
                    log("Не нашел ближайшее такси к себе ");
                    Thread.Sleep(1000);
                    _badTaxyError = "Не нашел ближайшее такси к себе " + areaId;
                    _badTaxyLoc = areaId;
                    return true;
                }
                var distMeToNode = CalcPathDistanceServer(Me.Location, bestNode.Location);
                log("Ближайшее такси ко мне  " + distMeToNode + "  " + bestNode.Id, LogLvl.Important);


                TaxiNode bestNodeLoc = MyGetBestNode(loc);
                if (bestNodeLoc == null)
                {
                    log("Не нашел ближайшее такси к локе " + areaId);
                    Thread.Sleep(1000);
                    _badTaxyError = "Не нашел ближайшее такси к локе " + areaId;
                    _badTaxyLoc = areaId;
                    return true;
                }

                if (bestNode.Id == bestNodeLoc.Id)
                {
                    log("Ближайшее такси к точке назначения таже самая что и ко мне ");
                    Thread.Sleep(1000);
                    _badTaxyError = "Ближайшее такси к точке назначения таже самая что и ко мне " + areaId;
                    _badTaxyLoc = areaId;
                    return true;
                }

                var distMeToLoc = CalcPathDistanceServer(Me.Location, loc);
                var distNodeToLoc = CalcPathDistanceServer(bestNodeLoc.Location, loc);

                log("Ближайшее такси к точке назначения  " + distNodeToLoc + "  " + bestNodeLoc.Id, LogLvl.Important);

                log("Пешком " + distMeToLoc, LogLvl.Important);
                if (distMeToLoc < distMeToNode + distNodeToLoc)
                {
                    log("Пешком ближе");
                    Thread.Sleep(1000);
                    _badTaxyError = "Пешком ближе " + areaId;
                    _badTaxyLoc = areaId;
                    return true;
                }


                if (Me.Distance(bestNode.Location) > 10)
                {
                    FarmModule.FarmState = FarmState.Disabled;
                    if (!CommonModule.MoveTo(bestNode.Location, 1))
                    {
                        FarmModule.FarmState = FarmState.AttackOnlyAgro;
                        return false;
                    }
                    FarmModule.FarmState = FarmState.AttackOnlyAgro;
                }

                Unit taxinpc = null;
                foreach (var npc in GetEntities<Unit>().OrderBy(i => Me.Distance(i)))
                {
                    if (!npc.IsTaxi)
                    {
                        continue;
                    }

                    taxinpc = npc;
                    break;
                }
                if (taxinpc == null)
                {
                    log("Не нашел такси");
                    Thread.Sleep(10000);
                    _badTaxyError = "Не нашел такси";
                    _badTaxyLoc = areaId;
                    return true;
                }
                if (!CommonModule.MoveTo(taxinpc, 1))
                {
                    return false;
                }

                CommonModule.MyUnmount();
                CanselForm();
                MyCheckIsMovingIsCasting();
                Thread.Sleep(1000);
                if (!OpenTaxi(taxinpc))
                {
                    log("Не смог использовать такси " + taxinpc.Name + "  " + GetLastError(), LogLvl.Error);
                    Thread.Sleep(2000);
                    log("Диалогов " + GetNpcDialogs().Count);
                    if (GetLastError() != ELastError.ActionNotAllowed)
                    {
                        Thread.Sleep(10000);
                        //  return false;
                    }

                }
                Thread.Sleep(2000);
                log("Диалогов " + GetNpcDialogs().Count);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.OptionNPC == EGossipOptionIcon.Taxi)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не удалось открыть список такси " + GetLastError(), LogLvl.Error);
                    }

                    log(gossipOptionsData.Text + " " + gossipOptionsData.OptionNPC + " " + gossipOptionsData.ClientOption);

                }
                Thread.Sleep(2000);
                MyNodeListFill();
                TaxiNode node = null;
                double bestDistnode = 99999999;
                foreach (var canLandNode in TaxiNodesData.CanLandNodes)
                {
                    log("Доступные ноды: " + canLandNode.Name + "[" + canLandNode.Id + "] MapId: " + canLandNode.MapId + "  " + "   " + canLandNode.Location + "  " + loc.Distance(canLandNode.Location), LogLvl.Important);
                    if (canLandNode.Id == 1965 && CharacterSettings.Mode != Mode.Questing)
                    {
                        continue;
                    }

                    var distNode = CalcPathDistanceServer(loc, canLandNode.Location);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    /* if (distNode == 0)
                     {
                         continue;
                     }*/

                    if (distNode < bestDistnode)
                    {
                        bestDistnode = distNode;
                        node = canLandNode;
                    }
                }



                if (node?.Id == bestNode.Id)
                {
                    node = null;
                }

                if (node == null && bestNodeLoc.Id == 7)
                {
                    foreach (var canLandNode in TaxiNodesData.CanLandNodes)
                    {
                        if (canLandNode.Id == 8)
                        {
                            node = canLandNode;
                        }
                    }
                }

                if (node != null && node.Id != 7 && bestNodeLoc.Id == 7)
                {
                    foreach (var canLandNode in TaxiNodesData.CanLandNodes)
                    {
                        if (canLandNode.Id == 8)
                        {
                            node = canLandNode;
                        }
                    }
                }



                if (node != null)
                {
                    log("Выбрал точку " + node.Name + " " + node.Id + "  " + node.MapId + "  " + node.Cost + "   " + node.Location, LogLvl.Ok);
                    Thread.Sleep(2000);

                    var result = UseTaxi(node.Id);
                    Thread.Sleep(1000);
                    if (result != ETaxiError.Ok)
                    {
                        log("Ошибка перелета " + result, LogLvl.Error);
                        if (result == ETaxiError.SameNode)
                        {
                            var path = GetServerPath(Me.Location, loc);
                            log("Маршрут " + path.Path.Count);
                            AutoQuests.EnableFarmProp = false;
                            foreach (var vector3F in path.Path)
                            {
                                if (!MainForm.On)
                                {
                                    AutoQuests.EnableFarmProp = true;
                                    return false;
                                }

                                if (!Me.IsAlive)
                                {
                                    AutoQuests.EnableFarmProp = true;
                                    return false;
                                }

                                CommonModule.ForceMoveTo2(vector3F);
                            }
                            AutoQuests.EnableFarmProp = true;
                        }
                    }
                    else
                    {
                        log("Улетел ", LogLvl.Ok);
                    }

                    Thread.Sleep(2000);
                }
                else
                {
                    log("Не найдено место назначения ", LogLvl.Error);
                    _badTaxyLoc = areaId;
                    _badTaxyError = "Не найдено место назначения";
                    _nextUseTaxy = DateTime.UtcNow.AddMinutes(RandGenerator.Next(20, 30));
                }


                var timer = 0;
                while (Me.IsInFlight)
                {
                    Thread.Sleep(1000);
                    timer++;
                    if (timer > 60)
                    {
                        SendKeyPress(0x20);//пробел
                        timer = 0;
                    }
                }

                Thread.Sleep(5000);
                //var safePoint = new List<Vector3F>();
                //var xc = Me.Location.X;
                //var yc = Me.Location.Y;

                //var radius = 10;
                //const double a = Math.PI / 16;
                //double u = 0;
                //for (var i = 0; i < 32; i++)
                //{
                //    var x1 = xc + radius * Math.Cos(u);
                //    var y1 = yc + radius * Math.Sin(u);
                //    u += a;
                //    var z1 = GetNavMeshHeight(new Vector3F(x1, y1, 0));
                //    if (IsInsideNavMesh(new Vector3F((float)x1, (float)y1, (float)z1)))
                //        safePoint.Add(new Vector3F((float)x1, (float)y1, (float)z1));
                //}

                //if (safePoint.Count > 0)
                //{
                //    var bestPoint = safePoint[RandGenerator.Next(safePoint.Count)];
                //    ComeTo(bestPoint);
                //}

                return true;
            }
            catch (Exception e)
            {
                log(e + "");
                return false;
            }
        }

        public bool RunRun;
        public bool Evade = false;

        public int GetChanceDeath()
        {
            if (Me.IsDeadGhost || !Me.IsAlive)
            {
                return 0;
            }

            if (GetAgroCreatures().Count == 0)
            {
                return 0;
            }

            var result = 100 - Me.HpPercents;
            if (GetAgroCreatures().Count == 1)
            {
                result -= 10;
            }

            foreach (var agroCreature in GetAgroCreatures())
            {
                if (agroCreature.CreatureTemplate.Rank != ECreatureRank.Normal)
                {
                    result += 10;
                }

                result += agroCreature.HpPercents / 5;
            }

            if (Evade)
                return 100;

            if (result < 0)
            {
                return 0;
            }

            return result;
        }

        public void MyMoveForvard(int time)
        {
            Thread.Sleep(2000);
            SetMoveStateForClient(true);
            MoveForward(true);
            Thread.Sleep(time);

            while (GameState != EGameState.Ingame)
            {
                if (!MainForm.On)
                {
                    return;
                }
                Thread.Sleep(100);
            }
            MoveForward(false);
            SetMoveStateForClient(false);
        }

        public void MyMoveBackward(int time)
        {
            Thread.Sleep(2000);
            SetMoveStateForClient(true);
            MoveBackward(true);
            Thread.Sleep(time);

            while (GameState != EGameState.Ingame)
            {
                if (!MainForm.On)
                {
                    return;
                }

                Thread.Sleep(100);
            }
            MoveBackward(false);
            SetMoveStateForClient(false);
        }

        public bool MyIsNeedLearnSpell()
        {
            if (!CharacterSettings.LearnSpell)
                return false;
            foreach (var learnSkill in CharacterSettings.LearnSkill)
            {
                if (Me.Level < learnSkill.Level)
                {
                    continue;
                }

                if (Me.Level > learnSkill.Level + 3)
                {
                    continue;
                }

                if (Me.Money < learnSkill.Price)
                {
                    continue;
                }

                if (SpellManager.GetSpell(learnSkill.Id) == null)
                {
                    log("Необходимо выучить скилл " + learnSkill.Id, LogLvl.Important);
                    return true;
                }
            }
            return false;
        }

        public void MyLearnSpell()
        {


            if (CharacterSettings.UseStoneForLearnSpell)
            {
                MyUseStone();
            }

            foreach (var learnSkill in CharacterSettings.LearnSkill)
            {
                if (Me.Level < learnSkill.Level)
                {
                    continue;
                }

                if (Me.Money < learnSkill.Price)
                {
                    continue;
                }

                if (Me.Level > learnSkill.Level + 3)
                {
                    continue;
                }

                if (SpellManager.GetSpell(learnSkill.Id) != null)
                {
                    continue;
                }

                log("Бегу учить " + learnSkill.Id);
                if (Me.Distance(learnSkill.Loc) > 1400)
                {
                    if (!MyUseTaxi(learnSkill.AreaId, learnSkill.Loc))
                    {
                        return;
                    }
                }


                if (Me.Distance(learnSkill.Loc) > 20)
                {
                    if (!CommonModule.MoveTo(learnSkill.Loc, 15))
                    {
                        return;
                    }
                }

                var npc = GetNpcById(learnSkill.NpcId);
                if (npc != null)
                {
                    if (Me.Distance(npc) > 2)
                    {
                        if (!CommonModule.MoveTo(npc, 1))
                        {
                            return;
                        }
                    }
                    /* if (GetTrainerSpells().Count > 0)
{*/
                    if (!MyOpenDialog(npc))
                    {
                        return;
                    }

                    Thread.Sleep(1000);
                    foreach (var d in GetNpcDialogs())
                    {
                        if (d.OptionNPC != EGossipOptionIcon.Trainer)
                        {
                            continue;
                        }

                        SelectNpcDialog(d);
                        break;
                    }
                    Thread.Sleep(1000);
                    // }


                    foreach (var trainerSpell in GetTrainerSpells())
                    {
                        /* if (trainerSpell.Id != 5117)
                                 continue;*/
                        // log(trainerSpell.Id + " " + trainerSpell.Spell.Name + "  " + trainerSpell.CanLearn() + "  " + trainerSpell.ReqLevel + "  " + trainerSpell.Spell.Id);
                        if (!GameDB.SpellInfoEntries.ContainsKey(trainerSpell.Id))
                        {
                            log("Скила " + trainerSpell.Spell.Name + "  " + trainerSpell.Id + " нет в базе");
                            continue;
                        }

                        var dbSpell = GameDB.SpellInfoEntries[trainerSpell.Id];
                        foreach (var dbSpellEffect in dbSpell.Effects)
                        {
                            foreach (var spellEffectInfo in dbSpellEffect.Value)
                            {
                                if (spellEffectInfo.Effect == ESpellEffectName.LEARN_SPELL && spellEffectInfo.TriggerSpell == learnSkill.Id)
                                {
                                    //  log(spellEffectInfo.Effect + " " + spellEffectInfo.TriggerSpell);
                                    if (!trainerSpell.CanLearn())
                                    {
                                        log("Нельзя выучить этот скилл");
                                        return;
                                    }

                                    log("Учу " + trainerSpell.Spell.Name);
                                    if (!LearnTrainerSpell(trainerSpell))
                                    {
                                        log("Не смог выучить " + trainerSpell.Spell.Name + " " + GetLastError(), LogLvl.Error);
                                    }

                                    Thread.Sleep(500);
                                    return;
                                }
                                //  log("Ошибка? " + spellEffectInfo.Effect  + " " + spellEffectInfo.TriggerSpell );
                            }
                        }

                    }

                    if (GetTrainerSpells().Count == 0)
                    {
                        MySendKeyEsc();
                    }
                }
                else
                {
                    log("Не нашел НПС с Id " + learnSkill.Id);
                }
            }
        }

        public int MeGetItemsCount(uint id, bool checkEquip = false, bool checkBug = false)
        {
            var count = 0;
            foreach (var item in ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                    item.Place == EItemPlace.InventoryItem)
                {
                    if (item.Id == id)
                    {
                        count += item.Count;
                    }
                }
                if (checkEquip && item.Place == EItemPlace.Equipment)
                {
                    if (item.Id == id)
                    {
                        count += item.Count;
                    }
                }

                if (checkBug && item.Place == EItemPlace.InventoryBag)
                {
                    if (item.Id == id)
                    {
                        count += item.Count;
                    }
                }
            }


            return count;
        }

        public NpcForAction FindNpcForActionArmored()
        {
            double bestDist = 999999;
            NpcForAction bestNpc = null;
            try
            {
                foreach (var npc in CharacterSettings.NpcForActionSettings)
                {
                    if (npc.MapId != MapID)
                    {
                        continue;
                    }

                    if (npc.AreaId != Area.Id)
                    {
                        continue;
                    }

                    if (!npc.IsArmorer)
                    {
                        continue;
                    }

                    if (Me.Distance(npc.Loc) < bestDist)
                    {
                        bestNpc = npc;
                        bestDist = Me.Distance(npc.Loc);
                    }
                }

                if (bestNpc == null)
                {
                    foreach (var npc in CharacterSettings.NpcForActionSettings)
                    {
                        if (npc.MapId != MapID)
                        {
                            continue;
                        }

                        if (!npc.IsArmorer)
                        {
                            continue;
                        }

                        if (Me.Distance(npc.Loc) < bestDist)
                        {
                            bestNpc = npc;
                            bestDist = Me.Distance(npc.Loc);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return bestNpc;
        }


        bool _isRuLang;
        public void SellAll()
        {

            log(GetCurrentAccount().ServerName);
            if (GetCurrentAccount().ServerName.Contains("Russia"))
            {
                _isRuLang = true;
            }

            var minimumCountForProcess = new Dictionary<EItemQuality, uint>
            {
                [EItemQuality.Normal] = 200,
                [EItemQuality.Uncommon] = 200,
                [EItemQuality.Rare] = 5,
                [EItemQuality.Epic] = 5
            };

            var minimumCheckingCount = new Dictionary<EItemQuality, int>
            {
                [EItemQuality.Normal] = 2000,
                [EItemQuality.Uncommon] = 500,
                [EItemQuality.Rare] = 200,
                [EItemQuality.Epic] = 1
            };

            if (ClientType == EWoWClient.Classic)
            {
                minimumCountForProcess = new Dictionary<EItemQuality, uint>
                {
                    [EItemQuality.Normal] = 5,
                    [EItemQuality.Uncommon] = 5,
                    [EItemQuality.Rare] = 5,
                    [EItemQuality.Epic] = 5
                };

                minimumCheckingCount = new Dictionary<EItemQuality, int>
                {
                    [EItemQuality.Normal] = 5,
                    [EItemQuality.Uncommon] = 5,
                    [EItemQuality.Rare] = 5,
                    [EItemQuality.Epic] = 1
                };
            }

            var itemIDsForSell = new List<uint>();
            foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
            {
                if (itemIDsForSell.Contains(Convert.ToUInt32(characterSettingsAukSettingse.Id)))
                {
                    continue;
                }

                if (characterSettingsAukSettingse.MaxCount != 0)
                {
                    var i = 0;
                    foreach (var myAuctionItem in GetMyAuctionItems())
                    {
                        if (myAuctionItem.ItemId == characterSettingsAukSettingse.Id)
                        {
                            i += myAuctionItem.Count;
                        }
                    }

                    if (i > characterSettingsAukSettingse.MaxCount)
                    {
                        continue;
                    }
                }

                itemIDsForSell.Add(Convert.ToUInt32(characterSettingsAukSettingse.Id));
            }

            if (itemIDsForSell.Count == 0)
            {
                return;
            }

            var items = new Dictionary<uint, List<Item>>();
            var averages = new Dictionary<uint, ulong>();
            foreach (var item in ItemManager.GetItems())
            {
                if (item.ItemQuality == EItemQuality.Epic)
                {
                    var needItem = false;
                    foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
                    {
                        if (item.Id == characterSettingsAukSettingse.Id)
                        {
                            if (item.Level == characterSettingsAukSettingse.Level)
                            {
                                needItem = true;
                            }
                        }
                    }
                    if (!needItem)
                    {
                        continue;
                    }
                }
                if (item.Place >= EItemPlace.InventoryBag && item.Place <= EItemPlace.Bag4)
                {
                    if (itemIDsForSell.Contains(item.Id))
                    {
                        if (!items.ContainsKey(item.Id))
                        {
                            items[item.Id] = new List<Item>();
                        }

                        items[item.Id].Add(item);
                    }
                }


            }

            /*  Dictionary<uint, ulong> epicPrices = new Dictionary<uint, ulong>
              {
                  {350, 0 },
                  {355, 0 },
                  {360, 0 }
              };*/

            var epicPrices = new Dictionary<KeyValuePair<uint, uint>, ulong>();
            foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
            {
                if (characterSettingsAukSettingse.Level != 0)
                {
                    if (epicPrices.ContainsKey(new KeyValuePair<uint, uint>(Convert.ToUInt32(characterSettingsAukSettingse.Id), Convert.ToUInt32(characterSettingsAukSettingse.Level))))
                    {
                        // epicPrices[Convert.ToUInt32(characterSettingsAukSettingse.Id)].Add(Convert.ToUInt32(characterSettingsAukSettingse.Level), characterSettingsAukSettingse.MaxPrice);
                    }
                    else
                    {
                        epicPrices.Add(new KeyValuePair<uint, uint>(Convert.ToUInt32(characterSettingsAukSettingse.Id), Convert.ToUInt32(characterSettingsAukSettingse.Level)),
                            characterSettingsAukSettingse.MaxPrice * 10000);
                    }
                }

            }

            foreach (var id in itemIDsForSell)
            {
                if (!items.ContainsKey(id))
                {
                    continue;
                }

                var totalCount = 0;
                foreach (var item in items[id])
                {
                    totalCount += item.Count;
                }

                var firstItem = items[id][0];
                var quality = firstItem.ItemQuality;
                var name = firstItem.Name;
                if (_isRuLang)
                {
                    name = firstItem.NameRu;
                }

                log("Проверяем предмет: " + name + "[" + firstItem.Id + "]. Суммарное количество в инвентаре: " + totalCount + ", размер стака: " + firstItem.MaxStackCount);
                log("Минимально ищем: " + minimumCheckingCount[firstItem.ItemQuality]);
                var req = new AuctionSearchRequest
                {
                    MaxReturnItems = 50,
                    SearchText = name,
                    // ExactMatch = true
                };


                var itemsCount = 0;



                var normalPrices = new Dictionary<ulong, int>();
                ulong mylotPrice = 0;
                while (itemsCount < minimumCheckingCount[firstItem.ItemQuality])
                {
                    if (!MainForm.On)
                    {
                        return;
                    }

                    var aucItems = GetAuctionBuyList(req);
                    if (aucItems == null || aucItems.Count == 0)
                    {
                        break;
                    }

                    foreach (var aucItem in aucItems)
                    {
                        if (aucItem.BuyoutPrice == 0)
                        {
                            continue;
                        }

                        log(aucItem.Count + "  " + aucItem.BuyoutPrice + " " + aucItem.ItemLevel);
                        if (quality == EItemQuality.Epic && epicPrices.ContainsKey(new KeyValuePair<uint, uint>(aucItem.ItemId, aucItem.ItemLevel)))
                        {
                            if (epicPrices[new KeyValuePair<uint, uint>(aucItem.ItemId, aucItem.ItemLevel)] == 0 ||
                                aucItem.BuyoutPrice < epicPrices[new KeyValuePair<uint, uint>(aucItem.ItemId, aucItem.ItemLevel)])
                            {
                                foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
                                {
                                    if (characterSettingsAukSettingse.Id == aucItem.ItemId && characterSettingsAukSettingse.Level == aucItem.ItemLevel)
                                    {
                                        epicPrices[new KeyValuePair<uint, uint>(aucItem.ItemId, aucItem.ItemLevel)] = aucItem.BuyoutPrice - (characterSettingsAukSettingse.Disscount * 10000);
                                    }
                                }
                            }
                        }

                        if (quality < EItemQuality.Epic)
                        {
                            if (ClientType == EWoWClient.Retail)
                            {
                                if (aucItem.Count < 50)
                                {
                                    continue;
                                }
                            }

                            var priceforone = aucItem.BuyoutPrice / (uint)aucItem.Count;
                            if (aucItem.Owner == Me.Guid)
                            {
                                mylotPrice = priceforone;
                            }

                            if (normalPrices.ContainsKey(priceforone))
                            {
                                normalPrices[priceforone] = normalPrices[priceforone] + aucItem.Count;
                            }
                            else
                            {
                                normalPrices.Add(priceforone, aucItem.Count);
                            }
                        }
                        itemsCount += aucItem.Count;

                    }
                    req.Page++;
                }




                if (itemsCount >= minimumCheckingCount[quality])
                {
                    if (quality == EItemQuality.Epic)
                    {
                        if (epicPrices.ContainsKey(new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)))
                        {
                            foreach (var @ulong in epicPrices)
                            {
                                if (@ulong.Key.Key == firstItem.Id)
                                {
                                    log("Средняя цена для " + itemsCount + " " + name + "{" + @ulong.Key + "}[" + firstItem.Id + "] = " + (@ulong.Value / 10000f).ToString("F2"), LogLvl.Important);
                                }
                            }

                            /* foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
                             {
                                 if (characterSettingsAukSettingse.Id != firstItem.Id || characterSettingsAukSettingse.Level != firstItem.Level)
                                     continue;
                                 if (characterSettingsAukSettingse.MaxPrice * 10000 < epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)] || epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)] == 0)
                                 {
                                     Averages[firstItem.Id] = RoundDown(characterSettingsAukSettingse.MaxPrice * 10000);
                                     log(firstItem.Level + "  цена 1 : " + (Averages[firstItem.Id] / 10000f).ToString("F2") + " " + Averages[firstItem.Id]);
                                     epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)] = RoundDown(characterSettingsAukSettingse.MaxPrice * 10000);
                                     //break;
                                 }
                                 if (characterSettingsAukSettingse.MaxPrice * 10000 >= epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)])
                                 {
                                     Averages[firstItem.Id] = RoundDown(epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)] - (characterSettingsAukSettingse.Disscount * 10000));
                                     log(firstItem.Level + "  цена 2: " + (Averages[firstItem.Id] / 10000f).ToString("F2") + " " + Averages[firstItem.Id]);
                                     epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)] = RoundDown(epicPrices[new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)] - (characterSettingsAukSettingse.Disscount * 10000));
                                 }
                             }*/


                        }


                        /* foreach (var epicPrice in epicPrices)
                         {
                             //  log(epicPrice.Key + "  цена: " + (epicPrice.Value / 10000f).ToString("F2"));
                             if (firstItem.Level != epicPrice.Key)
                                 continue;
                             foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
                             {
                                 if (characterSettingsAukSettingse.Id != firstItem.Id || characterSettingsAukSettingse.Level != firstItem.Level)
                                     continue;
                                 if (characterSettingsAukSettingse.MaxPrice * 10000 < epicPrice.Value || epicPrice.Value == 0)
                                 {
                                     Averages[firstItem.Id] = RoundDown(characterSettingsAukSettingse.MaxPrice * 10000);
                                     log(epicPrice.Key + "  цена 1 : " + (Averages[firstItem.Id] / 10000f).ToString("F2") + " " + Averages[firstItem.Id]);
                                     epicPrice.Value = RoundDown(characterSettingsAukSettingse.MaxPrice * 10000);
                                     break;
                                 }
                                 if (characterSettingsAukSettingse.MaxPrice * 10000 >= epicPrice.Value)
                                 {
                                     Averages[firstItem.Id] = RoundDown(epicPrice.Value - (characterSettingsAukSettingse.Disscount * 10000));
                                     log(epicPrice.Key + "  цена 2: " + (Averages[firstItem.Id] / 10000f).ToString("F2") + " " + Averages[firstItem.Id]);
                                 }
                             }

                             log("Средняя цена для " + itemsCount + " " + name + "[" + firstItem.Id + "] = " + (Averages[firstItem.Id] / 10000f).ToString("F2"));
                             //  Averages[firstItem.Id] = epicPrice.Value;
                         }*/
                    }
                    else
                    {
                        foreach (var normalPrice in normalPrices.OrderBy(i => i.Key))
                        {
                            if (ClientType == EWoWClient.Retail)
                            {
                                if (normalPrice.Value < 399)
                                {
                                    continue;
                                }
                            }

                            averages[firstItem.Id] = normalPrice.Key - 1;
                            if (mylotPrice == normalPrice.Key)
                            {
                                averages[firstItem.Id] = normalPrice.Key;
                            }

                            log("Цена " + normalPrice.Key + "  Кол-во " + normalPrice.Value + "  " + averages[firstItem.Id]);
                            break;
                        }
                        log("Средняя цена для " + itemsCount + " " + name + "{" + firstItem.Level + "}[" + firstItem.Id + "] = " + (averages[firstItem.Id] / 10000f).ToString("F2"));

                    }
                }
            }

            log("Проверяем итемы " + items.Count);
            foreach (var k in items)
            {
                foreach (var item in k.Value)
                {
                    log("Проверяем итем [" + item.Id + "] в количестве " + item.Count);
                    if (item.ItemQuality < EItemQuality.Epic)
                    {
                        if (!averages.ContainsKey(item.Id))
                        {
                            log("Не нашел цену");
                            continue;
                        }
                    }

                    var count = (uint)item.Count;
                    log("можем продать " + count);
                    while (count > 0)
                    {
                        if (!MainForm.On)
                        {
                            return;
                        }

                        var countToSell = Math.Min(count, minimumCountForProcess[item.ItemQuality]);
                        /*  if(countToSell < MinimumCheckingCount[item.ItemQuality])
                              break;*/
                        count -= countToSell;
                        ulong sellPrice = 0;
                        if (item.ItemQuality < EItemQuality.Epic)
                        {
                            sellPrice = averages[item.Id] * countToSell;
                        }

                        var name = item.Name;
                        if (_isRuLang)
                        {
                            name = item.NameRu;
                        }

                        var minbid = sellPrice;
                        if (item.ItemQuality == EItemQuality.Epic)
                        {
                            sellPrice = epicPrices[new KeyValuePair<uint, uint>(item.Id, item.Level)];
                            minbid = sellPrice - (sellPrice / 100 * 20);
                        }

                        var time = EAuctionSellTime.TwelveHours;
                        if (ClientType == EWoWClient.Classic)
                        {
                            time = EAuctionSellTime.TwoHours;

                            if (CharacterSettings.AukTime == 1)
                            {
                                time = EAuctionSellTime.EightHours;
                            }

                            if (CharacterSettings.AukTime == 2)
                            {
                                time = EAuctionSellTime.TwentyFourHours;
                            }
                        }
                        else
                        {
                            if (CharacterSettings.AukTime == 1)
                            {
                                time = EAuctionSellTime.TwentyFourHours;
                            }

                            if (CharacterSettings.AukTime == 2)
                            {
                                time = EAuctionSellTime.FortyEightHours;
                            }
                        }


                        log("Выставляем на продажу " + name + "{" + item.Level + "}[" + item.Id + "] в количестве " + countToSell + " штук за " + (sellPrice / 10000f).ToString("F2") + "   " + (minbid / 10000f).ToString("F2"), LogLvl.Important);
                        var result = item.AuctionSell(minbid, sellPrice, time, countToSell);
                        if (result == EAuctionHouseError.Ok)
                        {
                            log("Успешно", LogLvl.Ok);
                        }
                        else
                        {
                            log("Ошибка выставления на аукцион " + result + " " + GetLastError(), LogLvl.Error);
                            Thread.Sleep(5000);
                        }
                    }
                }
            }
        }


        public void MyDelBigObstacle(bool removeAll = false)
        {
            if (removeAll)
            {
                foreach (var dateTime in MyBigObstacleList)
                {
                    foreach (var u in dateTime.Value.BigObstacleList)
                    {
                        RemoveObstacle(u);
                    }
                }
                MyBigObstacleList.Clear();
                //return;
            }

            /* var keysForRemove = new List<uint>();
             foreach (var dateTime in MyBigObstacleList)
             {
                 if (dateTime.Value > DateTime.UtcNow.AddSeconds(30))
                 {
                     RemoveObstacle(dateTime.Key);
                     keysForRemove.Add(dateTime.Key);
                 }
             }

             foreach (var key in keysForRemove)
                 MyBigObstacleList.Remove(key);*/
        }

        public void MyBigObstacleAdd(Vector3F loc, WowGuid guid)
        {
            if (MyBigObstacleList.ContainsKey(guid))
            {
                if (loc.Distance(MyBigObstacleList[guid].Loc) > 5)
                {
                    foreach (var u1 in MyBigObstacleList[guid].BigObstacleList)
                    {
                        RemoveObstacle(u1);
                    }

                    MyBigObstacleList.Remove(guid);

                }
                return;
            }

            log("Ставлю большой обстакл " + guid);
            var sw = new Stopwatch();
            sw.Start();
            var xc = loc.X;
            var yc = loc.Y;
            var radius = 30;
            const double a = Math.PI / 16;
            double u = 0;
            for (var i = 0; i < 32; i++)
            {
                var x1 = xc + radius * Math.Cos(u);
                var y1 = yc + radius * Math.Sin(u);
                u += a;
                var z1 = loc.Z - 10;
                var aaa = AddObstacle(new Vector3F(x1, y1, z1 - 10), 7, 30);
                if (!MyBigObstacleList.ContainsKey(guid))
                {
                    MyBigObstacleList.Add(guid, new MyBigObstacle
                    {
                        BigObstacleList = new List<uint> { aaa },
                        Loc = loc,
                        Time = DateTime.UtcNow
                    });
                }
                else
                {
                    MyBigObstacleList[guid].BigObstacleList.Add(aaa);
                }
            }
            radius = 7;

            u = 0;
            for (var i = 0; i < 32; i++)
            {
                var x1 = xc + radius * Math.Cos(u);
                var y1 = yc + radius * Math.Sin(u);
                u += a;
                var z1 = loc.Z - 10;
                var aaa = AddObstacle(new Vector3F(x1, y1, z1 - 10), 7, 30);
                if (!MyBigObstacleList.ContainsKey(guid))
                {
                    MyBigObstacleList.Add(guid, new MyBigObstacle
                    {
                        BigObstacleList = new List<uint> { aaa },
                        Loc = loc,
                        Time = DateTime.UtcNow
                    });
                }
                else
                {
                    MyBigObstacleList[guid].BigObstacleList.Add(aaa);
                }

            }

            log("Построил                             " + sw.ElapsedMilliseconds + " мс. Всего точек ");
        }
        public Dictionary<WowGuid, MyBigObstacle> MyBigObstacleList = new Dictionary<WowGuid, MyBigObstacle>();

        public class MyBigObstacle
        {
            public Vector3F Loc;
            public List<uint> BigObstacleList = new List<uint>();
            public DateTime Time;
        }

        public class MyGameObjectLocs
        {
            public List<MyGameObjectLoc> GameObjectLocs = new List<MyGameObjectLoc>();
        }

        public class MyGameObjectLoc
        {
            public uint Id;
            public List<Vector3F> ListLoc = new List<Vector3F>();
            public int MapId = -1;

        }

        public class MyNpcLocs
        {
            public List<MyNpcLoc> NpcLocs = new List<MyNpcLoc>();
        }

        public class MyNpcLoc
        {
            public uint Id;
            public Vector3F Loc;
            public bool IsArmorer;
            public List<Vector3F> ListLoc = new List<Vector3F>();
            public int MapId = -1;
            public uint FactionId;
            public ETeam Team;
            public bool IsTaxy;
        }

        public List<MyPlayer> MyPlayers = new List<MyPlayer>();

        public class MyPlayer
        {
            public string Name;
            public int Count;
            public DateTime Time;
            public int Level;
        }

        public ETeam GetTeam(Unit unit)
        {
            switch (Me.Team)
            {
                case ETeam.Horde:
                    {
                        if (unit.GetReactionTo(Me) == EReputationRank.Friendly)
                        {
                            return ETeam.Horde;
                        }

                        if (unit.GetReactionTo(Me) == EReputationRank.Unfriendly)
                        {
                            return ETeam.Alliance;
                        }

                        if (unit.GetReactionTo(Me) == EReputationRank.Hated)
                        {
                            return ETeam.Alliance;
                        }
                    }
                    break;
                case ETeam.Alliance:
                    {
                        if (unit.GetReactionTo(Me) == EReputationRank.Friendly)
                        {
                            return ETeam.Alliance;
                        }

                        if (unit.GetReactionTo(Me) == EReputationRank.Unfriendly)
                        {
                            return ETeam.Horde;
                        }

                        if (unit.GetReactionTo(Me) == EReputationRank.Hated)
                        {
                            return ETeam.Horde;
                        }
                    }
                    break;
                case ETeam.Other:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return ETeam.Other;
        }

        private void MyCheckPlayer()
        {
            foreach (var entity in GetEntities<Player>())
            {
                MyPlayer myPlayer = null;
                foreach (var player in MyPlayers)
                {
                    if (player.Name == entity.Name)
                    {
                        myPlayer = player;
                    }
                }

                if (myPlayer != null && myPlayer.Time.AddSeconds(10) > DateTime.Now)
                {
                    continue;
                }

                if (myPlayer == null)
                {
                    myPlayer = new MyPlayer
                    {
                        Name = entity.Name,
                        Count = +1,
                        Time = DateTime.Now,
                        Level = entity.Level
                    };
                    MyPlayers.Add(myPlayer);
                }
                else
                {
                    for (var i = 0; i < MyPlayers.Count; i++)
                    {
                        if (MyPlayers[i] != myPlayer)
                        {
                            continue;
                        }

                        MyPlayers[i].Count++;
                        MyPlayers[i].Time = DateTime.Now;
                        MyPlayers[i].Level = entity.Level;
                        break;
                    }
                }

                var path = AssemblyDirectory + "\\Log\\" + GetCurrentAccount().Login + "_" + GetCurrentAccount().Name;
                if (!isReleaseVersion)
                {
                    path = AssemblyDirectory + "\\Plugins\\Quester\\Log\\" + GetCurrentAccount().Login + "_" + GetCurrentAccount().Name;
                }

                File.AppendAllText(path + "_log.txt",
                    DateTime.Now.ToString(DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                    ": " + Me.Name + "(" + Me.Level + ") " + "Встретил игрока(" + myPlayer.Count + ")   Дист: " + (int)Me.Distance(entity)
                    + " Ник: " + myPlayer.Name + "(" + entity.Level + ")" + Environment.NewLine), Encoding.UTF8);

                //  log("Встретил игрока: " + myPlayer.Name + "(" + entity.Level + ")  Кол-во: " + myPlayer.Count + "   Дист: " + Me.Distance(entity), LogLvl.Error);
            }
        }

        bool _isneedSave;

        public bool IsPropExitis(uint id)
        {
            foreach (var gameObject in GetEntities<GameObject>())
            {
                if (!FarmModule.FarmZone.ObjInZone(gameObject))
                {
                    continue;
                }

                if (gameObject.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public List<Vector3F> BuildQuad(double x, double y, int r, double xr, double yr, Unit obj)
        {
            var sw = new Stopwatch();
            sw.Start();
            var range = 30;
            var listPoint = new List<Vector3F>();
            for (var x1 = x - range; x1 < x + range; x1 += 4)
            {
                for (var y1 = y - range; y1 < y + range; y1 += 4)
                {
                    //  var z = GetNavMeshHeight(new Vector3F(x1, y1, obj.Location.Z));
                    /*    if (!IsInsideNavMesh(new Vector3F((float)x1, (float)y1, obj.Location.Z)))
                            continue;*/

                    var d = Math.Sqrt(Math.Pow(xr - x1, 2) + Math.Pow(yr - y1, 2));
                    if (d <= r + 1)
                    {
                        continue;
                        // Console.WriteLine("Точка М лежит в круге.");   
                    }
                    // else
                    // Console.WriteLine("Точка М лежит вне круга.");



                    listPoint.Add(new Vector3F(x1, y1, obj.Location.Z));
                    //  log("Ставлю обстакл 2 " + x1 + " " + y1 + "  " + obj.Location.Z);
                    AddObstacle(new Vector3F(x1, y1, obj.Location.Z), 4, 4);
                    //  Thread.Sleep(100);
                    //  CreateNewEditorGpsPoint(new Vector3F(x1, y1, 0));
                }
            }
            log("Построил                             " + sw.ElapsedMilliseconds + " мс. Всего точек " + listPoint.Count);
            return listPoint;
        }

        public DateTime NextSave = DateTime.MinValue;

        private void MyCheckNpc()
        {
            MyCheckPlayer();
            try
            {

                foreach (var gameObject in GetEntities<GameObject>())
                {
                    if (gameObject.Id == 0)
                    {
                        continue;
                    }

                    if (gameObject.Location == Vector3F.Zero)
                    {
                        continue;
                    }

                    if (MyGameObjectLocss.GameObjectLocs.Any(collectionInvItem => gameObject.Id == collectionInvItem.Id)
                    )
                    {
                        foreach (var myNpcLoc in MyGameObjectLocss.GameObjectLocs)
                        {
                            if (myNpcLoc.Id == gameObject.Id)
                            {
                                var newLoc = true;
                                if (myNpcLoc.MapId == -1)
                                {
                                    myNpcLoc.MapId = gameObject.Guid.GetMapId();
                                    //  log("Добавляю MapId " + gameObject.Guid.GetMapId());
                                    _isneedSave = true;
                                }

                                foreach (var vector3F in myNpcLoc.ListLoc)
                                {
                                    if (MyDistance(gameObject.Location, vector3F) < 10)
                                    {
                                        newLoc = false;
                                        break;
                                    }

                                }


                                if (newLoc)
                                {

                                    myNpcLoc.ListLoc.Add(gameObject.Location);
                                    //  log("Найдена новая локация GO [" + gameObject.Id + "]" + gameObject.Name + " " +gameObject.Location + "  " + myNpcLoc.ListLoc.Count, ELogLvl.Important);
                                    _isneedSave = true;
                                }

                            }
                        }

                        continue;
                    }

                    _isneedSave = true;
                    //log("Найден новый GO " + gameObject.Name + " " + gameObject.Type, ELogLvl.Important);
                    MyGameObjectLocss.GameObjectLocs.Add(new MyGameObjectLoc
                    {
                        Id = gameObject.Id,
                        MapId = gameObject.Guid.GetMapId(),
                        ListLoc = new List<Vector3F>
                        {
                            gameObject.Location
                        }

                    });
                }

                if (_isneedSave)
                {
                    // File.Copy(PathGameObjectLocs, PathNpCjsonCopy, true);
                    ConfigLoader.SaveConfig(_pathGameObjectLocs, MyGameObjectLocss);
                    _isneedSave = false;
                }

               
                foreach (var entity in GetEntities<Unit>())
                {
                    if (entity.Id == 0)
                    {
                        continue;
                    }

                    if (entity.OwnerGuid != WowGuid.Zero)
                    {
                        continue;
                    }

                    if (entity.Charmer != null)
                    {
                        continue;
                    }

                    if (entity.Type != EBotTypes.Unit)
                    {
                        if (MyNpcLocss.NpcLocs.Any(collectionInvItem => entity.Id == collectionInvItem.Id))
                        {
                            //  log("Ошибочный НПС в списке " + entity.Name + " [" + entity.Id + "] " + entity.Type,ELogLvl.Error);
                            if (entity.Type == EBotTypes.Pet || entity.Type == EBotTypes.Vehicle)
                            {
                                foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                                {
                                    if (myNpcLoc.Id == entity.Id)
                                    {
                                        MyNpcLocss.NpcLocs.Remove(myNpcLoc);
                                        break;
                                    }
                                }
                            }
                        }

                        continue;

                    }

                    if (entity.Type == EBotTypes.Pet || entity.Type == EBotTypes.Vehicle)
                    {
                        continue;
                    }

                    if (entity.IsSpellClick)
                    {
                        /* if (AdvancedLog)
                         log("Найден НПС SpellClick " + entity.Name + " " + Me.Distance(entity), LogLvl.Error);*/
                    }

                    if (entity.Location == Vector3F.Zero)
                    {
                        continue;
                    }

                    if (double.IsNaN(entity.Location.X))
                    {
                        continue;
                    }

                    if (MyNpcLocss.NpcLocs.Any(collectionInvItem => entity.Id == collectionInvItem.Id))
                    {
                        foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                        {
                            if (myNpcLoc.Id == entity.Id)
                            {
                                /* if (entity.FactionId != myNpcLoc.FactionId)
                                 {
                                     myNpcLoc.FactionId = entity.FactionId;
                                     log("Добавляю FactionId " + entity.FactionId);
                                     _isneedSave = true;
                                 }*/

                                if (entity.IsTaxi && !myNpcLoc.IsTaxy)
                                {
                                    myNpcLoc.IsTaxy = entity.IsTaxi;
                                    //   log("Добавляю IsTaxy  " + entity.Name);
                                    _isneedSave = true;
                                }

                                var newLoc = true;
                                if (myNpcLoc.MapId == -1)
                                {
                                    myNpcLoc.MapId = entity.Guid.GetMapId();
                                    //   log("Добавляю MapId " + entity.Guid.GetMapId());
                                    _isneedSave = true;
                                }

                                if (entity.IsAlive && myNpcLoc.Team == ETeam.Other)
                                {
                                    if (GetTeam(entity) != myNpcLoc.Team)
                                    {
                                        myNpcLoc.Team = GetTeam(entity);
                                        //    log("Добавляю пренадлежность " + myNpcLoc.Team + "  " + entity.Name);
                                        _isneedSave = true;
                                    }
                                }


                                foreach (var vector3F in myNpcLoc.ListLoc)
                                {
                                    /*      if (entity.Id == 37956)
                                          log(entity.Name + "  Location:" + entity.Location + " vector3F: " + vector3F + "  дистанция: " + entity.Distance(vector3F) + " MyDistance: " + MyDistance(entity.Location, vector3F));
                                   */
                                    if (MyDistance(entity.Location, vector3F) < 40)
                                    {
                                        newLoc = false;
                                        break;
                                    }

                                }

                                if (MyDistance(myNpcLoc.Loc, entity.Location) < 40)
                                {
                                    newLoc = false;
                                }

                                if (newLoc)
                                {

                                    myNpcLoc.ListLoc.Add(entity.Location);
                                    //   log("Найдена новая локация NPC [" + entity.Id + "]" + entity.Name + " " +entity.Location + "  " + myNpcLoc.ListLoc.Count, ELogLvl.Important);
                                    _isneedSave = true;
                                }

                            }
                        }

                        continue;
                    }

                    _isneedSave = true;
                    //    log("Найден новый НПС " + entity.Name + " " + entity.Type, ELogLvl.Important);
                    MyNpcLocss.NpcLocs.Add(new MyNpcLoc
                    {

                        Id = entity.Id,
                        Loc = entity.Location,
                        IsArmorer = entity.IsArmorer,
                        IsTaxy = entity.IsTaxi,
                        //IsQuestGiver = entity.IsQuestGiver,
                        MapId = entity.Guid.GetMapId(),
                        FactionId = entity.FactionId,
                        ListLoc = new List<Vector3F>
                        {
                            entity.Location
                        }

                    });
                }

                if (_isneedSave && DateTime.UtcNow > NextSave)
                {
                    
                    NextSave = DateTime.UtcNow.AddMinutes(3);
                    var sw = new Stopwatch();
                    sw.Start();

                    if (File.Exists(_pathNpCjson))
                    {
                        var myNpcLocss2 = (MyNpcLocs)ConfigLoader.LoadConfig(_pathNpCjson, typeof(MyNpcLocs), MyNpcLocss);
                        if (GetBotLogin() == "Daredevi1")
                            log("Старая " + MyNpcLocss.NpcLocs.Count + " /" + myNpcLocss2.NpcLocs.Count);
                        foreach (var myNpcLoc in myNpcLocss2.NpcLocs)
                        {
                            if (MyNpcLocss.NpcLocs.Any(i => i.Id == myNpcLoc.Id))
                                continue;

                            MyNpcLocss.NpcLocs.Add(myNpcLoc);
                            /* if (GetBotLogin() == "Daredevi1")
                                 log("Добавляю");*/

                        }
                    }

                    /*  if (File.Exists(_pathNpCjson))
                      {
                          File.Copy(_pathNpCjson, _pathNpCjsonCopy, true);
                      }*/

                    ConfigLoader.SaveConfig(_pathNpCjson, MyNpcLocss);
                    _isneedSave = false;
                    if (GetBotLogin() == "Daredevi1")
                        log("Сохраняю " + sw.ElapsedMilliseconds);
                }
            }
            catch (SystemException)
            {

            }
            catch (Exception e)
            {
                log(e + "");
            }

        }


        public List<MyNpcLoc> IsBadNpcLocs = new List<MyNpcLoc>();

        private readonly List<uint> _badNpcForSell = new List<uint>
        {
            143875,
            3319,
            46512,
            46359,
            54657,
            69977,
            69978,
            3314,
            32641,
            3331,
            5816,
            3330,
            3315,
            3317,
            3321,
            3316,
            3493,
            3492,
            142427,
            135072,
            131800,
            138151,
            141813
        };




        public bool MyUseItemAndWait(Item item, Entity target = null)
        {
            try
            {
                if (item == null)
                {
                    log("Нет предмета");
                    return false;
                }

                switch (item.Id)
                {
                    case 152572:
                        break;
                    default:
                        CanselForm();
                        break;
                }


                Thread.Sleep(100);
                if (SpellManager.GetItemCooldown(item.Id) > 0)
                {
                    if (AdvancedLog)
                    {
                        log(item.Name + " MyUseItemAndWait  GetItemCooldown:" + SpellManager.GetItemCooldown(item.Id));
                    }

                    return false;
                }
                else
                {
                    CommonModule.MyUnmount();
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsChanneling)
                    {
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(100);
                    var result = SpellManager.UseItem(item, target);
                    if (result == EInventoryResult.OK)
                    {
                        log("Использую " + item.Name + "[" + item.Id + "] " + item.Place, LogLvl.Ok);
                    }
                    else
                    {
                        log("Не получилось использовать  1 " + item.Name + "[" + item.Id + "]  на " + target?.Name + " " + result + "  " + GetLastError(), LogLvl.Error);
                    }

                    Thread.Sleep(1000);
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsChanneling)
                    {
                        Thread.Sleep(50);
                    }

                    if (CanPickupLoot())
                    {
                        if (!PickupLoot())
                        {
                            log("Не смог поднять дроп " + "   " + GetLastError(), LogLvl.Error);
                        }
                    }
                    return result == EInventoryResult.OK;
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return false;
        }

        private readonly List<Unit> _listMobs = new List<Unit>();
        public List<Unit> SkinUnits = new List<Unit>();
        public List<Unit> GetAgroCreatures()
        {
            try
            {
                var myListCreature = new List<Unit>();
                if (FarmModule.FarmState == FarmState.Disabled)
                {
                    return myListCreature;
                }

                for (var index = 0; index < SkinUnits.Count; index++)
                {
                    if (index == SkinUnits.Count)
                    {
                        break;
                    }

                    var skinUnit = SkinUnits[index];
                    if (GetVar(skinUnit, "skinFailed") != null)
                    {
                        SkinUnits.RemoveAt(index);
                        break;
                    }
                    if (index < SkinUnits.Count && !IsExists(skinUnit))
                    {
                        SkinUnits.RemoveAt(index);
                        break;
                    }
                }

                for (var i = 0; i < _listMobs.Count; i++)
                {
                    if (i == _listMobs.Count)
                    {
                        break;
                    }

                    if (!IsAlive(Me))
                    {
                        break;
                    }

                    if (i < _listMobs.Count && !IsExists(_listMobs[i]))
                    {
                        // log("2");
                        _listMobs.RemoveAt(i);
                        break;
                    }
                    if (i < _listMobs.Count && !IsAlive(_listMobs[i]))
                    {
                        // log("1");
                        _listMobs.RemoveAt(i);
                        KillMobsCount++;
                        break;
                    }
                }

                if (ClientType == EWoWClient.Classic)
                {
                    foreach (var entity in GetEntities<Unit>())
                    {
                        if (!entity.IsAlive)
                        {
                            continue;
                        }

                        if (entity == FarmModule.BestMob && entity.HpPercents < 80)
                        {
                            if (!CharacterSettings.KillRunaways)
                            {
                                if (entity.Target != Me)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (entity.Target != Me)
                            {
                                continue;
                            }
                            if (!entity.IsInCombat)
                                continue;
                        }

                        if (FarmModule.IsBadTarget(entity, FarmModule.TickTime))
                        {
                            continue;
                        }

                        if (FarmModule.IsImmuneTarget(entity))
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

                        if (!CanAttack(entity, CanSpellAttack))
                        {
                            continue;
                        }

                        myListCreature.Add(entity);
                        if (!_listMobs.Contains(entity) && IsAlive(entity))
                        {
                            _listMobs.Add(entity);
                        }

                        if (!SkinUnits.Contains(entity) && entity.IsAlive)
                        {
                            SkinUnits.Add(entity);
                        }
                    }

                    if (Me.GetPet() != null)
                    {
                        foreach (var entity in GetEntities<Unit>())
                        {
                            if (!entity.IsAlive)
                            {
                                continue;
                            }

                            if (entity == FarmModule.BestMob && entity.HpPercents < 80)
                            {
                                if (!CharacterSettings.KillRunaways)
                                {
                                    if (entity.Target != Me.GetPet())
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                if (entity.Target != Me.GetPet())
                                {
                                    continue;
                                }
                            }
                            if (entity.Type == EBotTypes.Player)
                            {
                                continue;
                            }

                            if (FarmModule.IsBadTarget(entity, FarmModule.TickTime))
                            {
                                continue;
                            }

                            if (FarmModule.IsImmuneTarget(entity))
                            {
                                continue;
                            }

                            if (entity.Type == EBotTypes.Pet)
                            {
                                continue;
                            }

                            if (!CanAttack(entity, CanSpellAttack))
                            {
                                continue;
                            }

                            if (myListCreature.Contains(entity))
                            {
                                continue;
                            }

                            myListCreature.Add(entity);
                            if (!_listMobs.Contains(entity) && IsAlive(entity))
                            {
                                _listMobs.Add(entity);
                            }

                            if (!SkinUnits.Contains(entity) && entity.IsAlive)
                            {
                                SkinUnits.Add(entity);
                            }
                        }
                    }

                    if (CharacterSettings.Mode == Mode.QuestingClassic)
                    {
                        if (AutoQuests?.Convoy != null)
                        {
                            foreach (var entity in GetEntities<Unit>())
                            {
                                if (!entity.IsAlive)
                                {
                                    continue;
                                }

                                if (entity == FarmModule.BestMob && entity.HpPercents < 80)
                                {
                                    if (!CharacterSettings.KillRunaways)
                                    {
                                        if (entity.Target != Me)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    if (entity.Target != AutoQuests.Convoy)
                                    {
                                        continue;
                                    }
                                }
                                if (entity.Type == EBotTypes.Player)
                                {
                                    continue;
                                }

                                if (entity.Type == EBotTypes.Pet)
                                {
                                    continue;
                                }

                                if (!CanAttack(entity, CanSpellAttack))
                                {
                                    continue;
                                }

                                if (myListCreature.Contains(entity))
                                {
                                    continue;
                                }

                                myListCreature.Add(entity);
                                if (!_listMobs.Contains(entity) && IsAlive(entity))
                                {
                                    _listMobs.Add(entity);
                                }

                                if (!SkinUnits.Contains(entity) && entity.IsAlive)
                                {
                                    SkinUnits.Add(entity);
                                }
                            }
                        }
                    }
                }


                foreach (var i in Me.GetThreats())
                {
                    if (i.Obj == null)
                    {
                        continue;
                    }

                    if (!i.Obj.IsAlive)
                    {
                        continue;
                    }

                    if (AutoQuests.BestQuestId != 47576)
                    {
                        if (Me.Distance(i.Obj) > 40)
                        {
                            continue;
                        }
                    }

                    if (i.Obj.Id == 128604)
                    {
                        continue;
                    }

                    if (myListCreature.Contains(i.Obj))
                    {
                        continue;
                    }

                    myListCreature.Add(i.Obj);
                    if (!_listMobs.Contains(i.Obj) && IsAlive(i.Obj))
                    {
                        _listMobs.Add(i.Obj);
                    }
                }

                return myListCreature;
            }
            catch (ThreadAbortException)
            {
                return null;
            }
            catch (Exception e)
            {
                log(e.ToString());
                var fixCreature = new List<Unit>();
                return fixCreature;
            }
        }

        public List<MyItemsStat> StartInv = new List<MyItemsStat>();
        public List<MyItemsStat> UpdateInv = new List<MyItemsStat>();

        public class MyItemsStat
        {
            public uint Id { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }


        public void GetStartInventory()
        {
            try
            {
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Bag1 ||
                        item.Place == EItemPlace.Bag2 || item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4)
                    {
                        var isNewItem = true;
                        foreach (var myItemsStat in StartInv)
                        {
                            if (item.Id == myItemsStat.Id)
                            {
                                myItemsStat.Count += item.Count;
                                isNewItem = false;
                            }
                        }
                        if (isNewItem)
                        {
                            StartInv.Add(new MyItemsStat { Id = item.Id, Count = item.Count, Name = item.Name });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        public void GetUpdateInventory()
        {
            try
            {
                UpdateInv.Clear();
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.InventoryItem || item.Place == EItemPlace.Bag1 ||
                        item.Place == EItemPlace.Bag2 || item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4)
                    {
                        var isNewItem = true;
                        foreach (var myItemsStat in UpdateInv)
                        {
                            if (item.Id == myItemsStat.Id)
                            {
                                myItemsStat.Count += item.Count;
                                isNewItem = false;
                            }
                        }
                        if (isNewItem)
                        {
                            UpdateInv.Add(new MyItemsStat { Id = item.Id, Count = item.Count, Name = item.Name });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        //public void CompInv()
        //{
        //    try
        //    {
        //        var result = new List<MyItemsStat>();



        //        foreach (var myItemsStat in UpdateInv)
        //        {
        //            var isFind = false;
        //            foreach (var itemsStat in StartInv)
        //            {
        //                if (myItemsStat.Id == itemsStat.Id) //Нашел
        //                {
        //                    if (myItemsStat.Count != itemsStat.Count)
        //                    {
        //                        var isNewItem = true;
        //                        foreach (var stat in result)
        //                        {

        //                            if (stat.Id == myItemsStat.Id)
        //                            {
        //                                isNewItem = false;
        //                                stat.Count = myItemsStat.Count - itemsStat.Count;
        //                            }
        //                        }
        //                        if (isNewItem)
        //                        {
        //                            result.Add(new MyItemsStat { Id = itemsStat.Id, Count = myItemsStat.Count - itemsStat.Count, Name = itemsStat.Name });
        //                        }
        //                    }
        //                    isFind = true;
        //                }

        //            }
        //            if (!isFind)
        //            {
        //                result.Add(new MyItemsStat { Id = myItemsStat.Id, Count = myItemsStat.Count, Name = myItemsStat.Name });
        //            }
        //        }
        //        var allTime = DateTime.Now - TimeWork;
        //        MainForm.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            MainForm.LabelInvUpdate.Content = "Имя: Получено (расчет на сутки)" + ItemManager.GetFreeInventorySlotsCount() + "\n";
        //        }));

        //        foreach (var myItemsStat in result)
        //        {
        //            MainForm.Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                MainForm.LabelInvUpdate.Content = MainForm.LabelInvUpdate.Content + myItemsStat.Name +/* "[" + myItemsStat.Id + */": " + myItemsStat.Count + " (" +
        //                                                  Math.Round((myItemsStat.Count /*+ doubleGold*/) / allTime.TotalDays, 2) + ")\n";
        //            }));
        //            //  log(myItemsStat.Name + "  " + myItemsStat.Count );
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log(e.ToString());
        //    }
        //}

        public void MySendKeyEsc()
        {

            if (CurrentInteractionGuid != WowGuid.Zero)
                SendKeyPress(0x1b);
            return;

        }

        public void FoodPet()
        {
            if (Me.Class != EClass.Hunter)
            {
                return;
            }

            if (Me.GetPet() == null)
            {
                return;
            }

            if (!Me.GetPet().IsAlive)
            {
                log("Питомец мертв");
                return;
            }

            if (Me.IsInCombat)
            {
                return;
            }

            if (Me.GetPet().GetPower(EPowerType.Happiness) > 667000)
            {
                return;
            }

            if (Me.GetPet().GetAuras().Any(i => i.SpellId == 1539))
            {
                return;
            }

            if (SpellManager.GetSpell(6991) == null)
            {
                return;
            }

            foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
            {
                if (characterSettingsItemSetting.Use == EItemUse.FoodPet)
                {
                    var item = MyGetItem(characterSettingsItemSetting.Id);
                    if (item != null)
                    {
                        log("Скармливаю питомцу " + item.Name);
                        var res = SpellManager.CastSpell(6991, item, Me.Location);
                        if (res != ESpellCastError.SUCCESS)
                        {
                            log("Не удалось скормить " + res + " " + GetLastError(), LogLvl.Error);
                        }
                        var waitTill = DateTime.UtcNow.AddSeconds(15);
                        while (waitTill > DateTime.UtcNow)
                        {
                            Thread.Sleep(100);
                            if (!MainForm.On)
                            {
                                return;
                            }

                            if (Me.IsInCombat)
                            {
                                return;
                            }
                        }
                        return;
                    }
                }
            }
        }

        public decimal MeMpPercent()
        {
            decimal power = Me.GetPower(Me.PowerType);
            decimal maxPower = Me.GetMaxPower(Me.PowerType);
            var percent = power * 100 / maxPower;
            if (Me.Class == EClass.Warrior)
            {
                percent = 100;
            }

            return percent;
        }

        public class MyObstacleDic
        {
            public uint Id;
            public Vector3F Loc;
        }

        public Dictionary<WowGuid, DateTime> ListGuidPic = new Dictionary<WowGuid, DateTime>();

        public bool CheckInvisible()
        {
            foreach (var aura in Me.GetAuras())
            {
                if (aura.SpellName == "Stealth")
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckCooldownInbisible()
        {
            try
            {
                if (GetBotLogin() != "Armin")
                {
                    return true;
                }

                if (!CharacterSettings.PikPocket)
                {
                    return true;
                }

                Spell advancedInvisibleSpell = null;
                var prepareSpell = SpellManager.GetSpell(14185);

                foreach (var spell in SpellManager.GetSpells())
                {
                    if (spell.Name == "Vanish")
                    {
                        advancedInvisibleSpell = spell;
                    }
                }

                if (SpellManager.GetSpellCooldown(advancedInvisibleSpell) != 0 && SpellManager.GetSpellCooldown(prepareSpell) != 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                log(e + " ");
                return true;
            }
        }

        public void AdvancedInvisible()
        {
            if (CharacterSettings.Mode != Mode.QuestingClassic)
            {
                if (!CharacterSettings.PikPocket)
                {
                    return;
                }
            }

            if (CheckInvisible())
            {
                return;
            }

            if (!Me.IsAlive)
            {
                return;
            }

            if (Me.IsDeadGhost)
            {
                return;
            }

            Spell invisibleSpell = null;
            foreach (var spell in SpellManager.GetSpells())
            {
                if (spell.Name == "Stealth")
                {
                    invisibleSpell = spell;
                }
            }


            if (invisibleSpell != null && !Me.IsInCombat && SpellManager.GetSpellCooldown(invisibleSpell) == 0)
            {
                log("Ухожу в инвиз");
                CommonModule.SuspendMove();
                FarmModule.UseSkillAndWait(invisibleSpell.Id);
                CommonModule.ResumeMove();
                return;
            }

            Spell advancedInvisibleSpell = null;
            if (!Me.IsInCombat)
            {
                return;
            }

            foreach (var spell in SpellManager.GetSpells())
            {
                if (spell.Name == "Vanish")
                {
                    advancedInvisibleSpell = spell;
                }
            }


            if (invisibleSpell != null && MyGetItem(5140) != null)
            {
                if (SpellManager.GetSpellCooldown(advancedInvisibleSpell) != 0)
                {
                    var prepareSpell = SpellManager.GetSpell(14185);
                    if (SpellManager.IsSpellReady(prepareSpell))
                    {
                        CommonModule.SuspendMove();
                        log("Сбрасываю КД");
                        FarmModule.UseSkillAndWait(prepareSpell.Id);
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        return;
                    }
                }

                if (advancedInvisibleSpell != null && SpellManager.GetSpellCooldown(advancedInvisibleSpell.Id) == 0)
                {

                    CommonModule.SuspendMove();
                    log("Ухожу в продвинутый инвиз");
                    var needMove = false;
                    foreach (var entity in GetEntities<Unit>())
                    {
                        if (Me.Distance(entity) < 2)
                        {
                            needMove = true;
                        }
                    }

                    if (needMove)
                    {
                        var safePoint = new List<Vector3F>();
                        var xc = Me.Location.X;
                        var yc = Me.Location.Y;

                        var radius = 4;
                        const double a = Math.PI / 16;
                        double u = 0;
                        for (var i = 0; i < 32; i++)
                        {
                            var x1 = xc + radius * Math.Cos(u);
                            var y1 = yc + radius * Math.Sin(u);
                            // log(" " + i + " x:" + x + " y:" + y);
                            u += a;

                            var z1 = GetNavMeshHeight(new Vector3F(x1, y1, 0));
                            if (IsInsideNavMesh(new Vector3F(x1, y1, z1)))
                            {
                                var vector = new Vector3F(x1, y1, z1);
                                var add = true;
                                foreach (var entity in GetEntities<Unit>())
                                {
                                    if (entity.Distance2D(vector) < 3)
                                    {
                                        add = false;
                                    }
                                }
                                if (add)
                                {
                                    safePoint.Add(new Vector3F(x1, y1, z1));
                                }
                            }

                        }
                        log("Пытаюсь обойти препядствие " + safePoint.Count);
                        if (safePoint.Count > 0)
                        {
                            var bestPoint = safePoint[RandGenerator.Next(safePoint.Count)];
                            ComeTo(bestPoint);
                        }
                        else
                        {
                            MyMoveBackward(1000);
                        }
                    }


                    FarmModule.UseSkillAndWait(advancedInvisibleSpell.Id);
                    Thread.Sleep(1000);
                    CommonModule.ResumeMove();
                }
            }
        }

        public Dictionary<WowGuid, MyObstacleDic> DicObstaclePic = new Dictionary<WowGuid, MyObstacleDic>();
        public Dictionary<WowGuid, int> DictionaryMove = new Dictionary<WowGuid, int>();
        public void ObstaclePic()
        {
            try
            {
                foreach (var entity in GetEntities<Unit>().OrderBy(i => Me.Distance(i)))
                {
                    if (entity.Id == 7269)
                    {
                        continue;
                    }

                    if (!CanAttack(entity, CanSpellAttack))
                    {
                        continue;
                    }

                    if (Me.Distance(entity) > 60)
                    {
                        if (!DicObstaclePic.ContainsKey(entity.Guid))
                        {
                            continue;
                        }

                        RemoveObstacle(DicObstaclePic[entity.Guid].Id);
                        DicObstaclePic.Remove(entity.Guid);
                    }
                    else
                    {
                        if (DicObstaclePic.ContainsKey(entity.Guid))
                        {
                            if (DicObstaclePic[entity.Guid].Loc.Distance2D(entity.Location) > 1)
                            {
                                //  log("Переставляю обстакл");
                                var rad = 2.5;
                                if (MapID == 0 || MapID == 1)
                                {
                                    rad = 5;
                                }

                                RemoveObstacle(DicObstaclePic[entity.Guid].Id);
                                var obsLoc = new Vector3F(entity.Location.X, entity.Location.Y, entity.Location.Z - 1);
                                var obs = AddObstacle(obsLoc, rad, 3);
                                DicObstaclePic[entity.Guid].Id = obs;
                                DicObstaclePic[entity.Guid].Loc = entity.Location;
                                if (!DictionaryMove.ContainsKey(entity.Guid))
                                {
                                    DictionaryMove.Add(entity.Guid, 0);
                                }
                                // CommonModule.MoveTo(loc);
                            }

                        }
                        else
                        {
                            var rad = 2;
                            if (entity.Distance(264.80, -388.54, 20.08) < 5 || entity.Distance(175.67, -429.14, 18.53) < 5)
                            {
                                rad = 1;
                            }
                            if (MapID == 0 || MapID == 1)
                            {
                                rad = 5;
                            }

                            // log("Ставлю новый обстакл");
                            var obsLoc = new Vector3F(entity.Location.X, entity.Location.Y, entity.Location.Z - 1);
                            var obs = AddObstacle(obsLoc, rad, 3);
                            DicObstaclePic.Add(entity.Guid, new MyObstacleDic { Loc = entity.Location, Id = obs });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log(e + "");
            }
        }

        public bool CheckPikPocket()
        {
            if (!CharacterSettings.PikPocket)
            {
                return false;
            }

            if (SpellManager.GetSpell(921) != null)
            {
                foreach (var aura in Me.GetAuras())
                {
                    if (aura.SpellName == "Stealth")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        DateTime _nextCallPet = DateTime.MinValue;
        public void CallPet()
        {
            if (Me.IsInFlight)
            {
                return;
            }

            if (Me.IsDeadGhost)
            {
                return;
            }

            if (Me.MountId != 0)
            {
                return;
            }

            var needsummon = true;

            if (Me.Class == EClass.Warlock)
            {
                if (SpellManager.GetSpell(688) == null)
                {
                    return;
                }

                if (_nextCallPet > DateTime.UtcNow)
                {
                    return;
                }

                foreach (var entity in GetEntities<Unit>())
                {
                    if (entity.Owner != Me)
                    {
                        continue;
                    }

                    needsummon = false;
                    /* if (!entity.IsAlive)
                         needrevive = true;*/
                }

                if (needsummon && CharacterSettings.SummonBattlePet)
                {
                    uint petSkill = 0;
                    switch (CharacterSettings.BattlePetNumber)
                    {
                        case 0:
                            petSkill = 688;
                            break;
                        case 1:
                            {
                                petSkill = 697;
                                if (MyGetItem(6265) == null)
                                    petSkill = 688;
                            }

                            break;
                            /*  case 2:
                                  petSkill = 83243;
                                  break;
                              case 3:
                                  petSkill = 83244;
                                  break;
                              case 4:
                                  petSkill = 83245;
                                  break;*/
                    }

                    if (petSkill == 0)
                    {
                        log("Неизвестный питомец " + CharacterSettings.BattlePetNumber, LogLvl.Error);
                    }
                    else
                    {
                        if (SpellManager.GetSpell(petSkill) != null)
                        {
                            CommonModule.SuspendMove();
                            MyCheckIsMovingIsCasting();
                            var pet = SpellManager.CastSpell(petSkill);
                            if (pet == ESpellCastError.SUCCESS)
                            {
                                log("Призвал питомца " + PetTameFailureReason, LogLvl.Ok);
                                Thread.Sleep(1000);
                            }

                            while (SpellManager.IsCasting)
                            {
                                Thread.Sleep(100);
                            }
                            CommonModule.ResumeMove();
                        }
                    }
                }

                return;
            }


            if (Me.Class != EClass.Hunter)
            {
                return;
            }

            if (SpellManager.GetSpell(6991) == null)
            {
                return;
            }

            if (_nextCallPet > DateTime.UtcNow)
            {
                return;
            }


            var needrevive = false;
            foreach (var entity in GetEntities<Unit>())
            {
                if (entity.Owner != Me)
                {
                    continue;
                }

                needsummon = false;
                /* if (!entity.IsAlive)
                     needrevive = true;*/
            }

            if (Me.GetPet() == null)
            {
                needsummon = true;
            }

            if (Me.GetPet() != null)
            {
                if (!Me.GetPet().IsAlive)
                {
                    needrevive = true;
                }
            }

            if (needrevive /*&& GetAgroCreatures().Count == 0*/)
            {
                if (!CharacterSettings.ResPetInCombat)
                {
                    if (GetAgroCreatures().Count > 0)
                    {
                        return;
                    }
                }

                CommonModule.SuspendMove();

                while (MeMpPercent() < CharacterSettings.ResPetMeMp)
                {
                    Thread.Sleep(1000);
                    if (GetAgroCreatures().Count > 0)
                    {
                        break;
                    }

                    if (!MainForm.On)
                    {
                        return;
                    }

                    if (!Me.IsAlive)
                    {
                        return;
                    }
                }

                Thread.Sleep(1000);

                var spellInfo = SpellManager.GetSpell(982).SpellInfo;
                var powerCosts = spellInfo.CalcPowerCost(Me, spellInfo.SchoolMask);
                var mpToCast = 0;
                foreach (var cost in powerCosts)
                {
                    if (SpellManager.GetPowerByClientType(cost.Power) == EPowerType.Mana)
                    {
                        mpToCast = cost.Amount;
                        break;
                    }
                }

                var pet = SpellManager.CastSpell(982);
                if (pet == ESpellCastError.SUCCESS)
                {
                    log("Воскрешаю питомца", LogLvl.Ok);
                }
                else
                {
                    log("Не удалось воскресить питомца " + pet + " Нужно MP " + mpToCast + " есть MP " + MeMpPercent(), LogLvl.Error);
                }

                Thread.Sleep(2000);
                while (SpellManager.IsCasting)
                {
                    Thread.Sleep(100);
                }

                CommonModule.ResumeMove();
            }

            // log("Пет " + needsummon + "  " + CharacterSettings.SummonBattlePet + );
            if (needsummon && CharacterSettings.SummonBattlePet)
            {
                uint petSkill = 0;
                switch (CharacterSettings.BattlePetNumber)
                {
                    case 0:
                        petSkill = 883;
                        break;
                    case 1:
                        petSkill = 83242;
                        break;
                    case 2:
                        petSkill = 83243;
                        break;
                    case 3:
                        petSkill = 83244;
                        break;
                    case 4:
                        petSkill = 83245;
                        break;
                }

                if (petSkill == 0)
                {
                    log("Неизвестный питомец " + CharacterSettings.BattlePetNumber, LogLvl.Error);
                }
                else
                {
                    if (SpellManager.GetSpell(petSkill) != null)
                    {
                        var pet = SpellManager.CastSpell(petSkill);
                        if (pet == ESpellCastError.SUCCESS)
                        {
                            log("Призвал питомца " + PetTameFailureReason, LogLvl.Ok);
                            Thread.Sleep(1000);
                        }

                        if (PetTameFailureReason == EPetTameFailureReason.NoPetAvailable)
                        {
                            log("Нет доступных питомцев ");
                            _nextCallPet = DateTime.UtcNow.AddSeconds(RandGenerator.Next(5, 15));
                        }
                        if (PetTameFailureReason == EPetTameFailureReason.Dead/* && GetAgroCreatures().Count == 0*/)
                        {
                            if (!CharacterSettings.ResPetInCombat)
                            {
                                if (GetAgroCreatures().Count > 0)
                                {
                                    return;
                                }
                            }
                            CommonModule.SuspendMove();
                            while (MeMpPercent() < CharacterSettings.ResPetMeMp)
                            {
                                Thread.Sleep(1000);
                                if (GetAgroCreatures().Count > 0)
                                {
                                    break;
                                }

                                if (!MainForm.On)
                                {
                                    return;
                                }

                                if (!Me.IsAlive)
                                {
                                    return;
                                }
                            }
                            Thread.Sleep(1000);
                            log("Pet = null");
                            var pet2 = SpellManager.CastSpell(982);
                            if (pet2 == ESpellCastError.SUCCESS)
                            {
                                log("Воскрешаю питомца", LogLvl.Ok);
                            }
                            else
                            {
                                log("Не удалось воскресить питомца " + pet2, LogLvl.Error);
                            }
                            Thread.Sleep(2000);
                            while (SpellManager.IsCasting)
                            {
                                Thread.Sleep(100);
                            }
                            CommonModule.ResumeMove();
                        }
                        else
                        {
                            _nextCallPet = DateTime.UtcNow.AddSeconds(RandGenerator.Next(5, 15));
                        }
                        if (pet != ESpellCastError.SUCCESS)
                        {
                            log("Не удалось призвать питомца " + pet + " " + PetTameFailureReason, LogLvl.Error);
                        }
                        //  }
                        while (SpellManager.IsCasting)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        public bool MyIsNeedRegen()
        {
            if (!Me.IsAlive)
            {
                return false;
            }

            if (Me.IsDeadGhost)
            {
                return false;
            }

            if (!CharacterSettings.UseRegen)
            {
                return false;
            }

            if (MeMpPercent() < CharacterSettings.MpRegen || Me.HpPercents < CharacterSettings.HpRegen)
            {
                return true;
            }

            if (Me.GetPet() != null)
            {
                if (Me.GetPet().IsAlive)
                {
                    if (Me.GetPet().HpPercents < CharacterSettings.PetRegen)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Unit CheckPathForMob(Entity target)
        {
            if (!CharacterSettings.AttackMobForDrop)
            {
                return null;
            }

            var path = GetSmoothPath(Me.Location, target.Location);
            for (var index = 0; index < path.Path.Count - 1; index++)
            {
                var fromVector3F = path.Path[index];
                var toVector3F = path.Path[index + 1];
                foreach (var entity in GetEntities<Unit>().OrderBy(i => fromVector3F.Distance(i.Location)))
                {
                    /*  if (entity.Distance(vector3F) > 40)
                        break;*/
                    if (entity.HpPercents < 100)
                    {
                        continue;
                    }
                    /* if (entity.Distance(vector3F) > 15)
   continue;*/

                    var zRange = Math.Abs(fromVector3F.Z - entity.Location.Z);

                    if (zRange > 5)
                    {
                        continue;
                    }

                    if (entity.GetReactionTo(Me) == EReputationRank.Neutral)
                    {
                        continue;
                    }

                    if (!CanAttack(entity, CanSpellAttack))
                    {
                        continue;
                    }

                    if (!IsAlive(entity))
                    {
                        continue;
                    }

                    if (FarmModule.IsBadTarget(entity, FarmModule.TickTime))
                    {
                        continue;
                    }

                    if (FarmModule.IsImmuneTarget(entity))
                    {
                        continue;
                    }

                    if (CharacterSettings.UseFilterMobs)
                    {
                        var mobsIgnore = false;
                        foreach (var characterSettingsMobsSetting in CharacterSettings.MobsSettings)
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

                    if (!CommonModule.CircleIntersects(entity.Location.X, entity.Location.Y, 10, fromVector3F.X, fromVector3F.Y, toVector3F.X, toVector3F.Y))
                    {
                        //  Host.log(entity.Name + " " + Host.Me.Distance(entity) + "   " + zRange);
                        continue;
                    }

                    if (FarmModule.BestMob != null)
                    {
                        if (Me.Distance(FarmModule.BestMob) < Me.Distance(entity))
                        {
                            return null;
                        }
                    }

                    if (entity != FarmModule.BestMob)
                    {
                        FarmModule.BestMob = entity;

                        CancelMoveTo();
                        CommonModule.SuspendMove();
                        if (CharacterSettings.LogScriptAction)
                        {
                            log(
                                "Перехват на луте: " + FarmModule.BestMob.Name + " дист:" + Me.Distance(entity) +
                                " всего:" + GetAgroCreatures().Count + "  IsAlive:" + IsAlive(entity) +
                                " HP:" + entity.Hp, LogLvl.Error);
                        }
                    }
                }
            }

            return null;
        }


        public bool CheckUnderWather()
        {
            if (GetTimerInfo(EMirrorTimerType.Breath).IsActivated)
            {
                log("Я под водой " + GetTimerInfo(EMirrorTimerType.Breath).SpellId + "  " + GetTimerInfo(EMirrorTimerType.Breath).Scale + " " + GetTimerInfo(EMirrorTimerType.Breath).Remain + " " + GetTimerInfo(EMirrorTimerType.Breath).MaxValue);
                if (Area.Id == 17 && Zone.Id == 391
                    || Area.Id == 14 && Zone.Id == 373
                    || MapID == 600 && Area.Id == 4196
                    || ClientType == EWoWClient.Classic
                    )
                {
                    if (GetTimerInfo(EMirrorTimerType.Breath).Remain < 30000)
                    {
                        log("Всплываю");
                        SetMoveStateForClient(true);
                        Ascend(true);
                        Thread.Sleep(2000);
                        Ascend(false);
                        SetMoveStateForClient(false);
                    }
                }
                return true;
            }

            return false;
        }

        public static int GetPercent(int b, int a)
        {
            if (b == 0)
            {
                return 0;
            }

            return (int)(a / (b / 100M));
        }

        public void MyUseGameObject(uint id)
        {
            var go = GetNpcById(id);
            if (go != null)
            {
                CommonModule.MyUnmount();
                switch (id)
                {
                    case 273992:
                        break;
                    case 291008:
                        break;
                    case 290773:
                        CommonModule.MoveTo(go, 6);
                        break;
                    default:
                        if (Me.Distance(go) > 2)
                        {
                            CommonModule.MoveTo(go, 3);
                        }

                        break;
                }


                MyCheckIsMovingIsCasting();
                Thread.Sleep(100);

                if (!((GameObject)go).Use())
                {
                    log("Не смог использовать " + go.Name + " " + GetLastError(), LogLvl.Error);
                    Thread.Sleep(5000);
                    if (go.Id == 2059)
                    {
                        log("Ошибка апи, перезапускаю");
                        //TerminateGameClient();
                    }
                    return;
                }
                else
                {
                    log("Использовал " + go.Name, LogLvl.Ok);
                    Thread.Sleep(1000);
                }




                while (SpellManager.IsCasting)
                {
                    Thread.Sleep(100);
                }

                while (SpellManager.IsChanneling)
                {
                    Thread.Sleep(100);
                }

                Thread.Sleep(500);
                if (CanPickupLoot())
                {
                    if (!PickupLoot())
                    {
                        log("Не смог поднять дроп " + GetLastError(), LogLvl.Error);
                    }
                }
            }
            else
            {
                log("Не нашел геймобжект " + id);
            }
        }

        public void MyUseGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            CommonModule.MyUnmount();
            switch (go.Id)
            {
                case 273992:
                    break;
                case 291008:
                    break;
                case 289675:
                    CommonModule.MoveTo(go, 15);
                    break;

                default:
                    CommonModule.MoveTo(go, 1);
                    break;
            }

            MyCheckIsMovingIsCasting();
            if (!go.Use())
            {
                log("Не смог использовать " + go.Name + " " + GetLastError(), LogLvl.Error);
                Thread.Sleep(5000);
                return;
            }
            log("Использовал " + go.Name, LogLvl.Ok);

            while (SpellManager.IsCasting)
            {
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
            if (!CanPickupLoot())
            {
                return;
            }

            if (!PickupLoot())
            {
                log("Не смог поднять дроп " + GetLastError(), LogLvl.Error);
            }
        }

        public void CheckAttack()
        {

            try
            {
                if (FarmModule.BestMob != null)
                {
                    //log(TimeAttack + "  " + _badBestMobHp + "  " + FarmModule.TickTime + "  " + (FarmModule.TickTime - TimeAttack ));
                    if (_badSid != FarmModule.BestMob.Guid)
                    {
                        _badSid = FarmModule.BestMob.Guid;
                        _badBestMobHp = FarmModule.BestMob.HpPercents;
                        TimeAttack = GetUnixTime();
                    }

                    if (_badBestMobHp == 0 && _badSid == FarmModule.BestMob.Guid)
                    {
                        _badBestMobHp = FarmModule.BestMob.HpPercents;
                        TimeAttack = GetUnixTime();
                    }

                    if (Me.IsMoving /*|| host.CommonModule.InFight()*/)
                    {
                        _badBestMobHp = FarmModule.BestMob.HpPercents;
                        TimeAttack = GetUnixTime();
                    }




                    if (FarmModule.BestMob.HpPercents < _badBestMobHp && _badSid == FarmModule.BestMob.Guid)
                    {
                        _badBestMobHp = FarmModule.BestMob.HpPercents;
                        TimeAttack = GetUnixTime();
                    }
                    else
                    {
                        if (TimeAttack + CharacterSettings.IgnoreMob < FarmModule.TickTime && IsAlive(Me))
                        {
                            log("Плохая цель :" + FarmModule.BestMob.Name, LogLvl.Error);
                            FarmModule.SetBadTarget(FarmModule.BestMob, 30000);
                            CommonModule.ResumeMove();
                            Evade = true;
                            FarmModule.BestMob = null;
                            if (SpellManager.CurrentAutoRepeatSpellId != 0)
                            {
                                SpellManager.CancelAutoRepeatSpell();
                            }
                        }
                    }
                }
                else
                {
                    _badBestMobHp = 0;
                    TimeAttack = GetUnixTime();
                }
            }
            catch (Exception e)
            {
                log(e + "");
            }
        }


        internal bool Check()
        {
            try
            {
                if (MainForm.On
                    && !CancelRequested
                    && FarmModule.BestMob == null
                    && GameState == EGameState.Ingame
                    && CheckCanUseGameActions()
                    // && IsAlive(Me)
                    && FarmModule.ReadyToActions
                    && MyGetAura(269824) == null
                    && FarmModule.MobsWithDropCount() + FarmModule.MobsWithSkinCount() == 0
                    && !NeedWaitAfterCombat
                //&& Me.ConditionPhase != EConditionPhase.Spirit
                //&& Me.ConditionPhase != EConditionPhase.Dead

                // && CurInvCount() < 30
                // && !host.commonModule.InFight()
                // && host.Me.GetAgroCreatures() == 0
                // && !IsPenaltyBuff()
                // && host.Me.HpPercents > 80
                )
                {
                    return true;
                }
            }
            catch (Exception err)
            {
                log("Check: " + err);
            }
            return false;
        }


        internal bool FindNearPointInRadius(double x, double y, double z, int radius)
        {
            foreach (var editorGpsPoint in GetEditorGpsPoints())
            {
                if (MyDistance(editorGpsPoint.X, editorGpsPoint.Y, editorGpsPoint.Z, x, y, z) < radius)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool FindNearPointInRadiusNoZ(double x, double y, double radius)
        {
            foreach (var editorGpsPoint in GetEditorGpsPoints())
            {
                if (MyDistanceNoZ(editorGpsPoint.X, editorGpsPoint.Y, x, y) < radius)
                {
                    return true;
                }
            }
            return false;
        }

        public Aura MyGetAura(uint id)
        {
            try
            {
                foreach (var aura in Me.GetAuras())
                {
                    if (aura.SpellId == id)
                    {
                        return aura;
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return null;
        }

        public Aura MyGetAura(string name)
        {
            try
            {
                foreach (var aura in Me.GetAuras())
                {
                    if (aura.SpellName == name)
                    {
                        return aura;
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return null;
        }

        public List<uint> NoShowSkill = new List<uint>
        {
            3365,//3365   Opening 
            6233,//6233   Closing 
            6246,//6246   Closing 
            6247,//Opening 6247 
            6477,//Opening 6477 
            6478,//Opening 6478 
            7266,//Duel 7266 
            7355,//Stuck 7355  
            21651,//Opening 21651 
            21652,//Closing 21652 
            22810,//Opening - No Text 22810 
            22027,//Удаление знака отличия
            7267,//Ползание
            2479,//Бесславная цель
            8386,//Нападение
        };

        public bool MyOpenTaxyRoute()
        {
            foreach (var entity in GetEntities<Unit>())
            {
                if (entity.TaxiStatus != ETaxiNodeStatus.Unlearned)
                {
                    continue;
                }


                if (!CommonModule.MoveTo(entity, 2))
                {
                    return false;
                }

                Thread.Sleep(500);
                if (!OpenTaxi(entity))
                {
                    log("Не смог открыть диалог с " + entity.Name + " TaxiStatus:" + entity.TaxiStatus + " IsTaxi:" + entity.IsTaxi + "  " + GetLastError(), LogLvl.Error);
                }

                MyNodeListFill();
            }
            return true;
        }

        public bool MyOpenLockedChest()
        {
            if (!Me.IsAlive)
            {
                return true;
            }

            if (Me.IsDeadGhost)
            {
                return true;
            }

            if (MyGetAura("Stealth") == null && !Me.IsInCombat)
            {
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 &&
                        item.Place != EItemPlace.Bag3 && item.Place != EItemPlace.Bag4 &&
                        item.Place != EItemPlace.InventoryItem)
                    {
                        continue;
                    }

                    if (item.Id == 16884 || item.Id == 16883 || item.Id == 16882 || item.Id == 16885)
                    {
                        if (item.IsLocked)
                        {
                            var res = SpellManager.CastSpell(1804, item);
                            if (res != ESpellCastError.SUCCESS)
                            {
                                log("Не удалось открыть " + item.NameRu + " " + item.IsLocked + " " + res + " " + GetLastError(), LogLvl.Error);
                            }

                            MyCheckIsMovingIsCasting();
                            var waitTill = DateTime.UtcNow.AddSeconds(1);
                            while (waitTill > DateTime.UtcNow)
                            {
                                Thread.Sleep(100);
                                if (!MainForm.On)
                                {
                                    return false;
                                }

                                if (!item.IsLocked)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            MyUseItemAndWait(item);
                            var waitTill = DateTime.UtcNow.AddSeconds(1);
                            while (waitTill > DateTime.UtcNow)
                            {
                                Thread.Sleep(100);
                                if (!MainForm.On)
                                {
                                    return false;
                                }

                                if (CanPickupLoot())
                                {
                                    break;
                                }
                            }
                            if (CanPickupLoot())
                            {
                                PickupLoot();
                            }

                            Thread.Sleep(500);
                        }
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        public MyNpcLoc MyGetLocNpcById(uint id)
        {
            foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
            {
                if (myNpcLoc.Id == id)
                {
                    return myNpcLoc;
                }
            }
            return null;
        }

        public bool IsNeedAuk()
        {

            if (!CharacterSettings.CheckAuk)
            {
                return false;
            }

            var count = MeGetItemsCount(CharacterSettings.FreeInvCountForAukId);
            if (count < CharacterSettings.FreeInvCountForAuk)
            {
                return false;
            }

            if (CharacterSettings.CheckAukInTimeRange)
            {
                log(CharacterSettings.StartAukTime + "   ");
                log(CharacterSettings.EndAukTime + "   ");
                log(DateTime.Now.TimeOfDay.IsBetween(CharacterSettings.StartAukTime, CharacterSettings.EndAukTime) + " ");
                if (!DateTime.Now.TimeOfDay.IsBetween(CharacterSettings.StartAukTime, CharacterSettings.EndAukTime))
                {
                    return false;
                }
            }
            return true;
        }

        public Vector3F MyGetCoordQuestNpc(uint id, AutoQuests autoQuests)
        {
            foreach (var entity in GetEntities())
            {
                if (entity.Id == id && (entity.QuestGiverStatus & EQuestGiverStatus.Available) == EQuestGiverStatus.Available)
                {
                    return entity.Location;
                }
            }

            double bestDist = 99999;
            var bestLoc = new Vector3F();
            foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
            {
                if (myNpcLoc.Id != id)
                {
                    continue;
                }

                var badloc = false;
                foreach (var f in autoQuests.BadLoc)
                {
                    if (f.Distance(myNpcLoc.Loc) < 20)
                    {
                        badloc = true;
                    }
                }
                if (!badloc)
                {
                    bestDist = Me.Distance(myNpcLoc.Loc);
                    bestLoc = myNpcLoc.Loc;
                }

                foreach (var vector3F in myNpcLoc.ListLoc)
                {
                    badloc = false;
                    foreach (var f in autoQuests.BadLoc)
                    {
                        if (f.Distance(vector3F) < 20)
                        {
                            badloc = true;
                        }
                    }
                    if (badloc)
                    {
                        continue;
                    }

                    if (Me.Distance(vector3F) < bestDist)
                    {
                        bestDist = Me.Distance(vector3F);
                        bestLoc = vector3F;
                    }
                }
                return bestLoc;

            }
            return Vector3F.Zero;
        }

        public bool MyUseSpellClick(uint mobId)
        {
            try
            {
                Entity unit = null;
                foreach (var entity in GetEntities().OrderBy(i => Me.Distance(i)))
                {
                    if (entity.Id != mobId)
                    {
                        continue;
                    }

                    if (entity.Type == EBotTypes.Unit && ((Unit)entity).IsSpellClick ||
                        entity.Type == EBotTypes.Vehicle && ((Vehicle)entity).IsSpellClick)
                    {
                        unit = entity;
                        break;
                    }
                }

                if (unit != null)
                {
                    if (unit.Type == EBotTypes.Unit && (unit as Unit).IsSpellClick ||
                        unit.Type == EBotTypes.Vehicle && ((Vehicle)unit).IsSpellClick)
                    {
                        if (mobId == 136434)
                        {
                            CommonModule.MoveTo(unit, 4);
                        }
                        else
                        {
                            CommonModule.MoveTo(unit, 2);
                        }

                        if (Me.Distance(unit) > 6)
                        {
                            return false;
                        }

                        CommonModule.MyUnmount();
                        MyCheckIsMovingIsCasting();
                        if (SpellManager.UseSpellClick(unit as Unit))
                        {
                            log("Использовал SpellClick " + mobId, LogLvl.Ok);
                            MyCheckIsMovingIsCasting();
                            while (SpellManager.IsChanneling)
                            {
                                Thread.Sleep(50);
                            }
                        }
                        else
                        {
                            log("Не смог использовать SpellClick " + GetLastError(), LogLvl.Error);
                            CanselForm();
                        }
                    }
                    else
                    {
                        log("Тип Entity не известен " + unit.Type);
                    }


                }
                else
                {
                    log("Не нашел НПС для использования скила ", LogLvl.Error);
                    if (mobId == 122683)
                    {
                        CommonModule.MoveTo(862.99, 3107.12, 123.84);
                    }

                    if (mobId == 129086)
                    {
                        CommonModule.MoveTo(2473.83, 1202.95, 7.28);
                    }
                }

                Thread.Sleep(1000);
                return false;
            }

            catch (Exception e)
            {
                log("" + e);
                return false;
            }
        }

        public bool MyUseSpellClick(Entity entity)
        {
            try
            {
                if (entity != null)
                {
                    if (entity.Type == EBotTypes.Unit && ((Unit)entity).IsSpellClick ||
                        entity.Type == EBotTypes.Vehicle && ((Vehicle)entity).IsSpellClick)
                    {
                        if (!CommonModule.MoveTo(entity, 2))
                        {
                            return false;
                        }

                        CommonModule.MyUnmount();
                        MyCheckIsMovingIsCasting();

                        if (SpellManager.UseSpellClick(entity as Unit))
                        {
                            log("Использовал SpellClick ", LogLvl.Ok);
                            MyCheckIsMovingIsCasting();
                            while (SpellManager.IsChanneling)
                            {
                                Thread.Sleep(50);
                            }

                            FarmModule.SetBadTarget(entity, 60000);
                            // return true;
                        }
                        else
                        {
                            log("Не смог использовать SpellClick " + GetLastError(), LogLvl.Error);
                            CanselForm();
                            FarmModule.SetBadTarget(entity, 60000);
                        }
                    }
                    else
                    {
                        log("Тип Entity не известен " + entity.Type);
                    }
                }
                else
                {
                    log("Не нашел НПС для использования скила ", LogLvl.Error);
                }
                Thread.Sleep(1000);
                return false;
            }
            catch (Exception e)
            {
                log("" + e);
                return false;
            }
        }


        public int TrainLevel;
        public bool TryLearnSpell()
        {
            try
            {
                if (!CharacterSettings.LearnAllSpell)
                {
                    return true;
                }

                if (TrainLevel == Me.Level)
                {
                    return true;
                }

                if (Me.Money < 100)
                {
                    return true;
                }

                Unit trainer = null;
                foreach (var entity in GetEntities<Unit>())
                {
                    if (!entity.IsTrainer)
                    {
                        continue;
                    }

                    if (!entity.SubName.Contains(Me.Class.ToString()))
                    {
                        continue;
                    }
                    //log(entity.Name + " " + entity.SubName);
                    trainer = entity;
                    break;
                }

                if (trainer == null)
                {
                    return true;
                }

                if (!CommonModule.MoveTo(trainer, 1))
                {
                    return false;
                }

                if (!MyOpenDialog(trainer))
                {
                    return false;
                }

                Thread.Sleep(1000);
                foreach (var d in GetNpcDialogs())
                {
                    if (d.OptionNPC != EGossipOptionIcon.Trainer)
                    {
                        continue;
                    }

                    SelectNpcDialog(d);
                    break;
                }
                Thread.Sleep(1000);

                var trainerSpells = GetTrainerSpells();
                //log("Всего скилов " + trainerSpells.Count);

                foreach (var trainerSpell in trainerSpells)
                {
                    if (!GameDB.SpellInfoEntries.ContainsKey(trainerSpell.Id))
                    {
                        continue;
                    }

                    if (!trainerSpell.CanLearn())
                    {
                        continue;
                    }

                    if (Me.Money < trainerSpell.MoneyCost)
                    {
                        continue;
                    }

                    var dbSpell = GameDB.SpellInfoEntries[trainerSpell.Id];
                    foreach (var dbSpellEffect in dbSpell.Effects)
                    {
                        foreach (var spellEffectInfo in dbSpellEffect.Value)
                        {
                            if (spellEffectInfo.Effect == ESpellEffectName.LEARN_SPELL)
                            {
                                //  log(spellEffectInfo.Effect + " " + spellEffectInfo.TriggerSpell);
                                if (SpellManager.GetSpell(spellEffectInfo.TriggerSpell) != null)
                                {
                                    break;
                                }

                                uint bestLevel = trainerSpell.ReqLevel;
                                foreach (var spell in SpellManager.GetSpells())
                                {
                                    if (spell.Name == trainerSpell.Spell.Name && GameDB.SpellInfoEntries.ContainsKey(spell.Id))
                                    {
                                        var spellInfo = GameDB.SpellInfoEntries[spell.Id];
                                        if (spellInfo.SpellLevel > bestLevel)
                                        {
                                            bestLevel = spellInfo.SpellLevel;
                                        }
                                    }
                                }
                                if (bestLevel > trainerSpell.ReqLevel)
                                {
                                    break;
                                }

                                log("Учу " + trainerSpell.Spell.Name + " " + " " + trainerSpell.ReqLevel);
                                /* foreach (var reqAbility in trainerSpell.GetReqAbilities())
                                 {
                                     log("reqAb" + reqAbility);
                                 }*/
                                if (!LearnTrainerSpell(trainerSpell))
                                {
                                    log("Не смог выучить " + trainerSpell.Spell.Name + " " + GetLastError(), LogLvl.Error);
                                }

                                Thread.Sleep(2500);

                            }
                        }
                    }
                }
                // log("Выучил");
                TrainLevel = Me.Level;
                return true;
            }
            catch (Exception e)
            {
                log(e + "");
                return true;
            }
        }


        public MyGameObjectLoc MyGetLocGameOjectById(uint id)
        {
            foreach (var myNpcLoc in MyGameObjectLocss.GameObjectLocs)
            {
                if (myNpcLoc.Id == id)
                {
                    return myNpcLoc;
                }
            }
            return null;
        }
        private int _fixBadDialog;


        private (int x, int y) ComputeGridCoord(float x, float y)
        {
            return ((int)(32.0f - (x / 533.3333f)), (int)(32.0f - (y / 533.3333f)));
        }

        private Vector3F GetHitSpherePointFor(Entity a, Vector3F v)
        {
            var t = (v - a.Location);
            var mag = t.Len();
            if (mag < 0.0000001f)
            {
                t = Vector3F.Zero;
            }
            else if (mag < 1.00001f && mag > 0.99999f)
            { }
            else
            {
                t = t * (1.0f / mag);
            }

            t = new Vector3F(a.Location + t * a.ObjectSize);
            return new Vector3F(t.X, t.Y, t.Z + 1.7);
        }

        public bool Raycast(Player a, Entity b)
        {
            var state = GetFarmState();
            if (a.Distance(b) < 0.1 || state.CurrentLoadedGeo.x == -1 || state.CurrentLoadedGeo.y == -1)
            {
                return false;
            }

            Vector3F hitPos = Vector3F.Zero;
            Triangle tri = new Triangle();
            var src = new Vector3F(a.Location.X, a.Location.Y, a.Location.Z + 1.7);
            var dst = b.Type == EBotTypes.Player ? new Vector3F(b.Location.X, b.Location.Y, b.Location.Z + 1.7) : GetHitSpherePointFor(b, a.Location);
            return state.GeoNav.RaycastNative(src, dst, ref hitPos, ref tri);
        }

        /*  public bool Raycast(Vector3F a, Vector3F b)
          {
              var state = GetFarmState();
              if (a.Distance(b) < 0.1 || state.CurrentLoadedGeo.x == -1 || state.CurrentLoadedGeo.y == -1)
              {
                  return false;
              }

              Vector3F hitPos = Vector3F.Zero;
              Triangle tri = new Triangle();
              var src = new Vector3F(a.X, a.Y, a.Z + 1.7);
              var dst = new Vector3F(b.X, b.Y, b.Z + 1.7);
              return state.GeoNav.RaycastNative(src, dst, ref hitPos, ref tri);
          }*/

        public double CalcPathDistance(Vector3F from, Vector3F to)
        {
            var pathToMob = GetSmoothPath2(from, to, new Vector3F(0.01, 2, 0.01), 0.5f, 0.1f, CommonModule.TryGetMidPoints);
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
        }

        public double CalcPathDistanceServer(Vector3F from, Vector3F to)
        {
            var pathToMob = GetServerPath(from, to);
            if (pathToMob == null || pathToMob.Path == null)
            {
                log("Нет маршрута");
                return double.MaxValue;
            }
            double pathLen = 0;
            for (var i = 1; i < pathToMob.Path.Count; i++)
            {
                var d = pathToMob.Path[i - 1].Distance(pathToMob.Path[i]);
                if (d > 250)
                {

                    log("Точка слишком далеко " + d);
                    return double.MaxValue;
                }
                pathLen += d;
            }
            return pathLen;
        }

        private readonly Stopwatch _calcSw = new Stopwatch();


        public Vector3F CalcNoLosPoint(Vector3F dest, double maxRange)
        {
            var best = Vector3F.Zero;

            try
            {
                if (maxRange > 30)
                {
                    maxRange = 30;
                }

                if (maxRange < 5)
                {
                    maxRange = 5;
                }

                _calcSw.Restart();

                var mobsAll = new Dictionary<WowGuid, Unit>();
                var aggroRadiuses = new Dictionary<WowGuid, double>();
                foreach (var mob in GetEntities<Unit>())
                {
                    if (mob.IsAlive /*&& MonstersData.ContainsKey(mob.Id) /*&& !AggroAll.ContainsKey(mob.Guid)*/)
                    {
                        mobsAll[mob.Guid] = mob;
                        aggroRadiuses[mob.Guid] = GetAggroRadius(mob);
                    }
                }

                double bestDist = double.MaxValue;
                double bestDist2D = double.MaxValue;



                for (double x = 1; x < maxRange; x += 2)
                {
                    for (int x1 = -1; x1 <= 1; x1 += 2)
                    {
                        for (double y = 1; y < maxRange; y += 2)
                        {
                            for (int y1 = -1; y1 <= 1; y1 += 2)
                            {
                                var testPoint2D = new Vector3F(dest.X + (x * x1), dest.Y + (y * y1), dest.Z);
                                if (Me.Distance2D(testPoint2D) > bestDist2D)
                                {
                                    continue;
                                }

                                if (Me.Distance2D(testPoint2D) < 1)
                                {
                                    continue;
                                }

                                var testPoint = new Vector3F(testPoint2D.X, testPoint2D.Y, GetNavMeshHeight(new Vector3F(testPoint2D.X, testPoint2D.Y, dest.Z)));
                                if (Math.Abs(testPoint.Z - dest.Z) > 8)
                                {
                                    testPoint = new Vector3F(testPoint2D.X, testPoint2D.Y, dest.Z /*GetNavMeshHeight(new Vector3F(testPoint2D.X, testPoint2D.Y, dest.Z))*/);
                                }

                                // ReSharper disable once CompareOfFloatsByEqualityOperator
                                if (testPoint.Z == 0 || Math.Abs(testPoint.Z - dest.Z) > 8)
                                {
                                    continue;
                                }
                                Vector3F hitPos = Vector3F.Zero;
                                Triangle tri = new Triangle();
                                if (Raycast(testPoint, dest, ref hitPos, ref tri))
                                {
                                    continue;
                                }



                                var pathLen = CalcPathDistance(testPoint, dest);
                                if (pathLen > maxRange)
                                {
                                    continue;
                                }

                                bool isSafe = true;
                                foreach (var m in mobsAll.Values)
                                {
                                    var aggroRadius = aggroRadiuses[m.Guid];
                                    if (testPoint.Distance(m.Location) < aggroRadius + 5)
                                    {
                                        var pathDist = CalcPathDistance(testPoint, m.Location);
                                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                                        if (pathDist == double.MaxValue && Math.Abs(testPoint.Z - m.Location.Z) < 3) //подобрать правильное
                                        {
                                            pathDist = testPoint.Distance(m.Location);
                                        }

                                        if (pathDist < aggroRadius + 5)
                                        {
                                            isSafe = false;
                                            break;
                                        }
                                    }
                                }

                                /*   foreach (var m in AggroAll.Values)
                                   {
                                       if (!MonstersData.ContainsKey(m.Id))
                                           continue;
                                       if (MonstersData[m.Id].StayAwayDistance > 0 && testPoint.Distance(m.Location) < MonstersData[m.Id].StayAwayDistance)
                                       {
                                           isSafe = false;
                                           break;
                                       }
                                   }*/
                                if (isSafe && Me.Distance(testPoint) < bestDist)
                                {
                                    bestDist = Me.Distance(testPoint);
                                    bestDist2D = Me.Distance2D(testPoint);
                                    best = testPoint;
                                }
                            }
                        }
                    }
                }
                return best;
            }
            finally
            {
                _calcSw.Stop();
                log("Нашел точку за: " + _calcSw.ElapsedMilliseconds + ": " + best + " dist " + maxRange);
            }

        }

        private class MyState
        {
            public NavManager GeoNav = new NavManager(NavManager.EProject.WOW);
            public (int x, int y) CurrentLoadedGeo;
        }


        private readonly MyState _state = new MyState();

        private MyState GetFarmState()
        {
            var coords = ComputeGridCoord(Me.Location.X, Me.Location.Y);
            if (_state.CurrentLoadedGeo.x != coords.x || _state.CurrentLoadedGeo.y != coords.y)
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "GeoClassic\\" + MapID.ToString("D4") + coords.x.ToString("D2") + coords.y.ToString("D2") + ".mmtile";
                if (File.Exists(path))
                {
                    if (_state.GeoNav.LoadMMTileGeometryNative(path))
                    {
                        // log("Loaded geo: " + path);
                        _state.CurrentLoadedGeo = coords;
                    }
                    else
                    {
                        _state.CurrentLoadedGeo = (-1, -1);
                    }
                }
                else
                {
                    _state.CurrentLoadedGeo = (-1, -1);
                }
            }

            return _state;
            // return null;
        }

        public double GetAggroRadius(Unit e)
        {
            if (e == null)
            {
                return 10;
            }
            // const float CREATURE_FAMILY_ASSISTANCE_RADIUS = 10f;
            const uint maxLevel = 60;
            const float maxAggroRadius = 45f;
            const float minAggroRadius = 5f;
            var baseAggroDistance = 20f - e.CombatReach;
            var aggroRadius = baseAggroDistance;
            uint playerLevel = Me.Level;
            uint creatureLevel = e.Level;
            if ((creatureLevel + 5) <= maxLevel)
            {
                aggroRadius += e.GetTotalAuraModifier(EAuraType.MOD_DETECT_RANGE);
                aggroRadius += Me.GetTotalAuraModifier(EAuraType.MOD_DETECTED_RANGE);
            }
            var diff = ((float)creatureLevel - playerLevel);
            aggroRadius += diff;
            if (FarmModule?.MobsWithSkinCount() + FarmModule?.MobsWithDropCount() == 0)
            {
                aggroRadius += 3f; //сейф
            }

            aggroRadius = Math.Min(maxAggroRadius, Math.Max(minAggroRadius, aggroRadius));
            return aggroRadius;
        }


    }

}

