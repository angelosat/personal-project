using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public sealed class GearType : Def
    {
        public enum Types { 
            None, 
            Mainhand, Offhand, Head, Chest, Hands, Legs, Feet }
        public readonly Types ID;
        //public readonly string Name;

        //public static readonly GearType Hauling = new GearType(Types.Hauling, "Hauling");
        public static readonly GearType Mainhand = new GearType(Types.Mainhand, "Mainhand");
        public static readonly GearType Offhand = new GearType(Types.Offhand, "Offhand");
        public static readonly GearType Head = new GearType(Types.Head, "Head");
        public static readonly GearType Chest = new GearType(Types.Chest, "Chest");
        public static readonly GearType Hands = new GearType(Types.Hands, "Hands");
        public static readonly GearType Legs = new GearType(Types.Legs, "Legs");
        public static readonly GearType Feet = new GearType(Types.Feet, "Feet");

        public static readonly Dictionary<Types, GearType> Dictionary = new Dictionary<Types, GearType>() {
        //{ Types.Hauling, GearType.Hauling },
        { Types.Mainhand, GearType.Mainhand },
        { Types.Offhand, GearType.Offhand },
        { Types.Head, GearType.Head },
        { Types.Chest, GearType.Chest },
        { Types.Hands, GearType.Hands },
        { Types.Legs, GearType.Legs },
        { Types.Feet, GearType.Feet }
        };

        GearType(Types id, string name) : base(name)
        {
            this.ID = id;
            //this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

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

        //ItemContainer EquipmentSlots { get { return (ItemContainer)this["EquipmentSlots"]; } set { this["EquipmentSlots"] = value; } }
        //public Dictionary<GearType, GameObjectSlot> EquipmentSlots { get { return (Dictionary<GearType, GameObjectSlot>)this["EquipmentSlots"]; } set { this["EquipmentSlots"] = value; } }
        public Container Equipment = new Container() { Name = "Equipment" };
        public float ArmorTotal;
        //{ get { return (Container)this["Equipment"]; } set {
        //    this["Equipment"] = value;
        //} }
        public GearComponent()
        {
            //this.EquipmentSlots = new Dictionary<GearType, GameObjectSlot>();// null;
            //this.Equipment = new Container() { Name = "Equipment" };
        }
        public GearComponent(ItemDef def)
        {
            foreach (var slot in def.ActorProperties.GearSlots)// (parent.Def as ActorDef).GearSlots)
                this.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = this.Equipment, Name = slot.Name });
        }
        public GearComponent(params GearType[] types)
        {
            //this.Equipment = new Container() { Name = "Equipment" };
            foreach (var slot in types)
                this.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = this.Equipment, Name = slot.Name });
            //this.EquipmentSlots = new Dictionary<GearType, GameObjectSlot>();// new ItemContainer();
            //for (int i = 0; i < types.Count(); i++)
            //{
            //    this.EquipmentSlots.Add(types[i], new GameObjectSlot((byte)i));
            //}
        }
        public override object Clone()
        {
            //return new GearComponent();
            var types = from gear in this.Equipment.Slots select GearType.Dictionary[(GearType.Types)gear.ID];
            GearComponent comp = new GearComponent(types.ToArray());
            //foreach (var gear in this.EquipmentSlots)
            //    comp.EquipmentSlots[gear.Key] = GameObjectSlot.Empty;// gear.Value.Clone();

            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }

            return comp;
        }
        

        public override void GetChildren(List<GameObjectSlot> list)
        {
            //list.AddRange(this.EquipmentSlots.Values);
        }
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.Equipment);
        }

        //static public GameObjectSlot GetObject(GameObject entity, GearType gearSlot)
        //{
        //    GameObjectSlot slot = null;
        //    entity.TryGetComponent<GearComponent>(i => slot = i.EquipmentSlots[gearSlot]);
        //    return slot;
        //}
        static public bool GetSlot(GameObject entity, GearType gearSlot, out GameObjectSlot slot)
        {
            //slot = entity.GetComponent<GearComponent>().EquipmentSlots[gearSlot]; 
            slot = GearComponent.GetSlot(entity, gearSlot);
            return slot.HasValue;
        }
        static public void TryGetHeldObject(GameObject entity, Action<GameObjectSlot> action)
        {
            //entity.TryGetComponent<GearComponent>(i =>
            entity.TryGetComponent<HaulComponent>(i =>
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
            //GameObject found = null;
            //entity.TryGetComponent<GearComponent>(i =>
            //{
            //    found = i.EquipmentSlots[gearSlot].Object;
            //});
            //obj = found;
            obj = GearComponent.GetSlot(entity, gearSlot).Object;
            return obj != null;
        }
        static public GameObjectSlot GetHeldObject(GameObject entity)
        {
            //GearComponent inv;
            //if (!entity.TryGetComponent<GearComponent>(out inv))
            HaulComponent inv;
            if (!entity.TryGetComponent<HaulComponent>(out inv))
                return GameObjectSlot.Empty;
            return inv.Holding;
        }


        

        

        public override string ToString()
        {
            string text = "";
            foreach (var slot in this.Equipment.Slots)
                //text += slot.Key.Name + ": " + (slot.Value.HasValue ? slot.Value.Object.Name : "<empty>") + "\n";
                text += (Types)slot.ID + ": " + (slot.HasValue ? slot.Object.Name : "<empty>") + "\n";

            return text.TrimEnd('\n');
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Equipment.Write(writer);
            //var occupiedSlots = (from slot in this.EquipmentSlots
            //                     where slot.Value.HasValue
            //                     select slot).ToList();
            //writer.Write(occupiedSlots.Count);
            //foreach (var slot in occupiedSlots)
            //{
            //    writer.Write((int)slot.Key.ID);
            //    //slot.Value.Object.Write(writer);
            //    slot.Value.Write(writer);
            //}
      
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Equipment.Read(reader);
            //int count = reader.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    var id = (GearType.Types)reader.ReadInt32();
            //    GearType type = GearType.Dictionary[id];
            //    this.EquipmentSlots[type].Read(reader);
            //}
        
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
            //var occupiedSlots = (from slot in this.EquipmentSlots
            //                     where slot.Value.HasValue
            //                     select slot).ToList();
            //SaveTag slotsTag = new SaveTag(SaveTag.Types.List, "Slots", SaveTag.Types.Compound);
            //foreach (var slot in occupiedSlots)
            //{
            //    SaveTag tag = new SaveTag(SaveTag.Types.Compound);
            //    tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)slot.Key.ID));
            //    tag.Add(new SaveTag(SaveTag.Types.Compound, "Object", slot.Value.Save()));
            //    slotsTag.Add(tag);
            //}
            //save.Add(slotsTag);
            save.Add(new SaveTag(SaveTag.Types.Compound, "Equipment", this.Equipment.Save()));
            return save;
        }
        internal override void Load(SaveTag compTag)
        {
            //foreach (var save in (compTag["Slots"].Value) as List<SaveTag>)
            //{
            //    if (save.Value == null)
            //        continue;
            //    GearType.Types type = (GearType.Types)(int)save["Type"].Value;
            //    this.EquipmentSlots[GearType.Dictionary[type]].Load(save["Object"] as SaveTag);
            //}
            compTag.TryGetTag("Equipment", tag => this.Equipment.Load(tag));
        }

        static public GameObjectSlot GetSlot(GameObject actor, GearType type)
        {
            //var slot = actor.GetComponent<GearComponent>().EquipmentSlots[type];
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
            //PersonalInventoryComponent.Insert(a, gearSlot);
            //PersonalInventoryComponent.Receive(a, gearSlot);

            // equip new item
            gearSlot.Swap(objSlot);

         
            //var bone = gearSlot.Object.Body;
            //Attach(a.Body, bone);

         
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
