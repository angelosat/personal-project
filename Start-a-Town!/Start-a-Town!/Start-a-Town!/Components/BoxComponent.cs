using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Components
{
    class BoxComponent : Component
    {
        public override string ComponentName
        {
            get { return "Box"; }
        }
        public GameObject.Types ContentID;

        public BoxComponent(GameObject.Types contentID)
        {
            ContentID = contentID;
        }

        public BoxComponent()
        {
        }

        public override string ToString()
        {
            return "Content: " + GameObject.Objects[ContentID].Name;
        }

        public override object Clone()
        {
            BoxComponent box = new BoxComponent();
            box.ContentID = ContentID;
            return box;
        }
    }
}
