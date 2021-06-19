using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    public class Container
    {
        public int ID;
        public string Name = "";
        public Func<GameObject, bool> Filter = o => true;
        //public List<Container> Children = new List<Container>();
        GameObject _Parent;
        public GameObject Parent
        {
            get { return this._Parent; }
            set
            {
                this._Parent = value;
                foreach (var slot in this.Slots)
                    slot.Parent = value;
            }
        }
        public List<GameObjectSlot> Slots = new List<GameObjectSlot>();
        public Container()
        {

        }
        public Container(int capacity)
        {
            Initialize(capacity);
        }
        public Container(int capacity, Func<GameObject, bool> filter)
        {
            this.Filter = filter;
            Initialize(capacity, filter);
        }
        private void Initialize(int capacity)
        {
            //this.Slots = new List<GameObjectSlot>();
            for (int i = 0; i < capacity; i++)
            {
                this.Slots.Add(new GameObjectSlot((byte)i) { ContainerNew = this });
            }
        }
        private void Initialize(int capacity, Func<GameObject, bool> filter)
        {
            //this.Slots = new List<GameObjectSlot>();
            for (int i = 0; i < capacity; i++)
            {
                this.Slots.Add(new GameObjectSlot((byte)i) { ContainerNew = this, Filter = filter });
            }
        }
        //public Container(GameObject parent, int id, int size)
        //{
        //    this.ID = id;
        //    this.Parent = parent;
        //    for (int i = 0; i < size; i++)
        //    {
        //        this.Slots.Add(new GameObjectSlot((byte)i) { Parent = parent });
        //    }
        //}
        public bool Contains(Predicate<GameObject> filter)
        {
            return (from slot in this.Slots
                    where slot.HasValue
                    where filter(slot.Object)
                    select slot).FirstOrDefault() != null;
        }

        public GameObjectSlot GetSlot(int id)
        {
            return this.Slots.FirstOrDefault(f => f.ID == id);
        }
        public GameObjectSlot GetSlot(string name)
        {
            return this.Slots.FirstOrDefault(f => f.Name == name);
        }

        public List<GameObjectSlot> GetNonEmpty()
        {
            return (from slot in this.Slots where slot.Object != null select slot).ToList();
        }
        //public List<GameObjectSlot> GetEmpty()
        //{
        //    return (from slot in this.Slots where slot.Object == null select slot).ToList();
        //}
        public IEnumerable<GameObjectSlot> GetEmpty()
        {
            return (from slot in this.Slots where slot.Object == null select slot);
        }
        public int GetAmount(Func<GameObject, bool> filter)
        {
            var a = 0;
            foreach (var found in from o in this.Slots where o.Object != null where filter(o.Object) select o)
                a += found.StackSize;
            return a;
        }
        public int Capacity { get { return this.Slots.Count; } }

        public void Write(System.IO.BinaryWriter writer)
        {
            var haveObjects = from slot in this.Slots where slot.Object != null select slot;
            writer.Write(haveObjects.Count());
            foreach (var slot in haveObjects)
            {
                writer.Write(slot.ID);
                slot.Write(writer);
            }
        }
        public void Read(System.IO.BinaryReader reader)
        {
            int haveObjects = reader.ReadInt32();
            for (int i = 0; i < haveObjects; i++)
            {
                var id = reader.ReadByte();
                var slot = this.Slots.FirstOrDefault(f => f.ID == id);
                slot.Read(reader);
            }
        }
        //public void Write(System.IO.BinaryWriter writer)
        //{
        //    writer.Write(this.ID);
        //    writer.Write(this.Name);
        //    writer.Write(this.Slots.Count);
        //    for (int i = 0; i < this.Slots.Count; i++)
        //    {
        //        var slot = this.Slots[i];
        //        //writer.Write(i);
        //        slot.Write(writer);
        //    }
        //}
        //public void Read(System.IO.BinaryReader reader)
        //{
        //    this.Slots = new List<GameObjectSlot>();
        //    this.ID = reader.ReadInt32();
        //    this.Name = reader.ReadString();
        //    int size = reader.ReadInt32();
        //    for (byte n = 0; n < size; n++)
        //    {
        //        var slot = new GameObjectSlot() { Parent = this.Parent, ContainerNew = this };
        //        slot.Read(reader);
        //        this.Slots.Add(slot);
        //    }
        //}
        public override string ToString()
        {
            return this.ID.ToString() + ":" + this.Name + ":" + this.Slots.Count.ToString();
        }
        public string ToStringFull()
        {
            var text = this.ToString();
            foreach (var slot in this.Slots)
            {
                text += '\n' + slot.ToString();
            }
            return text;
        }
        public bool InsertObject(GameObject obj, out GameObjectSlot foundSlot)
        {
            if (obj == null)
                throw new Exception();
            var net = obj.Net;

            // check if entity is already inside container
            if ((from slot in this.Slots
                 where slot.Object == obj
                 select slot).FirstOrDefault() != null)
                throw new Exception();

            // TODO: might want to refactor there 2
            GameObjectSlot firstStack =
                (from slot in this.Slots
                 where slot.HasValue
                 where slot.Object.IDType == obj.IDType
                 where slot.StackSize < slot.StackMax
                 select slot)
                 .FirstOrDefault();
            if (firstStack != null)
            {
                // TODO: handle case where stacksize exceeds stackmax
                foundSlot = firstStack;
                return true;
            }

            GameObjectSlot firstEmpty =
                (from slot in this.Slots
                 where !slot.HasValue
                 select slot)
                 .FirstOrDefault();
            if (firstEmpty == null)
            {
                foundSlot = null;
                return false;
            }
            foundSlot = firstEmpty;
            return true;
        }
        public bool InsertObject(Entity obj)
        {
            //return this.InsertObject(objSlot.ToSlotLink());
            var net = obj.NetNew;

            // check if entity is already inside container
            if ((from slot in this.Slots
                 where slot.Object == obj
                 select slot).FirstOrDefault() != null)
                return false;

            // TODO: might want to refactor there 2
            GameObjectSlot firstStack =
                (from slot in this.Slots
                 where slot.HasValue
                 where slot.Object.CanAbsorb(obj)
                 //where slot.StackSize < slot.StackMax
                 select slot)
                 .FirstOrDefault();
            if (firstStack != null)
            {
                // TODO: handle case where stacksize exceeds stackmax
                firstStack.StackSize += obj.StackSize;
                //this.Parent.Net.EventOccured(Message.Types.ItemGot, this.Parent, obj);
                // merge objects to existins object in slot, despawn and dispose old one
                net?.Despawn(obj);
                net.DisposeObject(obj);
                return true;
            }

            GameObjectSlot firstEmpty =
                (from slot in this.Slots
                 where !slot.HasValue
                 select slot)
                 .FirstOrDefault();
            if (firstEmpty == null)
            {
                // drop item?
                return false;
            }
            //firstEmpty.Object = obj;
            firstEmpty.SetObject(obj);
            //this.Parent.Net.EventOccured(Message.Types.ItemGot, this.Parent, obj);
            net?.Despawn(obj);
            return true;

        }
        public bool InsertObject(GameObjectSlot objSlot)
        {
            if (objSlot.Object == null)
                return false;
            var obj = objSlot.Object;
            //var net = obj.Net;
            var net = obj.NetNew;

            // check if entity is already inside container
            if ((from slot in this.Slots
                 where slot.Object == obj
                 select slot).FirstOrDefault() != null)
                return false;

            // TODO: might want to refactor there 2
            GameObjectSlot firstStack =
                (from slot in this.Slots
                 where slot.HasValue
                 //where slot.Object.IDType == obj.IDType
                 where slot.Object.CanAbsorb(obj)
                 where slot.StackSize < slot.StackMax // maybe not necessary with canabsorb
                 select slot)
                 .FirstOrDefault();
            if (firstStack != null)
            {
                // TODO: handle case where stacksize exceeds stackmax
                firstStack.StackSize += obj.StackSize;
                //this.Parent.Net.EventOccured(Message.Types.ItemGot, this.Parent, obj);
                // merge objects to existins object in slot, despawn and dispose old one
                net?.Despawn(obj);
                net.DisposeObject(obj);
                objSlot.Clear();
                return true;
            }

            GameObjectSlot firstEmpty =
                (from slot in this.Slots
                 where !slot.HasValue
                 select slot)
                 .FirstOrDefault();
            if (firstEmpty == null)
            {
                // drop item?
                return false;
            }
            //firstEmpty.Object = obj;
            firstEmpty.SetObject(obj);
            //this.Parent.Net.EventOccured(Message.Types.ItemGot, this.Parent, obj);
            net?.Despawn(obj);
            objSlot.Clear();
            return true;
        }
        public bool InsertObject(GameObjectSlot objSlot, out int index)
        {
            index = -1;
            if (objSlot.Object == null)
                return false;
            var obj = objSlot.Object;
            var net = obj.Net;

            // check if entity is already inside container
            if ((from slot in this.Slots
                 where slot.Object == obj
                 select slot).FirstOrDefault() != null)
                return false;

            // TODO: might want to refactor there 2
            GameObjectSlot firstStack =
                (from slot in this.Slots
                 where slot.HasValue
                 where slot.Object.IDType == obj.IDType
                 where slot.StackSize < slot.StackMax
                 select slot)
                 .FirstOrDefault();
            if (firstStack != null)
            {
                // TODO: handle case where stacksize exceeds stackmax
                firstStack.StackSize += obj.StackSize;
                // merge objects to existins object in slot, despawn and dispose old one
                net.Despawn(obj);
                net.DisposeObject(obj);
                objSlot.Clear();
                // haul existing item stack?
                //index = firstStack.ID;
                return true;
            }

            GameObjectSlot firstEmpty =
                (from slot in this.Slots
                 where !slot.HasValue
                 select slot)
                 .FirstOrDefault();
            if (firstEmpty == null)
                return false;

            //firstEmpty.Object = obj;
            firstEmpty.SetObject(obj);
            index = firstEmpty.ID;

            net.Despawn(obj);
            objSlot.Clear();
            return true;
        }
        public bool Remove(Entity item)
        {
            var slot = this.Slots.FirstOrDefault(s => s.Object == item);
            if (slot == null)
                return false;
            slot.Clear();
            return true;
        }
        public int Count(Func<GameObject, bool> condition)
        {
            int amount = 0;
            (from slot in this.Slots
             where slot.HasValue
             where condition(slot.Object)
             select slot)
             .ToList()
             .ForEach(slot => amount += slot.StackSize);
            return amount;
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> containerTag = new List<SaveTag>();
            containerTag.Add(new SaveTag(SaveTag.Types.Int, "ID", this.ID));
            containerTag.Add(new SaveTag(SaveTag.Types.Int, "Count", this.Slots.Count));
            SaveTag items = new SaveTag(SaveTag.Types.Compound, "Items");
            for (int i = 0; i < this.Slots.Count; i++)
            {
                GameObjectSlot objSlot = this.Slots[i];
                if (objSlot.Object != null)
                    items.Add(new SaveTag(SaveTag.Types.Compound, i.ToString(), objSlot.Save()));
            }
            containerTag.Add(items);
            return containerTag;
        }
        public Container Load(SaveTag containerTag)
        {
            Dictionary<string, SaveTag> shit = containerTag.Value as Dictionary<string, SaveTag>;//.ToDictionary();
            containerTag.TryGetTagValue<int>("ID", out this.ID);
            int count = (int)shit["Count"].Value;
            //this.Initialize(count);
            //List<SaveTag> itemList = shit["Items"].Value as List<SaveTag>;
            Dictionary<string, SaveTag> itemList = shit["Items"].Value as Dictionary<string, SaveTag>;
            foreach (SaveTag itemTag in itemList.Values)
            {
                if (itemTag.Value == null)
                    continue;
                int index = byte.Parse(itemTag.Name);
                GameObjectSlot slot = GameObjectSlot.Create(itemTag);
                slot.ContainerNew = this;
                slot.ID = (byte)index;
                //this.Slots[index] = slot;
                this.Slots[index].Object = slot.Object;
            }
            return this;
        }

        internal void Dispose()
        {
            foreach (var slot in this.Slots)
                if (slot.Object != null)
                    slot.Object.Dispose();
        }

        
    }
}
