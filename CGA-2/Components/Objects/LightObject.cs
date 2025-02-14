﻿using CGA2.Components.Lights;

namespace CGA2.Components.Objects
{
    public class LightObject : SceneObject
    {
        public override string Name { get; set; } = "Light";
        public Light Light { get; set; } = new SphericalLight();
    }
}