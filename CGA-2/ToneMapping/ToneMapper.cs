using System.Numerics;
using System.Runtime.CompilerServices;
using static CGA2.Settings;
using static System.Numerics.Vector3;
using static System.Single;

namespace CGA2.ToneMapping
{
    public abstract class ToneMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SrgbToLinear(Vector3 color) =>
            Create(color.X <= 0.04045f ? color.X / 12.92f : Pow((color.X + 0.055f) / 1.055f, 2.4f),
                   color.Y <= 0.04045f ? color.Y / 12.92f : Pow((color.Y + 0.055f) / 1.055f, 2.4f),
                   color.Z <= 0.04045f ? color.Z / 12.92f : Pow((color.Z + 0.055f) / 1.055f, 2.4f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LinearToSrgb(Vector3 color) => 
            Create(color.X <= 0.0031308f ? 12.92f * color.X : 1.055f * Pow(color.X, 1 / 2.4f) - 0.055f,
                   color.Y <= 0.0031308f ? 12.92f * color.Y : 1.055f * Pow(color.Y, 1 / 2.4f) - 0.055f,
                   color.Z <= 0.0031308f ? 12.92f * color.Z : 1.055f * Pow(color.Z, 1 / 2.4f) - 0.055f);
        public abstract Vector3 CompressColor(Vector3 color);
        protected static Vector3 CompressColor(Vector3 color, Func<Vector3, Vector3> callBack)
        {
            color *= Exposure;
            color = callBack(color);
            return LinearToSrgb(color);
        }
    }
}
