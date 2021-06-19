using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UI
{
    //public interface IInputHandler
    //{
    //    //float ZOrder { get; }
    //    void HandleInput(InputState input);
    //}

    public interface IKeyEventHandler
    {
        //float ZOrder { get; }
        void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e);
        void HandleKeyDown(System.Windows.Forms.KeyEventArgs e);
        void HandleKeyUp(System.Windows.Forms.KeyEventArgs e);
        void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e);
        void HandleMouseWheel(HandledMouseEventArgs e);
        void HandleLButtonDoubleClick(HandledMouseEventArgs e);
    }
}
