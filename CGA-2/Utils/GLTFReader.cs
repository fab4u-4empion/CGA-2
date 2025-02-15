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

            foreach (JToken gltfMesh in gltfData["meshes"])
            {
                Mesh mesh = new() { Name = (string)gltfMesh["name"] };

                foreach (JToken gltfPrimitive in gltfMesh["primitives"]!)
                {
                    JToken gltfBufferView;
                    JToken gltfAccessor;

                    gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["indices"]];
                    gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                    for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                        mesh.Triangles.Add(SpanReader.ReadUShort(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 2));

                    gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["attributes"]["POSITION"]];
                    gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                    for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                        mesh.Positions.Add(SpanReader.ReadVector3(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 12));

                    gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["attributes"]["NORMAL"]];
                    gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                    for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                        mesh.Normals.Add(SpanReader.ReadVector3(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 12));

                    gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["attributes"]["TEXCOORD_0"]];
                    gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                    for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                        mesh.UVs.Add(SpanReader.ReadVector2(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 8));
                }

                meshes.Add(mesh);
            }

            foreach (JToken gltfNode in gltfData["nodes"])
            {
                SceneObject newNode = null;

                if (gltfNode["mesh"] != null)
                    newNode = new MeshObject()
                    {
                        Mesh = meshes[(int)gltfNode["mesh"]]
                    };

                if (gltfNode["camera"] != null)
                    newNode = new CameraObject()
                    {
                        Camera = cameras[(int)gltfNode["camera"]]
                    };

                newNode ??= new();

                newNode.Name = (string)gltfNode["name"];
                if (gltfNode["rotation"] != null)
                    newNode.Rotation = new(
                        (float)gltfNode["rotation"][0], 
                        (float)gltfNode["rotation"][1], 
                        (float)gltfNode["rotation"][2], 
                        (float)gltfNode["rotation"][3]
                    );
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
                if (nodes[i] is MeshObject meshObject)
                    meshObjects.Add(meshObject);

                if (nodes[i] is CameraObject cameraObject)
                    cameraObjects.Add(cameraObject);
            }

            foreach (JToken gltfScene in gltfData["scenes"])
                foreach (JToken gltfIndex in gltfScene["nodes"])
                    rootObjects.Add(nodes[(int)gltfIndex]);

            scene.MeshObjects.AddRange(meshObjects);
            scene.CameraObjects.AddRange(cameraObjects);
            scene.RootObjects.AddRange(rootObjects);

            scene.Meshes.AddRange(meshes);
            scene.Cameras.AddRange(cameras);
            scene.Nodes.AddRange(nodes);
        }
    }
}
