using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
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
        //public GameObject.Types Product { get { return (GameObject.Types)this["Product"]; } set { this["Product"] = value; } }
        public int Level = 1;//{ get { return (int)this["Level"]; } set { this["Level"] = value; } }
        public PlantProperties Plant;
        //public ItemDef PlantDef;
        //public PlantProperties PlantProps;
        
        //public override void MakeChildOf(GameObject parent)
        //{
        //    parent.Name = this.PlantDef.Name + " seeds";
        //}
        public SeedComponent()
        {
        }
        public SeedComponent(SeedComponent toCopy)
        {
            //this.PlantDef = toCopy.PlantDef;
            this.Plant = toCopy.Plant;
            //this.PlantProps = toCopy.PlantProps;
            this.Level = toCopy.Level;
        }
        //public SeedComponent(ItemDef plantDef)
        //{
        //    this.PlantDef = plantDef;
        //}
        public void SetPlant(PlantProperties props)
        {
            this.Plant = props;
            this.Parent.Name = $"{props.Name} {this.Parent.Def.Label}";
            //this.PlantDef = props.PlantEntity;
        }

        public override object Clone()
        {
            return new SeedComponent(this);
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            //actions[PlayerInput.RButton]=  new Interactions.Planting();
        }
        //public override void Select(UISelectedInfo info, GameObject parent)
        //{
        //    info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", GameObject.Objects[this.Product].Name) });
        //}
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", this.Plant.Name) });
            //info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", this.PlantDef.Label) });
        }
        public override void GetHauledActions(GameObject parent, TargetArgs target, List<Interaction> actions)
        {
            return;
            //actions.Add(new Interactions.Planting());
        }

        internal override void AddSaveData(SaveTag tag)
        {
            //tag.Add(this.PlantDef.Save("PlantDef"));
            this.Plant.Save(tag, "Plant");
        }
        internal override void Load(SaveTag tag)
        {
            //this.PlantDef = tag.LoadDef<ItemDef>("PlantDef");
            this.Plant = tag.LoadDef<PlantProperties>("Plant");
        }
        public override void Write(BinaryWriter w)
        {
            this.Plant.Write(w);
            //this.PlantDef.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.Plant = Def.GetDef<PlantProperties>(r.ReadString());
            //this.PlantDef = r.ReadDef<ItemDef>();
        }

        public class Props : ComponentProps
        {
            public override Type CompType => typeof(SeedComponent);
        }
    }
}
