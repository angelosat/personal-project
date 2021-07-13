using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class VideoSettings : GameSettings
    {
        ComboBoxNewNew<DisplayMode> Combo_Resolutions;
        CheckBox Chk_Fullscreen;
        Rectangle TempResolution;
        GroupBox _Gui;
        internal GroupBox Gui => this._Gui ??= this.CreateGui();

        GroupBox CreateGui()
        {
            var box = new GroupBox();
            box.Name = "Video";

            var modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;

            var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");

            this.TempResolution = Game1.Instance.Window.ClientBounds;
            Rectangle getCurrentClientBounds() => this.TempResolution;
            string getClientBoundsString(Rectangle dm) => $"{dm.Width} x {dm.Height}";
            this.Combo_Resolutions = new ComboBoxNewNew<DisplayMode>(
               colormodes, 150, "Resolution",
               res => $"{res.Width} x {res.Height}",
               () => getClientBoundsString(getCurrentClientBounds()), res => this.TempResolution = new Rectangle(0, 0, res.Width, res.Height));

            this.Chk_Fullscreen = new CheckBox("Fullscreen", new Vector2(0, Combo_Resolutions.Bottom));
            this.Chk_Fullscreen.Checked = Game1.Instance.graphics.IsFullScreen;

            box.Controls.Add(this.Combo_Resolutions, this.Chk_Fullscreen);
            return box;
        }
        internal override void Apply()
        {
            Game1.Instance.graphics.PreferredBackBufferHeight = this.TempResolution.Height;
            Game1.Instance.graphics.PreferredBackBufferWidth = this.TempResolution.Width;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = this.TempResolution.Width.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = this.TempResolution.Height.ToString();
            Game1.Instance.graphics.IsFullScreen = this.Chk_Fullscreen.Checked;
            Game1.Instance.graphics.ApplyChanges();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Fullscreen").Value = this.Chk_Fullscreen.Checked.ToString();
            Engine.Config.Save("config.xml");
        }
    }

    //class VideoSettings : GroupBox
    //{
    //    ComboBoxNewNew<DisplayMode> Combo_Resolutions;
    //    CheckBox Chk_Fullscreen;
    //    Rectangle TempResolution;

    //    public VideoSettings()
    //    {
    //        this.Name = "Video";

    //        var modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;

    //        var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");

    //        this.TempResolution = Game1.Instance.Window.ClientBounds;
    //        Rectangle getCurrentClientBounds() => this.TempResolution;
    //        string getClientBoundsString(Rectangle dm) => $"{dm.Width} x {dm.Height}";
    //        this.Combo_Resolutions = new ComboBoxNewNew<DisplayMode>(
    //           colormodes, 150, "Resolution",
    //           res => $"{res.Width} x {res.Height}",
    //           () => getClientBoundsString(getCurrentClientBounds()), res => this.TempResolution = new Rectangle(0, 0, res.Width, res.Height));

    //        this.Chk_Fullscreen = new CheckBox("Fullscreen", new Vector2(0, Combo_Resolutions.Bottom));
    //        this.Chk_Fullscreen.Checked = Game1.Instance.graphics.IsFullScreen;

    //        this.Controls.Add(this.Combo_Resolutions, this.Chk_Fullscreen);
    //    }

    //    internal void Apply()
    //    {
    //        Game1.Instance.graphics.PreferredBackBufferHeight = this.TempResolution.Height;
    //        Game1.Instance.graphics.PreferredBackBufferWidth = this.TempResolution.Width;

    //        Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = this.TempResolution.Width.ToString();
    //        Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = this.TempResolution.Height.ToString();
    //        Game1.Instance.graphics.IsFullScreen = this.Chk_Fullscreen.Checked;
    //        Game1.Instance.graphics.ApplyChanges();
    //        Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Fullscreen").Value = this.Chk_Fullscreen.Checked.ToString();
    //        Engine.Config.Save("config.xml");
    //    }
    //}
}
