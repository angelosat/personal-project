using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Net
{
    public class RandomThreaded
    {
        //World World;
        static Random Random;
        [ThreadStatic]
        static Random _Local;
        Random Local
        {
            get
            {
                if (_Local == null)
                {
                    int seed;
                    //lock (World.Random) seed = World.Random.Next();
                    lock (Random) seed = Random.Next();
                    _Local = new Random(seed);
                    return _Local;
                }
                else
                {
                    Random old = _Local;
                    _Local = new Random(_Local.Next());
                    return old;
                }
            }
        }

        //public RandomThreaded(World world)
        //{
        //    World = world;
        //    //Global = new Random(seed);
        //}
        public RandomThreaded(Random random)
        {
            Random = random;
        }
        public double NextDouble()
        {
            return Local.NextDouble();
        }
        public int Next()
        {
            return Local.Next();
            //Random inst = Local;
            //if (inst.IsNull())
            //{
            //    int seed;
            //    lock (Global) seed = Global.Next();
            //    Local = inst = new Random(seed);
            //}
            //return inst.Next();
        }
        public int Next(int maxValue)
        {
            return Local.Next(maxValue);
        }
        public int Next(int minValue, int maxValue)
        {
            return Local.Next(minValue, maxValue);
        }
    }
}
