using static System.Single;

namespace CGA2.Components.Cameras
{
    public class PerspectiveCamera : Camera
    {
        public override string Name { get; set; } = "PerspectiveCamera";
        public float FieldOfView { get; set; } = Pi / 4f;
    }
}