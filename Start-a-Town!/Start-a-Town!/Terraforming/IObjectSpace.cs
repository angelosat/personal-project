using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IObjectSpace
    {
        float Distance(GameObject obj1, GameObject obj2);
        Vector3? DistanceVector(GameObject obj1, GameObject obj2);
    }
}
