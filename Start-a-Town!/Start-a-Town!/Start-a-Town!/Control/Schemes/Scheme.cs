using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.PlayerControl.Schemes
{
    abstract class Scheme
    {
        public virtual void HandleLeftMouseDown() { }
        public virtual void HandleLeftMouseUp() { }
    }
}
