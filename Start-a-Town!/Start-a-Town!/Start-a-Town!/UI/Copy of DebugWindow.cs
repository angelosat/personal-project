using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Start_a_Town_.PathFinding;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class DebugWindow : Window
    {
        static DebugWindow _Instance;
        public static DebugWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DebugWindow();
                return _Instance;
            }
        }

        Panel Panel_Tabs, Panel_Buttons, Panel_Mouseover;
        Panel Panel_Info;
        Panel Panel_PlayerInfo;
        //Label Label_Chunks, Label_Tile
        Label Label_Map, Label_Perfomance, Label_Mouseover;
        //Label Label_Tiles, 
        //    Label_Objects;
        //Label Label_Fps;
        //Label Label_DeltaTime;
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
            //Panel_Mouseover.AutoSize = true;
            //Panel_Mouseover.ClientSize = new Rectangle(0, 0, 200, 350);
            Panel_Mouseover.Height = 300;
            Panel_Mouseover.Width = Panel_Tabs.Width;
            Panel_Info = new Panel(new Vector2(0, Panel_Tabs.Bottom));
            Panel_Info.AutoSize = false;
            Panel_Info.Height = 300;
            Panel_Info.Width = Panel_Tabs.Width;
            //Panel_Info.ClientSize = new Rectangle(0, 0, Panel_Tabs.Width, 350);
            Label_Perfomance = new Label("Fps: \nDelta Time:");

            //Label_Chunks = new Label(new Vector2(0, Label_Perfomance.Bottom), "Chunks drawn: " + Map.Instance.VisibleChunks.Count.ToString());
            //Label_Tiles = new Label(new Vector2(0, Label_Chunks.Bottom), "Tiles drawn: ");
            //Label_Objects = new Label(new Vector2(0, Label_Tiles.Bottom), "Objects drawn: ");
            //Label_Tile = new Label(new Vector2(0, Label_Objects.Bottom));
            //Panel_Info.Controls.AddRange(new Control[] { Label_Chunks, Label_Tiles, Label_Perfomance, Label_Objects, Label_Tile });
            Label_Map = new Label(new Vector2(0, Label_Perfomance.Bottom));
            Label_Mouseover = new Label(Panel_Info.Width);//new Vector2(0, Label_Map.Bottom));
            Panel_Info.Controls.AddRange(new Control[] { Label_Map, Label_Perfomance });
            Panel_Mouseover.Controls.Add(Label_Mouseover);

            Controls.AddRange(new Control[] { Panel_Tabs });

            //Panel_PlayerInfo = new Panel(Panel_Info.Location, new Vector2(Panel_Info.Width, Panel_Info.Height));
            Panel_PlayerInfo = new Panel(Panel_Info.Location, new Vector2(Panel_Tabs.Width, Panel_Info.Height));
            Panel_PlayerInfo.AutoSize = false;
            Label_Player_Task = new Label(Panel_Info.Width);
            Panel_PlayerInfo.Controls.Add(Label_Player_Task);

            //Map.Instance.VisibleChunksChanged += new EventHandler<EventArgs>(Instance_VisibleChunksChanged);
            //UI.FpsCounter.Updated += new EventHandler<EventArgs>(UIFpsCounter_Updated);
            //Player.Instance.CurrentTaskChanged += new EventHandler<EventArgs>(Instance_CurrentTaskChanged);
            Location = new Vector2(0, Hud.DefaultHeight);


            Panel_Buttons = new Panel(new Vector2(0, Panel_Info.Bottom));
            Panel_Buttons.AutoSize = true;
            Button chunkshot = new Button("Render current chunk to file");
            Panel_Buttons.Controls.Add(chunkshot);
            Client.Controls.Add(Panel_Mouseover);
            chunkshot.LeftClick += new UIEvent(chunkshot_Click);
        }

        

        void chunkshot_Click(object sender, EventArgs e)
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            using (RenderTarget2D screenshot = new RenderTarget2D(gfx, Tile.Width * Chunk.Size, Map.MaxHeight * Tile.Height))//Player.Instance.Chunk.Bounds.Height))
            {
                gfx.SetRenderTarget(screenshot);
                gfx.Clear(Color.Transparent);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                //Player.Actor.Chunk.Render(sb);
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

        //void Instance_CurrentTaskChanged(object sender, EventArgs e)
        //{
        //    if (Player.Actor.CurrentTask != null)
        //    {
        //        Label_Player_Task.Text = Player.Actor.CurrentTask.ToString();
        //        Player.Actor.CurrentTask.Updated += new EventHandler<EventArgs>(CurrentTask_Updated);
        //        Player.Actor.CurrentTask.Finished += new EventHandler<EventArgs>(CurrentTask_Finished);
        //    }
        //}

        //void CurrentTask_Finished(object sender, EventArgs e)
        //{
        //    Label_Player_Task.Text = "";
        //    Interactions.Task task = sender as Interactions.Task;
        //    task.Finished -= CurrentTask_Finished;
        //    task.Updated -= CurrentTask_Updated;
        //}

        //void CurrentTask_Updated(object sender, EventArgs e)
        //{
        //    Label_Player_Task.Text = Player.Actor.CurrentTask.ToString();
        //}

        //void UIFpsCounter_Updated(object sender, EventArgs e)
        //{
        //    Label_Fps.Text = "Fps: " + Global.Fps.ToString();
        //    Label_DeltaTime.Text = "Delta Time: " + Global.DeltaTime.ToString();
        //}

        public override void Update()
        {
            //Label_Tiles.Text = "Tiles drawn: " + Map.Instance.TilesDrawn.ToString();
            //Label_Objects.Text = "Objects drawn: " + Map.Instance.ObjectsDrawn.ToString();
            Label_Map.Text =
                "ChunkLoader: " + ChunkLoader.Status + //State.ToString() +
                "\nChunkLighter: " + ChunkLighter.Status + //.State.ToString() +
                "\nPathfinder: " + Pathfinding.Status + //.State.ToString() +
                "\nChunks in memory: " + ChunkLoader.Count +
                "\nActive Chunks: " + Engine.Map.ActiveChunks.Count +
                "\nChunks Drawn: " + Engine.Map.ChunksDrawn +
                "\nCulling checks: " + Engine.Map.CullingChecks +
                "\nTiles Drawn: " + Engine.Map.TilesDrawn +
                "\nTile Outlines Drawn: " + Engine.Map.TileOutlinesDrawn +
                "\nTotal Tiles Drawn: " + (Engine.Map.TilesDrawn + Engine.Map.TileOutlinesDrawn) +
                "\nObjects Drawn: " + Engine.Map.ObjectsDrawn.ToString();

            Label_Perfomance.Text =
                "Fps: " + GlobalVars.Fps +
                "\nDelta Time: " + GlobalVars.DeltaTime;

            //Label_Player_Task.Text = //Player.Actor.GetComponent<TasksComponent>("Tasks").ToString() +
            //    "\n" + Player.Actor.GetComponent<MovementComponent>("Position").ToString() +
            //    "\n" + Player.Actor.GetComponent<PhysicsComponent>("Physics").ToString();
                


           // GameObject targ ;
           //// if (targ != null)
           // if(!Controller.Instance.Mouseover.TryGet<GameObject>(out targ))
           //     Label_Mouseover.Text = targ.ToString();
           // else
           //     Label_Mouseover.Text = "";




            base.Update();
        }

        public override bool Close()
        {
            //Map.Instance.VisibleChunksChanged -= Instance_VisibleChunksChanged;
            //Player.Actor.CurrentTaskChanged -= Instance_CurrentTaskChanged;
            //UI.FpsCounter.Updated -= UIFpsCounter_Updated;
            return base.Close();
        }
    }
}
