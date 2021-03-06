﻿using Rage;
using RealPolicePlugin.API.Events;
using RealPolicePlugin.API.Events.AmbientVehicle;
using RealPolicePlugin.API.Handlers;
using RealPolicePlugin.API.Interfaces;
using RealPolicePlugin.OffenceEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.API
{
    class EventsManager
    {


        private static List<GameFiber> _Fibers = new List<GameFiber>();



        public static void ForceEndFibers()
        {
            foreach (GameFiber fiber in _Fibers)
            {
                if (fiber.IsAlive)
                {
                    fiber.Abort();
                }
                _Fibers.Remove(fiber); 
            }
        }

        public void AddFiber(GameFiber fiber)
        {
            _Fibers.Add(fiber);
        }


        public static List<I_RealPoliceHandler> InitializeSubscriber()
        {

            List<I_RealPoliceHandler> handlers = new List<I_RealPoliceHandler>();

            CustomPulloverEventHandler customPulloverEventHandler = new CustomPulloverEventHandler();
            customPulloverEventHandler.EventHandler += HandleSetCustomPullover;

            ParkingTicketsEventHandler parkingTicketsEventHandler = new ParkingTicketsEventHandler();
            parkingTicketsEventHandler.EventHandler += HandleGiveParkingTickets;

            AmbientVehicleEventHandler ambientVehicleEventHandler = new AmbientVehicleEventHandler();
            ambientVehicleEventHandler.EventHandler += HandleRunAmbientVehicleEvent;

            handlers.Add(customPulloverEventHandler);
            handlers.Add(parkingTicketsEventHandler);
            handlers.Add(ambientVehicleEventHandler);

            return handlers;
        }


        public static void HandleAll(List<I_RealPoliceHandler> handlers)
        {
            foreach (I_RealPoliceHandler handler in handlers)
            {
                _Fibers.Add(GameFiber.StartNew(handler.Handle));
            }
        }

        public static void HandleGiveParkingTickets(object sender, GiveParkingTicketEvent giveParkingTicketEvent)
        {
            Functions.GiveParkingTicket(giveParkingTicketEvent);
        }

        public static void HandleRunAmbientVehicleEvent(object sender, AbstractAmbientVehicleEvent offenceEvent)
        {
            Functions.RunAmbientVehicleEvent(offenceEvent);
        }

        public static void HandleSetCustomPullover(object sender, CustomPulloverEvent customPulloverEvent)
        {
            Functions.SetCustomPulloverLocation(customPulloverEvent);
        }

    }
}
