using Rage;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RealPolicePlugin.OffenceEvent
{
    abstract class AbstractOffenceEvent
    {

        public Ped Driver { get; }
        public Vehicle Vehicle { get; }

        public Blip Blip = null;
        private List<GameFiber> Fibers = new List<GameFiber>();
        protected bool IsPerformedPullOver = false;
        protected bool IsEventRunning = true;

        public GameFiber MainFiber { get; set; }


        public AbstractOffenceEvent(Vehicle vehicle, String offenceMessage)
        {
            this.Driver = vehicle.Driver;
            this.Vehicle = vehicle;
            this.Driver.IsPersistent = true;
            this.Vehicle.IsPersistent = true;
            this.Driver.BlockPermanentEvents = true;
            this.Blip = this.Driver.AttachBlip();
            this.Blip.Color = System.Drawing.Color.OrangeRed;
            this.Blip.Flash(1, 0);
            this.Blip.Scale = 0.7F;
            Logger.Log("New event spoted: ~r~" + offenceMessage, true);

        }

        abstract public void HandleEvent();

        protected virtual void EndEvent()
        {
            Logger.LogTrivial("Request Garbage mobile phone event");
            this.IsEventRunning = false;
            if (this.Blip.IsValid() && this.Blip.Exists())
            {
                this.Blip.Delete();
            }
            if (false == Functions.IsPlayerPerformingPullover() && false == this.IsPerformedPullOver)
            {
                bool isPedNotInPursuit = false == PedsManager.isPedInPursuit(this.Driver);
                if (this.Driver.Exists() && isPedNotInPursuit)
                {
                    this.Driver.Dismiss();
                }

                if (this.Vehicle.Exists() && isPedNotInPursuit)
                {
                    this.Vehicle.Dismiss();
                }
                Logger.LogTrivial("Ending Garbage mobile phone event");
            }
            this.CleanFibers();
            OffencesManager.Instance.HandleEndEventOffence(this);
        }


        protected void HandleRecklessDrinving()
        {
            this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveLeft);
            if (GameFiber.CanSleepNow)
            {
                GameFiber.Sleep(200);
            }
            this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveRight);
            if (GameFiber.CanSleepNow)
            {
                GameFiber.Sleep(300);
            }
            this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, this.Vehicle.Speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
            if (GameFiber.CanSleepNow)
            {
                GameFiber.Sleep(6000);
            }
        }


        protected void AddFiber(GameFiber fiber)
        {
            this.Fibers.Add(fiber);
        }

        /// <summary>
        /// Clean every sub fibers
        /// </summary>
        private void CleanFibers()
        {
            FiberGarbage.Collect(this.Fibers, true); // Kill All alive fibers 
            if (null != this.MainFiber)
            {
                //Main.Fibers.Add(this.MainFiber);
            }

        }
    }
}
