using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ConstructionBlockComponent : TileComponent
    {
        GameObject Product { get { return (GameObject)this["Product"]; } set { this["Product"] = value; } }
        Blueprint Blueprint { get { return (Blueprint)this["Blueprint"]; } set { this["Blueprint"] = value; } }


        public ConstructionBlockComponent() : base(Tile.Types.Construction)
        {
            this["CurrentMaterials"] = new Dictionary<GameObject.Types, int>();
            this.Properties.Add("Blueprint");
            this.Properties.Add("Product");
            this[Stat.Stage.Name] = 0;
            this[Stat.Value.Name] = 60f;
        }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Spawn:
                    ToTile(parent);
                    Chunk.AddBlockObject(parent.Map, parent, parent.Global);
                    return true;

                case Message.Types.ApplyMaterial:
                    InventoryComponent inv;
                    if (!e.Sender.TryGetComponent<InventoryComponent>("Inventory", out inv))
                        return true;
                    if (inv.GetProperty<GameObjectSlot>("Holding").Object == null)
                    {
                        e.Sender.HandleMessage(Message.Types.NeedItem, parent, GetMissingMaterials().First().Object.ID);
                        return true;
                    }
                    GameObjectSlot haulingSlot = inv["Holding"] as GameObjectSlot;
                    GameObject hauling = haulingSlot.Object;
                    if (hauling == null)
                    {
                        e.Sender.HandleMessage(Message.Types.NeedItem, parent, GetMissingMaterials().First().Object.ID);
                        return true;
                    }
                    if (GetFilter().Apply(hauling))
                        if (Give(parent, e.Sender, inv.GetProperty<GameObjectSlot>("Holding")))//.Object))
                        {
                            //Map.RemoveObject(inv.GetProperty<GameObjectSlot>("Holding").Object);
                            Map.RemoveObject(hauling); 
                            GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulingSlot, hauling);
                            //if (CheckMaterials())
                            //    Complete(parent, e.Sender);
                            //inv.GetProperty<GameObjectSlot>("Holding").Object = null;
                            return true;
                        }
                    return true;

                case Message.Types.SetBlueprint:
                    SetBlueprint(e.Parameters[0] as Blueprint, (int)e.Parameters[1], (int)e.Parameters[2]);
                    return true;

                case Message.Types.Query:
                    Query(parent, e);
                    return true;

                default:
                    return true;
            }
        }

        public override void Query(GameObject parent, GameObjectEventArgs e)
        {
            List<Interaction> list = e.Parameters[0] as List<Interaction>;
            if (Blueprint == null)
            {
                //list.Add(new Interaction(TimeSpan.Zero, Message.Types.Activate, parent, "Set Blueprint"));
                return;
            }
            if (Product != null)
                if (SpriteComponent.HasOrientation(Product))
                    list.Add(new Interaction(TimeSpan.Zero, Message.Types.ManageEquipment, parent, "Change Orientation", "Changing Orientation"));
            //if (!InventoryComponent.IsHauling(e.Sender, GetFilter().Apply))
            //    return;
            if (Blueprint != null)
            {
                if (!MultiTile2Component.IsPositionValid(this.Product, parent.Map, parent.Global, (int)Product["Sprite"]["Orientation"]))
                    return;

                if (!CheckMaterials())
                {
                    ObjectFilter filter = GetFilter();

                    list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.ApplyMaterial, parent, "Apply material", range: (float)Math.Sqrt(2), 

                        effect: new InteractionEffect("Work"),

                        actorCond: new InteractionConditionCollection(
                            new InteractionCondition(foo => true, //Blueprint[0].Condition,//  agent => ControlComponent.HasAbility(agent, Message.Types.Build), "You need a tool to build with!", 
                                new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)),

                            new InteractionCondition(
                        //"Holding", true, AI.PlanType.FindNearest,
                        //precondition: foo => filter.Apply(foo) || ProductionComponent.CanProduce(foo, filter.Apply),
                            agent => InventoryComponent.IsHauling(agent, filter.Apply),
                            new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
                            new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
                        // TODO: i haven't tested this
                        //      new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                      )
                            )
                        ));
                    return;
                }

            }


            list.Add(new Interaction(new TimeSpan(0, 0, 1), Message.Types.Build, parent, "Build", stat: Stat.Building,
                //effect: new InteractionEffect[] { new InteractionEffect("Work") },
                 effect: new InteractionEffect("Work"),
                //cond: new InteractionCondition("Equipped", true, AI.PlanType.FindInventory, obj => FunctionComponent.HasAbility(obj, Message.Types.Build))));// need: new Need("Work", 20)));// "Work"));
               actorCond: new InteractionConditionCollection(
               new InteractionCondition(foo => true, new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory))
               )
               )
               );

        }

        public override bool Give(GameObject parent, GameObject giver, GameObjectSlot objSlot)
        {
            GameObject obj = objSlot.Object;
            if (obj == null)
                return true;
            objSlot.StackSize -= 1;
            if (Drop(parent, giver, obj))
            {
                //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
                Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
                int currentAmount;
                if (!currentMats.TryGetValue(obj.ID, out currentAmount))
                    currentAmount = 0;
                currentMats[obj.ID] = currentAmount + 1;
                return true;
            }
            return true;
        }

        private bool CheckMaterials()
        {
            foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentReqMaterials())
            {
                int currentAmount;
                if (!GetCurrentMaterials().TryGetValue(mat.Key, out currentAmount))
                    return false;
                if (currentAmount < mat.Value)
                    return false;
            }
            return true;
        }

        Dictionary<GameObject.Types, int> GetCurrentReqMaterials()
        {
            return this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];//Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }

        Dictionary<GameObject.Types, int> GetCurrentMaterials()
        {
            return this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }

        private void Complete(GameObject parent, GameObject builder)
        {
            //GameObject product = GameObject.Create(this.GetProperty<Blueprint>("Blueprint").ProductID);
            //Vector3 global = parent.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position").Global; // target.Position.Global;
            //SpriteComponent sprComp = product.GetComponent<SpriteComponent>("Sprite");
            if ((int)this[Stat.Stage.Name] < this.GetProperty<Blueprint>("Blueprint").Stages.Count - 1)
            {
                this[Stat.Stage.Name] = (int)this[Stat.Stage.Name] + 1;
                this[Stat.Value.Name] = 60f;
                return;
            }
            GameObject product = (GameObject)this["Product"];
            // GameObject product = GameObject.Create(GetProduct().ID);
            //product.Global = parent.Global;
            Chunk.AddObject(product, parent.Map, parent.Global);
            Chunk.RemoveBlockObject(parent);//, global);
            product.Spawn();
            product.HandleMessage(Message.Types.Constructed, parent, builder);
        }

        private ObjectFilter GetFilter()
        {
            ObjectFilter filter = new ObjectFilter(FilterType.Include);
            foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentReqMaterials())
            {
                int currentAmount;
                if (GetCurrentMaterials().TryGetValue(mat.Key, out currentAmount))
                    if (currentAmount >= mat.Value)
                        continue;
                List<GameObject.Types> existing;
                string type = GameObject.Objects[mat.Key].Type;
                if (!filter.TryGetValue(type, out existing))
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
            List<GameObjectSlot> list = new List<GameObjectSlot>();
            Dictionary<GameObject.Types, int> reqs = GetCurrentReqMaterials();
            Dictionary<GameObject.Types, int> current = GetCurrentMaterials();
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

        public override object Clone()
        {
            ConstructionBlockComponent comp = new ConstructionBlockComponent();
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            return comp;
        }

        static public ConstructionBlockComponent Create(GameObject obj)
        {
            TileComponent.Create(obj, Tile.Types.Construction, hasData: true, transparency: 0f, density: 1f);
            return new ConstructionBlockComponent();
            //return TileComponent.Create(obj, TileBase.Types.Farmland) as FarmlandComponent;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            // tooltip.Controls.Clear();
            Blueprint bp = this["Blueprint"] as Blueprint;
            if (bp == null)
                GetConstructionTooltip(parent, tooltip);
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
            SpriteComponent sprComp = bp != null ? GameObject.Objects[bp.ProductID].GetComponent<SpriteComponent>("Sprite") : obj.GetComponent<SpriteComponent>("Sprite");
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
            Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            Dictionary<GameObject.Types, int> reqMats = null;
            if (bp != null)
                reqMats = bp.Stages[(int)this[Stat.Stage.Name]];
            foreach (KeyValuePair<GameObject.Types, int> mat in reqMats)
            {
                Slot slot = new Slot(new Vector2(i, label.Bottom));
                slot.AutoText = false;
                GameObjectSlot invSlot = new GameObjectSlot();
                invSlot.Object = GameObject.Create(mat.Key);
                //slot.SetBottomRightText(mat.Value.ToString() + (bp != null ? "/" + reqMats[mat.Key] : ""));
                int currentAmount;
                slot.SetBottomRightText((currentMats.TryGetValue(mat.Key, out currentAmount) ? currentAmount.ToString() : "0") + "/" + mat.Value.ToString());
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
            SpriteComponent sprComp = bp != null ? GameObject.Objects[bp.ProductID].GetComponent<SpriteComponent>("Sprite") : obj.GetComponent<SpriteComponent>("Sprite");
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
            Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
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

            box.Controls.Add(lbl_bp, pnl_pic, panelMaterials);
            tooltip.Controls.Add(box);
        }

        public override void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent)// DrawObjectArgs e)
        {
            if (Product == null)
                return;
            MultiTile2Component multi;
            if (Product.TryGetComponent<MultiTile2Component>("Multi", out multi))
                multi.DrawPreview(sb, camera, parent.Global, (int)Product["Sprite"]["Orientation"]);
            else
            {
                //SpriteComponent sprite;
                //if (Product.TryGetComponent<SpriteComponent>("Sprite", out sprite))
                //    sprite.DrawPreview(sb, camera, Position.Round(parent.Global), (int)Product["Sprite"]["Orientation"]);
                SpriteComponent.DrawPreview(sb, camera, Position.Round(parent.Global), Product);
            }
        }

        internal override List<Tag> Save()
        {
            List<Tag> save = new List<Tag>();
            //  save.Add(new Tag(Tag.Types.Int, "ProductID", (int)this.GetProperty<Blueprint>("Blueprint").ProductID));
            save.Add(new Tag(Tag.Types.Compound, "Product", Product.Save()));
            save.Add(new Tag(Tag.Types.Byte, "Stage", (byte)(int)this[Stat.Stage.Name]));
            Tag mats = new Tag(Tag.Types.Compound, "Materials");
            foreach (KeyValuePair<GameObject.Types, int> mat in GetCurrentMaterials())
                mats.Add(new Tag(Tag.Types.Byte, ((int)mat.Key).ToString(), (byte)mat.Value));
            save.Add(mats);
            return save;
        }

        internal override void Load(Tag compTag)
        {
            Dictionary<string, Tag> byName = compTag.ToDictionary();
            this["Product"] = GameObject.Create(byName["Product"]);
           // this["Blueprint"] = BlueprintComponent.Blueprints[Product.ID];
            // TODO: this is ridiculous
            this["Blueprint"] = WorkbenchComponent.GetBlueprint(Product.ID);// WorkbenchComponent.Blueprints.Skip(1).ToList().Find(bp => ((Blueprint)bp["Blueprint"]["Blueprint"]).ProductID == Product.ID)["Blueprint"]["Blueprint"];
            this[Stat.Stage.Name] = (int)(byte)byName["Stage"].Value;
            foreach (Tag mat in (List<Tag>)byName["Materials"].Value)
            {
                if (mat.Value == null)
                    continue;
                GetCurrentMaterials()[(GameObject.Types)Enum.Parse(typeof(GameObject.Types), mat.Name)] = Convert.ToInt16((byte)mat.Value);
            }
        }
    }
}
