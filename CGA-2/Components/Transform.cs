using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.Components
{
    public class Transform()
    {
        public Vector3 Location { get; set; } = Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = One;
    }
}
