using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class Settings : Window
    {
        static Settings _Instance;
        static public Settings Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Settings();
                return _Instance;
            }
        }

        List<string> resolutions = new List<string>();
        DisplayModeCollection modes;
        ComboBox cb_resolutions;
        CheckBox check_fullscreen, CB_OUTLINETILES, CH_DebugTooltips;
        TrackBar TB_OUTLINES;
        Label LBL_Outlines, LBL_OutlinesMin, LBL_OutlinesMax;
        //public static Window Instance;
        public Settings()
            :base()
        {
            //Instance = this;
            Panel panel = new Panel();
            Panel buttons = new Panel();

            panel.ClientSize = new Rectangle(0, 0, 300, 150);
            
            //Console.WriteLine(Movable.ToString() + " " + base.Movable.ToString());
            Title = "Settings";

            modes = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes;

            var colormodes = modes.TakeWhile(mode => mode.Format.ToString() == "Color");
            List<DisplayMode> gamw = colormodes.ToList();
            
            //foreach (DisplayMode mode in colormodes)
            //    resolutions.Add(mode.Width.ToString() + "x" + mode.Height.ToString()); // + " (" + mode.Format.ToString() + ")");

            

            //cb_resolutions = new ComboBox(panel, new Vector2(panel.ClientSize.Width / 2, 0), 150, "Resolution");
            //Label label_resolutions = new Label(panel, new Vector2(panel.ClientSize.Width / 2, 0), "Resolution", TextAlignment.Right);
            Label label_resolutions = new Label(Vector2.Zero, "Resolution", TextAlignment.Left);
            cb_resolutions = new ComboBox(new Vector2(0, label_resolutions.Bottom), 150, "Resolution");
            

            foreach (DisplayMode mode in colormodes)
                cb_resolutions.Items.Add(mode);
            cb_resolutions.DisplayMemberFunc = new Func<object, string>(foo => (foo as DisplayMode).Width.ToString() + "x" + (foo as DisplayMode).Height.ToString());
            cb_resolutions.SelectedValue = Game1.Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes.FirstOrDefault(foo => foo.Width == Game1.Instance.graphics.PreferredBackBufferWidth && foo.Height == Game1.Instance.graphics.PreferredBackBufferHeight);

            check_fullscreen = new CheckBox("Fullscreen", new Vector2(0, cb_resolutions.Bottom));
            check_fullscreen.Checked = Game1.Instance.graphics.IsFullScreen;

            CH_DebugTooltips = new CheckBox("Debug info in tooltips", new Vector2(0, check_fullscreen.Bottom));
            CH_DebugTooltips.Checked = GlobalVars.DebugMode;

            CB_OUTLINETILES = new CheckBox("Outline Tile Edges", new Vector2(0, CH_DebugTooltips.Bottom));
            CB_OUTLINETILES.Checked = GlobalVars.Settings.OutlineTileEdges;

            

            //LBL_Outlines = new Label(new Vector2(CB_OUTLINETILES.Width / 2, CB_OUTLINETILES.Bottom), "Tile Outlines", TextAlignment.Center);
            LBL_Outlines = new Label(new Vector2(0, CH_DebugTooltips.Bottom), "Tile Outlines");

            TB_OUTLINES = new TrackBar(new Vector2(0, LBL_Outlines.Bottom), cb_resolutions.Width);
            TB_OUTLINES.Maximum = 2;
            TB_OUTLINES.LargeChange = 1;
            //TB_OUTLINES.ValueChanged += new EventHandler<EventArgs>(TB_OUTLINES_ValueChanged);
            TB_OUTLINES.Value = GlobalVars.Settings.TileEdgeOutlines;
            //string txt = "";
            //switch(Global.Settings.TileEdgeOutlines)
            //{
            //    case 0:
            //        txt = "None";
            //        break;
            //    case 1:
            //        txt = "Thin";
            //        break;
            //    case 2:
            //        txt = "Thick";
            //        break;
            //    default:
            //        break;
            //}
            LBL_OutlinesMin = new Label(new Vector2(0, TB_OUTLINES.Bottom), "None");
            LBL_OutlinesMax = new Label(new Vector2(cb_resolutions.Width, TB_OUTLINES.Bottom), "Thick", TextAlignment.Right);

            //ComboBox cb_uicolor = new ComboBox(new Vector2(0, LBL_OutlinesMin.Bottom), TB_OUTLINES.Width);
            //Color.

            panel.Controls.Add( cb_resolutions, label_resolutions, check_fullscreen, CH_DebugTooltips, TB_OUTLINES, LBL_Outlines, LBL_OutlinesMin, LBL_OutlinesMax );
            panel.ClientSize = panel.PreferredSize;

            //Button ok = new Button(panel, new Vector2(0, panel.ClientSize.Height - UIManager.DefaultButtonHeight), panel.ClientSize.Width, "Apply");
            Button ok = new Button(Vector2.Zero, panel.ClientSize.Width, "Apply");
            ok.Click += new UIEvent(ok_Click);
            buttons.Controls.Add(ok);
            buttons.Top = panel.Bottom;
            buttons.ClientSize = buttons.PreferredSize;

            Controls.Add(buttons);
            Controls.Add(panel);
            
            
            ClientSize = PreferredSize;
            //SizeToControl(panel);
            //ScreenLocation = CenterScreen;
            Location = CenterScreen;
            
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

        void ok_Click(Object sender, EventArgs e)
        {
            //DisplayMode mode = modes.ElementAt(cb_resolutions.SelectedValue);
            //DisplayMode currentmode = Game1.Instance.GraphicsDevice.DisplayMode;
            bool gfxchanged = false;
            DisplayMode mode = (DisplayMode)cb_resolutions.SelectedValue;
            if (Game1.Instance.graphics.PreferredBackBufferWidth != mode.Width || Game1.Instance.graphics.PreferredBackBufferHeight != mode.Height)
            {
                Game1.Instance.graphics.PreferredBackBufferHeight = mode.Height;
                Game1.Instance.graphics.PreferredBackBufferWidth = mode.Width;

                gfxchanged = true;
            }
            if (Game1.Instance.graphics.IsFullScreen != check_fullscreen.Checked)
            {
                Game1.Instance.graphics.IsFullScreen = check_fullscreen.Checked;
                gfxchanged = true;
            }

            if(gfxchanged)
                Game1.Instance.graphics.ApplyChanges();
            //Global.Settings.OutlineTileEdges = CB_OUTLINETILES.Checked;
            GlobalVars.Settings.TileEdgeOutlines = TB_OUTLINES.Value;
            GlobalVars.DebugMode = CH_DebugTooltips.Checked;
        }

        //void cb_resolutions_SelectedValueChanged(UIEventArgs e)
        //{
        //    Console.WriteLine("GAMAW");
        //}
    }
}