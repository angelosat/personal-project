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
        
        GameObject Product { get { return (GameObject)this["Product"]; } set { this["Product"] = value; } }
        Blueprint Blueprint { get { return (Blueprint)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        Dictionary<GameObject.Types, int> Materials { get { return (Dictionary<GameObject.Types, int>)this["Materials"]; } set { this["Materials"] = value; } }
        int Stage { get { return (int)this["Stage"]; } set { this["Stage"] = value; } }
        Dictionary<GameObject.Types, int> CurrentMaterials { get { return (Dictionary<GameObject.Types, int>)this["CurrentMaterials"]; } set { this["CurrentMaterials"] = value; } }
        public ConstructionComponent()
        {
            this.Materials = new Dictionary<GameObject.Types, int>();
            this.Properties.Add("Blueprint");
            this.Properties.Add("Product");
            this.CurrentMaterials = new Dictionary<GameObject.Types, int>();
            this.Stage = 0;
            this[Stat.Value.Name] = 60f;
        }
        public ConstructionComponent(Blueprint bp, bool keepPlacing = false) : this()
        {
            Dictionary<GameObject.Types, int> currentMats = new Dictionary<GameObject.Types, int>();
            this["Product"] = GameObject.Create(bp.ProductID);
            this["Blueprint"] = bp;
            this["CurrentMaterials"] = currentMats;
            //this[Stat.Stage.Name] = 0;

            foreach (BlueprintStage stage in bp.Stages)
                foreach (KeyValuePair<GameObject.Types, int> mat in stage)
                    currentMats.Add(mat.Key, 0);

        }

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

        public override bool Drop(GameObject self, GameObject actor, GameObject obj)
        {
            Blueprint bp = this.GetProperty<Blueprint>("Blueprint");
            int count, currentcount;
            Dictionary<GameObject.Types, int> currentMats = this.Materials;// this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            if (bp == null)
                return true;

            Dictionary<GameObject.Types, int> reqMats = bp.Stages[(int)this[Stat.Stage.Name]];
            if (reqMats.TryGetValue(obj.ID, out count))
            {
                if (currentMats.TryGetValue(obj.ID, out currentcount))
                    if (currentcount >= count)
                        return false;

                //Task task = new Task(actor, 
                //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
                //CurrentMaterials[obj.ID] += 1;
                return true;
            }

            return false;
        }

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
                
                //case Message.Types.Attack:
                //    return Activate(sender, parent);
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
                case Message.Types.Build:
                    //Interaction action = e.Parameters[0] as Interaction;
                    //if(action.Message!=Message.Types.Mechanical)
                    //    return false;
                    if (this.Materials.Count == 0)
                        return true;

                    Blueprint bp = this["Blueprint"] as Blueprint;
                    //if (bp == null)
                    //{
                    //    e.Sender.HandleMessage(Message.Types.Craft, parent);
                    //    return true;
                    //}
                    GameObject.Types firstMat = this.Materials.First().Key;
                    //sender.HandleMessage(GameObject.Objects[firstMat], Message.Types.Craft);

                    if (bp == null)
                    {
                        throw new NotImplementedException();
                        //e.Sender.PostMessage(Message.Types.Build, parent, GameObject.Objects[firstMat]);
                        return true;
                    }

                    BlueprintStage bps = bp[(int)this[Stat.Stage.Name]];
                    //foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentMaterials())
                    //{
                    //    if (mat.Value < bps[mat.Key])
                    //        return true;
                    //}
                    if (!CheckMaterials())
                        return true;

                    return Activate(e.Sender, parent);
                //return true;
                case Message.Types.DropOn:
                case Message.Types.ApplyMaterial:
                    GameObjectSlot holdSlot;
                    if (!InventoryComponent.TryGetHeldObject(sender, out holdSlot))
                        return true;
                    if (!GetFilter().Apply(holdSlot.Object))
                        return true;
                    if (!Give(parent, e.Sender, holdSlot))
                        return true;
                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, holdSlot, holdSlot.Object);

                    DesignatedConstructions.Remove(parent.Global);
                    parent["Sprite"]["Hidden"] = false;
                    parent["Sprite"]["Alpha"] = 1f;
                    parent["Sprite"]["Sprite"] = GameObject.Objects[GameObject.Types.Construction]["Sprite"]["Sprite"];
                    AIComponent.Invalidate(parent);
                    return true;
                case Message.Types.Activate://DropOn:

                    GameObjectSlot haulingSlot; 
                    if (!InventoryComponent.TryGetHeldObject(e.Sender, out haulingSlot))
                        return true;
                    GameObject hauling = haulingSlot.Object;//inv.GetProperty<GameObjectSlot>("Holding").Object;

                    

                    if (GetFilter().Apply(hauling))
                        if (Give(parent, e.Sender, haulingSlot))
                        {

              
                            //hauling.Remove();
                            e.Network.Despawn(hauling);

                            throw new NotImplementedException();
                            //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulingSlot, hauling);

                            // uncomment to immediately finish construction upon applying the last material
                            //if (CheckMaterials())
                            //    Complete(parent, e.Sender);

                            return true;
                        }
                    return false;

                //case Message.Types.Activate:
                //    firstMat = GetCurrentMaterials().First().Key;
                //    e.Sender.HandleMessage(Message.Types.Build, parent, GameObject.Objects[firstMat]);
                //    return true;

                case Message.Types.SetBlueprint:
                    SetBlueprint(e.Parameters[0] as Blueprint, (int)e.Parameters[1], (int)e.Parameters[2]);
                    parent["Sprite"]["Sprite"] = Product["Sprite"]["Sprite"];
                    return true;
                case Message.Types.ManageEquipment:
                    // e.Sender.HandleMessage(Message.Types.ChangeOrientation, parent);
                    ActorSpriteComponent.ChangeOrientation(Product);
                    return true;
                default:
                    return base.HandleMessage(parent, e);
            }
        }

        private bool CheckMaterials()
        {
            foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentReqMaterials())
            {
                int currentAmount;
                if (!this.Materials.TryGetValue(mat.Key, out currentAmount))
                    return false;
                if (currentAmount < mat.Value)
                    return false;
            }
            return true;
        }

        private ObjectFilter GetFilter()
        {
            ObjectFilter filter = new ObjectFilter(FilterType.Include);
            foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentReqMaterials())
            {
                int currentAmount;
                if (this.Materials.TryGetValue(mat.Key, out currentAmount))
                    if (currentAmount >= mat.Value)
                        continue;
                List<GameObject.Types> existing;
                string type = GameObject.Objects[mat.Key].Type;
                if(!filter.TryGetValue(type, out existing))
                {
                    existing = new List<GameObject.Types>();
                    filter[type] = existing;
                }
                existing.Add(mat.Key);
            }
            return filter;
        }

        private List<GameObjectSlot> GetMissingMaterials()
        {
            List<GameObjectSlot> list  = new List<GameObjectSlot>();
            Dictionary<GameObject.Types, int> reqs = GetCurrentReqMaterials();
            Dictionary<GameObject.Types, int> current = this.Materials;
            foreach (KeyValuePair<GameObject.Types, int> reqmat in reqs)
            {
                int currentAmount;
                if (current.TryGetValue(reqmat.Key, out currentAmount))
                    if (currentAmount == reqmat.Value)
                        continue;
                list.Add(new GameObjectSlot(GameObject.Objects[reqmat.Key], reqmat.Value - currentAmount));
            }
            return list;
        }

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
                    

            ObjectFilter filter = GetFilter();
            list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.DropOn, parent, "Apply material",
                        effect: new NeedEffectCollection() { new NeedEffect("Materials") },
                        cond:
                            new InteractionConditionCollection(
                //  new InteractionCondition(agent => InventoryComponent.IsHauling(agent, filter.Apply), "Requires materials!",
                                new InteractionCondition((actor, target) => InventoryComponent.IsHauling(actor, held => this.GetCurrentReqMaterials().ContainsKey(held.ID)), "Requires materials!",
                                    new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
                                    new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
                                ),
                           // ),
                        //targetCond:
                        //    new InteractionConditionCollection(
                                new InteractionCondition((actor, target) => !CheckMaterials(), "Materials already in place!")
                            )
                        )
                    );

            list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.FinishConstruction, parent, "Finish construction", 
                // effect: new InteractionEffect("Work"),
                        cond:
                            new InteractionConditionCollection(
                                new InteractionCondition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build), "I need a tool to " + Message.Types.Build.ToString().ToLower() + " with.", //, "Requires: " + Message.Types.Build,
                                    new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                                ),
                        //    ),
                        //targetCond:
                        //    new InteractionConditionCollection(
                                new InteractionCondition((actor, target) => Blueprint != null, "Blueprint not set!"),
                                new InteractionCondition((actor, target) => CheckMaterials(), "Materials missing!",
                                    new Precondition("Materials", i => i.Message == Message.Types.ApplyMaterial && i.Source == parent, AI.PlanType.FindNearest)
                                )
                            )
                        )
                    );

            return;

            if (Product != null)
                if (ActorSpriteComponent.HasOrientation(Product))
                    list.Add(new Interaction(TimeSpan.Zero, Message.Types.ManageEquipment, parent, "Change Orientation", "Changing Orientation"));
            //if (GetCurrentMaterials().Count == 0)
            //    return;
            if (Blueprint != null)
            {
                if (!MultiTile2Component.IsPositionValid(this.Product, parent.Map, parent.Global, (int)Product["Sprite"]["Orientation"]))
                    return;


                if (!CheckMaterials())
                {
                    //GameObjectSlot haulSlot = e.Sender["Inventory"]["Holding"] as GameObjectSlot;
                    //GameObject hauling = haulSlot.Object;
                    //if (hauling != null)
                    //    if (hauling.Type == ObjectType.Material)

                    //ObjectFilter filter = GetFilter();


                    //   list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.DropOn, parent, "Apply material", range: (float)Math.Sqrt(2),  //TimeSpan.Zero
                    list.Add(new Interaction(TimeSpan.Zero, Message.Types.DropOn, parent, "Apply material", 
                        //effect: new InteractionEffect[] { new InteractionEffect("Work") },
                        effect: new NeedEffectCollection() { new NeedEffect("Work") },
                        //cond: new InteractionCondition("Holding", true, AI.PlanType.FindNearest,
                        //    precondition: foo => filter.Apply(foo) || ProductionComponent.CanProduce(foo, filter.Apply),
                        //    finalCondition: agent => InventoryComponent.IsHauling(agent, filter.Apply))
                        cond: new InteractionConditionCollection(
                            //new InteractionCondition(agent => InventoryComponent.IsHauling(agent, filter.Apply)//true, //Blueprint[0].Condition,//  agent => ControlComponent.HasAbility(agent, Message.Types.Build), "You need a tool to build with!", 
                            //   ,new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                            //    ),

                            new InteractionCondition(
                        //"Holding", true, AI.PlanType.FindNearest,
                        //precondition: foo => filter.Apply(foo) || ProductionComponent.CanProduce(foo, filter.Apply),
                            (actor, target) => InventoryComponent.IsHauling(actor, filter.Apply),
                            "Requires materials!",
                            new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
                            new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
                        // TODO: i haven't tested this
                        //      new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                      )
                            )
                        ));
                    return;
                }
                else
                {
                    list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.FinishConstruction, parent, "Finish construction",
                        effect: new NeedEffectCollection() { new NeedEffect("Work", 20) },
                        cond: new InteractionConditionCollection(
                                            new InteractionCondition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build),
                                    new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                                                                    )
                                                                )
                                            )
                            );
                }

            }


            //list.Add(new Interaction(new TimeSpan(0, 0, 1), Message.Types.Build, parent, "Build", stat: Stat.Building,  
            //    //effect: new InteractionEffect[] { new InteractionEffect("Work") },
            //     effect: new InteractionEffect("Work"),
            //     //cond: new InteractionCondition("Equipped", true, AI.PlanType.FindInventory, obj => FunctionComponent.HasAbility(obj, Message.Types.Build))));// need: new Need("Work", 20)));// "Work"));
            //   cond: new InteractionConditionCollection(
            //   new InteractionCondition(foo=>true, new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory))
            //   )
            //   )
            //   );// need: new Need("Work", 20)));// "Work"));

            //if (Blueprint == null)
            //{
            //    lengths.Add(new Interaction(TimeSpan.Zero, Message.Types.SetBlueprint, "Set Blueprint"));
            //    return;
            //}
        }

        public bool Add(GameObject material)
        {
            this.Materials.Add(material.ID, 1);
            return true;
        }

        GameObject GetProduct()
        {
           // this.GetProperty<Blueprint>("Blueprint").ProductID;
            return (GameObject)this["Product"];
        }
        Dictionary<GameObject.Types, int> GetCurrentReqMaterials()
        {
            return this.Blueprint.IsNull() ? new Dictionary<GameObject.Types, int>() : this.Blueprint.Stages[(int)this[Stat.Stage.Name]];
            //return this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];//Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }
        //Dictionary<GameObject.Types, int> GetCurrentMaterials()
        //{
        //    return this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        //}

        public override bool Give(GameObject parent, GameObject giver, GameObjectSlot objSlot)
        {
            GameObject obj = objSlot.Object;
            if (obj == null)
                return true;
            objSlot.StackSize -= 1;
            if (Drop(parent, giver, obj))
            {
                //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
                Dictionary<GameObject.Types, int> currentMats = this.Materials;// GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
                int currentAmount;
                if (!currentMats.TryGetValue(obj.ID, out currentAmount))
                    currentAmount = 0;
                currentMats[obj.ID] = currentAmount + 1;
                return true;
            }
            return true;
        }

        //public override bool Give(GameObject self, GameObject actor, GameObject obj)
        //{
        //    if (Drop(self, actor, obj))
        //    {
        //        //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
        //        Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        //        int currentAmount;
        //        if (!currentMats.TryGetValue(obj.ID, out currentAmount))
        //            currentAmount = 0;
        //        currentMats[obj.ID] = currentAmount + 1;
        //        return true;
        //    }
        //    return false;
        //}

        public override bool Activate(GameObject sender, GameObject parent)
        {

            //Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            //Dictionary<GameObject.Types, int> reqMats = this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];
            //foreach (KeyValuePair<GameObject.Types, int> mats in reqMats)
            //{
            //    int currentcount;
            //    if (!currentMats.TryGetValue(mats.Key, out currentcount))
            //        return false;
            //    if (currentcount != mats.Value)
            //        return false;
            //}
            
            //sender.GetComponent<CooldownComponent>("Cooldown").Properties["Cooldown"] = 5f;
            float skill = sender["Stats"].GetPropertyOrDefault<float>(Stat.WorkSpeed.Name, 1f);
            float skillMod = 0.8f * skill / 100;
       //     sender["Cooldown"]["Cooldown"] = (1 - skillMod) * 5;
            float value = GetProperty<float>(Stat.Value.Name) - 1;//GlobalVars.DeltaTime*50;
            Properties[Stat.Value.Name] = value;
            if (value <= 0)
            {
                Complete(parent, sender);
                //Console.WriteLine("EAEGAGASFGAS CRAFTING");
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
            // tooltip.Controls.Clear();
            Blueprint bp = this["Blueprint"] as Blueprint;
            if (bp == null)
                return;
                //GetConstructionTooltip(parent, tooltip);
            else
                GetBlueprintTooltip(parent, tooltip);
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

            int i = 0;
            Dictionary<GameObject.Types, int> currentMats = this.Materials;// GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            Dictionary<GameObject.Types, int> reqMats = null;
            if(bp!=null)
                reqMats = bp.Stages[(int)this[Stat.Stage.Name]];
            foreach (KeyValuePair<GameObject.Types, int> mat in reqMats)
            {
                Slot slot = new Slot(new Vector2(i, label.Bottom));
                slot.AutoText = false;
                GameObjectSlot invSlot = new GameObjectSlot();
                invSlot.Object = GameObject.Create(mat.Key);
                int currentAmount;
                string matsText = (currentMats.TryGetValue(mat.Key, out currentAmount) ? currentAmount.ToString() : "0") + "/" + mat.Value.ToString();
                Label matsLbl = new Label(matsText) { Location = slot.Dimensions, MouseThrough = true, ClipToBounds = false };
                matsLbl.Anchor = Vector2.One;
                slot.Controls.Add(matsLbl);
                slot.Tag = invSlot;
                i += slot.Width;
                panelMaterials.Controls.Add(slot, new Label(slot.TopRight, invSlot.Object.Name));
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

            int i = 0;
            Dictionary<GameObject.Types, int> currentMats = this.Materials;// this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            foreach (KeyValuePair<GameObject.Types, int> mat in currentMats)
            {
                Slot slot = new Slot(new Vector2(i, label.Bottom));
                slot.AutoText = false;
                GameObjectSlot invSlot = new GameObjectSlot();
                invSlot.Object = GameObject.Create(mat.Key);
                slot.SetBottomRightText(mat.Value.ToString());
                slot.Tag = invSlot;
                i += slot.Width;
                panelMaterials.Controls.Add(slot);
            }
            //Blueprint bp;
            //if (this.TryGetProperty<Blueprint>("Blueprint", out bp))
            //{
            //    Dictionary<GameObject.Types, int> reqMats = bp.Stages[(int)this[Stat.Stage.Name]];
            //    //Dictionary<GameObject.Types, int> reqMats = this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];
            //    foreach (KeyValuePair<GameObject.Types, int> mat in currentMats)
            //    {
            //        Slot slot = new Slot(new Vector2(i, label.Bottom));
            //        slot.AutoText = false;
            //        GameObjectSlot invSlot = new GameObjectSlot();
            //        invSlot.Object = GameObject.Create(mat.Key);
            //        //invSlot.Object.GetGui().StackSize = mat.Value;
            //        //slot.SetBottomRightText(mat.Value + "/" + Materials[mat.Key]);
            //        //invSlot.Object.GetGui().StackSize = Materials[mat.Key];
            //        slot.SetBottomRightText(mat.Value + "/" + reqMats[mat.Key]);
            //        slot.Tag = invSlot;
            //        i += slot.Width;
            //        panelMaterials.Controls.Add(slot);
            //    }
            //}

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
            
            this["Product"] = product;
            
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
            if (Product == null)
                return;
            MultiTile2Component multi;
            if (Product.TryGetComponent<MultiTile2Component>("Multi", out multi))
                multi.DrawPreview(sb, camera, parent.Global, (int)Product["Sprite"]["Orientation"]);
            else
            {
                if (Product.IsBlock())
                {
                    ActorSpriteComponent.DrawPreview(sb, camera, parent.Global.Round(), Product);
                    return;
                }
                ActorSpriteComponent sprite;
                if (Product.TryGetComponent<ActorSpriteComponent>("Sprite", out sprite))
                    sprite.DrawPreview(sb, camera, parent.Global.Round(), (int)Product["Sprite"]["Orientation"]);
            }
        }

        #region Save/Load
        internal override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
          //  save.Add(new Tag(Tag.Types.Int, "ProductID", (int)this.GetProperty<Blueprint>("Blueprint").ProductID));
            save.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.GetProduct().Save()));
            save.Add(new SaveTag(SaveTag.Types.Byte, "Stage", (byte)(int)this[Stat.Stage.Name]));
            SaveTag mats = new SaveTag(SaveTag.Types.Compound, "Materials");
            foreach (KeyValuePair<GameObject.Types, int> mat in this.Materials)
                mats.Add(new SaveTag(SaveTag.Types.Byte, ((int)mat.Key).ToString(), (byte)mat.Value));
            save.Add(mats);
            return save;
        }

        internal override void Load(SaveTag compTag)
        {
            Dictionary<string, SaveTag> byName = compTag.ToDictionary();
            this["Product"] = GameObject.Create(byName["Product"]);
            this["Blueprint"] = BlueprintComponent.Blueprints[GetProduct().ID];
            this[Stat.Stage.Name] = (int)(byte)byName["Stage"].Value;
            foreach (SaveTag mat in (List<SaveTag>)byName["Materials"].Value)
            {
                if (mat.Value == null)
                    continue;
                this.Materials[(GameObject.Types)Enum.Parse(typeof(GameObject.Types), mat.Name)] = Convert.ToInt16((byte)mat.Value);
            }
        }
        #endregion

        //public override void Write(System.IO.BinaryWriter writer)
        //{
        //    writer.Write((int)this.Product.ID);
        //    writer.Write((byte)this.Stage);
        //    writer.Write(this.Materials.Count);
        //    foreach (var mat in this.Materials)
        //    {
        //        writer.Write((int)mat.Key);
        //        writer.Write(mat.Amount);
        //        writer.Write(mat.Max);
        //    }
        //}

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
            parent["Sprite"]["Sprite"] = Product["Sprite"]["Sprite"];
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

    }
}
