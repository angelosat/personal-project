using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns
{
    public class Zone
    {
        public Town Town;
        public int ID;
        public Vector3 Begin;
        public int Width, Height;

        public Zone(Town town, int id, Vector3 global, int w, int h)
        {
            this.Town = town;
            this.ID = id;
            this.Begin = global;
            this.Width = w;
            this.Height = h;
        }

        public bool Contains(Vector3 position)
        {
            var box = this.Begin.GetBox(this.Width, this.Height, 1);
            return box.Contains(position);
        }

        public virtual void OnGameEvent(GameEvent e) { }
    }
}
