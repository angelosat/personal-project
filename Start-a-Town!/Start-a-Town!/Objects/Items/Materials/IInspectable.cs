using System.Collections.Generic;

namespace Start_a_Town_
{
    public abstract class Inspectable : ILabeled
    {
        public virtual string Label => this.ToString();
        //set; }
        public virtual IEnumerable<(string item, object value)> Inspect()
        {
            foreach (var field in this.GetType().GetFields())
                yield return (field.Name, field.GetValue(this));
            foreach (var field in this.GetType().GetProperties())
                yield return (field.Name, field.GetValue(this));
        }
    }
    public interface IInspectable : ILabeled
    {
        IEnumerable<(string item, object value)> Inspect();
    }
}