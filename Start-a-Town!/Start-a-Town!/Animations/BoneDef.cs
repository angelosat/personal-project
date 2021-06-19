using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class BoneDef : Def
    {
        //public enum Types { None, Hips, Torso, RightHand, LeftHand, RightFoot, LeftFoot, Head, Mainhand, Offhand, Hauled, Helmet, EquipmentHead, EquipmentHandle, Item }
        public string Label;
        BoneDef(string name) : base(name)
        {
     
            this.Label = name;
        }
        BoneDef(string name, string label) : base(name)
        {
          
            this.Label = label;
        }
        static BoneDef Create(string name, string label)// = "")
        {
            var n = new BoneDef(name, label);
            Register(n);
            return n;
        }
        static BoneDef Create(string name)
        {
            return Create(name, name);
        }
        static public readonly BoneDef Hips = Create("Hips");
        static public readonly BoneDef Torso = Create("Torso");
        static public readonly BoneDef RightHand = Create("RightHand", "Right Hand");
        static public readonly BoneDef LeftHand = Create("LeftHand", "Left Hand");
        static public readonly BoneDef RightFoot = Create("RightFoot", "Right Foot");
        static public readonly BoneDef LeftFoot = Create("LeftFoot", "Left Foot");
        static public readonly BoneDef Head = Create("Head");
        static public readonly BoneDef Mainhand = Create("Mainhand");
        static public readonly BoneDef Offhand = Create("Offhand");
        static public readonly BoneDef Hauled = Create("Hauled");
        static public readonly BoneDef Helmet = Create("Helmet");
        static public readonly BoneDef EquipmentHead = Create("EquipmentHead", "Head");
        static public readonly BoneDef EquipmentHandle = Create("EquipmentHandle", "Handle");
        static public readonly BoneDef Item = Create("Item");
        static public readonly BoneDef PlantStem = Create("PlantStem", "Plant Stem");
        static public readonly BoneDef TreeTrunk = Create("TreeTrunk", "Tree Trunk");

        //static public readonly BoneDef Hips = new BoneDef("Hips");
        //static public readonly BoneDef Torso = new BoneDef("Torso");
        //static public readonly BoneDef RightHand = new BoneDef("RightHand", "Right Hand");
        //static public readonly BoneDef LeftHand = new BoneDef("LeftHand", "Left Hand");
        //static public readonly BoneDef RightFoot = new BoneDef("RightFoot", "Right Foot");
        //static public readonly BoneDef LeftFoot = new BoneDef("LeftFoot", "Left Foot");
        //static public readonly BoneDef Head = new BoneDef("Head");
        //static public readonly BoneDef Mainhand = new BoneDef("Mainhand");
        //static public readonly BoneDef Offhand = new BoneDef("Offhand");
        //static public readonly BoneDef Hauled = new BoneDef("Hauled");
        //static public readonly BoneDef Helmet = new BoneDef("Helmet");
        //static public readonly BoneDef EquipmentHead = new BoneDef("EquipmentHead", "Head");
        //static public readonly BoneDef EquipmentHandle = new BoneDef("EquipmentHandle", "Handle");
        //static public readonly BoneDef Item = new BoneDef("Item");
    }
}
