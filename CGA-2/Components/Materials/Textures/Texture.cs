using CGA2.Utils;
using System.Collections.Concurrent;
using System.Numerics;
using static System.Numerics.Vector4;
using static System.Single;
using static CGA2.Settings;
using static System.Int32;

namespace CGA2.Components.Materials.Textures
{
    public abstract class Texture : Component
    {
        public override string Name { get; set; } = "Texture";

        protected List<Buffer<Vector4>> MIP { get; set; }

        protected abstract Vector4 GetTransformedPixelFromSource(Bgra32Bitmap src, int x, int y);

        public Texture()
        {
            MIP = new(15);
        }

        public void Create(Bgra32Bitmap src)
        {
            Buffer<Vector4> mainLOD = new(src.PixelWidth, src.PixelHeight);

            Parallel.ForEach(Partitioner.Create(0, mainLOD.Height), (range) =>
            {
                for (int y = range.Item1; y < range.Item2; y++)
                    for (int x = 0; x < mainLOD.Width; x++)
                        mainLOD[x, y] = GetTransformedPixelFromSource(src, x, y);
            });

            MIP.Add(mainLOD);

            int sizeW = src.PixelWidth;
            int sizeH = src.PixelHeight;
            int currentLODIndx = 0;

            do
            {
                sizeW /= 2;
                sizeH /= 2;

                Buffer<Vector4> nextLOD = new(sizeW, sizeH);

                Parallel.ForEach(Partitioner.Create(0, nextLOD.Height), (range) =>
                {
                    for (int y = range.Item1; y < range.Item2; y++)
                        for (int x = 0; x < nextLOD.Width; x++)
                        {
                            int px = x * 2;
                            int py = y * 2;

                            Buffer<Vector4> currentLOD = MIP[currentLODIndx];

                            Vector4 color = currentLOD[px, py]
                                + currentLOD[px + 1, py]
                                + currentLOD[px, py + 1]
                                + currentLOD[px + 1, py + 1];

                            color *= 0.25f;

                            nextLOD[x, y] = color;
                        }
                });

                MIP.Add(nextLOD);

                currentLODIndx++;

            } while (sizeW > 1 && sizeH > 1);
        }

        public Vector4 GetSample(Vector2 uv, int mipIndex = 0)
        {
            Buffer<Vector4> src = MIP[mipIndex];

            float u = uv.X * src.Width - 0.5f;
            float v = uv.Y * src.Height - 0.5f;

            int x0 = (int)Floor(u);
            int y0 = (int)Floor(v);

            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float u_ratio = u - x0;
            float v_ratio = v - y0;

            x0 &= (src.Width - 1);
            x1 &= (src.Width - 1);

            y0 &= (src.Height - 1);
            y1 &= (src.Height - 1);

            return Lerp(
                Lerp(src[x0, y0], src[x1, y0], u_ratio),
                Lerp(src[x0, y1], src[x1, y1], u_ratio),
                v_ratio
            );
        }

        public Vector4 GetSample(Vector2 uv, Vector2 uv1, Vector2 uv2)
        {
            float length1 = ((uv - uv1) * MIP[0].Size).Length();
            float length2 = ((uv - uv2) * MIP[0].Size).Length();

            float max = Max(length1, length2);
            float min = Min(length1, length2);

            float aniso = MaxMagnitudeNumber(Min(max / min, MaxAnisotropy), 1);

            int N = (int)Round(aniso, MidpointRounding.AwayFromZero);
            float lod = Clamp(Log2(max / aniso), 0, MIP.Count - 1);

            int mainLod = (int)lod;
            int nextLod = Min(mainLod + 1, MIP.Count - 1);

            (Vector2 a, Vector2 b) = length1 > length2 ? (uv, uv1) : (uv, uv2);

            if (N == 1)
            {
                return Lerp(GetSample(uv, mainLod), GetSample(uv, nextLod), lod - mainLod);
            }
            else
            {
                Vector2 k = (b - a) / N;
                a += 0.5f * (k + a - b);

                Vector4 mainColor = Zero;
                Vector4 nextColor = Zero;

                for (int i = 0; i < N; i++, a += k)
                {
                    mainColor += GetSample(a, mainLod);
                    nextColor += GetSample(a, nextLod);
                }

                return Lerp(mainColor, nextColor, lod - mainLod) / N;
            }
        }

    }
}
