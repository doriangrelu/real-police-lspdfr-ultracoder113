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
    class Reckless : AbstractOffenceEvent
    {

        public const string OFFENCE_MESSAGE = "Reckless driving~r~(reckless)";

        private float OldSpeed;
        private float OldDriveInertia;
        private float OldInitialDriveForce;

        public Reckless(Vehicle vehicle) : base(vehicle, Reckless.OFFENCE_MESSAGE)
        {

        }

        public override void HandleEvent()
        {

            try
            {

                this.Vehicle.Mods.InstallModKit();

                if ((this.Vehicle.Mods.EngineModCount - 2) >= 0)
                {
                    this.Vehicle.Mods.EngineModIndex = this.Vehicle.Mods.EngineModCount - 2;
                }

                if ((this.Vehicle.Mods.ExhaustModCount - 2) >= 0)
                {
                    this.Vehicle.Mods.ExhaustModIndex = this.Vehicle.Mods.ExhaustModCount - 2;
                }
                if ((this.Vehicle.Mods.TransmissionModCount - 2) >= 0)
                {
                    this.Vehicle.Mods.TransmissionModIndex = this.Vehicle.Mods.TransmissionModCount - 2;
                }

                this.Vehicle.Mods.HasTurbo = true;
                this.OldSpeed = this.Vehicle.Speed;
                this.OldDriveInertia = this.Vehicle.HandlingData.DriveInertia;
                this.OldInitialDriveForce = this.Vehicle.HandlingData.InitialDriveForce;


                float newSpeed = 0.0F;

                if (this.OldSpeed >= 30F)
                {
                    newSpeed = 60F;
                    if (this.Vehicle.TopSpeed < 60F)
                    {
                        this.Vehicle.TopSpeed = 60F;
                    }
                }
                else
                {
                    newSpeed = this.OldSpeed * 1.9F; // add 90 percent
                    if (newSpeed < 20f) { newSpeed = 50F; }
                    if (newSpeed > 40f) { newSpeed = 70F; }

                    this.Vehicle.HandlingData.DriveInertia = this.OldDriveInertia * 1.7f;
                    this.Vehicle.HandlingData.InitialDriveForce = this.OldInitialDriveForce * 1.7f;

                    this.Driver.Tasks.ClearImmediately();
                    this.Driver.WarpIntoVehicle(this.Vehicle, -1);
                    this.Driver.KeepTasks = true;

                    this.Driver.Tasks.CruiseWithVehicle(this.Vehicle, newSpeed, Rage.VehicleDrivingFlags.FollowTraffic | Rage.VehicleDrivingFlags.YieldToCrossingPedestrians | Rage.VehicleDrivingFlags.AllowWrongWay);


                    this.AddFiber(GameFiber.StartNew(delegate
                    {
                        while (this.IsEventRunning)
                        {
                            GameFiber.Yield();
                            Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(this.Driver, 786603);
                        }
                    })); 

                    while (this.IsEventRunning)
                    {
                        GameFiber.Yield();
                        Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(this.Driver, 786603);
                        if (Functions.IsPlayerPerformingPullover())
                        {
                            Logger.Log("Police tips: ~b~Reckless driving", true); 
                            this.IsPerformedPullOver = true;
                            this.IsEventRunning = false;
                            break;
                        }
                        if (PedsManager.IsAwayFromLocalPlayer(this.Driver.Position))
                        {
                            this.IsEventRunning = false;
                            break;
                        }
                        if (Tools.HavingChance(3, 4))
                        {
                            this.HandleRecklessDrinving();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("---------- Exception ----------");
                Logger.Log(e.Message);
            }
            finally
            {
                this.EndEvent();
            }

        }


        protected override void EndEvent()
        {
            if (this.Vehicle.Exists())
            {
                this.Vehicle.HandlingData.DriveInertia = this.OldDriveInertia;
                this.Vehicle.HandlingData.InitialDriveForce = this.OldInitialDriveForce;
            }
            base.EndEvent();
        }


    }
}
