using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI.Settings;

namespace Start_a_Town_.UI
{
    class SettingsWindow : Window
    {
        static SettingsWindow _Instance;
        static public SettingsWindow Instance => _Instance ??= new SettingsWindow();

        Panel Panel, Panel_Buttons, Panel_Tabs;
        GraphicsSettings Box_Graphics;
        VideoSettings Box_Video;
        InterfaceSettings Box_UI;
        CameraSettings Box_Camera;
        ControlsSettings Box_Controls;
        List<GroupBox> Tabs;
        Panel Panel_TabsList;
        ListBox<GroupBox, Button> List_Tabs;

        SettingsWindow()
        {
            this.Title = "Settings";
            this.AutoSize = true;
            this.Panel_Tabs = new Panel();
            this.Panel_Tabs.AutoSize = true;
            this.Panel_Tabs.BackgroundStyle = BackgroundStyle.TickBox;
            this.Panel = new Panel(Panel_Tabs.BottomLeft, new Vector2(300, 300));
           
  
            this.Box_UI = new InterfaceSettings();
            this.Box_Camera = new CameraSettings();
            this.Box_Graphics = new GraphicsSettings();
            this.Box_Video = new VideoSettings();
            this.Box_Controls = new ControlsSettings();
            this.Tabs = new List<GroupBox>() { Box_Camera, Box_Graphics, this.Box_Video, Box_UI, Box_Controls };
            int w = 0, h = 0;
            foreach (var foo in this.Tabs)
            {
                w = Math.Max(w, foo.Width); 
                h = Math.Max(h, foo.Height);
            }
            this.Panel.ClientSize = new Rectangle(0, 0, Math.Max(w, Panel.ClientSize.Width), Math.Max(h, Panel.ClientSize.Height));

            foreach(var tab in this.Tabs)
            {
                var rd = new RadioButton(tab.Name, this.Panel_Tabs.Controls.TopRight);
                rd.Checked = tab == this.Panel.Controls.FirstOrDefault();
                rd.Tag = tab;
                rd.LeftClickAction = () =>
                {
                    SelectTab(tab);
                };
                this.Panel_Tabs.Controls.Add(rd);
            }

            this.Panel_TabsList = new Panel() { AutoSize = true };
            this.List_Tabs = new ListBox<GroupBox, Button>((from i in this.Tabs select i.Name).MaxWidth(UIManager.Font) + 2 * UIManager.defaultButtonSprite.Width, this.Panel.ClientSize.Height)
                .Build(this.Tabs, i => i.Name);
            this.List_Tabs.ItemChangedFunc = (tab) => SelectTab(tab);
            this.Panel_TabsList.Controls.Add(this.List_Tabs);
            this.List_Tabs.SelectItem(this.Box_Camera);

            this.Panel.Location = this.Panel_TabsList.TopRight;

            this.Client.Controls.Add(this.Panel_TabsList, Panel);
            this.CreateButtons();
            this.Client.Controls.Add(Panel_Buttons);

            this.SnapToScreenCenter();
            this.Anchor = new Vector2(0.5f);
        }

        private void SelectTab(GroupBox tab)
        {
            this.Panel.Controls.Clear();
            this.Panel.Controls.Add(tab);
        }

        private void CreateButtons()
        {
            Button ok = new("Apply", apply, 50);
            Button cancel = new("Cancel", () => this.Hide(), 50) { Location = ok.TopRight };

            Panel_Buttons = new Panel() { Location = this.Panel.BottomRight, AutoSize = true };
            Panel_Buttons.Controls.Add(ok, cancel);
            Panel_Buttons.Anchor = Vector2.UnitX;

            void apply()
            {
                this.Box_Graphics.Apply();
                this.Box_Video.Apply();
                this.Box_UI.Apply();
                this.Box_Camera.Apply();
            }
        }
    }
}