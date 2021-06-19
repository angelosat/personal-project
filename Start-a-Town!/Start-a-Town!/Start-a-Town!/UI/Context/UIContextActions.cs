using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class UIContextActions : Panel// GroupBox
    {
        //int TimerMax = Engine.TargetFps;
        //int Timer = 0;
        //TargetArgs TargetLast, Target;
        static Vector2 Offset = new Vector2(16, 0);

        public UIContextActions()
        {
            this.AutoSize = true;
            this.BackgroundStyle = BackgroundStyle.Tooltip;
            this.MouseThrough = true;
            this.Color = Color.White;
            //this.BackgroundColor = Color.Black * 0.8f;
        }

        public override void Update()
        {
            base.Update();

            var current = ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target ?? TargetArgs.Empty;
            var last = ScreenManager.CurrentScreen.ToolManager.ActiveTool.TargetLast ?? TargetArgs.Empty; 

            if(current != null && last == null)
            {
                this.Refresh();
                return;
            }
            if(current == null)
            {
                this.Controls.Clear();
                return;
            }
            //if (current.Object != last.Object )//||
            //    //current.Global != last.Global)
            //    this.Refresh();
            if (current.Object != last.Object ||
                current.Global != last.Global)
                this.Refresh(current);
            else
                this.Refresh();
        }

        public void Refresh()
        {
            foreach (var k in this.Labels.ToList())
            {
                var a = k.Key;
                var c = k.Value;
                var valid = a.Available();
                var active = this.Controls.Contains(c);
                if(valid)
                {
                    if (!active)
                    {
                        //this.Controls.Add(c);
                        //this.AlignTopToBottom();
                        this.Refresh(ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target);
                        this.Invalidate();
                    }
                }
                else
                {
                    if (active)
                    {
                        //this.Controls.Remove(c);
                        //this.AlignTopToBottom();
                        this.Refresh(ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target);
                        this.Invalidate();
                    }
                }
            }
        }

        Dictionary<ContextAction, Label> Labels = new Dictionary<ContextAction, Label>();
        bool ShowUnavailableActions = false;
        public void RefreshTest(TargetArgs target)
        {
            this.Labels.Clear();
            this.BackgroundStyle = BackgroundStyle.Tooltip;

            if (Player.Actor == null)
                return;
            if (Player.Actor.Net == null)
                return;

            if (target == null)
                target = TargetArgs.Empty;
            this.Controls.Clear();
            var actions = target.GetContextActionsFromInput();
            foreach (var a in actions)
            {
                var l = new Label(a.Key.ToString() + ": " + a.Value.Name()) { Location = this.Controls.BottomLeft, MouseThrough = true, Tag = a };
                if (a.Value.Available())
                    this.Controls.Add(l);
                this.Labels.Add(a.Value, l);
            }


            //var args = new ContextArgs();
            //target.GetContextActions(args);
            //foreach (var a in args.Actions)
            //{
            //    var l = new Label(a.Name()) { Location = this.Controls.BottomLeft, MouseThrough = true, Tag = a };
            //    if (a.Available())
            //        this.Controls.Add(l);
            //    this.Labels.Add(a, l);
            //}
            this.Location = new Microsoft.Xna.Framework.Vector2(UIManager.Width * 3 / 5, UIManager.Height / 2 - this.Height / 2);
            this.Invalidate();

        }
        public void Refresh(TargetArgs target)
        {
            this.Labels.Clear();
            this.BackgroundStyle = BackgroundStyle.Tooltip;

            if (Player.Actor == null)
                return;
            if (Player.Actor.Net == null)
                return;

            if (target == null)
                target = TargetArgs.Empty;
            this.Controls.Clear();
            //var actions = target.GetContextActionsWorld(Player.Actor);

            var args = new ContextArgs();
            target.GetContextAll(args);
            foreach (var a in args.Actions)
            {
                var l = new Label(a.ToString()) { Location = this.Controls.BottomLeft, MouseThrough = true, Tag = a };
                if (a.Available())
                    this.Controls.Add(l);
                this.Labels.Add(a, l);
            }
            this.Location = new Microsoft.Xna.Framework.Vector2(UIManager.Width * 3 / 5, UIManager.Height / 2 - this.Height / 2);
            this.Invalidate();

        }
        public void RefreshOld()
        {
            if (Player.Actor == null)
                return;
            if (Player.Actor.Net == null)
                return;

            var target = ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target;
            if (target == null)
                target = TargetArgs.Empty;
            this.Controls.Clear();
            var interactions = target.GetContextActionsWorld(Player.Actor);

            var args = new ContextArgs();
            target.GetContextActions(args);
            foreach(var a in args.Actions)
                this.Controls.Add(new Label(a.Name()) { Location = this.Controls.BottomLeft, MouseThrough = true });
            //foreach (var input in PlayerInput.DefaultInputs)
            //{
            //    var i = input.Value(Player.Actor, target);
            //    if (i == null)
            //        continue;
            //    var available = i.AvailabilityCondition(Player.Actor, target);
            //    if (available || (!available && this.ShowUnavailableActions))
            //        this.Controls.Add(new Label(input.Key.ToString() + ": " + i.Name) { Location = this.Controls.BottomLeft, MouseThrough = true, TextColorFunc = () => available ? Color.White : Color.Maroon });
            //}
            this.Location = new Microsoft.Xna.Framework.Vector2(UIManager.Width * 3 / 5, UIManager.Height / 2 - this.Height / 2);
        }
        public void RefreshOlder()
        {
            if (Player.Actor == null)
                return;

            var target = ScreenManager.CurrentScreen.ToolManager.ActiveTool.Target;
            if (target == null)
                //return;
                target = TargetArgs.Empty;
            this.Controls.Clear();
            var interactions = target.GetContextActionsWorld(Player.Actor);// Rooms.Ingame.GetNet());//Net.Client.Instance);
            if (interactions != null)
                foreach (var i in interactions)
                {
                    var available = i.Value.AvailabilityCondition(Player.Actor, target);
                    //if (available || (!available && this.ShowUnavailableActions))
                    //    this.Controls.Add(new Label(i.Key.ToString() + ": " + i.Value.Name) { Location = this.Controls.BottomLeft, MouseThrough = true, TextColorFunc = () => available ? Color.White : Color.Maroon });
                }
            foreach(var input in PlayerInput.DefaultInputs)
            {
                //if (!input.Value.AvailabilityCondition(Player.Actor, target))
                //    continue;
                //var i = input.Value;

                // workarounds until i tidy upthe situation where i store the player character before it's been sent back instantiated by the server
                if (Player.Actor == null)
                    return;
                if (Player.Actor.Net == null)
                    return;

                var i = input.Value(Player.Actor, target);
                if (i == null)
                    continue;
                var available = i.AvailabilityCondition(Player.Actor, target);
                if (available || (!available && this.ShowUnavailableActions))
                    this.Controls.Add(new Label(input.Key.ToString() + ": " + i.Name) { Location = this.Controls.BottomLeft, MouseThrough = true, TextColorFunc = () => available ? Color.White : Color.Maroon });
            }
            //foreach (var i in PlayerInput.KeyBindings.Keys.Except(interactions.Keys.Select(k => k.Action)))
            //{
            //    var def = PlayerInput.GetDefaultAction(i);
            //    if (def == null)
            //        continue;               
            //    if (!def.AvailabilityCondition(Player.Actor, target))
            //        continue;
            //    var ac = PlayerInput.GetKey(i);
            //    this.Controls.Add(new Label(ac.ToString() + ": " + def.Name) { Location = this.Controls.BottomLeft, MouseThrough = true });
            //}
            this.Location = new Microsoft.Xna.Framework.Vector2(UIManager.Width * 3 / 5, UIManager.Height / 2 - this.Height / 2);
        }

        public override Vector2 ScreenLocation
        {
            get
            {
                if(TooltipManager.Instance.Tooltip == null)
                return new Vector2(Math.Max(Math.Min(Controller.Instance.msCurrent.X / UIManager.Scale + Offset.X, UIManager.Width - Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y / UIManager.Scale + Offset.Y, UIManager.Height - Height), 0));
                return TooltipManager.Instance.Tooltip.ScreenLocation + TooltipManager.Instance.Tooltip.BottomLeft;
            }
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            //this.Location = UIManager.Mouse + Vector2.UnitX * 16;
            if (this.Controls.Count == 0)
                return;
            BackgroundStyle.Tooltip.Draw(sb, ScreenBounds, Color.White, 1);
            foreach (var c in this.Controls)
                c.Draw(sb, viewport);
            //return;
            //base.Draw(sb, viewport);
        }
    }
}
