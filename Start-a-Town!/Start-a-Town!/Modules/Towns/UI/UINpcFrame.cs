using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UINpcFrame : ButtonBase
    {
        readonly GroupBox PictureBoxBox;
        readonly PictureBox Sprite;
        readonly Label Label;
        public UINpcFrame(Actor actor)
        {
            this.MouseThrough = false;
            var padding = 5;
            this.AutoSize = true;
            this.PictureBoxBox = new GroupBox() { AutoSize = false };

            this.Sprite = new PictureBox(actor.Body.RenderIcon(actor, 1)) { LocationFunc = () => this.PictureBoxBox.Center, Anchor = Vector2.One*.5f };
            this.Sprite.MouseThrough = true;
            this.PictureBoxBox.BackgroundColorFunc = () => Color.Lerp(Color.Red * .5f, Color.Lime * .5f, actor.GetMood() / 100f);
            this.PictureBoxBox.Size = new Rectangle(0, 0, this.Sprite.Width * 2 + padding, this.Sprite.Width * 2 + padding);

            this.PictureBoxBox.AddControls(this.Sprite);

            Tag = actor;
            LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                    SelectionManager.AddToSelection(new TargetArgs(actor));
                else
                    SelectionManager.Select(new TargetArgs(actor));
            };
            this.Label = new Label()
            {
                LocationFunc = () => 
                new Vector2(this.PictureBoxBox.Width / 2f, this.PictureBoxBox.Height),
                Anchor = new Vector2(.5f, .5f),
                TextFunc = () => actor.Name.Split(' ').First(),
            };
            this.AddControls(
                this.PictureBoxBox
                ,this.Label
                );
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if(SelectionManager.IsSelected(this.Tag as GameObject))
            {
                this.BoundsScreen.DrawHighlightBorder(sb, .5f, 1);
            }
        }
    }
}
