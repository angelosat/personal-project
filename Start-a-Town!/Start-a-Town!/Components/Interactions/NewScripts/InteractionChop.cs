using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class InteractionChop : InteractionToolUse
    {
        protected override float Progress => 1 - this.HitPoints.Percentage;
        Resource HitPoints => this.Target.Object.GetResource(ResourceDefOf.HitPoints);
        Plant Plant => this.Target.Object as Plant;

        public InteractionChop() : base("Chopping")
        {

        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        protected override void ApplyWork(float workAmount)
        {
            this.HitPoints.Value -= workAmount;
            this.Plant.PlantComponent.Wiggle((float)Math.PI / 32f, 20, this.Plant.PlantComponent.PlantProperties.StemMaterial.Density);
        }

        protected override void Done()
        {
            var plant = this.Plant;
            var comp = plant.PlantComponent;
            comp.Harvest(plant, this.Actor);
            comp.CutDown(plant, this.Actor);
        }

        protected override Color GetParticleColor()
        {
            return this.Plant.PlantComponent.PlantProperties.StemMaterial.Color;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        protected override SkillDef GetSkill()
        {
            return SkillDefOf.Plantcutting;
        }

        protected override ToolUseDef GetToolUse()
        {
            return ToolUseDefOf.Chopping;
        }

        protected override float GetWorkAmount()
        {
            var plant = this.Plant;
            var plantProps = plant.PlantComponent.PlantProperties;
            var trunkHardness = plantProps.StemMaterial.Density;
            var toolEffect = (int)StatDefOf.WorkEffectiveness.GetValue(this.Actor);
            var amount = toolEffect / (float)trunkHardness;
            return amount;
        }
    }
}
