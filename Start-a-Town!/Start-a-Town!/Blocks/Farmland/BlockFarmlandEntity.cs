using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Blocks
{
    partial class BlockFarmland : Block
    {
        public class BlockFarmlandEntity : BlockEntity
        {
            static public float TurnToSoilProbability = .5f;

            public override object Clone()
            {
                return new BlockFarmlandEntity();
            }

            public Progress Sprout = new Progress();
            //public int PlantID { get; set; }
            public ItemDef PlantDef;
            public BlockFarmlandEntity()
            {
                this.Sprout = new Progress();
                //this.PlantID = -1;
            }
           
            //public BlockFarmlandEntity(int plantID, int seedLevel)
            //{
            //    var sproutMax = seedLevel * Engine.TicksPerSecond * 10;// *200; //2;
            //    this.PlantID = plantID;
            //    this.Sprout.Value = this.Sprout.Max = sproutMax; // * 10
            //}
            public BlockFarmlandEntity(ItemDef plantDef, int seedLevel)
            {
                var sproutMax = seedLevel * Engine.TicksPerSecond * 10;// *200; //2;
                this.PlantDef = plantDef;
                this.Sprout.Value = this.Sprout.Max = sproutMax; // * 10
            }
            public override void GetTooltip(UI.Control tooltip)
            {
                tooltip.Controls.Add(new Bar()
                {
                    Width = 200,
                    Name = "Next Growth Stage: ",//Grows in: ",
                    Location = tooltip.Controls.BottomLeft,
                    Object = this.Sprout,
                    TextFunc = this.Sprout.ToStringPercentage,// this.Sprout.ToStringAsSeconds,
                    Invert = true
                });
            }
            public override void Tick(IObjectProvider net, Vector3 global)
            {
                if (this.Sprout.Value <= 0)
                {
                    var plant = ItemFactory.CreateItem(this.PlantDef);// GameObject.Create(this.PlantID);
                    //plant.GetComponent<PlantComponent>().ResetGrowth(plant);
                    net.Spawn(plant, global + Vector3.UnitZ);
                    net.Map.EventOccured(Message.Types.PlantGrown, global, plant);

                    //net.Map.GetCell(global).BlockData = 0;
                    net.Map.SetCellData(global, 0);
                    net.Map.RemoveBlockEntity(global);

                    // turn farmland back to soil
                    TurnToSoil(net, global, net as Server);
                }
                this.Sprout.Value--;
            }
            //public override void Tick(IObjectProvider net, Vector3 global)
            //{
            //    if (this.Sprout.Value <= 0)
            //    {
            //        if (PlantID > -1)
            //        { //sprout
            //            var plant = GameObject.Create(this.PlantID);
            //            plant.GetComponent<PlantComponent>().ResetGrowth(plant);
            //            net.Spawn(plant, global + Vector3.UnitZ);
            //            net.Map.EventOccured(Message.Types.PlantGrown, global, plant);
            //            this.PlantID = -1;

            //            //net.Map.GetCell(global).BlockData = 0;
            //            net.Map.SetCellData(global, 0);
            //            net.Map.RemoveBlockEntity(global);

            //            // turn farmland back to soil
            //            TurnToSoil(net, global, net as Server);
            //        }
            //    }
            //    this.Sprout.Value--;
            //}

            private static void TurnToSoil(IObjectProvider net, Vector3 global, Server server)
            {
                if (server != null)
                {
                    var probability = TurnToSoilProbability;// .5f;
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
                //io.Write(this.PlantID);
                io.Write(this.PlantDef.Name);
            }
            protected override void ReadExtra(BinaryReader io)
            {
                this.Sprout.Read(io);
                //this.PlantID = io.ReadInt32();
                this.PlantDef = Def.GetDef<ItemDef>(io.ReadString());
            }
            //public override SaveTag Save(string name)
            //{
            //    SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
            //    tag.Add(this.Sprout.Save("Growth"));
            //    tag.Add(new SaveTag(SaveTag.Types.Int, "Product", this.PlantID));
            //    return tag;
            //}
            protected override void AddSaveData(SaveTag tag)
            {
                tag.Add(this.Sprout.Save("Growth"));
                //tag.Add(new SaveTag(SaveTag.Types.Int, "Product", this.PlantID));
                tag.Add(this.PlantDef.Name.Save("PlantDef"));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Growth", this.Sprout.Load);
                //tag.TryGetTagValue<int>("Product", v => this.PlantID = v);
                this.PlantDef = tag.LoadDef<ItemDef>("PlantDef");
            }
        }
    }
}
