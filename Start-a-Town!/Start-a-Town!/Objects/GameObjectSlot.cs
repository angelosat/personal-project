using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class GameObjectSlotCollection : List<GameObjectSlot>, IInterfaceable
    {
        public GameObjectSlotCollection()
        {
        }
        public GameObjectSlotCollection(List<GameObjectSlot> list)
        {
            this.AddRange(list);
        }

        public override string ToString()
        {
            string text = "";
            for (int i = 0; i < this.Count; i++)
                text += "[" + i + "]: " + this[i].ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public void GetUI(UI.Control ui)
        {
            int i = 0;
            foreach (var obj in this)
            {
                Slot slot = new Slot(new Vector2(0, i * Slot.DefaultHeight)) { Tag = obj };
                Label label = new Label(slot.CenterRight, obj.HasValue ? obj.Object.Name : "<empty>", HorizontalAlignment.Left, VerticalAlignment.Center);
                ui.Controls.Add(slot, label);
            }
        }
    }

    public class GameObjectSlot : ITooltippable
    {
        Func<GameObject, bool> _Filter = o => true;
        public Func<GameObject, bool> Filter
        {
            //get { return this._Filter; }// this.Container.IsNull() ? this._Filter : this.Container.Filter; }
            //get { return this.Container.IsNull() ? this._Filter : this.Container.Filter; }
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
        //public TargetArgs ParentTarget
        //{
        //    get { return this.Object != null ? this.Object.ParentTarget : null; }
        //    set
        //    {
        //        if (this.HasValue)
        //            this.Object.ParentTarget = value;
        //    }
        //}
        //byte _ID;
        public byte ID { get; set; }
        //{ get { return _ID; }
        //    set
        //    {
        //        this._ID = value;
        //    }
        //}

        public int StackSize
        {
            get { return this.HasValue ? this.Object.GetInfo().StackSize : 0; }
            set
            {
                //if (this.Object.GetInfo().StackSize != value)
                if (this.Parent != null)
                    if (this.Parent.Net != null)
                    this.Parent.Net.EventOccured(Components.Message.Types.InventoryChanged, this.Parent);
                if (value == 0)
                {
                    this.Object = null;
                    return;
                }
                if (this.HasValue)
                    this.Object.GetInfo().StackSize = value;
                
            }
            //get { return _StackSize; }
            //set
            //{
            //    _StackSize = value;
            //    if (value == 0)
            //        Object = null;
            //}
        }
        public int StackMax
        {
            get { return this.Object.IsNull() ? 1 : Object.GetInfo().StackMax; }
            //get { return this.Object.IsNull() ? 1 : (int)Object["Gui"]["StackMax"]; }
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
                
                //if(_Object != value)
                if (value != null)
                    if (!this.Filter(value))
                        return;
                if (this.Parent != null)
                    if (this.Parent.Net != null)
                    {
                        this.Parent.Net.EventOccured(Components.Message.Types.InventoryChanged, this.Parent);
                    }
                if (this._Object != null)
                    this._Object.Slot = null;
                _Object = value;
                ObjectChanged(value);
                OnObjectChanged();
                if (value != null)
                {
                    if (value.Slot != null && value.Slot != this)
                    {
                        value.Slot.Clear();
                    }
                    value.Slot = this;
                    value.Parent = this.Parent;
                }
            }
            //set
            //{
                
            //    //if(_Object != value)
            //    if (this.Parent != null)
            //        if (this.Parent.Net != null)
            //        this.Parent.Net.EventOccured(Components.Message.Types.InventoryChanged, this.Parent);
            //    _Object = value;
            //    OnObjectChanged();
            //    if (value == null)
            //        return;
            //    if (!this.Filter(value))
            //        return;
            //    //if (this.Container != null)
            //    //    _Object.Parent = this.Container.Parent;
            //    value.Parent = this.Parent;
            //}
        }//StackSize = 1; } }
        public bool HasValue { get { return Object != null; } }

        public Func<Icon> GetIcon;
        void OnObjectChanged()
        {
            //this.GetIcon = Object.IsNull() ? null : new Func<GameObject, Icon>(obj => Object["Gui"].GetProperty<Icon>("Icon"));
            this.GetIcon = () => Object.GetIcon();// Object["Gui"].GetProperty<Icon>("Icon");
        }

        //public Icon Icon;
        //void OnObjectChanged()
        //{
        //    //this.GetIcon = Object.IsNull() ? null : new Func<GameObject, Icon>(obj => Object["Gui"].GetProperty<Icon>("Icon"));
        //    if(!Object.IsNull()) this.Icon = Object["Gui"].GetProperty<Icon>("Icon");
        //}
        public GameObjectSlot(byte id)
        {
            this.ID = id;
        }
        public GameObjectSlot(GameObject obj = null, int stackSize = 1)
        {
            Object = obj;
            StackSize = obj == null ? 0 : stackSize;

        }
        //public GameObjectSlot(GameObject parent, GameObject obj = null, int stackSize = 1)
        //{
        //    this.Parent = parent;
        //    Object = obj;
        //    StackSize = obj == null ? 0 : stackSize;
        //}
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
        public bool SwapOld(GameObjectSlot otherSlot)
        {
            GameObjectSlot temp = otherSlot.Clone();

            otherSlot.Object = Object;
            otherSlot.StackSize = StackSize;

            this.Object = temp.Object;
            this.StackSize = temp.StackSize;
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
        public GameObjectSlot Copy(GameObjectSlot target)
        {
            GameObjectSlot temp = target.Clone();
            target.Object = this.Object;
            target.StackSize = this.StackSize;
            return temp;
        }
        [Obsolete]
        public bool Set(GameObject obj, int stacksize)
        {
            if (!this.Filter(obj))
                return false;
            this.Object = obj;
            this.StackSize = stacksize;
            return true;
        }
        public GameObjectSlot SetObject(GameObject obj)
        {
            //if (!this.Filter(obj))
            //    return false;
            this.Object = obj;
            //if (StackSize == 0)
            //    StackSize = 1;
            return this;
        }
        //public GameObjectSlot Set Objec

        /// <summary>
        /// Returns a new clone of the item and reduces stacksize by one, or the item itself if stacksize is one, or null if there's no item
        /// </summary>
        /// <returns></returns>
        public GameObject Take()
        {
            if (Object == null)
                return null;
            GameObject obj = StackSize == 1 ? this.Object : GameObject.Create(this.Object.IDType);
            StackSize -= 1;
            if (StackSize == 0)
                Clear();
            return obj;
        }

        /// <summary>
        /// Returns false if the slot is empty, or returns true and assigns a new copy of the object to the out param.
        /// </summary>
        /// <returns></returns>
        public bool Take(out GameObject obj)
        {
            if (Object == null)
            {
                obj = null;
                return false;
            } 
            obj = StackSize == 1 ? this.Object : GameObject.Create(this.Object.IDType);
            StackSize -= 1;
            if (StackSize == 0)
                Clear();
            return true;
        }

        public override string ToString()
        {
            //return (Object != null ? Object.Name + " (" + StackSize + ")" : "<empty>");
            return this.ID.ToString() + ":" + (!string.IsNullOrWhiteSpace(this.Name) ? this.Name + ":" : "") + (Object != null ? Object.Name + " (" + StackSize + ")" : "<empty>");
        }

        public void Instantiate(Action<GameObject> instantiator)
        {
            if (this.HasValue)
                this.Object.Instantiate(instantiator);
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(ID);
            writer.Write(this.Name);
            writer.Write(this.StackSize);
            if (this.HasValue)//StackSize > 0)
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
            //this.StackSize = size;

        }

        public List<SaveTag> Save()
        {
            //            return Object.Save();
            List<SaveTag> data = new List<SaveTag>();//Tag(Tag.Types.Compound, "Slot");
            data.Add(new SaveTag(SaveTag.Types.Compound, "Object", Object.SaveInternal()));
            data.Add(new SaveTag(SaveTag.Types.Byte, "Stack", (byte)StackSize));
            return data;
        }

        public SaveTag Save(string name)
        {
            return new SaveTag(SaveTag.Types.Compound, name, this.Save());
        }


        public List<SaveTag> SaveShallow()
        {
            //            return Object.Save();
            List<SaveTag> data = new List<SaveTag>();//Tag(Tag.Types.Compound, "Slot");
            data.Add(new SaveTag(SaveTag.Types.Int, "Object", this.HasValue ? (int)Object.IDType : -1));
            data.Add(new SaveTag(SaveTag.Types.Byte, "Stack", (byte)StackSize));
            return data;
        }

        //static public GameObjectSlot Create(Tag tag)
        //{
        //    //return new GameObjectSlot(GameObject.Create(tag));
        //    GameObjectSlot slot = new GameObjectSlot(GameObject.Create((Tag)tag["Object"]));
        //    slot.StackSize = Convert.ToInt16((byte)tag["Stack"].Value);
        //    return slot;
        //}
        //static public GameObjectSlot Load(SaveTag tag, Action<GameObject> factory)
        //{
        //    GameObjectSlot slot = Load(tag);
        //    if (slot.HasValue)
        //        factory(slot.Object);
        //    return slot;
        //}
        static public GameObjectSlot Create(SaveTag tag) { return Create(null, tag); }
        static public GameObjectSlot Create(ItemContainer parent, SaveTag tag)
        {
            GameObject obj;
            switch ((SaveTag.Types)tag["Object"].Type)
            {
                case SaveTag.Types.Int:
                    obj = GameObject.Create((int)tag["Object"].Value);
                    break;
                case SaveTag.Types.Compound:
                    obj = GameObject.Load((SaveTag)tag["Object"]);
                    break;
                default:
                    throw new ArgumentException("Invalid tag type");
            }
            GameObjectSlot slot = new GameObjectSlot(parent, obj);
            slot.StackSize = Convert.ToInt16((byte)tag["Stack"].Value);
           
            return slot;
        }
        public GameObject Load(SaveTag tag)
        {
            GameObject obj;
            switch ((SaveTag.Types)tag["Object"].Type)
            {
                case SaveTag.Types.Int:
                    obj = GameObject.Create((int)tag["Object"].Value);
                    break;
                case SaveTag.Types.Compound:
                    obj = GameObject.Load((SaveTag)tag["Object"]);
                    break;
                default:
                    throw new ArgumentException("Invalid tag type");
            }
            
            this.Object = obj;
            this.StackSize = Convert.ToInt16((byte)tag["Stack"].Value);
            return obj;
        }


        static public GameObjectSlot Create(int id)
        {
            //GameObjectSlot slot = new GameObjectSlot(GameObject.Create(id));
            //slot.StackSize = 0;
            //return slot;
            return new GameObjectSlot(GameObject.Create(id));
        }
        static public GameObjectSlot Create(GameObject.Types id, int stacksize = 1)
        {
            //GameObjectSlot slot = new GameObjectSlot(GameObject.Create(id));
            //slot.StackSize = 0;
            //return slot;
            return new GameObjectSlot(GameObject.Create(id), stacksize);
        }
        public int FreeSpace
        {
            get
            {
                GuiComponent gui = (GuiComponent)Object["Gui"];
                return gui.GetProperty<int>("StackMax") - StackSize;
            }
        }

        public void GetTooltipInfo(Tooltip tooltip)
        {
            if (Object != null)
            {
                ////tooltip.Controls.Add(new Label("StackSize: " + StackSize));
                ////   tooltip.Controls.Add(new Label(ToString()));
                //Object.GetTooltipInfo(tooltip);
                //Label name = (tooltip.Controls.First() as Label);
                //name.Text = ToString();
                //tooltip.Controls.Remove(name);
                //tooltip.Controls.Insert(0, name);
                this.Object.GetTooltipInfo(tooltip);
            }
            if (this.ContainerNew != null)
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, this.ContainerNew.ToString()));
            //tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "[Container ID: " + this.Container.ID.ToString() + "]"));
            //tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "[Slot ID: " + this.ID.ToString() + "]"));
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

        public bool Insert(GameObjectSlot toInsert)
        {
            if(toInsert.HasValue == false)
                return false;
            if(!this.HasValue)
            {
                this.Swap(toInsert);
                return true;
            }
            if(this.Object.IDType != toInsert.Object.IDType)
                return false;
            if (this.Object.StackSize + toInsert.StackSize > this.Object.StackMax)
                return false;
            this.Object.StackSize += toInsert.Object.StackSize;
            toInsert.Dispose();
            return true;
        }
        public void Dispose()
        {
            if (this.Object == null)
                return;
            if (this.Object.Net == null && this.Object.RefID != 0)
                throw new Exception();
            if (this.Object.Net != null)
                this.Object.Net.SyncDisposeObject(this.Object);
            this.Clear();
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
        internal void Consume()
        {
            if (this.Object == null)
                return;
            this.Object.Net.DisposeObject(this.Object);
            this.Clear();

            //var obj = this.Object;
            //if (obj == null)
            //    return;
            //obj.Net.DisposeObject(obj);
            //this.Clear();

        }
        internal void Consume(int amount)
        {
            if (this.Object == null)
                return;
            if (this.Object.StackSize > amount)
            {
                this.Object.StackSize-=amount;//--;
            }
            else
            {
                this.Object.Net.DisposeObject(this.Object);
                this.Clear();
            }
        }
    }

}
