using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    public abstract partial class ToolDrawing
    {
        public class Args
        {
            public IntVec3 Begin, End;
            public bool Removing, Replacing, Cheat;
            public Modes Mode;
            public int Orientation;
            public Args(Modes mode, IntVec3 begin, IntVec3 end, bool modkey, bool cheat, bool replacing = false, int orientation = 0)
            {
                this.Mode = mode;
                this.Begin = begin;
                this.End = end;
                this.Removing = modkey;
                this.Replacing = replacing;
                this.Orientation = orientation;
                this.Cheat = cheat;
            }
            public void Write(BinaryWriter w)
            {
                w.Write((byte)this.Mode);
                w.Write(this.Begin);
                w.Write(this.End);
                w.Write(this.Removing);
                w.Write(this.Replacing);
                w.Write(this.Orientation);
                w.Write(this.Cheat);
            }
            public Args(BinaryReader r)
            {
                this.Mode = (Modes)r.ReadByte();
                this.Begin = r.ReadIntVec3();
                this.End = r.ReadIntVec3();
                this.Removing = r.ReadBoolean();
                this.Replacing = r.ReadBoolean();
                this.Orientation = r.ReadInt32();
                this.Cheat = r.ReadBoolean();
            }
        }

       
    }
}
