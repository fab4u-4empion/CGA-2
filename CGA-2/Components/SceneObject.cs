using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA_2.Components
{
    public class SceneObject
    {
        public Vector3 Location { get; set; } = Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = One;
    }
}