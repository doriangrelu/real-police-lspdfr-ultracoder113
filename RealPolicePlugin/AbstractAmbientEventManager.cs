using RealPolicePlugin.API;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;

namespace RealPolicePlugin
{
    abstract class AbstractAmbientEventManager<E, D>
    {


        protected List<E> eventsRunning = new List<E>();
        private readonly List<D> entitiesInEvent = new List<D>();
        protected Type lastEvent = null;
        protected const int LOOP_SECURITY = 20;
        protected const int C_MAX_CURRENT_EVENT = 2;
        protected Random random;
        protected StopTimer timer { get; set; }




        public AbstractAmbientEventManager()
        {
            this.random = new Random();
            this.timer = new StopTimer();
        }


        public AbstractAmbientEventManager<E, D> AddPedInEvent(D entity)
        {
            this.entitiesInEvent.Add(entity);
            return this;
        }

        public void HandleEndEvent(E customEvent)
        {
            if (this.eventsRunning.Contains(customEvent))
            {
                this.eventsRunning.Remove(customEvent);
            }
        }

        virtual protected bool IsAllEventsCreated()
        {
            return this.eventsRunning.Count >= C_MAX_CURRENT_EVENT;
        }


        protected void Garbage()
        {
            if (this.entitiesInEvent.Count > 20)
            {
                this.entitiesInEvent.Clear();
            }
        }

        virtual protected bool CanCreateAnEvent()
        {
            return false == this.IsAllEventsCreated() && EventsManager.CanCreateAnEvent();
        }


        abstract public E GetRandomEvent();




    }
}
