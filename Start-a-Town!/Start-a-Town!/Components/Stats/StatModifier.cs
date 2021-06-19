using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    public class ValueModifierValue
    {
        public string Name;
        public float Value;
        public ValueModifierValue(string name, float value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
    public class ValueModifier
    {
        public Dictionary<string, ValueModifierValue> Values = new Dictionary<string, ValueModifierValue>();
        protected Func<ValueModifier, GameObject, float, float> Modifier = (parent, mod, v) => v;
        public string Name = "";
        public object Tag;
        //public GameObject Parent;
        public Func<ValueModifier, string> Description = mod => "";
        public ValueModifier(string name, Func<ValueModifier, GameObject, float, float> modifier, params ValueModifierValue[] values)
        {
            this.Name = name;
            this.Modifier = modifier;
            foreach (var val in values)
                this.Values.Add(val.Name, val);
        }
        public ValueModifier(string name, Func<ValueModifier, GameObject, float, float> modifier, object tag)
            : this(name, modifier)
        {
            this.Tag = tag;
        }
        public float Modify(GameObject parent, float value)
        {
            return this.Modifier(this, parent, value);
        }
        public float GetValue(string value)
        {
            return this.Values[value].Value;
        }
        public override string ToString()
        {
            return this.Description(this);
        }
        //public StatModifier Clone()
        //{
        //    StatModifier mod = new StatModifier(this.Name, this.Modifier, this.Values) { //Parent = this.Parent, 
        //        Description = this.Description, Tag = this.Tag };
        //    return mod;
        //}
    }

    //public class StatModifier
    //{
    //    Func<float, float> Modifier = v => v;
    //    public string Name = "";
    //    public object Tag;
    //    public StatModifier(string name, Func<float, float> modifier)
    //    {
    //        this.Name = name;
    //        this.Modifier = modifier;
    //    }
    //    public StatModifier(string name, Func<float, float> modifier, object tag)
    //        : this(name, modifier)
    //    {
    //        this.Tag = tag;
    //    }
    //    public float Modify(float value)
    //    {
    //        return this.Modifier(value);
    //    }
    //}
}
