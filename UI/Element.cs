using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UI
{
    public abstract class Element
    {
        //public Window Window;
        internal Func<Vector2> AnchorPoint;
        Func<Vector2> _LocationFunc = () => Vector2.Zero;
        public virtual Func<Vector2> LocationFunc
        { get { return _LocationFunc; } set { _LocationFunc = value; } }


        public Vector2 Location = Vector2.Zero;


        public Vector2 MouseOffset;
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual Vector2 Dimensions
        {
            get
            { return new Vector2(Width, Height); }
            set
            {
                Width = (int)value.X;
                Height = (int)value.Y;
            }
        }
        Vector2 _Anchor = Vector2.Zero;
        public virtual Vector2 Anchor// = Vector2.Zero;
        {
            get { return _Anchor; }
            set
            {
                _Anchor = value;
                //Location = Location -Dimensions * value;
            }
        }
        bool _MouseHover;
        public bool MouseHover
        {
            get { return _MouseHover; }
            set { this._MouseHover = value; }
        }
            public bool MouseHoverLast;
        public int Depth = 0;
        protected int z = 0;
        public virtual int Padding { get; }
        public int Z
        { get { return z; } }

        

        //public virtual Vector2 GetScreenLocation()
        //{
        //    //return Location + (Parent != null ? Parent.Location : Vector2.Zero);
        //    return ScreenLocation + (Parent != null ? Parent.GetScreenLocation() : Vector2.Zero);
        //}

        
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
