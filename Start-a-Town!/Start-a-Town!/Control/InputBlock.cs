using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UI;

namespace Start_a_Town_
{
    class InputBlock : IKeyEventHandler
    {
        public void HandleInput(InputState input) { input.Handled = true; }
        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e) { e.Handled = true; }
        public virtual void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e) { e.Handled = true; }
        public virtual void HandleKeyUp(System.Windows.Forms.KeyEventArgs e) { e.Handled = true; }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { e.Handled = true; }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e) { }
    }
}
