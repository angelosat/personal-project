using Start_a_Town_.Components.Resources;

namespace Start_a_Town_
{
    public static class ResourceDefOf
    {
        static public readonly ResourceDef Health =
            new Health()
                .AddThreshold("Dying", .25f)
                .AddThreshold("Critical", .5f)
                .AddThreshold("Injured", .75f)
                .AddThreshold("Healthy", 1f);

        static public readonly ResourceDef Stamina =
            new Stamina()
                .AddThreshold("Out of breath", .25f)
                .AddThreshold("Exhausted", .5f)
                .AddThreshold("Tired", .75f)
                .AddThreshold("Energetic", 1f);

        static public readonly ResourceDef Durability = new Durability().AddThreshold("Durability", 1);
        static public readonly ResourceDef HitPoints = new HitPoints();
        static ResourceDefOf()
        {
            Def.Register(Health);
            Def.Register(Stamina);
            Def.Register(Durability);
            Def.Register(HitPoints);
        }
    }
}
