using System.Numerics;
using static System.Numerics.Matrix4x4;

namespace CGA2.Components.Objects
{
    public class SceneObject : Component
    {
        public override string Name { get; set; } = "Object";

        public SceneObject? Parent { get; set; }
        public List<SceneObject> Children { get; set; } = [];

        public Vector3 Location { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4x4 WorldMatrix => CreateScale(Scale) * CreateFromQuaternion(Rotation) * CreateTranslation(Location) * (Parent?.WorldMatrix ?? Identity);
    }
}