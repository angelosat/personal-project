using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ColorPickerWindow : Window
    {
        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.TopLevelControl.Hide();
        }

        ColorPicker ColorPick;
        LightnessPicker LightnessPick;

        public Action<Color> Callback = c => { };
        Button BtnOk, BtnCancel;
        Panel PanelButtons, PanelHS, PanelV;

        public ColorPickerWindow()//Action<Color> callback)
        {
            this.Movable = false;
            this.AutoSize = true;
            this.PanelHS = new Panel() { AutoSize = true };

            this.ColorPick = new ColorPicker() { Callback = c => this.LightnessPick.Tint = c };
            this.PanelHS.Controls.Add(this.ColorPick);

            this.PanelV = new Panel() { AutoSize = true, Location = this.PanelHS.TopRight };
            this.LightnessPick = new LightnessPicker();
            this.PanelV.Controls.Add(this.LightnessPick);

            this.Client.Controls.Add(this.PanelHS, this.PanelV);

            this.BtnOk = new Button("Ok") { LeftClickAction = () => { this.Callback(this.LightnessPick.Selected.Multiply(this.ColorPick.Selected)); this.TopLevelControl.Hide(); } };
            this.BtnCancel = new Button("Cancel") { LeftClickAction = () => { this.TopLevelControl.Hide(); }, Location = this.BtnOk.TopRight };
            //  this.Callback = callback;
            this.PanelButtons = new Panel() { AutoSize = true, Location = this.PanelHS.BottomLeft };
            this.PanelButtons.Controls.Add(this.BtnOk);//, this.BtnCancel);
            this.Client.Controls.Add(this.PanelButtons);
            this.Location = this.CenterScreen;
        }

        public void SelectColor(Color rgb)
        {
            this.ColorPick.SelectColor(rgb);
            this.LightnessPick.SelectColor(rgb);
            this.LightnessPick.Tint = this.ColorPick.Selected;
        }
    }
}
