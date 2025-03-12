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

namespace CGA2.Renderers
{
    using ViewBufferData = (MeshObject? MeshObject, int Index);
    using ScreenToWorldParams = (Vector3 Dir0, Vector3 DdDx, Vector3 DdDy);

    public class Rasterizer : Renderer
    {
        public override Bgra32Bitmap Result { get; set; } = new(1, 1);

        private Matrix4x4 ViewportMatrix { get; set; } = CreateViewportLeftHanded(-0.5f, -0.5f, 1, 1, 0, 1);

        private Buffer<SpinLock> Spins = new(0, 0);
        private Buffer<float> ZBuffer = new(0, 0);
        private Buffer<ViewBufferData> ViewBuffer = new(0, 0);
        private Buffer<Vector3> HDRBuffer = new(0, 0);

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

        private static void TransformAttributes(CameraObject cameraObject, MeshObject meshObject)
        {
            Matrix4x4 worldMatrix = meshObject.WorldMatrix;
            Matrix4x4 viewMatrix = cameraObject.ViewMatrix;
            Matrix4x4 projectionMatrix = cameraObject.Camera.ProjectionMatrix;

            meshObject.WorldPositions = new Vector3[meshObject.Mesh.Positions.Count];
            meshObject.WorldNormals = new Vector3[meshObject.Mesh.Normals.Count];
            meshObject.ClipPositions = new Vector4[meshObject.Mesh.Positions.Count];

            Parallel.ForEach(Partitioner.Create(0, meshObject.Mesh.Positions.Count), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    meshObject.WorldPositions[i] = Transform(meshObject.Mesh.Positions[i], worldMatrix);
                    meshObject.ClipPositions[i] = Vector4.Transform(meshObject.WorldPositions[i], viewMatrix * projectionMatrix);
                }
            });

            Parallel.ForEach(Partitioner.Create(0, meshObject.Mesh.Normals.Count), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Invert(worldMatrix, out Matrix4x4 invWorldMatrix);
                    meshObject.WorldNormals[i] = Normalize(Transform(meshObject.Mesh.Normals[i], Transpose(invWorldMatrix)));
                }
            });
        }

        private void Rasterize(MeshObject meshObject, Action<int, int, float, ViewBufferData> callBack)
        {
            Parallel.ForEach(Partitioner.Create(0, meshObject.Mesh.Triangles.Count / 3), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    int index = i * 3;

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
                                    callBack(x, y, p.Z, (meshObject, i));
                                }
                            }
                        }
                    }

                    ArrayPool<Vector4>.Shared.Return(result);
                }
            });
        }

        private static Vector3 GetPixelColor(CameraObject cameraObject, List<LightObject> lightsObjects, ScreenToWorldParams screenToWorld, ViewBufferData objectInfo, int x, int y)
        {
            int index = objectInfo.Index * 3;

            int index1 = objectInfo.MeshObject!.Mesh.Triangles[index];
            int index2 = objectInfo.MeshObject!.Mesh.Triangles[index + 1];
            int index3 = objectInfo.MeshObject!.Mesh.Triangles[index + 2];

            Vector3 aw = objectInfo.MeshObject!.WorldPositions[index1];
            Vector3 bw = objectInfo.MeshObject!.WorldPositions[index2];
            Vector3 cw = objectInfo.MeshObject!.WorldPositions[index3];

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

            Vector3 n1 = objectInfo.MeshObject!.WorldNormals[index1];
            Vector3 n2 = objectInfo.MeshObject!.WorldNormals[index2];
            Vector3 n3 = objectInfo.MeshObject!.WorldNormals[index3];

            Vector3 n = u * n1 + v * n2 + w * n3;

            Vector3 pw = u * aw + v * bw + w * cw;

            Vector3 color = Zero;

            foreach (LightObject lightObject in lightsObjects)
                color += Max(Dot(n, lightObject.GetL(pw)), 0) * lightObject.GetIrradiance(pw);

            return color;
        }

        private void DrawViewBuffer(CameraObject cameraObject, List<LightObject> lightsObjects, ScreenToWorldParams screenToWorld)
        {
            Parallel.For(0, HDRBuffer.Height, (y) =>
            {
                for (int x = 0; x < HDRBuffer.Width; x++)
                {
                    if (ViewBuffer[x, y].MeshObject != null)
                    HDRBuffer[x, y] = GetPixelColor(cameraObject, lightsObjects, screenToWorld, ViewBuffer[x, y], x, y);
                }
            });
        }

        private void DrawHDRBuffer()
        {
            Parallel.For(0, HDRBuffer.Height, (y) =>
            {
            for (int x = 0; x < HDRBuffer.Width; x++)
                {
                    Result.SetPixel(x, y, ToneMapper.CompressColor(HDRBuffer[x, y]));
                }
            });
        }

        private ScreenToWorldParams GetViewportToWorldParams(CameraObject cameraObject)
        {
            float aspect = cameraObject.Camera.AspectRatio;
            float tan = Tan((cameraObject.Camera as PerspectiveCamera)!.FieldOfView / 2f);

            Matrix4x4 cameraRotation = CreateFromQuaternion(cameraObject.WorldRotation);

            Vector3 X = Create(cameraRotation.M11, cameraRotation.M12, cameraRotation.M13) * tan * aspect;
            Vector3 Y = Create(cameraRotation.M21, cameraRotation.M22, cameraRotation.M23) * tan;
            Vector3 Z = Create(cameraRotation.M31, cameraRotation.M32, cameraRotation.M33);

            Vector3 p0 = (1f / Result.PixelWidth - 1) * X + (-1f / Result.PixelHeight + 1) * Y - Z;
            Vector3 dpdx = X * 2 / Result.PixelWidth;
            Vector3 dpdy = Y * -2 / Result.PixelHeight;

            return (p0, dpdx, dpdy);
        }

        public override void Render(Scene scene)
        {
            if (scene.CameraObjects.Count == 0) return;

            CameraObject cameraObject = scene.CameraObjects[SelectedCamera];

            cameraObject.Camera.AspectRatio = (float)Result.PixelWidth / Result.PixelHeight;

            ScreenToWorldParams screenToWorld = GetViewportToWorldParams(cameraObject);

            Array.Fill(HDRBuffer.Array, scene.Environment.Color);
            Array.Fill(ZBuffer.Array, 1);
            Array.Fill(ViewBuffer.Array, (null, -1));

            Result.Source.Lock();

            foreach (MeshObject meshObject in scene.MeshObjects)
            {
                TransformAttributes(cameraObject, meshObject);
                Rasterize(meshObject, DrawIntoViewBuffer);
            }

            DrawViewBuffer(cameraObject, scene.LightObjects, screenToWorld);
            DrawHDRBuffer();

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

            Array.Fill(Spins.Array, new(false));

            ViewportMatrix = Matrix4x4.CreateViewportLeftHanded(-0.5f, -0.5f, Result.PixelWidth, Result.PixelHeight, 0, 1);
        }
    }
}
