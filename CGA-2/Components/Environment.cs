using CGA2.Components.Materials.Textures;
using System.Numerics;

namespace CGA2.Components
{
    public class Environment : Component
    {
        public override string Name { get; set; } = "Environment";
        public Vector3 Color { get; set; } = new(0.15f);

        public HDRTexture? IBLDiffuseMap { get; set; } = null;
        public List<HDRTexture> IBLSpecularMap { get; set; } = [];
        public HDRTexture BRDFLUT { get; set; } = new("Assets\\BRDFIntegrationMap.pfm");

        public Vector3 GetIBLDiffuseColor(Vector3 n)
        {
            return IBLDiffuseMap?.GetColor(n) ?? Color;
        }

        public Vector3 GetIBLSpecularColor(Vector3 n, int lod)
        {
            if (IBLSpecularMap.Count == 0)
                return Color;

            return IBLSpecularMap[lod].GetColor(n);
        }
    }
}
