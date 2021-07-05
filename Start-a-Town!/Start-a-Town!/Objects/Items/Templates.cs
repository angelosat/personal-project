using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public partial class ItemTemplate
    {
        static int _IDSequence = 0;
        public static int GetNextID()
        {
            return IDOffset + _IDSequence++;
        }
        const int IDOffset = 10000;

        static public void Initialize()
        {
        }

        static public Entity Item
        {
            get
            {
                var obj = new Entity();
                obj.AddComponent(new DefComponent());
                obj.AddComponent<PhysicsComponent>();
                obj.AddComponent(new OwnershipComponent());
                return obj;
            }
        }
    }
}
