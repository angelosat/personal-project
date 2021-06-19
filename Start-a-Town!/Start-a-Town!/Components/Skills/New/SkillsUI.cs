using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Skills.New
{
    class SkillsUI : GroupBox
    {
        //SlotGrid<Slot<Skill>> Slots;
        List<Slot<SkillNew>> Slots;
        public SkillsUI()
        {
            //this.Controls.Add("label".ToLabel());
        }
        public void Refresh(GameObject actor)
        {
            var skillsComp = actor.GetComponent<SkillsNewComponent>();
            if (skillsComp == null)
                return;
            //this.Slots = new SlotGrid<Slot<Skill>>(skillsComp.SkillList.Count, 8, this.InitSlot);
            this.Slots = new List<Slot<SkillNew>>();
            for (int i = 0; i < skillsComp.SkillList.Count; i++)
            {
                var slot = new Slot<SkillNew>();
                slot.Tag = skillsComp.SkillList[i];
                this.Slots.Add(slot);
            }
            this.Controls.Clear();
            this.Controls.AddRange(this.Slots);
            this.AlignHorizontally();
        }
        //Slot<Skill> InitSlot(Slot<Skill> slot)
        //{
        //    return slot;
        //}
    }
}
