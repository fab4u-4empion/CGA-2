using CGA2.Components.Lights;
using System.Globalization;
using System.Windows.Controls;
using static CGA2.Utils.Vector3ToColorConverter;
using static System.Single;

namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для SphericalLightSettings.xaml
    /// </summary>
    public partial class SphericalLightSettings : UserControl
    {
        private SphericalLight Light { get; set; }

        public event EventHandler? OnSave;

        public SphericalLightSettings(SphericalLight light)
        {
            InitializeComponent();

            LightNameTextBox.Text = light.Name;
            LightPowerTextBox.Text = light.Power.ToString("0.###", CultureInfo.InvariantCulture);
            LightRadiusTextBox.Text = light.Radius.ToString("0.###", CultureInfo.InvariantCulture);
            LightColorButton.Color = Vector3ToColor(light.Color);

            Light = light;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Light.Power = Max(Parse(LightPowerTextBox.Text, CultureInfo.InvariantCulture), 0f);
            Light.Radius = Max(Parse(LightRadiusTextBox.Text, CultureInfo.InvariantCulture), 0f);
            Light.Name = LightNameTextBox.Text;
            Light.Color = ColorToVector3(LightColorButton.Color);

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
