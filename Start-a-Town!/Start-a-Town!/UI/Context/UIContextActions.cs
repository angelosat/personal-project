using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class UIContextActions : Panel
    {
        static Vector2 Offset = new Vector2(16, 0);

        public UIContextActions()
        {
            this.AutoSize = true;
            this.BackgroundStyle = BackgroundStyle.Tooltip;
            this.MouseThrough = true;
            this.Color = Color.White;
        }

        public override void Update()
        {
            base.Update();

            var current = ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target ?? TargetArgs.Null;
            var last = ScreenManager.CurrentScreen.ToolManager.ActiveTool.TargetLast ?? TargetArgs.Null;

            if (current is not null && last is null)
            {
                this.Refresh();
                return;
            }
            if (current is null)
            {
                this.Controls.Clear();
                return;
            }
            if (current.Object is null)
            {
                this.Controls.Clear();
                return;
            }
            if (current.Object != last.Object ||
                current.Global != last.Global)
                this.Refresh(current);
            else
                this.Refresh();
        }

        public new void Refresh()
        {
            foreach (var k in this.Labels.ToList())
            {
                var a = k.Key;
                var c = k.Value;
                var valid = a.Available();
                var active = this.Controls.Contains(c);
                if (valid)
                {
                    if (!active)
                    {
                        this.Refresh(ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target);
                        this.Invalidate();
                    }
                }
                else
                {
                    if (active)
                    {
                        this.Refresh(ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target);
                        this.Invalidate();
                    }
                }
            }
        }

        Dictionary<ContextAction, Label> Labels = new Dictionary<ContextAction, Label>();
        public void Refresh(TargetArgs target)
        {
            this.Labels.Clear();
            this.BackgroundStyle = BackgroundStyle.Tooltip;

            if (PlayerOld.Actor == null)
                return;
            if (PlayerOld.Actor.Net == null)
                return;

            if (target == null)
                target = TargetArgs.Null;
            this.Controls.Clear();

            var args = new ContextArgs();
            ScreenManager.CurrentScreen.ToolManager.ActiveTool.GetContextActions(args);
            foreach (var a in args.Actions)
            {
                var l = new Label(a.ToString()) { Location = this.Controls.BottomLeft, MouseThrough = true, Tag = a };
                if (a.Available())
                    this.Controls.Add(l);
                this.Labels.Add(a, l);
            }
            this.Location = new Vector2(UIManager.Width * 3 / 5, UIManager.Height / 2 - this.Height / 2);
            this.Invalidate();

        }

        public override Vector2 ScreenLocation
        {
            get
            {
                if (TooltipManager.Instance.Tooltip == null)
                    return new Vector2(Math.Max(Math.Min(Controller.Instance.msCurrent.X / UIManager.Scale + Offset.X, UIManager.Width - Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y / UIManager.Scale + Offset.Y, UIManager.Height - Height), 0));
                return TooltipManager.Instance.Tooltip.ScreenLocation + TooltipManager.Instance.Tooltip.BottomLeft;
            }
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            if (this.Controls.Count == 0)
                return;
            BackgroundStyle.Tooltip.Draw(sb, BoundsScreen, Color.White, 1);
            foreach (var c in this.Controls)
                c.Draw(sb, viewport);
        }
    }
}
