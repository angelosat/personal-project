﻿using Microsoft.Xna.Framework;

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
}