using Out.Utility;
using System;
using System.Threading;
using WoWBot.Core;

namespace WowAI
{
    internal partial class Host
    {
        public void MyMail()
        {
            if (!MainForm.On || !CharacterSettings.ApplyMail)
            {
                return;
            }

            var path = CommonModule.GpsBase.GetPath(new Vector3F(1610.48, -4419.00, 14.14), Me.Location);

            switch (Me.Team)
            {
                case ETeam.Horde when ClientType == EWoWClient.Classic:
                    {
                        if (Area.Id == 1497)
                        {
                            CommonModule.MoveTo(1631.17, 220.38, -43.10);
                        }

                        if (Area.Id == 1637)
                        {
                            CommonModule.MoveTo(1615.27, -4394.44, 10.29);
                        }

                        if (Area.Id == 1638)
                        {
                            if (Me.Distance(-1262.08, 46.56, 127.37) > 1)
                            {
                                CommonModule.MoveTo(-1262.08, 46.56, 127.37);
                            }
                        }

                    }

                    break;
                case ETeam.Horde:
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

                        break;
                    }

                case ETeam.Alliance when ClientType == EWoWClient.Classic:
                    {
                        if (Area.Id == 1519)
                            CommonModule.MoveTo(-8876.25, 649.99, 96.03);
                        if (Area.Id == 1537)
                            CommonModule.MoveTo(-4904.30, -970.86, 501.45);
                        if (Area.Id == 440)
                            CommonModule.MoveTo(-7154.40, -3829.52, 8.75, 5);
                        if (Area.Id == 11)
                            CommonModule.MoveTo(-3793.98, -838.58, 9.54, 5);
                        if (Area.Id == 440)
                            CommonModule.MoveTo(-7154.40, -3829.52, 8.75, 5);
                        if (Area.Id == 1377)
                            CommonModule.MoveTo(-6840.31, 734.94, 42.19, 5);
                        if (Area.Id == 15)
                            CommonModule.MoveTo(-3618.42, -4437.94, 13.46, 5);
                        if (Area.Id == 1657)
                            CommonModule.MoveTo(9942.69, 2495.53, 1317.63, 5);

                        if (Area.Id == 47)
                        {
                            CommonModule.MoveTo(293.63, -2115.57, 121.77, 5);
                        }

                    }

                    break;
                case ETeam.Alliance:
                    {
                        path = CommonModule.GpsBase.GetPath(new Vector3F(-8860.24, 638.56, 96.35), Me.Location);
                        foreach (var vector3F in path)
                        {
                            log(path.Count + "  Путь " + Me.Distance(vector3F));
                            CommonModule.ForceMoveTo2(vector3F, 1, false);
                        }

                        break;
                    }

                case ETeam.Other:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            GameObject mailBox = null;
            foreach (var gameObject in GetEntities<GameObject>())
            {
                if (gameObject.Id == 206726 && gameObject.Distance(1607, -4424, 13) < 10)
                {
                    mailBox = gameObject;
                }

                if (gameObject.Id == 197135 && gameObject.Distance(-8860.24, 638.56, 96.35) < 10)
                {
                    mailBox = gameObject;
                }

                if (gameObject.Id == 173221)
                {
                    mailBox = gameObject;
                }

                if (gameObject.Id == 144131)
                {
                    mailBox = gameObject;
                }

                if (gameObject.Id == 143985)
                {
                    mailBox = gameObject;
                }

                if (gameObject.Id == 177044)
                {
                    mailBox = gameObject;
                }
                if (gameObject.Id == 144112)
                {
                    mailBox = gameObject;
                }
                if (gameObject.Id == 142094)
                {
                    mailBox = gameObject;
                }
                if (gameObject.Id == 180451)
                {
                    mailBox = gameObject;
                }
                if (gameObject.Id == 142095)
                {
                    mailBox = gameObject;
                }
                if (gameObject.Id == 171699)
                {
                    mailBox = gameObject;
                }

                if (gameObject.Id == 142110)
                {
                    mailBox = gameObject;
                }
                if (gameObject.Id == 144011)
                {
                    mailBox = gameObject;
                }
                
            }

            if (mailBox != null)
            {
                if (Me.Distance(mailBox) > 1)
                {
                    ComeTo(mailBox, 2);
                }

                MyCheckIsMovingIsCasting();
                Thread.Sleep(1000);
                if (!OpenMailbox(mailBox))
                {
                    log("Не удалось открыть ящик " + GetLastError(), LogLvl.Error);
                }
                else
                {
                    log("Открыл ящик", LogLvl.Ok);
                }

                Thread.Sleep(2000);
                log("Всего писем " + GetMails().Count);
                foreach (var mail in GetMails())
                {
                    log(mail.SenderType + " " + mail.GetAttachedItems().Count + " " + mail.Subject + " ");
                    if (mail.Cod != 0)
                    {
                        continue;
                    }

                    mail.MarkAsRead();
                    Thread.Sleep(500);
                    if (!mail.TakeAllAttachmentsAndGold())
                    {
                        log("Не удалось получить письмо " + GetLastError(), LogLvl.Error);
                    }
                    else
                    {
                        log("Получил письмо", LogLvl.Ok);
                    }

                    if (mail.Subject == "Auction won: WoW Token")
                    {
                        if (!mail.Delete())
                        {
                            log("Не удалось удалить письмо " + GetLastError(), LogLvl.Error);
                        }
                    }
                }
            }
        }
    }
}