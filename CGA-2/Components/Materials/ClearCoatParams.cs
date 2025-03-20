using System.Numerics;

namespace CGA2.Components.Materials
{
    public struct ClearCoatParams(float factor, float rougness, Vector3 normal)
    {
        public float Factor { get; set; } = factor;
        public float Rougness { get; set; } = rougness;
        public Vector3 Normal { get; set; } = normal;
    }
}
