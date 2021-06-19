using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Tokens
{
    public class TokensComponent : Component
    {
        public override string ComponentName
        {
            get { return "Tokens"; }
        }
        public override object Clone()
        {
            return new TokensComponent();
        }
        List<Token> Tokens = new List<Token>();
        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            Color c = Color.MediumSeaGreen;
            foreach (var token in this.Tokens)
                tooltip.Controls.Add(new Label(token.ToString()) { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => c });
        }
        static public bool AddToken(GameObject obj, Token token)
        {
            TokensComponent tokensComp;
            if(obj.TryGetComponent<TokensComponent>(out tokensComp))
                tokensComp.Tokens.Add(token);
            return true;
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Tokens.Count);
            foreach (var t in this.Tokens)
            {
                w.Write((int)t.ID);
                t.Write(w);
            }
        }
        public override void Read(System.IO.BinaryReader r)
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var id = r.ReadInt32();
                var token = Token.Create(id);
                token.Read(r);
                this.Tokens.Add(token);
            }
        }
    }
}
