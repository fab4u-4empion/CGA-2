using System.Numerics;
using static System.Numerics.Vector3;


namespace CGA2.ToneMapping
{
    public class ReinhardToneMapper : ToneMapper
    {
        private static Vector3 Reinhard(Vector3 color) => color / (One + color);
        public override Vector3 CompressColor(Vector3 color) => CompressColor(color, Reinhard);
    }
}
