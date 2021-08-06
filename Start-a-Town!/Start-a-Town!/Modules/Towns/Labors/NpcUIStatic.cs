using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    class NpcUIStatic : GroupBox
    {
        static NpcUIStatic _Instance;
        public static NpcUIStatic Instance => _Instance ??= new NpcUIStatic();

        public Actor Npc;
        NeedsUI Needs;
        Panel Buttons;
        Panel PanelNeeds;
        Panel PanelTask;
        Panel PanelStats;
        Panel PanelSkills;
        NpcLogUINew Log;
        InventoryUI Inventory;
        Panel PanelInventory;
        Label LabelTask;
        PersonalityUI PersonalityUI;
        StatsGui StatsUI;
        GroupBox SkillsUI;
        public NpcUIStatic()
        {
            this.PanelTask = new Panel() { AutoSize = true };
            this.LabelTask = new Label("Task: ");
            this.PanelTask.Controls.Add(this.LabelTask);

            this.PanelNeeds = new Panel() { AutoSize = true, Location = this.PanelTask.BottomLeft };
            this.Needs = new NeedsUI();
            this.PanelNeeds.Controls.Add(this.Needs);

            this.Inventory = new InventoryUI();
            this.PanelInventory = new Panel() { AutoSize = true, Location = this.PanelNeeds.TopRight };
            this.PanelInventory.AddControls(this.Inventory);

            this.PanelStats = new Panel() { AutoSize = true, Location = this.PanelInventory.TopRight };
            this.StatsUI = new StatsGui();
            this.PanelStats.AddControls(this.StatsUI);

            this.PanelSkills = new Panel() { AutoSize = true, Location = this.PanelStats.TopRight };
            this.SkillsUI = new GroupBox();
            this.PanelSkills.AddControls(this.SkillsUI);

            this.Buttons = new Panel() { AutoSize = true, Location = this.PanelNeeds.BottomLeft };
            this.Log = new NpcLogUINew();
            var btnlog = new Button("Log", this.PanelNeeds.ClientSize.Width);

            var btnpersonality = new Button("Personality", this.PanelNeeds.ClientSize.Width);
            this.PersonalityUI = new PersonalityUI();

            this.Buttons.AddControls(btnlog, btnpersonality);

            var paneltbs = new PanelTabs(400, 250);
            paneltbs
                .AddTab("Log", this.Log)
                .AddTab("Personality", this.PersonalityUI)
                .AddTab("Needs", this.Needs)
                .AddTab("Gear", this.Inventory)
                .AddTab("Stats", this.StatsUI)
                .AddTab("Skills", this.SkillsUI)
                ;
            this.AddControls(paneltbs);
        }
        public void Refresh(Actor npc)
        {
            this.Npc = npc;
            var aistate = AIState.GetState(npc);
            this.LabelTask.TextFunc = () => aistate.ToString();
            this.Log.Refresh(npc);
            this.Needs.Refresh(npc);
            this.Inventory.Refresh(npc);
            this.StatsUI.Refresh(npc);
            this.PersonalityUI.Refresh(npc);
            this.SkillsUI.ClearControls();
            this.SkillsUI.AddControls(NpcSkillsComponent.GetGUI(npc));

            this.PanelNeeds.Location = this.PanelTask.BottomLeft;
            this.PanelInventory.Location = this.PanelNeeds.TopRight;
            this.PanelStats.Location = this.PanelInventory.TopRight;
            this.PanelSkills.Location = this.PanelStats.TopRight;

            this.Buttons.Location = this.PanelNeeds.BottomLeft;
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.SelectedChanged:
                    var target = e.Parameters[0] as TargetArgs;
                    if(target.Type == TargetType.Entity &&
                        target.Object != this.Npc &&
                        target.Object.HasComponent<NpcComponent>())
                    {
                        this.Refresh(target.Object as Actor);
                        this.GetWindow().Title = target.Object.Name;
                    }

                    break;

                default:
                    base.OnGameEvent(e);
                    break;
            }
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
