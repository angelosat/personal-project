﻿using System;
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
    public class DefaultTool : ControlTool
    {
        protected Dictionary<System.Windows.Forms.Keys, KeyControl> KeyControls;

        public Hotbar Hotbar;
        bool MoveToggle = false;
        bool RButtonDown = false;

        //readonly Scheme CurrentScheme;
        bool MouseMove = true;
        //DefaultManagementTool ManagementTool = new DefaultManagementTool();

        public DefaultTool()
        {
            Hotbar = new Hotbar();
            this.KeyControls = new Dictionary<System.Windows.Forms.Keys, KeyControl>
            {
                //this.KeyControls.Add(GlobalVars.KeyBindings.PickUp, new KeyControl(PickUp, Carry));

                //this.KeyControls.Add(GlobalVars.KeyBindings.PickUp, new KeyControl(PickUp));
                //this.KeyControls.Add(GlobalVars.KeyBindings.Drop, new KeyControl(this.Drop, UseHauled));
                //this.KeyControls.Add(GlobalVars.KeyBindings.Activate, new KeyControl(Activate, Equip));

                { GlobalVars.KeyBindings.PickUp, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.PickUp), () => Client.PlayerInput(this.Target, PlayerInput.PickUpHold)) },
                { GlobalVars.KeyBindings.Drop, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.Drop), () => Client.PlayerInput(this.Target, PlayerInput.DropHold)) },
                //this.KeyControls.Add(GlobalVars.KeyBindings.Activate, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.Activate), () => Client.PlayerInput(this.Target, PlayerInput.ActivateHold)));
                { GlobalVars.KeyBindings.Activate, new KeyControl(this.Activate, () => Client.PlayerInput(this.Target, PlayerInput.ActivateHold)) },

                { GlobalVars.KeyBindings.Throw, new KeyControl(() => this.ThrowNew(), () => this.ThrowNew(true)) },
                { System.Windows.Forms.Keys.X, new KeyControl(() => Client.PlayerInput(this.Target, PlayerInput.Seathe)) },
                { System.Windows.Forms.Keys.Tab, new KeyControl(SwitchTool) }
            };

            //this.CurrentScheme = new Wasd();
        }

        protected virtual void SwitchTool()
        {
            Client.PlayerStopMoving();
            this.StopWalking();
            //ToolManager.Instance.ActiveTool = new DefaultManagementTool();// this.ManagementTool;
            ToolManager.SetTool(new ToolManagement());//ScreenManager.CurrentScreen.Camera));
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

        internal override void Throw()
        {
            Vector3 dir = GetDirection3();
            //Client.PlayerThrow(dir);
        }
        private void ThrowNew(bool all = false)
        {
            var dir3d = GetDirection3();
            Vector2 dir2d = dir3d.XY();// 2();
            Client.PlayerThrow(dir3d, all);
            //return;
            ////Client.PlayerInput(new TargetArgs(dir), new PlayerInput(PlayerActions.Throw));
            ////Client.PlayerInput(new TargetArgs(dir), new PlayerInput(all ? PlayerActions.ThrowAll : PlayerActions.Throw, all));
            //Client.PlayerInput(new TargetArgs(dir2d), PlayerInput.ThrowHold);
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

            
            // this must be called for mouse picking to work, because the call to mousepicking is in the camera update method
            // maybe put this in the base update? or seperate them?
            var global = PlayerOld.Actor == null ? Vector3.Zero : PlayerOld.Actor.Global;

            //var cam = ScreenManager.CurrentScreen.Camera;
            //cam.Update(Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map, global);

        }

        public virtual void MoveKeys()
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
                //var cam = ScreenManager.CurrentScreen.Camera;
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
                var direction = new Vector3(NextStep.X, NextStep.Y, PlayerOld.Actor.Velocity.Z);// posComp.GetProperty<Vector3>("Speed").Z);

                Client.PlayerChangeDirection(direction);
                if(!Walking)
                    StartWalking();
                Walking = true;

            }
            else
            {
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
        //bool KeyDown = false;
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
            //KeyDown = false;
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
                Client.Instance.Log.Write("Mouse move " + (this.MouseMove ? "Enabled" : "Disabled"));
            }

            base.HandleKeyUp(e);
            if (this.KeyControls.TryGetValue(e.KeyCode, out KeyControl key))
                key.Up();
        }

        void StartGathering()
        {

        }
        void StopGathering()
        {

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

        void Interact()
        {
            if (this.Target == null)
                return;
            var action = this.Target.GetRightClickAction();
            if (action != null)
                action.Action();
        }

        //internal override void Use()
        //{
        //    if (TargetOld.IsNull())
        //        return;
        //    GameObjectSlot heldObj;
        //    //if (!InventoryComponent.TryGetHeldObject(Player.Actor, out heldObj))
        //    //    return;

        //    // TODO: maybe do this server side? by sending a 'player used held item' packet

        //    GameObject held = Player.Actor.GetComponent<InventoryComponent>().Holding.Object;
        //    if (held.IsNull())
        //        return;
        //    UseComponentOld use;
        //    if (!held.TryGetComponent<UseComponentOld>(out use))
        //        return;
        //    if(use.Abilities.Count==0)
        //        return;
        //    Script.Types id = use.Abilities.First();
        //    Client.PostPlayerInput(Message.Types.StartScript, w =>
        //    {
        //        Ability.Write(w, id, new TargetArgs(TargetOld, Face));
        //    });

        //    //Client.PostPlayerInput(Target, Message.Types.UsedItem, w =>
        //    //{
        //    //    TargetArgs.Write(w, Player.Actor);
        //    //    TargetArgs.Write(w, heldObj.Object);
        //    //    w.Write(Face);
        //    //});
        //}

        //internal override void Activate()
        //{
        //    if (TargetOld.IsNull())
        //        return;
        //    Client.PlayerUse(this.Target);
        //    //Client.PostPlayerInput(Message.Types.StartScript, w =>
        //    //{
        //    //    Ability.Write(w, Script.Types.Activate, this.Target);// new TargetArgs(TargetOld, Face));//, ww => TargetArgs.Write(ww, Player.Actor));
        //    //});
        //    return;
        //}
        //void Carry()
        //{
        //    if (TargetOld.IsNull())
        //        return;
        //    //Client.PlayerInput
        //    if (this.Target.Type != TargetType.Entity)
        //        return;
        //    Client.PlayerCarry(this.Target);

        //}
        //void UseHauled()
        //{
        //    if (TargetOld.IsNull())
        //        return;
            
        //    Client.PlayerUseHauled(this.Target);
        //    return;
        //}
        //void Equip()
        //{
        //    if (TargetOld.IsNull())
        //        return;
        //    Client.PlayerEquip(this.Target);
        //    return;
        //}
        //internal override void Drop()
        //{
        //    //if (this.Target.Type != TargetType.Position)
        //    //    return;
        //    Client.PlayerDropHauled(this.Target);
        //}

        //internal override void PickUp()
        //{
        //    if (this.TargetOld.IsNull())
        //        return;

        //    Client.PlayerPickUp(this.Target);
        //}

        internal override void ManageEquipment()
        {
            if (!Controller.Instance.MouseoverBlock.TryGet(out GameObject _))
                return;

            //ToolManager.SetTool(new InteractionTool(tar, AbilitySlot.Function3, System.Windows.Forms.Keys.Q));//, Player.Actor["Inventory"]["Holding"]);
        }

        static public bool CanReach(GameObject obj1, GameObject obj2, float range)
        {
            //int height1 = obj1.GetInfo().Height, height2 = obj2.GetInfo().Height;
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


            //if (DragDropManager.Instance.Source is GameObjectSlot dragobj)
            //{
             

            //}
            //var inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");

            //if (inv.GetProperty<GameObjectSlot>("Holding").Object != null)
            //{
            //    //ToolManager.Instance.ActiveTool = new InteractionTool(tar, AbilitySlot.Function2, System.Windows.Forms.Keys.F);
            //    new InteractionTool(tar, AbilitySlot.PickUp, System.Windows.Forms.Keys.F);
            //    return Messages.Remove;
            //}
            //else
            //{
            //    ToolManager.SetTool(new InteractionTool(tar, AbilitySlot.Primary, System.Windows.Forms.Keys.LButton));
            //}

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
            var cam = Engine.Map.Camera;
            //int x = Controller.Instance.msCurrent.X - Rooms.Ingame.Instance.Camera.Width / 2;
            //int y = Controller.Instance.msCurrent.Y - Rooms.Ingame.Instance.Camera.Height / 2;
            int x = Controller.Instance.msCurrent.X - cam.Width / 2;
            int y = Controller.Instance.msCurrent.Y - cam.Height / 2;


            Coords.Ortho(x, y, out float xx, out float yy);
            //Coords.Rotate((int)ScreenManager.CurrentScreen.Camera.Rotation, xx, yy, out xxx, out yyy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out int xxx, out int yyy);

            var normal = new Vector2(xxx, yyy);

            normal.Normalize();
            var final = new Vector2(normal.X, normal.Y);
            return final;
        }
        public static Vector3 GetDirection3()
        {
            var cam = PlayerOld.Actor.Map.Camera;
            //var playerScreenPosition = Rooms.Ingame.Instance.Camera.GetScreenPosition(Player.Actor.Global);
            var playerScreenPosition = cam.GetScreenPosition(PlayerOld.Actor.Global);

            //int x = Controller.Instance.msCurrent.X - Rooms.Ingame.Instance.Camera.Width / 2;
            //int y = Controller.Instance.msCurrent.Y - Rooms.Ingame.Instance.Camera.Height / 2;
            int x = Controller.Instance.msCurrent.X - (int)playerScreenPosition.X;
            int y = Controller.Instance.msCurrent.Y - (int)playerScreenPosition.Y;
            //float x, y;
            //Coords.Rotate(ScreenManager.CurrentScreen.Camera.Rotation, Controller.Instance.msCurrent.X - ScreenManager.CurrentScreen.Camera.Width / 2, Controller.Instance.msCurrent.Y - ScreenManager.CurrentScreen.Camera.Height / 2, out x, out y);//


            Coords.Ortho(x, y, out float xx, out float yy);
            //Coords.Rotate((int)ScreenManager.CurrentScreen.Camera.Rotation, xx, yy, out xxx, out yyy);
            Coords.Rotate((int)cam.Rotation, xx, yy, out int xxx, out int yyy);
            var normal = new Vector2(xxx, yyy);

            normal.Normalize();
            var final = new Vector3(normal.X, normal.Y, 0);
            return final;
        }
        //public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (ContextMenu2.Instance.IsOpen)
        //        return;
        //    this.RButton = true;
        //    base.HandleRButtonDown(e);
        //}
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //this.RButtonDown = true;
            //this.RepeatAction = RightClick;
            //RightClick();
            //return Messages.Default;

            //if (ContextMenu2.Instance.IsOpen)
            //    return Messages.Default;

            if (e.Handled)
                return Messages.Default;
            this.RButtonDown = true;
            return base.MouseRightDown(e);
        }
        public override Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (!this.RButtonDown)
            //    return Messages.Default;
            //this.RButtonDown = false;
            //return Messages.Default;

            if (!this.RButtonDown)
                return Messages.Default;
            this.RButtonDown = false;
            if (ContextMenu2.Instance.IsOpen)
                return Messages.Default;

            // maybe just create a "direction" target if the current one is null?

            //RightClick();
            //return Messages.Default;

            if(InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
            {
                if (this.Target != null)
                    if (this.Target.Type == TargetType.Entity)
                        if (this.Target.Object.IDType == GameObject.Types.Npc)
                            ToolManager.SetTool(new AI.ToolNpcControl(this.Target.Object));
                            //ScreenManager.CurrentScreen.ToolManager.ActiveTool = new AI.ToolNpcControl(this.Target.Object);
            }
            RightClick();
            return Messages.Default;
            //this.Interact();
            //return Messages.Default;
        }
        
        private void RightClick()
        {

            if (this.Target == null)
                return;
            //this.Target.ToConsole();
            //var action = this.Target.GetRightClickAction();
            //var action = this.Target.GetContextActionsFromInput(PlayerInput.RButton);// this.Target.GetContextAction();
            var action = this.Target.GetContextRB();// this.Target.GetContextAction();

            if (action != null)
                if (action.Action != null)
                {
                    action.Action();
                    return;
                }
            if (!InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                this.Target.Precise = Vector3.Zero;
            Client.PlayerInput(this.Target, PlayerInput.RButton);
            // new PlayerInput(PlayerActions.RB));
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
            //if (TargetOld != null)
            //    drg.Object.DrawPreview(sb, camera, TargetOld.Global + Face);


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
                //var body = haul.Body;
                //var pos = cam.GetScreenPositionFloat(global);
                //pos += body.OriginGroundOffset * cam.Zoom;
                ////body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, Color.White, Color.Transparent, body.RestingFrame.Offset, 0, cam.Zoom, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
                //// TODO: fix difference between tint and material in this drawtree method
                //var tint = Color.White * .5f;// Color.Transparent;
                //body.DrawTree(haul, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
            }
        }

        readonly Action RepeatAction = () => { };
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

        internal override void SlotRightClick(GameObjectSlot slot)
        {
            Client.PlayerSlotInteraction(slot);
        }
    }
}
