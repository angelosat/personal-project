using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class EventSnapshot
    {
        public TimeSpan Time { get; set; }
        public GameObject Recipient { get; set; }
        public byte[] ObjectEventArgsData { get; set; }

        //public static EventSnapshot Read(BinaryReader reader)
        //{
        //    return new EventSnapshot() { Time = TimeSpan.FromMilliseconds(reader.ReadDouble()), Data = reader.ReadBytes(reader.ReadInt32()) };
        //}

        public EventSnapshot()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectEventArgsData">A serialized ObjectEventArgs.</param>
        public EventSnapshot(byte[] objectEventArgsData)
        {
            this.ObjectEventArgsData = objectEventArgsData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="objectEventArgsData">A serialized ObjectEventArgs.</param>
        public EventSnapshot(GameObject recipient, byte[] objectEventArgsData)
        {
            this.Recipient = recipient;
            this.ObjectEventArgsData = objectEventArgsData;
        }

        /// <summary>
        /// Creates an EventSnapshot from a reader, with the provided timestamp
        /// </summary>
        /// <param name="time"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
         public static EventSnapshot Read(TimeSpan time, IObjectProvider net, BinaryReader reader)
        {
            return new EventSnapshot()
            {
                Time = time,
                Recipient = TargetArgs.Read(net, reader).Object,
                ObjectEventArgsData = reader.ReadBytes(reader.ReadInt32())
            };
        }
        //public static EventSnapshot Read(TimeSpan time, BinaryReader reader)
        //{
        //    return new EventSnapshot()
        //    {
        //        Time = time,
        //        ObjectEventArgsData = reader.ReadBytes(reader.ReadInt32())
        //    };
        //}

        public void Write(BinaryWriter writer)
        {
            // writer.Write(Time.TotalMilliseconds);
            TargetArgs.Write(writer, Recipient);
            writer.Write(ObjectEventArgsData.Length);
            writer.Write(ObjectEventArgsData);
        }

        public void Execute(IObjectProvider net)
        {
            //Network.Deserialize(this.ObjectEventArgsData, reader =>
            //{
            //    Components.Message.Types type = (Components.Message.Types)reader.ReadByte();

            //    TargetArgs targetArgs = TargetArgs.Read(net, reader);
            //    GameObject recipient = targetArgs.Object;

            //    List<byte> p = new List<byte>();

            //    while (reader.BaseStream.Position < reader.BaseStream.Length)
            //        p.Add(reader.ReadByte());

            //    recipient.PostMessage(type, recipient, net, p.ToArray());
            //});

            net.PostLocalEvent(this.Recipient, this.ObjectEventArgsData.Deserialize<ObjectEventArgs>(r =>
            {
                return ObjectEventArgs.Create(net, r);
            }));

            //Network.Deserialize(this.Data, reader =>
            //{
            //    Components.Message.Types type = (Components.Message.Types)reader.ReadByte();

            //    TargetArgs targetArgs = TargetArgs.Read(net, reader);
            //    GameObject recipient = targetArgs.Object;

            //    List<byte> p = new List<byte>();

            //    while (reader.BaseStream.Position < reader.BaseStream.Length)
            //        p.Add(reader.ReadByte());

            //    recipient.PostMessage(type, recipient, net, p.ToArray());
            //});
        }
    }
}
