﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    class PersonalityUI : GroupBox
    {
        public PersonalityUI()
        {

        }
        public void Refresh(Actor npc)
        {
            this.ClearControls();
            var p = npc.Personality;
            foreach (var t in p.Traits)
            {
                //this.AddControlsBottomLeft(t.Value.GetUI());
                this.AddControlsBottomLeft(t.GetUI());
            }
        }
    }
}
