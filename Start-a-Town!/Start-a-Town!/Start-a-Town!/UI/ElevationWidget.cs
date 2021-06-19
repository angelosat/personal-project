using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ElevationWidget : Control
    {
        Camera Camera;
        Button Up, Up5, Down, Down5, Fog, HideUnderground, HideOverground, BorderShading, HideWalls;
        CheckBox Chk_HideWalls, Chk_HideTerrain, Chk_BlockOutlines, Chk_Nameplates//;
            , Chk_CullDark, Chk_HideDark, Chk_Fog;
        public ElevationWidget()
        {
            AutoSize = true;
            this.Camera = Rooms.Ingame.Instance.Camera;
            //Up5 = new Button(new Vector2(0), label: "+16", width: 100);
            //Up = new Button(new Vector2(0, Up5.Bottom), label: "+1", width: 100);
            //Down = new Button(new Vector2(0, Up.Bottom), label: "-1", width: 100);
            //Down5 = new Button(new Vector2(0, Down.Bottom), label: "-16", width: 100);
          //  Fog = new Button(new Vector2(0, Down5.Bottom), label: "Toggle Fog", width: 100);

            //HideUnderground = new Button(new Vector2(0, Down5.Bottom), label: "Hide Underground", width: 100);
            //HideOverground = new Button(new Vector2(0, HideUnderground.Bottom), label: "Hide Overground", width: 100);
            //HideOverground.HoverText = "Hides tiles that are directly exposed to sunlight.";
            //HideWalls = new Button(new Vector2(0, HideOverground.Bottom), label: "Hide Walls", width: 100);
            //BorderShading = new Button(new Vector2(0, HideUnderground.Bottom), label: "Border Shading", width: 100);

            Chk_HideWalls = new CheckBox("Hide Walls", Vector2.Zero, Engine.HideWalls); //Down5.BottomLeft
            Chk_HideWalls.LeftClick += new UIEvent(HideWalls_Click);

            //Chk_HideTerrain = new CheckBox("Hide Terrain", Chk_HideWalls.BottomLeft, Engine.HideTerrain);
            //Chk_HideTerrain.LeftClick += new UIEvent(HideOverground_Click);
            Chk_HideTerrain = new CheckBox("Hide Terrain", Chk_HideWalls.BottomLeft, Rooms.Ingame.Instance.Camera.HideTerrainAbovePlayer) { LeftClickAction = ()=> ScreenManager.CurrentScreen.Camera.ToggleHideBlocksAbove() };
            //Chk_HideTerrain.LeftClick += new UIEvent(HideOverground_Click);


            Chk_BlockOutlines = new CheckBox("Block Outlines", Chk_HideTerrain.BottomLeft, Engine.BlockOutlines);
            Chk_BlockOutlines.LeftClick += new UIEvent(Chk_BlockOutlines_Click);
            Chk_Nameplates = new CheckBox("Nameplates", Nameplate.Enabled) { Location = Chk_BlockOutlines.BottomLeft, LeftClickAction = () => Nameplate.Enabled = !Nameplate.Enabled };
            Chk_CullDark = new CheckBox("Cull Dark", Engine.CullDarkFaces) { Location = Chk_Nameplates.BottomLeft, LeftClickAction = () => Engine.CullDarkFaces = !Engine.CullDarkFaces };
            Chk_HideDark = new CheckBox("Hide Dark", Engine.HideOccludedBlocks) { Location = Chk_CullDark.BottomLeft, LeftClickAction = () => Engine.HideOccludedBlocks = !Engine.HideOccludedBlocks };
            Chk_Fog = new CheckBox("Fog", Camera.Fog) { Location = Chk_HideTerrain.BottomLeft, LeftClickAction = () => Camera.Fog = !Camera.Fog };
           // Chk_Nameplates.LeftClick += new UIEvent(Chk_Nameplates_Click);

            //Up.LeftClick += new UIEvent(Up_Click);
            //Up5.LeftClick += new UIEvent(Up5_Click);
            //Down.LeftClick += new UIEvent(Down_Click);
            //Down5.LeftClick += new UIEvent(Down5_Click);
           // Fog.LeftClick += new UIEvent(Fog_Click);
            //HideUnderground.LeftClick += new UIEvent(HideUnderground_Click);
            //HideOverground.LeftClick += new UIEvent(HideOverground_Click);
            //BorderShading.LeftClick += new UIEvent(BorderShading_Click);
            //HideWalls.LeftClick += new UIEvent(HideWalls_Click);

            Controls.Add(Chk_HideWalls, Chk_HideTerrain,
                //Chk_BlockOutlines, Chk_Nameplates, Chk_CullDark, Chk_HideDark, 
                Chk_Fog);//Fog, , BorderShading);

            Location = new Vector2(UIManager.Width, 0);// - Width, 0);
            Anchor = new Vector2(1, 0);
        }

        //void Chk_Nameplates_Click(object sender, EventArgs e)
        //{
        //    Nameplate.Enabled = !Nameplate.Enabled;
        //}

        void Chk_BlockOutlines_Click(object sender, EventArgs e)
        {
            Engine.BlockOutlines = !Engine.BlockOutlines;
        }

        //void HideOverground_Click(object sender, EventArgs e)
        //{
        //    //ScreenManager.CurrentScreen.Camera.HideTerrainAbovePlayer = !ScreenManager.CurrentScreen.Camera.HideTerrainAbovePlayer;
        //    ScreenManager.CurrentScreen.Camera.ToggleHideBlocksAbove();
        //}

        void HideWalls_Click(object sender, EventArgs e)
        {
            Engine.HideWalls = !Engine.HideWalls;
        }

        void BorderShading_Click(object sender, EventArgs e)
        {
            //Engine.Map.BorderShading = !Engine.Map.BorderShading;
            this.Camera.BorderShading = !this.Camera.BorderShading;
        }

        void HideUnderground_Click(object sender, EventArgs e)
        {
            //Engine.Map.HideUnderground = !Engine.Map.HideUnderground;
            this.Camera.HideUnderground = !this.Camera.HideUnderground;
        }

        void Fog_Click(object sender, EventArgs e)
        {
            //Engine.Map.Fog = !Engine.Map.Fog;
            Camera.Fog = !Camera.Fog;
            //Game1.Instance.Effect.Parameters["HasFog"].SetValue(Map.Instance.Fog);
        }

        //void Down5_Click(object sender, EventArgs e)
        //{
        //    Engine.Map.DrawLevel -= 16;
        //    //foreach (KeyValuePair<Vector2, Chunk> pair in Map.Instance.ActiveChunks)
        //    Engine.Map.Redraw = true;
        //}

        //void Down_Click(object sender, EventArgs e)
        //{
        //    Engine.Map.DrawLevel--;
        //    //foreach (KeyValuePair<Vector2, Chunk> pair in Map.Instance.ActiveChunks)
        //    Engine.Map.Redraw = true;
        //}

        //void Up5_Click(object sender, EventArgs e)
        //{
        //    Engine.Map.DrawLevel += 16;
        //    //foreach (KeyValuePair<Vector2, Chunk> pair in Map.Instance.ActiveChunks)
        //    Engine.Map.Redraw = true;
        //}

        //void Up_Click(object sender, EventArgs e)
        //{
        //    Engine.Map.DrawLevel++;
        //    //foreach (KeyValuePair<Vector2, Chunk> pair in Map.Instance.ActiveChunks)
        //    Engine.Map.Redraw = true;
        //}

        public override void Reposition(Vector2 ratio)
        {
            base.Reposition(ratio);
        }

        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        //{
        //    base.Draw(sb, this.Bounds);
        //}
    }
}
