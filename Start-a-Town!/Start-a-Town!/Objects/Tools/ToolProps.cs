using Start_a_Town_.Components.Crafting;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class ToolProps : Def, IItemDefVariator
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

        GameObject Create()
        {
            var tool = ToolDefs.Tool.CreateNew() as Tool;
            tool.ToolComponent.Props = this;
            tool.Body.Sprite = this.SpriteHandle;
            tool.Body[BoneDef.EquipmentHead].Sprite = this.SpriteHead;
            tool.Name = this.Label;
            return tool;
        }
        public Entity Create(Dictionary<string, Entity> ingredients)
        {
            var tool = Create() as Tool;
            tool.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
            return tool;
        }
        internal static void Init()
        {
            Register(ToolPropsDefof.Shovel);
            Register(ToolPropsDefof.Axe);
            Register(ToolPropsDefof.Hammer);
            Register(ToolPropsDefof.Handsaw);
            Register(ToolPropsDefof.Hoe);
            Register(ToolPropsDefof.Pickaxe);

            foreach (var toolProp in GetDefs<ToolProps>())
            {
                var obj = toolProp.Create();
                GameObject.AddTemplate(obj);
            }

            GenerateRecipesNew();
        }
        private static void GenerateRecipesNew()
        {
            var defs = Def.Database.Values.OfType<ToolProps>();
            foreach (var toolDef in defs)
            {
                var reagents = new List<Reaction.Reagent>();

                foreach (var reagent in ToolDefs.ToolCraftingProperties.Reagents)
                    reagents.Add(reagent.Value);

                var reaction = new Reaction(
                    toolDef.Name,
                    Reaction.CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
                    reagents,
                    new List<Reaction.Product>() { new Reaction.Product(toolDef.Create) },
                    SkillDef.Crafting);
            }
        }

        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, ToolDefs.Tool, this);
        }
    }
}
