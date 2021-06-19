using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Interactions;

namespace Start_a_Town_.UI
{
    class BuffSlot : Slot
    {
        Label BuffDuration;
        BuffBase Buff;

        public BuffSlot(Vector2 location)
            : base(location) 
        {
            //BuffDuration = new Label(this, new Vector2(Bottom, Width / 2), "");
            BuffDuration = new Label(location + new Vector2(Width / 2, Bottom - 5), "", TextAlignment.Center, TextAlignment.Top);
            //Controls = new List<Control>();
            Controls.Add(BuffDuration);
        }

        public override void Update()
        {
            base.Update();

            if (Tag != null)
            {
                Buff = Tag as BuffBase;
                BuffDuration.Text = ((int)((Buff.Duration - Buff.Remaining) / 60)).ToString() + " s";
            }
            
            BuffDuration.Update();
        }

        //public override void Draw(SpriteBatch sb)
        //{
        //    sb.Draw(UIManager.SlotSprite, ScreenLocation, null, Alpha, 0, new Vector2(0), 1, SprFx, Depth);
        //    if (Tag != null)
        //        {
        //            //Item.IconIndex.Draw(sb, ScreenLocation + new Vector2(3));
        //            sb.Draw(ItemManager.ItemSheet, ScreenLocation + new Vector2(3), ItemManager.Icons[Tag.IconIndex], Color.White);
        //            BuffDuration.Draw(sb);

        //        }
        //}
    }
}
