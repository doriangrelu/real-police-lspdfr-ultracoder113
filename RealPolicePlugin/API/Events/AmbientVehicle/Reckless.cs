using Rage;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using RealPolicePlugin.API.Events.AmbientVehicle;

namespace RealPolicePlugin.OffenceEvent
{
    class Reckless : AbstractAmbientVehicleEvent
    {

        public const string OFFENCE_MESSAGE = "Reckless driving";

        public Reckless(Vehicle vehicle) : base(vehicle, Reckless.OFFENCE_MESSAGE)
        {

        }
        public override void OnBeforeStartEvent()
        {

        }

        public override void OnProcessEvent()
        {
            this.HandleRecklessDrinving();
            if (this.IsPulledOverDriver())
            {
                Logger.Log("Police tips: ~b~Reckless driving", true);
                this.IsPerformedPullOver = true;
                this.IsEventRunning = false;
                return;
            }
            GameFiber.Yield();
            if (PedsManager.IsAwayFromLocalPlayer(this.Driver.Position))
            {
                this.IsEventRunning = false;
                return;
            }
            this.HandleSafeEventRunning();
            GameFiber.Sleep(300);
        }

    }
}
