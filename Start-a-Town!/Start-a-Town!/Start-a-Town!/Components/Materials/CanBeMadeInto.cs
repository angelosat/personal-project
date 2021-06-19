using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Components.Materials
{
    public class CanBeMadeInto
    {
        public List<GameObject> GetEntities(MaterialType type)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var subtype in type.SubTypes)
            {
                var templates = this.Initialize(subtype);
                subtype.ProcessingChain = new List<GameObject>(templates);
                list.AddRange(templates);
                //list.AddRange(this.Initialize(subtype));
            }
                //foreach (var template in this.Templates)
                //    list.Add(Initialize(template, subtype));
            return list;
        }

        public HashSet<MaterialType.RawMaterial> Templates;

        public CanBeMadeInto(params MaterialType.RawMaterial[] templates)
        {
            this.Templates = new HashSet<MaterialType.RawMaterial>(templates);
        }
        //static GameObject Initialize(MaterialType.RawMaterial template, Material mat)
        //{
        //    GameObject obj = template.Initialize();// template(IDSequence);
        //    obj.Name = mat.Name + " " + obj.Name;
        //    Sprite sprite = new Sprite(obj.GetSprite()) { Tint = mat.TextColor };
        //    obj.GetComponent<SpriteComponent>().Sprite = sprite;
        //    obj.Body.Sprite = sprite;
        //    obj.AddComponent<MaterialComponent>().Initialize(mat);
        //    return obj;
        //}
        List<GameObject> Initialize(Material mat)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var item in this.Templates)
            {
                //list.Add(this.Initialize(item, mat));
                list.Add(item.Initialize(mat));
                foreach (var child in item.Children)
                    list.AddRange(child.Initialize(mat));
            }
            return list;
        }
        //GameObject Initialize(MaterialType.RawMaterial template, Material mat)
        //{
        //    GameObject obj = template.Initialize();// template(IDSequence);
        //    obj.Name = mat.Name + " " + obj.Name;
        //    Sprite sprite = new Sprite(obj.GetSprite()) { Material = mat };// { Tint = mat.Color, Shininess = mat.Type.Shininess }; // this is the one 
        //    obj.GetComponent<SpriteComponent>().Sprite = sprite;
        //    obj.Body.Sprite = sprite;
        //    obj.Body.Material = mat;
        //    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
        //    return obj;
        //}
    }
}
