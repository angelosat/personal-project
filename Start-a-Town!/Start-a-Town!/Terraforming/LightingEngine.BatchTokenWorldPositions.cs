using System.Collections.Generic;

namespace Start_a_Town_
{
    public partial class LightingEngine
    {
        public class BatchTokenWorldPositions
        {
            public Queue<WorldPosition> Queue = new();
            public BatchTokenWorldPositions()
            {

            }
            public BatchTokenWorldPositions(IEnumerable<WorldPosition> positions)
            {
                this.Queue = new Queue<WorldPosition>(positions);
            }
        }
    }
}
