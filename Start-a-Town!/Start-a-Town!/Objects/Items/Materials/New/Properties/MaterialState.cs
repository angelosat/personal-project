using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public struct MaterialState
    {
        static readonly public MaterialState Gas = new();
        static readonly public MaterialState Solid = new();
        static readonly public MaterialState Liquid = new();
    }
}
