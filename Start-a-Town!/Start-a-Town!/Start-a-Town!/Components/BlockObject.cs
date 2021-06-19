using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class BlockObject : Component
    {
        public override string ComponentName
        {
            get { return "BlockObject"; }
        }

        BlockComponent Block { get; set; }

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            Chunk.AddBlockObject(net, parent);
        }

        public override void Despawn(//Net.IObjectProvider net,
            GameObject parent)
        {
            Chunk.RemoveBlockObject(parent.Map, parent.Global);
        }

        public override void MakeChildOf(GameObject parent)
        {
            BlockComponent block;
            if (!parent.TryGetComponent<BlockComponent>(out block))
                throw new Exception("BlockObject without Block Component");
            this.Block = block;
        }

        public override object Clone()
        {
            return new BlockObject();
        }
    }
}
