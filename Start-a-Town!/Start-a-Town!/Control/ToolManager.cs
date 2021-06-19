using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Windows.Forms;
using Start_a_Town_.Rooms;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net;
using UI;

namespace Start_a_Town_
{
    public class ToolManager : IKeyEventHandler //, IDisposable
    {
        protected Stack<ControlTool> Tools;

        #region Singleton
        static ToolManager _Instance;
        public static ToolManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ToolManager();
                return _Instance;
            }
        }
        #endregion
        
        public ToolManager()
        {
            Tools = new Stack<ControlTool>();
        }
        static public ControlTool GetDefaultTool()
        {
            return new ToolManagement();
        }// PlayerControl.DefaultTool();
        ControlTool _ActiveTool;
        public ControlTool ActiveTool
        {
            get
            {
                if (this._ActiveTool == null)
                    this._ActiveTool = GetDefaultTool();
                return _ActiveTool;
            }// ?? this.DefaultTool; }
            set
            {
                _ActiveTool = value ?? GetDefaultTool();// this.DefaultTool;
                if (_ActiveTool != null)
                {
                    _ActiveTool.Manager = this;
                    //PacketPlayerToolSwitch.Send(Client.Instance, Client.Instance.PlayerData.ID, _ActiveTool);
                    var net = Rooms.Ingame.CurrentMap.Net;
                    PacketPlayerToolSwitch.Send(net, net.GetPlayer().ID, _ActiveTool);

                }
            }
        }
        public void Update(IMap map)
        {
            if (this.ActiveTool == null)
                SetTool(GetDefaultTool());// this.DefaultTool);// new PlayerControl.DefaultTool());
            ActiveTool.Update();
            foreach (var pl in GetOtherPlayers(map))
            {
                if (pl.CurrentTool != null)
                {
                    pl.CurrentTool.UpdateRemote(pl.Target);
                }
            }
        }
        public void Update(IMap map, SceneState scene)
        {
            if (ActiveTool != null)
                ActiveTool.Update(scene);
            foreach (var pl in GetOtherPlayers(map))
            {
                if (pl.CurrentTool != null)
                {
                    pl.CurrentTool.UpdateRemote(pl.Target);
                }
            }
        }

        public bool Add(ControlTool tool)
        {
            Tools.Push(tool);
            return true;
        }
        internal void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (ActiveTool == null)
                return;
            ActiveTool.DrawBeforeWorld(sb, map, camera);
            //if (ActiveTool.Icon != null)
            //    ActiveTool.Icon.Draw(sb, Controller.Instance.MouseLocation);
        }
        internal void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var camera = map.Camera;
            if (ActiveTool == null)
                return;
            ActiveTool.DrawAfterWorld(sb, map);
            DrawPlayersBlockMouseover(sb, map);
        }

        private static void DrawPlayersBlockMouseover(MySpriteBatch sb, IMap map)
        {
            var camera = map.Camera;
            foreach (var pl in GetOtherPlayers(map))
                if (pl.CurrentTool != null)
                {
                    ToolManager.DrawBlockMouseover(sb, map, camera, pl.Target, pl.Color);
                    //continue;
                    //pl.CurrentTool.DrawAfterWorldRemote(sb, map, camera, pl);
                }
        }
        internal void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        {
            if (ActiveTool == null)
                return;
            ActiveTool.DrawWorld(sb, map, camera);
            if (ActiveTool.Icon != null)
                ActiveTool.Icon.Draw(sb, Controller.Instance.MouseLocation);
        }

        internal void DrawUI(SpriteBatch sb, IMap map)
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
                    //pl.CurrentTool.DrawUIRemote(sb, camera, pl);
                    //pl.CurrentTool.DrawUIRemote(sb, camera, pl.GetMousePosition(camera), pl.Target, pl);
            }}
            sb.End();
        }
        internal void DrawPlayerMousePointers(SpriteBatch sb, IMap map)
        {
            var camera = map.Camera;
            foreach (var pl in GetOtherPlayers(map))
            {
                //var icon = pl.CurrentTool.GetIcon();
                pl.Target.Map = map;
                if(pl.Target.Type != TargetType.Null && pl.Target.Exists)
                {
                    Vector2 pos = GetSmoothedMousePosition(camera, pl);
                    //Icon.Cursor.Draw(sb, new Vector2((int)pos.X, (int)pos.Y));
                    Icon.CursorGrayscale.Draw(sb, new Vector2((int)pos.X, (int)pos.Y), pl.Color);

                    //pl.CurrentTool.GetIcon().Draw(sb, pos, new Vector2(.5f));
                    pl.CurrentTool.DrawIcon(sb, pos + new Vector2(Icon.Cursor.AtlasToken.Rectangle.Width, 0));
                    //UIManager.DrawStringOutlined(sb, string.Format("{0}: {1}", pl.Name, pl.CurrentTool.GetType().Name), pos, new Vector2(.5f,1));
                    UIManager.DrawStringOutlined(sb, pl.CurrentTool.GetType().Name, pos, new Vector2(.5f, 1));
                    UIManager.DrawStringOutlined(sb, pl.Name, pos, new Vector2(.5f, 2));
                }
                //continue;
                //var mousepos = pl.GetMousePosition(camera);
                //sb.Draw(UIManager.Cursor, mousepos, Color.White);
                //UIManager.DrawStringOutlined(sb, pl.Name, mousepos + new Vector2(UIManager.Cursor.Width, 0), Vector2.Zero);
            }
        }

        private static Vector2 GetSmoothedMousePosition(Camera camera, PlayerData pl)
        {
            //if (!pl.Target.Exists)
            //    return pl.LastPointer.Value;
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
            //if (e.Handled)
            //    return;
            if (ActiveTool != null)
                ActiveTool.HandleKeyUp(e);
            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            
            //if (pressed.Contains(GlobalVars.KeyBindings.Menu))//System.Windows.Forms.Keys.Escape))
            //    IngameMenu.Instance.Toggle();
        }


        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            //Controller.Input.UpdateKeyStates();


            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            
            if (pressed.Contains(System.Windows.Forms.Keys.F3))
                DebugWindow.Instance.Toggle();
            if (this.ActiveTool == null)
                return;
            //if (pressed.Contains(System.Windows.Forms.Keys.T))
            //    StructuresWindowOld.Instance.Toggle();

            //if (pressed.Contains(System.Windows.Forms.Keys.Space))
            //    this.ActiveTool.Jump();

            //if (pressed.Contains(GlobalVars.KeyBindings.Npcs))//GlobalVars.KeyBindings.Needs))
            //    NpcInfoWindow.Instance.Toggle();

            if (pressed.Contains(GlobalVars.KeyBindings.Needs))
                NeedsWindow.Toggle(PlayerOld.Actor);
            //if (pressed.Contains(GlobalVars.KeyBindings.Crafting))
            //    Start_a_Town_.Modules.Crafting.UI.WindowCrafting.Instance.Toggle();

            //if (pressed.Contains(System.Windows.Forms.Keys.J))
            //    //Towns.TownJobsWindow.Instance.Show(Client.Instance.Map.GetTown());//.Show();
            //    Towns.WindowTasks.Instance.Toggle(Server.Instance.Map.GetTown());//.Show();

            if (pressed.Contains(System.Windows.Forms.Keys.U))
                TestWindow.Instance.Toggle();

            if (e.KeyCode == KeyBind.BlockTargeting.Key)// (int)System.Windows.Forms.Keys.V)
            {
                if (this.ActiveTool != null)
                {
                    //this.ActiveTool.BlockTargeting = !this.ActiveTool.BlockTargeting;
                    //Client.Console.Write("Block targeting " + (this.ActiveTool.BlockTargeting ? "on" : "off"));
                    Controller.BlockTargeting = !Controller.BlockTargeting;
                    Client.Instance.Log.Write("Block targeting " + (Controller.BlockTargeting ? "on" : "off"));
                }
            }

            //if (pressed.Contains(GlobalVars.KeyBindings.Menu))//System.Windows.Forms.Keys.Escape))
            //    IngameMenu.Instance.Toggle();

            if (ActiveTool == null)
                return;

            if (pressed.Contains(GlobalVars.KeyBindings.Jump))
                ActiveTool.Jump();

            //if (pressed.Contains(GlobalVars.KeyBindings.Activate))
            //    ActiveTool.Activate();

            //if (pressed.Contains(GlobalVars.KeyBindings.Throw))// || pressed.Contains(System.Windows.Forms.Keys.MButton))
            //    ActiveTool.Throw();

            //if (pressed.Contains(GlobalVars.KeyBindings.PickUp))
            //    ActiveTool.PickUp();
            //if (pressed.Contains(GlobalVars.KeyBindings.Drop))
            //    ActiveTool.Drop();
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

            //Console.WriteLine("lb: " + DateTime.Now.ToString());

            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseLeftPressed(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool(); ;// null; //ActiveTool.PreviousTool;// null; //
        }

        public void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseLeftUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool();// null; //ActiveTool.PreviousTool;// null;
        }

        public void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (e.Handled)
            //    return;
            //Console.WriteLine("rb: " + DateTime.Now.ToString());
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseRightDown(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool(); ;// null; // ActiveTool.PreviousTool;// null;
        }

        public void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseRightUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool(); ;// null; //ActiveTool.PreviousTool;// null;
        }
        public void HandleMButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseMiddleUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool(); ;// null; //ActiveTool.PreviousTool;// null;
        }
        public void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseMiddleDown(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool(); ;// null; //ActiveTool.PreviousTool;// null;
        }
        public void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseMiddleUp(e) == ControlTool.Messages.Remove)
                ActiveTool = GetDefaultTool(); ;// null; //ActiveTool.PreviousTool;// null;
        }
        public void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool != null)
                this.ActiveTool.HandleMouseWheel(e);

        //    if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
        //        return;
        //    //Rooms.Ingame.Instance.Camera.DrawLevel = Math.Min(Map.MaxHeight - 1, Math.Max(0, Rooms.Ingame.Instance.Camera.DrawLevel + e.Delta));
        //    Rooms.Ingame.Instance.Camera.AdjustDrawLevel(InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta);
        //    e.Handled = true;
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

        internal static void OnGameEvent(GameEvent e)
        {
            if (Instance.ActiveTool != null)
                Instance.ActiveTool.OnGameEvent(e); 
            switch (e.Type)
            {
                case Message.Types.PlayerControlNpc:
                    if (e.Parameters[0] == Client.Instance.GetPlayer())
                    {
                        var actor = e.Parameters[1] as GameObject;
                        e.Net.Map.Camera.ToggleFollowing(actor);
                        if (actor != null)
                            SetTool(new ToolControlActor());
                        //e.Net.Map.Camera.ToggleFollowing(e.Parameters[1] as GameObject);
                        //SetTool(new ToolControlActor());
                    }
                    break;

                default:
                    break;
            }
          
        }

        static public void SetTool(ControlTool tool)
        {
            //if (tool.PreviousTool == ScreenManager.CurrentScreen.ToolManager.ActiveTool)
            //    throw new Exception();
            //tool.PreviousTool = ScreenManager.CurrentScreen.ToolManager.ActiveTool;
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = tool;
        }
        static public void Clear()
        {
            ScreenManager.CurrentScreen.ToolManager.ClearTool();
        }

        public static TargetArgs CurrentTarget
        {
            get
            {
                return (Instance.ActiveTool != null ? (Instance.ActiveTool.Target ?? TargetArgs.Null) : TargetArgs.Null);
            }
        }
        public static TargetArgs LastValidTarget
        {
            get
            {
                //return (Instance.ActiveTool != null ? (Instance.ActiveTool.Target ?? Instance.ActiveTool.TargetLast) : TargetArgs.Null);
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
        internal static void DrawBlockMouseover(MySpriteBatch sb, IMap map, Camera camera, TargetArgs target)
        {
            DrawBlockMouseover(sb, map, camera, target, Color.White);
        }
        internal static void DrawBlockMouseover(MySpriteBatch sb, IMap map, Camera camera, TargetArgs target, Color color)
        {
            if (target == null)
                return;
            if (target.Face == Vector3.Zero)
                return;

            Rectangle bounds = Block.Bounds;
            float cd;
            Rectangle screenBounds;
            Vector2 screenLoc;
            //camera.GetEverything(map, TargetOld.Global, bounds, out cd, out screenBounds, out screenLoc);
            //cd = TargetOld.Global.GetDrawDepth(map, camera);
            camera.GetEverything(map, target.Global, bounds, out cd, out screenBounds, out screenLoc);
            var scrbnds = camera.GetScreenBoundsVector4(target.Global.X, target.Global.Y, target.Global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = target.Global.GetDrawDepth(map, camera);
            var cdback = cd - 2;// (this.Target.Global - Vector3.One).GetDrawDepth(map, camera);
            var highlight = Sprite.BlockHighlight; // WHY DO I USE AN ENTITY SPRITE INSTEAD OF A BLOCK TEXTURE IN THE BLOCK TEXTURE ATLAS???
            Sprite.Atlas.Begin(sb);
            //highlight.Draw(sb, screenLoc, Color.White * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            //highlight.Draw(sb, screenLoc, Color.White, Color.White, Color.White, Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);
            //highlight.Draw(sb, screenLoc, Color.White, Color.White, Color.White, Color.Transparent, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);

            var c = color * .5f;//*0.8f;
            //c = new Color(1f, 1f, 1f, 0.66f);
            //sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom), Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            sb.Draw(Sprite.BlockHightlightBack.AtlasToken.Atlas.Texture, screenLoc, Sprite.BlockHightlightBack.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cdback);
            sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(camera.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);

            sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
        }

        private static IEnumerable<PlayerData> GetOtherPlayers(IMap map)
        {
            
            //return Client.Instance.GetPlayers();
            return map.Net.GetPlayers();


            //return Client.Instance.GetOtherPlayers();

        }
    }
}
