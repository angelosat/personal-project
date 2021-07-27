using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using System;
using System.IO;
using System.Linq;

namespace Start_a_Town_.UI
{
    class DebugQueryWindow : Window
    {
        EntityComponent Component;
        Label ComponentInfo;
        readonly Panel Panel_Tabs, Panel_Mouseover;

        public DebugQueryWindow(GameObject obj)
        {
            this.Title = obj.Name;
            this.AutoSize = true;
            this.Movable = true;

            this.Panel_Tabs = new Panel();
            this.Panel_Tabs.AutoSize = true;

            this.Panel_Tabs.AddControlsVertically(obj.Components.Select(comp => new Button(comp.Key, () => selectTab(comp.Value))));

            this.Panel_Mouseover = new Panel(new Vector2(this.Panel_Tabs.Right, 0));
            this.Panel_Mouseover.AutoSize = true;

            this.Panel_Mouseover.Height = 100;
            this.Panel_Mouseover.Width = 100;

            if (this.ComponentInfo != null)
                this.Panel_Mouseover.Controls.Add(this.ComponentInfo);

            this.Client.Controls.Add(this.Panel_Tabs, this.Panel_Mouseover);
            this.Location = UIManager.Mouse;

            void selectTab(EntityComponent component)
            {
                this.Component = component;
                this.Panel_Mouseover.Controls.Clear();
                this.ComponentInfo = new Label(component.ToString());
                this.Panel_Mouseover.Controls.Add(this.ComponentInfo);

                this.Client.Controls.Remove(this.Panel_Mouseover);
                this.Client.Controls.Add(this.Panel_Mouseover);
            }
        }

        public override void Update()
        {

            if (this.ComponentInfo != null)
                this.ComponentInfo.Text = this.Component.ToString();
            base.Update();
        }
        void chunkshot_Click(object sender, EventArgs e)
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            using RenderTarget2D screenshot = new(gfx, Block.Width * Chunk.Size, MapBase.MaxHeight * Block.Height);
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
        public override bool Close()
        {
            return base.Close();
        }
    }
}
