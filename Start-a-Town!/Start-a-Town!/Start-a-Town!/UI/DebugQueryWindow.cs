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
    class DebugQueryWindow : Window
    {
        static DebugQueryWindow _Instance;

        //public 
        GameObject Object;
        Component Component;
        Label ComponentInfo;

        static public void SetObject(GameObject obj)  
        {
            

        }

        Panel Panel_Tabs, Panel_Mouseover;

        public DebugQueryWindow(GameObject obj)
        {
            Object = obj;
            Title = obj.Name;
            AutoSize = true;
            Movable = true;

            Panel_Tabs = new Panel();
            Panel_Tabs.AutoSize = true;

            RadioButton prev = null;
            foreach (KeyValuePair<string, Component> comp in obj.Components)
            {
                RadioButton rad = new RadioButton(comp.Key, new Vector2(0, prev != null ? prev.Bottom : 0));
                rad.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(rad_MouseLeftPress);
                rad.Tag = comp.Value;
                Panel_Tabs.Controls.Add(rad);
                prev = rad;

                if (comp.Value is GeneralComponent)
                {
                    Component = comp.Value;
                    ComponentInfo = new Label(comp.Value.ToString());
                    
                    rad.Checked = true;
                }
            }

            Panel_Mouseover = new Panel(new Vector2(Panel_Tabs.Right, 0));
            Panel_Mouseover.AutoSize = true;
            
            Panel_Mouseover.Height = 100;
            Panel_Mouseover.Width = 100;

            if (ComponentInfo != null)
                Panel_Mouseover.Controls.Add(ComponentInfo);

            Client.Controls.Add(Panel_Tabs, Panel_Mouseover);
            Location = UIManager.Mouse;
        }

        public override void Update()
        {
            
            if (ComponentInfo != null)
                ComponentInfo.Text = Component.ToString();
            base.Update();
        }

        void rad_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            RadioButton rad = sender as RadioButton;
            Component component = rad.Tag as Component;
            Component = component;
            Panel_Mouseover.Controls.Clear();
            ComponentInfo = new Label(component.ToString());
            Panel_Mouseover.Controls.Add(ComponentInfo);

            //Controls.Remove(Panel_Mouseover);
            Client.Controls.Add(Panel_Mouseover);
        }

        

        void chunkshot_Click(object sender, EventArgs e)
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            using (RenderTarget2D screenshot = new RenderTarget2D(gfx, Block.Width * Chunk.Size, Map.MaxHeight * Block.Height))//Player.Instance.Chunk.Bounds.Height))
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


        void RB_mouseover_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Controls.Add(Panel_Mouseover);
            else
                Controls.Remove(Panel_Mouseover);
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
