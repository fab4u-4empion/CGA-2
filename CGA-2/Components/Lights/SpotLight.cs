using System.Numerics;
using static System.Single;

namespace CGA2.Components.Lights
{
    public class SpotLight : SphericalLight
    {
        public override string Name { get; set; } = "SpotLight";
        public float Angle { get; set; } = Pi / 4;
        public float Falloff { get; set; } = 0.25f;
    }
}