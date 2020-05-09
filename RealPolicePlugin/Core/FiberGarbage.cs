using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.Core
{
    class FiberGarbage
    {


        public static void Collect(List<GameFiber> fibers, bool isAlive = true)
        {
           /* foreach (GameFiber fiber in fibers)
            {
                fibers.Remove(fiber);
                if (fiber != null)
                {
                    if (fiber.IsAlive == isAlive)
                    {
                        fiber.Abort();
                    }
                }
            }*/
        }

    }
}
