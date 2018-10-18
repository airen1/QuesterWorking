using Quester.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quester.Modules
{
    internal class QuestModule : Module
    {
        internal List<Quest> questsCanBeRun;
        
        public override void Start(Host host)
        {
            questsCanBeRun = new List<Quest>();
            base.Start(host);

            TurtleLakeQuests.AddQuests(ref questsCanBeRun, host);                                   //1
            CitraconimideethylToTheHebrewsQuests.AddQuests(ref questsCanBeRun, host);               //2
            
        }

        public override void Run()
        {
            try
            {
                while (!host.cancelRequested)
                {
                    base.Run();
                    Thread.Sleep(1000);
                    try
                    {

                        if (host.mainForm.On)
                        { 
                            var qList = questsCanBeRun.ToList();
                            for (int i = 0; i < qList.Count; i++)
                            {
                                if ((!host.farmModule.readyToActions) & (host.mainForm.On))
                                    break;
                                if ((host.commonModule.IsMoveSuspended()) & (host.mainForm.On))
                                    break;
                                if ((host.GameState == BlessBot.Core.EGameState.Ingame) & (host.mainForm.On))
                                {
                                    host.log("Выполняю квест: "+qList[i].Id.ToString());
                                    qList[i].RunQuest(host);
                                }
                                
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        host.log(err.Message.ToString());
                       
                    }
                }
            }
            catch { }
        }
    }
}
