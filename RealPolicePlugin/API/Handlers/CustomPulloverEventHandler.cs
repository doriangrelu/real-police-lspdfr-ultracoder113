using Rage;
using RealPolicePlugin.API.Events;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Handlers
{
    class CustomPulloverEventHandler : AbstractRealPoliceEventHandler<CustomPulloverEvent>
    {

        public static bool IsAlreadyFollowing = false;


        public const string K_POS_UP = "PositionUpKey";
        public const string K_POS_DOWN = "PositionDownKey";
        public const string K_POS_BACK = "PositionBackwardKey";
        public const string K_POS_FORWARD = "PositionForwardKey";
        public const string K_POS_LEFT = "PositionLeftKey";
        public const string K_POS_RESET = "PositionResetKey";
        public const string K_POS_RIGHT = "PositionRightKey";


        public const string K_CUSTOM_LOC = "CustomPulloverLocationKey";
        public const string K_CUSTOM_LOC_MOD = "CustomPulloverLocationModifierKey";

        public CustomPulloverEventHandler()
        {
        }

        public override void Handle()
        {

            while (Main.IsAlive)
            {
                GameFiber.Yield();
                if (KeysManager.IsKeyCombinationDownComputerCheck(
                    Configuration.Instance.ReadKey(K_CUSTOM_LOC, "W"), // CTRL + W by default
                    Configuration.Instance.ReadKey(K_CUSTOM_LOC_MOD, "LControlKey")
                    ) &&
                    false == IsAlreadyFollowing)
                {
                    this.OnSetCustomLocation();
                }
            }
        }



        public void OnSetCustomLocation()
        {
            CustomPulloverEventHandler.IsAlreadyFollowing = true;
            Logger.Log("Set Pullover");
            try
            {
                if (false == FunctionsLSPDFR.IsPlayerPerformingPullover() || false == PedsManager.LocalPlayer().IsInAnyVehicle(false))
                {
                    Logger.Log("Cancelled");
                    CustomPulloverEventHandler.IsAlreadyFollowing = false;
                    return;
                }

                Vehicle playerPatrolCar = PedsManager.LocalPlayer().CurrentVehicle;
                Vehicle pulledCar = (Vehicle)World.GetClosestEntity(playerPatrolCar.GetOffsetPosition(Vector3.RelativeFront * 8f), 8f, (GetEntitiesFlags.ConsiderGroundVehicles | GetEntitiesFlags.ConsiderBoats | GetEntitiesFlags.ExcludeEmptyVehicles | GetEntitiesFlags.ExcludeEmergencyVehicles));

                if (null == pulledCar || (false == pulledCar.IsValid() || (pulledCar == playerPatrolCar)))
                {
                    Logger.Log("No car or no valid car");
                    CustomPulloverEventHandler.IsAlreadyFollowing = false;
                    return;
                }

                if (pulledCar.Speed > 0.2f)
                {
                    Game.DisplayNotification("The vehicle must be stopped before you can do this.");
                    CustomPulloverEventHandler.IsAlreadyFollowing = false;
                    return;
                }

                Ped pulledDriver = pulledCar.Driver;
                pulledDriver.IsPersistent = true;
                if (!pulledDriver.IsPersistent || FunctionsLSPDFR.GetPulloverSuspect(FunctionsLSPDFR.GetCurrentPullover()) != pulledDriver)
                {
                    Logger.Log("Not persistent");
                    CustomPulloverEventHandler.IsAlreadyFollowing = false;
                    return;
                }

                CustomPulloverEvent customPulloverEvent = new CustomPulloverEvent(pulledCar, pulledDriver);
                this.OnEventHandler(customPulloverEvent);

            }
            catch (Exception e)
            {
                Game.LogTrivial("---------- EXCEPTION ---------");
                Game.LogTrivial(e.Message);
                Game.LogTrivial("---------- END  ---------");
            }
            finally
            {
                CustomPulloverEventHandler.IsAlreadyFollowing = false;
            }


        }





    }
}
