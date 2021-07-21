﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    class SettingsWindow : Window
    {
        static SettingsWindow _Instance;
        static public SettingsWindow Instance => _Instance ??= new SettingsWindow();

        Panel Panel;

        readonly GraphicsSettings GraphicSettings;
        readonly VideoSettings VideoSettings;
        readonly InterfaceSettings InterfaceSettings;
        readonly CameraSettings CameraSettings;
        readonly HotkeyManager Hotkeys;

        List<GameSettings> AllSettings;

        SettingsWindow()
        {
            this.Title = "Settings";
            this.AutoSize = true;

            var size = 400;
            this.Panel = new();
            this.Panel.ClientSize = new Rectangle(0, 0, size, size / 2);
            this.Panel.ConformToClientSize();

            this.InterfaceSettings = new InterfaceSettings();
            this.CameraSettings = new CameraSettings();
            this.GraphicSettings = new GraphicsSettings();
            this.VideoSettings = new VideoSettings();
            this.Hotkeys = new HotkeyManager();

            this.AllSettings = new List<GameSettings>() { this.CameraSettings, GraphicSettings, this.VideoSettings, this.InterfaceSettings, this.Hotkeys };
            var tabs = UIHelper.Wrap(this.AllSettings.Select(tab => new Button(tab.Gui.Name, () => selectTab(tab.Gui))));//, size);

            selectTab(this.CameraSettings.Gui);
            
            Button ok = new("Apply", apply, 50);
            Button cancel = new("Cancel", () => this.Hide(), 50);// { Location = ok.TopRight };

            this.Client.AddControlsVertically(//0, HorizontalAlignment.Right,
                tabs.ToPanel(),
                this.Panel,
                //UIHelper.Wrap(this.Panel.ClientSize.Width, ok, cancel));
                UIHelper.Wrap(ok, cancel));

            this.AnchorToScreenCenter();

            void apply()
            {
                foreach (var m in this.AllSettings)
                    m.Apply();
            }

            void selectTab(GroupBox tab)
            {
                this.Panel.Controls.Clear();
                this.Panel.Controls.Add(tab);
            }
        }
    }
}