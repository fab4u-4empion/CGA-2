using System.Numerics;

namespace CGA2.Components
{
    public class Environment : Component
    {
        public override string Name { get; set; } = "Environment";
        public Vector3 Color { get; set; } = new(0.15f);
    }
}
