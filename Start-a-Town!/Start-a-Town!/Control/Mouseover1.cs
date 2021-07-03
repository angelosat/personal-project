using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class Mouseover<T> 
    {
        public T Object;
        public bool Multifaceted;
        public Color Face;
        public float Depth = 1;

        //public void Set(object obj)
        //{
        //    this.Object = obj;
        //}
        public bool TrySet(float depth, T obj, Color face)
        {
            if (depth > this.Depth)
                return false;
            //    Console.WriteLine(depth + " this." + this.Depth);
            this.Object = obj;
            this.Depth = depth;
            this.Face = face;
            return true;
        }

        //public bool TryGet(out object obj)
        //{
        //    obj = this.Object;
        //    return obj != null;
        //}

        public bool TryGet(out T obj)
        {
            obj = this.Object;
            return obj is T;
        }

        //public void Reset()
        //{ Object = null; }
    }
}
