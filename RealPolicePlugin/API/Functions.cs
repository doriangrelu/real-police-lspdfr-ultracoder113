using Rage;
using Rage.Native;
using RealPolicePlugin.API.Events;
using RealPolicePlugin.API.Events.AmbientVehicle;
using RealPolicePlugin.API.Handlers;
using RealPolicePlugin.Core;
using RealPolicePlugin.OffenceEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API
{
    class Functions
    {


        public static void RunAmbientVehicleEvent(AbstractAmbientVehicleEvent offenceEvent)
        {
            offenceEvent.Prepare();
            try
            {
                offenceEvent.OnBeforeStartEvent();
                while (offenceEvent.IsRunning())
                {
                    offenceEvent.OnProcessEvent();
                    GameFiber.Yield();
                }
            }
            catch (Exception e)
            {
                Logger.Log("---------- Exception ----------");
                Logger.Log(e.Message);
            }
            finally
            {
                offenceEvent.OnEndEvent();
            }
        }



        public static void GiveParkingTicket(GiveParkingTicketEvent parkingTicketEvent)
        {
            Vehicle vehicle = parkingTicketEvent.Vehicle;
            string modelName = vehicle.Model.Name.ToLower();
            string licencePlate = vehicle.LicensePlate;
            string lexemArticle = ParkingTicketsEventHandler.Vowels.Contains<string>(modelName[0].ToString()) ? "an" : "a";

            string licencePlateAudioMessage = "";

            if (ParkingTicketsEventHandler.Numbers.Contains<string>(modelName.Last().ToString()))
            {
                modelName = modelName.Substring(0, modelName.Length - 1);
            }
            modelName = char.ToUpper(modelName[0]) + modelName.Substring(1);

            foreach (char character in licencePlate)
            {
                if (!Char.IsWhiteSpace(character))
                {
                    licencePlateAudioMessage = licencePlateAudioMessage + " " + character;
                }
            }

            ParkingTicketsEventHandler.Infos.Enabled = true;
            ParkingTicketsEventHandler.Infos.Text = "Vehicle " + modelName + " (" + licencePlate + ")";
            ParkingTicketsEventHandler.Infos.Enabled = false;

            Game.DisplayNotification("~g~Traffic Officer ~s~is reporting an ~r~illegally parked vehicle.");
            Game.DisplayNotification("~b~Processing a parking ticket (" + parkingTicketEvent.Amount + ") for " + lexemArticle + " ~r~" + modelName + "~b~ with licence plate: ~r~" + licencePlate + ".");
            Game.DisplayNotification("~b~The offending ~r~" + modelName + " ~b~is parked on ~o~" + World.GetStreetName(vehicle.Position) + ".");


            PedsManager.LocalPlayer().Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_UNARMED"), 0, true);
            Rage.Object notepad = new Rage.Object("prop_notepad_02", PedsManager.LocalPlayer().Position);
            int boneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(PedsManager.LocalPlayer(), (int)PedBoneId.LeftThumb2);
            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(notepad, PedsManager.LocalPlayer(), boneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, false, false, false, 2, 1);
            PedsManager.LocalPlayer().Tasks.PlayAnimation("veh@busted_std", "issue_ticket_cop", 1f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly).WaitForCompletion(8000);
            notepad.Delete();

            vehicle.IsPersistent = false;
            PedsManager.LocalPlayer().Tasks.PlayAnimation("random@arrests", "generic_radio_enter", 0.7f, AnimationFlags.UpperBodyOnly | AnimationFlags.StayInEndFrame).WaitForCompletion(1500);
            GameFiber.Sleep(2000);
            FunctionsLSPDFR.PlayScannerAudioUsingPosition("INTRO_01 OFFICERS_REPORT_02 ILLEGALLY_PARKED_VEHICLE IN_OR_ON_POSITION INTRO_02  OUTRO_03 TARGET_VEHICLE_LICENCE_PLATE UHH" + licencePlateAudioMessage + " NOISE_SHORT CODE4_ADAM OFFICER_INTRO_02 PROCEED_WITH_PATROL NOISE_SHORT OUTRO_02", PedsManager.LocalPlayer().Position);
            PedsManager.LocalPlayer().Tasks.PlayAnimation("random@arrests", "generic_radio_exit", 1.0f, AnimationFlags.UpperBodyOnly);
            ParkingTicketsEventHandler.AlreadyGivedTicketsLicencePlateCollection.Add(vehicle.LicensePlate);

        }


        public static void SetCustomPulloverLocation(CustomPulloverEvent customPulloverEvent)
        {
            Blip driverBlip = customPulloverEvent.PulledDriver.AttachBlip();
            driverBlip.Flash(500, -1);
            driverBlip.Color = System.Drawing.Color.Yellow;
            if (Tools.HavingChance(2, 10))
            {
                customPulloverEvent.PulledDriver.CanAttackFriendlies = true;
            }
            Vehicle playerPatrolCar = PedsManager.LocalPlayer().CurrentVehicle;
            int checkpoint = 0;

            try
            {
                Vector3 checkPointPosition = PedsManager.LocalPlayer().GetOffsetPosition(new Vector3(0f, 8f, -1f));
                checkpoint = NativeFunction.Natives.CREATE_CHECKPOINT<int>(46, checkPointPosition.X, checkPointPosition.Y, checkPointPosition.Z, checkPointPosition.X, checkPointPosition.Y, checkPointPosition.Z, 3.5f, 255, 0, 0, 255, 0); ;
                float xOffset = 0;
                float yOffset = 0;
                float zOffset = 0;
                bool SuccessfulSet = false;
                while (true)
                {
                    GameFiber.Wait(70);
                    Game.DisplaySubtitle("Set your desired ~o~pullover ~s~location. Hold ~b~Enter ~s~when done.", 100);
                    checkPointPosition = PedsManager.LocalPlayer().GetOffsetPosition(new Vector3((float)xOffset + 0.5f, (float)(yOffset + 8), (float)(-1 + zOffset)));
                    if (false == CustomPulloverEventHandler.IsAlreadyFollowing)
                    {
                        break;
                    }
                    if (false == FunctionsLSPDFR.IsPlayerPerformingPullover())
                    {
                        Game.DisplayNotification("You cancelled the ~b~Traffic Stop.");
                        break;
                    }
                    if (false == PedsManager.LocalPlayer().IsInVehicle(playerPatrolCar, false))
                    {
                        break;
                    }

                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_RESET, "NumPad5")))
                    {
                        xOffset = 0;
                        yOffset = 0;
                        zOffset = 0;
                    }
                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_FORWARD, "NumPad8")))
                    {
                        yOffset++;
                    }
                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_BACK, "NumPad2")))
                    {
                        yOffset--;
                    }
                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_RIGHT, "NumPad6")))
                    {
                        xOffset++;
                    }
                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_LEFT, "NumPad4")))
                    {
                        xOffset--;
                    }
                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_UP, "NumPad9")))
                    {
                        zOffset++;
                    }
                    if (KeysManager.IsKeyDownRightNowComputerCheck(Configuration.Instance.ReadKey(CustomPulloverEventHandler.K_POS_DOWN, "NumPad3")))
                    {
                        zOffset--;
                    }

                    if (KeysManager.IsKeyDownRightNowComputerCheck(System.Windows.Forms.Keys.Enter))
                    {
                        SuccessfulSet = true;
                        break;
                    }

                    NativeFunction.Natives.DELETE_CHECKPOINT(checkpoint);
                    checkpoint = NativeFunction.Natives.CREATE_CHECKPOINT<int>(46, checkPointPosition.X, checkPointPosition.Y, checkPointPosition.Z, checkPointPosition.X, checkPointPosition.Y, checkPointPosition.Z, 3f, 255, 0, 0, 255, 0);
                    NativeFunction.Natives.SET_CHECKPOINT_CYLINDER_HEIGHT(checkpoint, 2f, 2f, 2f);
                }
                NativeFunction.Natives.DELETE_CHECKPOINT(checkpoint);

                if (SuccessfulSet)
                {

                    try
                    {
                        playerPatrolCar.BlipSiren(true);
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("friends@frj@ig_1", "wave_c", 1f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
                    }
                    catch { }
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Vector3.Distance(customPulloverEvent.PulledDriver.Position, PedsManager.LocalPlayer().Position) > 25f)
                        {
                            Game.DisplaySubtitle("~h~~r~Stay close to the vehicle.", 700);
                        }

                        if (false == FunctionsLSPDFR.IsPlayerPerformingPullover())
                        {
                            Game.DisplayNotification("You cancelled the ~b~Traffic Stop.");
                            break;
                        }

                        if (false == PedsManager.LocalPlayer().IsInVehicle(playerPatrolCar, false) || false == CustomPulloverEventHandler.IsAlreadyFollowing)
                        {
                            break;
                        }


                        Rage.Task drivetask = customPulloverEvent.PulledDriver.Tasks.DriveToPosition(checkPointPosition, 12f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                        GameFiber.Wait(700);
                        if (false == drivetask.IsActive || Vector3.Distance(customPulloverEvent.PulledDriver.Position, checkPointPosition) < 1.5f) //exit if driver end task or is away from the expected position
                        {
                            break;
                        }
                    }
                    if (customPulloverEvent.pulledVehicle.Exists())
                    {
                        if (customPulloverEvent.PulledDriver.Exists())
                        {
                            customPulloverEvent.PulledDriver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                NativeFunction.Natives.DELETE_CHECKPOINT(checkpoint);
                Game.LogTrivial("---------- EXCEPTION ---------");
                Game.LogTrivial(e.Message);
                Game.LogTrivial("---------- END  ---------");
            }
            finally
            {
                if (driverBlip.IsValid() && driverBlip.Exists())
                {
                    driverBlip.Delete();
                }
                CustomPulloverEventHandler.IsAlreadyFollowing = false;
            }

        }


    }
}

