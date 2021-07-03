using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components
{
    class Spell : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Spell";
            }
        }

        public enum SpellTypes { Healing, Fireball }

        static List<Spell> _Registry;
        public static List<Spell> Registry
        {
            get
            {
                if (_Registry == null)
                    Initialize();
                return _Registry;
            }
        }

        static void Initialize()
        {
            _Registry = new List<Spell>()
            {
                new Spell().Initialize(SpellTypes.Healing, "Healing"),
                new Spell().Initialize(SpellTypes.Fireball, "Fireball")
            };
        }

        static Spell GetSpell(SpellTypes id)
        {
            return Registry.ToDictionary(foo => foo.ID, foo => foo)[id];
        }

        public override object Clone()
        {
            Spell spell = new Spell().Initialize(this.ID, this.Name);
            spell.Description = this.Description;
            spell.Effect = this.Effect;
            spell.Cost = this.Cost;
            spell.Time = this.Time;
            spell.IconID = this.IconID;
            return spell;
        }

        static public Spell Create(SpellTypes id)
        {
            return GetSpell(id).Clone() as Spell;
        }
        
        public SpellTypes ID {get;set;}
        public string Name { get; set; }
        public string Description { get; set; }
        public int IconID { get; set; }
        public TimeSpan Time { get; set; }
        public Dictionary<ResourceDef.ResourceTypes, float> Cost { get; set; }
        public Action<GameObject, GameObject> Effect { get; set; }

        public Spell()
        {


        }
        public Spell Initialize(SpellTypes id, string name)
        {
            this.ID = id;
            this.Name = name;
            this.Cost = new Dictionary<ResourceDef.ResourceTypes, float>();
            this.Effect = (caster, target) => { };
            this.Description = "";
            this.Time = TimeSpan.Zero;
            return this;
        }

        public GameObject ToObject()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(GameObject.Types.Spell, "Spell", this.Name, this.Description);
            obj.AddComponent<Spell>().Initialize(this.ID, this.Name);
            return obj;
        }
    }
}
