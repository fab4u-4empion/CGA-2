using System.Numerics;
using static System.Single;
using static System.Numerics.Matrix4x4;

namespace CGA2.Components.Cameras
{
    public class PerspectiveCamera : Camera
    {
        public override string Name { get; set; } = "PerspectiveCamera";
        public float FieldOfView { get; set; } = Pi / 4f;

        public override Matrix4x4 ProjectionMatrix => CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
    }
}