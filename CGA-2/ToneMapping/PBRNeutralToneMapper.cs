using System.Numerics;
using static System.Single;
using static System.Numerics.Vector3;

namespace CGA2.ToneMapping
{
    public class PBRNeutralToneMapper : ToneMapper
    {
        public override string Name { get; set; } = "PBRNeutral";

        private static Vector3 PBRNeutral(Vector3 color)
        {
            const float startCompression = 0.8f - 0.04f;
            const float desaturation = 0.15f;

            float x = float.Min(color.X, Min(color.Y, color.Z));
            float offset = x < 0.08f ? x - 6.25f * x * x : 0.04f;
            color -= Create(offset);

            float peak = Max(color.X, Max(color.Y, color.Z));
            if (peak < startCompression) return color;

            const float d = 1 - startCompression;
            float newPeak = 1 - d * d / (peak + d - startCompression);
            color *= newPeak / peak;

            float g = 1 - 1 / (desaturation * (peak - newPeak) + 1);
            return Lerp(color, newPeak * One, g);
        }
        public override Vector3 CompressColor(Vector3 color) => CompressColor(color, PBRNeutral);
    }
}
