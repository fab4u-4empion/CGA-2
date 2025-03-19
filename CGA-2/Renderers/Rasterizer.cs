using CGA2.Components;
using CGA2.Components.Objects;
using System.Numerics;
using static System.Int32;
using static System.Single;
using static CGA2.Settings;
using static System.Numerics.Vector3;
using static System.Numerics.Matrix4x4;
using System.Buffers;
using System.Collections.Concurrent;
using CGA2.Utils;
using CGA2.Components.Cameras;
using CGA2.Components.Materials;
using static CGA2.Utils.ArrayTools;
using System.Diagnostics;
using System.Windows;

namespace CGA2.Renderers
{
    using ViewBufferData = (MeshObject? MeshObject, int Index);
    using ScreenToWorldParams = (Vector3 Dir0, Vector3 DdDx, Vector3 DdDy);
    using Layer = (MeshObject? MeshObject, int Index, float Z);

    public class Rasterizer : Renderer
    {
        public override Bgra32Bitmap Result { get; set; } = new(1, 1);

        private Matrix4x4 ViewportMatrix { get; set; } = CreateViewportLeftHanded(-0.5f, -0.5f, 1, 1, 0, 1);

        private Buffer<SpinLock> Spins = new(0, 0);
        private Buffer<float> ZBuffer = new(0, 0);
        private Buffer<ViewBufferData> ViewBuffer = new(0, 0);
        private Buffer<Color> HDRBuffer = new(0, 0);

        private Buffer<int> OffsetBuffer = new(0, 0);
        private Buffer<byte> CountBuffer = new(0, 0);
        public Layer[] LayersBuffer = [];

        private static float PerpDotProduct(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        private void DrawIntoViewBuffer(int x, int y, float z, ViewBufferData data)
        {
            bool gotLock = false;

            Spins[x, y].Enter(ref gotLock);

            if (z < ZBuffer[x, y])
            {
                ViewBuffer[x, y] = data;
                ZBuffer[x, y] = z;
            }

            Spins[x, y].Exit(false);
        }

        private void IncDepth(int x, int y, float z, ViewBufferData data)
        {
            if (z < ZBuffer[x, y])
                Interlocked.Increment(ref OffsetBuffer[x, y]);
        }

        private void DrawPixelIntoLayers(int x, int y, float z, ViewBufferData data)
        {
            if (z < ZBuffer[x, y])
            {
                bool gotLock = false;
                Spins[x, y].Enter(ref gotLock);

                LayersBuffer[OffsetBuffer[x, y] + CountBuffer[x, y]] = (data.MeshObject, data.Index, z);
                CountBuffer[x, y] += 1;

                Spins[x, y].Exit(false);
            }
        }

        private static void TransformAttributes(CameraObject cameraObject, MeshObject meshObject)
        {
            Matrix4x4 worldMatrix = meshObject.WorldMatrix;
            Matrix4x4 viewMatrix = cameraObject.ViewMatrix;
            Matrix4x4 projectionMatrix = cameraObject.Camera.ProjectionMatrix;
            Matrix4x4 viewProjectionMatrix = viewMatrix * projectionMatrix;

            Invert(worldMatrix, out Matrix4x4 invWorldMatrix);
            invWorldMatrix = Transpose(invWorldMatrix);

            meshObject.WorldPositions.Clear();
            meshObject.WorldNormals.Clear();
            meshObject.WorldTangents.Clear();
            meshObject.ClipPositions.Clear();

            for (int i = 0; i < meshObject.Mesh.Positions.Count; i++)
            {
                meshObject.WorldPositions.Add(Transform(meshObject.Mesh.Positions[i], worldMatrix));
                meshObject.ClipPositions.Add(Vector4.Transform(meshObject.WorldPositions[i], viewProjectionMatrix));
            }

            for (int i = 0; i < meshObject.Mesh.Normals.Count; i++)
            {
                meshObject.WorldNormals.Add(Normalize(Transform(meshObject.Mesh.Normals[i], invWorldMatrix)));
                meshObject.WorldTangents.Add(Normalize(Transform(meshObject.Mesh.Tangents[i], invWorldMatrix)));
            }
        }

        private void Rasterize(List<MeshObject> meshObjects, List<Range> ranges, bool drawTransparentTriangles, Action<int, int, float, ViewBufferData> callBack)
        {
            Parallel.ForEach(Partitioner.Create(0, ranges[^1].End.Value), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    int m = BinarySearch(ranges, i);

                    MeshObject meshObject = meshObjects[m];

                    int triangleIndex = i - ranges[m].Start.Value;
                    int index = triangleIndex * 3;

                    if (drawTransparentTriangles != meshObject.Mesh.Materials[triangleIndex].IsTransparent)
                        continue;

                    int index1 = meshObject.Mesh.Triangles[index];
                    int index2 = meshObject.Mesh.Triangles[index + 1];
                    int index3 = meshObject.Mesh.Triangles[index + 2];

                    Vector4 v1 = meshObject.ClipPositions[index1];
                    Vector4 v2 = meshObject.ClipPositions[index2];
                    Vector4 v3 = meshObject.ClipPositions[index3];

                    Vector4[] result = ArrayPool<Vector4>.Shared.Rent(4);
                    byte count = 0;

                    if (v1.Z >= 0)
                        result[count++] = v1;

                    if (v1.Z < 0 != v2.Z < 0)
                    {
                        float t = -v1.Z / (v2.Z - v1.Z);
                        result[count++] = Vector4.Lerp(v1, v2, t);
                    }

                    if (v2.Z >= 0)
                        result[count++] = v2;

                    if (v2.Z < 0 != v3.Z < 0)
                    {
                        float t = -v2.Z / (v3.Z - v2.Z);
                        result[count++] = Vector4.Lerp(v2, v3, t);
                    }

                    if (v3.Z >= 0)
                        result[count++] = v3;

                    if (v1.Z < 0 != v3.Z < 0)
                    {
                        float t = -v1.Z / (v3.Z - v1.Z);
                        result[count++] = Vector4.Lerp(v1, v3, t);
                    }

                    for (int j = 0; j < count; j++)
                        result[j] = Vector4.Transform(result[j] / result[j].W, ViewportMatrix);

                    for (int j = 1; j < count - 1; j++)
                    {
                        Vector4 a = result[0];
                        Vector4 b = result[j];
                        Vector4 c = result[j + 1];

                        if (meshObject.Mesh.Materials[triangleIndex].DoubleSided || PerpDotProduct(new(c.X - a.X, c.Y - a.Y), new(b.X - a.X, b.Y - a.Y)) > 0)
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
                                    callBack(x, y, p.Z, (meshObject, triangleIndex));
                                }
                            }
                        }
                    }

                    ArrayPool<Vector4>.Shared.Return(result);
                }
            });
        }

        private static Color GetPixelColor(CameraObject cameraObject, List<LightObject> lightsObjects, Components.Environment environment, ScreenToWorldParams screenToWorld, ViewBufferData objectInfo, int x, int y)
        {
            int index = objectInfo.Index * 3;
            MeshObject meshObject = objectInfo.MeshObject!;

            int index1 = meshObject.Mesh.Triangles[index];
            int index2 = meshObject.Mesh.Triangles[index + 1];
            int index3 = meshObject.Mesh.Triangles[index + 2];

            Vector3 aw = meshObject.WorldPositions[index1];
            Vector3 bw = meshObject.WorldPositions[index2];
            Vector3 cw = meshObject.WorldPositions[index3];

            Vector3 D1 = screenToWorld.Dir0 + x * screenToWorld.DdDx + y * screenToWorld.DdDy;
            Vector3 D2 = D1 + screenToWorld.DdDx;
            Vector3 D3 = D1 + screenToWorld.DdDy;

            Vector3 tvec = cameraObject.WorldLocation - cw;

            Vector3 e1 = aw - cw;
            Vector3 e2 = bw - cw;

            Vector3 cross1 = Cross(e2, e1);
            Vector3 cross2 = Cross(e2, tvec);
            Vector3 cross3 = Cross(tvec, e1);

            float det1 = 1 / Dot(D1, cross1);
            float det2 = 1 / Dot(D2, cross1);
            float det3 = 1 / Dot(D3, cross1);

            float u = Dot(D1, cross2) * det1;
            float u1 = Dot(D2, cross2) * det2;
            float u2 = Dot(D3, cross2) * det3;

            float v = Dot(cross3, D1) * det1;
            float v1 = Dot(cross3, D2) * det2;
            float v2 = Dot(cross3, D3) * det3;

            float w = 1f - u - v;
            float w1 = 1f - u1 - v1;
            float w2 = 1f - u2 - v2;

            Vector3 n1 = meshObject.WorldNormals[index1];
            Vector3 n2 = meshObject.WorldNormals[index2];
            Vector3 n3 = meshObject.WorldNormals[index3];

            Vector2 uv1 = meshObject.Mesh.UVs[index1];
            Vector2 uv2 = meshObject.Mesh.UVs[index2];
            Vector2 uv3 = meshObject.Mesh.UVs[index3];

            Vector3 t1 = meshObject.WorldTangents[index1];
            Vector3 t2 = meshObject.WorldTangents[index2];
            Vector3 t3 = meshObject.WorldTangents[index3];

            Vector3 n = u * n1 + v * n2 + w * n3;

            Vector2 uv = u * uv1 + v * uv2 + w * uv3;
            Vector2 uv_x = u1 * uv1 + v1 * uv2 + w1 * uv3;
            Vector2 uv_y = u2 * uv1 + v2 * uv2 + w2 * uv3;

            Vector3 pw = u * aw + v * bw + w * cw;

            Color baseColor = meshObject.Mesh.Materials[objectInfo.Index].GetBaseColor(uv, uv_x, uv_y);
            PBRParams pbrParams = meshObject.Mesh.Materials[objectInfo.Index].GetPBRParams(uv, uv_x, uv_y);
            Vector3 emission = meshObject.Mesh.Materials[objectInfo.Index].GetEmission(uv, uv_x, uv_y);
            Vector3 normal = meshObject.Mesh.Materials[objectInfo.Index].GetNormal(uv, uv_x, uv_y);

            Vector3 t = (u * t1 + v * t2 + w * t3);
            Vector3 b = Cross(n, t) * meshObject.Mesh.Signs[objectInfo.Index];
            normal = t * normal.X + b * normal.Y + n * normal.Z;

            if (det1 < 0)
                normal = -normal;

            return Shader.GetColor(lightsObjects, environment, baseColor, emission, pbrParams, normal, cameraObject.WorldLocation, pw);
        }

        private void DrawViewBuffer(CameraObject cameraObject, List<LightObject> lightsObjects, Components.Environment environment, ScreenToWorldParams screenToWorld)
        {
            Parallel.For(0, HDRBuffer.Height, (y) =>
            {
                for (int x = 0; x < HDRBuffer.Width; x++)
                {
                    if (ViewBuffer[x, y].MeshObject != null)
                        HDRBuffer[x, y] = GetPixelColor(cameraObject, lightsObjects, environment, screenToWorld, ViewBuffer[x, y], x, y);
                }
            });
        }

        private Color GetResultColor(
            CameraObject cameraObject, 
            List<LightObject> lightsObjects, 
            Components.Environment environment, 
            ScreenToWorldParams screenToWorld, 
            int start, 
            int length, 
            int x, 
            int y
        )
        {
            float key;
            Layer layer;
            int j;
            for (int i = 1 + start; i < length + start; i++)
            {
                key = LayersBuffer[i].Z;
                layer = LayersBuffer[i];
                j = i - 1;

                while (j >= start && LayersBuffer[j].Z > key)
                {
                    LayersBuffer[j + 1] = LayersBuffer[j];
                    j--;
                }
                LayersBuffer[j + 1] = layer;
            }

            Vector3 color = Zero;
            float alpha = 0f;

            for (int i = start; i < length + start; i++)
            {
                Color pixel = GetPixelColor(cameraObject, lightsObjects, environment, screenToWorld, (LayersBuffer[i].MeshObject, LayersBuffer[i].Index), x, y);

                if (pixel.Alpha == 0) continue;

                color += (1f - alpha) * pixel.BaseColor;
                alpha += (1f - alpha) * pixel.Alpha;

                if (pixel.Alpha == 1f)
                    return new(color);
            }

            return new(color, alpha);
        }

        private void DrawLayers(CameraObject cameraObject, List<LightObject> lightsObjects, Components.Environment environment, ScreenToWorldParams screenToWorld)
        {
            Parallel.For(0, Result.PixelHeight, (y) =>
            {
                for (int x = 0; x < Result.PixelWidth; x++)
                {
                    Color color = new(Zero, 0f);

                    if (CountBuffer[x, y] > 0)
                    {
                        color = GetResultColor(cameraObject, lightsObjects, environment, screenToWorld, OffsetBuffer[x, y], CountBuffer[x, y], x, y);
                    }

                    if (color.Alpha == 1f)
                    {
                        HDRBuffer[x, y] = color;
                        continue;
                    }

                    if (ViewBuffer[x, y].MeshObject != null)
                    {
                        Color pixel = GetPixelColor(cameraObject, lightsObjects, environment, screenToWorld, ViewBuffer[x, y], x, y);
                        HDRBuffer[x, y] = new(pixel.BaseColor * (1f - color.Alpha) + color.BaseColor);
                        continue;
                    }

                    HDRBuffer[x, y] = color;
                }
            });
        }

        private void DrawHDRBuffer(Components.Environment environment, CameraObject cameraObject)
        {
            ScreenToWorldParams screenToWorld = GetViewportToWorldParams(cameraObject, float.Pi / 4f);

            Parallel.For(0, HDRBuffer.Height, (y) =>
            {
                for (int x = 0; x < HDRBuffer.Width; x++)
                {
                    Vector3 backColor = environment.Color;
                    if (UseSkybox && environment.IBLSpecularMap.Count > 0)
                    {
                        Vector3 p = screenToWorld.Dir0 + x * screenToWorld.DdDx + y * screenToWorld.DdDy;
                        backColor = environment.IBLSpecularMap[0].GetColor(Normalize(p));
                    }

                    Result.SetPixel(x, y, ToneMapper.CompressColor(HDRBuffer[x, y].BaseColor + (1f - HDRBuffer[x, y].Alpha) * backColor));
                }
            });
        }

        private ScreenToWorldParams GetViewportToWorldParams(CameraObject cameraObject, float? tanParam = null)
        {
            float aspect = cameraObject.Camera.AspectRatio;
            float tan = Tan(tanParam ?? ((cameraObject.Camera as PerspectiveCamera)!.FieldOfView / 2f));

            Matrix4x4 cameraRotation = CreateFromQuaternion(cameraObject.WorldRotation);

            Vector3 X = Create(cameraRotation.M11, cameraRotation.M12, cameraRotation.M13) * tan * aspect;
            Vector3 Y = Create(cameraRotation.M21, cameraRotation.M22, cameraRotation.M23) * tan;
            Vector3 Z = Create(cameraRotation.M31, cameraRotation.M32, cameraRotation.M33);

            Vector3 p0 = (1f / Result.PixelWidth - 1f) * X + (-1f / Result.PixelHeight + 1f) * Y - Z;
            Vector3 dpdx = X * 2f / Result.PixelWidth;
            Vector3 dpdy = Y * -2f / Result.PixelHeight;

            return (p0, dpdx, dpdy);
        }

        public override void Render(Scene scene)
        {
            if (scene.CameraObjects.Count == 0) return;

            CameraObject cameraObject = scene.CameraObjects[SelectedCamera];

            cameraObject.Camera.AspectRatio = (float)Result.PixelWidth / Result.PixelHeight;

            ScreenToWorldParams screenToWorld = GetViewportToWorldParams(cameraObject);

            Array.Fill(HDRBuffer.Array, new(Zero, 0f));
            Array.Fill(ZBuffer.Array, 1);
            Array.Fill(ViewBuffer.Array, (null, -1));
            Array.Fill(OffsetBuffer.Array, 0);
            Array.Fill(CountBuffer.Array, (byte)0);

            Result.Source.Lock();

            if (scene.MeshObjects.Count > 0)
            {
                Parallel.For(0, scene.MeshObjects.Count, i =>
                {
                    TransformAttributes(cameraObject, scene.MeshObjects[i]);
                });

                List<Range> ranges = [];
                int totalLength = 0;

                foreach (MeshObject meshObject in scene.MeshObjects)
                {
                    ranges.Add(totalLength..(totalLength + meshObject.Mesh.Triangles.Count / 3 - 1));
                    totalLength += meshObject.Mesh.Triangles.Count / 3;
                }

                Rasterize(scene.MeshObjects, ranges, false, DrawIntoViewBuffer);

                Rasterize(scene.MeshObjects, ranges, true, IncDepth);

                int prefixSum = 0;
                int depth = 0;

                for (int y = 0; y < Result.PixelHeight; y++)
                {
                    for (int x = 0; x < Result.PixelWidth; x++)
                    {
                        depth = OffsetBuffer[x, y];
                        OffsetBuffer[x, y] = prefixSum;
                        prefixSum += depth;
                    }
                }

                if (prefixSum > 0)
                {
                    LayersBuffer = new Layer[prefixSum];

                    Rasterize(scene.MeshObjects, ranges, true, DrawPixelIntoLayers);

                    DrawLayers(cameraObject, scene.LightObjects, scene.Environment, screenToWorld);
                }
                else
                {
                    DrawViewBuffer(cameraObject, scene.LightObjects, scene.Environment, screenToWorld);
                }
            }
            
            DrawHDRBuffer(scene.Environment, cameraObject);

            Result.Source.AddDirtyRect(new(0, 0, Result.PixelWidth, Result.PixelHeight));
            Result.Source.Unlock();
        }

        public override void ResizeBuffers(double width, double height)
        {
            Result = new(Max(1, (int)(width * Scaling)), Max(1, (int)(height * Scaling)));

            HDRBuffer = new(Result.PixelWidth, Result.PixelHeight);
            ZBuffer = new(Result.PixelWidth, Result.PixelHeight);
            ViewBuffer = new(Result.PixelWidth, Result.PixelHeight);
            Spins = new(Result.PixelWidth, Result.PixelHeight);
            OffsetBuffer = new(Result.PixelWidth, Result.PixelHeight);
            CountBuffer = new(Result.PixelWidth, Result.PixelHeight);

            Array.Fill(Spins.Array, new(false));

            ViewportMatrix = CreateViewportLeftHanded(-0.5f, -0.5f, Result.PixelWidth, Result.PixelHeight, 0, 1);
        }
    }
}
