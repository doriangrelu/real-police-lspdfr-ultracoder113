using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.API.Events
{
    class CustomPulloverEvent : EventArgs
    {
        public Vehicle pulledVehicle { get; }
        public Ped PulledDriver { get; }

        public CustomPulloverEvent(Vehicle pulledVehicle, Ped pulledDriver)
        {
            this.pulledVehicle = pulledVehicle;
            PulledDriver = pulledDriver;
        }


    }
}
