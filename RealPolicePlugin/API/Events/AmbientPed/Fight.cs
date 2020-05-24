using Rage;
using RealPolicePlugin.Core;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Events.AmbientPed
{
    class Fight : AbstractAmbientPedEvent
    {

        private const string MESSAGE = "Fight";

        private const int LOOP_SECURITY = 15;


        private bool attackPlayer = false;
        private bool fleeTheScene = false;

        public Fight(Ped entity) : base(entity, MESSAGE)
        {
            this.attackPlayer = Tools.HavingChance(4, 10);
            if (false == this.attackPlayer)
            {
                this.fleeTheScene = Tools.HavingChance(4, 15);
            }
        }

        public override void OnBeforeStartEvent()
        {
            this.Pedestrian.Armor = 200;
            Ped[] nearbyPeds = this.Pedestrian.GetNearbyPeds(10);
            int maxNumberPedIsFighting = Tools.GetNextInt(2, 4);
            int numberPedIsFighting = 0;


            foreach (Ped ped in nearbyPeds)
            {
                if (numberPedIsFighting >= maxNumberPedIsFighting)
                {
                    break;
                }
                if (IsPedEligible(this.Pedestrian, ped, 20F))
                {
                    numberPedIsFighting++;
                    ped.BlockPermanentEvents = true;
                    ped.Tasks.ClearImmediately();
                    ped.Tasks.FightAgainst(this.Pedestrian);
                    ped.KeepTasks = true;
                    ped.IsPersistent = true;
                    otherPedInEvent.Add(ped);
                }
                GameFiber.Yield();
            }
            if (numberPedIsFighting == 0) //Bad Chance... 
            {
                Logger.Log("Missing many peds");
                this.IsEventRunning = false;
            }
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
                foreach (Ped ped in this.otherPedInEvent)
                {
                    this.HandleBlip(ped, false);
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

                if (Tools.HavingChance(5, 10))
                {
                    this.HandleAttackWithKnife(this.Pedestrian);
                }

                foreach (Ped ped in this.otherPedInEvent)
                {
                    ped.Tasks.ClearImmediately();
                    ped.Tasks.FightAgainst(PedsManager.LocalPlayer());
                    ped.KeepTasks = true;
                    this.HandleLifePed(ped);
                    if (Tools.HavingChance(5, 20))
                    {
                        this.HandleAttackWithKnife(ped);
                    }
                }

                this.attackPlayer = false; //because player already attacked ;) 

            }

            if (this.fleeTheScene && PedsManager.IsNearby(PedsManager.LocalPlayer().Position, this.Pedestrian.Position, 10F))
            {
                this.fleeTheScene = false;

                this.Pedestrian.Tasks.ClearImmediately();
                this.Pedestrian.Tasks.ReactAndFlee(PedsManager.LocalPlayer());
                Vector3 fleeDirection;
                foreach (Ped ped in this.otherPedInEvent)
                {
                    fleeDirection = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around2D(500F));
                    ped.Tasks.ClearImmediately();
                    ped.Tasks.Flee(fleeDirection, 400F, 8000);
                    ped.KeepTasks = true;
                    FunctionsLSPDFR.SetPedResistanceChance(ped, Tools.GetNextInt(0, 100));
                    GameFiber.Sleep(3000);
                }

                GameFiber.Yield();
            }

        }


        public override void OnEndEvent()
        {
            if (false == this.isPursuit)
            {
                foreach (Ped ped in this.otherPedInEvent)
                {
                    if (ped.Exists())
                    {
                        ped.IsPersistent = false;
                        ped.KeepTasks = false;
                        ped.Tasks.ClearImmediately();
                    }
                }
                this.Pedestrian.IsPersistent = false;
                this.Pedestrian.KeepTasks = false;
                this.Pedestrian.Tasks.ClearImmediately();
            }
            base.OnEndEvent();
        }

    }
}
