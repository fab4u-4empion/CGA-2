namespace CGA2.Components.Objects
{
    public class MeshObject : SceneObject
    {
        public override string Name { get; set; } = "Mesh";
        public Mesh Mesh { get; set; } = new();
    }
}