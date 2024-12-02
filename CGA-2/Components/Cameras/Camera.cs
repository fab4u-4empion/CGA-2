namespace CGA2.Components.Cameras
{
    public abstract class Camera : Component
    {
        public float NearPlane { get; set; } = 0.01f;
        public float FarPlane { get; set; } = 1000f;
        public float AspectRatio { get; set; } = 16f / 9f;
    }
}