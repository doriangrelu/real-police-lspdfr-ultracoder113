using Rage;
using RealPolicePlugin.API.Events.AmbientVehicle;
using RealPolicePlugin.Core;
using RealPolicePlugin.OffenceEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Handlers
{
    class AmbientVehicleEventHandler : AbstractRealPoliceEventHandler<AbstractAmbientVehicleEvent>
    {


        private void HandleOffencesEvents()
        {
            AbstractAmbientVehicleEvent offenceEvent = OffencesManager.Instance.GetRandomOffenceEvent();
            if (null != offenceEvent)
            {
                this.OnEventHandler(offenceEvent);
            }
        }


        public override void Handle()
        {
            while (Main.IsAlive)
            {
                try
                {
                    GameFiber.Yield();
                    if (false == FunctionsLSPDFR.IsCalloutRunning())
                    {
                        this.HandleOffencesEvents();
                    }
                    GameFiber.Sleep(3000);

                }
                catch (Exception e)
                {
                    Logger.LogTrivial("START - EXCEPTION");
                    Logger.LogTrivial(e.Message);
                    Logger.LogTrivial(e.StackTrace);
                    Logger.LogTrivial("END - EXCEPTION");
                }
            }
        }

    }
}
