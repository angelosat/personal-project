using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class StructureComponent : Component
    {
        public override string ComponentName
        {
            get { return "Structure"; }
        }
        public override object Clone()
        {
            return new StructureComponent();
        }

        public List<ItemRequirement> Materials { get { return (List<ItemRequirement>)this["Materials"]; } set { this["Materials"] = value; } }
        Reaction.Product.ProductMaterialPair Product { get { return (Reaction.Product.ProductMaterialPair)this["Product"]; } set { this["Product"] = value; } }
        public Progress Progress { get { return (Progress)this["Progress"]; } set { this["Progress"] = value; } }
        float Timer { get { return (float)this["Timer"]; } set { this["Timer"] = value; } }

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            var sprite = new Sprite(this.Product.Product.Body.Sprite);
            sprite.Alpha = 0.5f;
            parent.Body.Sprite = sprite;
            parent.GetComponent<SpriteComponent>().Sprite = sprite;
        }
        public override void ObjectLoaded(GameObject parent)
        {
            var sprite = new Sprite(this.Product.Product.Body.Sprite);
            sprite.Alpha = 0.5f;
            parent.Body.Sprite = sprite;
            parent.GetComponent<SpriteComponent>().Sprite = sprite;
        }
        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new CraftingTooltip(this.Product.Product.ToSlot(), this.Product.Req) { Location = tooltip.Controls.BottomLeft });

            PanelLabeled panelMats = new PanelLabeled("Materials") { Location = tooltip.Controls.BottomLeft };
            foreach (var req in this.Materials)
            {
                SlotWithText slotReq = new SlotWithText(panelMats.Controls.BottomLeft) { Tag = GameObject.Objects[req.ObjectID].ToSlot() };
                slotReq.Slot.CornerTextFunc = o => req.Amount.ToString() + "/" + req.Max.ToString();
                slotReq.OnGameEventAction = (e) => {
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
        }

        public StructureComponent()
        {
            this.Timer = Engine.TargetFps;
            this.Materials = new List<ItemRequirement>();
            this.Progress = new Progress() { Min = 0, Max = 100, Value = 0 };
        }

        public bool BuildCondition()
        {
            foreach (var mat in this.Materials)
                if (mat.Remaining > 0)
                    return false;
            return this.Progress.Percentage < 1;
            //return true;
        }

        public override void Update(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            base.Update(net, parent, chunk);
            Timer--;
            if (Timer > 0)
                return;
            Timer = Engine.TargetFps;
            this.DetectMaterials(parent);
            //this.Ready = ConstructionFootprint.MaterialsAvailable(net, parent);
        }

        public void DetectMaterials(GameObject parent)
        {
            if (this.Product.Req.IsNull())
                return;
            this.Product.Req.Amount = 0;
            var nearbyMaterials = parent.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material);
            Dictionary<GameObject.Types, int> nearby = new Dictionary<GameObject.Types, int>();
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
            //var mats = new Queue<GameObject>(actor.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material).Where(o => o.GetInfo().ID == this.Product.Req.ObjectID));
            var mats = new Queue<GameObject>(actor.GetNearbyObjects(r => r <= 2, o => o.GetInfo().ID == this.Product.Req.ObjectID));
            foreach (var mat in mats)
                if (mat.GetInfo().ID == this.Product.Req.ObjectID)
                    this.Product.Req.Amount += mat.StackSize;
            int remaining = this.Product.Req.Max;
            List<GameObject> totake = new List<GameObject>();
            while(mats.Count>0)
            {
                var obj = mats.Dequeue();
                totake.Add(obj);
                remaining -= obj.StackSize;
                if (remaining <= 0)
                    break;
            }
            remaining = this.Product.Req.Max;
            foreach(var item in totake)
            {
                if(remaining >= item.StackSize)
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
            var clone = this.Product.Product.Clone();
            clone.Global = parent.Global;
            parent.Net.Spawn(clone);
            parent.Net.Despawn(parent);
            parent.Net.DisposeObject(parent);
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {
            //list.Add(new ScriptTask("Building", 2, actor => this.Finish(parent, actor), new TaskConditionCollection(
            //    new RangeCondition(() => parent.Global, Interaction.DefaultRange),
            //    new ScriptTaskCondition("Materials", actor => this.DetectMaterials(parent, actor), Message.Types.InsufficientMaterials)
            //    )));
            list.Add(new BuildStructure());
        }
        public override void GetRightClickActions(GameObject parent, List<ContextAction> list)
        {
            var item = new BuildStructure();
            list.Add(new ContextAction(() => item.Name, () => Client.PlayerInteract(new TargetArgs(parent))));
        }

        public void SetProduct(Reaction.Product.ProductMaterialPair product)
        {
            this.Product = product;
            this.Materials.Clear();
            this.Materials.Add(new ItemRequirement(product.Req));
        }

        void Build(GameObject parent, GameObject builder)
        {
            this.Progress.Value += 1f;// 0.1f;
            if (this.Progress.Percentage >= 1)
            {
                this.ProgressBarHideDelay = 0;
                this.Finish(parent, builder);
            }
            this.ProgressBarHideDelay = this.ProgressBarHideDelayMax;
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Product.Write(writer);
            writer.Write(this.Materials.Count);
            foreach (var mat in this.Materials)
                writer.Write(mat.Amount);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            //this.Product = new Reaction.Product.ProductMaterialPair(reader);
            this.SetProduct(new Reaction.Product.ProductMaterialPair(reader));
            //this.Materials.First().Amount = reader.ReadInt32();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.Materials[i].Amount = reader.ReadInt32();
            }
        }

        internal override List<SaveTag> Save()
        {
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
            //this.Product = new Reaction.Product.ProductMaterialPair(save);
            //this.SetProduct(new Reaction.Product.ProductMaterialPair(save));

            this.SetProduct(new Reaction.Product.ProductMaterialPair(save["Product"]));

            var matList = save.GetValue<List<SaveTag>>("Materials");
            foreach (var mat in matList)
            {
                var id = mat.GetValue<int>("ID");
                var amount = mat.GetValue<int>("Amount");
                var req = this.Materials.First(r => r.ObjectID == id);
                req.Amount = amount;
            }
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(new PlayerInput(PlayerActions.Drop), new InteractionCancelConstruction());
            list.Add(new PlayerInput(PlayerActions.PickUp), new InteractionAddMaterial());
            //list.Add(new PlayerInput(PlayerActions.RB), new InteractionFinishConstruction());// BuildStructure());
            list.Add(new PlayerInput(PlayerActions.Interact), new InteractionBuild(this.Build));// BuildStructure());
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
                UI.Bar.Draw(sb, camera, parent, "", this.Progress.Percentage, 1);//camera.Zoom);
        }


        class InteractionBuild : Interaction
        {
            Action<GameObject, GameObject> BuildAction;
            public InteractionBuild(Action<GameObject, GameObject> buildAction)
            {
                this.Name = "Build";
                this.RunningType = RunningTypes.Continuous;
                this.BuildAction = buildAction;
            }
            static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                        new ScriptTaskCondition("NotFinished", NotFinished, Message.Types.Default),
                        new ScriptTaskCondition("Materials", AvailabilityCondition, Message.Types.InteractionFailed))
                        );
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            static bool AvailabilityCondition(GameObject actor, TargetArgs target)
            {
                StructureComponent constr = target.Object.GetComponent<StructureComponent>();
                var toolSlot = GearComponent.GetSlot(actor, GearType.Mainhand);
                if (toolSlot.Object == null)
                    return false;
                if (!SkillComponent.HasSkill(toolSlot.Object, Skills.Skill.Building))
                    return false;
                return constr.BuildCondition();
            }
            static bool NotFinished(GameObject actor, TargetArgs target)
            {
                StructureComponent constr = target.Object.GetComponent<StructureComponent>();
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
                : base("Add Material",1)
            {
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
                StructureComponent constr = target.GetComponent<StructureComponent>();
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
                StructureComponent constr = t.Object.GetComponent<StructureComponent>();
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
                    RangeCheck.Sqrt2));
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
                    StructureComponent constr = t.Object.GetComponent<StructureComponent>();
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
        class InteractionFinishConstruction : Interaction
        {
            public InteractionFinishConstruction()
            {
                this.Name = "Finish Construction";
                this.SetSeconds(2);
            }
            static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new RangeCheck(t => t.Object.Global, InteractionOld.DefaultRange),
                        new ScriptTaskCondition("AllMaterials", AvailabilityCondition, Message.Types.InvalidTarget)
                    ));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            static bool AvailabilityCondition(GameObject a, TargetArgs t)
            {
                StructureComponent constr = t.Object.GetComponent<StructureComponent>();
                foreach (var mat in constr.Materials)
                    if (mat.Remaining > 0)
                        return false;
                return true;
            }

            public override void Perform(GameObject a, TargetArgs t)
            {
                StructureComponent constr = t.Object.GetComponent<StructureComponent>();
                constr.Finish(t.Object, a);
                //var server = a.Net as Server;
                //if (server != null)
                //{
                //    StructureComponent constr = t.Object.GetComponent<StructureComponent>();
                //    foreach (var mat in constr.Materials)
                //    {
                //        for (int i = 0; i < mat.Amount; i++)
                //        {
                //            var matEntity = GameObject.Create(mat.ObjectID);
                //            server.PopLoot(matEntity, t.Object);
                //        }
                //    }
                //}
                //a.Net.Despawn(t.Object);
                //a.Net.DisposeObject(t.Object);
            }
            public override object Clone()
            {
                return new InteractionFinishConstruction();
            }
        }
    }
}
