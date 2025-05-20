using CGA2.Components.Cameras;
using CGA2.Components.Lights;
using CGA2.Components.Objects;
using System.Numerics;
using static System.Numerics.Matrix4x4;
using static System.Numerics.Vector3;

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

        private static void TransformAttributes(MeshObject meshObject)
        {
            meshObject.WorldPositions.Clear();
            meshObject.WorldNormals.Clear();
            meshObject.WorldTangents.Clear();

            Matrix4x4 worldMatrix = meshObject.WorldMatrix;
            Invert(worldMatrix, out Matrix4x4 invWorldMatrix);
            invWorldMatrix = Transpose(invWorldMatrix);

            for (int i = 0; i < meshObject.Mesh.Positions.Count; i++)
            {
                meshObject.WorldPositions.Add(Transform(meshObject.Mesh.Positions[i], worldMatrix));
            }

            for (int i = 0; i < meshObject.Mesh.Normals.Count; i++)
            {
                meshObject.WorldNormals.Add(Normalize(Transform(meshObject.Mesh.Normals[i], invWorldMatrix)));
                meshObject.WorldTangents.Add(Normalize(Transform(meshObject.Mesh.Tangents[i], invWorldMatrix)));
            }
        }

        public void UpdateScene()
        {
            if (MeshObjects.Count > 0)
            {
                Parallel.For(0, MeshObjects.Count, i =>
                {
                    TransformAttributes(MeshObjects[i]);
                });
            }
        }

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