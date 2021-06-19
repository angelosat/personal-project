using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ControlCollection : List<Control>
    {
      //  List<Control> Collection;
        Control Owner;

        public ControlCollection(Control owner)
        {
            Owner = owner;
        //    Collection = new List<Control>();
        }

        public ControlCollection(Control owner, Control[] collection)
        {
            Owner = owner;
        //    Collection = new List<Control>(collection);
        }

        //     public Vector2 TopRight { get { return this.Count == 0 ? Vector2.Zero : this.Last().TopRight; } }
        public Vector2 BottomRight
        {
            get
            {
                float xMax = 0, yMax = 0;
                this.ForEach(c =>
                {
                    xMax = Math.Max(xMax, c.Location.X + c.Width);
                    yMax = Math.Max(yMax, c.Location.Y + c.Height);
                });
                return new Vector2(xMax, yMax);
            }
        }
        public Vector2 TopRight
        {
            get
            {
                float xMax = 0, y = 0;
                this.ForEach(c =>
                {
                    xMax = Math.Max(xMax, c.Location.X + c.Width);
                    y = Math.Min(y, c.Location.Y);
                });
                return new Vector2(xMax, y);
            }
        }
        public Vector2 BottomLeft //{ get { return this.Count == 0 ? Vector2.Zero : this.Last().BottomLeft; } }
        {
            get
            {
                if (this.Count == 0)
                    return Vector2.Zero;

                int x = 0, y = 0;
                foreach (var c in this)
                {
                    x = Math.Min(x, c.Left);
                    y = Math.Max(y, c.Bottom);
                }
                return new Vector2(x, y);
            }
        }
        public int Bottom //{ get { return this.Count == 0 ? Vector2.Zero : this.Last().BottomLeft; } }
        {
            get
            {
                if (this.Count == 0)
                    return 0;

                int y = 0;
                foreach (var c in this)
                {
                    y = Math.Max(y, c.Bottom);
                }
                return y;
            }
        }
        //public ControlCollection Copy()
        //{
        //    ControlCollection copy = new ControlCollection(Owner);
        //    foreach (Control control in this)
        //        copy.Add(control);
        //    return copy;
        //}

        //public Control this[int i]
        //{
        //    get { return Collection[i]; }
        //}
        
        //public new void Add(Control control)
        //{
        //    if (control.Parent != null)
        //        control.Parent.Controls.Remove(control);
        //    base.Add(control);
        //    control.Parent = Owner;
        //    Owner.OnControlAdded(control);
        //}

        public void Add(params Control[] controls)
        {
            foreach (Control control in controls)
            {
                if (control == this.Owner)
                    throw new Exception();
                if (control == null)
                    throw new Exception();
                if (control.Parent != null)
                    control.Parent.Controls.Remove(control);
                base.Add(control);
                control.Parent = Owner;
                Owner.OnControlAdded(control);
            }
        }

        public new void Insert(int index, Control control)
        {
            base.Insert(index, control);
            control.Parent = Owner;
            Owner.OnControlAdded(control);
        }
        public void AlignTopToBottom()
        {
            var prev = 0;
            foreach (var c in this)
            {
                c.Location.Y = prev;
                prev = c.Bottom;
            }
        }
        //public void AddRange(Control[] collection)
        //{
        //    //foreach (Control control in collection)
        //    //    base.Add(control);
        //    this.AddRange(collection);
        //    //Owner.OnControlAdded();
        //}

        //public new void AddRange(IEnumerable<Control> collection)
        //{
        //    foreach (Control control in collection)
        //        this.Add(control);
        //}

        //public List<Control>.Enumerator GetEnumerator()
        //{
        //    return this.GetEnumerator();
        //}

        //public int IndexOf(Control item)
        //{
        //    return this.IndexOf(item);
        //}

        public new void Clear()
        {
            //foreach (Control control in Collection)
            //    control.Dispose();

            //Collection.Clear();
            //Owner.OnControlRemoved();

            foreach (Control control in this.ToList())
                Remove(control);
                //Owner.OnControlRemoved(control);
            //base.Clear();
        }

        //public int FindIndex(Predicate<Control> func)
        //{
        //    return this.FindIndex(func);
        //}

        public new bool Remove(Control control)
        {
            bool removed = base.Remove(control);
            Owner.OnControlRemoved(control);

            Owner.Invalidate(true); //because i need to redraw owner graphics correctly after resize

            return removed;
        }

        //public int Count
        //{
        //    get { return this.Count; }
        //}

        //public List<Control> ToList()
        //{
        //    return this.ToList();
        //}
    }
}
