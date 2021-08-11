using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IInspectable : ILabeled
    {
        IEnumerable<(object item, object value)> Inspect();
    }
}