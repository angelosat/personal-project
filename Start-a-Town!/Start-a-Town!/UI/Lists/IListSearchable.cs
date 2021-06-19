using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    interface IListSearchable<TObject> 
    {
        void Filter(Func<TObject, bool> filter);
    }
}
