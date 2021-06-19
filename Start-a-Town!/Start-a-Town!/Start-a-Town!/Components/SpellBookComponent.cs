using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class SpellBookComponent : Component
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
        public SpellBookComponent Initialize(params Spell.Types[] spells)
        {
            spells.ToList().ForEach(id => this.Spells.Add(Spell.Create(id).ToObject().ToSlot()));
            return this;
        }
        SpellBookComponent(params Spell.Types[] spells)
        {
            this.Spells = new GameObjectSlotCollection();
            spells.ToList().ForEach(id => this.Spells.Add(Spell.Create(id).ToObject().ToSlot()));
        }

        public override object Clone()
        {
            return new SpellBookComponent(this.Spells.Select(spell => spell.Object.GetComponent<Spell>("Spell").ID).ToArray());
        }
    }
}
