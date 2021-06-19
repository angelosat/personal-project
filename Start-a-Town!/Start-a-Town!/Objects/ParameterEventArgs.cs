using System;

namespace Start_a_Town_
{
    public class ParameterEventArgs : EventArgs
    {
        public object[] Parameters;
        public ParameterEventArgs(params object[] p)
        {
            this.Parameters = p;
        }
    }
}
