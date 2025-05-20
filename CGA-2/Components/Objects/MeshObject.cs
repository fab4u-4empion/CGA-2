using System.Numerics;

namespace CGA2.Components.Objects
{
    public class MeshObject : SceneObject
    {
        public override string Name { get; set; } = "Mesh";
        public Mesh Mesh { get; set; } = new();

        public List<Vector3> WorldPositions { get; set; } = [];
        public List<Vector3> WorldNormals { get; set; } = [];
        public List<Vector3> WorldTangents { get; set; } = [];
    }
}