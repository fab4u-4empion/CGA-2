using System.Numerics;

namespace CGA2.Components
{
    public class Mesh : Component
    {
        public override string Name { get; set; } = "Mesh";

        public List<Vector3> Positions = [];
        public List<Vector3> Normals = [];
        public List<Vector2> UVs = [];
        public List<ushort> Triangles = [];
    }
}