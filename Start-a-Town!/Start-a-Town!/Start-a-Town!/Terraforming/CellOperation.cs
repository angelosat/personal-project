using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class CellOperation : IUndoable
    {
        public IObjectProvider Net;
        public Vector3 Global;
        public Block.Types Type;
        public byte Data;
        public int Variation, Orientation;
        public bool Performed { get; private set; }

        CellOperation UndoOperation;

        public CellOperation(IObjectProvider net, Vector3 global, Block.Types type, int variation = 0, int orientation = 0)
        {
            this.Net = net;
            this.Global = global;
            this.Type = type;
            this.Variation = variation;
            this.Orientation = orientation;
        }
        public bool Perform()
        {
            this.Performed = this.Global.TrySetCell(Net, Type, 0, Variation, Orientation, out UndoOperation);
            //this.Map.SetCell(this);
            //Performed = true;
            return Performed;
        }
        public bool Perform(IMap map, Vector3 loc)
        {
            //map.SetCell(loc + Global, Type, Variation, Orientation);
            return Performed;
        }
        public bool Undo()
        {
            if (!Performed)
                return false;
            Performed = !this.UndoOperation.Perform();
            return !Performed;
        }
        public bool Redo()
        {
            return Perform();
        }

        public SaveTag Save()
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound);
            tag.Add(new SaveTag(SaveTag.Types.Int, "X", (int)this.Global.X));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Y", (int)this.Global.Y));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Z", (int)this.Global.Z));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Orientation", Orientation));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Variation", Variation));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)this.Type));
            return tag;
        }
        static public CellOperation Load(IObjectProvider net, SaveTag tag)
        {
            int x = (int)tag["X"].Value;
            int y = (int)tag["Y"].Value;
            int z = (int)tag["Z"].Value;
            int o = (int)tag["Orientation"].Value;
            int v = (int)tag["Variation"].Value;
            int t = (int)tag["Type"].Value;

            return new CellOperation(net, new Vector3(x, y, z), (Block.Types)t, v, o);
        }

        public override string ToString()
        {
            return Global.ToString() + ": " + Type.ToString();
        }
    }
}
