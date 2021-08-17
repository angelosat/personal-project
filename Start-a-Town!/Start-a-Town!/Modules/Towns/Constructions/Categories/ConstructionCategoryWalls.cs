using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ConstructionCategoryWalls : ConstructionCategory
    {
        public override string Name => "Blocks";

        public ConstructionCategoryWalls()
        {
        }
        public override IconButton GetButton()
        {
            IconButton btn_Construct = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.Window.ToggleSmart(),
                HoverFunc = () => "Construct [" + GlobalVars.KeyBindings.Build + "]"
            };
            return btn_Construct;
        }

        public override IEnumerable<BuildToolDef> GetAvailableTools()
        {
            yield return BuildToolDefOf.Single;
            yield return BuildToolDefOf.Line;
            yield return BuildToolDefOf.Floor;
            yield return BuildToolDefOf.Wall;
            yield return BuildToolDefOf.Enclosure;
            yield return BuildToolDefOf.Box;
            yield return BuildToolDefOf.BoxFilled;
            yield return BuildToolDefOf.Pyramid;
            yield return BuildToolDefOf.Roof;
        }
    }
}