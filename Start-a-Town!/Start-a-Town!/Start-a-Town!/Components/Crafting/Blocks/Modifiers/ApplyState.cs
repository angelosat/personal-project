using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class ApplyState<T> : BlockConstruction.Product.Modifier where T: IBlockState, new()
    {
        IBlockState State { get; set; }
        public ApplyState(string localReagentName):base(localReagentName)
        {

        }
        //public ApplyState(T state)
        //{
        //    this.State = state;
        //}

        Func<T> StateGetter;
        Action<GameObject, T> StateSetter;
        Action<GameObject, byte> DataSetter;
        //public ApplyState(Func<T> stateGetter, Action<GameObject, T> stateSetterFromReagent)
        //{
        //    this.StateGetter = StateGetter;
        //    this.StateSetter = stateSetterFromReagent;
        //}
        //public ApplyState(Action<GameObject, T> stateSetterFromReagent)
        //{
        //    this.State = new T();
        //    this.StateSetter = stateSetterFromReagent;
        //}
        public override void Apply(GameObject reagent, ref byte data)
        {
            this.State.Apply(ref data);
        }
    }
}
