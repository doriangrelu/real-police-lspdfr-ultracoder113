using Rage;
using RealPolicePlugin.API.Events.AmbientPed;
using RealPolicePlugin.Core;
using System;

namespace RealPolicePlugin
{
    class AmbientPedEventManager : AbstractAmbientEventManager<AbstractAmbientPedEvent, Ped>
    {

        private static AmbientPedEventManager instance = null;

        private AmbientPedEventManager() : base()
        {
            this.timer.HandleTimer();
        }

        public static AmbientPedEventManager Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new AmbientPedEventManager();
                }
                return instance;
            }
        }



        private AbstractAmbientPedEvent CreateRandomEvent(Ped entity)
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
                index = this.random.Next(0, Main.PedestriantEvent().Count);
                eventType = Main.PedestriantEvent()[index];
                securityCounter++;
            }
            this.lastEvent = eventType;
            return (AbstractAmbientPedEvent)Activator.CreateInstance(eventType, new object[] { entity });

        }

        override protected bool IsAllEventsCreated()
        {
            return this.eventsRunning.Count >= 1;
        }


        override public AbstractAmbientPedEvent GetRandomEvent()
        {
            if (false == this.timer.CanCreateEvent())
            {
                Logger.LogTrivial("Pedestrian event: Can't create event: TIMER");
                return null;
            }

            if (false == this.CanCreateAnEvent())
            {
                Logger.LogTrivial("Pedestrian event: Can't create event: To many | Callout running | In pursuit | Is performing pullover | Is Paused");
                return null;
            }

            if (false == Tools.HavingChance(7, 10))
            {
                Logger.LogTrivial("Pedestrian event: Bad chance");
                return null;
            }

            this.Garbage();
            AbstractAmbientPedEvent pedEvent = null;
            int loopSecurity = 3;
            int loopCounter = 0;
            while (null == pedEvent && loopCounter < loopSecurity)
            {
                Ped[] peds = PedsManager.LocalPlayer().GetNearbyPeds(10);
                foreach (Ped ped in peds)
                {
                    //if (ped.Exists() && PedsManager.LocalPlayer() != ped && false == FunctionsLSPDFR.IsPedArrested(ped) && false == ped.IsDead && false == FunctionsLSPDFR.IsPedACop(ped) && PedsManager.IsNearby(PedsManager.LocalPlayer().Position, ped.Position, 350F))
                    if (AbstractAmbientPedEvent.IsPedEligible(PedsManager.LocalPlayer(), ped, 250F))
                    {
                        pedEvent = this.CreateRandomEvent(ped);
                        Logger.Log("Try launch event");
                        this.timer.HandleTimer();
                        break;
                    }
                }
                loopSecurity++;
                GameFiber.Sleep(4000);
            }
            this.eventsRunning.Add(pedEvent);
            return pedEvent;
        }

    }
}
