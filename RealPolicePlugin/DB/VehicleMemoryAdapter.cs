using Rage;
using RealPolicePlugin.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin.DB
{
    class VehicleMemoryAdapter : AbstractMemory<VehicleDetails>
    {


        private static VehicleMemoryAdapter _Instance = null;


        private VehicleMemoryAdapter()
        {

        }


        public static VehicleMemoryAdapter Instance
        {
            get
            {
                if (null == VehicleMemoryAdapter.Instance)
                {
                    VehicleMemoryAdapter._Instance = new VehicleMemoryAdapter();
                }
                return VehicleMemoryAdapter._Instance; 
            }
        }

        public VehicleDetails GetVehicle(string licencePlate)
        {
            return this._Elements.Select(veh => veh).FirstOrDefault(veh => veh.LicensePlate.Equals(licencePlate));
        }

        public VehicleDetails GetVehicle(Vehicle vehicle)
        {
            return this.GetVehicle(vehicle.LicensePlate); 
        }

        public bool Exists(string licencePlate)
        {
            return this.GetVehicle(licencePlate) != null;
        }

        public bool Exists(Vehicle vehicle)
        {
            return this.GetVehicle(vehicle.LicensePlate) != null;
        }

    }
}
