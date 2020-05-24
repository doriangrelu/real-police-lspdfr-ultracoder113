using Rage;
using RealPolicePlugin.Core;

namespace RealPolicePlugin.API.Events.AmbientVehicle
{
    class Reckless : AbstractAmbientVehicleEvent
    {

        public const string OFFENCE_MESSAGE = "Reckless driving";

        public Reckless(Vehicle entity) : base(entity, Reckless.OFFENCE_MESSAGE)
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
