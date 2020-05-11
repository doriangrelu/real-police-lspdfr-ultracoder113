using LSPD_First_Response.Mod.API;
using Rage;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.OffenceEvent
{
    class SuspectVehicle : AbstractOffenceEvent
    {

        public const string OFFENCE_MESSAGE = "~o~Suspicious vehicle detected";

        private float OldSpeed = 0.0F;

        public SuspectVehicle(Vehicle vehicle) : base(vehicle, SuspectVehicle.OFFENCE_MESSAGE)
        {

        }
        public override void HandleEvent()
        {

            bool isStolenCar = Tools.HavingChance(1, 10);
            try
            {
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
                    if (this.OldSpeed <= 15F)
                    {
                        newSpeed = 15.5F;
                    }
                    this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, newSpeed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians | VehicleDrivingFlags.AllowWrongWay));
                    this.AddFiber(GameFiber.StartNew(delegate //Add fiber and clean after !
                    {
                        while (this.IsEventRunning)
                        {
                            Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(this.Driver, 786603); //Drunk driving style
                            GameFiber.Yield();
                        }
                    }));
                }
                else
                {
                    if (isStolenCar)
                    {
                        this.Vehicle.IsStolen = true;
                    }
                }

                while (this.IsEventRunning)
                {
                    GameFiber.Yield();
                    if (this.IsPulledOverDriver())
                    {
                        Logger.Log("The driver's behaviour is ~o~supect. You can investigate", true);
                        this.IsPerformedPullOver = true;
                        this.IsEventRunning = false;
                        break;
                    }
                    if (PedsManager.IsAwayFromLocalPlayer(this.Driver.Position))
                    {
                        this.IsEventRunning = false;
                        break;
                    }
                    if (Tools.HavingChance(30, 100) && false == this.RecklessDriving)
                    {
                        this.HandleRecklessDrinving();
                    }
                    this.HandleSafeEventRunning();
                    GameFiber.Sleep(300);
                }
            }
            catch (Exception e)
            {
                Logger.LogTrivial("---------- Exception ----------");
                Logger.LogTrivial(e.Message);

            }
            finally
            {
                this.EndEvent();
            }






        }
    }
}
