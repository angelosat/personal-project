using System;
using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ToolAbilityDef : Def
    {
        static int _IDSequence = "skills".GetHashCode() >> 2;
        public static int IDSequence { get { return _IDSequence++; } }

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
        Func<TargetArgs, Interaction> WorkFactory = (t) => null;

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
        static public readonly ToolAbilityDef Digging = new("Digging", "Dig up soil and dirt blocks", (t) => new InteractionDigging());
        static public readonly ToolAbilityDef Building = new("Building", "Build blocks and other structures");
        static public readonly ToolAbilityDef Mining = new("Mining", "Dig up stone blocks", (t) => new InteractionMining());
        static public readonly ToolAbilityDef Chopping = new("Chopping", "Chop down trees and enemies with axes", (t) => new InteractionChopping());
        static public readonly ToolAbilityDef Argiculture = new("Argiculture", "Helps determine type and growth time of plants.");
        static public readonly ToolAbilityDef Planting = new("Planting", "Planting plants");
        static public readonly ToolAbilityDef Carpentry = new("Carpentry", "The craft of converting wood to useful equipment");

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
