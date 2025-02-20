using System.Numerics;
using static System.Numerics.Matrix4x4;
using static System.Single;
using static System.Numerics.Quaternion;

namespace CGA2.Components.Objects
{
    public class SceneObject : Component
    {
        public override string Name { get; set; } = "Object";

        public SceneObject? Parent { get; set; }
        public List<SceneObject> Children { get; set; } = [];

        public float Yaw { get; set; } = 0f;
        public float Pitch { get; set; } = 0f;
        public float Roll { get; set; } = 0f;

        public Vector3 Location { get; set; } = Vector3.Zero;
        public Quaternion Rotation { 
            get
            {
                return Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, Roll);
            }
            set
            {
                Quaternion q = Normalize(value);

                Yaw = Atan2(2 * (q.Y * q.W + q.X * q.Z), 1f - 2f * (q.Y * q.Y + q.X * q.X));
                Pitch = Asin(2 * (q.X * q.W - q.Z * q.Y));
                Roll = Atan2(2 * (q.Y * q.X + q.W * q.Z), 1f - 2f * (q.X * q.X + q.Z * q.Z));
            } 
        }
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4x4 WorldMatrix => CreateScale(Scale) * CreateFromQuaternion(Rotation) * CreateTranslation(Location) * (Parent?.WorldMatrix ?? Matrix4x4.Identity);

        public Quaternion WorldRotation => Rotation * (Parent?.WorldRotation ?? Quaternion.Identity);

        public Vector3 WorldLocation => Location + (Parent?.WorldLocation ?? Vector3.Zero);
    }
}