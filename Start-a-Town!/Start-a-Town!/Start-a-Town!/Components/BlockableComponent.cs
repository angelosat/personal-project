using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class BlockableComponent : Component
    {
        public override string ComponentName
        {
            get { return "Blockable"; }
        }
        public override object Clone()
        {
            return new BlockableComponent() { Block = this.Block };
        }
        public Block.Types Block { get { return (Block.Types)this["Block"]; } set { this["Block"] = value; } }
        public BlockableComponent Initialize(Block.Types block)
        {
            this.Block = block;
            return this;
        }
        public BlockableComponent()
        {

        }
        internal override void GetAvailableActions(List<Script> list)
        {
            list.Add(new Interactions.ScriptPlaceBlock(this.Block));
        }
    }
}
