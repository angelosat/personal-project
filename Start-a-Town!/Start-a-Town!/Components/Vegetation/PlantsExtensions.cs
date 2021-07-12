namespace Start_a_Town_.Components.Vegetation
{
    static class PlantsExtensions
    {
        /// <summary>
        /// TODO move to extensions class
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        static public bool IsSeedFor(this GameObject entity, PlantProperties plant)
        {
            return entity.Def == ItemDefOf.Seeds && entity.GetComp<SeedComponent>().Plant == plant;
        }

    }
}
