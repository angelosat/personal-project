using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI
{
    public class TooltipArgs : EventArgs
    {
        //public List<GroupBox> TooltipGroups;
        //public TooltipArgs(List<GroupBox> groups)
        //{
        //    TooltipGroups = groups;
        //}
        public Tooltip Tooltip;
        public TooltipArgs(Tooltip tooltip)
        {
            Tooltip = tooltip;
        }
    }

    public class TooltipManager
    {
        static public bool MouseTooltips = true;
        static public float DelayInterval = Engine.TargetFps;
        float DelayValue;
        public static int Width = 300;
        protected static TooltipManager _Instance;
        public static TooltipManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new TooltipManager();
                return _Instance;
            }
        }
        Tooltip _Tooltip;
        public Tooltip Tooltip
        {
            get { return _Tooltip; }
            set
            {
                if (_Tooltip != null)
                    _Tooltip.Dispose();
                _Tooltip = value;
            }
        }

        public void Initialize()
        {
        }

        //Stack<ITooltippable> Objects;
        //public void Push(ITooltippable obj)
        //{
        //    Objects.Push(obj);
        //}

        ITooltippable Object;
        //protected ITooltippable _Object;
        //public ITooltippable Object
        //{
        //    get { return _Object; }
        //    set
        //    {
                
        //        ITooltippable old = _Object;
        //            _Object = value;
        //        //if (_Object != null)
        //        //    Console.WriteLine(_Object.ToString());
        //        //else
        //        //    Console.WriteLine("null");
        //        if (old != _Object)
        //        {
        //            //if (old != null)
        //            //    old.TooltipChanged -= Object_TooltipChanged;
        //            //Console.WriteLine(old + " " + value);
        //            if (Tooltip != null)
        //                Tooltip.Dispose();

        //            if (value != null)
        //            {
        //                //Paint();
        //                //Tooltip = value.Tooltip;
        //            //    Object.TooltipChanged += new EventHandler<EventArgs>(Object_TooltipChanged);
        //                //if (Object.Tooltip != null)
        //                //    if (Object.Tooltip.Count > 0)
        //                        Build();
        //            }
        //            else
        //                Tooltip = null;
        //            //OnTooltipChanged();
        //        }
                
        //    }
        //}

        public void Update()
        {

            if (this.Object.IsNull())
                return;
            //if (!this.Tooltip.IsNull())
            //    return;
            this.DelayValue -= 1;//  GlobalVars.DeltaTime;
            
            if (DelayValue <= 0)
            {
                DelayValue = DelayInterval;
                if (this.Tooltip.IsNull())
                    Build();
                else
                {
                    //this.Tooltip.Invalidate(true); 
                    this.Tooltip.Update();
                }
            }
            if(this.Tooltip!=null)
                this.Tooltip.Update();
        }

        void Build()
        {
            //return;
           // t = Interval;
            Tooltip = new Tooltip(Object);
            Tooltip.AutoSize = true;
         //   Console.WriteLine(Object);
            //Tooltip.Tag = Object;
            Object.GetTooltipInfo(Tooltip);

            
            OnTooltipBuild(new TooltipArgs(Tooltip));

            if (Tooltip.Controls.Count > 0)
            {
                this.Tooltip.Update();
                //Tooltip.Paint();
                Tooltip.SetMousethrough(true, true);
            }
            else
                Tooltip = null;
            //this.Tooltip.Update();
            
        }

        void Object_TooltipChanged(object sender, EventArgs e)
        {
            Build();
        }

        BackgroundStyle BorderStyle = BackgroundStyle.Tooltip;

        TooltipManager()
        {
            //Objects = new Stack<ITooltippable>();
            Controller.MouseoverObjectChanged += new EventHandler<MouseoverEventArgs>(Controller_MouseoverObjectChanged);
        }

        void Controller_MouseoverObjectChanged(object sender, MouseoverEventArgs e)
        {
            Tooltip = null;
            ITooltippable obj = e.ObjectNext as ITooltippable;
            this.DelayValue = DelayInterval;
            var oldobj = this.Object;
            Object = Controller.Instance.MouseoverNext.Object as ITooltippable;//.Target;// 
            //Console.WriteLine((oldobj != null ? oldobj.ToString() : "null") + (Object != null ? Object.ToString() : "null"));

            //Object = obj; // uncomment to diplay block tooltip as a gameobject
            //if (this.Object != null)
            //    this.Object.ToConsole();
            //else
            //    "null".ToConsole();
        }

        //static public float Interval = Engine.TargetFps;
        //float t = Interval;
        //public void Update()
        //{
        //    if (Tooltip == null)
        //        return;

        //    t -= GlobalVars.DeltaTime;
        //    if (t < 0)
        //    {
        //        t = Interval;
        //     //   Tooltip.Update();
        //        Build();
                
        //    }
        //}

        public event EventHandler<EventArgs> TooltipChanged;
        protected void OnTooltipChanged()
        {
            if (TooltipChanged != null)
                TooltipChanged(this, EventArgs.Empty);
        }

        public event EventHandler<TooltipArgs> TooltipBuild;
        protected void OnTooltipBuild(TooltipArgs a)
        {
            if (TooltipBuild != null)
                TooltipBuild(this, a);
        }
        //Texture2D Sprite;

        public Vector2 ScreenLocation
        {
            get
            {
                //return new Vector2(Math.Max(Math.Min(Controller.X + 16, Camera.Width - Sprite.Width), 0), Math.Max(Math.Min(Controller.Y, Camera.Height - Sprite.Height), 0));
                return new Vector2(Math.Max(Math.Min(Controller.Instance.msCurrent.X + 16, Game1.Instance.graphics.PreferredBackBufferWidth - Tooltip.Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y, Game1.Instance.graphics.PreferredBackBufferHeight - Tooltip.Height), 0));
            }
        }
        public void Draw(SpriteBatch sb)
        {
           // object mouseover = Controller.Instance.MouseoverNext.Object;
            Mouseover mouseover = Controller.Instance.MouseoverNext;
            if (Tooltip != null)
                Tooltip.Draw(sb);
           // Controller.Instance.MouseoverNext.Object = mouseover;
            //Controller.Instance.MouseoverNext = mouseover; // WHY DO I DO THIS?
        }

        internal static void OnGameEvent(GameEvent e)
        {
            if (Instance.Tooltip != null)
                Instance.Tooltip.OnGameEvent(e);
        }
    }
}
