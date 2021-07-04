namespace Start_a_Town_
{
    interface IUndoable
    {
        bool Performed { get; }
        bool Undo();
        bool Redo();
    }
}
