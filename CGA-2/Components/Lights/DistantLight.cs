namespace CGA2.Components.Lights
{
    public class DistantLight : Light
    {
        public override string Name { get; set; } = "DistantLight";
        public float Irradiance { get; set; } = 10;
        public float Angle { get; set; } = 0.526f;
    }
}