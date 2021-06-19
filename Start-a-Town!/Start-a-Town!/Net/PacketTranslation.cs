using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    // TODO: register all and make a factory
    public abstract class PacketTranslator
    {
        public enum Types : byte
        {
            Byte,
            Short,
            Int32,
            Long,
            Single,
            Double,
            ByteArray,
            String,
            UInt16,
            UInt32,
            Bool,
            Vector2,
            Vector3
        }

        public abstract PacketTranslator Translate(IObjectProvider objProvider, byte[] data);
        public abstract PacketTranslator Translate(IObjectProvider objProvider, object[] parameters);
    }

    class DirectionEventArgs : PacketTranslator
    {
        public Vector3 Direction;

        //static public AttackEventArgs Translate(byte[] data)
        //{
        //    using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
        //    {
        //        return new AttackEventArgs() { Direction = reader.ReadVector3() };
        //    }
        //}

        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                Direction = reader.ReadVector3();
            }
            return this;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            Direction = (Vector3)parameters[0];
            return this;
        }
        static public void Translate(byte[] data, out Vector3 direction)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                direction = reader.ReadVector3();
            }
        }
    }
    class MoveEventArgs : PacketTranslator
    {
        public Vector3 Direction;
        public float Speed;
        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                Direction = reader.ReadVector3();
                Speed = reader.ReadSingle();
            }
            return this;
        }
        static public void Translate(byte[] data, out Vector3 direction, out float speed)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                direction = reader.ReadVector3();
                speed = reader.ReadSingle();
            }
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            Direction = (Vector3)parameters[0];
            Speed = (float)parameters[1];
            return this;
        }
    }
    class SenderEventArgs : PacketTranslator
    {
        //public int SenderID;
        public GameObject Sender;
        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                int id = reader.ReadInt32();
                // get correct object according to remote/local
                Sender = objProvider.GetNetworkObject(id);
            }
            return this;
        }
        static public void Translate(byte[] data, IObjectProvider objProvider, out GameObject sender)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                int id = reader.ReadInt32();
                sender = objProvider.GetNetworkObject(id);
            }
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            //int id = (int)parameters[0];
            //Sender = objProvider.GetNetworkObject(id);
            Sender = parameters[0] as GameObject;
            return this;
        }
    }
    //class AbilityEventArgs : PacketTranslator
    //{
    //    public Script.Types AbilityID;
    //    //public GameObject Actor;
    //    public TargetArgs Target;
    //    public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
    //    {
    //        using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
    //        {
    //            AbilityID = (Script.Types)reader.ReadInt32();
    //            //Actor = objProvider.GetNetworkObject(reader.ReadInt32());
    //            Target = TargetArgs.Read(objProvider, reader);
    //        }
    //        return this;
    //    }
    //    public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
    //    {
    //        AbilityID = (Script.Types)parameters[0];
    //        Target = parameters[1] as TargetArgs;
    //        return this;
    //    }
    //    //static public void Translate(byte[] data, IObjectProvider objProvider, out Script.Types abilityID, out GameObject actor, out GameObject target)
    //    //{
    //    //    using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
    //    //    {
    //    //        abilityID = (Script.Types)reader.ReadInt32();
    //    //        actor = objProvider.GetNetworkObject(reader.ReadInt32());
    //    //        target = objProvider.GetNetworkObject(reader.ReadInt32());
    //    //        reader.ReadVector3();
    //    //    }
    //    //}
    //    static public void Translate(byte[] data, IObjectProvider objProvider, out Script.Types abilityID, out GameObject actor, out GameObject target, out Vector3 face)
    //    {
    //        using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
    //        {
    //            abilityID = (Script.Types)reader.ReadInt32();
    //            actor = objProvider.GetNetworkObject(reader.ReadInt32());
    //            int targetID = reader.ReadInt32();
    //            if (targetID == 0)
    //            {
    //                target = null;
    //                face = Vector3.Zero;
    //                return;
    //            }
    //            target = objProvider.GetNetworkObject(reader.ReadInt32());
    //            face = reader.ReadVector3();
    //        }
    //    }
      
    //}

    class InventoryEventArgs : PacketTranslator
    {
        public int SlotID;
        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                SlotID = reader.ReadInt32();
            }
            return this;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            SlotID = (int)parameters[0];
            return this;
        }

        static public void Translate(byte[] data, IObjectProvider objProvider, out int slotID)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                slotID = reader.ReadInt32();
            }
        }
    }

    //class ScriptEventArgs : PacketTranslator
    //{
    //    public Script.Types ScriptID;
    //    public TargetArgs Target;
    //    public byte[] Parameters;

    //    public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
    //    {
    //        using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
    //        {
    //            this.ScriptID = (Script.Types)reader.ReadInt32();
    //            this.Target = TargetArgs.Read(objProvider, reader);
    //            this.Parameters = reader.ReadBytes(reader.ReadInt32());
    //        }
    //        return this;
    //    }
    //    public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
    //    {
    //        this.ScriptID = (Script.Types)parameters[0];
    //        this.Target = parameters[1] as TargetArgs;
    //        this.Parameters = parameters[2] as byte[];
    //        return this;
    //    }
    //    //static public void Read(BinaryReader reader)
    //    //{
    //    //    Script.Types scriptID = (Script.Types)reader.ReadInt32();
    //    //    TargetArgs target = TargetArgs.Read(e.Network, reader);
    //    //}
    //}

    public class ContainerOperationArgs : PacketTranslator
    {
        public TargetArgs SourceEntity { get; set; }
        public TargetArgs Object { get; set; }
        public byte SourceContainerID { get; set; }
        public byte SourceSlotID { get; set; }
        public byte TargetContainerID { get; set; }
        public byte TargetSlotID { get; set; }
        public byte Amount { get; set; }

        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                this.SourceEntity = TargetArgs.Read(objProvider, reader);
                this.Object = TargetArgs.Read(objProvider, reader);
                this.SourceContainerID = reader.ReadByte();
                this.SourceSlotID = reader.ReadByte();
                this.TargetContainerID = reader.ReadByte();
                this.TargetSlotID = reader.ReadByte();
                this.Amount = reader.ReadByte();
            }
            return this;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            int n = 0;
            this.SourceEntity = parameters[n++] as TargetArgs; // PROBLEM? was 0
            this.Object = parameters[n++] as TargetArgs;
            this.SourceContainerID = (byte)parameters[n++];
            this.SourceSlotID = (byte)parameters[n++];
            this.TargetContainerID = (byte)parameters[n++];
            this.TargetSlotID = (byte)parameters[n++];
            this.Amount = (byte)parameters[n++];
            return this;
        }
        static public void Write(BinaryWriter writer, TargetArgs sourceEntity, TargetArgs obj, byte sourceContainerId, byte sourceID, byte targetContainerID, byte targetID, byte amount)
        {
            sourceEntity.Write(writer);
            obj.Write(writer);
            writer.Write(sourceContainerId);
            writer.Write(sourceID);
            writer.Write(targetContainerID);
            writer.Write(targetID);
            writer.Write(amount);
        }
        public void Write(BinaryWriter writer)
        {
            this.SourceEntity.Write(writer);
            this.Object.Write(writer);
            writer.Write(this.SourceContainerID);
            writer.Write(this.SourceSlotID);
            writer.Write(this.TargetContainerID);
            writer.Write(this.TargetSlotID);
            writer.Write(this.Amount);
        }
    }

    public class ArrangeChildrenArgs : PacketTranslator
    {
        public TargetArgs SourceEntity { get; set; }
        public TargetArgs SourceObject { get; set; }
        public TargetArgs Object { get; set; }
        public byte SourceSlotID { get; set; }
        public byte TargetSlotID { get; set; }
        public byte Amount { get; set; }
        //public ArrangeChildrenArgs(TargetArgs sourceEntity, TargetArgs sourceobj, TargetArgs obj, byte sourceSlotID, byte targetSlotID, byte amount)
        //{
        //    this.SourceEntity = sourceEntity;
        //    this.SourceObject = sourceobj;
        //    this.Object = obj;
        //    this.SourceSlotID = sourceSlotID;
        //    this.TargetSlotID = targetSlotID;
        //    this.Amount = amount;
        //}
        static public ArrangeChildrenArgs Translate(IObjectProvider objProvider, BinaryReader reader)
        {
            var a = new ArrangeChildrenArgs();
            a.SourceEntity = TargetArgs.Read(objProvider, reader);
            a.SourceObject = TargetArgs.Read(objProvider, reader);
            a.Object = TargetArgs.Read(objProvider, reader);
            a.SourceSlotID = reader.ReadByte();
            a.TargetSlotID = reader.ReadByte();
            a.Amount = reader.ReadByte();
            return a;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                this.SourceEntity = TargetArgs.Read(objProvider, reader);
                this.SourceObject = TargetArgs.Read(objProvider, reader);
                this.Object = TargetArgs.Read(objProvider, reader);
                this.SourceSlotID = reader.ReadByte();
                this.TargetSlotID = reader.ReadByte();
                this.Amount = reader.ReadByte();
            }
            return this;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            int n = 0;
            this.SourceEntity = parameters[n++] as TargetArgs;
            this.SourceObject = parameters[n++] as TargetArgs;
            this.Object = parameters[n++] as TargetArgs;
            this.SourceSlotID = (byte)parameters[n++];
            this.TargetSlotID = (byte)parameters[n++];
            this.Amount = (byte)parameters[n++];
            return this;
        }
        static public void Write(BinaryWriter writer, TargetArgs sourceEntity, TargetArgs sourceobj, TargetArgs obj, byte sourceSlotID, byte targetSlotID, byte amount)
        {
            sourceEntity.Write(writer);
            sourceobj.Write(writer);
            obj.Write(writer);
            writer.Write(sourceSlotID);
            writer.Write(targetSlotID);
            writer.Write(amount);
        }
        public void Write(BinaryWriter writer)
        {
            this.SourceEntity.Write(writer);
            this.SourceObject.Write(writer);
            this.Object.Write(writer);
            writer.Write(this.SourceSlotID);
            writer.Write(this.TargetSlotID);
            writer.Write(this.Amount);
        }
    }

    public class ArrangeInventoryEventArgs : PacketTranslator
    {
        public TargetArgs SourceEntity { get; set; }
        public TargetArgs SourceObject { get; set; }
        public TargetArgs Object { get; set; }
        public byte SourceContainerID { get; set; }
        public byte SourceSlotID { get; set; }
        public byte TargetContainerID { get; set; }
        public byte TargetSlotID { get; set; }
        public byte Amount { get; set; }

        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                this.SourceEntity = TargetArgs.Read(objProvider, reader);
                this.SourceObject = TargetArgs.Read(objProvider, reader);
                this.Object = TargetArgs.Read(objProvider, reader);
                //this.SourceContainerID = reader.ReadByte();
                //this.SourceSlotID = reader.ReadByte();
                this.TargetContainerID = reader.ReadByte();
                this.TargetSlotID = reader.ReadByte();
                this.Amount = reader.ReadByte();
            }
            return this;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            int n = 0;
            this.SourceEntity = parameters[n++] as TargetArgs; // PROBLEM? was 0
            //this.SourceContainerID = (byte)parameters[n++];
            //this.SourceSlotID = (byte)parameters[n++];
            this.SourceObject = parameters[n++] as TargetArgs;
            this.Object = parameters[n++] as TargetArgs;
            this.TargetContainerID = (byte)parameters[n++];
            this.TargetSlotID = (byte)parameters[n++];
            this.Amount = (byte)parameters[n++];
            return this;
        }
        //static public void Write(BinaryWriter writer, TargetArgs sourceEntity, byte sourceContainerId, byte sourceID, byte targetContainerID, byte targetID, byte amount)
        static public void Write(BinaryWriter writer, TargetArgs sourceEntity, TargetArgs sourceobj, TargetArgs obj, byte targetContainerID, byte targetID, byte amount)
        {
            sourceEntity.Write(writer);
            sourceobj.Write(writer);
            obj.Write(writer);
            //writer.Write(sourceContainerId);
            //writer.Write(sourceID);
            writer.Write(targetContainerID);
            writer.Write(targetID);
            writer.Write(amount);
        }
        //static public void Read(BinaryReader reader)
        //{
        //    Script.Types scriptID = (Script.Types)reader.ReadInt32();
        //    TargetArgs target = TargetArgs.Read(e.Network, reader);
        //}
    }

    class UseItemEventArgs : PacketTranslator
    {
        public TargetArgs SourceEntity { get; set; }
        public TargetArgs UsedItem { get; set; }
        public Vector3 Face { get; set; }

        public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                this.SourceEntity = TargetArgs.Read(objProvider, reader);
                this.UsedItem = TargetArgs.Read(objProvider, reader);
                this.Face = reader.ReadVector3();
            }
            return this;
        }
        public override PacketTranslator Translate(IObjectProvider objProvider, object[] parameters)
        {
            int n = 0;
            this.SourceEntity = parameters[n++] as TargetArgs;
            this.UsedItem = parameters[n++] as TargetArgs;
            this.Face = (Vector3)parameters[n++];
            return this;
        }
        static public void Write(BinaryWriter writer, TargetArgs sourceEntity, TargetArgs usedItem, Vector3 face)
        {
            sourceEntity.Write(writer);
            usedItem.Write(writer);
            writer.Write(face);
        }
        //static public void Read(BinaryReader reader)
        //{
        //    Script.Types scriptID = (Script.Types)reader.ReadInt32();
        //    TargetArgs target = TargetArgs.Read(e.Network, reader);
        //}
    }


    ///// <summary>
    ///// Make an enum for type and put a switch case for each type
    ///// </summary>
    //class GenericEventArgs : PacketTranslator
    //{
    //    public Dictionary<string, object> Args;
    //    public GenericEventArgs()
    //    {
    //        this.Args = new Dictionary<string, object>();
    //    }
    //    public override PacketTranslator Translate(IObjectProvider objProvider, byte[] data)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    //public GenericEventArgs Translate(IObjectProvider objProvider, byte[] data, params Types[] types)
    //    static public GenericEventArgs Translate(IObjectProvider objProvider, byte[] data, params object[] types)
    //    {
    //        // List<object> args = new List<object>();
    //        //Queue<Types> queue;// = new Queue<Types>(types);
    //        GenericEventArgs gen = new GenericEventArgs();
    //        Queue<object> temp = new Queue<object>(types);
    //        Queue<Tuple<string, Types>> queue = new Queue<Tuple<string, Types>>();
    //        while (temp.Count > 0)
    //        {
    //            queue.Enqueue(Tuple.Create((string)temp.Dequeue(), (Types)temp.Dequeue())); //<string, Types>
    //        }

    //        using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
    //            while (queue.Count > 0)
    //            {
    //                var item = queue.Dequeue();
    //                object value;
    //                switch (item.Item2)
    //                {
    //                    case Types.Int32:
    //                        value = reader.ReadInt32();
    //                        break;

    //                    case Types.Bool:
    //                        value = reader.ReadBoolean();
    //                        break;

    //                    case Types.Vector2:
    //                        value = reader.ReadVector2();
    //                        break;

    //                    case Types.Vector3:
    //                        value = reader.ReadVector3();
    //                        break;

    //                    case Types.Single:
    //                        value = reader.ReadSingle();
    //                        break;

    //                    default:
    //                        throw new Exception("Unknown type");

    //                }
    //                gen.Args[item.Item1] = value;
    //            }
    //        return gen;  //args.Add(queue.Dequeue()());
    //    }
    //}


}
