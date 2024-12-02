﻿using static System.Single;

namespace CGA2.Components
{
    public class Camera
    {
        public float FieldOfView { get; set; } = Pi / 4f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 500f;
        public float AspectRatio { get; set; }
    }
}