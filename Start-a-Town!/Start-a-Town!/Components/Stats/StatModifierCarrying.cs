using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components
{
    partial class HaulComponent
    {
        class StatModifierCarrying : ValueModifier
        {
            public StatModifierCarrying()
                : base("Carrying", (mod, parent, v) => v * mod.GetValue("a"), new ValueModifierValue("a", 0.5f))
                //: base("Carrying", GetFinalSpeed, new StatModifierValue("a", 0.5f))

            {
                //this.Description = (mod) => "Carrying an item reduces your speed by " + (1 - mod.GetValue("a")).ToString("##%");
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
