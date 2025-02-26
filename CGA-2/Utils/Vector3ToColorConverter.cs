using System.Numerics;
using System.Windows.Media;
using static System.Numerics.Vector3;

namespace CGA2.Utils
{
    public class Vector3ToColorConverter
    {
        public static Color Vector3ToColor(Vector3 color)
        {
            color *= 255f;
            return Color.FromRgb((byte)color.X, (byte)color.Y, (byte)color.Z);
        }

        public static Vector3 ColorToVector3(Color color)
        {
            return Create(color.R, color.G, color.B) / 255f;
        }
    }
}
