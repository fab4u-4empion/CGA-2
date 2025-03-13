using CGA2.Components.Materials.Textures;
using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.Components.Materials
{
    public enum AlphaMode
    {
        OPAQUE,
        MASK,
        BLEND
    }

    public class Material : Component
    {
        public override string Name { get; set; } = "Material";

        public bool DoubleSided { get; set; } = false;

        public Vector3 BaseColor { get; set; } = One;

        public float Alpha { get; set; } = 1f;
        public float AlphaCutoff { get; set; } = 0.5f;
        public AlphaMode AlphaMode { get; set; } = AlphaMode.OPAQUE;

        public float OcclusionFactor { get; set; } = 1f;
        public float MetallicFactor { get; set; } = 1f;
        public float RoghnessFactor { get; set; } = 1f;
        public Vector3 EmissiveFactor { get; set; } = Zero;

        public RGBATexture? BaseColorTexture { get; set; } = null;
        public NormalTexture? NormalTexture { get; set; } = null;
        public NonColorTexture? RMTexture { get; set; } = null;
        public NonColorTexture? OcclusionTexture { get; set; } = null;
        public RGBATexture? EmissiveTexture { get; set; } = null;

        public bool IsTransparent => AlphaMode != AlphaMode.OPAQUE;

        public Color GetBaseColor(Vector2 uv, Vector2 uv1, Vector2 uv2)
        {
            Color color = new(BaseColorTexture?.GetSample(uv, uv1, uv2) ?? new(BaseColor, Alpha)) ;

            if (AlphaMode == AlphaMode.MASK)
                color.Alpha = color.Alpha < AlphaCutoff ? 0f : 1f;

            if (AlphaMode == AlphaMode.OPAQUE)
                color.Alpha = 1f;

            return color;
        }

        public Vector3 GetEmission(Vector2 uv, Vector2 uv1, Vector2 uv2)
        {
            return EmissiveTexture?.GetSample(uv, uv1, uv2).AsVector3() ?? EmissiveFactor;
        }

        public PBRParams GetPBRParams(Vector2 uv, Vector2 uv1, Vector2 uv2)
        {
            return new(RMTexture?.GetSample(uv, uv1, uv2).AsVector3() ?? new Vector3(1f, RoghnessFactor, MetallicFactor))
            {
                Occlusion = OcclusionTexture?.GetSample(uv, uv1, uv2).X ?? 1
            };
        }
    }
}
