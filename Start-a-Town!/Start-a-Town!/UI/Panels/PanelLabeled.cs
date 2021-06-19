using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class PanelLabeled : Panel
    {
        public Label Label { get; set; }
        public PanelLabeled(string label)
        {
            this.AutoSize = true;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Controls.Add(this.Label);
        }
        public PanelLabeled(string label, int width, int height)
            : base(Vector2.Zero, new Vector2(width, height))
        {
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Controls.Add(this.Label);
        }
        protected PanelLabeled() { }
    }
}
