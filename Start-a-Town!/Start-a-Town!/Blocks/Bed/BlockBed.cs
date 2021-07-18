using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    class BlockBed : Block
    {
        public enum Part { Top = 0x0, Bottom = 0x1 }

        public override Material GetMaterial(byte blockdata)
        {
            // TODO: implement
            return MaterialDefOf.LightWood;
        }

        readonly AtlasDepthNormals.Node.Token[] TopParts, BottomParts;
        readonly AtlasDepthNormals.Node.Token[][] Parts;
        public BlockBed() : base(Block.Types.Bed, 0f, 1f, false, true)
        {
            this.Furniture = FurnitureDefOf.Bed;
            this.BuildProperties = new BuildProperties(new Ingredient(amount: 4).IsBuildingMaterial(), 1);
            this.ToggleConstructionCategory(ConstructionsManager.Furniture, true);
            this.TopParts = new AtlasDepthNormals.Node.Token[] {
                Block.Atlas.Load("blocks/bed/bedslimtop", "blocks/bed/bedslimtopdepth", "blocks/bed/bedslimtopnormal"),
                Block.Atlas.Load("blocks/bed/bedslimtop2", "blocks/bed/bedslimtop2depth", "blocks/bed/bedslimtop2normal"),
                Block.Atlas.Load("blocks/bed/bedslimbottom", "blocks/bed/bedslimbottomdepth", "blocks/bed/bedslimbottomnormal"),
                Block.Atlas.Load("blocks/bed/bedslimbottom2", "blocks/bed/bedslimbottom2depth", "blocks/bed/bedslimbottom2normal")
            };
            this.BottomParts = new AtlasDepthNormals.Node.Token[] {
                Block.Atlas.Load("blocks/bed/bedslimbottom", "blocks/bed/bedslimbottomdepth", "blocks/bed/bedslimbottomnormal"),
                Block.Atlas.Load("blocks/bed/bedslimbottom2", "blocks/bed/bedslimbottom2depth", "blocks/bed/bedslimbottom2normal"),
                Block.Atlas.Load("blocks/bed/bedslimtop", "blocks/bed/bedslimtopdepth", "blocks/bed/bedslimtopnormal"),
                Block.Atlas.Load("blocks/bed/bedslimtop2", "blocks/bed/bedslimtop2depth", "blocks/bed/bedslimtop2normal")
            };

            this.Variations.Add(this.BottomParts.First());

            this.Parts = new AtlasDepthNormals.Node.Token[2][];
            this.Parts[0] = this.TopParts;
            this.Parts[1] = this.BottomParts;
            this.UtilitiesProvided.Add(Utility.Types.Sleeping);
        }
        public override bool IsRoomBorder => false;
        public override bool IsStandableOn => false;
        public override float GetHeight(byte data, float x, float y)
        {
            return 0.5f;
        }

        public override bool Multi
        {
            get
            {
                return true;
            }
        }
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Planks, this.GetMaterial(data)))
                    );
            return table;
        }
        public override Dictionary<IntVec3, byte> GetParts(IntVec3 global, int orientation) // TODO: depend on orientation
        {
            var dic = new Dictionary<IntVec3, byte>();

            var top = global;
            var bottom = orientation switch
            {
                0 => global + IntVec3.UnitX,
                1 => global + IntVec3.UnitY,
                2 => global - IntVec3.UnitX,
                3 => global - IntVec3.UnitY,
                _ => throw new Exception(),
            };
            var bottomdata = GetData(Part.Bottom, orientation);
            var topdata = GetData(Part.Top, orientation);
            dic[bottom] = bottomdata;
            dic[top] = topdata;
            return dic;
        }
        public override IEnumerable<IntVec3> GetParts(byte data)
        {
            GetState(data, out var part, out var ori);
            IntVec3 top, bottom;
            IntVec3 global = IntVec3.Zero;
            switch (ori)
            {
                case 1:
                    bottom = part == Part.Bottom ? global : global + IntVec3.UnitY;
                    top = bottom - IntVec3.UnitY;
                    break;

                case 2:
                    bottom = part == Part.Bottom ? global : global - IntVec3.UnitX;
                    top = bottom + IntVec3.UnitX;
                    break;

                case 3:
                    bottom = part == Part.Bottom ? global : global - IntVec3.UnitY;
                    top = bottom + IntVec3.UnitY;
                    break;

                default:
                    bottom = part == Part.Bottom ? global : global + IntVec3.UnitX;
                    top = bottom - IntVec3.UnitX;
                    break;
            }
            yield return top;
            yield return bottom;
        }
       
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            GetState(data, out var part, out var ori);
            var token = this.Parts[(int)part][(ori + cameraRotation) % 4];
            return token;
        }
        public static void GetState(byte data, out Part part, out int orientation)
        {
            part = (Part)(data & 0x1);
            orientation = (data & 0x6) >> 1;
        }
        public static int GetOrientation(byte data)
        {
            return (data & 0x6) >> 1;
        }
        public static void GetState(Cell cell, out Part part, out int orientation)
        {
            GetState(cell.BlockData, out part, out orientation);
        }
        public static void GetState(MapBase map, Vector3 global, out Part part, out int orientation)
        {
            GetState(map.GetCell(global), out part, out orientation);
        }
        public static byte GetData(Part part, int orientation)
        {
            byte data = 0;
            data = (byte)(data | (byte)part);
            data = (byte)(data | ((byte)orientation << 1));
            return data;
        }

        public static Dictionary<Part, Vector3> GetPartsDic(MapBase map, Vector3 global)
        {
            var parts = new Dictionary<Part, Vector3>();
            var partslist = BlockDefOf.Bed.GetParts(map, global).ToList();
            parts[Part.Top] = partslist[0];
            parts[Part.Bottom] = partslist[1];
            return parts;
        }
        public Dictionary<Part, IntVec3> GetPartsDic(byte data)
        {
            var parts = new Dictionary<Part, IntVec3>();
            var partslist = this.GetParts(data).ToList();
            parts[Part.Top] = partslist[0];
            parts[Part.Bottom] = partslist[1];
            return parts;
        }
        public override FurnitureDef GetFurnitureRole(MapBase map, IntVec3 global)
        {
            return IsTop(map, global) ? this.Furniture : null;
        }

        private bool IsTop(MapBase map, IntVec3 global)
        {
            return map.GetBlockEntity(global) is BlockBedEntity;
        }

        public override void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, bool notify = true)
        {
            if (!IsValidPosition(map, global, orientation))
            {
                return;
            }

            var top = global;

            var bottom = orientation switch
            {
                0 => top + IntVec3.UnitX,
                1 => top + IntVec3.UnitY,
                2 => top - IntVec3.UnitX,
                3 => top - IntVec3.UnitY,
                _ => throw new NotImplementedException()
            };

            map.SetBlock(bottom, Block.Types.Bed, GetData(Part.Bottom, orientation), 0, 0, notify);
            map.SetBlock(top, Block.Types.Bed, GetData(Part.Top, orientation), 0, 0, notify);
            var entity = new BlockBedEntity();
            map.AddBlockEntity(top, entity);
            map.Town.AddUtility(Utility.Types.Sleeping, top);
        }
        
        public override IntVec3 GetCenter(byte data, IntVec3 global)
        {
            return global + GetPartsDic(data)[Part.Top];
        }
        public override bool IsValidPosition(MapBase map, IntVec3 global, int orientation)
        {
            var positions = new List<IntVec3> { global };

            positions.Add(orientation switch
            {
                0 => global + IntVec3.UnitX,
                1 => global + IntVec3.UnitY,
                2 => global - IntVec3.UnitX,
                3 => global - IntVec3.UnitY,
                _ => throw new Exception()
            });
            foreach (var pos in positions)
            {
                if (map.GetBlock(pos).Type != Types.Air)
                {
                    return false;
                }
            }

            return true;
        }

        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            GetState(data, out var part, out var ori);
            var entity = GetEntity(chunk.Map, blockCoordinates);
            var col = entity.GetColorFromType();
            var token = this.Parts[(int)part][(ori + (int)camera.Rotation) % 4];
            return canvas.NonOpaque.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, col /*Color.White*/, sunlight, blocklight, depth, this, blockCoordinates);
        }
        public override void DrawPreview(MySpriteBatch sb, MapBase map, Vector3 global, Camera cam, Color tint, byte data, int variation = 0, int orientation = 0)
        {
            var top = global;
            var bottom = global + Vector3.UnitX;
            var bottomSecIndex = (int)cam.Rotation;
            var topSrcIndex = (int)cam.Rotation;

            switch (orientation)
            {
                case 1:
                    bottom = global + Vector3.UnitY;
                    bottomSecIndex += 1;
                    topSrcIndex += 1;
                    break;

                case 2:
                    bottom = global - Vector3.UnitX;
                    bottomSecIndex += 2;
                    topSrcIndex += 2;
                    break;

                case 3:
                    bottom = global - Vector3.UnitY;
                    bottomSecIndex += 3;
                    topSrcIndex += 3;
                    break;

                default: break;
            }
            bottomSecIndex %= 4;
            topSrcIndex %= 4;
            var topSrc = this.Parts[0][topSrcIndex];
            var bottomSrc = this.Parts[1][bottomSecIndex];

            var topd = top.GetDrawDepth(map, cam);
            var bottomd = bottom.GetDrawDepth(map, cam);
            if (topd > bottomd)
            {
                sb.DrawBlock(Block.Atlas.Texture, map, top, topSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
                sb.DrawBlock(Block.Atlas.Texture, map, bottom, bottomSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
            }
            else
            {
                sb.DrawBlock(Block.Atlas.Texture, map, bottom, bottomSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
                sb.DrawBlock(Block.Atlas.Texture, map, top, topSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
            }
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockBedEntity();
        }
       
        public static BlockBedEntity GetEntity(MapBase map, Vector3 global)
        {
            var parts = GetPartsDic(map, global);
            var entity = map.GetBlockEntity<BlockBedEntity>(parts[Part.Top]);
            return entity;
        }
        public override List<Interaction> GetAvailableTasks(MapBase map, IntVec3 global)
        {
            var list = new List<Interaction>();
            list.Add(new Blocks.Bed.InteractionStartSleep()); // commented out until i figure out how to seperate ai planting job on farmlands and player planting anywher
            return list;
        }

        internal override IEnumerable<IntVec3> GetOperatingPositions(Cell cell)
        {
            byte blockData = cell.BlockData;
            var parts = this.GetParts(blockData);
            var o = GetOrientation(blockData);
            switch (o)
            {
                case 2:
                case 0:
                    foreach (var pos in parts)
                    {
                        yield return pos + IntVec3.UnitY;
                        yield return pos - IntVec3.UnitY;
                    }
                    break;

                case 1:
                case 3:
                    foreach (var pos in parts)
                    {
                        yield return pos + IntVec3.UnitX;
                        yield return pos - IntVec3.UnitX;
                    }
                    break;

                default:
                    throw new Exception();
            }
        }
        static readonly IconButton ButtonSetVisitor = new(Icon.Construction) { HoverText = "Set to visitor bed" };
        static readonly IconButton ButtonUnsetVisitor = new(Icon.Construction, Icon.Cross) { HoverText = "Set to citizen bed" };
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var entity = GetEntity(map, vector3);
            entity.GetSelectionInfo(info, map, vector3);
        }

        private static void UpdateQuickButtons(MapBase map, Vector3 vector3, BlockBedEntity.Types t)
        {
            switch (t)
            {
                case BlockBedEntity.Types.Citizen:
                    UISelectedInfo.RemoveButton(ButtonUnsetVisitor);
                    UISelectedInfo.AddButton(ButtonSetVisitor, t => Packets.SetType(map.Net, map.Net.GetPlayer(), vector3, BlockBedEntity.Types.Visitor), (map, vector3));
                    return;

                case BlockBedEntity.Types.Visitor:
                    UISelectedInfo.RemoveButton(ButtonSetVisitor);
                    UISelectedInfo.AddButton(ButtonUnsetVisitor, t => Packets.SetType(map.Net, map.Net.GetPlayer(), vector3, BlockBedEntity.Types.Citizen), (map, vector3));
                    return;

                default:
                    throw new Exception();
            }
        }
        public static void SetType(MapBase map, IntVec3 vector3, BlockBedEntity.Types type)
        {
            GetEntity(map, vector3).Type = type;
            map.InvalidateCell(vector3);
            if (map.IsActive)
            {
                if (UISelectedInfo.GetSelected().SingleOrDefault() is TargetArgs target && target.Type == TargetType.Position && (IntVec3)target.Global == vector3)
                {
                    UpdateQuickButtons(map, vector3, type);
                }
            }
        }
        static class Packets
        {
            static readonly int PacketChangeType;
            static Packets()
            {
                PacketChangeType = Network.RegisterPacketHandler(SetType);
            }

            internal static void SetType(INetwork net, PlayerData playerData, IntVec3 vector3, BlockBedEntity.Types type)
            {
                if (net is Server)
                {
                    BlockBed.SetType(net.Map, vector3, type);
                }

                net.GetOutgoingStream().Write(PacketChangeType, playerData.ID, vector3, (int)type);
            }

            private static void SetType(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var vec = r.ReadIntVec3();
                var type = (BlockBedEntity.Types)r.ReadInt32();
                if (net is Client)
                {
                    BlockBed.SetType(net.Map, vec, type);
                }
                else
                {
                    SetType(net, player, vec, type);
                }
            }
        }
    }
}
