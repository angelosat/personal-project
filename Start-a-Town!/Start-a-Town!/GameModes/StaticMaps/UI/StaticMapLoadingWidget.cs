using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes.StaticMaps.UI
{
    class StaticMapLoadingWidget : GroupBox
    {
        Label LblLoading, LblInfo, LblPressStart;
        Panel PanelBar;
        Bar ProgressBar;
        Progress MapLoadProgress;
        public StaticMapLoadingWidget()
        {
            var w = (int)(UIManager.Width * 0.9f);
            this.MapLoadProgress = new Progress();
            this.LblLoading = new Label("Loading Map...") { Location = new Vector2((int)(w * 0.5f), 0)};
            this.LblLoading.Anchor = Vector2.UnitX * 0.5f;
            this.LblInfo = new Label("asdasdasdas") { Location = this.LblLoading.BottomLeft };
            this.PanelBar = new Panel() { AutoSize = true };
            this.ProgressBar = new Bar(this.MapLoadProgress) { Width = w};
            this.PanelBar.Controls.Add(this.ProgressBar);
            this.PanelBar.Location = new Vector2(0, this.LblInfo.Bottom);
            this.LblInfo.Location = new Vector2(this.PanelBar.Location.X + this.PanelBar.Width / 2, this.PanelBar.Location.Y);
            this.LblInfo.Anchor = new Vector2(0.5f, 1);
            this.LblPressStart = new Label("Press any key!") { Location = this.PanelBar.Location + Vector2.UnitX * this.PanelBar.Width / 2 };
            this.LblPressStart.Anchor = Vector2.UnitX * 0.5f;

            this.Controls.Add(this.LblLoading, this.LblInfo, this.PanelBar);
        }
        
        public void Refresh(StaticMapLoadingProgressToken token)
        {
            var text = token.Text;
            var perc = token.Percentage;
            this.LblInfo.Text = text;
            this.LblInfo.Location = new Vector2(this.PanelBar.Location.X + this.PanelBar.Width / 2, this.PanelBar.Location.Y);
            this.LblInfo.Anchor = new Vector2(0.5f, 1);
            this.MapLoadProgress.Percentage = perc;

            if (perc < 1)
                return;
            this.Controls.Remove(PanelBar);
            this.Controls.Add(this.LblPressStart);

        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb);
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
