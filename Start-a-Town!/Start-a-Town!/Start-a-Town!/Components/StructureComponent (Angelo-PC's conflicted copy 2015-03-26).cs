using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Scripts;
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

        public Reaction.Product.ProductMaterialPair Product { get { return (Reaction.Product.ProductMaterialPair)this["Product"]; } set { this["Product"] = value; } }
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
        }

        public StructureComponent()
        {
            this.Timer = Engine.TargetFps;
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
                if (mat.ID == this.Product.Req.ObjectID)
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
                if (mat.ID == this.Product.Req.ObjectID)
                    this.Product.Req.Amount += mat.StackSize;
            }
            return this.Product.Req.Amount >= this.Product.Req.Max;
        }

        public void Finish(GameObject parent, GameObject actor)
        {
            var mats = new Queue<GameObject>(actor.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material).Where(o=>o.ID == this.Product.Req.ObjectID));
            foreach (var mat in mats)
                if (mat.ID == this.Product.Req.ObjectID)
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

        internal override void GetAvailableTasks(GameObject parent, List<ScriptTask> list)
        {
            list.Add(new ScriptTask("Building", 2, actor => this.Finish(parent, actor), new TaskConditionCollection(
                new RangeCondition(() => parent.Global, Interaction.DefaultRange),
                new ScriptTaskCondition("Materials", actor => this.DetectMaterials(parent, actor), Message.Types.InsufficientMaterials)
                )));
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Product.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Product = new Reaction.Product.ProductMaterialPair(reader);
        }
        internal override List<SaveTag> Save()
        {
            return this.Product.Save();
        }
        internal override void Load(SaveTag save)
        {
            this.Product = new Reaction.Product.ProductMaterialPair(save);
        }
    }
}
