using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Tokens;

namespace Start_a_Town_.Components.Crafting
{
    class TokenMadeWithTools : Token
    {
        int Value;
        public TokenMadeWithTools()
        {
            this.ID = TokenTypes.ToolDurability; 
        }
        public TokenMadeWithTools(int value)
        {
            this.ID = TokenTypes.ToolDurability; 
            this.Value = value;
        }
        public override string ToString()
        {
            return "This item has been crafted with tools. (" + this.Value.ToString("+##;-##;##") + " Durability)";
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Value);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Value = r.ReadInt32();
        }
        public override object Clone()
        {
            return new TokenMadeWithTools();
        }
    }
}
