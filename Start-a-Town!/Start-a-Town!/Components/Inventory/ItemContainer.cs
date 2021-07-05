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

        public bool Check(int objectID, int amount)
        {
            return this.Check((GameObject.Types)objectID, amount);
        }
        public bool Check(GameObject.Types type, int amount)
        {
            int currentAmount = 0;
            this
                .FindAll(slot => slot.HasValue)
                .FindAll(slot => slot.Object.IDType == type)
                .ForEach(slot => currentAmount += slot.StackSize);
            if (currentAmount < amount)
            {
                return false;
            }
            return true;
        }
        public void Remove(int objID, int amount)
        {
            this.Remove((GameObject.Types)objID, amount);
        }
        public void Remove(GameObject.Types type, int amount)
        {
            int remaining = amount;
            foreach (var slot in this
                .FindAll(slot => slot.HasValue)
                .FindAll(slot => slot.Object.IDType == type))
            {
                int diff = remaining - slot.StackSize;
                slot.StackSize -= Math.Min(slot.StackSize, remaining);
                remaining -= diff;
                if (remaining < 0)
                    break;
            }
        }

        public void Consume(int objID, int amount)
        {
            int remaining = amount;
            foreach (var slot in this
                .FindAll(slot => slot.HasValue)
                .FindAll(slot => (int)slot.Object.IDType == objID))
            {
                GameObject obj = slot.Object;
                int diff = remaining - slot.StackSize;
                slot.StackSize -= Math.Min(slot.StackSize, remaining);
                if (slot.StackSize == 0)
                    obj.Dispose();
                remaining -= diff;
            }
        }

        public bool InsertObject(IObjectProvider net, GameObject obj)
        {
            if ((from slot in this
                 where slot.Object == obj
                 select slot).FirstOrDefault() != null)
                return false;

            // TODO: might want to refactor there 2
            GameObjectSlot firstStack =
                (from slot in this
                 where slot.HasValue
                 where slot.Object.IDType == obj.IDType
                 where slot.StackSize < slot.StackMax
                 select slot)
                 .FirstOrDefault();
            if (firstStack is not null)
            {
                firstStack.StackSize += obj.StackSize;
                // merge objects to existins object in slot, despawn and dispose old one
                net.Despawn(obj);
                net.DisposeObject(obj);
                return true;
            }

            GameObjectSlot firstEmpty =
                (from slot in this
                 where !slot.HasValue
                 select slot)
                 .FirstOrDefault();
            if (firstEmpty is null)
                return false;

            firstEmpty.Object = obj;
            net.Despawn(obj);
            return true;
        }
        public bool InsertObject(IObjectProvider net, GameObjectSlot objSlot)
        {
            if (InsertObject(net, objSlot.Object))
            {
                objSlot.Clear();
                // dispose if stacksize == 0?
                return true;
            }
            return false;
        }
        public bool InsertObject(GameObjectSlot objSlot)
        {
            if (objSlot.Object == null)
                return false;

            if (InsertObject(objSlot.Object.Net, objSlot.Object))
            {
                objSlot.Clear();
                return true;
            }
            return false;
        }
    }                        
}
