using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class InterfaceSettings : GameSettings
    {
        CheckBox Chkbox_MouseTooltip;
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();
        float newGuiScale, newTooltipDelay;

        GroupBox CreateGui()
        {
            var box = new GroupBox() { Name = "Interface" };
            this.newGuiScale = UIManager.Scale;
            var sldr_UIScale = SliderNew.CreateWithLabel("UI Scale", () => this.newGuiScale, v => this.newGuiScale = v, 100, 1, 2, 0.1f, "0%");

            this.newTooltipDelay = TooltipManager.DelayInterval / Engine.TicksPerSecond;
            //var sldr_TooltipDelay = new SliderNew(() => this.newTooltipDelay, v => this.newTooltipDelay = v, 100, 0, 2, 0.1f) { Name = "Tooltip Delay: {0}s" };
            var sldr_TooltipDelay = SliderNew.CreateWithLabel("Tooltip Delay", () => this.newTooltipDelay, v => this.newTooltipDelay = v, 100, 0, 2, 0.1f, "0.0s");

            this.Chkbox_MouseTooltip = new CheckBox("Mouse Tooltip");
            this.Chkbox_MouseTooltip.Checked = TooltipManager.MouseTooltips;
            this.Chkbox_MouseTooltip.HoverText = "Anchor tooltips to the mouse.";

            box.AddControlsVertically(
                //Lbl_UIScale, 
                sldr_UIScale, sldr_TooltipDelay,Chkbox_MouseTooltip);
            return box;
        }

        internal override void Apply()
        {
            UIManager.Scale = newGuiScale;
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface").GetOrCreateElement("Scale").Value = UIManager.Scale.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface").GetOrCreateElement("MouseTooltip").Value = Chkbox_MouseTooltip.Checked.ToString();
            Engine.Config.Save("config.xml");
        }
    }
}
