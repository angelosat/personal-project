using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class PropertyComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Property";
            }
        }

        public List<GameObject> Property { get { return (List<GameObject>)this["Property"]; } set { this["Property"] = value; } }

        public PropertyComponent()
        {
            this.Property = new List<GameObject>();
        }

        public override string ToString()
        {
            string text = "";
            foreach (GameObject obj in this.Property)
                text += obj.Name + " " + obj.Global + "\n";
            return text.TrimEnd('\n');
        }

        static public bool TryFindProperty(GameObject owner, Predicate<GameObject> filter, out GameObject property)
        {
            PropertyComponent prop;
            property = null;
            if(!owner.TryGetComponent<PropertyComponent>("Property", out prop))
                return false;
            foreach(GameObject obj in prop.Property)
                if (filter(obj))
                {
                    property = obj;
                    return true;
                }
            return false;
        }

        public override object Clone()
        {
            return new PropertyComponent();
        }
    }
}
