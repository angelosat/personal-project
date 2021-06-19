using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class SlotWithText : ButtonBase
    {
        public Slot Slot;
        public Label SlotText;
        //Label Name;
      //  Label Label;

        static public new Texture2D DefaultSprite;
        public override void Initialize()
        {
            DefaultSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-sample-narrow");
        }

        //public override ButtonBase SetText(string text)
        //{
        //    this.Text = text;
        //    TextSprite = UIManager.DrawTextOutlined(Text, Fill, Outline, FontStyle);
        //}


       

        public SlotWithText():this(Vector2.Zero)

        {
           
        }

        public SlotWithText(Vector2 location)
            : base(location)
        {

            this.Size = new Rectangle(0, 0, 50, Slot.DefaultHeight);
            AutoSize = true;
 
            Slot = new Slot();
            SlotText = new Label();

            Tag = GameObjectSlot.Empty;

            Controls.Add(Slot);
        }

        public virtual SlotWithText SetSlotText(string text = "")
        {
          //  Slot.SetBottomRightText(text);
         //   Slot.Text = text;
            SlotText = new Label(Slot.BottomRight, text);
            SlotText.Location -= new Vector2(SlotText.Width, SlotText.Height);
           // slotText.Anchor = slotText.BottomRight;
            SlotText.MouseThrough = true;
            Controls.Add(SlotText);
            return this;
        }


        //public new SlotWithText SetTag(GameObjectSlot tag)
       // public override Control SetTag(object tag)

        public override object Tag
        {
            get { return base.Tag; }
            set
            {
                //this.Tag = tag;
                //Slot.Tag = new GameObjectSlot(tag as GameObject);
                //SetText(Slot.Tag.HasValue ? Slot.Tag.Object.Name : "");
                base.Tag = value;
                Slot.Tag = value as GameObjectSlot;
                SetText(Slot.Tag.HasValue ? Slot.Tag.Object.Name : "");
                //Label Name = new Label(Slot.TopRight, Text);
                ////   Controls.Add(Name);
                //Controls.Clear();
                //Controls.Add(Slot, SlotText ?? "<empty>".ToLabel(), Name);
                //Name.MouseThrough = true;
            }
        }

        public ButtonBase SetTag(GameObjectSlot tag)
        {
            this.Tag = tag;
            return this;
        }

        public override ButtonBase SetText(string text)
        {
            base.SetText(text);
            Label Name = new Label(Slot.TopRight, Text);
            //   Controls.Add(Name);
            Controls.Clear();
            Controls.Add(Slot, SlotText ?? "<empty>".ToLabel(), Name);
            Name.MouseThrough = true;
            return this;
        }

        //public override GameObjectSlot Tag
        //{
        //    get
        //    {
        //        return base.Tag;
        //    }
        //    set
        //    {
        //        //base.Tag = value;// value;
        //        //Slot.Tag = base.Tag; // new GameObjectSlot(value as GameObject);// value as GameObjectSlot;
        //        //SetText(base.Tag.HasValue ? base.Tag.Object.Name : "");
        //        SetTag(value);
        //    }
        //}

        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            Color c = Color.Lerp(Color.Transparent, Color.White, opacity);
           // sb.Draw(DefaultSprite, destRect, sourceRect, c);
            // TODO: optimize
            if (Controller.Instance.Mouseover != null)
                if (Controller.Instance.Mouseover.Object == this)
                DrawHighlight(sb);
           // Slot.Draw(sb);
           // Slot.DrawSprite(sb, destRect, sourceRect, new Color(color.R, color.G, color.B, Slot.Alpha.A), 1);
            Slot.DrawSprite(sb, destRect, sourceRect, color*(Slot.MouseHover ? 1 : 0.5f), 1);
    //        Slot.DrawSprite(sb, destRect, sourceRect, color, Slot.Alpha.A);
        }

        /// <summary>
        /// same as button.drawtext()
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="position"></param>
        /// <param name="sourceRect"></param>
        /// <param name="color"></param>
        /// <param name="opacity"></param>
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            //if (TextSprite == null)
            //    return;
            //if (!sourceRect.HasValue)
            //    sourceRect = TextSprite.Bounds;
            //sb.Draw(TextSprite, position + Slot.TopRight, Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), color * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, Depth);

        }

    }
}
