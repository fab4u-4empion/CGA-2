using System.Numerics;
using static System.Numerics.Vector2;

namespace CGA2.Utils
{
    public class Buffer<T>(int width, int height)
    {
        public int Width { get; } = width;
        public int Height { get; } = height;
        public Vector2 Size { get; } = Create(width, height);
        private T[] Array { get; set; } = new T[width * height];

        public ref T this[int x, int y]
        {
            get => ref Array[y * Width + x];
        }

        public ref T this[int i]
        {
            get => ref Array[i];
        }

        public static implicit operator T[](Buffer<T> buffer) => buffer.Array;
        public static implicit operator Array(Buffer<T> buffer) => buffer.Array;
    }
}
