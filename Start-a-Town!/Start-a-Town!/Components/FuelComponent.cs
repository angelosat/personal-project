using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class FuelComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Fuel";
            }
        }

        public int Power { get { return (int)this["Power"]; } set { this["Power"] = value; } }

        public FuelComponent()
        {
            this.Power = 0;
        }

        public FuelComponent Initialize(int power = 1)
        {
            this.Power = power;
            return this;
        }

        FuelComponent(int power = 1)
        {
            this.Power = power;
            
        }

        public override object Clone()
        {
            return new FuelComponent(this.Power);
        }

        static public int GetPower(GameObject obj)
        {
            FuelComponent fuel;
            if (!obj.TryGetComponent<FuelComponent>("Fuel", out fuel))
                return 0;
            return fuel.Power;
        }
    }
}
