using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class GearComponent : EntityComponent
    {
        readonly static public string Name = "Gear";

        public override string ComponentName
        {
            get { return Name; }
        }

        public override void OnObjectLoaded(GameObject parent)
        {
            base.OnObjectLoaded(parent);
                        ResetBones(parent);
        }
        public override void OnObjectCreated(GameObject parent)
        {
            base.OnObjectCreated(parent);
            ResetBones(parent);
        }
        public override void MakeChildOf(GameObject parent)
        {
            parent.RegisterContainer(this.Equipment);
        }
        private void ResetBones(GameObject parent)
        {
            foreach (var gear in this.Equipment.Slots)
            {
                if (gear.Object == null)
                    continue;
                Attach(parent.Body, gear.Object.Body);
            }
        }

        public override void OnSpawn(IObjectProvider net, GameObject parent)
        {
            base.OnSpawn(net, parent);
            ResetBones(parent);

        }

        public Container Equipment = new Container() { Name = "Equipment" };
        public float ArmorTotal;
        public GearComponent()
        {
        }
        public GearComponent(ItemDef def)
        {
            foreach (var slot in def.ActorProperties.GearSlots)
                this.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = this.Equipment, Name = slot.Name });
        }
        public GearComponent(params GearType[] types)
        {
            foreach (var slot in types)
                this.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = this.Equipment, Name = slot.Name });
        }
        public override object Clone()
        {
            var types = from gear in this.Equipment.Slots select GearType.Dictionary[(GearType.Types)gear.ID];
            GearComponent comp = new GearComponent(types.ToArray());

            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }
            return comp;
        }
        
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.Equipment);
        }

        static public bool GetSlot(GameObject entity, GearType gearSlot, out GameObjectSlot slot)
        {
            slot = GearComponent.GetSlot(entity, gearSlot);
            return slot.HasValue;
        }
        static public void TryGetHeldObject(GameObject entity, Action<GameObjectSlot> action)
        {
            entity.TryGetComponent<HaulComponent>(i =>
            {
                GameObjectSlot slot = i.Holding;
                if (slot.HasValue)
                    action(slot);
           
            });
        }
        static public bool TryGetObject(GameObject entity, GearType gearSlot, out GameObject obj)
        {
            obj = GearComponent.GetSlot(entity, gearSlot).Object;
            return obj != null;
        }
        static public GameObjectSlot GetHeldObject(GameObject entity)
        {
            if (!entity.TryGetComponent<HaulComponent>(out var inv))
                return GameObjectSlot.Empty;
            return inv.Holding;
        }

        public override string ToString()
        {
            string text = "";
            foreach (var slot in this.Equipment.Slots)
                text += slot.ID + ": " + (slot.HasValue ? slot.Object.Name : "<empty>") + "\n";

            return text.TrimEnd('\n');
        }

        public override void Write(BinaryWriter writer)
        {
            this.Equipment.Write(writer);
        }
        public override void Read(BinaryReader reader)
        {
            this.Equipment.Read(reader);
        }

        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Compound, "Equipment", this.Equipment.Save()));
            return save;
        }
        internal override void Load(SaveTag compTag)
        {
            compTag.TryGetTag("Equipment", tag => this.Equipment.Load(tag));
        }

        static public GameObjectSlot GetSlot(GameObject actor, GearType type)
        {
            var gearComp = actor.GetComponent<GearComponent>();
            var slot = gearComp.Equipment.GetSlot((int)type.ID);
            return slot;
        }
        internal static GameObjectSlot GetSlot(GameObject actor, GameObject item)
        {
            var gearComp = actor.GetComponent<GearComponent>();
            var slot = gearComp.Equipment.Slots.FirstOrDefault(s => s.Object == item);
            return slot;
        }
        static public bool Equip(GameObject a, GameObjectSlot t)
        {
            if (t.Object == null)
                return false;

            GameObjectSlot objSlot =
                t.Object.IsSpawned ?
                t.Object.ToSlotLink() :
                (from slot in a.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == t.Object);

            if (objSlot == null)
                return false;
            if (objSlot.Object == null)
                return false;
            var geartype = (int)t.Object.GetComponent<EquipComponent>().Type.ID;

            GameObjectSlot gearSlot = a.GetComponent<GearComponent>().Equipment.Slots[geartype];

            // despawn item's entity from world (if it's spawned in the world)
            t.Object.Net.Despawn(objSlot.Object);

            // attempt to store current equipped item in inventory, otherwise drop it if inventory is full

            // equip new item
            gearSlot.Swap(objSlot);
         
            return true;
        }
        static public bool EquipToggle(Actor actor, Entity item)
        {
            if (actor.IsEquipping(item))
            {
                //unequip
                actor.InsertToInventory(item);
                actor.GetComponent<GearComponent>().RefreshStats();
                actor.Net.EventOccured(Message.Types.ActorGearUpdated, actor, null, item);
                return true;
            }
            var slotType = item.Def.GearType;
            var gearSlot = GetSlot(actor, slotType);
            var previousItem = gearSlot.Object as Entity;
            if (item == previousItem)
                return false;

            item.Despawn(); // in case the item is equipped from the world instead of from the inventory
                            // DESPAWN BEFORE EQUIPPING because then the item's global become's the actor's global and the item is despawned from the wrong chunk!

            gearSlot.SetObject(item);
            actor.GetComponent<GearComponent>().RefreshStats();
            if (previousItem != null)
                actor.InsertToInventory(previousItem);
            actor.Net.EventOccured(Message.Types.ActorGearUpdated, actor, item, previousItem);
            return true;
        }
        static void Attach(Bone body, Bone toattach)
        {
            return;
            if (toattach == null)
                return;
            var found = body.Descendants(toattach.Def).FirstOrDefault();
            if (found != null)
                found.SetBone(toattach);
        }

        public void RefreshStats()
        {
            this.ArmorTotal = 0;
            foreach(var i in this.Equipment.Slots.Where(s=>s.HasValue).Select(s=>s.Object))
            {
                this.ArmorTotal += i.Def.ApparelProperties?.ArmorValue ?? 0;
            }
        }

        public class Props : ComponentProps
        {
            public override Type CompType => typeof(GearComponent);
            public GearType[] Slots;
            public Props(params GearType[] defs)
            {
                this.Slots = defs;
            }
        }

    }
}
