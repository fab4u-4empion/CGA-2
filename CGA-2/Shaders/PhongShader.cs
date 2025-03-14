using CGA2.Components.Materials;
using CGA2.Components.Objects;
using System.Numerics;
using static System.Numerics.Vector3;
using static CGA2.Settings;
using static System.Single;

namespace CGA2.Shaders
{
    public class PhongShader : Shader
    {
        public override Color GetColor(
            List<LightObject> lights, 
            Components.Environment environment, 
            Color baseColor, Vector3 emission, 
            PBRParams pbrParams, 
            Vector3 n, 
            Vector3 cameraPosition, 
            Vector3 p
        )
        {
            Vector3 V = Normalize(cameraPosition - p);
            Vector3 N = Normalize(n);

            float glossiness = 1f - pbrParams.Roughness;
            float a2 = glossiness * glossiness;
            float a4 = a2 * a2;

            Vector3 color = baseColor.BaseColor * environment.Color * pbrParams.Occlusion + emission * EmissionIntensity;

            Vector3 F0 = Lerp(new(0.04f), baseColor.BaseColor, pbrParams.Metallic);

            foreach (LightObject light in lights)
            {
                Vector3 L = light.GetL(p);
                Vector3 H = Normalize(V + L);

                if (Dot(N, L) <= 0)
                    continue;

                Vector3 specular = F0 * a4 * Pow(Max(Dot(H, N), 0), a4 * 1024f) * 25f;

                color += (baseColor.BaseColor / float.Pi + specular) * light.GetIrradiance(p) * Max(Dot(N, L), 0f);
            }

            return new(color, baseColor.Alpha);
        }
    }
}
