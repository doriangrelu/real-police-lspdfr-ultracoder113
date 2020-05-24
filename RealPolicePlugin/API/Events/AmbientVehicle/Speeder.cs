using LSPD_First_Response.Mod.API;
using Rage;
using RealPolicePlugin.Core;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Events.AmbientVehicle
{
    class Speeder : AbstractAmbientVehicleEvent
    {

        private const string MESSAGE = "Speeder driver";

        public Speeder(Vehicle entity) : base(entity, MESSAGE)
        {

        }
        public override void OnBeforeStartEvent()
        {
            float actualSpeed = this.Vehicle.Speed;

            if (this.Vehicle.TopSpeed < VehicleManager.ConvertKMHToRage(150))
            {
                this.Vehicle.TopSpeed = VehicleManager.ConvertKMHToRage(155);
            }

            VehicleManager.InstallCarMod(this.Vehicle);

            this.Driver.Tasks.ClearImmediately();
            this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, GetRandomHightSpeed(), VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians);
            this.Driver.KeepTasks = true;

        }

        public static float GetRandomHightSpeed()
        {
            if (Tools.HavingChance(5, 10))
            {
                return 150.00F;
            }
            if (Tools.HavingChance(5, 10))
            {
                return 130.00F;
            }

            return 110.00F;
        }

        public override void OnProcessEvent()
        {
            Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(this.Driver, 786603);
            if (this.IsPulledOverDriver())
            {
                Logger.Log("Police tips: ~b~Speeder driving (~)", true);

                if (Tools.HavingChance(5, 15))
                {
                    LHandle pursuit = FunctionsLSPDFR.CreatePursuit();
                    FunctionsLSPDFR.AddPedToPursuit(pursuit, this.Driver);
                    this.hardClean = false;
                    FunctionsLSPDFR.SetPursuitIsActiveForPlayer(pursuit, true);
                    Functions.Dispatch(false);
                }


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
        }
    }
}
