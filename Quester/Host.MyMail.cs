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
            if (!MainForm.On)
                return;
            var path = CommonModule.GpsBase.GetPath(new Vector3F(1610.48, -4419.00, 14.14), Me.Location);

            switch (Me.Team)
            {
                case ETeam.Horde when ClientType == EWoWClient.Classic:
                    CommonModule.MoveTo(1615.27, -4394.44, 10.29);
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
                    CommonModule.MoveTo(-8876.25, 649.99, 96.03);
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
                    mailBox = gameObject;

                if (gameObject.Id == 197135 && gameObject.Distance(-8860.24, 638.56, 96.35) < 10)
                    mailBox = gameObject;

                if (gameObject.Id == 173221)
                    mailBox = gameObject;
                if (gameObject.Id == 144131)
                    mailBox = gameObject;
            }

            if (mailBox != null)
            {
                ComeTo(mailBox, 2);
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
    }
}