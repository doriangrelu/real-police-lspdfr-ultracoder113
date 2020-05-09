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
            this.Blip.Flash(100, 0);
            this.Blip.Scale = 0.7F;
            uint notification = Logger.DisplayNotification("New event detected: ~r~" + offenceMessage);
            GameFiber.Sleep(6000);
            Game.RemoveNotification(notification);
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
            GameFiber.Sleep(200);
            this.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveRight);
            GameFiber.Sleep(300);
            this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, this.Vehicle.Speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
            GameFiber.Sleep(6000);
        }


        protected void HandleAttack(bool withWeapon)
        {

            if (Rage.Native.NativeFunction.Natives.S_PED_SPRINTING<bool>(this.Driver))
            {
                this.Driver.Tasks.ClearImmediately();
            }
            this.Driver.BlockPermanentEvents = true;
            if (this.Driver.IsInVehicle(this.Vehicle, true))
            {
                this.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(3000);
            }


            if (Rage.Native.NativeFunction.Natives.S_PED_SPRINTING<bool>(this.Driver))
            {
                this.Driver.Tasks.ClearImmediately();
            }

            if (withWeapon)
            {
                if (this.Driver.Inventory.Weapons.Count() == 0)
                {
                    this.HandleDriverHaveWeapon();
                }
                Rage.Native.NativeFunction.Natives.TaskCombatPed(this.Driver, PedsManager.LocalPlayer(), 0, 16);
            }
            else
            {
                this.Driver.Tasks.FightAgainst(PedsManager.LocalPlayer(), 3000);
            }
        }


        protected void HandleDriverHaveWeapon()
        {
            this.Driver.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MICROSMG"), 500, true);
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
