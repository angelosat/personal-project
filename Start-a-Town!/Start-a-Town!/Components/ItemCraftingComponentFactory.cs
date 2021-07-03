using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    
    class ItemCraftingComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "CraftingComponent";
            }
        }

        //static List<GameObject> _Dictionary;
        public static HashSet<int> Registry = new HashSet<int>();
        //{
        //    get
        //    {
        //        if (_Dictionary.IsNull())
        //            Initialize();
        //        return _Dictionary;
        //    }
        //}

        public override object Clone()
        {
            return new ItemCraftingComponent() { Material = this.Material }; //should i clone?
        }


        public Material Material { get { return (Material)this["Material"]; } set { this["Material"] = value; } }


        public ItemCraftingComponent Initialize()
        {
            this.Material = MaterialDefOf.LightWood;
            return this;
        }
        public ItemCraftingComponent Initialize(Material material)
        {
            this.Material = material;
            return this;
        }

        public override void MakeChildOf(GameObject parent)
        {
            Registry.Add((int)parent.IDType);
        }
        public ItemCraftingComponent()
        {
            this.Material = MaterialDefOf.LightWood;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            switch (e.Type)
            {
                //case Message.Types.Activate:
                //    //float value = GetProperty<float>(Stat.Value.Name) - GlobalVars.DeltaTime;
                //    //Properties[Stat.Value.Name] = value;
                //    //if (value <= 0)
                //    //{
                //    //    Map.RemoveObject(parent);
                //    //    GameObject obj = GameObjectDb.Construction;
                //    //    obj.GetComponent<ConstructionComponent>("Construction").Add(parent);
                //    //    Chunk.AddObject(obj, parent.Global);
                //    //    return true;
                //    //}
                //    //return false;
                //    e.Sender.HandleMessage(new GameObjectEventArgs(Message.Types.Craft, parent, parent));
                //    return true;

                //case Message.Types.Query:
                //    //Dictionary<Message.Types, Interaction> lengths = e.Parameters[0] as Dictionary<Message.Types, Interaction>;
                //    //lengths.Add(Message.Types.Mechanical, new Interaction(Message.Types.Mechanical, "Build", stat: Stat.WorkSpeed));

                //    Query(e.Parameters[0] as List<Interaction>);
                //    return true;

                case Message.Types.Build:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.Craft, parent, parent);
                    //return true;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                default:
                    break;
            }
            return false;
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //   // List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Build, parent, "Build"));//, stat: Stat.WorkSpeed));
        //}

        public override string GetTooltipText()
        {
            return "Right click: Craft " + this.GetProperty<float>(Stat.ValueOld.Name).ToString();
        }

        

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            //tooltip.Controls.Add("Used in recipes:".ToLabel(tooltip.Controls.BottomLeft));
            //Recipe.GetRecipes(parent).ForEach(r =>
            //{
            //    tooltip.Controls.Add(new UI.Label(tooltip.Controls.BottomLeft, r.Name)
            //    {
            //        Active = true,
            //        Font = UI.UIManager.FontBold,
            //        TooltipFunc = (t) => r.ToObject().GetTooltip(t),
            //        LeftClickAction = () => r.ToObject().GetTooltip().ToWindow().Show()
            //    });
            //});
            GroupBox tip = new GroupBox() { Location = tooltip.Controls.BottomLeft };
            tip.Controls.Add(new Label(this.Material.Prefix) { TextColorFunc = () => this.Material.Color, Font = UIManager.FontBold });
            
            tooltip.Controls.Add(tip);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.Material.ID);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Material = Material.Database[reader.ReadInt32()];
        }
        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Int, "Material", this.Material.ID));
            return save;
        }
        internal override void Load(SaveTag save)
        {
            save.TryGetTagValue<int>("Material", v => this.Material = Material.Database[v]);
        }
    }    
}
