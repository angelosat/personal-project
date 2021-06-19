using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class GrassComponent : Component
    {
        public class State
        {
            static public byte GetData(int variation)
            {
                return (byte)(variation << 2);
            }
        }

        //static public readonly double TramplingChance = 0.1f;
        public override string ComponentName
        {
            get { return "Grass"; }
        }
        public override object Clone()
        {
            return new GrassComponent();
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    // turn block to dirt and chance to pop loot twigs
                    //parent.Global.TrySetCell(e.Network, Block.Types.Soil, parent.Global.GetData(e.Network.Map));
                    //e.Network.PopLoot(new LootTable(new Loot(GameObject.Types.Twig, 0.4f, 1)), parent.Global + Vector3.UnitZ, Vector3.Zero);
                    Script.Start(e.Parameters[0] as GameObject, "Ripping", 1, () => this.Rip(e.Network, parent));
                    return true;

                //case Message.Types.StepOn:
                //    //StepOn(e.Network, parent, e.Parameters[0] as GameObject);
                //    GameObject actor = e.Parameters[0] as GameObject;
                //    //e.Data = Net.Network.Serialize(actor.Write);
                //    e.Data = Net.Network.Serialize(w => TargetArgs.Write(w, actor));
                //    //e.Data =
                //    Action<System.IO.BinaryWriter> a = w=>TargetArgs.Write(w, actor);//.GetBytes();
                //    e.Data = a.GetBytes();
                //    e.Network.RandomEvent(parent, e, rnd => StepOn(e.Network, parent, actor, rnd));
                //    return true;

                default:
                    return true;
            }
        }

        //public override void RandomBlockUpdate(Net.IObjectProvider net, GameObject parent)
        //{
        //    //base.RandomBlockUpdate(net, parent);
        //    foreach (var n in Position.GetNeighbors(parent.Global))
        //    {
        //        Cell cell;
        //        if (!n.TryGetCell(net.Map, out cell))
        //            continue;
        //        if (cell.Type != Block.Types.Soil)
        //            continue;
        //        if ((n + Vector3.UnitZ).GetSunLight(net.Map) < 8)
        //            continue;
        //        n.TrySetCell(net, Block.Types.Grass);
        //    }
        //}

        //public override void RandomEvent(GameObject parent, RandomObjectEventArgs e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.StepOn:
        //            e.Data.Translate(e.Network, r =>
        //            {
        //                TargetArgs actor = TargetArgs.Read(e.Network, r);
        //                StepOn(e.Network, parent, actor.Object, e.Value);
        //            });
        //            return;

        //        default:
        //            return;
        //    }
        //}


        void Rip(Net.IObjectProvider net, GameObject parent)
        {
            // do a check that block is still the same, or do it in the script?
            //if (parent.Global.GetCell(net.Map).Block.Type != Block.Types.Grass)
            if (net.Map.GetCell(parent.Global).Block.Type != Block.Types.Grass)
                return;
            //parent.Global.TrySetCell(net, Block.Types.Soil, parent.Global.GetData(net.Map));
            parent.Global.TrySetCell(net, Block.Types.Soil, net.Map.GetData(parent.Global));

            net.PopLoot(new LootTable(new Loot(GameObject.Types.Twig, 0.4f, 1)), parent.Global + Vector3.UnitZ, Vector3.Zero);
        }

        //static void StepOn(Net.IObjectProvider net, GameObject parent, GameObject actor, double chance)
        //{
        //    //(net.ToString()+ " " + chance.ToString()).ToConsole();
        //    if (chance < TramplingChance)
        //        parent.Global.TrySetCell(net, Block.Types.Soil, parent.Global.GetData(net.Map));
        //}
    }
}
