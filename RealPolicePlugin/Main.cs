using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using RealPolicePlugin.Core;
using RealPolicePlugin.OffenceEvent;
using RealPolicePlugin.API.Interfaces;
using RealPolicePlugin.API;
using LSPD_First_Response.Mod.API;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin
{
    public class Main : Plugin
    {


        public static bool IsAlive = true;


        public const bool IN_PRODUCTION = false;

        public static List<GameFiber> Fibers = new List<GameFiber>();


        public Main()
        {
        }

        public override void Finally()
        {
            IsAlive = false;
            EventsManager.ForceEndFibers();
        }

        public override void Initialize()
        {

            FunctionsLSPDFR.OnOnDutyStateChanged += OnDutyStateChanged;
        }


        public void OnDutyStateChanged(bool OnDuty)
        {
            if (OnDuty)
            {
                Logger.Log("Real Police Plugin by ~b~Ultracoder113 ~o~loaded ! ~r~ On Duty !", true);
                List<I_RealPoliceHandler> handlers = EventsManager.InitializeSubscriber();
                EventsManager.HandleAll(handlers);
            }

        }
    }
}
