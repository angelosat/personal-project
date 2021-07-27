using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class InterfaceSettings : GameSettings
    {
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();
        float _tmpGuiScale, _tmpTooltipDelay;
        bool _tmpMouseTooltip;
        GroupBox CreateGui()
        {
            var box = new GroupBox() { Name = "Interface" };
            this._tmpGuiScale = UIManager.Scale;
            var sldr_UIScale = SliderNew.CreateWithLabel("UI Scale", () => this._tmpGuiScale, v => this._tmpGuiScale = v, 100, 1, 2, 0.1f, "0%");

            this._tmpTooltipDelay = TooltipManager.DelayInterval / Engine.TicksPerSecond;
            var sldr_TooltipDelay = SliderNew.CreateWithLabel("Tooltip Delay", () => this._tmpTooltipDelay, v => this._tmpTooltipDelay = v, 100, 0, 2, 0.1f, "0.0s");

            var chkbox_MouseTooltip = new CheckBoxNew("Mouse Tooltip", () => this._tmpMouseTooltip = !this._tmpMouseTooltip, () => this._tmpMouseTooltip);
            chkbox_MouseTooltip.HoverText = "Anchor tooltips to the mouse.";

            box.AddControlsVertically(
                sldr_UIScale, sldr_TooltipDelay, chkbox_MouseTooltip);
            return box;
        }

        internal override void Apply()
        {
            UIManager.Scale = this._tmpGuiScale;
            TooltipManager.MouseTooltips = this._tmpMouseTooltip;
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface").GetOrCreateElement("Scale").Value = this._tmpGuiScale.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface").GetOrCreateElement("MouseTooltip").Value = this._tmpMouseTooltip.ToString();
        }
        internal override void Cancel()
        {
        }
    }
}
