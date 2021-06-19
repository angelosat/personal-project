using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Crafting
{
    class Template
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Icon { get; set; }
        public Action<GameObject> OnSuccess { get; set; }
    }
}
