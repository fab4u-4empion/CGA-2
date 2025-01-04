using CGA2.Components;
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
                        mesh.Triangles.Add(SpanReader.ReadShort(buffer, (int)bufferView["byteOffset"] + i * 2));

                    accessor = gltfData["accessors"][(int)primitive["attributes"]["POSITION"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.Positions.Add(SpanReader.ReadVector3(buffer, (int)bufferView["byteOffset"] + i * 12));

                    accessor = gltfData["accessors"][(int)primitive["attributes"]["NORMAL"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.Normals.Add(SpanReader.ReadVector3(buffer, (int)bufferView["byteOffset"] + i * 12));

                    accessor = gltfData["accessors"][(int)primitive["attributes"]["TEXCOORD_0"]];
                    bufferView = gltfData["bufferViews"][(int)accessor["bufferView"]];
                    for (int i = 0; i < (int)accessor["count"]; i++)
                        mesh.UVs.Add(SpanReader.ReadVector2(buffer, (int)bufferView["byteOffset"] + i * 8));
                }

                meshes.Add(mesh);
            }

            List<MeshObject> meshObjects = [];
            foreach (JToken node in gltfData["nodes"])
            {
                if (node["mesh"] != null)
                    meshObjects.Add(new()
                        {
                            Name = (string)node["name"],
                            Mesh = meshes[(int)node["mesh"]]
                        }
                    );
            }
                
            scene.MeshObjects.AddRange(meshObjects);
            scene.Meshes.AddRange(meshes);
        }
    }
}
