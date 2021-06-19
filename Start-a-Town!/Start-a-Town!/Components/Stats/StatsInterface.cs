using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class StatsInterface : GroupBox
    {
        PanelLabeledNew PanelAttributes;
        PanelLabeledNew PanelStats;
        public StatsInterface()
        {
            this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };

        }
        public StatsInterface(GameObject actor)
        {
            throw new Exception();
            var comp = actor.GetComponent<StatsComponentNew>();
            var stats = comp.Stats;
            foreach (var stat in stats)
            {
                this.AddControlsBottomLeft(stat.Value.GetUI());
            }

            var attsComp = actor.GetComponent<AttributesComponent>();
            //this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            //attsComp.GetInterface(actor, this.PanelAttributes.Client);
            this.PanelAttributes = attsComp.GetGUI().ToPanelLabeled("Attributes");
            this.AddControlsTopRight(this.PanelAttributes);

            //this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
            //actor.GetComponent<StatsComponentNew>().GetInterface(actor, this.PanelStats.Client);
            this.PanelStats = actor.GetComponent<StatsComponentNew>().GetGUI().ToPanelLabeled("Stats");

            this.AddControlsBottomLeft(this.PanelStats);
        }
        public void Refresh(GameObject actor)
        {
            var comp = actor.GetComponent<StatsComponentNew>();
            var stats = comp.Stats;
            this.ClearControls();
            foreach (var stat in stats)
            {
                this.AddControlsBottomLeft(stat.Value.GetUI());
            }

            this.PanelAttributes.Client.ClearControls();
            actor.GetComponent<AttributesComponent>().GetInterface(actor, this.PanelAttributes.Client);
            this.AddControlsTopRight(this.PanelAttributes);

            this.PanelStats.Client.ClearControls();
            actor.GetComponent<StatsComponentNew>().GetInterface(actor, this.PanelStats.Client);
            this.AddControlsBottomLeft(this.PanelStats);
            this.Validate(true);
        }
        static StatsInterface Instance;
        internal static Window GetUI(Actor actor)
        {
            Window window;

            if (Instance == null)
            {
                Instance = new StatsInterface();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} needs", actor.Name);
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (!actor?.Equals(this.Tag) ?? false)
            {
                GetUI(actor);
            }
        }
    }
}
