using Start_a_Town_.UI;
using Start_a_Town_.Towns.Constructions;

namespace Start_a_Town_
{
    public class TerrainWindowNew : Window
    {
        BlockBrowserNewNew Browser;

        public TerrainWindowNew(ConstructionCategory category)
        {
            this.Title = "Constructions Browser";
            this.AutoSize = true;
            this.Movable = true;
            this.Browser = new BlockBrowserNewNew(category);
            this.Client.Controls.Add(Browser);
            
        }
        public TerrainWindowNew()
        {
            this.Title = "Constructions Browser";
            this.AutoSize = true;
            this.Movable = true;
            this.Browser = new BlockBrowserNewNew();
            this.Client.Controls.Add(Browser);
        }
        public override bool Hide()
        {
            this.Browser.Hide();
            return base.Hide();
        }
    }
}
