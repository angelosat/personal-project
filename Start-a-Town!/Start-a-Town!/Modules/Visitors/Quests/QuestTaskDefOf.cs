using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class QuestTaskDefOf
    {
        static public TaskDef AcceptQuest = new("AcceptQuest", typeof(TaskBehaviorGetQuest));
    }
}
