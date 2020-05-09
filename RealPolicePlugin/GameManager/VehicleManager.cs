using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using RealPolicePlugin.Core;
using RealPolicePlugin.DB;
using RealPolicePlugin.Entity;
using Rage.Native;

namespace RealPolicePlugin.GameManager
{
    class VehicleManager
    {

        private static VehicleManager _Instance = null;

        private static bool _IsOfficerTyping = false;


        private VehicleManager()
        {
        }

        public Vehicle[] getNearbyVehicles(int arraySize=10)
        {
            return Game.LocalPlayer.Character.GetNearbyVehicles(arraySize);
        }

        public static VehicleManager Instance
        {
            get
            {
                if (null == _Instance)
                {
                    _Instance = new VehicleManager();
                }

                return _Instance;
            }
        }



    }
}
