using CGA2.Components.Cameras;
using CGA2.Components.Lights;
using CGA2.Components.Objects;

namespace CGA2.Components
{
    public class Scene : Component
    {
        public override string Name { get; set; } = "Scene";

        public Environment Environment { get; set; } = new();

        public List<Camera> Cameras { get; set; } = [];
        public List<Light> Lights { get; set; } = [];
        public List<Mesh> Meshes { get; set; } = [];
        public List<SceneObject> Nodes { get; set; } = [];

        public List<CameraObject> CameraObjects { get; set; } = [];
        public List<LightObject> LightObjects { get; set; } = [];
        public List<MeshObject> MeshObjects { get; set; } = [];
        public List<SceneObject> RootObjects { get; set; } = [];

        public bool DeleteSceneObject(SceneObject sceneObject)
        {
            if (sceneObject is CameraObject)
                return false;

            if (sceneObject.Parent == null)
            {
                RootObjects.Remove(sceneObject);
                RootObjects.AddRange(sceneObject.Children);
            }
            else
            {
                sceneObject.Parent.Children.Remove(sceneObject);
                sceneObject.Parent.Children.AddRange(sceneObject.Children);
            }

            Nodes.Remove(sceneObject);

            if (sceneObject is MeshObject meshObject)
                MeshObjects.Remove(meshObject);

            if (sceneObject is LightObject lightObject)
                LightObjects.Remove(lightObject);

            foreach (SceneObject child in sceneObject.Children)
                child.Parent = sceneObject.Parent;

            return true;
        }
    }
}