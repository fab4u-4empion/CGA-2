using System.Windows.Media.Media3D;

namespace CGA_2.Components
{
    public class Scene
    {
        public Camera Camera { get; set; } = new();
        public List<Light> Lights { get; set; } = [];
        public List<Model> Models { get; set; } = [];
    }
}