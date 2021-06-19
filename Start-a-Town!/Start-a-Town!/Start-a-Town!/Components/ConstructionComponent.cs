using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    class ConstructionComponent : Component// ConstructionFootprint
    {
        public override string ComponentName
        {
            get { return "Construction"; }
        }
        public override object Clone()
        {
            return new ConstructionComponent();
        }
        private void InitParentSprite(GameObject parent)
        {
            return;
            var entity = this.Product.Product.GetEntity();
            Sprite sprite = new Sprite(entity.Body.Sprite);
            var tint = this.Product.Product.BlockState.GetTint(this.Product.Data);
            foreach (var overlay in sprite.Overlays)
            {
                overlay.Value.Tint = tint;
                overlay.Value.Alpha = 0.5f;
            }
            sprite.Alpha = 0;
            parent.Body.Sprite = sprite;
            parent.GetComponent<SpriteComponent>().Sprite = sprite;
        }
        public override void Spawn(IObjectProvider net, GameObject parent)
        {
            InitParentSprite(parent);
        }

        public ConstructionComponent()
        {
            this.Materials = new List<ItemRequirement>();
            this.Progress = new Progress() { Min = 0, Max = 100, Value = 0 };
        }

        public override void ObjectLoaded(GameObject parent)
        {
            InitParentSprite(parent);

            ////Sprite sprite = new Sprite(this.Product.Product.Entity.GetObject().GetSprite());
            //Sprite sprite = new Sprite(this.Product.Product.GetEntity().GetSprite());
            ////sprite.Alpha = 0.5f;
            //sprite.Tint = this.Product.Product.BlockState.GetTint(this.Product.Data);
            //gameObject.Body.Sprite = sprite;
            //gameObject.GetComponent<SpriteComponent>().Sprite = sprite;
        }

        //public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        //{
        //    Atlas.Node.Token token = this.Product.Product.Variations.First();
        //    Rectangle bounds = camera.GetScreenBounds(parent.Global, Block.Bounds);
        //    Vector2 pos = new Vector2(bounds.X, bounds.Y);
        //    this.Product.Product.Draw(sb, pos, Color.White, Vector4.One, Color.White * 0.5f, camera.Zoom, parent.Global.GetDrawDepth(parent.Map, camera), new Cell() { BlockData = this.Product.Data });
        //}

        public BlockConstruction.ProductMaterialPair Product { get { return (BlockConstruction.ProductMaterialPair)this["Product"]; } set { this["Product"] = value; } }
        public List<ItemRequirement> Materials { get { return (List<ItemRequirement>)this["Materials"]; } set { this["Materials"] = value; } }
        public Progress Progress { get { return (Progress)this["Progress"]; } set { this["Progress"] = value; } }
        Container Material;
        //public Block.Types BlockType { get; set; }
        //public byte BlockData { get; set; }

        //public void Finish(IObjectProvider net, GameObject parent, GameObject actor)
        //{
        //    List<GameObject> mats;
        //    if (ConstructionFootprint.MaterialsAvailable(net, parent, out mats))
        //    {
        //        foreach (var mat in mats)
        //        {
        //            net.Despawn(mat);
        //            net.SyncDisposeObject(mat);
        //        }
        //    }
        //    net.PostLocalEvent(parent, Message.Types.Construct, actor);

        //    //net.Spawn(this.Product.Object, parent.Global);
        //    net.SetBlock(parent.Global, this.Product.Product.Type, this.Product.Data);
        //    net.Despawn(parent);
        //    net.DisposeObject(parent.NetworkID);
        //    Skill.Award(net, actor, parent, Skill.Types.Construction, 1);
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.Insert:
                    if (this.Materials.Count == 0)
                        return true;
                    var objSlot = e.Parameters[0] as GameObjectSlot;
                    if (!objSlot.HasValue)
                        return true;
                    var obj = objSlot.Object;
                    //if (!this.Materials.Any(req => req.ObjectID == (int)obj.ID))
                    //    return true;
                    var req = this.Materials.FirstOrDefault(r => r.ObjectID == (int)obj.ID);
                    if (req == null)
                        return true;
                    int amount = Math.Min(req.Remaining, obj.StackSize);
                    objSlot.StackSize -= amount;
                    if(objSlot.StackSize == 0)
                    {
                        parent.Net.SyncDisposeObject(obj);
                    }
                    req.Amount += amount;
                    return true;

                default:
                    return true;
            }
        }

        public bool BuildCondition()
        {
            foreach(var mat in this.Materials)
                if (mat.Remaining > 0)
                    return false;
            return this.Progress.Percentage < 1;
            //return true;
        }
        public bool HasMaterials()
        {
            foreach (var mat in this.Materials)
                if (mat.Remaining > 0)
                    return false;
            return true;
        }
        public bool DetectMaterials(GameObject parent, GameObject actor)
        {
            //if (Vector3.Distance(parent.Global, actor.Global) > 2)
            //    return false;
            this.Product.Req.Amount = 0;
            var nearbyMaterials = actor.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material);
            foreach (var mat in nearbyMaterials)
            {
                //if (!nearby.ContainsKey(mat.ID))
                //{
                //    nearby[mat.ID] = mat.StackSize;
                //    continue;
                //}
                //nearby[mat.ID] += mat.StackSize;
                if (mat.GetInfo().ID == this.Product.Req.ObjectID)
                    this.Product.Req.Amount += mat.StackSize;
            }
            return this.Product.Req.Amount >= this.Product.Req.Max;
        }

        public void Finish(GameObject parent, GameObject actor)
        {
            parent.Net.Despawn(parent);            // despawn parent first because block can't be places on occupied cells
            parent.Net.DisposeObject(parent);
            this.Product.SpawnProduct(parent.Map, parent.Global);
            
            return;

            //var mats = new Queue<GameObject>(actor.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material).Where(o => o.GetInfo().ID == this.Product.Req.ObjectID));
            var mats = new Queue<GameObject>(actor.GetNearbyObjects(r => r <= 2, o => o.GetInfo().ID == this.Product.Req.ObjectID));
            foreach (var mat in mats)
                if (mat.GetInfo().ID == this.Product.Req.ObjectID)
                    this.Product.Req.Amount += mat.StackSize;
            int remaining = this.Product.Req.Max;
            List<GameObject> totake = new List<GameObject>();
            while (mats.Count > 0)
            {
                var obj = mats.Dequeue();
                totake.Add(obj);
                remaining -= obj.StackSize;
                if (remaining <= 0)
                    break;
            }
            remaining = this.Product.Req.Max;
            foreach (var item in totake)
            {
                if (remaining >= item.StackSize)
                {
                    parent.Net.Despawn(item);
                    parent.Net.DisposeObject(item);
                    remaining -= item.StackSize;
                }
                else
                {
                    item.StackSize -= remaining;
                }
            }
            //var clone = this.Product.Product.Clone();
            //clone.Global = parent.Global;
            //parent.Net.Spawn(clone);
            var orientation = 0;
            parent.Net.SyncSetBlock(parent.Global, this.Product.Product.Type, this.Product.Data, orientation);
            parent.Net.Despawn(parent);
            parent.Net.DisposeObject(parent);
        }
        internal override void GetAvailableActions(List<Script.Types> list)
        {
            list.Add(Script.Types.Build);
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            Block block = Product.Product;
            PanelLabeled panel = new PanelLabeled("Product") { Location = tooltip.Controls.BottomLeft, AutoSize = true };
            Slot<Block> icon = new Slot<Block>() { Location = panel.Controls.BottomLeft, Tag = block };
            //GroupBox objtooltip = new GroupBox() { Location = icon.TopRight };
            //product.Object.GetInfo().GetTooltip(product.Object, objtooltip);
            panel.Controls.Add(icon, new Label(block.Type.ToString()) { Location = icon.TopRight });
            //this.Controls.Add(panel);
            tooltip.Controls.Add(panel);

            PanelLabeled panelMats = new UI.PanelLabeled("Materials") { Location = tooltip.Controls.BottomLeft, AutoSize = true };
            Label lblMats = new Label("Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            panelMats.Controls.Add(lblMats);

            ////foreach (var req in this.Product.Req)
            ////{
            //var req = this.Product.Req;
            //    SlotWithText slotReq = new SlotWithText(panelMats.Controls.BottomLeft) { Tag = GameObject.Objects[req.ObjectID].ToSlot() };
            //    slotReq.Slot.CornerTextFunc = o => req.Amount.ToString() + "/" + req.Max.ToString();
            //    panelMats.Controls.Add(slotReq);
            ////}

            foreach(var req in this.Materials)
            {
                SlotWithText slotReq = new SlotWithText(panelMats.Controls.BottomLeft) { Tag = GameObject.Objects[req.ObjectID].ToSlot() };
                slotReq.Slot.CornerTextFunc = o => req.Amount.ToString() + "/" + req.Max.ToString();
                slotReq.OnGameEventAction = (e) =>
                {
                    if (e.Type != Message.Types.InventoryChanged)
                        return;
                    var obj = e.Parameters[0] as GameObject;
                    if (obj != parent)
                        return;
                    slotReq.Invalidate(true);
                };
                panelMats.Controls.Add(slotReq);
            }

            tooltip.Controls.Add(panelMats);
            new InteractionCustom("Building", 2, (a,t) => this.Finish(t.Object, a)).GetTooltip(tooltip);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            //writer.Write((int)this.BlockType);
            this.Product.Write(w);
            foreach (var mat in this.Materials)
                w.Write(mat.Amount);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            //this.BlockType = (Block.Types)reader.ReadInt32();
            //this.Product = new BlockConstruction.ProductMaterialPair(r);
            this.SetProduct(new BlockConstruction.ProductMaterialPair(r));
            this.Materials.First().Amount = r.ReadInt32();
        }

        internal override List<SaveTag> Save()
        {
            ////var save = new List<SaveTag>();
            //////save.Add(new SaveTag(SaveTag.Types.Int, "Block", (int)this.BlockType));
            ////save.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Save()));
            ////return save;

            //return this.Product.Save();

            List<SaveTag> save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Save()));
            var matsTag = new SaveTag(SaveTag.Types.List, "Materials", SaveTag.Types.Compound);
            foreach (var mat in this.Materials)
            {
                SaveTag tag = new SaveTag(SaveTag.Types.Compound);
                tag.Add(new SaveTag(SaveTag.Types.Int, "ID", mat.ObjectID));
                tag.Add(new SaveTag(SaveTag.Types.Int, "Amount", mat.Amount));
                matsTag.Add(tag);
            }
            save.Add(matsTag);
            return save;
        }
        internal override void Load(SaveTag save)
        {
            ////save.TryGetTagValue<int>("Block", value => this.BlockType = (Block.Types)value);
            ////save.TryGetTagValue<int>("Block", value => this.Product = new BlockConstruction.ProductMaterialPair(Block.Registry[(Block.Types)value], 0, new ItemRequirement(GameObject.Types.WoodenPlank, 1)));
            ////this.Product = new BlockConstruction.ProductMaterialPair(Block.Stone, 0, new ItemRequirement(GameObject.Types.WoodenPlank, 1));

            //this.SetProduct(new BlockConstruction.ProductMaterialPair(save));
            ////this.Product = new BlockConstruction.ProductMaterialPair(save);

            this.SetProduct(new BlockConstruction.ProductMaterialPair(save["Product"]));

            var matList = save.GetValue<List<SaveTag>>("Materials");
            foreach (var mat in matList)
            {
                //var matTag = mat.Value as SaveTag;
                var id = mat.GetValue<int>("ID");
                var amount =mat.GetValue<int>("Amount");
                //this.Materials[id].Amount = amount;
                var req = this.Materials.First(r => r.ObjectID == id);
                req.Amount = amount;
            }
        }

        public void SetProduct(BlockConstruction.ProductMaterialPair product)
        {
            this.Product = product;
            this.Materials.Clear();
            this.Materials.Add(new ItemRequirement(product.Req));
            this.Material = new Container(1, obj => (int)obj.ID == product.Req.ObjectID);
        }

        //public ScriptToken GetInteraction(GameObject parent)
        //{
        //    ScriptToken script = new ScriptToken();
        //    script.Components.Add(new ScriptTask("Building", 2, (a, t) => this.Finish(t.Object, a)));
        //    return script;
        //}
        internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {
            //list.Add(new ScriptTask("Building", 2, actor => this.Finish(parent, actor), new TaskConditionCollection(
            //    new RangeCondition(()=>parent.Global, Interaction.DefaultRange),
            //    new ScriptTaskCondition("Materials", actor => this.DetectMaterials(parent, actor), Message.Types.InsufficientMaterials)

            //    )));
            list.Add(new BuildBlock());
        }
        //public override void GetRightClickActions(GameObject parent, List<ContextAction> list)
        //{
        //    list.Add(new ContextAction(() => "Build", () => Client.PlayerInteract(new TargetArgs(parent))));
        //}

        //public override void GetPlayerActions(Dictionary<KeyBinding, Interaction> list)
        //{
        //    list.Add(new KeyBinding(GlobalVars.KeyBindings.Drop), new InteractionCancelConstruction());
        //    list.Add(new KeyBinding(GlobalVars.KeyBindings.PickUp), new InteractionAddMaterial());
        //    list.Add(new KeyBinding(System.Windows.Forms.Keys.RButton), new BuildBlock());
        //}
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(new PlayerInput(PlayerActions.Drop), new InteractionCancelConstruction());
            list.Add(new PlayerInput(PlayerActions.PickUp), new InteractionAddMaterial());
            //list.Add(new PlayerInput(PlayerActions.RB), new BuildBlock());
            //list.Add(new PlayerInput(PlayerActions.Interact), new InteractionBuild(this.Build));
            list.Add(new PlayerInput(PlayerActions.Interact), new InteractionBuild2(this));
        }

        void Build(GameObject parent, GameObject builder)
        {
            this.Progress.Value += 1f;// 0.1f;
            if(this.Progress.Percentage>=1)
            {
                this.ProgressBarHideDelay = 0;
                this.Finish(parent, builder);
                //finishCallback();
            }
            this.ProgressBarHideDelay = this.ProgressBarHideDelayMax;
        }
        public override void Update(GameObject parent)
        {
            if (ProgressBarHideDelay > 0)
                this.ProgressBarHideDelay += this.ProgressBarHideChangeRate;
        }
        public override void Focus(GameObject parent)
        {
            this.ProgressBarHideDelay = this.ProgressBarHideDelayMax;
            this.ProgressBarHideChangeRate = 0;
        }
        public override void FocusLost(GameObject parent)
        {
            this.ProgressBarHideDelay = this.ProgressBarHideDelayMax;
            this.ProgressBarHideChangeRate = -1;
        }
        float ProgressBarHideDelay, ProgressBarHideChangeRate;
        readonly float ProgressBarHideDelayMax = Engine.TargetFps * 5;
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (ProgressBarHideDelay > 0)
                UI.Bar.Draw(sb, camera, parent, "", this.Progress.Percentage, 1);// 0.3f * camera.Zoom);
        }
        internal override void HandleRemoteCall(GameObject parent, Message.Types type, System.IO.BinaryReader r)
        {
            switch(type)
            {
                case Message.Types.PlayerStartConstruction:
                    var actorID = r.ReadInt32();
                    var actor = parent.Net.GetNetworkObject(actorID);
                    actor.GetComponent<WorkComponent>().Perform(actor, new InteractionBuild2(this), new TargetArgs(parent));
                    break;

                default:
                    break;
            }
        }

        class InteractionBuild : Interaction
        {
            //Action<GameObject, GameObject> BuildAction;
            //public InteractionBuild(Action<GameObject, GameObject> buildAction)
            Action<GameObject, GameObject> BuildAction;
            public InteractionBuild(Action<GameObject, GameObject> buildAction)
            {
                this.Name = "Build";
                this.RunningType = RunningTypes.Continuous;
                this.BuildAction = buildAction;
            }
            public override TaskConditions Conditions
            {
                get
                {
                    return new TaskConditions(
                    new AllCheck(
                        new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                        new ScriptTaskCondition("NotFinished", this.NotFinished, Message.Types.Default),
                        new ScriptTaskCondition("Materials", this.AvailabilityCondition, Message.Types.InteractionFailed))
                        );
                }
            }
            public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
            {
                ConstructionComponent constr = target.Object.GetComponent<ConstructionComponent>();
                //foreach (var mat in constr.Materials)
                //    if (mat.Remaining > 0)
                //        return false;
                //return true;
                var toolSlot = GearComponent.GetSlot(actor, GearType.Mainhand);
                if (toolSlot.Object == null)
                    return false;
                if (!SkillComponent.HasSkill(toolSlot.Object, Skills.Skill.Building))
                    return false;
                return constr.BuildCondition();
            }
            bool NotFinished(GameObject actor, TargetArgs target)
            {
                ConstructionComponent constr = target.Object.GetComponent<ConstructionComponent>();
                return constr.Progress.Percentage < 1;
            }

            public override void Perform(GameObject a, TargetArgs t)
            {
                this.BuildAction(t.Object, a);
            }
            public override object Clone()
            {
                return new InteractionBuild(this.BuildAction);
            }
        }
        class InteractionAddMaterial : Interaction
        {
            public InteractionAddMaterial()
            {
                this.Name = "Add Material";
                this.Length = 1;
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                    new RangeCheck(t => t.Object.Global, InteractionOld.DefaultRange),
                    new ScriptTaskCondition("Material", (a, t) => IsMaterialValid(a, t.Object), Message.Types.InteractionFailed)));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            static bool IsMaterialValid(GameObject actor, GameObject target)
            {
                //var input = GearComponent.GetSlot(actor, GearType.Hauling);
                var input = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                if (input.Object == null)
                    return false;
                ConstructionComponent constr = target.GetComponent<ConstructionComponent>();
                foreach (var mat in constr.Materials)
                    if ((int)input.Object.ID == mat.ObjectID)
                        if (mat.Remaining > 0)
                            return true;
                return false;
            }

            public override void Perform(GameObject a, TargetArgs t)
            {
                //var input = GearComponent.GetSlot(a, GearType.Hauling);
                var input = a.GetComponent<HaulComponent>().GetSlot();//.Slot;

                if (input.Object == null)
                    return;
                ConstructionComponent constr = t.Object.GetComponent<ConstructionComponent>();
                foreach (var mat in constr.Materials)
                    if ((int)input.Object.ID == mat.ObjectID)
                        if (mat.Remaining > 0)
                        {
                            var obj = input.Object;
                            int toTake = Math.Min(mat.Remaining, input.StackSize);
                            mat.Amount += toTake;
                            a.Net.EventOccured(Message.Types.InventoryChanged, t.Object);
                            input.StackSize -= toTake;
                            if (input.StackSize == 0)
                                a.Net.DisposeObject(obj);
                        }
            }

            public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
            {
                return IsMaterialValid(actor, target.Object);
            }
            public override object Clone()
            {
                return new InteractionAddMaterial();
            }
        }
        class InteractionCancelConstruction : Interaction
        {
            public InteractionCancelConstruction()
            {
                this.Name = "Cancel Construction";
                this.Length = 1;
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                    new RangeCheck(t => t.Object.Global, InteractionOld.DefaultRange)));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                var server = a.Net as Server;
                if (server != null)
                {
                    ConstructionComponent constr = t.Object.GetComponent<ConstructionComponent>();
                    foreach (var mat in constr.Materials)
                    {
                        for (int i = 0; i < mat.Amount; i++)
                        {
                            var matEntity = GameObject.Create(mat.ObjectID);
                            server.PopLoot(matEntity, t.Object);
                        }
                    }
                }
                a.Net.Despawn(t.Object);
                a.Net.DisposeObject(t.Object);
            }
            public override object Clone()
            {
                return new InteractionCancelConstruction();
            }
        }

        public class InteractionBuild2 : Interaction
        {
            Action<GameObject, GameObject> BuildAction;
            ConstructionComponent Construction;
            public InteractionBuild2(ConstructionComponent comp)
            {
                this.Name = "Build";
                this.RunningType = RunningTypes.Continuous;
                //this.BuildAction = buildAction;
                this.Construction = comp;
            }
            static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new RangeCheck(t => t.Global, InteractionOld.DefaultRange)
                //,
                //new ScriptTaskCondition("NotFinished", this.NotFinished, Message.Types.Default)
                //,
                //new ScriptTaskCondition("Materials", this.AvailabilityCondition, Message.Types.InteractionFailed)
                        ));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            //public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
            //{
            //    ConstructionComponent constr = target.Object.GetComponent<ConstructionComponent>();
            //    var toolSlot = GearComponent.GetSlot(actor, GearType.Mainhand);
            //    if (toolSlot.Object == null)
            //        return false;
            //    if (!SkillComponent.HasSkill(toolSlot.Object, Skills.Skill.Building))
            //        return false;
            //    return constr.BuildCondition();
            //}
            bool NotFinished(GameObject actor, TargetArgs target)
            {
                ConstructionComponent constr = target.Object.GetComponent<ConstructionComponent>();
                return constr.Progress.Percentage < 1;
            }

            public override void Perform(GameObject a, TargetArgs t)
            {
                //var req = this.Construction.Materials.First();
                //if(!a.GetComponent<PersonalInventoryComponent>().Take(this.Construction.Materials))
                //{
                //    Client.Console.Write("Not enough materials");
                //    return;
                //}
             
                this.Construction.Build(t.Object, a);
                if (this.Construction.Progress.Percentage >= 1)
                    this.State = States.Finished;
            }
            public override void Start(GameObject a, TargetArgs t)
            {
                base.Start(a, t);
                if (this.Construction.HasMaterials())
                    return;
                //if (!a.GetComponent<PersonalInventoryComponent>().Take(this.Construction.Materials))
                //{
                //    Client.Console.Write("Not enough materials");
                //    this.State = States.Finished;
                //    return;
                //}
                foreach (var mat in this.Construction.Materials)
                    mat.Remaining = 0;
            }
            public override object Clone()
            {
                return new InteractionBuild2(this.Construction);
            }
        }
    }
}
