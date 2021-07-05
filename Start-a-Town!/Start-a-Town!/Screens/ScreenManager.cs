using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;
using Start_a_Town_.UI;
using System.Windows.Forms;

namespace Start_a_Town_
{
    public class ScreenManager
    {
        public UIManager WindowManager;

        public static Stack<GameScreen> GameScreens = new Stack<GameScreen>();
        static ScreenManager _Instance;
        static TextInputHandler TextInput;
        public static ScreenManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ScreenManager();
                return _Instance;
            }
        }

        static public void LoadContent()
        {
            MainScreen.LoadContent();
        }

        static public void Initialize()
        {
            Game1.TextInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(Instance.TextInput_KeyPress);
            Game1.TextInput.KeyDown += new System.Windows.Forms.KeyEventHandler(Instance.TextInput_KeyDown);
            Game1.TextInput.KeyUp += new System.Windows.Forms.KeyEventHandler(Instance.TextInput_KeyUp);
            Game1.TextInput.MouseMove += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_MouseMove);
            Game1.TextInput.LButtonDown += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_LButtonDown);
            Game1.TextInput.LMouseUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_LMouseUp);
            Game1.TextInput.RMouseDown += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_RMouseDown);
            Game1.TextInput.RMouseUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_RMouseUp);
            Game1.TextInput.MMouseUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_MiddleUp);
            Game1.TextInput.MMouseDown += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_MiddleDown);
            Game1.TextInput.MouseWheel += new EventHandler<HandledMouseEventArgs>(TextInput_MouseWheel);
            Game1.TextInput.LButtonDblClk += new EventHandler<HandledMouseEventArgs>(TextInput_LButtonDblClk);
        }

        private static void TextInput_LButtonDblClk(object sender, HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().HandleLButtonDoubleClick(e);
        }

        static void TextInput_RMouseUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            //DragDropManager.Instance.HandleRButtonUp(e);
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().HandleRButtonUp(e);
        }

        static void TextInput_MouseWheel(object sender, HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().HandleMouseWheel(e);
        }
        static void TextInput_MiddleDown(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().HandleMiddleDown(e);
        }
        static void TextInput_MiddleUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().HandleMiddleUp(e);
        }
        static void TextInput_RMouseDown(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            DragDropManager.Instance.HandleRButtonDown(e);
            GameScreens.Peek().HandleRButtonDown(e);
        }

        static void TextInput_LMouseUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            DragDropManager.Instance.HandleLButtonUp(e);
            GameScreens.Peek().HandleLButtonUp(e);
        }

        static void TextInput_LButtonDown(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            DragDropManager.Instance.HandleLButtonDown(e);
            GameScreens.Peek().HandleLButtonDown(e);
        }

        static void TextInput_MouseMove(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().HandleMouseMove(e);
        }

        void TextInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            //foreach (IKeyEventHandler handler in KeyHandlers)
            //    handler.HandleInput(e);
            GameScreens.Peek().HandleKeyDown(e);
        }

        void TextInput_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            //foreach (IKeyEventHandler handler in KeyHandlers)
            //    handler.HandleInput(e);
            GameScreens.Peek().HandleKeyUp(e);
        }

        void TextInput_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (GameScreens.Count == 0)
                return;
            //foreach (IKeyEventHandler handler in KeyHandlers)
            //    handler.HandleInput(e);
            GameScreens.Peek().HandleKeyPress(e);
        }

        public ScreenManager()
        {
            //Input = new InputState();
            //GameScreens = new Stack<GameScreen>();

            //GameScreens.Push(new MainScreen());

            this.WindowManager = new UIManager();
        }

        public static T GetCurrent<T>() where T : GameScreen
        {
            return GameScreens.Count > 0 ? GameScreens.Peek() as T : null;
        }

        public static GameScreen Current
        {
            get { return GameScreens.Count > 0 ? GameScreens.Peek() : null; }
        }

        static public GameScreen CurrentScreen
        {
            get
            {
                if (GameScreens.Count == 0)
                    return null;
                return GameScreens.Peek();
            }
        }

        static public InputState Input = new InputState();
        public void Update(Game1 game, GameTime gt)
        {
            if (GameScreens.Count == 0)
            {
                GameScreens.Push(MainScreen.Instance);//new MainScreen());
                GameScreens.Peek().Initialize(null);
            }
            GameScreen screen = GameScreens.Peek();
            TooltipManager.Instance.Update();
            DragDropManager.Instance.Update();

            screen.Update(game, gt);
            //screen.HandleInput(Input);
            //TooltipManager.Instance.Update();

            if (!Game1.Instance.IsActive)
                return;

          //  Input.Update();
            Controller.Input.Update();
            this.WindowManager.Update(game, gt);
           // HandleInput(Input);
        }

        //public void HandleInput(InputState input)
        //{
        //    GameScreens.Peek().HandleInput(input);
        //}

        public static bool Add(GameScreen screen)
        {

            //if (GameScreens.Count > 0)
            //    foreach (GameScreen scr in GameScreens)
            //        scr.WindowManager.Dispose();
            GameScreens.Push(screen);

            return true;
        }

        public void Draw(SpriteBatch sb)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().Draw(sb);
            //this.WindowManager.Draw(sb); // why was i drawing this here? why does screenmanager have a windowmanager?
        }

        
        /// <summary>
        /// Removes current screen
        /// </summary>
        /// <returns></returns>
        public static bool Remove()
        {
            if (GameScreens.Count == 1)
                return false;
            GameScreens.Pop().Dispose();
            return true;
        }
    }
}
