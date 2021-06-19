using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Resources;

namespace Start_a_Town_.UI
{
    public class UnitFrame : GroupBox
    {
        Panel Panel_Picture, Panel_Bars;
        Label Label_Name;
        PictureBox Picture;
        Bar Bar_Health, Bar_Stamina, Bar_StaminaRec;
        GameObject Character;
        Health Health;
        Stamina Stamina;

        public UnitFrame()
        {
            AutoSize = true;
            Panel_Picture = new Panel();
            Panel_Picture.AutoSize = true;
            Panel_Bars = new Panel();
            Panel_Bars.AutoSize = true;
        }

        public override void Update()
        {
            //this.Picture.Sprite = this.Character.Body.RenderNew(this.Character);
            this.Picture.Invalidate(); // repaint unitframes each frame for 2 reasons: 1) for some reason the player's texture isn't rendered at the beginning of game, 2) to have them animated
            base.Update();
        }

        public UnitFrame Track(GameObject obj)
        {
            this.Character = obj;
            this.Controls.Clear();
            this.Panel_Picture.Controls.Clear();

            //Sprite npcSprite = obj.Body["Head"].Sprite;
            //this.Picture = new PictureBox(Vector2.Zero, npcSprite.Texture, npcSprite.AtlasToken.Rectangle);
            Sprite npcSprite = obj.GetSprite();
            //this.Picture = new PictureBox(Vector2.Zero, npcSprite.Texture, npcSprite.AtlasToken.Rectangle);
            //this.Picture.DrawAction = () =>
            //{
            //    this.Picture.Texture = obj.Body.RenderNew(obj);// Graphics.Bone.Render(obj);
            //};

            //this.Picture = new PictureBox(Vector2.Zero, obj.Body.RenderNew(obj), null) { Renderer = (r) => obj.Body.RenderNewer(obj,r) }; //.RenderNew(obj)
            //this.Panel_Picture.Controls.Add(Picture);
            this.Picture = new PictureBox(new Vector2(32, 48), (r) => obj.Body.RenderNewer(obj, r));// { Renderer = (r) => obj.Body.RenderNewer(obj, r) }; //.RenderNew(obj)
            this.Panel_Picture.Controls.Add(Picture);

            this.Label_Name = new Label(Panel_Picture.TopRight, obj.Name);
            this.Panel_Bars.Controls.Clear();

            //this.Bar_Health = new Bar() { Color = Color.Orange };
            //this.Bar_Stamina = new Bar() { Color = Color.Yellow, Location = this.Bar_Health.BottomLeft };

            //var resources = obj.GetComponent<ResourcesComponent>().Resources;
            //this.Health = resources[Resource.Types.Health] as Health;
            //this.Stamina = resources[Resource.Types.Stamina] as Stamina;
            ////this.Bar_Health.PercFunc = () => this.Health.Percentage;
            ////this.Bar_Stamina.PercFunc = () => this.Stamina.Percentage;
            //this.Bar_Health.Object = this.Health;
            //this.Bar_Stamina.Object = this.Stamina;
            //this.Bar_StaminaRec = new Bar() { Object = this.Stamina.Rec, Location = this.Bar_Stamina.BottomLeft, Height = 2 };

            //this.Panel_Bars.Controls.Add(Bar_Health, this.Bar_Stamina, this.Bar_StaminaRec);

            foreach (var r in obj.GetComponent<ResourcesComponent>().Resources.Values)
            {
                var ui = r.GetControl();
                if (ui == null)
                    continue;
                ui.Location = this.Panel_Bars.Controls.BottomLeft;
                this.Panel_Bars.Controls.Add(ui);
            }

            this.Panel_Bars.Location = Label_Name.BottomLeft;
            this.Controls.Add(Label_Name,
                Panel_Picture, 
                Panel_Bars
                //,
                //Picture
                );

            this.Invalidate(true);//
            return this;
        }
    }

}
