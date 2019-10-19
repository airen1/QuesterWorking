using Out.Internal.Core;
using Out.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using WoWBot.Core;

namespace WowAI.Module
{
    internal partial class FarmModule
    {
        public int DeadCount;
        public int DeadCountInPvp;
        public DateTime DeathTime;
        private int _fixRes;

        private void PrepareForFight()
        {
            CheckWeDie();
        }

        private void MyReviveWithNpc()
        {
            if (!Host.MainForm.On) return;
            Host.log("Воскршаюсь на кладбище");
            Unit npc = null;
            foreach (var i in Host.GetEntities<Unit>())
            {
                if (!i.IsSpiritService)
                    continue;
                npc = i;
            }

            if (npc == null)
            {
                if (!Host.ReturnToRevivalBasePoint())
                {
                    Host.log("IsAlive: " + Host.IsAlive(Host.Me) + "Воскрешение не удачно " + Host.GetLastError(), LogLvl.Error);
                }
                else
                {
                    while (Host.GameState != EGameState.Ingame)
                    {
                        if (!Host.MainForm.On) return;
                        Thread.Sleep(1000);
                    }

                    Host.CancelMoveTo();
                    Thread.Sleep(5000);
                }

                return;
            }

            Host.CommonModule.MoveTo(npc, 2);
            Thread.Sleep(2000);

            while (true)
            {
                Thread.Sleep(1000);
                if (!Host.MainForm.On) return;
                var res = true;
                foreach (var entity in Host.GetEntities<Player>())
                {
                    if (entity.Team == Host.Me.Team)
                        continue;
                    Host.log("Враг " + entity.Name + " " + Host.Me.Distance(entity), LogLvl.Error);
                    res = false;
                }

                if (!res)
                    Host.log("Не воскрешаюсь, рядом враг ");
                break;
            }
            Host.MyCheckIsMovingIsCasting();
            if (!Host.ReviveWithNpc(npc))
            {
                Host.log("Воскрешение на кладбище не успешно 1 " + Host.GetLastError(), LogLvl.Error);
                _fixRes++;
                if (_fixRes > 2)
                {
                    Host.log("Выключаю окно игры, так как не смог реснутся", LogLvl.Error);
                    Host.TerminateGameClient();
                    return;
                }

                // FarmState = FarmState.AttackOnlyAgro;
                Thread.Sleep(10000);
                if (!Host.Me.IsDeadGhost)
                    _fixRes = 0;
            }


            if (Host.Me.IsDeadGhost) return;

            if (Host.Me.Location.Distance(-10604, 294, 32) < 10)
                Host.CommonModule.ForceMoveTo(-10652, 288, 40, 5);

            Thread.Sleep(60 * 1000);
        }


        private void CheckWeDie()
        {
            if (Host.IsAlive() && !Host.Me.IsDeadGhost)
                return;
            if (!Host.MainForm.On)
                return;
            try
            {
                Host.log("Умер " + Host.IsAlive() + " " + Host.Me.IsDeadGhost); //1735.48, -363.99, 9.50 данж 2    220.64, -303.54, 18.53 данж 1

                foreach (var myObstacleDic in Host.DicObstaclePic)
                    Host.RemoveObstacle(myObstacleDic.Value.Id);
                Host.DicObstaclePic.Clear();
                Host.MyDelBigObstacle(true);

                EventDeath = true;
                if (Host.CommonModule.AttackPlayer)
                {
                    EventDeathPlayer = true;
                }

                Host.FarmModule.BestMob = null;
                Host.FarmModule.BestProp = null;
                FarmState = FarmState.Disabled;
                Host.CancelMoveTo();

                if (!Host.IsAlive())
                    Thread.Sleep(3000);

                if (!Host.Me.IsDeadGhost)
                {
                    if (Host.CommonModule.AttackPlayer)
                        DeadCountInPvp++;
                    else
                        DeadCount++;
                    Host.CommonModule.AttackPlayer = false;
                    if (Host.MapID == 189 && Host.CharacterSettings.PikPocket)
                    {
                        if (Host.Me.Distance(220.64, -303.54, 18.53) < 500)
                            IsWing1 = false;
                        else
                            IsWing1 = true;
                    }

                    if (!Host.ReturnToRevivalBasePoint())
                    {
                        Host.log("IsAlive: " + Host.IsAlive(Host.Me) + " Воскрешение не удачно " + Host.GetLastError(),
                            LogLvl.Error);
                        return;
                    }
                    else
                    {
                       
                        Thread.Sleep(1000);
                        while (Host.GameState != EGameState.Ingame)
                            Thread.Sleep(1000);
                        while (!Host.CheckCanUseGameActions())
                            Thread.Sleep(1000);
                    }
                }


                if (Host.MapID == 1760 || Host.MapID == 1904)
                {
                    Thread.Sleep(30000);
                    //  host.FarmModule.farmState = FarmState.AttackOnlyAgro;
                    return;
                }


                Host.CommonModule.ResumeMove();
                Host.log("Ищу свой труп " + Host.ReviveCorpseInfo.IsValid + "  " + Host.ReviveCorpseInfo.Location + "  " + Host.ReviveCorpseInfo.MapId);

                if (Host.ReviveCorpseInfo.Location == Vector3F.Zero)
                {
                    Host.log("Не нашел труп", LogLvl.Error);
                    return;
                }

                var begin = DateTime.Now;
                var end = DeathTime;
                var rez = begin - end;

                Host.log("Последняя смерть " + rez.TotalMinutes);
                var min = 5;
                if (Host.CharacterSettings.Mode == Mode.Questing || Host.CharacterSettings.Mode == Mode.QuestingClassic)
                    min = 1;
                if (Host.CharacterSettings.PikPocket)
                {
                    if (Host.MyIsNeedSell())
                    {
                        MyReviveWithNpc();
                        return;
                    }
                    min = 1;
                }


                if (Host.CharacterSettings.Mode == Mode.QuestingClassic)
                {
                    if (Host.MyIsNeedRepair())
                    {
                        MyReviveWithNpc();
                        return;
                    }
                }




                if (rez.TotalMinutes < min)
                {
                    MyReviveWithNpc();
                    return;
                }
                else
                {
                    Host.log("Бегу к трупу");
                    if (Host.MapID != Host.ReviveCorpseInfo.MapId)
                    {
                        if (!Host.CommonModule.MoveTo(Host.ReviveCorpseInfo.Location, 0))
                            return;

                        if (Host.ReviveCorpseInfo.MapId == 429)
                        {
                            if (!Host.CommonModule.MoveTo(-3735.73, 934.64, 161.01, 0))
                                return;
                        }

                        if (Host.ReviveCorpseInfo.MapId == 189)
                        {
                            if (IsWing1)
                            {
                                if (!Host.CommonModule.MoveTo(2866.74, -821.74, 160.33, 0)
                                ) // 2884.65, -836.56, 160.33 данж 2
                                    return;
                                var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing1.xml";
                                Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                                var reader = new XmlSerializer(typeof(DungeonSetting));
                                using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite,
                                    FileShare.ReadWrite))
                                    Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                            }
                            else
                            {
                                if (!Host.CommonModule.MoveTo(2884.65, -836.56, 160.33, 0)) //  данж 2
                                    return;
                                var scriptName = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\Script\\wing2.xml";
                                Host.log("Применяю скрипт: " + scriptName, LogLvl.Ok);
                                var reader = new XmlSerializer(typeof(DungeonSetting));
                                using (var fs = File.Open(scriptName, FileMode.Open, FileAccess.ReadWrite,
                                    FileShare.ReadWrite))
                                    Host.DungeonSettings = (DungeonSetting)reader.Deserialize(fs);
                            }
                        }

                        Thread.Sleep(1000);
                        Host.MyMoveForvard(2000);
                        return;
                    }
                    else
                    {
                        if (!Host.CommonModule.MoveTo(Host.ReviveCorpseInfo.Location, 35))
                            return;
                        Thread.Sleep(3000);
                        while (true)
                        {
                            Thread.Sleep(1000);
                            if (!Host.MainForm.On) return;
                            var res = true;
                            foreach (var entity in Host.GetEntities<Player>())
                            {
                                if (entity.Team == Host.Me.Team)
                                    continue;
                                Host.log("Враг " + entity.Name + " " + Host.Me.Distance(entity), LogLvl.Error);
                                res = false;
                            }

                            if (!res)
                                Host.log("Не воскрешаюсь, рядом враг ");
                            break;
                        }

                        var zone = new RoundZone(Host.ReviveCorpseInfo.Location.X, Host.ReviveCorpseInfo.Location.Y, 38);
                        var timer = new Stopwatch();
                        timer.Start();
                        while (true)
                        {
                            if (!Host.MainForm.On)
                                return;
                            Thread.Sleep(100);
                            if (timer.ElapsedMilliseconds > 60000)
                                break;
                            var findNewPoint = false;
                            foreach (var entity in Host.GetEntities<Unit>())
                            {
                                if (!entity.IsAlive)
                                    continue;
                                if (Host.Me.Distance(entity) > 20)
                                    continue;
                                if (entity.Level < 4)
                                    continue;
                                var zRange = Math.Abs(Host.Me.Location.Z - entity.Location.Z);

                                if (zRange > 5)
                                    continue;

                                findNewPoint = true;
                            }

                            if (findNewPoint)
                            {
                                var randPoint = zone.GetRandomPoint();
                                var randLoc = new Vector3F(randPoint.X, randPoint.Y,
                                    Host.GetNavMeshHeight(new Vector3F(randPoint.X, randPoint.Y, Host.Me.Location.Z)));
                                if (!Host.CommonModule.CheckPathForLoc(Host.Me.Location, randLoc))
                                    randLoc = Vector3F.Zero;
                                else
                                {
                                    foreach (var entity in Host.GetEntities<Unit>())
                                    {
                                        if (!entity.IsAlive)
                                            continue;
                                        if (randLoc.Distance(entity.Location) > 20)
                                            continue;
                                        if (entity.Level < 4)
                                            continue;
                                        var zRange = Math.Abs(Host.Me.Location.Z - entity.Location.Z);
                                        if (zRange > 5)
                                            continue;
                                        randLoc = Vector3F.Zero;
                                    }
                                }


                                if (randLoc != Vector3F.Zero)
                                {
                                    Host.log("Нашел точку бегу в нее");
                                    Host.CommonModule.MoveTo(randLoc, 1);
                                }
                                else
                                {
                                    Host.log("Не нашел точку, ищу новую");
                                }
                            }
                            else
                            {
                                break;
                            }
                        }


                        if (!Host.ReviveWithCorpse())
                        {
                            Host.log("Воскрешение возле трупа не успешно  возвращаюсь на кладбище" + Host.GetLastError(), LogLvl.Error);
                            Thread.Sleep(10000);
                            MyReviveWithNpc();
                        }
                    }
                }


                DeathTime = DateTime.Now;
                Thread.Sleep(2000);

                /*   FarmState = FarmState.Disabled;
                   if (host.CharacterSettings.Mode == EMode.Questing)
                       FarmState = FarmState.AttackOnlyAgro;*/
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Host.log(e.ToString());
            }
            finally
            {
                if (!Host.Me.IsDeadGhost && Host.IsAlive())
                    FarmState = FarmState.AttackOnlyAgro;
                Host.NeedWaitAfterCombat = false;
            }
        }
    }
}