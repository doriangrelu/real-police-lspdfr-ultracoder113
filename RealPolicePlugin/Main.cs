using LSPD_First_Response.Mod.API;
using Rage;
using RealPolicePlugin.API;
using RealPolicePlugin.API.Events.AmbientPed;
using RealPolicePlugin.API.Events.AmbientVehicle;
using RealPolicePlugin.API.Interfaces;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin
{
    public class Main : Plugin
    {


        public static bool IsAlive = true;


        public const bool IN_PRODUCTION = false;

        public static List<GameFiber> Fibers = new List<GameFiber>();


        private static List<Type> PedestrianEvent = new List<Type>();
        private static List<Type> VehiclesEvent = new List<Type>();





        public Main()
        {
        }

        public static List<Type> PedestriantEvent()
        {
            return PedestrianEvent;
        }

        public static List<Type> VehicleEvent()
        {
            return VehiclesEvent;
        }

        public static void RegisterPedestriantEvent(Type pedestrianEvent)
        {
            PedestrianEvent.Add(pedestrianEvent);
        }


        public static void RegisterVehicleEvent(Type vehicleEvent)
        {
            VehiclesEvent.Add(vehicleEvent);
        }

        public override void Finally()
        {
            IsAlive = false;
            EventsManager.ForceEndFibers();
        }

        public override void Initialize()
        {

            FunctionsLSPDFR.OnOnDutyStateChanged += OnDutyStateChanged;


            RegisterVehicleEvent(typeof(Reckless));
            RegisterVehicleEvent(typeof(Speeder));
            RegisterVehicleEvent(typeof(MobilePhone));
            RegisterVehicleEvent(typeof(SuspectVehicle));


            RegisterPedestriantEvent(typeof(Fight));
            RegisterPedestriantEvent(typeof(PublicIntoxication));
            RegisterPedestriantEvent(typeof(SuspectPed));

        }


        public void OnDutyStateChanged(bool OnDuty)
        {
            if (OnDuty)
            {

                Logger.Log("Real Police Plugin by ~b~Ultracoder113 ~o~loaded ! ~r~ On Duty !", true);
                List<I_RealPoliceHandler> handlers = EventsManager.InitializeSubscriber();
                EventsManager.HandleAll(handlers);
            }

        }
    }
}
