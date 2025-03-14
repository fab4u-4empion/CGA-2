using CGA2.Shaders;
using CGA2.ToneMapping;

namespace CGA2
{
    public class Settings
    {
        public static float Scaling { get; set; } = 1f;

        public static int SelectedCamera { get; set; } = 0;

        public static float Exposure { get; set; } = 1f;

        public static int MaxAnisotropy { get; set; } = 1;

        public static ToneMapper ToneMapper { get; set; } = new AcesFilmicToneMapper();

        public static Shader Shader { get; set; } = new PBRShader();

        public static float EmissionIntensity { get; set; } = 1f;
    }
}
