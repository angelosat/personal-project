using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ConstructionFootprint : EntityComponent
    {
        public abstract class Spawner
        {
            public abstract void Spawn(IObjectProvider net, Vector3 global);
            class BlockSpawner : Spawner
            {
                Block.Types Type;
                public BlockSpawner(Block.Types type)
                {
                    this.Type = type;
                }
                public override void Spawn(IObjectProvider net, Vector3 global)
                {
                    net.SyncSetBlock(global, this.Type);
                }
            }
            class EntitySpawner : Spawner
            {
                GameObject.Types Type;
                public EntitySpawner(GameObject.Types type)
                {
                    this.Type = type;
                }
                public override void Spawn(IObjectProvider net, Vector3 global)
                {
                    net.Spawn(GameObject.Create(this.Type), global);
                }
                //static public readonly Spawner Block = new BlockSpawner();
                //static public readonly Spawner Entity = new EntitySpawner();
            }

            //public virtual void Spawn(IObjectProvider net, Vector3 global) { }
            //class BlockSpawner : Spawner
            //{
            //    Block.Types Type;
            //    public BlockSpawner(Block.Types type)
            //    {
            //        this.Type = type;
            //    }
            //    public void Spawn(IObjectProvider net, Vector3 global)
            //    {
            //        global.TrySetCell(net, this.Type);
            //    }
            //}
            //class EntitySpawner : Spawner
            //{
            //    GameObject.Types Type;
            //    public EntitySpawner(GameObject.Types type)
            //    {
            //        this.Type = type;
            //    }
            //    public void Spawn(IObjectProvider net, Vector3 global)
            //    {
            //        net.Spawn(GameObject.Create(this.Type), global);
            //    }
            //}
        }

        public override string ComponentName
        {
            get { return "ConstructionFootprint"; }
        }

        public override object Clone()
        {
            return new ConstructionFootprint();
        }

        public override void OnSpawn(IObjectProvider net, GameObject parent)
        {
            parent.Body.Sprite = this.Product.Object.GetSprite();
            parent.Body.Sprite.Alpha = 0.5f;
            parent.GetComponent<SpriteComponent>().Sprite = this.Product.Object.GetSprite();

            this.Items.Clear();
            foreach (var mat in this.Materials)
                Items[mat.Key] = new ItemRequirement(mat.Key, mat.Value);
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            parent.Body.Sprite = this.Product.Object.GetSprite();
            parent.Body.Sprite.Alpha = 0.5f;
            parent.GetComponent<SpriteComponent>().Sprite = this.Product.Object.GetSprite();

            this.Items.Clear();
            foreach (var mat in this.Materials)
                Items[mat.Key] = new ItemRequirement(mat.Key, mat.Value);
        }

        public GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        Dictionary<GameObject.Types, int> Materials { get { return (Dictionary<GameObject.Types, int>)this["Materials"]; } set { this["Materials"] = value; } }
        //List<GameObject> AreaOfEffect { get { return (List<GameObject>)this["AreaOfEffect"]; } set { this["AreaOfEffect"] = value; } }
        Dictionary<GameObject.Types, ItemRequirement> Items;
        float Timer { get { return (float)this["Timer"]; } set { this["Timer"] = value; } }
        public bool Ready { get { return (bool)this["Ready"]; } set { this["Ready"] = value; } }
        public ConstructionFootprint()
        {
            this.Product = GameObjectSlot.Empty;
            this.Materials = new Dictionary<GameObject.Types, int>();
            Timer = Engine.TicksPerSecond;
            Ready = false;
            this.Items = new Dictionary<GameObject.Types, ItemRequirement>();
            //this.Ready = false;
            //this.Timer = Engine.TargetFps;
        }

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            base.Tick(net, parent, chunk);
            Timer--;
            if (Timer > 0)
                return;
            Timer = Engine.TicksPerSecond;
            this.Ready = ConstructionFootprint.MaterialsAvailable(net, parent);
        }

        public GameObject SetBlueprint(Blueprint bp, int variation = 0, int orientation = 0)
        {
            //if (Blueprint != null)
            //    return null;
            //this["Blueprint"] = bp;
            GameObject product = GameObject.Create(bp.ProductID);
            product["Sprite"]["Variation"] = variation;
            product["Sprite"]["Orientation"] = orientation;
            
            //this["Product"] = product;
            this.Product = product.ToSlotLink();

            this.Materials.Clear();
            foreach (var mat in bp.Stages[0])
                this.Materials[mat.Key] = mat.Value;
                //this.Materials.Add(new ItemRequirement(mat.Key, mat.Value));
            this.Items.Clear();
            foreach (var mat in this.Materials)
                Items[mat.Key] = new ItemRequirement(mat.Key, mat.Value);
                //Items.Add(new ItemRequirement(mat.Key, mat.Value));

            return product;
        }

        /// <summary>
        /// Check if required materials are in range
        /// </summary>
        /// <param name="net"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        //public bool MaterialsAvailable(IObjectProvider net, GameObject parent)
        //{
        //    var nearbyMaterials = parent.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material);
        //    Dictionary<GameObject.Types, int> nearby = new Dictionary<GameObject.Types, int>();
        //    foreach(var mat in nearbyMaterials)
        //    {
        //        if(!nearby.ContainsKey(mat.ID))
        //        {
        //            nearby[mat.ID] = mat.StackSize;
        //            continue;
        //        }
        //        nearby[mat.ID] += mat.StackSize;
        //    }
        //    foreach(var mat in this.Materials)
        //    {
        //        if (!nearby.ContainsKey(mat.Key))
        //            return false;
        //        if (nearby[mat.Key] < mat.Value)
        //            return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Check if required materials are in range
        /// </summary>
        /// <param name="net"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        static public bool MaterialsAvailable(IObjectProvider net, GameObject parent)
        {
            var comp = parent.GetComponent<ConstructionFootprint>();
            if (comp == null)
                return false;
            var nearbyMaterials = parent.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material);
            Dictionary<GameObject.Types, int> nearby = new Dictionary<GameObject.Types, int>();
            foreach (var mat in nearbyMaterials)
            {
                if (!nearby.ContainsKey(mat.IDType))
                {
                    nearby[mat.IDType] = mat.StackSize;
                    continue;
                }
                nearby[mat.IDType] += mat.StackSize;
            }
            //foreach (var mat in comp.Materials)
            //{
            //    if (!nearby.ContainsKey(mat.Key))
            //        return false;
            //    if (nearby[mat.Key] < mat.Value)
            //        return false;
            //}
            //comp.Items.Clear();
            foreach (var mat in comp.Materials)
            {
                //var req = new ItemRequirement(mat.Key, mat.Value);
                //comp.Items.Add(req);

                //if (!nearby.ContainsKey(mat.Key))
                //    comp.Items[mat.Key].Amount = 0;
                //else
                //    comp.Items[mat.Key].Amount = nearby[mat.Key];
            }
            return comp.Items.Values.FirstOrDefault(req => req.AmountCurrent < req.AmountRequired) != null;
        }
        static public bool MaterialsAvailable(IObjectProvider net, GameObject parent, out List<GameObject> nearbyMaterials)
        {
            var comp = parent.GetComponent<ConstructionFootprint>();
            if (comp == null)
            {
                nearbyMaterials = new List<GameObject>();
                return false;
            }
            nearbyMaterials = parent.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material);
            Dictionary<GameObject.Types, int> nearby = new Dictionary<GameObject.Types, int>();
            foreach (var mat in nearbyMaterials)
            {
                if (!nearby.ContainsKey(mat.IDType))
                {
                    nearby[mat.IDType] = mat.StackSize;
                    continue;
                }
                nearby[mat.IDType] += mat.StackSize;
            }
            foreach (var mat in comp.Materials)
            {
                if (!nearby.ContainsKey(mat.Key))
                    return false;
                if (nearby[mat.Key] < mat.Value)
                    return false;
            }

            // should caller dispose materials instead?
            //foreach (var mat in nearbyMaterials)
            //{
            //    net.Despawn(mat);
            //    net.SyncDisposeObject(mat);
            //}

            return true;
        }

        public virtual void Finish(IObjectProvider net, GameObject parent, GameObject actor)
        {
            List<GameObject> mats;
            if (ConstructionFootprint.MaterialsAvailable(net, parent, out mats))
            {
                foreach (var mat in mats)
                {
                    net.Despawn(mat);
                    net.SyncDisposeObject(mat);
                }
            }
            net.PostLocalEvent(parent, Message.Types.Construct, actor);

            net.Spawn(this.Product.Object, parent.Global);
            net.Despawn(parent);
            net.DisposeObject(parent.RefID);
            //SkillsComponent.AwardSkill(net, parent, Skill.Types.Construction, 1);
            SkillOld.Award(net, actor, parent, SkillOld.Types.Construction, 1);
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            //this.Product.Object.GetTooltip(tooltip);
            if (this.Product.Object == null)
                return;

            //UI.Panel panel = new UI.Panel() { Location = tooltip.Controls.BottomLeft };
            //panel.AutoSize = true;
            ////panel.Controls.Add(this.Product.Object.GetInfo().GetTooltip());
            //this.Product.Object.GetInfo().GetTooltip(this.Product.Object, panel);
            //tooltip.Controls.Add(panel);

            PanelLabeled panel = new PanelLabeled("Product") { Location = tooltip.Controls.BottomLeft, AutoSize = true };
            Slot icon = new Slot() { Location = panel.Controls.BottomLeft, Tag = this.Product };
            GroupBox box = new GroupBox() { Location = icon.TopRight };
            this.Product.Object.GetInfo().OnTooltipCreated(this.Product.Object, box);
            panel.Controls.Add(icon, box);
            tooltip.Controls.Add(panel);

            PanelLabeled panelMats = new UI.PanelLabeled("Materials") { Location = tooltip.Controls.BottomLeft, AutoSize = true };
            Label lblMats = new Label("Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            panelMats.Controls.Add(lblMats);
            foreach (var req in this.Items.Values)
            {
                SlotWithText slotReq = new SlotWithText(panelMats.Controls.BottomLeft) { Tag = GameObject.Objects[req.ObjectID].ToSlotLink() };
                slotReq.Slot.CornerTextFunc = o => req.AmountCurrent.ToString() + "/" + req.AmountRequired.ToString();
                panelMats.Controls.Add(slotReq);
            }
            tooltip.Controls.Add(panelMats);
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            Product.Write(writer);
            writer.Write(this.Materials.Count);
            foreach(var mat in this.Materials)
            {
                writer.Write((int)mat.Key);
                writer.Write(mat.Value);
            }
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Product.Read(reader);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.Materials[(GameObject.Types)reader.ReadInt32()] = reader.ReadInt32();
            }
        }
        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Int, "ProductID", (int)this.Product.Object.IDType));
            var mats = new SaveTag(SaveTag.Types.Compound, "Materials");
            foreach(var material in this.Materials)
            {
                var matTag = new SaveTag(SaveTag.Types.Compound);
                matTag.Add(new SaveTag(SaveTag.Types.Int, "MaterialID", (int)material.Key));
                matTag.Add(new SaveTag(SaveTag.Types.Int, "Amount", material.Value));
                mats.Add(matTag);
            }
            save.Add(mats);
            return save;
        }
        internal override void Load(SaveTag save)
        {
            this.Product = GameObject.Create((GameObject.Types)(int)save["ProductID"].Value).ToSlotLink();
            foreach (var mat in save["Materials"].Value as List<SaveTag>)
            {
                if (mat.Value == null)
                    continue;
                this.Materials[(GameObject.Types)(int)mat["MaterialID"].Value] = (int)mat["Amount"].Value;
            }
        }
    }
}
