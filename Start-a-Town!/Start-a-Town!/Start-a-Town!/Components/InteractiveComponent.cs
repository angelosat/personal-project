using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class InteractiveComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Interactive";
            }
        }

        public InteractiveComponent()
        {
            this.Abilities = new List<Script.Types>();
        }

        //public List<GameObjectSlot> Abilities { get { return (List<GameObjectSlot>)this["Interactions"]; } set { this["Interactions"] = value; } }
        public List<Script.Types> Abilities { get { return (List<Script.Types>)this["Interactions"]; } set { this["Interactions"] = value; } }

        public InteractiveComponent(params Script.Types[] abilities)
        {
            this.Abilities = new List<Script.Types>(abilities);
        }
        public override object Clone()
        {
            return new InteractiveComponent(this.Abilities.ToArray());
        }

        static public bool HasAbility(GameObject obj, Script.Types ability)
        {
            InteractiveComponent inter;
            if (!obj.TryGetComponent<InteractiveComponent>("Interactive", out inter))
                return false;
            //return inter.Abilities.FindAll(a => ability == (Script.Types)a.Object["Ability"]["ID"]).Count > 0;
            return inter.Abilities.FindAll(a => ability == a).Count > 0;
        }

        public InteractiveComponent Initialize(params Script.Types[] scripts)
        {
            this.Abilities.AddRange(scripts);
            return this;
        }

        //static public void Add(GameObject obj, params Script.Types[] abilities)
        //{
        //    Add(obj, abilities.Select(a => Ability.GetAbilityObject(a)).ToArray());
        //}
        //static public void Add(GameObject obj, params GameObjectSlot[] abilities)
        //{
        //    InteractiveComponent comp;
        //    if (!obj.TryGetComponent<InteractiveComponent>("Interactive", out comp))
        //    {
        //        comp = new InteractiveComponent();
        //        obj["Interactive"] = comp;
        //    }
        //    comp.Abilities.AddRange(abilities);
        //}
    }
}
