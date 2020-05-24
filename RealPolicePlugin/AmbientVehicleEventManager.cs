using Rage;
using RealPolicePlugin.API.Events.AmbientVehicle;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;

namespace RealPolicePlugin
{
    class AmbientVehicleEventManager : AbstractAmbientEventManager<AbstractAmbientVehicleEvent, Vehicle>
    {


        public List<string> PlateAlreadyUsed = new List<string>();
        private static AmbientVehicleEventManager instance = null;

        private AmbientVehicleEventManager() : base()
        {
            this.timer.HandleTimer();

        }

        public static AmbientVehicleEventManager Instance
        {
            get
            {
                if (AmbientVehicleEventManager.instance == null)
                {
                    AmbientVehicleEventManager.instance = new AmbientVehicleEventManager();
                }
                return AmbientVehicleEventManager.instance;
            }
        }




        private AbstractAmbientVehicleEvent CreateRandomEvent(Vehicle entity)
        {
            int index = 0;
            int securityCounter = 0;
            Type eventType = null;
            while (eventType == this.lastEvent)
            {
                if (securityCounter >= LOOP_SECURITY)
                {
                    throw new Exception("LOOP¨security violation. Find no More events");
                }
                index = this.random.Next(0, Main.VehicleEvent().Count);
                eventType = Main.VehicleEvent()[index];
                securityCounter++;
            }
            this.lastEvent = eventType;
            Logger.Log("Created event " + eventType);



            return (AbstractAmbientVehicleEvent)Activator.CreateInstance(eventType, new object[] { entity });
        }

        public override AbstractAmbientVehicleEvent GetRandomEvent()
        {
            if (false == this.timer.CanCreateEvent())
            {
                return null;
            }

            if (false == this.CanCreateAnEvent())
            {
                return null;
            }

            if (Tools.HavingChance(6, 10))
            {

                GameFiber.Sleep(3000);
                return null;
            }

            this.Garbage();
            try
            {
                AbstractAmbientVehicleEvent offenceEvent = null;
                int loopSecurity = 3;
                int loopCounter = 0;
                while (null == offenceEvent && loopCounter < loopSecurity)
                {
                    Vehicle[] vehicles = VehicleManager.Instance.GetNearbyVehicles(10);
                    foreach (Vehicle vehicle in vehicles)
                    {
                        if (vehicle.Exists())
                        {
                            if (vehicle.HasDriver &&
                                vehicle.Driver.Exists() &&
                                false == vehicle.IsPoliceVehicle &&
                                false == vehicle.HasSiren &&
                                vehicle.Driver != PedsManager.LocalPlayer() &&
                                false == vehicle.IsBoat &&
                                false == vehicle.IsBike &&
                                false == vehicle.IsBicycle &&
                                false == this.PlateAlreadyUsed.Contains(vehicle.LicensePlate))
                            {
                                this.PlateAlreadyUsed.Add(vehicle.LicensePlate);
                                offenceEvent = this.CreateRandomEvent(vehicle);
                                this.timer.HandleTimer();
                                break;
                            }
                        }
                    }
                    loopSecurity++;
                    GameFiber.Sleep(4000);
                }
                this.eventsRunning.Add(offenceEvent);
                return offenceEvent;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
