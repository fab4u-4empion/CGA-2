using CGA2.ToneMapping;

namespace CGA2
{
    public class Settings
    {
        public static float Scaling { get; set; } = 1f;

        public static int SelectedCamera { get; set; } = 0;

        public static float Exposure { get; set; } = 1f;

        public static ToneMapper ToneMapper { get; set; } = new LinearToneMapper();
    }
}
