using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using RealPolicePlugin.Core;
using RealPolicePlugin.GameManager;
using RealPolicePlugin.OffenceEvent;

namespace RealPolicePlugin
{
    public class Main : Plugin
    {


        public const bool IN_PRODUCTION = true; 

        public static List<GameFiber> Fibers = new List<GameFiber>();


        public Main()
        {
        }

        public override void Finally()
        {
        }

        public override void Initialize()
        {
            Logger.Log("Real Police Plugin by ~b~Ultracoder113 ~g~loaded! ", true);
            Functions.OnOnDutyStateChanged += OnDutyStateChanged;
        }


        private void HandleInitialize()
        {
            Logger.Log("~g~On ~g~duty, Real Police Plugin ", true);
            Logger.LogTrivial("Main Trick");
            GameFiber.Wait(6000);
            Logger.Log("Officer on duty ~b~(Real Police by Ultracoder113)", true);
        }



        private void HandleOffencesEvents()
        {
            AbstractOffenceEvent offenceEvent = OffencesManager.Instance.GetRandomOffenceEvent();
            if (null != offenceEvent)
            {
                GameFiber fiber = GameFiber.StartNew(offenceEvent.HandleEvent);
                offenceEvent.MainFiber = fiber; //using after end by garbage
            }
        }


        public void OnDutyStateChanged(bool OnDuty)
        {
            if (OnDuty)
            {
                this.HandleInitialize();

                GameFiber.StartNew(delegate
                {
                    while (true)
                    {
                        GameFiber.Yield();
                        this.HandleOffencesEvents();
                        GameFiber.Sleep(3000);
                        //FiberGarbage.Collect(Main.Fibers);
                    }
                });
            }
        }
    }
}
