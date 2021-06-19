using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Workbench
{
    class WorkbenchWindow : Window
    {
        static WorkbenchWindow _Instance;
        public static new WorkbenchWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WorkbenchWindow();
                return _Instance;
            }
        }

        WorkbenchWindow()
        {
            this.AutoSize = true;
            this.Movable = true;
            this.Title = "Workbench";
        }

        public WorkbenchWindow Refresh(GameObject bench)
        {
            this.Client.Controls.Clear();
            this.Client.Controls.Add(new WorkbenchInterface(bench));
            this.Location = this.CenterScreen;
            return this;
        }
    }
}
