using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class TerrainWindowNew : Window
    {
        readonly BlockBrowserNewNew Browser;

        public TerrainWindowNew()
        {
            this.Title = "Constructions Browser";
            this.AutoSize = true;
            this.Movable = true;
            this.Browser = new BlockBrowserNewNew();
            this.Client.Controls.Add(this.Browser);
        }
        public override bool Hide()
        {
            this.Browser.Hide();
            return base.Hide();
        }
    }
}
