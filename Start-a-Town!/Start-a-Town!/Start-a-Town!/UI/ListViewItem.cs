using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class ListViewItem : IconButton//ButtonBase //Control// :
    {
        public Vector2 Position;
        public int IconIndex = -1;
        public int Index;
        public ListView ListView { get; set; }
        public ListViewGroup Group { get; set; }
        //public Icon Icon;
        //public bool Focused { get; set; }
        //public Vector2 Position { get; set; }
        //public string Name;
        
        public ListViewItem() { }
        public ListViewItem(string name)
        {
            Name = name;
        }
        public override void Validate()
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;

            RenderTarget2D bg = new RenderTarget2D(gfx, Width, Height);

            gfx.SetRenderTarget(bg);
            gfx.Clear(Color.Transparent);
            sb.Begin();
            //BorderStyleRegions.LargeButton.Draw(sb, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, Width, Height), 0.5f);
            BackgroundStyle.LargeButton.Draw(sb, Size, 0.5f);
            sb.End();
            gfx.SetRenderTarget(null);
            BackgroundTexture = bg;
            base.Validate();
        }

        public override void GetTooltipInfo(Tooltip tooltip) //List<GroupBox> TooltipGroups
        {
            //get
            //{
            //    if (Tag is ITooltippable)
            //        return (Tag as ITooltippable).TooltipGroups;
            //    return null;
            //}
        }

        public override Rectangle ScreenBounds
        {
            get
            {
                return new Rectangle((int)(ListView.ScreenLocation.X + Position.X), (int)(ListView.ScreenLocation.Y + Position.Y), ListView.ItemBackground.Width, ListView.ItemBackground.Height);

                //switch (ListView.View)
                //{
                //    case View.IconOnly:
                //        return new Rectangle((int)Position.X, (int)Position.Y, UIManager.LargeButton.Height, UIManager.LargeButton.Height);
                //    default:
                //        break;
                //}
                //return new Rectangle((int)Position.X, (int)Position.Y, Icon.SourceRect.Width, Icon.SourceRect.Height);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            if (IsTopMost)
                sb.Draw(BackgroundTexture, ScreenLocation, Color);
                                //BorderStyleRegions.LargeButton.Draw(sb, new Rectangle((int)(ScreenLocation.X + Bounds.X), (int)(ScreenLocation.Y + Bounds.Y), Bounds.Width, Bounds.Height), 0.5f);
            Icon.Draw(sb, ScreenLocation + new Vector2((Width - Icon.SourceRect.Width) / 2, (Height - Icon.SourceRect.Height) / 2));

            base.Draw(sb);
        }

        //public override List<GroupBox> TooltipGroups
        //{
        //    get
        //    {
        //        if (Tag != null)
        //            if (Tag is ITooltippable)
        //                return ((ITooltippable)Tag).TooltipGroups;
        //        return null;
        //    }
        //}
    }
}
