using CGA2.Components.Lights;
using System.Numerics;
using static System.Numerics.Vector3;

namespace CGA2.Components.Objects
{
    public class LightObject : SceneObject
    {
        public override string Name { get; set; } = "Light";
        public Light Light { get; set; } = new SphericalLight();

        public Vector3 GetIrradiance(Vector3 point)
        {
            if (Light is DistantLight distantLight)
                return distantLight.Color * distantLight.Irradiance;

            float distance = Distance(Location, point);

            SphericalLight sphericalLight = (SphericalLight)Light;

            Vector3 irradiance = sphericalLight.Color * sphericalLight.Power / (4f * float.Pi * distance * distance);

            if (sphericalLight is SpotLight spotLight)
            {
                float outer = float.Cos(spotLight.Angle);
                float inner = float.Cos(spotLight.Angle * (1 - spotLight.Falloff));
                float spotDirDotL = Dot(GetL(point), -Transform(UnitY, Rotation));
                float spotAttenuation = float.Clamp((spotDirDotL - outer) / (inner - outer), 0, 1);

                return irradiance * spotAttenuation;
            }

            return irradiance;
        }

        public Vector3 GetL(Vector3 point)
        {
            if (Light is DistantLight)
                return Transform(UnitY, Rotation);

            return Normalize(Location - point);
        }
    }
}