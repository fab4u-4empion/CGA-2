using CGA2.Components.Cameras;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static System.Single;


namespace CGA2.UI.ObjectSettings
{
    /// <summary>
    /// Логика взаимодействия для PerspectiveCameraSettings.xaml
    /// </summary>
    public partial class PerspectiveCameraSettings : UserControl
    {
        private PerspectiveCamera Camera { get; set; }

        public event EventHandler? OnSave;

        public PerspectiveCameraSettings(PerspectiveCamera camera)
        {
            InitializeComponent();

            CameraNameTextBox.Text = camera.Name;
            CameraNearPlaneTextBox.Text = camera.NearPlane.ToString("0.###", CultureInfo.InvariantCulture);
            CameraFarPlaneTextBox.Text = camera.FarPlane.ToString("0.###", CultureInfo.InvariantCulture);
            CameraFoVTextBox.Text = RadiansToDegrees(camera.FieldOfView).ToString("0.###", CultureInfo.InvariantCulture);

            Camera = camera;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            Camera.Name = CameraNameTextBox.Text;
            Camera.NearPlane = Parse(CameraNearPlaneTextBox.Text, CultureInfo.InvariantCulture);
            Camera.FarPlane = Parse(CameraFarPlaneTextBox.Text, CultureInfo.InvariantCulture);
            Camera.FieldOfView = Clamp(DegreesToRadians(Parse(CameraFoVTextBox.Text, CultureInfo.InvariantCulture)), Pi / 6f, Pi / 2f);

            OnSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
