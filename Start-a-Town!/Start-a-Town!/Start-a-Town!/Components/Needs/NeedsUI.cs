using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Needs
{
    class NeedsUI : GroupBox
    {
        GameObject Entity;
        public NeedsUI(GameObject entity)
        {
            this.Entity = entity;

            var needs = entity.GetComponent<NeedsComponent>();
            this.Controls.Add(needs.NeedsHierarchy.GetUI(entity));
            //foreach (var level in needs.NeedsHierarchy.Inner.Values)
            //    foreach (var need in level.Values)
            //    {
            //        var ui = need.GetUI(entity);
            //        ui.Location = this.Controls.BottomLeft;
            //        this.Controls.Add(ui);
            //    }
        }
    }
}
