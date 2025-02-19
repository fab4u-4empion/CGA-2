using CGA2.Components.Cameras;
using System.Numerics;
using static System.Numerics.Matrix4x4;
using static System.Numerics.Vector3;
using static System.Single;

namespace CGA2.Components.Objects
{
    public class CameraObject : SceneObject
    {
        public override string Name { get; set; } = "Camera";
        public Camera Camera { get; set; } = new PerspectiveCamera();

        public Matrix4x4 ViewMatrix => CreateLookTo(WorldLocation, Transform(-UnitZ, WorldRotation), Transform(UnitY, WorldRotation));

        public void Rotate(float dYaw, float dPitch, float dRoll)
        {
            Yaw += dYaw;
            Pitch = Clamp(Pitch + dPitch, -float.Pi / 2f, float.Pi / 2f);
            Roll = Clamp(Roll + dRoll, -float.Pi / 2f, float.Pi / 2f);
        }

        public void Move(float dForward, float dSide, float dUp)
        {
            Location += Transform(UnitZ, WorldRotation) * dForward + Transform(UnitX, WorldRotation) * dSide + UnitY * dUp;
        }
    }
}