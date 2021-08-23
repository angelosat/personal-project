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
            this._tmpGuiScale = UIManager.Scale;
            this._tmpTooltipDelay = TooltipManager.DelayInterval / Ticks.TicksPerSecond;

            return new GroupBox() { Name = "Interface" }.AddControlsVertically(
                    SliderNew.CreateWithLabel("UI Scale", () => this._tmpGuiScale, v => this._tmpGuiScale = v, 100, 1, 2, 0.1f, "0%"),
                    SliderNew.CreateWithLabel("Tooltip Delay", () => this._tmpTooltipDelay, v => this._tmpTooltipDelay = v, 100, 0, 2, 0.1f, "0.0s"),
                    new CheckBoxNew("Mouse Tooltip", () => this._tmpMouseTooltip = !this._tmpMouseTooltip, () => this._tmpMouseTooltip) { HoverText = "Anchor tooltips to the mouse." },
                    new GroupBox().AddControlsHorizontally(new ButtonColorNew(() => UIManager.Tint, c => UIManager.Tint = c), new Label("UI tint primary")),
                    new GroupBox().AddControlsHorizontally(new ButtonColorNew(() => Panel.DefaultColor, c => Panel.DefaultColor = c), new Label("UI tint secondary"))
                ) as GroupBox;
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
