using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Label Label_UIScale;
        DisplayModeCollection modes;
        ComboBox<DisplayMode> Combo_Resolutions;
        CheckBox check_fullscreen, 
            CB_OUTLINETILES, CH_DebugTooltips, 
            Chk_HideWalls, Chk_HideTerrain;
        TrackBar TB_OUTLINES;
        SliderNew Slider_UIScale;
        Label LBL_Outlines;
        RadioButton Rd_Game, Rd_Graphics, Rd_UI;
        List<GroupBox> Tabs;
        Panel Panel_TabsList;
        ListBox<GroupBox, Button> List_Tabs;

        bool HideWalls;
        void RestoreValues()
        {
            Engine.HideWalls = HideWalls;
        }

        SettingsWindow()
        {
            Title = "Settings";
            AutoSize = true;
            Panel_Tabs = new Panel();
            Panel_Tabs.AutoSize = true;
            Panel_Tabs.BackgroundStyle = BackgroundStyle.TickBox;
            Rd_Game = new RadioButton("Game", Vector2.Zero);
            Rd_Graphics = new RadioButton("Graphics", new Vector2(Rd_Game.Width, 0));
            Rd_UI = new RadioButton("UI", Rd_Graphics.TopRight, false);
            Panel = new Panel(Panel_Tabs.BottomLeft, new Vector2(300, 300));
            modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;
            var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");
            List<DisplayMode> gamw = colormodes.ToList();
            Label label_resolutions = new Label(Vector2.Zero, "Resolution", HorizontalAlignment.Left);
            string currentResolution = Game1.Instance.Window.ClientBounds.Width + "x" + Game1.Instance.Window.ClientBounds.Height;
            ListBox<DisplayMode, Button> resolutions = new ListBox<DisplayMode, Button>(new Rectangle(0, 0, 150, 10 * Button.DefaultHeight));
            resolutions.Build(colormodes, foo => foo.Width.ToString() + "x" + foo.Height.ToString());
            Combo_Resolutions = new ComboBox<DisplayMode>(resolutions, res => res.Width.ToString() + "x" + res.Height.ToString()) { Location = label_resolutions.BottomLeft, Text = currentResolution };//, TextSelector = res => res.Width.ToString() + "x" + res.Height.ToString() };
            check_fullscreen = new CheckBox("Fullscreen", new Vector2(0, Combo_Resolutions.Bottom));
            check_fullscreen.Checked = Game1.Instance.graphics.IsFullScreen;
            Label_UIScale = new Label(Vector2.Zero, "UI Scale: " + UIManager.Scale);
            Slider_UIScale = new SliderNew(new Vector2(0, Label_UIScale.Bottom), Combo_Resolutions.Width, 1, 2, 0.1f, UIManager.Scale) { Name = "UI Scale: {0}" };
            Label lbl_delay = new Label("Tooltip Delay") { Location = Slider_UIScale.BottomLeft };
            CH_DebugTooltips = new CheckBox("Debug info in tooltips", new Vector2(0, check_fullscreen.Bottom));
            CH_DebugTooltips.Checked = GlobalVars.DebugMode;
            CB_OUTLINETILES = new CheckBox("Outline Tile Edges", new Vector2(0, CH_DebugTooltips.Bottom));
            CB_OUTLINETILES.Checked = GlobalVars.Settings.OutlineTileEdges;
            LBL_Outlines = new Label(new Vector2(0, CH_DebugTooltips.Bottom), "Tile Outlines");
            TB_OUTLINES = new TrackBar(new Vector2(0, LBL_Outlines.Bottom), Combo_Resolutions.Width);
            TB_OUTLINES.Maximum = 2;
            TB_OUTLINES.LargeChange = 1;
            TB_OUTLINES.Value = GlobalVars.Settings.TileEdgeOutlines;
            this.Box_UI = new InterfaceSettings();
            this.Box_Camera = new CameraSettings();
            Rd_Game.Checked = true;
            this.Box_Graphics = new GraphicsSettings();
            this.Box_Video = new VideoSettings();
            this.Box_Controls = new ControlsSettings();
            Tabs = new List<GroupBox>() { Box_Camera, Box_Graphics, this.Box_Video, Box_UI, Box_Controls };
            int w = 0, h = 0;
            foreach (var foo in this.Tabs)
            {
                w = Math.Max(w, foo.Width); 
                h = Math.Max(h, foo.Height);
            }
            Panel.ClientSize = new Rectangle(0, 0, Math.Max(w, Panel.ClientSize.Width), Math.Max(h, Panel.ClientSize.Height));
            Rd_Game.Tag = Box_Camera;
            Rd_Graphics.Tag = Box_Graphics;
            Rd_UI.Tag = Box_UI;

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

            Client.Controls.Add(this.Panel_TabsList, Panel);
            CreateButtons();
            Client.Controls.Add(Panel_Buttons);

            this.SnapToScreenCenter();
            Anchor = new Vector2(0.5f);
        }

        private void SelectTab(GroupBox tab)
        {
            this.Panel.Controls.Clear();
            this.Panel.Controls.Add(tab);
        }

        private void CreateButtons()
        {
            Button ok = new Button(Vector2.Zero, 50, "Apply");
            ok.LeftClick += new UIEvent(ok_Click);
            Button cancel = new Button(ok.TopRight, 50, "Cancel");
            cancel.LeftClick += new UIEvent(cancel_Click);

            Panel_Buttons = new Panel() { Location = this.Panel.BottomRight, AutoSize = true };
            Panel_Buttons.Controls.Add(ok, cancel);
            Panel_Buttons.Anchor = Vector2.UnitX;
        }

        void Chk_HideTerrain_Click(object sender, EventArgs e)
        {
            Engine.HideTerrain = Chk_HideTerrain.Checked;
        }

        void cancel_Click(object sender, EventArgs e)
        {
            RestoreValues();
            Hide();
        }

        void Chk_HidePlayer_Click(object sender, EventArgs e)
        {
            Engine.HideWalls = Chk_HideWalls.Checked;
        }

        void tab_Click(object sender, EventArgs e)
        {
            RadioButton r = sender as RadioButton;
            GroupBox b = r.Tag as GroupBox;
            Panel.Controls.Clear();
            Panel.Controls.Add(b);
        }

        void Slider_UIScale_ValueChanged(object sender, EventArgs e)
        {
            Label_UIScale.Text = "UI Scale: "  + (sender as SliderNew).Value.ToString();
        }

        void ok_Click(Object sender, EventArgs e)
        {

            this.Box_Graphics.Apply();
            this.Box_Video.Apply();
            this.Box_UI.Apply();
            this.Box_Camera.Apply();
        }
    }
}