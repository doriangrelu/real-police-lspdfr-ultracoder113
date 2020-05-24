using Rage;
using RealPolicePlugin.API.Events.AmbientVehicle;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.API.Events
{
    internal class SpeedVehicleEvent : AbstractAmbientVehicleEvent
    {


        public string TargetModel { get; }

        public Blip Blip { get; }
        public int Speed { get; }
        public int SpeedLimit { get; }

        public SpeedVehicleEvent(Vehicle targetVehicle, string targetModel, int speed, int speedLimit) : base(targetVehicle, "Speed vehicle spoted with speedGun")
        {
            TargetModel = targetModel;
            Speed = speed;
            SpeedLimit = speedLimit;

        }

        public override void OnBeforeStartEvent()
        {
            this.Blip.Color = Color.DarkRed;
            this.Blip.Scale = 0.5F;
            this.Blip.Flash(500, 0);
        }

        public override void OnProcessEvent()
        {
            if (this.IsPulledOverDriver())
            {
                this.IsEventRunning = false;
                this.IsPerformedPullOver = true;
                Game.DisplayNotification("Police Tips: Speed limit violation <br /> <b>" + this.Speed + "</b> (limit " + this.SpeedLimit + ")");
            }
        }
    }
}
