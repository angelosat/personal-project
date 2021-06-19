using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public struct Selection// : EventArgs
    {
        Dictionary<string, object> Params;// = new Dictionary<string, object>();
        public Object Target;
        public object this[string paramName]
        { set { Params[paramName] = value; } }
        public Selection(Object target)
        {
            Target = target;
            Params = new Dictionary<string, object>();
        }
        public T GetParameter<T>(string name)
        {
            object param;
            if (Params.TryGetValue(name, out param))
                return (T)param;
            return default(T);
        }
        public Selection(Object target, Dictionary<string, object> args)
        {
            Target = target;
            Params = args;
        }
        public void AddParameter(string name, object value)
        {
            Params.Add(name, value);
        }
        public override string ToString()
        {
            string text = "";
            foreach (KeyValuePair<string, object> param in Params)
            {
                if (text.Length > 0)
                    text += '\n';
                text += param.Key + ": " + param.Value;
            }
            return text;
        }

        public void Clear()
        {
            Target = null;
            Params.Clear();
        }
        static public void Copy(Selection source, Selection destination)
        {
            destination.Target = source.Target;
            destination.Params = source.Params;
        }
    }
    //public class Selection// : EventArgs
    //{
    //    //public Tile NextTile, LastTile;
    //    public string Notes;
    //    //public BitVector32 NextParams, Params;
    //    Dictionary<string, object> Params;//,NextChunk;
    //    public Object Target;//,NextTarget;
    //    public object this[string paramName]
    //    { set { Params[paramName] = value; } }
    //    public Selection()
    //    {
    //        Params = new Dictionary<string, object>();
    //    }
    //    public Selection(Object target)
    //    {
    //        Target = target;
    //        Params = new Dictionary<string, object>();
    //    }
    //    public T GetParameter<T>(string name)
    //    {
    //        object param;
    //        if (Params.TryGetValue(name, out param))
    //            return (T)param;
    //        return default(T);
    //    }
    //    public Selection(Object target, Dictionary<string, object> args)
    //    {
    //        Target = target;
    //        Params = args;
    //    }
    //    public void AddParameter(string name, object value)
    //    {
    //        Params.Add(name, value);
    //    }
    //    public override string ToString()
    //    {
    //        string text = "";
    //        foreach (KeyValuePair<string, object> param in Params)
    //        {
    //            if (text.Length > 0)
    //                text += '\n';
    //            text += param.Key + ": " + param.Value;
    //        }
    //        return text;
    //    }

    //    public void Clear()
    //    {
    //        Target = null;
    //        Params.Clear();
    //    }
    //    static public void Copy(Selection source, Selection destination)
    //    {
    //        destination.Target = source.Target;
    //        destination.Params = source.Params;
    //    }
    //}
}
