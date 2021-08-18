using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public abstract class Inspectable : ILabeled
    {
        public virtual string Label => this.ToString();
        public virtual IEnumerable<(string item, object value)> Inspect()
        {
            foreach (var field in this.GetType().GetFields().Where(p => !Attribute.IsDefined(p, typeof(InspectorHidden))))
                yield return (field.Name, field.GetValue(this));
            foreach (var field in this.GetType().GetProperties().Where(p=> !Attribute.IsDefined(p, typeof(InspectorHidden))))
                yield return (field.Name, field.GetValue(this));
        }
    }
    public interface IInspectable : ILabeled
    {
        IEnumerable<(string item, object value)> Inspect();
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InspectorHidden : Attribute
    {
    }
}