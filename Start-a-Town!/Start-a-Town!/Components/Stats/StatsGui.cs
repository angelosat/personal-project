using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class StatsGui : GroupBox
    {
        readonly PanelLabeledNew PanelAttributes;
        readonly PanelLabeledNew PanelStats;
        public StatsGui()
        {
            this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
        }
        public StatsGui(GameObject actor)
        {
            var comp = actor.GetComponent<StatsComponent>();
            var attsComp = actor.GetComponent<AttributesComponent>();
            this.PanelAttributes = attsComp.GetGUI().ToPanelLabeled("Attributes");
            this.AddControlsTopRight(this.PanelAttributes);
            this.PanelStats = comp.GetGUI().ToPanelLabeled("Stats");
            this.AddControlsBottomLeft(this.PanelStats);
        }
        public void Refresh(GameObject actor)
        {
            var comp = actor.GetComponent<StatsComponent>();
            this.ClearControls();
          
            this.PanelAttributes.Client.ClearControls();
            actor.GetComponent<AttributesComponent>().GetInterface(actor, this.PanelAttributes.Client);
            this.AddControlsTopRight(this.PanelAttributes);

            this.PanelStats.Client.ClearControls();
            comp.GetInterface(actor, this.PanelStats.Client);
            this.AddControlsBottomLeft(this.PanelStats);
            this.Validate(true);
        }
        static StatsGui Instance;
        internal static Window GetGui(Actor actor)
        {
            Window window;

            if (Instance is null)
            {
                Instance = new StatsGui();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = $"{actor.Name} needs";
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (!actor?.Equals(this.Tag) ?? false)
                GetGui(actor);
        }
    }
}
