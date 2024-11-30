using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA_2.Components.Lights
{
    public class Light : SceneObject
    {
        public Vector3 Color { get; set; } = One;
    }
}