using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class CanProduce : Reaction.Reagent.ReagentFilter
    {
        //List<Reaction.Product.Types> Types { get; set; }
        //public MaterialCanProduce(params Reaction.Product.Types[] types)
        //{
        //    foreach (var type in types)
        //        this.Types.Add(type);
        //}

        public override string Name
        {
            get
            {
                return "Can Produce";
            }
        }

        Reaction.Product.Types Type { get; set; }
        public CanProduce(Reaction.Product.Types type)
        {
            this.Type = type;
        }

        public override bool Condition(Entity obj)
        {
            return obj?.Def?.CanProcessInto.Contains(this.Type) ?? false;
            return obj.GetComponent<ReagentComponent>()?.CanProduce(this.Type) ?? false;

            ReagentComponent reagent;
            if (!obj.TryGetComponent<ReagentComponent>(out reagent))
                return false;
            return reagent.Products.Contains(this.Type);
        }

        public override string ToString()
        {
            return Name + ": " + this.Type.ToString();
        }
    }
}
