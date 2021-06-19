using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    struct DesignObject
    {
        public int Variation, Orientation;
        public Block.Types Type;
        public DesignObject(Block.Types type, int variation = 0, int orientation = 0)
        {
            this.Type = type;
            this.Variation = variation;
            this.Orientation = orientation;
        }
    }

    class BuildingPlanComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "BuildingPlan"; }
        }
        public List<Dictionary<Vector3, DesignObject>> Stages { get { return (List<Dictionary<Vector3, DesignObject>>)this["Stages"]; } set { this["Stages"] = value; } }

        public BuildingPlanComponent()
        {
            this.Stages = new List<Dictionary<Vector3, DesignObject>>();
            this.Stages.Add(new Dictionary<Vector3,DesignObject>());
        }

        public override object Clone()
        {
            BuildingPlanComponent comp = new BuildingPlanComponent();
            comp.Stages = new List<Dictionary<Vector3, DesignObject>>();
            foreach (var stage in this.Stages)
                comp.Stages.Add(stage.ToDictionary(foo => foo.Key, foo => foo.Value));
            return comp;
        }

        //static public bool AddStage(GameObject obj)//, Dictionary<Vector3, DesignObject> stage)
        //{
        //    BuildingPlanComponent comp;
        //    if (!obj.TryGetComponent<BuildingPlanComponent>("Project", out comp))
        //        return false;

        //    //comp.Stages[index] = stage;
        //    comp.Stages.Add(new Dictionary<Vector3, DesignObject>()); //  stage);
        //    return true;
        //}
    }
}
