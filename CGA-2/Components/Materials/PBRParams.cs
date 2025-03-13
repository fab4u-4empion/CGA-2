using System.Numerics;

namespace CGA2.Components.Materials
{
    public struct PBRParams
    {
        public float Metallic { get; set; }

        public float Roughness { get; set; }

        public float Occlusion { get; set; }

        public PBRParams(float occlusion, float roughness, float metallic)
        {
            Occlusion = occlusion;
            Roughness = roughness;
            Metallic = metallic;
        }

        public PBRParams(Vector3 orm)
        {
            Occlusion = orm.X;
            Roughness = orm.Y;
            Metallic = orm.Z;
        }
    }
}
