using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.ToneMapping
{
    public class LinearToneMapper : ToneMapper
    {
        private static Vector3 Linear(Vector3 color) => Clamp(color, Zero, One);
        public override Vector3 CompressColor(Vector3 color) => CompressColor(color, Linear);
    }
}
