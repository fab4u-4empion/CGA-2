using CGA2.Components.Cameras;
using System.Numerics;
using System.Windows;
using static System.Numerics.Matrix4x4;
using static System.Numerics.Vector3;
using static System.Single;

namespace CGA2.Components.Objects
{
    public class CameraObject : SceneObject
    {
        public enum CameraMode
        {
            Free,
            Arcball
        }

        public override string Name { get; set; } = "Camera";
        public Camera Camera { get; set; } = new PerspectiveCamera();

        public CameraMode Mode { get; set; } = CameraMode.Arcball;

        public float TargetRadius { get; set; } = 25f;

        public Matrix4x4 ViewMatrix => CreateLookTo(WorldLocation, Transform(-UnitZ, WorldRotation), Transform(UnitY, WorldRotation));

        public void Rotate(float dYaw, float dPitch, float dRoll)
        {
            if (Mode == CameraMode.Arcball)
            {
                Vector3 target = Normalize(Transform(-UnitZ, WorldRotation)) * TargetRadius;

                Yaw += dYaw * 4f;
                Pitch = Clamp(Pitch + dPitch * 4f, -float.Pi / 2f, float.Pi / 2f);
                Roll = Clamp(Roll + dRoll, -float.Pi / 2f, float.Pi / 2f);

                Location -= (Normalize(Transform(-UnitZ, WorldRotation)) * TargetRadius - target);
            }

            if (Mode == CameraMode.Free)
            {
                Yaw += dYaw;
                Pitch = Clamp(Pitch + dPitch, -float.Pi / 2f, float.Pi / 2f);
                Roll = Clamp(Roll + dRoll, -float.Pi / 2f, float.Pi / 2f);
            }
        }

        public void UpdateTragetRadius(float dR)
        {
            if (Mode == CameraMode.Arcball)
            {
                Vector3 target = Normalize(Transform(-UnitZ, WorldRotation)) * TargetRadius;
                TargetRadius = Max(TargetRadius + dR, 0.1f);
                Location -= (Normalize(Transform(-UnitZ, WorldRotation)) * TargetRadius - target);
            }
                
        }

        public void Move(float dForward, float dSide, float dUp)
        {
            Location += Transform(UnitZ, WorldRotation) * dForward + Transform(UnitX, WorldRotation) * dSide + UnitY * dUp;
        }
    }
}