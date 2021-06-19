using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Needs
{
    public class AIAdvertisement
    {
        public readonly Need.Types NeedID;
        public readonly string Name;
        public readonly float Value;

        public AIAdvertisement(string name, float value = 0)
        {
            this.Name = name;
            this.Value = value;
        }
        public AIAdvertisement(Need.Types needID, float value = 0)
        {
            this.NeedID = needID;
            this.Value = value;
        }
    }
}
