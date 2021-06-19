using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    interface IDropTarget
    {
        //event EventHandler<DragDropArgs> DragEnter;
        //event EventHandler<DragDropArgs> DragLeave;
        event EventHandler<DragEventArgs> DragDrop;
        Func<DragEventArgs, DragDropEffects> DragDropAction { get; set; }
        DragDropEffects Drop(DragEventArgs args);
    }
}
