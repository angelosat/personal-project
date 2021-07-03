using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ResourcesComponent : EntityComponent
    {
        public Resource[] Resources;
        
        public override string ComponentName
        {
            get
            {
                return "Resources";
            }
        }
        public ResourcesComponent(ItemDef def)
        {
            var defs = def.ActorProperties.Resources;
            var count = defs.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
            {
                this.Resources[i] = new Resource(defs[i]);
            }
        }
        public ResourcesComponent()
        {
        }
        public ResourcesComponent(params Resource[] resources)
        {
            var count = resources.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
            {
                this.Resources[i] = resources[i].Clone();
            }
        }
        public ResourcesComponent(params ResourceDef[] defs)
        {
            var count = defs.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
            {
                this.Resources[i] = new Resource(defs[i]);
            }
        }
        internal override void Initialize(ComponentProps componentProps)
        {
            var props = componentProps as Props;
            var count = props.Defs.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
            {
                this.Resources[i] = new Resource(props.Defs[i]);
            }
        }
        public override void Tick(GameObject parent)
        {
            foreach (var item in this.Resources)
                item.Tick(parent);
        }
        
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            
            foreach (var item in this.Resources)
                item.HandleMessage(parent, e);

            switch (e.Type)
            {
                default:
                    break;
            }
            return false;
        }

        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            foreach (var item in this.Resources)
                item.HandleRemoteCall(parent, e);
        }

        public override void OnNameplateCreated(GameObject parent, UI.Nameplate plate)
        {
            foreach (var res in this.Resources)
                res.OnNameplateCreated(parent, plate);
        }
        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate)
        {
            foreach (var res in this.Resources)
                res.OnHealthBarCreated(parent, plate);
        }
        public override object Clone()
        {
            return new ResourcesComponent(this.Resources);
        }

        public override string ToString()
        {
            string text = "";
            foreach (var item in this.Resources)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }

        internal override void AddSaveData(SaveTag tag)
        {
            this.Resources.SaveImmutable(tag, "Resources");

        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            this.Resources.TryLoadImmutable(tag, "Resources");
        }
        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Resources.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Resources.Read(reader);
        }

        internal Resource GetResource(ResourceDef def)
        {
            return this.Resources.FirstOrDefault(r => r.ResourceDef == def);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            var box = new GroupBox();
            for (int i = 0; i < this.Resources.Length; i++)
            {
                box.AddControlsBottomLeft(this.Resources[i].GetControl());
            }
            info.AddInfo(box);
        }

        internal void AddModifier(ResourceRateModifier resourceRateModifier)
        {
            var resource = this.GetResource(resourceRateModifier.Def.Source);
            resource.AddModifier(resourceRateModifier);
        }
        internal override void Initialize(Entity parent, Dictionary<string, Material> materials)
        {
            for (int i = 0; i < this.Resources.Length; i++)
            {
                this.Resources[i].ResourceDef.InitMaterials(parent, materials);
            }
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var r in this.Resources)
                tooltip.AddControlsBottomLeft(r.GetControl());
        }
        public class Props : ComponentProps
        {
            public override Type CompType => typeof(ResourcesComponent);
            public ResourceDef[] Defs;
            public Props(params ResourceDef[] defs)
            {
                this.Defs = defs;
            }
        }
    }
}
