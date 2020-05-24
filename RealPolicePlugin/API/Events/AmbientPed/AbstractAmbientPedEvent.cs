using LSPD_First_Response.Mod.API;
using Rage;
using RealPolicePlugin.API.Interfaces;
using RealPolicePlugin.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FunctionsLSPDFR = LSPD_First_Response.Mod.API.Functions;

namespace RealPolicePlugin.API.Events.AmbientPed
{
    abstract class AbstractAmbientPedEvent : EventArgs, I_AmbientEvent
    {

        #region public proterties
        public Ped Pedestrian { get; }
        public bool IsEventRunning { get; protected set; }
        public Blip BlipArea { get; private set; }

        protected List<Ped> otherPedInEvent = new List<Ped>();
        #endregion

        #region private proterties
        protected bool isPursuit = false;
        private readonly Random random;

        protected bool isAlreadyPedBlipCreated = false;
        protected bool isDisplayHelpShowed = false;
        protected bool canReportCrime = false;


        protected bool hardClean = true;
        #endregion


        protected List<Blip> blips = new List<Blip>();


        public AbstractAmbientPedEvent(Ped ped, String offenceMessage)
        {
            this.random = new Random();
            this.Pedestrian = ped;
            this.Pedestrian.IsPersistent = true;
            this.Pedestrian.BlockPermanentEvents = true;

            uint notification = Logger.DisplayNotification("New event detected: ~g~" + offenceMessage);
            GameFiber.Sleep(6000);
            Game.RemoveNotification(notification);
            Pedestrian = ped;

            this.BlipArea = new Blip(this.Pedestrian.Position.Around(5f, 15f), 40F);
            this.BlipArea.Color = System.Drawing.Color.OrangeRed;
            this.BlipArea.Alpha = 0.5f;

            this.HandleLifePed(this.Pedestrian);
        }

        protected bool CanCreatedBlipsAndShowHelp()
        {
            return false == this.isAlreadyPedBlipCreated && PedsManager.IsNearby(PedsManager.LocalPlayer().Position, this.Pedestrian.Position, 50F);
        }

        protected void DisplayReportCrimeHelp()
        {
            Game.DisplayHelp("~b~You can press ~o~" + Configuration.Instance.ReadKey("ReportCrime", "J").ToString() + " to ~r~report crime");
        }

        protected void HandleShowHelp()
        {
            if (PedsManager.IsNearby(PedsManager.LocalPlayer().Position, this.Pedestrian.Position, 50F) && false == this.isDisplayHelpShowed)
            {
                this.isDisplayHelpShowed = true;
                Game.DisplayHelp("You can press ~b~End to ~o~end this event");
            }
        }


        protected bool IsEventRequireEnd()
        {
            if (this.isDisplayHelpShowed && KeysManager.IsKeyDownComputerCheck(Keys.End))
            {
                Game.DisplayHelp("You ~g~finish event");
                this.IsEventRunning = false;
                return true;
            }

            if (PedsManager.IsAway(PedsManager.LocalPlayer().Position, this.Pedestrian.Position, 450F))
            {
                this.IsEventRunning = false;
                return true;
            }

            return false;
        }

        protected bool IsOfficerReportCrime()
        {
            if (this.canReportCrime && KeysManager.IsKeyDownComputerCheck(Configuration.Instance.ReadKey("ReportCrime", "J")))
            {
                LHandle pursuit = FunctionsLSPDFR.CreatePursuit();
                FunctionsLSPDFR.AddPedToPursuit(pursuit, this.Pedestrian);
                this.hardClean = false;
                this.IsEventRunning = false;
                foreach (Ped ped in this.otherPedInEvent)
                {
                    FunctionsLSPDFR.AddPedToPursuit(pursuit, ped);
                }
                FunctionsLSPDFR.SetPursuitIsActiveForPlayer(pursuit, true);
                Functions.Dispatch();
                return true;
            }
            return false;
        }

        protected void HandleAttackWithKnife(Ped ped)
        {
            ped.Tasks.Wander();
            this.HandlePedHaveKnife(ped);
            FunctionsLSPDFR.SetPedResistanceChance(ped, Tools.GetNextInt(60, 100));
        }

        protected void HandlePedHaveKnife(Ped ped)
        {
            ped.Inventory.GiveNewWeapon("WEAPON_KNIFE", -1, true);
        }

        protected void HandleLifePed(Ped ped)
        {
            ped.Health = 209;
            ped.Armor = 92;
        }



        protected void HandleBlip(Ped ped, bool isPrincipal)
        {
            Blip blip = ped.AttachBlip();
            blip.Color = isPrincipal ? Color.Red : Color.DarkOrange;
            blip.Scale = 0.5F;
            this.blips.Add(blip);
        }


        protected void HandleDeleteBlips()
        {
            foreach (Blip blip in this.blips)
            {
                if (blip.IsValid() && blip.Exists())
                {
                    blip.Delete();
                }
            }
        }

        public void Prepare()
        {
            this.IsEventRunning = true;
        }

        public bool IsRunning()
        {
            this.HandleSafeEventRunning();
            return this.IsEventRunning && Main.IsAlive;
        }

        abstract public void OnBeforeStartEvent();

        abstract public void OnProcessEvent();


        public virtual void OnEndEvent()
        {
            this.IsEventRunning = false;
            if (this.BlipArea.IsValid() && this.BlipArea.Exists())
            {
                this.BlipArea.Delete();
            }
            this.HandleDeleteBlips();
            if (this.hardClean)
            {
                this.Pedestrian.IsPersistent = false;
            }
            AmbientPedEventManager.Instance.HandleEndEvent(this);
        }


        protected void HandleSafeEventRunning()
        {
            if (FunctionsLSPDFR.IsCalloutRunning())
            {
                this.IsEventRunning = false;
            }
        }

        protected void HandleAttack(bool withWeapon)
        {
            if (Tools.HavingChance(8, 10))
            {
                this.Pedestrian.BlockPermanentEvents = true;
            }
            this.Pedestrian.Tasks.Clear();
            GameFiber.Wait(600);
            if (withWeapon)
            {
                if (this.Pedestrian.Inventory.Weapons.Count() == 0)
                {
                    this.HandleAddWeapon();
                }
                GameFiber.Wait(600);
                Rage.Native.NativeFunction.Natives.TaskCombatPed(this.Pedestrian, PedsManager.LocalPlayer(), 0, 16);
            }
            else
            {
                this.Pedestrian.KeepTasks = true;
                this.Pedestrian.Tasks.FightAgainst(PedsManager.LocalPlayer());
            }

        }

        protected void HandleAddWeapon()
        {
            this.Pedestrian.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MICROSMG"), 500, true);
        }

        /// <summary>
        /// Best method ever lol 
        /// </summary>
        /// <param name="targetPed"></param>
        /// <param name="ped"></param>
        /// <param name="nearbyArea"></param>
        /// <returns></returns>
        public static bool IsPedEligible(Ped targetPed, Ped ped, float nearbyArea)
        {
            return ped.Exists() &&
                PedsManager.LocalPlayer() != ped &&
                false == FunctionsLSPDFR.IsPedArrested(ped) &&
                false == ped.IsDead &&
                ped.IsAlive &&
                false == ped.IsCuffed &&
                ped.IsHuman &&
                false == ped.IsInAnyVehicle(true) &&
                false == FunctionsLSPDFR.IsPedACop(ped) &&
                PedsManager.IsNearby(targetPed.Position, ped.Position, nearbyArea);
        }

    }
}
