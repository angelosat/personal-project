using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class RandomObjectEventArgs : ObjectEventArgs
    {
        public readonly double Value;// { get; set; }
        public RandomObjectEventArgs(double random)
        {
            this.Value = random;
        }
        static public RandomObjectEventArgs Create(Message.Types type, byte[] data, double random)
        {
            return new RandomObjectEventArgs(random) { Type = type, Data = data };
        }
    }
    public class ObjectEventArgs : EventArgs
    {
        private Action _Success = () => { };
        private Action _Fail = () => { };

        public Stack<ObjectEventArgs> Trace = new Stack<ObjectEventArgs>();
        public IObjectProvider Network;
        public byte[] Data;
        public Message.Types Type;
        public GameObject Sender, Target;
        public object[] Parameters;
        public PacketTranslator Translated;
     //   public TargetArgs TargetArgs { get; set; }
        public Action Success { get { return _Success; } set { _Success = value; } }
        public Action Fail { get { return _Fail; } set { _Fail = value; } }
       // public TargetArgs Source { get; set; }
        public Vector3 Face;

        public ObjectEventArgs()
        {
           // this.Network = net;
          //  this.Data = new byte[0]; // maybe leave it null?
            this.Parameters = new object[0];
            this.Data = new byte[0];
        }
        
        public ObjectEventArgs(Message.Types type)
        {
            this.Target = null;
            this.Parameters = new object[0];
            this.Sender = null;
            Type = type;
            this.Face = Vector3.Zero;
        }
        public ObjectEventArgs(Message.Types type, GameObject sender, params object[] parameters)
        {
            this.Target = null;
            this.Face = Vector3.Zero;
            Type = type;
            Sender = sender;
            Parameters = parameters;
        }
        public ObjectEventArgs(Message.Types type, Vector3 face, GameObject sender, params object[] parameters)
        {
            this.Target = null;
            this.Face = face;
            Type = type;
            Sender = sender;
            Parameters = parameters;
        }
        public ObjectEventArgs(Message.Types type, Vector3 face, GameObject sender, GameObject target, params object[] parameters)
        {
            this.Face = face;
            Type = type;
            Sender = sender;
            Parameters = parameters;
            this.Target = target;
        }

        //static public ObjectEventArgs Create(IObjectProvider net, BinaryWriter writer)
        //{
        //    return new ObjectEventArgs().Read(net, reader);
        //}
        static public ObjectEventArgs Create(IObjectProvider net, BinaryReader reader)
        {
            return new ObjectEventArgs().Read(net, reader);
        }
        //static public ObjectEventArgs Create<T>(IObjectProvider net, BinaryReader reader) where T:PacketTranslator , new()
        //{
        //    return new ObjectEventArgs().Read<T>(net, reader);
        //}
        static public ObjectEventArgs Create(Message.Types type, Action success, Action fail, params object[] parameters)
        {
            return new ObjectEventArgs() { Type = type, Parameters = parameters, Success = success, Fail = fail };
        }
        static public ObjectEventArgs Create(Message.Types type, Action success, object[] parameters)
        {
            return new ObjectEventArgs() { Type = type, Parameters = parameters, Success = success };
        }
        static public ObjectEventArgs Create(Message.Types type, object[] parameters)
        {
            return new ObjectEventArgs() { Type = type, Parameters = parameters };
        }
        static public ObjectEventArgs Create(IObjectProvider net, Message.Types type, params object[] parameters)
        {
            return new ObjectEventArgs() { Type = type, Parameters = parameters, Network = net };
        }
        [Obsolete]
        static public ObjectEventArgs Create(Message.Types type, TargetArgs source, params object[] parameters)
        {
            return new ObjectEventArgs() { Type = type, Parameters = parameters };
        }
        [Obsolete]
        static public ObjectEventArgs Create(Message.Types type, TargetArgs source)
        {
            return new ObjectEventArgs() { Type = type };
        }
        [Obsolete]
        static public ObjectEventArgs Create(Message.Types type)
        {
            return new ObjectEventArgs() { Type = type };
        }
        static public ObjectEventArgs Create(Message.Types type, byte[] data)
         {
             return new ObjectEventArgs() { Type = type, Data = data};
         }
        static public ObjectEventArgs Create(Message.Types type, Action<BinaryWriter> writer)
         {
             using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
             {
                 writer(w);
                 return new ObjectEventArgs() { Type = type, Data = (w.BaseStream as MemoryStream).ToArray() };
             }
             
         }
        
        public static void Write(BinaryWriter writer, Message.Types type, Action<BinaryWriter> argsWriter)
         {
             writer.Write((byte)type);
             argsWriter(writer);
         }

        public ObjectEventArgs Read(IObjectProvider net, BinaryReader reader)
        {
            this.Type = (Message.Types)reader.ReadByte();
            //     this.Source = TargetArgs.Read(net, reader);
            //  this.TargetArgs = TargetArgs.Read(net, reader);

            // WARNING!!! better prefix manually
            int dataLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            this.Data = reader.ReadBytes(dataLength);
            this.Network = net;
            return this;
        }
        //public ObjectEventArgs Read<T>(IObjectProvider net, BinaryReader reader) where T : PacketTranslator, new()
        //{
        //    this.Type = (Message.Types)reader.ReadByte();
        //    //     this.Source = TargetArgs.Read(net, reader);
        //    //    this.TargetArgs = TargetArgs.Read(net, reader);

        //    // WARNING!!! better prefix manually
        //    int dataLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
        //    this.Data = reader.ReadBytes(dataLength);
        //    this.Translated = this.Data.Translate<T>(net);
        //    this.Network = net;
        //    return this;
        //} 

        public void Translate(Action<BinaryReader> read)
        {
            this.Data.Translate(this.Network, read);
        }
    }

}
