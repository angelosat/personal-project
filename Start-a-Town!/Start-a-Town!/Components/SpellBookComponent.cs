using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class SpellBookComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Spells";
            }
        }

        public GameObjectSlotCollection Spells { get { return (GameObjectSlotCollection)this["Spells"]; } set { this["Spells"] = value; } }

        public SpellBookComponent()
        {
            this.Spells = new GameObjectSlotCollection();
        }
        public SpellBookComponent Initialize(params Spell.SpellTypes[] spells)
        {
            spells.ToList().ForEach(id => this.Spells.Add(Spell.Create(id).ToObject().ToSlotLink()));
            return this;
        }
        SpellBookComponent(params Spell.SpellTypes[] spells)
        {
            this.Spells = new GameObjectSlotCollection();
            spells.ToList().ForEach(id => this.Spells.Add(Spell.Create(id).ToObject().ToSlotLink()));
        }

        public override object Clone()
        {
            return new SpellBookComponent(this.Spells.Select(spell => spell.Object.GetComponent<Spell>("Spell").ID).ToArray());
        }
    }
}
