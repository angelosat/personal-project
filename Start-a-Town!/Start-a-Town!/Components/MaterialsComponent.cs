using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public class PartMaterialPair
    {
        public string Name { get; set; }
        public Material Material { get; set; }
        public PartMaterialPair(string name, Material material)
        {
            this.Name = name;
            this.Material = material;
        }
    }
    [Obsolete]
    class MaterialsComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Materials";
            }
        }

        public override object Clone()
        {
            var newcomp = new MaterialsComponent(this.Parts);
            return newcomp; //should i clone?
        }

        public Dictionary<string, PartMaterialPair> Parts { get { return (Dictionary<string, PartMaterialPair>)this["Parts"]; } 
            set { this["Parts"] = value; } }
        
        public MaterialsComponent Initialize(params PartMaterialPair[] parts)
        {
            //this.Parts = new List<PartMaterialPair>(parts);
            this.Parts = parts.ToDictionary(foo => foo.Name, foo => foo);
            return this;
        }

        public override void OnSpawn(IObjectProvider net, GameObject parent)
        {
            this.OnObjectLoaded(parent);
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            foreach (var item in this.Parts.Values)
            {
                Sprite ol;
                if (parent.Body.Sprite.Overlays.TryGetValue(item.Name, out ol))
                {
                    ol.Tint = item.Material.Color;
                    ol.Shininess = item.Material.Type.Shininess;
                }
            }
        }
        public override void Instantiate(GameObject parent, Action<GameObject> instantiator)
        {
            this.OnObjectLoaded(parent);
        }

        public MaterialsComponent()
        {
            //this.Parts = new List<PartMaterialPair>();
            this.Parts = new Dictionary<string, PartMaterialPair>();
        }
        MaterialsComponent(Dictionary<string, PartMaterialPair> dictionary)
        {
            this.Parts = new Dictionary<string, PartMaterialPair>(dictionary);
        }
        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            GroupBox tip = new GroupBox() { Location = tooltip.Controls.BottomLeft };
            foreach (var item in this.Parts.Values)
                tip.Controls.Add(new Label(item.Name + ": " + item.Material.Name) { Location = tip.Controls.BottomLeft, TextColorFunc = () => item.Material.Color, Font = UIManager.FontBold });
            tooltip.Controls.Add(tip);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Parts.Count);
            foreach(var item in this.Parts.Values)
            {
                w.Write(item.Name);
                w.Write(item.Material.ID);
            }
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Parts = new Dictionary<string, PartMaterialPair>();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = r.ReadString();
                this.Parts.Add(name, new PartMaterialPair(name, Material.Registry[r.ReadInt32()]));
            }
        }
        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            var list = new SaveTag(SaveTag.Types.List, "Parts", SaveTag.Types.Compound);
            foreach (var item in this.Parts.Values)
            {
                SaveTag itemTag = new SaveTag(SaveTag.Types.Compound);
                itemTag.Add(new SaveTag(SaveTag.Types.String, "Name", item.Name));
                itemTag.Add(new SaveTag(SaveTag.Types.Int, "Material", item.Material.ID));
                list.Add(itemTag);
            }
            save.Add(list);
            return save;
        }
        internal override void Load(SaveTag save)
        {
            this.Parts = new Dictionary<string, PartMaterialPair>();
            var list = save["Parts"].Value as List<SaveTag>;
            foreach(var item in list)
            {
                if(item.Value.IsNull())
                    continue;
                //var tag = item.Value as SaveTag;
                string name = item.GetValue<string>("Name");
                Material material = Material.Registry[item.GetValue<int>("Material")];
                this.Parts.Add(name, new PartMaterialPair(name, material));
            }
        }

        static public bool IsFuel(GameObject item)
        {
            var comp = item.GetComponent<MaterialsComponent>();
            if (comp == null)
                return false;
            return comp.Parts.Values.Any(mat => mat.Material.Fuel.Value > 0);
        }
    }    
}
