using System.Numerics;
using static System.Single;

namespace CGA2.Components.Lights
{
    public class SphericalLight : Light
    {
        public override string Name { get; set; } = "SphericalLight";
        public float Power { get; set; } = 100;
        public float Radius { get; set; } = 0;
    }
}