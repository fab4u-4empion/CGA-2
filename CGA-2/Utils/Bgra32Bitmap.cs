using System.Numerics;
using System.Windows.Media.Imaging;
using static System.Windows.Media.PixelFormats;

namespace CGA2.Utils
{
    public unsafe class Bgra32Bitmap
    {
        private byte* BackBuffer { get; set; }
        private int BackBufferStride { get; set; }
        private int BytesPerPixel { get; set; }

        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public WriteableBitmap Source { get; private set; }

        public Bgra32Bitmap(int pixelWidth, int pixelHeight)
        {
            Source = new(pixelWidth, pixelHeight, 96, 96, Pbgra32, null);
            InitializeProperties();
        }

        public void Clear()
        {
            for (int i = 0; i < PixelHeight * PixelWidth; i++)
            {
                *(int*)(BackBuffer + i * BytesPerPixel) = 0;
            }
        }

        public Bgra32Bitmap(BitmapSource source)
        {
            Source = new(source.Format != Bgra32 ? new FormatConvertedBitmap(source, Bgra32, null, 0) : source);
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            PixelWidth = Source.PixelWidth;
            PixelHeight = Source.PixelHeight;
            BackBuffer = (byte*)Source.BackBuffer;
            BackBufferStride = Source.BackBufferStride;
            BytesPerPixel = Source.Format.BitsPerPixel / 8;
        }

        private byte* GetPixelAddress(int x, int y)
        {
            return BackBuffer + y * BackBufferStride + x * BytesPerPixel;
        }

        public Vector4 GetRGBAPixel(int x, int y)
        {
            byte* pixel = GetPixelAddress(x, y);
            float b = pixel[0] / 255f;
            float g = pixel[1] / 255f;
            float r = pixel[2] / 255f;
            float a = pixel[3] / 255f;
            return new(r, g, b, a);
        }

        public Vector3 GetRGBPixel(int x, int y)
        {
            return GetRGBAPixel(x, y).AsVector3();
        }

        public void SetPixel(int x, int y, Vector4 color)
        {
            byte* pixel = GetPixelAddress(x, y);
            pixel[0] = (byte)(255 * color.Z);
            pixel[1] = (byte)(255 * color.Y);
            pixel[2] = (byte)(255 * color.X);
            pixel[3] = (byte)(255 * color.W);
        }

        public void SetPixel(int x, int y, Vector3 color)
        {
            SetPixel(x, y, new Vector4(color, 255));
        }

        public void ClearPixel(int x, int y)
        {
            *(int*)GetPixelAddress(x, y) = 0;
        }
    }
}
