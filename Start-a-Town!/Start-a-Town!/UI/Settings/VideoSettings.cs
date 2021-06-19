using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI.Settings
{
    class VideoSettings : GroupBox
    {
        ComboBox<DisplayMode> Combo_Resolutions;
        CheckBox Chk_Fullscreen;
        bool Changed;

        public VideoSettings()
        {
            this.Name = "Video";

            var modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;

            var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");
            List<DisplayMode> gamw = colormodes.ToList();

            Label label_resolutions = new Label(Vector2.Zero, "Resolution", HorizontalAlignment.Left);
            string currentResolution = Game1.Instance.Window.ClientBounds.Width + "x" + Game1.Instance.Window.ClientBounds.Height; 

            ListBox<DisplayMode, Button> resolutions = new ListBox<DisplayMode, Button>(new Rectangle(0, 0, 150, 10 * Button.DefaultHeight));
            resolutions.Build(colormodes, foo => foo.Width.ToString() + "x" + foo.Height.ToString());
            Combo_Resolutions = new ComboBox<DisplayMode>(resolutions, res => res.Width.ToString() + "x" + res.Height.ToString()) { Location = label_resolutions.BottomLeft, Text = currentResolution };
            this.Combo_Resolutions.ItemChangedFunction = OnResolutionChanged;

            Chk_Fullscreen = new CheckBox("Fullscreen", new Vector2(0, Combo_Resolutions.Bottom));
            Chk_Fullscreen.Checked = Game1.Instance.graphics.IsFullScreen;
            Chk_Fullscreen.LeftClickAction = () => this.Changed = true;



            this.Controls.Add(Combo_Resolutions, label_resolutions, Chk_Fullscreen);
        }

        private void OnResolutionChanged(DisplayMode obj)
        {
            this.Changed = true;
        }

        public void Apply()
        {
            if (!Changed)
                return;
            this.Changed = false;
            DisplayMode displayMode = Combo_Resolutions.SelectedItem;
            if (displayMode != null)
            {
                Game1.Instance.graphics.PreferredBackBufferHeight = displayMode.Height;
                Game1.Instance.graphics.PreferredBackBufferWidth = displayMode.Width;
                //Engine.Settings["Settings"]["Video"]["Resolution"]["Width"].InnerText = displayMode.Width.ToString();
                //Engine.Settings["Settings"]["Video"]["Resolution"]["Height"].InnerText = displayMode.Height.ToString();

                Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = displayMode.Width.ToString();
                Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = displayMode.Height.ToString();

            }
            Game1.Instance.graphics.IsFullScreen = this.Chk_Fullscreen.Checked;
            Combo_Resolutions.SelectedItem = null;
            Game1.Instance.graphics.ApplyChanges();
            //Engine.Settings["Settings"]["Video"]["Fullscreen"].InnerText = this.Chk_Fullscreen.Checked.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Fullscreen").Value = this.Chk_Fullscreen.Checked.ToString();
            Engine.Config.Save("config.xml");
            //Engine.Settings.Save("config.xml");
        }

    }
}
