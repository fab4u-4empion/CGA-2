using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using CGA2.Utils;
using static CGA2.Utils.BinaryReaderTools;
using static CGA2.Utils.UnsafeTools;
using static CGA2.Settings;
using static System.Single;
using static System.Numerics.Vector3;
using static System.Int32;

namespace CGA2.Components.Materials.Textures
{
    public class HDRTexture : Component
    {
        public override string Name { get; set; } = "HDRTexture";

        private Buffer<Vector3> Source { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public HDRTexture(string path)
        {
            using Stream stream = File.OpenRead(path);
            using BinaryReader reader = new(stream, Encoding.Default, true);

            ReadLine(reader);

            int[] size = [.. ReadLine(reader)
                .Split(" ")
                .Select(e => int.Parse(e))
            ];

            Source = new(size[0], size[1]);
            (Width, Height) = (size[0], size[1]);

            float order = float.Parse(ReadLine(reader), CultureInfo.InvariantCulture);

            for (int y = Height - 1; y > -1; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    float r = reader.ReadSingle();
                    float g = reader.ReadSingle();
                    float b = reader.ReadSingle();

                    if (order == 1)
                    {
                        r = ChangeEndianness(r);
                        g = ChangeEndianness(g);
                        b = ChangeEndianness(b);
                    }

                    Source[x, y] = new(r, g, b);
                }
            }
        }

        public Bgra32Bitmap ToLDR()
        {
            Bgra32Bitmap bmp = new(Width, Height);

            bmp.Source.Lock();

            Parallel.ForEach(Partitioner.Create(0, Height), range =>
            {
                for (int y = range.Item1; y < range.Item2; y++)
                    for (int x = 0; x < Width; x++)
                        bmp.SetPixel(x, y, ToneMapper.CompressColor(Source[x, y]));
            });

            bmp.Source.AddDirtyRect(new(0, 0, Width, Height));
            bmp.Source.Unlock();

            return bmp;
        }

        private Vector3 GetColorXY(float x, float y)
        {
            int x0 = (int)Floor(x);
            int y0 = (int)Floor(y);

            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float x_ratio = x - x0;
            float y_ratio = y - y0;

            x0 &= (Source!.Width - 1);
            x1 &= (Source!.Width - 1);

            y0 = Max(0, y0);
            y1 = Min(Source.Height - 1, y1);

            return Lerp(
                Lerp(Source[x0, y0], Source[x1, y0], x_ratio),
                Lerp(Source[x0, y1], Source[x1, y1], x_ratio),
                y_ratio
            );
        }

        public Vector3 GetColor(Vector3 N)
        {
            float theta = Acos(Clamp(N.Y, -1, 1));
            float phi = Atan2(N.X, -N.Z) + float.Pi;

            float x = phi / float.Tau * Width - 0.5f;
            float y = theta / float.Pi * Height - 0.5f;

            return GetColorXY(x, y);
        }

        public Vector3 GetColor(float u, float v)
        {
            float x = u * Width - 0.5f;
            float y = v * Height - 0.5f;

            return GetColorXY(x, y);
        }
    }
}
