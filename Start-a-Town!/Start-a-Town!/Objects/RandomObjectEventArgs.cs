using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class RandomObjectEventArgs : ObjectEventArgs
    {
        public readonly double Value;
        public RandomObjectEventArgs(double random)
        {
            this.Value = random;
        }
        static public RandomObjectEventArgs Create(Message.Types type, byte[] data, double random)
        {
            return new RandomObjectEventArgs(random) { Type = type, Data = data };
        }
    }

}
