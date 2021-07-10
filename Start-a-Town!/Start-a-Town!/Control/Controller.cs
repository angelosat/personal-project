using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UI;

namespace Start_a_Town_
{
    public enum MouseButtons { Left, Right, Middle }
    public enum MessageID{KeyDown, KeyUp}

    public class Controller
    {
        static public bool GetMouseover<T>(out T obj) where T : GameObject
        {
            return Instance.MouseoverBlock.TryGet<T>(out obj);
        }

        static public event EventHandler<MouseoverEventArgs> MouseoverObjectChanged;
        public void OnMouseoverObjectChanged(MouseoverEventArgs e)
        {
            if (MouseoverObjectChanged != null)
                MouseoverObjectChanged(this, e);
        }

        public Mouseover MouseoverEntity, MouseoverEntityNext;

        public Entity GetMouseoverEntity()
        {
            return this.MouseoverEntity.Object as Entity;
        }
        public Mouseover MouseoverBlock;
        public Mouseover MouseoverBlockNext;
        
        public bool BuildMode;

        public Rectangle MouseRect;
        
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

        public static event InputEvent MouseLeftPress, MouseLeftRelease, MouseLeftDown, MouseWheelUp, MouseWheelDown, EscapeDown,
            MouseRightPress, MouseRightDown, MouseRightRelease;

        public static event EventHandler<EventArgs> MouseOverEntityChanged, MouseOverTileChanged, KeyPressed;
        public static event EventHandler<KeyEventArgs2> KeyDown, KeyUp;
        public static event EventHandler<KeyPressEventArgs2> KeyPress2;
        public static event EventHandler<System.Windows.Forms.KeyPressEventArgs> KeyPress;

        protected void OnKeyPressed()
        {
            KeyPressed?.Invoke(this, EventArgs.Empty);
        }
        protected void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            KeyPress?.Invoke(this, e);
        }
        protected void OnKeyPress(KeyPressEventArgs2 e)
        {
            KeyPress2?.Invoke(this, e);
        }
        protected void OnKeyUp(Keys[] keysnew, Keys[] keysold)
        {
            KeyUp?.Invoke(this, new KeyEventArgs2(keysnew, keysold));
        }
        protected void OnKeyDown(Keys[] keysnew, Keys[] keysold)
        {
            KeyDown?.Invoke(this, new KeyEventArgs2(keysnew, keysold));
        }
        protected void OnMouseOverTileChanged()
        {
            MouseOverTileChanged?.Invoke(this, EventArgs.Empty);
        }
        protected void OnMouseOverEntityChanged()
        {
            MouseOverEntityChanged?.Invoke(this, EventArgs.Empty);
        }

        static public InputState Input = new InputState();
        public Controller()
        {
            MouseoverBlock = new Mouseover();
            MouseoverBlockNext = new Mouseover();
            MouseoverEntity = new();
            MouseoverEntityNext = new();
        }

        public void OnMouseLeftPress()
        {
            MouseLeftPress?.Invoke();
        }

        public void OnMouseLeftDown()
        {
            MouseLeftDown?.Invoke();
        }
        public void OnMouseLeftRelease()
        {
            MouseLeftRelease?.Invoke();
        }
        public void OnMouseRightPress()
        {
            MouseRightPress?.Invoke();
        }
        public void OnMouseRightDown()
        {
            MouseRightDown?.Invoke();
        }
        public void OnMouseRightRelease()
        {
            MouseRightRelease?.Invoke();
        }

        void GetInputStates()
        {
            ksCurrent = Keyboard.GetState();
            msCurrent = Mouse.GetState();
        }
        
        public Vector2 MouseLocation
        { get { return new Vector2(msCurrent.X, msCurrent.Y); } }
        public Mouseover GetMouseover()
        {
            return MouseoverEntity.Target != null ? MouseoverEntity : MouseoverBlock;
        }
        public void Update()
        {
            AltDown = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            UpdateMousovers();

            GetInputStates();
            this.MouseRect = new Rectangle(msCurrent.X, msCurrent.Y, 1, 1);


            if (ksCurrent.GetPressedKeys().Count() > 0)
                OnKeyPressed();


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
            ksPrevious = ksCurrent;
            msPrevious = msCurrent;
        }
        private void UpdateMousovers()
        {
            object mouseover, mouseoverNext;
            MouseoverBlock.TryGet(out mouseover);
            MouseoverBlockNext.TryGet(out mouseoverNext);

            if (mouseover != mouseoverNext)
            {
                OnMouseoverObjectChanged(new MouseoverEventArgs(mouseoverNext, mouseover));
                if (mouseover is GameObject objLast)
                    objLast.FocusLost();
                if (mouseoverNext is GameObject objNext)
                    objNext.Focus();
            }
            
            MouseoverBlock = MouseoverBlockNext;
            MouseoverBlockNext = new Mouseover();
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

        public Keys[] GetKeys()
        {
            return ksCurrent.GetPressedKeys();
        }

        static public void TrySetMouseoverEntity(Camera camera, GameObject entity, Vector3 face, float drawdepth)
        {
            var global = entity.Global;
            if (!camera.IsDrawable(global))
                return;
            float mouseoverDepth = drawdepth;
            if (mouseoverDepth >= Instance.MouseoverBlockNext.Depth)
            {
                if (Instance.MouseoverBlock.Object is TargetArgs target)
                {
                    if (target.Object != entity)
                        target = new TargetArgs(entity, face) { Map = Engine.Map };
                }
                else
                    target = new TargetArgs(entity, face) { Map = Engine.Map };

                Instance.MouseoverBlockNext.Target = target;
                Instance.MouseoverBlockNext.Object = target;
                Instance.MouseoverBlockNext.Face = face;
                Instance.MouseoverBlockNext.Depth = drawdepth;
            }
        }
        static public void SetMouseoverBlock(Camera camera, MapBase map, Vector3 global, Vector3 face, Vector3 precise)
        {
            var target = new TargetArgs(map, global, face, precise);

            if (global != _LastMouseoverBlockGlobal || Instance.MouseoverBlock.Object is Element) // very hacky
                Instance.MouseoverBlockNext.Object = target;
            else
                Instance.MouseoverBlockNext.Object = Instance.MouseoverBlock.Object;
            Instance.MouseoverBlockNext.Face = face;
            Instance.MouseoverBlockNext.Precise = precise;
            Instance.MouseoverBlockNext.Target = target;
            Instance.MouseoverBlockNext.Depth = global.GetDrawDepth(map, camera);
            _LastMouseoverBlockGlobal = global;
        }
        static Vector3 _LastMouseoverBlockGlobal;
        static public bool BlockTargeting;
        
        static bool AltDown;
        static public bool IsBlockTargeting()
        {
            bool keydown = AltDown;
            return keydown ^ BlockTargeting;
        }

        public static TargetArgs TargetCell { get { return Instance.MouseoverBlock.TargetCell; } }
        public static TargetArgs TargetEntity { get { return Instance.MouseoverBlock.TargetEntity; } }
    }
}
