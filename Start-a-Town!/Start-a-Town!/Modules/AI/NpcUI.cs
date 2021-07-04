using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.AI
{
    class NpcUI : GroupBox
    {
        public Actor Npc;
        readonly NeedsUI Needs;
        readonly Panel Buttons;
        readonly Panel PanelNeeds;
        readonly Panel PanelTask;
        readonly Panel PanelStats;
        readonly NpcLogUINew Log;
        readonly InventoryUI Inventory;
        public NpcUI(Actor npc)
        {
            this.PanelTask = new Panel() { AutoSize = true };
            var aistate = AIState.GetState(npc);
            var labeltask = new Label("Task: ") { TextFunc = () => aistate.ToString() };
            this.PanelTask.Controls.Add(labeltask);

            this.PanelNeeds = new Panel() { AutoSize = true, Location = this.PanelTask.BottomLeft };
            this.Npc = npc;
            this.Needs = new NeedsUI(npc);
            this.PanelNeeds.Controls.Add(this.Needs);

            this.Inventory = new InventoryUI(npc);
            var panelInv = new Panel() { AutoSize = true, Location = this.PanelNeeds.TopRight };
            panelInv.AddControls(this.Inventory);

            this.PanelStats = new Panel() { AutoSize = true , Location = panelInv.TopRight};
            var statsui = new StatsInterface(npc);
            this.PanelStats.AddControls(statsui);
            

            this.Buttons = new Panel() { AutoSize = true, Location = this.PanelNeeds.BottomLeft };
            this.Log = new NpcLogUINew(this.Npc);
            var winlog = this.Log.ToWindow(Npc.Name + "'s Log");
            var btnlog = new Button("Log", this.PanelNeeds.ClientSize.Width) { LeftClickAction = () => ShowLog(winlog) };

            var winPersonality = npc.Personality.GetUI().ToWindow(Npc.Name + "'s Personality");

            var btnpersonality = new Button("Personality", this.PanelNeeds.ClientSize.Width) { Location = btnlog.BottomLeft, LeftClickAction = () => winPersonality.ToggleSmart() };

            this.Buttons.AddControls(btnlog, btnpersonality);

            this.AddControls(this.PanelTask, this.PanelNeeds,
                this.PanelStats,
                panelInv,
                this.Buttons);

            GameMode.Current.OnUIEvent(UIManager.Events.NpcUICreated, this);
        }

        public void AddButton(Button btn)
        {
            btn.Width = this.PanelNeeds.ClientSize.Width;
            this.Buttons.AddControlsBottomLeft(btn);
        }

        private void ShowLog(Window winlog)
        {
            this.Log.Refresh();
            winlog.ToggleSmart();
        }
    }
}
