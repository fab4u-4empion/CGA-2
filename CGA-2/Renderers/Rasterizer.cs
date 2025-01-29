using CGA2.Components;
using CGA2.Components.Objects;
using System.Numerics;
using Utils;
using static System.Int32;
using static System.Single;
using static CGA2.Settings;
using static System.Numerics.Matrix4x4;
using static System.Numerics.Vector4;
using System.Buffers;
using System.Collections.Concurrent;

namespace CGA2.Renderers
{
    public class Rasterizer : Renderer
    {
        public override Pbgra32Bitmap Result { get; set; } = new(1, 1);

        private Matrix4x4 ViewportMatrix { get; set; } = CreateViewportLeftHanded(-0.5f, -0.5f, 1, 1, 0, 1);

        public static float PerpDotProduct(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public override void Render(Scene scene)
        {
            Result.Source.Lock();

            foreach(MeshObject meshObject in scene.MeshObjects)
            {
                Matrix4x4 worldMatrix = meshObject.WorldMatrix;
                Matrix4x4 viewMatrix = scene.CameraObjects[SelectedCamera].ViewMatrix;
                Matrix4x4 projectionMatrix = scene.CameraObjects[SelectedCamera].Camera.ProjectionMatrix;

                Matrix4x4 matrix =  worldMatrix * viewMatrix * projectionMatrix;
                
                Parallel.ForEach(Partitioner.Create(0, meshObject.Mesh.Triangles.Count, 3), (range) =>
                {
                    Vector4[] result = ArrayPool<Vector4>.Shared.Rent(4);

                    int index1 = meshObject.Mesh.Triangles[range.Item1];
                    int index2 = meshObject.Mesh.Triangles[range.Item1 + 1];
                    int index3 = meshObject.Mesh.Triangles[range.Item1 + 2];

                    byte count = 0;

                    Vector4 v1 = Transform(meshObject.Mesh.Positions[index1], matrix);
                    Vector4 v2 = Transform(meshObject.Mesh.Positions[index2], matrix);
                    Vector4 v3 = Transform(meshObject.Mesh.Positions[index3], matrix);

                    if (v1.Z >= 0)
                        result[count++] = v1;

                    if (v1.Z < 0 != v2.Z < 0)
                    {
                        float t = -v1.Z / (v2.Z - v1.Z);
                        result[count++] = Lerp(v1, v2, t);
                    }

                    if (v2.Z >= 0)
                        result[count++] = v2;

                    if (v2.Z < 0 != v3.Z < 0)
                    {
                        float t = -v2.Z / (v3.Z - v2.Z);
                        result[count++] = Lerp(v2, v3, t);
                    }

                    if (v3.Z >= 0)
                        result[count++] = v3;

                    if (v1.Z < 0 != v3.Z < 0)
                    {
                        float t = -v1.Z / (v3.Z - v1.Z);
                        result[count++] = Lerp(v1, v3, t);
                    }

                    for (int j = 0; j < count; j++)
                        result[j] = Transform(result[j] / result[j].W, ViewportMatrix);

                    for (int j = 1; j < count - 1; j++)
                    {
                        Vector4 a = result[0];
                        Vector4 b = result[j];
                        Vector4 c = result[j + 1];

                        if (PerpDotProduct(new(c.X - a.X, c.Y - a.Y), new(b.X - a.X, b.Y - a.Y)) > 0)
                        {

                            if (b.X < a.X)
                                (a, b) = (b, a);

                            if (c.X < a.X)
                                (a, c) = (c, a);

                            if (c.X < b.X)
                                (b, c) = (c, b);

                            Vector4 k1 = (c - a) / (c.X - a.X);
                            Vector4 k2 = (b - a) / (b.X - a.X);
                            Vector4 k3 = (c - b) / (c.X - b.X);

                            int left = Max((int)Ceiling(a.X), 0);
                            int right = Min((int)Ceiling(c.X), Result.PixelWidth);

                            for (int x = left; x < right; x++)
                            {
                                Vector4 p1 = a + (x - a.X) * k1;
                                Vector4 p2 = x < b.X ? a + (x - a.X) * k2 : b + (x - b.X) * k3;

                                Vector4 k = (p2 - p1) / (p2.Y - p1.Y);

                                int top = Max((int)Ceiling(Min(p1.Y, p2.Y)), 0);
                                int bottom = Min((int)Ceiling(Max(p1.Y, p2.Y)), Result.PixelHeight);

                                for (int y = top; y < bottom; y++)
                                {
                                    Vector4 p = p1 + (y - p1.Y) * k;
                                    Result.SetPixel(x, y, Vector3.UnitZ);
                                }
                            }
                        }
                    }

                    ArrayPool<Vector4>.Shared.Return(result);
                });
            }

            Result.Source.AddDirtyRect(new(0, 0, Result.PixelWidth, Result.PixelHeight));
            Result.Source.Unlock();
        }

        public override void ResizeBuffers(double width, double height)
        {
            Result = new(Max(1, (int)(width * Scaling)), Max(1, (int)(height * Scaling)));

            ViewportMatrix = CreateViewportLeftHanded(-0.5f, -0.5f, Result.PixelWidth, Result.PixelHeight, 0, 1);
        }
    }
}
