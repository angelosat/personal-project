using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ToolAbilityDef : Def, IItemPreferenceContext
    {
        static int _IDSequence = "skills".GetHashCode() >> 2;
        public static int IDSequence => _IDSequence++;

        static Dictionary<int, ToolAbilityDef> _Dictionary;
        public static Dictionary<int, ToolAbilityDef> Dictionary
        {
            get
            {
                if (_Dictionary == null)
                    _Dictionary = new Dictionary<int, ToolAbilityDef>();
                return _Dictionary;
            }
        }

        public int ID { get; protected set; }
        public string Description { get; protected set; }

        readonly Func<TargetArgs, Interaction> WorkFactory = (t) => null;

        public ToolAbilityDef(string name) : base(name)
        {

        }

        protected ToolAbilityDef(string name, string description) : this(name, description, null) { }
        protected ToolAbilityDef(string name, string description, Func<TargetArgs, Interaction> workFactory) : base(name)
        {
            this.ID = IDSequence;
            Dictionary[this.ID] = this;

            this.Description = description;
            this.WorkFactory = workFactory;
        }

        public virtual Interaction GetInteraction(GameObject actor, TargetArgs target)
        {
            if (this.WorkFactory == null)
                return null;
            return this.WorkFactory(target);
        }
        public virtual Interaction GetInteraction()
        {
            if (this.WorkFactory == null)
                return null;
            return this.WorkFactory(TargetArgs.Null);
        }
        public static readonly ToolAbilityDef Digging = new("Digging", "Dig up soil and dirt blocks", (t) => new InteractionDigging());
        public static readonly ToolAbilityDef Building = new("Building", "Build blocks and other structures");
        public static readonly ToolAbilityDef Mining = new("Mining", "Dig up stone blocks", (t) => new InteractionMining());
        public static readonly ToolAbilityDef Chopping = new("Chopping", "Chop down trees and enemies with axes", (t) => new InteractionChopping());
        public static readonly ToolAbilityDef Argiculture = new("Argiculture", "Helps determine type and growth time of plants.");
        public static readonly ToolAbilityDef Planting = new("Planting", "Planting plants");
        public static readonly ToolAbilityDef Carpentry = new("Carpentry", "The craft of converting wood to useful equipment");

        internal static ToolAbilityDef GetSkill(int skillID)
        {
            return Dictionary[skillID];
        }
        internal static Control GetUI(int skillID, float value)
        {
            var skill = GetSkill(skillID);
            var label = new Label(skill.Name + ": " + value.ToString());
            return label;
        }
        public override string ToString()
        {
            return this.Name;
        }
        static ToolAbilityDef()
        {
            Register(Digging);
            Register(Building);
            Register(Mining);
            Register(Chopping);
            Register(Argiculture);
            Register(Planting);
            Register(Carpentry);
        }
    }
}
