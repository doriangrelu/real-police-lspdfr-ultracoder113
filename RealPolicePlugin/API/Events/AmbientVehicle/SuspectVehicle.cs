using Rage;
using RealPolicePlugin.Core;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Events.AmbientVehicle
{
    class SuspectVehicle : AbstractAmbientVehicleEvent
    {

        public const string OFFENCE_MESSAGE = "~o~Suspicious vehicle detected";

        private float OldSpeed = 0.0F;

        public SuspectVehicle(Vehicle entity) : base(entity, SuspectVehicle.OFFENCE_MESSAGE)
        {
        }


        public override void OnBeforeStartEvent()
        {
            bool isStolenCar = Tools.HavingChance(1, 10);
            this.Vehicle.IsPersistent = true;
            this.Driver.BlockPermanentEvents = true;
            this.Driver.IsPersistent = true;

            if (false == isStolenCar && Tools.HavingChance(1, 10))
            {
                AnimationSet drunkAnimation = new AnimationSet("move_m@drunk@verydrunk");
                drunkAnimation.LoadAndWait();
                this.Driver.MovementAnimationSet = drunkAnimation;
                Rage.Native.NativeFunction.Natives.SET_PED_IS_DRUNK(this.Driver, true);
                this.OldSpeed = this.Vehicle.Speed;
                float newSpeed = this.OldSpeed - 4;
                if (this.OldSpeed <= 20F)
                {
                    newSpeed = 70F;
                }
                else
                {
                    if (this.OldSpeed <= 60)
                    {
                        newSpeed = 5F;
                    }
                }
                this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, newSpeed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians | VehicleDrivingFlags.AllowWrongWay));
                GameFiber.StartNew(delegate //Add fiber and clean after !
                {
                    while (this.IsEventRunning)
                    {
                        Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(this.Driver, 786603); //Drunk driving style
                        GameFiber.Yield();
                    }
                });
            }
            else
            {
                if (isStolenCar)
                {
                    this.Vehicle.IsStolen = true;
                }
            }
        }

        public override void OnProcessEvent()
        {
            GameFiber.Yield();
            if (this.IsPulledOverDriver())
            {
                if (Tools.HavingChance(5, 10))
                {
                    this.Driver.CanAttackFriendlies = true;
                }
                Logger.Log("The driver's behaviour is ~o~supect. You can investigate", true);
                FunctionsLSPDFR.PlayScannerAudioUsingPosition("INTRO_01 OFFICERS_REPORT_02 SUSPICIOUS PERSON IN_OR_ON_POSITION OUTRO_03 NOISE_SHORT CODE4_ADAM PROCEED_WITH_PATROL NOISE_SHORT OUTRO_02", PedsManager.LocalPlayer().Position);
                this.IsPerformedPullOver = true;
                this.IsEventRunning = false;
                return;
            }
            if (PedsManager.IsAwayFromLocalPlayer(this.Driver.Position))
            {
                this.IsEventRunning = false;
                return;
            }
            if (Tools.HavingChance(30, 100) && false == this.RecklessDriving)
            {
                this.HandleRecklessDrinving();
            }
            this.HandleSafeEventRunning();
            GameFiber.Sleep(300);
        }
    }
}
