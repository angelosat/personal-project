using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System.Linq;

namespace Start_a_Town_
{
    class VideoSettings : GameSettings
    {
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();

        bool _tempFullscreen;
        Rectangle _tempResolution;

        GroupBox CreateGui()
        {
            var box = new GroupBox("Video");

            var modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;

            var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");

            this._tempResolution = Game1.Instance.Window.ClientBounds;
            Rectangle getCurrentClientBounds()
            {
                return this._tempResolution;
            }

            string getClientBoundsString(Rectangle dm)
            {
                return $"{dm.Width} x {dm.Height}";
            }

            var comboResolutions = new ComboBoxNewNew<DisplayMode>(
               colormodes, 150, "Resolution",
               res => $"{res.Width} x {res.Height}",
               () => getClientBoundsString(getCurrentClientBounds()), res => this._tempResolution = new Rectangle(0, 0, res.Width, res.Height));

            this._tempFullscreen = Game1.Instance.graphics.IsFullScreen;
            var chkFullscreen = new CheckBoxNew("Fullscreen", () => this._tempFullscreen = !this._tempFullscreen, () => this._tempFullscreen);

            box.AddControlsVertically(comboResolutions, chkFullscreen);
            return box;
        }
        internal override void Apply()
        {
            Game1.Instance.graphics.PreferredBackBufferHeight = this._tempResolution.Height;
            Game1.Instance.graphics.PreferredBackBufferWidth = this._tempResolution.Width;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = this._tempResolution.Width.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = this._tempResolution.Height.ToString();
            Game1.Instance.graphics.IsFullScreen = this._tempFullscreen;
            Game1.Instance.graphics.ApplyChanges();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Fullscreen").Value = this._tempFullscreen.ToString();
        }
        internal override void Cancel()
        {
        }
    }
}
