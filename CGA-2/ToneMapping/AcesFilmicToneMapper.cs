using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.ToneMapping
{
    public class AcesFilmicToneMapper : ToneMapper
    {
        public override string Name { get; set; } = "ACES";

        private static readonly Matrix4x4 ACESInputMat = new
        (
            0.59719f, 0.07600f, 0.02840f, 0f,
            0.35458f, 0.90834f, 0.13383f, 0f,
            0.04823f, 0.01566f, 0.83777f, 0f,
                  0f,       0f,       0f, 1f
        );

        private static readonly Matrix4x4 ACESOutputMat = new
        (
             1.60475f, -0.10208f, -0.00327f, 0f,
            -0.53108f,  1.10813f, -0.07276f, 0f,
            -0.07367f, -0.00605f,  1.07602f, 0f,
                   0f,        0f,        0f, 1f
        );

        private static Vector3 RRTAndODTFit(Vector3 v)
        {
            Vector3 a = v * (v + Create(0.0245786f)) - Create(0.000090537f);
            Vector3 b = v * (0.983729f * v + Create(0.4329510f)) + Create(0.238081f);
            return a / b;
        }

        public static Vector3 AcesFilmic(Vector3 color)
        {
            color /= 0.6f;
            color = Transform(color, ACESInputMat);
            color = RRTAndODTFit(color);
            color = Transform(color, ACESOutputMat);
            color = Clamp(color, Zero, One);
            return color;
        }

        public override Vector3 CompressColor(Vector3 color) => CompressColor(color, AcesFilmic);
    }
}
