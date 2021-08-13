using Start_a_Town_.Components.Crafting;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class ToolProps : Def, IItemDefVariator
    {
        public string Description;
        public ToolUse Ability;
        public HashSet<JobDef> AssociatedJobs = new();
        public Sprite SpriteHandle, SpriteHead;
        internal SkillDef Skill;

        public ToolProps(string name) : base(name)
        {
        }
        public ToolProps(ToolUse ability)
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
            tool.Body[BoneDefOf.ToolHead].Sprite = this.SpriteHead;
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
            
            ToolPropsDefof.Init();
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
                    toolDef.Label,
                    Reaction.CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
                    reagents,
                    new List<Reaction.Product>() { new Reaction.Product(toolDef.Create) },
                    SkillDefOf.Crafting);
            }
        }

        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, ToolDefs.Tool, this);
        }
    }
}
