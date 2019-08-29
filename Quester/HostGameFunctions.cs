using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WowAI.Modules;
using Out.Internal.Core;
using WoWBot.Core;
using System.Linq;
using Out.Utility;
using WowAI.UI;

namespace WowAI
{
    static class TimeSpanExtensions
    {
        static public bool IsBetween(this TimeSpan time, TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime == startTime)
                return true;
            if (endTime < startTime)
                return time <= endTime || time >= startTime;
            return time >= startTime && time <= endTime;
        }
    }

    internal partial class Host
    {
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
                    return;
                if (Me.IsMoving)
                {
                    fixMove++;
                    log(SpellManager.IsCasting + "    " + Me.IsMoving);
                }

                Thread.Sleep(100);
                if (!Me.IsAlive)
                    return;
                if (!MainForm.On)
                    return;

                if (fixMove > 50)
                {
                    SetMoveStateForClient(true);
                    if (RandGenerator.Next(0, 2) == 0)
                        StrafeLeft(true);
                    else
                        StrafeRight(true);
                    Thread.Sleep(2000);
                    StrafeRight(false);
                    StrafeLeft(false);
                    SetMoveStateForClient(false);
                    CancelMoveTo();
                    fixMove = 0;
                }
            }
        }

        public void Wait(int time)
        {
            var waitTime = time;
            while (waitTime > 0)
            {
                if (!MainForm.On)
                    return;
                Thread.Sleep(1000);
                waitTime = waitTime - 1000;
                log("Ожидаю " + waitTime + "/" + time);
            }
        }



        public bool MyDialog(Entity npc)
        {
            var result = false;
            CommonModule.MoveTo(npc, 3);
            Thread.Sleep(1000);
            if (!OpenDialog(npc))
            {
                log("Не смог открыть диалог " + GetLastError(), LogLvl.Error);
            }

            if (GetNpcDialogs().Count == 0)
            {
                SendKeyPress(0x1b);
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                {
                    log("Не смог открыть диалог 2 " + GetLastError(), LogLvl.Error);
                    return false;
                }

            }

            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                log(" " + gossipOptionsData.OptionNPC + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption);
                if (gossipOptionsData.OptionNPC == EGossipOptionIcon.Chat)
                {
                    log("Выбираю диалог");
                    if (!SelectNpcDialog(gossipOptionsData))
                        log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                    result = true;
                    break;
                }

            }

            return result;

        }

        public bool MyDialog(Entity npc, string text)
        {

            if (GetBotLogin() == "Daredevi1")
            {
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   " + text);
                }
                log("Надо убрать этот код");
                return false;
            }

            var result = false;
            ForceMoveTo(npc.Location);
            Thread.Sleep(1000);
            if (!OpenDialog(npc))
                log("Не смог открыть диалог " + GetLastError(), LogLvl.Error);

            if (GetNpcDialogs().Count == 0)
            {
                SendKeyPress(0x1b);
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                    log("Не смог открыть диалог 2 " + GetLastError(), LogLvl.Error);
            }



            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                if (gossipOptionsData.Text.Contains("Подгород") || gossipOptionsData.Text.Contains(text))
                {
                    if (!SelectNpcDialog(gossipOptionsData))
                        log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                    result = true;
                }
                log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   " + text);
            }
            return result;
        }



        public bool MyDialog(Entity npc, int index)
        {
            var result = false;
            ForceMoveTo(npc.Location);
            Thread.Sleep(1000);
            if (!OpenDialog(npc))
                log("Не смог открыть диалог " + GetLastError(), LogLvl.Error);

            if (GetNpcDialogs().Count == 0)
            {
                SendKeyPress(0x1b);
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                    log("Не смог открыть диалог 2 " + GetLastError(), LogLvl.Error);
            }

            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                if (gossipOptionsData.ClientOption == index)
                {
                    log("Выбираю диалог");
                    if (!SelectNpcDialog(gossipOptionsData))
                        log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                    result = true;
                }
                log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
            }

            if (npc.Id == 127128 || npc.Id == 130905 || npc.Id == 130929 || npc.Id == 131135 || npc.Id == 137613)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == index)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
            }

            if (npc.Id == 130905 || npc.Id == 130929)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == index)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
            }

            if (npc.Id == 281536)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 4)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 0)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 3)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 1)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
            }
            return result;
        }


        public void FlyForm()
        {
            CancelMoveTo();
            Thread.Sleep(1000);
            if (Me.Class == EClass.Druid)
            {
                if (MyGetAura(783) == null)
                    CanselForm();
                while (MyGetAura(783) == null)
                {
                    if (!MainForm.On)
                        return;
                    foreach (var spell in SpellManager.GetSpells())
                    {
                        if (spell.Id == 783)
                        {
                            var resultForm = SpellManager.CastSpell(spell.Id);
                            if (resultForm != ESpellCastError.SUCCESS)
                            {
                                if (AdvancedLog)
                                    log("Не удалось поменять форму " + spell.Name + "  " + resultForm, LogLvl.Error);
                                if (resultForm == ESpellCastError.NOT_MOUNTED)
                                    CommonModule.MyUnmount();
                            }

                            while (SpellManager.IsCasting)
                                Thread.Sleep(100);
                            Thread.Sleep(2000);
                        }
                    }
                }
            }

            while (Me.Class != EClass.Druid)
            {
                Thread.Sleep(1000);
                if (Me.MountId != 0)
                    return;
                Spell mountSpell = null;
                foreach (var s in SpellManager.GetSpells())
                {
                    if (!s.SkillLines.Contains(777))
                        continue;
                    if (s.Id != 32297)
                        continue;
                    mountSpell = s;
                    break;
                }
                if (mountSpell == null)
                    continue;

                CancelMoveTo();
                Thread.Sleep(500);
                MyCheckIsMovingIsCasting();
                var result = SpellManager.CastSpell(mountSpell.Id);

                if (result != ESpellCastError.SUCCESS)
                    log("Не удалось призвать маунта " + mountSpell.Name + "  " + result, LogLvl.Error);
                else
                {
                    log("Призвал маунта", LogLvl.Ok);
                    while (SpellManager.IsCasting)
                        Thread.Sleep(100);
                    return;
                }
            }
            Jump();
            Thread.Sleep(1000);
            Jump();
        }


        public void Mail()
        {
            var path = CommonModule.GpsBase.GetPath(new Vector3F(1610.48, -4419.00, 14.14), Me.Location);

            if (Me.Team == ETeam.Horde)
            {
                if (CharacterSettings.AlternateAuk)
                {
                    path = CommonModule.GpsBase.GetPath(new Vector3F(2029.39, -4683.23, 28.16), Me.Location);
                }
                log(path.Count + "  Путь");
                foreach (var vector3F in path)
                {
                    log(path.Count + "  Путь " + Me.Distance(vector3F));
                    CommonModule.ForceMoveTo2(vector3F, 1, false);
                }
            }
            if (Me.Team == ETeam.Alliance)
            {
                path = CommonModule.GpsBase.GetPath(new Vector3F(-8860.24, 638.56, 96.35), Me.Location);
                foreach (var vector3F in path)
                {
                    log(path.Count + "  Путь " + Me.Distance(vector3F));
                    CommonModule.ForceMoveTo2(vector3F, 1, false);
                }
            }


            GameObject mailBox = null;
            foreach (var gameObject in GetEntities<GameObject>())
            {
                if (gameObject.Id == 206726 && gameObject.Distance(1607, -4424, 13) < 10)
                    mailBox = gameObject;

                if (gameObject.Id == 197135 && gameObject.Distance(-8860.24, 638.56, 96.35) < 10)
                    mailBox = gameObject;
            }

            if (mailBox != null)
            {
                ForceComeTo(mailBox, 2);
                MyCheckIsMovingIsCasting();
                Thread.Sleep(1000);
                if (!OpenMailbox(mailBox))
                    log("Не удалось открыть ящик " + GetLastError(), LogLvl.Error);
                else
                    log("Открыл ящик", LogLvl.Ok);
                Thread.Sleep(2000);

                foreach (var mail in GetMails())
                {
                    if (mail.Cod != 0)
                        continue;
                    log(mail.SenderType + " " + mail.GetAttachedItems().Count + " " + mail.Subject + " ");
                    mail.MarkAsRead();
                    Thread.Sleep(500);
                    if (GetBotLogin() == "deathstar")
                        Thread.Sleep(500);
                    if (!mail.TakeAllAttachmentsAndGold())
                        log("Не удалось получить письмо " + GetLastError(), LogLvl.Error);
                    else
                        log("Получил письмо", LogLvl.Ok);

                    if (mail.Subject == "Auction won: WoW Token")
                        if (!mail.Delete())
                            log("Не удалось удалить письмо " + GetLastError(), LogLvl.Error);

                }
            }
            //   SendKeyPress(0x1b);
        }



        public void Auk()
        {
            var path = CommonModule.GpsBase.GetPath(new Vector3F(1635, -4445, 17), Me.Location);
            if (Me.Team == ETeam.Horde)
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
            Unit npc = null;
            foreach (var entity in GetEntities<Unit>())
            {
                if (!entity.IsAuctioner)
                    continue;
                if (entity.Id == 44868)
                    continue;
                if (entity.Id == 44865)
                    continue;
                if (entity.Id == 44866)
                    npc = entity;
                if (entity.Id == 8719)
                    npc = entity;
                if (entity.Id == 46640)
                    npc = entity;
            }

            if (npc == null)
            {
                log("Нет НПС для аука", LogLvl.Error);
                Thread.Sleep(5000);
                return;
            }
            log("Выбран " + npc.Name + " " + npc.Id);
            CommonModule.MoveTo(npc, 3);
            MyCheckIsMovingIsCasting();
            if (!OpenAuction(npc))
                log("Не смог открыть диалог для аука " + GetLastError(), LogLvl.Error);
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
            Thread.Sleep(2000);
            while (GetAgroCreatures().Count > 0)
            {
                if (!MainForm.On)
                    return false;
                Thread.Sleep(1000);
            }

            if (auk)
                if (MapID == 0 || MapID == 1)
                    return true;
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
                                    return false;
                            }
                            finally
                            {
                                NeedAuk = false;
                            }
                        }
                    }
                    FarmModule.farmState = FarmState.AttackOnlyAgro;
                    if (GetAgroCreatures().Count != 0)
                        return false;
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
                        Thread.Sleep(50);
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

        public bool MyUseStone2(bool auk = false)
        {
            Thread.Sleep(2000);
            while (GetAgroCreatures().Count > 0)
            {
                if (!MainForm.On)
                    return false;
                Thread.Sleep(1000);
            }

            foreach (var item in ItemManager.GetItems())
            {
                if (item.Id == 141605)
                {
                    FarmModule.farmState = FarmState.AttackOnlyAgro;
                    while (SpellManager.GetItemCooldown(item) != 0)
                    {
                        if (!MainForm.On)
                            return false;
                        Thread.Sleep(5000);
                        log("Свисток в КД " + SpellManager.GetItemCooldown(item));
                        //  break;
                    }


                    FarmModule.farmState = FarmState.AttackOnlyAgro;
                    if (GetAgroCreatures().Count != 0)
                        return false;
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
                        Thread.Sleep(50);
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
                    i.Cancel();
                if (i.SpellId == 768)//Облик кошки
                    i.Cancel();
                if (i.SpellId == 24858)//Облик лунного совуха    
                    i.Cancel();
                if (i.SpellId == 783)//Походный облик
                    i.Cancel();
            }
        }

        public long GetUnixTime()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public double Distance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2) + Math.Pow((z1 - z2), 2));
        }
        public double MyDistance(Vector3F loc1, Vector3F loc2)
        {
            return Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) + Math.Pow((loc1.Z - loc2.Z), 2));
        }
        public double DistanceGpcPoint(GpsPoint loc1, GpsPoint loc2)
        {
            return Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) + Math.Pow((loc1.Z - loc2.Z), 2));
        }

        public double DistanceVectorGpsPoint(Vector3F loc1, GpsPoint loc2)
        {
            return Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) + Math.Pow((loc1.Z - loc2.Z), 2));
        }

        public double DistanceNoZ(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }


        public void FarmSpellClick(List<uint> farmMobIds)
        {
            Thread.Sleep(100);
            if (!MainForm.On)
                return;
            if (GetAgroCreatures().Count > 0)
                return;



            var entitylist = GetEntities();
            Entity needEntity = null;
            foreach (var entity in entitylist.OrderBy(i => Me.Distance(i)))
            {
                if (!farmMobIds.Contains(entity.Id))
                    continue;
                if (Me.Distance(entity) < 10)
                    continue;
                if (FarmModule.IsBadTarget(entity, ComboRoute.TickTime))
                    continue;
                needEntity = entity;
                break;
            }

            if (needEntity != null)
            {
                AutoQuests.MyUseSpellClick(needEntity);
            }

        }



        internal Entity GetNpcById(uint id, bool checkBad = true)
        {
            try
            {
                var listEntity = GetEntities();
                foreach (var npc in listEntity.OrderBy((i => Me.Distance(i))))
                {
                    if (npc.Distance2D(new Vector3F(-2054.99, 753.27, 7.13)) < 10 && id == 126034)
                        continue;
                    if (npc.Distance(2475.74, 1130.51, 5.97) < 10)
                        continue;
                    if (!checkBad)
                        if (FarmModule.IsBadTarget(npc, ComboRoute.TickTime))
                            continue;
                    if (npc.Id == 138449 && npc.Distance(3899.11, 405.34, 148.84) < 5)
                        continue;
                    if (npc.Id == id)
                        return npc;
                    if (npc.Guid.GetEntry() == id)
                        return npc;
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return null;
        }


        internal Item MyGetItem(uint id)
        {
            foreach (var item in ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 || item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 || item.Place == EItemPlace.InventoryItem)
                    if (item.Id == id)
                        return item;
            }
            return null;
        }

        internal bool MyUseTaxi(uint areaId, Vector3F loc)
        {
            try
            {
                if (Me.GetThreats().Count > 0)
                {
                    FarmModule.farmState = FarmState.AttackOnlyAgro;
                    return false;
                }

                var needArea = GetAreaById(areaId);

                if (needArea == null)
                {
                    log("Не нашел зону с айди " + areaId, LogLvl.Error);
                    Thread.Sleep(10000);
                    return false;
                }
                log("Нужно в зону " + areaId + "    " + needArea.AreaName + "  " + Me.Distance(loc), LogLvl.Important);

                double bestDist = 9999999;
                TaxiNode bestNode = null;

                foreach (var i in GetallNodesOnMyMap())
                {
                    if (Me.Distance(i.Location) < bestDist)
                    {
                        if (i.Id == 1839)//Настрондир  1220  0  81.2824630737305  1839
                            continue;
                        if (i.Id == 2161)//Настрондир  1220  0  81.2824630737305  1839
                            continue;
                        if (i.Id == 2078)//Настрондир  1220  0  81.2824630737305  1839
                            continue;

                        if (i.Id == 2073 && CharacterSettings.Mode != EMode.Questing)//Throne Room, Zuldazar  1642  0  37.4401321411133  2073
                            continue;

                        if (i.Id == 2116 && CharacterSettings.Mode != EMode.Questing)//Disabled   Quest Path 6698: Horde Embassy, Zuldazar -> Throne Room, Zuldazar  1642  0  270.530822753906  2116
                            continue;
                        if (i.Id == 2015 && CharacterSettings.Mode != EMode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
                            continue;
                        if (i.Id == 1961 && CharacterSettings.Mode != EMode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
                            continue;

                        if (i.Id == 2080 && CharacterSettings.Mode != EMode.Questing && Me.Team == ETeam.Horde)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2062 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2273 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2114)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2144 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2112 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 1642 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2153 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2147 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2145 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2157 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2148 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2129 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2012 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 1962 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2110 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;
                        if (i.Id == 2112 && CharacterSettings.Mode != EMode.Questing)//Grimwatt's Crash, Nazmir  1642  0  245.250076293945  2080
                            continue;

                        if (i.Id == 2274 && CharacterSettings.Mode != EMode.Questing)
                            continue;

                        if (i.Id == 2275 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2091 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2107 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2135 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2127 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2108 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2057 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2056 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2033 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2093 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2090 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2156 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2105 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2054 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2282 && CharacterSettings.Mode != EMode.Questing)
                            continue;
                        if (i.Id == 2144)
                            continue;
                        if (i.Id == 2110)
                            continue;
                        /* if (i.Id == 1642 && CharacterSettings.Mode != EMode.Questing)
                             continue;*/


                        log(i.Name + "  " + i.MapId + "  " + i.Cost + "  " + Me.Distance(i.Location) + "  " + i.Id + " ");
                        bestNode = i;
                        bestDist = Me.Distance(i.Location);
                    }
                }

                if (bestNode == null)
                {
                    log("Не нашел ближайшее такси");
                    Thread.Sleep(10000);
                    return false;
                }
                if (Me.Distance(bestNode.Location) > 10)
                    if (!CommonModule.MoveTo(bestNode.Location))
                        return false;

                Unit taxinpc = null;
                foreach (var npc in GetEntities<Unit>())
                {
                    if (!npc.IsTaxi)
                        continue;
                    taxinpc = npc;
                    break;
                }
                if (taxinpc == null)
                {
                    log("Не нашел НПС");
                    Thread.Sleep(10000);
                    return false;
                }
                if (!ComeTo(taxinpc, 1))
                    return false;


                CommonModule.MyUnmount();
                CanselForm();
                MyCheckIsMovingIsCasting();
                Thread.Sleep(1000);
                if (!OpenTaxi(taxinpc))
                {
                    log("Не смог использовать такси " + taxinpc.Name + "  " + GetLastError(), LogLvl.Error);
                    Thread.Sleep(10000);
                    if (GetLastError() != ELastError.ActionNotAllowed)
                        return false;
                }

                TaxiNode node = null;
                double bestDistnode = 99999999;
                foreach (var canLandNode in TaxiNodesData.CanLandNodes)
                {
                    if (canLandNode.Id == 1965 && CharacterSettings.Mode != EMode.Questing)
                        continue;
                    if (Distance(loc.X, loc.Y, loc.Z, canLandNode.Location.X, canLandNode.Location.Y, canLandNode.Location.Z) < bestDistnode)
                    {
                        bestDistnode = Distance(loc.X, loc.Y, loc.Z, canLandNode.Location.X, canLandNode.Location.Y, canLandNode.Location.Z);
                        node = canLandNode;
                    }
                }



                if (node != null)
                {
                    log("Выбрал точку " + node.Name + " " + node.Id + "  " + node.MapId, LogLvl.Ok);
                    Thread.Sleep(2000);
                    log(node.Id + "  " + node.Name + " " + node.MapId + "  " + node.Cost + "   " + node.Location);
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

                    Thread.Sleep(2000);
                }
                else
                {
                    log("Не найдено место назначения ", LogLvl.Error);
                }



                while (Me.IsInFlight)
                {
                    Thread.Sleep(1000);
                }

                Thread.Sleep(5000);
                return true;
            }
            catch (Exception e)
            {
                log(e + "");
                return false;
            }
        }



        public NpcForAction FindNpcForActionVendor()
        {
            double bestDist = 999999;
            NpcForAction bestNpc = null;
            try
            {
                foreach (var npc in CharacterSettings.NpcForActionSettings)
                {
                    if (npc.MapId != MapID)
                        continue;
                    if (npc.AreaId != Area.Id)
                        continue;
                    if (!npc.IsVendor)
                        continue;
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
                            continue;
                        if (!npc.IsVendor)
                            continue;
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

        public int MeGetItemsCount(uint id)
        {
            var count = 0;
            foreach (var item in ItemManager.GetItems())
                if (item.Id == id)
                    count = count + item.Count;

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
                        continue;
                    if (npc.AreaId != Area.Id)
                        continue;
                    if (!npc.IsArmorer)
                        continue;
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
                            continue;
                        if (!npc.IsArmorer)
                            continue;
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


        ulong RoundDown(ulong toRound)
        {
            return toRound - toRound % 1000000;
        }
        ulong RoundDown2(ulong toRound)
        {
            return toRound - toRound % 100;
        }

        bool isRuLang = false;
        public void SellAll()
        {
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
            var itemIDsForSell = new List<uint>();
            foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
            {
                if (itemIDsForSell.Contains(Convert.ToUInt32(characterSettingsAukSettingse.Id)))
                    continue;
                if (characterSettingsAukSettingse.MaxCount != 0)
                {
                    var i = 0;
                    foreach (var myAuctionItem in GetMyAuctionItems())
                    {
                        if (myAuctionItem.ItemId == characterSettingsAukSettingse.Id)
                            i = i + myAuctionItem.Count;
                    }

                    if (i > characterSettingsAukSettingse.MaxCount)
                        continue;
                }

                itemIDsForSell.Add(Convert.ToUInt32(characterSettingsAukSettingse.Id));
            }

            if (itemIDsForSell.Count == 0)
                return;
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
                                needItem = true;
                        }
                    }
                    if (!needItem)
                        continue;
                }
                if (item.Place >= EItemPlace.InventoryBag && item.Place <= EItemPlace.Bag4)
                {
                    if (itemIDsForSell.Contains(item.Id))
                    {
                        if (!items.ContainsKey(item.Id))
                            items[item.Id] = new List<Item>();
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
                    continue;
                var totalCount = 0;
                foreach (var item in items[id])
                    totalCount += item.Count;
                var firstItem = items[id][0];
                var quality = firstItem.ItemQuality;
                var name = firstItem.Name;
                if (isRuLang)
                    name = firstItem.NameRu;
                log("Проверяем предмет: " + name + "[" + firstItem.Id + "]. Суммарное количество в инвентаре: " + totalCount + ", размер стака: " + firstItem.MaxStackCount);
                log("Минимально ищем: " + minimumCheckingCount[firstItem.ItemQuality]);
                var req = new AuctionSearchRequest
                {
                    MaxReturnItems = 50,
                    SearchText = name,
                    ExactMatch = true
                };

                ulong priceSumm = 0;
                var itemsCount = 0;



                var normalPrices = new Dictionary<ulong, int>();
                ulong mylotPrice = 0;
                while (itemsCount < minimumCheckingCount[firstItem.ItemQuality])
                {
                    if (!MainForm.On)
                        return;
                    var aucItems = GetAuctionBuyList(req);
                    if (aucItems == null || aucItems.Count == 0)
                        break;
                    foreach (var aucItem in aucItems)
                    {
                        if (aucItem.BuyoutPrice == 0)
                            continue;
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
                            if (aucItem.Count < 50)
                                continue;

                            var priceforone = aucItem.BuyoutPrice / (uint)aucItem.Count;
                            if (aucItem.Owner == Me.Guid)
                                mylotPrice = priceforone;

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
                        priceSumm += aucItem.BuyoutPrice;
                    }
                    req.Page++;
                }




                if (itemsCount >= minimumCheckingCount[quality])
                {

                    var averagePrice = priceSumm / (uint)itemsCount;
                    if (quality == EItemQuality.Epic)
                    {
                        if (epicPrices.ContainsKey(new KeyValuePair<uint, uint>(firstItem.Id, firstItem.Level)))
                        {
                            foreach (var @ulong in epicPrices)
                            {
                                if (@ulong.Key.Key == firstItem.Id)
                                    log("Средняя цена для " + itemsCount + " " + name + "{" + @ulong.Key + "}[" + firstItem.Id + "] = " + (@ulong.Value / 10000f).ToString("F2"), LogLvl.Important);

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
                            if (normalPrice.Value < 399)
                                continue;
                            averages[firstItem.Id] = normalPrice.Key - 1;
                            if (mylotPrice == normalPrice.Key)
                                averages[firstItem.Id] = normalPrice.Key;
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
                        if (!averages.ContainsKey(item.Id))
                            continue;
                    var count = (uint)item.Count;
                    log("можем продать " + count);
                    while (count > 0)
                    {
                        if (!MainForm.On)
                            return;
                        var countToSell = Math.Min(count, minimumCountForProcess[item.ItemQuality]);
                        /*  if(countToSell < MinimumCheckingCount[item.ItemQuality])
                              break;*/
                        count -= countToSell;
                        ulong sellPrice = 0;
                        if (item.ItemQuality < EItemQuality.Epic)
                            sellPrice = averages[item.Id] * countToSell;
                        var name = item.Name;
                        if (isRuLang)
                            name = item.NameRu;
                        var minbid = sellPrice;
                        if (item.ItemQuality == EItemQuality.Epic)
                        {
                            sellPrice = epicPrices[new KeyValuePair<uint, uint>(item.Id, item.Level)];
                            minbid = sellPrice - (sellPrice / 100 * 20);
                        }

                        var time = EAuctionSellTime.TwelveHours;
                        if (CharacterSettings.AukTime == 1)
                            time = EAuctionSellTime.TwentyFourHours;
                        if (CharacterSettings.AukTime == 2)
                            time = EAuctionSellTime.FortyEightHours;

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


        internal bool MyIsNeedRepair()
        {
            try
            {
                if (MapID == 1904)
                    return false;
                if (MapID == 1220 && CharacterSettings.Mode == EMode.Questing)
                    return false;
                if (!CharacterSettings.CheckRepair)
                    return false;
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Equipment)
                        continue;
                    if (item.MaxDurability == 0)
                        continue;
                    if (item.Durability < CharacterSettings.RepairCount)
                    {
                        log("Нужен ремонт " + item.Name + "  " + item.Durability + "/" + item.MaxDurability, LogLvl.Important);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return false;
        }

        internal bool MyAllItemsRepair()
        {
            var result = false;
            try
            {
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place != EItemPlace.Equipment)
                        continue;
                    if (item.MaxDurability == 0)
                        continue;
                    if (item.Durability < item.MaxDurability)
                    {
                        log("Ремонтируюсь, так как в городе " + item.Name + "  " + item.Durability + "/" + item.MaxDurability, LogLvl.Important);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return result;
        }

        internal bool MyRepair()
        {
            try
            {
                if (GetBotLogin() == "wowklausvovot")
                {

                }
                else
                if (!MyAllItemsRepair())
                    return true;

                if (CharacterSettings.SummonMount && IsOutdoors)
                {
                    var mountSell = SpellManager.GetSpell(61447);//Тундровый мамонт путешественника
                    if (mountSell == null)
                        mountSell = SpellManager.GetSpell(61425);//Тундровый мамонт путешественника

                    if (mountSell != null)
                    {
                        if (CharacterSettings.UseMountMyLoc)
                        {

                        }
                        else
                        {
                            if (CharacterSettings.MountLocX != 0)
                                if (!MoveTo(CharacterSettings.MountLocX, CharacterSettings.MountLocY, CharacterSettings.MountLocZ))
                                    return false;
                        }
                        /*  var mount = CommonModule.MyGetAura(61447);
                          if(mount == null)
                          {

                          }
                         */
                        CommonModule.MyUnmount();
                        CanselForm();
                        CancelMoveTo();
                        Thread.Sleep(500);
                        MyCheckIsMovingIsCasting();
                        var result = SpellManager.CastSpell(mountSell.Id);

                        if (result != ESpellCastError.SUCCESS)
                        {
                            log("Не удалось призвать маунта " + mountSell.Name + "  " + result, LogLvl.Error);
                            return false;
                        }
                        else
                            log("Призвал маунта", LogLvl.Ok);
                        Thread.Sleep(2000);
                        while (SpellManager.IsCasting)
                            Thread.Sleep(100);
                        Thread.Sleep(2000);
                        foreach (var npc in GetEntities<Unit>())
                        {
                            if (npc.Id == 32641 || npc.Id == 32639)
                            {
                                Thread.Sleep(1000);
                                if (!OpenShop(npc))
                                {
                                    log("Не смог открыть шоп 5 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(), LogLvl.Error);
                                    if (InteractionObject != null)
                                        log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                                    else
                                    {
                                        log("InteractionNpc = null " + CurrentInteractionGuid);
                                    }
                                    Thread.Sleep(5000);
                                    /* if (GetLastError() != ELastError.ActionNotAllowed)
                                     {
                                         return false;
                                     }*/


                                }
                                else
                                {
                                    log("Открыл шоп");
                                }

                                Thread.Sleep(1000);
                                if (!ItemManager.RepairAllItems())
                                {
                                    log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                    /* if (GetLastError() == ELastError.NoItemForRepair)
                                         return true;*/
                                    Thread.Sleep(10000);
                                    //  return false;
                                }
                                else
                                {
                                    log("Отремонтировал ", LogLvl.Ok);
                                }
                                Thread.Sleep(1000);

                                SellItems();
                                CommonModule.MyUnmount();
                                AutoQuests.NeedActionNpcSell = false;
                                AutoQuests.NeedActionNpcRepair = false;
                                return true;
                            }
                        }
                    }

                }


                var vendor = FindNpcForActionArmored();
                if (vendor == null)
                {
                    double bestDist = 9999999;
                    MyNpcLoc npcLoc = null;
                    foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                    {
                        if (_badNpcForSell.Contains(myNpcLoc.Id))
                            continue;
                        if (IsBadNpcLocs.Contains(myNpcLoc))
                            continue;
                        if (!myNpcLoc.IsArmorer)
                            continue;
                        if (Me.Distance(myNpcLoc.Loc) > bestDist)
                            continue;
                        bestDist = Me.Distance(myNpcLoc.Loc);
                        npcLoc = myNpcLoc;
                    }
                    if (npcLoc != null)
                    {
                        log("Выбрал нпс " + npcLoc.Id);
                        if (!CommonModule.MoveTo(npcLoc.Loc, 10, 10))
                            return false;
                        var listUnit2 = GetEntities<Unit>();

                        foreach (var npc in listUnit2.OrderBy(i => Me.Distance(i)))
                        {
                            if (!npc.IsArmorer)
                                continue;
                            if (!CommonModule.MoveTo(npc, 3))
                                return false;
                            Thread.Sleep(1000);
                            if (!OpenShop(npc))
                            {
                                log("Не смог открыть шоп 2 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(),
                                    LogLvl.Error);
                                if (InteractionObject != null)
                                    log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " +
                                        Me.Distance(InteractionObject.Location) + "  " + CurrentInteractionGuid);
                                else
                                {
                                    log("InteractionNpc = null " + CurrentInteractionGuid);
                                }
                                Thread.Sleep(5000);
                                /*  if (GetLastError() != ELastError.ActionNotAllowed)
                                  {
                                      return false;
                                  }*/


                            }
                            else
                            {
                                log("Открыл шоп");
                            }

                            foreach (var gossipOptionsData in GetNpcDialogs())
                            {
                                if (gossipOptionsData.Text.Contains("buy from you"))
                                    SelectNpcDialog(gossipOptionsData);
                                log(gossipOptionsData.Text);
                            }
                            Thread.Sleep(1000);
                            if (!ItemManager.RepairAllItems())
                            {
                                log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                /* if (GetLastError() == ELastError.NoItemForRepair)
                                     return true;*/
                                Thread.Sleep(10000);
                                //  return false;
                            }
                            else
                            {
                                log("Отремонтировал ", LogLvl.Ok);
                            }
                            SellItems();
                            return true;

                        }
                    }
                    IsBadNpcLocs.Add(npcLoc);

                    log("Не указаны координаты для ремонта", LogLvl.Error);
                    Thread.Sleep(10000);
                    return false;
                }
                log("Выбран НПС для ремонта  " + vendor.Name, LogLvl.Ok);

                if (vendor.AreaId != Area.Id)
                {
                    MyUseTaxi(vendor.AreaId, vendor.Loc);
                    return false;
                }

                if (!CommonModule.MoveTo(vendor.Loc, 10))
                    return false;

                var listUnit = GetEntities<Unit>();

                foreach (var npc in listUnit.OrderBy(i => Me.Distance(i)))
                {
                    if (!npc.IsArmorer)
                        continue;
                    if (!CommonModule.MoveTo(npc, 3))
                        return false;
                    Thread.Sleep(1000);
                    if (CurrentInteractionGuid == npc.Guid)
                    {
                        if (InteractionObject != null)
                            log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                        else
                        {
                            log("InteractionNpc = null " + CurrentInteractionGuid);
                        }
                    }
                    else
                    {
                        if (!OpenShop(npc))
                        {
                            log("Не смог открыть шоп 6 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(), LogLvl.Error);
                            if (InteractionObject != null)
                                log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                            else
                            {
                                log("InteractionNpc = null " + CurrentInteractionGuid);
                            }
                            Thread.Sleep(5000);
                            /* if (GetLastError() != ELastError.ActionNotAllowed)
                             {
                                 return false;
                             }*/
                        }
                        else
                        {
                            log("Открыл шоп");
                        }
                    }



                    foreach (var gossipOptionsData in GetNpcDialogs())
                    {
                        if (gossipOptionsData.Text.Contains("I need to repair"))
                        {
                            SelectNpcDialog(gossipOptionsData);
                            Thread.Sleep(1000);
                        }

                    }

                    Thread.Sleep(1000);
                    if (!ItemManager.RepairAllItems())
                    {
                        log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                        /* if (GetLastError() == ELastError.NoItemForRepair)
                             return true;*/
                        Thread.Sleep(10000);
                        //   return false;
                    }
                    else
                    {
                        log("Отремонтировал ", LogLvl.Ok);
                        Thread.Sleep(2000);
                    }
                    SellItems();
                    return true;
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return false;
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
        }

        public List<MyPlayer> MyPlayers = new List<MyPlayer>();

        public class MyPlayer
        {
            public string Name;
            public int Count;
            public DateTime Time;
            public int Level;
        }

        internal void MyCheckPlayer()
        {
            foreach (var entity in GetEntities<Player>())
            {
                MyPlayer myPlayer = null;
                foreach (var player in MyPlayers)
                    if (player.Name == entity.Name)
                        myPlayer = player;

                if (myPlayer != null && myPlayer.Time.AddSeconds(10) > DateTime.Now)
                {
                    /*log("Уже видел игрока");
                    log(myPlayer.Time.AddSeconds(10).ToString());
                    log(DateTime.Now.ToString());*/
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
                            continue;
                        MyPlayers[i].Count++;
                        MyPlayers[i].Time = DateTime.Now;
                        MyPlayers[i].Level = entity.Level;
                        break;
                    }
                }

                var path = AssemblyDirectory + "\\Log\\" + GetCurrentAccount().Login + "_" + GetCurrentAccount().Name;
                if (!isReleaseVersion)
                    path = AssemblyDirectory + "\\Plugins\\Quester\\Log\\" + GetCurrentAccount().Login + "_" + GetCurrentAccount().Name;

                File.AppendAllText(path + "_log.txt",
                    DateTime.Now.ToString(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ": " + Me.Name + "(" + Me.Level + ") " + "Встретил игрока(" + myPlayer.Count + ")   Дист: " + (int)Me.Distance(entity)
                    + " Ник: " + myPlayer.Name + "(" + entity.Level + ")" + Environment.NewLine), System.Text.Encoding.UTF8);

                //  log("Встретил игрока: " + myPlayer.Name + "(" + entity.Level + ")  Кол-во: " + myPlayer.Count + "   Дист: " + Me.Distance(entity), LogLvl.Error);
            }
        }

        bool _isneedSave;

        public List<Vector3F> BuildQuad(double x, double y, int r, double xr, double yr, Unit obj)
        {
            var sw = new Stopwatch();
            sw.Start();
            var range = 30;
            var listPoint = new List<Vector3F>();
            for (var x1 = x - range; x1 < x + range; x1 = x1 + 4)
            {
                for (var y1 = y - range; y1 < y + range; y1 = y1 + 4)
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

        internal void MyCheckNPC()
        {
            MyCheckPlayer();
            if (GetBotLogin() != "Daredevi1")
                return;
            // log(PathNPCjson);

            foreach (var gameObject in GetEntities<GameObject>())
            {
                if (gameObject.Id == 0)
                    continue;
                if (gameObject.Location == Vector3F.Zero)
                    continue;
                if (MyGameObjectLocss.GameObjectLocs.Any(collectionInvItem => gameObject.Id == collectionInvItem.Id))
                {
                    foreach (var myNpcLoc in MyGameObjectLocss.GameObjectLocs)
                    {
                        if (myNpcLoc.Id == gameObject.Id)
                        {
                            

                            var newLoc = true;
                            if (myNpcLoc.MapId == -1)
                            {
                                myNpcLoc.MapId = gameObject.Guid.GetMapId();
                                log("Добавляю MapId " + gameObject.Guid.GetMapId());
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
                                log("Найдена новая локация GO [" + gameObject.Id + "]" + gameObject.Name + " " + gameObject.Location + "  " + myNpcLoc.ListLoc.Count, LogLvl.Important);
                                _isneedSave = true;
                            }

                        }
                    }
                    continue;
                }

                _isneedSave = true;
                log("Найден новый GO " + gameObject.Name + " " + gameObject.Type, LogLvl.Important);
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
                ConfigLoader.SaveConfig(PathGameObjectLocs, MyGameObjectLocss);
            }

            _isneedSave = false;
            foreach (var entity in GetEntities<Unit>())
            {
                if (entity.Id == 0)
                    continue;
                if (entity.Type != EBotTypes.Unit)
                {
                    if (MyNpcLocss.NpcLocs.Any(collectionInvItem => entity.Id == collectionInvItem.Id))
                    {
                        log("Ошибочный НПС в списке " + entity.Name + " [" + entity.Id + "] " + entity.Type, LogLvl.Error);
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
                    continue;
                if (entity.IsSpellClick)
                {
                    /* if (AdvancedLog)
                         log("Найден НПС SpellClick " + entity.Name + " " + Me.Distance(entity), LogLvl.Error);*/
                }

                if (entity.Location == Vector3F.Zero)
                    continue;

                if (MyNpcLocss.NpcLocs.Any(collectionInvItem => entity.Id == collectionInvItem.Id))
                {
                    foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                    {
                        if (myNpcLoc.Id == entity.Id)
                        {
                            if (entity.FactionId != myNpcLoc.FactionId)
                            {
                                myNpcLoc.FactionId = entity.FactionId;
                                log("Добавляю FactionId " + entity.FactionId);
                                _isneedSave = true;
                            }

                            var newLoc = true;
                            if (myNpcLoc.MapId == -1)
                            {
                                myNpcLoc.MapId = entity.Guid.GetMapId();
                                log("Добавляю MapId " + entity.Guid.GetMapId());
                                _isneedSave = true;
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
                                newLoc = false;


                            if (newLoc)
                            {

                                myNpcLoc.ListLoc.Add(entity.Location);
                                log("Найдена новая локация NPC [" + entity.Id + "]" + entity.Name + " " + entity.Location + "  " + myNpcLoc.ListLoc.Count, LogLvl.Important);
                                _isneedSave = true;
                            }

                        }
                    }
                    continue;
                }

                _isneedSave = true;
                log("Найден новый НПС " + entity.Name + " " + entity.Type, LogLvl.Important);
                MyNpcLocss.NpcLocs.Add(new MyNpcLoc
                {

                    Id = entity.Id,
                    Loc = entity.Location,
                    IsArmorer = entity.IsArmorer,
                    //IsQuestGiver = entity.IsQuestGiver,
                    MapId = entity.Guid.GetMapId(),
                    FactionId = entity.FactionId,
                    ListLoc = new List<Vector3F>
                    {
                        entity.Location
                    }

                });
            }

            if (_isneedSave)
            {
                File.Copy(PathNpCjson, PathNpCjsonCopy, true);
                ConfigLoader.SaveConfig(PathNpCjson, MyNpcLocss);
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


        internal bool MySell(bool noMount = false)
        {
            try
            {
                var mountSell = SpellManager.GetSpell(61447);//Тундровый мамонт путешественника
                if (mountSell == null)
                    mountSell = SpellManager.GetSpell(61425);//Тундровый мамонт путешественника
                if (noMount)
                    mountSell = null;
                if (mountSell != null && IsOutdoors)
                {

                    if (CharacterSettings.UseMountMyLoc)
                    {

                    }
                    else
                    {
                        if (CharacterSettings.MountLocX != 0)
                            if (!MoveTo(CharacterSettings.MountLocX, CharacterSettings.MountLocY, CharacterSettings.MountLocZ))
                                return false;
                    }


                    CommonModule.MyUnmount();
                    CanselForm();
                    CancelMoveTo();
                    Thread.Sleep(500);
                    MyCheckIsMovingIsCasting();
                    var result = SpellManager.CastSpell(mountSell.Id);

                    if (result != ESpellCastError.SUCCESS)
                    {
                        log("Не удалось призвать маунта " + mountSell.Name + "  " + result, LogLvl.Error);
                        return false;
                    }
                    else
                        log("Призвал маунта", LogLvl.Ok);
                    Thread.Sleep(2000);
                    while (SpellManager.IsCasting)
                        Thread.Sleep(100);
                    Thread.Sleep(2000);
                    foreach (var npc in GetEntities<Unit>())
                    {
                        if (npc.Id == 32641 || npc.Id == 32639)
                        {
                            Thread.Sleep(1000);
                            if (!OpenShop(npc))
                            {
                                log("Не смог открыть шоп 7 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(), LogLvl.Error);
                                if (InteractionObject != null)
                                    log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + " " + CurrentInteractionGuid);
                                else
                                {
                                    log("InteractionNpc = null " + CurrentInteractionGuid);
                                }
                                Thread.Sleep(5000);
                                /* if (GetLastError() != ELastError.ActionNotAllowed)
                                 {
                                     return false;
                                 }*/


                            }
                            else
                            {
                                log("Открыл шоп");
                            }
                            Thread.Sleep(1000);

                            SellItems();

                            Thread.Sleep(1000);
                            if (!ItemManager.RepairAllItems())
                            {
                                log("Не смог отремонтировать " + GetLastError(), LogLvl.Error);
                                if (GetLastError() == ELastError.NoItemForRepair)
                                    return true;
                                Thread.Sleep(10000);
                                return false;
                            }
                            else
                            {
                                log("Отремонтировал ", LogLvl.Ok);
                            }
                            AutoQuests.NeedActionNpcSell = false;
                            AutoQuests.NeedActionNpcRepair = false;
                            CommonModule.MyUnmount();
                            return true;
                        }
                    }
                }

                var vendor = FindNpcForActionVendor();
                if (vendor == null)
                {
                    double bestDist = 9999999;
                    MyNpcLoc npcLoc = null;
                    foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                    {
                        if (_badNpcForSell.Contains(myNpcLoc.Id))
                            continue;
                        if (IsBadNpcLocs.Contains(myNpcLoc))
                            continue;
                        if (!myNpcLoc.IsArmorer)
                            continue;
                        if (Me.Distance(myNpcLoc.Loc) > bestDist)
                            continue;
                        bestDist = Me.Distance(myNpcLoc.Loc);
                        npcLoc = myNpcLoc;
                    }
                    if (npcLoc != null)
                    {
                        log("Выбрал нпс " + npcLoc.Id);
                        if (!CommonModule.MoveTo(npcLoc.Loc, 10, 10))
                            return false;
                        var listUnit2 = GetEntities<Unit>();

                        foreach (var npc in listUnit2.OrderBy(i => Me.Distance(i)))
                        {
                            if (!npc.IsArmorer)
                                continue;
                            if (!CommonModule.MoveTo(npc, 3))
                                return false;
                            Thread.Sleep(1000);
                            if (!OpenShop(npc))
                            {
                                log("Не смог открыть шоп 3 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(),
                                    LogLvl.Error);
                                if (InteractionObject != null)
                                    log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " +
                                        Me.Distance(InteractionObject.Location) + "  " + CurrentInteractionGuid);
                                else
                                {
                                    log("InteractionNpc = null " + CurrentInteractionGuid);
                                }
                                Thread.Sleep(5000);
                                /*   if (GetLastError() != ELastError.ActionNotAllowed)
                                   {
                                       return false;
                                   }*/


                            }
                            else
                            {
                                log("Открыл шоп");
                            }
                            Thread.Sleep(1000);
                            foreach (var gossipOptionsData in GetNpcDialogs())
                            {
                                if (gossipOptionsData.Text.Contains("buy from you"))
                                    SelectNpcDialog(gossipOptionsData);
                                log(gossipOptionsData.Text);
                            }
                            SellItems();
                            return true;
                        }
                        IsBadNpcLocs.Add(npcLoc);
                        log("Не указаны координаты бакалейщика " + npcLoc.Id, LogLvl.Error);
                    }

                    Thread.Sleep(10000);
                    return false;
                }


                log("Выбран НПС для продажи " + vendor.Name, LogLvl.Ok);
                if (vendor.AreaId != Area.Id)
                {
                    MyUseTaxi(vendor.AreaId, vendor.Loc);
                    return false;
                }


                if (!CommonModule.MoveTo(vendor.Loc, 5))
                    return false;

                var listUnit = GetEntities<Unit>();

                foreach (var npc in listUnit.OrderBy(i => Me.Distance(i)))
                {
                    if (!npc.IsVendor)
                        continue;
                    if (!CommonModule.MoveTo(npc, 3))
                        return false;
                    Thread.Sleep(1000);
                    if (!OpenShop(npc))
                    {
                        log("Не смог открыть шоп 4 " + npc.Name + "[" + npc.Id + "]  " + GetLastError(), LogLvl.Error);
                        if (InteractionObject != null)
                            log("Открыт диалог с " + InteractionObject.Name + "  " + InteractionObject.Id + "  " + Me.Distance(InteractionObject.Location) + "  " + CurrentInteractionGuid);
                        else
                        {
                            log("InteractionNpc = null " + CurrentInteractionGuid);
                        }
                        Thread.Sleep(5000);
                        /*   if (GetLastError() != ELastError.ActionNotAllowed)
                           {
                               return false;
                           }*/


                    }
                    else
                    {
                        log("Открыл шоп");
                    }
                    Thread.Sleep(1000);

                    SellItems();
                    return true;
                }
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception e)
            {
                log(e.ToString());
                return false;
            }
            return false;
        }


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
                        {

                        }
                        break;
                    default:
                        {
                            CanselForm();
                        }
                        break;
                }


                Thread.Sleep(1000);
                if (SpellManager.GetItemCooldown(item.Id) > 0)
                {
                    if (AdvancedLog)
                        log(item.Name + " MyUseItemAndWait  GetItemCooldown:" + SpellManager.GetItemCooldown(item.Id));
                    return false;
                }
                else
                {
                    CommonModule.MyUnmount();
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsChanneling)
                        Thread.Sleep(50);

                    Thread.Sleep(500);
                    var result = SpellManager.UseItem(item, target);
                    if (result == EInventoryResult.OK)
                        log("Использую " + item.Name + "[" + item.Id + "] " + item.Place, LogLvl.Ok);
                    else
                        log("Не получилось использовать  1 " + item.Name + "[" + item.Id + "]  на " + target?.Name +" " + result + "  " + GetLastError(), LogLvl.Error);
                    Thread.Sleep(1000);
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsChanneling)
                        Thread.Sleep(50);
                    if (result == EInventoryResult.OK)
                        return true;
                    else
                    {
                        return false;
                    }

                }

            }
            catch (Exception e)
            {
                log(e.ToString());

            }
            return false;
        }



        private List<Unit> _listMobs = new List<Unit>();
        public List<Unit> GetAgroCreatures()
        {
            try
            {


                var myListCreature = new List<Unit>();
                if (FarmModule.farmState == FarmState.Disabled)
                    return myListCreature;
                for (var i = 0; i < _listMobs.Count; i++)
                {
                    if (i == _listMobs.Count)
                        break;
                    if (!IsAlive(Me))
                        break;
                    if (i < _listMobs.Count && !IsExists(_listMobs[i]))
                    {
                        _listMobs.RemoveAt(i);
                        break;
                    }
                    if (i < _listMobs.Count && !IsAlive(_listMobs[i]))
                    {
                        _listMobs.RemoveAt(i);
                        KillMobsCount++;
                        break;
                    }
                }

                foreach (var entity in GetEntities<Unit>())
                {
                    if (!entity.IsAlive)
                        continue;
                    if (entity.Target != Me)
                        continue;
                    if (entity.Type == EBotTypes.Player)
                        continue;
                    if (!CanAttack(entity, CanSpellAttack))
                        continue;
                    myListCreature.Add(entity);
                    if (!_listMobs.Contains(entity) && IsAlive(entity))
                        _listMobs.Add(entity);
                }
                if(Me.GetPet() != null)
                foreach (var entity in GetEntities<Unit>())
                {
                    if (!entity.IsAlive)
                        continue;
                    if (entity.Target != Me.GetPet())
                        continue;
                    if (entity.Type == EBotTypes.Player)
                        continue;
                    if (!CanAttack(entity, CanSpellAttack))
                        continue;
                    myListCreature.Add(entity);
                    if (!_listMobs.Contains(entity) && IsAlive(entity))
                        _listMobs.Add(entity);
                }

                foreach (var i in Me.GetThreats())
                {
                    if (!i.IsAlive)
                        continue;
                    if (AutoQuests.BestQuestId != 47576)
                        if (Me.Distance(i) > 40)
                            continue;
                    if (i.Id == 128604)
                        continue;

                    //   log(i.Obj.Name + "  " + Me.Distance(i.Obj ) + "   " + i.Value);
                    myListCreature.Add(i);
                    if (!_listMobs.Contains(i) && IsAlive(i))
                        _listMobs.Add(i);
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
                                myItemsStat.Count = myItemsStat.Count + item.Count;
                                isNewItem = false;
                            }
                        }
                        if (isNewItem)
                            StartInv.Add(new MyItemsStat { Id = item.Id, Count = item.Count, Name = item.Name });
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
                                myItemsStat.Count = myItemsStat.Count + item.Count;
                                isNewItem = false;
                            }
                        }
                        if (isNewItem)
                            UpdateInv.Add(new MyItemsStat { Id = item.Id, Count = item.Count, Name = item.Name });
                    }
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        public void CompInv()
        {
            try
            {
                var result = new List<MyItemsStat>();



                foreach (var myItemsStat in UpdateInv)
                {
                    var isFind = false;
                    foreach (var itemsStat in StartInv)
                    {
                        if (myItemsStat.Id == itemsStat.Id) //Нашел
                        {
                            if (myItemsStat.Count != itemsStat.Count)
                            {
                                var isNewItem = true;
                                foreach (var stat in result)
                                {

                                    if (stat.Id == myItemsStat.Id)
                                    {
                                        isNewItem = false;
                                        stat.Count = myItemsStat.Count - itemsStat.Count;
                                    }
                                }
                                if (isNewItem)
                                {
                                    result.Add(new MyItemsStat { Id = itemsStat.Id, Count = myItemsStat.Count - itemsStat.Count, Name = itemsStat.Name });
                                }
                            }
                            isFind = true;
                        }

                    }
                    if (!isFind)
                    {
                        result.Add(new MyItemsStat { Id = myItemsStat.Id, Count = myItemsStat.Count, Name = myItemsStat.Name });
                    }
                }
                var allTime = DateTime.Now - TimeWork;
                MainForm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainForm.LabelInvUpdate.Content = "Имя: Получено (расчет на сутки)" + ItemManager.GetFreeInventorySlotsCount() + "\n";
                }));

                foreach (var myItemsStat in result)
                {
                    MainForm.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MainForm.LabelInvUpdate.Content = MainForm.LabelInvUpdate.Content + myItemsStat.Name +/* "[" + myItemsStat.Id + */": " + myItemsStat.Count + " (" +
                                                          Math.Round((myItemsStat.Count /*+ doubleGold*/) / allTime.TotalDays, 2) + ")\n";
                    }));
                    //  log(myItemsStat.Name + "  " + myItemsStat.Count );
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        /// <summary>
        /// Продажа предметов
        /// </summary>
        /// <returns></returns>
        private void SellItems()
        {
            try
            {
                //Глобальная продажа
                foreach (var item in ItemManager.GetItems())
                {

                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                        item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                        item.Place == EItemPlace.InventoryItem)
                    {
                        if (item.GetSellPrice() == 0)
                            continue;
                        foreach (var characterSettingsMyItemGlobal in CharacterSettings.MyItemGlobals)
                        {
                            if (item.ItemClass == characterSettingsMyItemGlobal.Class && item.ItemQuality == characterSettingsMyItemGlobal.Quality)
                            {
                                var isNosell = false;
                                foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
                                {
                                    if (item.Id == characterSettingsItemSetting.Id && characterSettingsItemSetting.Use == 4)
                                        isNosell = true;
                                }

                                if (characterSettingsMyItemGlobal.ItemLevel != 0)
                                {
                                    if (item.Level > characterSettingsMyItemGlobal.ItemLevel)
                                        isNosell = true;
                                }
                                
                                if (!isNosell)
                                {
                                    log("Продаю " + item.Name + "  [" + item.Id + "]  Цена:" + (item.GetSellPrice() / 10000f).ToString("F2"), LogLvl.Ok);
                                    var result = item.Sell();
                                    if (result != ESellResult.Success)
                                    {
                                        log("Не смог продать  " + item.Name + "[" + item.Id + "] Цена:" + (item.GetSellPrice() / 10000f).ToString("F2") + "  " + result + " " + " " + GetLastError(), LogLvl.Error);
                                        Thread.Sleep(5000);
                                        if (GetLastError() == ELastError.CantFindNpc)
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }
                //Обычная продажа
                foreach (var item in ItemManager.GetItems())
                {
                    if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 || item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 || item.Place == EItemPlace.InventoryItem)
                    {
                        if (item.GetSellPrice() == 0)
                            continue;

                        foreach (var itemSettingse in CharacterSettings.ItemSettings)
                        {
                            if (itemSettingse.Id == item.Id && itemSettingse.Use == 2 && Me.Level > itemSettingse.MeLevel)
                            {
                                log("Продаю " + item.Name + "  " + item.Id + "  Цена:" + item.GetSellPrice(), LogLvl.Ok);
                                var result = item.Sell();
                                if (result != ESellResult.Success)
                                {
                                    log("Не смог продать  " + item.Name + "[" + item.Id + "] Цена:" + item.GetSellPrice() + "  " + result + " " + " " + GetLastError(), LogLvl.Error);
                                    Thread.Sleep(5000);
                                }
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception err)
            {
                log("SellItems2: " + err);
            }
        }
        public static int GetPercent(int b, int a)
        {
            if (b == 0)
                return 0;
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
                        CommonModule.MoveTo(go, 3);
                        break;
                }


                MyCheckIsMovingIsCasting();
                if (!(go as GameObject).Use())
                {
                    log("Не смог использовать " + go.Name + " " + GetLastError(), LogLvl.Error);
                    Thread.Sleep(5000);
                    return;
                }
                else
                {
                    log("Использовал " + go.Name, LogLvl.Ok);
                    Thread.Sleep(1000);
                }

                while (SpellManager.IsCasting)
                    Thread.Sleep(100);
                while (SpellManager.IsChanneling)
                    Thread.Sleep(100);

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
                return;
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
                    CommonModule.MoveTo(go);
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
                Thread.Sleep(100);

            Thread.Sleep(500);
            if (!CanPickupLoot())
                return;
            if (!PickupLoot())
                log("Не смог поднять дроп " + GetLastError(), LogLvl.Error);
        }


        /// <summary>
        /// Общая проверка
        /// </summary>
        /// <returns></returns>
        internal bool Check()
        {
            try
            {
                if (MainForm.On
                    && !cancelRequested
                    && FarmModule.BestMob == null
                    && GameState == EGameState.Ingame
                    && CheckCanUseGameActions()
                    // && IsAlive(Me)
                    && FarmModule.readyToActions
                    && MyGetAura(269824) == null
                    //&& Me.ConditionPhase != EConditionPhase.Spirit
                    //&& Me.ConditionPhase != EConditionPhase.Dead

                    // && CurInvCount() < 30
                    // && !host.commonModule.InFight()
                    // && host.Me.GetAgroCreatures() == 0
                    // && !IsPenaltyBuff()
                    // && host.Me.HpPercents > 80

                    )
                    return true;
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
                if (Distance(editorGpsPoint.X, editorGpsPoint.Y, editorGpsPoint.Z, x, y, z) < radius)
                    return true;
            }
            return false;
        }

        internal bool FindNearPointInRadiusNoZ(double x, double y, double radius)
        {
            foreach (var editorGpsPoint in GetEditorGpsPoints())
            {
                if (DistanceNoZ(editorGpsPoint.X, editorGpsPoint.Y, x, y) < radius)
                    return true;
            }
            return false;
        }

        internal Aura MyGetAura(uint id)
        {
            foreach (var aura in Me.GetAuras())
            {
                if (aura.SpellId == id)
                    return aura;
            }
            return null;
        }


    }
}
