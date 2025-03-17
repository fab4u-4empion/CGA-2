using CGA2.Components;
using CGA2.Components.Cameras;
using CGA2.Components.Lights;
using CGA2.Components.Materials;
using CGA2.Components.Materials.Textures;
using CGA2.Components.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace CGA2.Utils
{
    public class GLTFReader
    {
        private enum TextureTypes
        {
            RGB,
            NonColor,
            Normal
        }

        private static Texture LoadTexture(string directory, JToken gltfData, int index, TextureTypes type = TextureTypes.NonColor)
        {
            Texture texture = new NonColorTexture();

            JToken gltfImageURI = gltfData["images"][(int)gltfData["textures"][index]["source"]]["uri"];

            if (type == TextureTypes.RGB)
                texture = new RGBATexture();

            if (type == TextureTypes.Normal)
                texture = new NormalTexture();

            texture.Create(new(new BitmapImage(new Uri(Path.Combine(directory, (string)gltfImageURI), UriKind.Absolute))));

            return texture;
        }

        public static void OpenFile(string path, Scene scene)
        {
            string directory = Path.GetDirectoryName(path)!;

            string jsonData = File.ReadAllText(path);
            JToken gltfData = JsonConvert.DeserializeObject<dynamic>(jsonData)!;

            List<byte[]> buffers = [];

            List<Mesh> meshes = [];
            List<Light> lights = [];
            List<SceneObject> nodes = [];

            List<MeshObject> meshObjects = [];
            List<CameraObject> cameraObjects = [];
            List<LightObject> lightObjects = [];
            List<SceneObject> rootObjects = [];

            List<Material> materials = [];

            List<Texture> textures = [];

            if (gltfData["textures"] != null)
                textures.AddRange(new Texture[gltfData["textures"].Count()]);

            if (gltfData["buffers"] != null)
                foreach (JToken gltfBuffer in gltfData["buffers"])
                    buffers.Add(File.ReadAllBytes(Path.Combine(directory, (string)gltfBuffer["uri"])));

            if (gltfData["materials"] != null)
                foreach (JToken gltfMaterial in gltfData["materials"])
                {
                    Material material = new()
                    {
                        Name = (string)gltfMaterial["name"]
                    };

                    if (gltfMaterial["normalTexture"] != null)
                    {
                        int index = (int)gltfMaterial["normalTexture"]["index"];

                        if (textures[index] == null)
                            textures[index] = LoadTexture(directory, gltfData, index, TextureTypes.Normal);

                        material.NormalTexture = (NormalTexture)textures[index];
                    }

                    if (gltfMaterial["occlusionTexture"] != null)
                    {
                        int index = (int)gltfMaterial["occlusionTexture"]["index"];

                        if (textures[index] == null)
                            textures[index] = LoadTexture(directory, gltfData, index);

                        material.OcclusionTexture = (NonColorTexture)textures[index];
                    }

                    if (gltfMaterial["emissiveTexture"] != null)
                    {
                        int index = (int)gltfMaterial["emissiveTexture"]["index"];

                        if (textures[index] == null)
                            textures[index] = LoadTexture(directory, gltfData, index, TextureTypes.RGB);

                        material.EmissiveTexture = (RGBATexture)textures[index];
                    }

                    if (gltfMaterial["pbrMetallicRoughness"] != null)
                    {
                        if (gltfMaterial["pbrMetallicRoughness"]["baseColorTexture"] != null)
                        {
                            int index = (int)gltfMaterial["pbrMetallicRoughness"]["baseColorTexture"]["index"];

                            if (textures[index] == null)
                                textures[index] = LoadTexture(directory, gltfData, index, TextureTypes.RGB);

                            material.BaseColorTexture = (RGBATexture)textures[index];
                        }

                        if (gltfMaterial["pbrMetallicRoughness"]["metallicRoughnessTexture"] != null)
                        {
                            int index = (int)gltfMaterial["pbrMetallicRoughness"]["metallicRoughnessTexture"]["index"];

                            if (textures[index] == null)
                                textures[index] = LoadTexture(directory, gltfData, index);

                            material.RMTexture = (NonColorTexture)textures[index];
                        }

                        if (gltfMaterial["pbrMetallicRoughness"]["baseColorFactor"] != null)
                        {
                            material.BaseColor = new(
                                (float)gltfMaterial["pbrMetallicRoughness"]["baseColorFactor"][0],
                                (float)gltfMaterial["pbrMetallicRoughness"]["baseColorFactor"][1],
                                (float)gltfMaterial["pbrMetallicRoughness"]["baseColorFactor"][2]
                            );
                            material.Alpha = (float)gltfMaterial["pbrMetallicRoughness"]["baseColorFactor"][3];
                        }

                        if (gltfMaterial["pbrMetallicRoughness"]["metallicFactor"] != null)
                            material.MetallicFactor = (float)gltfMaterial["pbrMetallicRoughness"]["metallicFactor"];

                        if (gltfMaterial["pbrMetallicRoughness"]["rougnessfactor"] != null)
                            material.MetallicFactor = (float)gltfMaterial["pbrMetallicRoughness"]["roughnessFactor"];
                    }

                    if (gltfMaterial["emissiveFactor"] != null)
                        material.Emission = new(
                            (float)gltfMaterial["emissiveFactor"][0], 
                            (float)gltfMaterial["emissiveFactor"][1], 
                            (float)gltfMaterial["emissiveFactor"][2]
                        );

                    if (gltfMaterial["alphaMode"] != null)
                    {
                        if ((string)gltfMaterial["alphaMode"] == "MASK")
                            material.AlphaMode = AlphaMode.MASK;

                        if ((string)gltfMaterial["alphaMode"] == "BLEND")
                            material.AlphaMode = AlphaMode.BLEND;
                    }

                    if (gltfMaterial["alphaCutoff"] != null)
                        material.AlphaCutoff = (float)gltfMaterial["alphaCutoff"];

                    if (gltfMaterial["doubleSided"] != null)
                        material.DoubleSided = (bool)gltfMaterial["doubleSided"];

                    materials.Add(material);
                }

            if (gltfData["meshes"] != null)
                foreach (JToken gltfMesh in gltfData["meshes"])
                {
                    Mesh mesh = new() { Name = (string)gltfMesh["name"] };

                    foreach (JToken gltfPrimitive in gltfMesh["primitives"]!)
                    {
                        JToken gltfBufferView;
                        JToken gltfAccessor;
                        Span<byte> buffer;

                        int trianglesCount = 0;

                        gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["indices"]];
                        gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                        buffer = buffers[(int)gltfBufferView["buffer"]].AsSpan();
                        for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                        {
                            mesh.Triangles.Add(SpanReader.ReadUShort(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 2));
                            trianglesCount++;
                        }

                        gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["attributes"]["POSITION"]];
                        gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                        buffer = buffers[(int)gltfBufferView["buffer"]].AsSpan();
                        for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                            mesh.Positions.Add(SpanReader.ReadVector3(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 12));

                        gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["attributes"]["NORMAL"]];
                        gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                        buffer = buffers[(int)gltfBufferView["buffer"]].AsSpan();
                        for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                            mesh.Normals.Add(SpanReader.ReadVector3(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 12));

                        gltfAccessor = gltfData["accessors"][(int)gltfPrimitive["attributes"]["TEXCOORD_0"]];
                        gltfBufferView = gltfData["bufferViews"][(int)gltfAccessor["bufferView"]];
                        buffer = buffers[(int)gltfBufferView["buffer"]].AsSpan();
                        for (int i = 0; i < (int)gltfAccessor["count"]; i++)
                            mesh.UVs.Add(SpanReader.ReadVector2(buffer, (int)(gltfBufferView["byteOffset"] ?? 0) + i * 8));

                        Material material = gltfPrimitive["material"] != null ? materials[(int)gltfPrimitive["material"]] : new();

                        Material[] primitiveMaterials = new Material[trianglesCount / 3];
                        Array.Fill(primitiveMaterials, material);

                        mesh.Materials.AddRange(primitiveMaterials);
                    }

                    meshes.Add(Mesh.CalculateTangents(mesh));
                }


            if (gltfData["extensions"] != null)
            {
                if (gltfData["extensions"]["KHR_lights_punctual"] != null)
                {
                    foreach (JToken gltfLight in gltfData["extensions"]["KHR_lights_punctual"]["lights"])
                    {
                        Light light = null;

                        if ((string)gltfLight["type"] == "point")
                            light = new SphericalLight()
                            {
                                Power = (float)(gltfLight["intensity"] ?? 1f)
                            };                            

                        if ((string)gltfLight["type"] == "directional")
                            light = new DistantLight()
                            {
                                Irradiance = (float)(gltfLight["intensity"] ?? 1f)
                            };

                        if ((string)gltfLight["type"] == "spot")
                        {
                            light = new SpotLight()
                            {
                                Power = (float)(gltfLight["intensity"] ?? 1f)
                            };

                            float outerAngle = (float)(gltfLight["spot"]["outerConeAngle"] ?? (float.Pi / 4f));
                            float innerAngle = (float)(gltfLight["spot"]["innerConeAngle"] ?? 0f);

                            (light as SpotLight)!.Angle = outerAngle;
                            (light as SpotLight)!.Falloff = 1f - innerAngle / outerAngle;
                        }
                            

                        light.Name = (string)gltfLight["name"];

                        if (gltfLight["color"] != null)
                            light.Color = new(
                                (float)gltfLight["color"][0],
                                (float)gltfLight["color"][1],
                                (float)gltfLight["color"][2]
                            );

                        lights.Add(light);
                    }
                }
            }

            if (gltfData["nodes"] != null)
            {
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
                            Camera = new PerspectiveCamera()
                        };

                    if (gltfNode["extensions"] != null)
                        if (gltfNode["extensions"]["KHR_lights_punctual"] != null)
                            newNode = new LightObject()
                            {
                                Light = lights[(int)gltfNode["extensions"]["KHR_lights_punctual"]["light"]]
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

                    if (gltfNode["scale"] != null)
                        newNode.Scale = new(
                            (float)gltfNode["scale"][0],
                            (float)gltfNode["scale"][1],
                            (float)gltfNode["scale"][2]
                        );

                    if (gltfNode["translation"] != null)
                        newNode.Location = new(
                            (float)gltfNode["translation"][0],
                            (float)gltfNode["translation"][1],
                            (float)gltfNode["translation"][2]
                        );

                    if (gltfNode["matrix"] != null)
                    {
                        Matrix4x4 matrix = new(
                            (float)gltfNode["matrix"][0], (float)gltfNode["matrix"][4], (float)gltfNode["matrix"][8], (float)gltfNode["matrix"][12],
                            (float)gltfNode["matrix"][1], (float)gltfNode["matrix"][5], (float)gltfNode["matrix"][9], (float)gltfNode["matrix"][13],
                            (float)gltfNode["matrix"][2], (float)gltfNode["matrix"][6], (float)gltfNode["matrix"][10], (float)gltfNode["matrix"][14],
                            (float)gltfNode["matrix"][3], (float)gltfNode["matrix"][7], (float)gltfNode["matrix"][11], (float)gltfNode["matrix"][15]
                        );

                        if (Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
                        {
                            newNode.Location = translation;
                            newNode.Scale = scale;
                            newNode.Rotation = rotation;
                        }
                    }

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

                    if (nodes[i] is LightObject lightObject)
                        lightObjects.Add(lightObject);
                }
            }
                
            if (gltfData["scenes"] != null)
                foreach (JToken gltfScene in gltfData["scenes"])
                    foreach (JToken gltfIndex in gltfScene["nodes"])
                        rootObjects.Add(nodes[(int)gltfIndex]);
            else
                rootObjects.AddRange(nodes);

            scene.MeshObjects.AddRange(meshObjects);
            scene.LightObjects.AddRange(lightObjects);
            scene.CameraObjects.AddRange(cameraObjects);
            scene.RootObjects.AddRange(rootObjects);

            scene.Meshes.AddRange(meshes);
            scene.Lights.AddRange(lights);
            scene.Nodes.AddRange(nodes);
        }
    }
}
