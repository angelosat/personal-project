using System;
using System.Collections.Generic;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class GameObjectSlot : ITooltippable
    {
        Func<GameObject, bool> _Filter = o => true;
        public Func<GameObject, bool> Filter
        {
            get { return this.ContainerNew == null ? this._Filter : this.ContainerNew.Filter; }
            set { this._Filter = value; }
        }
        public Action<GameObject> ObjectChanged = o => { };
        public string Name = "";
        public ItemContainer Container { get; set; }
        public Container ContainerNew;
        GameObject _Parent;
        public GameObject Parent
        {
            get { return this.Container == null ? this._Parent : this.Container.Parent; }
            set
            {
                this._Parent = value;
                if (this.HasValue)
                    this.Object.Parent = value;
            }
        }
        
        public byte ID { get; set; }
       
        public int StackSize
        {
            get { return this.HasValue ? this.Object.StackSize : 0; }
            set
            {
                if (value == 0)
                {
                    this.Object = null;
                    return;
                }
                if (this.HasValue)
                    this.Object.StackSize = value;
                
            }
        }
        public int StackMax
        {
            get { return this.Object is null ? 1 : Object.StackMax; }
        }
        GameObject _Link;
        public GameObject Link
        {
            get { return this._Link; }
            set
            {
                var old = this._Link;
                this._Link = value;
                if (old != this._Link)
                    ObjectChanged(this._Link);
            }
        }
        GameObject _Object;
        public virtual GameObject Object
        {
            get
            { 
                return this.Link ?? _Object; 
            }
            set
            {
                if (value is not null)
                    if (!this.Filter(value))
                        return;
                if (this._Object != null)
                    this._Object.Slot = null;
                _Object = value;
                ObjectChanged(value);
                OnObjectChanged();
                if (value != null)
                {
                    if (value.Slot is not null && value.Slot != this)
                    {
                        value.Slot.Clear();
                    }
                    value.Slot = this;
                    value.Parent = this.Parent;
                }
            }
        }
        public bool HasValue { get { return Object != null; } }

        public Func<Icon> GetIcon;
        void OnObjectChanged()
        {
            this.GetIcon = () => Object.GetIcon();
        }

        public GameObjectSlot(byte id)
        {
            this.ID = id;
        }
        public GameObjectSlot(GameObject obj = null, int stackSize = 1)
        {
            Object = obj;
            StackSize = obj == null ? 0 : stackSize;
        }
        
        public GameObjectSlot(ItemContainer parent, GameObject obj = null, int stackSize = 1)
        {
            this.Container = parent;
            Object = obj;
            StackSize = obj == null ? 0 : stackSize;
        }
        public bool Swap(GameObjectSlot otherSlot)
        {
            var otherobj = otherSlot.Object;
            otherSlot.Object = this.Object;
            this.Object = otherobj;
            return true;
        }
        static public bool Swap(GameObjectSlot slot1, GameObjectSlot slot2)
        {
            return slot1.Swap(slot2);
        }
        
        /// <summary>
        /// Copies the object and stack amount of one slot to another, and returns the old values of the target slot.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        static public GameObjectSlot Copy(GameObjectSlot source, GameObjectSlot target)
        {
            GameObjectSlot temp = target.Clone();
            target.Object = source.Object;
            target.StackSize = source.StackSize;
            return temp;
        }
        
        public GameObjectSlot SetObject(GameObject obj)
        {
            this.Object = obj;
            return this;
        }

        /// <summary>
        /// Returns a new clone of the item and reduces stacksize by one, or the item itself if stacksize is one, or null if there's no item
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public GameObject Take()
        {
            throw new Exception();
        }

        public override string ToString()
        {
            return $"{this.ID}: {(!string.IsNullOrWhiteSpace(this.Name) ? this.Name + ":" : "")} + {(Object is not null ? Object.Name + $" ({StackSize})" : "<empty>")}";
        }

        public void Instantiate(Action<GameObject> instantiator)
        {
            this.Object?.Instantiate(instantiator);
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(ID);
            writer.Write(this.Name);
            writer.Write(this.StackSize);
            if (this.HasValue)
                this.Object.Write(writer);
        }
        public void Read(System.IO.BinaryReader reader)
        {
            this.ID = reader.ReadByte();
            this.Name = reader.ReadString();
            int size = reader.ReadInt32();
            if (size == 0)
                return;

            // set backing field instead of property so inventorychanged event isn't raised
            this._Object = GameObject.CreatePrefab(reader);
            this._Object.Parent = this.Parent;
            this._Object.Slot = this;
            // no need to set stacksize here since it's saved along with the object
            // PLUS i don't want to raise inventorychanged event that's raised in the property setter
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            data.Add(new SaveTag(SaveTag.Types.Compound, "Object", Object.SaveInternal()));
            data.Add(new SaveTag(SaveTag.Types.Byte, "Stack", (byte)StackSize));
            return data;
        }

        static public GameObjectSlot Create(SaveTag tag) { return Create(null, tag); }
        static public GameObjectSlot Create(ItemContainer parent, SaveTag tag)
        {
            GameObject obj = (SaveTag.Types)tag["Object"].Type switch
            {
                SaveTag.Types.Compound => GameObject.Load((SaveTag)tag["Object"]),
                _ => throw new ArgumentException("Invalid tag type"),
            };
            GameObjectSlot slot = new GameObjectSlot(parent, obj);
            slot.StackSize = Convert.ToInt16((byte)tag["Stack"].Value);
           
            return slot;
        }
        public GameObject Load(SaveTag tag)
        {
            GameObject obj = (SaveTag.Types)tag["Object"].Type switch
            {
                SaveTag.Types.Compound => GameObject.Load((SaveTag)tag["Object"]),
                _ => throw new ArgumentException("Invalid tag type"),
            };
            this.Object = obj;
            this.StackSize = Convert.ToInt16((byte)tag["Stack"].Value);
            return obj;
        }

        public void GetTooltipInfo(Control tooltip)
        {
            if (Object is not null)
            {
                this.Object.GetTooltipInfo(tooltip);
            }
            if (this.ContainerNew is not null)
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, this.ContainerNew.ToString()));
            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, this.ToString()));
        }

        public GameObjectSlot Clone()
        {
            // maybe return new object?
            return new GameObjectSlot(Object, StackSize) { Filter = this.Filter, Container = this.Container, Parent = this.Parent };
        }

        static public GameObjectSlot Empty
        {
            get { return new GameObjectSlot(); }
        }

        /// <summary>
        /// Sets Object to null and returns true if Object was non-null.
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            bool had = HasValue;
            Object = null;
            StackSize = 0; // WARNING! i had this commented out for some reason
            this.Link = null;
            return had;
        }
        internal void Consume(int amount)
        {
            if (this.Object is null)
                return;
            if (this.Object.StackSize > amount)
            {
                this.Object.StackSize-=amount;
            }
            else
            {
                this.Object.Net.DisposeObject(this.Object);
                this.Clear();
            }
        }
    }
}
