using Out.Internal.Core;
using System.Threading;
using WoWBot.Core;

namespace WowAI
{
    internal partial class Host
    {
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
                MySendKeyEsc();
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                {
                    log("Не смог открыть диалог 2 " + GetLastError(), LogLvl.Error);
                    return false;
                }
            }

            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                log(" " + gossipOptionsData.OptionNPC + " " + gossipOptionsData.Text + " " +
                    gossipOptionsData.ClientOption);
                if (gossipOptionsData.OptionNPC == EGossipOptionIcon.Chat)
                {
                    log("Выбираю диалог");
                    if (!SelectNpcDialog(gossipOptionsData))
                    {
                        log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                    }

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
                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "  " + "   " + text);
                }

                log("Надо убрать этот код");
                return false;
            }

            var result = false;
            ForceMoveTo(npc.Location);
            Thread.Sleep(1000);
            if (!OpenDialog(npc))
            {
                log("Не смог открыть диалог " + GetLastError(), LogLvl.Error);
            }

            if (GetNpcDialogs().Count == 0)
            {
                MySendKeyEsc();
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                {
                    log("Не смог открыть диалог 2 " + GetLastError(), LogLvl.Error);
                }
            }


            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                if (gossipOptionsData.Text.Contains("Подгород") || gossipOptionsData.Text.Contains(text))
                {
                    if (!SelectNpcDialog(gossipOptionsData))
                    {
                        log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                    }

                    result = true;
                }

                log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                    gossipOptionsData.ClientOption + "  " + "   " + text);
            }

            return result;
        }

        public bool MyDialog(Entity npc, int index)
        {
            var result = false;
            ForceMoveTo(npc.Location);
            Thread.Sleep(1000);
            if (!OpenDialog(npc))
            {
                log("Не смог открыть диалог " + GetLastError(), LogLvl.Error);
            }

            if (GetNpcDialogs().Count == 0)
            {
                MySendKeyEsc();
                Thread.Sleep(1000);
                if (!OpenDialog(npc))
                {
                    log("Не смог открыть диалог 2 " + GetLastError(), LogLvl.Error);
                }
            }

            foreach (var gossipOptionsData in GetNpcDialogs())
            {
                if (gossipOptionsData.ClientOption == index)
                {
                    log("Выбираю диалог");
                    if (!SelectNpcDialog(gossipOptionsData))
                    {
                        log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                    }

                    result = true;
                }

                log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                    gossipOptionsData.ClientOption + "  " + "   " + gossipOptionsData.OptionNPC);
            }

            if (npc.Id == 127128 || npc.Id == 130905 || npc.Id == 130929 || npc.Id == 131135 || npc.Id == 137613 || npc.Id == 11956)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == index)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                        {
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        }

                        result = true;
                    }

                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "   " + gossipOptionsData.OptionNPC);
                }
            }

            if (npc.Id == 130905 || npc.Id == 130929|| npc.Id == 11956)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == index)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                        {
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        }

                        result = true;
                    }

                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "   " + gossipOptionsData.OptionNPC);
                }
            }

            if (npc.Id == 281536|| npc.Id == 11956)
            {
                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 4)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                        {
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        }

                        result = true;
                    }

                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "  " + "   ");
                }

                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 0)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                        {
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        }

                        result = true;
                    }

                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "  " + "   ");
                }

                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 3)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                        {
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        }

                        result = true;
                    }

                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "  " + "   ");
                }

                Thread.Sleep(1000);
                foreach (var gossipOptionsData in GetNpcDialogs())
                {
                    if (gossipOptionsData.ClientOption == 1)
                    {
                        if (!SelectNpcDialog(gossipOptionsData))
                        {
                            log("Не смог выбрать диалог " + GetLastError(), LogLvl.Error);
                        }

                        result = true;
                    }

                    log(" " + gossipOptionsData.Confirm + " " + gossipOptionsData.Text + " " +
                        gossipOptionsData.ClientOption + "  " + "   ");
                }
            }

            return result;
        }

        public bool MyOpenDialog(Entity npc)
        {
            if (npc.Guid == CurrentInteractionGuid)
            {
                log("Диалог уже открыт " + GetNpcQuestDialogs().Count + "  " + _fixBadDialog + "/15", LogLvl.Ok);
                if (GetNpcQuestDialogs().Count == 0)
                {
                    _fixBadDialog++;
                    if (_fixBadDialog >= 15)
                    {
                        log("Перезапускаю окно, нет диалогов");
                        TerminateGameClient();
                        return false;
                    }
                }
            }
            else
            {
                _fixBadDialog = 0;
                Thread.Sleep(500);
                if (!OpenDialog(npc))
                {
                    log("Не смог начать диалог для начала квеста с " + npc.Name + " " + GetLastError(), LogLvl.Error);
                    log("npc.Guid: " + npc.Guid);
                    log("CurrentInteractionGuid:" + CurrentInteractionGuid);
                    CommonModule.MyUnmount();
                    foreach (var entity in GetEntities())
                    {
                        if (entity.Guid != CurrentInteractionGuid)
                        {
                            continue;
                        }

                        log("Имя: " + entity.Name);
                        CommonModule.MoveTo(entity, 1);
                    }

                    if (GetLastError() == ELastError.TooFarDistance)
                    {
                        CommonModule.MoveTo(npc, 0);
                    }
                    MySendKeyEsc();
                    Thread.Sleep(500);
                    return false;
                }
            }
            Thread.Sleep(500);
            return true;
        }
    }
}