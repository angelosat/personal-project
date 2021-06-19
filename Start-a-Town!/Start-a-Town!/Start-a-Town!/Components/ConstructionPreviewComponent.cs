using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class ConstructionPreviewComponent : Component
    {
        static List<GameObject> DesignatedConstructions = new List<GameObject>();

        Blueprint Blueprint { get { return (Blueprint)this["Blueprint"]; } set { this["Blueprint"] = value; } }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e = null)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.ApplyMaterial:
                    GameObjectSlot holding = sender["Inventory"]["Holding"] as GameObjectSlot;
                    GameObject mat = holding.Object;
                    if (mat == null)
                        return true;
                    if (!Blueprint.GetFilter(0).Apply(mat))
                        return true;
                    return true;
                default:
                    return true;
            }
        }

        public override object Clone()
        {
            return new ConstructionPreviewComponent();
        }
    }
}
