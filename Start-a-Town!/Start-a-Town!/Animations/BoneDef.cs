namespace Start_a_Town_
{
    public class BoneDef : Def
    {
        public string Label;
        
        BoneDef(string name, string label) : base(name)
        {
          
            this.Label = label;
        }
        static BoneDef Create(string name, string label)
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
    }
}
