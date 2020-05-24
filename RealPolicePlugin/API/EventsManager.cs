using Rage;
using RealPolicePlugin.API.Events;
using RealPolicePlugin.API.Events.AmbientVehicle;
using RealPolicePlugin.API.Handlers;
using RealPolicePlugin.API.Interfaces;
using System.Collections.Generic;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

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
            ambientVehicleEventHandler.EventHandler += HandleRunAmbientEvent;

            AmbientPedEventHandler ambientPedEventHandler = new AmbientPedEventHandler();
            ambientPedEventHandler.EventHandler += HandleRunAmbientEvent;


            handlers.Add(customPulloverEventHandler);
            handlers.Add(parkingTicketsEventHandler);
            handlers.Add(ambientVehicleEventHandler);
            handlers.Add(ambientPedEventHandler);

            return handlers;
        }

        public static bool CanCreateAnEvent()
        {
            return false == FunctionsLSPDFR.IsCalloutRunning() &&
                null == FunctionsLSPDFR.GetActivePursuit() &&
                false == FunctionsLSPDFR.IsPlayerPerformingPullover() &&
                false == Game.IsPaused;
        }


        public static void HandleAll(List<I_RealPoliceHandler> handlers)
        {
            foreach (I_RealPoliceHandler handler in handlers)
            {
                _Fibers.Add(GameFiber.StartNew(handler.Handle));
            }
        }



        public static void HandleSpeedEvent(object sender, AbstractAmbientVehicleEvent speedEvent)
        {
            Functions.RunAmbientEvent(speedEvent);
        }

        public static void HandleGiveParkingTickets(object sender, GiveParkingTicketEvent giveParkingTicketEvent)
        {
            Functions.GiveParkingTicket(giveParkingTicketEvent);
        }

        public static void HandleRunAmbientEvent(object sender, I_AmbientEvent ambientEvent)
        {
            Functions.RunAmbientEvent(ambientEvent);
        }

        public static void HandleSetCustomPullover(object sender, CustomPulloverEvent customPulloverEvent)
        {
            Functions.SetCustomPulloverLocation(customPulloverEvent);
        }

    }
}
