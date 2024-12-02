namespace CGA2.Components.Objects
{
    public class MeshObject : Object
    {
        public override string Name { get; set; } = "Mesh";
        public Mesh Mesh { get; set; } = new();
    }
}