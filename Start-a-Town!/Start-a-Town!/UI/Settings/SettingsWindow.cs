using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI.Settings;

namespace Start_a_Town_.UI
{
    class SettingsWindow : Window
    {
        #region Singleton
        static SettingsWindow _Instance;
        static public SettingsWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SettingsWindow();
                return _Instance;
            }
        }
        #endregion

        #region Fields
        Panel Panel, Panel_Buttons, Panel_Tabs;
        //GroupBox //Box_Graphics,
        //    Box_Game;//, Box_UI;
        GraphicsSettings Box_Graphics;
        VideoSettings Box_Video;
        InterfaceSettings Box_UI;
        CameraSettings Box_Camera;
        ControlsSettings Box_Controls;
        Label Label_UIScale;
        Slider Slider_UIScale, Sldr_TooltipDelay;
        List<string> resolutions = new List<string>();
        DisplayModeCollection modes;
        ComboBox<DisplayMode> Combo_Resolutions;
        CheckBox check_fullscreen, 
            CB_OUTLINETILES, CH_DebugTooltips, 
            //CB_AsyncLoading,
            //CB_MouseTooltip, 
            Chk_HideWalls, Chk_HideTerrain;
        TrackBar TB_OUTLINES;
        Label LBL_Outlines, LBL_OutlinesMin, LBL_OutlinesMax;
        RadioButton Rd_Game, Rd_Graphics, Rd_UI;
        List<GroupBox> Tabs;
        Panel Panel_TabsList;
        ListBox<GroupBox, Button> List_Tabs;
        #endregion

        #region Original Values
        bool HideWalls;//, HideTerrain;

        //void DefaultValues()
        //{
        //    HideWalls = Engine.HideWalls;
        //    Chk_HideWalls.Checked = Engine.HideWalls;

        //    HideTerrain = Engine.HideTerrain;
        //    Chk_HideTerrain.Checked = HideTerrain;

        //    Sldr_TooltipDelay.Value = TooltipManager.DelayInterval / Engine.TargetFps;
        //    Slider_UIScale.Value = UIManager.Scale;

        //}
        void RestoreValues()
        {
            Engine.HideWalls = HideWalls;
        }
        #endregion

        SettingsWindow()
        {
            Title = "Settings";
            AutoSize = true;



            Panel_Tabs = new Panel();
            Panel_Tabs.AutoSize = true;
            Panel_Tabs.BackgroundStyle = BackgroundStyle.TickBox;

            Rd_Game = new RadioButton("Game", Vector2.Zero);
            //Rd_Game.LeftClick += new UIEvent(tab_Click);

            Rd_Graphics = new RadioButton("Graphics", new Vector2(Rd_Game.Width, 0));
            //Rd_Graphics.LeftClick += new UIEvent(tab_Click);

            Rd_UI = new RadioButton("UI", Rd_Graphics.TopRight, false);
            //Rd_UI.LeftClick += new UIEvent(tab_Click);

            //Panel_Tabs.Controls.Add(Rd_Game, Rd_Graphics, Rd_UI);


            Panel = new Panel(Panel_Tabs.BottomLeft, new Vector2(300, 300));



            modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;

            var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");
            List<DisplayMode> gamw = colormodes.ToList();

            Label label_resolutions = new Label(Vector2.Zero, "Resolution", HorizontalAlignment.Left);
            string currentResolution = Game1.Instance.Window.ClientBounds.Width + "x" + Game1.Instance.Window.ClientBounds.Height; //Game1.ScreenSize.X + "x" + Game1.ScreenSize.Y;

            ListBox<DisplayMode, Button> resolutions = new ListBox<DisplayMode, Button>(new Rectangle(0, 0, 150, 10 * Button.DefaultHeight));
            resolutions.Build(colormodes, foo => foo.Width.ToString() + "x" + foo.Height.ToString());
            Combo_Resolutions = new ComboBox<DisplayMode>(resolutions, res => res.Width.ToString() + "x" + res.Height.ToString()) { Location = label_resolutions.BottomLeft, Text = currentResolution };//, TextSelector = res => res.Width.ToString() + "x" + res.Height.ToString() };
            //Combo_Resolutions.SelectedItemChanged += new EventHandler<EventArgs>(cb_resolutions_SelectedItemChanged);

            check_fullscreen = new CheckBox("Fullscreen", new Vector2(0, Combo_Resolutions.Bottom));
            check_fullscreen.Checked = Game1.Instance.graphics.IsFullScreen;

            //CB_AsyncLoading = new CheckBox("Async Loading", new Vector2(0, check_fullscreen.Bottom));
            //CB_AsyncLoading.Checked = (bool)Engine.Instance["AsyncLoading"];
            //CB_AsyncLoading.HoverText = "If enabled, chunks will load on-the-fly as you play.";



            Label_UIScale = new Label(Vector2.Zero, "UI Scale: " + UIManager.Scale); //new Vector2(0, CB_MouseTooltip.Bottom)
            Slider_UIScale = new Slider(new Vector2(0, Label_UIScale.Bottom), Combo_Resolutions.Width, 1, 2, 0.1f, UIManager.Scale) { Name = "UI Scale: {0}" };

            Label lbl_delay = new Label("Tooltip Delay") { Location = Slider_UIScale.BottomLeft };

            Sldr_TooltipDelay = new Slider(lbl_delay.BottomLeft, Combo_Resolutions.Width, 0, 2, 0.1f, TooltipManager.DelayInterval / Engine.TicksPerSecond) { Name = "Tooltip Delay: {0}s" };

            //CB_MouseTooltip = new CheckBox("Mouse Tooltip", Sldr_TooltipDelay.BottomLeft);
            //CB_MouseTooltip.Checked = (bool)Engine.Instance["MouseTooltip"];
            //CB_MouseTooltip.HoverText = "Anchor tooltips to the mouse.";

            CH_DebugTooltips = new CheckBox("Debug info in tooltips", new Vector2(0, check_fullscreen.Bottom));
            CH_DebugTooltips.Checked = GlobalVars.DebugMode;

            CB_OUTLINETILES = new CheckBox("Outline Tile Edges", new Vector2(0, CH_DebugTooltips.Bottom));
            CB_OUTLINETILES.Checked = GlobalVars.Settings.OutlineTileEdges;



            LBL_Outlines = new Label(new Vector2(0, CH_DebugTooltips.Bottom), "Tile Outlines");

            TB_OUTLINES = new TrackBar(new Vector2(0, LBL_Outlines.Bottom), Combo_Resolutions.Width);
            TB_OUTLINES.Maximum = 2;
            TB_OUTLINES.LargeChange = 1;
            TB_OUTLINES.Value = GlobalVars.Settings.TileEdgeOutlines;
           
            LBL_OutlinesMin = new Label(new Vector2(0, TB_OUTLINES.Bottom), "None");
            LBL_OutlinesMax = new Label(new Vector2(Combo_Resolutions.Width, TB_OUTLINES.Bottom), "Thick", HorizontalAlignment.Right);

      
            //Box_Graphics.Controls.Add(Combo_Resolutions, label_resolutions, check_fullscreen, CB_AsyncLoading);//CH_DebugTooltips, , LBL_Outlines, LBL_OutlinesMin, LBL_OutlinesMax); //TB_OUTLINES

            //Box_UI = new GroupBox() { AutoSize = true, Name = "Interface" };
            //Box_UI.Controls.Add(Label_UIScale, Slider_UIScale, CB_MouseTooltip, lbl_delay, Sldr_TooltipDelay);
            this.Box_UI = new InterfaceSettings();


            //Chk_HideWalls = new CheckBox("Hide Player", Vector2.Zero, Engine.HideWalls);
            //Chk_HideWalls.LeftClick += new UIEvent(Chk_HidePlayer_Click);
            //Chk_HideWalls.HoverText = "Don't draw blocks that are in front of the player character.";
            //Chk_HideTerrain = new CheckBox("Hide Terrain", Chk_HideWalls.BottomLeft, Engine.HideTerrain);
            //Chk_HideTerrain.LeftClick += new UIEvent(Chk_HideTerrain_Click);
            //Chk_HideTerrain.HoverText = "Don't draw blocks that are above the player character.";
            //Box_Game = new GroupBox() { Name = "Game" };//(Vector2.Zero, new Vector2(Box_Graphics.Size.Width, Box_Graphics.Size.Height));
            //Box_Game.Controls.Add(Chk_HideWalls, Chk_HideTerrain);


            this.Box_Camera = new CameraSettings();
            //this.Panel.Controls.Add(this.Box_Camera);

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
                .Build(this.Tabs, i => i.Name);//, (tab, btn) => { btn.LeftClickAction = () => SelectTab(tab); });
            this.List_Tabs.ItemChangedFunc = (tab) => SelectTab(tab);
            this.Panel_TabsList.Controls.Add(this.List_Tabs);
            this.List_Tabs.SelectItem(this.Box_Camera);//this.Panel.Controls.FirstOrDefault() as GroupBox);

            //this.Panel.Location = this.Panel_Tabs.BottomLeft;
            this.Panel.Location = this.Panel_TabsList.TopRight;

            //Client.Controls.Add(Panel_Tabs, Panel, Panel_Buttons);
            Client.Controls.Add(this.Panel_TabsList, Panel);//, Panel_Buttons);
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

        void Sldr_TooltipDelay_ValueChanged(object sender, EventArgs e)
        {
          //  throw new NotImplementedException();
        }

        //void Combo_Resolutions_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (Combo_Resolutions.Open)
        //        this.Controls.Remove(Combo_Resolutions.List);
        //    else
        //        this.Controls.Add(Combo_Resolutions.List);
        //    Combo_Resolutions.Open = !Combo_Resolutions.Open;
        //}

        private void CreateButtons()
        {
            //Button ok = new Button(Vector2.Zero, Panel.ClientSize.Width, "Apply");
            //ok.LeftClick += new UIEvent(ok_Click);

            //Button cancel = new Button(ok.BottomLeft, Panel.ClientSize.Width, "Cancel");
            //cancel.LeftClick += new UIEvent(cancel_Click);

            Button ok = new Button(Vector2.Zero, 50, "Apply");
            ok.LeftClick += new UIEvent(ok_Click);
            Button cancel = new Button(ok.TopRight, 50, "Cancel");
            cancel.LeftClick += new UIEvent(cancel_Click);


            //Panel_Buttons = new Panel() { Location = Panel.BottomLeft, AutoSize = true };
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

        void CB_AsyncLoading_Click(object sender, EventArgs e)
        {
         //   CB_AsyncLoading.Checked = !CB_AsyncLoading.Checked;
        }

        void check_fullscreen_Click(object sender, EventArgs e)
        {
         //   check_fullscreen.Checked = !check_fullscreen.Checked;
        }

        void Slider_UIScale_ValueChanged(object sender, EventArgs e)
        {
            Label_UIScale.Text = "UI Scale: "  + (sender as Slider).Value.ToString();
        }

        void cb_resolutions_SelectedItemChanged(object sender, EventArgs e)
        {
           // cb_resolutions.Close();

        }

        //void TB_OUTLINES_ValueChanged(object sender, EventArgs e)
        //{
        //    string txt = "";
        //    switch ((sender as TrackBar).Value)
        //    {
        //        case 0:
        //            txt = "None";
        //            break;
        //        case 1:
        //            txt = "Thin";
        //            break;
        //        case 2:
        //            txt = "Thick";
        //            break;
        //        default:
        //            break;
        //    }
        //    LBL_Outlines.Text = "Tile Outlines: " + txt;
        //}

        //public override bool Show()
        //{
        //    DefaultValues();
        //    return base.Show();
        //}

        void ok_Click(Object sender, EventArgs e)
        {

            this.Box_Graphics.Apply();
            this.Box_Video.Apply();
            this.Box_UI.Apply();
            this.Box_Camera.Apply();
            //    Engine.Instance["AsyncLoading"] = CB_AsyncLoading.Checked;
            //    Engine.Instance["MouseTooltip"] = CB_MouseTooltip.Checked;
            //UIManager.Scale = Slider_UIScale.Value;
            //Engine.Settings["Settings"]["Interface"]["UIScale"].InnerText = UIManager.Scale.ToString();
            //Engine.Settings["Settings"]["Interface"]["MouseTooltip"].InnerText = CB_MouseTooltip.Checked.ToString();

            //Engine.Settings["Settings"]["Engine"]["AsyncLoading"].InnerText = CB_AsyncLoading.Checked.ToString();
           
        }

        
    }
}