using Rage;
using RealPolicePlugin.Core;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Events.AmbientPed
{
    class SuspectPed : AbstractAmbientPedEvent
    {

        private const string MESSAGE = "Suspect people";
        private bool attackPlayer = false;

        public SuspectPed(Ped entity) : base(entity, MESSAGE)
        {
            this.attackPlayer = Tools.HavingChance(2, 20);
            this.HandleNotification();
        }


        public override void OnBeforeStartEvent()
        {
            this.Pedestrian.Tasks.ClearImmediately();
            this.Pedestrian.KeepTasks = true;
            this.Pedestrian.Tasks.Wander();
            this.HandlePedHaveKnife(this.Pedestrian);
            FunctionsLSPDFR.SetPedResistanceChance(this.Pedestrian, Tools.GetNextFloat(1, 100));
        }

        public override void OnProcessEvent()
        {
            if (this.CanCreatedBlipsAndShowHelp())
            {
                this.isAlreadyPedBlipCreated = true;
                this.canReportCrime = true;
                this.DisplayReportCrimeHelp();
                if (this.BlipArea.IsValid() && this.BlipArea.Exists())
                {
                    this.BlipArea.Delete();
                }

                this.HandleBlip(this.Pedestrian, true);
            }

            this.HandleShowHelp();

            if (IsEventRequireEnd())
            {
                return;
            }

            if (this.IsOfficerReportCrime())
            {
                return;
            }

            if (this.attackPlayer && Tools.HavingChance(8, 10) && PedsManager.IsNearby(PedsManager.LocalPlayer().Position, this.Pedestrian.Position, 30F))
            {
                this.Pedestrian.Tasks.ClearImmediately();
                this.Pedestrian.Tasks.FightAgainst(PedsManager.LocalPlayer());
                this.Pedestrian.KeepTasks = true;
                this.Pedestrian.Armor = 150;
                this.HandleAttackWithKnife(this.Pedestrian);
                this.attackPlayer = false; //because player already attacked ;) 

            }

        }
    }
}
