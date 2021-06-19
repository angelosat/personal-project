using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.Components;

using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI
{
    class LandscapingWindow : Window
    {
        #region Singleton
        static LandscapingWindow _Instance;
        public static LandscapingWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new LandscapingWindow();
                return _Instance;
            }
        }
        #endregion

        Panel Panel_Slots;

        //List<Slot> Slots;
        Panel Panel_Variations;
      //  Vector2 Variation;

        LandscapingWindow()
        {
            Title = "Landscaping";
            Movable = true;
            AutoSize = true;

            Panel_Slots = new Panel();
            Panel_Slots.AutoSize = true;
            int i = 0, j=0, n =0;
            //foreach (Tile.Types type in TileComponent.TileMapping.Keys)
            foreach (var block in BlockComponent.Blocks.Values)
            {
                Slot slot = new Slot(new Vector2(i* Slot.DefaultHeight, j* Slot.DefaultHeight));
                slot.Tag = block.Entity.ToSlot();
                slot.LeftClick += new UIEvent(slot_Click);
                n++;
                i = n % 8;
                j = n / 8;
                Panel_Slots.Controls.Add(slot);
            }

            Panel_Variations = new Panel(new Vector2(Panel_Slots.Width, 0));
            Panel_Variations.AutoSize = true;

            this.Controls.Add(Panel_Slots);
            Location = BottomLeftScreen;
        }

        void slot_Click(object sender, EventArgs e)
        {
            GameObjectSlot objSlot = (sender as Slot).Tag as GameObjectSlot;
            GameObject obj = objSlot.Object;
            DesignTool.Brush = new DesignObject((Block.Types)obj["Physics"]["Type"]);
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = DesignTool.Instance;
            //InitVariationPanel(Block.TileSprites[DesignTool.Brush.Type]);
        }


        void InitVariationPanel(Sprite sprite)
        {
            this.Controls.Remove(Panel_Variations);
            //Variation = Vector2.Zero;
            int k = 0, n = 0;
            foreach (PictureBox picBox in Panel_Variations.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;
                picBox.MouseLeftPress -= variation_MouseLeftPress;
            }
            Panel_Variations.Controls.Clear();
            foreach (Rectangle[] strip in sprite.SourceRects)
            {
                foreach (Rectangle rect in strip)
                {
                    PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, HorizontalAlignment.Left);
                    variation.Tag = new Vector2(k, n);
                    variation.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(variation_MouseLeftPress);
                    variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
                    variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
                    Panel_Variations.Controls.Add(variation);
                    k += 1;
                }
                n += 1;
                k = 0;
            }

            Controls.Add(Panel_Variations);

            //Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.LastScreenHeight);
            Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - UIManager.Height);

        }

        void variation_DrawItem(object sender, DrawItemEventArgs e)
        {
            PictureBox box = sender as PictureBox;
            if ((Vector2)box.Tag == new Vector2(DesignTool.Brush.Orientation, DesignTool.Brush.Variation))//  Variation)
                box.DrawHighlight(e.SpriteBatch, 0.5f);
            e.SpriteBatch.Draw(box.Texture, box.ScreenLocation, box.SourceRect, Color.White, 0, box.PictureOrigin, 1, SpriteEffects.None, 0);
        }

        void variation_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Vector2 v = (Vector2)(sender as PictureBox).Tag;
            //DesignTool.Brush = new DesignObject((Tile.Types)obj["Physics"]["Type"], );
            DesignTool.Brush.Variation = (int)v.Y;
            DesignTool.Brush.Orientation = (int)v.X;
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = DesignTool.Instance;
        }
    }
}
