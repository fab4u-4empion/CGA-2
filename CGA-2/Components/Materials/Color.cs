using System.Numerics;

namespace CGA2.Components.Materials
{
    public struct Color
    {
        public Vector3 BaseColor { get; set; }
        public float Alpha { get; set; }

        public Color(Vector3 baseColor)
        {
            BaseColor = baseColor;
            Alpha = 1;
        }

        public Color(Vector3 baseColor, float alpha)
        {
            BaseColor = baseColor;
            Alpha = alpha;
        }

        public Color(Vector4 baseColor)
        {
            BaseColor = baseColor.AsVector3();
            Alpha = baseColor.W;
        }
    }
}
