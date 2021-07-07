using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public partial class LightingEngine
    {
        public class BatchToken
        {
            public Queue<Vector3> Queue = new();
            public Action Callback = () => { };
            public BatchToken()
            {

            }
            public BatchToken(IEnumerable<Vector3> positions)
            {
                this.Queue = new Queue<Vector3>(positions);
            }

            public BatchToken(Action callback)
            {
                this.Callback = callback;
            }
            public BatchToken(IEnumerable<Vector3> positions, Action callback)
            {
                this.Callback = callback;
                this.Queue = new Queue<Vector3>(positions);
            }
        }
    }
}
