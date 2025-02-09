using System.Numerics;

namespace CGA2.Utils
{
    public class Buffer<T>(int width, int height)
    {
        public int Width { get; } = width;
        public int Height { get; } = height;
        public Vector2 Size { get; } = new(width, height);
        public T[] Array { get; set; } = new T[width * height];

        public ref T this[int x, int y]
        {
            get => ref Array[y * Width + x];
        }
    }
}
