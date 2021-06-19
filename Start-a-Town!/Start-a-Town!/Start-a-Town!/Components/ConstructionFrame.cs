using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class ItemRequirement
    {
        public string Name = "";
        public int ObjectID;
        public int Amount, Max;

        public int Remaining
        {
            get { return this.Max - this.Amount; }
            set { this.Amount = this.Max - value; }
        }

        public ItemRequirement(GameObject.Types id, int max, int amount = 0)
        {
            this.ObjectID = (int)id;
            this.Max = max;
            this.Amount = amount;
        }
        public ItemRequirement(ItemRequirement toCopy)
        {
            this.ObjectID = toCopy.ObjectID;
            this.Max = toCopy.Max;
            this.Amount = toCopy.Amount;
        }
        public ItemRequirement(SaveTag tag)
        {
            this.Name = tag.GetValue<string>("Name");
            this.ObjectID = tag.GetValue<int>("Material");
            this.Max = tag.GetValue<int>("Max");
            this.Amount = tag.GetValue<int>("Amount");
        }
        public List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Material", this.ObjectID));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Amount", this.Amount));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Max", this.Max));
            return tag;
        }
        public ItemRequirement(BinaryReader r)
        {
            this.Name = r.ReadString();
            this.ObjectID = r.ReadInt32();
            this.Max = r.ReadInt32();
            this.Amount = r.ReadInt32();
            this.Name = r.ReadString();
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Name);
            w.Write((int)this.ObjectID);
            w.Write(this.Max);
            w.Write(this.Amount);
            w.Write(this.Name);
        }

        public bool Full { get { return this.Amount == this.Max; } }

        public SlotWithText GetUI(Vector2 loc)
        {
            SlotWithText slotReq = new SlotWithText(loc) { Tag = GameObject.Objects[this.ObjectID].ToSlot() };
            slotReq.Slot.CornerTextFunc = o => this.Amount.ToString() + "/" + this.Max.ToString();
            //slotReq.OnGameEventAction = (e) =>
            //{
            //    if (e.Type != Message.Types.InventoryChanged)
            //        return;
            //    var obj = e.Parameters[0] as GameObject;
            //    if (obj != parent)
            //        return;
            //    slotReq.Invalidate(true);
            //};
            return slotReq;
        }
    }
    class ConstructionFrame : Component
    {
        public override string ComponentName
        {
            get { return "Construction"; }
        }
        static public bool AutoShow = true;

        public GameObjectSlot Blueprint { get { return (GameObjectSlot)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        public GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        public List<ItemRequirement> Materials { get { return (List<ItemRequirement>)this["Materials"]; } set { this["Materials"] = value; } }
        public GameObject.Types Reserved { get { return (GameObject.Types)this["Reserved"]; } set { this["Reserved"] = value; } }

        public ConstructionFrame()
        {
            this.Blueprint = GameObjectSlot.Empty;
            this.Product = GameObjectSlot.Empty;
            this.Materials = new List<ItemRequirement>();
            this.Reserved = GameObject.Types.Default;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.Activate:
                    //e.Translate(r =>
                    //{
                        GameObject actor = e.Parameters[0] as GameObject;// TargetArgs.Read(e.Network, r);
                        //GameObjectSlot materialSlot = actor.GetComponent<GearComponent>().Holding;
                        GameObjectSlot materialSlot = actor.GetComponent<HaulComponent>().Holding;

                        GameObject materialObj = materialSlot.Object;
                        if (materialObj.IsNull())
                            return true;
                        
                    ItemRequirement req = this.Materials.FirstOrDefault(foo => foo.ObjectID == materialObj.GetInfo().ID);
                    if (req.IsNull())
                        return true;
                    if (req.Full)
                        return true;
                    materialSlot.StackSize--;
                    if (materialSlot.StackSize == 0)
                        e.Network.DisposeObject(materialObj);
                    req.Amount++;
                    //parent.Global.GetChunk(e.Network.Map).Invalidate();//.Saved = true;
                    e.Network.Map.GetChunk(parent.Global).Invalidate();//.Saved = true;

                    return true;

                case Message.Types.SetBlueprint:
                    GameObject.Types bpID = GameObject.Types.Default;
                    e.Data.Translate(e.Network, r =>
                    {
                        bpID = (GameObject.Types)r.ReadInt32();
                    });
                    GameObject bpObj = GameObject.Objects[bpID];
                    Blueprint bp = bpObj.GetComponent<BlueprintComponent>().Blueprint;
                    this.Blueprint.Object = bpObj;
                    this.Product = GameObject.Objects[bp.ProductID].ToSlot();
                    this.Materials.Clear();
                    foreach (var mat in bp.Stages[0])
                        this.Materials.Add(new ItemRequirement(mat.Key, mat.Value));
                    //parent.Global.GetChunk(e.Network.Map).Invalidate();//.Saved = true;
                    e.Network.Map.GetChunk(parent.Global).Invalidate();//.Saved = true;

                    return true;

                //case Message.Types.SetBlueprint:
                //   // GameObject.Types objID = (GameObject.Types)e.Parameters[0];
                //    //GameObject bpObj = WorkbenchComponent.Blueprints.First(foo => foo["Blueprint"].GetProperty<Blueprint>("Blueprint").ProductID == objID);
                //    GameObject bpObj = e.Parameters[0] as GameObject;//
                //    Blueprint bp = bpObj["Blueprint"].GetProperty<Blueprint>("Blueprint");
                //    Block.Types block = (Block.Types)GameObject.Objects[bp.ProductID]["Physics"]["Type"]; //.GetProperty<Block.Types>
                //    //this.Product = GameObject.Objects[objID].ToSlot();
                //    this.Product = GameObject.Objects[bp.ProductID].ToSlot();
                //    this.Materials.Clear();
                //    foreach (var mat in bp.Stages[0])
                //        this.Materials.Add(new ItemRequirement(mat.Key, mat.Value));
                ////    "done".ToConsole();
                //    return true;

                case Message.Types.DropOn:
                    GameObjectSlot haulSlot;
                    if (!InventoryComponent.TryGetHeldObject(e.Sender, out haulSlot))
                        return true;
                    GameObject hauling = haulSlot.Object;
                    req = this.Materials.FirstOrDefault(foo => foo.ObjectID == hauling.GetInfo().ID);
                    if (req.IsNull())
                        return true;
                    if (req.Full)
                        return true;
                    haulSlot.StackSize--;
                    req.Amount++;
                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulSlot, haulSlot.Object);
                    return true;

                case Message.Types.Construct:
                    //GameObject.Create(this.Product.Object.ID, parent.Map, parent.Global);
                    //e.Network.InstantiateObject(GameObject.Create(this.Product.Object.ID).SetGlobal(parent.Global));
                    GameObject builder = e.Parameters[0] as GameObject;
                    Net.IObjectProvider net = e.Network;
                    FinishConstruction(parent, builder, net);
                    return true;

                case Message.Types.Crafted:
                    GameObject.Types reservedMat = (GameObject.Types)e.Parameters[1];
                    this.Reserved = reservedMat;
             //       throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.SkillAward, parent, Skill.Types.Construction, 1);
                    return true;

                case Message.Types.Chop:
                    LootTable table = new LootTable(new Loot(this.Reserved, 1, 1));
                    foreach (var mat in this.Materials)
                        table.Add(new Loot(mat.ObjectID, 1, mat.Amount));
                    e.Network.PopLoot(table, parent.Global, parent.Velocity);
                    Chunk.RemoveBlockObject(e.Network.Map, parent.Global);
                    parent.Global.TrySetCell(e.Network, Block.Types.Air);
                    return true;

                default:
                    return false;
            }
        }

        private void FinishConstruction(GameObject parent, GameObject builder, Net.IObjectProvider net)
        {
            GameObject objBlock = GameObject.Create(this.Product.Object.ID);//.SetGlobal(target.Object.Global + target.Face);
            objBlock.Global = parent.Global;
            objBlock.Spawn(net);

            Chunk.RemoveBlockObject(net.Map, parent.Global);
            //        throw new NotImplementedException();
            //e.Sender.PostMessage(Message.Types.SkillAward, parent, Skill.Types.Construction, 1);
            RecoverMaterials(net, parent, builder);
        }
        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Set Construction", () =>
            {
                parent.GetUi().Show();
                //return true;
            }));
        }
        private void RecoverMaterials(Net.IObjectProvider net, GameObject parent, GameObject actor)
        {
            float chance = StatsComponent.GetStatOrDefault(actor, Stat.Types.MatRecover, 0f);
            LootTable table = new LootTable(new Loot(this.Reserved, chance));
            net.GenerateLoot(table);
            //if ((float)Engine.Random.NextDouble() > chance)
            //    return;
            //Loot.PopLoot(parent, GameObject.Create(this.Reserved));
        }

        public override void Query(GameObject parent, List<InteractionOld> actions)
        {
            actions.Add(
                new InteractionOld(
                    TimeSpan.Zero,
                    (actor, target) =>
                    {
                        throw new NotImplementedException();
                        //actor.PostMessage(Message.Types.Interface, parent);
                    },
                    parent,
                    name: "Set Construction"
                    ));

            actions.Add(
               new InteractionOld(
                   TimeSpan.FromSeconds(1),
                   (actor, target) =>
                   {
                       throw new NotImplementedException();
                       //target.Object.PostMessage(Message.Types.Death, actor);
                   },
                   parent,
                   name: "Dismantle",
                   verb: "Dismantling"
                   )
                   );

            actions.Add(
                new InteractionOld(
                TimeSpan.FromSeconds(1),
                (actor, target) =>
                {
                    throw new NotImplementedException();
                    //target.Object.PostMessage(Message.Types.FinishConstruction, actor);
                },
                parent,
                "Finish construction",
                cond:
                new ConditionCollection(
                    new Condition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build), "I need a tool to " + Message.Types.Build.ToString().ToLower() + " with.",
                        new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)),
                //        ),
                //targetCond:
                //new InteractionConditionCollection(
                        new Condition((actor, target) => this.Product != null, "Blueprint not set!"),
                        new Condition((actor, target) => this.Materials.TrueForAll(r => r.Amount == r.Max), "Materials missing!",
                            new Precondition("Materials", i => i.Message == Message.Types.ApplyMaterial && i.Source == parent, AI.PlanType.FindNearest)
                        )
                    )
                )
                );
        }

        public override object Clone()
        {
            return new ConstructionFrame();
        }

        //public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> handlers)
        {
            var p = new Panel() { AutoSize = true };
            var list = new ListBox<GameObjectSlot, SlotWithText>(new Rectangle(0, 0, 200, 200));
            list.Build(
                WorkbenchComponent.Blueprints.FindAll(foo => foo.Type == ObjectType.Plan).Select(foo => foo.ToSlot()),
                foo => foo.Object.Name,
                onControlInit: (obj, ctrl) =>
                {
                    ctrl.Slot.LeftClickAction = () =>
                    {
                      //  throw new NotImplementedException();
                        //parent.PostMessage(Message.Types.SetBlueprint, parent, ctrl.Slot.Tag.Object);//["Blueprint"]["Blueprint"] as Blueprint);
                        Net.Client.PostPlayerInput(parent, Message.Types.SetBlueprint, w =>
                        {
                            //w.Write((int)ctrl.Slot.Tag.Object.GetComponent<BlueprintComponent>().Blueprint.ProductID);
                            w.Write((int)ctrl.Slot.Tag.Object.ID);
                        });
                        ui.TopLevelControl.Hide();
                    };
                });
            p.Controls.Add(list);
            ui.Controls.Add(p);
            ui.Controls.Add(new CheckBox("Auto open", p.BottomLeft, AutoShow) { LeftClickAction = () => AutoShow = !AutoShow });
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(
                new Label(tooltip.Controls.BottomLeft, "Product:"));
            tooltip.Controls.Add(
                new SlotWithText(tooltip.Controls.BottomLeft) { Tag = this.Product }
                );
            tooltip.Controls.Add(
                new Label(tooltip.Controls.BottomLeft, "Materials:"));
            foreach (var mat in this.Materials)
            {
                tooltip.Controls.Add(
                    new SlotWithText(tooltip.Controls.BottomLeft).SetTag(GameObject.Objects[mat.ObjectID].ToSlot()).SetText(GameObject.Objects[mat.ObjectID].Name + " " + mat.Amount + "/" + mat.Max));
            }
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "Reserved_Material", (int)this.Reserved));

            if (!this.Blueprint.HasValue)
                return tag;

            tag.Add(new SaveTag(SaveTag.Types.Int, "Blueprint", this.Blueprint.Object.ID));
           // tag.Add(new SaveTag(SaveTag.Types.Byte, "Material_Count", (byte)this.Materials.Count));
            SaveTag matsTag = new SaveTag(SaveTag.Types.List, "Materials", SaveTag.Types.Compound);
            foreach (var mat in this.Materials)
            {
                SaveTag matTag = new SaveTag(SaveTag.Types.Compound, "Material");
                matTag.Add(new SaveTag(SaveTag.Types.Int, "ID", mat.ObjectID));
                matTag.Add(new SaveTag(SaveTag.Types.Int, "Amount", mat.Amount));
                matTag.Add(new SaveTag(SaveTag.Types.Int, "Max", mat.Max));
                matsTag.Add(matTag);
            }
            tag.Add(matsTag);

            return tag;
        }

        internal override void Load(SaveTag tag)
        {
            this.Reserved = (GameObject.Types)tag.GetValue<int>("Reserved_Material"); //GameObject.Types.WoodenPlank;//
            SaveTag bptag;
            if (!tag.TryGetTag("Blueprint", out bptag))
                return;
            var bpID = (GameObject.Types)tag["Blueprint"].Value;

            //this.Blueprint.Object = GameObject.Objects[bpID];
            //this.Product.Object = GameObject.Objects[this.Blueprint.Object.GetComponent<BlueprintComponent>().Blueprint.ProductID];
            //foreach (var matTag in (List<SaveTag>)tag["Materials"].Value)
            //{
            //    this.Materials.Add(new ItemRequirement((GameObject.Types)matTag.GetValue<int>("ID"), matTag.GetValue<int>("Max"), matTag.GetValue<int>("Amount")));
            //}
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write((int)this.Reserved);
            writer.Write(this.Blueprint.HasValue);
            if (!this.Blueprint.HasValue)
                return;
            writer.Write((int)this.Blueprint.Object.ID);
            writer.Write(this.Materials.Count);
            foreach (var mat in this.Materials)
            {
                writer.Write((int)mat.ObjectID);
                writer.Write(mat.Amount);
                writer.Write(mat.Max);
            }
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Reserved = (GameObject.Types)reader.ReadInt32();
            bool hasBlueprint = reader.ReadBoolean();
            if (!hasBlueprint)
                return;
            this.Blueprint.Object = GameObject.Objects[(GameObject.Types)reader.ReadInt32()];
            this.Product.Object = GameObject.Objects[this.Blueprint.Object.GetComponent<BlueprintComponent>().Blueprint.ProductID];
            int matCount = reader.ReadInt32();
            for (int i = 0; i < matCount; i++)
            {
                GameObject.Types t = (GameObject.Types)reader.ReadInt32();
                int amount = reader.ReadInt32();
                int max = reader.ReadInt32();
                this.Materials.Add(new ItemRequirement(t, max, amount));
            }
        }
    }
}
