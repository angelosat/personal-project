namespace Start_a_Town_
{
    static class ExtensionsVisitors
    {
        internal static VisitorProperties GetVisitorProperties(this Actor actor)
        {
            return actor.Map.World.Population.GetVisitorProperties(actor);
        }
    }
}
