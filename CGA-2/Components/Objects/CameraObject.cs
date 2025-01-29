using CGA2.Components.Cameras;
using System.Numerics;
using static System.Numerics.Matrix4x4;
using static System.Numerics.Vector3;

namespace CGA2.Components.Objects
{
    public class CameraObject : Object
    {
        public override string Name { get; set; } = "Camera";
        public Camera Camera { get; set; } = new PerspectiveCamera();

        public Matrix4x4 ViewMatrix => CreateLookAt(Location, Zero, UnitY);
    }
}