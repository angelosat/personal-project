
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;
using Start_a_Town_.Graphics;
using UI;

namespace Start_a_Town_.UI
{
    public class UIScaleEventArgs : EventArgs
    {
        public float NewScale, OldScale;
        public UIScaleEventArgs(float oldScale, float newScale)
        {
            this.NewScale = newScale;
            this.OldScale = oldScale;
        }
    }
    public enum HorizontalAlignment { Left, Center, Right }
    public enum VerticalAlignment { Top, Bottom, Center }
    public struct OuterPadding
    {
        public static Rectangle
            TopLeft = new Rectangle(0, 0, 11, 11),
            TopRight = new Rectangle(12, 0, 11, 11),
            BottomLeft = new Rectangle(0, 12, 11, 11),
            BottomRight = new Rectangle(12, 12, 11, 11),
            Top = new Rectangle(12, 0, 1, 11),
            Left = new Rectangle(0, 12, 11, 1),
            Right = new Rectangle(12, 11, 11, 1),
            Bottom = new Rectangle(11, 12, 1, 11),
            Center = new Rectangle(11, 11, 1, 1);
    }
    public struct InnerPadding
    {
        public static Rectangle
            TopLeft = new Rectangle(0, 0, 19, 19),
            TopRight = new Rectangle(19, 0, 19, 19),
            BottomLeft = new Rectangle(0, 19, 19, 19),
            BottomRight = new Rectangle(19, 19, 19, 19),
            Top = new Rectangle(19, 0, 1, 19),
            Left = new Rectangle(0, 19, 19, 1),
            Right = new Rectangle(19, 19, 19, 1),
            Bottom = new Rectangle(19, 19, 1, 19),
            Center = new Rectangle(18, 18, 1, 1);
    }
    public enum LayerTypes { Nameplates, Speechbubbles, Hud, Windows, Dialog }
    public class UIManager : IDisposable, IKeyEventHandler
    {
        public enum Events { NpcUICreated,
            SelectedChanged
        }
            //"Nameplates"] =
            //"Speechbubbles"
            //"Main"] = new L
            //"Dialog"] = new

        static float _Scale = 1;
        static public float Scale// = 2;
        {
            get { return _Scale; }
            set
            {
                UIScaleEventArgs e = new UIScaleEventArgs(_Scale, value);
                foreach (UIManager manager in WindowManagers)
                    foreach (var layer in manager.Layers)
                        foreach (var ctrl in layer.Value)
                        {
                            ctrl.Reposition(e);
                        }
                _Scale = value;
                UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Width, Height);
                
            }
        }



        static public Rectangle Bounds
        { get { return new Rectangle(0, 0, Width, Height); } }
        static public Vector2 Center { get { return Game1.ScreenSize / (2 * Scale); } }
        //static public int Width { get { return (int)(Game1.ScreenSize.X / Scale); } }
        //static public int Height { get { return (int)(Game1.ScreenSize.Y / Scale); } }
        static public int Width { get { 
            //return Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth; 
            //return Game1.Instance.graphics.PreferredBackBufferWidth; 

            return (int)( Game1.Instance.graphics.PreferredBackBufferWidth / Scale); 

        } }// (int)(Game1.ScreenSize.X / Scale); } }
        static public int Height { get { 
            //return Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;
            //return Game1.Instance.graphics.PreferredBackBufferHeight;
            return (int)(Game1.Instance.graphics.PreferredBackBufferHeight / Scale); 

            //return Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight; 

        } }// (int)(Game1.ScreenSize.Y / Scale); } }

        static public Vector2 Size { get { return new Vector2(Width, Height); } }
        static public Vector2 Mouse { get { return Controller.Instance.MouseLocation / Scale; } }
        static public Rectangle MouseRect { get { return new Rectangle((int)Mouse.X, (int)Mouse.Y, 1, 1); } }
        public static SortedList<float, Control> MouseOverList;
        public static SpriteFont Font = Game1.Instance.Content.Load<SpriteFont>("DefaultFont");
        public static SpriteFont FontBold = Game1.Instance.Content.Load<SpriteFont>("BoldFont");
        public static SpriteFont Symbols = Game1.Instance.Content.Load<SpriteFont>("Symbols");
        public static Color Tint = Color.SteelBlue; //Color.Teal; //new Color(40, 0, 40, 255);
        public static Texture2D frameSprite, SlotSprite, defaultButtonSprite, DefaultIconButtonSprite, ItemSheet, DefaultProgressBar, DefaultProgressBarStrip, ProgressBarBorder, DefaultDropdownSprite, DefaultLoadingSprite,
            DefaultVScrollbarSprite, DefaultHScrollbarSprite,
            DefaultTrackBarSprite, DefaultTrackBarThumbSprite, SampleButton, Icons16x16, LargeButton, Icons32, Icon16Background, TextureTickBox;
        public static int DefaultButtonHeight = 23, LineHeight;
        public static int BorderPx = 5, TitlePx = 26;
        public static Color DefaultTextColor = Color.LightGray;

        public DialogBlock DialogBlock;

        static public List<UIManager> WindowManagers = new List<UIManager>();

        private FpsCounterOld fps;
        public static Window MouseOverWindow;
        public Window ActiveWindow
        {
            get
            {
                if (ActiveControl != null)
                    return ActiveControl.GetWindow();
                return null;
            }
        }

       // public List<Control> Controls = new List<Control>();
        public Dictionary<LayerTypes, List<Control>> Layers = new Dictionary<LayerTypes, List<Control>>();
        public List<Control> this[LayerTypes layer] { get { return this.Layers[layer]; } }
        public int LastScreenWidth, LastScreenHeight;
        public Window Dialog;

        

        static Texture2D _Highlight;
        static public Texture2D Highlight
        {
            get
            {
                if (_Highlight == null)
                {
                    _Highlight = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                    //_Highlight.SetData(new Color[] { Color.Lerp(Color.White, Color.Transparent, 0.5f) });
                    _Highlight.SetData(new Color[] { Color.White });
                }
                return _Highlight;
            }
        }
        static Texture2D _TransparentHighlight;
        static public Texture2D TransparentHighlight
        {
            get
            {
                if (_TransparentHighlight == null)
                {
                    _TransparentHighlight = new Texture2D(Game1.Instance.GraphicsDevice, 3, 3);
                    _TransparentHighlight.SetData(new Color[] { Color.White, Color.White, Color.White, Color.White, Color.Transparent, Color.White, Color.White, Color.White, Color.White });
                }
                return _TransparentHighlight;
            }
        }

        static Texture2D _Shade;
        static public Texture2D Shade
        {
            get
            {
                if (_Shade == null)
                {
                    _Shade = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                    _Shade.SetData(new Color[] { Color.Black });
                }
                return _Shade;
            }
        }

        static public Texture2D Cursor;
        public RenderTarget2D DimScreen;

        public event UIEvent MouseDown, MouseUp, MouseWheelDown, MouseWheelUp, TopMostControlChanged;
        public event InputEvent MouseMove, MouseLeftPress, MouseLeftUp, MouseLeftDown;

        public event EventHandler<KeyPressEventArgs2> KeyPress;

        protected virtual void OnKeyPress(KeyPressEventArgs2 e)
        {
            if (KeyPress != null)
                KeyPress(this, e);
        }
        protected virtual void OnMouseWheelUp()
        {
            if (MouseWheelUp != null)
                MouseWheelUp(this, new EventArgs());
        }
        protected virtual void OnMouseWheelDown()
        {
            if (MouseWheelDown != null)
                MouseWheelDown(this, new EventArgs());
        }
        protected virtual void OnMouseMove()
        {
            if (MouseMove != null)
                MouseMove();
        }
        protected virtual void OnMouseLeftPress()
        {
            if (MouseLeftPress != null)
                MouseLeftPress();
        }
        protected virtual void OnMouseLeftUp()
        {
            if (MouseLeftUp != null)
                MouseLeftUp();
        }
        protected virtual void OnMouseLeftDown()
        {
            if (MouseLeftDown != null)
                MouseLeftDown();
        }
        protected virtual void OnMouseDown()
        {
            if (MouseDown != null)
                MouseDown(this, new EventArgs());
        }
        protected virtual void OnMouseUp()
        {
            if (MouseUp != null)
                MouseUp(this, new EventArgs());
        }



        public void OnTopMostControlChanged()
        {

            if (TopMostControlChanged != null)
                TopMostControlChanged(this, new EventArgs());
        }



       // public GameScreen Parent;
        //protected bool _Active;
        //public bool Active
        //{
        //    get { return _Active; }
        //    set
        //    {
        //        //Console.WriteLine(Parent.ToString());
        //        if (value != _Active)
        //        {
        //            if (value)
        //            {
        //                //Parent.MouseLeftPress += new InputEvent(Screen_MouseLeftPress);
        //                //Parent.MouseLeftUp += new InputEvent(Screen_MouseLeftUp);
        //                //Parent.MouseMove += new InputEvent(Screen_MouseMove);
        //                ////Parent.KeyPress += new EventHandler<KeyEventArgs>(Screen_KeyPress);
        //                //Parent.KeyPress += new EventHandler<KeyPressEventArgs>(Screen_KeyPress);
        //            }
        //            else
        //            {
        //                //Parent.MouseLeftPress -= Screen_MouseLeftPress;
        //                //Parent.MouseLeftUp -= Screen_MouseLeftUp;
        //                //Parent.MouseMove -= Screen_MouseMove;
        //                //Parent.KeyPress -= Screen_KeyPress;
        //            }
        //        }
        //        _Active = value;
        //    }
        //}
        void Screen_KeyPress(object sender, KeyPressEventArgs2 e)
        {
            OnKeyPress(e);
        }

        static public Atlas Atlas = new Atlas("UI");
        static public readonly Atlas.Node.Token Btn16 = Atlas.Load("Graphics/Gui/gui-icon");
        static public readonly Atlas.Node.Token ArrowUp = Atlas.Load("Graphics/Gui/gui-up1");
        static public readonly Atlas.Node.Token ArrowDown = Atlas.Load("Graphics/Gui/gui-down1");
        static public readonly Atlas.Node.Token ArrowLeft = Atlas.Load("Graphics/Gui/gui-left1");
        static public readonly Atlas.Node.Token ArrowRight = Atlas.Load("Graphics/Gui/gui-right1");
        static public readonly Atlas.Node.Token IconX = Atlas.Load("Graphics/Gui/gui-x");
        static public readonly Atlas.Node.Token IconSave = Atlas.Load("Graphics/Gui/gui-save");
        static public readonly Atlas.Node.Token IconLoad = Atlas.Load("Graphics/Gui/gui-load");
        static public readonly Atlas.Node.Token IconDisk = Atlas.Load("Graphics/Gui/gui-saveload");
        static public readonly Atlas.Node.Token Tickbox = Atlas.Load("Graphics/Gui/gui-tickbox2a");
        static public readonly Atlas.Node.Token MousePointer = Atlas.Load("Graphics/cursor-default");
        static public readonly Atlas.Node.Token MousePointerGrayscale = Atlas.Load("Graphics/cursor-default", true);
        static public readonly Atlas.Node.Token SpriteConstruction = Atlas.Load("Graphics/Icons/spr-constr");

        static public readonly Texture2D UpArrow = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-up1");
        static public readonly Texture2D DownArrow = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-down1");

        
        static public void LoadContent()
        {
            Cursor = Game1.Instance.Content.Load<Texture2D>("Graphics/cursor-default");
            //Font = Game1.Instance.Content.Load<SpriteFont>("DefaultFont");
            //FontBold = Game1.Instance.Content.Load<SpriteFont>("BoldFont");
            LineHeight = (int)Font.MeasureString("(gA1,|)").Y;
            SlotSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-frame1");
            frameSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-window2");
            defaultButtonSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/guiButton");
            DefaultDropdownSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/ui_dropdown_strip");
            ItemSheet = Game1.Instance.Content.Load<Texture2D>("Graphics/ItemSheet");
            DefaultProgressBar = Game1.Instance.Content.Load<Texture2D>("Graphics/progressbarBW");
            DefaultProgressBarStrip = Game1.Instance.Content.Load<Texture2D>("Graphics/barBwStrip");
            ProgressBarBorder = Game1.Instance.Content.Load<Texture2D>("Graphics/progressbarBorder");
            DefaultVScrollbarSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/vscrollbar");
            DefaultHScrollbarSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/hscrollbar");
            DefaultTrackBarSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-tickbox2a");
            Icon16Background = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-icon");
            DefaultTrackBarThumbSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-down1");
            DefaultIconButtonSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/iconbutton");
            SampleButton = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/samplebutton");
            Icons16x16 = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/Icons16x16");
            Icons32 = Game1.Instance.Content.Load<Texture2D>("Graphics/IconSheet");
            LargeButton = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/LargeButton");
            TextureTickBox = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/CheckBox");
            DefaultLoadingSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/spinning3");

            Atlas.Initialize();
            Atlas.Bake();

            //float scale;
            //if (float.TryParse(Engine.Settings["Settings"]["Interface"]["UIScale"].InnerText, out scale))
            //    Scale = scale;
            //else
            //    Scale = 1;

            var scaleNode = Engine.Config.Descendants("UIScale").FirstOrDefault();
            float scale;
            if(float.TryParse(scaleNode.Value, out scale)) // do i need to check for scalenode == null?
                Scale = scale;
            else
                Scale = 1;
           // UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Width, Height);
        }

        internal void OnSelectedTargetChanged(TargetArgs target)
        {
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.OnSelectedTargetChanged(target);
            }
        }

        static RenderTarget2D UITexture;// = new RenderTarget2D(gd, (int)(Game1.ScreenSize.X / Scale), (int)(Game1.ScreenSize.Y / Scale)))
        public UIManager()//GameScreen parent)
        //    : this()
        {
         //   this.Parent = parent;
        //}
        //public UIManager()
        //{
            Layers[LayerTypes.Nameplates] = new List<Control>();
            Layers[LayerTypes.Speechbubbles] = new List<Control>();
            Layers[LayerTypes.Hud] = new List<Control>();
            Layers[LayerTypes.Windows] = new List<Control>();
            Layers[LayerTypes.Dialog] = new List<Control>();

            WindowManagers.Add(this);
            fps = new FpsCounterOld();



           // Game1.Instance.graphics.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
           //Game1.Instance.Window.ClientSizeChanged += new EventHandler<EventArgs>(graphics_DeviceReset);
           Game1.Instance.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);


          //  ScreenWidth = Game1.Instance.graphics.PreferredBackBufferWidth;
           // ScreenHeight = Game1.Instance.graphics.PreferredBackBufferHeight;
            //ScreenWidth = Game1.Instance.Window.ClientBounds.Width;// (int)Game1.ScreenSize.X;
            //ScreenHeight = Game1.Instance.Window.ClientBounds.Height;  //(int)Game1.ScreenSize.Y;
            LastScreenWidth = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth;// Game1.Instance.Window.ClientBounds.Width;// (int)Game1.ScreenSize.X;
            LastScreenHeight = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;// Game1.Instance.Window.ClientBounds.Height;  //(int)Game1.ScreenSize.Y;

            DimScreen = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1);
            Game1.Instance.GraphicsDevice.SetRenderTarget(DimScreen);
            Game1.Instance.GraphicsDevice.Clear(Color.Black);
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);

            //InGameElements.Add(new NotificationArea());

           // DialogBlock = new DialogBlock();
        }

        public List<Control> ControlsInMemory = new List<Control>();
        void graphics_DeviceReset(object sender, EventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            int newWidth = Game1.Instance.graphics.PreferredBackBufferWidth;
            int newHeight = Game1.Instance.graphics.PreferredBackBufferHeight;
            //int newWidth = UIManager.Width;
            //int newHeight = UIManager.Height;
            //int newWidth = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
            //int newHeight = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;

            float wRatio = newWidth / (float)LastScreenWidth;
            float hRatio = newHeight / (float)LastScreenHeight;
            //(LastScreenWidth.ToString() + " -> " + newWidth.ToString() + " , " + wRatio.ToString()).ToConsole();
            //(LastScreenHeight.ToString() + " -> " + newHeight.ToString() + " , " + hRatio.ToString()).ToConsole();

            //wRatio.ToConsole();
            //hRatio.ToConsole();
            LastScreenWidth = newWidth;
            LastScreenHeight = newHeight;

            UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, newWidth, newHeight);// Width, Height);
            foreach (Control ctrl in ControlsInMemory)
            {
                ctrl.Reposition(new Vector2(wRatio, hRatio));

                ctrl.Invalidate(true);// Paint();
                //ctrl.Paint(); // dont paint here, i paint in their update if they're invalid anyway
            }

            //ScreenWidth = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
            //ScreenHeight = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;
            //ScreenWidth = newWidth;// Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
            //ScreenHeight = newHeight;// Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;
            DimScreen = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1);//ScreenWidth, ScreenHeight);
            Game1.Instance.GraphicsDevice.SetRenderTarget(DimScreen);
            Game1.Instance.GraphicsDevice.Clear(Color.Black);
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);

            //UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, newWidth, newHeight);// Width, Height);
        }


        public Control FocusedControl;

        public Control ActiveControl { 
            //get; 
            //set; 
            get { return Controller.Instance.MouseoverBlock.Object as Control; }
            set { Controller.Instance.MouseoverBlock.Object = value; }
        }


        public void Screen_MouseMove()
        {


            OnMouseMove();


            MouseOverWindow = null;

        }
        void Screen_MouseLeftPress()
        {
            OnMouseLeftPress();
        }
        void Screen_MouseLeftUp()
        {
            //Console.WriteLine(Parent.ToString());

            OnMouseLeftUp();
        }
        void Controller_MouseWheelDown()
        {
            OnMouseWheelDown();
        }
        void Controller_MouseWheelUp()
        {
            OnMouseWheelUp();
        }

        public void Update(Game1 game, GameTime gt)
        {
            //if (Controls.Count == 0)
            //    return;
            //if (Controls.Last() == DialogBlock)
            //    Controls.Remove(DialogBlock);

            //foreach (Control window in Controls.ToList())
            //    window.Update();

            Control lastactive = Controller.Instance.MouseoverBlock.Object as Control;// ActiveControl;
            foreach (var layer in Layers)
                foreach (var ctrl in layer.Value.ToList())
                {
                    ctrl.Update();
                    ctrl.Update(new Rectangle(0, 0, (int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y));
                }

            Control nextactive = Controller.Instance.MouseoverBlockNext.Object as Control;

            if (nextactive != lastactive)
            {
                if (nextactive != null)
                    nextactive.OnMouseEnter();
                if (lastactive != null)
                    lastactive.OnMouseLeave();

            }

            if (fps != null)
                fps.Update(game, gt);
        }

        //public void Remove(DrawableInterfaceElement element)
        //{

        //    InGameElements.Remove(element);
        //}

        public bool CloseAll()
        {

            bool closedsomething = false;
            List<Control> toclose = new List<Control>();// Controls.FindAll(foo => foo is Window);
            foreach (var layer in Layers)
                toclose.AddRange(layer.Value.FindAll(foo => foo is Window));
            if (toclose.Count > 0)
            {
                foreach (Control c in toclose)
                    if (c is Window)
                        if ((c as Window).Closable)
                        {
                            (c as Window).Close();
                            closedsomething = true;
                        }
            }

            return closedsomething;
        }

        public void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            foreach (var layer in Layers)
                foreach (var ctrl in layer.Value.ToList())
                    ctrl.DrawOnCamera(sb, camera);
        }

        public void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            foreach (var layer in Layers)
                foreach (var ctrl in layer.Value.ToList())
                    ctrl.DrawWorld(sb, camera);
        }

        public void Draw(SpriteBatch sb, Camera camera)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            RenderTarget2D uiTexture = UITexture;
            gd.SetRenderTarget(uiTexture);
            gd.Clear(Color.Transparent);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            //sb.Begin();
            if (camera != null)
                this.DrawOnCamera(sb, camera);
            foreach (var layer in Layers)
                foreach (var ctrl in layer.Value.ToList())
                    ctrl.Draw(sb, Bounds);
            TooltipManager.Instance.Draw(sb);
            DragDropManager.Instance.Draw(sb);
            sb.End();
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Scale % 1 > 0 ? SamplerState.AnisotropicClamp : SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            sb.Draw(uiTexture, new Rectangle(0, 0, (int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y), Color.White);
            sb.End();
        }
        static public RenderTarget2D CreateTextureString(string text, Color bgCol)
        {
            var stringSize = UIManager.Font.MeasureString(text);
            var gd = Game1.Instance.GraphicsDevice;
            var ren = new RenderTarget2D(gd, (int)stringSize.X, (int)stringSize.Y);

            gd.SetRenderTarget(ren);
            gd.Clear(bgCol);
            var sb = new SpriteBatch(gd);
            sb.Begin();
            DrawStringOutlined(sb, text, Vector2.One);
            sb.End();
            gd.SetRenderTarget(null);
            return ren;
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color fill, Color outline, SpriteFont font)
        {
            if (text == null)
                return;
            if (text == "")
                return;
            position = position.Round();
            //Vector2 tempVect;
            Vector2 origin = font.MeasureString(text) * texOrigin;
            origin = new Vector2((int)origin.X, (int)origin.Y);
            //for (int d = 0; d < 8; d++)
            //{
            //    double dir = d * (Math.PI / 4);
            //    tempVect = new Vector2((float)Math.Round(Math.Cos(dir)), -(float)Math.Round(Math.Sin(dir)));
            //    sb.DrawString(font, text, position + new Vector2(tempVect.X, tempVect.Y), outline, 0, origin, 1, SpriteEffects.None, 0);
            //}
            foreach (var n in Vector2.Zero.GetNeighbors8())
                sb.DrawString(font, text, position + n, outline, 0, origin, 1, SpriteEffects.None, 0);

            sb.DrawString(font, text, position, fill, 0, origin, 1, SpriteEffects.None, 0);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 bounds, Color fill, Color outline, SpriteFont font, HorizontalAlignment halign)
        {
            if (text == null)
                return;
            if (text == "")
                return;
            var lines = text.Split('\n');
            position = position.Round();
            foreach (var line in lines)
            {
                var lineRect = font.MeasureString(line);
                Vector2 lineorigin = Vector2.Zero;// = new Vector2(originX, 1) * lineRect;
                //if (halign == HorizontalAlignment.Left)
                //{
                //    lineorigin += Vector2.Zero;
                //}
                //position.X = 0;
                if (halign == HorizontalAlignment.Center)
                {
                    //position.X += bounds.X / 2;
                    position.X = bounds.X / 2;
                    lineorigin = new Vector2(lineRect.X * .5f, 0);
                }
                else if (halign == HorizontalAlignment.Right)
                {
                    lineorigin = new Vector2(lineRect.X, 0);
                    //position.X += bounds.X;
                    position.X = bounds.X - 1;
                }
                lineorigin = lineorigin.Floor();
                position = position.Floor();
                foreach (var n in Vector2.Zero.GetNeighbors8())
                    sb.DrawString(font, line, position + n, outline, 0, lineorigin, 1, SpriteEffects.None, 0);
                sb.DrawString(font, line, position, fill, 0, lineorigin, 1, SpriteEffects.None, 0);
                position.Y += lineRect.Y + 1;
            }
        }
        static public void DrawStringOutlinedPrevious(SpriteBatch sb, string text, Vector2 position, Color fill, Color outline, SpriteFont font, HorizontalAlignment halign)
        {
            if (text == null)
                return;
            if (text == "")
                return;
            var lines = text.Split('\n');
            position = position.Round();
            foreach (var line in lines)
            {
                var lineRect = font.MeasureString(line);
                Vector2 lineorigin = Vector2.Zero;// = new Vector2(originX, 1) * lineRect;
                //if (halign == HorizontalAlignment.Left)
                //{
                //    lineorigin += Vector2.Zero;
                //}
                if (halign == HorizontalAlignment.Center)
                {
                    lineorigin = new Vector2(lineRect.X * .5f, 0);
                }
                else if(halign == HorizontalAlignment.Right)
                    lineorigin = new Vector2(lineRect.X, 0);
                lineorigin = lineorigin.Floor();
                foreach (var n in Vector2.Zero.GetNeighbors8())
                    sb.DrawString(font, line, position + n, outline, 0, lineorigin, 1, SpriteEffects.None, 0);
                sb.DrawString(font, line, position, fill, 0, lineorigin, 1, SpriteEffects.None, 0);
                position.Y += lineRect.Y + 1;
            }
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color fill, Color outline)
        {
            DrawStringOutlined(sb, text, position, texOrigin, fill, outline, UIManager.Font);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color c)
        {
            DrawStringOutlined(sb, text, position, texOrigin, c, Color.Black);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin)
        {
            DrawStringOutlined(sb, text, position, texOrigin, DefaultTextColor, Color.Black);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, SpriteFont font)
        {
            DrawStringOutlined(sb, text, position, texOrigin, DefaultTextColor, Color.Black, font);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f, float depth = 0f)
        {
            DrawStringOutlined(sb, text, position, Color.Black, DefaultTextColor, hAlign, vAlign, opacity, depth);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f, float depth = 0)
        {
            Vector2 origin = Vector2.Zero;
            Vector2 size = UIManager.Font.MeasureString(text);
            int xx = 0, yy = 0;
            switch (hAlign)
            {
                //case HorizontalAlignment.Left:
                //   // origin = Vector2.Zero;
                //    xx = 0;
                //    break;
                case HorizontalAlignment.Center:
                   // origin = new Vector2(width / 2, 0);
                    xx = (int)size.X / 2;
                    break;
                case HorizontalAlignment.Right:
                  //  origin = new Vector2(width, 0);
                    xx = (int)size.X;
                    break;
                default:
                    break;
            }
            switch (vAlign)
            {
                //case VerticalAlignment.Top:
                //    // origin = Vector2.Zero;
                //    yy = 0;
                //    break;
                case VerticalAlignment.Center:
                    // origin = new Vector2(width / 2, 0);
                    yy = (int)size.Y / 2;
                    break;
                case VerticalAlignment.Bottom:
                    //  origin = new Vector2(width, 0);
                    yy = (int)size.Y;
                    break;
                default:
                    break;
            }
            origin = new Vector2(xx, yy);

            //for (int d = 0; d < 8; d++)
            //{
            //    double dir = d * (Math.PI / 4);
            //    tempVect = new Vector2((float)Math.Round(Math.Cos(dir)), -(float)Math.Round(Math.Sin(dir)));
            //    sb.DrawString(Font, text, position + new Vector2(tempVect.X, tempVect.Y), new Color(0f, 0f, 0f, opacity), 0, origin, 1, SpriteEffects.None, 0); //Color.Black * opacity
            //}
            foreach(var n in Vector2.Zero.GetNeighbors8() )
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, 1, SpriteEffects.None, depth); //Color.Black * opacity

            sb.DrawString(Font, text, position, new Color(1f, 1f, 1f, opacity), 0, origin, 1, SpriteEffects.None, depth);
        }
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, Color bgColor, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f)
        {
            Vector2 origin = Vector2.Zero;
            Vector2 size = UIManager.Font.MeasureString(text);
            int xx = 0, yy = 0;
            switch (hAlign)
            {
                case HorizontalAlignment.Center:
                    xx = (int)size.X / 2;
                    break;
                case HorizontalAlignment.Right:
                    xx = (int)size.X;
                    break;
                default:
                    break;
            }
            switch (vAlign)
            {
                case VerticalAlignment.Center:
                    yy = (int)size.Y / 2;
                    break;
                case VerticalAlignment.Bottom:
                    yy = (int)size.Y;
                    break;
                default:
                    break;
            }
            origin = new Vector2(xx, yy);
            position = position.Round();
            //var bounds = new Rectangle((int)position.X + 1, (int)position.Y + 1, (int)size.X + 2, (int)size.Y + 2);
            var bounds = new Vector4(position.X, position.Y, size.X + 2, size.Y + 2);

            bounds.DrawHighlight(sb, bgColor, new Vector2(xx / size.X, yy / size.Y), 0);
            foreach (var n in Vector2.Zero.GetNeighbors8())
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, 1, SpriteEffects.None, 0); //Color.Black * opacity

            sb.DrawString(Font, text, position, new Color(1f, 1f, 1f, opacity), 0, origin, 1, SpriteEffects.None, 0);
        }
        
        static public void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, float scale, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f)
        {
            Vector2 tempVect, origin = Vector2.Zero;
            Vector2 size = UIManager.Font.MeasureString(text);
            int xx = 0, yy = 0;
            switch (hAlign)
            {
                //case HorizontalAlignment.Left:
                //   // origin = Vector2.Zero;
                //    xx = 0;
                //    break;
                case HorizontalAlignment.Center:
                    // origin = new Vector2(width / 2, 0);
                    xx = (int)size.X / 2;
                    break;
                case HorizontalAlignment.Right:
                    //  origin = new Vector2(width, 0);
                    xx = (int)size.X;
                    break;
                default:
                    break;
            }
            switch (vAlign)
            {
                //case VerticalAlignment.Top:
                //    // origin = Vector2.Zero;
                //    yy = 0;
                //    break;
                case VerticalAlignment.Center:
                    // origin = new Vector2(width / 2, 0);
                    yy = (int)size.Y / 2;
                    break;
                case VerticalAlignment.Bottom:
                    //  origin = new Vector2(width, 0);
                    yy = (int)size.Y;
                    break;
                default:
                    break;
            }
            origin = new Vector2(xx, yy);

            //for (int d = 0; d < 8; d++)
            //{
            //    double dir = d * (Math.PI / 4);
            //    tempVect = new Vector2((float)Math.Round(Math.Cos(dir)), -(float)Math.Round(Math.Sin(dir)));
            //    sb.DrawString(Font, text, position + new Vector2(tempVect.X, tempVect.Y), new Color(0f, 0f, 0f, opacity), 0, origin, scale, SpriteEffects.None, 0); //Color.Black * opacity
            //}
            foreach (var n in Vector2.Zero.GetNeighbors8())
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, scale, SpriteEffects.None, 0); //Color.Black * opacity

            sb.DrawString(Font, text, position, new Color(1f, 1f, 1f, opacity), 0, origin, scale, SpriteEffects.None, 0);
        }
        public static Control GetMouseOver()
        {
            return MouseOverList.Count == 0 ? null : MouseOverList[MouseOverList.Keys.Max()];
        }
        public static bool IsMouseOver(Control control)
        {
            return MouseOverList[MouseOverList.Keys.Max()] == control;
        }

        public bool RemoveWindow(Window window)
        {
            //return Layers[window.Layer].Remove(window);

            /// if the window was a dialog, move the dialogblock behind the next topmost window, or remove it if there are no other dialogs
            if (Layers[window.Layer].Remove(window))
            {
                if (window.Layer == LayerTypes.Dialog)
                {
                    Layers[LayerTypes.Dialog].Remove(DialogBlock.Instance);
                    int index = Layers[LayerTypes.Dialog].Count - 1;
                    if (index < 0)
                        return true;
                    Layers[LayerTypes.Dialog].Insert(index, DialogBlock.Instance);
                }
                return true;
            }
            return false;
        }
        public bool Remove(Control control)
        {
            //return Layers[window.Layer].Remove(window);

            /// if the window was a dialog, move the dialogblock behind the next topmost window, or remove it if there are no other dialogs
            if (Layers[control.Layer].Remove(control))
            {
                if (control.Layer == LayerTypes.Dialog)
                {
                    Layers[LayerTypes.Dialog].Remove(DialogBlock.Instance);
                    int index = Layers[LayerTypes.Dialog].Count - 1;
                    if (index < 0)
                        return true;
                    Layers[LayerTypes.Dialog].Insert(index, DialogBlock.Instance);
                }
                return true;
            }
            return false;
        }

        public static void DrawBorder(SpriteBatch sb, BackgroundStyle regions, Rectangle bounds)
        {
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X, bounds.Y), regions.TopLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X + bounds.Width - 11, bounds.Y), regions.TopRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - 11), regions.BottomLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X + bounds.Width - 11, bounds.Y + bounds.Height - 11), regions.BottomRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y, bounds.Width - 22, 11), regions.Top, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X, bounds.Y + 11, 11, bounds.Height - 22), regions.Left, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + bounds.Width - 11, bounds.Y + 11, 11, bounds.Height - 22), regions.Right, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y + bounds.Height - 11, bounds.Width - 22, 11), regions.Bottom, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y + 11, bounds.Width - 22, bounds.Height - 22), regions.Center, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }
        public static void DrawBorder(SpriteBatch sb, BackgroundStyle regions, Rectangle bounds, Color color)
        {
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X, bounds.Y), regions.TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X + bounds.Width - 11, bounds.Y), regions.TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - 11), regions.BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(bounds.X + bounds.Width - 11, bounds.Y + bounds.Height - 11), regions.BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y, bounds.Width - 22, 11), regions.Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X, bounds.Y + 11, 11, bounds.Height - 22), regions.Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + bounds.Width - 11, bounds.Y + 11, 11, bounds.Height - 22), regions.Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y + bounds.Height - 11, bounds.Width - 22, 11), regions.Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(regions.SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y + 11, bounds.Width - 22, bounds.Height - 22), regions.Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }
        public static void DrawBorder(SpriteBatch sb, BackgroundStyle regions, int width, int height)
        {
            sb.Draw(regions.SpriteSheet, new Vector2(0), regions.TopLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(width - 11, 0), regions.TopRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(0, height - 11), regions.BottomLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Vector2(width - 11, height - 11), regions.BottomRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(regions.SpriteSheet, new Rectangle(11, 0, width - 22, 11), regions.Top, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(0, 11, 11, height - 22), regions.Left, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(width - 11, 11, 11, height - 22), regions.Right, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(regions.SpriteSheet, new Rectangle(11, height - 11, width - 22, 11), regions.Bottom, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(regions.SpriteSheet, new Rectangle(11, 11, width - 22, height - 22), regions.Center, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }
        public static void DrawBorder(SpriteBatch sb, Texture2D spritesheet, BackgroundStyle regions, int width, int height)
        {
            sb.Draw(spritesheet, new Vector2(0), regions.TopLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(spritesheet, new Vector2(width - 11, 0), regions.TopRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(spritesheet, new Vector2(0, height - 11), regions.BottomLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(spritesheet, new Vector2(width - 11, height - 11), regions.BottomRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(spritesheet, new Rectangle(11, 0, width - 22, 11), regions.Top, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(spritesheet, new Rectangle(0, 11, 11, height - 22), regions.Left, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(spritesheet, new Rectangle(width - 11, 11, 11, height - 22), regions.Right, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(spritesheet, new Rectangle(11, height - 11, width - 22, 11), regions.Bottom, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(spritesheet, new Rectangle(11, 11, width - 22, height - 22), regions.Center, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }

        public static string WrapText(string text, int maxwidth)
        {
            int pos = 0, linewidth = 0;
            char currentchar;
            string oldtext = text, newtext = "", currentword = "";
            while (pos < oldtext.Length)
            {
                currentchar = text[pos];
                linewidth += (int)(UIManager.Font.MeasureString(currentchar.ToString())).X;
                switch (currentchar.ToString())
                {
                    case " ":
                        if (linewidth >= maxwidth)
                        {
                            newtext += "\n";
                            linewidth = 0;
                        }
                        newtext += currentword + " ";
                        currentword = "";
                        break;
                    case "\n":
                        newtext += currentword + "\n";
                        currentword = "";
                        linewidth = 0;
                        break;
                    default:
                        if (linewidth >= maxwidth)
                        {
                            newtext += "\n";
                            //currentword.Insert(0, "\n");
                            linewidth = 0;
                        }
                        currentword += currentchar.ToString();
                        break;
                }
                pos++;
            }
            newtext += currentword;
            return newtext;
        }


        public void Dispose()
        {
            WindowManagers.Remove(this);

            Game1.Instance.graphics.DeviceReset -= graphics_DeviceReset;

            //foreach (var layer in Layers)
            //    foreach (Control ctrl in layer.Value)
            //        ctrl.Dispose();
        }

        public void Initialize()
        {
            foreach (var layer in Layers)
                foreach (Control ctrl in layer.Value)
                    ctrl.Dispose();
            //Controls.Clear();
            Layers[LayerTypes.Nameplates] = new List<Control>();
            Layers[LayerTypes.Speechbubbles] = new List<Control>();
            Layers[LayerTypes.Windows] = new List<Control>();
            Layers[LayerTypes.Dialog] = new List<Control>();
        }

        public void OnGotFocus() { }
        public void OnLostFocus() { }

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            foreach (var layer in Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleKeyPress(e);
            }
        }
        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (var layer in Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleKeyDown(e);
                //if (!e.Handled)
                //{
                //    //if ((char)e.KeyCode == 27) // escape
                //    if (e.KeyCode == System.Windows.Forms.Keys.Escape)// 27) // escape
                //    {
                //        // hide topmost window or all windows?
                //        //Window topWindow = wins.Find(control => control as Window != null) as Window;
                //        //if (topWindow != null)
                //        //    topWindow.Close();
                //        foreach(var win in from control in wins where control is Window select control)
                //        {
                //            win.Hide();
                //        }
                //    }
                //}
            }
        }
        public void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;


            foreach (var layer in Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                foreach (Control c in wins)
                    c.HandleKeyUp(e);
                
                
                    ////if ((char)e.KeyCode == 27) // escape
                    //if (e.KeyCode == System.Windows.Forms.Keys.Escape)// 27) // escape
                    //{
                    //    // hide topmost window or all windows?
                    //    //Window topWindow = wins.Find(control => control as Window != null) as Window;
                    //    //if (topWindow != null)
                    //    //    topWindow.Close();
                    //    foreach (var win in from control in wins where control is Window select control)
                    //    {
                    //        win.Hide();
                    //    }
                    //}
                
            }
        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ////ActiveControl = null;
            ////Controller.Instance.MouseoverNext.Object = null;
            //foreach (var layer in Layers)
            //{
            //    List<Control> wins = layer.Value.ToList();
            //    wins.Reverse();
            //    //e.Update();

            //    foreach (Control c in wins)
            //        c.HandleMouseMove(e);
            //    //if (ActiveControl != null)
            //    //    Controller.Instance.MouseoverNext.Object = ActiveControl;

            //    // Console.WriteLine(ActiveControl);

            //    //List<Control> wins = Controls.ToList();
            //    //foreach (Control c in wins)
            //    //    c.HandleMouseMove(e);
            //}
            if (this.ActiveControl == null)
                return;
            //   ActiveControl.ToConsole();
            ActiveControl.HandleMouseMove(e);
            e.Handled = true;
        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (this.ActiveControl == null)
            //    return;
            //ActiveControl.HandleLButtonDown(e);
            //e.Handled = true;

            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleLButtonDown(e);
            }

            if (this.ActiveControl != null)
                e.Handled = true;
        }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
        //   ActiveControl.HandleLButtonUp();
            //ActiveControl = null;
            //Controller.Input.Update();
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleLButtonUp(e);
            }
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (ActiveControl == null)
            //    return;
            //ActiveControl.HandleRButtonDown(e);
            //e.Handled = true;

            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleRButtonDown(e);
            }
            if (this.ActiveControl != null)
                e.Handled = true;
        }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (ActiveControl != null)
            //    ActiveControl.HandleRButtonUp(e);
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleRButtonUp(e);
            }
        }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (ActiveControl != null)
            //    ActiveControl.HandleRButtonUp(e);
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleMiddleUp(e);
            }
        }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (ActiveControl != null)
            //    ActiveControl.HandleRButtonUp(e);
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleMiddleDown(e);
            }
        }
        public virtual void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if(ActiveControl!=null)
            //ActiveControl.HandleMouseWheel(e);

            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleMouseWheel(e);
            }
        }
        public virtual void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleLButtonDoubleClick(e);
            }
        }
        internal void OnGameEvent(GameEvent e)
        {
            foreach (var layer in Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.OnGameEvent(e);
            }
        }
       
    
        

        static List<Rectangle> DivideScreenOcta(Rectangle screen, Rectangle rect)
        {
            var xx = rect.X + rect.Width;
            var yy = rect.Y + rect.Height;
            var dx = screen.Width - xx;
            var dy = screen.Height - yy;

            var list = new List<Rectangle>(){
                new Rectangle(screen.X, screen.Y, rect.X, rect.Y),
                new Rectangle(rect.X, screen.Y, rect.Width, rect.Y),
                new Rectangle(xx, screen.Y, dx, rect.Y),

                new Rectangle(screen.X, rect.Y, rect.X, rect.Height),
                new Rectangle(xx, rect.Y, dx, rect.Height),

                new Rectangle(screen.X, yy, rect.X, dy),
                new Rectangle(rect.X, yy, rect.Width, dy),
                new Rectangle(xx, yy, dx, dy)
            };
            return list;
        }
        static List<Rectangle> DivideScreenQuad(Rectangle screen, Rectangle rect)
        {
            var right = rect.X + rect.Width;
            var bottom = rect.Y + rect.Height;
            var dx = screen.X + screen.Width - right;
            var dy = screen.Y + screen.Height - bottom;

            //var list = new List<Rectangle>(){
            //    new Rectangle(screen.X, screen.Y, screen.Width, rect.Y),
            //    new Rectangle(screen.X, bottom, screen.Width, dy),
            //    new Rectangle(screen.X, screen.Y, rect.X, screen.Height),
            //    new Rectangle(right, screen.Y, dx, screen.Height)
            //};
            var list = new List<Rectangle>();
            if (rect.Y > screen.Y) 
                list.Add(new Rectangle(screen.X, screen.Y, screen.Width, rect.Y - screen.Y));
            if (bottom < screen.Y + screen.Height) 
                list.Add(new Rectangle(screen.X, bottom, screen.Width, dy));
            if (rect.X > screen.X) 
                //list.Add(new Rectangle(screen.X, screen.Y, rect.X, screen.Height));
                list.Add(new Rectangle(screen.X, screen.Y, rect.X - screen.X, screen.Height));
            if (right < screen.X + screen.Width) 
                list.Add(new Rectangle(right, screen.Y, dx, screen.Height));

            return list;
        }
        static List<Rectangle> GetIntersections(List<Rectangle> list1, List<Rectangle> list2)
        {
            var intersections = new List<Rectangle>();
            foreach (var rect1 in list1)
                foreach (var rect2 in list2)
                {
                    if(rect1.Intersects(rect2))
                    intersections.Add(Rectangle.Intersect(rect1, rect2));
                    //var i = rect1.Intersects(rect2);
                }


            //foreach (var rect in intersections)
            //    foreach (var test in intersections.Except(new Rectangle[] { rect }))
            //        if (rect.Intersects(test))
            //            throw new Exception();

            return intersections;
        }
        List<Rectangle> GetFreeRectangles()
        {
            List<Rectangle> freeRects = new List<Rectangle>() { UIManager.Bounds };
            var divided = true;
            var windows = this.Layers[LayerTypes.Windows].Where(c => c is Window).ToList();
            windows.Reverse();
            while (divided)
            {
                divided = false;
               
                foreach (var control in windows) // TODO: fix HUD being a control itself
                {
                    foreach (var rect in freeRects.ToList())
                    {
                        var bounds = control.BoundsScreen;
                        if (rect.Intersects(bounds))
                        {
                            //divided = true;
                            freeRects.Remove(rect);
                            var divisions = DivideScreenQuad(rect, bounds);
                            foreach (var div in divisions)
                                if (!freeRects.Contains(div))
                                    freeRects.Add(div);
                           // break;
                            //freeRects.AddRange(divisions);
                        }
                    }
                    //if (divided)
                    //    break;
                    //var divisions = DivideScreenQuad(UIManager.Bounds, control.Bounds);
                    //freeRects.AddRange(divisions);
                }
            }
            return freeRects;
        }
        public Rectangle FindBestUncoveredRectangle(Vector2 dimensions)
        {
            int w = (int)dimensions.X, h = (int)dimensions.Y;
            var freeRects = this.GetFreeRectangles();
            Rectangle bestRect = new Rectangle(0, 0, w, h);
            var minSize = int.MaxValue;
            var size = w*h;
            var ordered = freeRects.OrderBy(r => new Vector2(r.X, r.Y).LengthSquared()).ToList();
            foreach (var rect in ordered)
            {
                if (w <= rect.Width && h <= rect.Height)
                    return rect;
                //else
                //    continue;

                //var rectsize = rect.Width * rect.Height;
                //if (rectsize < size)
                //    continue;
                //if (rectsize > minSize)
                //    continue;
                //minSize = rectsize;
                //bestRect = rect;
            }
            return bestRect;
        }

    }
}
