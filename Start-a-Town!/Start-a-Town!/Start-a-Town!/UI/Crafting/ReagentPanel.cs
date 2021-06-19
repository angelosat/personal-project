using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class ReagentPanel : Panel
    {
        public Action<Reaction.Product.ProductMaterialPair> Callback = a => { };

        //public ReagentPanel(Rectangle clientSize)
        //{
        //    this.ClientSize = clientSize;
        //}
        public ReagentPanel()
        {

        }
        public void Refresh(Reaction reaction, List<GameObjectSlot> reagentSlots)
        {
            this.Controls.Clear();
            this.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            this.Controls.Add(new Label(this.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            List<GameObjectSlot> mats = new List<GameObjectSlot>();
            GameObjectSlot tool = GameObjectSlot.Empty;
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                matSlot.Name = reagent.Name;
                mats.Add(matSlot);
                Slot slot = new Slot(this.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString() + "\n"; } return t.TrimEnd('\n') + "\n\nLeft click to choose material\nRight click to clear"; };//
                Label matName = new Label(slot.TopRight, reagent.Name);
                slot.LeftClickAction = () =>
                {
                    ItemPicker.Instance.Label.Text = "Material for: " + reagent.Name;
                    //ItemPicker.Instance.Show(UIManager.Mouse, reagent.Filter, o =>
                    ItemPicker.Instance.Show(UIManager.Mouse, reagent.Filter, reagentSlots, o =>
                    {
                        slot.Tag.Link = o;
                        //slot.Tag.Object = o;
                    });
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };

                matSlot.Filter = reagent.Filter;
                matSlot.ObjectChanged = o =>
                {
                    CheckIfProduct(reaction, mats, tool);
                };
                this.Controls.Add(slot, matName);
            }
            //return; // make the reaction use the equipped tool instead of selecting it via ui
            if (reaction.Skill != null)
            {
                this.Controls.Add(new Label(this.Controls.BottomLeft, "Tool") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });

                tool.Name = "Tool";
                Slot toolSlot = new Slot(this.Controls.BottomLeft) { Tag = tool, CustomTooltip = true };
                toolSlot.HoverFunc = () => "Has ability: " + reaction.Skill.Skill.Name;
                this.Controls.Add(new Label(toolSlot.TopRight, reaction.Skill.ToolRequired ? "Required" : "Optional"));
                toolSlot.LeftClickAction = () =>
                {
                    ItemPicker.Instance.Label.Text = "Select tool";
                    ItemPicker.Instance.Show(UIManager.Mouse, o => SkillComponent.HasSkill(o, reaction.Skill.Skill), Player.Actor.GetChildren(), o =>
                    {
                        //toolSlot.Tag.Object = o;
                        toolSlot.Tag.Link = o;
                    });
                };
                toolSlot.RightClickAction = () =>
                {
                    toolSlot.Tag.Clear();
                };
                tool.ObjectChanged = o =>
                {
                    CheckIfProduct(reaction, mats, tool);
                };
                this.Controls.Add(toolSlot);
            }
        }

        [Obsolete]
        public void Refresh(Reaction reaction)
        {
            this.Controls.Clear();
            this.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            this.Controls.Add(new Label(this.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            List<GameObjectSlot> mats = new List<GameObjectSlot>();
            GameObjectSlot tool = GameObjectSlot.Empty;
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                matSlot.Name = reagent.Name;
                mats.Add(matSlot);
                Slot slot = new Slot(this.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString() + "\n"; } return t.TrimEnd('\n') + "\n\nLeft click to choose material\nRight click to clear"; };//
                Label matName = new Label(slot.TopRight, reagent.Name);
                slot.LeftClickAction = () =>
                {
                    ItemPicker.Instance.Label.Text = "Material for: " + reagent.Name;
                    //ItemPicker.Instance.Show(UIManager.Mouse, reagent.Filter, o =>
                    ItemPicker.Instance.Show(UIManager.Mouse, reagent.Filter, Player.Actor.GetComponent<PersonalInventoryComponent>().Slots, o =>
                    {
                        slot.Tag.Link = o;
                        //slot.Tag.Object = o;
                    });
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };
                
                matSlot.Filter = reagent.Filter;
                matSlot.ObjectChanged = o =>
                {
                    CheckIfProduct(reaction, mats, tool);
                };
                this.Controls.Add(slot, matName);
            }
            //return; // make the reaction use the equipped tool instead of selecting it via ui
            if (reaction.Skill != null)
            {
                this.Controls.Add(new Label(this.Controls.BottomLeft, "Tool") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });

                tool.Name = "Tool";
                Slot toolSlot = new Slot(this.Controls.BottomLeft) { Tag = tool, CustomTooltip = true };
                toolSlot.HoverFunc = () => "Has ability: " + reaction.Skill.Skill.Name;
                this.Controls.Add(new Label(toolSlot.TopRight, reaction.Skill.ToolRequired ? "Required" : "Optional"));
                toolSlot.LeftClickAction = () =>
                {
                    ItemPicker.Instance.Label.Text = "Select tool";
                    ItemPicker.Instance.Show(UIManager.Mouse, o => SkillComponent.HasSkill(o, reaction.Skill.Skill), Player.Actor.GetChildren(), o => 
                    { 
                        //toolSlot.Tag.Object = o;
                        toolSlot.Tag.Link = o; 
                    });
                };
                toolSlot.RightClickAction = () =>
                {
                    toolSlot.Tag.Clear();
                };
                tool.ObjectChanged = o =>
                {
                    CheckIfProduct(reaction, mats, tool);
                };
                this.Controls.Add(toolSlot);
            }
        }
        private void CheckIfProduct(Reaction reaction, List<GameObjectSlot> mats, GameObjectSlot tool)
        {
            if ((from sl in mats where !sl.HasValue select sl).FirstOrDefault() != null)
            {
                this.Callback(null);
                return;
            }
            if (reaction.Skill != null)
                if (reaction.Skill.ToolRequired)
                    if (tool.Object == null)
                    {
                        this.Callback(null);
                        return;
                    }
            var product = reaction.Products.First().GetProduct(reaction, Player.Actor, mats, tool.Object);
            this.Callback(product);

            //var equippedTool = GearComponent.GetHeldObject(Player.Actor).Object;
            //if (reaction.Tool != null)
            //{
            //    if (equippedTool == null)
            //    {
            //        this.Callback(null);
            //        return;
            //    }
            //    var skillComp = equippedTool.GetComponent<SkillComponent>();
            //    if(skillComp == null)
            //    {
            //        this.Callback(null);
            //        return;
            //    }
            //    if(skillComp.Skill != reaction.Tool.Skill)
            //    {
            //        this.Callback(null);
            //        return;
            //    }
            //}
            //var product = reaction.Products.First().GetProduct(reaction, Player.Actor, mats, equippedTool);
            //this.Callback(product);
        }
    }
}
