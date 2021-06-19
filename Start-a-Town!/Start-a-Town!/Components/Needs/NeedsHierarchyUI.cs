using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Needs
{
    class NeedsHierarchyUI : GroupBox
    {
        public NeedsHierarchyUI(GameObject entity)
        {
            throw new Exception();
            //var inner = entity.GetComponent<NeedsComponent>().NeedsHierarchy.Inner;
            //foreach (var item in inner)
            //{
            //    var panel = new PanelLabeled(item.Key) { Location = this.BottomLeft };
            //    foreach (var n in item.Value.Values)
            //    {
            //        var ui = n.GetUI(entity);
            //        ui.Location = panel.Controls.BottomLeft;
            //        panel.AddControls(ui);
            //    }
            //    this.AddControls(panel);
            //}
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
