using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class DebugWindow : Window
    {
        static DebugWindow _Instance;
        public static DebugWindow Instance => _Instance ??= new DebugWindow();

        Panel Panel_Tabs, Panel_Buttons, Panel_Mouseover;
        Panel Panel_Info;
        Panel Panel_PlayerInfo;
        Label Label_Map, Label_Perfomance, Label_Mouseover;
        Label Label_Player_Task;

        public DebugWindow()
        {
            Title = "Debug";
            AutoSize = true;
            Movable = true;

            Panel_Tabs = new Panel();
            Panel_Tabs.AutoSize = true;
            RadioButton RB_mouseover = new RadioButton("Mouseover", Vector2.Zero);
            RadioButton RB_map = new RadioButton("Map", new Vector2(RB_mouseover.Right, 0));
            RadioButton RB_player = new RadioButton("Player", new Vector2(RB_map.Right, 0));
            RB_mouseover.Checked = true;

            RB_mouseover.CheckedChanged+=new EventHandler<EventArgs>(RB_mouseover_CheckedChanged);
            RB_map.CheckedChanged += new EventHandler<EventArgs>(RB_map_CheckedChanged);
            RB_player.CheckedChanged += new EventHandler<EventArgs>(RB_player_CheckedChanged);
            Panel_Tabs.Controls.Add(RB_map, RB_player, RB_mouseover);
           
            Panel_Mouseover = new Panel(new Vector2(0, Panel_Tabs.Bottom));
            Panel_Mouseover.Height = 300;
            Panel_Mouseover.Width = Panel_Tabs.Width;
            Panel_Info = new Panel(new Vector2(0, Panel_Tabs.Bottom));
            Panel_Info.AutoSize = false;
            Panel_Info.Height = 300;
            Panel_Info.Width = Panel_Tabs.Width;
            Label_Perfomance = new Label("Fps: \nDelta Time:");

            Label_Map = new Label(new Vector2(0, Label_Perfomance.Bottom)) { AutoSize = true };
            Label_Mouseover = new Label(Panel_Info.Width);
            Panel_Info.Controls.Add(Label_Map, Label_Perfomance);
            Panel_Mouseover.Controls.Add(Label_Mouseover);

            Panel_PlayerInfo = new Panel(Panel_Info.Location, new Vector2(Panel_Tabs.Width, Panel_Info.Height));
            Panel_PlayerInfo.AutoSize = false;
            Label_Player_Task = new Label(Panel_Info.Width);
            Panel_PlayerInfo.Controls.Add(Label_Player_Task);

            Location = new Vector2(0, Hud.DefaultHeight);

            Panel_Buttons = new Panel(new Vector2(0, Panel_Info.Bottom));
            Panel_Buttons.AutoSize = true;
            Button chunkshot = new Button("Render current chunk to file");
            Panel_Buttons.Controls.Add(chunkshot);
            Client.Controls.Add(Panel_Tabs, Panel_Mouseover);
            chunkshot.LeftClick += new UIEvent(chunkshot_Click);
        }

        void chunkshot_Click(object sender, EventArgs e)
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            using RenderTarget2D screenshot = new RenderTarget2D(gfx, Block.Width * Chunk.Size, Map.MaxHeight * Block.Height);
            gfx.SetRenderTarget(screenshot);
            gfx.Clear(Color.Transparent);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            sb.End();
            gfx.SetRenderTarget(null);

            string directory = Directory.GetCurrentDirectory() + @"/Screenshots/";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string filename = @"Start-a-town!" + DateTime.Now.ToString("ddMMyy_HHmmss") + @".png";
            FileStream stream = new FileStream(directory + filename, System.IO.FileMode.OpenOrCreate);
            screenshot.SaveAsPng(stream, screenshot.Width, screenshot.Height);
            NotificationArea.Write("Screenshot saved as \"" + filename + "\".");
            stream.Close();
        }

        void RB_player_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Controls.Add(Panel_PlayerInfo);
            else
                Controls.Remove(Panel_PlayerInfo);
        }
        void RB_map_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Controls.Add(Panel_Info);
            else
                Controls.Remove(Panel_Info);
        }

        void RB_mouseover_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Controls.Add(Panel_Mouseover);
            else
                Controls.Remove(Panel_Mouseover);
        }

        public override void Update()
        {
            Label_Map.Text =
                "ChunkLoader: " + ChunkLoader.Status + 
                "\nChunks in memory: " + ChunkLoader.Count +
                "\nActive Chunks: " + Engine.Map.GetActiveChunks().Count;

            Label_Perfomance.Text =
                "Fps: " + GlobalVars.Fps +
                "\nDelta Time: " + GlobalVars.DeltaTime;

            base.Update();
        }

        public override bool Close()
        {
            return base.Close();
        }
    }
}
