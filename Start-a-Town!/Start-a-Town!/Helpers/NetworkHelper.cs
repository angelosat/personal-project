using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public static class NetworkHelper
    {
        public static void Send(this byte[] data, long packetID, PacketType type, Socket so, EndPoint ip)
        {
            Packet.Create(packetID, type, data).BeginSendTo(so, ip);
        }
        public static T Translate<T>(this byte[] data, IObjectProvider objProvider) where T : PacketTranslator, new()
        {
            return new T().Translate(objProvider, data) as T;
        }
        public static void Translate(this byte[] data, IObjectProvider objProvider, Action<BinaryReader> reader)
        {
            using var r = new BinaryReader(new MemoryStream(data));
            reader(r);
        }
        public static void Translate(this byte[] data, Action<BinaryReader> reader)
        {
            using var r = new BinaryReader(new MemoryStream(data));
            reader(r);
        }
        public static T Deserialize<T>(this byte[] data, Func<BinaryReader, T> reader)
        {
            return Network.Deserialize<T>(data, reader);
        }
        public static void Deserialize(this byte[] data, Action<BinaryReader> reader)
        {
            Network.Deserialize(data, reader);
        }
        public static byte[] GetBytes(this Action<BinaryWriter> writer)
        {
            using var w = new BinaryWriter(new MemoryStream());
            writer(w);
            return (w.BaseStream as MemoryStream).ToArray();
        }
        public static byte[] Decompress(this byte[] compressed)
        {
            using var compressedStream = new MemoryStream(compressed);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zipStream.CopyTo(output);
            return output.ToArray();
        }
        public static byte[] Compress(this byte[] data)
        {
            byte[] compressed;
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                    input.CopyTo(zip);
                compressed = output.ToArray();
            }
            return compressed;
        }

    }
}
