using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    /// <summary>
    /// <para>float Min { get; }</para>
    /// <para>float Max { get; }</para>
    /// <para>float Value { get; }</para>
    /// <para>float Percentage { get; }</para>
    /// </summary>
    public interface IProgressBar
    {
        float Min { get; }
        float Max { get; }
        float Value { get; }
        float Percentage { get; }
    }
}
