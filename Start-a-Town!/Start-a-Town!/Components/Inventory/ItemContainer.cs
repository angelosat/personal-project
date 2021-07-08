using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace Start_a_Town_.Components
{
    public class ItemContainer : GameObjectSlotCollection
    {
        public Func<GameObject, bool> Filter = o => true;
        GameObject _Parent;
        public GameObject Parent
        {
            get { return this._Parent; }
            set
            {
                this._Parent = value;
                foreach (var slot in this)
                    slot.Parent = value;
            }
        }
        public List<ItemContainer> Children { get; set; }
        public byte ID;
        new public byte Capacity { get; set; }
        public ItemContainer(GameObject parent, byte size = 0) : this(parent, size, o => true) { }
        public ItemContainer(GameObject parent, byte size, Func<GameObject, bool> filter)
        {
            this.Parent = parent;
            this.Children = new List<ItemContainer>();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
            {
                var id = parent.ChildrenSequence;
                var sl = new GameObjectSlot(id) { Container = this };
                Add(sl);
            }
            this.Filter = filter;
        }
        public ItemContainer(byte size = 0)
        {
            this.Parent = null;
            this.Children = new List<ItemContainer>();
            this.Capacity = size;
        }
        private void Initialize(byte size)
        {
            this.Clear();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(n) { Container = this });
        }
        public ItemContainer(byte size, Func<byte> indexSequence) 
        {
            this.Children = new List<ItemContainer>();
            this.Clear();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(indexSequence()) { Container = this });
        }
        public ItemContainer(GameObject parent, byte size, Func<byte> indexSequence)
        {
             this.Parent = parent;
            this.Children = new List<ItemContainer>();
            this.Clear();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(indexSequence()) { Container = this });
        }
        public ItemContainer(BinaryReader reader)
        {
            this.ID = reader.ReadByte();
            this.Capacity = (byte)reader.ReadInt32();
            this.Children = new List<ItemContainer>();
            int occupiedslotCount = reader.ReadInt32();
            for (byte n = 0; n < this.Capacity; n++)
                Add(new GameObjectSlot(n) { Container = this });
            for (int i = 0; i < occupiedslotCount; i++)
            {
                int slotID = reader.ReadInt32();
                var slot = this[slotID];
                slot.Read(reader);
                slot.Container = this;
            }
        }
        
        public override string ToString()
        {
            string text = "";
            for (int i = 0; i < this.Count; i++)
                text += "[" + i + "]: " + this[i].ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.ID);
            writer.Write(this.Count);
            var slots = from slot in this where slot.HasValue select slot;
            writer.Write(slots.Count());
            for (int i = 0; i < this.Count; i++)
            {
                var slot = this[i];
                if (!slot.HasValue)
                    continue;
                writer.Write(i);
                slot.Write(writer);
            }
        }
        public void Read(BinaryReader reader)
        {
            this.Clear();
            this.ID = reader.ReadByte();
            int size = reader.ReadInt32();
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(n) { Container = this });
            int occupiedslotCount = reader.ReadInt32();
            for (int i = 0; i < occupiedslotCount; i++)
            {
                int slotID = reader.ReadInt32();
                GameObjectSlot slot = this[slotID];
                slot.Container = this;
                slot.Read(reader);
            }
        }
        static public ItemContainer Create(BinaryReader reader) { return Create(null, reader); }
        static public ItemContainer Create(GameObject parent, BinaryReader reader)
        {
            var id = reader.ReadByte();
            ItemContainer cont = new ItemContainer(parent, (byte)reader.ReadInt32()) { ID = id };
            int occupiedslotCount = reader.ReadInt32();
            for (int i = 0; i < occupiedslotCount; i++)
            {
                int slotID = reader.ReadInt32();
                var slot = cont[slotID];
                slot.Read(reader);
                slot.Container = cont;
            }
            return cont;
        }

        public List<GameObjectSlot> Empty()
        {
            var contents = new List<GameObjectSlot>();
            foreach (var slot in
                from slot in this
                where slot.HasValue
                select slot)
            {
                contents.Add(slot.Clone());
                slot.Clear();
            }
            return contents;
        }
        public List<GameObjectSlot> Clear(byte newCapacity)
        {
            var contents = new List<GameObjectSlot>();
            foreach (var slot in
                from slot in this
                where slot.HasValue
                select slot)
            {
                contents.Add(slot.Clone());
                slot.Clear();
            }

            Initialize(newCapacity);

            return contents;
        }
        public List<SaveTag> Save()
        {
            List<SaveTag> containerTag = new List<SaveTag>();
            containerTag.Add(new SaveTag(SaveTag.Types.Byte, "ID", this.ID));
            containerTag.Add(new SaveTag(SaveTag.Types.Byte, "Capacity", (byte)this.Capacity));
            SaveTag items = new SaveTag(SaveTag.Types.Compound, "Items");
            for (int i = 0; i < this.Count; i++)
            {
                GameObjectSlot objSlot = this[i];
                if (objSlot.Object != null)
                    items.Add(new SaveTag(SaveTag.Types.Compound, i.ToString(), objSlot.Save()));
            }
            containerTag.Add(items);
            return containerTag;
        }

        static public ItemContainer Create(GameObject parent, SaveTag containerTag)
        {
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;
        
            byte capacity = (byte)shit["Capacity"].Value;
            ItemContainer container = new ItemContainer(parent, capacity);
            containerTag.TryGetTagValue<byte>("ID", out container.ID);
            Dictionary<string, SaveTag> itemList = shit["Items"].Value as Dictionary<string, SaveTag>;

            foreach (SaveTag itemTag in itemList.Values)
            {
                if (itemTag.Value == null)
                    continue;
                int index = byte.Parse(itemTag.Name);
                container[index].Load(itemTag);
            }
            return container;
        }
        static public ItemContainer Create(SaveTag containerTag)
        {
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;
            byte capacity = (byte)shit["Capacity"].Value;
            ItemContainer container = new ItemContainer(capacity);
            containerTag.TryGetTagValue<byte>("ID", out container.ID);
            List<SaveTag> itemList = shit["Items"].Value as List<SaveTag>;
            foreach (SaveTag itemTag in itemList)
            {
                if (itemTag.Value == null)
                    continue;
                int index = byte.Parse(itemTag.Name);
                GameObjectSlot slot = GameObjectSlot.Create(itemTag);
                slot.Container = container;
                slot.ID = (byte)index;
                container[index] = slot;
            }
            return container;
        }
        public ItemContainer Load(SaveTag containerTag)
        {
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;
            containerTag.TryGetTagValue<byte>("ID", out this.ID);
            byte capacity = (byte)shit["Capacity"].Value;
            this.Initialize(capacity);
            List<SaveTag> itemList = shit["Items"].Value as List<SaveTag>;
            foreach (SaveTag itemTag in itemList)
            {
                if (itemTag.Value == null)
                    continue;
                int index = byte.Parse(itemTag.Name);
                GameObjectSlot slot = GameObjectSlot.Create(itemTag);
                slot.Container = this;
                slot.ID = (byte)index;
                this[index] = slot;
            }
            return this;
        }
    }                        
}
