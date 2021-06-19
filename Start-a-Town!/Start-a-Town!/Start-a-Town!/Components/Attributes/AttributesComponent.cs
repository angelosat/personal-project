using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class AttributeCollection : List<Attribute>// Dictionary<Attribute.Types, int> { }
    {
        public override string ToString()
        {
            string text = "";
            this.ForEach(foo => text += foo.ToString() + "\n");
            return text.TrimEnd('\n');
        }

        public AttributeCollection(params Attribute[] atts)
        {
            this.AddRange(atts);
        }

        public void Update(GameObject parent)
        {
            foreach (var a in this)
                a.Update(parent);
        }
    }

    class AttributesComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Attributes";
            }
        }

        public AttributeCollection Attributes { get { return (AttributeCollection)this["Attributes"]; } set { this["Attributes"] = new AttributeCollection(); } }

        public AttributesComponent()
        {
            this.Attributes = new AttributeCollection();
        }

        public AttributesComponent(params Attribute[] atts)
            : this()
        {
            this.Attributes.AddRange(atts);
        }
        public AttributesComponent Initialize(params Attribute[] atts)
        {
            this.Attributes.AddRange(atts);
            return this;
        }
        public override void Update(GameObject parent)
        {
            this.Attributes.Update(parent);
        }

        public override object Clone()
        {
            //List<Attribute> newAtts = this.Attributes.Select(foo => foo.Clone() as Attribute).ToList();
            IEnumerable<Attribute> newAtts = this.Attributes.Select(foo => foo.Clone() as Attribute);
            Attribute[] atts = newAtts.ToArray();
            //AttributeCollection wtf = new AttributeCollection(){p
            AttributesComponent c = new AttributesComponent()
            {
                //Properties = this.Properties.ToDictionary(foo => foo.Key, foo => foo.Value) as ComponentPropertyCollection
                //  this.Attributes.ForEach(foo=>Attributes.Add(foo.Clone()))

                Properties = new ComponentPropertyCollection() { { "Attributes", new AttributeCollection(atts) } }
                //Attributes = this.Attributes.Select(foo => foo.Clone() as Attribute).ToList() as AttributeCollection
            };
            return c;
        }

        public override string GetStats()
        {
            return this.Attributes.ToString();
        }

        public Attribute GetAttribute(Attribute.Types type)
        {
            return this.Attributes.FirstOrDefault(att => att.ID == type);
        }
        static public Attribute GetAttribute(GameObject parent, Attribute.Types type)
        {
            var comp = parent.GetComponent<AttributesComponent>();
            if (comp == null)
                return null;
            return comp.Attributes.FirstOrDefault(att => att.ID == type);
        }
        static public float GetValueOrDefault(GameObject parent, Attribute.Types type, float dflt)
        {
            var comp = parent.GetComponent<AttributesComponent>();
            if (comp == null)
                return dflt;
            var found = comp.Attributes.FirstOrDefault(att => att.ID == type);
            return found != null ? found.Value : dflt;
        }
    }
}
