using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class SettingsWindow : Window
    {
        static SettingsWindow _Instance;
        static public SettingsWindow Instance => _Instance ??= new SettingsWindow();

        readonly GraphicsSettings GraphicSettings;
        readonly VideoSettings VideoSettings;
        readonly InterfaceSettings InterfaceSettings;
        readonly CameraSettings CameraSettings;
        readonly HotkeyManager Hotkeys;
        readonly List<GameSettings> AllSettings;

        SettingsWindow()
        {
            this.Title = "Settings";
            this.AutoSize = true;
            this.Closable = false;
            GroupBox selectedSettings = null;

            var size = 400;
            var panel = new PanelLabeledScrollable(() => selectedSettings?.Name, size, size / 2, ScrollModes.Vertical);

            this.InterfaceSettings = new InterfaceSettings();
            this.CameraSettings = new CameraSettings();
            this.GraphicSettings = new GraphicsSettings();
            this.VideoSettings = new VideoSettings();
            this.Hotkeys = new HotkeyManager();

            this.AllSettings = new List<GameSettings>() { this.CameraSettings, GraphicSettings, this.VideoSettings, this.InterfaceSettings, this.Hotkeys };
            var tabs = UIHelper.Wrap(this.AllSettings.Select(tab => new Button(tab.Gui.Name, () => selectTab(tab.Gui))), panel.Client.Width);

            selectTab(this.CameraSettings.Gui);
            
            var btnok = new Button("Apply", apply, 50);
            var btncancel = new Button("Cancel", cancel, 50);
            
            this.Client.AddControlsVertically(0, HorizontalAlignment.Right,
                tabs.ToPanel(),
                panel,
                UIHelper.Wrap(btnok, btncancel));

            this.AnchorToScreenCenter();

            void apply()
            {
                foreach (var m in this.AllSettings)
                    m.Apply();
                Engine.SaveConfig();
            }
            void cancel()
            {
                foreach (var m in this.AllSettings)
                    m.Cancel();
                this.Hide();
            }
            void selectTab(GroupBox tab)
            {
                selectedSettings = tab;
                panel.Client.ClearControls();
                panel.Client.AddControls(tab);
            }
        }
    }
}