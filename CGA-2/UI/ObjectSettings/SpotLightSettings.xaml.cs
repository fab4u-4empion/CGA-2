using CGA2.Components.Lights;
using System.Globalization;
using System.Windows.Controls;
using static CGA2.Utils.Vector3ToColorConverter;
using static System.Single;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для SpotLightSettings.xaml
    /// </summary>
    public partial class SpotLightSettings : UserControl
    {
        private SpotLight Light { get; set; }

        public event EventHandler? OnSave;

        public SpotLightSettings(SpotLight light)
        {
            InitializeComponent();

            LightNameTextBox.Text = light.Name;
            LightPowerTextBox.Text = light.Power.ToString("0.###", CultureInfo.InvariantCulture);
            LightRadiusTextBox.Text = light.Radius.ToString("0.###", CultureInfo.InvariantCulture);
            LightAngleTextBox.Text = RadiansToDegrees(light.Angle).ToString("0.###", CultureInfo.InvariantCulture);
            LightFalloffTextBox.Text = light.Falloff.ToString("0.###", CultureInfo.InvariantCulture);
            LightColorButton.Color = Vector3ToColor(light.Color);

            Light = light;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Light.Power = Max(Parse(LightPowerTextBox.Text, CultureInfo.InvariantCulture), 0f);
            Light.Radius = Max(Parse(LightRadiusTextBox.Text, CultureInfo.InvariantCulture), 0f);
            Light.Angle = Clamp(DegreesToRadians(Parse(LightAngleTextBox.Text, CultureInfo.InvariantCulture)), 0f, Pi / 2f);
            Light.Falloff = Clamp(Parse(LightFalloffTextBox.Text, CultureInfo.InvariantCulture), 0f, 1f);
            Light.Name = LightNameTextBox.Text;
            Light.Color = ColorToVector3(LightColorButton.Color);

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
