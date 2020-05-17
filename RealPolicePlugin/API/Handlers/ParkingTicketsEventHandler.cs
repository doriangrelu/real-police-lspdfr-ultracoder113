using LSPD_First_Response.Mod.API;
using Rage;
using RealPolicePlugin.API.Events;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Drawing;
using RealPolicePlugin.API.Type;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Handlers
{


    class ParkingTicketsEventHandler : AbstractRealPoliceEventHandler<GiveParkingTicketEvent>
    {

        private const float MAXIMUM_DIST_VEH = 3.5F;
        private const string K_OPEN_MENU = "ParkingTicketMenu";
        public static List<string> AlreadyGivedTicketsLicencePlateCollection = new List<string>();

        public static string[] Vowels = new string[] { "a", "e", "o", "i", "u" };
        public static string[] Numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };



        private static UIMenu _Menu = null;
        private static UIMenuItem _DangerousParkedVehicle;
        private static UIMenuItem _MissingTicketsParkedVehicle;
        private static UIMenuItem _Infos;


        public ParkingTicketsEventHandler()
        {
            Menu.OnItemSelect += this.OnSelectMenuItem;
        }

        public static UIMenuItem Infos
        {
            get
            {
                if (null == _Infos)
                {
                    _Infos = new UIMenuItem("None");
                    Menu.AddItem(_Infos);
                }

                return _Infos;
            }
        }

        public static UIMenu Menu
        {
            get
            {
                if (null == _Menu)
                {
                    _Menu = new UIMenu("Parking ticket", "By Ultracodder113");

                    UICustomMenuManager.MenuPool.Add(_Menu);

                    _DangerousParkedVehicle = new UIMenuItem("Dangerous parked vehicle - 375$");
                    _MissingTicketsParkedVehicle = new UIMenuItem("Awkward parked vehicle - 275$");


                    _DangerousParkedVehicle.ForeColor = Color.Red;
                    _MissingTicketsParkedVehicle.ForeColor = Color.Orange;
                    _Infos.ForeColor = Color.Blue;

                    _Menu.AddItem(_DangerousParkedVehicle);
                    _Menu.AddItem(_MissingTicketsParkedVehicle);

                    UICustomMenuManager.MenuPool.Add(_Menu);
                }
                return _Menu;

            }
        }


        public override void Handle()
        {
            while (true)
            {
                while (true)
                {
                    GameFiber.Yield();
                    if (KeysManager.IsKeyDownComputerCheck(Configuration.Instance.ReadKey(K_OPEN_MENU, "F5")) && !UICustomMenuManager.MenuPool.IsAnyMenuOpen() && this.CanCreateTicket())
                    {
                        Menu.Visible = !Menu.Visible;
                    }
                    UICustomMenuManager.MenuPool.ProcessMenus();
                }
            }
        }


        public void OnSelectMenuItem(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != Menu) return;

            if (selectedItem == _DangerousParkedVehicle || selectedItem == _MissingTicketsParkedVehicle)
            {
                ParkingTicketsOffences offenceType = (selectedItem == _DangerousParkedVehicle) ? ParkingTicketsOffences.DANGEROUS : ParkingTicketsOffences.AWKWARD;
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
            if (ParkingTicketsEventHandler.AlreadyGivedTicketsLicencePlateCollection.Contains(vehicle.LicensePlate))
            {
                Game.DisplayNotification("You have ~o~already given that vehicle a ~b~parking ticket");
                FunctionsLSPDFR.PlayScannerAudio("BEEP");
                return false;
            }
            return true;
        }

    }
}
