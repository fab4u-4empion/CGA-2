using CGA2.Utils;
using System.Numerics;

namespace CGA2.Components.Materials.Textures
{
    public class NonColorTexture : Texture
    {
        public override string Name { get; set; } = "NonColorTexture";

        protected override Vector4 GetTransformedPixelFromSource(Bgra32Bitmap src, int x, int y)
        {
            return src.GetRGBAPixel(x, y);
        }
    }
}
