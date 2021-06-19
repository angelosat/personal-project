using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    public class FarmlandComponent : Component// BlockComponent// : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "Farmland"; }
        }

        GameObject Product { get { return (GameObject)this["Product"]; } set { this["Product"] = value; } }
        float Value { get { return (float)this["Value"]; } set { this["Value"] = value; } }
        TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
        double Length { get { return (double)this["Duration"]; } set { this["Duration"] = value; } }

        public FarmlandComponent()
        {
            //this.Type = Block.Types.Farmland;
            //this.ProductID = 0;
            this.Length = 0;
            Properties[Stat.ValueOld.Name] = 0f;
            this.Time = TimeSpan.Zero;
            this.Properties.Add("Product");

        }

        internal override void GetAvailableActions(List<Script> list)
        {
            list.Add(Script.Registry[Script.Types.Planting]);
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk)
        {
            if (Product == null)
            {
                net.DisposeObject(parent);
                //Chunk.RemoveBlockObject(parent);
                return;
            }

            //if (this.Length <= 0)
            if(this.Value >= this.Length)
            {
                Sprout(net, parent);
            }
            //this.Length--;// -= 1000 / Engine.TargetFps;
            this.Value++;

            //TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(1000 / Engine.TargetFps));
            //this.Time = this.Time.Subtract(sub);

            //if (this.Time.TotalMilliseconds <= 0)
            //{
            //    net.InstantiateObject(GameObject.Create(Product.ID).SetGlobal(parent.Global + Vector3.UnitZ));
            //    parent.Global.GetCell(parent.Map).Variation = 0;
            //    net.DestroyObject(parent.NetworkID);
            //    //Chunk.RemoveBlockObject(parent);
            //}
        }

        private void Sprout(Net.IObjectProvider net, GameObject parent)
        {
            //net.InstantiateObject(GameObject.Create(Product.ID).SetGlobal(parent.Global + Vector3.UnitZ));
            GameObject pr = GameObject.Create(Product.ID);
            net.Spawn(pr, parent.Global + Vector3.UnitZ);//

            //parent.Global.GetCell(net.Map).Variation = 0;
            //Chunk.RemoveBlockObject(net.Map, parent.Global);
            net.Map.GetCell(parent.Global).Variation = 0;
            Chunk.RemoveBlockObject(net.Map, parent.Global);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                //case Message.Types.Give:
                //    Plant(parent, e.Sender, e.Parameters[0] as GameObjectSlot);//, (Vector3)e.Parameters[0]);
                //    return true;
                
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                case Message.Types.Plant:
                    Plant(parent, e.Sender, e.Parameters[1] as GameObjectSlot);
                    break;

                case Message.Types.UsedItem:
                    //TargetArgs item = e.Data.
                    e.Data.Translate(e.Network, r =>
                    {
                        TargetArgs actor = TargetArgs.Read(e.Network, r);
                        TargetArgs item = TargetArgs.Read(e.Network, r);
                        Vector3 face = r.ReadVector3();

                        //actor.Object.Name.ToConsole();
                        //item.Object.Name.ToConsole();
                        //face.ToConsole();
                    });

                    var args = e.Data.Translate<UseItemEventArgs>(e.Network);

                    //Interaction.StartNew(e.Network, args.SourceEntity.Object, new TargetArgs(parent),
                    //    new Script(Script.Types.Dynamic, a =>
                    //    {
                    //        this.UsedItem(parent, args);
                    //    }, "Planting", 1, null, null, null));

                    InteractionOld.StartNew(e.Network, args.SourceEntity.Object, new TargetArgs(parent),  // do i need to pass the target?
                        () =>
                        {
                            this.UsedItem(e.Network, parent, args);
                        }, "Planting", 1000, null, null, null);

                    //UsedItem(parent, args);
                    return true;

                default:
                    //return true;
                    return base.HandleMessage(parent, e);
            }
            return true;
        }

        private void UsedItem(Net.IObjectProvider net, GameObject parent, UseItemEventArgs args)
        {
            if (args.UsedItem.Object.GetComponent<SeedComponent>() == null)
                return;

            var foundSlot = (
              //  from cont in args.SourceEntity.Object.GetComponent<ContainerComponent>().Containers
                from slot in args.SourceEntity.Object.GetChildren() // cont
                where slot.Object == args.UsedItem.Object
                select slot).SingleOrDefault();
            if (foundSlot.IsNull())
                return;
            //if (args.UsedItem.Object != foundSlot.Take())
            //    throw new ArgumentException();

            //Interaction.StartNew(

            Plant(parent, foundSlot.Take());
            //parent.Global.GetCell(net.Map).Variation = 1;
            //Chunk.AddBlockObject(net.Map, parent, parent.Global);
            net.Map.GetCell(parent.Global).Variation = 1;
            Chunk.AddBlockObject(net.Map, parent, parent.Global);
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    //if (FunctionComponent.HasAbility((e.Sender["Inventory"]["Holding"] as GameObjectSlot).Object, Message.Types.Plant))
        //    //    (e.Parameters[0] as List<Interaction>).Add(new Interaction(new TimeSpan(0, 0, 1), Message.Types.Plant, parent, "Plant", "Planting", stat: Stat.Farming, range: (float)Math.Sqrt(2), need: new Need() { Name = "Work", Value = 20 }));// need: "Work"));
            
        //    list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Plant, parent, "Plant", "Planting", 
        //        cond: new ConditionCollection(
        //                new Condition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Plant), "Requires: " + Message.Types.Plant)),
        //        stat: Stat.Farming, need: new Need() { Name = "Work", Value = 20 }));// need: "Work"));
        //}

        public bool Plant(GameObject parent, GameObject sender, GameObjectSlot objSlot)//, Vector3 face)//GameObject obj)
        {
          //  Vector3 global = parent.Global + face;
            //GameObjectSlot holding = sender.GetComponent<GearComponent>().Holding; // = sender["Inventory"]["Holding"] as GameObjectSlot;
            GameObjectSlot holding = sender.GetComponent<HaulComponent>().Holding; // = sender["Inventory"]["Holding"] as GameObjectSlot;

            //PersonalInventoryComponent.TryGetHeldObject(sender, out holding);
            if (!holding.HasValue)
                return true;
            if (holding.Object != objSlot.Object)
                return true;

            GameObject obj = objSlot.Object;
            SeedComponent seed;
            if (!obj.TryGetComponent<SeedComponent>("Seed", out seed))
                return true;
            //parent.Global.GetCell(parent.Map).Variation = 1;
            parent.Map.GetCell(parent.Global).Variation = 1;

            parent["Sprite"]["Variation"] = 1;

           // Product = (GameObject.Types)seed["Product"];
            Product = GameObject.Objects[(GameObject.Types)seed["Product"]];
          // Chunk.AddObject(parent, parent.Map, parent.Global);
          //  parent.Spawn(parent.Map, parent.Global);
            Chunk.AddBlockObject(parent.Map, parent, parent.Global);
            objSlot.StackSize--;
         // this.Value = 240f;
            this.Time = new TimeSpan(0, 0, 6);
            this.Length = Engine.TargetFps * seed.Level * 60;
            this.Value = 0;
            return true;
        }
        public bool Plant(GameObject parent, GameObject seed)//, Vector3 face)//GameObject obj)
        {
            GameObject obj = seed;

            Product = GameObject.Objects[seed.GetComponent<SeedComponent>().Product];
            //ProductID = Product.ID;
            //parent.Global.GetCell(parent.Map).Variation = 1;

            this.Length = 50000;
         //   this.Time = new TimeSpan(0, 0, 6);
            return true;
        }
        public override object Clone()
        {
            FarmlandComponent comp = new FarmlandComponent();
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            return comp;
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Length);
            writer.Write(this.Value);
            if (Length > 0)
                writer.Write((byte)this.Product.ID);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Length = reader.ReadDouble();
            this.Value = reader.ReadSingle();
            if (Length > 0)
                this.Product = GameObject.Objects[(GameObject.Types)reader.ReadByte()];
        }

        static public FarmlandComponent Create(GameObject obj)
        {
            //BlockComponent.Create(obj, Block.Types.Farmland, hasData: true, transparency: 0f, density: 1f);
            return new FarmlandComponent();
        }

        // TODO: only save the product ID if value > 0
        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            data.Add(new SaveTag(SaveTag.Types.Int, "ProductID", this.Product != null ? (int)this.Product.ID : -1));
            data.Add(new SaveTag(SaveTag.Types.Float, "Value", this.Value));
            data.Add(new SaveTag(SaveTag.Types.Double, "Duration", this.Length));
            return data;
        }

        internal override void Load(SaveTag compTag)
        {
            int prodID = (int)compTag["ProductID"].Value;
            if (prodID > -1)
                this.Product = GameObject.Objects[(GameObject.Types)compTag["ProductID"].Value];
            this.Value = (float)compTag["Value"].Value;
            this.Length = compTag.TagValueOrDefault<double>("Duration", 0);// (double)compTag["Duration"].Value;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            if (this.Product.IsNull())
                return;
            if (SkillCheck(Player.Actor))
            {
                tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Product: " + (Product != null ? Product.Name : "")));
                TimeSpan time = TimeSpan.FromSeconds((this.Length - this.Value) / (double)Engine.TargetFps);
                tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Time left: " + (Product != null ? time.ToString(@"hh\:mm\:ss") : "")));
            }
            else
            {
                tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Product: ???"));
                tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Time left: ???"));
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Low Argiculture Skill!") { TextColorFunc = () => Color.Red });
            }
        }

        public override void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.Product.IsNull())
                return;

            if(SkillCheck(Player.Actor))
                Bar.Draw(sb, camera, parent, this.Product.Name, this.Value / (float)this.Length);
            else
                Bar.Draw(sb, camera, parent, "Low skill", 0);

            //PlantComponent plant;
            //this.Product.TryGetComponent<PlantComponent>(out plant);
            //int lvl = SkillsComponent.GetSkillLevel(Player.Actor, Skill.Types.Argiculture);
            //if (lvl < plant.Level)
            //    Bar.Draw(sb, camera, parent, "Low skill", 0);
            //else
            //    Bar.Draw(sb, camera, parent, this.Product.Name, this.Value / (float)this.Length);
        }
        bool SkillCheck(GameObject actor)
        {
            PlantComponent plant;
            this.Product.TryGetComponent<PlantComponent>(out plant);
            int lvl = SkillsComponent.GetSkillLevel(Player.Actor, SkillOld.Types.Argiculture);
            if (lvl < plant.Level)
                return false;
            else
                return true;
        }
    }
}
