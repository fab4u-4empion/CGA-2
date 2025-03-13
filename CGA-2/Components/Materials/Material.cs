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

        public Vector3 Emission { get; set; } = Zero;

        public RGBATexture? BaseColorTexture { get; set; } = null;
        public NormalTexture? NormalTexture { get; set; } = null;
        public NonColorTexture? RMTexture { get; set; } = null;
        public NonColorTexture? OcclusionTexture { get; set; } = null;
        public RGBATexture? EmissiveTexture { get; set; } = null;

        public bool IsTransparent => AlphaMode != AlphaMode.OPAQUE;
    }
}
