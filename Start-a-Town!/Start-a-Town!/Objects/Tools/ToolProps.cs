using Start_a_Town_.Components;
using System;
using System.Collections.Generic;

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

        GameObject Create()
        {
            var tool = ToolDefs.Tool.CreateNew() as Tool;
            tool.ToolComponent.Props = this;
            tool.Body.Sprite = this.SpriteHandle;
            tool.Body[BoneDef.EquipmentHead].Sprite = this.SpriteHead;
            tool.Name = $"Tool:{this.Label}";
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
        }
    }
}
