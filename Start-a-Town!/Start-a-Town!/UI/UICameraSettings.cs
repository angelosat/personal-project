using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class UICameraSettings : Panel
    {
        Camera Camera;
        CheckBoxNew Chk_HideWalls, Chk_HideTerrain, Chk_BlockOutlines, Chk_Fog, Chk_Unknown, Chk_TopSlice, Chk_Designations;
        Camera GetCamera()
        {
            return Rooms.Ingame.GetMap().Camera;
        }
        public UICameraSettings(Camera camera)
        {
            AutoSize = true;
            this.Camera = camera;
            
            Chk_HideWalls = new CheckBoxNew("Hide Walls", Vector2.Zero, Engine.HideWalls) { HoverText = "Hides blocks obscuring an entity when the camera is following it" };
            Chk_HideWalls.LeftClickAction = ()=> Engine.HideWalls = !Engine.HideWalls;
            Chk_HideWalls.TickedFunc = () => Engine.HideWalls;
            Chk_HideTerrain = new CheckBoxNew("Hide Terrain", Chk_HideWalls.BottomLeft, camera.HideTerrainAbovePlayer) { LeftClickAction = () => camera.ToggleHideBlocksAbove() };
            Chk_BlockOutlines = new CheckBoxNew("Block Outlines", Chk_HideTerrain.BottomLeft, Engine.BlockOutlines);
            Chk_BlockOutlines.LeftClickAction = ()=> Engine.BlockOutlines = !Engine.BlockOutlines;
            Chk_BlockOutlines.TickedFunc = () => Engine.BlockOutlines;
            Chk_Fog = new CheckBoxNew("Fog", Camera.Fog) { HoverText = "Toggles drawing fog at lower elevation levels than the one under the mouse cursor", Location = Chk_HideTerrain.BottomLeft, LeftClickAction = () => Camera.Fog = !Camera.Fog };
            Chk_Unknown = new CheckBoxNew("Hide blocks", this.GetCamera().HideUnknownBlocks) {HoverText = "Toggles blocks not exposed to air being drawn as unknown blocks", Location = Chk_Fog.BottomLeft, LeftClickAction = () => this.GetCamera().HideUnknownBlocks = !this.GetCamera().HideUnknownBlocks };
            Chk_TopSlice = new CheckBoxNew("Map boundaries", this.Camera.DrawTopSlice) { HoverText = "Draws blocks at the boundaries of the map", Location = Chk_Unknown.BottomLeft, LeftClickAction = () => this.Camera.DrawTopSlice = !this.Camera.DrawTopSlice };
            Chk_Designations = new CheckBoxNew("Designations", this.Camera.DrawZones) { HoverText = "Toggles drawing of designated zones", Location = Chk_TopSlice.BottomLeft, LeftClickAction = () => this.Camera.DrawZones = !this.Camera.DrawZones };
            var chk_Nameplates = new CheckBoxNew("Nameplates") { IsToggledFunc = () => Rooms.Ingame.Instance.NameplateManager.NameplatesEnabled, HoverText = "Toggles entity nameplates", LeftClickAction = () => Rooms.Ingame.Instance.NameplateManager.ToggleNameplates() };
            var rooms = new CheckBoxNew("Rooms") { TickedFunc = () => Engine.DrawRooms, LeftClickAction = () => Engine.DrawRooms = !Engine.DrawRooms };

            this.AddControlsVertically(Chk_HideWalls,
                Chk_Fog
                ,Chk_Unknown
                ,Chk_TopSlice
                ,Chk_Designations,
                chk_Nameplates,
                rooms
                );
        }
       
        void BorderShading_Click(object sender, EventArgs e)
        {
            this.Camera.BorderShading = !this.Camera.BorderShading;
        }

        void HideUnderground_Click(object sender, EventArgs e)
        {
            this.Camera.HideUnderground = !this.Camera.HideUnderground;
        }

        void Fog_Click(object sender, EventArgs e)
        {
            Camera.Fog = !Camera.Fog;
        }

        public override void Reposition(Vector2 ratio)
        {
            base.Reposition(ratio);
        }
    }
}
