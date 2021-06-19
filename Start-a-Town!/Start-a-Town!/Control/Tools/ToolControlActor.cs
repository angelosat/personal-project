using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_
{
    class ToolControlActor : ControlTool
    {
        //private GameObject Actor;
        bool Up, Down, Left, Right, Walking;
        private bool MoveToggle;
        private bool MouseMove = true;
        //Camera Camera { get { return Rooms.Ingame.Instance.Camera; } }
        public ToolControlActor()
        {

        }
        public override void Update()
        {
            base.Update();
            if (MoveToggle)
                MoveMouse();
            else
                MoveKeys();

            //foreach (var key in this.KeyControls.Values)
            //    key.Update();


            // this must be called for mouse picking to work, because the call to mousepicking is in the camera update method
            // maybe put this in the base update? or seperate them?
            var global = PlayerOld.Actor == null ? Vector3.Zero : PlayerOld.Actor.Global;

            //var cam = ScreenManager.CurrentScreen.Camera;
            //cam.Update(Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map, global);

        }
        public void MoveMouse()
        {
            Walking = true;
            Vector3 final = GetDirection3();
            //Player.Actor.PostMessage(Message.Types.Move, null, final, 1f);
            this.ChangeDirection(final);
        }
        public void ChangeDirection(Vector3 direction)
        {
            PacketPlayerInput.Send(Net.Client.Instance, direction.XY());
            //Client.PlayerChangeDirection(direction);
        }
        public static Vector3 GetDirection3()
        {
            var cam = Net.Client.Instance.GetPlayer().ControllingEntity.Map.Camera;
            //var playerScreenPosition = Rooms.Ingame.Instance.Camera.GetScreenPosition(Net.Client.Instance.GetPlayer().Character.Global);
            var playerScreenPosition = cam.GetScreenPosition(Net.Client.Instance.GetPlayer().ControllingEntity.Global);

            //int x = Controller.Instance.msCurrent.X - Rooms.Ingame.Instance.Camera.Width / 2;
            //int y = Controller.Instance.msCurrent.Y - Rooms.Ingame.Instance.Camera.Height / 2;
            int x = Controller.Instance.msCurrent.X - (int)playerScreenPosition.X;
            int y = Controller.Instance.msCurrent.Y - (int)playerScreenPosition.Y;
            //float x, y;
            //Coords.Rotate(ScreenManager.CurrentScreen.Camera.Rotation, Controller.Instance.msCurrent.X - ScreenManager.CurrentScreen.Camera.Width / 2, Controller.Instance.msCurrent.Y - ScreenManager.CurrentScreen.Camera.Height / 2, out x, out y);//
            float xx, yy;

            int xxx, yyy;

            Coords.Ortho(x, y, out xx, out yy);
            //Coords.Rotate((int)ScreenManager.CurrentScreen.Camera.Rotation, xx, yy, out xxx, out yyy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out xxx, out yyy);

            Vector2 normal = new Vector2(xxx, yyy);

            normal.Normalize();
            Vector3 final = new Vector3(normal.X, normal.Y, 0);
            return final;
        }
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {

            if (e.Handled)
                return;
            if (e.KeyCode == GlobalVars.KeyBindings.North || e.KeyCode == System.Windows.Forms.Keys.Up)
                PressUp();
            if (e.KeyCode == GlobalVars.KeyBindings.South || e.KeyCode == System.Windows.Forms.Keys.Down)
                PressDown();
            if (e.KeyCode == GlobalVars.KeyBindings.West || e.KeyCode == System.Windows.Forms.Keys.Left)
                PressLeft();
            if (e.KeyCode == GlobalVars.KeyBindings.East || e.KeyCode == System.Windows.Forms.Keys.Right)
                PressRight();
            //if (e.KeyCode == GlobalVars.KeyBindings.Attack)
            //    StartAttack();
            //if (e.KeyCode == GlobalVars.KeyBindings.Block)
            //    StartBlock();

            if (e.KeyCode == GlobalVars.KeyBindings.RunWalk)
                PacketPlayerToggleWalk.Send(Net.Client.Instance, true);
            if (e.KeyCode == GlobalVars.KeyBindings.Sprint)
                PacketPlayerToggleSprint.Send(Net.Client.Instance, true);
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            //Moving = false;
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
                //case System.Windows.Forms.Keys.Z:
                //    FinishAttack();
                //    break;
                //case System.Windows.Forms.Keys.X:
                //    FinishBlock();
                    //break;
                default:
                    break;
            }
            if (!(Up || Down || Left || Right))
                StopWalking();

            if (e.KeyCode == GlobalVars.KeyBindings.RunWalk)
                PacketPlayerToggleWalk.Send(Net.Client.Instance, false);
            if (e.KeyCode == GlobalVars.KeyBindings.Sprint)
                PacketPlayerToggleSprint.Send(Net.Client.Instance, false);

            if (e.KeyCode == System.Windows.Forms.Keys.M)
            {
                this.MouseMove = !this.MouseMove;
                //Rooms.Ingame.Instance.Hud.Chat.Write("Mouse move " + (this.MouseMove ? "Enabled" : "Disabled"));
                Rooms.Ingame.Instance.Hud.Chat.Write("Mouse move " + (this.MouseMove ? "Enabled" : "Disabled"));
            }

            base.HandleKeyUp(e);
            //KeyControl key;
            //if (this.KeyControls.TryGetValue(e.KeyCode, out key))
            //    key.Up();
        }

        private void PressRight()
        {
            this.Right = true;
        }

        private void PressLeft()
        {
            this.Left = true;
        }

        private void PressDown()
        {
            this.Down = true;
        }

        private void PressUp()
        {
            this.Up = true;
        }

        public virtual void MoveKeys()
        {
            int xx = 0, yy = 0;

            if (Up)
            {
                xx -= 1;
                yy -= 1;
            }
            else if (Down)
            {
                yy += 1;
                xx += 1;
            }
            if (Left)
            {
                yy += 1;
                xx -= 1;
            }
            else if (Right)
            {
                yy -= 1;
                xx += 1;
            }
            if (xx != 0 || yy != 0)
            {
                //var cam = ScreenManager.CurrentScreen.Camera;
                
                var cam = Rooms.Ingame.CurrentMap.Camera; // Player.Actor.Map.Camera;
                double rx, ry;
                double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
                rx = (xx * cos - yy * sin);
                ry = (xx * sin + yy * cos);
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                Vector2 nextStep = new Vector2(roundx, roundy);
                nextStep.Normalize();
                //Vector3 direction = new Vector3(nextStep.X, nextStep.Y, Player.Actor.Velocity.Z);// posComp.GetProperty<Vector3>("Speed").Z);
                PacketPlayerInput.Send(Net.Client.Instance, nextStep);
                //Client.PlayerChangeDirection(direction);
                if (!Walking)
                    StartWalking();
                Walking = true;

            }
            else
            {
                StopWalking();
            }
        }
        public void StartWalking()
        {
            PacketPlayerToggleMove.Send(Net.Client.Instance, true);
            //Client.PlayerStartMoving();
        }
        protected void StopWalking()
        {
            if (!Walking)
                return;
            if (MoveToggle)
                return;

            //Client.PlayerStopMoving();

            PacketPlayerToggleMove.Send(Net.Client.Instance, false);

            Walking = false;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;

            if (this.MouseMove)
            {
                MoveToggle = true;
                StartWalking();
            }
            else
                this.StartAttack();
            return Messages.Default;
        }

        private void StartAttack()
        {
            throw new NotImplementedException();
        }
        internal override void Jump()
        {
            PacketPlayerJump.Send(Net.Client.Instance);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            PacketControlNpc.Send(Net.Client.Instance, -1);
            //return Messages.Remove;
            return base.MouseRightDown(e);
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleMouseWheel(e);
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
            {
                var delta = InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta;
                var camera = Net.Client.Instance.Map.Camera;
                //this.Camera.AdjustDrawLevel(delta);
                camera.AdjustDrawLevel(delta);
                e.Handled = true;
                return;
            }
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.PlayerControlNpc:
                    if (e.Parameters[0] == Net.Client.Instance.GetPlayer())
                    {
                        var entity = e.Parameters[1] as GameObject;
                        //Rooms.Ingame.Instance.Camera.ToggleFollowing(entity);
                        Net.Client.Instance.Map.Camera.ToggleFollowing(entity);
                        if (entity == null)
                            ToolManager.SetTool(null);//e.Parameters[1] as GameObject));
                    }
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.MouseMove)
            {
                MoveToggle = false;
                StopWalking();
            }
            else
                this.FinishAttack();
            return base.MouseLeftUp(e);
        }

        private void FinishAttack()
        {
            throw new NotImplementedException();
        }
    }
}
