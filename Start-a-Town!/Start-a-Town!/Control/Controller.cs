using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using UI;

namespace Start_a_Town_
{
    public enum MouseButtons { Left, Right, Middle }
    public enum MessageID { KeyDown, KeyUp }

    public class Controller
    {
        public static bool GetMouseover<T>(out T obj) where T : GameObject
        {
            return Instance.MouseoverBlock.TryGet(out obj);
        }

        public static event EventHandler<MouseoverEventArgs> MouseoverObjectChanged;
        public void OnMouseoverObjectChanged(MouseoverEventArgs e)
        {
            MouseoverObjectChanged?.Invoke(this, e);
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
                {
                    _Instance = new Controller();
                }

                return _Instance;
            }
        }

        public static event InputEvent MouseLeftPress, MouseLeftRelease, MouseLeftDown, MouseWheelUp, MouseWheelDown,
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

        public static InputState Input = new InputState();
        public Controller()
        {
            this.MouseoverBlock = new Mouseover();
            this.MouseoverBlockNext = new Mouseover();
            this.MouseoverEntity = new();
            this.MouseoverEntityNext = new();
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
            this.ksCurrent = Keyboard.GetState();
            this.msCurrent = Mouse.GetState();
        }

        public Vector2 MouseLocation
        { get { return new Vector2(this.msCurrent.X, this.msCurrent.Y); } }
        public Mouseover GetMouseover()
        {
            return this.MouseoverEntity.Target != null ? this.MouseoverEntity : this.MouseoverBlock;
        }
        public void Update()
        {
            AltDown = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            this.UpdateMousovers();

            this.GetInputStates();
            this.MouseRect = new Rectangle(this.msCurrent.X, this.msCurrent.Y, 1, 1);


            if (this.ksCurrent.GetPressedKeys().Count() > 0)
            {
                this.OnKeyPressed();
            }

            if (this.msCurrent.LeftButton == ButtonState.Pressed)
            {
                if (this.msPrevious.LeftButton == ButtonState.Released)
                {
                    this.OnMouseLeftPress();
                }
                else
                {
                    this.OnMouseLeftDown();
                }
            }
            else if (this.msCurrent.LeftButton == ButtonState.Released && this.msPrevious.LeftButton == ButtonState.Pressed)
            {
                this.OnMouseLeftRelease();
            }

            if (this.msCurrent.RightButton == ButtonState.Pressed)
            {
                if (this.msPrevious.RightButton == ButtonState.Released)
                {
                    this.OnMouseRightPress();
                }
                else
                {
                    this.OnMouseRightDown();
                }
            }
            else if (this.msCurrent.RightButton == ButtonState.Released && this.msPrevious.RightButton == ButtonState.Pressed)
            {
                this.OnMouseRightRelease();
            }

            if (this.msCurrent.ScrollWheelValue > this.msPrevious.ScrollWheelValue)
            {
                this.OnMouseWheelUp();
            }
            else if (this.msCurrent.ScrollWheelValue < this.msPrevious.ScrollWheelValue)
            {
                this.OnMouseWheelDown();
            }

            if (this.ksCurrent.IsKeyDown(Keys.LeftAlt) || this.ksCurrent.IsKeyDown(Keys.RightAlt))
            {
                if (this.KeyPressCheck(Keys.Enter))
                {
                    Game1.Instance.graphics.ToggleFullScreen();
                }
            }

            Keys[] keysnew = this.ksCurrent.GetPressedKeys();
            Keys[] keysold = this.ksPrevious.GetPressedKeys();
            if (keysnew.Length > 0)
            {
                this.OnKeyDown(keysnew, keysold);
            }

            if (keysnew.Length < keysold.Length)
            {
                this.OnKeyUp(keysnew, keysold);
            }

            this.ksPrevious = this.ksCurrent;
            this.msPrevious = this.msCurrent;
        }
        private void UpdateMousovers()
        {
            this.MouseoverBlock.TryGet(out object mouseover);
            this.MouseoverBlockNext.TryGet(out object mouseoverNext);

            if (mouseover != mouseoverNext)
            {
                this.OnMouseoverObjectChanged(new MouseoverEventArgs(mouseoverNext, mouseover));
                if (mouseover is GameObject objLast)
                {
                    objLast.FocusLost();
                }

                if (mouseoverNext is GameObject objNext)
                {
                    objNext.Focus();
                }
            }

            this.MouseoverBlock = this.MouseoverBlockNext;
            this.MouseoverBlockNext = new Mouseover();
        }

        void OnMouseWheelUp()
        {
            if (MouseWheelUp != null)
            {
                MouseWheelUp();
            }
        }
        void OnMouseWheelDown()
        {
            if (MouseWheelDown != null)
            {
                MouseWheelDown();
            }
        }
        public bool KeyPressCheck(Keys key)
        {
            return this.ksCurrent.IsKeyDown(key) && this.ksPrevious.IsKeyUp(key);
        }

        public Keys[] GetKeys()
        {
            return this.ksCurrent.GetPressedKeys();
        }

        public static void TrySetMouseoverEntity(Camera camera, GameObject entity, Vector3 face, float drawdepth)
        {
            var global = entity.Global;
            if (!camera.IsDrawable(entity.Map, global))
            {
                return;
            }

            float mouseoverDepth = drawdepth;
            if (mouseoverDepth >= Instance.MouseoverBlockNext.Depth)
            {
                if (Instance.MouseoverBlock.Object is TargetArgs target)
                {
                    if (target.Object != entity)
                    {
                        target = new TargetArgs(entity, face) { Map = Engine.Map };
                    }
                }
                else
                {
                    target = new TargetArgs(entity, face) { Map = Engine.Map };
                }

                Instance.MouseoverBlockNext.Target = target;
                Instance.MouseoverBlockNext.Object = target;
                Instance.MouseoverBlockNext.Face = face;
                Instance.MouseoverBlockNext.Depth = drawdepth;
            }
        }
        public static void SetMouseoverBlock(Camera camera, MapBase map, Vector3 global, Vector3 face, Vector3 precise)
        {
            var target = new TargetArgs(map, global, face, precise);

            if (global != _LastMouseoverBlockGlobal || Instance.MouseoverBlock.Object is Element) // very hacky
            {
                Instance.MouseoverBlockNext.Object = target;
            }
            else
            {
                Instance.MouseoverBlockNext.Object = Instance.MouseoverBlock.Object;
            }

            Instance.MouseoverBlockNext.Face = face;
            Instance.MouseoverBlockNext.Precise = precise;
            Instance.MouseoverBlockNext.Target = target;
            Instance.MouseoverBlockNext.Depth = global.GetDrawDepth(map, camera);
            _LastMouseoverBlockGlobal = global;
        }
        static Vector3 _LastMouseoverBlockGlobal;
        public static bool BlockTargeting;

        static bool AltDown;
        public static bool IsBlockTargeting()
        {
            bool keydown = AltDown;
            return keydown ^ BlockTargeting;
        }

        public static TargetArgs TargetCell { get { return Instance.MouseoverBlock.TargetCell; } }
        public static TargetArgs TargetEntity { get { return Instance.MouseoverBlock.TargetEntity; } }
    }
}
