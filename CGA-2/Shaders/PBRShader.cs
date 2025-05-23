﻿using CGA2.Components.Materials;
using CGA2.Components.Objects;
using System.Numerics;
using static System.Numerics.Vector3;
using static System.Single;
using static CGA2.Settings;

namespace CGA2.Shaders
{
    public class PBRShader : Shader
    {
        public override string Name { get; set; } = "PBR";

        private static Vector3 FresnelSchlick(float VdotH, Vector3 F0)
        {
            float t = 1 - VdotH;
            float t2 = t * t;
            float t5 = t2 * t2 * t;
            return F0 + (One - F0) * t5;
        }

        private static float Distribution(float NdotH, float roughness)
        {
            float a2 = roughness * roughness;
            float k = NdotH * NdotH * (a2 - 1) + 1;
            return MinNumber(1e8f, a2 / (float.Pi * k * k));
        }

        private static float Visibility(float NdotV, float NdotL, float roughness)
        {
            float a2 = roughness * roughness;
            float v = NdotL * Sqrt(NdotV * NdotV * (1 - a2) + a2);
            float l = NdotV * Sqrt(NdotL * NdotL * (1 - a2) + a2);
            return Min(1e8f, 0.5f / (v + l));
        }

        public override Color GetColor(
            List<LightObject> lights,
            Components.Environment environment,
            Color baseColor, 
            Vector3 emission, 
            PBRParams pbrParams, 
            Vector3 n, 
            Vector3 cameraPosition, 
            Vector3 p,
            ClearCoatParams clearCoat,
            float transmission
        )
        {
            float r2 = pbrParams.Roughness * pbrParams.Roughness;
            float cr2 = clearCoat.Rougness * clearCoat.Rougness;
            transmission = 1f - transmission;

            Vector3 N = Normalize(n);
            Vector3 CN = Normalize(clearCoat.Normal);
            Vector3 V = Normalize(cameraPosition - p);

            float NdotV = Max(Dot(N, V), 0);
            float CNdotV = Max(Dot(CN, V), 0);

            Vector3 color = Zero;

            Vector3 F0 = Lerp(Create(0.04f), baseColor.BaseColor, pbrParams.Metallic);
            Vector3 albedo = (1 - pbrParams.Metallic) * baseColor.BaseColor;
            Vector3 diffuse = albedo / float.Pi * transmission;

            foreach (LightObject light in lights)
            {
                Vector3 L = light.GetL(p);

                float NdotL = Dot(N, L);

                if (NdotL <= 0)
                    continue;

                Vector3 H = Normalize(V + L);

                float NdotH = Max(Dot(N, H), 0);
                float VdotH = Max(Dot(V, H), 0);
                float CNdotL = Max(Dot(CN, L), 0);
                float CNdotH = Max(Dot(CN, H), 0);

                float distribution = Distribution(NdotH, r2);
                float visibility = Visibility(NdotV, NdotL, r2);
                Vector3 reflectance = FresnelSchlick(VdotH, F0);

                Vector3 specular = reflectance * visibility * distribution;
                Vector3 irradiance = light.GetIrradiance(p);

                float coatDistribution = Distribution(CNdotH, cr2);
                float coatVisibility = Visibility(CNdotV, CNdotL, cr2);
                Vector3 coatReflectance = FresnelSchlick(VdotH, Create(0.04f)) * clearCoat.Factor;

                Vector3 coatSpecular = coatReflectance * coatVisibility * coatDistribution;

                color += (((One - reflectance) * diffuse + specular) * (One - coatReflectance) * NdotL + coatSpecular * CNdotL) * irradiance;
            }

            Vector3 ambientReflectance = F0;
            Vector3 ambientDiffuse = albedo / float.Pi * transmission;
            Vector3 ambientIrradiance = environment.GetIBLDiffuseColor(N);

            float lod = pbrParams.Roughness * (environment.IBLSpecularMap.Count - 1);
            int lod0 = (int)lod, lod1 = int.Min(lod0 + 1, environment.IBLSpecularMap.Count - 1);

            Vector3 R = Reflect(-V, N);

            Vector3 ambientSpecularLight0 = environment.GetIBLSpecularColor(R, lod0);
            Vector3 ambientSpecularLight1 = environment.GetIBLSpecularColor(R, lod1);
            Vector3 ambientSpecularLight = Lerp(ambientSpecularLight0, ambientSpecularLight1, lod - lod0);
            Vector3 brdf = environment.BRDFLUT.GetColor(NdotV, 1 - pbrParams.Roughness);
            Vector3 ambientSpecular = ambientSpecularLight * (ambientReflectance * brdf.X + Create(brdf.Y));

            Vector3 coatReflectanceIBL = Create(0.04f) * clearCoat.Factor;
            lod = clearCoat.Rougness * (environment.IBLSpecularMap.Count - 1);
            lod0 = (int)lod;
            lod1 = int.Min(lod0 + 1, environment.IBLSpecularMap.Count - 1);
            R = Reflect(-V, CN);

            Vector3 coatSpecularLight0 = environment.GetIBLSpecularColor(R, lod0);
            Vector3 coatSpecularLight1 = environment.GetIBLSpecularColor(R, lod1);
            Vector3 coatSpecularLight = Lerp(coatSpecularLight0, coatSpecularLight1, lod - lod0);
            brdf = environment.BRDFLUT.GetColor(CNdotV, 1 - clearCoat.Rougness);
            Vector3 coatSpecularIBL = coatSpecularLight * (coatReflectanceIBL * brdf.X + Create(brdf.Y) * clearCoat.Factor);

            color += (((One - ambientReflectance) * ambientDiffuse * ambientIrradiance + ambientSpecular) * (One - coatReflectanceIBL) + coatSpecularIBL) * pbrParams.Occlusion;

            color += emission * EmissionIntensity;

            return new(color * baseColor.Alpha, baseColor.Alpha * transmission);
        }
    }
}
