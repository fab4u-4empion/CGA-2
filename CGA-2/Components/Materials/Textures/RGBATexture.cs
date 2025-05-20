using CGA2.Utils;
using System.Numerics;
using static CGA2.ToneMapping.ToneMapper;

namespace CGA2.Components.Materials.Textures
{
    public class RGBATexture : Texture
    {
        public override string Name { get; set; } = "BaseColorTexture";

        protected override Vector4 GetTransformedPixelFromSource(Bgra32Bitmap src, int x, int y)
        {
            Vector4 color = src.GetRGBAPixel(x, y);

            return Vector4.Create(SrgbToLinear(color.AsVector3()), color.W);
        }
    }
}
