namespace Start_a_Town_.Components
{
    class CombatComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Combat"; }
        }
        public StatCollection Stats { get { return (StatCollection)this["Stats"]; } set { this["Stats"] = value; } }

        public CombatComponent()
        {
            this.Stats = new StatCollection();
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Refresh:
                case Message.Types.Hold:
                    Refresh(parent);
                    return true;

                default:
                    return false;
            }
        }

        private void Refresh(GameObject parent)
        {
            this.Stats = new StatCollection();
            BodyComponent.PollStats(parent, this.Stats);
            InventoryComponent.PollStats(parent, this.Stats);
        }

        public override object Clone()
        {
            return new CombatComponent();
        }
    }
}
