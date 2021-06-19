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

namespace Start_a_Town_.PlayerControl
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

        ControlTool _ActiveTool;
        public ControlTool ActiveTool
        {
            get { return _ActiveTool; }
            set
            {
                _ActiveTool = value;
            }
        }
        public void Update()
        {
            if (ActiveTool != null)
                ActiveTool.Update();
        }
        public void Update(SceneState scene)
        {
            if (ActiveTool != null)
                ActiveTool.Update(scene);
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
        internal void DrawAfterWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (ActiveTool == null)
                return;
            ActiveTool.DrawAfterWorld(sb, map, camera);
            //if (ActiveTool.Icon != null)
            //    ActiveTool.Icon.Draw(sb, Controller.Instance.MouseLocation);
        }
        internal void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        {


            if (ActiveTool == null)
                return;
            //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
        //    sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            ActiveTool.DrawWorld(sb, map, camera);
            if (ActiveTool.Icon != null)
                ActiveTool.Icon.Draw(sb, Controller.Instance.MouseLocation);
        //    sb.End();
        }

        internal void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            if (ActiveTool == null)
                return;
            sb.Begin();
            ActiveTool.DrawUI(sb, camera);
            sb.End();
        }
        
        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            //switch (e.KeyChar)
            //{
            //    case '/':
            //        LogWindow.Instance.TextBox.Text = "/";
            //        //LogWindow.Instance.Show();
            //        LogWindow.Instance.StartTyping();
            //        break;

            //    case '\r':
            //        LogWindow.Instance.StartTyping();
            //        break;

            //    //case 'n':
            //    //    NpcInfoWindow.Instance.Toggle();
            //    //    break;

            //    //case 'o':
            //    //    SpawningWindow.Instance.Toggle();
            //    //    break;

            //    //case ' ':
            //    //    if (ActiveTool != null)
            //    //        ActiveTool.Jump();
            //    //    break;

            //    default:
            //        break;
            //}

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
            if (pressed.Contains(System.Windows.Forms.Keys.T))
                StructuresWindowOld.Instance.Toggle();
            //if (pressed.Contains(System.Windows.Forms.Keys.Space))
            //    this.ActiveTool.Jump();
            if (pressed.Contains(GlobalVars.KeyBindings.Npcs))//GlobalVars.KeyBindings.Needs))
                NpcInfoWindow.Instance.Toggle();

            if (pressed.Contains(GlobalVars.KeyBindings.Needs))
                NeedsWindow.Toggle(Player.Actor);
            if (pressed.Contains(GlobalVars.KeyBindings.Crafting))
                Start_a_Town_.Modules.Crafting.UI.WindowCrafting.Instance.Toggle();

            if (pressed.Contains(System.Windows.Forms.Keys.J))
                Towns.TownJobsWindow.Instance.Show(Net.Client.Instance.Map.GetTown());//.Show();

            if (pressed.Contains(System.Windows.Forms.Keys.U))
                TestWindow.Instance.Toggle();

            if (e.KeyValue == (int)System.Windows.Forms.Keys.V)
            {
                if (this.ActiveTool != null)
                {
                    //this.ActiveTool.BlockTargeting = !this.ActiveTool.BlockTargeting;
                    //Net.Client.Console.Write("Block targeting " + (this.ActiveTool.BlockTargeting ? "on" : "off"));
                    Controller.BlockTargeting = !Controller.BlockTargeting;
                    Net.Client.Console.Write("Block targeting " + (Controller.BlockTargeting ? "on" : "off"));
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
                ActiveTool = null;
        }

        public void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseLeftUp(e) == ControlTool.Messages.Remove)
                ActiveTool = null;
        }

        public void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (e.Handled)
            //    return;
            //Console.WriteLine("rb: " + DateTime.Now.ToString());
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseRightDown(e) == ControlTool.Messages.Remove)
                ActiveTool = null;
        }

        public void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveTool == null)
                return;
            if (ActiveTool.MouseRightUp(e) == ControlTool.Messages.Remove)
                ActiveTool = null;
        }

        public void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
        //    if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
        //        return;
        //    //Rooms.Ingame.Instance.Camera.DrawLevel = Math.Min(Map.MaxHeight - 1, Math.Max(0, Rooms.Ingame.Instance.Camera.DrawLevel + e.Delta));
        //    Rooms.Ingame.Instance.Camera.AdjustDrawLevel(InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta);
        //    e.Handled = true;
        }

        internal void ClearTool()
        {
            this.ActiveTool = null;
        }

        internal static void OnGameEvent(GameEvent e)
        {
            if (Instance.ActiveTool != null)
                Instance.ActiveTool.OnGameEvent(e);
        }
    }
}
