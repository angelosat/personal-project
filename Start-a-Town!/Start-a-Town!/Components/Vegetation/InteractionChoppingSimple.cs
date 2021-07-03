using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Components
{
    public class InteractionChoppingSimple : InteractionPerpetual
    {
        static int MaxStrikes = 3;
        int StrikeCount = 0;
        ParticleEmitterSphere EmitterStrike;
        List<Rectangle> ParticleRects;
        public InteractionChoppingSimple()
            : base("Chopping")
        { }
        static readonly ScriptTaskCondition cancel = new Exists();
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new Exists(),
                        new RangeCheck()
                ));
        public override ScriptTaskCondition CancelState
        {
            get
            {
                return cancel;
            }
            set
            {
                base.CancelState = value;
            }
        }
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);

            this.EmitterStrike = new ParticleEmitterSphere();
            this.EmitterStrike.Source = t.Global + Vector3.UnitZ;
            this.EmitterStrike.SizeBegin = 1;
            this.EmitterStrike.SizeEnd = 1;
            this.EmitterStrike.ParticleWeight = 1;
            this.EmitterStrike.Radius = 1f;// .5f;
            this.EmitterStrike.Force = .1f;
            this.EmitterStrike.Friction = .5f;
            this.EmitterStrike.AlphaBegin = 1;
            this.EmitterStrike.AlphaEnd = 0;
            this.EmitterStrike.ColorBegin = MaterialDefOf.LightWood.Color;
            this.EmitterStrike.ColorEnd = MaterialDefOf.LightWood.Color;
            this.EmitterStrike.Lifetime = Engine.TicksPerSecond * 2;
            this.EmitterStrike.Rate = 0;
            this.ParticleRects = ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            if (a.Net is Net.Client)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            this.StrikeCount++;
            if (this.StrikeCount < MaxStrikes)
            {
                return;
            }
            this.Done(a, t);
            this.Finish(a, t);
        }
        public void Done(GameObject a, TargetArgs t)
        {
            CutDownPlant(a as Actor, t.Object as Plant);
        }
        public override object Clone()
        {
            return new InteractionChoppingSimple();
        }

        static public void CutDownPlant(Actor actor, Plant plant)
        {
            var comp = plant.PlantComponent;
            comp.Harvest(plant, actor);
            comp.CutDown(plant, actor);
        }
    }
}
