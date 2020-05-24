using Rage;
using RealPolicePlugin.Core;

namespace RealPolicePlugin.API.Events.AmbientVehicle
{
    class MobilePhone : AbstractAmbientVehicleEvent
    {

        public const string OFFENCE_MESSAGE = "Driver using phone";


        public MobilePhone(Vehicle entity) : base(entity, MobilePhone.OFFENCE_MESSAGE)
        {
        }

        public override void OnBeforeStartEvent()
        {
            Logger.LogTrivial("Start Mobile phone fiber SLEEP");
            GameFiber.Sleep(200);
            Logger.LogTrivial("Run Mobile phone logic");
            this.Driver.Tasks.ClearImmediately();
            this.Driver.WarpIntoVehicle(this.Vehicle, -1);
            this.Driver.KeepTasks = true;

            Logger.LogTrivial("Prepare driver phone tasks");

            Rage.Native.NativeFunction.Natives.TASK_USE_MOBILE_PHONE_TIMED(this.Driver, 100000);

            Logger.LogTrivial("Set driver using phone");

            this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, 12F, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.AllowWrongWay);

            Logger.LogTrivial("Set driver is bad driver");
        }

        public override void OnProcessEvent()
        {
            GameFiber.Sleep(200);
            if (this.IsPulledOverDriver())
            {
                Logger.Log("Police tips: ~r~" + MobilePhone.OFFENCE_MESSAGE, true);
                this.IsEventRunning = false;
                this.IsPerformedPullOver = true;
                this.Driver.Tasks.ClearSecondary();
                return;
            }
            if (PedsManager.IsAwayFromLocalPlayer(this.Driver.Position))
            {
                this.IsEventRunning = false;
                return;
            }
            this.HandleSafeEventRunning();
        }

        public override void OnEndEvent()
        {
            Logger.LogTrivial("Requesting Local Garbage mobile phone event");
            if (this.Driver.Exists())
            {
                this.Driver.KeepTasks = false;
                if (Rage.Native.NativeFunction.Natives.IS_PED_RUNNING_MOBILE_PHONE_TASK<bool>(this.Driver))
                {
                    this.Driver.Tasks.ClearSecondary();
                }
            }
            base.OnEndEvent();
        }
    }
}
