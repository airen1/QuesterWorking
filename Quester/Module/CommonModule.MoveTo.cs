using Out.Internal.Core;
using Out.Utility;
using System;
using System.Threading;
using WoWBot.Core;

namespace WowAI.Module
{
    internal partial class CommonModule
    {
        public int MoveFailCount;
        private bool _isMovementSuspended;
        private bool _isMoveToNow;
        public bool TryGetMidPoints = true;
        private bool IsMovementSuspended
        {
            get => _isMovementSuspended;
            set => _isMovementSuspended = value;
        }

        public bool IsMoveToNow
        {
            get => _isMoveToNow;
            set => _isMoveToNow = value;
        }

        public bool IsMoveSuspended()
        {
            return IsMovementSuspended;
        }

        public void SuspendMove()
        {
            Host.CancelMoveTo();
            IsMovementSuspended = true;
        }

        public void ResumeMove()
        {
            IsMovementSuspended = false;
        }

        private void CheckMoveFailed(bool result, Vector3F loc)
        {
            try
            {
                Host.CheckUnderWather();

                if (!result)
                {
                    var le = Host.GetLastError();

                    if (le != ELastError.Movement_MoveCanceled && le != ELastError.Movement_AnotherMoveCalled && le != ELastError.NotIngame)
                    {
                        MoveFailCount++;
                        Host.log("Остановился: " + Host.GetLastError() + "(" + MoveFailCount + ")" + " " + Host.Me.MovementFlagExtra + " " + Host.Me.MovementFlags + " " + Host.Me.SwimBackSpeed +
                                 " " + Host.Me.SwimSpeed + " loc: " + loc + " " + Host.Me.Distance(loc),
                            LogLvl.Error);
                        if (Host.Me.MovementFlags == EMovementFlag.Pending_root)
                        {
                            MoveFailCount = 0;
                            return;
                        }

                        while ((Host.Me.MovementFlags & EMovementFlag.DisableGravity) == EMovementFlag.DisableGravity &&
                               (Host.Me.MovementFlags & EMovementFlag.Root) == EMovementFlag.Root)
                        {
                            if (!Host.MainForm.On)
                                return;
                            if (Host.MyGetAura(269564) != null)
                                break;
                            if (Host.MyGetAura(245831) != null)
                                break;
                            if (Host.AutoQuests.BestQuestId == 52472)
                                break;
                            if (Host.MapID == 1718)
                                break;
                            Host.log("Ожидаю возврата движения " + Host.Me.MovementFlags);
                            Thread.Sleep(5000);
                        }
                    }
                    else
                    {
                        if (Host.Area.Id == 141 && Host.MapID == 1 && Host.Zone.Id == 262)
                        {
                        }
                        else
                        {
                            MoveFailCount = 0;
                        }
                    }
                }
                else
                    MoveFailCount = 0;


                if (MoveFailCount > 0)
                {
                    //  Host.log("Прыжок");

                    Host.SetMoveStateForClient(true);
                    Host.MoveForward(true);
                    Host.SetMoveStateForClient(false);
                    Host.Jump();
                    Host.SetMoveStateForClient(true);
                    Host.MoveForward(false);
                    Host.SetMoveStateForClient(false);
                    //   Host.log("Прыгнул");
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

                /*  if (MoveFailCount > 2)
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
                          u += a;
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
                  }*/

                if (MoveFailCount > 3 && Host.CharacterSettings.UnmountMoveFail)
                    MyUnmount();

                if (MoveFailCount > 4 && MoveFailCount < 10)
                {
                    Host.CancelMoveTo();
                    Host.SetMoveStateForClient(true);
                    Host.MoveBackward(true);
                    Thread.Sleep(2000);
                    Host.MoveBackward(false);
                    Host.SetMoveStateForClient(false);
                }

                if (MoveFailCount > 4)
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

                if (MoveFailCount > 10)
                {
                    if (Host.Me.IsDeadGhost)
                        Host.FarmModule.DeathTime = DateTime.Now;
                }

                //  if (Host.CharacterSettings.Mode != EMode.Questing)
                if (MoveFailCount > 20 || Host.Me.Distance(1477.98, 1591.57, 39.66) < 5)
                {
                    if (Host.MapID != 1643)
                    {
                        Host.CanselForm();
                        Host.CancelMoveTo();
                        Host.MyCheckIsMovingIsCasting();
                        Thread.Sleep(2000);
                        if (Host.CharacterSettings.UseStoneIfStuck)
                        {
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
                                        Host.log(
                                            "Не удалось использовать камень " + item.Name + " " + result2 + " " +
                                            Host.GetLastError(), LogLvl.Error);
                                    }
                                    else
                                    {
                                        Host.log("Использовал камень ", LogLvl.Ok);
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
                        else
                        {
                            Host.log("Застрял, выключаю окно игры");
                            Host.TerminateGameClient();
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

        public bool MoveToConvoy(Entity obj, double dist = 1)
        {
            try
            {
                if (!Host.MainForm.On)
                    return false;


                IsMoveToNow = true;


                var req = new MoveParams()
                {
                    Location = obj.Location,
                    Obj = null,
                    Dist = dist,
                    DoneDist = Host.Me.RunSpeed / 5.0,
                    UseNavCall = true,
                    NoWaitResult = true,
                    IgnoreStuckCheck = true
                };
                var result = Host.MoveTo(req);

                // CheckMoveFailed(result);             
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

        public bool MyUseGpsCustom(Vector3F loc, bool useMount = true)
        {
            var pointNeatMe = false;
            var pointNearDest = false;
            foreach (var allGpsPoint in _gpsBaseCustom.GetAllGpsPoints())
            {
                if (Host.Me.Distance(allGpsPoint.X, allGpsPoint.Y, allGpsPoint.Z) < 40)
                    pointNeatMe = true;
                if (loc.Distance(allGpsPoint.X, allGpsPoint.Y, allGpsPoint.Z) < 40)
                    pointNearDest = true;
            }

            if (Host.MyGetAura(269564) != null)
                useMount = false;
            if (pointNearDest && pointNeatMe)
            {
                var path = _gpsBaseCustom.GetPath(loc, Host.Me.Location);
                if (path.Count > 1)
                {
                    Host.log("Нашел путь по ГПС " + path.Count);
                    for (var index = 1; index < path.Count; index++)
                    {
                        var vector3F = path[index];
                        if (!Host.Me.IsAlive)
                            return false;
                        if (!Host.MainForm.On)
                            return false;
                        if (Host.GetAgroCreatures().Count > 0)
                            return false;
                        if (Host.Me.Distance(vector3F) > 100)
                        {
                            if (!ForceMoveTo2(vector3F, 1, useMount, true))
                                return false;
                        }

                        if (!ForceMoveTo2(vector3F, 1, useMount))
                            return false;
                    }
                }
            }

            return true;
        }

        public bool MyUseGps(Vector3F loc, bool useMount = true)
        {
            if (Host.ClientType == EWoWClient.Classic && Host.Area.Id == 1519)
                return true;
            if (Host.ClientType == EWoWClient.Classic && Host.Area.Id == 1637)
                return true;

            var pointNeatMe = false;
            var pointNearDest = false;
            foreach (var allGpsPoint in GpsBase.GetAllGpsPoints())
            {
                if (Host.Me.Distance(allGpsPoint.X, allGpsPoint.Y, allGpsPoint.Z) < 20)
                    pointNeatMe = true;
                if (loc.Distance(allGpsPoint.X, allGpsPoint.Y, allGpsPoint.Z) < 20)
                    pointNearDest = true;
            }

            if (Host.MyGetAura(269564) != null)
                useMount = false;

            if (pointNearDest && pointNeatMe)
            {
                var path = GpsBase.GetPath(loc, Host.Me.Location);
                if (path.Count > 1)
                {
                    Host.log("Нашел путь по ГПС 1 " + path.Count);
                    foreach (var vector3F in path)
                    {
                        if (!Host.Me.IsAlive)
                            return false;
                        if (!Host.MainForm.On)
                            return false;
                        if (!ForceMoveTo2(vector3F, 1, useMount))
                            return false;
                    }
                }
            }

            return true;
        }

        public double CalcPathDistance(Vector3F from, Vector3F to)
        {
            var pathToMob = Host.GetSmoothPath2(@from, to, new Vector3F(0.01, 2, 0.01), 0.5f, 0.1f, false);
            if (pathToMob == null || pathToMob.Path == null || pathToMob.InternalResult <= 0)
            {
                return double.MaxValue;
            }

            double pathLen = 0;
            for (int i = 1; i < pathToMob.Path.Count; i++)
            {
                var d = pathToMob.Path[i - 1].Distance(pathToMob.Path[i]);
                if (d > 5)
                    return double.MaxValue;
                if (d > pathLen)
                    pathLen = d;
            }

            return pathLen;
        }

        public bool CheckPathForLoc(Vector3F from, Vector3F to)
        {
            var dist = CalcPathDistance(@from, to);
            if (dist > 5)
            {
                //  Host.log("Не нашел путь  " + dist, ELogLvl.Important);
                return false;
            }

            return true;
        }

        public bool CheckServerPath(Vector3F loc, double dist)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Host.GetNavMeshHeight(new Vector3F(loc.X, loc.Y, 0)) == 0 && Host.Me.Distance(loc) > 300)
            {
                if (Host.AutoQuests?.BestQuestId == 50751)
                {
                    if (Host.Me.Distance(loc) > 1800)
                    {
                        Host.MyUseTaxi(8501, new Vector3F(2034.23, 4810.68, 71.18));
                        return false;
                    }
                }

                if (Host.AutoQuests?.BestQuestId == 50703 && Host.GetQuest(50703) != null)
                    if (Host.Area.Id == 8501) //волдун
                    {
                        Host.MyUseTaxi(8499, new Vector3F(-1035.45, 758.30, 435.33));
                        return false;
                    }


                var path = Host.GetServerPath(Host.Me.Location, loc);
                if (path == null
                    // || Host.ClientType == EWoWClient.Classic
                    || Host.Me.Distance(-976.23, 1200.57, 283.16) < 80
                    || Host.Me.Distance(-972.04, 1121.50, 241.91) < 80
                    || Host.Me.Distance(-1036.92, 1130.74, 189.23) < 80
                    || path.Path.Count < 10)
                {
                    Host.log("Не нашел путь " + Host.Me.Distance(loc) + " " + loc);

                    // loc.Z = 0;
                    // Host.log("Бегу по серверным мешам " + Host.Me.Location + " в " + loc);
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
                        CheckMoveFailed(Host.ComeTo(resX, resY, z, 50, 50), new Vector3F(resX, resY, z));
                        return false;
                    }
                    else
                    {
                        Host.log("Точка вне мешей " + Host.Me.Distance(resX, resY, z) + " " + expectedDistance);
                        CheckMoveFailed(Host.ComeTo(resX, resY, z, 250, 250), new Vector3F(resX, resY, z));
                        return false;
                    }

                    // loc.Z = 116;
                }

                Host.log("Бегу по серверным мешам " + Host.Me.Location + " в " + loc + "  всего точек " + path.Path.Count + " Дист " + Host.Me.Distance(loc));


                for (var i = 0; i < path.Path.Count - 2; i++)
                {
                    if (!Host.IsInsideNavMesh(path.Path[i]))
                        continue;
                    if (Host.Me.Distance(path.Path[i]) < 200)
                        continue;
                    if (Host.FarmModule.BestMob != null)
                        return false;
                    if (!Host.Me.IsAlive)
                        return false;
                    if (!Host.MainForm.On)
                        return false;
                    Host.log("Начал бег " + path.Path[i] + "  промежуточная точка  " + Host.Me.Distance(path.Path[i]) + " всего " + Host.Me.Distance(loc));
                    var moveParams = new MoveParams
                    {
                        Location = path.Path[i],
                        Dist = dist,
                        DoneDist = Host.Me.RunSpeed / 5.0,
                        IgnoreStuckCheck = true,
                        ForceRandomJumps = Host.CharacterSettings.RandomJump,
                        UseNavCall = true,
                        TryGetMidPoints = TryGetMidPoints
                    };

                    if (!Host.MoveTo(moveParams))
                    {
                        Host.log("Не добежал");
                        CheckMoveFailed(false, path.Path[i]);
                        return false;
                    }

                    Host.log("Закончил бег " + path.Path[i] + " промежуточная точка " + Host.Me.Distance(path.Path[i]) + " всего " + Host.Me.Distance(loc));

                    // return false;
                    //   Host.log("Закончил бег");
                }

                if (Host.Me.IsAlive && !Host.Me.IsInCombat && Host.Me.Distance(loc) > dist)
                    Host.ComeTo(loc, dist, Host.Me.RunSpeed / 5.0);

                return false;
            }

            return true;
        }

        public bool MoveTo(Vector3F loc, double dist, bool useMount = true)
        {
            if (IsMovementSuspended)
                return false;
            if (!Host.MainForm.On)
                return false;

            try
            {
                if (!MyUseGps(loc, useMount))
                    return false;
                if (!Host.CommonModule.MyUseGpsCustom(loc))
                    return false;
                if (useMount)
                    MySitMount(loc);

                IsMoveToNow = true;

                if (Host.ClientType == EWoWClient.Retail)
                    if (!MoveToBadLoc(loc))
                    {
                        CheckMoveFailed(false, loc);
                        return false;
                    }

                if (Host.AutoQuests?.BestQuestId == 50934)
                {
                    if (Host.Me.Distance(loc) > 1000)
                    {
                        Host.MyUseTaxi(8500, new Vector3F(2007.54, -79.64, 1.91));
                        return false;
                    }
                }


                if (!CheckServerPath(loc, dist))
                    return false;


                if (loc.Distance(-986.00, -3797.00, 0.11) < 5)
                    loc.Z = (float)5.2;
                if (Host.Me.Distance(713.81, 3128.34, 133.02) < 30 && Host.Me.Distance(loc) > 300)
                    Host.MoveTo(749.13, 3099.93, 133.11);


                if (_logRun)
                    Host.log(
                        "MoveTo Run Vector3F:  " + loc + "  Distance: " + Host.Me.Distance(loc) + "   dist: " + dist + "  " + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ",
                        LogLvl.Important);
                var moveParams = new MoveParams
                {
                    Location = loc,
                    Dist = dist,
                    DoneDist = Host.Me.RunSpeed / 5.0,
                    IgnoreStuckCheck = true,
                    ForceRandomJumps = Host.CharacterSettings.RandomJump,
                    UseNavCall = true,
                    TryGetMidPoints = TryGetMidPoints
                };

                var result = Host.MoveTo(moveParams);
                if (_logRun)
                    Host.log("MoveTo End Vector3F: " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ",
                        LogLvl.Important);
                //  var result = Host.ComeTo(loc, dist, doneDist);
                //   Host.log("Закончил бег в " + loc + "  дист: " + Host.Me.Distance(loc));

                CheckMoveFailed(result, loc);
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

        public bool MoveTo(Entity obj, double dist)
        {
            if (IsMovementSuspended)
                return false;
            if (!Host.MainForm.On)
                return false;

            try
            {
                if (!MyUseGps(obj.Location, false))
                    return false;
                IsMoveToNow = true;
                if (Host.ClientType == EWoWClient.Retail)
                    if (!MoveToBadLoc(obj.Location))
                    {
                        CheckMoveFailed(false, obj.Location);
                        return false;
                    }

                if (_logRun)
                    Host.log(
                        "MoveTo Run Entity:  " + obj.Name + "  Distance: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(obj.Location) + " ",
                        LogLvl.Important);

                var moveParams = new MoveParams
                {
                    Obj = obj,
                    Dist = dist,
                    DoneDist = Host.Me.RunSpeed / 5.0,
                    IgnoreStuckCheck = true,
                    ForceRandomJumps = Host.CharacterSettings.RandomJump,
                    UseNavCall = true,
                    TryGetMidPoints = TryGetMidPoints
                };

                var result = Host.MoveTo(moveParams);


                if (_logRun)
                    Host.log("MoveTo End Entity: " + obj.Name + "  дист: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(obj.Location) + " ",
                        LogLvl.Important);

                CheckMoveFailed(result, obj.Location);
                return result;
            }
            finally
            {
                IsMoveToNow = false;
            }
        }

        public bool MoveTo(double x, double y, double z, double dist = 1)
        {
            return MoveTo(new Vector3F(x, y, z), dist);
            /* if (IsMovementSuspended)
                 return false;
             if (!Host.MainForm.On)
                 return false;



             try
             {
                 MySitMount(new Vector3F(x, y, z));
                 IsMoveToNow = true;

                 if (Host.GetNavMeshHeight(new Vector3F(x, y, 0)) == 0 && Host.Me.Distance(x, y, z) > 300)
                 {
                     Host.log("Бегу по серверным мешам x, y, z,");
                     var path = Host.GetServerPath(Host.Me.Location, new Vector3F(x, y, z));
                     for (var i = 0; i < path.Path.Count - 1; i++)
                     {
                         if (Host.Me.Distance(path.Path[i]) < 50)
                             continue;
                         if (!Host.Me.IsAlive)
                             return false;
                         if (!Host.MainForm.On)
                             return false;
                         if (!Host.ComeTo(path.Path[i], dist, Host.Me.RunSpeed / 5.0))
                         {
                             CheckMoveFailed(false, path.Path[i]);
                             return false;
                         }
                     }
                 }

                 if (_logRun)
                     Host.log("MoveTo Run x, y, z,:  " + x + ", " + y + ", " + z + "  Distance: " + Host.Me.Distance(x, y, z) + "   dist: " + dist, ELogLvl.Important);
                 var moveParams = new MoveParams
                 {
                     Location = new Vector3F(x, y, z),
                     Dist = dist,
                     DoneDist = Host.Me.RunSpeed / 5.0,
                     IgnoreStuckCheck = true,
                     ForceRandomJumps = Host.CharacterSettings.RandomJump,
                     UseNavCall = true,
                     TryGetMidPoints = true
                 };

                 var result = Host.MoveTo(moveParams);
                 if (_logRun)
                     Host.log("MoveTo End x, y, z,:  " + x + ", " + y + ", " + z + "  Distance: " + Host.Me.Distance(x, y, z) + "   dist: " + dist, ELogLvl.Important);
                 CheckMoveFailed(result, new Vector3F(x, y, z));
                 return result;
             }
             finally
             {
                 IsMoveToNow = false;
             }*/
        }

        public bool ForceMoveTo(Vector3F loc, double dist = 1)
        {
            if (!Host.MainForm.On)
                return false;
            MySitMount(loc);


            var moveParams = new MoveParams();
            bool result;

            if (!CheckServerPath(loc, dist))
                return false;

            if (Host.CharacterSettings.PikPocket)
            {
                if (_logRun)
                    Host.log(
                        "ForceMoveTo Run Vector3F:  " + loc + "  Distance: " + Host.Me.Distance(loc) + "   dist: " +
                        dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);

                moveParams.Location = loc;
                moveParams.Dist = dist;
                moveParams.DoneDist = Host.Me.RunSpeed / 5.0;
                moveParams.IgnoreStuckCheck = false;
                moveParams.ForceRandomJumps = false;
                moveParams.UseNavCall = true;
                moveParams.TryGetMidPoints = TryGetMidPoints;

                result = Host.MoveTo(moveParams);
                if (_logRun)
                    Host.log(
                        "ForceMoveTo End Vector3F: " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ",
                        LogLvl.Important);
                CheckMoveFailed(result, loc);
            }
            else
            {
                if (_logRun)
                    Host.log(
                        "ForceMoveTo Run Vector3F:  " + loc + "  Distance: " + Host.Me.Distance(loc) + "   dist: " +
                        dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);

                moveParams = new MoveParams
                {
                    Location = loc,
                    Dist = dist,
                    DoneDist = Host.Me.RunSpeed / 5.0,
                    IgnoreStuckCheck = true,
                    ForceRandomJumps = Host.CharacterSettings.RandomJump,
                    UseNavCall = true,
                    TryGetMidPoints = TryGetMidPoints
                };

                result = Host.MoveTo(moveParams);
                if (_logRun)
                    Host.log(
                        "ForceMoveTo End Vector3F: " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ",
                        LogLvl.Important);
                CheckMoveFailed(result, loc);
            }


            return result;
        }

        public bool ForceMoveTo(Entity obj, double dist = 0.5, bool mount = true)
        {
            if (!Host.MainForm.On)
                return false;


            if (mount)
            {
                MySitMount(obj.Location);
            }

            if (!MyUseGps(obj.Location))
                return false;


            var moveParams = new MoveParams();
            bool result;

            if (Host.CharacterSettings.Mode == Mode.Script)
            {
                if (Host.CharacterSettings.PikPocket)
                {
                    /*  if (Host.ClientType == EWoWClient.Classic)
                          Host.SetCTMMovement(true);*/
                    if (_logRun)
                        Host.log("ForceMoveTo Run Entity:  " + obj.Name + "  Distance: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(obj.Location) + " ", LogLvl.Important);
                    moveParams.Obj = obj;
                    moveParams.Dist = dist;
                    moveParams.DoneDist = Host.Me.RunSpeed / 5.0;
                    moveParams.IgnoreStuckCheck = false;
                    moveParams.ForceRandomJumps = false;
                    moveParams.UseNavCall = true;
                    moveParams.TryGetMidPoints = TryGetMidPoints;

                    result = Host.MoveTo(moveParams);
                    if (_logRun)
                        Host.log("ForceMoveTo End Entity: " + obj.Name + "  дист: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(obj.Location) + " ", LogLvl.Important);
                    CheckMoveFailed(result, obj.Location);
                }
                else
                {
                    Host.log("Бегу без учета застреваний");
                    moveParams.Obj = obj;
                    moveParams.Dist = dist;
                    moveParams.DoneDist = Host.Me.RunSpeed / 5.0;
                    moveParams.IgnoreStuckCheck = true;
                    moveParams.ForceRandomJumps = Host.CharacterSettings.RandomJump;
                    moveParams.UseNavCall = true;

                    result = Host.MoveTo(moveParams);
                    Host.log("Добежал");
                    CheckMoveFailed(result, obj.Location);
                }
            }
            else
            {
                var loc = obj.Location;
                if (Host.Me.MovementFlags == EMovementFlag.Swimming && Host.Me.Distance2D(obj.Location) > 3)
                {
                    loc = new Vector3F(obj.Location.X, obj.Location.Y, Host.GetNavMeshHeight(obj.Location));
                    obj = null;
                }

                if (Host.Me.Distance(obj) < 30)
                {
                    /* if (Host.ClientType == EWoWClient.Classic)
                         Host.SetCTMMovement(true);*/

                    if (_logRun)
                        Host.log("ForceMoveTo Run Entity:  " + obj?.Name + "  Distance: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);


                    if (obj != null)
                    {
                        moveParams.Obj = obj;
                        moveParams.Dist = dist;
                        moveParams.DoneDist = Host.Me.RunSpeed / 5.0;
                        moveParams.IgnoreStuckCheck = true;
                        moveParams.ForceRandomJumps = Host.CharacterSettings.RandomJump;
                        moveParams.UseNavCall = true;
                        moveParams.TryGetMidPoints = TryGetMidPoints;
                        result = Host.MoveTo(moveParams);
                        CheckMoveFailed(result, obj.Location);
                    }
                    else
                    {
                        moveParams.Location = loc;
                        moveParams.Dist = dist;
                        moveParams.DoneDist = Host.Me.RunSpeed / 5.0;
                        moveParams.IgnoreStuckCheck = true;
                        moveParams.ForceRandomJumps = Host.CharacterSettings.RandomJump;
                        moveParams.UseNavCall = true;
                        moveParams.TryGetMidPoints = TryGetMidPoints;
                        result = Host.MoveTo(moveParams);
                        CheckMoveFailed(result, loc);
                    }


                    if (_logRun)
                        Host.log("ForceMoveTo End Entity: " + obj?.Name + "  дист: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);

                    /*  if (Host.ClientType == EWoWClient.Classic && Host.CharacterSettings.Mode != EMode.QuestingClassic)
                          Host.SetCTMMovement(false);*/
                }
                else
                {
                    if (_logRun)
                        Host.log("ForceMoveTo Run Entity:  " + obj?.Name + "  Distance: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);
                    if (obj == null)
                    {
                        result = Host.ComeTo(loc, dist, Host.Me.RunSpeed / 5.0);
                        CheckMoveFailed(result, loc);
                    }
                    else
                    {
                        result = Host.ComeTo(obj, dist, Host.Me.RunSpeed / 5.0);

                        CheckMoveFailed(result, obj.Location);
                    }

                    if (_logRun)
                        Host.log("ForceMoveTo End Entity: " + obj?.Name + "  дист: " + Host.Me.Distance(obj) + "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);
                }
            }

            return result;
        }

        public bool ForceMoveTo(double x, double y, double z, double dist)
        {
            return ForceMoveTo(new Vector3F(x, y, z), dist);
        }

        public bool ForceMoveTo2(Vector3F loc, double dist = 1, bool useMount = true, bool useNav = false)
        {
            try
            {
                if (useMount)
                    MySitMount(loc);
                else
                    MyUnmount();

                if (!Host.MainForm.On)
                    return false;

                IsMoveToNow = true;
                var doneDist = Host.Me.RunSpeed / 5.0;
                bool result;
                if (useNav)
                {
                    if (_logRun)
                        Host.log(
                            "ForceMoveTo2 Run Vector3F:  " + loc + "  Distance: " + Host.Me.Distance(loc) +
                            "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);
                    result = Host.ComeTo(loc, dist, doneDist);
                    if (_logRun)
                        Host.log(
                            "ForceMoveTo2 End Vector3F: " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " +
                            dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);
                }
                else
                {
                    if (_logRun)
                        Host.log(
                            "ForceMoveTo2 Run Vector3F:  " + loc + "  Distance: " + Host.Me.Distance(loc) +
                            "   dist: " + dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);
                    result = Host.ForceComeTo(loc, dist, doneDist);
                    if (_logRun)
                        Host.log(
                            "ForceMoveTo2 End Vector3F: " + loc + "  дист: " + Host.Me.Distance(loc) + "   dist: " +
                            dist + Host.IsInsideNavMesh(Host.Me.Location) + "/" + Host.IsInsideNavMesh(loc) + " ", LogLvl.Important);
                }


                CheckMoveFailed(result, loc);
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

        public bool MoveToBadLoc(Vector3F loc)
        {
            if (loc.Distance(1798.27, -4363.27, 102.85) < 20 && Host.Me.Distance(1567.82, -4400.66, 16.20) < 30)
            {
                Host.FlyForm();
                Host.ForceFlyTo(1565.31, -4401.46, 175.71);
                Host.ForceFlyTo(1783.75, -4323.89, 155.91);
                Host.ForceFlyTo(1802.62, -4366.94, 102.61);
            }

            if (Host.Me.Distance(-1130.89, 805.10, 500.08) < 20 && Host.Me.Location.Z > 480 &&
                loc.Distance(-1130.89, 805.10, 500.08) > 30 && Host.AutoQuests.BestQuestId != 46930)
            {
                Host.log("Прыгаю вниз");
                if (!Host.ForceComeTo(-1100.62, 795.14, 497.08))
                    return false;
                if (!Host.ForceComeTo(-1087.30, 765.32, 487.73))
                    return false;
                if (!Host.ForceComeTo(-1046.30, 769.34, 435.33))
                    return false;
                return true;
            }

            // Host.log(loc.Distance(9835.40, 1558.26, 1292.05) + "  ");
            if (Host.Area.Id == 141 && Host.MapID == 1 && Host.Zone.Id == 262 &&
                loc.Distance(9835.40, 1558.26, 1292.05) > 300)
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


            if (Host.Me.Distance(9845.00, 441.00, 1318.00) > 10 && loc.Distance(9845.00, 441.00, 1318.00) < 5 &&
                Host.Me.Location.Z < 1316)
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


    }
}