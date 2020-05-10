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

        public const string OFFENCE_MESSAGE = "Reckless driving";

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
                while (this.IsEventRunning)
                {
                    this.HandleRecklessDrinving();
                    if (this.IsPulledOverDriver())
                    {
                        Logger.Log("Police tips: ~b~Reckless driving", true);
                        this.IsPerformedPullOver = true;
                        this.IsEventRunning = false;
                        break;
                    }
                    GameFiber.Yield();
                    if (PedsManager.IsAwayFromLocalPlayer(this.Driver.Position))
                    {
                        this.IsEventRunning = false;
                        break;
                    }
                    this.HandleSafeEventRunning();
                    GameFiber.Sleep(300); 
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
