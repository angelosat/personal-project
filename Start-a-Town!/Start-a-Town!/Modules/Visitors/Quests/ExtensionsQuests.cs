using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static class ExtensionsQuests
    {
        static public QuestDef GetQuest(this Towns.Town town, int questID)
        {
            return town.QuestManager.GetQuest(questID);
        }
    }
}
