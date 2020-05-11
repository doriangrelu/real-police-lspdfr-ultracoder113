using LSPD_First_Response.Mod.API;
using Rage;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealPolicePlugin
{
    class ParkingTicketManager
    {

        private const float MAXIMUL_VEH_DIST = 4F;
        private string[] Vowels = new string[] { "a", "e", "o", "i", "u" };
        private string[] Numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

        private static ParkingTicketManager _Instance = null;

        private ParkingTicketManager()
        {

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


        public void HandleTicket()
        {
            if (Functions.IsPlayerPerformingPullover() || null != Functions.GetActivePursuit())
            {
                Game.DisplayHelp("You must finish current pullover or pursuit !", 5000);
                return;
            }
            Vehicle[] nearbyVehicles = PedsManager.LocalPlayer().GetNearbyVehicles(1); //Get one nearby vehicle
            if (nearbyVehicles.Length == 0)
            {
                Game.DisplayHelp("Nothing vehicle", 5000);
                return;
            }
            Vehicle vehicle = nearbyVehicles[0];
            if (PedsManager.Distance(vehicle.Position) > ParkingTicketManager.MAXIMUL_VEH_DIST)
            {
                return;
            }

            if (vehicle.IsPoliceVehicle)
            {
                Game.DisplayHelp("~b~Police vehicles ~s~are exempt from parking laws", 5000);
                Functions.PlayScannerAudio("BEEP");
                return;
            }

            if (vehicle.Metadata.IsParkingTicketIssued)
            {
                Game.DisplayNotification("You have ~o~already given that vehicle a ~b~parking ticket");
                Functions.PlayScannerAudio("BEEP");
                return;
            }

            string modelName = vehicle.Model.Name.ToLower();
            string licencePlate = vehicle.LicensePlate;
            string lexemArticle = this.Vowels.Contains<string>(modelName[0].ToString()) ? "an" : "a";

            if (this.Numbers.Contains<string>(modelName.Last().ToString()))
            {
                modelName = modelName.Substring(0, modelName.Length - 1);
            }
            modelName = char.ToUpper(modelName[0]) + modelName.Substring(1);




        }


    }
}
