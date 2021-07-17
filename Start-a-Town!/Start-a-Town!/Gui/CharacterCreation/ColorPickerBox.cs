using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ColorPickerBox : GroupBox
    {
        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;
            this.BtnCancel.LeftClickAction();
            e.Handled = true;
        }

        ColorHueSaturationPicker ColorPick;
        ColorLightnessPicker LightnessPick;
        public Action<Color> ColorChangedFunc = c => { };
        public Action<Color> Callback = c => { };
        Button BtnOk, BtnCancel;
        Panel PanelButtons, PanelHS, PanelV;
        internal Func<Color> CancelAction;

        public ColorPickerBox()
        {
            this.PanelHS = new Panel() { AutoSize = true };

            this.ColorPick = new ColorHueSaturationPicker()
            {
                Callback = c => {
                    this.ColorChangedFunc(this.CurrentColor);
                    this.LightnessPick.Tint = c;
                }
            };
            this.PanelHS.Controls.Add(this.ColorPick);

            this.PanelV = new Panel() { AutoSize = true, Location = this.PanelHS.TopRight };
            this.LightnessPick = new ColorLightnessPicker() { Callback = c => this.ColorChangedFunc(this.CurrentColor) };
            this.PanelV.Controls.Add(this.LightnessPick);

            this.Controls.Add(this.PanelHS, this.PanelV);
            this.BtnOk = new Button("Ok") { LeftClickAction = () => { this.Callback(this.LightnessPick.Selected.Multiply(this.ColorPick.Selected)); this.TopLevelControl.Hide(); } };
            this.BtnCancel = new Button("Cancel") { LeftClickAction = () => { this.CancelAction?.Invoke(); this.TopLevelControl.Hide(); } };
            this.PanelButtons = new Panel() { AutoSize = true, Location = this.PanelHS.BottomLeft };
            this.PanelButtons.AddControlsHorizontally(this.BtnOk, this.BtnCancel);
            this.Controls.Add(this.PanelButtons);
        }
        public Color CurrentColor => this.LightnessPick.Selected.Multiply(this.ColorPick.Selected);

        public void SelectColor(Color rgb)
        {
            this.ColorPick.SelectColor(rgb);
            this.LightnessPick.SelectColor(rgb);
            this.LightnessPick.Tint = this.ColorPick.Selected;
        }
    }
}
