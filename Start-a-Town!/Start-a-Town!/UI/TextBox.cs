using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    public class TextEventArgs : EventArgs
    {
        public char Char;
        public TextEventArgs(char c)
        {
            Char = c;
        }
    }

    public class TextBox : Label //Control
    {
        // TODO: make a static list of active textboxes so i can disable other textboxes when selecting one

        #region Fields

       // public Texture2D Texture;
        Timer2 CursorTimer;
        int _SelectionStart = 0;
        //protected string _Text = "";
        bool CursorVisible;
        public Func<char, bool> InputFilter = c => true;
        public Func<string, char, string> InputFunc = (current, input) => char.IsControl(input) ? current : (current + input); // { return current; };               
        public Action<System.Windows.Forms.KeyPressEventArgs> TextEnterFunc = (e) => { };
        public Action<string> EnterFunc = (text) => { };
        public Action<System.Windows.Forms.KeyPressEventArgs> EscapeFunc = (e) => { };
        public Action<string> TextChangedFunc = txt => { };
        public int MaxLength = int.MaxValue;
        #endregion

        #region Events

        
        //public event EventHandler TextChanged;
        //protected virtual void OnTextChanged()
        //{
        //    if (TextChanged != null)
        //        TextChanged(this, EventArgs.Empty);
        //}
        protected override void OnTextChanged()
        {
            this.Invalidate();
        }
        public event EventHandler<TextEventArgs> TextEntered;
        protected virtual void OnTextEntered(char c)
        {
            if (TextEntered != null)
                TextEntered(this, new TextEventArgs(c));
            TextChangedFunc(this.Text);
        }

        #endregion

        #region Public Properties

        //public string Text
        //{
        //    get { return _Text; }
        //    set
        //    {
        //        _Text = value;
        //        Paint();
        //        OnTextChanged();
        //    }
        //}
        public int SelectionStart
        {
            get { return _SelectionStart; }
            set { _SelectionStart = value; }
        }

        #endregion

        public TextBox() : base()
        { 
            //this.Initialize(); 
            Active = true;
            CursorTimer = new Timer2(Engine.TicksPerSecond / 2f);
            CursorTimer.Tick += new EventHandler<EventArgs>(CursorTimer_Tick);
            this.BackgroundColor = Color.Black * 0.5f;
            this.BackgroundColor = Color.White * .1f;
    }
        public TextBox(Vector2 position) : this() { Location = position; }// this.Initialize(); }
       // public TextBox(Vector2 position, Vector2 size) : base(position, size) { this.Initialize(); }
        public TextBox(Vector2 position, Vector2 size)
            : this(position)
        {
            this.Width = (int)size.X;
            this.Height = (int)size.Y; 
            //this.Initialize();
 
        }
        public TextBox(int width)
            : this()
        {
            this.Width = width;
            this.Height = DefaultHeight;
        }
       
        //public override void Initialize()
        //{
        //    CursorTimer = new Timer2(Engine.TargetFps / 2f);
        //    CursorTimer.Tick += new EventHandler<EventArgs>(CursorTimer_Tick);
        //    this.BackgroundColor = Color.Black * 0.5f;
        //}



        void CursorTimer_Tick(object sender, EventArgs e)
        {
            CursorVisible = !CursorVisible;
            this.Invalidate();
        }

        public override int Height
        {
            get
            {
                return UIManager.LineHeight;// UIManager.Font.LineSpacing;
            }
            set
            {
                base.Height = value;
            }
        }


        public override void Select()
        {
            Enabled = true;

            base.Select();
        }

        public override void Unselect()
        {
            Enabled = false;
            base.Unselect();
        }

        public override void OnLostFocus()
        {
            Enabled = false;
            //if (!Enabled)
            base.OnLostFocus();
        }

        bool _Enabled;
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                bool old = _Enabled;
                _Enabled = value;
                if (_Enabled)
                {
                    if (!old)
                    {
                        CursorTimer.Start();
                        CursorVisible = true;
                    }
                }
                else
                {
                    CursorTimer.Stop();
                    CursorVisible = false;
                }
                this.Invalidate();
            }
        }
        public override void OnPaint(SpriteBatch sb)
        {
            //DrawShade(sb, 0.5f);
            //sb.Draw(Texture, Vector2.Zero, Color.White);
            base.OnPaint(sb);
            if (CursorVisible)
            {
                Vector2 textbounds = UIManager.Font.MeasureString(Text);
                sb.Draw(UIManager.Highlight, new Rectangle((int)textbounds.X, 0, 3, Height), Color.White);
            }
        }

        public override void Dispose()
        {
            Texture.Dispose();
            CursorTimer.Stop();
        }

        public void AppendText(string text)
        {
            Text += text;
        }

        public Action<int> HistoryNextPrev = i => { };

        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (!Enabled)
                return;
            if (e.Handled)
                return;
            e.Handled = true;

            switch (e.KeyValue)
            {
                case 37:
                    Console.WriteLine("back");
                    break;
                case 39:
                    Console.WriteLine("forward");
                    break;
                //case (int)System.Windows.Forms.Keys.V:
                //    "paste".ToConsole();
                //    break;
                //case (char)27: //escape
                //    Enabled = false;
                //    break;
                    
                default:
                    if ((System.Windows.Forms.Keys)e.KeyValue == System.Windows.Forms.Keys.Up ||
                        (System.Windows.Forms.Keys)e.KeyValue == System.Windows.Forms.Keys.Down)
                    {
                        int index = (((System.Windows.Forms.Keys)e.KeyValue == System.Windows.Forms.Keys.Up) ? 1 : -1);
                        this.HistoryNextPrev(index);
                    }
                    break;
            }
        }

     //   [STAThread]
        void GetTextFromClipboard()
        {
            //Task.Factory.StartNew(
            //() =>
            //{
                //string text = Clipboard.ContainsText() ? Clipboard.GetText() : "";
            //}
            //);
            // TODO: filter each char on a callback function
            Text += Clipboard.ContainsText() ? Clipboard.GetText() : "";
        }

        /// <summary>
        /// temporary fix until i figure out what the fuck is wrong with changing background styles messing up size of the control
        /// </summary>
        public override BackgroundStyle BackgroundStyle
        {
            get { return _BackgroundStyle; }
            set
            {
                _BackgroundStyle = value;
                ClientLocation = new Vector2(_BackgroundStyle.Border);
            }
        }

        public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!Enabled)
                return;
            if (e.Handled)
                return;
            e.Handled = true;
            switch (e.KeyChar)
            {
                case '\b':
                    Text = Text.Length > 0 ? Text.Remove(Text.Length - 1) : "";
                    OnTextEntered(e.KeyChar);
                    break;

                case (char)27: //escape
                    EscapeFunc(e);
                    break;

                case '\r'://13) //enter
                   // if (this.Text.Length > 0)
                        EnterFunc(this.Text);
                    this.Text = "";
                    break;

                case (char)22: //paste
                //case 'v':
                    Thread t = new Thread(GetTextFromClipboard);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    //string paste = Clipboard.GetText();
                    //Text += paste;
                    break;

                default:
                    if (this.Text.Length == this.MaxLength)
                        break;
                    if (!this.InputFilter(e.KeyChar))
                        break;
                    this.Text = InputFunc(this.Text, e.KeyChar);
                    TextEnterFunc(e);
                    OnTextEntered(e.KeyChar);
                    break;
            }
        //    TextEnterFunc(e);
        }
        
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (!Enabled)
                return;
            if (e.Handled)
                return;
            e.Handled = true;
            switch (e.KeyValue)
            {
                case 27: //escape
                    Enabled = false;
                    break;
                default:
                    break;
            }
        }

        //public override void HandleInput(InputState e)
        //{
        //    base.HandleInput(e);
        //    e.KeyHandled = Enabled;
        //    if (!Enabled)
        //        return;
        //}

        //public override void HandleInput(InputState input)
        //{
        //    base.HandleInput(input);
        //    if (!Enabled)
        //        return;

        //    //input.Handled = true;
        //    Keys[] keys = input.CurrentKeyboardState.GetPressedKeys();
        //    if (keys.Length > 0)
        //        foreach (Keys key in keys)
        //            if (input.IsKeyPressed(key))
        //            {
        //                input.Handled = true;
        //                switch (key)
        //                {
        //                    case Keys.Back:
        //                        if (Text.Length > 0)
        //                            Text = Text.Remove(Text.Length - 1);
        //                        break;
        //                    case Keys.OemMinus:
        //                        Text += "-";
        //                        break;
        //                    case Keys.OemPlus:
        //                        Text += "+";
        //                        break;
        //                    default:
        //                        char letter = KeyboardMap.MapKey(key, keys);

        //                        if (letter != '\0')
        //                        {
        //                            //Console.WriteLine(letter);
        //                            input.Handled = true;
        //                            OnTextEntered(letter);
        //                        }
        //                        break;
        //                }
        //            }
        //}

        static public void DefaultTextHandling(TextBox txtBox, TextEventArgs e)
        {
            if (e.Char == 13) //enter
            {
                txtBox.Enabled = false;
            }
            else if (e.Char == 27) //escape
            {
                txtBox.Enabled = false;
            }
            else if(!char.IsControl(e.Char))
           // else
                txtBox.Text += e.Char;
        }
    }
}
