using System;
using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class StatNewDef : Def
    {
        public abstract class ValueBuilder : Def
        {
            public ValueBuilder(string name) : base(name)
            {
                //this.Name = name;
            }
            protected abstract float BaseGet(GameObject parent);
            protected List<Expression> Expressions = new List<Expression>();

            public float Get(GameObject parent)
            {
                var val = this.BaseGet(parent);
                foreach (var exp in this.Expressions)
                    val = exp.Perform(val);
                return val;
            }

            public class ExpressionDef : Def
            {
                public string Label;
                internal Func<float, float, float> Operator;
                public ExpressionDef(string name, string label, Func<float, float, float> op) : base(name)
                {
                    this.Label = label;
                  
                    this.Operator = op;
                }

                static readonly public ExpressionDef Division = new ExpressionDef("Division", "/", (a, b) => a / b);
            }
            protected class Expression
            {
                ExpressionDef Def;
                readonly float Value;

                public Expression(ExpressionDef def, float val)
                {
                    this.Def = def;
                    this.Value = val;
                }
                public float Perform(float a)
                {
                    return this.Def.Operator(a, this.Value);
                }

            }


            //static public readonly BaseGetter AttributeGetter = new BaseGetter("Attribute", AttributeDef att);
        }
        class ValueBuilderFromAttribute : ValueBuilder
        {
            AttributeDef Def;
            public ValueBuilderFromAttribute(AttributeDef def) : base("AttributeGetter")
            {
                this.Def = def;
            }
            protected override float BaseGet(GameObject parent)
            {
                return parent.GetAttribute(this.Def)?.Level ?? 0;
            }

            internal ValueBuilder DivideBy(int v)
            {
                this.Expressions.Add(new Expression(ExpressionDef.Division, 2));
                return this;
            }
        }

        public enum Types { Scalar, Percentile };

        public float BaseValue;
        public string Label;
        public string Description;
        Func<GameObject, float> Formula;
        //public ValueBuilder Builder;
        public Types Type = Types.Scalar;
        public string StringFormat = "";

        public StatNewDef(string name, Func<GameObject, float> formula) : base(name)
        {
     
            this.Formula = formula;
        }
        public StatNewDef(string name) : base(name)
        {
    
        }
        float ApplyModifiers(GameObject parent, float value)
        {
            var mods = parent.GetStatModifiers(this);
            if (mods is not null)
                foreach (var mod in mods)
                    value = mod.Def.Mod(parent, value);
            return value;
        }
        public float GetValue(GameObject parent)
        {
            if (this.Type == Types.Scalar)
            {
                var value = this.Formula(parent);
                //value = this.Builder.Get(parent);
                var modified = this.ApplyModifiers(parent, value);
                return this.BaseValue + modified;
            }
            else if (this.Type == Types.Percentile)
            {
                return this.ApplyModifiers(parent, 1);
            }
            else throw new Exception();
        }
        public Control GetControl(GameObject parent)
        {
            return new Label() {
                TextFunc = () => string.Format("{0}: {1}", this.Name, this.GetValue(parent)),
                //HoverFunc = () => string.Format("Base: {0} ({1} / 2)", this.GetValue(parent), AttributeDef.Strength.Name)
            };
        }
        //static public readonly StatNewDef MaxHaulWeight = new StatNewDef("MaxHaulWeight", a => (a.GetAttribute(AttributeDef.Strength)?.Level ?? 0))// / 2)
        //{
        //    Description = "Exceeding the limit will drain stamina",
        //    Label = "Haul weight limit",
        //    //Builder = new ValueBuilderFromAttribute(AttributeDef.Strength).DivideBy(2)
        //};
        //static public readonly StatNewDef Encumberance = new StatNewDef(
        //    "Encumberance",
        //    a =>
        //    {
        //        var haulWeight = a.GetHauled()?.TotalWeight ?? 0;
        //        if (haulWeight == 0)
        //            return 0;
        //        var maxWeight = StatNewDef.MaxHaulWeight.GetValue(a);
        //        var ratio = haulWeight / maxWeight;// (maxWeight - haulWeight) / haulWeight;
        //        ratio = MathHelper.Clamp(ratio, 0, 1);
        //        return ratio;// 1 - ratio;
        //    })
        //{
        //    Description = "Being encumbered affects walking speed.",
        //    Label = "Encumberance",
        //};

        //static public readonly StatNewDef WalkSpeed = new StatNewDef("WalkSpeed")
        //{
        //    Description = "Speed of walking",
        //    Label = "Walk speed",
        //    Type = Types.Percentile
        //};

        //static public readonly StatNewDef StaminaThresholdForWork = new StatNewDef("StaminaThresholdForWork",
        //    a =>
        //    {
        //        var actor = a as Actor;
        //        //var stamina = actor.GetResource(ResourceDef.Stamina);
        //        //var staminaPercentage = stamina.Percentage;
        //        var staminaBaseThreshold = .25f; //placeholder?
        //        var stamina = actor.GetResource(ResourceDef.Stamina);
        //        staminaBaseThreshold = stamina.GetThresholdValue(0);
        //        var activity1 = actor.GetTrait(TraitDef.Activity).Normalized;
        //        var num = activity1 * staminaBaseThreshold * .5f;
        //        var threshold = staminaBaseThreshold - num;
        //        return threshold;
        //        //var tired = staminaPercentage < threshold;
        //        //return tired;
        //    })
        //{
        //    Description = "Won't start new tasks if stamina below this percentage.",
        //    Label = "Work Stamina Threshold",
        //    //Type = Types.Percentile
        //};

        //static public readonly StatNewDef MoodChangeRate = new StatNewDef("MoodChangeRate",
        //    a =>
        //    {
        //        var actor = a as Actor;
        //        var resilience = actor.GetTrait(TraitDef.Resilience).Normalized;
        //        var value = 1 + resilience * .5f;
        //        return value;
        //    })
        //{
        //    Description = "The speed at which mood changes to reach target value.",
        //    Label = "Mood change rate",
        //    //Type = Types.Percentile
        //};

        //static public readonly StatNewDef[] NpcStatPackage = new StatNewDef[] { MaxHaulWeight, Encumberance, WalkSpeed };

    }
}
