using CGA2.Components.Lights;

namespace CGA2.Components
{
    public class Scene
    {
        public List<Camera> Cameras { get; set; } = [];
        public List<Light> Lights { get; set; } = [];
        public List<Mesh> Meshes { get; set; } = [];
    }
}