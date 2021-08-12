using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class Label : ButtonBase
    {
        public static int DefaultHeight = UIManager.Font.LineSpacing + 2;

        HorizontalAlignment _Halign;
        public HorizontalAlignment Halign
        {
            get => this._Halign;
            set => this._Halign = value;
        }
        public VerticalAlignment Valign;
        public Color TextBackground = Color.Transparent;
        public Func<Color> TextBackgroundFunc = () => Color.Transparent;

        public override string ToString()
        {
            return "Label: " + this.Text ?? this.Name;
        }

        public override int Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                if (value > 0)
                    this.Invalidate();
            }
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            var c = this.TextBackgroundFunc();
            this.BoundsScreen.DrawHighlight(sb, c);
            base.Draw(sb, viewport);
        }

        public Color ActiveColor = Color.Lime;
        public override void OnPaint(SpriteBatch sb)
        {
            var pos = new Vector2((int)this.Halign * .5f, .5f);
            var outlineOffset = this.Halign == HorizontalAlignment.Left ? 1 : (this.Halign == HorizontalAlignment.Right ? -1 : 0);
            UIManager.DrawStringOutlined(
                sb,
                this.Text,
                pos * this.Dimensions + new Vector2(outlineOffset, 0) + ((this.Active && this.Pressed) ? Vector2.UnitY : Vector2.Zero),
                pos,
                (this.MouseHover && this.Active) ? this.ActiveColor : this.TextColor,
                this.TextOutline,
                this.Font);
        }


        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            this.Height = Math.Max(this.Height, Label.DefaultHeight);

            switch (this.Halign)
            {
                case HorizontalAlignment.Left:
                    this.Origin.X = 0;
                    break;
                case HorizontalAlignment.Center:
                    this.Origin.X = this.Width / 2;
                    break;
                case HorizontalAlignment.Right:
                    this.Origin.X = this.Width;
                    break;
                default:
                    break;
            }
            switch (this.Valign)
            {
                case VerticalAlignment.Top:
                    this.Origin.Y = 0;
                    break;
                case VerticalAlignment.Center:
                    this.Origin.Y = this.Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    this.Origin.Y = this.Height;
                    break;
                default:
                    break;
            }
        }

        public Label(int width) : base()
        {
            this.Text = "";
            this.Width = width;
            this.Height = Label.DefaultHeight;
            this.Active = false;
        }
        public Label() : this(Vector2.Zero, "") { }
        public Label(Func<string> textFunc) : this(Vector2.Zero)
        {
            this.TextFunc = textFunc;
        }
        //public Label(IDetails obj):this(obj.Label)
        //{
        //    this.Active = true;
        //    this.LeftClickAction = () => throw new NotImplementedException();
        //}
        public Label(string text) : this(Vector2.Zero, text) { }
        public Label(object obj) : this(Vector2.Zero, obj?.ToString() ?? "")
        {
            if (obj is not null)
            {
                this.HoverText = this.Text;
                if (obj is Inspectable objdetails)
                {
                    this.Active = true;
                    this.LeftClickAction = () => Inspector.Refresh(objdetails);
                    this.Text = $"[{this.Text}]";
                    //this.Text = $"[{objdetails.Label}]";
                    this.TextColor = Color.LightBlue;
                }
            }
        }
        public Label(object obj, Action action) : this(Vector2.Zero, obj.ToString())
        {
            this.Active = true;
            this.LeftClickAction = action;
        }
        public Label(Func<string> textGetter, Action action) : this(textGetter)
        {
            this.Active = true;
            this.LeftClickAction = action;
        }
        public Label(Vector2 location, string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base(location)
        {
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            this.Text = text;
            this.Font = font ?? UIManager.Font;
            this.Active = false;
        }
        public Label(string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base()
        {
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            this.Text = text;
            this.Font = font ?? UIManager.Font;
            this.Active = false;
        }
        public Label(Vector2 location, string text, HorizontalAlignment halign)
            : base(location)
        {
            this.Halign = halign;
            this.Text = text;
        }
        public Label(Vector2 location, string text, HorizontalAlignment halign, VerticalAlignment valign)
            : base(location)
        {
            this.Halign = halign;
            this.Valign = valign;
            this.Text = text;
            switch (halign)
            {
                case HorizontalAlignment.Left:
                    this.Origin.X = 0;
                    break;
                case HorizontalAlignment.Center:
                    this.Origin.X = this.Width / 2;
                    break;
                case HorizontalAlignment.Right:
                    this.Origin.X = this.Width;
                    break;
                default:
                    break;
            }
            switch (valign)
            {
                case VerticalAlignment.Top:
                    this.Origin.Y = 0;
                    break;
                case VerticalAlignment.Center:
                    this.Origin.Y = this.Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    this.Origin.Y = this.Height;
                    break;
                default:
                    break;
            }
            this.Location = this.Location - this.Origin;
        }

        public Label(string text, string format)
            : this(text)
        {
            this.Text = text;
            this.TextFormat = format ?? text;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            this.DrawText(sb, this.ScreenLocation, null, this.MouseHover && this.Active ? Color.Lime : Color.White, this.Opacity); //Color.White
        }
        public static void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, int width, Color color, float opacity, HorizontalAlignment hAlign)
        {
            var c = color * opacity;
            // TODO: fix the origin so it's not always at the center
            Vector2 offset = new(1, 1), origin = Vector2.Zero;
            switch (hAlign)
            {
                case HorizontalAlignment.Left:
                    offset = new Vector2(1, 1);
                    origin = Vector2.Zero;
                    break;
                case HorizontalAlignment.Center:
                    offset = new Vector2((float)Math.Floor(width * 0.5), 1);
                    origin = new Vector2(textSprite.Width / 2, 0);
                    break;
                case HorizontalAlignment.Right:
                    offset = new Vector2(width - 2, 1);
                    origin = new Vector2(textSprite.Width, 0);
                    break;
                default:
                    break;
            }

            sb.Draw(textSprite, position + offset, null, c, 0, origin, 1, SpriteEffects.None, 0);
        }
        public static void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, Rectangle? sourceRect, int width, Color color, float opacity, HorizontalAlignment hAlign)
        {
            var c = color * opacity;

            if (!sourceRect.HasValue)
                sourceRect = textSprite.Bounds;
            // TODO: fix the origin so it's not always at the center

            Vector2 offset = new(1, 1), origin = Vector2.Zero;
            switch (hAlign)
            {
                case HorizontalAlignment.Left:
                    offset = new Vector2(1, 1);
                    origin = Vector2.Zero;
                    break;
                case HorizontalAlignment.Center:
                    offset = new Vector2((float)Math.Floor(width * 0.5), 1);
                    origin = new Vector2(textSprite.Width / 2, 0);
                    break;
                case HorizontalAlignment.Right:
                    offset = new Vector2(width - 2, 1);
                    origin = new Vector2(textSprite.Width, 0);
                    break;
                default:
                    break;
            }

            sb.Draw(textSprite, position + offset, Rectangle.Intersect(textSprite.Bounds, sourceRect.Value), c, 0, origin, 1, SpriteEffects.None, 0);
        }
        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (this.Active) 
                this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            if (this.Active) 
                this.Invalidate();
        }

        public HorizontalAlignment TextHAlign { get; set; }

        public override Control SetLeftClickAction(Action<ButtonBase> action)
        {
            this.Active = true;
            return base.SetLeftClickAction(action);
        }
        public static IEnumerable<Label> Parse(string text)
        {
            var array = text.Split(' ');
            for (int i = 0; i < array.Length; i++)
            {
                var txt = array[i];
                if (txt.First() == '[' && txt.Last() == ']')
                {
                    var token = txt.Substring(1, txt.Length - 2).Split(',');
                    var inner = token[0];
                    var lbl = new Label($"{inner}");
                    if (inner.Length > 1)
                        if (token[1].TryParseColor(out var col))
                            lbl.TextColor = col;
                    yield return lbl;
                }
                else
                    yield return new Label(txt);
            }
        }
        static Label ParseToken(string txt)
        {
            var token = txt.Split(',');
            var inner = token[0];
            var lbl = new Label($"{inner}") { TextColor = Color.Gold, Font = UIManager.FontBold };
            if (token.Length > 1)
            {
                if (token[1].TryParseColor(out var col))
                    lbl.TextColor = col;
            }
            return lbl;
        }
        public static IEnumerable<Label> ParseNew(string text)
        {
            var posCurrent = 0;
            int posFrom = 0;
            do
            {
                posFrom = text.IndexOf('[', posCurrent);
                if (posFrom != -1)
                {
                    var plainText = text.Substring(posCurrent, posFrom - posCurrent);
                    
                    var f = plainText.Split(' ');
                    foreach (var i in f)
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                    var posTo = text.IndexOf(']', posFrom + 1);
                    if (posTo != -1)
                    {
                        var token = text.Substring(posFrom + 1, posTo - posFrom - 1);
                        posCurrent = posTo + 1;
                        yield return ParseToken(token);
                    }
                }
                else
                {
                    var plainText = text.Substring(posCurrent, text.Length - posCurrent);
                    foreach (var i in plainText.Split(' '))
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                }
            } while (posFrom != -1);
        }

        internal static IEnumerable<Label> ParseNewNew(object value)
        {
            if (value is string str)
                return ParseNew(str);
            else if (value is IEnumerable<string> strEnum)
                return strEnum.SelectMany(s => ParseNew(s));
            else if (value is IEnumerable<object> objEnum)
                return objEnum.Select(o => new Label(o));
            else
                return new Label[] { new Label(value) };
            
            //if (value is string str)
            //    return new GroupBox().AddControlsHorizontally(ParseNew(str).ToArray());
            //else if (value is IEnumerable<string> strEnum)
            //    return new GroupBox().AddControlsHorizontally(strEnum.SelectMany(s => ParseNew(s)).ToArray());
            //else if (value is IEnumerable<object> objEnum)
            //    return new GroupBox().AddControlsHorizontally(objEnum.Select(o => new Label(o)).ToArray());
            //else
            //    return new Label(value);
        }
    }
}
