using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Crafting;
using Start_a_Town_.UI;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    class BuildProgress : ProgressOld
    {
        public BuildProgress(float min, float max, float value) : base(min, max, value) { }
    }

    class CraftableComponent : Component
    {
        public override string ComponentName
        {
            get { return "Craftable"; }
        }
        //public Dictionary<GameObject.Types, int> CurrentMaterials; //Materials, 
        //List<InventorySlot> CurrentMaterials;
        //public GameObject.Types ProductID;
        public int Variation, Orientation;
        BitVector32 _Data;
        public BuildProgress Progress;
        static BitVector32.Section _KeepPlacing;

        public bool KeepPlacing
        {
            get { return _Data[_KeepPlacing] == 1 ? true : false; }
            set { _Data[_KeepPlacing] = (value == true ? 1 : 0); }
        }

        //public static Dictionary<
        public CraftableComponent()
        {
            //Materials = new Dictionary<GameObject.Types, int>();
            this["CurrentMaterials"] = new Dictionary<GameObject.Types, int>();
            Progress = new BuildProgress(0, 10, 0);

            _KeepPlacing = BitVector32.CreateSection(1);
        }
        public CraftableComponent(Blueprint bp, bool keepPlacing = false)
        {
            //ProductID = bp.ProductID;
            //Materials = bp.Materials;
            Dictionary<GameObject.Types, int> currentMats = new Dictionary<GameObject.Types, int>();

            this["Blueprint"] = bp;
            this["CurrentMaterials"] = currentMats;
            this[Stat.Stage.Name] = 0;

            foreach (BlueprintStage stage in bp.Stages)
                foreach (KeyValuePair<GameObject.Types, int> mat in stage)
                    currentMats.Add(mat.Key, 0);

            _KeepPlacing = BitVector32.CreateSection(1);
            Progress = new BuildProgress(0, 10, 0);
            //CraftingManager.Instance.AddBlueprint(type, materials);
            KeepPlacing = keepPlacing;
        }

        public override string ToString()
        {
            //return GlobalVars.KeyBindings.Use + ": Activate";
            return "Right click: Use";
            //string text = "Materials:\n";
            //foreach (KeyValuePair<GameObject.Types, int> pair in Materials)
            //{
            //    //text += " {" + StaticObject.Objects[pair.Key].Name + " x" + pair.Value + "}";
            //    int count = pair.Value, currentcount = CurrentMaterials[pair.Key];
            //    text += "{" + StaticObject.Objects[pair.Key].Name + " " + currentcount + "/" + count + "}";
            //}
            //text += "\nProgress: " + Progress.Percentage * 100 + "%";
            //text += "\nKeep placing: " + KeepPlacing.ToString()
            //    + "\nVariation: " + Variation;
            //return text;
        }

        public override object Clone()
        {
            CraftableComponent craft = new CraftableComponent();
            craft._Data = new BitVector32(_Data);
            //craft.ProductID = ProductID;
            craft["Blueprint"] = this["Blueprint"];
            craft[Stat.Stage.Name] = this[Stat.Stage.Name];
            craft.Variation = Variation;
            craft.Orientation = Orientation;
            craft.KeepPlacing = KeepPlacing;
            foreach (BlueprintStage stage in this.GetProperty<Blueprint>("Blueprint").Stages)
                foreach (KeyValuePair<GameObject.Types, int> mat in stage)
                    craft.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials").Add(mat.Key, 0);
            return craft;
        }

        public override bool Drop(GameObject self, GameObject actor, GameObject obj)
        {
            int count, currentcount;
            Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            Dictionary<GameObject.Types, int> reqMats = this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];
            if (reqMats.TryGetValue(obj.ID, out count))
            {
                if (currentMats.TryGetValue(obj.ID, out currentcount))
                    if (currentcount >= count)
                        return false;

                //Task task = new Task(actor, 
                //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
                //CurrentMaterials[obj.ID] += 1;
                return true;
            }

            return false;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                //case Message.Types.Attack:
                //    return Activate(sender, parent);
                case Message.Types.Death:
                    Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
                    foreach (KeyValuePair<GameObject.Types, int> mat in currentMats)
                    {
                        if (mat.Value > 0)
                        {
                            
                            //currentMaterial.GetGui().StackSize = mat.Value;
                            for (int i = 0; i < mat.Value; i++)
                            {
                                GameObject currentMaterial = GameObject.Create(mat.Key);
                                Chunk.AddObject(currentMaterial, parent.Map, parent.Transform.Position.Rounded + new Vector3(0, 0, parent.GetPhysics().Height));
                                double angle = parent.Map.GetWorld().GetRandom().NextDouble() * (Math.PI + Math.PI);
                                throw new NotImplementedException();

                            }
                        }
                    }
                    Chunk.RemoveObject(parent);
                    return false;
                case Message.Types.Activate:
                    return Activate(sender, parent);
                    //return false;
                case Message.Types.Give:
                    InventoryComponent inv;
                    if (!sender.TryGetComponent<InventoryComponent>("Inventory", out inv))
                        return false;
                    if (inv.GetProperty<GameObjectSlot>("Holding").Object == null)
                        return false;
                    if (Give(parent, sender, inv.GetProperty<GameObjectSlot>("Holding")))
                    {
                   
                        //inv.Holding.Object.Remove();
                        e.Network.Despawn(inv.Holding.Object);
                        inv.GetProperty<GameObjectSlot>("Holding").Object = null;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public bool Add(GameObject material)
        {
            GetCurrentMaterials().Add(material.ID, 1);
            return true;
        }

        Dictionary<GameObject.Types, int> GetCurrentMaterials()
        {
            return this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }

        public override bool Give(GameObject self, GameObject actor, GameObjectSlot objSlot)//GameObject obj)
        {
            GameObject obj = objSlot.Object;
            if (Drop(self, actor, obj))
            {
                //obj.GetComponent<GuiComponent>("Gui").StackSize -= 1;
                Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
                currentMats[obj.ID] += 1;
                return true;
            }
            return false;
        }

        public bool Activate(GameObject sender, GameObject parent)
        {
            

            //Task task = actor.GetComponent<TasksComponent>("Tasks").CurrentTask;
            //if (task.Progress == null)
            //    task.Progress = Progress;
            Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
            Dictionary<GameObject.Types, int> reqMats = this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];
            foreach (KeyValuePair<GameObject.Types, int> mats in reqMats)
            {
                int currentcount;
                if (!currentMats.TryGetValue(mats.Key, out currentcount))
                    return false;
                if (currentcount != mats.Value)
                    return false;
            }
            
            //sender.GetComponent<CooldownComponent>("Cooldown").Properties["Cooldown"] = 5f;
            float skill = sender["Stats"].GetProperty<float>(Stat.WorkSpeed.Name);
            float skillMod = 0.8f * skill / 100;
            sender["Cooldown"]["Cooldown"] = (1 - skillMod) * 5;
            Progress.Value += 0.5f + skill / 100f;
            if (Progress.Value >= Progress.Max)
            {
                throw new NotImplementedException();
                //Complete(parent);

                return true;
            }
            return false;
        }

        private void Complete(Net.IObjectProvider net, GameObject parent)
        {
            GameObject product = GameObject.Create(this.GetProperty<Blueprint>("Blueprint").ProductID);

            Vector3 global = parent.Global;
            SpriteComponent sprComp = product.GetComponent<SpriteComponent>("Sprite");
            sprComp.Variation = Variation;
            sprComp.Orientation = Orientation;
            throw new NotImplementedException();
        }

        public override string GetTooltipText()
        {
            return ToString();
        }


        public override void Focus(GameObject parent)
        {
            //parent.Components.Remove("Sprite");
            //parent.AddComponent("Sprite", StaticObject.Objects[ProductID].GetComponent<SpriteComponent>("Sprite"));
            //parent.GetComponent<SpriteComponent>("Sprite").Sprite = StaticObject.Objects[ProductID].GetComponent<SpriteComponent>("Sprite").Sprite;
            //if (Bar != null)
            //    Bar.Show();
        }
        public override void FocusLost(GameObject parent)
        {
            //parent.Components.Remove("Sprite");
            //parent.AddComponent("Sprite", StaticObject.Objects[7].GetComponent<SpriteComponent>("Sprite"));
            //parent.GetComponent<SpriteComponent>("Sprite").Sprite = StaticObject.Objects[7].GetComponent<SpriteComponent>("Sprite").Sprite;
            //if (Bar != null)
            //    Bar.Hide();
        }

        //public override void GetTooltip(GameObject parent, Tooltip tooltip)
        //{
        //   // tooltip.Controls.Clear();
        //    int left = 0, bottom = 0;
        //    foreach (UI.Control b in tooltip.Controls)
        //    {
        //        left = Math.Min(left, b.Left);
        //        bottom = Math.Max(left, b.Bottom);
        //    }


        //    GroupBox box = new GroupBox(new Vector2(left, bottom));
        //    GameObject obj = GameObject.Objects[this.GetProperty<Blueprint>("Blueprint").ProductID];
        //    SpriteComponent sprComp = obj.GetComponent<SpriteComponent>("Sprite");
        //    PictureBox pic = new PictureBox(new Vector2(0), sprComp.Sprite.Texture, sprComp.Sprite.SourceRect[Variation][Orientation], TextAlignment.Left);
        //    Panel panel = new Panel();
        //    panel.AutoSize = true;
        //    panel.Controls.Add(pic);

        //    //Label labelName = new Label(new Vector2(panel.Right, 0), obj.GetInfo().Name + "\nProgress: " + (int)(Progress.Percentage * 100) + "%");
        //    Label labelName = new Label(new Vector2(panel.Right, 0), obj.Name + "\n" + (int)(Progress.Percentage * 100) + "% Complete");

        //    Label label = new Label("Materials:");

        //    GroupBox panelMaterials = new GroupBox(new Vector2(panel.Right, labelName.Bottom)); //new GroupBox(new Vector2(0, panel.Bottom)); //
        //    panelMaterials.AutoSize = true;
        //    panelMaterials.Controls.Add(label);
            
        //    int i = 0;
        //    Dictionary<GameObject.Types, int> currentMats = this.GetProperty<Dictionary<GameObject.Types, int>>("CurrentMaterials");
        //    Dictionary<GameObject.Types, int> reqMats = this.GetProperty<Blueprint>("Blueprint").Stages[(int)this[Stat.Stage.Name]];
        //    foreach (KeyValuePair<GameObject.Types, int> mat in currentMats)
        //    {
        //        Slot slot = new Slot(new Vector2(i, label.Bottom));
        //        slot.AutoText = false;
        //        GameObjectSlot invSlot = new GameObjectSlot();
        //        invSlot.Object = GameObject.Create(mat.Key);
        //        //invSlot.Object.GetGui().StackSize = mat.Value;
        //        //slot.SetBottomRightText(mat.Value + "/" + Materials[mat.Key]);
        //        //invSlot.Object.GetGui().StackSize = Materials[mat.Key];
        //        slot.SetBottomRightText(mat.Value + "/" + reqMats[mat.Key]);
        //        slot.Tag = invSlot;
        //        i += slot.Width;
        //        panelMaterials.Controls.Add(slot);
        //    }

        //    //foreach (UI.Control control in tooltip.Controls)
        //    //{
        //    //    control.Location.X += panel.Width;
        //    //}
        //    box.Controls.Add(panel, panelMaterials, labelName);
        //    tooltip.Controls.Add(box);
        //}

        
    }
}
