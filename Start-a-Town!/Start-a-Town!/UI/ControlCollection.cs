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
        Control Parent;

        public ControlCollection(Control owner)
        {
            Parent = owner;
        //    Collection = new List<Control>();
        }

        public ControlCollection(Control owner, Control[] collection)
        {
            Parent = owner;
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


        public void Add(params Control[] controls)
        {
            foreach (Control control in controls)
            {
                if (base.Contains(control))
                    throw new Exception();
                if (control == this.Parent)
                    throw new Exception();
                if (control == null)
                    throw new Exception();
                if (control.Parent != null)
                    control.Parent.Controls.Remove(control);
                base.Add(control);
                control.Parent = Parent;
                Parent.OnControlAdded(control);

                // TODO: cache where i can! this has a performance impact
                //control.Validate(true); // TODO: this messes up things when saving! fix it!
            }
            //Parent.OnControlsChanged();
        }

        public new void Insert(int index, Control control)
        {
            base.Insert(index, control);
            control.Parent = Parent;
            Parent.OnControlAdded(control);
            //Parent.OnControlsChanged();
        }
        public void AlignVertically()
        {
            var prev = 0;
            foreach (var c in this)
            {
                c.Location.Y = prev;
                prev = c.Bottom;
            }
            if(this.Parent.AutoSize)
            this.Parent.ClientSize = this.Parent.PreferredClientSize;
        }
        public void AlignHorizontally()
        {
            var prev = 0;
            foreach (var c in this)
            {
                c.Location.X = prev;
                prev = c.Right;
            }
            if (this.Parent.AutoSize)
            this.Parent.ClientSize = this.Parent.PreferredClientSize;
        }
        public void AlignCenterHorizontally()
        {
            var maxheight = this.Max(c => c.Height);
            foreach (var c in this)
            {
                //if (c.Height == maxheight)
                //    return;
                c.Location = new Vector2(c.Location.X, maxheight * .5f);
                c.Anchor = new Vector2(0, .5f);
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
            //Parent.OnControlsChanged();
        }

        //public int FindIndex(Predicate<Control> func)
        //{
        //    return this.FindIndex(func);
        //}

        public new bool Remove(Control control)
        {
            bool removed = base.Remove(control);
            Parent.OnControlRemoved(control);
            //Parent.OnControlsChanged();

            Parent.Invalidate(true); //because i need to redraw owner graphics correctly after resize
            return removed;
        }
        public new int RemoveAll(Predicate<Control> predicate)
        {
            var count = base.RemoveAll(predicate);
            //Parent.OnControlsChanged();
            Parent.Invalidate(true); //because i need to redraw owner graphics correctly after resize
            return count;
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
