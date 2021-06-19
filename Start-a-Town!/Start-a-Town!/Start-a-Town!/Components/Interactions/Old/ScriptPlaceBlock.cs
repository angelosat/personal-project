using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptPlaceBlock : Script
    {
        Block.Types Block;
   

        public override Script.Types ID
        {
            get
            {
                return Script.Types.PlaceBlock;
            }
        }
        public override string Name
        {
            get
            {
                return "ScriptPlaceBlock";
            }
        }

        public ScriptPlaceBlock(Block.Types block)
        {
            this.Block = block;
            this.AddComponent(new ScriptTimer("Placing", 1, this.Success));
            this.AddComponent(new ScriptConsumeHeldItem());
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
        }

        //public override void Start(ScriptArgs args)
        //{
        //    base.Start(args);
        //    args.Net.PostLocalEvent(args.Actor, Message.Types.Speak, "Test");
        //    Finish(args);
        //}

        //public override void Start(ScriptArgs args)
        //{
        //    //this.ScriptState = ScriptState.Finished;
            
        //    this.Args = args;
        //    base.Start(args);
        //    //Finish(args);
        //}

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            base.Update(net, parent, chunk);
        }

        //void Success()
        //{
        //    this.Success(this.ArgsSnapshot);
        //}

        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            args.Target.FinalGlobal.TrySetCell(args.Net, this.Block);
        }

        public override object Clone()
        {
            return new ScriptPlaceBlock(this.Block);
        }
    }
}
