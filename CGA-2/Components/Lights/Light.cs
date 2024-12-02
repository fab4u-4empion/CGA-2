using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.Components.Lights
{
    public abstract class Light : Component
    {
        public Vector3 Color { get; set; } = One;
    }
}