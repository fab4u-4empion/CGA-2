using CGA2.Components.Lights;

namespace CGA2.Components.Objects
{
    public class LightObject : SceneObject
    {
        public Light Light { get; set; } = new();
    }
}
