using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SettingsWindow : Window
    {
        static SettingsWindow _Instance;
        static public SettingsWindow Instance => _Instance ??= new SettingsWindow();

        Panel Panel, Panel_Buttons;

        readonly GraphicsSettings GraphicSettings;
        readonly VideoSettings VideoSettings;
        readonly InterfaceSettings InterfaceSettings;
        readonly CameraSettings CameraSettings;
        readonly ControlsSettings ControlSettings;

        List<GroupBox> Tabs;

        SettingsWindow()
        {
            this.Title = "Settings";
            this.AutoSize = true;

            var size = 300;
            this.Panel = new();
            this.Panel.ClientSize = new Rectangle(0, 0, size, size);
            this.Panel.ConformToClientSize();

            this.InterfaceSettings = new InterfaceSettings();
            this.CameraSettings = new CameraSettings();
            this.GraphicSettings = new GraphicsSettings();
            this.VideoSettings = new VideoSettings();
            this.ControlSettings = new ControlsSettings();

            this.Tabs = new List<GroupBox>() { this.CameraSettings.Gui, GraphicSettings.Gui, this.VideoSettings.Gui, this.InterfaceSettings.Gui, this.ControlSettings.Gui };
            var tabs = UIHelper.Wrap(this.Tabs.Select(tab => new Button(tab.Name, () => selectTab(tab))), size);

            selectTab(this.CameraSettings.Gui);
            
            Button ok = new("Apply", apply, 50);
            Button cancel = new("Cancel", () => this.Hide(), 50) { Location = ok.TopRight };

            Panel_Buttons = new Panel() { AutoSize = true };
            Panel_Buttons.Controls.Add(ok, cancel);

            this.Client.AddControlsVertically(
                tabs.ToPanel(),
                this.Panel,
                UIHelper.Wrap(this.Panel.ClientSize.Width, ok, cancel).ToPanel());

            this.SnapToScreenCenter();
            this.Anchor = new Vector2(0.5f);

            void apply()
            {
                this.GraphicSettings.Apply();
                this.VideoSettings.Apply();
                this.InterfaceSettings.Apply();
                this.CameraSettings.Apply();
                this.ControlSettings.Apply();
            }

            void selectTab(GroupBox tab)
            {
                this.Panel.Controls.Clear();
                this.Panel.Controls.Add(tab);
            }
        }
    }
}