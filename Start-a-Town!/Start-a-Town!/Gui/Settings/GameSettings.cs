using Start_a_Town_.UI;

namespace Start_a_Town_
{
    abstract class GameSettings
    {
        internal abstract GroupBox Gui { get; }
        internal abstract void Apply();
    }
}
