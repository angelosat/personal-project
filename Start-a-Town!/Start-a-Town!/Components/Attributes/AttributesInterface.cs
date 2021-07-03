using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Attributes
{
    class AttributesInterface : GroupBox
    {
        AttributesComponent Component;
        public AttributesInterface(AttributesComponent component)
        {
            this.Component = component;
            this.Refresh();
        }
        void Refresh()
        {
            this.Controls.Clear();
            var atts = this.Component.Attributes;
            foreach(var a in atts)
            {
                var lbl = new Label(a.ToString()) { Location = this.Controls.BottomLeft };
                lbl.Tag = a;
                lbl.TooltipFunc = tip =>
                {
                    var tiplbl = new Label("Next: " + a.Progress.ToString()) { Location = tip.Controls.BottomLeft };
                    tip.Controls.Add(tiplbl);
                    tip.Controls.Add(new Bar(a.Progress) { Location = tip.Controls.BottomLeft });
                    tip.Controls.Add(new Bar(a.Rec) { Location = tip.Controls.BottomLeft, Height = 2 });
                    tiplbl.TextFunc = () => "Next: " + a.Progress.ToString();
                    tip.OnGameEventAction = e =>
                    {
                        switch (e.Type)
                        {
                            case Message.Types.AttributeProgressChanged:
                                var att = e.Parameters[0] as AttributeDef;
                                if (a.Def == att)
                                    tiplbl.Invalidate();
                                break;

                            default:
                                break;
                        }
                    };
                };
                this.Controls.Add(lbl);
            }
        }
        void Refresh(AttributeDef a)
        {
            var lbl = (from ctrl in this.Controls where ctrl is Label where ctrl.Tag == a select ctrl).FirstOrDefault() as Label;
            if (lbl == null)
                return;
            lbl.Text = a.ToString();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.AttributeChanged:
                    var a = e.Parameters[0] as AttributeDef;
                    this.Refresh(a);
                    break;

                default:
                    break;
            }
        }
    }
}
