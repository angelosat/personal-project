using System;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Needs
{
    class NeedsHierarchyUI : GroupBox
    {
        public NeedsHierarchyUI(GameObject entity)
        {
            throw new NotImplementedException();
            
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.NeedUpdated:
                    break;

                default:
                    break;
            }
        }
    }
}
