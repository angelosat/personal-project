using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Start_a_Town_
{
    [EnsureInit]
    public class ToolManagement : DefaultTool
    {
        bool Up, Down, Left, Right;
        private DateTime MouseMiddleTimestamp;
        Vector2 MouseScrollOrigin;
        Vector2 CameraCoordinatesOrigin;
        Action ScrollingMode;
        static readonly HotkeyContext HotkeyContext = new("Management");
        static ToolManagement()
        {
            HotkeyManager.RegisterHotkey(HotkeyContext, "Pause/Resume", PauseResume, Keys.Space);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Speed: Normal", delegate { SetSpeed(1); }, Keys.D1);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Speed: Fast", delegate { SetSpeed(2); }, Keys.D2);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Speed: Faster", delegate { SetSpeed(3); }, Keys.D3);
            HotkeyManager.RegisterHotkey(HotkeyContext, "Toggle Forbidden", delegate { }, Keys.F);
        }
        public ToolManagement()
        {
        }
        public override Icon GetIcon()
        {
            return null;
        }
        TargetArgs Origin;
        Vector2? SelectionRectangleOrigin;
        bool LeftPressed, DblClicked;

        public override void Update()
        {
            var map = Ingame.GetMap();
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

        static int LastSpeed = 1;
        internal override void Jump()
        {
            PauseResume();
        }

        static void PauseResume()
        {
            int nextSpeed = Client.Instance.Speed == 0 ? LastSpeed : 0;
            if (Client.Instance.Speed != 0)
                LastSpeed = Client.Instance.Speed;
            PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, nextSpeed);
        }

        private void SelectEntity(TargetArgs target)
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                UISelectedInfo.AddToSelection(target);
            else
                UISelectedInfo.Refresh(target);
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
            var map = Ingame.GetMap();
            var cam = map.Camera;
            cam.Move(this.CameraCoordinatesOrigin - delta / cam.Zoom);
        }

        public override void MoveKeys()
        {
            int xx = 0, yy = 0;

            if (this.Up)
            {
                yy -= 1;
            }
            else if (this.Down)
            {
                yy += 1;
            }
            if (this.Left)
            {
                xx -= 1;
            }
            else if (this.Right)
            {
                xx += 1;
            }
            if (xx != 0 || yy != 0)
            {
                var cam = Ingame.CurrentMap.Camera;

                double rx, ry;
                double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
                rx = xx * cos - yy * sin;
                ry = xx * sin + yy * cos;
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                var nextStep = new Vector2(roundx, roundy);
                nextStep.Normalize();

                var speed = InputState.IsKeyDown(Keys.ShiftKey) ? 3 : 1;
                cam.Move(cam.Coordinates += new Vector2(xx, yy) * 4 * speed);
            }
        }
        public override void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            HotkeyManager.PerformHotkey(HotkeyContext, e.KeyCode);
        
            if (e.KeyCode == GlobalVars.KeyBindings.North || e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                this.Up = true;
            }
            if (e.KeyCode == GlobalVars.KeyBindings.South || e.KeyCode == System.Windows.Forms.Keys.Down)
            {
                e.Handled = true;
                this.Down = true;
            }
            if (e.KeyCode == GlobalVars.KeyBindings.West || e.KeyCode == System.Windows.Forms.Keys.Left)
            {
                e.Handled = true;
                this.Left = true;
            }
            if (e.KeyCode == GlobalVars.KeyBindings.East || e.KeyCode == System.Windows.Forms.Keys.Right)
            {
                e.Handled = true;
                this.Right = true;
            }

            if (this.KeyControls.TryGetValue(e.KeyCode, out KeyControl key))
            {
                key.Down();
                e.Handled = true;
            }
        }


        private static void SetSpeed(int value)
        {
            PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, value);
        }

        public override void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    this.Up = false;
                    break;
                case Keys.Left:
                case Keys.A:
                    this.Left = false;
                    break;
                case Keys.Right:
                case Keys.D:
                    this.Right = false;
                    break;
                case Keys.Down:
                case Keys.S:
                    this.Down = false;
                    break;
                default:
                    break;
            }
        }
        public override void HandleMouseWheel(HandledMouseEventArgs e)
        {
            base.HandleMouseWheel(e);
            var map = Ingame.GetMap();
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
        public override Messages MouseLeftPressed(HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.LeftPressed = true;
            this.SelectionRectangleOrigin = UIManager.Mouse;
            return Messages.Default;
        }
        public override Messages MouseLeftUp(HandledMouseEventArgs e)
        {
            if (this.DblClicked)
            {
                this.DblClicked = false;
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
        public override Messages MouseMiddleDown(HandledMouseEventArgs e)
        {
            if (this.ScrollingMode != this.MouseScroll)
                this.MouseMiddleTimestamp = DateTime.Now;
            this.ScrollingMode = this.MouseDrag;
            this.MouseScrollOrigin = UIManager.Mouse;
            var map = Ingame.GetMap();
            var cam = map.Camera;
            this.CameraCoordinatesOrigin = cam.Coordinates;
            return Messages.Default;
        }
        public override Messages MouseMiddleUp(HandledMouseEventArgs e)
        {
            var d = DateTime.Now - this.MouseMiddleTimestamp;
            var c = TimeSpan.FromMilliseconds(100);
            if (d < c && this.ScrollingMode != this.MouseScroll)
                this.ScrollingMode = this.MouseScroll;
            else
                this.ScrollingMode = null;
            return Messages.Default;
        }
        public override Messages MouseMiddle()
        {
            return base.MouseMiddle();
        }
        public override Messages MouseRightDown(HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;

            if (!this.TryShowForceTaskGUI(this.Target))
                Ingame.CurrentMap.Town.ToggleQuickMenu();

            e.Handled = true;
            return Messages.Default;
        }
        public override Messages MouseRightUp(HandledMouseEventArgs e)
        {
            return Messages.Default;
        }

        public override void HandleLButtonDoubleClick(HandledMouseEventArgs e)
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
                                UISelectedInfo.Refresh(new BoundingBox(a, b).GetBox().Select(t => new TargetArgs(Ingame.GetMap(), t)));
                        }));
            }
            this.DblClicked = true;
            e.Handled = true;
        }
        internal override void SlotLeftClick(GameObjectSlot slot)
        {
            if (slot.Object != null)
                WindowTargetManagement.Refresh(new TargetArgs(slot.Object));
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
                        return new UI.Button(task.GetForceTaskText())
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

        static readonly UI.Control UIForceTask = new UI.Panel() { AutoSize = true };

        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
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
