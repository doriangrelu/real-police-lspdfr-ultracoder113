using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API; 

namespace RealPolicePlugin
{
    class PedsManager
    {

        public const float MAX_DISTANCE = 300F; 

        public static Ped LocalPlayer()
        {
            return Game.LocalPlayer.Character; 
        }


        public static bool IsAwayFromLocalPlayer(Vector3 position)
        {
            return Vector3.Distance(Game.LocalPlayer.Character.Position, position) > PedsManager.MAX_DISTANCE; 
        }

        public static bool isPedInPursuit(Ped ped)
        {
            return ped.Exists() && Functions.GetActivePursuit() != null && Functions.GetPursuitPeds(Functions.GetActivePursuit()).Contains(ped); 
        }


    }
}
