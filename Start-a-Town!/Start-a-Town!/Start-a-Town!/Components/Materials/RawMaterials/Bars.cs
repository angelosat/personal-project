using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Materials.RawMaterials
{
    class Bars : MaterialType.RawMaterial
    {
        protected override GameObject Template()
        {
            GameObject obj = new GameObject();
            obj["Info"] = new GeneralComponent(this.ID, ObjectType.Bars, "Bars", "Used for crafting of weapons, armor, and tools.") { ItemSubType = ItemSubType.Bars };
            obj.AddComponent<GuiComponent>().Initialize(17, 1);
            obj["Sprite"] = new SpriteComponent(new Sprite("metalbars", Map.BlockDepthMap) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) });
            obj["Physics"] = new PhysicsComponent(size: 1);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
            return obj;
        }
        ////public override GameObject Create(int id)
        //public override GameObject Initialize()
        //{
        //    this.ID = IDSequence;
        //    return this.Template();
        //    //GameObject obj = new GameObject();
        //    //obj["Info"] = new GeneralComponent(this.ID, ObjectType.Bars, "Bars", "Used for crafting of weapons, armor, and tools.") { ItemSubType = ItemSubType.Bars };
        //    //obj.AddComponent<GuiComponent>().Initialize(17, 1);
        //    //obj["Sprite"] = new SpriteComponent(new Sprite("metalbars", Map.BlockDepthMap) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) });
        //    //obj["Physics"] = new PhysicsComponent(size: 1);
        //    //obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
        //    //return obj;
        //}

        public override GameObject Create(List<GameObjectSlot> materials)
        {
            //GameObject obj = GameObject.Create(this.ID);
            GameObject obj = this.Template();
            Material mat = materials.First(m => m.Name == "Body").Object.GetComponent<MaterialsComponent>().Parts["Body"].Material;
            obj.Name = mat.Name + " " + obj.Name;
            Sprite sprite = new Sprite(obj.GetSprite()) { Tint = mat.Color, Shininess = mat.Type.Shininess };
            obj.GetComponent<SpriteComponent>().Sprite = sprite;
            obj.Body.Sprite = sprite;
            obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
            return obj;
        }
    }
}
