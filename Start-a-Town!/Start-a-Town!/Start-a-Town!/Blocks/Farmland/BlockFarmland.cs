using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components;
using Start_a_Town_.Towns.Farming;

namespace Start_a_Town_.Blocks
{
    //public partial class Block
    //{
        partial class BlockFarmland : Block
        {
            public override Material GetMaterial(byte blockdata)
            {
                return Material.Soil;
            }
            public BlockFarmland()
                : base(Types.Farmland, GameObject.Types.Farmland)
            {
                //this.Material = Material.Soil;
                AssetNames = "farmland1, farmland2";
            }

            public override BlockEntity GetBlockEntity()
            {
                return new Entity();
            }
            //public override ContextAction GetRightClickAction(Vector3 global)
            //{
            //    return new ContextAction(() => "Plant", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
            //}

            //public override void Remove(IMap map, Vector3 global)
            //{
            //    base.Remove(map, global);
            //    map.RemoveBlockEntity(global);
            //}
            public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
            {
                var list = new List<Interaction>();
                list.Add(new InteractionPlantSeed());
                return list;
            }

            static public void Plant(IMap map, Vector3 global, GameObject obj)
            {
                if (obj.IsNull())
                    throw new ArgumentNullException();
                SeedComponent seed;
                if (!obj.TryGetComponent<SeedComponent>(out seed))
                    throw new ArgumentException();
                var plantID = (int)seed.Product; //(int)obj.ID;
                var growthMax = seed.Level * Engine.TargetFps * 2;// *200;
                var entity = new Entity(plantID, growthMax);
                map.AddBlockEntity(global, entity);
                // change block texure
                map.GetCell(global).Variation = 1;
                map.GetChunk(global).Valid = false;
            }
            static public void Fertilize(IMap map, Vector3 global, float potency)
            {
                var entity = map.GetBlockEntity(global) as BlockFarmland.Entity;
                if (entity == null)
                    throw new Exception();
                entity.Growth.Value -= entity.Growth.Max * potency;
            }
            //public override void GetPlayerActionsWorld(IMap map, Vector3 global, Dictionary<PlayerInput, Components.Interactions.Interaction> list)
            //{
            //    var hauling = PersonalInventoryComponent.GetHauling(Player.Actor);
            //}
            //public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
            //{
            //    base.Draw(sb, screenBounds, sunlight, blocklight, fog, Components.Materials.Material.Templates[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
            //}
        }
    //}
}
