﻿namespace UI
{
    public class GuiLayer
    {
        string Name;
        public GuiLayer(string name)
        {
            this.Name = name;
        }
        public override string ToString()
        {
            return this.Name;
        }
    }
}
