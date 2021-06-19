﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ColorHueSaturationPicker : PictureBox
    {
        static readonly Color[] Colors = new Color[360 * 256];

        static readonly Texture2D ColorWheel = CreateTexture();

        bool Pressed;
        public Color Selected;
        Vector2 SelectedCoords;

        static Texture2D CreateTexture()
        {
            //for (int v = 0; v < 360; v++)
            //    for (int h = 0; h < 360; h++)
            //    {
            //        Color color = HSVtoRGB(h, 1f, 1 - v / 360f);
            //        Colors[v * 360 + h] = color;
            //    }
            //Texture2D texture = new Texture2D(Game1.Instance.GraphicsDevice, 360, 360);
            for (int v = 0; v < 256; v++)
                for (int h = 0; h < 360; h++)
                {
                    //Color color = HSVtoRGB(h, 1f, 1 - v / 256f);
                    Color color = HSVtoRGB(h, 1 - v / 256f, 1f);
                    Colors[v * 360 + h] = color;
                }
            Texture2D texture = new Texture2D(Game1.Instance.GraphicsDevice, 360, 256);
            texture.SetData<Color>(Colors);
            return texture;
        }

        public void SelectColor(Color rgb)
        {
            float h, s, v;
            h = s = v = 0;
            ColorHueSaturationPicker.RGBtoHSV(rgb, ref h, ref s, ref v);
            //Vector2 pos = new Vector2(h, 255 - v);
            Vector2 pos = new Vector2(h, (1-s) * 255);
            //Vector2 pos = new Vector2(h, v);
            this.Selected = Colors[(int)pos.Y * ColorWheel.Width + (int)pos.X];
            this.SelectedCoords = pos;
        }

        public Action<Color> Callback;

        public ColorHueSaturationPicker()
            : base(Vector2.Zero, ColorWheel, ColorWheel.Bounds)
        {
            this.Callback = c => { };
        }

        public override void Update()
        {
            base.Update();
            if (!this.Pressed)
                return;
            if (!this.MouseHover)
                return;
            Vector2 pos = UIManager.Mouse - this.ScreenLocation;
            var x = (int)Math.Max(0, Math.Min(ColorWheel.Width - 1, pos.X));
            var y = (int)Math.Max(0, Math.Min(ColorWheel.Height - 1, pos.Y));

            this.Selected = Colors[y * ColorWheel.Width + x];
            this.SelectedCoords = new Vector2(x,y);
            this.Callback(this.Selected);
        }

        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.MouseHover)
                return;
            //Vector2 pos = UIManager.Mouse - this.ScreenLocation;
            //this.Selected = Colors[(int)pos.Y * ColorWheel.Width + (int)pos.X];
            //this.SelectedCoords = pos;
            //this.Callback(this.Selected);
            this.Pressed = true;
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Pressed = false;
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (this.Selected == null)
                return;
            var rect = new Rectangle((int)this.ScreenLocation.X + (int)this.SelectedCoords.X - 1, (int)this.ScreenLocation.Y + (int)this.SelectedCoords.Y - 1, 3, 3);
            rect.DrawHighlight(sb, Color.Black, Vector2.Zero, 0);
        }

        //public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    this.TopLevelControl.Hide();
        //}

        static public void RGBtoHSV(Color rgb, ref float h, ref float s, ref float v)
        {
            float r = rgb.R, g = rgb.G, b = rgb.B;
            RGBtoHSV(r, g, b, ref h, ref s, ref v);
        }
        static public Color HSVtoRGB(float h, float s, float v)
        {
            float r, g, b;
            r = g = b = 0;
            HSVtoRGB(ref r, ref g, ref b, h, s, v);
            return new Color(r, g, b);
        }

        static public void RGBtoHSV(float r, float g, float b, ref float h, ref float s, ref float v)
        {
            float min, max, delta;
            min = Math.Min(r, Math.Min(g, b));
            max = Math.Max(r, Math.Max(g, b));
            v = max;               // v
            delta = max - min;
            if (max != 0)
                s = delta / max;       // s
            else
            {
                // r = g = b = 0        // s = 0, v is undefined
                s = 0;
                h = -1;
                return;
            }
            if (r == max)
                h = (g - b) / delta;     // between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta; // between cyan & yellow
            else
                h = 4 + (r - g) / delta; // between magenta & cyan
            h *= 60;               // degrees
            if (h < 0)
                h += 360;
        }
        static public void HSVtoRGB(ref float r, ref float g, ref float b, float h, float s, float v)
        {
            int i;
            float f, p, q, t;
            if (s == 0)
            {
                // achromatic (grey)
                r = g = b = v;
                return;
            }
            h /= 60;            // sector 0 to 5
            i = (int)Math.Floor(h);
            f = h - i;          // factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:        // case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
        }
    }
}
