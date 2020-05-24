using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RealPolicePlugin.API.Events;
using RealPolicePlugin.API.Type;
using RealPolicePlugin.Core;
using System.Collections.Generic;
using System.Drawing;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Handlers
{


    class ParkingTicketsEventHandler : AbstractRealPoliceEventHandler<GiveParkingTicketEvent>
    {

        private const float MAXIMUM_DIST_VEH = 3.5F;
        private const string K_OPEN_MENU = "ParkingTicketMenu";
        public static readonly List<string> alreadyGivedTicketsLicencePlateCollection = new List<string>();

        public static string[] vowels = new string[] { "a", "e", "o", "i", "u" };
        public static string[] numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };



        private static UIMenu menu = null;
        private static UIMenuItem dangerousParkedVehicle;
        private static UIMenuItem missingTicketsParkedVehicle;
        private static UIMenuItem infos;


        public ParkingTicketsEventHandler()
        {
            Menu.OnItemSelect += this.OnSelectMenuItem;
        }

        public static UIMenuItem Infos
        {
            get
            {
                if (null == infos)
                {
                    infos = new UIMenuItem("None");
                    infos.ForeColor = Color.Blue;
                    Menu.AddItem(infos);
                }

                return infos;
            }
        }

        public static UIMenu Menu
        {
            get
            {
                if (null == menu)
                {
                    menu = new UIMenu("Parking ticket", "By Ultracodder113");

                    UICustomMenuManager.MenuPool.Add(menu);

                    dangerousParkedVehicle = new UIMenuItem("Dangerous parked vehicle - 375$");
                    missingTicketsParkedVehicle = new UIMenuItem("Awkward parked vehicle - 275$");


                    dangerousParkedVehicle.ForeColor = Color.Red;
                    missingTicketsParkedVehicle.ForeColor = Color.Orange;

                    menu.AddItem(dangerousParkedVehicle);
                    menu.AddItem(missingTicketsParkedVehicle);

                    UICustomMenuManager.MenuPool.Add(menu);
                }
                return menu;

            }
        }


        public override void Handle()
        {
            while (Main.IsAlive)
            {
                GameFiber.Yield();
                if (KeysManager.IsKeyDownComputerCheck(Configuration.Instance.ReadKey(K_OPEN_MENU, "F5")) && !UICustomMenuManager.MenuPool.IsAnyMenuOpen() && this.CanCreateTicket())
                {
                    Menu.Visible = !Menu.Visible;
                }
                UICustomMenuManager.MenuPool.ProcessMenus();
            }
        }


        public void OnSelectMenuItem(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != Menu) return;

            if (selectedItem == dangerousParkedVehicle || selectedItem == missingTicketsParkedVehicle)
            {
                ParkingTicketsOffences offenceType = (selectedItem == dangerousParkedVehicle) ? ParkingTicketsOffences.DANGEROUS : ParkingTicketsOffences.AWKWARD;
                GiveParkingTicketEvent giveParkingTicketEvent = new GiveParkingTicketEvent(VehicleManager.GetNearbyVehicle(), offenceType);
                this.OnEventHandler(giveParkingTicketEvent);
                UICustomMenuManager.MenuPool.CloseAllMenus();
            }

        }


        private bool CanCreateTicket()
        {
            if (FunctionsLSPDFR.IsPlayerPerformingPullover() || null != FunctionsLSPDFR.GetActivePursuit())
            {
                Game.DisplayHelp("You must finish current pullover or pursuit !", 5000);
                return false;
            }
            Vehicle vehicle = VehicleManager.GetNearbyVehicle();
            if ((null == vehicle || vehicle.IsPoliceVehicle || PedsManager.Distance(vehicle.Position) > MAXIMUM_DIST_VEH))
            {
                return false;
            }
            if (ParkingTicketsEventHandler.alreadyGivedTicketsLicencePlateCollection.Contains(vehicle.LicensePlate))
            {
                Game.DisplayNotification("You have ~o~already given that vehicle a ~b~parking ticket");
                FunctionsLSPDFR.PlayScannerAudio("BEEP");
                return false;
            }
            return true;
        }

    }
}
