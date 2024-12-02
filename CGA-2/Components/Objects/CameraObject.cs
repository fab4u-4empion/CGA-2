using CGA2.Components.Cameras;

namespace CGA2.Components.Objects
{
    public class CameraObject : Object
    {
        public override string Name { get; set; } = "Camera";
        public Camera Camera { get; set; } = new PerspectiveCamera();
    }
}