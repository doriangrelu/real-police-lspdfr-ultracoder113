using Rage;
using RealPolicePlugin.Core;

namespace RealPolicePlugin.API.Events.AmbientPed
{
    class PublicIntoxication : AbstractAmbientPedEvent
    {

        private const string MESSAGE = "Public intoxication";


        public PublicIntoxication(Ped entity) : base(entity, MESSAGE)
        {

        }

        public override void OnBeforeStartEvent()
        {
            AnimationSet drunkAnimation = new AnimationSet("move_m@drunk@verydrunk");
            drunkAnimation.LoadAndWait();
            this.Pedestrian.MovementAnimationSet = drunkAnimation;
            this.Pedestrian.IsPersistent = true;
            Rage.Native.NativeFunction.Natives.SET_PED_IS_DRUNK(this.Pedestrian, true);
        }

        public override void OnProcessEvent()
        {
            Logger.Log("Process public intoxication event");
            if (false == this.isAlreadyPedBlipCreated && PedsManager.IsNearby(PedsManager.LocalPlayer().Position, this.Pedestrian.Position, 40F))
            {

                if (this.BlipArea.IsValid() && this.BlipArea.Exists())
                {
                    this.BlipArea.Delete();
                }
                this.HandleBlip(this.Pedestrian, true);

                this.canReportCrime = true;
                this.DisplayReportCrimeHelp();

                this.isAlreadyPedBlipCreated = true;


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

        }
    }
}
