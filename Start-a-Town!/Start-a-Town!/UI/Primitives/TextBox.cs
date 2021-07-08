using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using System.Threading;

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

    public class TextBox : Label
    {
        Timer2 CursorTimer;
        int _SelectionStart = 0;
        bool CursorVisible;
        public Func<char, bool> InputFilter = c => true;
        public Func<string, char, string> InputFunc = (current, input) => char.IsControl(input) ? current : (current + input);              
        public Action<KeyPressEventArgs> TextEnterFunc = (e) => { };
        public Action<string> EnterFunc = (text) => { };
        public Action<KeyPressEventArgs> EscapeFunc = (e) => { };
        public Action<string> TextChangedFunc = txt => { };
        public int MaxLength = int.MaxValue;

        protected override void OnTextChanged()
        {
            this.Invalidate();
        }
        public event EventHandler<TextEventArgs> TextEntered;
        protected virtual void OnTextEntered(char c)
        {
            TextEntered?.Invoke(this, new TextEventArgs(c));
            TextChangedFunc(this.Text);
        }

        public int SelectionStart
        {
            get { return _SelectionStart; }
            set { _SelectionStart = value; }
        }

        public TextBox() : base()
        { 
            Active = true;
            CursorTimer = new Timer2(Engine.TicksPerSecond / 2f);
            CursorTimer.Tick += new EventHandler<EventArgs>(CursorTimer_Tick);
            this.BackgroundColor = Color.Black * 0.5f;
            this.BackgroundColor = Color.White * .1f;
    }
        public TextBox(Vector2 position) : this() { Location = position; }
        public TextBox(Vector2 position, Vector2 size)
            : this(position)
        {
            this.Width = (int)size.X;
            this.Height = (int)size.Y; 
        }
        public TextBox(int width)
            : this()
        {
            this.Width = width;
            this.Height = DefaultHeight;
        }

        void CursorTimer_Tick(object sender, EventArgs e)
        {
            CursorVisible = !CursorVisible;
            this.Invalidate();
        }

        public override int Height
        {
            get
            {
                return UIManager.LineHeight;
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
               
                default:
                    if ((Keys)e.KeyValue == Keys.Up ||
                        (Keys)e.KeyValue == Keys.Down)
                    {
                        int index = ((Keys)e.KeyValue == Keys.Up) ? 1 : -1;
                        this.HistoryNextPrev(index);
                    }
                    break;
            }
        }

        void GetTextFromClipboard()
        {
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

        public override void HandleKeyPress(KeyPressEventArgs e)
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
                        EnterFunc(this.Text);
                    this.Text = "";
                    break;

                case (char)22: //paste
                    Thread t = new Thread(GetTextFromClipboard);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
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
        }
        
        public override void HandleKeyUp(KeyEventArgs e)
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
            else if (!char.IsControl(e.Char))
                txtBox.Text += e.Char;
        }
    }
}
