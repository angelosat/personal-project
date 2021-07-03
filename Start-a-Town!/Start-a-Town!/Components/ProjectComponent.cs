using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class ProjectComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Project"; }
        }
        static List<GameObject> _ProjectList;
        static public List<GameObject> ProjectList
        {
            get
            {
                if (_ProjectList == null)
                    _ProjectList = new List<GameObject>();
                return _ProjectList;
            }
            set { _ProjectList = value; }
        }

        static public GameObject Create(string name)
        {
            GameObject project = new GameObject();
            project["Info"] = new DefComponent(GameObject.Types.Project, objType: ObjectType.Project, name: name, description: "A construction project");
            project["Project"] = new ProjectComponent();
            ProjectList.Add(project);
            return project;
        }

        public Dictionary<Vector3, Block.Types> Tiles { get { return (Dictionary<Vector3, Block.Types>)this["Tiles"]; } set { this["Tiles"] = value; } }

        public ProjectComponent()
        {
            Tiles = new Dictionary<Vector3, Block.Types>();
        }

        public override object Clone()
        {
            return new ProjectComponent();
        }
    }
}
