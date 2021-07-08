using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System;

namespace Start_a_Town_.Blocks
{
    partial class BlockFarmland : Block
    {
        [Obsolete]
        public class BlockFarmlandEntity : BlockEntity
        {
            static public float TurnToSoilChance = .5f;

            public override object Clone()
            {
                return new BlockFarmlandEntity();
            }

            public Progress Sprout = new();
            public ItemDef PlantDef;
            public BlockFarmlandEntity()
            {
                this.Sprout = new Progress();
            }
           
            public BlockFarmlandEntity(ItemDef plantDef, int seedLevel)
            {
                var sproutMax = seedLevel * Engine.TicksPerSecond * 10;
                this.PlantDef = plantDef;
                this.Sprout.Value = this.Sprout.Max = sproutMax;
            }
            public override void GetTooltip(Control tooltip)
            {
                tooltip.Controls.Add(new Bar()
                {
                    Width = 200,
                    Name = "Next Growth Stage: ",
                    Location = tooltip.Controls.BottomLeft,
                    Object = this.Sprout,
                    TextFunc = this.Sprout.ToStringPercentage,
                    Invert = true
                });
            }
            public override void Tick(IObjectProvider net, Vector3 global)
            {
                if (this.Sprout.Value <= 0)
                {
                    var plant = ItemFactory.CreateItem(this.PlantDef);
                    net.Spawn(plant, global + Vector3.UnitZ);
                    net.Map.EventOccured(Message.Types.PlantGrown, global, plant);
                    net.Map.SetCellData(global, 0);
                    net.Map.RemoveBlockEntity(global);
                    // turn farmland back to soil
                    TurnToSoil(net, global, net as Server);
                }
                this.Sprout.Value--;
            }

            private static void TurnToSoil(IObjectProvider net, Vector3 global, Server server)
            {
                if (server != null)
                {
                    var probability = TurnToSoilChance;
                    var turnToSoil = server.GetRandom().NextDouble();
                    if (turnToSoil <= probability)
                    {
                        var variation = server.GetRandom().Next(0, BlockDefOf.Soil.Variations.Count - 1);
                        server.SyncPlaceBlock(net.Map, global, BlockDefOf.Soil, 0, variation, 0);
                    }
                }
            }

            protected override void WriteExtra(BinaryWriter io)
            {
                this.Sprout.Write(io);
                io.Write(this.PlantDef.Name);
            }
            protected override void ReadExtra(BinaryReader io)
            {
                this.Sprout.Read(io);
                this.PlantDef = Def.GetDef<ItemDef>(io.ReadString());
            }
           
            protected override void AddSaveData(SaveTag tag)
            {
                tag.Add(this.Sprout.Save("Growth"));
                tag.Add(this.PlantDef.Name.Save("PlantDef"));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Growth", this.Sprout.Load);
                this.PlantDef = tag.LoadDef<ItemDef>("PlantDef");
            }
        }
    }
}
