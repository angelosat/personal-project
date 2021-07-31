using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ControlCollection : IList<Control>
    {
        readonly Control Parent;
        readonly List<Control> Inner = new();
        public ControlCollection(Control owner)
        {
            Parent = owner;
        }

        public Vector2 BottomRight
        {
            get
            {
                float xMax = 0, yMax = 0;
                this.Inner.ForEach(c =>
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
                this.Inner.ForEach(c =>
                {
                    xMax = Math.Max(xMax, c.Location.X + c.Width);
                    y = Math.Min(y, c.Location.Y);
                });
                return new Vector2(xMax, y);
            }
        }
        public Vector2 BottomLeft
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
        public int Bottom
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

        public int Count => ((ICollection<Control>)this.Inner).Count;

        public bool IsReadOnly => ((ICollection<Control>)this.Inner).IsReadOnly;

        public Control this[int index] { get => ((IList<Control>)this.Inner)[index]; set => ((IList<Control>)this.Inner)[index] = value; }

        public void Add(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (this.Inner.Contains(control))
                    throw new Exception();
                if (control == this.Parent)
                    throw new Exception();
                if (control == null)
                    throw new Exception();
                if (control.Parent != null)
                    control.Parent.Controls.Remove(control);
                this.Inner.Add(control);
                control.Parent = Parent;
                Parent.OnControlAdded(control);
            }
            //this.Parent.Validate();
        }
        public void Insert(int index, IEnumerable<Control> controls)
        {
            foreach (var c in controls)
                this.Insert(index++, c);
        }
        public void Insert(int index, Control control)
        {
            this.Inner.Insert(index, control);
            control.Parent = Parent;
            Parent.OnControlAdded(control);
        }
        public void AlignVertically(int spacing = 0)
        {
            var prev = 0;
            foreach (var c in this)
            {
                c.Location.Y = prev;
                prev = c.Bottom + spacing;
            }
            if (this.Parent.AutoSize)
                this.Parent.ClientSize = this.Parent.PreferredClientSize;
        }
        public void AlignHorizontally(int spacing = 0)
        {
            var prev = 0;
            foreach (var c in this)
            {
                c.Location.X = prev;
                prev = c.Right + spacing;
            }
            if (this.Parent.AutoSize)
                this.Parent.ClientSize = this.Parent.PreferredClientSize;
        }
        public void AlignCenterHorizontally()
        {
            var maxheight = this.Max(c => c.Height);
            foreach (var c in this)
            {
                c.Location = new Vector2(c.Location.X, maxheight * .5f);
                c.Anchor = new Vector2(0, .5f);
            }
        }

        public void Clear()
        {
            foreach (Control control in this.ToList())
                Remove(control);
        }

        public bool Remove(Control control)
        {
            bool removed = this.Inner.Remove(control);
            Parent.OnControlRemoved(control);
            return removed;
        }
        public int RemoveAll(Predicate<Control> predicate)
        {
            var count = this.Inner.RemoveAll(predicate);
            return count;
        }
        public void RemoveAt(int index)
        {
            var ctrl = this[index];
            this.Inner.RemoveAt(index);
            this.Parent.OnControlRemoved(ctrl);
        }

        public int IndexOf(Control item)
        {
            return ((IList<Control>)this.Inner).IndexOf(item);
        }

        public void Add(Control item)
        {
            //((ICollection<Control>)this.Inner).Add(item);
            this.Add(new Control[] { item });
        }

        public bool Contains(Control item)
        {
            return ((ICollection<Control>)this.Inner).Contains(item);
        }

        public void CopyTo(Control[] array, int arrayIndex)
        {
            ((ICollection<Control>)this.Inner).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Control> GetEnumerator()
        {
            return ((IEnumerable<Control>)this.Inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Inner).GetEnumerator();
        }

        public int FindIndex(Predicate<Control> predicate)
        {
            return this.Inner.FindIndex(predicate);
        }

        public Control Find(Predicate<Control> p)
        {
            return this.Inner.Find(p);
        }
    }
    //public class ControlCollection : List<Control>
    //{
    //    Control Parent;

    //    public ControlCollection(Control owner)
    //    {
    //        Parent = owner;
    //    }

    //    public Vector2 BottomRight
    //    {
    //        get
    //        {
    //            float xMax = 0, yMax = 0;
    //            this.ForEach(c =>
    //            {
    //                xMax = Math.Max(xMax, c.Location.X + c.Width);
    //                yMax = Math.Max(yMax, c.Location.Y + c.Height);
    //            });
    //            return new Vector2(xMax, yMax);
    //        }
    //    }
    //    public Vector2 TopRight
    //    {
    //        get
    //        {
    //            float xMax = 0, y = 0;
    //            this.ForEach(c =>
    //            {
    //                xMax = Math.Max(xMax, c.Location.X + c.Width);
    //                y = Math.Min(y, c.Location.Y);
    //            });
    //            return new Vector2(xMax, y);
    //        }
    //    }
    //    public Vector2 BottomLeft
    //    {
    //        get
    //        {
    //            if (this.Count == 0)
    //                return Vector2.Zero;

    //            int x = 0, y = 0;
    //            foreach (var c in this)
    //            {
    //                x = Math.Min(x, c.Left);
    //                y = Math.Max(y, c.Bottom);
    //            }
    //            return new Vector2(x, y);
    //        }
    //    }
    //    public int Bottom
    //    {
    //        get
    //        {
    //            if (this.Count == 0)
    //                return 0;

    //            int y = 0;
    //            foreach (var c in this)
    //            {
    //                y = Math.Max(y, c.Bottom);
    //            }
    //            return y;
    //        }
    //    }

    //    public void Add(params Control[] controls)
    //    {
    //        foreach (Control control in controls)
    //        {
    //            if (base.Contains(control))
    //                throw new Exception();
    //            if (control == this.Parent)
    //                throw new Exception();
    //            if (control == null)
    //                throw new Exception();
    //            if (control.Parent != null)
    //                control.Parent.Controls.Remove(control);
    //            base.Add(control);
    //            control.Parent = Parent;
    //            Parent.OnControlAdded(control);
    //        }
    //        //this.Parent.Validate();
    //    }
    //    public void Insert(int index, IEnumerable<Control> controls)
    //    {
    //        foreach (var c in controls)
    //            this.Insert(index++, c);
    //    }
    //    public new void Insert(int index, Control control)
    //    {
    //        base.Insert(index, control);
    //        control.Parent = Parent;
    //        Parent.OnControlAdded(control);
    //    }
    //    public void AlignVertically(int spacing = 0)
    //    {
    //        var prev = 0;
    //        foreach (var c in this)
    //        {
    //            c.Location.Y = prev;
    //            prev = c.Bottom + spacing;
    //        }
    //        if (this.Parent.AutoSize)
    //            this.Parent.ClientSize = this.Parent.PreferredClientSize;
    //    }
    //    public void AlignHorizontally(int spacing = 0)
    //    {
    //        var prev = 0;
    //        foreach (var c in this)
    //        {
    //            c.Location.X = prev;
    //            prev = c.Right + spacing;
    //        }
    //        if (this.Parent.AutoSize)
    //            this.Parent.ClientSize = this.Parent.PreferredClientSize;
    //    }
    //    public void AlignCenterHorizontally()
    //    {
    //        var maxheight = this.Max(c => c.Height);
    //        foreach (var c in this)
    //        {
    //            c.Location = new Vector2(c.Location.X, maxheight * .5f);
    //            c.Anchor = new Vector2(0, .5f);
    //        }
    //    }

    //    public new void Clear()
    //    {
    //        foreach (Control control in this.ToList())
    //            Remove(control);
    //    }

    //    public new bool Remove(Control control)
    //    {
    //        bool removed = base.Remove(control);
    //        Parent.OnControlRemoved(control);
    //        return removed;
    //    }
    //    public new int RemoveAll(Predicate<Control> predicate)
    //    {
    //        var count = base.RemoveAll(predicate);
    //        return count;
    //    }
    //    public new void RemoveAt(int index)
    //    {
    //        var ctrl = this[index];
    //        base.RemoveAt(index);
    //        this.Parent.OnControlRemoved(ctrl);
    //    }
    //}
}
