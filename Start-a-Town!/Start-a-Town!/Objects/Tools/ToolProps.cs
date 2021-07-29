using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class ToolProps : Def
    {
        public string Label, Description;
        public ToolAbility Ability;
        public HashSet<JobDef> AssociatedJobs = new();
        public Sprite SpriteHandle, SpriteHead;
        public ToolProps(string name)
            : base($"ToolProps_{name}")
        {
            this.Label = name;
        }
        public ToolProps(ToolAbility ability)
        {
            this.Ability = ability;
        }
        public ToolProps AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }

        public GameObject Create()
        {
            var tool = ItemFactory.CreateTool(ToolDefs.Tool);
            tool.AddComponent(new ResourcesComponent(ResourceDef.Durability));
            tool.AddComponent(new OwnershipComponent());
            tool.AddComponent(new ToolAbilityComponent(this));
            tool.Body.Sprite = this.SpriteHandle;
            tool.Body[BoneDef.EquipmentHead].Sprite = this.SpriteHead;
            tool.Name = $"Tool:{this.Label}";
            return tool;
        }

        internal static Entity CreateTemplate()
        {
            throw new NotImplementedException();
        }

        internal static void Init()
        {
            Register(ToolPropsDefof.Shovel);
            Register(ToolPropsDefof.Axe);

            foreach (var toolDef in GetDefs<ToolProps>())
            {
                var obj = toolDef.Create();
                GameObject.AddTemplate(obj);
            }
        }
    }

    static class ToolPropsDefof
    {
        public static readonly ToolProps Shovel = new("Shovel")
        {
            Description = "Used to dig out grainy material like soil dirt and sand.",
            SpriteHandle = ItemContent.ShovelHandle,
            SpriteHead = ItemContent.ShovelHead,
            Ability = new ToolAbility(ToolAbilityDef.Digging, 5),
            AssociatedJobs = new() { JobDefOf.Digger }
        };
        public static readonly ToolProps Axe = new("Axe")
        {
            Description = "Chops down trees.",
            SpriteHandle = ItemContent.AxeHandle,
            SpriteHead = ItemContent.AxeHead,
            Ability = new ToolAbility(ToolAbilityDef.Chopping, 5),
            AssociatedJobs = new() { JobDefOf.Lumberjack }
        };
    }
}
