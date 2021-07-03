using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class ToolManagement : DefaultTool
    {
        bool Up, Down, Left, Right;
        private DateTime MouseMiddleTimestamp;
        Vector2 MouseScrollOrigin;
        Vector2 CameraCoordinatesOrigin;
        Action ScrollingMode;

        public ToolManagement()
        {
        }
        public override Icon GetIcon()
        {
            return null;
        }
        TargetArgs Origin;
        Vector2? SelectionRectangleOrigin;

        public override void Update()
        {
            var map = Rooms.Ingame.GetMap();
            var cam = map.Camera;
            cam.MousePicking(map);
            this.UpdateTargetNew();

            if (this.Origin != null && this.Target != null && this.Origin.Global != this.Target.Global)
            {
                ToolManager.SetTool(new ToolSelect(this.Origin));
                this.Origin = null;
                return;
            }
            if (this.SelectionRectangleOrigin.HasValue &&
                Vector2.DistanceSquared(this.SelectionRectangleOrigin.Value, UIManager.Mouse) > 50)
            {
                ToolManager.SetTool(new ToolSelectRectangle(this.SelectionRectangleOrigin.Value));
                this.SelectionRectangleOrigin = null;
                return;
            }
            if (this.ScrollingMode != null)
            {
                this.ScrollingMode();
            }
            else
                this.MoveKeys();

            this.OnUpdate();
        }
        protected virtual void OnUpdate() { }

        int LastSpeed = 1;
        internal override void Jump()
        {
            int nextSpeed = Client.Instance.Speed == 0 ? this.LastSpeed : 0;
            if (Client.Instance.Speed != 0)
                this.LastSpeed = Client.Instance.Speed;
            PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, nextSpeed);
        }

        private void MouseScroll()
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.MouseScrollOrigin;
            var l = delta.Length();
            if (l < 5)
                return;
            l -= 5;

            delta.Normalize();
            var minL = Math.Min(Math.Max(l, 1), 500);
            delta *= minL;

            delta *= .01f;
            var cam = Engine.Map.Camera;
            cam.Move(cam.Coordinates += delta * 4);

        }
        private void MouseDrag()
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.MouseScrollOrigin;
            var map = Rooms.Ingame.GetMap();
            var cam = map.Camera;
            cam.Move(this.CameraCoordinatesOrigin - delta / cam.Zoom);
        }

        public override void MoveKeys()
        {
            int xx = 0, yy = 0;

            if (Up)
            {
                yy -= 1;
            }
            else if (Down)
            {
                yy += 1;
            }
            if (Left)
            {
                xx -= 1;
            }
            else if (Right)
            {
                xx += 1;
            }
            if (xx != 0 || yy != 0)
            {
                var cam = Rooms.Ingame.CurrentMap.Camera;

                double rx, ry;
                double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
                rx = (xx * cos - yy * sin);
                ry = (xx * sin + yy * cos);
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                Vector2 NextStep = new Vector2(roundx, roundy);
                NextStep.Normalize();

                var speed = InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? 3 : 1;
                cam.Move(cam.Coordinates += new Vector2(xx, yy) * 4 * speed);
            }
            else
            {
            }
        }
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.D1:
                    PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, 1);
                    e.Handled = true;
                    break;

                case System.Windows.Forms.Keys.D2:
                    PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, 2);
                    e.Handled = true;
                    break;

                case System.Windows.Forms.Keys.D3:
                    PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, 3);
                    e.Handled = true;
                    break;

                case System.Windows.Forms.Keys.F7:
                    Engine.DrawRegions = !Engine.DrawRegions;
                    e.Handled = true;
                    break;

                default:
                    break;
            }

            if (e.KeyCode == GlobalVars.KeyBindings.North || e.KeyCode == System.Windows.Forms.Keys.Up)
            {
                e.Handled = true;
                Up = true;
            }
            if (e.KeyCode == GlobalVars.KeyBindings.South || e.KeyCode == System.Windows.Forms.Keys.Down)
            {
                e.Handled = true;
                Down = true;
            }
            if (e.KeyCode == GlobalVars.KeyBindings.West || e.KeyCode == System.Windows.Forms.Keys.Left)
            {
                e.Handled = true;
                Left = true;
            }
            if (e.KeyCode == GlobalVars.KeyBindings.East || e.KeyCode == System.Windows.Forms.Keys.Right)
            {
                e.Handled = true;
                Right = true;
            }

            KeyControl key;
            if (this.KeyControls.TryGetValue(e.KeyCode, out key))
            {
                key.Down();
                e.Handled = true;
            }
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.W:
                    Up = false;
                    break;
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.A:
                    Left = false;
                    break;
                case System.Windows.Forms.Keys.Right:
                case System.Windows.Forms.Keys.D:
                    Right = false;
                    break;
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.S:
                    Down = false;
                    break;
                default:
                    break;
            }
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleMouseWheel(e);
            var map = Rooms.Ingame.GetMap();
            var cam = map.Camera;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
            {
                cam.AdjustDrawLevel(InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta);
                return;
            }
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            {
                cam.Rotation += e.Delta;
                return;
            }
            if (e.Delta < 0)
                cam.ZoomDecrease();
            else
                cam.ZoomIncrease();

        }
        bool LeftPressed;
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.LeftPressed = true;
            this.SelectionRectangleOrigin = UIManager.Mouse;
            return Messages.Default;
        }
        bool DblClicked;
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (DblClicked)
            {
                DblClicked = false;
                return base.MouseLeftUp(e);
            }
            if (!e.Handled && this.LeftPressed)
                if (this.Target.Type != TargetType.Null)
                    this.SelectEntity(this.Target);
            this.Origin = null;
            this.SelectionRectangleOrigin = null;
            this.LeftPressed = false;
            return base.MouseLeftUp(e);
        }
        private void SelectEntity(TargetArgs target)
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                UISelectedInfo.AddToSelection(target);
            else
                UISelectedInfo.Refresh(target);
        }

        public override ControlTool.Messages MouseMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ScrollingMode != MouseScroll)
                this.MouseMiddleTimestamp = DateTime.Now;
            this.ScrollingMode = MouseDrag;
            this.MouseScrollOrigin = UIManager.Mouse;
            var map = Rooms.Ingame.GetMap();
            var cam = map.Camera;
            this.CameraCoordinatesOrigin = cam.Coordinates;
            return Messages.Default;
        }
        public override ControlTool.Messages MouseMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var d = DateTime.Now - this.MouseMiddleTimestamp;
            var c = TimeSpan.FromMilliseconds(100);
            if (d < c && this.ScrollingMode != MouseScroll)
                this.ScrollingMode = MouseScroll;
            else
                this.ScrollingMode = null;
            return Messages.Default;
        }
        public override ControlTool.Messages MouseMiddle()
        {
            return base.MouseMiddle();
        }
        public override void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target != null)
            {
                if (this.Target.Type == TargetType.Entity)
                    UISelectedInfo.SelectAllVisible(this.Target.Object.Def);

                // TODO: drawing multiple block selection textures is slow, need to optimize
                else if (this.Target.Type == TargetType.Position)
                    ToolManager.SetTool(
                        new ToolSelectRectangleBlocks(this.Target.Global,
                        (a, b, r) =>
                        {
                            if (a == b)
                                UISelectedInfo.Refresh(this.Target);
                            else
                                UISelectedInfo.Refresh(new BoundingBox(a, b).GetBox().Select(t => new TargetArgs(Rooms.Ingame.GetMap(), t)));
                        }));
            }
            DblClicked = true;
            e.Handled = true;
        }
        internal override void SlotLeftClick(GameObjectSlot slot)
        {
            if (slot.Object != null)
                WindowTargetManagement.Refresh(new TargetArgs(slot.Object));
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;

            if (!TryShowForceTaskGUI(this.Target))
                Rooms.Ingame.CurrentMap.Town.ToggleQuickMenu();

            e.Handled = true;
            return Messages.Default;
        }

        private bool TryShowForceTaskGUI(TargetArgs target)
        {
            var actor = UISelectedInfo.GetSingleSelectedEntity() as Actor;

            if (!(actor?.IsCitizen ?? false))
                return false;

            var tasks = actor.GetPossibleTasksOnTarget(target);
            if (tasks?.Any() ?? false)
            {
                UIForceTask.ClearControls();
                UIForceTask.AddControlsBottomLeft(tasks
                    .Select(t =>
                    {
                        var task = t.Task;
                        var giver = t.Source;
                        return new Button(task.GetForceTaskText())
                        {
                            LeftClickAction = () =>
                            {
                                PacketForceTask.Send(giver, actor, target);
                                UIForceTask.Hide();
                            }
                        };

                    }).ToArray());

                UIForceTask.Location = UIManager.Mouse;
                UIForceTask.Show();
                return true;
            }
            else
            {
                UIForceTask?.Hide();
                return false;
            }
        }

        static Control UIForceTask = new Panel() { AutoSize = true };

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            this.DrawBlockMouseover(sb, map, cam);

            if (Engine.DrawRegions)
                if (this.Target != null)
                    if (this.Target.Type != TargetType.Null)
                    {
                        map.Regions.Draw(this.Target.Global, sb, cam);
                    }

        }
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            if (this.ScrollingMode == this.MouseScroll)
                Icon.Cross.Draw(sb, this.MouseScrollOrigin, Vector2.One * .5f);
            base.DrawUI(sb, camera);
        }
    }
}
