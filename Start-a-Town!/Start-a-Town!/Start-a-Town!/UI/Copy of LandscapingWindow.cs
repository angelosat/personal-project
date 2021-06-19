using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.Components;

using Start_a_Town_.Control;

namespace Start_a_Town_.UI
{
    class LandscapingWindow : Window
    {
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


        List<PictureBox> Tiles;

        public LandscapingWindow()  
            : base()
        {
            Title = "Landscaping";
            Movable = true;
            AutoSize = true;

            Tiles = new List<PictureBox>();
            int i = 0, j = 0, n = 0;
            foreach (Tile.Types type in Enum.GetValues(typeof(Tile.Types)))
            {
                if (type == Tile.Types.Air)
                    continue;
                string tileName = Enum.GetName(typeof(Tile.Types), type);
                TileSprite tileSprite;
                Rectangle sourceRect;// = TileBase.TileSprites[type].SourceRects[0,0];
                if (!Tile.TileSprites.TryGetValue(type, out tileSprite))
                    continue;
                sourceRect = tileSprite.SourceRects[0, 0];
                PictureBox box = new PictureBox(new Vector2(i * 32, j * 32), Map.TerrainSprites, sourceRect, HorizontalAlignment.Left);
                box.Tag = type;
                box.MouseLeftPress += new EventHandler<InputState>(box_MouseLeftPress);
                box.DrawItem += new EventHandler<DrawItemEventArgs>(box_DrawItem);
                box.DrawMode = UI.DrawMode.OwnerDrawVariable;
                i = n % 4;
                j = n / 4;
                n++;
                Controls.Add(box);
            }

            Tool = new PlanningTool();
        }

        void box_DrawItem(object sender, DrawItemEventArgs e)
        {
            PictureBox box = sender as PictureBox;
    

            //if ((Tile.Types)box.Tag == Tool.Type)
            //    box.DrawHighlight(e.SpriteBatch, 0.5f);

            e.SpriteBatch.Draw(box.Texture, box.ScreenLocation, box.SourceRect, Color.White, 0, box.PictureOrigin, 1, SpriteEffects.None, 0);
        }

        //TileBase.Types Type;
        PlanningTool Tool;
        void box_MouseLeftPress(object sender, InputState e)
        {
            PictureBox box = sender as PictureBox;
            Tile.Types selected = (Tile.Types)box.Tag;
            //if (Tool.Type == selected)
            //{
            //    Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            //    Tool.Type = 0;
            //    return;
            //}
            //Tool.Type = (Tile.Types)box.Tag;


            Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;// new LandscapingTool(selected);
        }

        public override bool Close()
        {
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.Close();
        }
    }
}
