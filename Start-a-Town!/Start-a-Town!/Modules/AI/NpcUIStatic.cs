using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.AI
{
    class NpcUIStatic : GroupBox
    {
        static NpcUIStatic _Instance;
        public static NpcUIStatic Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new NpcUIStatic();
                return _Instance;
            }
        }

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
        StatsInterface StatsUI;
        GroupBox SkillsUI;
        public NpcUIStatic()
        {
            this.PanelTask = new Panel() { AutoSize = true };
            //var aistate = AIState.GetState(npc);
            this.LabelTask = new Label("Task: ");// { TextFunc = () => aistate.ToString() };
            this.PanelTask.Controls.Add(this.LabelTask);

            this.PanelNeeds = new Panel() { AutoSize = true, Location = this.PanelTask.BottomLeft };
            this.Needs = new NeedsUI();
            this.PanelNeeds.Controls.Add(this.Needs);

            this.Inventory = new InventoryUI();
            this.PanelInventory = new Panel() { AutoSize = true, Location = this.PanelNeeds.TopRight };
            this.PanelInventory.AddControls(this.Inventory);

            this.PanelStats = new Panel() { AutoSize = true, Location = this.PanelInventory.TopRight };
            this.StatsUI = new StatsInterface();
            this.PanelStats.AddControls(this.StatsUI);

            this.PanelSkills = new Panel() { AutoSize = true, Location = this.PanelStats.TopRight };
            this.SkillsUI = new GroupBox();
            this.PanelSkills.AddControls(this.SkillsUI);



            this.Buttons = new Panel() { AutoSize = true, Location = this.PanelNeeds.BottomLeft };
            //this.Log = new NpcLogUINew(this.Npc);
            //var winlog = this.Log.ToWindow(Npc.Name + "'s Log");
            this.Log = new NpcLogUINew();
            var btnlog = new Button("Log", this.PanelNeeds.ClientSize.Width);// { LeftClickAction = () => ShowLog(winlog) };

            //var winPersonality = AIState.GetState(npc).Personality.GetUI().ToWindow(Npc.Name + "'s Personality");
            var btnpersonality = new Button("Personality", this.PanelNeeds.ClientSize.Width);// { Location = btnlog.BottomLeft, LeftClickAction = () => winPersonality.ToggleSmart() };
            this.PersonalityUI = new PersonalityUI();

            this.Buttons.AddControls(btnlog, btnpersonality);

            //this.AddControls(this.PanelTask, this.PanelNeeds,
            //    this.PanelStats,
            //    this.PanelInventory,
            //    this.Buttons);

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

            GameMode.Current.OnUIEvent(UIManager.Events.NpcUICreated, this);
        }
        public NpcUIStatic(Actor npc)
        {
            this.PanelTask = new Panel() { AutoSize = true };
            var aistate = AIState.GetState(npc);// npc.GetComponent<AIComponent>().State;
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

            //var winPersonality = AIState.GetState(npc).Personality.GetUI().ToWindow(Npc.Name + "'s Personality");
            var winPersonality = npc.Personality.GetUI().ToWindow(Npc.Name + "'s Personality");
            var btnpersonality = new Button("Personality", this.PanelNeeds.ClientSize.Width) { Location = btnlog.BottomLeft, LeftClickAction = () => winPersonality.ToggleSmart() };


            

            this.Buttons.AddControls(btnlog, btnpersonality);


   

            this.AddControls(this.PanelTask, this.PanelNeeds,
                this.PanelStats,
                panelInv,
                this.Buttons);

            GameMode.Current.OnUIEvent(UIManager.Events.NpcUICreated, this);

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
            this.PersonalityUI.Refresh(npc as Actor);
            this.SkillsUI.ClearControls();
            //NpcSkillsComponent.GetUI(npc, this.SkillsUI);
            this.SkillsUI.AddControls(NpcSkillsComponent.GetGUI(npc));

            this.PanelNeeds.Location = this.PanelTask.BottomLeft;
            this.PanelInventory.Location = this.PanelNeeds.TopRight;
            this.PanelStats.Location = this.PanelInventory.TopRight;
            this.PanelSkills.Location = this.PanelStats.TopRight;

            this.Buttons.Location = this.PanelNeeds.BottomLeft;

            //var win = this.GetWindow();
            //this.Refresh();
            //win.Client.Refresh();
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
