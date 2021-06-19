using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class TooltipComponent : Component
    {
        //public enum Types { Object };

        static public SortedDictionary<Types, Interaction> TooltipDictionary;
        public Types Type;

        static TooltipComponent _Instance;
        static public TooltipComponent Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new TooltipComponent();
                return _Instance;
            }
        }

        public TooltipComponent()
        {
            //TooltipDictionary = new SortedDictionary<Types, Action>();

            //TooltipDictionary.Add(Types.Object, () => GetTooltip());
        }

        public List<GroupBox> GetTooltip()
        {
            List<GroupBox> panels = new List<GroupBox>();

            GroupBox info = new GroupBox();
            Label label = new Label(ToString());
            info.Controls.Add(label);
            panels.Add(info);

            return panels;
        }

        public virtual List<GroupBox> TooltipGroups
        {
            get
            {
                List<GroupBox> panels = new List<GroupBox>();

                GroupBox info = new GroupBox();
                Label label = new Label(ToString());
                info.Controls.Add(label);
                panels.Add(info);

                return panels;
            }
        }

        public override object Clone()
        {
            return new TooltipComponent();
        }
    }
}
