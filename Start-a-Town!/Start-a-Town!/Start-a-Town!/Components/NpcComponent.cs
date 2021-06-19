using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class NpcComponent : GeneralComponent
    {
        static public event EventHandler<EventArgs> NpcDirectoryChanged;
        static void OnNpcDirectoryChanged()
        {
            if (NpcDirectoryChanged != null)
                NpcDirectoryChanged(null, EventArgs.Empty);
        }

        static public List<GameObject> NpcDirectory = new List<GameObject>();

        public override void Initialize(GameObject parent)
        {
            Name = RandomName();
            NpcDirectory.Add(parent);
            OnNpcDirectoryChanged();
        }

        public NpcComponent() { }
        public NpcComponent(GameObject.Types id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null) : base(id, objType, name, description, quality) { }
        public NpcComponent(int id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null) : base(id, objType, name, description, quality) { }


        static public string RandomName()
        {
            List<string> parts = new List<string>() { "an", "ro", "sta", "da", "be", "an", "stath", "jo", "cam", "gro", "ma", "ob", "the", "pa", "er", "ble", "arn", "old", "ohn", "ni", "ick", "ber", "tie", "dim", "ste", "ve" };
            Random r = new Random();

            string first = "";
            for (int i = 0; i < r.Next(3) + 2; i++)
                first += parts[r.Next(parts.Count)];

            string last = "";
            for (int i = 0; i < r.Next(3) + 2; i++)
                last += parts[r.Next(parts.Count)];

            return char.ToUpper(first[0]) + first.Substring(1) + " " + char.ToUpper(last[0]) + last.Substring(1);
        }

        public override object Clone()
        {
            NpcComponent phys = new NpcComponent(ID, Type, Name, Description, Quality);
            //foreach (KeyValuePair<string, object> property in Properties)
            //{
            //    phys.Properties[property.Key] = property.Value;
            //}
            return phys;
        }



        public override void Despawn(//Net.IObjectProvider net,
                    GameObject parent)
        {
            NpcDirectory.Remove(parent);
            OnNpcDirectoryChanged();
        }
    }
}
