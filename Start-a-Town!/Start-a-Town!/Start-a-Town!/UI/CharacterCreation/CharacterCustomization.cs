using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class CharacterCustomization : PanelLabeled
    {
        //Color Color;
        //ColorPicker Picker;
        ColorPickerWindow Picker;
        //Picker ColorPick;
        public CharacterColors Colors;
        //PanelLabeled PanelPicker;
        RandomizeButton BtnRandomize;

        public CharacterCustomization()
            : base("Customize")
        {
            this.AutoSize = true;
            this.BtnRandomize = new RandomizeButton()
            {
                Location = this.Label.TopRight,
                LeftClickAction = () => { Randomize(this.Colors); Refresh(); }
            };
            this.Controls.Add(this.BtnRandomize);

            //this.Colors = new CharacterColors(
            //    new CharacterColor("Hair", "Head:Hair"),
            //    new CharacterColor("Skin", "Head", "Right Hand", "Left Hand"),
            //    new CharacterColor("Pants", "Right Foot", "Left Foot"),
            //    new CharacterColor("Shoes", "Right Foot:Shoe", "Left Foot:Shoe"),
            //    new CharacterColor("Shirt", "", "Right Hand:Shirt", "Left Hand:Shirt"));
            this.Colors = new CharacterColors(
               new CharacterColor("Hair", "Hair"),
               new CharacterColor("Skin", "Skin"),
               new CharacterColor("Pants", "Pants"),
               new CharacterColor("Shoes", "Shoes"),
               new CharacterColor("Shirt", "Shirt"));
            Randomize(this.Colors);
            //this.PanelPicker = new PanelLabeled("untitled") { AutoSize = true };
            //this.Picker = new ColorPicker() { Location = this.PanelPicker.Controls.BottomLeft };
            this.Picker = new ColorPickerWindow() { Layer = LayerTypes.Dialog };
            this.Picker.Location = this.Picker.CenterScreen;

            //this.ColorPick = new UI.Picker() { Location = this.PanelPicker.Controls.BottomLeft };
            //this.PanelPicker.Controls.Add(this.ColorPick);
            //this.PanelPicker.Layer = LayerTypes.Dialog;
            //this.PanelPicker.Location = this.PanelPicker.CenterScreen;

            foreach (var item in this.Colors.Colors.Values)
            {
                Label btn = new Label(item.Name) { Location = this.Controls.BottomLeft };//, LeftClickAction = () => this.TogglePicker(item.Key) };
                btn.Tag = item;
                this.Controls.Add(btn);

                //ColoredRectangle rect = new ColoredRectangle(10, 10) { Location = btn.CenterRight + new Vector2(10, 0), Anchor = new Vector2(0.5f), Tint = Color.White };
                //rect.Tag = btn.Tag;
                //rect.Tint = (rect.Tag as CharacterColor).Color;

                ButtonColor btnColor = new ButtonColor() { Location = btn.CenterRight, Anchor = Vector2.UnitY * 0.5f };
                btnColor.SelectedColor = item.Color;
                btnColor.Tag = btn.Tag;
                //btn.Tag = rect;
                btn.LeftClickAction = () =>
                {
                    //this.PanelPicker.Label.Text = btn.Text;
                    this.Picker.Title = btn.Text;
                    //this.ColorPick.SelectColor(btnColor.SelectedColor);
                    this.Picker.SelectColor(btnColor.SelectedColor);
                    this.Picker.Callback = c =>
                    //this.ColorPick.Callback = c =>
                    {
                        (btn.Tag as CharacterColor).Color = c;
                        btnColor.SelectedColor = c;
                        //rect.Tint = c;
                        //this.PanelPicker.Hide(); 
                    };
                    //this.PanelPicker.Toggle();
                    this.Picker.Toggle();
                };
                btnColor.LeftClickAction = btn.LeftClickAction;
                this.Controls.Add(btnColor); //rect, 
            }

            //int btnwidth = this.Colors.Colors.Keys.MaxWidth();
            //int maxw = 0;
            //foreach (var item in this.Colors.Colors.Values)
            //{
            //    Button btn = new Button(item.Name) { Location = this.Controls.BottomLeft };//, LeftClickAction = () => this.TogglePicker(item.Key) };
            //    btn.Tag = item;
            //    maxw = Math.Max(maxw, btn.Width);
            //    this.Controls.Add(btn);
            //}
            //foreach (var btn in (from ctrl in this.Controls where ctrl is Button select ctrl as Button).ToList())// this.Controls.Where(c=>c is Button).ToList())
            //{
            //    btn.Width = maxw;
            //    ColoredRectangle rect = new ColoredRectangle(10, 10) { Location = btn.CenterRight + new Vector2(10, 0), Anchor = new Vector2(0.5f), Tint = Color.White };
            //    rect.Tag = btn.Tag;
            //    rect.Tint = (rect.Tag as CharacterColor).Color;

            //    ButtonColor btnColor = new ButtonColor() { Location = btn.CenterRight, Anchor = Vector2.UnitY * 0.5f };
            //    btnColor.SelectedColor = (rect.Tag as CharacterColor).Color;
            //    btnColor.Tag = btn.Tag;
            //    //btn.Tag = rect;
            //    btn.LeftClickAction = () =>
            //    {
            //        this.PanelPicker.Label.Text = btn.Text;
            //        this.ColorPick.SelectColor(btnColor.SelectedColor);
            //        //this.Picker.Callback = c =>
            //        this.ColorPick.Callback = c =>
            //        {
            //            (btn.Tag as CharacterColor).Color = c;
            //            rect.Tint = c;
            //            //this.PanelPicker.Hide(); 
            //        };
            //        this.PanelPicker.Toggle();
            //    };
            //    btnColor.LeftClickAction = btn.LeftClickAction;
            //    this.Controls.Add(btnColor); //rect, 
            //}
        }

        static public void Randomize(CharacterColors colors)
        {
            Random rand = new Random();
            foreach (var item in colors.Colors.Values)
            {
                Color c = new Color();
                c.R = (byte)rand.Next(256);
                c.G = (byte)rand.Next(256);
                c.B = (byte)rand.Next(256);
                c.A = 255;
                item.Color = c;
            }
        }

        void Refresh()
        {
            //foreach (var rect in (from ctrl in this.Controls where ctrl is ColoredRectangle select ctrl as ColoredRectangle).ToList())
            //{
            //    rect.Tint = (rect.Tag as CharacterColor).Color;
            //}
            foreach (var rect in (from ctrl in this.Controls where ctrl is ButtonColor select ctrl as ButtonColor).ToList())
            {
                rect.SelectedColor = (rect.Tag as CharacterColor).Color;
            }
        }

        //void TogglePicker(string part)
        //{
        //    //this.Picker.Toggle();
        //    //this.Picker.Location = this.Picker.CenterScreen;
        //    this.PanelPicker.Label.Text = part;
        //    this.PanelPicker.Layer = LayerTypes.Dialog;
        //    this.Picker.Callback = c => { this.PanelPicker.Hide(); };
        //    this.PanelPicker.Toggle();
        //}
        //void TogglePicker(string part, Action<Color> callback)
        //{
        //    //this.Picker.Toggle();
        //    //this.Picker.Location = this.Picker.CenterScreen;
        //    this.PanelPicker.Label.Text = part;
        //    this.PanelPicker.Layer = LayerTypes.Dialog;
        //    this.Picker.Callback = callback;// c => { this.PanelPicker.Hide(); };
        //    this.PanelPicker.Toggle();
        //}
    }
}
