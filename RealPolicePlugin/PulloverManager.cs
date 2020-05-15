using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealPolicePlugin
{
    class PulloverManager
    {


        private static PulloverManager _Instance = null;


        private static string[] numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };


        private Blip Blip;
        private bool isFollowing = false;
        private Vector3 CheckPointPosition;
        private int Checkpoint = 0;


        private const string K_POS_UP = "PositionUpKey";
        private const string K_POS_DOWN = "PositionDownKey";
        private const string K_POS_BACK = "PositionBackwardKey";
        private const string K_POS_FORWARD = "PositionForwardKey";
        private const string K_POS_LEFT = "PositionLeftKey";
        private const string K_POS_RESET = "PositionResetKey";
        private const string K_POS_RIGHT = "PositionRightKey";


        private const string K_CUSTOM_LOC = "CustomPulloverLocationKey";
        private const string K_CUSTOM_LOC_MOD = "CustomPulloverLocationModifierKey";



        private PulloverManager()
        {

        }

        public static PulloverManager Instance
        {
            get
            {
                if (null == PulloverManager._Instance)
                {
                    PulloverManager._Instance = new PulloverManager();
                }

                return PulloverManager._Instance;
            }
        }


        public void Handle()
        {

            while (true)
            {
                GameFiber.Yield();
                if (KeysManager.IsKeyCombinationDownComputerCheck(
                    Configuration.Instance.ReadKey(K_CUSTOM_LOC, "W"), // CTRL + W by default
                    Configuration.Instance.ReadKey(K_CUSTOM_LOC_MOD, "LControlKey")
                    ) &&
                    false == this.isFollowing)
                {
                    GameFiber.StartNew(this.SetCustomPulloverLocation);
                }
            }

        }


        private bool HandleCheckpoint(Vehicle playerPatrolCar, Ped pulledDriver)
        {
            this.Blip = pulledDriver.AttachBlip();
            this.Blip.Flash(500, -1);
            this.Blip.Color = System.Drawing.Color.Yellow;
            playerPatrolCar.BlipSiren(true);

            Vector3 CheckPointPosition = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0f, 8f, -1f));
            this.Checkpoint = NativeFunction.Natives.CREATE_CHECKPOINT<int>(46, CheckPointPosition.X, CheckPointPosition.Y, CheckPointPosition.Z, CheckPointPosition.X, CheckPointPosition.Y, CheckPointPosition.Z, 3.5f, 255, 0, 0, 255, 0); ;

            float xOffset = 0;
            float yOffset = 0;
            float zOffset = 0;

            while (true)
            {
                GameFiber.Wait(70);
                Game.DisplaySubtitle("Set your desired ~o~pullover ~s~location. Hold ~b~Enter ~s~when done.", 100);
                this.CheckPointPosition = PedsManager.LocalPlayer().GetOffsetPosition(new Vector3((float)xOffset + 0.5f, (float)(yOffset + 8), (float)(-1 + zOffset)));
                if (false == this.isFollowing)
                {
                    return false;
                }
                if (false == Functions.IsPlayerPerformingPullover())
                {
                    Game.DisplayNotification("You cancelled the ~b~Traffic Stop.");
                    return false;
                }
                if (false == PedsManager.LocalPlayer().IsInVehicle(playerPatrolCar, false))
                {
                    return false;
                }

                if (Game.IsKeyDown(Keys.Enter))
                {
                    return true;
                }

                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_RESET, "NumPad5")))
                {
                    xOffset = 0;
                    yOffset = 0;
                    zOffset = 0;
                }
                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_FORWARD, "NumPad8")))
                {
                    yOffset++;
                }
                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_BACK, "NumPad2")))
                {
                    yOffset--;
                }
                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_RIGHT, "NumPad6")))
                {
                    xOffset++;
                }
                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_LEFT, "NumPad4")))
                {
                    xOffset--;
                }
                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_UP, "NumPad9")))
                {
                    zOffset++;
                }
                if (Game.IsKeyDown(Configuration.Instance.ReadKey(K_POS_DOWN, "NumPad3")))
                {
                    zOffset--;
                }

                NativeFunction.Natives.DELETE_CHECKPOINT(this.Checkpoint);
                this.Checkpoint = NativeFunction.Natives.CREATE_CHECKPOINT<int>(46, this.CheckPointPosition.X, this.CheckPointPosition.Y, this.CheckPointPosition.Z, this.CheckPointPosition.X, this.CheckPointPosition.Y, this.CheckPointPosition.Z, 3f, 255, 0, 0, 255, 0);
                NativeFunction.Natives.SET_CHECKPOINT_CYLINDER_HEIGHT(this.Checkpoint, 2f, 2f, 2f);
            }
        }


        public void SetCustomPulloverLocation()
        {
            this.isFollowing = true;
            try
            {
                if (!Functions.IsPlayerPerformingPullover() || false == PedsManager.LocalPlayer().IsInAnyVehicle(false))
                {
                    this.isFollowing = false;
                    return;
                }

                Vehicle playerPatrolCar = PedsManager.LocalPlayer().CurrentVehicle;
                Vehicle pulledOverCar = (Vehicle)World.GetClosestEntity(playerPatrolCar.GetOffsetPosition(Vector3.RelativeFront * 8f), 8f, (GetEntitiesFlags.ConsiderGroundVehicles | GetEntitiesFlags.ConsiderBoats | GetEntitiesFlags.ExcludeEmptyVehicles | GetEntitiesFlags.ExcludeEmergencyVehicles));

                if (null == pulledOverCar || (false == pulledOverCar.IsValid() || (pulledOverCar == playerPatrolCar)))
                {
                    this.isFollowing = false;
                    return;
                }

                if (pulledOverCar.Speed > 0.2f)
                {
                    Game.DisplayNotification("The vehicle must be stopped before you can do this.");
                    this.isFollowing = false;
                    return;
                }

                Ped pulledDriver = pulledOverCar.Driver;
                if (!pulledDriver.IsPersistent || Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) != pulledDriver)
                {
                    this.isFollowing = false;
                    return;
                }

                bool SuccessfulSet = this.HandleCheckpoint(playerPatrolCar, pulledDriver);
                NativeFunction.Natives.DELETE_CHECKPOINT(this.Checkpoint);
                if (SuccessfulSet)
                {
                    try
                    {
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("friends@frj@ig_1", "wave_c", 1f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
                    }
                    catch { }
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Vector3.Distance(pulledDriver.Position, Game.LocalPlayer.Character.Position) > 25f)
                        {
                            Game.DisplaySubtitle("~h~~r~Stay close to the vehicle.", 700);
                        }

                        if (!Functions.IsPlayerPerformingPullover())
                        {
                            Game.DisplayNotification("You cancelled the ~b~Traffic Stop.");
                            break;
                        }

                        if (false == PedsManager.LocalPlayer().IsInVehicle(playerPatrolCar, false) || false == this.isFollowing)
                        {
                            break;
                        }


                        Rage.Task drivetask = pulledDriver.Tasks.DriveToPosition(CheckPointPosition, 12f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                        GameFiber.Wait(700);
                        if (false == drivetask.IsActive || Vector3.Distance(pulledDriver.Position, CheckPointPosition) < 1.5f)
                        {
                            break;
                        }
                    }
                    if (pulledOverCar.Exists())
                    {
                        if (pulledDriver.Exists())
                        {
                            pulledDriver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                NativeFunction.Natives.DELETE_CHECKPOINT(this.Checkpoint);
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("CustomPulloverLocationError handled.");
            }
            finally
            {
                if (this.Blip.IsValid() && this.Blip.Exists())
                {
                    this.Blip.Delete();
                }
                this.isFollowing = false;
            }

        }


    }
}
