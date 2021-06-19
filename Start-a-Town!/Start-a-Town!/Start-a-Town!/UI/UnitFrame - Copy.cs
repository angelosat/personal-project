using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class UnitFrame : Panel
    {
        Panel Panel_Picture, Panel_Health;
        Label Label_Name;
        PictureBox Picture;
        Bar Health;

        public UnitFrame(Vector2 loc)
            : this()
        {
            this.Location = loc;
        }

        public UnitFrame()
        {
            AutoSize = true;
            Panel_Picture = new Panel();
            Panel_Picture.AutoSize = true;
            Panel_Health = new Panel();
            Panel_Health.AutoSize = true;
        }

        public UnitFrame Initialize(GameObject obj)
        {
            Controls.Clear();
            //Panel = new Panel();
            //Panel.AutoSize = true;

            Panel_Picture.Controls.Clear();
            Sprite npcSprite = obj["Sprite"]["Sprite"] as Sprite;
            Picture = new PictureBox(Vector2.Zero, npcSprite.Texture, npcSprite.SourceRects[(int)obj["Sprite"]["Variation"]][(int)obj["Sprite"]["Orientation"]]);
            Panel_Picture.Controls.Add(Picture);
            Label_Name = new Label(Panel_Picture.TopRight, obj.Name);

            //   HealthComponent healthComp = obj["Health"] as HealthComponent;
            Panel_Health.Controls.Clear();
            Health = new Bar() { Color = Color.Orange };
            Tag = obj;
            Panel_Health.Controls.Add(Health);
            //  Panel_Health.Location = Panel_Picture.TopRight;
            Panel_Health.Location = Label_Name.BottomLeft;
            //Panel.Controls.Add(Panel_Picture, Panel_Health);
            //Controls.Add(Panel);
            Controls.Add(Label_Name, Panel_Picture, Panel_Health);
            return this;
        }

        public override void Update()
        {
            base.Update();
            if (Tag == null)
                return;
            //HealthComponent healthComp = (Tag as GameObject)["Health"] as HealthComponent;
            //Health.Percentage = healthComp.Value / (float)healthComp.Max;

        }
    }

}
