using System;
using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.PlayerControl
{
    public class DefaultTool : ControlTool
    {
        protected Dictionary<System.Windows.Forms.Keys, KeyControl> KeyControls;

        public Hotbar Hotbar;
        bool MoveToggle = false;
        bool RButtonDown = false;

        bool MouseMove = true;

        public DefaultTool()
        {
            Hotbar = new Hotbar();
            this.KeyControls = new Dictionary<System.Windows.Forms.Keys, KeyControl>
            {
                { GlobalVars.KeyBindings.PickUp, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.PickUp), () => Client.PlayerInput(this.Target, PlayerInput.PickUpHold)) },
                { GlobalVars.KeyBindings.Drop, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.Drop), () => Client.PlayerInput(this.Target, PlayerInput.DropHold)) },
                { GlobalVars.KeyBindings.Activate, new KeyControl(this.Activate, () => Client.PlayerInput(this.Target, PlayerInput.ActivateHold)) },
                { GlobalVars.KeyBindings.Throw, new KeyControl(() => this.ThrowNew(), () => this.ThrowNew(true)) },
                { System.Windows.Forms.Keys.X, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.Seathe)) },
                { System.Windows.Forms.Keys.Tab, new KeyControl(SwitchTool) }
            };
        }

        protected virtual void SwitchTool()
        {
            Client.PlayerStopMoving();
            this.StopWalking();
            ToolManager.SetTool(new ToolManagement());
            Client.Instance.Log.Write("Switched to management mode");
        }

        public void ChangeDirection(Vector3 direction)
        {
            Client.PlayerChangeDirection(direction);
        }

        public void StartWalking()
        {
            Client.PlayerStartMoving();
        }
        protected void StopWalking()
        {
            if (!Walking)
                return;
            if (MoveToggle)
                return;
            Client.PlayerStopMoving();
            Walking = false;
        }

        internal override void Jump()
        {
            Client.PlayerJump();
        }
        
        private void ThrowNew(bool all = false)
        {
            var dir3d = GetDirection3();
            Client.PlayerThrow(dir3d, all);
        }
        internal override void Activate()
        {
            base.Activate();
            if (this.Target == null)
                return;
            var action = this.Target.GetContextActivate();
            if (action != null)
                if (action.Action != null)
                {
                    action.Action();
                    return;
                }
            if (!InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                this.Target.Precise = Vector3.Zero;
            Client.PlayerInput(this.Target, PlayerInput.Activate);
        }
        public override void Update()
        {
            base.Update();
            if (MoveToggle)
                MoveMouse();
            else
                MoveKeys();

            foreach (var key in this.KeyControls.Values)
                key.Update();
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
                var cam = PlayerOld.Actor.Map.Camera;

                double rx, ry;
                double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
                rx = (xx * cos - yy * sin);
                ry = (xx * sin + yy * cos);
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                var NextStep = new Vector2(roundx, roundy);
                NextStep.Normalize();
                var direction = new Vector3(NextStep.X, NextStep.Y, PlayerOld.Actor.Velocity.Z);

                Client.PlayerChangeDirection(direction);
                if(!Walking)
                    StartWalking();
                Walking = true;

            }
            else
            {
                StopWalking();
            }
        }

        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleMouseMove(e);
        }

        bool Walking = false;
        bool Up, Down, Left, Right;
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {

            if (e.Handled)
                return;
            if (e.KeyCode == GlobalVars.KeyBindings.North || e.KeyCode == System.Windows.Forms.Keys.Up)
                Up = true;
            if (e.KeyCode == GlobalVars.KeyBindings.South || e.KeyCode == System.Windows.Forms.Keys.Down)
                Down = true;
            if (e.KeyCode == GlobalVars.KeyBindings.West || e.KeyCode == System.Windows.Forms.Keys.Left)
                Left = true;
            if (e.KeyCode == GlobalVars.KeyBindings.East || e.KeyCode == System.Windows.Forms.Keys.Right)
                Right = true;
            if (e.KeyCode == GlobalVars.KeyBindings.Attack)
                StartAttack();
            if (e.KeyCode == GlobalVars.KeyBindings.Block)
                StartBlock();

            if (e.KeyCode == GlobalVars.KeyBindings.RunWalk)
                Client.PlayerToggleWalk(true);
            if (e.KeyCode == GlobalVars.KeyBindings.Sprint)
                Client.PlayerToggleSprint(true);
            
            if (this.KeyControls.TryGetValue(e.KeyCode, out KeyControl key))
                key.Down();
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if(e.Handled)
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
                case System.Windows.Forms.Keys.Z:
                    FinishAttack();
                    break;
                case System.Windows.Forms.Keys.X:
                    FinishBlock();
                    break;
                default:
                    break;
            }
            if (!(Up || Down || Left || Right))
                StopWalking();

            if (e.KeyCode == GlobalVars.KeyBindings.RunWalk)
                Client.PlayerToggleWalk(false);
            if (e.KeyCode == GlobalVars.KeyBindings.Sprint)
                Client.PlayerToggleSprint(false);

            if (e.KeyCode == System.Windows.Forms.Keys.M)
            {
                this.MouseMove = !this.MouseMove;
                Client.Instance.Log.Write("Mouse move " + (this.MouseMove ? "Enabled" : "Disabled"));
            }

            base.HandleKeyUp(e);
            if (this.KeyControls.TryGetValue(e.KeyCode, out KeyControl key))
                key.Up();
        }

        private void StartBlock()
        {
            if (Blocking)
                return;
            Blocking = true;
            Client.PlayerStartBlocking();
        }
        private void FinishBlock()
        {
            Blocking = false;
            Client.PlayerFinishBlocking();
        }
        bool Attacking, Blocking;
        private void StartAttack()
        {
            if (Attacking)
                return;
            Attacking = true;
            Client.PlayerAttack();
        }
        private void FinishAttack()
        {
            Client.PlayerFinishAttack(GetDirection3());
            Attacking = false;
        }

        internal override void ManageEquipment()
        {
            if (!Controller.Instance.MouseoverBlock.TryGet(out GameObject _))
                return;
        }

        static public bool CanReach(GameObject obj1, GameObject obj2, float range)
        {
            if (range < 0)
                return true;
            float height1 = obj1["Physics"].GetProperty<float>("Height");
            float height2 = obj2.TryGetComponent("Physics", out PhysicsComponent phys) ? (float)phys["Height"] : 1;

            Vector3 global1 = obj1.Global, global2 = obj2.Global;
            for (int i = 0; i < height1; i++)
                for (int j = 0; j < height2; j++)
                {
                    float dist = Vector3.Distance(global1 + new Vector3(0, 0, i), global2 + new Vector3(0, 0, j));
                    if (dist < range)
                        return true;
                }

            return false;
        }
        static public bool CanReach(GameObject obj1, GameObject obj2)
        {
            return CanReach(obj1, obj2, 2);

        }

        public Messages OnKey(System.Windows.Forms.Keys key)
        {
            if (!Controller.Instance.MouseoverBlock.TryGet(out GameObject _))
                return Messages.Default;
            return Messages.Default;
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

        public void MoveMouse()
        {
            Walking = true;
            Vector3 final = GetDirection3();
            this.ChangeDirection(final);
        }
        public static Vector2 GetDirection2()
        {
            var cam = Engine.Map.Camera;
            int x = Controller.Instance.msCurrent.X - cam.Width / 2;
            int y = Controller.Instance.msCurrent.Y - cam.Height / 2;
            Coords.Ortho(x, y, out float xx, out float yy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out int xxx, out int yyy);
            var normal = new Vector2(xxx, yyy);
            normal.Normalize();
            var final = new Vector2(normal.X, normal.Y);
            return final;
        }
        public static Vector3 GetDirection3()
        {
            var cam = PlayerOld.Actor.Map.Camera;
            var playerScreenPosition = cam.GetScreenPosition(PlayerOld.Actor.Global);
            int x = Controller.Instance.msCurrent.X - (int)playerScreenPosition.X;
            int y = Controller.Instance.msCurrent.Y - (int)playerScreenPosition.Y;
            Coords.Ortho(x, y, out float xx, out float yy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out int xxx, out int yyy);
            var normal = new Vector2(xxx, yyy);
            normal.Normalize();
            var final = new Vector3(normal.X, normal.Y, 0);
            return final;
        }
        
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            this.RButtonDown = true;
            return base.MouseRightDown(e);
        }
        public override Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.RButtonDown)
                return Messages.Default;
            this.RButtonDown = false;
            if (ContextMenu2.Instance.IsOpen)
                return Messages.Default;

            // maybe just create a "direction" target if the current one is null?

            RightClick();
            return Messages.Default;
        }
        
        private void RightClick()
        {
            if (this.Target == null)
                return;
            var action = this.Target.GetContextRB();

            if (action != null)
                if (action.Action != null)
                {
                    action.Action();
                    return;
                }
            if (!InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                this.Target.Precise = Vector3.Zero;
            Client.PlayerInput(this.Target, PlayerInput.RButton);
        }

        internal override void GetContextActions(ContextArgs args)
        {
            this.Target.GetContextAll(args);
        }

        internal override void DrawWorld(SpriteBatch sb, IMap map, Camera camera)
        {
            base.DrawWorld(sb, map, camera);
            if (DragDropManager.Instance.Source is GameObjectSlot drg)
                if (this.Target != null)
                    drg.Object.DrawPreview(sb, camera, this.Target.FaceGlobal);
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            base.DrawAfterWorld(sb, map);
            var haul = PersonalInventoryComponent.GetHauling(PlayerOld.Actor).Object;
            if (haul != null)
            {
                if (this.Target == null)
                    return;
                var global = this.Target.Global + this.Target.Face + (!InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
                haul.DrawPreview(sb, cam, global);
            }
        }

        readonly Action RepeatAction = () => { };
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.InteractionSuccessful:
                    this.RepeatAction();
                    break;

                default:
                    break;
            }
        }

        internal override void SlotRightClick(GameObjectSlot slot)
        {
            Client.PlayerSlotInteraction(slot);
        }
    }
}
