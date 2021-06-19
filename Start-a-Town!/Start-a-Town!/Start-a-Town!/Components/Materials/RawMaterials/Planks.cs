using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Materials.RawMaterials
{
    class Planks : MaterialType.RawMaterial
    {
        protected override GameObject Template()//GameObject Template()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Material, "Planks", "Processed logs", Quality.Common).Initialize(ItemSubType.Planks);
            obj.GetInfo().StackMax = 8;

            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24)));
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 1);
            obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Framing);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
            return obj;
        }

        ////static public readonly int ID_ = IDSequence;
        ////public override GameObject Create(int id)
        //public override GameObject Initialize()
        //{
            
        //    //Factory.Initialize();
        //    //return Factory.Default;
        //    this.ID = IDSequence;
        //    return Template();


        //    //GameObject obj = new GameObject();
        //    //obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Material, "Planks", "Processed logs", Quality.Common).Initialize(ItemSubType.Planks);
        //    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24)));
        //    //obj.AddComponent<GuiComponent>().Initialize(10, 8);
        //    //obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //    //obj.AddComponent<UseComponent>().Initialize(Script.Types.Framing);
        //    //obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
        //    //return obj;

            
        //}
        
        //public override GameObject CreateFrom(Material mat)
        //{
        //    GameObject obj = new GameObject();
        //    obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, mat.Name + " " + "Planks", "Processed logs", Quality.Common).Initialize(ItemSubType.Planks);
        //    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24)));
        //    var sprite = new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24));
        //    obj.AddComponent(new SpriteComponent(new Bone(Bone.Types.Torso, sprite) { Material = mat }));
        //    //obj.AddComponent<GuiComponent>().Initialize(10, 8);
        //    obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 1);
        //    obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
        //    return obj;
        //}

        //static readonly Reaction PlanksReaction = new Reaction(
        //    "Planks",
        //    Reaction.CanBeMadeAt(
        //        Reaction.Site.Person,
        //        ItemTemplate.Workbench.ID),
        //    Reaction.Reagent.Create(
        //        new Reaction.Reagent("Base", Reaction.Reagent.IsOfSubType(ItemSubType.Logs))),
        //    Reaction.Product.Create(new Reaction.Product(new MaterialType.RawMaterial.Factory("Base").Create))
        //    ) { Skill = new Reaction.ToolRequirement(Skills.Skill.Carpentry, true) };//{ Skill = Skills.Skill.Carpentry };

        //static public Reaction Reaction { get { return PlanksReaction; } }

        //public override GameObject Create(List<GameObjectSlot> materials)
        //{
        //    //GameObject obj = GameObject.Create(this.ID);
        //    GameObject obj = Template();
        //    Material mat = materials.First(m => m.Name == "Body").Object.GetComponent<MaterialsComponent>().Parts["Body"].Material;
        //    obj.Name = mat.Name + " " + obj.Name;
        //    Sprite sprite = new Sprite(obj.GetSprite()) { Tint = mat.Color };
        //    obj.GetComponent<SpriteComponent>().Sprite = sprite;
        //    obj.Body.Sprite = sprite;
        //    obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
        //    return obj;
        //}

        //public class Factory
        //{
        //    string ReagentName;
        //    public Factory(string reagentName)
        //    {
        //        this.ReagentName = reagentName;
        //    }
        //    public GameObject Create(List<GameObjectSlot> materials)
        //    {
        //        //var mat = reagents.First(n => n.Name == this.ReagentName);

        //        GameObject obj = Template();
        //        Material mat = materials.First(m => m.Name == this.ReagentName).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material;
        //        obj.Name = mat.Name + " " + obj.Name;
        //        Sprite sprite = new Sprite(obj.GetSprite()) { Tint = mat.Color };
        //        obj.GetComponent<SpriteComponent>().Sprite = sprite;
        //        obj.Body.Sprite = sprite;
        //        obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
        //        return obj;
        //    }

        //    GameObject Template()
        //    {
        //        GameObject obj = new GameObject();
        //        obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Material, "Planks", "Processed logs", Quality.Common).Initialize(ItemSubType.Planks);
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24)));
        //        obj.AddComponent<GuiComponent>().Initialize(10, 8);
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Framing);
        //        obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
        //        return obj;
        //    }
        //}
    }
}
