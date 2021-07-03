namespace Start_a_Town_.Components.Spells
{
    class SpellComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Spell"; }
        }
        public Spell Spell;

        public override object Clone()
        {
            return new SpellComponent();
        }
    }
}
