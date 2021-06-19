using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI.Settings
{
    class InterfaceSettings : GroupBox
    {
        //bool Changed;
        readonly Slider Sldr_UIScale, Sldr_TooltipDelay;
        readonly CheckBox 
         Chkbox_MouseTooltip;
        readonly Label Lbl_UIScale;
            //, 
            //Lbl_Delay
            //;

        public InterfaceSettings()
        {
            this.Name = "Interface";

            this.Lbl_UIScale = new Label(Vector2.Zero, "UI Scale: " + UIManager.Scale); //new Vector2(0, CB_MouseTooltip.Bottom)
            this.Sldr_UIScale = new Slider(new Vector2(0, Lbl_UIScale.Bottom), 100, 1, 2, 0.1f, UIManager.Scale) { Name = "UI Scale: {0}" };

            Label lbl_delay = new Label("Tooltip Delay") { Location = Sldr_UIScale.BottomLeft };

            this.Sldr_TooltipDelay = new Slider(lbl_delay.BottomLeft, 100, 0, 2, 0.1f, TooltipManager.DelayInterval / Engine.TicksPerSecond) { Name = "Tooltip Delay: {0}s" };
            this.Chkbox_MouseTooltip = new CheckBox("Mouse Tooltip", Sldr_TooltipDelay.BottomLeft);
            this.Chkbox_MouseTooltip.Checked = TooltipManager.MouseTooltips;// (bool)Engine.Instance["MouseTooltip"];
            this.Chkbox_MouseTooltip.HoverText = "Anchor tooltips to the mouse.";

            this.Controls.Add(Lbl_UIScale, Sldr_UIScale, Chkbox_MouseTooltip, lbl_delay, Sldr_TooltipDelay);
        }

        public void Apply()
        {
            //if (!this.Changed)
            //    return;
            //this.Changed = false;

            UIManager.Scale = Sldr_UIScale.Value;
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface").GetOrCreateElement("UIScale").Value = UIManager.Scale.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface").GetOrCreateElement("MouseTooltip").Value = Chkbox_MouseTooltip.Checked.ToString();
            Engine.Config.Save("config.xml");
        }
    }
}
