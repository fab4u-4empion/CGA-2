using System.Numerics;
using static System.Numerics.Matrix4x4;

namespace CGA2.Components.Cameras
{
    public class OrthographicCamera : Camera
    {
        public override string Name { get; set; } = "OrthographicCamera";
        public float Scale { get; set; } = 1;

        public override Matrix4x4 ProjectionMatrix => CreateOrthographic(AspectRatio * Scale, Scale, NearPlane, FarPlane);
    }
}