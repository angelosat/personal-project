using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Tokens
{
    public class TokenCollection
    {
        
        Dictionary<string, TokenOld> List = new Dictionary<string, TokenOld>();
        public void Add(TokenOld token)
        {
            this.List.Add(token.Name, token);
        }
        public T Get<T>() where T : TokenOld
        {
            var found = this.List.FirstOrDefault(f => f.Value is T);
            return found as T;
        }
    }
}
