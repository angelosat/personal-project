﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    public class TooltipManager
    {
        public static bool MouseTooltips = true;
        public static float DelayInterval = Engine.TicksPerSecond / 4;
        float DelayValue;
        public static int Width = 300;
        protected static TooltipManager _Instance;
        public static TooltipManager Instance => _Instance ??= new TooltipManager();

        Tooltip _Tooltip;
        public Tooltip Tooltip
        {
            get { return this._Tooltip; }
            set
            {
                if (this._Tooltip is not null)
                {
                    this._Tooltip.Dispose();
                }

                this._Tooltip = value;
            }
        }

        ITooltippable Object;


        public void Update()
        {
            if (this.Object is null)
                return;

            this.DelayValue -= 1;

            if (this.DelayValue <= 0)
            {
                this.DelayValue = DelayInterval;
                if (this.Tooltip is null)
                    this.Build();
                else
                    this.Tooltip.Update();
            }
            this.Tooltip?.Update();
        }

        void Build()
        {
            this.Tooltip = new(this.Object);
            this.Tooltip.AutoSize = true;
            this.Object.GetTooltipInfo(this.Tooltip);
            foreach (var comp in Game1.Instance.GameComponents)
            {
                comp.OnTooltipCreated(this.Object, this.Tooltip);
            }

            if (this.Tooltip.Controls.Count > 0)
            {
                this.Tooltip.Update();
                this.Tooltip.SetMousethrough(true, true);
            }
            else
                this.Tooltip = null;
        }

        void Object_TooltipChanged(object sender, EventArgs e)
        {
            this.Build();
        }

        TooltipManager()
        {
            Controller.MouseoverObjectChanged += new EventHandler<MouseoverEventArgs>(this.Controller_MouseoverObjectChanged);
        }

        void Controller_MouseoverObjectChanged(object sender, MouseoverEventArgs e)
        {
            this.Tooltip = null;
            ITooltippable obj = e.ObjectNext as ITooltippable;
            this.DelayValue = DelayInterval;
            this.Object = Controller.Instance.MouseoverBlockNext.Object as ITooltippable;//.Target;// 
        }

        public Vector2 ScreenLocation
        {
            get
            {
                return new(Math.Max(Math.Min(Controller.Instance.msCurrent.X + 16, Game1.Instance.graphics.PreferredBackBufferWidth - this.Tooltip.Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y, Game1.Instance.graphics.PreferredBackBufferHeight - this.Tooltip.Height), 0));
            }
        }
        public void Draw(SpriteBatch sb)
        {
            this.Tooltip?.Draw(sb);
        }

        internal static void OnGameEvent(GameEvent e)
        {
            Instance.Tooltip?.OnGameEvent(e);
        }
    }
}
