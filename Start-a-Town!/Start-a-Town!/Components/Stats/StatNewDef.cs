using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class StatNewDef : Def
    {
        public abstract class ValueBuilder : Def
        {
            public ValueBuilder(string name) : base(name)
            {
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

                public static readonly ExpressionDef Division = new("Division", "/", (a, b) => a / b);
            }
            protected class Expression
            {
                readonly ExpressionDef Def;
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
        }
        class ValueBuilderFromAttribute : ValueBuilder
        {
            readonly AttributeDef Def;
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
        readonly Func<GameObject, float> Formula;
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
            return new Label()
            {
                TextFunc = () => string.Format("{0}: {1}", this.Name, this.GetValue(parent)),
            };
        }
    }
}
