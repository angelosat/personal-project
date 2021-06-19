using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    public interface IUISelection
    {
        void AddInfo(Control control);
        void AddTabAction(string label, Action action);
        void AddIcon(IconButton button);
    }
}
