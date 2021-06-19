using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Start_a_Town_
{
    sealed public class KeyBind
    {
        public string Name;
        public Keys Key;
        KeyBind(string name, Keys key)
        {
            this.Name = name;
            this.Key = key;
        }
        public static readonly KeyBind ToggleForbidden = new KeyBind("Toggle Forbidden", Keys.F);
        public static readonly KeyBind Cancel = new KeyBind("Cancel", Keys.C);
        public static readonly KeyBind Build = new KeyBind("Build", Keys.B);
        public static readonly KeyBind DigMine = new KeyBind("DigMine", Keys.M);
        public static readonly KeyBind Deconstruct = new KeyBind("Deconstruct", Keys.X);
        public static readonly KeyBind SliceZ = new KeyBind("Slice to Z-Level", Keys.Z);
        public static readonly KeyBind BlockTargeting = new KeyBind("Block Targeting", Keys.V);
    }
}
