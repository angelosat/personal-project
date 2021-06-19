using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class ToolManagement : DefaultTool// ControlTool
    {
        //bool KeyDown = false;
        bool Up, Down, Left, Right;
        private DateTime MouseMiddleTimestamp;
        //bool MouseScrolling;
        Vector2 MouseScrollOrigin;
        Vector2 CameraCoordinatesOrigin;
        //Camera Camera { get { return Rooms.Ingame.Instance.Camera; } }
        //TargetArgs Selected;
        Action ScrollingMode;

        public ToolManagement()
        {
            //this.ScrollingMode = this.MouseScroll;
            //this.ScrollingMode = this.MouseDrag;
        }
        public override Icon GetIcon()
        {
            return null;// UI.Icon.Cursor;
        }
        TargetArgs Origin;
        Vector2? SelectionRectangleOrigin;
        //protected override void SwitchTool()
        //{
        //    return;
        //    Client.PlayerStopMoving();
        //    this.StopWalking();
        //    ToolManager.Instance.ActiveTool = null;
        //    ContextActionBar.Remove();
        //    Client.Console.Write("Switched to movement mode");
        //}

        public override void Update()
        {
            //var cam = ScreenManager.CurrentScreen.Camera;
            //var map = Rooms.Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map;
            var map = Rooms.Ingame.GetMap();
            var cam = map.Camera;
            //if(map != null)
                cam.MousePicking(map);
            //this.UpdateTarget();
            this.UpdateTargetNew();

            if (this.Origin != null && this.Target != null && this.Origin.Global != this.Target.Global)
            {
                ToolManager.SetTool(new ToolSelect(this.Origin));
                this.Origin = null;
                return;
            }
            if (this.SelectionRectangleOrigin.HasValue && //this.OriginRectangleSelection.Value != UIManager.Mouse)
                Vector2.DistanceSquared(this.SelectionRectangleOrigin.Value, UIManager.Mouse) > 50)
            {
                ToolManager.SetTool(new ToolSelectRectangle(this.SelectionRectangleOrigin.Value));
                //ToolManager.SetTool(new ToolSelectRectangle(this.SelectionRectangleOrigin.Value));

                this.SelectionRectangleOrigin = null;
                return;
            }
            if (this.ScrollingMode != null) //this.MouseScrolling)
            {
                this.ScrollingMode();
                //MouseScroll();
                //this.MouseDrag();
            }
            else
                this.MoveKeys();
            
            //this.Camera.Update(Rooms.Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);
            this.OnUpdate();
        }
        protected virtual void OnUpdate() { }

        int LastSpeed = 1;
        internal override void Jump()
        {
            //var prevSpeed = Client.Instance.Speed;
            //if (this.LastSpeed == 0 && Client.Instance.Speed == 0)
            //    this.LastSpeed = 1;
            //PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, this.LastSpeed);
            //this.LastSpeed = prevSpeed;
            int nextSpeed = Client.Instance.Speed == 0 ? this.LastSpeed : 0;
            if (Client.Instance.Speed != 0)
                this.LastSpeed = Client.Instance.Speed;
            PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, nextSpeed);
        }

        private void MouseScroll()
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.MouseScrollOrigin;
            //double x, y;
            //ScreenManager.CurrentScreen.Camera.Transform((int)delta.X, (int)delta.Y, out x, out y);
            //int roundx, roundy;
            //roundx = (int)Math.Round(x);
            //roundy = (int)Math.Round(y);
            //Vector2 nextStep = new Vector2(roundx, roundy);
            var l = delta.Length();
            if (l < 5)
                return;
            l -= 5;
            //l = (float)Math.Pow(l, 2);

            delta.Normalize();
            var minL = Math.Min(Math.Max(l, 1), 500);
            delta *= minL;

            delta *= .01f;
            var cam = Engine.Map.Camera;
            //this.Camera.Move(this.Camera.Coordinates += delta * 4);
            cam.Move(cam.Coordinates += delta * 4);

        }
        private void MouseDrag()
        {
            var currentMouse = UIManager.Mouse;
            var delta = currentMouse - this.MouseScrollOrigin;
            var map = Rooms.Ingame.GetMap();
            //var cam = Rooms.Ingame.CurrentMap.Camera;
            var cam = map.Camera;
            //this.Camera.Move(this.CameraCoordinatesOrigin - delta / this.Camera.Zoom);
            cam.Move(this.CameraCoordinatesOrigin - delta / cam.Zoom);

        }

        public override void MoveKeys()
        {
            //    if (e.KeyHandled)
            //if (e.Handled)
            //    return;
            int xx = 0, yy = 0;
            //if (input.IsKeyDown(GlobalVars.KeyBindings.North))

            if (Up)
            {
                yy -= 1;
            }
            else if (Down) ///GlobalVars.KeyBindings.South))
            {
                yy += 1;
            }
            if (Left)//GlobalVars.KeyBindings.East))
            {
                xx -= 1;
            }
            else if (Right)//GlobalVars.KeyBindings.West))
            {
                xx += 1;
            }
            // Walking.ToConsole();
            if (xx != 0 || yy != 0)
            {
                //PhysicsComponent physComp = Player.Actor.GetPhysics();
                //var cam = ScreenManager.CurrentScreen.Camera;
                //var cam = Net.Client.Instance.Map.Camera;
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

                //var global = Player.Actor == null ? Vector3.Zero : Player.Actor.Global;
                var speed = InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? 3 : 1;
                //this.Camera.Move(this.Camera.Coordinates += new Vector2(xx, yy) * 4 * speed);
                cam.Move(cam.Coordinates += new Vector2(xx, yy) * 4 * speed);

                //Vector3 direction = new Vector3(NextStep.X, NextStep.Y, Player.Actor.Velocity.Z);// posComp.GetProperty<Vector3>("Speed").Z);
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
                    //Client.PlayerSetSpeed(1);
                    PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, 1);
                    e.Handled = true;
                    break;

                case System.Windows.Forms.Keys.D2:
                    //Client.PlayerSetSpeed(2);
                    PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, 2);
                    e.Handled = true;
                    break;

                case System.Windows.Forms.Keys.D3:
                    PacketPlayerSetSpeed.Send(Client.Instance, Client.Instance.PlayerData.ID, 3);
                    //Client.PlayerSetSpeed(3);
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
            //KeyDown = false;
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

            //return;
            ////base.HandleKeyUp(e);
            //KeyControl key;
            //if (this.KeyControls.TryGetValue(e.KeyCode, out key))
            //    key.Up();
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
            //this.SelectEntity(this.Target ?? TargetArgs.Null);

            if (this.Target == null)
                return Messages.Default;
            this.LeftPressed = true;
            //return Messages.Default;
            //if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
            //    this.Origin = this.Target;
            //else
                this.SelectionRectangleOrigin = UIManager.Mouse;
            return Messages.Default;
        }
        bool DblClicked;
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if(DblClicked)
            {
                DblClicked = false;
                return base.MouseLeftUp(e);
            }

            //if (!this.SelectionRectangleOrigin.HasValue)
            //{
            if (!e.Handled && this.LeftPressed)
                if (this.Target.Type != TargetType.Null)
                    this.SelectEntity(this.Target);// ?? TargetArgs.Null);
            //}
            this.Origin = null;
            this.SelectionRectangleOrigin = null;
            this.LeftPressed = false;
            return base.MouseLeftUp(e);
        }
        private void SelectEntity(TargetArgs target)
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                UISelectedInfo.AddToSelection(target);
                //Hud.AddToSelection(target);
            else
                UISelectedInfo.Refresh(target);
                //Hud.Select(target);
        }

        public override ControlTool.Messages MouseMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if(this.ScrollingMode != MouseScroll)
                this.MouseMiddleTimestamp = DateTime.Now;
            this.ScrollingMode = MouseDrag;
            //this.MouseScrolling = true;
            this.MouseScrollOrigin = UIManager.Mouse;
            var map = Rooms.Ingame.GetMap();
            //var cam = Rooms.Ingame.CurrentMap.Camera;// Net.Client.Instance.Map.Camera;
            var cam = map.Camera;

            //this.CameraCoordinatesOrigin = this.Camera.Coordinates;
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
                //this.MouseScrolling = false;
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
                    //UISelectedInfo.SelectAllVisible(this.Target.Object.ID);
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

            //if(UISelectedInfo.ClearTargets())
            //    return Messages.Default;
           
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
                //UIForceTask.AddControlsBottomLeft(tasks.Select(t => t.GetControl()).ToArray());
                UIForceTask.AddControlsBottomLeft(tasks
                    //.Where(t=>t.Task.Def!=null)
                    .Select(t =>
                {
                    var task = t.Task;
                    var giver = t.Source;
                    return new Button(task.GetForceTaskText())
                    { 
                        LeftClickAction = ()=>
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
            //base.DrawAfterWorld(sb, map, cam);
            this.DrawBlockMouseover(sb, map, cam);

            if(Engine.DrawRegions)
            if (this.Target != null)
                if (this.Target.Type != TargetType.Null)
                {
                    map.Regions.Draw(this.Target.Global, sb, cam);
                }
            
        }
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            
            //if (this.MouseScrolling && this.ScrollingMode == this.MouseScroll)
                if (this.ScrollingMode == this.MouseScroll)
                    Icon.Cross.Draw(sb, this.MouseScrollOrigin, Vector2.One * .5f);
            base.DrawUI(sb, camera);
        }
        //internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        //{
        //    base.DrawUI(sb, camera);
        //    if (this.Selected != null)
        //    {
        //        if (this.Selected.Type == TargetType.Entity)
        //            Components.SpriteComponent.DrawHighlight(this.Selected.Object, sb, camera);
        //    }
        //}
    }
}
