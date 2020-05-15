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

        public static Model TowtruckModel = "TOWTRUCK";
        public static Model FlatbedModel = "FLATBED";

        private static bool _IsOfficerTyping = false;


        private VehicleManager()
        {
        }

        public static float ConvertKMHToRage(float expected)
        {
            float kmReference = 50F;
            float rageReference = 13.8888889F;
            if (expected == kmReference)
            {
                return rageReference;
            }
            return (expected * rageReference) / kmReference;
        }


        public static float ConvertRageToKMH(float expected)
        {
            float kmReference = 50F;
            float rageReference = 13.8888889F;
            if (expected == rageReference)
            {
                return kmReference;
            }
            return (expected * kmReference) / rageReference;
        }

        public Vehicle[] getNearbyVehicles(int arraySize = 10)
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
