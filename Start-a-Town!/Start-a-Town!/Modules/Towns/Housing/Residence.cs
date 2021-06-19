using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns.Housing
{
    public class Residence
    {
        public Town Town;
        public Guid Owner;// = Guid.NewGuid();
        public int ID;
        public HashSet<Vector3> Positions = new HashSet<Vector3>();
        //public string Name = "untitled residence";
        string _Name;
        public string Name { get { return !string.IsNullOrWhiteSpace(_Name) ? _Name : "Residence " + this.ID.ToString(); }
            set { _Name = value; }
        }

        public Residence()
        {

        }
        public Residence(Town town, int id, Vector3 global, int w, int h)
        {
            this.Town = town;
            this.ID = id;
            this.Positions = new HashSet<Vector3>(global.GetRectangle(w, h));
        }

        internal bool Edit(Vector3 global, int w, int h, bool remove)
        {
            var vectors = global.GetRectangle(w, h);
            if (!remove)
                this.Add(vectors);
            else
                this.Remove(vectors);
            return true;
        }

        private void Remove(List<Vector3> vectors)
        {
            foreach (var v in vectors)
                this.Positions.Remove(v);
        }

        private void Add(List<Vector3> vectors)
        {
            foreach (var v in vectors)
                this.Positions.Add(v);
        }
    }
}
