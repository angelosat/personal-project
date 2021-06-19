using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public abstract class Element
    {
        //public Window Window;

        Func<Vector2> _LocationFunc = () => Vector2.Zero;
        public virtual Func<Vector2> LocationFunc
        { get { return _LocationFunc; } set { _LocationFunc = value; } }

        //protected Vector2 _Location = Vector2.Zero;
        public Vector2 Location = Vector2.Zero;
        //{
        //    get { return LocationFunc.IsNull() ? _Location : LocationFunc(); }
        //    set { _Location = value; }
        //}

        public Vector2 MouseOffset;

        bool _MouseHover;
        public bool MouseHover
        {
            get { return _MouseHover; }
            set { this._MouseHover = value; }
        }
            public bool MouseHoverLast;
        public int Depth = 0;
        protected int z = 0;
        public int Z
        { get { return z; } }

        public virtual Vector2 ScreenLocation
        {
            get { return LocationFunc() + Location + (Parent != null ? Parent.ScreenLocation + Parent.ClientLocation : new Vector2(0)); } 
            //get { return Location + (Parent != null ? Parent.ScreenLocation : new Vector2(0)); } 
        }

        //public virtual Vector2 GetScreenLocation()
        //{
        //    //return Location + (Parent != null ? Parent.Location : Vector2.Zero);
        //    return ScreenLocation + (Parent != null ? Parent.GetScreenLocation() : Vector2.Zero);
        //}

        /// <summary>
        /// Gets the location of the control relative to the parent window.
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetLocation()
        {
            //return Location + (Parent != null ? Parent.Location : Vector2.Zero);
            return Location + (Parent != null ? Parent.ClientLocation + Parent.GetLocation() : Vector2.Zero);
        }


        Control _Parent;
        public Control Parent
        {
            get { return _Parent; }
            set
            {
                _Parent = value;
                //UpdateScreenLocation();
                OnParentChanged();
            }
        }
        public event EventHandler<EventArgs> ParentChanged;
        protected virtual void OnParentChanged()
        {
            if (ParentChanged != null)
                ParentChanged(this, EventArgs.Empty);
        }

        public virtual void Update()
        {
            //UpdateScreenLocation();
        }
        //public virtual void UpdateScreenLocation()
        //{
        //    ScreenLocation = Location + (Parent != null ? Parent.ScreenLocation + Parent.ClientLocation : new Vector2(0));
        //}

        
        
        public int X
        {
            get { return (int)ScreenLocation.X; }
        }
        public int Y
        {
            get { return (int)ScreenLocation.Y; }
        }
        
        //public int Top
        //{ get { return Bounds.Top; } }
        //public int Left
        //{ get { return Bounds.Left; } }
        //public int Bottom
        //{ get { return Bounds.Bottom; } }
        //public int Right
        //{ get { return Bounds.Right; } }
        

       // public virtual void Draw(SpriteBatch sb) { }
    }
}
