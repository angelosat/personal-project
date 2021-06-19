using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    //class Picker : GroupBox
    //{
    //    public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
    //    {
    //        this.TopLevelControl.Hide();
    //    }

    //    ColorHueSaturationPicker ColorPick;
    //    ColorLightnessPicker LightnessPick;
    //    public Action<Color> Callback = c => { };
    //    Button BtnOk, BtnCancel;

    //    public Picker()//Action<Color> callback)
    //    {
    //        this.ColorPick = new ColorHueSaturationPicker() { Callback = c => this.LightnessPick.Tint = c };
    //        this.LightnessPick = new ColorLightnessPicker() { Location = this.ColorPick.TopRight };
    //        this.Controls.Add(this.ColorPick, this.LightnessPick);
    //        this.BtnOk = new Button("Ok") { LeftClickAction = () => { this.Callback(this.LightnessPick.Selected.Multiply(this.ColorPick.Selected)); this.TopLevelControl.Hide(); }, Location = this.ColorPick.BottomLeft };
    //        this.BtnCancel = new Button("Cancel") { LeftClickAction = () => { this.TopLevelControl.Hide(); }, Location = this.BtnOk.TopRight };
    //      //  this.Callback = callback;
    //        this.Controls.Add(this.BtnOk, this.BtnCancel);
    //    }

    //    public void SelectColor(Color rgb)
    //    {
    //        this.ColorPick.SelectColor(rgb);
    //        this.LightnessPick.SelectColor(rgb);
    //        this.LightnessPick.Tint = this.ColorPick.Selected;
    //    }
    //}

    class ColorLightnessPicker : PictureBox
    {
        static readonly int TexWidth = 20;
        static readonly Color[] Gradient = new Color[TexWidth * 256];
        static readonly Texture2D GradientTexture = CreateGradient();
        public Color Selected;
        bool Pressed;
        Vector2 SelectedCoords;
        static Texture2D CreateGradient()
        {
            for (int v = 0; v < 256; v++)
                for (int x = 0; x < TexWidth; x++)
                {
                    float val = 1 - v / 255f;
                    //Color color = new Color(val, val, val, 255);
                    Color color = ColorHueSaturationPicker.HSVtoRGB(0, 0, val);
                    Gradient[v * TexWidth + x] = color;
                }
            Texture2D texture = new Texture2D(Game1.Instance.GraphicsDevice, TexWidth, 256);
            texture.SetData<Color>(Gradient);
            return texture;
        }
        public Action<Color> Callback;

        public ColorLightnessPicker()
            : base(Vector2.Zero, GradientTexture, GradientTexture.Bounds)
        {
            this.Callback = c => { };
        }

        public void SelectColor(Color rgb)
        {
            float h, s, v;
            h = s = v = 0;
            ColorHueSaturationPicker.RGBtoHSV(rgb, ref h, ref s, ref v);
            Vector2 pos = new Vector2(0, 255-v);
            this.Selected = Gradient[(int)pos.Y * GradientTexture.Width + (int)pos.X];
            this.SelectedCoords = pos;
        }

        public override void Update()
        {
            base.Update();
            if (!this.Pressed)
                return;
            if (!this.MouseHover)
                return;
            Vector2 pos = UIManager.Mouse - this.ScreenLocation;
            var x = (int)Math.Max(0, Math.Min(GradientTexture.Width - 1, pos.X));
            var y = (int)Math.Max(0, Math.Min(GradientTexture.Height - 1, pos.Y));

            this.Selected = Gradient[y * GradientTexture.Width + x];
            this.SelectedCoords = new Vector2(x, y);// pos;
            this.Callback(this.Selected);
        }

        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.MouseHover)
                return;
            //Vector2 pos = UIManager.Mouse - this.ScreenLocation;
            //this.Selected = Gradient[(int)pos.Y * GradientTexture.Width + (int)pos.X];
            //this.SelectedCoords = pos;
            //this.Callback(this.Selected);
            this.Pressed = true;
            //this.TopLevelControl.Hide();
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Pressed = false;
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (this.Selected.IsNull())
                return;
            var rect = new Rectangle((int)this.ScreenLocation.X + (int)this.SelectedCoords.X - 1, (int)this.ScreenLocation.Y + (int)this.SelectedCoords.Y - 1, 3, 3);
            //rect.DrawHighlight(sb);
            rect.DrawHighlight(sb, Color.White, Vector2.Zero, 0);
        }
        //public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    this.TopLevelControl.Hide();
        //}
    }
}
