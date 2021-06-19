using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    class ConstructionComponent : Component
    {
        public override string ComponentName
        {
            get { return "Construction"; }
        }
        static Dictionary<Vector3, GameObject> DesignatedConstructions = new Dictionary<Vector3, GameObject>();

        static public void Initialize()
        {
            DesignatedConstructions = new Dictionary<Vector3, GameObject>();
        }

        GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        Blueprint Blueprint { get { return (Blueprint)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        public List<ItemRequirement> Materials { get { return (List<ItemRequirement>)this["Materials"]; } set { this["Materials"] = value; } }
      //  Dictionary<GameObject.Types, int> Materials { get { return (Dictionary<GameObject.Types, int>)this["Materials"]; } set { this["Materials"] = value; } }
        int Stage { get { return (int)this["Stage"]; } set { this["Stage"] = value; } }
        Dictionary<GameObject.Types, int> CurrentMaterials { get { return (Dictionary<GameObject.Types, int>)this["CurrentMaterials"]; } set { this["CurrentMaterials"] = value; } }
        public ConstructionComponent()
        {
            //this.Materials = new Dictionary<GameObject.Types, int>();
            this.Product = GameObjectSlot.Empty;
            this.Materials = new List<ItemRequirement>();
            this.Properties.Add("Blueprint");

            this.CurrentMaterials = new Dictionary<GameObject.Types, int>();
            this.Stage = 0;
            this[Stat.Value.Name] = 60f;
        }
        //public ConstructionComponent(Blueprint bp, bool keepPlacing = false) : this()
        //{
        //    Dictionary<GameObject.Types, int> currentMats = new Dictionary<GameObject.Types, int>();
        //    this["Product"] = GameObject.Create(bp.ProductID);
        //    this["Blueprint"] = bp;
        //    this["CurrentMaterials"] = currentMats;
        //    //this[Stat.Stage.Name] = 0;

        //    foreach (BlueprintStage stage in bp.Stages)
        //        foreach (KeyValuePair<GameObject.Types, int> mat in stage)
        //            currentMats.Add(mat.Key, 0);

        //}

        //public override string GetWorldText(GameObject actor)
        //{
        //    return GetCurrentMaterials().Count == 0 ? "Awaiting materials" : "Right click: Build";
        //}



        public override object Clone()
        {
            ConstructionComponent craft = new ConstructionComponent();
            Blueprint bp = (Blueprint)this["Blueprint"];
            craft["Blueprint"] = bp;
            craft[Stat.Stage.Name] = this[Stat.Stage.Name];
            if (bp != null)
                foreach (BlueprintStage stage in this.GetProperty<Blueprint>("Blueprint").Stages)
                    foreach (KeyValuePair<GameObject.Types, int> mat in stage)
                        craft.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials").Add(mat.Key, 0);
            return craft;
        }

        //public override bool Drop(GameObject self, GameObject actor, GameObject obj)
        //{
        //    Blueprint bp = this.GetProperty<Blueprint>("Blueprint");
        //    int count, currentcount;
        //    //Dictionary<GameObject.Types, int> currentMats = this.Materials;// this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");

        //    if (bp == null)
        //        return true;

        //    Dictionary<GameObject.Types, int> reqMats = bp.Stages[(int)this[Stat.Stage.Name]];
        //    if (reqMats.TryGetValue(obj.ID, out count))
        //    {
        //        if (currentMats.TryGetValue(obj.ID, out currentcount))
        //            if (currentcount >= count)
        //                return false;

        //        //Task task = new Task(actor, 
        //        //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
        //        //CurrentMaterials[obj.ID] += 1;
        //        return true;
        //    }

        //    return false;
        //}

        public override void ChunkLoaded(Net.IObjectProvider net, GameObject parent)
        {
            if (Materials.Count > 0)
                return;
            MakeGhostConstruction(net, parent);
        }

        //public override void Spawn(GameObject parent)
        //{
        //    Chunk.AddObject(parent, parent.Map, parent.Global);
        //    //MakeGhostConstruction(parent);
        //}
        //public override void GameLoaded(GameObject parent)
        //{
        //    base.GameLoaded(parent);
        //    if (this.Materials.Count > 0)
        //    {
        //        parent["Sprite"]["Alpha"] = 0.5f;
        //        parent["Sprite"]["Sprite"] = Product["Sprite"]["Sprite"];
        //    }
        //}

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            parent["Sprite"]["Alpha"] = 0.5f;
          //  parent["Sprite"]["Sprite"] = Product["Sprite"]["Sprite"];
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)//GameObject sender, Message.Types msg)
        {
            GameObject sender = e.Sender;
            switch (e.Type)
            {
                case Message.Types.BlockCollision:
                    //e.Network.PostLocalEvent(parent, Message.Types.Death);
                    e.Network.Despawn(parent);
                    e.Network.SyncDisposeObject(parent);
                    return true;

                case Message.Types.Death:
                    DesignatedConstructions.Remove(parent.Global);//.Remove(parent.Global, parent);
                    Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
                    foreach (KeyValuePair<GameObject.Types, int> mat in currentMats)
                    {
                        if (mat.Value > 0)
                        {

                            //currentMaterial.GetGui().StackSize = mat.Value;
                            for (int i = 0; i < mat.Value; i++)
                            {
                                GameObject currentMaterial = GameObject.Create(mat.Key);
                                Chunk.AddObject(currentMaterial, parent.Map, parent.GetPosition().GetProperty<Position>("Position").Rounded + new Vector3(0, 0, parent["Physics"].GetProperty<int>("Height")));
                                double angle = parent.Map.World.Random.NextDouble() * (Math.PI + Math.PI);
                                throw new NotImplementedException();
                                //GameObject.PostMessage(currentMaterial, Message.Types.ApplyForce, parent, new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));
                            }
                        }
                    }
                    Chunk.RemoveObject(parent);//, parent.GetPosition().GetProperty<Position>("Position").Rounded);
                    return false;
                //case Message.Types.Activate:
                //case Message.Types.Query:
                //    //Dictionary<Message.Types, Interaction> lengths = e.Parameters[0] as Dictionary<Message.Types, Interaction>;
                //    //lengths.Add(Message.Types.Mechanical, new Interaction(Message.Types.Mechanical, "Build"));
                //    Query(parent, e);
                //    return true;
                case Message.Types.FinishConstruction:
                    if (CheckMaterials())
                        Complete(parent, e.Sender);
                    return true;


                case Message.Types.Construct:
                    GameObject builder = e.Parameters[0] as GameObject;
                    Net.IObjectProvider net = e.Network;
                    //FinishConstruction(parent, builder, net);
                    e.Network.Spawn(this.Product.Object, parent.Global);
                    e.Network.Despawn(parent);
                    e.Network.DisposeObject(parent.NetworkID);
                    //e.Network.InstantiateObject(this.Product);
                    //e.Network.Sync(this.Product);
                    return true;

                //case Message.Types.Build:
                //    if (this.Materials.Count == 0)
                //        return true;

                //    Blueprint bp = this["Blueprint"] as Blueprint;
                //    GameObject.Types firstMat = this.Materials.First().Key;

                //    if (bp == null)
                //    {
                //        throw new NotImplementedException();
                //        return true;
                //    }

                //    BlueprintStage bps = bp[(int)this[Stat.Stage.Name]];

                //    if (!CheckMaterials())
                //        return true;

                //    return Activate(e.Sender, parent);

                //case Message.Types.DropOn:
                //case Message.Types.ApplyMaterial:
                //    GameObjectSlot holdSlot;
                //    if (!InventoryComponent.TryGetHeldObject(sender, out holdSlot))
                //        return true;
                //    if (!GetFilter().Apply(holdSlot.Object))
                //        return true;
                //    if (!Give(parent, e.Sender, holdSlot))
                //        return true;
                //    throw new NotImplementedException();
                //    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, holdSlot, holdSlot.Object);

                //    DesignatedConstructions.Remove(parent.Global);
                //    parent["Sprite"]["Hidden"] = false;
                //    parent["Sprite"]["Alpha"] = 1f;
                //    parent["Sprite"]["Sprite"] = GameObject.Objects[GameObject.Types.Construction]["Sprite"]["Sprite"];
                //    AIComponent.Invalidate(parent);
                //    return true;

                case Message.Types.Activate:
                    GameObject actor = e.Parameters[0] as GameObject;// TargetArgs.Read(e.Network, r);
                    GameObjectSlot materialSlot = actor.GetComponent<GearComponent>().Holding;
                    GameObject materialObj = materialSlot.Object;
                    if (materialObj.IsNull())
                        return true;

                    ItemRequirement req = this.Materials.FirstOrDefault(foo => foo.ObjectID == materialObj.ID);
                    if (req.IsNull())
                        return true;
                    if (req.Full)
                        return true;
                    materialSlot.StackSize--;
                    if (materialSlot.StackSize == 0)
                        e.Network.DisposeObject(materialObj);
                    req.Amount++;
                    parent.Global.GetChunk(e.Network.Map).Changed = true;
                    return true;


                //case Message.Types.SetBlueprint:
                //    SetBlueprint(e.Parameters[0] as Blueprint, (int)e.Parameters[1], (int)e.Parameters[2]);
                //    parent["Sprite"]["Sprite"] = this.Product.Object.GetComponent<ActorSpriteComponent>().Sprite;// Product["Sprite"]["Sprite"];
                //    return true;

                case Message.Types.SetBlueprint:
                    GameObject.Types bpID = GameObject.Types.Default;
                    e.Data.Translate(e.Network, r =>
                    {
                        bpID = (GameObject.Types)r.ReadInt32();
                    });
                    GameObject bpObj = GameObject.Objects[bpID];
                    Blueprint bp = bpObj.GetComponent<BlueprintComponent>().Blueprint;
                    //this.Blueprint.Object = bpObj;
                    this.Product = GameObject.Objects[bp.ProductID].ToSlot();
                    this.Materials.Clear();
                    foreach (var mat in bp.Stages[0])
                        this.Materials.Add(new ItemRequirement(mat.Key, mat.Value));
                    parent.Global.GetChunk(e.Network.Map).Changed = true;

                    //parent["Sprite"]["Alpha"] = 0.5f; // TODO: put an alpha parameter in the sprite class? and clone the sprite
                    //parent["Sprite"]["Sprite"] = Product.Object["Sprite"]["Sprite"];

                    return true;

                case Message.Types.ManageEquipment:
                    // e.Sender.HandleMessage(Message.Types.ChangeOrientation, parent);
                    ActorSpriteComponent.ChangeOrientation(Product.Object);
                    return true;
                default:
                    return base.HandleMessage(parent, e);
            }
        }

        private bool CheckMaterials()
        {
            foreach (var mat in this.Materials)
            {
                if (mat.Amount < mat.Max)
                    return false;
            }
            return true;

            //foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentReqMaterials())
            //{
            //    int currentAmount;
            //    if (!this.Materials.TryGetValue(mat.Key, out currentAmount))
            //        return false;
            //    if (currentAmount < mat.Value)
            //        return false;
            //}
            //return true;
        }

        //private ObjectFilter GetFilter()
        //{
        //    ObjectFilter filter = new ObjectFilter(FilterType.Include);
        //    foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentReqMaterials())
        //    {
        //        int currentAmount;
        //        if (this.Materials.TryGetValue(mat.Key, out currentAmount))
        //            if (currentAmount >= mat.Value)
        //                continue;
        //        List<GameObject.Types> existing;
        //        string type = GameObject.Objects[mat.Key].Type;
        //        if (!filter.TryGetValue(type, out existing))
        //        {
        //            existing = new List<GameObject.Types>();
        //            filter[type] = existing;
        //        }
        //        existing.Add(mat.Key);
        //    }
        //    return filter;
        //}

        //private List<GameObjectSlot> GetMissingMaterials()
        //{
        //    List<GameObjectSlot> list  = new List<GameObjectSlot>();
        //    Dictionary<GameObject.Types, int> reqs = GetCurrentReqMaterials();
        //    Dictionary<GameObject.Types, int> current = this.Materials;
        //    foreach (KeyValuePair<GameObject.Types, int> reqmat in reqs)
        //    {
        //        int currentAmount;
        //        if (current.TryGetValue(reqmat.Key, out currentAmount))
        //            if (currentAmount == reqmat.Value)
        //                continue;
        //        list.Add(new GameObjectSlot(GameObject.Objects[reqmat.Key], reqmat.Value - currentAmount));
        //    }
        //    return list;
        //}

        //private void SetBlueprint(Blueprint bp)
        //{
        //    if (Blueprint == null)
        //        Blueprint = bp;
        //}

        public override void Query(GameObject parent, List<Interaction> list)//GameObjectEventArgs e)
        {
          //  List<Interaction> list = e.Parameters[0] as List<Interaction>;
            //if (Blueprint == null)
            //{
            //    list.Add(new Interaction(TimeSpan.Zero, Message.Types.Activate, parent, "Set Blueprint"));
            //    return;
            //}

            list.Add(
                new Interaction(
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
                    

            //ObjectFilter filter = GetFilter();
            //list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.DropOn, parent, "Apply material",
            //            effect: new NeedEffectCollection() { new NeedEffect("Materials") },
            //            cond:
            //                new InteractionConditionCollection(
            //    //  new InteractionCondition(agent => InventoryComponent.IsHauling(agent, filter.Apply), "Requires materials!",
            //                    new InteractionCondition((actor, target) => InventoryComponent.IsHauling(actor, held => this.GetCurrentReqMaterials().ContainsKey(held.ID)), "Requires materials!",
            //                        new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
            //                        new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
            //                    ),
            //               // ),
            //            //targetCond:
            //            //    new InteractionConditionCollection(
            //                    new InteractionCondition((actor, target) => !CheckMaterials(), "Materials already in place!")
            //                )
            //            )
            //        );

            list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.FinishConstruction, parent, "Finish construction", 
                // effect: new InteractionEffect("Work"),
                        cond:
                            new ConditionCollection(
                                new Condition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build), "I need a tool to " + Message.Types.Build.ToString().ToLower() + " with.", //, "Requires: " + Message.Types.Build,
                                    new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                                ),
                        //    ),
                        //targetCond:
                        //    new InteractionConditionCollection(
                                new Condition((actor, target) => Blueprint != null, "Blueprint not set!"),
                                new Condition((actor, target) => CheckMaterials(), "Materials missing!",
                                    new Precondition("Materials", i => i.Message == Message.Types.ApplyMaterial && i.Source == parent, AI.PlanType.FindNearest)
                                )
                            )
                        )
                    );

            return;

            if (Product != null)
                if (ActorSpriteComponent.HasOrientation(Product.Object))
                    list.Add(new Interaction(TimeSpan.Zero, Message.Types.ManageEquipment, parent, "Change Orientation", "Changing Orientation"));

            //if (Blueprint != null)
            //{
            //    if (!MultiTile2Component.IsPositionValid(this.Product, parent.Map, parent.Global, (int)Product["Sprite"]["Orientation"]))
            //        return;


            //    if (!CheckMaterials())
            //    {
            //        list.Add(new Interaction(TimeSpan.Zero, Message.Types.DropOn, parent, "Apply material", 
            //            effect: new NeedEffectCollection() { new NeedEffect("Work") },
            //            cond: new InteractionConditionCollection(
            //                new InteractionCondition(
            //                (actor, target) => InventoryComponent.IsHauling(actor, filter.Apply),
            //                "Requires materials!",
            //                new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
            //                new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
            //          )
            //                )
            //            ));
            //        return;
            //    }
            //    else
            //    {
            //        list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.FinishConstruction, parent, "Finish construction",
            //            effect: new NeedEffectCollection() { new NeedEffect("Work", 20) },
            //            cond: new InteractionConditionCollection(
            //                                new InteractionCondition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build),
            //                        new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
            //                                                        )
            //                                                    )
            //                                )
            //                );
            //    }

            //}


        }

        //public bool Add(GameObject material)
        //{
        //    this.Materials.Add(material.ID, 1);
        //    return true;
        //}

        //GameObject GetProduct()
        //{
        //   // this.GetProperty<Blueprint>("Blueprint").ProductID;
        //    return this.Product.Object;
        //}
        Dictionary<GameObject.Types, int> GetCurrentReqMaterials()
        {
            return this.Blueprint.IsNull() ? new Dictionary<GameObject.Types, int>() : this.Blueprint.Stages[(int)this[Stat.Stage.Name]];
            //return this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];//Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }
        //Dictionary<GameObject.Types, int> GetCurrentMaterials()
        //{
        //    return this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        //}

        //public override bool Give(GameObject parent, GameObject giver, GameObjectSlot objSlot)
        //{
        //    GameObject obj = objSlot.Object;
        //    if (obj == null)
        //        return true;
        //    objSlot.StackSize -= 1;
        //    if (Drop(parent, giver, obj))
        //    {
        //        //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
        //        Dictionary<GameObject.Types, int> currentMats = this.Materials;// GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        //        int currentAmount;
        //        if (!currentMats.TryGetValue(obj.ID, out currentAmount))
        //            currentAmount = 0;
        //        currentMats[obj.ID] = currentAmount + 1;
        //        return true;
        //    }
        //    return true;
        //}


        public override bool Activate(GameObject sender, GameObject parent)
        {
            float skill = sender["Stats"].GetPropertyOrDefault<float>(Stat.WorkSpeed.Name, 1f);
            float skillMod = 0.8f * skill / 100;
            float value = GetProperty<float>(Stat.Value.Name) - 1;
            Properties[Stat.Value.Name] = value;
            if (value <= 0)
            {
                Complete(parent, sender);
                return true;
            }
            return false;
        }

        private void Complete(GameObject parent, GameObject builder)
        {
            if ((int)this[Stat.Stage.Name] < this.GetProperty<Blueprint>("Blueprint").Stages.Count - 1)
            {
                this[Stat.Stage.Name] = (int)this[Stat.Stage.Name] + 1;
                this[Stat.Value.Name] = 60f;
                return;
            }
            throw new NotImplementedException();
            //Product.Spawn(parent.Map, parent.Global);
            throw new NotImplementedException();
                //parent.Remove();
                throw new NotImplementedException();
            //Product.PostMessage(Message.Types.Constructed, parent, builder);
            
        }

        public override string GetTooltipText()
        {
            return ToString();
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

            //GetConstructionTooltip(parent, tooltip);
            //return;
            //// tooltip.Controls.Clear();
            //Blueprint bp = this["Blueprint"] as Blueprint;
            //if (bp == null)
            //    return;
            //    //GetConstructionTooltip(parent, tooltip);
            //else
            //    GetBlueprintTooltip(parent, tooltip);
        }


        private void GetBlueprintTooltip(GameObject parent, UI.Control tooltip)
        {
            int left = 0, bottom = 0;
            foreach (UI.Control b in tooltip.Controls)
            {
                left = Math.Min(left, b.Left);
                bottom = Math.Max(left, b.Bottom);
            }

            Blueprint bp = this["Blueprint"] as Blueprint;
            GroupBox box = new GroupBox(new Vector2(left, bottom));
            GameObject obj = parent;// GameObject.Objects[this.GetProperty<Blueprint>("Blueprint").ProductID];
            ActorSpriteComponent sprComp = bp != null ? GameObject.Objects[bp.ProductID].GetComponent<ActorSpriteComponent>("Sprite") : obj.GetComponent<ActorSpriteComponent>("Sprite");
            //PictureBox pic = new PictureBox(new Vector2(0), sprComp.Sprite.Texture, sprComp.Sprite.SourceRect[Variation][Orientation], TextAlignment.Left);
            PictureBox pic = new PictureBox(new Vector2(0), sprComp.Sprite.Texture, sprComp.Sprite.SourceRects[0][0], HorizontalAlignment.Left);
            Panel pnl_pic = new Panel();
            pnl_pic.AutoSize = true;
            pnl_pic.Controls.Add(pic);

            Label lbl_bp = new Label(new Vector2(pnl_pic.Right, 0), "Blueprint:\n" + (bp != null ? bp.Name : "Not set"));

            Label lbl_progress = new Label(new Vector2(pnl_pic.Right, lbl_bp.Bottom), (int)(100 - (float)this[Stat.Value.Name] * 100 / 60f) + "% Complete");
            // Label labelName = new Label(new Vector2(panel.Right, 0), obj.Name + "\n% Complete");
            //Label label = new Label("Current stage: " + (int)this[Stat.Stage.Name]);
            Label label = new Label("Stage " + ((int)this[Stat.Stage.Name] + 1) + "/" + (this.GetProperty<Blueprint>("Blueprint").Stages.Count));
            GroupBox panelMaterials = new GroupBox(new Vector2(pnl_pic.Right, lbl_progress.Bottom));//labelName.Bottom)); //new GroupBox(new Vector2(0, panel.Bottom)); //
            panelMaterials.AutoSize = true;
            panelMaterials.Controls.Add(label);

            //int i = 0;
            //Dictionary<GameObject.Types, int> currentMats = this.Materials;// GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            //Dictionary<GameObject.Types, int> reqMats = null;
            //if(bp!=null)
            //    reqMats = bp.Stages[(int)this[Stat.Stage.Name]];
            //foreach (KeyValuePair<GameObject.Types, int> mat in reqMats)
            //{
            //    Slot slot = new Slot(new Vector2(i, label.Bottom));
            //    slot.AutoText = false;
            //    GameObjectSlot invSlot = new GameObjectSlot();
            //    invSlot.Object = GameObject.Create(mat.Key);
            //    int currentAmount;
            //    string matsText = (currentMats.TryGetValue(mat.Key, out currentAmount) ? currentAmount.ToString() : "0") + "/" + mat.Value.ToString();
            //    Label matsLbl = new Label(matsText) { Location = slot.Dimensions, MouseThrough = true, ClipToBounds = false };
            //    matsLbl.Anchor = Vector2.One;
            //    slot.Controls.Add(matsLbl);
            //    slot.Tag = invSlot;
            //    i += slot.Width;
            //    panelMaterials.Controls.Add(slot, new Label(slot.TopRight, invSlot.Object.Name));
            //}
            foreach (var mat in this.Materials)
            {
                tooltip.Controls.Add(
                    new SlotWithText(tooltip.Controls.BottomLeft).SetTag(GameObject.Objects[mat.ObjectID].ToSlot()).SetText(GameObject.Objects[mat.ObjectID].Name + " " + mat.Amount + "/" + mat.Max));
            }

            box.Controls.Add(lbl_progress, lbl_bp, pnl_pic, panelMaterials);//, labelName);
            tooltip.Controls.Add(box);
        }


        private void GetConstructionTooltip(GameObject parent, UI.Control tooltip)
        {
            int left = 0, bottom = 0;
            foreach (UI.Control b in tooltip.Controls)
            {
                left = Math.Min(left, b.Left);
                bottom = Math.Max(left, b.Bottom);
            }

            Blueprint bp = this["Blueprint"] as Blueprint;
            GroupBox box = new GroupBox(new Vector2(left, bottom));
            GameObject obj = parent;// GameObject.Objects[this.GetProperty<Blueprint>("Blueprint").ProductID];
            ActorSpriteComponent sprComp = bp != null ? GameObject.Objects[bp.ProductID].GetComponent<ActorSpriteComponent>("Sprite") : obj.GetComponent<ActorSpriteComponent>("Sprite");
            //PictureBox pic = new PictureBox(new Vector2(0), sprComp.Sprite.Texture, sprComp.Sprite.SourceRect[Variation][Orientation], TextAlignment.Left);
            PictureBox pic = new PictureBox(new Vector2(0), sprComp.Sprite.Texture, sprComp.Sprite.SourceRects[0][0], HorizontalAlignment.Left);
            Panel pnl_pic = new Panel();
            pnl_pic.AutoSize = true;
            pnl_pic.Controls.Add(pic);


            Label lbl_bp = new Label(new Vector2(pnl_pic.Right, 0), "Blueprint:\n" + (bp != null ? bp.Name : "Not set"));

            //Label labelName = new Label(new Vector2(panel.Right, 0), obj.Name + "\n" + (int)(Progress.Percentage * 100) + "% Complete");
            // Label labelName = new Label(new Vector2(panel.Right, 0), obj.Name + "\n% Complete");
            Label label = new Label("Current Materials:");

            GroupBox panelMaterials = new GroupBox(new Vector2(pnl_pic.Right, lbl_bp.Bottom));//labelName.Bottom)); //new GroupBox(new Vector2(0, panel.Bottom)); //
            panelMaterials.AutoSize = true;
            panelMaterials.Controls.Add(label);

            //int i = 0;
            //Dictionary<GameObject.Types, int> currentMats = this.Materials;// this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            //foreach (KeyValuePair<GameObject.Types, int> mat in currentMats)
            //{
            //    Slot slot = new Slot(new Vector2(i, label.Bottom));
            //    slot.AutoText = false;
            //    GameObjectSlot invSlot = new GameObjectSlot();
            //    invSlot.Object = GameObject.Create(mat.Key);
            //    slot.SetBottomRightText(mat.Value.ToString());
            //    slot.Tag = invSlot;
            //    i += slot.Width;
            //    panelMaterials.Controls.Add(slot);
            //}
            foreach (var mat in this.Materials)
            {
                tooltip.Controls.Add(
                    new SlotWithText(tooltip.Controls.BottomLeft).SetTag(GameObject.Objects[mat.ObjectID].ToSlot()).SetText(GameObject.Objects[mat.ObjectID].Name + " " + mat.Amount + "/" + mat.Max));
            }
            box.Controls.Add(lbl_bp, pnl_pic, panelMaterials);//, labelName);
            tooltip.Controls.Add(box);
        }

        public GameObject SetBlueprint(Blueprint bp, int variation = 0, int orientation = 0)
        {
            if (Blueprint != null)
                return null;
            this["Blueprint"] = bp;
            GameObject product = GameObject.Create(bp.ProductID);
            product["Sprite"]["Variation"] = variation;
            product["Sprite"]["Orientation"] = orientation;
            
            //this["Product"] = product;
            this.Product = product.ToSlot();

            this.Materials.Clear();
            foreach (var mat in bp.Stages[0])
                this.Materials.Add(new ItemRequirement(mat.Key, mat.Value));

            return product;
        }

        //public GameObject SetBlueprint(Blueprint bp)
        //{
        //    if (Blueprint != null)
        //        return null;
        //    this["Blueprint"] = bp;
        //    GameObject product = GameObject.Create(bp.ProductID);
        //    this["Product"] = product;
        //    return product;
        //}

        public override void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent)// DrawObjectArgs e)
        {
            //if (e.Controller.Mouseover.Object != e.Object)
            //    return;
            if (!this.Product.HasValue)
                return;
            MultiTile2Component multi;
            if (Product.Object.TryGetComponent<MultiTile2Component>("Multi", out multi))
                multi.DrawPreview(sb, camera, parent.Global, (int)Product.Object["Sprite"]["Orientation"]);
            else
            {
                if (Product.Object.IsBlock())
                {
                    ActorSpriteComponent.DrawPreview(sb, camera, parent.Global.Round(), Product.Object);
                    return;
                }
                ActorSpriteComponent sprite;
                if (Product.Object.TryGetComponent<ActorSpriteComponent>("Sprite", out sprite))
                    sprite.DrawPreview(sb, camera, parent.Global.Round(), (int)Product.Object["Sprite"]["Orientation"]);
            }
        }

        #region Save/Load
        internal override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
          //  save.Add(new Tag(Tag.Types.Int, "ProductID", (int)this.GetProperty<Blueprint>("Blueprint").ProductID));
            if (this.Product.HasValue)
            {
                //save.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Object.Save()));
                save.Add(new SaveTag(SaveTag.Types.Int, "ProductID", this.Product.Object.ID));
            }
            save.Add(new SaveTag(SaveTag.Types.Byte, "Stage", (byte)(int)this[Stat.Stage.Name]));
            //SaveTag mats = new SaveTag(SaveTag.Types.Compound, "Materials");
            //foreach (KeyValuePair<GameObject.Types, int> mat in this.Materials)
            //    mats.Add(new SaveTag(SaveTag.Types.Byte, ((int)mat.Key).ToString(), (byte)mat.Value));
            //save.Add(mats);
            SaveTag matsTag = new SaveTag(SaveTag.Types.List, "Materials", SaveTag.Types.Compound);
            foreach (var mat in this.Materials)
            {
                SaveTag matTag = new SaveTag(SaveTag.Types.Compound, "Material");
                matTag.Add(new SaveTag(SaveTag.Types.Int, "ID", mat.ObjectID));
                matTag.Add(new SaveTag(SaveTag.Types.Int, "Amount", mat.Amount));
                matTag.Add(new SaveTag(SaveTag.Types.Int, "Max", mat.Max));
                matsTag.Add(matTag);
            }
            save.Add(matsTag);
            return save;
        }

        internal override void Load(SaveTag compTag)
        {
            Dictionary<string, SaveTag> byName = compTag.ToDictionary();
            //this.Product = GameObject.Create(byName["Product"]).ToSlot();
            this.Product = compTag.TagValueOrDefault<int, GameObjectSlot>("ProductID", v => GameObject.Create((GameObject.Types)v).ToSlot(), GameObjectSlot.Empty);
            this[Stat.Stage.Name] = (int)(byte)byName["Stage"].Value;
            //foreach (SaveTag mat in (List<SaveTag>)byName["Materials"].Value)
            //{
            //    if (mat.Value == null)
            //        continue;
            //    this.Materials[(GameObject.Types)Enum.Parse(typeof(GameObject.Types), mat.Name)] = Convert.ToInt16((byte)mat.Value);
            //}
            foreach (var matTag in (List<SaveTag>)compTag["Materials"].Value)
            {
                this.Materials.Add(new ItemRequirement((GameObject.Types)matTag.GetValue<int>("ID"), matTag.GetValue<int>("Max"), matTag.GetValue<int>("Amount")));
            }
        }
        #endregion

        public override void Write(System.IO.BinaryWriter writer)
        {
            if (this.Product.HasValue)
                writer.Write((int)this.Product.Object.ID);
            else
                writer.Write(0);
            writer.Write((byte)this.Stage);
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
            //GameObject.Types prodID = (GameObject.Types)reader.ReadInt32();
            int prodID = reader.ReadInt32();
            if (prodID > 0)
            //GameObject.Types prodID = (GameObject.Types)reader.ReadInt32();
                this.Product.Object = GameObject.Create((GameObject.Types)prodID);
            this.Stage = reader.ReadByte();
            int matCount = reader.ReadInt32();
            for (int i = 0; i < matCount; i++)
            {
                GameObject.Types t = (GameObject.Types)reader.ReadInt32();
                int amount = reader.ReadInt32();
                int max = reader.ReadInt32();
                this.Materials.Add(new ItemRequirement(t, max, amount));
            }
        }
        #region Ghost Construction Methods
        //static public bool ToggleConstructions(bool value)
        //{
        //    foreach (var obj in DesignatedConstructions)
        //    {
        //        obj.Value["Sprite"]["Hidden"] = !value;
        //        Nameplate.Toggle(obj.Value, value);
        //    }
        //    return value;
        //}

        private void MakeGhostConstruction(Net.IObjectProvider net, GameObject parent)
        {
           // parent["Sprite"]["Hidden"] = true;// BuildWindow.Instance.IsOpen() ? false : true;
            parent["Sprite"]["Alpha"] = 0.5f; // TODO: put an alpha parameter in the sprite class? and clone the sprite
            parent["Sprite"]["Sprite"] = Product.Object["Sprite"]["Sprite"];
            RemoveDesignatedConstruction(net, parent.Global);
            DesignatedConstructions.Add(parent.Global, parent);
      //      Nameplate.Hide(parent);
            // BuildWindow.Instance.Show();
        }

        static public bool RemoveDesignatedConstruction(Net.IObjectProvider net, Vector3 global)
        {
            GameObject obj;

            if (!DesignatedConstructions.TryGetValue(global, out obj))
                return false;
            DesignatedConstructions.Remove(global);
           
            //obj.Remove();
            net.Despawn(obj);
            return true;
        }
        static public bool RemoveDesignatedConstruction(Net.IObjectProvider net, GameObject obj)
        {
            return RemoveDesignatedConstruction(net, obj.Global);
        }
        #endregion

        //public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<Net.GameEvent>> handlers)
        {
            var p = new Panel() { AutoSize = true };
            var list = new ListBox<GameObjectSlot, SlotWithText>(new Rectangle(0, 0, 200, 200));
            list.Build(
                WorkbenchComponent.Blueprints.Select(foo => foo.ToSlot()),
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
                            //w.Write(0); //variation
                            //w.Write(0); //orientation
                        });
                        ui.TopLevelControl.Hide();
                    };
                });
            p.Controls.Add(list);
            ui.Controls.Add(p);
        }

    }
}
