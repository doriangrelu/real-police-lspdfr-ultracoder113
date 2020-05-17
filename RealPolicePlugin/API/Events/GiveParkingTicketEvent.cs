using Rage;
using RealPolicePlugin.API.Type;
using System;
using System.Collections.Generic;

namespace RealPolicePlugin.API.Events
{
    public class GiveParkingTicketEvent : EventArgs
    {


        public Vehicle Vehicle { get; }
        public ParkingTicketsOffences OffenceType { get; }

        public GiveParkingTicketEvent(Vehicle vehicle, ParkingTicketsOffences offenceType)
        {
            Vehicle = vehicle;
            OffenceType = offenceType;
        }


        public int Amount
        {
            get
            {
                return this.OffenceType == ParkingTicketsOffences.DANGEROUS ? 475 : 275;
            }
        }


    }
}
