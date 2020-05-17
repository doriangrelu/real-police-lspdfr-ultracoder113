
using RealPolicePlugin.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.API.Handlers
{
    internal abstract class AbstractRealPoliceEventHandler<E> : I_RealPoliceHandler where E : EventArgs
    {

        public EventHandler<E> EventHandler;

        public abstract void Handle();

        virtual protected void OnEventHandler(E eventArgs)
        {
            this.EventHandler?.Invoke(this, eventArgs);
        }
    }
}
