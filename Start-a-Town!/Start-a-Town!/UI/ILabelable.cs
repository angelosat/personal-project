using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    public interface ILabelable
    {
        Func<string> TextFunc { get; set; }
    }
}
