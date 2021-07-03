using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    partial class HaulComponent : EntityComponent
    {

        public override string ComponentName
        {
            get { return "Haul"; }
        }
        PersonalInventoryComponent Inventory;
        public GameObjectSlot GetSlot()
        {
            return this.Inventory.GetHauling();
        }
        public GameObject GetObject()
        {
            return this.Inventory.GetHauling().Object;
        }

        public Animation AnimationHaul = new(AnimationDef.Haul) { Weight = 0 };

        public override void OnObjectCreated(GameObject parent)
        {
            this.Inventory = parent.GetComponent<PersonalInventoryComponent>();
            if (this.Inventory == null || this.Inventory.Slots.Capacity == 0)
                throw new Exception("HaulComponent requires a parent entity with PersonalInventoryComponent and an inventory of at least size 1");
            parent.AddResourceModifier(new ResourceRateModifier(ResourceRateModifierDef.HaulingStaminaDrain));
            parent.AddStatModifier(new StatNewModifier(StatNewModifierDef.WalkSpeedHaulingWeight));
            parent.AddAnimation(this.AnimationHaul);
        }
        
        public bool Carry(GameObject parent, GameObjectSlot objSlot)
        {
            return this.Carry(parent.Net, parent, objSlot);
        }
        public bool Carry(IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot == null)
                return true;
            if (!objSlot.HasValue)
                return true;

            if (!CheckWeight(parent, objSlot.Object))
                return true;

            GameObjectSlot hauling = this.GetSlot();

            var existing = hauling.Object;
            if (existing != null)
                if (existing.IDType == objSlot.Object.IDType)
                {
                    existing.StackSize++;
                    objSlot.Object.Despawn();
                    net.DisposeObject(objSlot.Object);
                    return true;
                }

            // else
            // drop currently hauled object and pick up new one
            this.Throw(Vector3.Zero, parent);

            net.Despawn(objSlot.Object);
            hauling.Object = objSlot.Object;
            return true;
        }

        static public bool CheckWeight(GameObject a, GameObject t)
        {
            float w = t.Physics.Weight;
            float maxW = StatsComponentNew.GetStatValueOrDefault(a, Stat.Types.MaxWeight, 0);
            return maxW >= w;
        }


        bool Throw(IObjectProvider net, Vector3 velocity, GameObject parent, bool all)
        {
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
           
            var slot = this.GetSlot();
            GameObjectSlot hauling = slot;// this.Slot;
            if (hauling.Object == null)
                return false;

            GameObject newobj = all ? hauling.Object : hauling.Take();


            newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            net.Spawn(newobj);

            if (all)
                hauling.Clear();
            return true;
        }
        public bool Throw(Vector3 velocity, GameObject parent, bool all = false)
        {
            return this.Throw(parent.Net, velocity, parent, all);
        }
        public bool Throw(GameObject parent, Vector3 direction, bool all = false)
        {
            Vector3 velocity = direction * 0.1f + parent.Velocity;
            return this.Throw(parent.Net, velocity, parent, all);
        }
        static public bool ThrowHauled(GameObject parent, Vector3 direction, bool all = false)
        {
            var haulComp = parent.GetComponent<HaulComponent>();
            return haulComp.Throw(parent, direction, all);
        }
        static public GameObjectSlot GetHolding(GameObject parent)
        {
            return parent.GetComponent<HaulComponent>().Holding;
        }
        public GameObjectSlot Holding
        {
            get
            {
                GameObjectSlot slot = this.GetSlot();//.Slot;
                return slot;
            }
        }

        public override object Clone()
        {
            return new HaulComponent();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            this.AnimationHaul.Write(w);

        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.AnimationHaul.Read(r);
        }
        
        internal override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.AnimationHaul.Save("AnimationHaul"));
        }
        internal override void Load(SaveTag save)
        {
            save.TryGetTag("AnimationHaul", this.AnimationHaul.Load);
        }

        internal override void HandleRemoteCall(GameObject gameObject, Message.Types type, System.IO.BinaryReader r)
        {
            switch(type)
            {
                case Message.Types.Carry:
                    var objid = r.ReadInt32();
                    var obj = gameObject.Net.GetNetworkObject(objid);
                    this.Carry(gameObject, obj.ToSlotLink());
                    break;

                default:
                    break;
            }
        }
        
        static public bool Carry(GameObject parent, GameObject obj)
        {
            return parent.GetComponent<HaulComponent>().Carry(parent, obj.ToSlotLink());
        }
    }
}
