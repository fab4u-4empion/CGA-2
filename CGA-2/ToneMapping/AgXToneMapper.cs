using System.Numerics;
using static System.Numerics.Vector3;
using static System.Single;

namespace CGA2.ToneMapping
{
    public class AgXToneMapper : ToneMapper
    {
        private static readonly Matrix4x4 LinearRec709ToLinearFilmLightEGamut = new
        (
            0.5594630473276861f, 0.0762332608733703f, 0.0655375095152927f, 0f,
            0.3047758110283366f, 0.7879523952184488f, 0.1645427298716744f, 0f,
            0.1358129414038276f, 0.1357748488287584f, 0.7697415276874705f, 0f,
                             0f, 0f, 0f, 1f
        );

        private static readonly Vector3 AgXMinEV = Create(-12.47393f);
        private static readonly Vector3 AgXMaxEV = Create(12.5260688117f);
        private static readonly Lut3D AgXBaseSrgb = new("AgX_Base_sRGB.cube");

        private static Vector3 AgX(Vector3 color)
        {
            color = Max(Zero, Transform(color, LinearRec709ToLinearFilmLightEGamut));
            color = Clamp((Log2(color) - AgXMinEV) / (AgXMaxEV - AgXMinEV), Zero, One);
            color = AgXBaseSrgb.TetrahedralSample(color);
            color = Create(Pow(color.X, 2.4f), Pow(color.Y, 2.4f), Pow(color.Z, 2.4f));
            return color;
        }

        public override Vector3 CompressColor(Vector3 color) => CompressColor(color, AgX);
    }
}
