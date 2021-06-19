using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using UI;

namespace Start_a_Town_
{
    public class KeyboardEventArgs
    {
        public bool Handled;
    }

    public class ControlTool : IKeyEventHandler
    {
        public enum Messages { Default, Remove }

        protected void Sync()
        {
            PacketPlayerToolSwitch.Send(Client.Instance, Client.Instance.PlayerData.ID, this);
        }
        public void DrawIcon(SpriteBatch sb, Vector2 pos)
        {
            var icon = this.GetIcon();
            if (icon != null)
                icon.Draw(sb, pos);
        }
        public TargetArgs Target;
        public TargetArgs TargetLast;
        //public Icon Icon = UI.Icon.Cursor;
        public virtual Icon Icon
        {
            get { return UI.Icon.Cursor; }
        }
        
        public virtual Icon GetIcon()
        {
            return this.Icon;
        }
        public PlayerOld Owner;
        public ToolManager Manager;

        public virtual GameObject GetTarget()
        {
            return Controller.Instance.MouseoverBlock.Object as GameObject;
        }

        public virtual bool TryGetTarget(out GameObject target)
        {
            return Controller.Instance.MouseoverBlock.TryGet<GameObject>(out target);
        }
        public virtual bool TryGetTarget(out GameObject target, out Vector3 face)
        {
            face = Controller.Instance.MouseoverBlock.Face;
            return Controller.Instance.MouseoverBlock.TryGet<GameObject>(out target);
        }
        public virtual void UpdateRemote(TargetArgs target) { this.Target = target; }//.Type != TargetType.Null ? target : this.Target; }
        public virtual void Update()
        {
            var cam = Client.Instance.Map.Camera;// ScreenManager.CurrentScreen.Camera;
            cam.MousePicking(Rooms.Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);

            UpdateTarget();
            //UpdateTargetNew();

            this.ChangeFocus();
            if (this.Target != null)
                if (this.TargetLast != this.Target)
                    OnTargetChanged();

            if (InputState.IsKeyDown(Keys.LButton))
                return;
            if (InputState.IsKeyDown(Keys.RButton))
                return;

            //TargetOld = Controller.Instance.Mouseover.Object as GameObject;
            //Face = InputState.IsKeyDown(Keys.RShiftKey) ? Vector3.Forward : Controller.Instance.Mouseover.Face;
            //Precise = Controller.Instance.Mouseover.Precise;

            
            // TODO: WARNING: handle case where block changes from server command AFTER being set here

            //if(this.Target is TargetArgs)
            //if (this.Target.Type == TargetType.Position)
            //    if (Client.Instance.Map.GetBlock(this.Target.Global).Type == Block.Types.Air)
            //        throw new Exception();
        }
        protected void UpdateTargetNew()
        {
            //this.TargetLast = this.Target;
            if ((Controller.Instance.MouseoverBlock.Object is Element))
            {
                //Controller.Instance.MouseoverBlock.Object.ToConsole();
                this.Target = TargetArgs.Null;
            }
            else
            {
                //var mouseover = Controller.IsBlockTargeting() ? Controller.Instance.Mouseover : Controller.Instance.MouseoverEntity;
                var mouseover = (Controller.IsBlockTargeting() || this.TargetOnlyBlocks) ? Controller.Instance.MouseoverBlock : Controller.Instance.GetMouseover();

                //this.Target = Controller.Instance.Mouseover.Target ?? TargetArgs.Null;
                if (mouseover.Target != null)
                {
                    this.Target = mouseover.Target;
                    this.TargetLast = this.Target.Type != TargetType.Null ? this.Target : this.TargetLast;

                }
                else
                    this.Target = TargetArgs.Null;
            }
            //if ((Controller.Instance.Mouseover.Object is UI.Element))
            //    this.Target = TargetArgs.Null;
            //else
            //    this.Target = Controller.Instance.Mouseover.Target ?? TargetArgs.Null;
        }
        protected void UpdateTarget()
        {
            //this.TargetLast = this.Target;
            if ((Controller.Instance.MouseoverBlock.Object is Element))
                this.Target = TargetArgs.Null;
            else
            {
                //this.Target = Controller.Instance.Mouseover.Target ?? TargetArgs.Null;
                if (Controller.Instance.MouseoverBlock.Target != null)
                {
                    this.Target = Controller.Instance.MouseoverBlock.Target;
                    this.TargetLast = this.Target.Type != TargetType.Null ? this.Target : this.TargetLast;

                }
                else
                    this.Target = TargetArgs.Null;
            }
            //if ((Controller.Instance.Mouseover.Object is UI.Element))
            //    this.Target = TargetArgs.Null;
            //else
            //    this.Target = Controller.Instance.Mouseover.Target ?? TargetArgs.Null;
        }
        public virtual void Update(SceneState scene)
        {
            this.Update();
        }

        //TargetArgs GetClosestObject()
        //{
        //    //if (!Camera.BlockTargeting)
        //    //    return null;
        
        //    if (Player.Actor == null)
        //        return null;
        //    if (!Player.Actor.IsSpawned)
        //        return null;
        //    var closest = Player.Actor.GetNearbyObjects(r => r <= InteractionOld.DefaultRange).OrderBy(o=>Vector3.DistanceSquared(o.Global, Player.Actor.Global)).FirstOrDefault();
        //    return closest != null ? new TargetArgs(closest) : null;
        //}

        public virtual Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseMiddle() { return Messages.Default; }
        public virtual Messages MouseMiddleUp(System.Windows.Forms.HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseMiddleDown(System.Windows.Forms.HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseWheel(InputState e, int value) { return Messages.Default; }

        public virtual Messages OnKey(System.Windows.Forms.KeyEventArgs e) { return Messages.Default; }

        void ChangeFocus()
        {
            var obj = this.Target != null ? this.Target.Object as GameObject : null;
            var objLast = this.TargetLast != null ? this.TargetLast.Object as GameObject : null;
            if (obj != objLast)
            {
                if (objLast != null)
                    objLast.FocusLost();
                if (obj != null)
                    obj.Focus();
            }
        }

        protected virtual void OnTargetChanged()
        {
            
        }
        internal virtual void KeyDown(InputState input) { }
        internal virtual void PickUp() { }
        internal virtual void Drop() { }
        internal virtual void Activate() { }
        internal virtual void ManageEquipment() { }

        public virtual void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e) { }
        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e) { }
        public virtual void HandleKeyUp(System.Windows.Forms.KeyEventArgs e) { }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleInput(InputState e) { }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDoubleClick(HandledMouseEventArgs e) { }

        //internal virtual void HandleContextMenuSelection(InteractionOld inter) { }

        internal virtual void Jump() { }
        internal virtual void Use() { }
        internal virtual void Throw() { }


        //internal virtual void Draw(SpriteBatch sb, Camera camera) { }
        internal virtual void DrawUI(SpriteBatch sb, Camera camera)
        {
            var icon = this.GetIcon();
            if (icon != null)
                icon.Draw(sb, UI.UIManager.Mouse + new Vector2(icon.SourceRect.Width / 2, 0));
        }
        internal virtual void DrawWorld(SpriteBatch sb, IMap map, Camera camera)
        {
            DrawTileHighlight(sb, map, camera);
          //  DrawAction(sb, camera);
        }
        internal virtual void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            //this.DrawBlockMouseover(sb, map, camera);
            //return;
            //DrawTileHighlight(sb, map, camera);
        }
     

        public bool IsCtrlKeyDown()
        {
            return Controller.Input.GetKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        public bool IsAltKeyDown()
        {
            return Controller.Input.GetKeyDown(System.Windows.Forms.Keys.LMenu);
        }
        public bool IsShiftKeyDown()
        {
            return Controller.Input.GetKeyDown(System.Windows.Forms.Keys.LShiftKey);
        }
        
        public virtual void DrawBlockMouseover(MySpriteBatch sb, IMap map, Camera camera)
        {
            //if (TargetOld.IsNull())
            //    return;
            //if (this.Face == Vector3.Zero)
            //    return;
            if (this.Target == null)
                return;
            if (this.Target.Face == Vector3.Zero)
                return;

            Rectangle bounds = Block.Bounds;
            float cd;
            Rectangle screenBounds;
            Vector2 screenLoc;
            //camera.GetEverything(map, TargetOld.Global, bounds, out cd, out screenBounds, out screenLoc);
            //cd = TargetOld.Global.GetDrawDepth(map, camera);
            camera.GetEverything(map, this.Target.Global, bounds, out cd, out screenBounds, out screenLoc);
            var scrbnds = camera.GetScreenBoundsVector4(this.Target.Global.X, this.Target.Global.Y, this.Target.Global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = this.Target.Global.GetDrawDepth(map, camera);
            var cdback = cd - 2;// (this.Target.Global - Vector3.One).GetDrawDepth(map, camera);
            var highlight = Sprite.BlockHighlight; // WHY DO I USE AN ENTITY SPRITE INSTEAD OF A BLOCK TEXTURE IN THE BLOCK TEXTURE ATLAS???
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;

            //highlight.Draw(sb, screenLoc, Color.White * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            //highlight.Draw(sb, screenLoc, Color.White, Color.White, Color.White, Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            //highlight.Draw(sb, screenLoc, Color.White, Color.White, Color.White, Color.Transparent, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);

            var c = Color.White *.5f;//*0.8f;
            //c = new Color(1f, 1f, 1f, 0.66f);
            //sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom), Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            sb.Draw(Sprite.BlockHightlightBack.AtlasToken.Atlas.Texture, screenLoc, Sprite.BlockHightlightBack.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cdback);
            sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom), 
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
        }
        public virtual void DrawTileHighlight(MySpriteBatch sb, IMap map, Camera camera)
        {
            //Console.WriteLine(Target.IsNull() ? "null" : Target.Name);
            //if (TargetOld.IsNull())
            //    return;
            if (this.Target == null) return;
            //if (!Target.IsBlock()) // TODO: change that in case i want to draw face highlights on object instead of just blocks
            //    return;
            if (this.Target.Face == Vector3.Zero)
                return;
            //Sprite sprite = Block.TileSprites[Block.Types.Sand];
            Rectangle bounds = Block.Bounds;
            float cd;
            Rectangle screenBounds;
            Vector2 screenLoc;
            //camera.GetEverything(TargetOld.Global + Face, sprite.GetBounds(), out cd, out screenBounds, out screenLoc);
            //camera.GetEverything(map, TargetOld.Global + Face, bounds, out cd, out screenBounds, out screenLoc);
            camera.GetEverything(map, this.Target.FaceGlobal, bounds, out cd, out screenBounds, out screenLoc);

            int x = (int)Math.Abs(this.Target.Face.X), y = (int)Math.Abs(this.Target.Face.Y), r = (int)camera.Rotation;
            int z = (int)Math.Abs(this.Target.Face.Z);
            int highlightIndex = (2 * z) + (1 - z) * ((x + r) % 2);
            int frontback = InputState.IsKeyDown(Keys.RShiftKey) ? 1 : 0;

            //sb.Draw(Map.TerrainSprites, screenLoc, Block.TileHighlights[frontback][highlightIndex], Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            //sb.Draw(Map.TerrainSprites, screenLoc, Block.TileHighlights[frontback][highlightIndex], Color.White * 0.4f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

            //cd = (TargetOld.Global + Face).GetDrawDepth(map, camera);
            cd = (this.Target.FaceGlobal).GetDrawDepth(map, camera);

            //sb.Draw(Map.TerrainSprites, screenLoc, Block.TileHighlights[frontback][highlightIndex], 0, Vector2.Zero, camera.Zoom, Color.White, SpriteEffects.None, cd);
                //Sprite.Shadow.Draw(sb, pos, Color.White, 0, Sprite.Shadow.Origin, camera.Zoom, SpriteEffects.None, dn);
            var rotatedIndex = this.Target.Face.Rotate(camera.Rotation);
            if (!Sprite.BlockFaceHighlights.ContainsKey(rotatedIndex))
                return;
            var highlight = Sprite.BlockFaceHighlights[rotatedIndex];
            //Game1.Instance.GraphicsDevice.Textures[3] = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepthFar");
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            highlight.Draw(sb, screenLoc, Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            sb.Flush();
            //sb.Draw(Map.TerrainSprites, screenLoc, Block.TileHighlights[frontback][highlightIndex], Color.White * 0.4f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

        }
        public virtual void DrawTileHighlight(SpriteBatch sb, IMap map, Camera camera)
        {
            //Console.WriteLine(Target.IsNull() ? "null" : Target.Name);
            //if (TargetOld.IsNull())
            //    return;
            if (this.Target == null)
                return;
            //if (!Target.IsBlock()) // TODO: change that in case i want to draw face highlights on object instead of just blocks
            //    return;
            if (this.Target.Face == Vector3.Zero)
                return;
            //Sprite sprite = Block.TileSprites[Block.Types.Sand];
            float cd;
            Rectangle screenBounds;
            Vector2 screenLoc;
            //camera.GetEverything(TargetOld.Global + Face, sprite.GetBounds(), out cd, out screenBounds, out screenLoc);
            //camera.GetEverything(map, TargetOld.Global + Face, Block.Bounds, out cd, out screenBounds, out screenLoc);
            camera.GetEverything(map, this.Target.FaceGlobal, Block.Bounds, out cd, out screenBounds, out screenLoc);

            int x = (int)Math.Abs(this.Target.Face.X), y = (int)Math.Abs(this.Target.Face.Y), r = (int)camera.Rotation;
            int z = (int)Math.Abs(this.Target.Face.Z);
            int highlightIndex = (2 * z) + (1 - z) * ((x + r) % 2);
            int frontback = InputState.IsKeyDown(Keys.RShiftKey) ? 1 : 0;

            sb.Draw(Map.TerrainSprites, screenLoc, Block.TileHighlights[frontback][highlightIndex], Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            sb.Draw(Map.TerrainSprites, screenLoc, Block.TileHighlights[frontback][highlightIndex], Color.White * 0.4f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        }

        internal virtual void OnGameEvent(GameEvent e)
        {
          
        }

        internal virtual void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var camera = map.Camera;
            ToolManager.DrawBlockMouseover(sb, map, camera, this.Target);
            return;
            //this.DrawBlockMouseover(sb, map, camera);
        }
        internal virtual void GetContextActions(ContextArgs args) { }
        internal virtual void OnActiveToolSet() { }
        internal virtual void SlotRightClick(GameObjectSlot slot) { }
        internal virtual void SlotLeftClick(GameObjectSlot gameObjectSlot) { }


        internal void Write(BinaryWriter w)
        {
            w.Write(this.GetType().FullName);
            this.WriteData(w);
        }
        ControlTool Read(BinaryReader r)
        {
            this.ReadData(r);
            return this;
        }
        protected virtual void WriteData(BinaryWriter w) { }
        protected virtual void ReadData(BinaryReader r) { }
        
        internal static ControlTool Create(BinaryReader r)
        {
            var type = Type.GetType(r.ReadString());
            return (Activator.CreateInstance(type) as ControlTool).Read(r);
        }
        internal static ControlTool CreateOrSync(BinaryReader r, Net.PlayerData player)
        {
            var type = Type.GetType(r.ReadString());
            var tool = player.CurrentTool;
            if(tool.GetType() == type)
                return tool.Read(r).Read(player);
            return (Activator.CreateInstance(type) as ControlTool).Read(r).Read(player);
        }

        internal virtual ControlTool Read(Net.PlayerData player)
        {
            return this;
        }
        internal virtual void DrawUIRemote(SpriteBatch sb, Camera camera, Vector2 vector2, TargetArgs targetArgs, Net.PlayerData player)
        {
            UIManager.DrawStringOutlined(sb, this.GetType().Name, vector2 + new Vector2(0, UIManager.Cursor.Height));
        }
        internal virtual void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, PlayerData player)
        {
            //this.DrawAfterWorld(sb, map, camera);
        }

        internal virtual void DrawUIRemote(SpriteBatch sb, Camera camera, PlayerData pl)
        {
        }

        public virtual string HelpText { get; } = "";
        public virtual bool TargetOnlyBlocks { get; } = false;
    }
}
