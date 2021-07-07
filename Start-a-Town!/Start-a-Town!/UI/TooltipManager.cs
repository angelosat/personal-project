using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class TooltipArgs : EventArgs
    {
        public Tooltip Tooltip;
        public TooltipArgs(Tooltip tooltip)
        {
            Tooltip = tooltip;
        }
    }

    public class TooltipManager
    {
        static public bool MouseTooltips = true;
        static public float DelayInterval = Engine.TicksPerSecond / 4;
        float DelayValue;
        public static int Width = 300;
        protected static TooltipManager _Instance;
        public static TooltipManager Instance => _Instance ??= new TooltipManager();

        Tooltip _Tooltip;
        public Tooltip Tooltip
        {
            get { return _Tooltip; }
            set
            {
                if (_Tooltip is not null)
                    _Tooltip.Dispose();
                _Tooltip = value;
            }
        }

        ITooltippable Object;


        public void Update()
        {

            if (this.Object is null)
                return;
            this.DelayValue -= 1;

            if (DelayValue <= 0)
            {
                DelayValue = DelayInterval;
                if (this.Tooltip is null)
                    Build();
                else
                {
                    this.Tooltip.Update();
                }
            }
            if (this.Tooltip != null)
                this.Tooltip.Update();
        }

        void Build()
        {
            Tooltip = new Tooltip(Object);
            Tooltip.AutoSize = true;
            Object.GetTooltipInfo(Tooltip);
            foreach (var comp in Game1.Instance.GameComponents)
                comp.OnTooltipCreated(this.Object, this.Tooltip);

            if (Tooltip.Controls.Count > 0)
            {
                this.Tooltip.Update();
                Tooltip.SetMousethrough(true, true);
            }
            else
                Tooltip = null;
        }

        void Object_TooltipChanged(object sender, EventArgs e)
        {
            Build();
        }

        TooltipManager()
        {
            Controller.MouseoverObjectChanged += new EventHandler<MouseoverEventArgs>(Controller_MouseoverObjectChanged);
        }

        void Controller_MouseoverObjectChanged(object sender, MouseoverEventArgs e)
        {
            Tooltip = null;
            ITooltippable obj = e.ObjectNext as ITooltippable;
            this.DelayValue = DelayInterval;
            Object = Controller.Instance.MouseoverBlockNext.Object as ITooltippable;//.Target;// 
        }

        public Vector2 ScreenLocation
        {
            get
            {
                return new Vector2(Math.Max(Math.Min(Controller.Instance.msCurrent.X + 16, Game1.Instance.graphics.PreferredBackBufferWidth - Tooltip.Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y, Game1.Instance.graphics.PreferredBackBufferHeight - Tooltip.Height), 0));
            }
        }
        public void Draw(SpriteBatch sb)
        {
            Tooltip?.Draw(sb);
        }

        internal static void OnGameEvent(GameEvent e)
        {
            Instance.Tooltip?.OnGameEvent(e);
        }
    }
}
