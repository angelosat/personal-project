using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Start_a_Town_.Components.Tokens
{
    public abstract class Token : ICloneable
    {
        public enum TokenTypes { ToolDurability }
        static Dictionary<TokenTypes, Token> _Registry;

        public static Dictionary<TokenTypes, Token> Registry
        {
            get
            {
                if (_Registry == null)
                    Initialize();
                return _Registry;
            }
        }
        static void Initialize()
        {
            _Registry = new Dictionary<TokenTypes, Token>();
            _Registry.Add(TokenTypes.ToolDurability, new Components.Crafting.TokenMadeWithTools());
        }
        static public Token Create(int type)
        {
            return Create((TokenTypes)type);
        }
        static public Token Create(TokenTypes type)
        {
            return Registry[type].Clone() as Token;
        }

        public TokenTypes ID;
        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }
        public virtual SaveTag Save() { return null; }
        public virtual void Load(SaveTag tag) { }
        public abstract object Clone();
    }
}
