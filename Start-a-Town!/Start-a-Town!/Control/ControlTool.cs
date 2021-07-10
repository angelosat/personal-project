using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using UI;

namespace Start_a_Town_
{
    public class ControlTool : IKeyEventHandler
    {
        public enum Messages { Default, Remove }

        protected void Sync()
        {
            PacketPlayerToolSwitch.Send(Client.Instance, Client.Instance.PlayerData.ID, this);
        }
        public void DrawIcon(SpriteBatch sb, Vector2 pos)
        {
            if (this.GetIcon() is Icon icon)
                icon.Draw(sb, pos);
        }
        public TargetArgs Target;
        public TargetArgs TargetLast;
        public virtual Icon Icon
        {
            get { return Icon.Cursor; }
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
        public virtual void UpdateRemote(TargetArgs target) { this.Target = target; }
        public virtual void Update()
        {
            var cam = Client.Instance.Map.Camera;
            cam.MousePicking(Rooms.Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);

            UpdateTarget();

            this.ChangeFocus();
            if (this.Target != null)
                if (this.TargetLast != this.Target)
                    OnTargetChanged();

            if (InputState.IsKeyDown(Keys.LButton))
                return;
            if (InputState.IsKeyDown(Keys.RButton))
                return;
        }
        protected void UpdateTargetNew()
        {
            if ((Controller.Instance.MouseoverBlock.Object is Element))
            {
                this.Target = TargetArgs.Null;
            }
            else
            {
                var mouseover = (Controller.IsBlockTargeting() || this.TargetOnlyBlocks) ? Controller.Instance.MouseoverBlock : Controller.Instance.GetMouseover();
                if (mouseover.Target != null)
                {
                    this.Target = mouseover.Target;
                    this.TargetLast = this.Target.Type != TargetType.Null ? this.Target : this.TargetLast;

                }
                else
                    this.Target = TargetArgs.Null;
            }
        }
        protected void UpdateTarget()
        {
            if ((Controller.Instance.MouseoverBlock.Object is Element))
                this.Target = TargetArgs.Null;
            else
            {
                if (Controller.Instance.MouseoverBlock.Target != null)
                {
                    this.Target = Controller.Instance.MouseoverBlock.Target;
                    this.TargetLast = this.Target.Type != TargetType.Null ? this.Target : this.TargetLast;

                }
                else
                    this.Target = TargetArgs.Null;
            }
        }
        public virtual void Update(SceneState scene)
        {
            this.Update();
        }

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

        internal virtual void Jump() { }
        internal virtual void Use() { }
        internal virtual void Throw() { }

        internal virtual void DrawUI(SpriteBatch sb, Camera camera)
        {
            var icon = this.GetIcon();
            if (icon != null)
                icon.Draw(sb, UI.UIManager.Mouse + new Vector2(icon.SourceRect.Width / 2, 0));
        }
     
        internal virtual void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
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
        
        public virtual void DrawBlockMouseover(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (this.Target == null)
                return;
            if (this.Target.Face == Vector3.Zero)
                return;

            Rectangle bounds = Block.Bounds;
            camera.GetEverything(map, this.Target.Global, bounds, out float cd, out Rectangle screenBounds, out Vector2 screenLoc);
            var scrbnds = camera.GetScreenBoundsVector4(this.Target.Global.X, this.Target.Global.Y, this.Target.Global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = this.Target.Global.GetDrawDepth(map, camera);
            var cdback = cd - 2; // TODO: why - 2???
            var highlight = Sprite.BlockHighlight; // WHY DO I USE AN ENTITY SPRITE INSTEAD OF A BLOCK TEXTURE IN THE BLOCK TEXTURE ATLAS???
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;

            var c = Color.White *.5f;

            sb.Draw(Sprite.BlockHightlightBack.AtlasToken.Atlas.Texture, screenLoc, Sprite.BlockHightlightBack.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cdback);
            sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom), 
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
        }
        public virtual void DrawTileHighlight(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (this.Target == null) 
                return;
            if (this.Target.Face == Vector3.Zero)
                return;
            Rectangle bounds = Block.Bounds;
            camera.GetEverything(map, this.Target.FaceGlobal, bounds, out float cd, out Rectangle screenBounds, out Vector2 screenLoc);

            //int x = (int)Math.Abs(this.Target.Face.X), y = (int)Math.Abs(this.Target.Face.Y), r = (int)camera.Rotation;
            //int z = (int)Math.Abs(this.Target.Face.Z);
            //int highlightIndex = (2 * z) + (1 - z) * ((x + r) % 2);
            //int frontback = InputState.IsKeyDown(Keys.RShiftKey) ? 1 : 0;

            cd = (this.Target.FaceGlobal).GetDrawDepth(map, camera);

            var rotatedIndex = this.Target.Face.Rotate(camera.Rotation);
            if (!Sprite.BlockFaceHighlights.ContainsKey(rotatedIndex))
                return;
            var highlight = Sprite.BlockFaceHighlights[rotatedIndex];
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            highlight.Draw(sb, screenLoc, Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            sb.Flush();
        }
       
        internal virtual void OnGameEvent(GameEvent e)
        {
          
        }

        internal virtual void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            ToolManager.DrawBlockMouseover(sb, map, camera, this.Target);
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
        internal virtual void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
        }

        internal virtual void DrawUIRemote(SpriteBatch sb, Camera camera, PlayerData pl)
        {
        }

        public virtual string HelpText { get; } = "";
        public virtual bool TargetOnlyBlocks { get; } = false;
    }
}
