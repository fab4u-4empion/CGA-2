using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CGA2.Utils
{
    public class GLTFReader
    {
        public static void OpenFile(string path, Scene scene)
        {
            string directory = Path.GetDirectoryName(path)!;

            string jsonData = File.ReadAllText(path);
            JToken gltfData = JsonConvert.DeserializeObject<dynamic>(jsonData)!;

            Span<byte> buffer = File.ReadAllBytes(Path.Combine(directory, (string)gltfData["buffers"][0]["uri"])).AsSpan();

            List<Mesh> meshes = [];
            List<Camera> cameras = [];
            List<SceneObject> nodes = [];

            List<MeshObject> meshObjects = [];
            List<CameraObject> cameraObjects = [];
            List<SceneObject> rootObjects = [];

            foreach (JToken item in gltfData["meshes"])
            {
                Mesh mesh = new() { Name = (string)item["name"] };

                foreach (JToken primitive in item["primitives"]!)
                {
                    JToken bufferView;
                    JToken accessor;

                    accessor = gltfData["accessors"][(int)primitive["indices"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.Triangles.Add(SpanReader.ReadShort(buffer, (int)(bufferView["byteOffset"] ?? 0) + i * 2));

                    accessor = gltfData["accessors"][(int)primitive["attributes"]["POSITION"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.Positions.Add(SpanReader.ReadVector3(buffer, (int)(bufferView["byteOffset"] ?? 0) + i * 12));

                    accessor = gltfData["accessors"][(int)primitive["attributes"]["NORMAL"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.Normals.Add(SpanReader.ReadVector3(buffer, (int)(bufferView["byteOffset"] ?? 0) + i * 12));

                    accessor = gltfData["accessors"][(int)primitive["attributes"]["TEXCOORD_0"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.UVs.Add(SpanReader.ReadVector2(buffer, (int)(bufferView["byteOffset"] ?? 0) + i * 8));
                }

                meshes.Add(mesh);
            }

            foreach (JToken node in gltfData["nodes"])
            {
                SceneObject newNode = null;

                if (node["mesh"] != null)
                    newNode = new MeshObject()
                    {
                        Mesh = meshes[(int)node["mesh"]]
                    };

                if (node["camera"] != null)
                    newNode = new CameraObject()
                    {
                        Camera = cameras[(int)node["camera"]]
                    };

                if (newNode == null)
                    newNode = new();

                newNode.Name = (string)node["name"];
                nodes.Add(newNode);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                JToken gltfNode = gltfData["nodes"][i];
                if (gltfNode["children"] != null)
                    foreach (JToken index in gltfNode["children"]) 
                    {
                        nodes[i].Children.Add(nodes[(int)index]);
                        nodes[(int)index].Parent = nodes[i];
                    }
                if (nodes[i] is MeshObject)
                    meshObjects.Add((MeshObject)nodes[i]);

                if (nodes[i] is CameraObject)
                    cameraObjects.Add((CameraObject)nodes[i]);
            }

            foreach (JToken gltfScene in gltfData["scenes"])
                foreach (JToken index in gltfScene["nodes"])
                    rootObjects.Add(nodes[(int)index]);

            scene.MeshObjects.AddRange(meshObjects);
            scene.CameraObjects.AddRange(cameraObjects);
            scene.RootObjects.AddRange(rootObjects);

            scene.Meshes.AddRange(meshes);
            scene.Cameras.AddRange(cameras);
            scene.Nodes.AddRange(nodes);
        }
    }
}
