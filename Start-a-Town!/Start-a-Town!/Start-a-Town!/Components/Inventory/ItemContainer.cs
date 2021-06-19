using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
namespace Start_a_Town_.Components
{
    public class ItemContainer : GameObjectSlotCollection//, IObjectSpace
    {
        //public Func<GameObject, bool> Filter { get; set; }
        //public byte ID;
        public Func<GameObject, bool> Filter = o => true;
        //public Func<GameObject, bool> Filter
        //{
        //    get { return this.Container.IsNull() ? this._Filter : this.Container.Filter; }
        //    set { this._Filter = value; }
        //}
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
        public byte ID;// { get; set; }
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
                //if (sl.ID > 11 && (int)parent.ID == 10000)
                //    "tistoputso".ToConsole();
            }
            //Initialize(size);
            this.Filter = filter;
        }
        public ItemContainer(byte size = 0)
         //: this(null, size)
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
        public ItemContainer(byte size, Func<byte> indexSequence) //GameObject parent, 
        // : base(size)
        {
           // this.Parent = parent;
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
        //public byte AddChild(ItemContainer itemcontainer)
        //{
        //    itemcontainer.ID = (byte)this.Children.Count;
        //    itemcontainer.Parent = this;
        //    this.Children.Add(itemcontainer);
        //    return itemcontainer.ID;
        //}

        public override string ToString()
        {
            string text = "";
            //foreach (GameObjectSlot slot in this)
            //    text += slot.ToString() + "\n";
            for (int i = 0; i < this.Count; i++)
                text += "[" + i + "]: " + this[i].ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            //TargetArgs.Write(writer, this.Parent);
            writer.Write(this.ID);
            writer.Write(this.Count);
            var slots = from slot in this where slot.HasValue select slot;
            writer.Write(slots.Count());
            //slots.ToList().ForEach(s => s.Write(writer));
            for (int i = 0; i < this.Count; i++)
            {
                var slot = this[i];
                if (!slot.HasValue)
                    continue;
                writer.Write(i);
                slot.Write(writer);
            }
        }
        public void Read(System.IO.BinaryReader reader)
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
                //this[slotID].Read(reader);
                GameObjectSlot slot = this[slotID];

                slot.Container = this;
                //   slot.ID = (byte)slotID; //redundant?
                slot.Read(reader);
            }
        }
        static public ItemContainer Create(System.IO.BinaryReader reader) { return Create(null, reader); }
        static public ItemContainer Create(GameObject parent, System.IO.BinaryReader reader)
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
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;// containerTag.ToDictionary();
        
            byte capacity = (byte)shit["Capacity"].Value;
            ItemContainer container = new ItemContainer(parent, capacity);
            containerTag.TryGetTagValue<byte>("ID", out container.ID);
            //List<SaveTag> itemList = shit["Items"].Value as List<SaveTag>;
            Dictionary<string, SaveTag> itemList = shit["Items"].Value as Dictionary<string, SaveTag>;
            //foreach (SaveTag itemTag in itemList)

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
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;//.ToDictionary();
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
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;//.ToDictionary();
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
                .FindAll(slot => slot.Object.ID == type)
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
                .FindAll(slot => slot.Object.ID == type))
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
                .FindAll(slot => (int)slot.Object.ID == objID))
            {
                GameObject obj = slot.Object;
                int diff = remaining - slot.StackSize;
                slot.StackSize -= Math.Min(slot.StackSize, remaining);
                if (slot.StackSize == 0)
                    //obj.Net.DisposeObject(obj);
                    obj.Dispose();
                remaining -= diff;
            }
        }

        public void Operation(Net.IObjectProvider net, ArrangeInventoryEventArgs args)
        {
            GameObjectSlot targetSlot = this[args.TargetSlotID];
            int amount = args.Amount;
            GameObject source = args.SourceObject.Object;

            GameObject obj = args.Object.Object;

            // TODO: HANDLE SLOT OVERFLOW

            if (!obj.IsNull())
                if (targetSlot.HasValue)
                    if (obj.ID == targetSlot.Object.ID)
                    {
                        // combine stacks and dispose added object
                        targetSlot.StackSize += amount;
                        net.DisposeObject(obj);
                        return;
                    }

            targetSlot.Set(obj, amount);
            return;

            if (targetSlot.HasValue)
            {
                // if objects have the same id, DESTROY source object from the network if source stacksize == 0
                if (obj.ID == targetSlot.Object.ID)
                {
                    int distFromMax = Math.Max(0, targetSlot.StackSize + amount - targetSlot.StackMax);
                    int amountTransferred = amount - distFromMax;
                    targetSlot.StackSize += amountTransferred;
                    net.DisposeObject(args.Object.Object);
                    return;
                }
            }
            else
            {
                // if object and source have the same network id, request new instantiated object from server
                if (source != null)
                    if (obj.Network.ID == source.Network.ID)
                    {
                        //   net.InstantiateInContainer(obj, parent, targetSlot.Parent.ID, targetSlot.ID, (byte)amount);
                        if (net.Instantiate(obj) != null)
                        {
                            targetSlot.Object = obj;
                            targetSlot.StackSize = amount;
                        }
                        return;
                    }
                targetSlot.Set(obj, amount);
            }
        }

        public bool InsertObject(Net.IObjectProvider net, GameObject obj)
        {
            if ((from slot in this
                 where slot.Object == obj
                 select slot).FirstOrDefault() != null)
                return false;

            // TODO: might want to refactor there 2
            GameObjectSlot firstStack =
                (from slot in this
                 where slot.HasValue
                 where slot.Object.ID == obj.ID
                 where slot.StackSize < slot.StackMax
                 select slot)
                 .FirstOrDefault();
            if (!firstStack.IsNull())
            {
                //firstStack.StackSize++;
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
            if (firstEmpty.IsNull())
                return false;

            firstEmpty.Object = obj;
            //firstEmpty.StackSize = 1;
            net.Despawn(obj);
            return true;
        }
        public bool InsertObject(Net.IObjectProvider net, GameObjectSlot objSlot)
        {
            if (InsertObject(net, objSlot.Object))
            {
              //  objSlot.StackSize--;
                

                objSlot.Clear();
                // dispose if stacksize == 0?
                //net.EventOccured(Components.Message.Types.InventoryChanged, objSlot.Parent);
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
