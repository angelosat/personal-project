using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.UI
{
    class LoadingTask
    {
        public string Name;
        public Action Task;
        public LoadingTask(string name, Action task)
        {
            this.Name = name;
            this.Task = task;
        }
    }

}
