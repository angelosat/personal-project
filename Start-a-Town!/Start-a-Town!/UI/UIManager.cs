using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UIManager : IDisposable, IKeyEventHandler
    {
        public enum Events
        {
            NpcUICreated,
            SelectedChanged
        }

        static float _Scale = 1;
        public static float Scale
        {
            get { return _Scale; }
            set
            {
                UIScaleEventArgs e = new UIScaleEventArgs(_Scale, value);
                foreach (UIManager manager in WindowManagers)
                {
                    foreach (var layer in manager.Layers)
                    {
                        foreach (var ctrl in layer.Value)
                        {
                            ctrl.Reposition(e);
                        }
                    }
                }

                _Scale = value;
                UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Width, Height);

            }
        }



        public static Rectangle Bounds
        { get { return new Rectangle(0, 0, Width, Height); } }
        public static Vector2 Center { get { return Game1.ScreenSize / (2 * Scale); } }
        public static int Width
        {
            get
            {
                return (int)(Game1.Instance.graphics.PreferredBackBufferWidth / Scale);
            }
        }
        public static int Height
        {
            get
            {
                return (int)(Game1.Instance.graphics.PreferredBackBufferHeight / Scale);
            }
        }

        public static Vector2 Size { get { return new Vector2(Width, Height); } }
        public static Vector2 Mouse { get { return Controller.Instance.MouseLocation / Scale; } }
        public static Rectangle MouseRect { get { return new Rectangle((int)Mouse.X, (int)Mouse.Y, 1, 1); } }
        public static SortedList<float, Control> MouseOverList;
        public static SpriteFont Font = Game1.Instance.Content.Load<SpriteFont>("DefaultFont");
        public static SpriteFont FontBold = Game1.Instance.Content.Load<SpriteFont>("BoldFont");
        public static SpriteFont Symbols = Game1.Instance.Content.Load<SpriteFont>("Symbols");
        public static Color Tint = Color.SteelBlue;
        public static Texture2D frameSprite, SlotSprite, defaultButtonSprite, DefaultIconButtonSprite, ItemSheet, DefaultProgressBar, DefaultProgressBarStrip, ProgressBarBorder, DefaultDropdownSprite, DefaultLoadingSprite,
            DefaultVScrollbarSprite, DefaultHScrollbarSprite,
            DefaultTrackBarSprite, DefaultTrackBarThumbSprite, SampleButton, Icons16x16, LargeButton, Icons32, Icon16Background, TextureTickBox;
        public static int DefaultButtonHeight = 23, LineHeight;
        public static int BorderPx = 5, TitlePx = 26;
        public static Color DefaultTextColor = Color.LightGray;

        public DialogBlock DialogBlock;

        public static List<UIManager> WindowManagers = new();

        public static Window MouseOverWindow;
        public Window ActiveWindow => this.ActiveControl?.GetWindow();

        public static readonly GuiLayer
            LayerNameplates = new(),
            LayerSpeechbubbles = new(),
            LayerHud = new(),
            LayerWindows = new(),
            LayerDialog = new();
        public readonly Dictionary<GuiLayer, List<Control>> Layers = new()
        {
            { LayerNameplates, new() },
            { LayerSpeechbubbles, new() },
            { LayerHud, new() },
            { LayerWindows, new() },
            { LayerDialog, new() }
        };

        public List<Control> this[GuiLayer layer] { get { return this.Layers[layer]; } }

        public int LastScreenWidth, LastScreenHeight;
        public Window Dialog;

        static Texture2D _Highlight;
        public static Texture2D Highlight
        {
            get
            {
                if (_Highlight == null)
                {
                    _Highlight = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                    _Highlight.SetData(new Color[] { Color.White });
                }
                return _Highlight;
            }
        }
        static Texture2D _TransparentHighlight;
        public static Texture2D TransparentHighlight
        {
            get
            {
                if (_TransparentHighlight is null)
                {
                    _TransparentHighlight = new Texture2D(Game1.Instance.GraphicsDevice, 3, 3);
                    _TransparentHighlight.SetData(new Color[] { Color.White, Color.White, Color.White, Color.White, Color.Transparent, Color.White, Color.White, Color.White, Color.White });
                }
                return _TransparentHighlight;
            }
        }

        static Texture2D _Shade;
        public static Texture2D Shade
        {
            get
            {
                if (_Shade is null)
                {
                    _Shade = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                    _Shade.SetData(new Color[] { Color.Black });
                }
                return _Shade;
            }
        }

        public static Texture2D Cursor;
        public RenderTarget2D DimScreen;

        public static Atlas Atlas = new Atlas("UI");
        public static readonly Atlas.Node.Token Btn16 = Atlas.Load("Graphics/Gui/gui-icon");
        public static readonly Atlas.Node.Token ArrowUp = Atlas.Load("Graphics/Gui/gui-up1");
        public static readonly Atlas.Node.Token ArrowDown = Atlas.Load("Graphics/Gui/gui-down1");
        public static readonly Atlas.Node.Token ArrowLeft = Atlas.Load("Graphics/Gui/gui-left1");
        public static readonly Atlas.Node.Token ArrowRight = Atlas.Load("Graphics/Gui/gui-right1");
        public static readonly Atlas.Node.Token IconX = Atlas.Load("Graphics/Gui/gui-x");
        public static readonly Atlas.Node.Token IconSave = Atlas.Load("Graphics/Gui/gui-save");
        public static readonly Atlas.Node.Token IconLoad = Atlas.Load("Graphics/Gui/gui-load");
        public static readonly Atlas.Node.Token IconDisk = Atlas.Load("Graphics/Gui/gui-saveload");
        public static readonly Atlas.Node.Token Tickbox = Atlas.Load("Graphics/Gui/gui-tickbox2a");
        public static readonly Atlas.Node.Token MousePointer = Atlas.Load("Graphics/cursor-default");
        public static readonly Atlas.Node.Token MousePointerGrayscale = Atlas.Load("Graphics/cursor-default", true);
        public static readonly Atlas.Node.Token SpriteConstruction = Atlas.Load("Graphics/Icons/spr-constr");

        public static readonly Texture2D UpArrow = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-up1");
        public static readonly Texture2D DownArrow = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-down1");


        public static void LoadContent()
        {
            Cursor = Game1.Instance.Content.Load<Texture2D>("Graphics/cursor-default");
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

            var scaleNode = Engine.Config.Descendants("UIScale").FirstOrDefault();
            if (float.TryParse(scaleNode.Value, out float scale)) // do i need to check for scalenode == null?
            {
                Scale = scale;
            }
            else
            {
                Scale = 1;
            }
        }

        internal void OnSelectedTargetChanged(TargetArgs target)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.OnSelectedTargetChanged(target);
                }
            }
        }

        static RenderTarget2D UITexture;
        public UIManager()
        {
            WindowManagers.Add(this);

            Game1.Instance.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(this.graphics_DeviceReset);
            this.LastScreenWidth = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
            this.LastScreenHeight = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;

            this.DimScreen = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1);
            Game1.Instance.GraphicsDevice.SetRenderTarget(this.DimScreen);
            Game1.Instance.GraphicsDevice.Clear(Color.Black);
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        public List<Control> ControlsInMemory = new List<Control>();
        void graphics_DeviceReset(object sender, EventArgs e)
        {
            this.Reset();
        }

        private void Reset()
        {
            int newWidth = Game1.Instance.graphics.PreferredBackBufferWidth;
            int newHeight = Game1.Instance.graphics.PreferredBackBufferHeight;

            float wRatio = newWidth / (float)this.LastScreenWidth;
            float hRatio = newHeight / (float)this.LastScreenHeight;

            this.LastScreenWidth = newWidth;
            this.LastScreenHeight = newHeight;

            UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, newWidth, newHeight);
            foreach (Control ctrl in this.ControlsInMemory)
            {
                ctrl.Reposition(new Vector2(wRatio, hRatio));
                ctrl.Invalidate(true);
            }

            this.DimScreen = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1);
            Game1.Instance.GraphicsDevice.SetRenderTarget(this.DimScreen);
            Game1.Instance.GraphicsDevice.Clear(Color.Black);
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        public Control FocusedControl;

        public Control ActiveControl
        {
            get { return Controller.Instance.MouseoverBlock.Object as Control; }
            set { Controller.Instance.MouseoverBlock.Object = value; }
        }

        public void Screen_MouseMove()
        {
            MouseOverWindow = null;
        }

        public void Update(Game1 game, GameTime gt)
        {

            Control lastactive = Controller.Instance.MouseoverBlock.Object as Control;// ActiveControl;
            foreach (var layer in this.Layers)
            {
                foreach (var ctrl in layer.Value.ToList())
                {
                    ctrl.Update();
                    ctrl.Update(new Rectangle(0, 0, (int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y));
                }
            }

            Control nextactive = Controller.Instance.MouseoverBlockNext.Object as Control;

            if (nextactive != lastactive)
            {
                nextactive?.OnMouseEnter();
                lastactive?.OnMouseLeave();
            }
        }

        public bool CloseAll()
        {

            bool closedsomething = false;
            List<Control> toclose = new List<Control>();
            foreach (var layer in this.Layers)
            {
                toclose.AddRange(layer.Value.FindAll(foo => foo is Window));
            }

            if (toclose.Count > 0)
            {
                foreach (Control c in toclose)
                {
                    if (c is Window)
                    {
                        if ((c as Window).Closable)
                        {
                            (c as Window).Close();
                            closedsomething = true;
                        }
                    }
                }
            }

            return closedsomething;
        }

        public void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            foreach (var layer in this.Layers)
            {
                foreach (var ctrl in layer.Value.ToList())
                {
                    ctrl.DrawOnCamera(sb, camera);
                }
            }
        }

        public void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            foreach (var layer in this.Layers)
            {
                foreach (var ctrl in layer.Value.ToList())
                {
                    ctrl.DrawWorld(sb, camera);
                }
            }
        }

        public void Draw(SpriteBatch sb, Camera camera)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            RenderTarget2D uiTexture = UITexture;
            gd.SetRenderTarget(uiTexture);
            gd.Clear(Color.Transparent);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            if (camera != null)
            {
                this.DrawOnCamera(sb, camera);
            }

            foreach (var layer in this.Layers)
            {
                foreach (var ctrl in layer.Value.ToList())
                {
                    ctrl.Draw(sb, Bounds);
                }
            }

            TooltipManager.Instance.Draw(sb);
            DragDropManager.Instance.Draw(sb);
            sb.End();
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Scale % 1 > 0 ? SamplerState.AnisotropicClamp : SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            sb.Draw(uiTexture, new Rectangle(0, 0, (int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y), Color.White);
            sb.End();
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color fill, Color outline, SpriteFont font)
        {
            if (text == null)
            {
                return;
            }

            if (text == "")
            {
                return;
            }

            position = position.Round();
            Vector2 origin = font.MeasureString(text) * texOrigin;
            origin = new Vector2((int)origin.X, (int)origin.Y);
            foreach (var n in Vector2.Zero.GetNeighbors8())
            {
                sb.DrawString(font, text, position + n, outline, 0, origin, 1, SpriteEffects.None, 0);
            }

            sb.DrawString(font, text, position, fill, 0, origin, 1, SpriteEffects.None, 0);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color fill, Color outline)
        {
            DrawStringOutlined(sb, text, position, texOrigin, fill, outline, UIManager.Font);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin)
        {
            DrawStringOutlined(sb, text, position, texOrigin, DefaultTextColor, Color.Black);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, SpriteFont font)
        {
            DrawStringOutlined(sb, text, position, texOrigin, DefaultTextColor, Color.Black, font);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f, float depth = 0f)
        {
            DrawStringOutlined(sb, text, position, Color.Black, DefaultTextColor, hAlign, vAlign, opacity, depth);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f, float depth = 0)
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

            foreach (var n in Vector2.Zero.GetNeighbors8())
            {
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, 1, SpriteEffects.None, depth);
            }

            sb.DrawString(Font, text, position, new Color(1f, 1f, 1f, opacity), 0, origin, 1, SpriteEffects.None, depth);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, float scale, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Top, float opacity = 1f)
        {
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
            var origin = new Vector2(xx, yy);
            foreach (var n in Vector2.Zero.GetNeighbors8())
            {
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, scale, SpriteEffects.None, 0);
            }

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
            /// if the window was a dialog, move the dialogblock behind the next topmost window, or remove it if there are no other dialogs
            if (this.Layers[window.Layer].Remove(window))
            {
                if (window.Layer == LayerDialog)
                {
                    this.Layers[LayerDialog].Remove(DialogBlock.Instance);
                    int index = this.Layers[LayerDialog].Count - 1;
                    if (index < 0)
                    {
                        return true;
                    }

                    this.Layers[LayerDialog].Insert(index, DialogBlock.Instance);
                }
                return true;
            }
            return false;
        }
        public bool Remove(Control control)
        {
            /// if the window was a dialog, move the dialogblock behind the next topmost window, or remove it if there are no other dialogs
            if (this.Layers[control.Layer].Remove(control))
            {
                if (control.Layer == LayerDialog)
                {
                    this.Layers[LayerDialog].Remove(DialogBlock.Instance);
                    int index = this.Layers[LayerDialog].Count - 1;
                    if (index < 0)
                    {
                        return true;
                    }

                    this.Layers[LayerDialog].Insert(index, DialogBlock.Instance);
                }
                return true;
            }
            return false;
        }

        public static string WrapText(string text, int maxwidth)
        {
            int pos = 0, linewidth = 0;
            char currentchar;
            string oldtext = text, newtext = "", currentword = "";
            while (pos < oldtext.Length)
            {
                currentchar = text[pos];
                linewidth += (int)(Font.MeasureString(currentchar.ToString())).X;
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
            Game1.Instance.graphics.DeviceReset -= this.graphics_DeviceReset;
        }

        public void Initialize()
        {
            foreach (var layer in this.Layers)
            {
                foreach (Control ctrl in layer.Value)
                {
                    ctrl.Dispose();
                }
            }
        }

        public void OnGotFocus() { }
        public void OnLostFocus() { }

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            foreach (var layer in this.Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleKeyPress(e);
                }
            }
        }
        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (var layer in this.Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleKeyDown(e);
                }
            }
        }
        public void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            foreach (var layer in this.Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                foreach (Control c in wins)
                {
                    c.HandleKeyUp(e);
                }
            }
        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveControl == null)
            {
                return;
            }

            this.ActiveControl.HandleMouseMove(e);
            e.Handled = true;
        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleLButtonDown(e);
                }
            }

            if (this.ActiveControl != null)
            {
                e.Handled = true;
            }
        }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleLButtonUp(e);
                }
            }
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleRButtonDown(e);
                }
            }
            if (this.ActiveControl is not null)
            {
                e.Handled = true;
            }
        }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleRButtonUp(e);
                }
            }
        }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleMiddleUp(e);
                }
            }
        }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleMiddleDown(e);
                }
            }
        }
        public virtual void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleMouseWheel(e);
                }
            }
        }
        public virtual void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.HandleLButtonDoubleClick(e);
                }
            }
        }
        internal void OnGameEvent(GameEvent e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                {
                    c.OnGameEvent(e);
                }
            }
        }

        static List<Rectangle> DivideScreenQuad(Rectangle screen, Rectangle rect)
        {
            var right = rect.X + rect.Width;
            var bottom = rect.Y + rect.Height;
            var dx = screen.X + screen.Width - right;
            var dy = screen.Y + screen.Height - bottom;

            var list = new List<Rectangle>();
            if (rect.Y > screen.Y)
            {
                list.Add(new Rectangle(screen.X, screen.Y, screen.Width, rect.Y - screen.Y));
            }

            if (bottom < screen.Y + screen.Height)
            {
                list.Add(new Rectangle(screen.X, bottom, screen.Width, dy));
            }

            if (rect.X > screen.X)
            {
                list.Add(new Rectangle(screen.X, screen.Y, rect.X - screen.X, screen.Height));
            }

            if (right < screen.X + screen.Width)
            {
                list.Add(new Rectangle(right, screen.Y, dx, screen.Height));
            }

            return list;
        }
        List<Rectangle> GetFreeRectangles()
        {
            List<Rectangle> freeRects = new List<Rectangle>() { UIManager.Bounds };
            var divided = true;
            var windows = this.Layers[LayerWindows].Where(c => c is Window).ToList();
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
                            freeRects.Remove(rect);
                            var divisions = DivideScreenQuad(rect, bounds);
                            foreach (var div in divisions)
                            {
                                if (!freeRects.Contains(div))
                                {
                                    freeRects.Add(div);
                                }
                            }
                        }
                    }
                }
            }
            return freeRects;
        }
        public Rectangle FindBestUncoveredRectangle(Vector2 dimensions)
        {
            int w = (int)dimensions.X, h = (int)dimensions.Y;
            var freeRects = this.GetFreeRectangles();
            Rectangle bestRect = new Rectangle(0, 0, w, h);
            var size = w * h;
            var ordered = freeRects.OrderBy(r => new Vector2(r.X, r.Y).LengthSquared()).ToList();
            foreach (var rect in ordered)
            {
                if (w <= rect.Width && h <= rect.Height)
                {
                    return rect;
                }
            }
            return bestRect;
        }

    }
}
