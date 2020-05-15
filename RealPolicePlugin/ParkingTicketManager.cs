using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RealPolicePlugin.Core;
using RealPolicePlugin.GameManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin
{
    class ParkingTicketManager
    {



        private const float MAXIMUL_VEH_DIST = 4F;
        private string[] Vowels = new string[] { "a", "e", "o", "i", "u" };
        private string[] Numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

        private static string _TicketAmount = "0$";


        private const string K_OPEN_MENU = "ParkingTicketMenu";

        private static UIMenu _MainMenu;
        private static MenuPool _MenuPool;
        private static UIMenuItem _DangerousParkedVehicle;
        private static UIMenuItem _MissingTicketsParkedVehicle;
        private static UIMenuItem _Infos;


        private List<string> TicketsVehicles = new List<string>();
        //private static UIMenuItem _Informations;


        private static ParkingTicketManager _Instance = null;

        private ParkingTicketManager()
        {
            ParkingTicketManager._MenuPool = new MenuPool();

            ParkingTicketManager._MainMenu = new UIMenu("Parking ticket", "By Ultracodder113");

            ParkingTicketManager._MenuPool.Add(ParkingTicketManager._MainMenu);


            ParkingTicketManager._DangerousParkedVehicle = new UIMenuItem("Dangerous parked vehicle - 375$");
            ParkingTicketManager._MissingTicketsParkedVehicle = new UIMenuItem("Awkward parked vehicle - 275$");
            ParkingTicketManager._Infos = new UIMenuItem("None");

            ParkingTicketManager._DangerousParkedVehicle.ForeColor = Color.Red;
            ParkingTicketManager._MissingTicketsParkedVehicle.ForeColor = Color.Orange;
            ParkingTicketManager._Infos.ForeColor = Color.Blue;


            ParkingTicketManager._MainMenu.AddItem(ParkingTicketManager._Infos);
            ParkingTicketManager._MainMenu.AddItem(ParkingTicketManager._DangerousParkedVehicle);
            ParkingTicketManager._MainMenu.AddItem(ParkingTicketManager._MissingTicketsParkedVehicle);

            ParkingTicketManager._MainMenu.OnItemSelect += OnItemSelect;
        }



        public static ParkingTicketManager Instance
        {
            get
            {
                if (null == ParkingTicketManager._Instance)
                {
                    ParkingTicketManager._Instance = new ParkingTicketManager();
                }

                return ParkingTicketManager._Instance;
            }
        }

        public void Handle()
        {
            while (true)
            {
                GameFiber.Yield();
                if (KeysManager.IsKeyDownComputerCheck(Configuration.Instance.ReadKey(K_OPEN_MENU, "F5")) && !_MenuPool.IsAnyMenuOpen() && this.CanCreateTicket())
                {
                    _MainMenu.Visible = !_MainMenu.Visible;
                }
                _MenuPool.ProcessMenus();
            }
        }

        public static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != _MainMenu) return;

            if (selectedItem == _DangerousParkedVehicle || selectedItem == _MissingTicketsParkedVehicle)
            {
                ParkingTicketManager._TicketAmount = selectedItem == _DangerousParkedVehicle ? "375$" : "375$";
                _MenuPool.CloseAllMenus();
                GameFiber.StartNew(ParkingTicketManager.Instance.HandleTicket);
            }
        }



        private Vehicle GetNearbyVehicle()
        {
            Vehicle[] nearbyVehicles = PedsManager.LocalPlayer().GetNearbyVehicles(1); //Get one nearby vehicle
            if (nearbyVehicles.Length == 0)
            {
                return null;
            }
            return nearbyVehicles[0];
        }


        private bool CanCreateTicket()
        {
            if (Functions.IsPlayerPerformingPullover() || null != Functions.GetActivePursuit())
            {
                Game.DisplayHelp("You must finish current pullover or pursuit !", 5000);
                return false;
            }
            Vehicle vehicle = this.GetNearbyVehicle();
            if (null == vehicle)
            {
                return false;
            }

            if (PedsManager.Distance(vehicle.Position) > ParkingTicketManager.MAXIMUL_VEH_DIST)
            {
                return false;
            }

            if (vehicle.IsPoliceVehicle)
            {
                Game.DisplayHelp("~b~Police vehicles ~s~are exempt from parking laws", 5000);
                Functions.PlayScannerAudio("BEEP");
                return false;
            }

            if (this.TicketsVehicles.Contains(vehicle.LicensePlate))
            {
                Game.DisplayNotification("You have ~o~already given that vehicle a ~b~parking ticket");
                Functions.PlayScannerAudio("BEEP");
                return false;
            }

            return true;
        }


        private void HandleTicket()
        {
            Vehicle vehicle = this.GetNearbyVehicle();
            string modelName = vehicle.Model.Name.ToLower();
            string licencePlate = vehicle.LicensePlate;
            string lexemArticle = this.Vowels.Contains<string>(modelName[0].ToString()) ? "an" : "a";

            string licencePlateAudioMessage = "";

            if (this.Numbers.Contains<string>(modelName.Last().ToString()))
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


            _Infos.Enabled = true;
            _Infos.Text = "Vehicle " + modelName + " (" + licencePlate + ")";
            _Infos.Enabled = false;

            Game.DisplayNotification("~g~Traffic Officer ~s~is reporting an ~r~illegally parked vehicle.");
            Game.DisplayNotification("~b~Processing a parking ticket (" + ParkingTicketManager._TicketAmount + ") for " + lexemArticle + " ~r~" + modelName + "~b~ with licence plate: ~r~" + licencePlate + ".");
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
            Functions.PlayScannerAudioUsingPosition("INTRO_01 OFFICERS_REPORT_02 ILLEGALLY_PARKED_VEHICLE IN_OR_ON_POSITION INTRO_02  OUTRO_03 TARGET_VEHICLE_LICENCE_PLATE UHH" + licencePlateAudioMessage + " NOISE_SHORT INTRO_01 CODE4_ADAM OFFICER_INTRO_02 PROCEED_WITH_PATROL NOISE_SHORT OUTRO_02 INTRO_01", PedsManager.LocalPlayer().Position);
            PedsManager.LocalPlayer().Tasks.PlayAnimation("random@arrests", "generic_radio_exit", 1.0f, AnimationFlags.UpperBodyOnly);


            if (this.TicketsVehicles.Count > 10)
            {
                this.TicketsVehicles.Clear();
            }

            this.TicketsVehicles.Add(vehicle.LicensePlate);
        }

    }
}
