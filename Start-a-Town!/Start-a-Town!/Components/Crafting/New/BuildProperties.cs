using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class BuildProperties
    {
        public Ingredient Ingredient;
        public float ToolSensitivity;

        public BuildProperties(Ingredient ingredient, float toolContribution)
        {
            this.Ingredient = ingredient;
            this.ToolSensitivity = toolContribution;
        }
    }
}
