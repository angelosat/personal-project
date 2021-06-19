using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    /// <summary>
    /// Enables a GameObject to have a Gui representation.
    /// </summary>
    public class GuiComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Gui";
            }
        }

        public Icon Icon { get { return (Icon)this["Icon"]; } set { this["Icon"] = value; } }
        public int StackMax { get { return (int)this["StackMax"]; } set { this["StackMax"] = value; } }
        

        public GuiComponent() : base()
        {
            Properties.Add("Icon", new Icon(Map.ItemSheet, 0, 32));
            Properties.Add("StackMax", 1);
            //this.StackSize = 1;
            this.Icon = new Icon(Map.ItemSheet, 0, 32);
            this.StackMax = 1;
           // Properties.Add("StackSize", 1);
        }
        public GuiComponent Initialize(string assetName, int stackMax = 1)
        {
            this.Icon = new Icon(assetName);
            this.StackMax = stackMax;
            return this;
        }
        public GuiComponent Initialize(int iconID = 0, int stackMax = 1)
        {
            this.Icon = new Icon(Map.ItemSheet, (uint)iconID, 32);
            this.StackMax = stackMax;
            return this;
        }
        public GuiComponent Initialize(Icon icon, int stackMax = 1)
        {
            this.Icon = icon;
            this.StackMax = stackMax;
            return this;
        }
        GuiComponent(int iconID = 0, int stackMax = 1) : this()
        {
     //       GetProperty<Icon>("Icon").Index = (uint)iconID;
            this.Icon = new Icon(Map.ItemSheet, (uint)iconID, 32);
            Properties["StackMax"] = stackMax;
        }
        GuiComponent(Icon icon, int stackMax = 1)
            : this()
        {
            this["Icon"] = icon;
            Properties["StackMax"] = stackMax;
        }

        public override object Clone()
        {
            GuiComponent phys = new GuiComponent();
            foreach (KeyValuePair<string, object> property in Properties)
            {
                phys.Properties[property.Key] = property.Value;
            }
            return phys;
        }


        //public override bool HandleMessage(GameObject parent, GameObject sender, Message.Types msg)
        //{
        //    switch (msg)
        //    {
        //        default:
        //            return false;
        //    }
        //}

        //public override bool Drop(GameObject self, GameObject actor, GameObject obj)
        //{
        //    if (obj.ID == self.ID)
        //        if (GetProperty<int>("StackSize") < GetProperty<int>("StackMax"))
        //            return true;
        //    return false;
        //}

        //public override bool Give(GameObject self, GameObject actor, GameObject obj)
        //{
        //    if (!Drop(self, actor, obj))
        //        return false;

        //    self.GetComponent<GuiComponent>("Gui").Properties["StackSize"] = self.GetComponent<GuiComponent>("Gui").GetProperty<int>("StackSize") + 1;

        //    return true;
        //}

        
    }
}
