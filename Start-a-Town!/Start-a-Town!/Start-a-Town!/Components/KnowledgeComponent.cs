using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Components
{
    class KnowledgeCollection : List<Knowledge2>
    {
        public override string ToString()
        {
            string text = "";
            foreach (Knowledge2 kn in this)
            {
                text += kn.Object + ": " + kn.Value + "\n";
            }
            return text.TrimEnd('\n');
        }
        public KnowledgeCollection(params Knowledge2[] entries) : base(entries) { }
    }
    struct Knowledge2
    {
        public GameObject.Types Object;
        public float Value;

        public Knowledge2(GameObject.Types obj, float value = 1)
        {
            this.Object = obj;
            this.Value = value;
        }
    }

    class KnowledgeComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Memory";
            }
        }

        public KnowledgeCollection Memories { get { return (KnowledgeCollection)this["Memories"]; } set { this["Memories"] = value; } }


        public MemorizedBlueprints Blueprints { get { return (MemorizedBlueprints)this["Blueprints"]; } set { this["Blueprints"] = value; } }
        static public List<GameObject> GetMemorizedBlueprints(GameObject actor)
        {
            return actor.GetComponent<KnowledgeComponent>().Blueprints.GetMemorizedBlueprints();
        }

        public KnowledgeComponent() 
        { 
            this.Memories = new KnowledgeCollection();
        }
        public KnowledgeComponent Initialize(params Knowledge2[] memories)
        {
            this.Memories.AddRange(memories);
            return this;
        }
        public KnowledgeComponent Initialize(params BlueprintMemory[] blueprints)
        {
            this.Blueprints = new MemorizedBlueprints(blueprints);
            return this;
        }
        public KnowledgeComponent(params Knowledge2[] memories)
            : this()
        {
            this.Memories.AddRange(memories);
        }

        public override object Clone()
        {
            return new KnowledgeComponent(this.Memories.ToArray()) { Blueprints = this.Blueprints.Clone() };
        }

        static public List<GameObject> GetKnownRecipes(GameObject actor)
        {
            List<GameObject> Memorized = new List<GameObject>();
            KnowledgeCollection memorized = new KnowledgeCollection(actor["Memory"].GetProperty<KnowledgeCollection>("Memories")
                .FindAll(foo => GameObject.Objects[foo.Object].Type == ObjectType.Blueprint || GameObject.Objects[foo.Object].Type == ObjectType.Plan)
                .ToArray());
            Memorized.AddRange(memorized.ConvertAll<GameObject>(mem => GameObject.Objects[mem.Object]));
            return Memorized;
        }
    }
}
