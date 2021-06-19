using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Start_a_Town_.Net
{
    public class WorldSnapshot
    {
        public TimeSpan Time;
        public List<ObjectSnapshot> ObjectSnapshots = new List<ObjectSnapshot>();
      //  public Queue<EventSnapshot> EventSnapshots = new Queue<EventSnapshot>();

        public override string ToString()
        {
            //return this.Time.ToString() + " States:" + ObjectSnapshots.Count + " Events:" + EventSnapshots.Count;
            return this.Time.ToString() + " States:" + ObjectSnapshots.Count;// +" Events:" + EventSnapshots.Count;
        }

    }
}
