using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class TooltipEventArgs : EventArgs
    {
        public List<GroupBox> TooltipGroups;
        public TooltipEventArgs(List<GroupBox> tooltipGroups)
        {
            TooltipGroups = tooltipGroups;
        }
    }
    /// <summary>
    /// void GetTooltipInfo(Tooltip tooltip);
    /// </summary>
    public interface ITooltippable
    {

        //List<GroupBox> TooltipGroups { get; }
        void GetTooltipInfo(Tooltip tooltip);
    }
}
