using System;
using System.Collections.Generic;
//using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;
using System.Runtime.InteropServices;
using UI;

namespace Start_a_Town_
{
    public class Mouseover
    {
        public bool Valid;
        object _Object;
        public object Object
        {
            get
            {
                return _Object;
            }
            set
            {
                this._Object = value;
                if (value is Slot)
                {
                    this.Target = new TargetArgs((value as Slot).Tag);
                }
                else if (value is GameObject)
                { 
                    this.Target = new TargetArgs(value as GameObject);
                    this.TargetEntity = this.Target;
                }
                else if (value is TargetArgs)
                {
                    var target = value as TargetArgs;
                    if (target.Type == TargetType.Position)
                        this.TargetCell = target;
                    else if (target.Type == TargetType.Entity)
                        this.Target = target;
                }
            }
        }
        public bool Multifaceted;
        public Vector3 Face;
        public Vector3 Precise;
        public TargetArgs Target, TargetCell, TargetEntity;
        public float Depth = float.MinValue;//1;


        public bool TryGet<T>(out T obj) where T : class
        {
            obj = this.Object as T;
            return obj is T;
        }
        public override string ToString()
        {
            return Object != null ? Object.ToString() : "<null>";
        }
    }

    public class Mouseover<T> 
    {
        public T Object;// GameObject Object;
        public bool Multifaceted;
        public Color Face;
        public float Depth = 1;

        //public void Set(object obj)
        //{
        //    this.Object = obj;
        //}
        public bool TrySet(float depth, T obj, Color face)
        {
            if (depth > this.Depth)
                return false;
            //    Console.WriteLine(depth + " this." + this.Depth);
            this.Object = obj;
            this.Depth = depth;
            this.Face = face;
            return true;
        }

        //public bool TryGet(out object obj)
        //{
        //    obj = this.Object;
        //    return obj != null;
        //}

        public bool TryGet(out T obj)
        {
            obj = this.Object;
            return obj is T;
        }

        //public void Reset()
        //{ Object = null; }
    }

    public class MouseoverEventArgs : EventArgs
    {
        public Object ObjectNext, ObjectLast;
        public MouseoverEventArgs(Object objNext, Object objLast)
        {
            ObjectNext = objNext;
            ObjectLast = objLast;
        }
    }

    public enum MouseButtons { Left, Right, Middle }

    public enum MessageID{
            KeyDown, KeyUp}

    public class KeyEventArgs2 : EventArgs
    {
        public Keys[] KeysNew, KeysOld;
        public KeyEventArgs2(Keys[] keysnew, Keys[] keysold)
        {
            KeysNew = keysnew;
            KeysOld = keysold;
        }
    }

    public class KeyPressEventArgs2 : EventArgs
    {
        public Keys Key;
        public InputState Input;
        public KeyPressEventArgs2(Keys key, InputState input)
        {
            Key = key;
            Input = input;
        }
    }

    public class InputState : EventArgs
    {
        bool _Handled;
        public bool Handled
        {
            get { return _Handled; }
            set { _Handled = value; }
        }
        public bool KeyHandled;

        [DllImport("user32.dll")]
        static extern short GetKeyState(int key);
        [DllImport("user32.dll")]
        static public extern bool GetKeyboardState(byte[] lpKeyState);

        static public bool IsKeyDown(System.Windows.Forms.Keys key)
        {
            return GetKeyState((int)key) < 0;
        }
        //static extern short GetKeyboardState(

        public byte[] KeyState = new byte[256], LastKeyState;
        public Vector2 LastMouse;
        public Vector2 CurrentMouse;// { get { return new Vector2(System.Windows.Forms.Control.MousePosition.X - Game1.Instance.Window.ClientBounds.X, System.Windows.Forms.Control.MousePosition.Y - Game1.Instance.Window.ClientBounds.Y); } }
        public KeyboardState CurrentKeyboardState, LastKeyboardState;
        public MouseState CurrentMouseState, LastMouseState;
        //public bool IsKeyDown()
        //{
        //    Keys[] keys = CurrentKeyboardState.GetPressedKeys();
        //    if (keys.Length > 0)
        //        return true;
        //    return false;
        //}
        public bool GetKeyDown(System.Windows.Forms.Keys key)
        {
            int i = (int)key;
            byte b = this.KeyState[i];
            return ((b & 0x80) != 0);
        }
        public bool IsKeyDown(Keys key)
        {
         //   return CurrentKeyboardState.IsKeyDown(key);
            byte b = this.KeyState[(int)key];
            return ((b & 0x80) != 0);
        }
        public bool IsKeyPressed(Keys key)
        {
            //if (CurrentKeyboardState.IsKeyDown(key))
            //    return LastKeyboardState.IsKeyUp(key);

            byte b = this.KeyState[(int)key];
            if ((b & 0x80) != 0)
                return ((this.LastKeyState[(int)key] & 0x80) == 0);

            //byte[] keys = new byte[256];
            //GetKeyboardState(keys);

            return false;
        }
        public bool IsKeyReleased(Keys key)
        {
            //if (LastKeyboardState.IsKeyDown(key))
            //    return CurrentKeyboardState.IsKeyUp(key);
            if ((this.KeyState[(int)key] & 0x80) == 0)
                return ((this.LastKeyState[(int)key] & 0x80) != 0);
            return false;
        }
        public bool LeftButtonPressed
        {
            get { return CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton != ButtonState.Pressed; }
        }
        public bool LeftButtonReleased
        {
            get { return CurrentMouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton != ButtonState.Released; }
        }
        public bool RightButtonPressed
        {
            get { return CurrentMouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton != ButtonState.Pressed; }
        }
        public bool RightButtonReleased
        {
            get { return CurrentMouseState.RightButton == ButtonState.Released && LastMouseState.RightButton != ButtonState.Released; }
        }
        public List<System.Windows.Forms.Keys> GetPressedKeys()
        {
           // UpdateKeyStates();
            List<System.Windows.Forms.Keys> keys = new List<System.Windows.Forms.Keys>();
            for (int i = 0; i < 256; i++)
            {
                byte current = this.KeyState[i];
                byte last = this.LastKeyState[i];
                if ((current & 0x80) != 0)
                    if ((last & 0x80) == 0)
                        keys.Add((System.Windows.Forms.Keys)i);
            }
            return keys;
        }
        public List<Keys> GetReleasedKeys()
        {
            List<Keys> keys = new List<Keys>();
            for (int i = 0; i < 256; i++)
            {
                byte current = this.KeyState[i];
                byte last = this.LastKeyState[i];
                if ((current & 0x80) == 0)
                    if ((last & 0x80) != 0)
                        keys.Add((Keys)i);
            }
            return keys;
        }
        public void UpdateKeyStates()
        {
            this.LastKeyState = this.KeyState;
            GetKeyState(0);
            this.KeyState = new byte[256];
            GetKeyboardState(this.KeyState);
        }
        public void Update()
        {
          //  UpdateKeyStates();
            KeyHandled = false;
            Handled = false;
            LastKeyboardState = CurrentKeyboardState;
            LastMouseState = CurrentMouseState;
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
            this.LastMouse = this.CurrentMouse;
           this.CurrentMouse = new Vector2(System.Windows.Forms.Control.MousePosition.X - Game1.Instance.Window.ClientBounds.X, System.Windows.Forms.Control.MousePosition.Y - Game1.Instance.Window.ClientBounds.Y);
        }
    }

    //public struct InputMessage
    //{
    //    IInputHandler Handler;
    //    MessageID Msg;
    //    int Param;

    //    public InputMessage(IInputHandler handler, MessageID msg, int param)
    //    {
    //        Handler = handler;
    //        Msg = msg;
    //        Param = param;
    //    }
    //}

    public class Controller
    {
       // static public TextInputHandler TextInput;
        static public bool GetMouseover<T>(out T obj) where T : GameObject
        {
            return Instance.MouseoverBlock.TryGet<T>(out obj);
           // return Instance.Mouseover.Object as T;
        }
        //static public Stack<IInputHandler> InputHandlers = new Stack<IInputHandler>();

        static public event EventHandler<MouseoverEventArgs> MouseoverObjectChanged;
        public void OnMouseoverObjectChanged(MouseoverEventArgs e)
        {
            //if (MouseoverLast != null)
            //    Console.Write(MouseoverLast);
            //if (MouseoverNext != null)
            //    Console.WriteLine(MouseoverNext);
            //Console.WriteLine("ASD");
           // Console.WriteLine(e.ObjectLast + "|" + e.ObjectNext);
            if (MouseoverObjectChanged != null)
                MouseoverObjectChanged(this, e);
        }

        //public Mouseover Mouseover, MouseoverNext;
        //public Mouseover MouseoverEntityNext;
        //public Mouseover MouseoverEntity;
        //Mouseover _Mouseover, _MouseoverNext;
        public Mouseover MouseoverEntity, MouseoverEntityNext;

        public Entity GetMouseoverEntity()
        {
            return this.MouseoverEntity.Object as Entity;
        }
        public Mouseover MouseoverBlock;
        //{
        //    get { return _Mouseover; }
        //    set
        //    {
        //        _Mouseover = value;
        //    }
        //}
        public Mouseover MouseoverBlockNext;
        //{
        //    get { return _MouseoverNext; }
        //    set
        //    {
        //        _MouseoverNext = value;
        //    }
        //}
        public bool BuildMode;
        //ITool Tool;

        public Rectangle MouseRect;
        //{
        //    get { return new Rectangle(msCurrent.X, msCurrent.Y, 1, 1); }
        //}



       // public Stack<ITool> ToolStack; //control is passed to whichever tool is ontop of the stack (buildtool, walktool etc)
        //public event MouseEventHandler MouseClick;
        //void OnMouseClick()
        //{
        //    if (MouseClick != null)
        //        MouseClick(this, new MouseEventArgs(msCurrent));
        //}
        public KeyboardState ksCurrent, ksPrevious;
        public MouseState msCurrent, msPrevious;
        public Game1 game;


        static Controller _Instance;
        public static Controller Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Controller();
                return _Instance;
            }            
        }


        // events
        public static event InputEvent MouseLeftPress, MouseLeftRelease, MouseLeftDown, MouseWheelUp, MouseWheelDown, EscapeDown,
            MouseRightPress, MouseRightDown, MouseRightRelease; //, onMsR;


        public static event EventHandler<EventArgs> MouseOverEntityChanged, MouseOverTileChanged, KeyPressed;
        public static event EventHandler<KeyEventArgs2> KeyDown, KeyUp;
        public static event EventHandler<KeyPressEventArgs2> KeyPress2;
        public static event EventHandler<System.Windows.Forms.KeyPressEventArgs> KeyPress;

        protected void OnKeyPressed()
        {
            if (KeyPressed != null)
                KeyPressed(this, EventArgs.Empty);
        }
        protected void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (KeyPress != null)
                KeyPress(this, e);
        }
        protected void OnKeyPress(KeyPressEventArgs2 e)
        {
            if (KeyPress2 != null)
                KeyPress2(this, e);
        }
        protected void OnKeyUp(Keys[] keysnew, Keys[] keysold)
        {
            if (KeyUp != null)
                KeyUp(this, new KeyEventArgs2(keysnew, keysold));
        }
        protected void OnKeyDown(Keys[] keysnew, Keys[] keysold)
        {
            if (KeyDown != null)
                KeyDown(this, new KeyEventArgs2(keysnew, keysold));
        }
        protected void OnMouseOverTileChanged()
        {
            if (MouseOverTileChanged != null)
                MouseOverTileChanged(this, EventArgs.Empty);
        }
        protected void OnMouseOverEntityChanged()
        {
            if (MouseOverEntityChanged != null)
                MouseOverEntityChanged(this, EventArgs.Empty);
        }

        //public static Vector2 Location
        //{
        //    get { return new Vector2(X, Y); }
        //}

        //public static int X
        //{
        //    get { return msCurrent.X; }
        //}
        //public static int Y
        //{
        //    get { return msCurrent.Y; }
        //}

        static public InputState Input = new InputState();
        public Controller()
        {
            //Input = new InputState();
            MouseoverBlock = new Mouseover();
            MouseoverBlockNext = new Mouseover();
            MouseoverEntity = new();
            MouseoverEntityNext = new();

            //this.MouseoverEntityNext = new Mouseover();
            //this.MouseoverEntity = new Mouseover();

            //  TextInput = new TextInputHandler(Game1.Instance.Window.Handle);
            //   TextInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TextInput_KeyPress);
            //   TextInput.KeyDown += new System.Windows.Forms.KeyEventHandler(TextInput_KeyDown);
            //   TextInput.KeyUp += new System.Windows.Forms.KeyEventHandler(TextInput_KeyUp);
        }

        //void TextInput_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //void TextInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //void TextInput_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        //{
        //    OnKeyPress(e);
        //}

        public void OnMouseLeftPress()
        {
            if (MouseLeftPress != null)
                MouseLeftPress();
        }

        public void OnMouseLeftDown()
        {
            if (MouseLeftDown != null)
                MouseLeftDown();
        }
        public void OnMouseLeftRelease()
        {
            if (MouseLeftRelease != null)
                MouseLeftRelease();
        }
        public void OnMouseRightPress()
        {
            if (MouseRightPress != null)
                MouseRightPress();
        }
        public void OnMouseRightDown()
        {
            if (MouseRightDown != null)
                MouseRightDown();
        }
        public void OnMouseRightRelease()
        {
            if (MouseRightRelease != null)
                MouseRightRelease();
        }


        void GetInputStates()
        {
            ksCurrent = Keyboard.GetState();
            msCurrent = Mouse.GetState();
        }
        bool CheckMouseMove()
        {
            return (!(msCurrent.X == msPrevious.X && msCurrent.Y == msPrevious.Y));  
        }

        public Vector2 MouseLocation
        { get { return new Vector2(msCurrent.X, msCurrent.Y); } }
        //Stack<IInputHandler> Handlers;
        public Mouseover GetMouseover()
        {
            //return MouseoverEntity.Depth > Mouseover.Depth ? MouseoverEntity : Mouseover;
            //return MouseoverEntity.Target ?? Mouseover.Target;
            return MouseoverEntity.Target != null ? MouseoverEntity : MouseoverBlock;

        }
        public void Update()
        {
            //if (!Game1.Instance.IsActive)
            //  return;
            AltDown = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            UpdateMousovers();
            //UpdateMousovers(ref MouseoverBlock, ref MouseoverBlockNext);
            //UpdateMousovers(ref MouseoverEntity, ref MouseoverEntityNext);


            //this.MouseoverEntity = this.MouseoverEntityNext;
            //this.MouseoverEntityNext = new Start_a_Town_.Mouseover();

            GetInputStates();
            this.MouseRect = new Rectangle(msCurrent.X, msCurrent.Y, 1, 1);


            if (ksCurrent.GetPressedKeys().Count() > 0)
                OnKeyPressed();

            {

                if (msCurrent.LeftButton == ButtonState.Pressed)
                {
                    if (msPrevious.LeftButton == ButtonState.Released)
                        OnMouseLeftPress();
                    else
                        OnMouseLeftDown();
                }
                else if (msCurrent.LeftButton == ButtonState.Released && msPrevious.LeftButton == ButtonState.Pressed)
                    OnMouseLeftRelease();

                if (msCurrent.RightButton == ButtonState.Pressed)
                {
                    if (msPrevious.RightButton == ButtonState.Released)
                        OnMouseRightPress();
                    else
                        OnMouseRightDown();
                }
                else if (msCurrent.RightButton == ButtonState.Released && msPrevious.RightButton == ButtonState.Pressed)
                    OnMouseRightRelease();

                if (msCurrent.ScrollWheelValue > msPrevious.ScrollWheelValue)
                    OnMouseWheelUp();
                else if (msCurrent.ScrollWheelValue < msPrevious.ScrollWheelValue)
                    OnMouseWheelDown();



                if (ksCurrent.IsKeyDown(Keys.LeftAlt) || ksCurrent.IsKeyDown(Keys.RightAlt))
                    if (KeyPressCheck(Keys.Enter))
                        Game1.Instance.graphics.ToggleFullScreen();

                Keys[] keysnew = ksCurrent.GetPressedKeys();
                Keys[] keysold = ksPrevious.GetPressedKeys();
                if (keysnew.Length > 0)
                    OnKeyDown(keysnew, keysold);
                if (keysnew.Length < keysold.Length)
                    OnKeyUp(keysnew, keysold);
            }
            ksPrevious = ksCurrent;
            msPrevious = msCurrent;
        }
        private void UpdateMousovers(ref Mouseover current, ref Mouseover next)
        {
            object mouseover, mouseoverNext;
            current.TryGet(out mouseover);
            next.TryGet(out mouseoverNext);

            if (mouseover != mouseoverNext)
            {
                OnMouseoverObjectChanged(new MouseoverEventArgs(mouseoverNext, mouseover));
                GameObject objLast = mouseover as GameObject;
                GameObject objNext = mouseoverNext as GameObject;
                if (objLast != null)
                    objLast.FocusLost();
                if (objNext != null)
                    objNext.Focus();
            }
            if (next.Valid) // i added this here to fix mouseover flickering but it caused other problems
            {
                //Mouseover = MouseoverNext;
                //MouseoverNext = new Start_a_Town_.Mouseover();
            }
            current = next;
            next = new Mouseover();
        }
        private void UpdateMousovers()
        {
            object mouseover, mouseoverNext;
            MouseoverBlock.TryGet(out mouseover);
            MouseoverBlockNext.TryGet(out mouseoverNext);

            if (mouseover != mouseoverNext)
            {
                OnMouseoverObjectChanged(new MouseoverEventArgs(mouseoverNext, mouseover));
                GameObject objLast = mouseover as GameObject;
                GameObject objNext = mouseoverNext as GameObject;
                if (objLast != null)
                    objLast.FocusLost();
                if (objNext != null)
                    objNext.Focus();
            }
            if (MouseoverBlockNext.Valid) // i added this here to fix mouseover flickering but it caused other problems
            {
                //Mouseover = MouseoverNext;
                //MouseoverNext = new Start_a_Town_.Mouseover();
            }
            MouseoverBlock = MouseoverBlockNext;
            MouseoverBlockNext = new Start_a_Town_.Mouseover();
        }

        private void OnEscapeDown()
        {
            if (EscapeDown != null)
                EscapeDown();//this, EventArgs.Empty);
        }

        void OnMouseWheelUp()
        {
            if (MouseWheelUp != null)
                MouseWheelUp();
        }
        void OnMouseWheelDown()
        {
            if (MouseWheelDown != null)
                MouseWheelDown();
        }
        public bool KeyPressCheck(Keys key)
        {
            return ksCurrent.IsKeyDown(key) && ksPrevious.IsKeyUp(key);
        }

        //public static Tile GetTopMostTile()
        //{
        //    //determine clicked tile if any
        //    if (ClickedTiles.Count > 0)
        //    {
        //        Tile clicked = null;
        //        float max_depth = Map.MinDepth;
        //        foreach (Tile _tile in ClickedTiles)
        //            if (_tile.Depth > max_depth)
        //            {
        //                max_depth = _tile.Depth;
        //                clicked = _tile;
        //            }
        //        ClickedTiles.Clear();
        //        return clicked;
        //    }
        //    return null;
        //}

        public Keys[] GetKeys()
        {
            return ksCurrent.GetPressedKeys();
        }

        static public void TrySetMouseoverEntity(Camera camera, GameObject entity, Vector3 face, float drawdepth)
        {
            var global = entity.Global;
            if (!camera.IsDrawable(global))
                return;
            float mouseoverDepth = drawdepth;// global.GetMouseoverDepth(entity.Map, camera);
            //if (mouseoverDepth >= Instance.MouseoverEntityNext.Depth)
            //{
            //    if (Instance.MouseoverEntityNext.Object is TargetArgs target)
            //    {
            //        if (target.Object != entity)
            //            target = new TargetArgs(entity, face) { Map = Engine.Map };
            //    }
            //    else
            //        target = new TargetArgs(entity, face) { Map = Engine.Map };

            //    Instance.MouseoverEntityNext.Target = target;
            //    Instance.MouseoverEntityNext.Object = target;// entity;
            //    Instance.MouseoverEntityNext.Face = face;
            //    Instance.MouseoverEntityNext.Depth = drawdepth; //mouseoverDepth;// why???
            //}
            //return;
            if (mouseoverDepth >= Controller.Instance.MouseoverBlockNext.Depth)
            {
                if (Controller.Instance.MouseoverBlock.Object is TargetArgs target)
                {
                    if (target.Object != entity)
                        target = new TargetArgs(entity, face) { Map = Engine.Map };
                }
                else
                    target = new TargetArgs(entity, face) { Map = Engine.Map };

                Controller.Instance.MouseoverBlockNext.Target = target;
                Controller.Instance.MouseoverBlockNext.Object = target;// entity;
                Controller.Instance.MouseoverBlockNext.Face = face;
                Controller.Instance.MouseoverBlockNext.Depth = drawdepth; //mouseoverDepth;// why???
            }
        }
        static public void SetMouseoverBlock(Camera camera, IMap map, Vector3 global, Vector3 face, Vector3 precise)
        {
            //var target = new TargetArgs(global, rotVec, precise) { Network = map.GetNetwork() };
            //if (global != this.LastMouseover)
            //    Controller.Instance.MouseoverNext.Object = target;// tar;
            //else
            //    Controller.Instance.MouseoverNext.Object = Controller.Instance.Mouseover.Object;
            //Controller.Instance.MouseoverNext.Face = rotVec;// faceColor;
            //Controller.Instance.MouseoverNext.Precise = precise;
            //Controller.Instance.MouseoverNext.Target = target;// new TargetArgs(global, rotVec, precise);
            //Controller.Instance.MouseoverNext.Depth = global.GetMouseoverDepth(map, this);

            //var target = new TargetArgs(global, face, precise) { Network = map.Net, Map = map };
            var target = new TargetArgs(map, global, face, precise);// { Map = map };

            if (global != _LastMouseoverBlockGlobal || Controller.Instance.MouseoverBlock.Object is Element) // very hacky
                Controller.Instance.MouseoverBlockNext.Object = target;// tar;
            else
                Controller.Instance.MouseoverBlockNext.Object = Controller.Instance.MouseoverBlock.Object;
            Controller.Instance.MouseoverBlockNext.Face = face;// faceColor;
            Controller.Instance.MouseoverBlockNext.Precise = precise;
            Controller.Instance.MouseoverBlockNext.Target = target;// new TargetArgs(global, rotVec, precise);
            Controller.Instance.MouseoverBlockNext.Depth = global.GetDrawDepth(map, camera);// global.GetMouseoverDepth(map, camera);
            //}
            _LastMouseoverBlockGlobal = global;
        }
        static Vector3 _LastMouseoverBlockGlobal;
        static public bool BlockTargeting;
        //static public TargetArgs GetMouseover()
        //{
        //    if (Instance.MouseoverEntity.Object != null)
        //        if (Instance.MouseoverEntity.Depth >= Instance.Mouseover.Depth)
        //            return Instance.MouseoverEntity.Target;

        //    return Instance.Mouseover.Target;

        //    //return Instance.MouseoverEntity.Depth <= Instance.Mouseover.Depth ? Instance.MouseoverEntity.Target : Instance.Mouseover.Target;
        //}
        //static public TargetArgs GetMouseoverEntityPriority()
        //{
        //    if (Instance.MouseoverEntity.Object != null)
        //            return Instance.MouseoverEntity.Target;

        //    return Instance.Mouseover.Target;

        //    //return Instance.MouseoverEntity.Depth <= Instance.Mouseover.Depth ? Instance.MouseoverEntity.Target : Instance.Mouseover.Target;
        //}
        //static public TargetArgs GetMouseoverBlock()
        //{
        //    return Instance.Mouseover.Target;
        //}
        //static public TargetArgs GetMouseoverEntity()
        //{
        //    return Instance.MouseoverEntity.Target;
        //}
        static bool AltDown;
        static public bool IsBlockTargeting()
        {
            bool keydown = AltDown;// InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            return keydown ^ BlockTargeting;
        }

        public static TargetArgs TargetCell { get { return Instance.MouseoverBlock.TargetCell; } }
        public static TargetArgs TargetEntity { get { return Instance.MouseoverBlock.TargetEntity; } }
    }
}
