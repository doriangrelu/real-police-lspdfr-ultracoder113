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

        public static void GetSpawnPoint(Vector3 StartPoint, out Vector3 SpawnPoint1, out float Heading1, bool UseSpecialID)
        {
            Vector3 tempspawn = World.GetNextPositionOnStreet(StartPoint.Around2D(55));
            Vector3 SpawnPoint = Vector3.Zero;
            float Heading = 0;
            unsafe
            {
                if (!UseSpecialID || !NativeFunction.Natives.GET_NTH_CLOSEST_VEHICLE_NODE_FAVOUR_DIRECTION<bool>(tempspawn.X, tempspawn.Y, tempspawn.Z, StartPoint.X, StartPoint.Y, StartPoint.Z, 0, out SpawnPoint, out Heading, 0, 0x40400000, 0) || !ExtensionMethods.IsNodeSafe(SpawnPoint))
                {
                    Game.LogTrivial("Unsuccessful specialID");
                    SpawnPoint = World.GetNextPositionOnStreet(StartPoint.Around2D(55f));
                    Vector3 directionFromVehicleToPed1 = (StartPoint - SpawnPoint);
                    directionFromVehicleToPed1.Normalize();

                    Heading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);
                }

            }
            SpawnPoint1 = SpawnPoint;
            Heading1 = Heading;
        }


        public static Vehicle SpawnVehicle(Vector3 position, Model model)
        {
            float Heading;
            Vector3 SpawnPoint;
            VehicleManager.GetSpawnPoint(position, out SpawnPoint, out Heading, true);
            return new Vehicle(model, SpawnPoint, Heading);
        }


        public static Model GetTowModel(Vehicle vehicle)
        {
            if (vehicle.Model.IsCar && !vehicle.IsDead)
            {
                return TowtruckModel;
            }
            return FlatbedModel;
        }

        public static void driveToEntity(Ped driver, Vehicle driverCar, Vehicle vehicleTo, bool GetCloseToEntity)
        {

            Ped playerPed = PedsManager.LocalPlayer();
            int drivingLoopCount = 0;
            bool transportVanTeleported = false;
            int waitCount = 0;
            bool forceCloseSpawn = false;
            //Get close to player with various checks
            try
            {

                Rage.Task driveToPed = null;
                driver.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraight).WaitForCompletion(500);

                while (Vector3.Distance(driverCar.Position, vehicleTo.Position) > 35f)
                {
                    if (!vehicleTo.Exists() || !vehicleTo.IsValid())
                    {
                        return;
                    }

                    driverCar.Repair();
                    if (driveToPed == null || !driveToPed.IsActive)
                    {
                        driver.Tasks.DriveToPosition(vehicleTo.Position, 15f, VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    }
                    NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(driver, 786607);
                    NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(driver, 0f);
                    NativeFunction.Natives.SET_DRIVER_ABILITY(driver, 1f);


                    if (driverCar.Speed < 2f)
                    {
                        drivingLoopCount++;
                    }

                    //If Van is stuck, relocate it
                    if ((drivingLoopCount >= 33 && drivingLoopCount <= 38))
                    {
                        StopThePed.API.Functions.callTowService();
                        Vector3 SpawnPoint;
                        float Heading;
                        bool UseSpecialID = true;
                        float travelDistance;
                        int wC = 0;
                        while (true)
                        {
                            GetSpawnPoint(vehicleTo.Position, out SpawnPoint, out Heading, UseSpecialID);
                            travelDistance = Rage.Native.NativeFunction.Natives.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS<float>(SpawnPoint.X, SpawnPoint.Y, SpawnPoint.Z, vehicleTo.Position.X, vehicleTo.Position.Y, vehicleTo.Position.Z);
                            wC++;
                            if (Vector3.Distance(vehicleTo.Position, SpawnPoint) > 55F - 15f)
                            {
                                if (travelDistance < (55F * 4.5f))
                                {
                                    Vector3 directionFromVehicleToPed1 = (vehicleTo.Position - SpawnPoint);
                                    directionFromVehicleToPed1.Normalize();
                                    float HeadingToPlayer = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed1);
                                    if (Math.Abs(MathHelper.NormalizeHeading(Heading) - MathHelper.NormalizeHeading(HeadingToPlayer)) < 150f)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (wC >= 400)
                            {
                                UseSpecialID = false;
                            }
                            GameFiber.Yield();
                        }
                        Game.Console.Print("Relocating because service was stuck...");
                        driverCar.Position = SpawnPoint;
                        driverCar.Heading = Heading;
                        drivingLoopCount = 39;
                    }

                    // if van is stuck for a 2nd time or takes too long, spawn it very near to the car
                    else if (((drivingLoopCount >= 70 || waitCount >= 110) && true) || forceCloseSpawn)
                    {
                        Game.Console.Print("Relocating service to a close position");

                        Vector3 SpawnPoint = World.GetNextPositionOnStreet(vehicleTo.Position.Around2D(15f));

                        int waitCounter = 0;
                        while ((SpawnPoint.Z - vehicleTo.Position.Z < -3f) || (SpawnPoint.Z - vehicleTo.Position.Z > 3f) || (Vector3.Distance(SpawnPoint, vehicleTo.Position) > 26f))
                        {
                            waitCounter++;
                            SpawnPoint = World.GetNextPositionOnStreet(vehicleTo.Position.Around(20f));
                            GameFiber.Yield();
                            if (waitCounter >= 500)
                            {
                                SpawnPoint = vehicleTo.Position.Around(20f);
                                break;
                            }
                        }
                        Vector3 directionFromVehicleToPed = (vehicleTo.Position - SpawnPoint);
                        directionFromVehicleToPed.Normalize();

                        float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                        driverCar.Heading = vehicleHeading + 180f;
                        driverCar.Position = SpawnPoint;

                        transportVanTeleported = true;

                        break;
                    }

                }

                forceCloseSpawn = true;
                //park the van
                Game.HideHelp();
                if (!GetCloseToEntity)
                {
                    while ((Vector3.Distance(vehicleTo.Position, driverCar.Position) > 19f && (driverCar.Position.Z - vehicleTo.Position.Z < -2.5f) || (driverCar.Position.Z - vehicleTo.Position.Z > 2.5f)) && !transportVanTeleported)
                    {
                        if (!vehicleTo.Exists() || !vehicleTo.IsValid())
                        {
                            return;
                        }

                        Rage.Task parkNearcar = driver.Tasks.DriveToPosition(vehicleTo.Position, 6f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                        parkNearcar.WaitForCompletion(900);

                        if (Vector3.Distance(vehicleTo.Position, driverCar.Position) > 60f)
                        {
                            Vector3 SpawnPoint = World.GetNextPositionOnStreet(vehicleTo.Position.Around(10f));

                            int waitCounter = 0;
                            while ((SpawnPoint.Z - vehicleTo.Position.Z < -3f) || (SpawnPoint.Z - vehicleTo.Position.Z > 3f) || (Vector3.Distance(SpawnPoint, vehicleTo.Position) > 26f))
                            {
                                waitCounter++;
                                SpawnPoint = World.GetNextPositionOnStreet(vehicleTo.Position.Around(20f));
                                GameFiber.Yield();
                                if (waitCounter >= 500)
                                {
                                    SpawnPoint = vehicleTo.Position.Around(20f);
                                    break;
                                }
                            }
                            Vector3 directionFromVehicleToPed = (vehicleTo.Position - SpawnPoint);
                            directionFromVehicleToPed.Normalize();

                            float vehicleHeading = MathHelper.ConvertDirectionToHeading(directionFromVehicleToPed);
                            driverCar.Heading = vehicleHeading + 180f;
                            driverCar.Position = SpawnPoint;

                            transportVanTeleported = true;
                        }
                    }
                }
            }

            catch (Exception)
            {
                return;
            }
        }



    }
}
