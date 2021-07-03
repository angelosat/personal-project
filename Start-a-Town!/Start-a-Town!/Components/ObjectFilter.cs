using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public enum FilterType { None, Exclude, Include }

    public class ObjectFilter : Dictionary<string, List<GameObject.Types>>
    {
        public ObjectFilter Clone()
        {
            var f = new ObjectFilter(Type);
            foreach (KeyValuePair<string, List<GameObject.Types>> filter in this)
                f[filter.Key] = filter.Value.ToList();
            return f;
        }


        public FilterType Type;
        public ObjectFilter(FilterType type = FilterType.None)
        {
            this.Type = type;
        }

        public virtual bool Apply(GameObject obj)
        {
            if (obj.Type != ObjectType.Material)
                return false;
            List<GameObject.Types> list;
            switch (Type)
            {
                case FilterType.Include:
                    if (!this.TryGetValue(obj.Type, out list))
                        return false;
                    if (!list.Contains(obj.IDType))
                        return false;
                    break;
                case FilterType.Exclude:
                    if (this.TryGetValue(obj.Type, out list))

                        if (list.Contains(obj.IDType))
                            return false;

                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// Set object filters.
        /// </summary>
        /// <param name="operation">True to add, false to remove filter.</param>
        /// <param name="objects">An param array of objects to set as filters.</param>
        public void Set(bool operation, params GameObject[] objects)
        {
            List<GameObject.Types> list;
            foreach (GameObject obj in objects)
            {
                switch (operation)
                {
                    case true:
                        if (!this.TryGetValue(obj.Type, out list))
                        {
                            this[obj.Type] = new List<GameObject.Types>() { obj.IDType };
                            continue;
                        }
                        if (!list.Contains(obj.IDType))
                            list.Add(obj.IDType);
                        break;
                    default:
                        if (!this.TryGetValue(obj.Type, out list))
                            continue;
                        if (list.Contains(obj.IDType))
                            list.Remove(obj.IDType);
                        if (list.Count == 0)
                            this.Remove(obj.Type);
                        break;
                }
            }
        }

        //public bool Apply(GameObject obj)
        //{
        //    switch (Type)
        //    {
        //        case FilterType.Include:
        //            //if (!this.Contains(obj.Type))
        //            //    return false;
        //            if (!ObjectTypes.Contains(obj.Type))
        //                return false;
        //            if (!ObjectIDs.Contains(obj.ID))
        //                return false;
        //            break;
        //        case FilterType.Exclude:
        //            //if (this.Contains(obj.Type))
        //            //    return false;
        //            if (ObjectTypes.Contains(obj.Type))
        //                return false;
        //            if (ObjectIDs.Contains(obj.ID))
        //                return false;
        //            break;
        //        default:
        //            break;
        //    }
        //    return true;
        //}
    }
}
