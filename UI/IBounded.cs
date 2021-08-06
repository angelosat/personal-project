using Microsoft.Xna.Framework;

namespace UI
{
    public interface IBounded
    {
        Rectangle Bounds { get; }
    }
    public interface IBoundedCollection
    {
        Rectangle ContainerSize { get; }
        IBounded[] Children { get; }
    }
    public class BoundedCollection
    {
        Rectangle ContainerSize { get; }
        Rectangle[] Children { get; }
    }
}
