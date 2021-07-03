using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components
{
    partial class HaulComponent
    {
        class StatModifierCarrying : ValueModifier
        {
            public StatModifierCarrying()
                : base("Carrying", (mod, parent, v) => v * mod.GetValue("a"), new ValueModifierValue("a", 0.5f))
            {
                this.Modifier = this.GetFinalSpeed;
            }

            float GetFinalSpeed(ValueModifier mod, GameObject parent, float value)
            {
                var ratio = StatMaxWeight.GetRatio(parent);
                var final = 0.5f + 0.5f * ratio;
                return final;
            }
        }

    }
}
