using System;
using System.Linq;
using System.Reflection;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class SettingsWindow : Window
    {
        static SettingsWindow _Instance;
        static public SettingsWindow Instance => _Instance ??= new SettingsWindow();

        SettingsWindow()
        {
            this.Title = "Settings";
            this.AutoSize = true;
            GroupBox selectedSettings = null;

            var size = 400;
            var panel = new PanelLabeledScrollable(() => selectedSettings?.Name, size, size / 2, ScrollModes.Vertical);

            var settingsTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(GameSettings)));
            var allSettings = settingsTypes.Select(t => Activator.CreateInstance(t) as GameSettings);

            var tabs = UIHelper.Wrap(allSettings.Select(tab => new Button(tab.Gui.Name, () => selectTab(tab.Gui))), panel.Client.Width);

            selectTab(allSettings.First().Gui);
            
            var btnok = new Button("Apply", apply, 50);
            var btncancel = new Button("Cancel", cancel, 50);
            
            this.Client.AddControlsVertically(0, HorizontalAlignment.Right,
                tabs.ToPanel(),
                panel,
                UIHelper.Wrap(btnok, btncancel));

            this.AnchorToScreenCenter();

            void apply()
            {
                foreach (var m in allSettings)
                    m.Apply();
                Engine.SaveConfig();
            }
            void cancel()
            {
                foreach (var m in allSettings)
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