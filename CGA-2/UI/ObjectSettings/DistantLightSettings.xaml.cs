using CGA2.Components.Lights;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static System.Single;
using static CGA2.Utils.Vector3ToColorConverter;
using static CGA2.ToneMapping.ToneMapper;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для DistantLightSettings.xaml
    /// </summary>
    public partial class DistantLightSettings : UserControl
    {
        private DistantLight Light { get; set; }

        public event EventHandler? OnSave;

        public DistantLightSettings(DistantLight light)
        {
            InitializeComponent();

            LightNameTextBox.Text = light.Name;
            LightIrradianceTextBox.Text = light.Irradiance.ToString("0.###", CultureInfo.InvariantCulture);
            LightAngleTextBox.Text = light.Angle.ToString("0.###", CultureInfo.InvariantCulture);
            LightColorButton.Color = Vector3ToColor(LinearToSrgb(light.Color));

            Light = light;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            Light.Angle = Clamp(Parse(LightAngleTextBox.Text, CultureInfo.InvariantCulture), 0f, 90f);
            Light.Irradiance = Max(Parse(LightIrradianceTextBox.Text, CultureInfo.InvariantCulture), 0f);
            Light.Name = LightNameTextBox.Text;
            Light.Color = SrgbToLinear(ColorToVector3(LightColorButton.Color));

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
