using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptReaction : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Reaction;
            }
        }
        public override string Name
        {
            get
            {
                return "Reaction";
            }
        }
        public override object Clone()
        {
            return new ScriptReaction();
        }

        public override void OnStart(ScriptArgs args)
        {
            //GameObject product = args.Args.Deserialize<GameObject>(GameObject.CreateCustomObject);
            //using (BinaryReader r = new BinaryReader(new Stream(args.Args)))
            GameObject product;
            using(MemoryStream stream = new MemoryStream(args.Args))
            {
                BinaryReader reader = new BinaryReader(stream);
                product = GameObject.CreatePrefab(reader);
                //product = GameObject.CreateCustomObject(reader);
            }
            args.Net.PopLoot(product, args.Target.Object);
        }
    }
}
