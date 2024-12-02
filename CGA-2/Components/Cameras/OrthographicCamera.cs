namespace CGA2.Components.Cameras
{
    public class OrthographicCamera : Camera
    {
        public override string Name { get; set; } = "OrthographicCamera";
        public float Scale { get; set; } = 1;
    }
}