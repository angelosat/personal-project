using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Label : ButtonBase //control
    {
        //static public int DefaultHeight = UIManager.LineHeight + 2;
        static public int DefaultHeight = UIManager.Font.LineSpacing + 2;

        HorizontalAlignment _Halign;
        public HorizontalAlignment Halign
        {
            get { return _Halign; }
            set
            {
                //if(value != _Halign)
                //    this.Invalidate();

                _Halign = value;
                //switch (value)
                //{
                //    case HorizontalAlignment.Left:
                //        Origin.X = 0;
                //        break;
                //    case HorizontalAlignment.Center:
                //        Origin.X = Width / 2;
                //        break;
                //    case HorizontalAlignment.Right:
                //        Origin.X = Width;
                //        break;
                //    default:
                //        break;
                //}
            }
        }
        public VerticalAlignment Valign;

        public Color TextBackground = Color.Transparent;
        public Func<Color> TextBackgroundFunc = () => Color.Transparent;

        public override string ToString()
        {
            // return base.ToString() + " " + Text;
            return "Label: " + this.Text ?? this.Name;
        }

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;

                if (value > 0)
                    this.Invalidate();
            }
        }

        public override void OnBeforeDraw(SpriteBatch sb, Rectangle viewport)
        {
            //this.BoundsScreen.DrawHighlight(sb, this.TextBackgroundFunc());

        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            //this.BoundsScreen.DrawHighlight(sb, 1f);
            var c = this.TextBackgroundFunc();
            this.BoundsScreen.DrawHighlight(sb, c);// Color.Red * .5f);// * .5f);// .5f);
            base.Draw(sb, viewport);
        }

        public Color ActiveColor = Color.Lime;
        public override void OnPaint(SpriteBatch sb)
        {
            //UIManager.DrawStringOutlined(
            //    sb,
            //    this.Text,
            //    //new Vector2(1, 1) + new Vector2(Anchor.X, 0) * Dimensions + ((Active && Pressed) ? Vector2.UnitY : Vector2.Zero),
            //    Vector2.One + ((Active && Pressed) ? Vector2.UnitY : Vector2.Zero),
            //    Dimensions,
            //    (MouseHover && Active) ? ActiveColor : TextColor,
            //    TextOutline,
            //    Font,
            //    this.TextHAlign);
            //return;

            var pos = new Vector2((int)this.Halign * .5f, .5f);
            var outlineOffset = this.Halign == HorizontalAlignment.Left ? 1 : (this.Halign == HorizontalAlignment.Right ? -1 : 0);
            UIManager.DrawStringOutlined(
                sb,
                this.Text,
                //new Vector2(1, 1) + new Vector2(Anchor.X, 0) * Dimensions + ((Active && Pressed) ? Vector2.UnitY : Vector2.Zero),
                pos * Dimensions + new Vector2(outlineOffset, 0) + ((Active && Pressed) ? Vector2.UnitY : Vector2.Zero),
                pos,
                (MouseHover && Active) ? ActiveColor : TextColor,
                TextOutline,
                Font);
            //sb.DrawString(Font, this.Text, Vector2.Zero, (MouseHover && Active) ? ActiveColor : TextColor);
            return;
        }

        public event UIEvent TextChanged;
        protected override void OnTextChanged()
        {

            base.OnTextChanged();
            this.Height = Math.Max(this.Height, Label.DefaultHeight);

            switch (Halign)
            {
                case HorizontalAlignment.Left:
                    Origin.X = 0;
                    break;
                case HorizontalAlignment.Center:
                    Origin.X = Width / 2;
                    break;
                case HorizontalAlignment.Right:
                    Origin.X = Width;
                    break;
                default:
                    break;
            }
            switch (Valign)
            {
                case VerticalAlignment.Top:
                    Origin.Y = 0;
                    break;
                case VerticalAlignment.Center:
                    Origin.Y = Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    Origin.Y = Height;
                    break;
                default:
                    break;
            }

            if (TextChanged != null)
                TextChanged(this, new UIEventArgs(this));
        }
        public Label(int width) : base()
        {
            Text = "";
            this.Width = width;
            this.Height = Label.DefaultHeight;
            Active = false;
        }
        public Label() : this(Vector2.Zero, "") { }
        public Label(Func<string> textFunc) : this(Vector2.Zero)
        {
            this.TextFunc = textFunc;
        }

        public Label(string text) : this(Vector2.Zero, text) { }
        public Label(object obj) : this(Vector2.Zero, obj.ToString()) { }
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
            Text = text;
            this.Font = font ?? UIManager.Font;
            Active = false;
        }
        public Label(string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base()
        {
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            Text = text;
            this.Font = font ?? UIManager.Font;
            Active = false;
        }
        public Label(Vector2 location, string text, HorizontalAlignment halign)
            : base(location)
        {
            this.Halign = halign;
            Text = text;
        }
        public Label(Vector2 location, string text, HorizontalAlignment halign, VerticalAlignment valign)
            : base(location)
        {
            Halign = halign;
            Valign = valign;
            Text = text;
            switch (halign)
            {
                case HorizontalAlignment.Left:
                    Origin.X = 0;
                    break;
                case HorizontalAlignment.Center:
                    Origin.X = Width / 2;
                    break;
                case HorizontalAlignment.Right:
                    Origin.X = Width;
                    break;
                default:
                    break;
            }
            switch (valign)
            {
                case VerticalAlignment.Top:
                    Origin.Y = 0;
                    break;
                case VerticalAlignment.Center:
                    Origin.Y = Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    Origin.Y = Height;
                    break;
                default:
                    break;
            }
            Location = Location - Origin;
        }

        public Label(string text, string format)
            : this(text)
        {
            this.Text = text;
            this.TextFormat = format ?? text;
            //this.Dimensions = UIManager.Font.MeasureString(TextFormat) + new Vector2(2); //i don't need this, dimensions are set in buttonbase.onpaint
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            DrawText(sb, this.ScreenLocation, null, MouseHover && Active ? Color.Lime : Color.White, Opacity); //Color.White
        }
        static public void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, int width, Color color, float opacity, HorizontalAlignment hAlign)
        {
            Color c = color * opacity;
            // TODO: fix the origin so it's not always at the center
            Vector2 offset = new Vector2(1, 1), origin = Vector2.Zero;
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

            //sb.Draw(TextSprite, position +  new Vector2((Width - TextSprite.Width) / 2, (Height - TextSprite.Height) / 2 + ((Pressed && Active) ? 1 : 0)), Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), c, 0, Vector2.Zero, 1, SpriteEffects.None, Depth);
            sb.Draw(textSprite, position + offset, null, c, 0, origin, 1, SpriteEffects.None, 0);
        }
        static public void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, Rectangle? sourceRect, int width, Color color, float opacity, HorizontalAlignment hAlign)
        {
            Color c = color * opacity;

            if (!sourceRect.HasValue)
                sourceRect = textSprite.Bounds;
            // TODO: fix the origin so it's not always at the center

            Vector2 offset = new Vector2(1, 1), origin = Vector2.Zero;
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

            //sb.Draw(TextSprite, position +  new Vector2((Width - TextSprite.Width) / 2, (Height - TextSprite.Height) / 2 + ((Pressed && Active) ? 1 : 0)), Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), c, 0, Vector2.Zero, 1, SpriteEffects.None, Depth);
            sb.Draw(textSprite, position + offset, Rectangle.Intersect(textSprite.Bounds, sourceRect.Value), c, 0, origin, 1, SpriteEffects.None, 0);
        }
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            //Color c = color * opacity;

            //if (!sourceRect.HasValue)
            //    sourceRect = TextSprite.Bounds;
            //// TODO: fix the origin so it's not always at the center

            //Vector2 offset = new Vector2(1, 1), origin = Vector2.Zero;
            //switch (Halign)
            //{
            //    case HorizontalAlignment.Left:
            //        offset = new Vector2(1, 1);
            //        origin = Vector2.Zero;
            //        break;
            //    case HorizontalAlignment.Center:
            //        offset = new Vector2((float)Math.Floor(Width * 0.5), 1);
            //        origin = new Vector2(TextSprite.Width / 2, 0);
            //        break;
            //    case HorizontalAlignment.Right:
            //        offset = new Vector2(Width - 2, 1);
            //        origin = new Vector2(TextSprite.Width, 0);
            //        break;
            //    default:
            //        break;
            //}

            //sb.Draw(TextSprite, position + offset, Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), c, 0, origin, 1, SpriteEffects.None, Depth);

        }
        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (Active) this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            if (Active) this.Invalidate();
        }

        public HorizontalAlignment TextHAlign { get; set; }

        public override Control SetLeftClickAction(Action<ButtonBase> action)
        {
            this.Active = true;
            return base.SetLeftClickAction(action);
        }
        static public IEnumerable<Label> Parse(string text)
        {
            var array = text.Split(' ');
            for (int i = 0; i < array.Length; i++)
            {
                var txt = array[i];
                if (txt.First() == '[' && txt.Last() == ']')
                {
                    var token = txt.Substring(1, txt.Length - 2).Split(',');
                    var inner = token[0];
                    //var lbl = new Label(int.TryParse(inner, out var refID) ? $"{inner}" : txt);
                    var lbl = new Label($"{inner}");

                    if (inner.Length > 1)
                    {
                        //var colArray = token[1].Split(':');
                        //lbl.TextColor = coloa new Color(int.Parse(colArray[0]), int.Parse(colArray[1]), int.Parse(colArray[2]));
                        if (token[1].TryParseColor(out var col))
                            lbl.TextColor = col;
                    }
                    //if (int.TryParse(inner, out var refID))
                    //{
                    //    var item = net.GetNetworkObject(refID);
                    //    yield return new Label(item.Name);
                    //}
                    //else
                    //    yield return new Label(txt);
                    yield return lbl;
                }
                else
                    yield return new Label(txt);
            }
        }
        static public Label ParseToken(string txt)
        {
            var token = txt.Split(',');
            var inner = token[0];
            var lbl = new Label($"{inner}") { TextColor = Color.Gold };
            if (token.Length > 1)
            {
                if (token[1].TryParseColor(out var col))
                    lbl.TextColor = col;
            }
            return lbl;
        }
        static public IEnumerable<Label> ParseNew(string text)
        {
            var posCurrent = 0;
            int posFrom = 0;
            do
            {
                posFrom = text.IndexOf('[', posCurrent);
                if (posFrom != -1)
                {
                    var plainText = text.Substring(posCurrent, posFrom - posCurrent);
                    //if (!plainText.IsNullEmptyOrWhiteSpace())
                    //{
                    //yield return new Label(plainText);

                    var f = plainText.Split(' ');
                    foreach (var i in f)
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                    //}
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
                    //yield return new Label(plainText);
                    foreach (var i in plainText.Split(' '))
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                }
            } while (posFrom != -1);
        }
        static public IEnumerable<Label> ParseOld(INetwork net, string text)
        {
            var array = text.Split(' ');
            for (int i = 0; i < array.Length; i++)
            {
                var txt = array[i];
                if (txt.First() == '[' && txt.Last() == ']')
                {
                    var token = txt.Substring(1, txt.Length - 2).Split(',');
                    var inner = token[0];
                    var lbl = new Label(int.TryParse(inner, out var refID) ? $"{net.GetNetworkObject(refID).Name}:{refID}" : txt);
                    //if (inner.Length > 1)
                    //{
                    //    var colArray = token[1].Split(',');
                    //    lbl.TextColor = new Color(int.Parse(colArray[0]), int.Parse(colArray[1]), int.Parse(colArray[2]));
                    //    Color.White.ToString();
                    //}
                    //if (int.TryParse(inner, out var refID))
                    //{
                    //    var item = net.GetNetworkObject(refID);
                    //    yield return new Label(item.Name);
                    //}
                    //else
                    //    yield return new Label(txt);
                    yield return lbl;
                }
                else
                    yield return new Label(txt);
            }
        }
    }
}
