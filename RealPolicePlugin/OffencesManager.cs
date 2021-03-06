﻿using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using RealPolicePlugin.OffenceEvent;
using System.Collections.ObjectModel;
using RealPolicePlugin.Core;
using System.Runtime.CompilerServices;
using RealPolicePlugin.API.Events.AmbientVehicle;

namespace RealPolicePlugin
{
    class OffencesManager
    {
        private const int LOOP_SECURITY = 20;
        private List<AbstractAmbientVehicleEvent> CurrentsEvents = new List<AbstractAmbientVehicleEvent>();
        private static List<Type> OffencesRegistred = new List<Type>();
        private const int C_MAX_CURRENT_EVENT = 2;
        private Type LastOffenceCreated = null;
        private Random Random;


        private List<string> PlateAlreadyUsed = new List<string>();

        private StopTimer Timer = null;

        /// <summary>
        /// Singleton
        /// </summary>
        private static OffencesManager _Instance = null;


        private OffencesManager()
        {
            this.Timer = new StopTimer();
            this.Timer.HandleTimer();
            this.Random = new Random();
            OffencesManager.RegisterNewOffence(typeof(MobilePhone));
            OffencesManager.RegisterNewOffence(typeof(Reckless));
            OffencesManager.RegisterNewOffence(typeof(SuspectVehicle));
        }



        public static OffencesManager Instance
        {
            get
            {
                if (OffencesManager._Instance == null)
                {
                    OffencesManager._Instance = new OffencesManager();
                }
                return OffencesManager._Instance;
            }
        }


        public static void RegisterNewOffence(Type offenceClassType)
        {
            if (false == OffencesManager.OffencesRegistred.Contains(offenceClassType))
            {
                OffencesManager.OffencesRegistred.Add(offenceClassType);
            }
        }


        private AbstractAmbientVehicleEvent GenerateRandomOffenceInstance(Vehicle vehicle)
        {
            int index = 0;
            int securityCounter = 0;
            Type offenceClassType = null;
            while (offenceClassType == this.LastOffenceCreated)
            {
                if (securityCounter >= OffencesManager.LOOP_SECURITY)
                {
                    throw new Exception("LOOP¨security violation. Find no More events");
                }
                index = this.Random.Next(0, OffencesManager.OffencesRegistred.Count);
                offenceClassType = OffencesManager.OffencesRegistred[index];
                securityCounter++;
            }
            this.LastOffenceCreated = offenceClassType;
            return (AbstractAmbientVehicleEvent)Activator.CreateInstance(offenceClassType, new object[] { vehicle });
        }




        public bool isAllEventsCreated()
        {
            return this.CurrentsEvents.Count >= OffencesManager.C_MAX_CURRENT_EVENT;
        }

        public void HandleEndEventOffence(AbstractAmbientVehicleEvent offenceEvent)
        {
            if (this.CurrentsEvents.Contains(offenceEvent))
            {
                this.CurrentsEvents.Remove(offenceEvent); //Garbage collector clean instance (break references) 
            }
        }

        private bool CanCreateAnEvent()
        {
            return false == this.isAllEventsCreated() &&
                false == Functions.IsCalloutRunning() &&
                null == Functions.GetActivePursuit() &&
                false == Functions.IsPlayerPerformingPullover() &&
                false == Game.IsPaused;
        }

        private void HandleCleanUsedVehicles()
        {
            if (this.PlateAlreadyUsed.Count > 15)
            {
                this.PlateAlreadyUsed.Clear();
            }
        }

        public AbstractAmbientVehicleEvent GetRandomOffenceEvent()
        {

            if (false == this.Timer.CanCreateEvent())
            {
                Logger.LogTrivial("Can't create event: TIMER");
                return null;
            }

            if (false == this.CanCreateAnEvent())
            {
                Logger.LogTrivial("Can't create event: To many | Callout running | In pursuit | Is performing pullover");
                return null;
            }

            if (Tools.HavingChance(10, 100))
            {
                Logger.LogTrivial("Can't create event: Bad Chance");
                GameFiber.Sleep(3000);
                return null;
            }

            this.HandleCleanUsedVehicles();
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
                            offenceEvent = this.GenerateRandomOffenceInstance(vehicle);
                            this.Timer.HandleTimer();
                            break; //Bad but have no choice in Foreach ;) 
                        }
                    }
                }
                loopSecurity++;
                GameFiber.Sleep(4000);
            }
            this.CurrentsEvents.Add(offenceEvent);
            return offenceEvent;
        }
    }
}
