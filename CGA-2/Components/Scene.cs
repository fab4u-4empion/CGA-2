using CGA2.Components.Lights;
using CGA2.Components.Objects;

namespace CGA2.Components
{
    public class Scene
    {
        public List<Camera> Cameras { get; set; } = [];
        public List<Light> Lights { get; set; } = [];
        public List<Mesh> Meshes { get; set; } = [];

        public List<CameraObject> CameraObjects { get; set; } = [];
        public List<LightObject> LightObjects { get; set; } = [];
        public List<MeshObject> MeshObjects { get; set; } = [];
    }
}