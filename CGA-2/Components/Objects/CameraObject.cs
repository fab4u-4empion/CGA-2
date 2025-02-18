using CGA2.Components.Cameras;
using System.Numerics;
using static System.Numerics.Matrix4x4;
using static System.Numerics.Vector3;

namespace CGA2.Components.Objects
{
    public class CameraObject : SceneObject
    {
        public override string Name { get; set; } = "Camera";
        public Camera Camera { get; set; } = new PerspectiveCamera();

        public Matrix4x4 ViewMatrix => CreateLookTo(WorldLocation, Transform(-UnitZ, WorldRotation), Transform(UnitY, WorldRotation));

        public void Rotate(float dYaw, float dPitch, float dRoll)
        {
            Rotation = Rotation * Quaternion.CreateFromYawPitchRoll(0, dPitch, 0);
            Rotation = Quaternion.CreateFromYawPitchRoll(dYaw, 0, 0) * Rotation;
            Rotation = Rotation * Quaternion.CreateFromYawPitchRoll(0, 0, dRoll);
        }

        public void Move(float dForward, float dSide, float dUp)
        {
            Location += Transform(UnitZ, WorldRotation) * dForward + Transform(UnitX, WorldRotation) * dSide + UnitY * dUp;
        }
    }
}