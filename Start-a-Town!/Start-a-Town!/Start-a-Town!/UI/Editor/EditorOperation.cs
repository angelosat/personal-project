using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Editor
{
    class EditorOperation : IUndoable
    {
        public enum Types { SetOrigin }

        Types Type;
        Bldg Bldg;
        object[] Parameters;
        EditorOperation UndoOp;
        public bool Performed { get; private set; }

        public EditorOperation(Types type, Bldg bldg, params object[] p)
        {
            this.Type = type;
            this.Bldg = bldg;
            this.Parameters = p.ToArray();
        }

        public bool Perform()
        {
            switch(this.Type)
            {
                case Types.SetOrigin:
                    UndoOp = new EditorOperation(this.Type, this.Bldg, Bldg.Origin);
                    Bldg.Origin = (Vector3)Parameters[0];
                    break;
                default:
                    break;
            }
            this.Performed = true;
            return true;
        }

        public bool Undo()
        {
            if (!Performed)
                return false;
            Performed = !this.UndoOp.Perform();
            return !Performed;
        }
        public bool Redo() 
        { return Perform(); }
        
    }
}
