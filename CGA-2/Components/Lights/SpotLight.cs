using static System.Single;

namespace CGA_2.Components.Lights
{
    public class SpotLight : SphericalLight
    {
        public float Angle { get; set; } = Pi / 4;
        public float Falloff { get; set; } = 0.25f;
    }
}