using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class GameObjectCollection// SortedDictionary<GameObject.Types, GameObject>
    {
        SortedDictionary<int, GameObject> Dictionary;// = new SortedDictionary<int, GameObject>();

        public GameObject this[GameObject.Types type]
        {
            get { return this.Dictionary[(int)type]; }
            set { this.Dictionary[(int)type] = value; }
        }
        public GameObject this[int type]
        {
            get { return this.Dictionary[type]; }
            set { this.Dictionary[type] = value; }
        }
        public SortedDictionary<int, GameObject>.ValueCollection Values
        {
            get { return Dictionary.Values; }
        }
        public void Add(GameObject obj)
        {
            //Add(obj.ID, obj);
            this.Dictionary.Add((int)obj.IDType, obj);
        }
        public void Add(params GameObject[] objects)
        {
            foreach (GameObject obj in objects)
                Add(obj);
        }
        public SortedDictionary<string, GameObject> ByName()
        {
            //return new SortedDictionary<string, GameObject>(GameObject.Objects.ToDictionary(pair => pair.Value.Name, pair => pair.Value));
            return new SortedDictionary<string, GameObject>(this.Dictionary.ToDictionary(pair => pair.Value.Name, pair => pair.Value));
        }

        public GameObject this[string objName]
        { get { return ByName()[objName]; } }
        public bool TryGetValue(string objName, out GameObject obj)
        {
            if (ByName().TryGetValue(objName, out obj))
                return true;
            return false;
        }
        public bool TryGetValue(int id, out GameObject obj)
        {
            if (this.Dictionary.TryGetValue(id, out obj))
                return true;
            return false;
        }
        internal bool ContainsKey(GameObject.Types id)
        {
            return false;
            //return this.Dictionary.ContainsKey((int)id);
        }
    }
}
