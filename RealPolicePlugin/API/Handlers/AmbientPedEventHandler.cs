using Rage;
using RealPolicePlugin.API.Events.AmbientPed;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Handlers
{
    class AmbientPedEventHandler : AbstractRealPoliceEventHandler<AbstractAmbientPedEvent>
    {

        private void HandleEvent()
        {
            AbstractAmbientPedEvent offenceEvent = AmbientPedEventManager.Instance.GetRandomEvent();
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
                        this.HandleEvent();
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
