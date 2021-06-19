using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    /// <summary>
    /// <para>bool Undo();</para>
    /// <para>bool Redo();</para>
    /// </summary>
    interface IUndoable
    {
        bool Performed { get; }
        bool Undo();
        bool Redo();
    }
}
