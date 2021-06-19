using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Control;

namespace Start_a_Town_.Components
{
    class EditorComponent : Component
    {
        public ControlTool Tool { get { return (ControlTool)this["Tool"]; } set { this["Tool"] = value; } }

        public override object Clone()
        {
            return new EditorComponent();
        }
    }
}
