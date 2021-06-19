using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public interface ISelectable
    {
        string GetName();
        //void Select(UISelectedInfo panel);
        void GetSelectionInfo(IUISelection panel);
        IEnumerable<(string name, Action action)> GetInfoTabs();
        void GetQuickButtons(UISelectedInfo panel);
        bool Exists { get; }
        void TabGetter(Action<string, Action> getter);
        //bool IsEqual(object o);
    }
}
