using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Start_a_Town_.Components
{
    class ResourcesComponent : Component
    {
        public Dictionary<Resource.Types, Resource> Resources { get { return (Dictionary<Resource.Types, Resource>)this["Resources"]; } set { this["Resources"] = value; } }
        //ResourceCollection Resources { get { return (ResourceCollection)this["Resources"]; } set { this["Resources"] = value; } }
        public override string ComponentName
        {
            get
            {
                return "Resources";
            }
        }
        public ResourcesComponent()
        {
            this.Resources = new Dictionary<Resource.Types, Resource>();
        }

        public override void Update(GameObject parent)
        {
            //this.Resources.Update(parent);
            foreach (var item in this.Resources.Values)
                item.Update(parent);
        }
        static public bool HasResource(GameObject entity, Resource.Types type)// Resource resource)
        {
            ResourcesComponent comp;
            if (!entity.TryGetComponent<ResourcesComponent>(out comp))
                return false;
            return comp.Resources.ContainsKey(type);
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            //foreach (var prop in from prop in this.Properties.Values where prop is Resource select prop as Resource)
            //{
            //    prop.HandleMessage(parent, e);
            //}

            //return true;
            foreach (var item in this.Resources.Values)
                item.HandleMessage(parent, e);

            switch(e.Type)
            {
                default:
                    break;
            }
            return false;
        }

        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            foreach (var item in this.Resources.Values)
                item.HandleRemoteCall(parent, e);
        }

        public ResourcesComponent Initialize(params Resource[] resources)
        {
            //resources.ToList().ForEach(foo => this[foo.Name] = foo);
            this.Resources = resources.ToDictionary(foo => foo.ID, foo => foo);
            return this;
        }
        public ResourcesComponent(params Resource[] resources)
        {
            //resources.ToList().ForEach(foo => this[foo.Name] = foo);
            this.Resources = resources.ToDictionary(foo => foo.ID, foo => foo);
        }

        public override void OnNameplateCreated(UI.Nameplate plate)
        {
            foreach (var res in this.Resources.Values)
                res.OnNameplateCreated(plate);
        }
        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate)
        {
            foreach (var res in this.Resources.Values)
                res.OnHealthBarCreated(parent, plate);
        }
        public override object Clone()
        {
            //return new ResourcesComponent(this.Properties.Values.Select(foo => foo as Resource).ToArray());
            ResourcesComponent comp = new ResourcesComponent();

            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }

            return comp;
        }

        public override string ToString()
        {
            string text = "";
            foreach (var item in this.Resources.Values)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.Resources.Count);
            foreach (var res in this.Resources.Values)
            {
                writer.Write((byte)res.ID);
                res.Write(writer);
            }
            //List<Resource> resources =
            //    (from prop in this.Properties.Values
            //     where prop is Resource
            //     select prop as Resource).ToList();
            //writer.Write(resources.Count);
            //foreach (var res in resources)
            //{
            //    writer.Write((byte)res.ID);
            //    res.Write(writer);
            //}
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Resource.Types id = (Resource.Types)reader.ReadByte();
                Resource res = Resource.Create(id, reader.ReadSingle(), reader.ReadSingle());
                this.Resources[id] = res;
            }
            //int count = reader.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    Resource.Types id = (Resource.Types)reader.ReadByte();
            //    Resource res = Resource.Create(id, reader.ReadSingle(), reader.ReadSingle());
            //    this[res.Name] = res;
            //}
        }
    }
}
