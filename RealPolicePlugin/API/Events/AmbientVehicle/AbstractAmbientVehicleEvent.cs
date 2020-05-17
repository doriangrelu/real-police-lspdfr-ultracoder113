using Rage;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Events.AmbientVehicle
{
    abstract class AbstractAmbientVehicleEvent : EventArgs
    {

        public Ped Driver { get; }
        public Vehicle Vehicle { get; }

        public Blip Blip = null;
        private List<GameFiber> Fibers = new List<GameFiber>();
        protected bool IsPerformedPullOver = false;
        protected bool IsEventRunning { get; set; }
        protected Random random = null;
        public GameFiber MainFiber { get; set; }
        protected bool RecklessDriving = false;



        public AbstractAmbientVehicleEvent(Vehicle vehicle, String offenceMessage)
        {
            this.random = new Random();
            this.Driver = vehicle.Driver;
            this.Vehicle = vehicle;
            this.Driver.IsPersistent = true;
            this.Vehicle.IsPersistent = true;
            this.Driver.BlockPermanentEvents = true;
            this.Blip = this.Driver.AttachBlip();
            this.Blip.Color = System.Drawing.Color.OrangeRed;
            this.Blip.Flash(100, 0);
            this.Blip.Scale = 0.7F;
            uint notification = Logger.DisplayNotification("New event detected: ~r~" + offenceMessage);
            GameFiber.Sleep(6000);
            Game.RemoveNotification(notification);
        }


        public void Prepare()
        {
            this.IsEventRunning = true;
        }

        public bool IsRunning()
        {
            return this.IsEventRunning; 
        }

        abstract public void OnBeforeStartEvent();

        abstract public void OnProcessEvent();


        public virtual void OnEndEvent()
        {
            Logger.LogTrivial("Request Garbage mobile phone event");
            this.IsEventRunning = false;
            if (this.Blip.IsValid() && this.Blip.Exists())
            {
                this.Blip.Delete();
            }
            if (false == FunctionsLSPDFR.IsPlayerPerformingPullover() && false == this.IsPerformedPullOver)
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

            OffencesManager.Instance.HandleEndEventOffence(this);
        }

        /// <summary>
        /// If callout is running delete events for better experience and performances
        /// </summary>
        protected void HandleSafeEventRunning()
        {
            if (FunctionsLSPDFR.IsCalloutRunning())
            {
                this.IsEventRunning = false;
            }
        }
        /// <summary>
        /// Dangerous driving
        /// </summary>
        protected void HandleRecklessDrinving()
        {
            if (this.RecklessDriving)
            {
                return;
            }

            float speed = this.Vehicle.Speed;
            if (VehicleManager.ConvertRageToKMH(speed) <= 50)
            {
                speed = VehicleManager.ConvertKMHToRage((float)this.random.Next(70, 110));
            }
            else
            {
                if (VehicleManager.ConvertRageToKMH(speed) <= 110)
                {
                    speed = VehicleManager.ConvertKMHToRage((float)this.random.Next(110, 150));
                }
            }

            if (false == this.IsEventRunning)
            {
                return;
            }

            this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveLeft);
            GameFiber.Sleep(200);

            if (Tools.HavingChance(7, 10))
            {
                this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
                GameFiber.Sleep(600);
                this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.BurnOut);
                GameFiber.Sleep(200);
                this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
                GameFiber.Sleep(6000);
            }

            if (false == this.IsEventRunning)
            {
                this.Driver.Tasks.ClearSecondary();
                return;
            }

            this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveRight);
            GameFiber.Sleep(300);
            if (false == this.IsEventRunning)
            {
                this.Driver.Tasks.ClearSecondary();
                return;
            }
            this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
            GameFiber.Sleep(6000);
            this.Driver.Tasks.ClearSecondary();
            this.RecklessDriving = true;
        }

        protected bool IsPulledOverDriver()
        {
            if (FunctionsLSPDFR.IsPlayerPerformingPullover())
            {
                Ped currentSuspect = FunctionsLSPDFR.GetPulloverSuspect(FunctionsLSPDFR.GetCurrentPullover());
                return currentSuspect == this.Driver;
            }
            return false;
        }


        protected void HandleAttack(bool withWeapon)
        {
            if (Tools.HavingChance(8, 10))
            {
                this.Driver.BlockPermanentEvents = true;
            }



            if (this.Driver.IsInVehicle(this.Vehicle, true))
            {
                if (Tools.HavingChance(2, 10))
                {
                    this.Driver.Tasks.SmashCarWindow();
                    GameFiber.Wait(600);
                }
                this.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
                this.Driver.Tasks.Clear();
            }

            GameFiber.Wait(600);

            if (withWeapon)
            {
                if (this.Driver.Inventory.Weapons.Count() == 0)
                {
                    this.HandleDriverHaveWeapon();
                }
                GameFiber.Wait(600);
                Rage.Native.NativeFunction.Natives.TaskCombatPed(this.Driver, PedsManager.LocalPlayer(), 0, 16);
            }
            else
            {
                this.Driver.KeepTasks = true;
                this.Driver.Tasks.FightAgainst(PedsManager.LocalPlayer());
            }

        }


        protected void HandleDriverHaveWeapon()
        {
            this.Driver.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MICROSMG"), 500, true);
        }

    }
}
