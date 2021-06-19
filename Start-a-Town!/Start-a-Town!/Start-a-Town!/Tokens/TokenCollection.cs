using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Tokens
{
    public class TokenCollection
    {
        
        Dictionary<string, Token> List = new Dictionary<string, Token>();
        public void Add(Token token)
        {
            this.List.Add(token.Name, token);
        }
        public T Get<T>() where T : Token
        {
            var found = this.List.FirstOrDefault(f => f.Value is T);
            return found as T;
        }
    }
}
