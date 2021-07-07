using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UICharacterCustomization : PanelLabeledNew
    {
        ColorPickerWindow Picker;
        PictureBox BodyFrame;
        Actor Actor;
        public CharacterColors Colors;
        IconButton BtnRandomize;
        public UICharacterCustomization(Actor actor)
            : base("Customize")
        {
            this.AutoSize = true;
            this.BtnRandomize = IconButton.CreateRandomizeButton();
            this.BtnRandomize.Location = this.Label.TopRight;
            this.BtnRandomize.LeftClickAction = () => { Randomize(this.Colors); Refresh(); };
            this.Controls.Add(this.BtnRandomize);

            this.Colors = new CharacterColors(actor.Sprite.Customization);

            this.Actor = actor;
            this.BodyFrame = new PictureBox(actor, 2);

            this.Client.AddControlsBottomLeft(this.BodyFrame.ToPanel());

            var picker = new ColorPickerBox();
            var pickerPanel = picker.ToPanelLabeled("");

            var table = new TableScrollableCompactNew<CharacterColor>(this.Colors.Colors.Count) { ClientBoxColor = Color.Transparent }
                .AddColumn(null, "Name", 50, c => new Label(c.Name), 0)
                .AddColumn(null, "Color", 16, cc => {
                    var btn = new ButtonColor();
                    btn.SelectedColor = cc.Color;
                    btn.Tag = cc;
                    btn.LeftClickAction = () =>
                    {
                        pickerPanel.Label.Text = string.Format("Choose {0} color", cc.Name);
                        var oldColor = cc.Color;
                        picker.CancelAction = () => cc.Color = oldColor;
                        picker.SelectColor(btn.SelectedColor);
                        picker.Callback = c =>
                        {
                            (btn.Tag as CharacterColor).Color = c;
                            btn.SelectedColor = c;
                        };
                        picker.ColorChangedFunc = c => cc.Color = c;
                        if(!pickerPanel.IsOpen)
                            pickerPanel.SetLocation(btn.ScreenLocation + Vector2.UnitX * btn.Width).Show();
                    };
                    return btn;
                }, 0);
            table.Build(this.Colors.Colors.Values);
            this.Client.AddControlsTopRight(table);
        }
        public override void Update()
        {
            base.Update();
            this.Actor.Body.RenderIcon(this.Actor, this.BodyFrame.Texture, this.Colors);
        }
        public UICharacterCustomization()
            : base("Customize")
        {
            this.AutoSize = true;
            this.BtnRandomize = new RandomizeButton()
            {
                Location = this.Label.TopRight,
                LeftClickAction = () => { Randomize(this.Colors); Refresh(); }
            };
            this.Controls.Add(this.BtnRandomize);

            this.Colors = new CharacterColors(
               new CharacterColor("Hair", "Hair"),
               new CharacterColor("Skin", "Skin"),
               new CharacterColor("Pants", "Pants"),
               new CharacterColor("Shoes", "Shoes"),
               new CharacterColor("Shirt", "Shirt"));
            Randomize(this.Colors);
            this.Picker = new ColorPickerWindow() { Layer = LayerTypes.Dialog };
            this.Picker.SnapToScreenCenter();

            foreach (var item in this.Colors.Colors.Values)
            {
                Label btn = new Label(item.Name) { Location = this.Controls.BottomLeft };
                btn.Tag = item;
                this.Controls.Add(btn);

                ButtonColor btnColor = new ButtonColor() { Location = btn.CenterRight, Anchor = Vector2.UnitY * 0.5f };
                btnColor.SelectedColor = item.Color;
                btnColor.Tag = btn.Tag;
                btn.LeftClickAction = () =>
                {
                    this.Picker.Title = btn.Text;
                    this.Picker.SelectColor(btnColor.SelectedColor);
                    this.Picker.Callback = c =>
                    {
                        (btn.Tag as CharacterColor).Color = c;
                        btnColor.SelectedColor = c;
                    };
                    this.Picker.Toggle();
                };
                btnColor.LeftClickAction = btn.LeftClickAction;
                this.Controls.Add(btnColor);
            }
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
            foreach (var rect in (from ctrl in this.Controls where ctrl is ButtonColor select ctrl as ButtonColor).ToList())
            {
                rect.SelectedColor = (rect.Tag as CharacterColor).Color;
            }
        }
    }
}
