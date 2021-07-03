using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Stats
{
    class StatMaxWeight : Stat
    {
        public StatMaxWeight()
        {
            this.ID = Types.MaxWeight;
            this.Name = "Max Carry Weight";
            this.Modifiers.Add(new ValueModifier("Strength", (mod, parent, v) => GetWeightFromStrength(parent)) { Description = mod => "Every 2 points of strength raises max carried weight by 1" });
        }

        float GetWeightFromStrength(GameObject parent)
        {
            var str = parent.GetAttribute(AttributeDef.Strength)?.Level ?? 0;
            return str / 2;
        }

        /// <summary>
        /// Returns a value between 0 if the maximum weight that can be lifted equals the lifted object's weight, and 1 if the maximum weight is more or equal of double the object's weight.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        static public float GetRatio(GameObject parent)
        {
            var obj = parent.GetHauled();// parent.GetComponent<HaulComponent>().GetObject();//.Slot.Object;

            if (obj == null)
                return 0;
            var w = obj.Physics.Weight; // sometimes obj is null? hauled obj is dropped without removing the modifier from the attribute?
            var maxW = StatsComponentNew.GetStat(parent, Stat.Types.MaxWeight).GetFinalValue(parent);
            var ratio = (maxW - w) / w;
            ratio = MathHelper.Clamp(ratio, 0, 1);
            return ratio;
        }
    }
}
