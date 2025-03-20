using CGA2.Components.Materials;
using CGA2.Components.Objects;
using System.Numerics;

namespace CGA2.Shaders
{
    public abstract class Shader
    {
        public abstract string Name { get; set; }

        public abstract Color GetColor(
            List<LightObject> lights,
            Components.Environment environment,
            Color baseColor,
            Vector3 emission,
            PBRParams pbrParams,
            Vector3 n,
            Vector3 cameraPosition,
            Vector3 p,
            float transmission
        );
    }
}
