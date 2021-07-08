using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_
{
    class PacketDesignateConstruction
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        
        internal static void Send(IObjectProvider net, ToolDrawing.Args a)
        {
            Send(net, null, a);
        }
        
        static public void Send(IObjectProvider net, BlockRecipe.ProductMaterialPair item, ToolDrawing.Args a)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            a.Write(stream);
            if(!a.Removing)
                item.Write(stream);
            
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var args = new ToolDrawing.Args(r);
            var product = args.Removing ? null : new BlockRecipe.ProductMaterialPair(r);
            var positions = ToolDrawing.GetPositions(args);
            net.Map.Town.ConstructionsManager.Handle(args, product, positions);
            

            if (net is Server)
                Send(net, product, args);
            return;
        }
    }
}
