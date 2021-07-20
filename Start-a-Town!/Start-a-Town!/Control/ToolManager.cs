using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using UI;

namespace Start_a_Town_
{
    public class ToolManager : IKeyEventHandler
    {
        protected Stack<ControlTool> Tools;

        static ToolManager _Instance;
        public static ToolManager Instance => _Instance ??= new ToolManager();
        
        public ToolManager()
        {
            Tools = new Stack<ControlTool>();
        }
        static public ControlTool GetDefaultTool()
        {
            return new ToolManagement();
        }
        ControlTool _ActiveTool;
        public ControlTool ActiveTool
        {
            get
            {
                if (this._ActiveTool == null)
                    this._ActiveTool = GetDefaultTool();
                return _ActiveTool;
            }
            set
            {
                _ActiveTool = value ?? GetDefaultTool();
                if (_ActiveTool != null)
                {
                    _ActiveTool.Manager = this;
                    var net = Rooms.Ingame.CurrentMap.Net;
                    PacketPlayerToolSwitch.Send(net, net.GetPlayer().ID, _ActiveTool);
                }
            }
        }
        
        public void Update(MapBase map, SceneState scene)
        {
            if (ActiveTool != null)
                ActiveTool.Update(scene);
            foreach (var pl in GetOtherPlayers(map))
                pl.CurrentTool?.UpdateRemote(pl.Target);
        }

        public bool Add(ControlTool tool)
        {
            Tools.Push(tool);
            return true;
        }
        internal void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (ActiveTool == null)
                return;
            ActiveTool.DrawBeforeWorld(sb, map, camera);
        }
        internal void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            if (ActiveTool == null)
                return;
            ActiveTool.DrawAfterWorld(sb, map);
            DrawPlayersBlockMouseover(sb, map);
        }

        private static void DrawPlayersBlockMouseover(MySpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            foreach (var pl in GetOtherPlayers(map))
                if (pl.CurrentTool != null)
                {
                    ToolManager.DrawBlockMouseover(sb, map, camera, pl.Target, pl.Color);
                }
        }
        
        internal void DrawUI(SpriteBatch sb, MapBase map)
        {
            if (ActiveTool == null)
                return;
            sb.Begin();
            var camera = map.Camera;
            this.DrawPlayerMousePointers(sb, map);
            this.ActiveTool.DrawUI(sb, camera);
            foreach (var pl in GetOtherPlayers(map))
            {
                if (pl.CurrentTool != null)
                {
                    if (pl.Target.Type == TargetType.Entity && pl.Target.Exists)
                        pl.Target.Object.GetScreenBounds(camera).DrawHighlightBorder(sb, pl.Color, camera.Zoom);
            }}
            sb.End();
        }
        internal void DrawPlayerMousePointers(SpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            foreach (var pl in GetOtherPlayers(map))
            {
                pl.Target.Map = map;
                if(pl.Target.Type != TargetType.Null && pl.Target.Exists)
                {
                    Vector2 pos = GetSmoothedMousePosition(camera, pl);
                    Icon.CursorGrayscale.Draw(sb, new Vector2((int)pos.X, (int)pos.Y), pl.Color);
                    pl.CurrentTool.DrawIcon(sb, pos + new Vector2(Icon.Cursor.AtlasToken.Rectangle.Width, 0));
                    UIManager.DrawStringOutlined(sb, pl.CurrentTool.GetType().Name, pos, new Vector2(.5f, 1));
                    UIManager.DrawStringOutlined(sb, pl.Name, pos, new Vector2(.5f, 2));
                }
            }
        }

        private static Vector2 GetSmoothedMousePosition(Camera camera, PlayerData pl)
        {
            Vector2 pos = camera.GetScreenPosition(pl.Target);
            if (pl.LastPointer.HasValue)
            {
                var lastpos = pl.LastPointer.Value;
                var diff = pos - lastpos;
                pos = (lastpos + diff * .1f);
            }
            pl.LastPointer = pos;
            return pos;
        }

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.Handled)
                return;

            if (ActiveTool != null)
                ActiveTool.HandleKeyPress(e);
        }

        public void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (ActiveTool != null)
                ActiveTool.HandleKeyUp(e);
            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
        }

        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            
            if (pressed.Contains(System.Windows.Forms.Keys.F3))
                DebugWindow.Instance.Toggle();
            if (this.ActiveTool == null)
                return;
            
            if (e.KeyCode == KeyBind.BlockTargeting.Key)
            {
                if (this.ActiveTool != null)
                {
                    Controller.BlockTargeting = !Controller.BlockTargeting;
                    Client.Instance.Log.Write("Block targeting " + (Controller.BlockTargeting ? "on" : "off"));
                }
            }

            if (ActiveTool == null)
                return;

            if (pressed.Contains(GlobalVars.KeyBindings.Jump))
                ActiveTool.Jump();

            ActiveTool.HandleKeyDown(e);
        }

        public void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            this.ActiveTool.HandleMouseMove(e);
        }

        public void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseLeftPressed(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }

        public void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseLeftUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }

        public void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseRightDown(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }

        public void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseRightUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }
        public void HandleMButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseMiddleUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }
        public void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseMiddleDown(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }
        public void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseMiddleUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();
        }
        public void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool != null)
                this.ActiveTool.HandleMouseWheel(e);
        }
        public void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool != null)
                this.ActiveTool.HandleLButtonDoubleClick(e);
        }

        internal void ClearTool()
        {
            this.ActiveTool = null;
        }
        static readonly ToolControlActor ToolControlActor = new();
        internal static void OnGameEvent(INetwork net, GameEvent e)
        {
            if (Instance.ActiveTool != null)
                Instance.ActiveTool.OnGameEvent(e); 
            switch (e.Type)
            {
                case Message.Types.PlayerControlNpc:
                    if (e.Parameters[0] == Client.Instance.GetPlayer())
                    {
                        var actor = e.Parameters[1] as GameObject;
                        net.Map.Camera.ToggleFollowing(actor);
                        if (actor is not null)
                            SetTool(ToolControlActor);
                    }
                    break;

                default:
                    break;
            }
          
        }

        static public void SetTool(ControlTool tool)
        {
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = tool;
        }
        static public void Clear()
        {
            ScreenManager.CurrentScreen.ToolManager.ClearTool();
        }

        public static TargetArgs CurrentTarget => Instance.ActiveTool != null ? (Instance.ActiveTool.Target ?? TargetArgs.Null) : TargetArgs.Null;
         
        public static TargetArgs LastValidTarget
        {
            get
            {
                if (Instance.ActiveTool != null)
                {
                    if (Instance.ActiveTool.Target != null && Instance.ActiveTool.Target.Type != TargetType.Null)
                        return Instance.ActiveTool.Target;
                    else
                        return Instance.ActiveTool.TargetLast;
                }
                return null;
            }
        }
        internal static void DrawBlockMouseover(MySpriteBatch sb, MapBase map, Camera camera, TargetArgs target)
        {
            DrawBlockMouseover(sb, map, camera, target, Color.White);
        }
        internal static void DrawBlockMouseover(MySpriteBatch sb, MapBase map, Camera camera, TargetArgs target, Color color)
        {
            if (target == null)
                return;
            if (target.Face == Vector3.Zero)
                return;

            Rectangle bounds = Block.Bounds;
            float cd;
            Rectangle screenBounds;
            Vector2 screenLoc;
            camera.GetEverything(map, target.Global, bounds, out cd, out screenBounds, out screenLoc);
            var scrbnds = camera.GetScreenBoundsVector4(target.Global.X, target.Global.Y, target.Global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = target.Global.GetDrawDepth(map, camera);
            var cdback = cd - 2; // TODO: why -2?
            var highlight = Sprite.BlockHighlight; // WHY DO I USE AN ENTITY SPRITE INSTEAD OF A BLOCK TEXTURE IN THE BLOCK TEXTURE ATLAS???
            Sprite.Atlas.Begin(sb);

            var c = color * .5f;

            sb.Draw(Sprite.BlockHightlightBack.AtlasToken.Atlas.Texture, screenLoc, Sprite.BlockHightlightBack.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cdback);
            sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
        }

        private static IEnumerable<PlayerData> GetOtherPlayers(MapBase map)
        {
            return map.Net.GetPlayers();
        }
    }
}
