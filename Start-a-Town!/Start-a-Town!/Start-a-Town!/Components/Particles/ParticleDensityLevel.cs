using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Particles
{
    class ParticleDensityLevel
    {
        public string Name;
        public float Factor;
        static List<ParticleDensityLevel> _All = new List<ParticleDensityLevel>();

        ParticleDensityLevel(string name, float factor)
        {
            this.Name = name;
            this.Factor = factor;
            _All.Add(this);
        }
        static public readonly ParticleDensityLevel None = new ParticleDensityLevel("None", 0);
        static public readonly ParticleDensityLevel Low = new ParticleDensityLevel("Low", .25f);
        static public readonly ParticleDensityLevel Medium = new ParticleDensityLevel("Medium", .5f);
        static public readonly ParticleDensityLevel High = new ParticleDensityLevel("High", 1);

        static public ParticleDensityLevel Current = High;

        public static List<ParticleDensityLevel> All { get { return _All.ToList(); } }
    }
}
