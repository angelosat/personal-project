using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Resources
{
    class Durability : ResourceDef
    {
        public Durability() : base("Durability")
        {

        }
        public override ResourceDef.ResourceTypes ID
        {
            get { return ResourceDef.ResourceTypes.Durability; }
        }

        public override string Description
        {
            get { return "Basic Durability resource"; }
        }
      
        public override string Format
        {
            get
            {
                return "##0";
            }
        }
        internal override void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials)
        {
            var count = materials.Count;
            var totaldensity = materials.Values.Sum(m => m.Density);
            var dur = obj.GetResource(this);
            dur.Initialize(this.BaseMax + totaldensity / count, 1);
            dur.Value = 1; // HACK
        }
        
        public override Color GetBarColor(Resource resource)
        {
            return Color.LightGray;
        }
        public override string GetBarLabel(Resource resource)
        {
            return $"{resource.Value} / {resource.Max}";
        }
        public override string GetBarHoverText(Resource resource)
        {
            return this.GetLabel(resource);
        }
    }
}
