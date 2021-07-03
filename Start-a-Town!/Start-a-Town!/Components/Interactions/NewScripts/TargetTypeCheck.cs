using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class TargetTypeCheck : ScriptTaskCondition
    {
        TargetType Type { get; set; }

        public TargetTypeCheck(TargetType type)
            : base("TargetType")
        {
            this.Type = type;
            this.ErrorEvent = Message.Types.InvalidTargetType;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            return target.Type == this.Type;
        }

        public override ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            if (!this.Condition(actor, target))
                return this;
            return null;
        }
    }
}
