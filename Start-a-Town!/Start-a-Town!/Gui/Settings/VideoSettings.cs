﻿using Microsoft.Xna.Framework;
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

            box.AddControlsVertically(comboResolutions);//, chkFullscreen); // fullscreen tickbox not required because i enable borderless fullscreen when the desktop resolution is selected
            return box;
        }
        internal override void Apply()
        {
            Game1.Instance.graphics.PreferredBackBufferHeight = this._tempResolution.Height;
            Game1.Instance.graphics.PreferredBackBufferWidth = this._tempResolution.Width;

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = this._tempResolution.Width.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = this._tempResolution.Height.ToString();
            Game1.Instance.graphics.IsFullScreen = this._tempFullscreen;

            // i've put this here before applychanges, because it i apply the changes with a resolution that equals the desktop resolution, then the window gets shrunk vertically to fit the title bar
            // when that happens, the window.clientbounds gets smaller but the graphics.prefferedbuffer remains the same
            // then the uimanager texture gets drawn distorted and the mouse cursor isn't mapped correctly to screen coordinates
            //System.Windows.Forms.Application.EnableVisualStyles();
            //System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Game1.Instance.Window.Handle);
            //if (Game1.Instance.GraphicsDevice.Adapter.CurrentDisplayMode is var mode && mode.Width == this._tempResolution.Width && mode.Height == this._tempResolution.Height)
            //    gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //else
            //    gameForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Game1.ToggleBorderlessFullscreen();
            Game1.Instance.graphics.ApplyChanges();

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Fullscreen").Value = this._tempFullscreen.ToString();
        }
        internal override void Cancel()
        {
        }
    }
}
