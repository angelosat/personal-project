using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class SeedComponent : Component
    {
        readonly static public string Name = "Seed";

        public override string ComponentName
        {
            get
            {
                return Name;
            }
        }

        public GameObject.Types Product { get { return (GameObject.Types)this["Product"]; } set { this["Product"] = value; } }
        public int Level { get { return (int)this["Level"]; } set { this["Level"] = value; } }

        public SeedComponent()
        {

        }
        public SeedComponent Initialize(GameObject.Types productID, int level = 1)
        {
            this["Product"] = productID;
            this.Level = 1;
            return this;
        }

        SeedComponent(GameObject.Types productID, int level = 1)
        {
            this["Product"] = productID;
            this.Level = 1;
        }

        public override object Clone()
        {
            SeedComponent comp = new SeedComponent(Product);
        //    foreach (KeyValuePair<string, object> parameter in Properties)
        //        comp[parameter.Key] = parameter.Value;
            return comp;
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interactions.Interaction> actions)
        {
            actions.Add(new PlayerInput(PlayerActions.Interact), new Interactions.Planting());
        }
        public override void GetHauledActions(GameObject parent, TargetArgs target, List<Interactions.Interaction> actions)
        {
            actions.Add(new Interactions.Planting());
        }
        //public override string GetInventoryText(GameObject actor)
        //{
        //    return "Can be planted";
        //}

        
    }
}
