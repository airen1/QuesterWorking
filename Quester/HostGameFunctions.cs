using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WowAI.Modules;
using Out.Internal.Core;
using WoWBot.Core;
using System.Linq;
using System.Windows.Documents;
using Out.Utility;
using WowAI.UI;
using WoWBot.Database;

namespace WowAI
{
    static class TimeSpanExtensions
    {
        static public bool IsBetween(this TimeSpan time,
            TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime == startTime)
            {
                return true;
            }
            if (endTime < startTime)
            {
                return time <= endTime ||
                       time >= startTime;
            }
            return time >= startTime &&
                   time <= endTime;
        }
    }
    internal partial class Host
    {



        public List<uint> NoShowSkill = new List<uint>
        {
            3365,   //Opening IsPassive =  False
            6233,   //Closing IsPassive =  False
            6246,   //Closing IsPassive =  False
            6247,   //Opening IsPassive =  False
            6477,  // Opening IsPassive =  False
            6478,   //Opening IsPassive =  False
            7266,   //Duel IsPassive =  False
            7267,   //Grovel IsPassive =  False
            21651,   //Opening IsPassive =  False
            21652,   //Closing IsPassive =  False
            61437,   //Opening IsPassive =  False
            96220,   //Opening IsPassive =  False
            68398,   //Opening IsPassive =  False
            7355,   //Stuck IsPassive =  
            22027,   //Remove Insignia IsPassive =  False
            22810,   //Opening - No Text IsPassive =  False
            161691,   //Garrison Ability IsPassive =  False
            45927,   //Summon Friend IsPassive =  False
            211390,   //Combat Ally IsPassive =  False
            59752,    //Every Man for Himself IsPassive =  False
            //Охотник
            1494,   //Track Beasts IsPassive =  False
            19878,   //Track Demons IsPassive =  False
            19879,   //Track Dragonkin IsPassive =  False
            19880,   //Track Elementals IsPassive =  False
            19882,   //Track Giants IsPassive =  False
            19885,   //Track Hidden IsPassive =  False
            19883,   //Track Humanoids IsPassive =  False
            19884,   //Track Undead IsPassive =  False
            200749,   //Activating Specialization IsPassive =  False
            883,   //Call Pet 1 IsPassive =  False
            982,   //Revive Pet IsPassive =  False
            175686,   //Stopping Power IsPassive =  False

          //  6603,   //Auto Attack IsPassive =  False
            8386,  // Attacking IsPassive =  False
          //  5019,  // Shoot IsPassive =  False
            205243,   //Снятие шкур IsPassive =  False
            194174,   //Навыки снятия шкур IsPassive =  False
          //  8613,   //Снятие шкур IsPassive =  False
            212801,   //Смещение IsPassive =  False
        };


        /* public Aura MyGetAura(uint id)
         {
             foreach (var aura in Me.GetAuras())
             {
                 if (aura.SpellId == id)
                     return aura;
             }

             return null;
         }*/


        public static bool IsInRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {

            return dateToCheck >= startDate && dateToCheck <= endDate;
        }

        public void MyCheckIsMovingIsCasting()
        {


            var fixMove = 0;
            while (SpellManager.IsCasting || Me.IsMoving)
            {
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


        public bool MyDialog(Entity npc, string text)
        {
            var result = false;
            ForceMoveTo(npc.Location);
            Thread.Sleep(1000);
            if (!OpenDialog(npc))
                log("Не смог открыть диалог " + GetLastError(), Host.LogLvl.Error);

            if (GetNpcDialogs().Count == 0)
            {
                SendKeyPress(0x1b);
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                    log("Не смог открыть диалог 2 " + GetLastError(), Host.LogLvl.Error);
            }



            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                if (gossipOptionsData.Text.Contains("Подгород") || gossipOptionsData.Text.Contains(text))
                {

                    if (!SelectNpcDialog(gossipOptionsData))
                        log("Не смог выбрать диалог " + GetLastError(), Host.LogLvl.Error);
                    // Thread.Sleep(1000);
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
                log("Не смог открыть диалог " + GetLastError(), Host.LogLvl.Error);

            if (GetNpcDialogs().Count == 0)
            {
                SendKeyPress(0x1b);
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                    log("Не смог открыть диалог 2 " + GetLastError(), Host.LogLvl.Error);
            }



            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                if (gossipOptionsData.ClientOption == index)
                {

                    if (!SelectNpcDialog(gossipOptionsData))
                        log("Не смог выбрать диалог " + GetLastError(), Host.LogLvl.Error);
                    // Thread.Sleep(1000);
                    result = true;
                }
                log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
            }

            if (npc.Id == 127128 || npc.Id == 130905 || npc.Id == 130929 || npc.Id == 131135)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == index)
                    {

                        if (!SelectNpcDialog(gossipOptionsData))
                            log("Не смог выбрать диалог " + GetLastError(), Host.LogLvl.Error);
                        // Thread.Sleep(1000);
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
                            log("Не смог выбрать диалог " + GetLastError(), Host.LogLvl.Error);
                        // Thread.Sleep(1000);
                        result = true;
                    }
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " + gossipOptionsData.ClientOption + "  " + "   ");
                }
            }



            return result;
        }



        public void FlyForm()
        {

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
                                log("Не удалось поменять форму " + spell.Name + "  " + resultForm, Host.LogLvl.Error);
                                if (resultForm == ESpellCastError.NOT_MOUNTED)
                                    CommonModule.MyUnmount();
                            }
                            else
                                log("Поменял форму " + spell.Name, Host.LogLvl.Ok);

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
                    // var IsNeedMount = false;

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
                {
                    log("Не удалось призвать маунта " + mountSpell.Name + "  " + result, Host.LogLvl.Error);
                }
                else
                {
                    log("Призвал маунта", Host.LogLvl.Ok);
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
                    CommonModule.ForceMoveTo2(vector3F);
                }
            }
            if (Me.Team == ETeam.Alliance)
            {
                path = CommonModule.GpsBase.GetPath(new Vector3F(-8860.24, 638.56, 96.35), Me.Location);
                foreach (var vector3F in path)
                {
                    log(path.Count + "  Путь " + Me.Distance(vector3F));
                    CommonModule.ForceMoveTo2(vector3F);
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
                {
                    log("Открыл ящик", LogLvl.Ok);
                }
                Thread.Sleep(2000);

                foreach (var mail in GetMails())
                {
                    log(mail.SenderType + " " + mail.GetAttachedItems().Count);
                    mail.MarkAsRead();
                    Thread.Sleep(500);
                    if (GetBotLogin() == "deathstar")
                        Thread.Sleep(500);
                    if (!mail.TakeAllAttachmentsAndGold())
                    {
                        log("Не удалось получить письмо " + GetLastError(), LogLvl.Error);
                    }
                    else
                    {
                        log("Получил письмо", LogLvl.Ok);
                    }
                }
            }
        }


        /*  public GameObject GetNearestMailBox()
          {
              double bestDist = double.MaxValue;
              GameObject result = null;
              foreach (var e in GetEntities<GameObject>())
              {
                  if (e.GameObjectType != EGameObjectType.Mailbox)
                      continue;
                  if (Me.Distance(e) < bestDist)
                  {
                      bestDist = Me.Distance(e);
                      result = e;
                  }
              }
              return result;
          }
          public void HandleMails()
          {
              var mb = GetNearestMailBox();
              if (mb == null)
                  return;
              OpenMailbox(mb);
              Console.WriteLine(GetMails().Count);
              foreach (var mail in GetMails())
              {
                  mail.MarkAsRead();
                  mail.TakeAllAttachmentsAndGold();
                  //пустые письма клиент удаляет сам, хз
              }
          }*/

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
                        CommonModule.ForceMoveTo2(vector3F);
                    }
                }

            if (Me.Team == ETeam.Alliance)
            {
                path = CommonModule.GpsBase.GetPath(new Vector3F(-8816.10, 660.36, 98.01), Me.Location);
                foreach (var vector3F in path)
                {
                    log(path.Count + "  Путь " + Me.Distance(vector3F));
                    CommonModule.ForceMoveTo2(vector3F);
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
                log("Нет НПС для аука", Host.LogLvl.Error);
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

        public bool NeedAuk = false;
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
            {
                if (MapID == 0 || MapID == 1)
                    return true;
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
                        log("Не удалось использовать камень " + item.Name + " " + result + " " + GetLastError(), Host.LogLvl.Error);
                        return false;
                    }
                    else
                    {
                        log("Использовал камень ", Host.LogLvl.Ok);
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
                        log("Не удалось использовать свисток " + item.Name + " " + result + " " + GetLastError(), Host.LogLvl.Error);
                        return false;
                    }
                    else
                    {
                        log("Использовал свисток ", Host.LogLvl.Ok);
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

        /// <summary>
        /// Возвращает дистанцию между координатами
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>
        /// <returns></returns>
        public double Distance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2) + Math.Pow((z1 - z2), 2));
        }

        public double DistanceGpcPoint(GpsPoint loc1, GpsPoint loc2)
        {
            return
                Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) +
                          Math.Pow((loc1.Z - loc2.Z), 2));
        }

        public double DistanceVectorGpsPoint(Vector3F loc1, GpsPoint loc2)
        {
            return
                Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) +
                          Math.Pow((loc1.Z - loc2.Z), 2));
        }

        /// <summary>
        /// Возвращает дистанцию между координатами без учета Z
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает Creature по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Entity GetNpcById(uint id)
        {
            try
            {
                var listEntity = GetEntities();
                foreach (var npc in listEntity.OrderBy((i => Me.Distance(i))))
                {
                    if (npc.Distance(2475.74, 1130.51, 5.97) < 10)
                        continue;
                    /*if (!npc.IsVisible)
                        continue;*/
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
                if (item.Place == EItemPlace.Bag1 || item.Place == EItemPlace.Bag2 ||
                    item.Place == EItemPlace.Bag3 || item.Place == EItemPlace.Bag4 ||
                    item.Place == EItemPlace.InventoryItem)
                    if (item.Id == id)
                    {
                        return item;
                    }
            }
            return null;
        }

        internal bool MyUseTaxi(uint areaId, Vector3F loc)
        {
            try
            {
                if (GetThreats().Count > 0)
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

                        if (i.Id == 2073 && CharacterSettings.Mode != EMode.Questing)//Throne Room, Zuldazar  1642  0  37.4401321411133  2073
                            continue;

                        if (i.Id == 2116 && CharacterSettings.Mode != EMode.Questing)//Disabled   Quest Path 6698: Horde Embassy, Zuldazar -> Throne Room, Zuldazar  1642  0  270.530822753906  2116
                            continue;
                        if (i.Id == 2015 && CharacterSettings.Mode != EMode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
                            continue;
                        if (i.Id == 1961 && CharacterSettings.Mode != EMode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
                            continue;
                        if (i.Id == 2078 && CharacterSettings.Mode != EMode.Questing)//  Disabled Quest Path 6437: 8.0 Nazmir - Q49082 - Flight out of Hir'eek's Lair -LWB  1642  0  0  2015
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


                        log(i.Name + "  " + i.MapId + "  " + i.Cost + "  " + Me.Distance(i.Location) + "  " + i.Id);
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
                    if (Distance(loc.X, loc.Y, loc.Z, canLandNode.Location.X, canLandNode.Location.Y, canLandNode.Location.Z) < bestDistnode)
                    {
                        bestDistnode = Distance(loc.X, loc.Y, loc.Z, canLandNode.Location.X, canLandNode.Location.Y, canLandNode.Location.Z);
                        node = canLandNode;
                    }
                }
                log("Выбрал точку " + node.Name + " " + node.Id + "  " + node.MapId, LogLvl.Ok);


                if (node != null)
                {
                    Thread.Sleep(1000);
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
                return false;
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
            {
                if (item.Id == id)
                    count = count + item.Count;
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

            Dictionary<EItemQuality, uint> MinimumCountForProcess = new Dictionary<EItemQuality, uint>();
            MinimumCountForProcess[EItemQuality.Normal] = 200;
            MinimumCountForProcess[EItemQuality.Uncommon] = 200;
            MinimumCountForProcess[EItemQuality.Rare] = 5;
            MinimumCountForProcess[EItemQuality.Epic] = 5;

            Dictionary<EItemQuality, int> MinimumCheckingCount = new Dictionary<EItemQuality, int>();
            MinimumCheckingCount[EItemQuality.Normal] = 2000;
            MinimumCheckingCount[EItemQuality.Uncommon] = 2000;
            MinimumCheckingCount[EItemQuality.Rare] = 200;
            MinimumCheckingCount[EItemQuality.Epic] = 1;
            List<uint> ItemIDsForSell = new List<uint>();
            foreach (var characterSettingsAukSettingse in CharacterSettings.AukSettingses)
            {
                if (ItemIDsForSell.Contains(Convert.ToUInt32(characterSettingsAukSettingse.Id)))
                    continue;
                ItemIDsForSell.Add(Convert.ToUInt32(characterSettingsAukSettingse.Id));
            }

            if (ItemIDsForSell.Count == 0)
                return;
            Dictionary<uint, List<Item>> Items = new Dictionary<uint, List<Item>>();
            Dictionary<uint, ulong> Averages = new Dictionary<uint, ulong>();
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
                    if (ItemIDsForSell.Contains(item.Id))
                    {
                        if (!Items.ContainsKey(item.Id))
                            Items[item.Id] = new List<Item>();
                        Items[item.Id].Add(item);
                    }
                }


            }

            /*  Dictionary<uint, ulong> epicPrices = new Dictionary<uint, ulong>
              {
                  {350, 0 },
                  {355, 0 },
                  {360, 0 }
              };*/

            Dictionary<KeyValuePair<uint, uint>, ulong> epicPrices = new Dictionary<KeyValuePair<uint, uint>, ulong>();
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

            foreach (var id in ItemIDsForSell)
            {
                if (!Items.ContainsKey(id))
                    continue;
                int totalCount = 0;
                foreach (var item in Items[id])
                    totalCount += item.Count;
                var firstItem = Items[id][0];
                var quality = firstItem.ItemQuality;
                string name = firstItem.Name;
                if (isRuLang)
                    name = firstItem.NameRu;
                log("Проверяем предмет: " + name + "[" + firstItem.Id + "]. Суммарное количество в инвентаре: " + totalCount + ", размер стака: " + firstItem.MaxStackCount);
                log("Минимально ищем: " + MinimumCheckingCount[firstItem.ItemQuality]);
                var req = new AuctionSearchRequest();
                req.MaxReturnItems = 50;
                req.SearchText = name;
                req.ExactMatch = true;

                ulong priceSumm = 0;
                int itemsCount = 0;

                Dictionary<ulong, int> normalPrices = new Dictionary<ulong, int>();
                ulong mylotPrice = 0;
                while (itemsCount < MinimumCheckingCount[firstItem.ItemQuality])
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
                                    if (characterSettingsAukSettingse.Id == aucItem.ItemId &&
                                        characterSettingsAukSettingse.Level == aucItem.ItemLevel)
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




                if (itemsCount >= MinimumCheckingCount[quality])
                {

                    ulong averagePrice = priceSumm / (uint)itemsCount;
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
                            Averages[firstItem.Id] = normalPrice.Key - 1;
                            if (mylotPrice == normalPrice.Key)
                                Averages[firstItem.Id] = normalPrice.Key;
                            log("Цена " + normalPrice.Key + "  Кол-во " + normalPrice.Value + "  " + Averages[firstItem.Id]);

                            break;
                        }

                        log("Средняя цена для " + itemsCount + " " + name + "{" + firstItem.Level + "}[" + firstItem.Id + "] = " + (Averages[firstItem.Id] / 10000f).ToString("F2"));

                    }
                }
            }

            log("Проверяем итемы " + Items.Count);
            foreach (var k in Items)
            {
                foreach (var item in k.Value)
                {
                    log("Проверяем итем [" + item.Id + "] в количестве " + item.Count);
                    if (item.ItemQuality < EItemQuality.Epic)
                        if (!Averages.ContainsKey(item.Id))
                            continue;
                    var count = (uint)item.Count;
                    log("можем продать " + count);
                    while (count > 0)
                    {
                        if (!MainForm.On)
                            return;
                        var countToSell = Math.Min(count, MinimumCountForProcess[item.ItemQuality]);
                        /*  if(countToSell < MinimumCheckingCount[item.ItemQuality])
                              break;*/
                        count -= countToSell;
                        ulong sellPrice = 0;
                        if (item.ItemQuality < EItemQuality.Epic)
                            sellPrice = Averages[item.Id] * countToSell;
                        string name = item.Name;
                        if (isRuLang)
                            name = item.NameRu;
                        var minbid = sellPrice;
                        if (item.ItemQuality == EItemQuality.Epic)
                        {

                            sellPrice = epicPrices[new KeyValuePair<uint, uint>(item.Id, item.Level)];

                            minbid = sellPrice - (sellPrice / 100 * 20);
                        }

                        log("Выставляем на продажу " + name + "{" + item.Level + "}[" + item.Id + "] в количестве " + countToSell + " штук за " + (sellPrice / 10000f).ToString("F2") + "   " + (minbid / 10000f).ToString("F2"), LogLvl.Important);
                         var result = item.AuctionSell(minbid, sellPrice, EAuctionSellTime.TwelveHours, countToSell);
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
                if (!MyAllItemsRepair())
                    return true;


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
                        log("Не удалось призвать маунта " + mountSell.Name + "  " + result, Host.LogLvl.Error);
                        return false;
                    }
                    else
                        log("Призвал маунта", Host.LogLvl.Ok);
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
                                if (GetLastError() == ELastError.NoItemForRepair)
                                    return true;
                                Thread.Sleep(10000);
                                return false;
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


                var Vendor = FindNpcForActionArmored();
                if (Vendor == null)
                {
                    double bestDist = 9999999;
                    MyNpcLoc npcLoc = null;
                    foreach (var myNpcLoc in MyNpcLocss.NpcLocs)
                    {
                        if (myNpcLoc.Id == 3319)
                            continue;
                        if (myNpcLoc.Id == 46512)
                            continue;
                        if (myNpcLoc.Id == 46359)
                            continue;
                        if (myNpcLoc.Id == 54657)
                            continue;
                        if (myNpcLoc.Id == 69977)
                            continue;
                        if (myNpcLoc.Id == 69978)
                            continue;
                        if (myNpcLoc.Id == 3314)
                            continue;
                        if (myNpcLoc.Id == 32641)
                            continue;
                        if (myNpcLoc.Id == 3331)
                            continue;
                        if (myNpcLoc.Id == 5816)
                            continue;
                        if (myNpcLoc.Id == 3330)
                            continue;
                        if (myNpcLoc.Id == 3315)
                            continue;
                        if (myNpcLoc.Id == 3317)
                            continue;
                        if (myNpcLoc.Id == 3321)
                            continue;
                        if (myNpcLoc.Id == 3316)
                            continue;
                        if (myNpcLoc.Id == 3493)
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
                            return true;

                        }
                    }
                    IsBadNpcLocs.Add(npcLoc);

                    log("Не указаны координаты для ремонта", LogLvl.Error);
                    Thread.Sleep(10000);
                    return false;
                }
                log("Выбран НПС для ремонта  " + Vendor.Name, LogLvl.Ok);

                if (Vendor.AreaId != Area.Id)
                {
                    MyUseTaxi(Vendor.AreaId, Vendor.Loc);
                    return false;
                }

                if (!CommonModule.MoveTo(Vendor.Loc, 10))
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
                        if (GetLastError() == ELastError.NoItemForRepair)
                            return true;
                        Thread.Sleep(10000);
                        return false;
                    }
                    else
                    {
                        log("Отремонтировал ", LogLvl.Ok);
                        Thread.Sleep(2000);
                    }
                    return true;
                }

            }
            catch (Exception e)
            {
                log(e.ToString());

            }
            return false;
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
            public bool IsQuestGiver;
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
                {
                    if (player.Name == entity.Name)
                        myPlayer = player;
                }

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

                var path = AssemblyDirectory + "\\Log\\" + GetCurrentAccount().Login;
                if (!isReleaseVersion)
                    path = AssemblyDirectory + "\\Plugins\\Quester\\Log\\" + GetCurrentAccount().Login;

                File.AppendAllText(path + "_log.txt",
                    DateTime.Now.ToString(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ": " + Me.Name + "(" + Me.Level + ") " + "Встретил игрока(" + myPlayer.Count + ")   Дист: " + (int)Me.Distance(entity) + " Ник: " + myPlayer.Name + "(" + entity.Level + ")" + Environment.NewLine));

                //  log("Встретил игрока: " + myPlayer.Name + "(" + entity.Level + ")  Кол-во: " + myPlayer.Count + "   Дист: " + Me.Distance(entity), LogLvl.Error);
            }
        }

        internal void MyCheckNPC()
        {
            MyCheckPlayer();
            if (GetBotLogin() != "Daredevi1")
                return;
            // log(PathNPCjson);
            foreach (var entity in GetEntities<Unit>())
            {
                if (MyNpcLocss.NpcLocs.Any(collectionInvItem => entity.Id == collectionInvItem.Id))
                    continue;

                log("Найден новый НПС " + entity.Name + " IsArmorer:" + entity.IsArmorer + " IsQuestGiver:" + entity.IsQuestGiver, LogLvl.Important);
                MyNpcLocss.NpcLocs.Add(new MyNpcLoc
                {
                    Id = entity.Id,
                    Loc = entity.Location,
                    IsArmorer = entity.IsArmorer,
                    IsQuestGiver = entity.IsQuestGiver

                });
            }

            ConfigLoader.SaveConfig(PathNpCjson, MyNpcLocss);
        }

        public List<MyNpcLoc> IsBadNpcLocs = new List<MyNpcLoc>();

        internal bool MySell()
        {
            try
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


                    CommonModule.MyUnmount();
                    CanselForm();
                    CancelMoveTo();
                    Thread.Sleep(500);
                    MyCheckIsMovingIsCasting();
                    var result = SpellManager.CastSpell(mountSell.Id);

                    if (result != ESpellCastError.SUCCESS)
                    {
                        log("Не удалось призвать маунта " + mountSell.Name + "  " + result, Host.LogLvl.Error);
                        return false;
                    }
                    else
                        log("Призвал маунта", Host.LogLvl.Ok);
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
                        if (myNpcLoc.Id == 3319)
                            continue;
                        if (myNpcLoc.Id == 46512)
                            continue;
                        if (myNpcLoc.Id == 46359)
                            continue;
                        if (myNpcLoc.Id == 54657)
                            continue;
                        if (myNpcLoc.Id == 69977)
                            continue;
                        if (myNpcLoc.Id == 69978)
                            continue;
                        if (myNpcLoc.Id == 3314)
                            continue;
                        if (myNpcLoc.Id == 32641)
                            continue;
                        if (myNpcLoc.Id == 3331)
                            continue;
                        if (myNpcLoc.Id == 5816)
                            continue;
                        if (myNpcLoc.Id == 3330)
                            continue;
                        if (myNpcLoc.Id == 3315)
                            continue;
                        if (myNpcLoc.Id == 3317)
                            continue;
                        if (myNpcLoc.Id == 3321)
                            continue;
                        if (myNpcLoc.Id == 3316)
                            continue;
                        if (myNpcLoc.Id == 3493)
                            continue;
                        if (myNpcLoc.Id == 3492)
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

                            SellItems();
                            return true;
                        }
                    }
                    IsBadNpcLocs.Add(npcLoc);
                    log("Не указаны координаты бакалейщика", LogLvl.Error);
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
                    MyCheckIsMovingIsCasting();
                    while (SpellManager.IsChanneling)
                        Thread.Sleep(50);

                    Thread.Sleep(500);
                    var result = SpellManager.UseItem(item, target);
                    if (result == EInventoryResult.OK)
                    {
                        log("Использую " + item.Name + "[" + item.Id + "] " + item.Place, Host.LogLvl.Ok);
                        //  FarmModule.SetBadTarget(host.FarmModule.bestMob, 120000);
                    }

                    else
                        log("Не получилось использовать " + item.Name + "[" + item.Id + "]  " + result + "  " + GetLastError(), Host.LogLvl.Error);
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
        /// <summary>
        /// Возвращает Creature по Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
      /*  internal Entity GetNpcByName(string name)
        {
            try
            {
                foreach (var npc in GetEntities())
                {
                    if (!npc.IsVisible)
                        continue;
                    if (npc.Type == EBotTypes.Npc && npc.Name == name)
                        return npc;
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }
            return null;
        }*/


        public int ResTimer = 0;
        /// <summary>
        /// Воскрешение
        /// </summary>
        /// <returns></returns>
        internal bool RessurectTalkWithNpc()
        {
            try
            {
                Thread.Sleep(5000);
                while (GameState != EGameState.Ingame)
                    Thread.Sleep(1000);
                CancelMoveTo();

                Thread.Sleep(RandGenerator.Next(150, 400));
                if (!IsAlive(Me))
                    return false;
                else
                {
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
        }

        private List<Unit> _listMobs = new List<Unit>();


        /// <summary>
        /// Возвращает лист Агро мобов
        /// </summary>
        /// <returns></returns>
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

                foreach (var i in GetThreats(Me))
                {
                    if (!i.Obj.IsAlive)
                        continue;
                    if (Me.Distance(i.Obj) > 40 && i.Value == 0)
                        continue;
                    if (i.Obj.Id == 128604)
                        continue;

                    //   log(i.Obj.Name + "  " + Me.Distance(i.Obj ) + "   " + i.Value);
                    myListCreature.Add(i.Obj);
                    if (!_listMobs.Contains(i.Obj) && IsAlive(i.Obj))
                        _listMobs.Add(i.Obj);
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
                            if (item.ItemClass == characterSettingsMyItemGlobal.Class &&
                                item.ItemQuality == characterSettingsMyItemGlobal.Quality)
                            {
                                var isNosell = false;
                                foreach (var characterSettingsItemSetting in CharacterSettings.ItemSettings)
                                {
                                    if (item.Id == characterSettingsItemSetting.Id &&
                                        characterSettingsItemSetting.Use == 4)
                                        isNosell = true;
                                }
                                if (!isNosell)
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


        public void MyUseGameObject(uint id)
        {
            var go = GetNpcById(id);
            if (go != null)
            {
                switch (id)
                {

                    case 273992:

                        break;
                    case 291008:
                        break;
                    default:
                        CommonModule.MoveTo(go);
                        break;
                }


                MyCheckIsMovingIsCasting();
                if (!(go as GameObject).Use())
                {
                    log("Не смог использовать " + go.Name + " " + GetLastError(), Host.LogLvl.Error);
                    Thread.Sleep(5000);
                    return;
                }
                else
                {
                    log("Использовал " + go.Name, LogLvl.Ok);
                }

                while (SpellManager.IsCasting)
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(500);
                if (CanPickupLoot())
                {
                    if (!PickupLoot())
                    {
                        /* host.CommonModule.ForceMoveTo(m.Location, 1, 1);
                         if (!m.PickUp())
                         {*/

                        log("Не смог поднять дроп " + GetLastError(), Host.LogLvl.Error);

                        //   }
                    }
                }

            }
            else
            {
                log("Не нашел геймобжект " + id);
            }
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



        internal void ChangeForBestChanel()
        {

            try
            {

            }
            catch (Exception e)
            {
                log(e.ToString());
            }
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

        /*  internal bool IsPropExitis(int id)
          {
              foreach (var prop in GetEntities<Prop>())
              {
                  if (prop.Id != id)
                      continue;

                  return true;
              }
              return false;
          }*/

        /*
        private int GetNearestBoss()
        {
            foreach (var creature in host.GetCreatures())
            {
                if (creature.Type != EBotTypes.Npc)
                    continue;
                if ((creature as Npc).Db.Grade != ENPCGradeType.Boss)
                    continue;
                return creature.Id;
            }
            return 0;
        }
*/
    }
}
