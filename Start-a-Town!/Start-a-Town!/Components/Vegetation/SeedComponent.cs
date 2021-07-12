using Start_a_Town_.UI;
using System;
using System.IO;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class SeedComponent : EntityComponent
    {
        readonly static public string Name = "Seed";

        public override string ComponentName
        {
            get
            {
                return Name;
            }
        }
        public int Source;
        public int Level = 1;
        public PlantProperties Plant;
        
        public SeedComponent()
        {
        }
        public SeedComponent(SeedComponent toCopy)
        {
            this.Plant = toCopy.Plant;
            this.Level = toCopy.Level;
        }
       
        public void SetPlant(PlantProperties props)
        {
            this.Plant = props;
            this.Parent.Name = $"{props.Name} {this.Parent.Def.Label}";
        }

        public override object Clone()
        {
            return new SeedComponent(this);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", this.Plant.Name) });
        }
        
        internal override void AddSaveData(SaveTag tag)
        {
            this.Plant.Save(tag, "Plant");
        }
        internal override void Load(SaveTag tag)
        {
            this.Plant = tag.LoadDef<PlantProperties>("Plant");
        }
        public override void Write(BinaryWriter w)
        {
            this.Plant.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.Plant = Def.GetDef<PlantProperties>(r.ReadString());
        }

        public class Props : ComponentProps
        {
            public override Type CompType => typeof(SeedComponent);
        }
    }
}
