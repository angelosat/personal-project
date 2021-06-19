using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.PlayerControl.Schemes;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    public class MoveTool : ControlTool
    {
        bool MoveToggle = false;
        bool RButtonDown = false;

        Scheme CurrentScheme = new Wasd();
        bool MouseMove = true;

        public void ChangeDirection(Vector3 direction)
        {
            Net.Client.PlayerChangeDirection(direction);
        }

        public void StartWalking()
        {
            Net.Client.PlayerStartMoving();
        }
        private void StopWalking()
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

        public override void Update()
        {
            base.Update();
            if (MoveToggle)
                MoveMouse();
            else
                MoveKeys();
        }

        public void MoveKeys()
        {
            //    if (e.KeyHandled)
            //if (e.Handled)
            //    return;
            int xx = 0, yy = 0;
            //if (input.IsKeyDown(GlobalVars.KeyBindings.North))

            if (Up)
            {
                xx -= 1;
                yy -= 1;
            }
            else if (Down) ///GlobalVars.KeyBindings.South))
            {
                yy += 1;
                xx += 1;
            }
            if (Left)//GlobalVars.KeyBindings.East))
            {
                yy += 1;
                xx -= 1;
            }
            else if (Right)//GlobalVars.KeyBindings.West))
            {
                yy -= 1;
                xx += 1;
            }
           // Walking.ToConsole();
            if (xx != 0 || yy != 0)
            {
                //PhysicsComponent physComp = Player.Actor.GetPhysics();
                var cam = ScreenManager.CurrentScreen.Camera;
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
                Vector3 direction = new Vector3(NextStep.X, NextStep.Y, Player.Actor.Velocity.Z);// posComp.GetProperty<Vector3>("Speed").Z);

                Net.Client.PlayerChangeDirection(direction);
                if(!Walking)
                    StartWalking();
                Walking = true;

            }
            else
            {
              //  Console.WriteLine("GAMW TO THEO");
                StopWalking();
                //if (!Walking)
                //    return;
                //Player.Actor.PostMessage(Message.Types.StopWalking, null);
                //Walking = false;
            }
        }

        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (this.RButton)
            //    return;
            //if (ContextMenu2.Instance.IsOpen)
            //    return;
            //TargetOld = Controller.Instance.Mouseover.Object as GameObject;
            //Face = Controller.Instance.Mouseover.Face;
            //this.TargetLast = this.Target;
            //this.Target = Controller.Instance.Mouseover.Target;
            base.HandleMouseMove(e);
        }


        bool Walking = false;

        bool Up, Down, Left, Right;
        bool KeyDown = false;
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


        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            KeyDown = false;
            //Moving = false;
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
                Net.Client.Console.Write("Mouse move " + (this.MouseMove ? "Enabled" : "Disabled"));
            }

            base.HandleKeyUp(e);

        }

        private void StartBlock()
        {
            if (Blocking)
                return;
            Blocking = true;
            Client.PlayerStartBlocking();
            //Client.PostPlayerInput(Message.Types.StartScript, w =>
            //{
            //    Ability.Write(w, Script.Types.Block, new TargetArgs(this.TargetOld), ww =>
            //    {
            //        ww.Write(GetDirection());
            //    });
            //});
        }
        private void FinishBlock()
        {
            Blocking = false;
            Client.PlayerFinishBlocking();
            //Client.PostPlayerInput(Message.Types.FinishScript, w =>
            //{
            //    // DONT write script type for finishscript
            //    Ability.Write(w, Script.Types.Block, new TargetArgs(this.TargetOld), ww =>
            //    {
            //        ww.Write(GetDirection());
            //    });
            //});
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

        static public bool CanReach(GameObject obj1, GameObject obj2, float range)
        {
            //int height1 = obj1.GetInfo().Height, height2 = obj2.GetInfo().Height;
            if (range < 0)
                return true;
            float height1 = obj1["Physics"].GetProperty<float>("Height");
            PhysicsComponent phys;
            float height2 = obj2.TryGetComponent<PhysicsComponent>("Physics", out phys) ? (float)phys["Height"] : 1;

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
            GameObjectSlot dragobj = DragDropManager.Instance.Source as GameObjectSlot;
            GameObject tar;

            if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return Messages.Default;

            //if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            //{
            //    PartyComponent party;
            //    if (Player.Actor.TryGetComponent<PartyComponent>("Party", out party))
            //    {
            //        GameObjectSlot memberSlot = party.Members.FirstOrDefault();
            //        if (memberSlot != null)
            //            if (memberSlot.HasValue)
            //            {
            //                Message.Types message = (Components.Message.Types)ControlComponent.GetAbility(memberSlot.Object)[AbilitySlot.Primary].Object["Ability"]["Message"];

            //                Interaction inter;
            //                List<Interaction> interactions = new List<Interaction>();
            //                tar.Query(memberSlot.Object, interactions);
            //                inter = interactions.FirstOrDefault(i => i.Message == message);
            //                if (inter == null)
            //                {
            //                    Console.WriteLine("NO INTERACTION FOUND (" + message + ")");
            //                    return Messages.Default;
            //                }

            //                memberSlot.Object.PostMessage(Components.Message.Types.Order, null, inter, Controller.Instance.Mouseover.Face);
            //                return Messages.Default;
            //            }
            //    }
            //}

            InventoryComponent inv;
            if (dragobj != null)
            {
                //inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");

                //GameObjectSlot source = DragDropManager.Instance.Source as GameObjectSlot;

                //if (Player.Actor.PostMessage(Components.Message.Types.Begin, null, tar, Message.Types.Give, dragobj, Controller.Instance.Mouseover.Face))
                //{

                //    if (dragobj.StackSize == 0)
                //        DragDropManager.Instance.Clear();
                //    else
                //        DragDropManager.Instance.Item = DragDropManager.Instance.Source;

                //    return Messages.Default;
                //}

            }
            inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");
            if (inv.GetProperty<GameObjectSlot>("Holding").Object != null)
            {
                //ToolManager.Instance.ActiveTool = new InteractionTool(tar, AbilitySlot.Function2, System.Windows.Forms.Keys.F);
                new InteractionTool(tar, AbilitySlot.PickUp, System.Windows.Forms.Keys.F);
                return Messages.Remove;
            }
            else
            {
                ToolManager.Instance.ActiveTool = new InteractionTool(tar, AbilitySlot.Primary, System.Windows.Forms.Keys.LButton);
            }

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
            //Player.Actor.PostMessage(Message.Types.Move, null, final, 1f);
            this.ChangeDirection(final);
        }
        public static Vector2 GetDirection2()
        {
            int x = Controller.Instance.msCurrent.X - Rooms.Ingame.Instance.Camera.Width / 2;
            int y = Controller.Instance.msCurrent.Y - Rooms.Ingame.Instance.Camera.Height / 2;
            //float x, y;
            //Coords.Rotate(ScreenManager.CurrentScreen.Camera.Rotation, Controller.Instance.msCurrent.X - ScreenManager.CurrentScreen.Camera.Width / 2, Controller.Instance.msCurrent.Y - ScreenManager.CurrentScreen.Camera.Height / 2, out x, out y);//
            float xx, yy;

            int xxx, yyy;

            Coords.Ortho(x, y, out xx, out yy);
            Coords.Rotate((int)ScreenManager.CurrentScreen.Camera.Rotation, xx, yy, out xxx, out yyy);
            Vector2 normal = new Vector2(xxx, yyy);

            normal.Normalize();
            Vector2 final = new Vector2(normal.X, normal.Y);
            return final;
        }
        public static Vector3 GetDirection3()
        {
            var playerScreenPosition = Rooms.Ingame.Instance.Camera.GetScreenPosition(Player.Actor.Global);
            //int x = Controller.Instance.msCurrent.X - Rooms.Ingame.Instance.Camera.Width / 2;
            //int y = Controller.Instance.msCurrent.Y - Rooms.Ingame.Instance.Camera.Height / 2;
            int x = Controller.Instance.msCurrent.X - (int)playerScreenPosition.X;
            int y = Controller.Instance.msCurrent.Y - (int)playerScreenPosition.Y;
            //float x, y;
            //Coords.Rotate(ScreenManager.CurrentScreen.Camera.Rotation, Controller.Instance.msCurrent.X - ScreenManager.CurrentScreen.Camera.Width / 2, Controller.Instance.msCurrent.Y - ScreenManager.CurrentScreen.Camera.Height / 2, out x, out y);//
            float xx, yy; 
            
            int xxx, yyy;
            
            Coords.Ortho(x, y, out xx, out yy);
            Coords.Rotate((int)ScreenManager.CurrentScreen.Camera.Rotation, xx, yy, out xxx, out yyy);
            Vector2 normal = new Vector2(xxx, yyy);

            normal.Normalize();
            Vector3 final = new Vector3(normal.X, normal.Y, 0);
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

            if(InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
            {
                if (this.Target != null)
                    if (this.Target.Type == TargetType.Entity)
                        if (this.Target.Object.ID == GameObject.Types.Npc)
                            ScreenManager.CurrentScreen.ToolManager.ActiveTool = new AI.ToolNpcControl(this.Target.Object);
            }
            RightClick();
            return Messages.Default;
        }

        private void RightClick()
        {
            if (this.Target == null)
                return;
            var action = this.Target.GetRightClickAction();
            //var action = PlayerInput.GetDefaultInput(Player.Actor, this.Target, PlayerInput.RButton);
            if (action != null)
                action.Action();
            else
                Client.PlayerInput(this.Target, PlayerInput.RButton);// new PlayerInput(PlayerActions.RB));
        }

        internal override void DrawWorld(SpriteBatch sb, IMap map, Camera camera)
        {
            base.DrawWorld(sb, map, camera);
            GameObjectSlot drg = DragDropManager.Instance.Source as GameObjectSlot;
            if (drg != null)
                if (this.Target != null)
                    drg.Object.DrawPreview(sb, camera, this.Target.FaceGlobal);
                //if (TargetOld != null)
                //    drg.Object.DrawPreview(sb, camera, TargetOld.Global + Face);

        }

        Action RepeatAction = () => { };
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.InteractionSuccessful:
                    //if (this.RButtonDown)
                    //    RightClick();
                    this.RepeatAction();
                    break;

                default:
                    break;
            }
        }
    }
}
