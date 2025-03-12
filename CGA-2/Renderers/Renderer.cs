using CGA2.Components;
using CGA2.Utils;

namespace CGA2.Renderers
{
    public abstract class Renderer
    {
        public abstract Bgra32Bitmap Result { get; set; }

        public abstract void ResizeBuffers(double width, double height);

        public abstract void Render(Scene scene);
    }
}
