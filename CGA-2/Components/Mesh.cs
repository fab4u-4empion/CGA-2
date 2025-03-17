using CGA2.Components.Materials;
using System.Numerics;
using static System.Numerics.Vector3;
using static System.Single;

namespace CGA2.Components
{
    public class Mesh : Component
    {
        public override string Name { get; set; } = "Mesh";

        public List<Vector3> Positions { get; set; } = [];
        public List<Vector3> Normals { get; set; } = [];
        public List<Vector2> UVs { get; set; } = [];
        public List<ushort> Triangles { get; set; } = [];
        public List<Vector3> Tangents { get; set; } = [];
        public List<sbyte> Signs { get; set; } = [];
        public List<Material> Materials { get; set; } = [];

        public static Mesh CalculateTangents(Mesh mesh)
        {
            Mesh newMesh = new() { Materials = mesh.Materials};

            Dictionary<(ushort, sbyte), int> indicesDictionary = [];

            for (int i = 0; i < mesh.Triangles.Count; i += 3)
            {
                Vector3 p0 = mesh.Positions[mesh.Triangles[i]];
                Vector3 p1 = mesh.Positions[mesh.Triangles[i + 1]];
                Vector3 p2 = mesh.Positions[mesh.Triangles[i + 2]];

                Vector2 uv0 = mesh.UVs[mesh.Triangles[i]];
                Vector2 uv1 = mesh.UVs[mesh.Triangles[i + 1]];
                Vector2 uv2 = mesh.UVs[mesh.Triangles[i + 2]];

                Vector3 e1 = p1 - p0;
                Vector3 e2 = p2 - p0;

                float x1 = uv1.X - uv0.X, x2 = uv2.X - uv0.X;
                float y1 = uv1.Y - uv0.Y, y2 = uv2.Y - uv0.Y;

                float det = (x1 * y2 - x2 * y1);
                Vector3 t = (e1 * y2 - e2 * y1) / det;

                sbyte sign = (sbyte)Sign(det);
                newMesh.Signs.Add(sign);

                for (int j = i; j < i + 3; j++)
                {
                    (ushort, sbyte) key = (mesh.Triangles[j], sign);

                    if (!indicesDictionary.TryGetValue(key, out int index))
                    {
                        indicesDictionary.Add(key, newMesh.Positions.Count);
                        index = newMesh.Positions.Count;

                        newMesh.Positions.Add(mesh.Positions[mesh.Triangles[j]]);
                        newMesh.UVs.Add(mesh.UVs[mesh.Triangles[j]]);
                        newMesh.Normals.Add(mesh.Normals[mesh.Triangles[j]]);
                        newMesh.Tangents.Add(new());
                    }

                    newMesh.Triangles.Add((ushort)index);

                    Vector3 n = newMesh.Normals[index];
                    newMesh.Tangents[index] += t - Dot(t, n) * n;
                }
            }

            for (int i = 0; i < newMesh.Tangents.Count; i++)
                if (newMesh.Tangents[i].Length() > 0)
                    newMesh.Tangents[i] = Normalize(newMesh.Tangents[i]);

            return newMesh;
        }
    }
}