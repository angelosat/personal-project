using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using Start_a_Town_.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public enum NetworkSideType { Local, Server }

    public class Network
    {
        public class Packets
        {
            static public int PacketSyncReport, PacketTimestamped;
            static public void Init()
            {
                PacketSyncReport = RegisterPacketHandler(HandleSyncReport);
                PacketTimestamped = RegisterPacketHandler(ReceiveTimestamped);
            }

            private static void ReceiveTimestamped(IObjectProvider net, BinaryReader r)
            {
                //var tick = r.ReadUInt64();
                if(net is Client client)
                    client.HandleTimestamped(r);
            }

            public static void SendSyncReport(Server server, string text)
            {
                server.GetOutgoingStream().Write(PacketSyncReport, text);
            }
            private static void HandleSyncReport(IObjectProvider net, BinaryReader r)
            {
                if (net is not Net.Client)
                    throw new Exception();
                net.Report(r.ReadString());
            }
        }
        static public ConsoleBoxAsync Console { get { return LobbyWindow.Instance.Console; } }

        public Client Client;// = Client.Instance;
        public Server Server;// = Server.Instance;

        public void CreateClient()
        {
            this.Client = Client.Instance;
        }

        public void CreateServer()
        {
            this.Server = Server.Instance;
        }
        static Network()
        {
            Packets.Init();
        }
        public Network()
        {
            this.CreateClient();
            this.CreateServer();
        }
        public void Update(GameTime gt)
        {
            this.Server.Update(gt);
            this.Client.Update();
            var d = this.Server.CurrentTick - this.Client.CurrentTick;
            //if (d != Client.ClientClockDelayMS)
            //    "client clock out of sync".ToConsole();
        }
        public static void SyncReport(Server server, string text)
        {
            Packets.SendSyncReport(server, text);
        }
        /// <summary>
        /// TODO: Maybe don't compress every packet? 
        /// </summary>
        /// <param name="dataGetter"></param>
        /// <returns></returns>
        static public byte[] Serialize(Action<BinaryWriter> dataGetter)
        {
            // with compression
            byte[] data;
            using (MemoryStream output = new())
            {
                using (MemoryStream mem = new())
                using (BinaryWriter bin = new(mem))
                using (GZipStream zip = new(output, CompressionMode.Compress))
                {
                    dataGetter(bin);
                    mem.Position = 0;
                    mem.CopyTo(zip);
                }
                data = output.ToArray();
            }
            return data;

            // no compression
            //byte[] data;
            //using (MemoryStream output = new MemoryStream())
            //{
            //    using (BinaryWriter bin = new BinaryWriter(output))
            //        dataGetter(bin);
            //    data = output.ToArray();
            //}
            //return data;



            //var data = dataGetter.GetBytes();
            //var compressed = data.Compress();
            //return compressed;

            //byte[] data;
            //using (MemoryStream mem = new MemoryStream())
            //{
            //    using (BinaryWriter bin = new BinaryWriter(mem))
            //    {
            //        dataGetter(bin);

            //        using (MemoryStream outStream = new MemoryStream())
            //        {
            //            using (GZipStream zip = new GZipStream(outStream, CompressionMode.Compress))
            //            {
            //                mem.Position = 0;
            //                mem.CopyTo(zip);
            //            }
            //            data = outStream.ToArray();
            //        }
            //    }
            //}
            //return data;
        }

        static public object Deserialize(byte[] compressed, Func<BinaryReader, object> dataReader)
        {
            using (MemoryStream memStream = new MemoryStream(compressed))
            {
                using (GZipStream decompress = new GZipStream(memStream, CompressionMode.Decompress))
                {
                    //MemoryStream memory = new MemoryStream();
                    //decompress.CopyTo(memory);
                    //memory.Position = 0;
                    //return memory;//.ToArray();
                    using (MemoryStream decompressed = new MemoryStream())
                    {
                        decompress.CopyTo(decompressed);
                        decompressed.Position = 0;
                        using (BinaryReader reader = new BinaryReader(decompressed))
                            return dataReader(reader);
                        //return reader;
                    }
                }
            }
        }
        static public T Deserialize<T>(byte[] compressed, Func<BinaryReader, T> dataReader)
        {
            // w/ compression
            using (MemoryStream input = new MemoryStream(compressed))
            using (GZipStream zip = new GZipStream(input, CompressionMode.Decompress))
            using (BinaryReader reader = new BinaryReader(zip))
            {
                return dataReader(reader);
            }

            // w/o compression
            //using (MemoryStream input = new MemoryStream(compressed))
            //using (BinaryReader reader = new BinaryReader(input))
            //    return dataReader(reader);

            //old
            //using (MemoryStream memStream = new MemoryStream(compressed))
            //using (GZipStream decompress = new GZipStream(memStream, CompressionMode.Decompress))
            //using (MemoryStream decompressed = new MemoryStream())
            //{
            //    decompress.CopyTo(decompressed);
            //    decompressed.Position = 0;
            //    BinaryReader reader = new BinaryReader(decompressed);
            //    return dataReader(reader);
            //}
        }
        static public void Deserialize(byte[] compressed, Action<BinaryReader> dataReader)
        {
            // uncompressed input
            //using (MemoryStream memStream = new MemoryStream(compressed))
            //using (BinaryReader reader = new BinaryReader(memStream))
            //    dataReader(reader);
  

            // compressed input
            using (MemoryStream memStream = new MemoryStream(compressed))
            {
                using (GZipStream decompress = new GZipStream(memStream, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressed = new MemoryStream())
                    {
                        decompress.CopyTo(decompressed);
                        decompressed.Position = 0;
                        using (BinaryReader reader = new BinaryReader(decompressed))
                            dataReader(reader);
                    }
                }
            }
        }

        
        static public void InventoryOperation(IObjectProvider net, GameObject sourceObj, GameObjectSlot targetSlot, GameObjectSlot sourceSlot, int amount)
        {
            //net.EventOccured(Components.Message.Types.InventoryChanged, targetSlot.Parent);
            //net.EventOccured(Components.Message.Types.InventoryChanged, sourceSlot.Parent);

            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            {
                //if (targetSlot.Set(sourceObj, amount))
                //    sourceSlot.StackSize -= amount;
                // SINCE I STORE STACKSIZE INSIDE THE OBJECT, NO NEED TO ADJUST SOURCE SLOT'S STACKSIZE
                targetSlot.Object = sourceObj;
                sourceSlot.Clear();
                return;
            }
            if (sourceSlot.Object.IDType == targetSlot.Object.IDType)
            {
                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                {
                    targetSlot.StackSize += sourceSlot.StackSize;
                    net.DisposeObject(sourceSlot.Object.RefID);
                    sourceSlot.Clear();
                    //merge slots
                    return;
                }
            }
            else
                if (amount < sourceSlot.StackSize)
                    return;

            if (targetSlot.Filter(sourceObj))
                if(sourceSlot.Filter(targetSlot.Object))
                    targetSlot.Swap(sourceSlot);
        }
        static public void InventoryOperation(IObjectProvider net, GameObjectSlot sourceSlot, GameObjectSlot targetSlot, int amount)
        {
            //net.EventOccured(Components.Message.Types.InventoryChanged, targetSlot.Parent);
            //net.EventOccured(Components.Message.Types.InventoryChanged, sourceSlot.Parent);
            var obj = sourceSlot.Object;
            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            {
                if (amount < sourceSlot.StackSize)
                {
                    obj = sourceSlot.Object.Clone();
                    obj.GetInfo().StackSize = amount;
                    sourceSlot.Object.GetInfo().StackSize -= amount;
                    net.InstantiateObject(obj);
                }
                else
                    sourceSlot.Clear();
                targetSlot.Object = obj;
                return;
            }
            if (sourceSlot.Object.IDType == targetSlot.Object.IDType)
            {
                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                {
                    targetSlot.StackSize += sourceSlot.StackSize;
                    net.DisposeObject(sourceSlot.Object.RefID);
                    sourceSlot.Clear();
                    //merge slots
                    return;
                }
            }
            else
                if (amount < sourceSlot.StackSize)
                    return;

            if (targetSlot.Filter(obj))
                if (sourceSlot.Filter(targetSlot.Object))
                    targetSlot.Swap(sourceSlot);
        }

        static int PacketIDSequence = 10000;
        internal static int RegisterPacketHandler(Action<IObjectProvider, BinaryReader> handler)
        {
            var id = PacketIDSequence++;
            Server.RegisterPacketHandler(id, handler);
            Client.RegisterPacketHandler(id, handler);
            return id;
        }
    }
}
