using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    [Obsolete]
    public class CanBeMadeInto
    {
        public HashSet<MaterialType.RawMaterial> Templates;

        public CanBeMadeInto(params MaterialType.RawMaterial[] templates)
        {
            this.Templates = new HashSet<MaterialType.RawMaterial>(templates);
        }
    }
}
