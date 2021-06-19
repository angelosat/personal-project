using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ResourcesComponent : EntityComponent
    {
        //public Dictionary<ResourceDef.ResourceTypes, ResourceDef> Resources { get { return (Dictionary<ResourceDef.ResourceTypes, ResourceDef>)this["Resources"]; } set { this["Resources"] = value; } }
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
            //this.Resources = new Dictionary<ResourceDef.ResourceTypes, ResourceDef>();
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
        //static public bool HasResource(GameObject entity, ResourceDef.ResourceTypes type)// Resource resource)
        //{
        //    ResourcesComponent comp;
        //    if (!entity.TryGetComponent<ResourcesComponent>(out comp))
        //        return false;
        //    return comp.Resources.ContainsKey(type);
        //}
        //static public bool HasResource(GameObject entity, ResourceDef type)// Resource resource)
        //{
        //    return entity.GetResource(type) != null;
        //}
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

        //public ResourcesComponent Initialize(params ResourceDef[] resources)
        //{
        //    this.Resources = resources.ToDictionary(foo => foo.ID, foo => foo);
        //    return this;
        //}
        //public ResourcesComponent(params ResourceDef[] resources)
        //{
        //    this.Resources = resources.ToDictionary(foo => foo.ID, foo => foo);
        //}

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
            //ResourcesComponent comp = new ResourcesComponent();

            //using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            //{
            //    this.Write(w);
            //    w.BaseStream.Position = 0;
            //    using (BinaryReader r = new BinaryReader(w.BaseStream))
            //        comp.Read(r);
            //}

            //return comp;
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
            //this.Resources.SaveNewBEST(tag, "Resources");
            this.Resources.SaveImmutable(tag, "Resources");

        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            //tag.TryLoad("Resources", ref this.Resources);
            this.Resources.TryLoadImmutable(tag, "Resources");
        }
        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Resources.Write(writer);
            //writer.Write(this.Resources.Count);
            //foreach (var res in this.Resources.Values)
            //{
            //    writer.Write((byte)res.ID);
            //    res.Write(writer);
            //}
            
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Resources.Read(reader);
            //int count = reader.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    ResourceDef.ResourceTypes id = (ResourceDef.ResourceTypes)reader.ReadByte();
            //    ResourceDef res = ResourceDef.Create(id, reader.ReadSingle(), reader.ReadSingle());
            //    this.Resources[id] = res;
            //}
          
        }

        //static public ResourceDef GetResource(GameObject entity, ResourceDef.ResourceTypes type)
        //{
        //    var resources = entity.GetComponent<ResourcesComponent>();
        //    if (resources == null)
        //        return null;
        //    ResourceDef resource;
        //    resources.Resources.TryGetValue(type, out resource);
        //    return resource;
        //}

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

    //class ResourcesComponent : EntityComponent
    //{
    //    public Dictionary<ResourceDef.ResourceTypes, ResourceDef> Resources { get { return (Dictionary<ResourceDef.ResourceTypes, ResourceDef>)this["Resources"]; } set { this["Resources"] = value; } }
    //    //ResourceCollection Resources { get { return (ResourceCollection)this["Resources"]; } set { this["Resources"] = value; } }
    //    public override string ComponentName
    //    {
    //        get
    //        {
    //            return "Resources";
    //        }
    //    }
    //    public ResourcesComponent()
    //    {
    //        this.Resources = new Dictionary<ResourceDef.ResourceTypes, ResourceDef>();
    //    }

    //    public override void Tick(GameObject parent)
    //    {
    //        //this.Resources.Update(parent);
    //        foreach (var item in this.Resources.Values)
    //            item.Tick(parent);
    //    }
    //    static public bool HasResource(GameObject entity, ResourceDef.ResourceTypes type)// Resource resource)
    //    {
    //        ResourcesComponent comp;
    //        if (!entity.TryGetComponent<ResourcesComponent>(out comp))
    //            return false;
    //        return comp.Resources.ContainsKey(type);
    //    }
    //    public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
    //    {
    //        //foreach (var prop in from prop in this.Properties.Values where prop is Resource select prop as Resource)
    //        //{
    //        //    prop.HandleMessage(parent, e);
    //        //}

    //        //return true;
    //        foreach (var item in this.Resources.Values)
    //            item.HandleMessage(parent, e);

    //        switch(e.Type)
    //        {
    //            default:
    //                break;
    //        }
    //        return false;
    //    }

    //    internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
    //    {
    //        foreach (var item in this.Resources.Values)
    //            item.HandleRemoteCall(parent, e);
    //    }

    //    public ResourcesComponent Initialize(params ResourceDef[] resources)
    //    {
    //        //resources.ToList().ForEach(foo => this[foo.Name] = foo);
    //        this.Resources = resources.ToDictionary(foo => foo.ID, foo => foo);
    //        return this;
    //    }
    //    public ResourcesComponent(params ResourceDef[] resources)
    //    {
    //        //resources.ToList().ForEach(foo => this[foo.Name] = foo);
    //        this.Resources = resources.ToDictionary(foo => foo.ID, foo => foo);
    //    }

    //    public override void OnNameplateCreated(GameObject parent, UI.Nameplate plate)
    //    {
    //        foreach (var res in this.Resources.Values)
    //            res.OnNameplateCreated(parent, plate);
    //    }
    //    public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate)
    //    {
    //        foreach (var res in this.Resources.Values)
    //            res.OnHealthBarCreated(parent, plate);
    //    }
    //    public override object Clone()
    //    {
    //        //return new ResourcesComponent(this.Properties.Values.Select(foo => foo as Resource).ToArray());
    //        ResourcesComponent comp = new ResourcesComponent();

    //        using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
    //        {
    //            this.Write(w);
    //            w.BaseStream.Position = 0;
    //            using (BinaryReader r = new BinaryReader(w.BaseStream))
    //                comp.Read(r);
    //        }

    //        return comp;
    //    }

    //    public override string ToString()
    //    {
    //        string text = "";
    //        foreach (var item in this.Resources.Values)
    //            text += item.ToString() + "\n";
    //        return text.TrimEnd('\n');
    //    }

    //    public override void Write(System.IO.BinaryWriter writer)
    //    {
    //        writer.Write(this.Resources.Count);
    //        foreach (var res in this.Resources.Values)
    //        {
    //            writer.Write((byte)res.ID);
    //            res.Write(writer);
    //        }
    //        //List<Resource> resources =
    //        //    (from prop in this.Properties.Values
    //        //     where prop is Resource
    //        //     select prop as Resource).ToList();
    //        //writer.Write(resources.Count);
    //        //foreach (var res in resources)
    //        //{
    //        //    writer.Write((byte)res.ID);
    //        //    res.Write(writer);
    //        //}
    //    }
    //    public override void Read(System.IO.BinaryReader reader)
    //    {
    //        int count = reader.ReadInt32();
    //        for (int i = 0; i < count; i++)
    //        {
    //            ResourceDef.ResourceTypes id = (ResourceDef.ResourceTypes)reader.ReadByte();
    //            ResourceDef res = ResourceDef.Create(id, reader.ReadSingle(), reader.ReadSingle());
    //            this.Resources[id] = res;
    //        }
    //        //int count = reader.ReadInt32();
    //        //for (int i = 0; i < count; i++)
    //        //{
    //        //    Resource.Types id = (Resource.Types)reader.ReadByte();
    //        //    Resource res = Resource.Create(id, reader.ReadSingle(), reader.ReadSingle());
    //        //    this[res.Name] = res;
    //        //}
    //    }

    //    static public ResourceDef GetResource(GameObject entity, ResourceDef.ResourceTypes type)
    //    {
    //        var resources = entity.GetComponent<ResourcesComponent>();
    //        if (resources == null)
    //            return null;
    //        ResourceDef resource;
    //        resources.Resources.TryGetValue(type, out resource);
    //        return resource;
    //    }
    //}
}
