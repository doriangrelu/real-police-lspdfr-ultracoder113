using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.API.Interfaces
{
    interface I_AmbientEvent
    {
        void OnBeforeStartEvent();

        void OnProcessEvent();


        void OnEndEvent();


        bool IsRunning();

        void Prepare();

    }
}
