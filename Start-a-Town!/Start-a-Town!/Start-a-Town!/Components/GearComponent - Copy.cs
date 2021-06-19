using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public sealed class GearType
    {
        public enum Types { Hauling, Mainhand, Offhand, Head, Chest, Hands, Legs, Feet }
        public readonly Types ID;
        public readonly string Name;

        public static readonly GearType Hauling = new GearType(Types.Hauling, "Hauling");
        public static readonly GearType Mainhand = new GearType(Types.Mainhand, "Mainhand");
        public static readonly GearType Offhand = new GearType(Types.Offhand, "Offhand");
        public static readonly GearType Head = new GearType(Types.Head, "Head");
        public static readonly GearType Chest = new GearType(Types.Chest, "Chest");
        public static readonly GearType Hands = new GearType(Types.Hands, "Hands");
        public static readonly GearType Legs = new GearType(Types.Legs, "Legs");
        public static readonly GearType Feet = new GearType(Types.Feet, "Feet");

        public static readonly Dictionary<Types, GearType> Dictionary = new Dictionary<Types, GearType>() {
        { Types.Hauling, GearType.Hauling },
        { Types.Mainhand, GearType.Mainhand },
        { Types.Offhand, GearType.Offhand },
        { Types.Head, GearType.Head },
        { Types.Chest, GearType.Chest },
        { Types.Hands, GearType.Hands },
        { Types.Legs, GearType.Legs },
        { Types.Feet, GearType.Feet }
        };

        GearType(Types id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class GearComponent : Component
    {
        public override string ComponentName
        {
            get { return "Gear"; }
        }

        //ItemContainer EquipmentSlots { get { return (ItemContainer)this["EquipmentSlots"]; } set { this["EquipmentSlots"] = value; } }
        public Dictionary<GearType, GameObjectSlot> EquipmentSlots { get { return (Dictionary<GearType, GameObjectSlot>)this["EquipmentSlots"]; } set { this["EquipmentSlots"] = value; } }

        public override void MakeChildOf(GameObject parent)
        {
            foreach (var g in this.EquipmentSlots.Values)
                g.Parent = parent;
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.EquipmentSlots.Values);
        }

        public GearComponent Initialize(GameObject parent, params GearType[] slots)
        {
            this.EquipmentSlots = new Dictionary<GearType, GameObjectSlot>();// new ItemContainer();
            for (int i = 0; i < slots.Count(); i++)
            {
                this.EquipmentSlots.Add(slots[i], new GameObjectSlot() { Parent = parent });// slots[i+1] as GameObjectSlot);
            }
            return this;
        }

        public GearComponent()
        {
            this.EquipmentSlots = new Dictionary<GearType, GameObjectSlot>();// null;
        }

        static public GameObjectSlot GetObject(GameObject entity, GearType gearSlot)
        {
            GameObjectSlot slot = null;
            entity.TryGetComponent<GearComponent>(i => slot = i.EquipmentSlots[gearSlot]);
            return slot;
        }
        static public bool GetSlot(GameObject entity, GearType gearSlot, out GameObjectSlot slot)
        {
            slot = entity.GetComponent<GearComponent>().EquipmentSlots[gearSlot]; 
            return slot.HasValue;
        }
        static public void TryGetHeldObject(GameObject entity, Action<GameObjectSlot> action)
        {
            entity.TryGetComponent<GearComponent>(i =>
            {
                //if (i.Holding.HasValue)
                //    action(i.Holding);

                GameObjectSlot slot = i.Holding;
                if (slot.HasValue)
                    action(slot);

                //GameObjectSlot slot = i.EquipmentSlots[GearType.Hauling];
                //if (!slot.HasValue)
                //    slot = i.EquipmentSlots[GearType.Mainhand];
                //if (slot.HasValue)
                //    action(slot);                
            });
        }
        static public bool TryGetObject(GameObject entity, GearType gearSlot, out GameObject obj)
        {
            GameObject found = null;
            entity.TryGetComponent<GearComponent>(i =>
            {
                found = i.EquipmentSlots[gearSlot].Object;
            });
            obj = found;
            return obj != null;
        }
        static public GameObjectSlot GetHeldObject(GameObject entity)
        {
            GearComponent inv;
            if (!entity.TryGetComponent<GearComponent>(out inv))
                return GameObjectSlot.Empty;
            return inv.Holding;
        }
        public bool Carry(GameObject parent, GameObjectSlot objSlot)
        {
            return this.Carry(parent.Net, parent, objSlot);
        }
        public bool Carry(Net.IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot == null)
                return true;
            if (!objSlot.HasValue)
                return true;

            //if (this.Holding.HasValue)
            //    net.PostLocalEvent(parent, Message.Types.Receive, this.Holding.Object);
            GameObjectSlot hauling = this.EquipmentSlots[GearType.Hauling];

            // if currently hauling object of same type, increase held stacksize and dispose other object
            var existing = hauling.Object;
            if(existing != null)
                if(existing.ID == objSlot.Object.ID)
                {
                    existing.StackSize++;
                    objSlot.Object.Despawn();
                    net.DisposeObject(objSlot.Object);
                    return true;
                }

            // else
            // drop currently hauled object and pick up new one
            //hauling.Clear();
            this.Throw(Vector3.Zero, parent);

            net.Despawn(objSlot.Object);
            hauling.Object = objSlot.Object;
            //hauling.Swap(objSlot);
            return true;
        }

        bool Throw(Net.IObjectProvider net, Vector3 velocity, GameObject parent)
        {
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
            //if (!this.Holding.HasValue)
            //    return false;
            //GameObject newobj = this.Holding.Take();

            GameObjectSlot hauling = this.EquipmentSlots[GearType.Hauling];
            if(hauling.Object == null)
                return false;
            GameObject newobj = hauling.Take();

            newobj.Global = parent.Global + new Vector3(0, 0, parent.GetPhysics().Height);
            newobj.Velocity = velocity;
            net.Spawn(newobj);
            return true;
        }
        public bool Throw(Vector3 velocity, GameObject parent)
        {
            return this.Throw(parent.Net, velocity, parent);
        }
        public bool Throw(GameObject parent, Vector3 direction)
        {
            Vector3 velocity = direction * 0.1f + parent.Velocity;
            return this.Throw(parent.Net, velocity, parent);
        }
        public GameObjectSlot Holding
        {
            get
            {
                GameObjectSlot slot = this.EquipmentSlots[GearType.Hauling];
                if (!slot.HasValue)
                    slot = this.EquipmentSlots[GearType.Mainhand];
                return slot;
                //if (this.EquipmentSlots[GearType.Hauling].HasValue)
                //    return this.EquipmentSlots[GearType.Hauling];
                //return this.EquipmentSlots[GearType.Mainhand];
            }
        }

        public override object Clone()
        {
            //return new GearComponent();
            GearComponent comp = new GearComponent();
            foreach (var gear in this.EquipmentSlots)
                comp.EquipmentSlots[gear.Key] = GameObjectSlot.Empty;// gear.Value.Clone();

            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }

            return comp;
        }

        public override string ToString()
        {
            string text = "";
            foreach (var slot in this.EquipmentSlots)
                text += slot.Key.Name + ": " + (slot.Value.HasValue ? slot.Value.Object.Name : "<empty>") + "\n";
            return text.TrimEnd('\n');
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            var occupiedSlots = (from slot in this.EquipmentSlots
                                 where slot.Value.HasValue
                                 select slot).ToList();
            writer.Write(occupiedSlots.Count);
            foreach (var slot in occupiedSlots)
            {
                writer.Write((int)slot.Key.ID);
                //slot.Value.Object.Write(writer);
                slot.Value.Write(writer);
            }
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var id = (GearType.Types)reader.ReadInt32();
                GearType type = GearType.Dictionary[id];
                this.EquipmentSlots[type].Read(reader);
            }
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
            var occupiedSlots = (from slot in this.EquipmentSlots
                                 where slot.Value.HasValue
                                 select slot).ToList();
            //save.Add(new SaveTag(SaveTag.Types.Byte, "Count", occupiedSlots.Count));
            SaveTag slotsTag = new SaveTag(SaveTag.Types.List, "Slots", SaveTag.Types.Compound);
            foreach (var slot in occupiedSlots)
            {
                SaveTag tag = new SaveTag(SaveTag.Types.Compound);
                tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)slot.Key.ID));
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Object", slot.Value.Save()));
                slotsTag.Add(tag);
            }
            save.Add(slotsTag);
            return save;
        }
        internal override void Load(SaveTag compTag)
        {
            //foreach (var save in compTag.Value as List<SaveTag>)
            //foreach (var save in (compTag.Value as Dictionary<string, SaveTag>).Values)
            foreach (var save in (compTag["Slots"].Value) as List<SaveTag>)
            {
                if (save.Value == null)
                    continue;
                GearType.Types type = (GearType.Types)(int)save["Type"].Value;
                this.EquipmentSlots[GearType.Dictionary[type]].Load(save["Object"] as SaveTag);
            }


            //foreach (var save in (compTag.Value as Dictionary<string, SaveTag>).Values)
            //{
            //    if (save.Value == null)
            //        continue;
            //    GearType.Types type = (GearType.Types)(int)save["Type"].Value;
            //    this.EquipmentSlots[GearType.Dictionary[type]].Load(save["Object"] as SaveTag);
            //}
        }

        static public GameObjectSlot GetSlot(GameObject actor, GearType type)
        {
            var slot = actor.GetComponent<GearComponent>().EquipmentSlots[type];
            return slot;
        }
    }
}
