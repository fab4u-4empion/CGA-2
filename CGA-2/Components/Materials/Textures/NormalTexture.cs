using System.Numerics;
using CGA2.Utils;
using static System.Numerics.Vector4;

namespace CGA2.Components.Materials.Textures
{
    public class NormalTexture : Texture
    {
        public override string Name { get; set; } = "NormalTexture";

        protected override Vector4 GetTransformedPixelFromSource(Bgra32Bitmap src, int x, int y)
        {
            Vector4 color = src.GetRGBAPixel(x, y);

            return 2f * color - One; 
        }
    }
}
