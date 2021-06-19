using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Items
{
    public enum ToolAbilities { Digging, Chopping, Picking }
    class ToolComponent : Component
    {
        public override string ComponentName
        {
            get { return "Tool"; }
        }
        public List<ToolAbilities> Abilities = new List<ToolAbilities>();
        public ToolComponent()
        {

        }
        public ToolComponent(params ToolAbilities[] abilities)
        {
            this.Abilities.AddRange(abilities);
        }
        static public bool HasAbility(GameObject obj, ToolAbilities ability)
        {
            ToolComponent tool;
            if (obj.TryGetComponent<ToolComponent>(out tool))
                return tool.Abilities.Contains(ability);
            return false;
        }
        public override object Clone()
        {
            return new ToolComponent(this.Abilities.ToArray());
        }
    }
}
