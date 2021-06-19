using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Tokens
{
    public class IsWorkstation : Token
    {
        public enum Types { None, Workbench, Smeltery, Carpentry }
        public override string Name
        {
            get { return "IsWorkstation"; }
        }
        public Types Type;
        public IsWorkstation(Types type)
        {
            this.Type = type;
        }
        //public static bool Contains(this Block block, Types type)
        //{
        //    IsWorkstation token = block.Tokens.Get<IsWorkstation>();
        //    if (token != null)
        //        return token.Type == type;
        //    else
        //        return false;
        //}
    }
}
