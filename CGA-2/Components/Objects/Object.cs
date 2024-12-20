﻿using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.Components.Objects
{
    public abstract class Object : Component
    {
        public Vector3 Location { get; set; } = Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = One;
    }
}