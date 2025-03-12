using CGA2.Utils;
using System.Collections.Concurrent;
using System.Numerics;

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
    }
}
