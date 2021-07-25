﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Start_a_Town_
{
    public abstract class Def
    {
        public string Name;
        static public Dictionary<string, Def> Database = new();
        public Def()
        {

        }
        protected Def(string name)
        {
            this.Name = name.Replace(" ", "");
        }
       
        static public void Register(Def def)
        {
            if (def.Name.IsNullEmptyOrWhiteSpace())
                throw new Exception();
            Database.Add(def.Name, def);
        }
        static public Def GetDef(string defName)
        {
            if (defName == null || string.IsNullOrEmpty(defName) || string.IsNullOrWhiteSpace(defName))
                return null;
            if (Database.TryGetValue(defName, out var result))
                return result;
            string.Format("def \"{0}\" does not exist", defName);
            return null;
        }
        static public T GetDef<T>(string defName) where T : Def
        {
            if (TryGetDef<T>(defName) is not T def)
                throw new Exception($"def \"{defName}\" does not exist");
            return def;
        }
        static public T TryGetDef<T>(string defName) where T : Def
        {
            if (defName == null || string.IsNullOrEmpty(defName) || string.IsNullOrWhiteSpace(defName))
                throw new Exception();
            if (Database.TryGetValue(defName, out var result))
                return result as T;
            return null;
        }
        public override string ToString()
        {
            return this.GetType().Name + ": " + this.Name;
        }
        public SaveTag Save(string name = "")
        {
            return this.Name.Save(name);
        }
        public void Save(SaveTag tag, string name = "")
        {
            tag.Add(this.Name.Save(name));
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Name);
        }
        internal static IEnumerable<T> GetDefs<T>() where T: Def
        {
            return Database.Values.OfType<T>();
        }
    }
}
