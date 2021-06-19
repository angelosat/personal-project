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
        public class Entity : BlockEntity
        {
            static public float TurnToSoilProbability = .5f;

            public override object Clone()
            {
                return new Entity();
            }

            public Progress Growth = new Progress();
            public int PlantID { get; set; }

            public Entity()
            {
                this.Growth = new Progress();
                this.PlantID = -1;
            }

            public Entity(int plantID, int growthMax)
            {
                this.PlantID = plantID;
                this.Growth.Value = this.Growth.Max = growthMax; // * 10
            }

            public override void GetTooltip(UI.Control tooltip)
            {
                tooltip.Controls.Add(new Bar()
                {
                    Width = 200,
                    Name = "Grows in: ",
                    Location = tooltip.Controls.BottomLeft,
                    Object = this.Growth,
                    TextFunc = this.Growth.ToStringAsSeconds,
                    Invert = true
                });
            }

            public override void Update(Net.IObjectProvider net, Vector3 global)
            {
                if (this.Growth.Value <= 0)
                {
                    if (PlantID > -1)
                    { //sprout
                        var plant = GameObject.Create(this.PlantID);
                        //net.Instantiate(plant);
                        plant.GetComponent<PlantComponent>().ResetGrowth(plant);
                        net.Spawn(plant, global + Vector3.UnitZ);
                        net.Map.EventOccured(Message.Types.PlantGrown, plant);
                        this.PlantID = -1;

                        net.Map.GetCell(global).Variation = 0;
                        net.Map.RemoveBlockEntity(global);

                        // turn farmland back to soil
                        TurnToSoil(net, global, net as Server);
                    }
                }
                this.Growth.Value--;
            }

            private static void TurnToSoil(Net.IObjectProvider net, Vector3 global, Server server)
            {
                if (server != null)
                {
                    var probability = TurnToSoilProbability;// .5f;
                    var turnToSoil = server.GetRandom().NextDouble();
                    if (turnToSoil <= probability)
                    {
                        var variation = server.GetRandom().Next(0, Block.Soil.Variations.Count - 1);
                        server.SyncPlaceBlock(net.Map, global, Block.Soil, 0, variation, 0);
                    }
                }
            }

            public override void Write(BinaryWriter io)
            {
                this.Growth.Write(io);
                io.Write(this.PlantID);
            }
            public override void Read(BinaryReader io)
            {
                this.Growth.Read(io);
                this.PlantID = io.ReadInt32();
            }
            public override SaveTag Save(string name)
            {
                SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
                tag.Add(this.Growth.Save("Growth"));
                tag.Add(new SaveTag(SaveTag.Types.Int, "Product", this.PlantID));
                return tag;
            }
            public override void Load(SaveTag tag)
            {
                tag.TryGetTag("Growth", this.Growth.Load);
                tag.TryGetTagValue<int>("Product", v => this.PlantID = v);
            }
        }
    }
}
