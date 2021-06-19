using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI
{
    class MapPopUp : PanelLabeled
    {
        MapThumb MapThumb;
        Camera Camera;
        public Button BtnEnter;
        Action<IMap> Callback;

        public MapPopUp(Action<IMap> callback)
            : base("FUCK")
        {
            this.AutoSize = true;
            this.Callback = callback;
            BtnEnter = new Button("Enter Map") { Location = this.Controls.BottomLeft, LeftClickAction = () => this.Callback(this.MapThumb.Map) };
            this.Controls.Add(this.BtnEnter);
        }

        public override void Update()
        {
            //this.Location = this.Camera.GetScreenBounds(this.MapThumb.Global, this.MapThumb.Sprites[MapThumb.CurrentZoom].SourceRects.First().First());
            var bounds = this.Camera.GetScreenBounds(this.MapThumb.Global, this.MapThumb.Sprites[MapThumb.CurrentZoom].SourceRects.First().First());
            //this.Location = new Vector2(bounds.X + bounds.Width / 2 - this.Width / 2, bounds.Y - this.Height);
            this.Location = new Vector2(bounds.X - this.Width / 2, bounds.Y - this.Height);

            base.Update();
        }

        public void Show(MapThumb thumb, Camera camera)
        {
            base.Show();
            this.MapThumb = thumb;
            this.Camera = camera;
            this.Label.Text = thumb.Map.GetOffset().ToString();

            var bounds = this.Camera.GetScreenBounds(this.MapThumb.Global, this.MapThumb.Sprites[MapThumb.CurrentZoom].SourceRects.First().First());
            this.Location = new Vector2(bounds.X - this.Width / 2, bounds.Y - this.Height);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
